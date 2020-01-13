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
        /// Returns ColumnList for data grid.
        /// </summary>
        private static List<Grid2Column> LoadColumnList(Grid2 grid)
        {
            Page page = grid.ComponentOwner<Page>();
            var configFieldDictionary = ConfigFieldDictionary(grid);
            AppJson appJson = page.ComponentOwner<AppJson>();

            var result = new List<Grid2Column>();
            var fieldList = UtilDalType.TypeRowToFieldListDictionary(grid.TypeRow);
            foreach (var propertyInfo in UtilDalType.TypeRowToPropertyInfoList(grid.TypeRow))
            {
                var field = fieldList[propertyInfo.Name];
                configFieldDictionary.TryGetValue(propertyInfo.Name, out FrameworkConfigFieldBuiltIn configField);
                NamingConvention namingConvention = appJson.NamingConventionInternal(grid.TypeRow);
                string columnText = namingConvention.ColumnTextInternal(grid.TypeRow, propertyInfo.Name, configField?.Text);
                bool isVisible = namingConvention.ColumnIsVisibleInternal(grid.TypeRow, propertyInfo.Name, configField?.IsVisible);
                double sort = namingConvention.ColumnSortInternal(grid.TypeRow, propertyInfo.Name, field, configField?.Sort);
                result.Add(new Grid2Column
                {
                    FieldNameCSharp = field.FieldNameCSharp,
                    ColumnText = columnText,
                    Description = configField?.Description,
                    IsVisible = isVisible,
                    Sort = sort,
                    SortField = field.Sort
                });
            }
            result = result
                .Where(item => item.IsVisible == true)
                .OrderBy(item => item.Sort)
                .ThenBy(item => item.SortField) // Make it deterministic if multiple columns have same Sort.
                .ToList();
            // Column.Id
            int columnId = 0;
            foreach (var item in result)
            {
                item.Id = columnId += 1;
            }
            return result;
        }

        /// <summary>
        /// Returns RowStateList for data grid.
        /// </summary>
        private static List<Grid2RowState> LoadRowStateList(Grid2 grid)
        {
            var result = new List<Grid2RowState>();
            result.Add(new Grid2RowState { RowId = null, RowEnum = GridRowEnum.Filter });
            int rowId = 0;
            foreach (var row in grid.RowList)
            {
                rowId += 1;
                result.Add(new Grid2RowState { RowId = rowId, RowEnum = GridRowEnum.Index });
            }
            result.Add(new Grid2RowState { RowId = null, RowEnum = GridRowEnum.New });
            // RowState.Id
            int count = 0;
            foreach (var rowState in result)
            {
                count += 1;
                rowState.Id = count;
            }
            return result;
        }

        private static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> list, TKey key, Func<TKey, TValue> valueFactory, out bool isAdded)
        {
            TValue result;
            isAdded = false;
            if (!list.TryGetValue(key, out result))
            {
                isAdded = true;
                result = valueFactory(key);
                list.Add(key, result);
            }
            return result;
        }

        /// <summary>
        /// Overload.
        /// </summary>
        private static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> list, TKey key, Func<TKey, TValue> valueFactory)
        {
            return list.GetOrAdd(key, valueFactory, out bool isAdded);
        }

        /// <summary>
        /// Render Grid.CellList.
        /// </summary>
        /// <param name="cell">If not null, force all cells on this column to render updated text.</param>
        private static void Render(Grid2 grid, Grid2Cell cell = null)
        {
            // IsVisibleScroll
            int count = 0;
            foreach (var column in grid.ColumnList)
            {
                count += 1;
                column.IsVisibleScroll = count - 1 >= grid.OffsetColumn && count - 1 < grid.OffsetColumn + ConfigColumnCountMax(ConfigGrid(grid));
            }
            var columnList = grid.ColumnList.Where(item => item.IsVisibleScroll).ToList();
            foreach (var rowState in grid.RowStateList)
            {
                rowState.IsVisibleScroll = true;
            }
            var rowStateList = grid.RowStateList.Where(item => item.IsVisibleScroll).ToList();

            // CellList
            var cellList = grid.CellList.ToDictionary(item => (item.ColumnId, item.RowStateId, item.CellEnum)); // Key (ColumnId, RowState, CellEnum)
            grid.CellList = new List<Grid2Cell>();
            foreach (var cellLocal in cellList.Values)
            {
                cellLocal.IsVisibleScroll = false;
            }

            // Render StyleColumn
            StringBuilder styleColumnList = new StringBuilder();
            foreach (var column in columnList)
            {
                styleColumnList.Append("minmax(0, 1fr) ");
            }
            grid.StyleColumn = styleColumnList.ToString();

            // Render Cell
            Page page = grid.ComponentOwner<Page>();
            var fieldList = UtilDalType.TypeRowToFieldListDictionary(grid.TypeRow);
            var filter = new Grid2Filter(grid);
            var filterValueList = filter.FilterValueList();
            foreach (var rowState in rowStateList)
            {
                // Filter
                if (rowState.RowEnum == GridRowEnum.Filter)
                {
                    // Filter Header
                    foreach (var column in columnList)
                    {
                        var cellLocal = cellList.GetOrAdd((column.Id, rowState.Id, Grid2CellEnum.HeaderColumn), (key) => new Grid2Cell
                        {
                            ColumnId = key.Item1,
                            RowStateId = key.Item2,
                            CellEnum = key.Item3,
                            ColumnText = column.ColumnText,
                            Description = column.Description,
                        });
                        grid.CellList.Add(cellLocal);
                        cellLocal.IsSort = Grid2SortValue.IsSortGet(grid, column.FieldNameCSharp);
                        cellLocal.IsVisibleScroll = true;
                    }
                    // Filter Value
                    foreach (var column in columnList)
                    {
                        filterValueList.TryGetValue(column.FieldNameCSharp, out Grid2FilterValue filterValue);
                        var cellLocal = cellList.GetOrAdd((column.Id, rowState.Id, Grid2CellEnum.Filter), (key) => new Grid2Cell
                        {
                            ColumnId = key.Item1,
                            RowStateId = key.Item2,
                            CellEnum = key.Item3,
                            Placeholder = "Search"
                        });
                        grid.CellList.Add(cellLocal);
                        cellLocal.Text = filterValue?.Text;
                        if (column.FieldNameCSharp == filterValue?.FieldNameCSharp && filterValue?.IsFocus == true)
                        {
                            cellLocal.TextLeave = filterValue.TextLeave;
                        }
                        cellLocal.IsVisibleScroll = true;
                    }
                }

                // Index
                if (rowState.RowEnum == GridRowEnum.Index)
                {
                    Row row;
                    if (rowState.RowNew != null)
                    {
                        row = rowState.RowNew;
                    }
                    else
                    {
                        row = grid.RowList[rowState.RowId.Value - 1];
                    }
                    foreach (var column in columnList)
                    {
                        var cellLocal = cellList.GetOrAdd((column.Id, rowState.Id, Grid2CellEnum.Index), (key) => new Grid2Cell
                        {
                            ColumnId = key.Item1,
                            RowStateId = key.Item2,
                            CellEnum = key.Item3,
                        }, out bool isAdded);
                        grid.CellList.Add(cellLocal);
                        if (cellLocal.RowStateId == cell?.RowStateId) // Cell on same row.
                        {
                            isAdded = true; // Trigger Cell.Text update
                        }
                        if (isAdded)
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
                            if (cellLocal.ErrorParse == null) // Do not override user entered text as long as in ErrorParse mode.
                            {
                                if (cellLocal == cell)
                                {
                                    cellLocal.TextLeave = UtilFramework.StringEmpty(text); // Do not change text while user modifies.
                                }
                                else
                                {
                                    cellLocal.Text = text;
                                }
                            }
                        }
                        cellLocal.IsVisibleScroll = true;
                    }
                }

                // New
                if (rowState.RowEnum == GridRowEnum.New)
                {
                    foreach (var column in columnList)
                    {
                        var cellLocal = cellList.GetOrAdd((column.Id, rowState.Id, Grid2CellEnum.New), (key) => new Grid2Cell
                        {
                            ColumnId = key.Item1,
                            RowStateId = key.Item2,
                            CellEnum = key.Item3,
                            Placeholder = "New",
                        });
                        grid.CellList.Add(cellLocal);
                        cellLocal.IsVisibleScroll = true;
                    }
                }
            }

            // Preserve cell in ErrorParse or ErrorSave state
            foreach (var cellLocal in cellList.Values)
            {
                if (cellLocal.IsVisibleScroll == false) // Cell not visible
                {
                    if (cellLocal.ErrorParse != null || cellLocal.ErrorSave != null || cellLocal.Warning != null)
                    {
                        grid.CellList.Add(cellLocal); // Preserve cell
                    }
                }
            }

            // Cell.Id
            count = 0;
            foreach (var cellLocal in grid.CellList)
            {
                count += 1;
                cellLocal.Id = count;
            }

            // Cell.IsSelect
            foreach (var cellLocal in grid.CellList)
            {
                Grid2RowState rowState = grid.RowStateList[cellLocal.RowStateId - 1];
                cellLocal.IsSelect = rowState.IsSelect;
            }
        }

        /// <summary>
        /// Load (full) with config.
        /// </summary>
        private static async Task LoadFullAsync(Grid2 grid, Page page, IQueryable query)
        {
            // Get config grid and field query
            Page.GridConfigResult gridConfigResult = new Page.GridConfigResult();
            page.Grid2QueryConfig(grid, UtilDalType.TypeRowToTableNameCSharp(grid.TypeRow), gridConfigResult);

            // Load config grid
            grid.ConfigGridList = await Data.SelectAsync(gridConfigResult.ConfigGridQuery);
            var configGrid = ConfigGrid(grid);
            query = Data.QuerySkipTake(query, 0, ConfigRowCountMax(configGrid));

            // Load config field (Task)
            var configFieldListTask = Data.SelectAsync(gridConfigResult.ConfigFieldQuery);

            // Load row (Task)
            var rowListTask = Data.SelectAsync(query);

            await Task.WhenAll(configFieldListTask, rowListTask); // Load config field and row in parallel

            // Load config field
            grid.ConfigFieldList = configFieldListTask.Result;

            // RowList
            grid.RowList = rowListTask.Result;

            // ColumnList
            grid.ColumnList = LoadColumnList(grid);
        }

        /// <summary>
        /// Load (reload)
        /// </summary>
        private static async Task LoadReloadAsync(Grid2 grid, IQueryable query)
        {
            var configGrid = ConfigGrid(grid);

            // Filter
            if (grid.FilterValueList != null)
            {
                foreach (var filter in grid.FilterValueList)
                {
                    if (!filter.IsClear)
                    {
                        query = Data.QueryFilter(query, filter.FieldNameCSharp, filter.FilterValue, filter.FilterOperator);
                    }
                }
            }

            // Sort
            if (grid.SortValueList != null)
            {
                IOrderedQueryable queryOrder = null;
                bool isFirst = true;
                foreach (var value in grid.SortValueList)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                        queryOrder = Data.QueryOrderBy(query, value.FieldNameCSharp, value.IsSort); ;
                    }
                    else
                    {
                        queryOrder = Data.QueryOrderByThenBy(queryOrder, value.FieldNameCSharp, value.IsSort); ;
                    }
                }
                if (queryOrder != null)
                {
                    query = queryOrder;
                }
            }

            // Skip, Take
            query = Data.QuerySkipTake(query, grid.OffsetRow, ConfigRowCountMax(configGrid));

            // Load row
            grid.RowList = await Data.SelectAsync(query);
        }

        /// <summary>
        /// Load or reload data grid.
        /// </summary>
        public static async Task LoadAsync(Grid2 grid)
        {
            Page page = grid.ComponentOwner<Page>();
            IQueryable query = page.Grid2Query(grid);
            Type typeRowOld = grid.TypeRow;
            grid.TypeRow = query?.ElementType;
            grid.DatabaseEnum = DatabaseMemoryInternal.DatabaseEnum(query);

            if (grid.TypeRow == null)
            {
                grid.ColumnList = new List<Grid2Column>(); ;
                grid.RowList = new List<Row>();
                grid.CellList = new List<Grid2Cell>();
                Render(grid);
                return;
            }

            // Load full (with config) or reload
            if (typeRowOld != query?.ElementType)
            {
                // ColumnList, RowList
                await LoadFullAsync(grid, page, query);
            }
            else
            {
                // RowList
                await LoadReloadAsync(grid, query);
            }

            // RowStateList
            grid.RowStateList = LoadRowStateList(grid);

            await RowSelectAsync(grid, grid.RowStateList.Where(item => item.RowEnum == GridRowEnum.Index).FirstOrDefault());

            grid.CellList = new List<Grid2Cell>();

            Render(grid);
        }

        /// <summary>
        /// Returns data grid configuration record.
        /// </summary>
        private static FrameworkConfigGridBuiltIn ConfigGrid(Grid2 grid)
        {
            var result = grid.ConfigGridList.Where(item => item.ConfigName == grid.ConfigName).SingleOrDefault(); // LINQ to memory
            return result;
        }

        /// <summary>
        /// Returns RowCountMax rows to load.
        /// </summary>
        private static int ConfigRowCountMax(FrameworkConfigGridBuiltIn configGrid)
        {
            return configGrid.RowCountMax == null ? 10 : configGrid.RowCountMax.Value; // By default load 10 rows.
        }

        /// <summary>
        /// Returns ColumnCountMax of columns to render.
        /// </summary>
        private static int ConfigColumnCountMax(FrameworkConfigGridBuiltIn configGrid)
        {
            return 3;
        }

        /// <summary>
        /// Returns data grid field configuration records. (FieldName, FrameworkConfigFieldBuiltIn).
        /// </summary>
        private static Dictionary<string, FrameworkConfigFieldBuiltIn> ConfigFieldDictionary(Grid2 grid)
        {
            Dictionary<string, FrameworkConfigFieldBuiltIn> result = grid.ConfigFieldList.Where(item => item.ConfigName == grid.ConfigName).ToDictionary(item => item.FieldNameCSharp); // LINQ to memory
            return result;
        }

        /// <summary>
        /// After load, if no row in data grid is selected, select first row.
        /// </summary>
        private static async Task RowSelectAsync(Grid2 grid, Grid2RowState rowState)
        {
            foreach (var item in grid.RowStateList)
            {
                item.IsSelect = false;
            }
            if (rowState != null)
            {
                Row row = grid.RowList[rowState.RowId.Value - 1];
                rowState.IsSelect = true;
                Page page = grid.ComponentOwner<Page>();
                await page.GridRowSelectedAsync(grid, row);
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

            // IsTextLeave
            ProcessIsTextLeave();
        }

        private static async Task ProcessIsClickSortAsync()
        {
            if (UtilSession.Request(RequestCommand.Grid2IsClickSort, out RequestJson requestJson, out Grid2 grid))
            {
                Grid2Cell cell = grid.CellList[requestJson.Grid2CellId - 1];
                Grid2Column column = grid.ColumnList[cell.ColumnId - 1];

                Grid2SortValue.IsSortSwitch(grid, column.FieldNameCSharp);

                await LoadAsync(grid);
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

        /// <summary>
        /// Synchronize Cell.Text when user leaves field.
        /// </summary>
        private static void ProcessIsTextLeave()
        {
            if (UtilSession.Request(RequestCommand.Grid2IsTextLeave, out RequestJson requestJson, out Grid2 grid))
            {
                Grid2Cell cell = grid.CellList[requestJson.Grid2CellId - 1];
                cell.Text = cell.TextLeave;
                cell.TextLeave = null;
            }
        }

        private static void ProcessCellIsModifyWarning(Grid2 grid, Grid2Cell cell)
        {
            foreach (var item in grid.CellList)
            {
                if (item.RowStateId == cell.RowStateId)
                {
                    if (item.IsModified)
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
                string errorParse = null;
                page.GridCellParse(grid, rowNew, column.FieldNameCSharp, UtilFramework.StringEmpty(cell.Text), out isHandled, ref errorParse); // Custom parse of user entered text.
                // Parse default
                if (!isHandled)
                {
                    Data.CellTextParse(field, cell.Text, rowNew, out errorParse);
                }
                cell.ErrorParse = errorParse;
            }
            catch (Exception exception)
            {
                cell.ErrorParse = UtilFramework.ExceptionToString(exception);
            }
        }

        /// <summary>
        /// Parse
        /// </summary>
        private static void ProcessCellIsModifyParseFilter(Grid2 grid, Page page, Grid2Column column, Field field, Grid2Cell cell)
        {
            cell.ErrorParse = null;
            // Parse
            try
            {
                // Parse custom
                bool isHandled = false;
                string errorParse = null;
                page.GridCellParseFilter(grid, column.FieldNameCSharp, UtilFramework.StringEmpty(cell.Text), new Grid2Filter(grid), out isHandled, ref errorParse); // Custom parse of user entered text.
                if (!isHandled)
                {
                    Data.CellTextParseFilter(field, cell.Text, new Grid2Filter(grid), out errorParse); // Parse default
                }
                cell.ErrorParse = errorParse;
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
        /// Set Text and preserve TextOld.
        /// </summary>
        private static void ProcessCellIsModifyText(Grid2Cell cell, RequestJson requestJson)
        {
            string textOld = cell.Text;
            cell.Text = requestJson.Grid2CellText;
            if (cell.IsModified == false && cell.Text != textOld)
            {
                cell.IsModified = true;
                cell.TextOld = textOld;
            }
            if (cell.IsModified == true && cell.Text == cell.TextOld)
            {
                cell.IsModified = false;
                cell.TextOld = null;
            }
        }

        /// <summary>
        /// Update IsModify flag.
        /// </summary>
        private static void ProcessCellIsModifyUpdate(Grid2 grid)
        {
            foreach (var cell in grid.CellList)
            {
                if (cell.IsModified && cell.Text == cell.TextOld || cell.TextLeave == cell.TextOld)
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
                    Render(grid);
                    // RenderRowIsSelectedUpdate(grid);
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
                ProcessCellIsModifyText(cell, requestJson);

                cell.Warning = null;

                // Parse Filter
                if (rowState.RowEnum == GridRowEnum.Filter)
                {
                    new Grid2Filter(grid).TextSet(column.FieldNameCSharp, cell.Text); // Used after data grid reload to restore filter.

                    // Parse
                    ProcessCellIsModifyParseFilter(grid, page, column, field, cell);
                    if (!ProcessCellIsModifyIsErrorParse(grid, cell))
                    {
                        // Reload
                        await LoadAsync(grid);
                    }
                }

                // Parse Index
                if (rowState.RowEnum == GridRowEnum.Index)
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
                    Render(grid, cell); // Set Text
                    ProcessCellIsModifyUpdate(grid); // Update IsModify
                    ProcessCellIsModifyWarning(grid, cell);
                }

                // Parse New
                if (rowState.RowEnum == GridRowEnum.New)
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
                            grid.RowStateList.Add(new Grid2RowState { Id = grid.RowStateList.Count + 1, RowEnum = GridRowEnum.New });

                            ProcessCellIsModifyReset(grid, cell);
                            Render(grid, cell); // Set Text
                            ProcessCellIsModifyUpdate(grid); // Update IsModify
                            await RowSelectAsync(grid, rowState); // TODO Add to command queue
                            Render(grid);
                        }
                    }
                    ProcessCellIsModifyWarning(grid, cell);
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
                    // Reset filter, sort
                    grid.FilterValueList = null;
                    grid.SortValueList = null;
                    grid.OffsetRow = 0;
                    grid.OffsetColumn = 0;

                    await LoadAsync(grid);
                }

                // Grid page up
                if (requestJson.GridIsClickEnum == GridIsClickEnum.PageUp)
                {
                    var configGrid = ConfigGrid(grid);
                    grid.OffsetRow -= ConfigRowCountMax(configGrid);
                    if (grid.OffsetRow < 0)
                    {
                        grid.OffsetRow = 0;
                    }
                    await LoadAsync(grid);
                }

                // Grid page down
                if (requestJson.GridIsClickEnum == GridIsClickEnum.PageDown)
                {
                    var configGrid = ConfigGrid(grid);
                    int rowCount = grid.RowList.Count;
                    if (rowCount == configGrid.RowCountMax) // Page down further on full grid only.
                    {
                        grid.OffsetRow += ConfigRowCountMax(configGrid);
                    }
                    await LoadAsync(grid);
                }

                // Grid page left
                if (requestJson.GridIsClickEnum == GridIsClickEnum.PageLeft)
                {
                    grid.OffsetColumn -= 1;
                    if (grid.OffsetColumn < 0)
                    {
                        grid.OffsetColumn = 0;
                    }
                    Render(grid);
                }

                // Grid page right
                if (requestJson.GridIsClickEnum == GridIsClickEnum.PageRight)
                {
                    grid.OffsetColumn += 1;
                    Render(grid);
                }
            }
        }
    }

    /// <summary>
    /// Wrapper providing value store functions.
    /// </summary>
    public class Grid2Filter
    {
        internal Grid2Filter(Grid2 grid)
        {
            this.Grid = grid;
        }

        internal readonly Grid2 Grid;

        /// <summary>
        /// Returns filter value for field.
        /// </summary>
        private Grid2FilterValue FilterValue(string fieldNameCSharp)
        {
            Grid2FilterValue result = Grid.FilterValueList.Where(item => item.FieldNameCSharp == fieldNameCSharp).SingleOrDefault();
            if (result == null)
            {
                result = new Grid2FilterValue(fieldNameCSharp);
                Grid.FilterValueList.Add(result);
            }
            return result;
        }

        /// <summary>
        /// Set filter value on a column. If text is not equal to text user entered, it will appear as soon as user leves field.
        /// </summary>
        /// <param name="isClear">If true, filter is not applied.</param>
        public void ValueSet(string fieldNameCSharp, object filterValue, FilterOperator filterOperator, string text, bool isClear = false)
        {
            Grid2FilterValue result = FilterValue(fieldNameCSharp);
            result.FilterValue = filterValue;
            result.FilterOperator = filterOperator;
            if (result.IsFocus == false)
            {
                result.Text = text;
            }
            else
            {
                result.TextLeave = text;
            }
            result.IsClear = isClear;
        }

        internal void TextSet(string fieldNameCSharp, string text)
        {
            Grid.FilterValueList.ForEach(item => item.IsFocus = false);
            Grid2FilterValue result = FilterValue(fieldNameCSharp);
            result.Text = text;
            result.IsFocus = true;
        }

        /// <summary>
        /// (FieldNameCSharp, FilterValue).
        /// </summary>
        internal Dictionary<string, Grid2FilterValue> FilterValueList()
        {
            var result = new Dictionary<string, Grid2FilterValue>();
            if (Grid.FilterValueList != null)
            {
                foreach (var item in Grid.FilterValueList)
                {
                    result.Add(item.FieldNameCSharp, item);
                }
            }
            return result;
        }
    }
}
