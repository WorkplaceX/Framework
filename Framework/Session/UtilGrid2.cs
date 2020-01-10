namespace Framework.Session
{
    using Database.dbo;
    using Framework.DataAccessLayer;
    using Framework.DataAccessLayer.DatabaseMemory;
    using Framework.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using static Framework.DataAccessLayer.UtilDalType;

    /// <summary>
    /// Grid load and process.
    /// </summary>
    internal static class UtilGrid2
    {
        /// <summary>
        /// Load data into grid. Override method Page.GridQuery(); to define query. It's also called to reload data.
        /// </summary>
        public static async Task LoadAsync(Grid2 grid)
        {
            Page page = grid.ComponentOwner<Page>();
            IQueryable query = page.Grid2Query(grid);
            grid.TypeRow = query?.ElementType;
            grid.DatabaseEnum = DatabaseMemoryInternal.DatabaseEnum(query);
            if (grid.TypeRow != null)
            {
                // Get config grid and field query
                Page.GridConfigResult gridConfigResult = new Page.GridConfigResult();
                page.Grid2QueryConfig(grid, UtilDalType.TypeRowToTableNameCSharp(grid.TypeRow), gridConfigResult);

                // Load config grid
                grid.ConfigGridList = await Data.SelectAsync(gridConfigResult.ConfigGridQuery);
                var configGrid = grid.ConfigGridList.Where(item => item.ConfigName == grid.ConfigName).SingleOrDefault(); // LINQ to memory
                query = Data.QuerySkipTake(query, 0, configGrid.RowCountMax == null ? 10 : configGrid.RowCountMax.Value); // By default load 10 rows.

                // Load config field (Task)
                var configFieldListTask = Data.SelectAsync(gridConfigResult.ConfigFieldQuery);

                // Load row (Task)
                var rowListTask = Data.SelectAsync(query);

                await Task.WhenAll(configFieldListTask, rowListTask); // Load config field and row in parallel

                // Load config field
                grid.ConfigFieldList = configFieldListTask.Result;

                // Load row
                grid.RowList = rowListTask.Result;

                var configFieldDictionary = grid.ConfigFieldList.ToDictionary(item => (item.ConfigName, item.FieldNameCSharp), item => item);

                grid.ColumnList = new List<Grid2Column>();
                AppJson appJson = page.ComponentOwner<AppJson>();
                var fieldList = UtilDalType.TypeRowToFieldListDictionary(grid.TypeRow);
                foreach (var propertyInfo in UtilDalType.TypeRowToPropertyInfoList(grid.TypeRow))
                {
                    var field = fieldList[propertyInfo.Name];
                    configFieldDictionary.TryGetValue((grid.ConfigName, propertyInfo.Name), out FrameworkConfigFieldBuiltIn configField);
                    NamingConvention namingConvention = appJson.NamingConventionInternal(grid.TypeRow);
                    string columnText = namingConvention.ColumnTextInternal(grid.TypeRow, propertyInfo.Name, configField?.Text);
                    bool isVisible = namingConvention.ColumnIsVisibleInternal(grid.TypeRow, propertyInfo.Name, configField?.IsVisible);
                    double sort = namingConvention.ColumnSortInternal(grid.TypeRow, propertyInfo.Name, field, configField?.Sort);
                    grid.ColumnList.Add(new Grid2Column
                    {
                        FieldNameCSharp = field.FieldNameCSharp,
                        ColumnText = columnText,
                        Description = configField?.Description,
                        IsVisible = isVisible,
                        Sort = sort,
                        SortField = field.Sort
                    });
                }
                grid.ColumnList = grid.ColumnList.
                    Where(item => item.IsVisible == true).
                    OrderBy(item => item.Sort).
                    ThenBy(item => item.SortField).ToList(); // Make it deterministic if multiple columns have same Sort.
                int columnId = 0;
                foreach (var column in grid.ColumnList)
                {
                    column.Id = columnId += 1;
                }
            }

            Render(grid);
            await LoadRowFirstSelect(grid);
            RenderRowIsSelectedUpdate(grid);
        }

        /// <summary>
        /// Reload rows after sort, filter. Do not load grid and field configuration.
        /// </summary>
        private static async Task ReloadAsync(Grid2 grid)
        {
            Page page = grid.ComponentOwner<Page>();
            IQueryable query = page.Grid2Query(grid);
            var typeRow = query?.ElementType;
            if (typeRow != null && typeRow == grid.TypeRow) // Make sure grid and field configuration are correct.
            {
                var configGrid = grid.ConfigGridList.Where(item => item.ConfigName == grid.ConfigName).SingleOrDefault(); // LINQ to memory
                query = Data.QuerySkipTake(query, 0, configGrid.RowCountMax == null ? 10 : configGrid.RowCountMax.Value); // By default load 10 rows.

                // Sort
                foreach (var column in grid.ColumnList)
                {
                    if (column.IsSort != null)
                    {
                        query = Data.QueryOrderBy(query, column.FieldNameCSharp, column.IsSort.Value);
                    }
                }

                // Load row
                grid.RowList = await Data.SelectAsync(query);
            }
        }

        /// <summary>
        /// After load, if no row in data grid is selected, select first row.
        /// </summary>
        private static async Task LoadRowFirstSelect(Grid2 grid)
        {
            foreach (var rowState in grid.RowStateList)
            {
                if (rowState.RowEnum == GridRowEnum.Index) // Select data rows only.
                {
                    Row row = grid.RowList[rowState.RowId.Value - 1];
                    rowState.IsSelect = true;
                    Page page = grid.ComponentOwner<Page>();
                    await page.GridRowSelectedAsync(grid, row);
                    break;
                }
            }
        }

        /// <summary>
        /// Process incoming RequestJson.
        /// </summary>
        public static async Task ProcessAsync()
        {
            // IsClickSort
            await ProcessIsClickSortAsync();

            // CellIsModify
            await ProcessCellIsModify();

            // IsClickEnum
            await ProcessIsClickEnum();

            // RowIsClick
            await ProcessRowIsClickAsync();

            // RowIsClick
            await ProcessIsClickConfigAsync();
        }

        /// <summary>
        /// Add data row new.
        /// </summary>
        private static void RenderRowNewAdd(Grid2 grid)
        {
            int rowStateId = grid.RowStateList.Count;
            int cellId = grid.CellList.Count;
            // Render data row New
            grid.RowStateList.Add(new Grid2RowState { Id = rowStateId += 1, RowEnum = GridRowEnum.New });
            foreach (var column in grid.ColumnList)
            {
                grid.CellList.Add(new Grid2Cell
                {
                    Id = cellId += 1,
                    ColumnId = column.Id,
                    RowStateId = rowStateId,
                    CellEnum = Grid2CellEnum.New,
                    Placeholder = "New",
                });
            }
        }

        private static void RenderRowUpdate(Grid2 grid, Grid2Cell cell)
        {
            // Get page and row
            Page page = grid.ComponentOwner<Page>();
            Grid2RowState rowState = grid.RowStateList[cell.RowStateId - 1];
            Row row;
            if (rowState.RowNew != null)
            {
                row = rowState.RowNew;
            }
            else
            {
                row = grid.RowList[rowState.RowId.Value - 1];
            }

            var fieldList = UtilDalType.TypeRowToFieldListDictionary(grid.TypeRow);
            foreach (var item in grid.CellList)
            {
                if (item.RowStateId == cell.RowStateId) // Cells on same column
                {
                    var column = grid.ColumnList[item.ColumnId - 1];
                    var field = fieldList[column.FieldNameCSharp];
                    string text = null;
                    object value = field.PropertyInfo.GetValue(row);
                    if (value != null)
                    {
                        text = page.GridCellText(grid, row, field.PropertyInfo.Name); // Custom convert database value to cell text.
                        text = UtilFramework.StringNull(text);
                        if (text == null)
                        {
                            text = field.FrameworkType().CellTextFromValue(value);
                        }
                    }
                    item.TextLeave = null;
                    if (item.ErrorParse == null) // Do not change input text while it's not parsed and written to row.
                    {
                        if (item == cell)
                        {
                            item.TextLeave = UtilFramework.StringEmpty(text); // Do not change text while user modifies.
                        }
                        else
                        {
                            item.Text = text;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Update IsSelect flag of all cells.
        /// </summary>
        private static void RenderRowIsSelectedUpdate(Grid2 grid)
        {
            foreach (var cell in grid.CellList)
            {
                Grid2RowState rowState = grid.RowStateList[cell.RowStateId - 1];
                cell.IsSelect = rowState.IsSelect;
            }
        }

        private static void Render(Grid2 grid)
        {
            Page page = grid.ComponentOwner<Page>();
            grid.CellList = new List<Grid2Cell>();
            grid.RowStateList = new List<Grid2RowState>();
            StringBuilder styleColumnList = new StringBuilder();
            int cellId = 0;
            int rowStateId = 0;
            // Render Filter
            grid.RowStateList.Add(new Grid2RowState { Id = rowStateId += 1, RowEnum = GridRowEnum.Filter });
            foreach (var column in grid.ColumnList)
            {
                styleColumnList.Append("minmax(0, 1fr) ");
                grid.CellList.Add(new Grid2Cell
                {
                    Id = cellId += 1,
                    ColumnId = column.Id,
                    RowStateId = rowStateId,
                    CellEnum = Grid2CellEnum.HeaderColumn,
                    ColumnText = column.ColumnText,
                    Description = column.Description,
                    IsSort = column.IsSort,
                });
            }
            foreach (var column in grid.ColumnList)
            {
                grid.CellList.Add(new Grid2Cell
                {
                    Id = cellId += 1,
                    ColumnId = column.Id,
                    RowStateId = rowStateId,
                    CellEnum = Grid2CellEnum.Filter,
                    Placeholder = "Search",
                });
            }
            // Render Index
            var fieldList = UtilDalType.TypeRowToFieldListDictionary(grid.TypeRow);
            int rowId = 0;
            foreach (var row in grid.RowList)
            {
                grid.RowStateList.Add(new Grid2RowState { Id = rowStateId += 1, RowId = rowId += 1, RowEnum = GridRowEnum.Index });
                foreach (var column in grid.ColumnList)
                {
                    var field = fieldList[column.FieldNameCSharp];
                    string text = null;
                    object value = field.PropertyInfo.GetValue(row);
                    if (value != null)
                    {
                        text = page.GridCellText(grid, row, field.PropertyInfo.Name); // Custom convert database value to cell text.
                        text = UtilFramework.StringNull(text);
                        if (text == null)
                        {
                            text = field.FrameworkType().CellTextFromValue(value);
                        }
                    }
                    grid.CellList.Add(new Grid2Cell
                    {
                        Id = cellId += 1,
                        ColumnId = column.Id,
                        RowStateId = rowStateId,
                        CellEnum = Grid2CellEnum.Index,
                        Text = text,
                    });
                }
            }
            // Render New
            RenderRowNewAdd(grid);
            grid.StyleColumn = styleColumnList.ToString();
        }

        private static async Task ProcessIsClickSortAsync()
        {
            if (UtilSession.Request(RequestCommand.Grid2IsClickSort, out RequestJson requestJson, out Grid2 grid))
            {
                Grid2Cell cell = grid.CellList[requestJson.Grid2CellId - 1];
                Grid2Column column = grid.ColumnList[cell.ColumnId - 1];
                // Reset sort on other columns
                foreach (var item in grid.ColumnList)
                {
                    if (item != column)
                    {
                        item.IsSort = null;
                    }
                }
                if (column.IsSort == null)
                {
                    column.IsSort = false;
                }
                else
                {
                    column.IsSort = !column.IsSort;
                }
                await ReloadAsync(grid);
                Render(grid);
                await LoadRowFirstSelect(grid);
                RenderRowIsSelectedUpdate(grid);
            }
        }

        /// <summary>
        /// User clicked column header configuration icon.
        /// </summary>
        private static async Task ProcessIsClickConfigAsync()
        {
            if (UtilSession.Request(RequestCommand.Grid2IsClickConfig, out RequestJson requestJson, out Grid2 grid))
            {
                Grid2Cell cell = grid.CellList[requestJson.Grid2CellId - 1];
                Grid2Column column = grid.ColumnList[cell.ColumnId - 1];
                Page page = grid.ComponentOwner<Page>();

                string tableNameCSharp = UtilDalType.TypeRowToTableNameCSharp(grid.TypeRow);
                string configName = grid.ConfigName;
                var pageConfigGrid = new PageConfigGrid(page);
                pageConfigGrid.Init(tableNameCSharp, configName, column.FieldNameCSharp);
                await pageConfigGrid.InitAsync();
            }
        }

        private static void ProcessCellIsModifyWarning(Grid2 grid, Grid2Cell cell)
        {
            foreach (var item in grid.CellList)
            {
                if (item.RowStateId == cell.RowStateId)
                {
                    if (item.IsModified && item.Text != item.TextOld)
                    {
                        if (item.ErrorParse == null && item.ErrorSave == null)
                        {
                            item.Warning = "Not saved because of other errors!";
                        }
                    }
                    else
                    {
                        item.Warning = null;
                    }
                }
            }
        }

        /// <summary>
        /// Call after successful save.
        /// </summary>
        private static void ProcessCellIsModifyReset(Grid2 grid, Grid2Cell cell)
        {
            foreach (var item in grid.CellList)
            {
                if (item.RowStateId == cell.RowStateId)
                {
                    item.IsModified = false;
                    item.TextOld = null;
                    item.ErrorParse = null;
                    item.ErrorSave = null;
                    item.Warning = null;
                }
            }
        }

        /// <summary>
        /// Parse
        /// </summary>
        private static void ProcessCellIsModifyParse(Grid2 grid, Page page, Row rowNew, Grid2Column column, Field field, Grid2Cell cell)
        {
            cell.ErrorParse = null;
            // Parse
            try
            {
                // Validate
                if (cell.Text == null)
                {
                    if (!UtilFramework.IsNullable(field.PropertyInfo.PropertyType))
                    {
                        throw new Exception("Value can not be null!");
                    }
                }
                // Parse custom
                bool isHandled = false;
                if (cell.Text != null)
                {
                    page.GridCellParse(grid, rowNew, column.FieldNameCSharp, cell.Text, out isHandled); // Custom parse of user entered text.
                }
                // Parse default
                if (!isHandled)
                {
                    Data.CellTextParse(field, cell.Text, rowNew, out string errorParse);
                    cell.ErrorParse = errorParse;
                }
            }
            catch (Exception exception)
            {
                cell.ErrorParse = UtilFramework.ExceptionToString(exception);
            }
        }

        /// <summary>
        /// Save (Update).
        /// </summary>
        private static async Task ProcessCellIsModifyUpdateAsync(Grid2 grid, Page page, Row row, Row rowNew, Grid2Cell cell)
        {
            // Save
            try
            {
                bool isHandled = await page.GridUpdateAsync(grid, row, rowNew, grid.DatabaseEnum);
                if (!isHandled)
                {
                    await Data.UpdateAsync(row, rowNew, grid.DatabaseEnum);
                }
            }
            catch (Exception exception)
            {
                cell.ErrorSave = UtilFramework.ExceptionToString(exception);
            }
        }

        /// <summary>
        /// Save (Insert).
        /// </summary>
        private static async Task ProcessCellIsModifyInsertAsync(Grid2 grid, Page page, Row rowNew, Grid2Cell cell)
        {
            // Save
            try
            {
                // Save custom
                bool isHandled = await page.GridInsertAsync(grid, rowNew, grid.DatabaseEnum);
                if (!isHandled)
                {
                    // Save default
                    await Data.InsertAsync(rowNew, grid.DatabaseEnum);
                }
            }
            catch (Exception exception)
            {
                cell.ErrorSave = UtilFramework.ExceptionToString(exception);
            }
        }

        /// <summary>
        /// Reset ErrorSave on all cells on row.
        /// </summary>
        private static void ProcessCellIsModifyErrorSaveReset(Grid2 grid, Grid2Cell cell)
        {
            foreach (var item in grid.CellList)
            {
                if (item.RowStateId == cell.RowStateId)
                {
                    item.ErrorSave = null;
                }
            }
        }

        /// <summary>
        /// Returns true, if row has a not solved parse error.
        /// </summary>
        private static bool ProcessCellIsModifyIsErrorParse(Grid2 grid, Grid2Cell cell)
        {
            bool result = false;
            foreach (var item in grid.CellList)
            {
                if (item.RowStateId == cell.RowStateId)
                {
                    result = result || item.ErrorParse != null;
                }
            }
            return result;
        }

        /// <summary>
        /// Track IsModified and TextOld.
        /// </summary>
        private static void ProcessCellIsModifyTextOld(Grid2Cell cell, RequestJson requestJson)
        {
            if (cell.IsModified == false)
            {
                cell.IsModified = true;
                cell.TextOld = cell.Text;
            }
            else
            {
                if (requestJson.Grid2CellText == cell.TextOld)
                {
                    cell.IsModified = false;
                    cell.TextOld = null;
                }
            }
        }

        /// <summary>
        /// User selected data row.
        /// </summary>
        private static async Task ProcessRowIsClickAsync()
        {
            if (UtilSession.Request(RequestCommand.Grid2IsClickRow, out RequestJson requestJson, out Grid2 grid))
            {
                Grid2Cell cell = grid.CellList[requestJson.Grid2CellId - 1];
                Row rowSelected = null;
                foreach (var rowState in grid.RowStateList)
                {
                    if (rowState.RowEnum == GridRowEnum.Index)
                    {
                        rowState.IsSelect = rowState.Id == cell.RowStateId;
                        if (rowState.IsSelect)
                        {
                            rowSelected = grid.RowList[rowState.RowId.Value - 1];
                        }
                    }
                }
                if (rowSelected != null)
                {
                    RenderRowIsSelectedUpdate(grid);
                    Page page = grid.ComponentOwner<Page>();
                    await page.GridRowSelectedAsync(grid, rowSelected);
                }
            }
        }

        private static async Task ProcessCellIsModify()
        {
            if (UtilSession.Request(RequestCommand.Grid2CellIsModify, out RequestJson requestJson, out Grid2 grid))
            {
                Grid2Cell cell = grid.CellList[requestJson.Grid2CellId - 1];
                Grid2Column column = grid.ColumnList[cell.ColumnId - 1];
                var field = UtilDalType.TypeRowToFieldListDictionary(grid.TypeRow)[column.FieldNameCSharp];
                Page page = grid.ComponentOwner<Page>();
                Grid2RowState rowState = grid.RowStateList[cell.RowStateId - 1];

                // Track IsModified
                ProcessCellIsModifyTextOld(cell, requestJson);

                cell.Text = requestJson.Grid2CellText;
                cell.Warning = null;
                switch (rowState.RowEnum)
                {
                    case GridRowEnum.Filter:
                        break;
                    case GridRowEnum.Index:
                        {
                            Row row = grid.RowList[rowState.RowId.Value - 1];
                            if (rowState.RowNew == null)
                            {
                                rowState.RowNew = (Row)Activator.CreateInstance(grid.TypeRow);
                                Data.RowCopy(row, rowState.RowNew);
                            }
                            // ErrorSave reset
                            ProcessCellIsModifyErrorSaveReset(grid, cell);
                            // Parse
                            ProcessCellIsModifyParse(grid, page, rowState.RowNew, column, field, cell);
                            if (!ProcessCellIsModifyIsErrorParse(grid, cell))
                            {
                                // Save
                                await ProcessCellIsModifyUpdateAsync(grid, page, row, rowState.RowNew, cell);
                                if (cell.ErrorSave == null)
                                {
                                    Data.RowCopy(rowState.RowNew, row); // Copy new Id to 
                                    ProcessCellIsModifyReset(grid, cell);
                                }
                            }
                            RenderRowUpdate(grid, cell);
                            ProcessCellIsModifyWarning(grid, cell);
                        }
                        break;
                    case GridRowEnum.New:
                        {
                            if (rowState.RowNew == null)
                            {
                                rowState.RowNew = (Row)Activator.CreateInstance(grid.TypeRow);
                            }
                            // ErrorSave reset
                            ProcessCellIsModifyErrorSaveReset(grid, cell);
                            // Parse
                            ProcessCellIsModifyParse(grid, page, rowState.RowNew, column, field, cell);
                            if (!ProcessCellIsModifyIsErrorParse(grid, cell))
                            {
                                // Save
                                await ProcessCellIsModifyInsertAsync(grid, page, rowState.RowNew, cell);
                                if (cell.ErrorSave == null)
                                {
                                    grid.RowList.Add(rowState.RowNew);
                                    rowState.RowId = grid.RowList.Count;
                                    foreach (var item in grid.CellList)
                                    {
                                        if (item.RowStateId == cell.RowStateId) // Cells in same row
                                        {
                                            rowState.RowEnum = GridRowEnum.Index; // From New to Index
                                            item.Placeholder = null;
                                        }
                                    }
                                    rowState.RowNew = null;
                                    ProcessCellIsModifyReset(grid, cell);
                                    RenderRowUpdate(grid, cell);
                                    RenderRowNewAdd(grid);
                                }
                            }
                            ProcessCellIsModifyWarning(grid, cell);
                        }
                        break;
                    default:
                        throw new Exception("Enum unknown!");
                }
            }
        }

        private static async Task ProcessIsClickEnum()
        {
            if (UtilSession.Request(RequestCommand.Grid2IsClickEnum, out RequestJson requestJson, out Grid2 grid))
            {
                // Grid config
                if (requestJson.GridIsClickEnum == GridIsClickEnum.Config)
                {
                    if (grid.TypeRow != null) // Do not show config if for example no query is defined for data grid.
                    {
                        Page page = grid.ComponentOwner<Page>();

                        string tableNameCSharp = UtilDalType.TypeRowToTableNameCSharp(grid.TypeRow);
                        string configName = grid.ConfigName;
                        var pageConfigGrid = new PageConfigGrid(page);
                        pageConfigGrid.Init(tableNameCSharp, configName, null);
                        await pageConfigGrid.InitAsync();
                    }
                }

                // Grid reload
                if (requestJson.GridIsClickEnum == GridIsClickEnum.Reload)
                {
                    await ReloadAsync(grid);
                    Render(grid);
                    await LoadRowFirstSelect(grid);
                    RenderRowIsSelectedUpdate(grid);
                }
            }
        }
    }
}
