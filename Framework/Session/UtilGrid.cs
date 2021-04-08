namespace Framework.Session
{
    using Database.dbo;
    using DatabaseIntegrate.dbo;
    using Framework.DataAccessLayer;
    using Framework.DataAccessLayer.DatabaseMemory;
    using Framework.Json;
    using Framework.Server;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using static Framework.DataAccessLayer.UtilDalType;
    using static Framework.Json.Grid;

    internal enum GridRowEnum
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
            var configFieldDictionary = ConfigFieldDictionary(grid);
            AppJson appJson = grid.ComponentOwner<AppJson>();

            var result = new List<GridColumn>();
            var fieldList = UtilDalType.TypeRowToFieldListDictionary(grid.TypeRow);
            foreach (var propertyInfo in UtilDalType.TypeRowToPropertyInfoList(grid.TypeRow))
            {
                var field = fieldList[propertyInfo.Name];
                configFieldDictionary.TryGetValue(propertyInfo.Name, out List<FrameworkConfigFieldIntegrate> configFieldList);
                if (configFieldList == null) // No ConfigField defined
                {
                    configFieldList = new List<FrameworkConfigFieldIntegrate>
                    {
                        null
                    };
                }
                foreach (var configField in configFieldList)
                {
                    NamingConvention namingConvention = appJson.NamingConventionInternal(grid.TypeRow);
                    string columnText = namingConvention.ColumnTextInternal(grid.TypeRow, propertyInfo.Name, configField?.Text);
                    bool isVisible = namingConvention.ColumnIsVisibleInternal(grid.TypeRow, propertyInfo.Name, configField?.IsVisible);
                    bool isReadOnly = namingConvention.ColumnIsReadOnlyInternal(grid.TypeRow, propertyInfo.Name, configField?.IsReadOnly);
                    bool isMultiline = namingConvention.ColumnIsMultilineInternal(grid.TypeRow, propertyInfo.Name, configField?.IsMultiline);
                    double sort = namingConvention.ColumnSortInternal(grid.TypeRow, propertyInfo.Name, field, configField?.Sort);
                    result.Add(new GridColumn
                    {
                        FieldNameCSharp = field.FieldNameCSharp,
                        ColumnText = columnText,
                        Description = configField?.Description,
                        IsVisible = isVisible,
                        IsReadOnly = isReadOnly,
                        IsMultiline = isMultiline,
                        Sort = sort,
                        SortField = field.Sort
                    });
                }
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
            var result = new List<GridRowState>
            {
                new GridRowState { RowId = null, RowEnum = GridRowEnum.Filter }
            };
            int rowId = 0;
            foreach (var row in grid.RowListInternal)
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
            isAdded = false;
            if (!list.TryGetValue(key, out TValue result))
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
            return list.GetOrAdd(key, valueFactory, out bool _);
        }

        private static void RenderAnnotation(Grid grid, GridCell cell, string fieldNameCSharp, GridRowEnum rowEnum, Row row, bool isReadOnly, bool isMultiline)
        {
            var result = new Grid.AnnotationResult { IsReadOnly = isReadOnly, IsMultiline = isMultiline };
            grid.CellAnnotationInternal(rowEnum, row, fieldNameCSharp, result);
            if (isReadOnly)
            {
                // Annotation can not override IsReadOnly if true in configuration.
                result.IsReadOnly = true;
            }
            cell.Html = UtilFramework.StringNull(result.Html);
            cell.HtmlIsEdit = result.HtmlIsEdit;
            cell.HtmlLeft = UtilFramework.StringNull(result.HtmlLeft);
            cell.HtmlRight = UtilFramework.StringNull(result.HtmlRight);
            cell.IsReadOnly = result.IsReadOnly;
            cell.IsMultiline = result.IsMultiline;
            cell.IsPassword = result.IsPassword;
            cell.Align = result.Align;
            cell.IsFileUpload = result.IsFileUpload;
            if (result.PlaceHolder != null)
            {
                cell.Placeholder = result.PlaceHolder; // Override default "Search", "New"
            }
        }

        /// <summary>
        /// Render HeaderColumn.
        /// </summary>
        private static GridCell RenderCellFilterHeaderColumn(Grid grid, GridRowState rowState, GridColumn column, Dictionary<(int, int, GridCellEnum), GridCell> cellList)
        {
            var result = cellList.GetOrAdd((column.Id, rowState.Id, GridCellEnum.HeaderColumn), (key) => new GridCell
            {
                ColumnId = key.Item1,
                RowStateId = key.Item2,
                CellEnum = key.Item3,
                ColumnText = column.ColumnText,
                Description = column.Description,
            });
            grid.CellList.Add(result);
            result.IsSort = GridSortValue.IsSortGet(grid, column.FieldNameCSharp);
            result.IsVisibleScroll = true;

            return result;
        }

        /// <summary>
        /// Render HeaderRow.
        /// </summary>
        private static GridCell RenderCellFilterHeaderRow(Grid grid, GridRowState rowState, GridColumn column, Dictionary<(int, int, GridCellEnum), GridCell> cellList)
        {
            var result = cellList.GetOrAdd((column.Id, rowState.Id, GridCellEnum.HeaderRow), (key) => new GridCell
            {
                ColumnId = key.Item1,
                RowStateId = key.Item2,
                CellEnum = key.Item3,
                ColumnText = column.ColumnText,
            });
            grid.CellList.Add(result);
            result.IsVisibleScroll = true;

            return result;
        }

        /// <summary>
        /// Render Filter (search) value.
        /// </summary>
        private static GridCell RenderCellFilterValue(Grid grid, GridRowState rowState, GridColumn column, Dictionary<(int, int, GridCellEnum), GridCell> cellList, Dictionary<string, GridFilterValue> filterValueList)
        {
            filterValueList.TryGetValue(column.FieldNameCSharp, out GridFilterValue filterValue);
            var result = cellList.GetOrAdd((column.Id, rowState.Id, GridCellEnum.Filter), (key) => new GridCell
            {
                ColumnId = key.Item1,
                RowStateId = key.Item2,
                CellEnum = key.Item3,
                Placeholder = "Search"
            }, out bool isAdded);
            grid.CellList.Add(result);
            result.Text = filterValue?.Text;
            if (column.FieldNameCSharp == filterValue?.FieldNameCSharp && filterValue?.IsFocus == true)
            {
                result.TextLeave = filterValue.TextLeave;
            }
            result.IsVisibleScroll = true;

            if (isAdded)
            {
                RenderAnnotation(grid, result, column.FieldNameCSharp, rowState.RowEnum, null, result.IsReadOnly, result.IsMultiline); // Cell Filter
            }

            return result;
        }

        /// <summary>
        /// Render Cell Index.
        /// </summary>
        private static GridCell RenderCellIndex(Grid grid, GridRowState rowState, GridColumn column, Dictionary<(int, int, GridCellEnum), GridCell> cellList, Dictionary<string, Field> fieldList, GridCell cell, bool isTextLeave)
        {
            Row row;
            if (rowState.Row != null)
            {
                row = rowState.Row;
            }
            else
            {
                row = grid.RowListInternal[rowState.RowId.Value - 1];
            }

            var result = cellList.GetOrAdd((column.Id, rowState.Id, GridCellEnum.Index), (key) => new GridCell
            {
                ColumnId = key.Item1,
                RowStateId = key.Item2,
                CellEnum = key.Item3,
            }, out bool isAdded);
            grid.CellList.Add(result);
            if (result.RowStateId == cell?.RowStateId) // Cell on same row.
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
                    text = field.FrameworkType().CellTextFromValue(value);
                    text = grid.CellTextInternal(row, field.PropertyInfo.Name, text); // Custom convert database value to cell text.
                    text = UtilFramework.StringNull(text);
                }
                result.TextLeave = null;
                if (result.ErrorParse == null) // Do not override user entered text as long as in ErrorParse mode.
                {
                    if (result == cell && isTextLeave)
                    {
                        result.TextLeave = UtilFramework.StringEmpty(text); // Do not change text while user modifies.
                    }
                    else
                    {
                        result.Text = text;
                    }
                }
            }
            result.IsVisibleScroll = true;
            if (grid.GridLookup != null)
            {
                if (grid.GridLookupDestRowStateId == rowState.Id && grid.GridLookupDestFieldNameCSharp == column.FieldNameCSharp)
                {
                    result.GridLookup = grid.GridLookup;
                }
            }

            if (isAdded)
            {
                RenderAnnotation(grid, result, column.FieldNameCSharp, rowState.RowEnum, row, column.IsReadOnly, column.IsMultiline); // Cell
            }

            return result;
        }

        /// <summary>
        /// Render new field.
        /// </summary>
        private static GridCell RenderCellNew(Grid grid, GridRowState rowState, GridColumn column, Dictionary<(int, int, GridCellEnum), GridCell> cellList)
        {
            var result = cellList.GetOrAdd((column.Id, rowState.Id, GridCellEnum.New), (key) => new GridCell
            {
                ColumnId = key.Item1,
                RowStateId = key.Item2,
                CellEnum = key.Item3,
                Placeholder = "New",
            }, out bool isAdded);
            grid.CellList.Add(result);
            result.IsVisibleScroll = true;

            if (isAdded)
            {
                RenderAnnotation(grid, result, column.FieldNameCSharp, rowState.RowEnum, null, result.IsReadOnly, result.IsMultiline); // Cell New
            }

            return result;
        }

        /// <summary>
        /// Render data grid in table mode.
        /// </summary>
        private static void RenderModeTable(Grid grid, FrameworkConfigGridIntegrate configGrid, List<GridColumn> columnList, List<GridRowState> rowStateList, Dictionary<(int, int, GridCellEnum), GridCell> cellList, GridCell cell, bool isTextLeave)
        {
            // Render Grid.StyleColumnList
            grid.StyleColumnList = new List<GridStyleColumn>();
            double widthValueTotal = 0;
            double widthCount = 0;
            foreach (var column in columnList)
            {
                if (column.WidthValue != null)
                {
                    widthCount += 1;
                    widthValueTotal += column.WidthValue.Value;
                }
            }
            double widthValueAvg = (100 - widthValueTotal) / (columnList.Count - widthCount);
            foreach (var column in columnList)
            {
                string width;
                double? widthValue;
                string widthUnit;
                if (column.WidthValue != null)
                {
                    width = column.WidthValue + "%";
                    widthValue = column.WidthValue;
                    widthUnit = "%";
                }
                else
                {
                    width = widthValueAvg + "%";
                    widthValue = widthValueAvg;
                    widthUnit = "%";
                }
                if (column != columnList.Last())
                {
                    grid.StyleColumnList.Add(new GridStyleColumn { Width = width, WidthValue = widthValue, WidthUnit = widthUnit });
                }
                else
                {
                    grid.StyleColumnList.Add(new GridStyleColumn());
                }
            }

            // Render Grid.StyleRowList
            grid.StyleRowList = new List<GridStyleRow>();
            foreach (var rowState in rowStateList)
            {
                switch (rowState.RowEnum)
                {
                    case GridRowEnum.Filter:
                        if ((configGrid?.IsShowHeader).GetValueOrDefault(true))
                        {
                            grid.StyleRowList.Add(new GridStyleRow()); // See also enum GridCellEnum.HeaderColumn
                            grid.StyleRowList.Add(new GridStyleRow()); // Filter value
                        }
                        break;
                    case GridRowEnum.Index:
                        grid.StyleRowList.Add(new GridStyleRow()); // Cell value
                        break;
                    case GridRowEnum.New:
                        if ((configGrid?.IsAllowInsert).GetValueOrDefault(true))
                        {
                            grid.StyleRowList.Add(new GridStyleRow()); // Cell value
                        }
                        break;
                }
            }

            // Render Grid.CellList
            var fieldList = UtilDalType.TypeRowToFieldListDictionary(grid.TypeRow);
            var filter = new GridFilter(grid);
            var filterValueList = filter.FilterValueList();
            foreach (var rowState in rowStateList)
            {
                // Render Filter
                if (rowState.RowEnum == GridRowEnum.Filter && (configGrid?.IsShowHeader).GetValueOrDefault(true))
                {
                    // Filter Header
                    foreach (var column in columnList)
                    {
                        RenderCellFilterHeaderColumn(grid, rowState, column, cellList);
                    }
                    // Filter Value
                    foreach (var column in columnList)
                    {
                        RenderCellFilterValue(grid, rowState, column, cellList, filterValueList);
                    }
                }

                // Render Index
                if (rowState.RowEnum == GridRowEnum.Index)
                {
                    foreach (var column in columnList)
                    {
                        RenderCellIndex(grid, rowState, column, cellList, fieldList, cell, isTextLeave);
                    }
                }

                // Render New
                if (rowState.RowEnum == GridRowEnum.New && (configGrid?.IsAllowInsert).GetValueOrDefault(true))
                {
                    foreach (var column in columnList)
                    {
                        RenderCellNew(grid, rowState, column, cellList);
                    }
                }
            }
        }

        /// <summary>
        /// Render data grid in stack mode.
        /// </summary>
        private static void RenderModeStack(Grid grid, FrameworkConfigGridIntegrate configGrid, List<GridColumn> columnList, List<GridRowState> rowStateList, Dictionary<(int, int, GridCellEnum), GridCell> cellList, GridCell cell, bool isTextLeave)
        {
            // Render Grid.StyleColumnList
            grid.StyleColumnList = new List<GridStyleColumn>();
            grid.StyleColumnList.Add(new GridStyleColumn()); // One column for ModeStack

            // Render Grid.StyleRowList
            grid.StyleRowList = new List<GridStyleRow>();
            foreach (var rowState in rowStateList)
            {
                foreach (var column in columnList)
                {
                    switch (rowState.RowEnum)
                    {
                        case GridRowEnum.Filter:
                            if ((configGrid?.IsShowHeader).GetValueOrDefault(true))
                            {
                                grid.StyleRowList.Add(new GridStyleRow()); // See also enum GridCellEnum.HeaderColumn
                                grid.StyleRowList.Add(new GridStyleRow()); // Filter value
                            }
                            break;
                        case GridRowEnum.Index:
                            grid.StyleRowList.Add(new GridStyleRow()); // See also enum GridCellEnum.HeaderRow
                            grid.StyleRowList.Add(new GridStyleRow()); // Cell value
                            break;
                        case GridRowEnum.New:
                            if ((configGrid?.IsAllowInsert).GetValueOrDefault(true))
                            {
                                grid.StyleRowList.Add(new GridStyleRow()); // See also enum GridCellEnum.HeaderRow
                                grid.StyleRowList.Add(new GridStyleRow()); // Cell value
                            }
                            break;
                    }
                }
            }

            // Render Grid.CellList
            var fieldList = UtilDalType.TypeRowToFieldListDictionary(grid.TypeRow);

            var filter = new GridFilter(grid);
            var filterValueList = filter.FilterValueList();

            int count = 0;
            foreach (var rowState in rowStateList)
            {
                count += 1;

                // Render Filter
                if (rowState.RowEnum == GridRowEnum.Filter && (configGrid?.IsShowHeader).GetValueOrDefault(true))
                {
                    foreach (var column in columnList)
                    {
                        // Filter Header
                        RenderCellFilterHeaderColumn(grid, rowState, column, cellList);

                        // Filter Value
                        RenderCellFilterValue(grid, rowState, column, cellList, filterValueList);
                    }
                }

                // Render Index
                if (rowState.RowEnum == GridRowEnum.Index)
                {
                    foreach (var column in columnList)
                    {
                        // Filter Header
                        RenderCellFilterHeaderRow(grid, rowState, column, cellList).IsOdd = count % 2 == 1;

                        // Index
                        RenderCellIndex(grid, rowState, column, cellList, fieldList, cell, isTextLeave).IsOdd = count % 2 == 1;
                    }
                }

                // Render New
                if (rowState.RowEnum == GridRowEnum.New && (configGrid?.IsAllowInsert).GetValueOrDefault(true))
                {
                    foreach (var column in columnList)
                    {
                        // Filter Header
                        RenderCellFilterHeaderRow(grid, rowState, column, cellList);

                        // Index
                        RenderCellNew(grid, rowState, column, cellList);
                    }
                }
            }
        }

        /// <summary>
        /// Render Grid.CellList.
        /// </summary>
        /// <param name="cell">If not null, method GridCellText(); is called for all cells on this data row.</param>
        private static void Render(Grid grid, GridCell cell = null, bool isTextLeave = true)
        {
            UtilFramework.LogDebug(string.Format("RENDER ({0}) IsCell={1};", grid.TypeRow?.Name, cell != null));

            var configGrid = UtilGrid.ConfigGrid(grid);

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

            // Grid.CellList clear
            var cellList = grid.CellList.ToDictionary(item => (item.ColumnId, item.RowStateId, item.CellEnum)); // Key (ColumnId, RowState, CellEnum)
            grid.CellList = new List<GridCell>();
            foreach (var cellLocal in cellList.Values)
            {
                cellLocal.IsVisibleScroll = false;
            }

            if (grid.Mode == GridMode.Table)
            {
                RenderModeTable(grid, configGrid, columnList, rowStateList, cellList, cell, isTextLeave);
            }
            else
            {
                RenderModeStack(grid, configGrid, columnList, rowStateList, cellList, cell, isTextLeave);
            }

            // IsHidePagination
            grid.IsHidePagination = !(configGrid?.IsShowPagination).GetValueOrDefault(true);

            // IsShowConfig
            var settingResult = AppJson.SettingInternal(grid, new AppJson.SettingArgs { Grid = grid });
            grid.IsShowConfig = settingResult.GridIsShowConfig;
            grid.IsShowConfigDeveloper = settingResult.GridIsShowConfigDeveloper;

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
        /// <param name="query">Query for rows. If null, only config is loaded.</param>
        private static async Task LoadFullAsync(Grid grid, IQueryable query)
        {
            if (query != null)
            {
                UtilFramework.Assert(grid.TypeRow == query.ElementType);
            }
            
            // Get config grid and field query
            string tableNameCSharp = UtilDalType.TypeRowToTableNameCSharp(grid.TypeRow);
            Grid.QueryConfigResult queryConfigResult;
            if (grid.IsGridLookup == false)
            {
                queryConfigResult = grid.QueryConfigInternal(tableNameCSharp);
            }
            else
            {
                queryConfigResult = grid.GridDest.LookupQueryConfigInternal(grid, tableNameCSharp);
            }

            // Display grid mode
            grid.Mode = queryConfigResult.GridMode;

            // Load config grid
            grid.ConfigGridList = new List<FrameworkConfigGridIntegrate>();
            if (queryConfigResult.ConfigGridQuery != null)
            {
                grid.ConfigGridList = await queryConfigResult.ConfigGridQuery.QueryExecuteAsync();
                // In memory ConfigName filter
                grid.ConfigGridList = grid.ConfigGridList.Where(item => item.ConfigName == queryConfigResult.ConfigName).ToList();
            }
            var configGrid = ConfigGrid(grid);

            // Load row
            if (query != null)
            {
                query = Data.QuerySkipTake(query, 0, ConfigRowCountMax(configGrid));
            }

            // Load config field (Task)
            var configFieldListTask = Task.FromResult(new List<FrameworkConfigFieldIntegrate>());
            if (queryConfigResult.ConfigFieldQuery != null)
            {
                configFieldListTask = queryConfigResult.ConfigFieldQuery.QueryExecuteAsync();
            }

            // Load row (Task)
            var rowListTask = query?.QueryExecuteAsync();

            // Load config field and row in parallel
            if (rowListTask == null)
            {
                await Task.WhenAll(configFieldListTask); 
            }
            else
            {
                await Task.WhenAll(configFieldListTask, rowListTask);
            }

            // Load config field
            grid.ConfigFieldList = configFieldListTask.Result;
            // In memory ConfigName filter
            grid.ConfigFieldList = grid.ConfigFieldList.Where(item => item.ConfigName == queryConfigResult.ConfigName).ToList();

            // RowList
            if (query != null)
            {
                grid.RowListInternal = rowListTask.Result;
            }

            // Truncate
            grid.TruncateInternal(grid.RowListInternal);

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
                    if (!filter.IsClear && !(filter.FilterOperator == FilterOperator.None))
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
            grid.RowListInternal = await query.QueryExecuteAsync();

            // Trunctae
            grid.TruncateInternal(grid.RowListInternal);

        }

        /// <summary>
        /// Load or reload data grid. Also first load for lookup.
        /// </summary>
        private static async Task LoadAsync(Grid grid, IQueryable query, bool isRowSelectFirst)
        {
            Type typeRowOld = grid.TypeRow;
            grid.TypeRow = query?.ElementType;
            grid.DatabaseEnum = DatabaseMemoryInternal.DatabaseEnum(query);

            if (grid.TypeRow == null)
            {
                grid.ColumnList = new List<GridColumn>(); ;
                grid.RowStateList = new List<GridRowState>();
                grid.RowListInternal = new List<Row>();
                grid.CellList = new List<GridCell>();
                grid.GridLookup = null;
                Render(grid);
                return;
            }

            // Load full (with config) or reload
            if (typeRowOld != query?.ElementType)
            {
                // ColumnList, RowList
                await LoadFullAsync(grid, query);
            }
            else
            {
                // RowList
                await LoadReloadAsync(grid, query);
            }

            // RowStateList
            grid.RowStateList = LoadRowStateList(grid);

            // Select first row on data grid. But not on lookup grid.
            if (grid.IsGridLookup == false && isRowSelectFirst) 
            {
                await RowSelectAsync(grid, grid.RowStateList.Where(item => item.RowEnum == GridRowEnum.Index).FirstOrDefault());
            }

            grid.CellList = new List<GridCell>();

            Render(grid);
        }

        /// <summary>
        /// Load or reload data grid and (re)render.
        /// </summary>
        public static async Task LoadAsync(Grid grid)
        {
            IQueryable query;
            bool isRowSelectFirst = false;
            if (grid.IsGridLookup == false)
            {
                grid.QueryInternal(out query, out isRowSelectFirst);
            }
            else
            {
                GridLookupToGridDest(grid, out var gridDest, out var rowDest, out string fieldNameCSharpDest, out var cellDest);
                query = gridDest.LookupQueryInternal(rowDest, fieldNameCSharpDest, cellDest.Text);
            }
            await LoadAsync(grid, query, isRowSelectFirst);
        }

        /// <summary>
        /// Load or reload data grid configuration only. For example after config change. See also class PageConfigGrid.
        /// </summary>
        internal static async Task LoadConfigOnlyAsync(Grid grid)
        {
            await LoadFullAsync(grid, null);

            grid.CellList = new List<GridCell>();

            Render(grid);
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
                rowDest = gridDest.RowListInternal[rowStateDest.RowId.Value - 1];
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
            if (grid.GridLookup?.GridLookupDestFieldNameCSharp != fieldNameCSharp)
            {
                GridLookupClose(grid); // User changed the column focus on same row without closing lookup data grid
            }

            if (grid.GridLookup == null)
            {
                UtilFramework.Assert(cell.GridLookup == null);
                Grid gridLookup = new Grid<Row>(grid);
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
        /// Returns one data grid configuration record.
        /// </summary>
        private static FrameworkConfigGridIntegrate ConfigGrid(Grid grid)
        {
            var result = grid.ConfigGridList.SingleOrDefault(); // LINQ to memory
            return result;
        }

        /// <summary>
        /// Returns RowCountMax rows to load.
        /// </summary>
        private static int ConfigRowCountMax(FrameworkConfigGridIntegrate configGrid)
        {
            return configGrid?.RowCountMax == null ? 10 : configGrid.RowCountMax.Value; // By default load 10 rows.
        }

        /// <summary>
        /// Returns ColumnCountMax of columns to render.
        /// </summary>
        private static int ConfigColumnCountMax(FrameworkConfigGridIntegrate configGrid)
        {
            return 5;
        }

        /// <summary>
        /// Returns data grid field configuration records. (FieldName, FrameworkConfigFieldIntegrate). One FieldName can have multiple FrameworkConfigFieldIntegrate because of InstanceName.
        /// </summary>
        private static Dictionary<string, List<FrameworkConfigFieldIntegrate>> ConfigFieldDictionary(Grid grid)
        {
            var result = new Dictionary<string, List<FrameworkConfigFieldIntegrate>>();
            foreach (var item in grid.ConfigFieldList)
            {
                if (!result.ContainsKey(item.FieldNameCSharp))
                {
                    result[item.FieldNameCSharp] = new List<FrameworkConfigFieldIntegrate>();
                }
                result[item.FieldNameCSharp].Add(item);
            }
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
                Row row = grid.RowListInternal[rowState.RowId.Value - 1];
                rowState.IsSelect = true;
                UtilFramework.Assert(row == grid.RowSelect);
                await grid.RowSelectAsync();
            }

            if (isRender)
            {
                Render(grid);
            }
        }

        /// <summary>
        /// User modified text, now open lookup window.
        /// </summary>
        private static async Task ProcessGridLookupOpenAsync(Grid grid, GridRowState rowState, GridColumn column, GridCell cell)
        {
            var query = grid.LookupQueryInternal(rowState.Row, column.FieldNameCSharp, cell.Text);
            if (query != null)
            {
                GridLookupOpen(grid, rowState, column.FieldNameCSharp, cell);
                await LoadAsync(grid.GridLookup, query, isRowSelectFirst: false);
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
            if (UtilSession.Request(appJson, CommandEnum.GridIsClickSort, out CommandJson commandJson, out Grid grid))
            {
                GridCell cell = grid.CellList[commandJson.GridCellId - 1];
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
            if (UtilSession.Request(appJson, CommandEnum.GridIsClickConfig, out CommandJson commandJson, out Grid grid))
            {
                if (grid.IsShowConfig || grid.IsShowConfigDeveloper)
                {

                    GridCell cell = grid.CellList[commandJson.GridCellId - 1];
                    GridColumn column = grid.ColumnList[cell.ColumnId - 1];
                    Page page = grid.ComponentOwner<Page>();

                    string tableNameCSharp = UtilDalType.TypeRowToTableNameCSharp(grid.TypeRow);
                    string configName = null; // Never show data grid header column config in developer config (mode).
                    var pageConfigGrid = new PageConfigGrid(page, tableNameCSharp, column.FieldNameCSharp, configName);
                    await pageConfigGrid.InitAsync();
                }
            }
        }

        /// <summary>
        /// Synchronize Cell.Text when user leaves field.
        /// </summary>
        private static void ProcessIsTextLeave(AppJson appJson)
        {
            if (UtilSession.Request(appJson, CommandEnum.GridIsTextLeave, out CommandJson commandJson, out Grid grid))
            {
                GridCell cell = grid.CellList[commandJson.GridCellId - 1];
                cell.Text = cell.TextLeave;
                cell.TextLeave = null;
            }
        }

        /// <summary>
        /// Send column width to server after user resizes a column.
        /// </summary>
        private static void ProcessStyleColumn(AppJson appJson)
        {
            if (UtilSession.Request(appJson, CommandEnum.StyleColumnWidth, out CommandJson commandJson, out Grid grid))
            {
                var columnList = grid.ColumnList.Where(item => item.IsVisibleScroll).ToArray();
                columnList[commandJson.ResizeColumnIndex].WidthValue = commandJson.ResizeColumnWidthValue;
                Render(grid); // Update Grid.StyleColumnList
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
        private static async Task ProcessCellIsModifyParseAsync(Grid grid, GridRowEnum rowEnum, Row row, GridColumn column, Field field, GridCell cell, CommandJson commandJson, FileUpload fileUpload)
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
                Grid.ParseResultInternal result = new Grid.ParseResultInternal();
                grid.CellParseInternal(row, column.FieldNameCSharp, UtilFramework.StringEmpty(cell.Text), result); // Custom parse of user entered text.
                if (result.IsHandled == false)
                {
                    await grid.CellParseInternalAsync(row, column.FieldNameCSharp, UtilFramework.StringEmpty(cell.Text), result);
                }
                // Parse custom (FileUpload)
                if (commandJson.GridCellTextBase64 != null)
                {
                    var startsWith = "data:application/octet-stream;base64,";
                    // User uploaded empty (0 bytes) file
                    if (commandJson.GridCellTextBase64 == "data:")
                    {
                        startsWith = "data:";
                    }
                    UtilFramework.Assert(commandJson.GridCellTextBase64.StartsWith(startsWith));
                    var data = System.Convert.FromBase64String(commandJson.GridCellTextBase64.Substring(startsWith.Length));
                    fileUpload.FieldName = column.FieldNameCSharp;
                    fileUpload.Data = data;
                    fileUpload.FileName = commandJson.GridCellTextBase64FileName;
                    // Is handled in terms of data is available in update and insert args parameter.
                    result.IsHandled = true;
                }
                // Parse default
                if (!result.IsHandled)
                {
                    if (result.ErrorParse != null)
                    {
                        throw new Exception("ErrorParse has been set without IsHandled!"); // Custom parse workflow!
                    }
                    Data.CellTextParse(field, cell.Text, row, out string errorParse);
                    result.IsHandled = true;
                    result.ErrorParse = errorParse;
                }
                cell.ErrorParse = result.ErrorParse;
            }
            catch (Exception exception)
            {
                cell.ErrorParse = UtilFramework.ExceptionToString(exception);
            }
        }

        /// <summary>
        /// Parse
        /// </summary>
        private static void ProcessCellIsModifyParseFilter(Grid grid, GridColumn column, Field field, GridCell cell)
        {
            cell.ErrorParse = null;
            // Parse
            try
            {
                // Parse custom
                GridCellParseFilterResult result = new GridCellParseFilterResult(new GridFilter(grid));
                grid.CellParseFilter(column.FieldNameCSharp, UtilFramework.StringEmpty(cell.Text), result); // Custom parse of user entered text.
                string errorParse = result.ErrorParse;
                if (!result.IsHandled)
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
        private static async Task ProcessCellIsModifyUpdateAsync(Grid grid, Row rowOld, Row row, GridCell cell, FileUpload fileUpload)
        {
            // Save
            try
            {
                Grid.UpdateResultInternal result = new Grid.UpdateResultInternal();
                await grid.UpdateInternalAsync(rowOld, row, fileUpload, grid.DatabaseEnum, result);
                if (!result.IsHandled)
                {
                    await Data.UpdateAsync(rowOld, row, grid.DatabaseEnum);
                }
            }
            catch (Exception exception)
            {
                cell.ErrorSave = UtilFramework.ExceptionToString(exception);
            }

            // Truncate
            grid.TruncateInternal(new List<Row>(new Row[] { row }));
        }

        /// <summary>
        /// Save (Insert).
        /// </summary>
        private static async Task ProcessCellIsModifyInsertAsync(Grid grid, Row row, GridCell cell, FileUpload fileUpload)
        {
            // Save
            try
            {
                // Save custom
                Grid.InsertResultInternal result = new Grid.InsertResultInternal();
                await grid.InsertInternalAsync(row, fileUpload, grid.DatabaseEnum, result);
                if (!result.IsHandled)
                {
                    // Save default
                    await Data.InsertAsync(row, grid.DatabaseEnum);
                }
            }
            catch (Exception exception)
            {
                cell.ErrorSave = UtilFramework.ExceptionToString(exception);
            }

            // Truncate
            grid.TruncateInternal(new List<Row>(new Row[] { row }));
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
        private static void ProcessCellIsModifyText(GridCell cell, CommandJson commandJson)
        {
            string textOld = cell.Text;
            cell.Text = commandJson.GridCellText;
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
        /// Create and queue RowIsClick command.
        /// </summary>
        public static void QueueRowIsClick(Grid grid, Row row)
        {
            var rowIndex = grid.RowListInternal.IndexOf(row);
            var rowState = grid.RowStateList.First(item => item.RowId == rowIndex + 1);
            var cell = grid.CellList.First(item => item.RowStateId == rowState.Id);
            grid.ComponentOwner<AppJson>().RequestJson.CommandAdd(new CommandJson { CommandEnum = CommandEnum.GridIsClickRow, ComponentId = grid.Id, GridCellId = cell.Id });
        }

        /// <summary>
        /// User selected data row.
        /// </summary>
        private static async Task ProcessRowIsClickAsync(AppJson appJson)
        {
            if (UtilSession.Request(appJson, CommandEnum.GridIsClickRow, out CommandJson commandJson, out Grid grid))
            {
                var rowSelectLocal = grid.RowSelect;

                int rowStateId;
                // Get rowStateId either from cell or directly from command.
                if (commandJson.GridCellId != 0)
                {
                    GridCell cell = grid.CellList[commandJson.GridCellId - 1];
                    rowStateId = cell.RowStateId;
                }
                else
                {
                    rowStateId = commandJson.RowStateId;
                }

                Row rowSelect = null;
                foreach (var rowState in grid.RowStateList)
                {
                    if (rowState.RowEnum == GridRowEnum.Index)
                    {
                        rowState.IsSelect = rowState.Id == rowStateId;
                        if (rowState.IsSelect)
                        {
                            rowSelect = grid.RowListInternal[rowState.RowId.Value - 1];
                        }
                    }
                }

                if (rowSelect != null)
                {
                    if (grid.IsGridLookup == false)
                    {
                        // Grid normal row selected
                        GridLookupClose(grid);
                        Render(grid);
                        UtilFramework.Assert(rowSelect == grid.RowSelect);
                        if (rowSelect != rowSelectLocal)
                        {
                            await grid.RowSelectAsync(); // Load detail data grid
                        }
                    }
                    else
                    {
                        // Grid lookup row selected
                        GridLookupClose(grid.GridDest);
                        UtilFramework.Assert(rowSelect == grid.RowSelect);
                        LookupRowSelectArgs args = new LookupRowSelectArgs { RowSelect = grid.RowSelect, FieldName = grid.GridLookupDestFieldNameCSharp };
                        LookupRowSelectResult result = new LookupRowSelectResult();
                        grid.GridDest.LookupRowSelect(args, result);
                        if (result.Text != null)
                        {
                            GridLookupToGridDest(grid, out var gridDest, out _, out _, out var cellDest);
                            // TODO Use command queue
                            appJson.RequestJson = new RequestJson(new CommandJson { CommandEnum = CommandEnum.GridCellIsModify, ComponentId = gridDest.Id, GridCellId = cellDest.Id, GridCellText = result.Text, GridCellTextIsLookup = true });

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
            if (UtilSession.Request(appJson, CommandEnum.GridCellIsModify, out CommandJson commandJson, out Grid grid))
            {
                GridCell cell = grid.CellList[commandJson.GridCellId - 1];
                if (cell.IsReadOnly)
                {
                    throw new ExceptionSecurity("Cell IsReadOnly!");
                }
                GridColumn column = grid.ColumnList[cell.ColumnId - 1];
                var field = UtilDalType.TypeRowToFieldListDictionary(grid.TypeRow)[column.FieldNameCSharp];
                GridRowState rowState = grid.RowStateList[cell.RowStateId - 1];

                // Track IsModified
                ProcessCellIsModifyText(cell, commandJson);

                cell.Warning = null;

                // Parse Filter
                if (rowState.RowEnum == GridRowEnum.Filter)
                {
                    new GridFilter(grid).TextSet(column.FieldNameCSharp, cell.Text); // Used after data grid reload to restore filter.

                    // Parse
                    ProcessCellIsModifyParseFilter(grid, column, field, cell);
                    if (!ProcessCellIsModifyIsErrorParse(grid, cell))
                    {
                        // Reload
                        grid.OffsetRow = 0; // Back to first row.
                        await LoadAsync(grid);
                    }
                }

                var fileUpload = new FileUpload();

                // Parse Index
                if (rowState.RowEnum == GridRowEnum.Index)
                {
                    Row row = grid.RowListInternal[rowState.RowId.Value - 1];
                    if (rowState.Row == null)
                    {
                        rowState.Row = (Row)Activator.CreateInstance(grid.TypeRow);
                        Data.RowCopy(row, rowState.Row);
                    }
                    // ErrorSave reset
                    ProcessCellIsModifyErrorSaveReset(grid, cell);
                    // Parse
                    await ProcessCellIsModifyParseAsync(grid, rowState.RowEnum, rowState.Row, column, field, cell, commandJson, fileUpload);
                    if (!ProcessCellIsModifyIsErrorParse(grid, cell))
                    {
                        // Save
                        await ProcessCellIsModifyUpdateAsync(grid, row, rowState.Row, cell, fileUpload);
                        if (cell.ErrorSave == null)
                        {
                            Data.RowCopy(rowState.Row, row); // Copy new Id to 
                            ProcessCellIsModifyReset(grid, cell);
                        }
                    }
                    // Lookup
                    if (!commandJson.GridCellTextIsLookup) // Do not open lookup again after lookup row has been clicked by user.
                    {
                        await ProcessGridLookupOpenAsync(grid, rowState, column, cell);
                    }

                    // Do not set Cell.TextLeave if user clicked lookup row.
                    bool isTextLeave = true;
                    if (commandJson.GridCellTextIsLookup)
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
                    if (rowState.Row == null)
                    {
                        rowState.Row = (Row)Activator.CreateInstance(grid.TypeRow);
                    }
                    // ErrorSave reset
                    ProcessCellIsModifyErrorSaveReset(grid, cell);
                    // Parse
                    await ProcessCellIsModifyParseAsync(grid, rowState.RowEnum, rowState.Row, column, field, cell, commandJson, fileUpload);
                    if (!ProcessCellIsModifyIsErrorParse(grid, cell))
                    {
                        // Save
                        await ProcessCellIsModifyInsertAsync(grid, rowState.Row, cell, fileUpload);
                        if (cell.ErrorSave == null)
                        {
                            grid.RowListInternal.Add(rowState.Row);
                            rowState.RowId = grid.RowListInternal.Count;
                            foreach (var item in grid.CellList)
                            {
                                if (item.RowStateId == cell.RowStateId) // Cells in same row
                                {
                                    rowState.RowEnum = GridRowEnum.Index; // From New to Index
                                    item.Placeholder = null;
                                }
                            }
                            rowState.Row = null;
                            grid.RowStateList.Add(new GridRowState { Id = grid.RowStateList.Count + 1, RowEnum = GridRowEnum.New });

                            ProcessCellIsModifyReset(grid, cell);
                            await RowSelectAsync(grid, rowState);
                        }
                    }
                    // Lookup
                    if (!commandJson.GridCellTextIsLookup) // Do not open lookup again after lookup row has been clicked by user.
                    {
                        await ProcessGridLookupOpenAsync(grid, rowState, column, cell);
                    }

                    // Do not set Cell.TextLeave if user clicked lookup row.
                    bool isTextLeave = true;
                    if (commandJson.GridCellTextIsLookup)
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
            if (UtilSession.Request(appJson, CommandEnum.GridIsClickEnum, out CommandJson commandJson, out Grid grid))
            {
                var isClickConfig = commandJson.GridIsClickEnum == GridIsClickEnum.Config;
                var isClickConfigDeveloper = commandJson.GridIsClickEnum == GridIsClickEnum.ConfigDeveloper;

                // Security
                if (isClickConfig)
                {
                    if (!grid.IsShowConfig)
                    {
                        throw new ExceptionSecurity("Grid Config not allowed!");
                    }
                }
                if (isClickConfigDeveloper)
                {
                    if (!grid.IsShowConfigDeveloper)
                    {
                        throw new ExceptionSecurity("Grid ConfigDeveloper not allowed!");
                    }
                }

                // Grid config
                if (isClickConfig || isClickConfigDeveloper)
                {
                    if (grid.TypeRow != null) // Do not show config if for example no query is defined for data grid.
                    {
                        Page page = grid.ComponentOwner<Page>();

                        // Show data grid config in developer config (mode) if user clicked developer button.
                        string configName = null;
                        if (commandJson.GridIsClickEnum == GridIsClickEnum.ConfigDeveloper)
                        {
                            // Gets Strongly typed name "Developer"
                            configName = FrameworkConfigGridIntegrateFramework.IdEnum.dboFrameworkConfigFieldDisplayDeveloper.Row().ConfigName;
                        }

                        string tableNameCSharp = UtilDalType.TypeRowToTableNameCSharp(grid.TypeRow);
                        var pageConfigGrid = new PageConfigGrid(page, tableNameCSharp, null, configName);
                        await pageConfigGrid.InitAsync();
                    }
                }

                // Grid reload
                if (commandJson.GridIsClickEnum == GridIsClickEnum.Reload)
                {
                    // Reset filter, sort
                    grid.FilterValueList = null;
                    grid.SortValueList = null;
                    grid.OffsetRow = 0;
                    grid.OffsetColumn = 0;

                    await LoadAsync(grid);
                }

                // Grid page up
                if (commandJson.GridIsClickEnum == GridIsClickEnum.PageUp)
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
                if (commandJson.GridIsClickEnum == GridIsClickEnum.PageDown)
                {
                    var configGrid = ConfigGrid(grid);
                    int rowCount = grid.RowListInternal.Count;
                    int rowCountMax = ConfigRowCountMax(configGrid);
                    if (rowCount == rowCountMax) // Page down further on full grid only.
                    {
                        grid.OffsetRow += rowCountMax;
                    }
                    await LoadAsync(grid);
                }

                // Grid page left
                if (commandJson.GridIsClickEnum == GridIsClickEnum.PageLeft)
                {
                    grid.OffsetColumn -= 1;
                    if (grid.OffsetColumn < 0)
                    {
                        grid.OffsetColumn = 0;
                    }
                    Render(grid);
                }

                // Grid page right
                if (commandJson.GridIsClickEnum == GridIsClickEnum.PageRight)
                {
                    if (grid.OffsetColumn + ConfigColumnCountMax(ConfigGrid(grid)) < grid.ColumnList.Count)
                    {
                        grid.OffsetColumn += 1;
                        Render(grid);
                    }
                }

                // Grid mode table
                if (commandJson.GridIsClickEnum == GridIsClickEnum.ModeTable)
                {
                    grid.Mode = GridMode.Table;
                    Render(grid);
                }

                // Grid mode stack
                if (commandJson.GridIsClickEnum == GridIsClickEnum.ModeStack)
                {
                    grid.Mode = GridMode.Stack;
                    Render(grid);
                }

                // Excel
                if (commandJson.GridIsClickEnum == GridIsClickEnum.ExcelDownload)
                {
                    UtilGridExcel.Export(grid);
                }
            }
        }
    }
}

namespace Framework.Session
{
    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Spreadsheet;
    using Framework.DataAccessLayer;
    using Framework.Json;
    using System;
    using System.IO;
    using Page = Json.Page;
    using CellExcel = DocumentFormat.OpenXml.Spreadsheet.Cell;
    using RowExcel = DocumentFormat.OpenXml.Spreadsheet.Row;
    using Row = DataAccessLayer.Row;

    internal static class UtilGridExcel
    {
        private static void ExportStyle(SpreadsheetDocument spreadsheetDocument)
        {
            // See also: https://stackoverflow.com/questions/11116176/cell-styles-in-openxml-spreadsheet-spreadsheetml

            var stylesPart = spreadsheetDocument.WorkbookPart.AddNewPart<WorkbookStylesPart>();
            stylesPart.Stylesheet = new Stylesheet
            {
                // blank font list
                Fonts = new Fonts()
            };
            stylesPart.Stylesheet.Fonts.AppendChild(new Font());
            stylesPart.Stylesheet.Fonts.Count = (uint)stylesPart.Stylesheet.Fonts.ChildElements.Count;

            // White font            
            Font font = new Font();
            font.Append(new Color() { Rgb = "ffffff" });
            font.Append(new Bold());
            stylesPart.Stylesheet.Fonts.AppendChild(font);
            stylesPart.Stylesheet.Fonts.Count = (uint)stylesPart.Stylesheet.Fonts.ChildElements.Count;

            // create fills
            stylesPart.Stylesheet.Fills = new Fills();

            // create a solid red fill
            var solidBlue = new PatternFill() { PatternType = PatternValues.Solid };
            solidBlue.ForegroundColor = new ForegroundColor { Rgb = HexBinaryValue.FromString("FF0000FF") }; // red fill
            solidBlue.BackgroundColor = new BackgroundColor { Indexed = 64 };

            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.None } }); // required, reserved by Excel
            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.Gray125 } }); // required, reserved by Excel
            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = solidBlue });
            stylesPart.Stylesheet.Fills.Count = (uint)stylesPart.Stylesheet.Fills.ChildElements.Count;

            //// blank border list
            stylesPart.Stylesheet.Borders = new Borders();
            stylesPart.Stylesheet.Borders.AppendChild(new Border());
            stylesPart.Stylesheet.Borders.Count = (uint)stylesPart.Stylesheet.Borders.ChildElements.Count;

            //// blank cell format list
            stylesPart.Stylesheet.CellStyleFormats = new CellStyleFormats();
            stylesPart.Stylesheet.CellStyleFormats.AppendChild(new CellFormat());
            stylesPart.Stylesheet.CellStyleFormats.Count = (uint)stylesPart.Stylesheet.CellStyleFormats.ChildElements.Count;

            // cell format list
            stylesPart.Stylesheet.CellFormats = new CellFormats();
            // empty one for index 0, seems to be required
            stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat());
            // cell format references style format 0, font 0, border 0, fill 2 and applies the fill
            stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat { FormatId = 0, FontId = 1, BorderId = 0, FillId = 2, ApplyFill = true });
                // .AppendChild(new Alignment { Horizontal = HorizontalAlignmentValues.Center });
            stylesPart.Stylesheet.CellFormats.Count = (uint)stylesPart.Stylesheet.CellFormats.ChildElements.Count;

            stylesPart.Stylesheet.Save();
        }

        private static string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        private static void ExportData(WorksheetPart worksheetPart, Grid grid)
        {
            var fieldList = UtilDalType.TypeRowToFieldListDictionary(grid.TypeRow);

            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

            var columnList = grid.ColumnList;

            int rowCount = 0;
            foreach (var rowState in grid.RowStateList)
            {
                rowCount += 1;
                var rowExcel = new RowExcel() { RowIndex = (uint)rowCount };
                sheetData.Append(rowExcel);

                int columnCount = 0;
                foreach (var column in columnList)
                {
                    columnCount += 1;
                    string cellReference = GetExcelColumnName(columnCount) + rowCount;

                    // Column header cell
                    if (rowState.RowEnum == GridRowEnum.Filter)
                    {
                        var cell = new CellExcel() { CellReference = cellReference, DataType = CellValues.String, CellValue = new CellValue(column.ColumnText), StyleIndex = 1 };
                        rowExcel.Append(cell);
                    }

                    // Data cell
                    if (rowState.RowEnum == GridRowEnum.Index)
                    {
                        Row row;
                        if (rowState.Row == null)
                        {
                            row = grid.RowListInternal[rowState.RowId.Value - 1];
                        }
                        else
                        {
                            row = rowState.Row;
                        }

                        var field = fieldList[column.FieldNameCSharp];
                        string text = null;
                        object value = field.PropertyInfo.GetValue(row);
                        if (value != null)
                        {
                            text = grid.CellTextInternal(row, field.FieldNameCSharp, text); // Custom convert database value to cell text.
                            text = UtilFramework.StringNull(text);
                            if (text == null)
                            {
                                text = field.FrameworkType().CellTextFromValue(value);
                            }
                        }
                        var cell = new CellExcel() { CellReference = cellReference, DataType = CellValues.String, CellValue = new CellValue(text) };
                        rowExcel.Append(cell);
                    }
                }
            }
        }

        public static void Export(Grid grid)
        {
            var fileName = Path.GetTempFileName();
            using (var spreadsheetDocument = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook))
            {
                // Add a WorkbookPart to the document.
                WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
                workbookpart.Workbook = new Workbook();

                // Add a WorksheetPart to the WorkbookPart.
                WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                // Add Sheets to the Workbook.
                Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.
                AppendChild<Sheets>(new Sheets());

                // Append a new worksheet and associate it with the workbook.
                Sheet sheet = new Sheet()
                {
                    Id = spreadsheetDocument.WorkbookPart.
                    GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Data"
                };
                sheets.Append(sheet);

                ExportStyle(spreadsheetDocument);
                ExportData(worksheetPart, grid);

                workbookpart.Workbook.Save();

                // Close the document.
                spreadsheetDocument.Close();
            }

            // Send file with app.json to download in client.
            AppJson appJson = grid.ComponentOwner<AppJson>();
            appJson.Download(File.ReadAllBytes(fileName), "Grid.xlsx");
            File.Delete(fileName);
        }
    }
}