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

        private void GridLoadAddFilter(int gridIndex)
        {
            GridSession gridSession = GridSessionList[gridIndex];
            gridSession.GridRowSessionList.Add(new GridRowSession());
            int rowIndex = gridSession.GridRowSessionList.Count - 1;
            PropertyInfo[] propertyInfoList = null;
            GridLoad(gridIndex, rowIndex, null, gridSession.TypeRow, GridRowEnum.Filter, ref propertyInfoList);
        }

        private void GridLoadAddRowNew(int gridIndex)
        {
            GridSession gridSession = GridSessionList[gridIndex];
            gridSession.GridRowSessionList.Add(new GridRowSession());
            int rowIndex = gridSession.GridRowSessionList.Count - 1;
            PropertyInfo[] propertyInfoList = null;
            GridLoad(gridIndex, rowIndex, null, gridSession.TypeRow, GridRowEnum.New, ref propertyInfoList);
        }

        private void GridLoad(Grid grid, List<Row> rowList, Type typeRow)
        {
            if (grid.Index  == null)
            {
                GridSessionList.Add(new GridSession());
                grid.Index = GridSessionList.Count - 1;
            }

            PropertyInfo[] propertyInfoList = UtilDal.TypeRowToPropertyInfoList(typeRow);

            int gridIndex = UtilSession.GridToIndex(grid);
            GridSession gridSession = GridSessionList[gridIndex];
            gridSession.GridRowSessionList.Clear();
            gridSession.FieldNameList.Clear();
            gridSession.TypeRow = typeRow;
            foreach (PropertyInfo propertyInfo in propertyInfoList)
            {
                gridSession.FieldNameList.Add(propertyInfo.Name);
            }

            if (rowList != null)
            {
                GridLoadAddFilter(gridIndex); // Add "filter row".
                foreach (Row row in rowList)
                {
                    GridRowSession gridRowSession = new GridRowSession();
                    gridSession.GridRowSessionList.Add(gridRowSession);
                    GridLoad(gridIndex, gridSession.GridRowSessionList.Count - 1, row, typeRow, GridRowEnum.Index, ref propertyInfoList);
                }
                GridLoadAddRowNew(gridIndex); // Add one "new row" to end of grid.
            }
        }

        private async Task GridLoadAsync(Grid grid, IQueryable query)
        {
            List<Row> rowList = null;
            if (query != null)
            {
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
                gridItem.Grid.Header = new GridHeader();
                gridItem.Grid.Header.ColumnList = new List<GridColumn>();
                gridItem.Grid.RowList = new List<GridRow>();

                if (gridItem.Grid?.Index != null && gridItem.GridSession.GridIsEmpty() == false) // Otherwise grid is not loaded or has no header columns.
                {
                    // Grid Header
                    foreach (GridColumnItem gridColumnItem in gridItem.GridColumnItemList)
                    {
                        gridItem.Grid.Header.ColumnList.Add(new GridColumn() { Text = gridColumnItem.PropertyInfo.Name });
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

        private async Task ProcessGridSaveAsync()
        {
            // RowUpdate
            foreach (var grid in UtilServer.AppJson.ListAll().OfType<Grid>())
            {
                if (grid.Index != null)
                {
                    int gridIndex = UtilSession.GridToIndex(grid);
                    GridSession gridSession = GridSessionList[gridIndex];
                    if (gridSession.GridIsEmpty() == false)
                    {
                        PropertyInfo[] propertyInfoList = UtilDal.TypeRowToPropertyInfoList(gridSession.TypeRow);
                        if (grid.RowList != null) // Process incoming grid. Has no rows rendered if new created.
                        {
                            for (int rowIndex = 0; rowIndex < grid.RowList?.Count; rowIndex++)
                            {
                                GridRow gridRow = grid.RowList[rowIndex];
                                GridRowSession gridRowSession = gridSession.GridRowSessionList[rowIndex];
                                for (int cellIndex = 0; cellIndex < grid.RowList[rowIndex].CellList.Count; cellIndex++)
                                {
                                    GridCell gridCell = gridRow.CellList[cellIndex];
                                    GridCellSession gridCellSession = gridRowSession.GridCellSessionList[cellIndex];
                                    if (gridCell.IsModify)
                                    {
                                        gridCellSession.IsModify = true;
                                        gridCellSession.Text = gridCell.Text;
                                        PropertyInfo propertyInfo = propertyInfoList[cellIndex];
                                        Row row;
                                        if (gridRowSession.Row != null)
                                        {
                                            // Update
                                            if (gridRowSession.RowUpdate == null)
                                            {
                                                gridRowSession.RowUpdate = UtilDal.RowCopy(gridRowSession.Row);
                                            }
                                            row = gridRowSession.RowUpdate;
                                        }
                                        else
                                        {
                                            // Insert
                                            if (gridRowSession.RowInsert == null)
                                            {
                                                gridRowSession.RowInsert = (Row)Activator.CreateInstance(gridSession.TypeRow);
                                            }
                                            row = gridRowSession.RowInsert;
                                        }
                                        UtilDal.CellTextToValue(row, propertyInfo, gridCell.Text); // Parse user entered text.
                                    }
                                    gridCellSession.MergeId = gridCell.MergeId;
                                }
                            }
                        }
                    }
                }
            }

            // Row Save
            for (int gridIndex = 0; gridIndex < GridSessionList.Count; gridIndex++)
            {
                var gridSession = GridSessionList[gridIndex];
                for (int rowIndex = 0; rowIndex < gridSession.GridRowSessionList.Count; rowIndex++)
                {
                    GridRowSession gridRowSession = gridSession.GridRowSessionList[rowIndex];

                    // Update
                    if (gridRowSession.RowEnum == GridRowEnum.Index && gridRowSession.RowUpdate != null)
                    {
                        try
                        {
                            await UtilDal.UpdateAsync(gridRowSession.Row, gridRowSession.RowUpdate);
                            gridRowSession.Row = gridRowSession.RowUpdate;
                            foreach (GridCellSession gridCellSession in gridRowSession.GridCellSessionList)
                            {
                                gridCellSession.IsModify = false;
                                gridCellSession.Error = null;
                            }
                        }
                        catch (Exception exception)
                        {
                            gridRowSession.Error = exception.Message;
                        }
                        gridRowSession.RowUpdate = null;
                    }
                    // Insert
                    if (gridRowSession.RowEnum == GridRowEnum.New && gridRowSession.RowInsert != null)
                    {
                        try
                        {
                            var rowNew = await UtilDal.InsertAsync(gridRowSession.RowInsert);
                            gridRowSession.Row = rowNew;

                            // Load new primary key into data grid.
                            PropertyInfo[] propertyInfoList = null;
                            GridLoad(gridIndex, rowIndex, rowNew, gridSession.TypeRow, GridRowEnum.Index, ref propertyInfoList);

                            // Add new "insert row" at end of data grid.
                            GridLoadAddRowNew(gridIndex);
                        }
                        catch (Exception exception)
                        {
                            gridRowSession.Error = exception.Message;
                        }
                        gridRowSession.RowInsert = null;
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
                gridRowSession.IsSelect = true;
                await grid.Owner<Page>().GridRowSelectedAsync(grid);
                break;
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
                            Row row = UtilSession.GridRowFromIndex(gridIndex, (int)gridItemLookup.Grid.LookupRowIndex);
                            string fieldName = UtilSession.GridFieldNameFromCellIndex(gridIndex, (int)gridItemLookup.Grid.LookupCellIndex);
                            string text = gridItemLookup.Grid.Owner<Page>().GridLookupSelected(grid, row, fieldName, gridRowItemLookup.GridRowSession.Row);

                            GridCell gridCell = UtilSession.GridCellFromIndex(gridIndex, (int)gridItemLookup.Grid.LookupRowIndex, (int)gridItemLookup.Grid.LookupCellIndex);
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
                        rowIndexIsClick = gridRowItem.RowIndex;
                        break;
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

            ProcessGridLookupRowIsClick();
            await ProcessGridSaveAsync();
            await ProcessGridRowIsClick(); // Load for example detail grids.
            await ProcessGridLookupOpenAsync(); // Load lookup data grid.

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

        public List<string> FieldNameList = new List<string>();

        public List<GridRowSession> GridRowSessionList = new List<GridRowSession>();
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
        Filter = 1,
        Index = 2,
        New = 3,
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

        public int MergeId;

        public string Error;
    }
}
