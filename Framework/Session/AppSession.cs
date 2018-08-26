namespace Framework.Session
{
    using Framework.Application;
    using Framework.Dal;
    using Framework.Json;
    using Framework.Server;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
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
        /// Load a single row and create its cells.
        /// </summary>
        private void GridLoad(int gridIndex, int rowIndex, Row row, Type typeRow, GridRowEnum gridRowEnum, ref PropertyInfo[] propertyInfoListCache)
        {
            if (propertyInfoListCache == null)
            {
                propertyInfoListCache = UtilDal.TypeRowToPropertyInfoList(typeRow);
            }

            GridSession gridSession = GridSessionList[gridIndex];
            GridRowSession gridRowSession = new GridRowSession();
            gridSession.GridRowSessionList[rowIndex] = gridRowSession;
            gridRowSession.Row = row;
            gridRowSession.RowEnum = gridRowEnum;
            foreach (PropertyInfo propertyInfo in propertyInfoListCache)
            {
                GridCellSession gridCellSession = new GridCellSession();
                gridRowSession.GridCellSessionList.Add(gridCellSession);
                string text = null;
                if (gridRowSession.Row != null)
                {
                    text = UtilDal.CellTextFromValue(gridRowSession.Row, propertyInfo);
                }
                gridCellSession.Text = text;
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
            PropertyInfo[] propertyInfoList = null;
            GridLoad(gridIndex, rowIndex, null, gridSession.TypeRow, GridRowEnum.Filter, ref propertyInfoList);
        }

        /// <summary>
        /// Add empty data row. Thats where the user enters a new record.
        /// </summary>
        private void GridLoadAddRowNew(int gridIndex)
        {
            GridSession gridSession = GridSessionList[gridIndex];
            gridSession.GridRowSessionList.Add(new GridRowSession());
            int rowIndex = gridSession.GridRowSessionList.Count - 1;
            PropertyInfo[] propertyInfoList = null;
            GridLoad(gridIndex, rowIndex, null, gridSession.TypeRow, GridRowEnum.New, ref propertyInfoList);
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
        /// Copy data grid cell values to AppSession.
        /// </summary>
        private void GridLoad(Grid grid, List<Row> rowList, Type typeRow)
        {
            UtilSession.GridReset(grid);
            GridLoadSessionCreate(grid);

            PropertyInfo[] propertyInfoList = UtilDal.TypeRowToPropertyInfoList(typeRow);

            int gridIndex = UtilSession.GridToIndex(grid);
            GridSession gridSession = GridSessionList[gridIndex];

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
                gridSession.GridColumnSessionList.Clear();
                gridSession.GridRowSessionList.Clear();
                foreach (PropertyInfo propertyInfo in propertyInfoList)
                {
                    gridSession.GridColumnSessionList.Add(new GridColumnSession() { FieldName = propertyInfo.Name });
                }
                GridLoadAddFilter(gridIndex); // Add "filter row".
            }

            if (rowList != null)
            {
                foreach (Row row in rowList)
                {
                    GridRowSession gridRowSession = new GridRowSession();
                    gridSession.GridRowSessionList.Add(gridRowSession);
                    GridLoad(gridIndex, gridSession.GridRowSessionList.Count - 1, row, typeRow, GridRowEnum.Index, ref propertyInfoList);
                }
                GridLoadAddRowNew(gridIndex); // Add one "new row" to end of grid.
            }
        }

        /// <summary>
        /// Select data from database.
        /// </summary>
        private async Task GridLoadAsync(Grid grid, IQueryable query)
        {
            List<Row> rowList = null;
            if (query != null)
            {
                GridLoadSessionCreate(grid);
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
                            query = UtilDal.QueryFilter(query, fieldName, filterValue, filterOperator);
                        }
                    }
                }

                // Sort
                GridColumnSession gridColumnSessionSort = gridSession.GridColumnSessionList.Where(item => item.IsSort != null).SingleOrDefault();
                if (gridColumnSessionSort != null)
                {
                    query = UtilDal.QueryOrderBy(query, gridColumnSessionSort.FieldName, (bool)gridColumnSessionSort.IsSort);
                }

                // Skip, Take
                query = UtilDal.QuerySkipTake(query, gridSession.OffsetRow, gridSession.RowCountMax);

                rowList = await UtilDal.SelectAsync(query);
            }
            GridLoad(grid, rowList, query?.ElementType);
        }

        public async Task GridLoadAsync(Grid grid)
        {
            var query = grid.Owner<Page>().GridQuery(grid);
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
                // Grid Reset
                gridItem.Grid.ColumnList = new List<GridColumn>();
                gridItem.Grid.RowList = new List<GridRow>();
                gridItem.Grid.IsClickEnum = GridIsClickEnum.None;

                if (gridItem.Grid?.Index != null && gridItem.GridSession.GridIsEmpty() == false) // Otherwise grid is not loaded or has no header columns.
                {
                    // Grid Header
                    foreach (GridColumnItem gridColumnItem in gridItem.GridColumnItemList)
                    {
                        if (gridItem.GridSession.IsRange(gridColumnItem.CellIndex))
                        {
                            GridColumn gridColumn = new GridColumn();
                            gridColumn.Text = gridColumnItem.PropertyInfo.Name;
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

        /// <summary>
        /// Process GridIsClickEnum.
        /// </summary>
        /// <returns></returns>
        private async Task ProcessGridIsClickEnumAsync()
        {
            foreach (GridItem gridItem in UtilSession.GridItemList())
            {
                if (gridItem.GridSession.GridIsEmpty() == false)
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
                        gridItem.GridSession.OffsetRow -= gridItem.GridSession.RowCountMax;
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
                        if (rowCount == gridItem.GridSession.RowCountMax) // Page down further on full grid only.
                        {
                            gridItem.GridSession.OffsetRow += gridItem.GridSession.RowCountMax;
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
                                        gridRowItem.GridRowSession.RowUpdate = UtilDal.RowCopy(gridRowItem.GridRowSession.Row);
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
                                    gridCellItem.GridCellSession.Text = gridCellItem.GridCell.Text; // Set back to database selected value, once successfully saved.
                                    UtilDal.CellTextToValue(gridCellItem.GridCellSession.Text, gridCellItem.PropertyInfo, row); // Parse user entered text.
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
                            await UtilDal.UpdateAsync(gridRowItem.GridRowSession.Row, gridRowItem.GridRowSession.RowUpdate);
                            gridRowItem.GridRowSession.Row = gridRowItem.GridRowSession.RowUpdate;
                            foreach (GridCellSession gridCellSession in gridRowItem.GridRowSession.GridCellSessionList)
                            {
                                gridCellSession.IsModify = false;
                                gridCellSession.Error = null;
                            }
                        }
                        catch (Exception exception)
                        {
                            gridRowItem.GridRowSession.Error = exception.Message;
                        }
                        gridRowItem.GridRowSession.RowUpdate = null;
                    }
                    if (gridRowItem.GridRowSession.RowEnum == GridRowEnum.New && gridRowItem.GridRowSession.RowInsert != null)
                    {
                        // Insert to database
                        try
                        {
                            var rowNew = await UtilDal.InsertAsync(gridRowItem.GridRowSession.RowInsert);
                            gridRowItem.GridRowSession.Row = rowNew;

                            // Load new primary key into data grid.
                            PropertyInfo[] propertyInfoList = null;
                            GridLoad(gridItem.GridIndex, gridRowItem.RowIndex, rowNew, gridItem.GridSession.TypeRow, GridRowEnum.Index, ref propertyInfoList);

                            // Add new "insert row" at end of data grid.
                            GridLoadAddRowNew(gridItem.GridIndex);
                        }
                        catch (Exception exception)
                        {
                            gridRowItem.GridRowSession.Error = exception.Message;
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
                    await grid.Owner<Page>().GridRowSelectedAsync(grid);
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
                            var query = gridItem.Grid.Owner<Page>().GridLookupQuery(gridItem.Grid, gridRowItem.GridRowSession.Row, gridCellItem.FieldName, gridCellItem.GridCell.Text);
                            if (query != null)
                            {
                                await GridLoadAsync(gridItem.Grid.GridLookup(), query);
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
                                gridCellItem.GridCellSession.Text = gridCellItem.GridCell.Text;
                                if (gridRowItem.GridRowSession.RowEnum == GridRowEnum.Filter)
                                {
                                    try
                                    {
                                        gridCellItem.GridCellSession.FilterValue = UtilDal.CellTextToValue(gridCellItem.GridCellSession.Text, gridCellItem.PropertyInfo);
                                        gridCellItem.GridCellSession.FilterOperator = FilterOperator.Equal;
                                        if (gridCellItem.PropertyInfo.PropertyType == typeof(string))
                                        {
                                            gridCellItem.GridCellSession.FilterOperator = FilterOperator.Like;
                                        }
                                        gridCellItem.GridCellSession.IsModify = false;
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
                            string text = gridItemLookup.Grid.Owner<Page>().GridLookupSelected(grid, row, fieldName, gridRowItemLookup.GridRowSession.Row);

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
                    await gridItem.Grid.Owner<Page>().GridRowSelectedAsync(gridItem.Grid);
                }
            }
        }

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

    internal class GridSession
    {
        public Type TypeRow;

        /// <summary>
        /// Returns true, if grid has no header and rows.
        /// </summary>
        /// <returns></returns>
        public bool GridIsEmpty()
        {
            return TypeRow == null;
        }

        public List<GridColumnSession> GridColumnSessionList = new List<GridColumnSession>();

        public List<GridRowSession> GridRowSessionList = new List<GridRowSession>();

        public int RowCountMax = 4;

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
        public string FieldName;

        public bool? IsSort;
    }

    internal class GridRowSession
    {
        public Row Row;

        public Row RowUpdate;

        public Row RowInsert;

        public bool IsSelect;

        public string Error;

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
