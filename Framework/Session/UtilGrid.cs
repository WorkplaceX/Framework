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
    using static Framework.Json.Page;

    public enum GridRowEnum
    {
        None = 0,

        /// <summary>
        /// Filter row where user enters search text.
        /// </summary>
        Filter = 1,

        /// <summary>
        /// Data row loaded from database.
        /// </summary>
        Index = 2,

        /// <summary>
        /// Data row not yet inserted into database.
        /// </summary>
        New = 3,

        /// <summary>
        /// Data row at the end of the grid showing total.
        /// </summary>
        Total = 4
    }

    /// <summary>
    /// Grid load and process.
    /// </summary>
    internal static class UtilGrid
    {
        /// <summary>
        /// Returns ColumnList for data grid.
        /// </summary>
        private static List<GridColumn> LoadColumnList(Grid grid)
        {
            Page page = grid.ComponentOwner<Page>();
            var configFieldDictionary = ConfigFieldDictionary(grid);
            AppJson appJson = page.ComponentOwner<AppJson>();

            var result = new List<GridColumn>();
            var fieldList = UtilDalType.TypeRowToFieldListDictionary(grid.TypeRow);
            foreach (var propertyInfo in UtilDalType.TypeRowToPropertyInfoList(grid.TypeRow))
            {
                var field = fieldList[propertyInfo.Name];
                configFieldDictionary.TryGetValue(propertyInfo.Name, out FrameworkConfigFieldBuiltIn configField);
                NamingConvention namingConvention = appJson.NamingConventionInternal(grid.TypeRow);
                string columnText = namingConvention.ColumnTextInternal(grid.TypeRow, propertyInfo.Name, configField?.Text);
                bool isVisible = namingConvention.ColumnIsVisibleInternal(grid.TypeRow, propertyInfo.Name, configField?.IsVisible);
                double sort = namingConvention.ColumnSortInternal(grid.TypeRow, propertyInfo.Name, field, configField?.Sort);
                result.Add(new GridColumn
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
        private static List<GridRowState> LoadRowStateList(Grid grid)
        {
            var result = new List<GridRowState>();
            result.Add(new GridRowState { RowId = null, RowEnum = GridRowEnum.Filter });
            int rowId = 0;
            foreach (var row in grid.RowList)
            {
                rowId += 1;
                result.Add(new GridRowState { RowId = rowId, RowEnum = GridRowEnum.Index });
            }
            result.Add(new GridRowState { RowId = null, RowEnum = GridRowEnum.New });
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

        private static void RenderAnnotation(Grid gird, GridCell cell, Page page, string fieldNameCSharp, GridRowEnum rowEnum, Row row)
        {
            var result = new GridCellAnnotationResult();
            page.GridCellAnnotation(gird, fieldNameCSharp, rowEnum, row, result);
            cell.Html = UtilFramework.StringNull(result.Html);
            cell.HtmlIsEdit = result.HtmlIsEdit;
            cell.HtmlLeft = UtilFramework.StringNull(result.HtmlLeft);
            cell.HtmlRight = UtilFramework.StringNull(result.HtmlRight);
            cell.IsReadOnly = result.IsReadOnly;
            cell.IsPassword = result.IsPassword;
            cell.Align = result.Align;
        }

        /// <summary>
        /// Render Grid.CellList.
        /// </summary>
        /// <param name="cell">If not null, method GridCellText(); is called for all cells on this data row.</param>
        private static void Render(Grid grid, GridCell cell = null, bool isTextLeave = true)
        {
            UtilFramework.LogDebug(string.Format("RENDER ({0}) IsCell={1};", grid.TypeRow?.Name, cell != null));

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
            grid.CellList = new List<GridCell>();
            foreach (var cellLocal in cellList.Values)
            {
                cellLocal.IsVisibleScroll = false;
            }

            // Render StyleColumn
            StringBuilder styleColumnList = new StringBuilder();
            bool isFirst = true;
            int widthEndsWithPxCount = 0;
            foreach (var column in columnList)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    styleColumnList.Append(" ");
                }
                string width = column.Width != null ? width = column.Width : width = "minmax(0, 1fr)";
                if (width.EndsWith("px"))
                {
                    widthEndsWithPxCount += 1;
                }
                if (columnList.Count == widthEndsWithPxCount) // Set last column to dynamic, if all columns have a fix width defined.
                {
                    width = "minmax(0, 1fr)";
                }
                styleColumnList.Append(width);
            }
            grid.StyleColumn = styleColumnList.ToString();

            // Render Cell
            Page page = grid.ComponentOwner<Page>();
            var fieldList = UtilDalType.TypeRowToFieldListDictionary(grid.TypeRow);
            var filter = new GridFilter(grid);
            var filterValueList = filter.FilterValueList();
            foreach (var rowState in rowStateList)
            {
                // Render Filter
                if (rowState.RowEnum == GridRowEnum.Filter)
                {
                    // Filter Header
                    foreach (var column in columnList)
                    {
                        var cellLocal = cellList.GetOrAdd((column.Id, rowState.Id, GridCellEnum.HeaderColumn), (key) => new GridCell
                        {
                            ColumnId = key.Item1,
                            RowStateId = key.Item2,
                            CellEnum = key.Item3,
                            ColumnText = column.ColumnText,
                            Description = column.Description,
                        });
                        grid.CellList.Add(cellLocal);
                        cellLocal.IsSort = GridSortValue.IsSortGet(grid, column.FieldNameCSharp);
                        cellLocal.Width = column.Width;
                        cellLocal.IsVisibleScroll = true;
                    }
                    // Filter Value
                    foreach (var column in columnList)
                    {
                        filterValueList.TryGetValue(column.FieldNameCSharp, out GridFilterValue filterValue);
                        var cellLocal = cellList.GetOrAdd((column.Id, rowState.Id, GridCellEnum.Filter), (key) => new GridCell
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

                // Render Index
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
                        var cellLocal = cellList.GetOrAdd((column.Id, rowState.Id, GridCellEnum.Index), (key) => new GridCell
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
                            cellLocal.TextLeave = null;
                            if (cellLocal.ErrorParse == null) // Do not override user entered text as long as in ErrorParse mode.
                            {
                                if (cellLocal == cell && isTextLeave)
                                {
                                    cellLocal.TextLeave = UtilFramework.StringEmpty(text); // Do not change text while user modifies.
                                }
                                else
                                {
                                    cellLocal.Text = text;
                                }
                            }
                            RenderAnnotation(grid, cellLocal, page, column.FieldNameCSharp, rowState.RowEnum, row);
                        }
                        cellLocal.IsVisibleScroll = true;
                        if (grid.GridLookup != null)
                        {
                            if (grid.GridLookupDestRowStateId == rowState.Id && grid.GridLookupDestFieldNameCSharp == column.FieldNameCSharp)
                            {
                                cellLocal.GridLookup = grid.GridLookup;
                            }
                        }
                    }
                }

                // Render New
                if (rowState.RowEnum == GridRowEnum.New)
                {
                    foreach (var column in columnList)
                    {
                        var cellLocal = cellList.GetOrAdd((column.Id, rowState.Id, GridCellEnum.New), (key) => new GridCell
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
                GridRowState rowState = grid.RowStateList[cellLocal.RowStateId - 1];
                cellLocal.IsSelect = rowState.IsSelect;
            }
        }

        /// <summary>
        /// Load (full) data grid with config.
        /// </summary>
        private static async Task LoadFullAsync(Grid grid, IQueryable query, Page page)
        {
            UtilFramework.Assert(grid.TypeRow == query.ElementType);
            // Get config grid and field query
            Page.GridConfigResult gridConfigResult = new Page.GridConfigResult();
            if (grid.IsGridLookup == false)
            {
                page.GridQueryConfig(grid, UtilDalType.TypeRowToTableNameCSharp(grid.TypeRow), gridConfigResult);
            }
            else
            {
                page.GridLookupQueryConfig(grid, UtilDalType.TypeRowToTableNameCSharp(grid.TypeRow), gridConfigResult);
            }

            // Load config grid
            grid.ConfigGridList = new List<FrameworkConfigGridBuiltIn>();
            if (gridConfigResult.ConfigGridQuery != null)
            {
                grid.ConfigGridList = await Data.SelectAsync(gridConfigResult.ConfigGridQuery);
            }
            var configGrid = ConfigGrid(grid);
            query = Data.QuerySkipTake(query, 0, ConfigRowCountMax(configGrid));

            // Load config field (Task)
            var configFieldListTask = Task.FromResult(new List<FrameworkConfigFieldBuiltIn>());
            if (gridConfigResult.ConfigFieldQuery != null)
            {
                configFieldListTask = Data.SelectAsync(gridConfigResult.ConfigFieldQuery);
            }

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
        /// Load (reload) data grid. Config is not loaded again.
        /// </summary>
        private static async Task LoadReloadAsync(Grid grid, IQueryable query)
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
        /// Load or reload data grid. Also first load for lookup.
        /// </summary>
        private static async Task LoadAsync(Grid grid, IQueryable query, Page page)
        {
            Type typeRowOld = grid.TypeRow;
            grid.TypeRow = query?.ElementType;
            grid.DatabaseEnum = DatabaseMemoryInternal.DatabaseEnum(query);

            if (grid.TypeRow == null)
            {
                grid.ColumnList = new List<GridColumn>(); ;
                grid.RowStateList = new List<GridRowState>();
                grid.RowList = new List<Row>();
                grid.CellList = new List<GridCell>();
                grid.GridLookup = null;
                Render(grid);
                return;
            }

            // Load full (with config) or reload
            if (typeRowOld != query?.ElementType)
            {
                // ColumnList, RowList
                await LoadFullAsync(grid, query, page);
            }
            else
            {
                // RowList
                await LoadReloadAsync(grid, query);
            }

            // RowStateList
            grid.RowStateList = LoadRowStateList(grid);

            // Select first row on data grid. But not on lookup grid.
            if (grid.GridLookup == null) 
            {
                await RowSelectAsync(grid, grid.RowStateList.Where(item => item.RowEnum == GridRowEnum.Index).FirstOrDefault());
            }

            grid.CellList = new List<GridCell>();

            Render(grid);
        }

        /// <summary>
        /// Load or reload data grid.
        /// </summary>
        public static async Task LoadAsync(Grid grid)
        {
            Page page = grid.ComponentOwner<Page>();
            IQueryable query;
            if (grid.IsGridLookup == false)
            {
                query = page.GridQuery(grid);
            }
            else
            {
                GridLookupToGridDest(grid, out var gridDest, out var rowDest, out string fieldNameCSharpDest, out var cellDest);
                query = page.GridLookupQuery(gridDest, rowDest, fieldNameCSharpDest, cellDest.Text);
            }
            await LoadAsync(grid, query, page);
        }

        /// <summary>
        /// GridLookup to GridDest. GridDest is the destination grid to write to when user selects a row in lookup grid.
        /// </summary>
        private static void GridLookupToGridDest(Grid gridLookup, out Grid gridDest, out Row rowDest, out string fieldNameCSharpDest, out GridCell cellDest)
        {
            UtilFramework.Assert(gridLookup.IsGridLookup);
            gridDest = gridLookup.GridDest;
            var rowStateDest = gridDest.RowStateList[gridLookup.GridLookupDestRowStateId.Value - 1];
            rowDest = null;
            if (rowStateDest.RowId != null)
            {
                rowDest = gridDest.RowList[rowStateDest.RowId.Value - 1];
            }
            fieldNameCSharpDest = gridLookup.GridLookupDestFieldNameCSharp;
            var fieldNameCSharpDestLocal = fieldNameCSharpDest;
            var columnDest = gridDest.ColumnList.Where(item => item.FieldNameCSharp == fieldNameCSharpDestLocal).Single();
            var rowStateDestLocal = rowStateDest;
            cellDest = gridDest.CellList.Where(item => item.RowStateId == rowStateDestLocal.Id && item.ColumnId == columnDest.Id).Single();
        }

        /// <summary>
        /// Close lookup data grid.
        /// </summary>
        private static void GridLookupClose(Grid grid)
        {
            UtilFramework.Assert(grid.IsGridLookup == false);
            if (grid.GridLookup != null)
            {
                UtilFramework.Assert(grid.GridLookup.IsGridLookup);
                GridLookupToGridDest(grid.GridLookup, out var _, out var _, out var _, out var cellDest);
                cellDest.GridLookup = null;
                grid.GridLookup = null;
            }
        }

        /// <summary>
        /// Open lookup data grid.
        /// </summary>
        private static void GridLookupOpen(Grid grid, GridRowState rowState, string fieldNameCSharp, GridCell cell)
        {
            if (grid.GridLookup == null)
            {
                UtilFramework.Assert(cell.GridLookup == null);
                Grid gridLookup = new Grid(grid);
                grid.GridLookup = gridLookup;
                cell.GridLookup = gridLookup;

                gridLookup.IsHide = true; // Render data grid to cell. See also property GridCell.GridLookup
                gridLookup.IsGridLookup = true;
                grid.GridLookup.GridDest = grid;
                gridLookup.GridLookupDestRowStateId = rowState.Id;
                gridLookup.GridLookupDestFieldNameCSharp = fieldNameCSharp;
            }
            UtilFramework.Assert(grid.GridLookup.IsGridLookup);
            UtilFramework.Assert(grid.GridLookup.GridLookupDestRowStateId == rowState.Id && cell.RowStateId == rowState.Id);
            UtilFramework.Assert(grid.GridLookup.GridLookupDestFieldNameCSharp == fieldNameCSharp && grid.ColumnList[cell.ColumnId - 1].FieldNameCSharp == fieldNameCSharp);
            UtilFramework.Assert(grid.GridLookup.GridDest == grid);
        }

        /// <summary>
        /// Returns data grid configuration record.
        /// </summary>
        private static FrameworkConfigGridBuiltIn ConfigGrid(Grid grid)
        {
            var result = grid.ConfigGridList.Where(item => item.ConfigName == grid.ConfigName).SingleOrDefault(); // LINQ to memory
            return result;
        }

        /// <summary>
        /// Returns RowCountMax rows to load.
        /// </summary>
        private static int ConfigRowCountMax(FrameworkConfigGridBuiltIn configGrid)
        {
            return configGrid?.RowCountMax == null ? 10 : configGrid.RowCountMax.Value; // By default load 10 rows.
        }

        /// <summary>
        /// Returns ColumnCountMax of columns to render.
        /// </summary>
        private static int ConfigColumnCountMax(FrameworkConfigGridBuiltIn configGrid)
        {
            return 5;
        }

        /// <summary>
        /// Returns data grid field configuration records. (FieldName, FrameworkConfigFieldBuiltIn).
        /// </summary>
        private static Dictionary<string, FrameworkConfigFieldBuiltIn> ConfigFieldDictionary(Grid grid)
        {
            Dictionary<string, FrameworkConfigFieldBuiltIn> result = grid.ConfigFieldList.Where(item => item.ConfigName == grid.ConfigName).ToDictionary(item => item.FieldNameCSharp); // LINQ to memory
            return result;
        }

        /// <summary>
        /// After load, if no row in data grid is selected, select first row.
        /// </summary>
        public static async Task RowSelectAsync(Grid grid, GridRowState rowState, bool isRender = false)
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
                UtilFramework.Assert(row == grid.RowSelected);
                await page.GridRowSelectedAsync(grid);
            }

            if (isRender)
            {
                Render(grid);
            }
        }

        /// <summary>
        /// User modified text, now open lookup window.
        /// </summary>
        private static async Task ProcessGridLookupOpenAsync(Grid grid, Page page, GridRowState rowState, GridColumn column, GridCell cell)
        {
            var query = page.GridLookupQuery(grid, rowState.RowNew, column.FieldNameCSharp, cell.Text);
            if (query != null)
            {
                GridLookupOpen(grid, rowState, column.FieldNameCSharp, cell);
                await LoadAsync(grid.GridLookup, query, page);
            }
        }

        /// <summary>
        /// Process incoming RequestJson.
        /// </summary>
        public static async Task ProcessAsync(AppJson appJson)
        {
            // IsClickSort (user clicked column sort)
            await ProcessIsClickSortAsync(appJson);

            // CellIsModify (user changed text)
            await ProcessCellIsModify(appJson);

            // IsClickEnum (user clicked paging button)
            await ProcessIsClickEnum(appJson);

            // RowIsClick (user clicked data row)
            await ProcessRowIsClickAsync(appJson);

            // IsClickConfig (user clicked column configuration button)
            await ProcessIsClickConfigAsync(appJson);

            // IsTextLeave (user clicked tab button to leave cell)
            ProcessIsTextLeave(appJson);

            // StyleColumn (user changed with mouse column width)
            ProcessStyleColumn(appJson);
        }

        /// <summary>
        /// User clicked column sort.
        /// </summary>
        private static async Task ProcessIsClickSortAsync(AppJson appJson)
        {
            if (UtilSession.Request(appJson, RequestCommand.GridIsClickSort, out RequestJson requestJson, out Grid grid))
            {
                GridCell cell = grid.CellList[requestJson.GridCellId - 1];
                GridColumn column = grid.ColumnList[cell.ColumnId - 1];

                GridSortValue.IsSortSwitch(grid, column.FieldNameCSharp);
                grid.OffsetRow = 0; // Page to first row after user clicked column sort.

                await LoadAsync(grid);
            }
        }

        /// <summary>
        /// User clicked column header configuration icon.
        /// </summary>
        private static async Task ProcessIsClickConfigAsync(AppJson appJson)
        {
            if (UtilSession.Request(appJson, RequestCommand.GridIsClickConfig, out RequestJson requestJson, out Grid grid))
            {
                GridCell cell = grid.CellList[requestJson.GridCellId - 1];
                GridColumn column = grid.ColumnList[cell.ColumnId - 1];
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
        private static void ProcessIsTextLeave(AppJson appJson)
        {
            if (UtilSession.Request(appJson, RequestCommand.GridIsTextLeave, out RequestJson requestJson, out Grid grid))
            {
                GridCell cell = grid.CellList[requestJson.GridCellId - 1];
                cell.Text = cell.TextLeave;
                cell.TextLeave = null;
            }
        }

        /// <summary>
        /// Send column width to server after user resizes a column.
        /// </summary>
        private static void ProcessStyleColumn(AppJson appJson)
        {
            if (UtilSession.Request(appJson, RequestCommand.GridStyleColumn, out RequestJson requestJson, out Grid grid))
            {
                var columnList = grid.ColumnList.Where(item => item.IsVisibleScroll).ToArray();
                for (int i = 0; i < columnList.Length; i++)
                {
                    var column = columnList[i];
                    var width = requestJson.GridStyleColumnList[i];
                    column.Width = width;
                }
                Render(grid); // Update Grid.StyleColumn
            }
        }

        /// <summary>
        /// Show warning message on cell if row could not be saved.
        /// </summary>
        private static void ProcessCellIsModifyWarningNotSaved(Grid grid, GridCell cell)
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
        private static void ProcessCellIsModifyReset(Grid grid, GridCell cell)
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
        private static async Task ProcessCellIsModifyParseAsync(Grid grid, Page page, Row rowNew, GridColumn column, Field field, GridCell cell)
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
                if (isHandled == false)
                {
                    var result = await page.GridCellParseAsync(grid, rowNew, column.FieldNameCSharp, UtilFramework.StringEmpty(cell.Text));
                    isHandled = result.isHandled;
                    errorParse = result.errorParse;
                }
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
        private static void ProcessCellIsModifyParseFilter(Grid grid, Page page, GridColumn column, Field field, GridCell cell)
        {
            cell.ErrorParse = null;
            // Parse
            try
            {
                // Parse custom
                bool isHandled = false;
                string errorParse = null;
                page.GridCellParseFilter(grid, column.FieldNameCSharp, UtilFramework.StringEmpty(cell.Text), new GridFilter(grid), out isHandled, ref errorParse); // Custom parse of user entered text.
                if (!isHandled)
                {
                    Data.CellTextParseFilter(field, cell.Text, new GridFilter(grid), out errorParse); // Parse default
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
        private static async Task ProcessCellIsModifyUpdateAsync(Grid grid, Page page, Row row, Row rowNew, GridCell cell)
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
        private static async Task ProcessCellIsModifyInsertAsync(Grid grid, Page page, Row rowNew, GridCell cell)
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
        private static void ProcessCellIsModifyErrorSaveReset(Grid grid, GridCell cell)
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
        private static bool ProcessCellIsModifyIsErrorParse(Grid grid, GridCell cell)
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
        private static void ProcessCellIsModifyText(GridCell cell, RequestJson requestJson)
        {
            string textOld = cell.Text;
            cell.Text = requestJson.GridCellText;
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
        private static void ProcessCellIsModifyUpdate(Grid grid)
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
        private static async Task ProcessRowIsClickAsync(AppJson appJson)
        {
            if (UtilSession.Request(appJson, RequestCommand.GridIsClickRow, out RequestJson requestJson, out Grid grid))
            {
                GridCell cell = grid.CellList[requestJson.GridCellId - 1];
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
                    Page page = grid.ComponentOwner<Page>();
                    if (grid.IsGridLookup == false)
                    {
                        // Grid normal row selected
                        GridLookupClose(grid);
                        Render(grid);
                        UtilFramework.Assert(rowSelected == grid.RowSelected);
                        await page.GridRowSelectedAsync(grid); // Load detail data grid
                    }
                    else
                    {
                        // Grid lookup row selected
                        GridLookupClose(grid.GridDest);
                        UtilFramework.Assert(rowSelected == grid.RowSelected);
                        string text = page.GridLookupRowSelected(grid);
                        if (text != null)
                        {
                            GridLookupToGridDest(grid, out var gridDest, out var rowDest, out var _, out var cellDest);
                            appJson.RequestJson = new RequestJson { Command = RequestCommand.GridCellIsModify, ComponentId = gridDest.Id, GridCellId = cellDest.Id, GridCellText = text, GridCellTextIsLookup = true };

                            await ProcessCellIsModify(appJson);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// User modified text.
        /// </summary>
        private static async Task ProcessCellIsModify(AppJson appJson)
        {
            if (UtilSession.Request(appJson, RequestCommand.GridCellIsModify, out RequestJson requestJson, out Grid grid))
            {
                GridCell cell = grid.CellList[requestJson.GridCellId - 1];
                GridColumn column = grid.ColumnList[cell.ColumnId - 1];
                var field = UtilDalType.TypeRowToFieldListDictionary(grid.TypeRow)[column.FieldNameCSharp];
                Page page = grid.ComponentOwner<Page>();
                GridRowState rowState = grid.RowStateList[cell.RowStateId - 1];

                // Track IsModified
                ProcessCellIsModifyText(cell, requestJson);

                cell.Warning = null;

                // Parse Filter
                if (rowState.RowEnum == GridRowEnum.Filter)
                {
                    new GridFilter(grid).TextSet(column.FieldNameCSharp, cell.Text); // Used after data grid reload to restore filter.

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
                    await ProcessCellIsModifyParseAsync(grid, page, rowState.RowNew, column, field, cell);
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
                    // Lookup
                    if (!requestJson.GridCellTextIsLookup) // Do not open lookup again after lookup row has been clicked by user.
                    {
                        await ProcessGridLookupOpenAsync(grid, page, rowState, column, cell);
                    }

                    // Do not set Cell.TextLeave if user clicked lookup row.
                    bool isTextLeave = true;
                    if (requestJson.GridCellTextIsLookup)
                    {
                        isTextLeave = false;
                    }

                    Render(grid, cell, isTextLeave); // Call method GridCellText(); for all cells on this data row
                    ProcessCellIsModifyUpdate(grid); // Update IsModify
                    ProcessCellIsModifyWarningNotSaved(grid, cell); // Not saved warning
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
                    await ProcessCellIsModifyParseAsync(grid, page, rowState.RowNew, column, field, cell);
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
                            grid.RowStateList.Add(new GridRowState { Id = grid.RowStateList.Count + 1, RowEnum = GridRowEnum.New });

                            ProcessCellIsModifyReset(grid, cell);
                            await RowSelectAsync(grid, rowState);
                        }
                    }
                    // Lookup
                    if (!requestJson.GridCellTextIsLookup) // Do not open lookup again after lookup row has been clicked by user.
                    {
                        await ProcessGridLookupOpenAsync(grid, page, rowState, column, cell);
                    }

                    // Do not set Cell.TextLeave if user clicked lookup row.
                    bool isTextLeave = true;
                    if (requestJson.GridCellTextIsLookup)
                    {
                        isTextLeave = false;
                    }

                    Render(grid, cell, isTextLeave); // Call method GridCellText(); for all cells on this data row
                    ProcessCellIsModifyUpdate(grid); // Update IsModify
                    ProcessCellIsModifyWarningNotSaved(grid, cell); // Not saved warning
                }
            }
        }

        /// <summary>
        /// User clicked paging button.
        /// </summary>
        private static async Task ProcessIsClickEnum(AppJson appJson)
        {
            if (UtilSession.Request(appJson, RequestCommand.GridIsClickEnum, out RequestJson requestJson, out Grid grid))
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
                    int rowCountMax = ConfigRowCountMax(configGrid);
                    if (rowCount == rowCountMax) // Page down further on full grid only.
                    {
                        grid.OffsetRow += rowCountMax;
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
                    if (grid.OffsetColumn + ConfigColumnCountMax(ConfigGrid(grid)) < grid.ColumnList.Count)
                    {
                        grid.OffsetColumn += 1;
                        Render(grid);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Wrapper providing value store functions.
    /// </summary>
    public class GridFilter
    {
        internal GridFilter(Grid grid)
        {
            this.Grid = grid;
        }

        internal readonly Grid Grid;

        /// <summary>
        /// Returns filter value for field.
        /// </summary>
        private GridFilterValue FilterValue(string fieldNameCSharp)
        {
            GridFilterValue result = Grid.FilterValueList.Where(item => item.FieldNameCSharp == fieldNameCSharp).SingleOrDefault();
            if (result == null)
            {
                result = new GridFilterValue(fieldNameCSharp);
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
            GridFilterValue result = FilterValue(fieldNameCSharp);
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
            GridFilterValue result = FilterValue(fieldNameCSharp);
            result.Text = text;
            result.IsFocus = true;
        }

        /// <summary>
        /// (FieldNameCSharp, FilterValue).
        /// </summary>
        internal Dictionary<string, GridFilterValue> FilterValueList()
        {
            var result = new Dictionary<string, GridFilterValue>();
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
