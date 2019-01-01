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
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using static Framework.DataAccessLayer.UtilDalType;
    using static Framework.Session.UtilSession;

    /// <summary>
    /// Application server side session state. Get it with property UtilServer.AppInternal.AppSession
    /// </summary>
    internal class AppSession
    {
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

            List<Field> fieldList = UtilDalType.TypeRowToFieldList(typeRow);

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
            if (typeRow == null || configFieldQuery == null)
            {
                foreach (var columnSession in gridSession.GridColumnSessionList)
                {
                    columnSession.TextConfig = null; // Reset
                }
            }
            else
            {
                string tableNameCSharp = UtilDalType.TypeRowToTableNameCSharp(typeRow);
                var configFieldList = await Data.SelectAsync(configFieldQuery);
                // (FieldName, FrameworkConfigFieldBuiltIn)
                Dictionary<string, FrameworkConfigFieldBuiltIn> fieldList = new Dictionary<string, FrameworkConfigFieldBuiltIn>();
                foreach (var frameworkConfigFieldBuiltIn in configFieldList)
                {
                    if (frameworkConfigFieldBuiltIn.TableNameCSharp != null) // If set, it needs to be correct.
                    {
                        UtilFramework.Assert(frameworkConfigFieldBuiltIn.TableNameCSharp == tableNameCSharp, string.Format("TableNameCSharp wrong! ({0}; {1})", tableNameCSharp, frameworkConfigFieldBuiltIn.TableNameCSharp));
                    }
                    fieldList.Add(frameworkConfigFieldBuiltIn.FieldNameCSharp, frameworkConfigFieldBuiltIn);
                }
                foreach (var columnSession in gridSession.GridColumnSessionList)
                {
                    if (fieldList.TryGetValue(columnSession.FieldName, out var frameworkConfigFieldBuiltIn))
                    {
                        columnSession.TextConfig = frameworkConfigFieldBuiltIn.Text;
                    }
                }
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
                Page.ConfigResult configResult = new Page.ConfigResult();
                grid.ComponentOwner<Page>().GridQueryConfig(grid, configResult);
                // Load config into session state.
                await GridLoadConfigAsync(grid, typeRow, configResult.ConfigGridQuery);
                fieldConfigLoad = GridLoadConfigAsync(grid, typeRow, configResult.ConfigFieldQuery);
            }

            // Select rows and load data into session state.
            var rowLoad = GridLoadRowAsync(grid, query); // Load grid.

            await Task.WhenAll(fieldConfigLoad, rowLoad); // Load field config and row in parallel. Grid config needs to be loaded before because of RowCountMaxConfig dependency.
        }

        public async Task GridLoadAsync(Grid grid)
        {
            var query = grid.ComponentOwner<Page>().GridQuery(grid);

            await GridLoadAsync(grid, query);
            await GridRowSelectFirstAsync(grid);
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
                        // Grid Header
                        foreach (GridColumnItem gridColumnItem in gridItem.GridColumnItemList)
                        {
                            if (gridItem.GridSession.IsRange(gridColumnItem.CellIndex))
                            {
                                GridColumn gridColumn = new GridColumn();
                                gridColumn.Text = gridColumnItem.GridColumnSession.TextGet();
                                gridColumn.IsSort = gridColumnItem.GridColumnSession.IsSort;
                                gridItem.Grid.ColumnList.Add(gridColumn);
                            }
                        }

                        // Grid Row
                        foreach (GridRowItem gridRowItem in gridItem.GridRowList)
                        {
                            if (gridRowItem.GridRowSession != null)
                            {
                                GridRow gridRow = new GridRow();
                                gridRow.RowEnum = gridRowItem.GridRowSession.RowEnum;
                                gridRow.ErrorSave = gridRowItem.GridRowSession.ErrorSave;
                                gridItem.Grid.RowList.Add(gridRow);
                                gridRow.IsSelect = gridRowItem.GridRowSession.IsSelect;
                                gridRow.CellList = new List<GridCell>();

                                // Grid Cell
                                foreach (GridCellItem gridCellItem in gridRowItem.GridCellList)
                                {
                                    if (gridCellItem.GridCellSession != null)
                                    {
                                        if (gridItem.GridSession.IsRange(gridCellItem.CellIndex))
                                        {
                                            GridCell gridCell = new GridCell();
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
                                            gridCell.IsModify = gridCellItem.GridCellSession.IsModify;
                                            gridCell.MergeId = gridCellItem.GridCellSession.MergeId;

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
        /// Process GridIsClickEnum.
        /// </summary>
        /// <returns></returns>
        private async Task ProcessGridIsClickEnumAsync()
        {
            foreach (GridItem gridItem in UtilSession.GridItemList())
            {
                if (gridItem.Grid != null)
                {
                    // PageLeft
                    if (gridItem.Grid.IsClickEnum == GridIsClickEnum.PageLeft)
                    {
                        gridItem.GridSession.OffsetColumn -= 1;
                        if (gridItem.GridSession.OffsetColumn < 0)
                        {
                            gridItem.GridSession.OffsetColumn = 0;
                        }
                    }
                    // PageRight
                    if (gridItem.Grid.IsClickEnum == GridIsClickEnum.PageRight)
                    {
                        gridItem.GridSession.OffsetColumn += 1;
                        if (gridItem.GridSession.OffsetColumn > (gridItem.GridSession.GridColumnSessionList.Count - gridItem.GridSession.ColumnCountMax))
                        {
                            gridItem.GridSession.OffsetColumn = gridItem.GridSession.GridColumnSessionList.Count - gridItem.GridSession.ColumnCountMax;
                            if (gridItem.GridSession.OffsetColumn < 0)
                            {
                                gridItem.GridSession.OffsetColumn = 0;
                            }
                        }
                    }
                    // PageUp
                    if (gridItem.Grid.IsClickEnum == GridIsClickEnum.PageUp)
                    {
                        gridItem.GridSession.OffsetRow -= gridItem.GridSession.RowCountMaxGet();
                        if (gridItem.GridSession.OffsetRow < 0)
                        {
                            gridItem.GridSession.OffsetRow = 0;
                        }
                        await GridLoadAsync(gridItem.Grid);
                    }
                    // PageDown
                    if (gridItem.Grid.IsClickEnum == GridIsClickEnum.PageDown)
                    {
                        int rowCount = gridItem.GridSession.GridRowSessionList.Where(item => item.RowEnum == GridRowEnum.Index).Count();
                        if (rowCount == gridItem.GridSession.RowCountMaxGet()) // Page down further on full grid only.
                        {
                            gridItem.GridSession.OffsetRow += gridItem.GridSession.RowCountMaxGet();
                            await GridLoadAsync(gridItem.Grid);
                        }
                    }
                    // Reload
                    if (gridItem.Grid.IsClickEnum == GridIsClickEnum.Reload)
                    {
                        gridItem.GridSession.OffsetRow = 0;
                        gridItem.GridSession.OffsetColumn = 0;
                        await GridLoadAsync(gridItem.Grid);
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
            // Parse user entered text
            foreach (GridItem gridItem in UtilSession.GridItemList())
            {
                foreach (GridRowItem gridRowItem in gridItem.GridRowList)
                {
                    foreach (GridCellItem gridCellItem in gridRowItem.GridCellList)
                    {
                        if (gridCellItem.GridCell != null)
                        {
                            if (gridCellItem.GridCell.IsModify)
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
                                    gridCellItem.GridCellSession.Text = gridCellItem.GridCell.TextGet(); // Set back to database selected value, once successfully saved.
                                    Grid grid = gridItem.Grid;
                                    Page page = grid.ComponentOwner<Page>();
                                    object valueBefore = gridCellItem.Field.PropertyInfo.GetValue(row);
                                    bool isHandled = false;
                                    gridCellItem.GridCellSession.ErrorParse = null;
                                    string text = gridCellItem.GridCellSession.Text;
                                    string errorParse = null;
                                    try
                                    {
                                        if (!UtilFramework.IsNullable(gridCellItem.Field.PropertyInfo.PropertyType) && text == null)
                                        {
                                            throw new Exception("Value can not be null!");
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
                                        errorParse = exception.Message;
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
                            gridCellItem.GridCellSession.MergeId = gridCellItem.GridCell.MergeId;
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
                                gridCellSession.Error = null;
                            }
                        }
                        catch (Exception exception)
                        {
                            gridRowItem.GridRowSession.ErrorSave = exception.Message;
                        }
                        gridRowItem.GridRowSession.RowUpdate = null;
                    }
                    if (gridRowItem.GridRowSession.RowEnum == GridRowEnum.New && gridRowItem.GridRowSession.RowInsert != null)
                    {
                        // Insert to database
                        try
                        {
                            var rowNew = await Data.InsertAsync(gridRowItem.GridRowSession.RowInsert, gridItem.GridSession.DatabaseEnum);
                            gridRowItem.GridRowSession.Row = rowNew;

                            // Load new primary key into data grid.
                            List<Field> fieldList = null;
                            GridLoad(gridItem.GridIndex, gridRowItem.RowIndex, rowNew, gridItem.GridSession.TypeRow, GridRowEnum.Index, ref fieldList);

                            // Add new "insert row" at end of data grid.
                            GridLoadAddRowNew(gridItem.GridIndex);
                        }
                        catch (Exception exception)
                        {
                            gridRowItem.GridRowSession.ErrorSave = exception.Message;
                        }
                        gridRowItem.GridRowSession.RowInsert = null;
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
            foreach (GridItem gridItem in UtilSession.GridItemList())
            {
                foreach (GridRowItem gridRowItem in gridItem.GridRowList)
                {
                    foreach (GridCellItem gridCellItem in gridRowItem.GridCellList)
                    {
                        if (gridCellItem.GridCell?.IsModify == true)
                        {
                            gridCellItem.GridCellSession.IsLookup = true;
                            var query = gridItem.Grid.ComponentOwner<Page>().GridLookupQuery(gridItem.Grid, gridRowItem.GridRowSession.Row, gridCellItem.FieldName, gridCellItem.GridCell.TextGet());
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

        private async Task ProcessGridFilterAsync()
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
                            if (gridCellItem.GridCell.IsModify)
                            {
                                gridCellItem.GridCellSession.IsModify = true; // Set back to null, once successfully parsed.
                                gridCellItem.GridCellSession.TextOld = UtilFramework.StringNull(gridCellItem.GridCellSession.Text);
                                gridCellItem.GridCellSession.Text = gridCellItem.GridCell.TextGet();
                                if (gridRowItem.GridRowSession.RowEnum == GridRowEnum.Filter)
                                {
                                    try
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
                                                page.GridCellParseFilter(grid, gridItem.GridSession.TypeRow, gridCellItem.FieldName, gridCellItem.GridCellSession.Text, filter, out isHandled); // Custom parse user entered filter text.
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
                                    catch (Exception exception)
                                    {
                                        gridCellItem.GridCellSession.Error = exception.ToString();
                                    }
                                }
                            }
                            gridCellItem.GridCellSession.MergeId = gridCellItem.GridCell.MergeId;
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

        private async Task ProcessGridIsSortClickAsync()
        {
            List<GridItem> gridItemReloadList = new List<GridItem>();
            foreach (GridItem gridItem in UtilSession.GridItemList())
            {
                foreach (GridColumnItem gridColumnItem in gridItem.GridColumnItemList)
                {
                    if (gridColumnItem.GridColumn?.IsClickSort == true)
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
                }
            }

            // Grid reload from database
            foreach (GridItem gridItem in gridItemReloadList)
            {
                await GridLoadAsync(gridItem.Grid);
            }
        }

        private void ProcessGridLookupRowIsClick()
        {
            var gridItemList = UtilSession.GridItemList();
            foreach (GridItem gridItemLookup in gridItemList)
            {
                if (gridItemLookup.Grid?.GridLookupIsOpen() == true)
                {
                    foreach (GridRowItem gridRowItemLookup in gridItemLookup.GridRowList)
                    {
                        if (gridRowItemLookup.GridRow.IsClick)
                        {
                            int gridIndex = (int)gridItemLookup.Grid.LookupGridIndex;
                            Grid grid = UtilSession.GridFromIndex(gridIndex);
                            GridSession gridSession = UtilSession.GridSessionFromIndex(gridIndex);
                            Row row = UtilSession.GridRowFromIndex(gridIndex, (int)gridItemLookup.Grid.LookupRowIndex);
                            string fieldName = UtilSession.GridFieldNameFromCellIndex(gridIndex, (int)gridItemLookup.Grid.LookupCellIndex);
                            string text = gridItemLookup.Grid.ComponentOwner<Page>().GridLookupSelected(grid, row, fieldName, gridRowItemLookup.GridRowSession.Row);

                            GridCell gridCell = UtilSession.GridCellFromIndex(gridIndex, (int)gridItemLookup.Grid.LookupRowIndex, (int)gridItemLookup.Grid.LookupCellIndex - gridSession.OffsetColumn);
                            gridCell.Text = text;
                            gridCell.IsModify = true;
                            gridItemLookup.Grid.GridLookupClose(gridItemList[gridIndex], true);
                            return;
                        }
                    }
                }
            }
        }

        private async Task ProcessGridRowIsClick()
        {
            foreach (GridItem gridItem in UtilSession.GridItemList())
            {
                // Get IsClick
                int rowIndexIsClick = -1;
                foreach (GridRowItem gridRowItem in gridItem.GridRowList)
                {
                    if (gridRowItem.GridRow?.IsClick == true)
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

            // ResponseCount
            appInternal.AppSession.ResponseCount += 1;
            appInternal.AppJson.ResponseCount = ResponseCount;
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
    internal class GridSession
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

    internal class GridColumnSession
    {
        /// <summary>
        /// FieldName CSharp.
        /// </summary>
        public string FieldName;

        /// <summary>
        /// Session state for column header.
        /// </summary>
        public string TextConfig;

        /// <summary>
        /// Returns column header.
        /// </summary>
        public string TextGet()
        {
            return TextConfig != null ? TextConfig : FieldName;
        }

        public bool? IsSort;
    }

    internal class GridRowSession
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

    internal class GridCellSession
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

        public int MergeId;

        public string Error;
    }
}
