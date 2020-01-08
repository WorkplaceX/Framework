namespace Framework.Session
{
    using Database.dbo;
    using Framework.Application;
    using Framework.DataAccessLayer;
    using Framework.DataAccessLayer.DatabaseMemory;
    using Framework.Json;
    using Framework.Server;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using static Framework.DataAccessLayer.UtilDalType;
    using static Framework.Session.UtilSession;

    /// <summary>
    /// Application server side session state. Get it with property UtilServer.AppInternal.AppSession
    /// </summary>
    internal sealed class AppSession
    {
        /// <summary>
        /// Gets or sets RequestCount. Managed and incremented by client only.
        /// </summary>
        public int RequestCount;

        /// <summary>
        /// Gets or sets ResponseCount. Managed and incremented by server only.
        /// </summary>
        public int ResponseCount;

        /// <summary>
        /// Server side session state for each data grid.
        /// </summary>
        public List<GridSession> GridSessionList = new List<GridSession>();

        /// <summary>
        /// Load a single row into session and create its cells.
        /// </summary>
        private void GridLoad(int gridIndex, int rowIndex, Row row, Type typeRow, GridRowEnum gridRowEnum, ref List<Field> fieldListCache)
        {
            if (fieldListCache == null)
            {
                fieldListCache = UtilDalType.TypeRowToFieldList(typeRow);
            }

            GridSession gridSession = GridSessionList[gridIndex];
            GridRowSession gridRowSession = new GridRowSession();
            gridRowSession.IsSelect = gridSession.GridRowSessionList[rowIndex].IsSelect;
            gridSession.GridRowSessionList[rowIndex] = gridRowSession;
            gridRowSession.Row = row;
            gridRowSession.RowEnum = gridRowEnum;
            Grid grid = UtilSession.GridFromIndex(gridIndex);
            Page page = grid.ComponentOwner<Page>();
            foreach (Field field in fieldListCache)
            {
                GridCellSession gridCellSession = new GridCellSession();
                gridRowSession.GridCellSessionList.Add(gridCellSession);
                Data.CellTextFromValue(page, grid, gridRowSession, field, gridCellSession, row);
            }
        }

        /// <summary>
        /// Add data filter row. That's where the user enters the filter (search) text.
        /// </summary>
        private void GridLoadAddFilter(int gridIndex)
        {
            GridSession gridSession = GridSessionList[gridIndex];
            gridSession.GridRowSessionList.Add(new GridRowSession());
            int rowIndex = gridSession.GridRowSessionList.Count - 1;
            List<Field> fieldList = null;
            GridLoad(gridIndex, rowIndex, null, gridSession.TypeRow, GridRowEnum.Filter, ref fieldList);
        }

        /// <summary>
        /// Add empty data row. Thats where the user enters a new record.
        /// </summary>
        private void GridLoadAddRowNew(int gridIndex)
        {
            GridSession gridSession = GridSessionList[gridIndex];
            gridSession.GridRowSessionList.Add(new GridRowSession());
            int rowIndex = gridSession.GridRowSessionList.Count - 1;
            List<Field> fieldList = null;
            GridLoad(gridIndex, rowIndex, null, gridSession.TypeRow, GridRowEnum.New, ref fieldList);
        }

        private void GridLoadSessionCreate(Grid grid)
        {
            if (grid.Index == null)
            {
                GridSessionList.Add(new GridSession());
                grid.Index = GridSessionList.Count - 1;
            }
        }

        /// <summary>
        /// Load column definitions into session state.
        /// </summary>
        private void GridLoadColumn(Grid grid, Type typeRow)
        {
            GridSession gridSession = UtilSession.GridSessionFromGrid(grid);

            if (gridSession.TypeRow != typeRow)
            {
                if (typeRow == null)
                {
                    gridSession.GridColumnSessionList.Clear();
                }
                else
                {
                    PropertyInfo[] propertyInfoList = UtilDalType.TypeRowToPropertyInfoList(typeRow);
                    foreach (PropertyInfo propertyInfo in propertyInfoList)
                    {
                        gridSession.GridColumnSessionList.Add(new GridColumnSession() { FieldName = propertyInfo.Name });
                    }
                }
            }
        }

        /// <summary>
        /// Copy data grid cell values to AppSession.
        /// </summary>
        private void GridLoad(Grid grid, List<Row> rowList, Type typeRow, DatabaseEnum databaseEnum)
        {
            UtilSession.GridReset(grid);

            GridSession gridSession = UtilSession.GridSessionFromGrid(grid);
            int gridIndex = UtilSession.GridToIndex(grid);

            // Reset GridRowSessionList but keep filter row.
            var filterRow = gridSession.GridRowSessionList.Where(item => item.RowEnum == GridRowEnum.Filter).SingleOrDefault();
            gridSession.GridRowSessionList.Clear();
            if (filterRow != null)
            {
                gridSession.GridRowSessionList.Add(filterRow);
            }

            if (gridSession.TypeRow != typeRow)
            {
                gridSession.TypeRow = typeRow;
                gridSession.DatabaseEnum = databaseEnum;
                gridSession.GridRowSessionList.Clear();
                GridLoadAddFilter(gridIndex); // Add "filter row".
            }

            if (rowList != null)
            {
                List<Field> fieldList = UtilDalType.TypeRowToFieldList(typeRow);
                foreach (Row row in rowList)
                {
                    GridRowSession gridRowSession = new GridRowSession();
                    gridSession.GridRowSessionList.Add(gridRowSession);
                    GridLoad(gridIndex, gridSession.GridRowSessionList.Count - 1, row, typeRow, GridRowEnum.Index, ref fieldList);
                }
                GridLoadAddRowNew(gridIndex); // Add one "new row" to end of grid.
            }
        }

        /// <summary>
        /// Load FrameworkConfigGrid to GridSession.
        /// </summary>
        private async Task GridLoadConfigAsync(Grid grid, Type typeRow, IQueryable<FrameworkConfigGridBuiltIn> configGridQuery)
        {
            GridSession gridSession = UtilSession.GridSessionFromGrid(grid);
            if (typeRow == null || configGridQuery == null)
            {
                gridSession.RowCountMaxConfig = null; // Reset
            }
            else
            {
                var configGridList = await Data.SelectAsync(configGridQuery);
                UtilFramework.Assert(configGridList.Count == 0 || configGridList.Count == 1);
                var frameworkConfigGrid = configGridList.SingleOrDefault();
                if (frameworkConfigGrid != null)
                {
                    string tableNameCSharp = UtilDalType.TypeRowToTableNameCSharp(typeRow);
                    if (frameworkConfigGrid.TableNameCSharp != null)
                    {
                        UtilFramework.Assert(tableNameCSharp == frameworkConfigGrid.TableNameCSharp); // TableNameCSharp. See also file Framework.sql
                    }
                    if (frameworkConfigGrid.RowCountMax.HasValue)
                    {
                        gridSession.RowCountMaxConfig = frameworkConfigGrid.RowCountMax.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Load FrameworkConfigField to GridSession.
        /// </summary>
        private async Task GridLoadConfigAsync(Grid grid, Type typeRow, IQueryable<FrameworkConfigFieldBuiltIn> configFieldQuery)
        {
            GridSession gridSession = UtilSession.GridSessionFromGrid(grid);
            // (FieldName, FrameworkConfigFieldBuiltIn)
            Dictionary<string, FrameworkConfigFieldBuiltIn> fieldBuiltInList = new Dictionary<string, FrameworkConfigFieldBuiltIn>();
            if (!(typeRow == null || configFieldQuery == null))
            {
                string tableNameCSharp = UtilDalType.TypeRowToTableNameCSharp(typeRow);
                var configFieldList = await Data.SelectAsync(configFieldQuery);
                foreach (var frameworkConfigFieldBuiltIn in configFieldList)
                {
                    if (frameworkConfigFieldBuiltIn.TableNameCSharp != null) // If set, it needs to be correct.
                    {
                        UtilFramework.Assert(frameworkConfigFieldBuiltIn.TableNameCSharp == tableNameCSharp, string.Format("TableNameCSharp wrong! ({0}; {1})", tableNameCSharp, frameworkConfigFieldBuiltIn.TableNameCSharp));
                    }
                    fieldBuiltInList.Add(frameworkConfigFieldBuiltIn.FieldNameCSharp, frameworkConfigFieldBuiltIn);
                }
            }

            AppJson appJson = UtilServer.AppJson;
            NamingConvention namingConvention = appJson.NamingConventionInternal(typeRow);
            var fieldList = UtilDalType.TypeRowToFieldListDictionary(typeRow);

            foreach (var columnSession in gridSession.GridColumnSessionList)
            {
                Field field = fieldList[columnSession.FieldName];

                string textConfig = null;
                string description = null;
                bool? isVisibleConfig = null;
                double? sortConfig = null;
                if (fieldBuiltInList.TryGetValue(columnSession.FieldName, out var frameworkConfigFieldBuiltIn))
                {
                    textConfig = frameworkConfigFieldBuiltIn.Text;
                    description = frameworkConfigFieldBuiltIn.Description;
                    isVisibleConfig = frameworkConfigFieldBuiltIn.IsVisible;
                    sortConfig = frameworkConfigFieldBuiltIn.Sort;
                }
                columnSession.Text = namingConvention.ColumnTextInternal(typeRow, columnSession.FieldName, textConfig);
                columnSession.Description = description;
                columnSession.IsVisible = namingConvention.ColumnIsVisibleInternal(typeRow, columnSession.FieldName, isVisibleConfig);
                columnSession.Sort = namingConvention.ColumnSortInternal(typeRow, columnSession.FieldName, field, sortConfig);
            }
        }

        /// <summary>
        /// Select data from database and write to session.
        /// </summary>
        private async Task GridLoadRowAsync(Grid grid, IQueryable query)
        {
            List<Row> rowList = null;
            if (query != null)
            {
                GridSession gridSession = UtilSession.GridSessionFromGrid(grid);

                // Filter
                GridRowSession gridRowSessionFilter = gridSession.GridRowSessionList.Where(item => item.RowEnum == GridRowEnum.Filter).FirstOrDefault();
                if (gridRowSessionFilter != null)
                {
                    for (int index = 0; index < gridSession.GridColumnSessionList.Count; index++)
                    {
                        string fieldName = gridSession.GridColumnSessionList[index].FieldName;
                        object filterValue = gridRowSessionFilter.GridCellSessionList[index].FilterValue;
                        FilterOperator filterOperator = gridRowSessionFilter.GridCellSessionList[index].FilterOperator;
                        if (filterValue != null)
                        {
                            query = Data.QueryFilter(query, fieldName, filterValue, filterOperator);
                        }
                    }
                }

                // Sort
                GridColumnSession gridColumnSessionSort = gridSession.GridColumnSessionList.Where(item => item.IsSort != null).SingleOrDefault();
                if (gridColumnSessionSort != null)
                {
                    query = Data.QueryOrderBy(query, gridColumnSessionSort.FieldName, (bool)gridColumnSessionSort.IsSort);
                }

                // Skip, Take
                query = Data.QuerySkipTake(query, gridSession.OffsetRow, gridSession.RowCountMaxGet());

                rowList = await Data.SelectAsync(query);
            }
            DatabaseEnum databaseEnum = DatabaseMemoryInternal.DatabaseEnum(query);
            GridLoad(grid, rowList, query?.ElementType, databaseEnum);
        }

        /// <summary>
        /// Load first grid config, then field config and data rows in parallel.
        /// </summary>
        private async Task GridLoadAsync(Grid grid, IQueryable query)
        {
            GridLoadSessionCreate(grid);
            GridSession gridSession = UtilSession.GridSessionFromGrid(grid);
            Type typeRow = query?.ElementType;

            // Load column definition into session state.
            GridLoadColumn(grid, typeRow);

            // Config get
            Task fieldConfigLoad = Task.FromResult(0);
            if (gridSession.TypeRow != typeRow)
            {
                Page.GridConfigResult gridConfigResult = new Page.GridConfigResult();
                grid.ComponentOwner<Page>().GridQueryConfig(grid, UtilDalType.TypeRowToTableNameCSharp(typeRow), gridConfigResult);
                // Load config into session state.
                await GridLoadConfigAsync(grid, typeRow, gridConfigResult.ConfigGridQuery);
                fieldConfigLoad = GridLoadConfigAsync(grid, typeRow, gridConfigResult.ConfigFieldQuery);
            }

            // Select rows and load data into session state.
            var rowLoad = GridLoadRowAsync(grid, query); // Load grid.

            await Task.WhenAll(fieldConfigLoad, rowLoad); // Load field config and row in parallel. Grid config needs to be loaded before because of RowCountMaxConfig dependency.
        }

        public async Task GridLoadAsync(Grid grid)
        {
            IQueryable query;
            if (grid.LookupDestGridIndex == null)
            {
                // Normal data grid
                query = grid.ComponentOwner<Page>().GridQuery(grid);

                await GridLoadAsync(grid, query);
                await GridRowSelectFirstAsync(grid);
            }
            else
            {
                // Lookup data grid
                var gridItemList = UtilSession.GridItemList();
                Grid gridLookup = grid;
                Grid gridDest = gridItemList.Where(item => item.GridIndex == gridLookup.LookupDestGridIndex).First().Grid;
                var gridItemLookup = gridItemList.Where(item => item.GridIndex == gridLookup.Index).First();
                int gridIndex = (int)gridItemLookup.Grid.LookupDestGridIndex;
                GridSession gridSession = UtilSession.GridSessionFromIndex(gridIndex);
                string fieldName = UtilSession.GridFieldNameFromCellIndex(gridIndex, (int)gridItemLookup.Grid.LookupDestCellIndex);
                GridCell gridCell = UtilSession.GridCellFromIndex(gridIndex, (int)gridItemLookup.Grid.LookupDestRowIndex, (int)gridItemLookup.Grid.LookupDestCellIndex - gridSession.OffsetColumn);
                Row row = UtilSession.GridRowFromIndex(gridIndex, (int)gridItemLookup.Grid.LookupDestRowIndex);
                query = gridDest.ComponentOwner<Page>().GridLookupQuery(gridDest, row, fieldName, gridCell.TextGet());

                await GridLoadAsync(grid, query);
            }
        }

        /// <summary>
        /// Refresh rows and cells of each data grid.
        /// </summary>
        public void GridRender()
        {
            foreach (GridItem gridItem in UtilSession.GridItemList())
            {
                if (gridItem.Grid != null)
                {
                    Page page = gridItem.Grid.ComponentOwner<Page>();

                    // Grid Reset
                    gridItem.Grid.ColumnList = new List<GridColumn>();
                    gridItem.Grid.RowList = new List<GridRow>();
                    gridItem.Grid.IsClickEnum = GridIsClickEnum.None;

                    if (gridItem.Grid.IsHide == false)
                    {
                        var config = new UtilColumnIndexConfig(gridItem);

                        // Grid Header
                        int gridColumnId = 0;
                        foreach (GridColumnItem gridColumnItem in config.ConfigList(gridItem.GridColumnItemList))
                        {
                            if (gridItem.GridSession.IsRange(config.IndexToIndexConfig(gridColumnItem.CellIndex)))
                            {
                                GridColumn gridColumn = new GridColumn();
                                gridColumn.Id = gridColumnId += 1;
                                gridColumn.Text = gridColumnItem.GridColumnSession.Text;
                                gridColumn.Description = gridColumnItem.GridColumnSession.Description;
                                gridColumn.IsSort = gridColumnItem.GridColumnSession.IsSort;
                                gridItem.Grid.ColumnList.Add(gridColumn);
                            }
                        }

                        // Grid Row
                        int gridRowId = 0;
                        foreach (GridRowItem gridRowItem in gridItem.GridRowList)
                        {
                            if (gridRowItem.GridRowSession != null)
                            {
                                GridRow gridRow = new GridRow();
                                gridRow.Id = gridRowId += 1;
                                gridRow.RowEnum = gridRowItem.GridRowSession.RowEnum;
                                gridRow.ErrorSave = gridRowItem.GridRowSession.ErrorSave;
                                gridItem.Grid.RowList.Add(gridRow);
                                gridRow.IsSelect = gridRowItem.GridRowSession.IsSelect;
                                gridRow.CellList = new List<GridCell>();

                                // Grid Cell
                                int gridCellId = 0;
                                foreach (GridCellItem gridCellItem in config.ConfigList(gridRowItem.GridCellList))
                                {
                                    if (gridCellItem.GridCellSession != null)
                                    {
                                        if (gridItem.GridSession.IsRange(config.IndexToIndexConfig(gridCellItem.CellIndex)))
                                        {
                                            GridCell gridCell = new GridCell();
                                            gridCell.Id = gridCellId += 1;
                                            gridRow.CellList.Add(gridCell);
                                            gridCell.Text = gridCellItem.GridCellSession.Text;
                                            
                                            // GridCellTextHtml
                                            Page.GridCellAnnotationResult result = new Page.GridCellAnnotationResult();
                                            page.GridCellAnnotation(gridItem.Grid, gridCellItem.FieldName, gridRowItem.GridRowSession.RowEnum, gridRowItem.GridRowSession.Row, result);
                                            gridCell.Html = UtilFramework.StringNull(result.Html);
                                            gridCell.HtmlIsEdit = result.HtmlIsEdit;
                                            gridCell.HtmlLeft = UtilFramework.StringNull(result.HtmlLeft);
                                            gridCell.HtmlRight = UtilFramework.StringNull(result.HtmlRight);
                                            gridCell.IsReadOnly = result.IsReadOnly;
                                            gridCell.IsPassword = result.IsPassword;
                                            gridCell.Align = result.Align;

                                            if (gridRowItem.GridRowSession.RowEnum == GridRowEnum.Filter)
                                            {
                                                gridCell.Placeholder = "Search";
                                            }
                                            if (gridRowItem.GridRowSession.RowEnum == GridRowEnum.New)
                                            {
                                                gridCell.Placeholder = "New";
                                            }
                                            gridCell.ErrorParse = gridCellItem.GridCellSession.ErrorParse;

                                            // Lookup open, close
                                            if (gridCellItem.GridCellSession.IsLookup == true)
                                            {
                                                if (gridCellItem.GridCellSession.IsLookupCloseForce == true)
                                                {
                                                    gridCellItem.GridCellSession.IsLookup = false;
                                                }
                                            }
                                            gridCell.IsLookup = gridCellItem.GridCellSession.IsLookup;
                                            gridCellItem.GridCellSession.IsLookupCloseForce = false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Process GridIsClickEnum for grid paging.
        /// </summary>
        private async Task ProcessGridIsClickEnumAsync()
        {
            var appJson = UtilServer.AppJson;

            if (appJson.RequestJson.Command == RequestCommand.GridIsClickEnum)
            {
                foreach (GridItem gridItem in UtilSession.GridItemList())
                {
                    if (gridItem.Grid != null)
                    {
                        var isClickEnum = gridItem.Grid.IsClickEnum;
                        if (appJson.RequestJson.Command == RequestCommand.GridIsClickEnum && appJson.RequestJson.ComponentId == gridItem.Grid.Id)
                        {
                            isClickEnum = appJson.RequestJson.GridIsClickEnum;
                        }
                        // PageLeft
                        if (isClickEnum == GridIsClickEnum.PageLeft)
                        {
                            gridItem.GridSession.OffsetColumn -= 1;
                            if (gridItem.GridSession.OffsetColumn < 0)
                            {
                                gridItem.GridSession.OffsetColumn = 0;
                            }
                        }
                        // PageRight
                        if (isClickEnum == GridIsClickEnum.PageRight)
                        {
                            gridItem.GridSession.OffsetColumn += 1;
                            var config = new UtilColumnIndexConfig(gridItem);
                            if (gridItem.GridSession.OffsetColumn > (config.Count - gridItem.GridSession.ColumnCountMax))
                            {
                                gridItem.GridSession.OffsetColumn = config.Count - gridItem.GridSession.ColumnCountMax;
                                if (gridItem.GridSession.OffsetColumn < 0)
                                {
                                    gridItem.GridSession.OffsetColumn = 0;
                                }
                            }
                        }
                        // PageUp
                        if (isClickEnum == GridIsClickEnum.PageUp)
                        {
                            gridItem.GridSession.OffsetRow -= gridItem.GridSession.RowCountMaxGet();
                            if (gridItem.GridSession.OffsetRow < 0)
                            {
                                gridItem.GridSession.OffsetRow = 0;
                            }
                            await GridLoadAsync(gridItem.Grid);
                        }
                        // PageDown
                        if (isClickEnum == GridIsClickEnum.PageDown)
                        {
                            int rowCount = gridItem.GridSession.GridRowSessionList.Where(item => item.RowEnum == GridRowEnum.Index).Count();
                            if (rowCount == gridItem.GridSession.RowCountMaxGet()) // Page down further on full grid only.
                            {
                                gridItem.GridSession.OffsetRow += gridItem.GridSession.RowCountMaxGet();
                                await GridLoadAsync(gridItem.Grid);
                            }
                        }
                        // Reload
                        if (isClickEnum == GridIsClickEnum.Reload)
                        {
                            gridItem.GridSession.OffsetRow = 0;
                            gridItem.GridSession.OffsetColumn = 0;
                            await GridLoadAsync(gridItem.Grid);
                        }
                        // Config
                        if (isClickEnum == GridIsClickEnum.Config)
                        {
                            if (gridItem.GridSession.TypeRow != null) // Do not show config if for example no query is defined for data grid.
                            {
                                Page page = gridItem.Grid.ComponentOwner<Page>();
                                string tableNameCSharp = UtilDalType.TypeRowToTableNameCSharp(gridItem.GridSession.TypeRow);
                                string configName = gridItem.Grid.ConfigName;
                                await page.ComponentPageShowAsync<PageConfigGrid>(init: (PageConfigGrid pageGridConfig) =>
                                {
                                    pageGridConfig.Init(tableNameCSharp, configName, null);
                                });
                            }
                        }
                    }
                }
            }
        }

        private static void ProcessGridSaveCellTextParse(Page page, Grid grid, GridRowItem gridRowItem, Row row, string fieldNameExclude)
        {
            foreach (GridCellItem gridCellItem in gridRowItem.GridCellList)
            {
                if (gridCellItem.GridCell != null)
                {
                    if (gridCellItem.FieldName != fieldNameExclude)
                    {
                        Data.CellTextFromValue(page, grid, gridRowItem.GridRowSession, gridCellItem.Field, gridCellItem.GridCellSession, row);
                    }
                }
            }
        }

        /// <summary>
        /// Update and insert data into database.
        /// </summary>
        private async Task ProcessGridSaveAsync()
        {
            var appJson = UtilServer.AppJson;
            if (appJson.RequestJson.Command == RequestCommand.GridCellIsModify)
            {
                // Parse user entered text
                foreach (GridItem gridItem in UtilSession.GridItemList())
                {
                    foreach (GridRowItem gridRowItem in gridItem.GridRowList)
                    {
                        foreach (GridCellItem gridCellItem in gridRowItem.GridCellList)
                        {
                            if (gridCellItem.GridCell != null)
                            {
                                bool isModify = appJson.RequestJson.Command == RequestCommand.GridCellIsModify && appJson.RequestJson.ComponentId == gridItem.Grid.Id && appJson.RequestJson.GridRowId == gridRowItem.GridRow.Id && appJson.RequestJson.GridCellId == gridCellItem.GridCell.Id;
                                if (isModify)
                                {
                                    Row row = null;
                                    if (gridRowItem.GridRowSession.RowEnum == GridRowEnum.Index)
                                    {
                                        // Parse Update
                                        if (gridRowItem.GridRowSession.RowUpdate == null)
                                        {
                                            gridRowItem.GridRowSession.RowUpdate = Data.RowCopy(gridRowItem.GridRowSession.Row);
                                        }
                                        row = gridRowItem.GridRowSession.RowUpdate;
                                    }
                                    if (gridRowItem.GridRowSession.RowEnum == GridRowEnum.New)
                                    {
                                        // Parse Insert
                                        if (gridRowItem.GridRowSession.RowInsert == null)
                                        {
                                            gridRowItem.GridRowSession.RowInsert = (Row)UtilFramework.TypeToObject(gridItem.GridSession.TypeRow);
                                        }
                                        row = gridRowItem.GridRowSession.RowInsert;
                                    }
                                    if (row != null)
                                    {
                                        gridCellItem.GridCellSession.IsModify = true; // Set back to null, once successfully saved.
                                        string textGet = gridCellItem.GridCell.TextGet();
                                        textGet = appJson.RequestJson.GridCellText;
                                        gridCellItem.GridCellSession.Text = textGet; // Set back to database selected value, once successfully saved.
                                        Grid grid = gridItem.Grid;
                                        Page page = grid.ComponentOwner<Page>();
                                        object valueBefore = gridCellItem.Field.PropertyInfo.GetValue(row);
                                        bool isHandled = false;
                                        gridCellItem.GridCellSession.ErrorParse = null;
                                        string text = gridCellItem.GridCellSession.Text;
                                        string errorParse = null;
                                        try
                                        {
                                            if (text == null && !UtilFramework.IsNullable(gridCellItem.Field.PropertyInfo.PropertyType))
                                            {
                                                if (!(gridRowItem.GridRowSession.RowEnum == GridRowEnum.New)) // Not nullable value in cell can be set back to null in new row.
                                                {
                                                    throw new Exception("Value can not be null!");
                                                }
                                            }
                                            if (text != null)
                                            {
                                                page.GridCellParse(grid, gridCellItem.Field.PropertyInfo.Name, gridCellItem.GridCellSession.Text, row, out isHandled); // Custom parse user entered cell text.
                                            }
                                            if (!isHandled)
                                            {
                                                Data.CellTextParse(gridCellItem.Field, gridCellItem.GridCellSession.Text, row, out errorParse); // Default parse user entered cell text.
                                                gridCellItem.GridCellSession.ErrorParse = errorParse;
                                            }

                                        }
                                        catch (Exception exception)
                                        {
                                            errorParse = UtilFramework.ExceptionToString(exception);
                                            gridRowItem.GridRowSession.RowUpdate = null;
                                            gridRowItem.GridRowSession.RowInsert = null;
                                        }
                                        gridCellItem.GridCellSession.ErrorParse = errorParse;

                                        // Autocomplete
                                        bool isAutocomplete = Data.CellTextParseIsAutocomplete(gridCellItem);
                                        string fieldNameExclude = null;
                                        if (!isAutocomplete)
                                        {
                                            // If method CellTextParse(); did not change underlying value do not call method CellTextFromValue(); for this field. Prevent for example autocomplete if user entered "2." to "2" for decimal value.
                                            fieldNameExclude = gridCellItem.FieldName;
                                        }
                                        ProcessGridSaveCellTextParse(page, grid, gridRowItem, row, fieldNameExclude);
                                    }
                                }
                            }
                        }
                    }
                }

                // Save row to database
                foreach (GridItem gridItem in UtilSession.GridItemList())
                {
                    foreach (GridRowItem gridRowItem in gridItem.GridRowList)
                    {
                        if (gridRowItem.GridRowSession.RowEnum == GridRowEnum.Index && gridRowItem.GridRowSession.RowUpdate != null)
                        {
                            // Update to database
                            gridRowItem.GridRowSession.ErrorSave = null;
                            try
                            {
                                bool isHandled = await gridItem.Grid.ComponentOwner<Page>().GridUpdateAsync(gridItem.Grid, gridRowItem.GridRowSession.Row, gridRowItem.GridRowSession.RowUpdate, gridItem.GridSession.DatabaseEnum);
                                if (!isHandled)
                                {
                                    await Data.UpdateAsync(gridRowItem.GridRowSession.Row, gridRowItem.GridRowSession.RowUpdate, gridItem.GridSession.DatabaseEnum); // Default database record update
                                }
                                else
                                {
                                    // Custom database record update might also have changed other fields like new primary key or UOM.
                                    List<Field> fieldList = null;
                                    GridLoad(gridItem.GridIndex, gridRowItem.RowIndex, gridRowItem.GridRowSession.RowUpdate, gridItem.GridSession.TypeRow, GridRowEnum.Index, ref fieldList);
                                }
                                gridRowItem.GridRowSession.Row = gridRowItem.GridRowSession.RowUpdate;
                                foreach (GridCellSession gridCellSession in gridRowItem.GridRowSession.GridCellSessionList)
                                {
                                    gridCellSession.IsModify = false;
                                    gridCellSession.TextOld = null;
                                    gridCellSession.ErrorParse = null;
                                }
                            }
                            catch (Exception exception)
                            {
                                gridRowItem.GridRowSession.ErrorSave = UtilFramework.ExceptionToString(exception);
                            }
                            gridRowItem.GridRowSession.RowUpdate = null;
                        }
                        if (gridRowItem.GridRowSession.RowEnum == GridRowEnum.New && gridRowItem.GridRowSession.RowInsert != null)
                        {
                            // Insert to database
                            try
                            {
                                bool isHandled = await gridItem.Grid.ComponentOwner<Page>().GridInsertAsync(gridItem.Grid, gridRowItem.GridRowSession.RowInsert, gridItem.GridSession.DatabaseEnum);
                                if (!isHandled)
                                {
                                    await Data.InsertAsync(gridRowItem.GridRowSession.RowInsert, gridItem.GridSession.DatabaseEnum);
                                }
                                gridRowItem.GridRowSession.Row = gridRowItem.GridRowSession.RowInsert;

                                // Load new primary key from session into data grid.
                                List<Field> fieldList = null;
                                GridLoad(gridItem.GridIndex, gridRowItem.RowIndex, gridRowItem.GridRowSession.Row, gridItem.GridSession.TypeRow, GridRowEnum.Index, ref fieldList);

                                // Add new "insert row" at end of data grid.
                                GridLoadAddRowNew(gridItem.GridIndex);
                            }
                            catch (Exception exception)
                            {
                                gridRowItem.GridRowSession.ErrorSave = UtilFramework.ExceptionToString(exception);
                            }
                            gridRowItem.GridRowSession.RowInsert = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// If no row in data grid is selected, select first row.
        /// </summary>
        private async Task GridRowSelectFirstAsync(Grid grid)
        {
            AppInternal appInternal = UtilServer.AppInternal;
            int gridIndex = UtilSession.GridToIndex(grid);
            foreach (GridRowSession gridRowSession in GridSessionList[gridIndex].GridRowSessionList)
            {
                if (gridRowSession.RowEnum == GridRowEnum.Index) // By default only select data rows. 
                {
                    gridRowSession.IsSelect = true;
                    await grid.ComponentOwner<Page>().GridRowSelectedAsync(grid);
                    break;
                }
            }
        }

        private async Task ProcessGridLookupOpenAsync()
        {
            var appJson = UtilServer.AppJson;
            if (appJson.RequestJson.Command == RequestCommand.GridCellIsModify)
            {
                foreach (GridItem gridItem in UtilSession.GridItemList())
                {
                    foreach (GridRowItem gridRowItem in gridItem.GridRowList)
                    {
                        foreach (GridCellItem gridCellItem in gridRowItem.GridCellList)
                        {
                            bool isModify = appJson.RequestJson.Command == RequestCommand.GridCellIsModify && appJson.RequestJson.ComponentId == gridItem.Grid?.Id && appJson.RequestJson.GridRowId == gridRowItem.GridRow?.Id && appJson.RequestJson.GridCellId == gridCellItem.GridCell?.Id;
                            if (appJson.RequestJson.GridCellTextIsInternal)
                            {
                                isModify = false; // Do not open lookup (again) after lookup row select.
                            }
                            if (isModify == true)
                            {
                                gridCellItem.GridCellSession.IsLookup = true;
                                string textGet = appJson.RequestJson.GridCellText;
                                var query = gridItem.Grid.ComponentOwner<Page>().GridLookupQuery(gridItem.Grid, gridRowItem.GridRowSession.Row, gridCellItem.FieldName, textGet);
                                if (query != null)
                                {
                                    await GridLoadAsync(gridItem.Grid.GridLookup(), query); // Load lookup.
                                    gridItem.Grid.GridLookupOpen(gridItem, gridRowItem, gridCellItem);
                                }
                                return;
                            }
                        }
                    }
                }
            }
        }

        private async Task ProcessGridFilterAsync()
        {
            var appJson = UtilServer.AppJson;
            if (appJson.RequestJson.Command == RequestCommand.GridCellIsModify)
            {
                List<GridItem> gridItemReloadList = new List<GridItem>();
                foreach (GridItem gridItem in UtilSession.GridItemList())
                {
                    foreach (GridRowItem gridRowItem in gridItem.GridRowList)
                    {
                        foreach (GridCellItem gridCellItem in gridRowItem.GridCellList)
                        {
                            if (gridCellItem.GridCell != null)
                            {
                                bool isModify = appJson.RequestJson.Command == RequestCommand.GridCellIsModify && appJson.RequestJson.ComponentId == gridItem.Grid.Id && appJson.RequestJson.GridRowId == gridRowItem.GridRow.Id && appJson.RequestJson.GridCellId == gridCellItem.GridCell.Id;
                                if (isModify)
                                {
                                    gridCellItem.GridCellSession.IsModify = true; // Set back to null, once successfully parsed.
                                    gridCellItem.GridCellSession.TextOld = UtilFramework.StringNull(gridCellItem.GridCellSession.Text);
                                    string textGet = appJson.RequestJson.GridCellText;
                                    gridCellItem.GridCellSession.Text = textGet;
                                    if (gridRowItem.GridRowSession.RowEnum == GridRowEnum.Filter)
                                    {
                                        Grid grid = gridItem.Grid;
                                        Page page = grid.ComponentOwner<Page>();
                                        Filter filter = new Filter();
                                        filter.Load(gridRowItem);
                                        gridCellItem.GridCellSession.ErrorParse = null;
                                        string errorParse = null;
                                        try
                                        {
                                            bool isHandled = false;
                                            if (gridCellItem.GridCellSession.Text != null)
                                            {
                                                page.GridCellParseFilter(grid, gridCellItem.FieldName, gridCellItem.GridCellSession.Text, filter, out isHandled); // Custom parse user entered filter text.
                                            }
                                            if (isHandled == false)
                                            {
                                                Data.CellTextParseFilter(gridCellItem.Field, gridCellItem.GridCellSession.Text, filter, out errorParse); // Default parse user entered filter text.
                                            }
                                            filter.Save(gridRowItem);
                                        }
                                        catch (Exception exception)
                                        {
                                            errorParse = exception.Message;
                                        }
                                        gridCellItem.GridCellSession.IsModify = false;
                                        gridCellItem.GridCellSession.TextOld = null;
                                        gridCellItem.GridCellSession.ErrorParse = errorParse;
                                        if (!gridItemReloadList.Contains(gridItem))
                                        {
                                            gridItemReloadList.Add(gridItem);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Grid reload from database
                foreach (GridItem gridItem in gridItemReloadList)
                {
                    await GridLoadAsync(gridItem.Grid);
                }
            }
        }

        private async Task ProcessGridIsSortClickAsync()
        {
            var appJson = UtilServer.AppJson;
            if (appJson.RequestJson.Command == RequestCommand.GridIsClickSort || appJson.RequestJson.Command == RequestCommand.GridIsClickConfig)
            {
                List<GridItem> gridItemReloadList = new List<GridItem>();
                foreach (GridItem gridItem in UtilSession.GridItemList())
                {
                    foreach (GridColumnItem gridColumnItem in gridItem.GridColumnItemList)
                    {
                        bool isClickSort = appJson.RequestJson.Command == RequestCommand.GridIsClickSort && appJson.RequestJson.ComponentId == gridItem.Grid?.Id && appJson.RequestJson.GridColumnId == gridColumnItem.GridColumn?.Id;
                        if (isClickSort)
                        {
                            if (!gridItemReloadList.Contains(gridItem))
                            {
                                gridItemReloadList.Add(gridItem);
                            }
                            bool? isSort = gridItem.GridSession.GridColumnSessionList[gridColumnItem.CellIndex].IsSort;
                            if (isSort == null)
                            {
                                isSort = false;
                            }
                            else
                            {
                                isSort = !isSort;
                            }
                            foreach (GridColumnSession gridColumnSession in gridItem.GridSession.GridColumnSessionList)
                            {
                                gridColumnSession.IsSort = null;
                            }
                            gridItem.GridSession.GridColumnSessionList[gridColumnItem.CellIndex].IsSort = isSort;
                            gridItem.GridSession.OffsetRow = 0; // Reset paging.
                        }
                        bool isClickConfig = appJson.RequestJson.Command == RequestCommand.GridIsClickConfig && appJson.RequestJson.ComponentId == gridItem.Grid?.Id && appJson.RequestJson.GridColumnId == gridColumnItem.GridColumn?.Id;
                        if (isClickConfig)
                        {
                            Page page = gridItem.Grid.ComponentOwner<Page>();
                            string tableNameCSharp = UtilDalType.TypeRowToTableNameCSharp(gridItem.GridSession.TypeRow);
                            string configName = gridItem.Grid.ConfigName;
                            string fieldName = gridColumnItem.Field.PropertyInfo.Name;
                            await page.ComponentPageShowAsync<PageConfigGrid>(init: (PageConfigGrid pageGridConfig) =>
                            {
                                pageGridConfig.Init(tableNameCSharp, configName, fieldName);
                            });
                        }
                    }
                }

                // Grid reload from database
                foreach (GridItem gridItem in gridItemReloadList)
                {
                    await GridLoadAsync(gridItem.Grid);
                }
            }
        }

        private void ProcessGridLookupRowIsClick()
        {
            bool isClickRow = UtilSession.Request(RequestCommand.GridIsClickRow, out RequestJson requestJson, out Grid gridLookUp);
            var gridItemList = UtilSession.GridItemList();
            foreach (GridItem gridItemLookup in gridItemList)
            {
                if (gridItemLookup.Grid?.GridLookupIsOpen() == true)
                {
                    int gridIndex = (int)gridItemLookup.Grid.LookupDestGridIndex;
                    if (requestJson.ComponentId != gridItemLookup.Grid?.Id) // Close if not clicked into lookup grid
                    {
                        // Close LookUp
                        gridItemLookup.Grid.GridLookupClose(gridItemList[gridIndex], true);
                        gridItemLookup.Grid.LookupDestGridIndex = null;
                    }
                    if (isClickRow && requestJson.ComponentId == gridItemLookup.Grid.Id) // Clicked row in lookup grid
                    {
                        Grid grid = UtilSession.GridFromIndex(gridIndex);
                        GridSession gridSession = UtilSession.GridSessionFromIndex(gridIndex);
                        GridRowEnum gridRowEnum = UtilSession.GridRowSessionFromIndex(gridIndex, (int)gridItemLookup.Grid.LookupDestRowIndex).RowEnum;
                        string fieldName = UtilSession.GridFieldNameFromCellIndex(gridIndex, (int)gridItemLookup.Grid.LookupDestCellIndex);
                        bool isClose = false;
                        foreach (GridRowItem gridRowItemLookup in gridItemLookup.GridRowList)
                        {
                            if (requestJson.GridRowId == gridRowItemLookup.GridRow.Id && gridRowItemLookup.GridRowSession.RowEnum == GridRowEnum.Index)
                            {
                                string text = gridItemLookup.Grid.ComponentOwner<Page>().GridLookupRowSelected(grid, fieldName, gridRowEnum, gridRowItemLookup.GridRowSession.Row);
                                GridCell gridCell = UtilSession.GridCellFromIndex(gridIndex, (int)gridItemLookup.Grid.LookupDestRowIndex, (int)gridItemLookup.Grid.LookupDestCellIndex - gridSession.OffsetColumn);
                                GridRow gridRow = gridItemList[gridIndex].GridRowList[gridItemLookup.Grid.LookupDestRowIndex.Value].GridRow;
                                UtilServer.AppJson.RequestJson = new RequestJson { Command = RequestCommand.GridCellIsModify, ComponentId = grid.Id, GridRowId = gridRow.Id, GridCellId = gridCell.Id, GridCellText = text, GridCellTextIsInternal = true };
                                isClose = true;
                                break;
                            }
                        }

                        // Close LookUp
                        if (isClose)
                        {
                            gridItemLookup.Grid.GridLookupClose(gridItemList[gridIndex], true);
                            gridItemLookup.Grid.LookupDestGridIndex = null;
                        }
                    }
                }
            }
        }

        private async Task ProcessGridRowIsClick()
        {
            var appJson = UtilServer.AppJson;
            if (appJson.RequestJson.Command == RequestCommand.GridIsClickRow)
            {
                foreach (GridItem gridItem in UtilSession.GridItemList())
                {
                    // Get IsClick
                    int rowIndexIsClick = -1;
                    foreach (GridRowItem gridRowItem in gridItem.GridRowList)
                    {
                        bool isClick = appJson.RequestJson.Command == RequestCommand.GridIsClickRow && appJson.RequestJson.ComponentId == gridItem.Grid?.Id && appJson.RequestJson.GridRowId == gridRowItem.GridRow.Id;
                        if (isClick)
                        {
                            if (gridRowItem.GridRowSession.RowEnum == GridRowEnum.Index) // Do not select filter or new data row.
                            {
                                rowIndexIsClick = gridRowItem.RowIndex;
                                break;
                            }
                        }
                    }

                    // Set IsSelect
                    if (rowIndexIsClick != -1)
                    {
                        foreach (GridRowItem gridRowItem in gridItem.GridRowList)
                        {
                            if (gridRowItem.GridRowSession != null) // Outgoing grid might have less rows
                            {
                                gridRowItem.GridRowSession.IsSelect = false;
                            }
                        }
                        foreach (GridRowItem gridRowItem in gridItem.GridRowList)
                        {
                            if (gridRowItem.GridRowSession != null && gridRowItem.RowIndex == rowIndexIsClick)
                            {
                                gridRowItem.GridRowSession.IsSelect = true;
                                break;
                            }
                        }
                        await gridItem.Grid.ComponentOwner<Page>().GridRowSelectedAsync(gridItem.Grid);
                    }
                }
            }
        }

        /// <summary>
        /// Process incoming data grid.
        /// </summary>
        public async Task ProcessAsync()
        {
            AppInternal appInternal = UtilServer.AppInternal;

            await ProcessGridFilterAsync();
            await ProcessGridIsSortClickAsync();
            ProcessGridLookupRowIsClick();
            await ProcessGridSaveAsync();
            await ProcessGridRowIsClick(); // Load for example detail grids.
            await ProcessGridLookupOpenAsync(); // Load lookup data grid.
            await ProcessGridIsClickEnumAsync();

            await Grid2.ProcessAsync();
        }
    }

    /// <summary>
    /// Grid filter row.
    /// </summary>
    public class Filter
    {
        /// <summary>
        /// (FieldName, FilterItem).
        /// </summary>
        private Dictionary<string, FilterItem> filterList = new Dictionary<string, FilterItem>();

        /// <summary>
        /// Set filter value.
        /// </summary>
        public void SetValue(string fieldName, object filterValue, FilterOperator filterOperator)
        {
            filterList[fieldName].FilterValue = filterValue;
            filterList[fieldName].FilterOperator = filterOperator;
        }

        /// <summary>
        /// Set filter value with autocomplete text.
        /// </summary>
        /// <param name="text">Autocomplete text.</param>
        public void SetValue(string fieldName, object filterValue, FilterOperator filterOperator, string text)
        {
            filterList[fieldName].FilterValue = filterValue;
            filterList[fieldName].FilterOperator = filterOperator;
            filterList[fieldName].Text = text;
        }

        /// <summary>
        /// Load session into filter.
        /// </summary>
        internal void Load(GridRowItem gridRowItem)
        {
            filterList.Clear();
            foreach (GridCellItem gridCellItem in gridRowItem.GridCellList)
            {
                FilterItem filterItem = new FilterItem()
                {
                    Text = gridCellItem.GridCellSession.Text,
                    FilterValue = gridCellItem.GridCellSession.FilterValue,
                    FilterOperator = gridCellItem.GridCellSession.FilterOperator
                };
                filterList.Add(gridCellItem.FieldName, filterItem);
            }
        }

        /// <summary>
        /// Save filter to session.
        /// </summary>
        internal void Save(GridRowItem gridRowItem)
        {
            foreach (GridCellItem gridCellItem in gridRowItem.GridCellList)
            {
                FilterItem filterItem = filterList[gridCellItem.FieldName];
                if (filterItem.FilterValue != null)
                {
                    UtilFramework.Assert(filterItem.FilterValue.GetType() == UtilFramework.TypeUnderlying(gridCellItem.Field.PropertyInfo.PropertyType), "FilterValue wrong type!");
                }

                // Autocomplete
                bool isAutocomplete = Data.CellTextParseIsAutocomplete(gridCellItem);
                if (isAutocomplete)
                {
                    // Autocomplete only not delete key pressed and no error.
                    gridCellItem.GridCellSession.Text = filterItem.Text;
                }
                gridCellItem.GridCellSession.FilterValue = filterItem.FilterValue;
                gridCellItem.GridCellSession.FilterOperator = filterItem.FilterOperator;
            }
        }
    }

    internal class FilterItem
    {
        /// <summary>
        /// Set filter text for autocmplete.
        /// </summary>
        public string Text;

        public object FilterValue;

        public FilterOperator FilterOperator;
    }

    /// <summary>
    /// Stores server side grid session data.
    /// </summary>
    internal sealed class GridSession
    {
        /// <summary>
        /// TypeRow of loaded data grid.
        /// </summary>
        public Type TypeRow;

        /// <summary>
        /// Determines where to write data back. To database or memory.
        /// </summary>
        public DatabaseEnum DatabaseEnum;

        /// <summary>
        /// Grid columns in server side session state.
        /// </summary>
        public List<GridColumnSession> GridColumnSessionList = new List<GridColumnSession>();

        /// <summary>
        /// Grid rows in server side session state.
        /// </summary>
        public List<GridRowSession> GridRowSessionList = new List<GridRowSession>();

        public int? RowCountMaxConfig;

        /// <summary>
        /// Returns number of rows to load. Default value is 10.
        /// </summary>
        public int RowCountMaxGet()
        {
            return RowCountMaxConfig.HasValue ? RowCountMaxConfig.Value : 10; // Default value if no config.
        }

        public int ColumnCountMax = 5;

        public int OffsetRow = 0;

        public int OffsetColumn = 0;

        public bool IsRange(int index)
        {
            return index >= OffsetColumn && index <= OffsetColumn + (ColumnCountMax - 1);
        }
    }

    internal sealed class GridColumnSession
    {
        /// <summary>
        /// FieldNameCSharp.
        /// </summary>
        public string FieldName;

        /// <summary>
        /// Gets or sets Text. Session state for column header text.
        /// </summary>
        public string Text;

        /// <summary>
        /// Gets or sets Description. Hover with mouse over column to see tooltip window.
        /// </summary>
        public string Description;

        /// <summary>
        /// Gets or sets IsVisible. Session state indicating column is shown.
        /// </summary>
        public bool IsVisible;

        /// <summary>
        /// Gets or sets Sort (FieldNameCSharpSort). Session state for column order.
        /// </summary>
        public double? Sort;

        /// <summary>
        /// Gets or sets IsSort. Session statue for column sort down or up.
        /// </summary>
        public bool? IsSort;
    }

    internal sealed class GridRowSession
    {
        public Row Row;

        public Row RowUpdate;

        public Row RowInsert;

        public bool IsSelect;

        public string ErrorSave;

        public List<GridCellSession> GridCellSessionList = new List<GridCellSession>();

        public GridRowEnum RowEnum;
    }

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

    internal sealed class GridCellSession
    {
        public string Text;

        public string TextOld;

        public string ErrorParse;

        public bool IsModify;

        public bool IsLookup;

        /// <summary>
        /// Gets pr sets IsLookupCloseForce. Enforce lookup closing even if set to open later in the process.
        /// </summary>
        public bool IsLookupCloseForce;

        public object FilterValue;

        public FilterOperator FilterOperator;
    }
}
