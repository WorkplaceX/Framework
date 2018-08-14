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
        /// Load a single row.
        /// </summary>
        private void GridLoad(int gridIndex, int rowIndex, Row row, Type typeRow, ref PropertyInfo[] propertyInfoList)
        {
            if (propertyInfoList == null)
            {
                propertyInfoList = UtilDal.TypeRowToPropertyList(typeRow);
            }

            GridSession gridSession = GridSessionList[gridIndex];
            GridRowSession gridRowSession = new GridRowSession();
            gridSession.GridRowSessionList[rowIndex] = gridRowSession;
            gridRowSession.Row = row;
            foreach (PropertyInfo propertyInfo in propertyInfoList)
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

        private void GridLoadAddRowNew(int gridIndex)
        {
            GridSession gridSession = GridSessionList[gridIndex];
            gridSession.GridRowSessionList.Add(new GridRowSession());
            int rowIndex = gridSession.GridRowSessionList.Count - 1;
            PropertyInfo[] propertyInfoList = null;
            GridLoad(gridIndex, rowIndex, null, gridSession.TypeRow, ref propertyInfoList);
        }

        private void GridLoad(Grid grid, List<Row> rowList, Type typeRow)
        {
            if (grid.Id == null)
            {
                GridSessionList.Add(new GridSession());
                grid.Id = GridSessionList.Count;
            }
            int gridIndex = grid.Index();
            GridSession gridSession = GridSessionList[gridIndex];
            gridSession.TypeRow = typeRow;
            gridSession.GridRowSessionList.Clear();

            if (rowList != null)
            {
                PropertyInfo[] propertyInfoList = UtilDal.TypeRowToPropertyList(typeRow);
                for (int rowIndex = 0; rowIndex < rowList.Count; rowIndex++)
                {
                    GridRowSession gridRowSession = new GridRowSession();
                    gridSession.GridRowSessionList.Add(gridRowSession);
                    Row row = rowList[rowIndex];
                    GridLoad(gridIndex, rowIndex, row, typeRow, ref propertyInfoList);
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
            var query = grid.Owner<Page>().GridLoadQuery(grid);
            await GridLoadAsync(grid, query);
            await GridRowSelectFirst(grid);
        }

        /// <summary>
        /// Refresh rows and cells of each data grid.
        /// </summary>
        public void GridRender()
        {
            foreach (Grid grid in UtilServer.AppJson.ListAll().OfType<Grid>())
            {
                // Grid reset
                grid.Header = new GridHeader();
                grid.Header.ColumnList = new List<GridColumn>();
                grid.RowList = new List<GridRow>();

                if (grid.Id != null)
                {
                    GridSession gridSession = GridSessionList[grid.Index()];

                    if (gridSession.GridIsEmpty() == false) // Otherwise grid has no header columns
                    {
                        PropertyInfo[] propertyInfoList = UtilDal.TypeRowToPropertyList(gridSession.TypeRow);

                        // Grid Header
                        foreach (PropertyInfo propertyInfo in propertyInfoList)
                        {
                            grid.Header.ColumnList.Add(new GridColumn() { Text = propertyInfo.Name });
                        }

                        // Grid Row, Cell
                        foreach (GridRowSession gridRowSession in gridSession.GridRowSessionList)
                        {
                            GridRow gridRow = new GridRow();
                            grid.RowList.Add(gridRow);
                            gridRow.IsSelect = gridRowSession.IsSelect;
                            gridRow.CellList = new List<GridCell>();
                            for (int cellIndex = 0; cellIndex < propertyInfoList.Length; cellIndex++)
                            {
                                PropertyInfo propertyInfo = propertyInfoList[cellIndex];
                                GridCellSession gridCellSession = gridRowSession.GridCellSessionList[cellIndex];

                                GridCell gridCell = new GridCell();
                                gridRow.CellList.Add(gridCell);
                                gridCell.Text = gridCellSession.Text;
                                gridCell.IsModify = gridCellSession.IsModify;
                                gridCell.MergeId = gridCellSession.MergeId;
                                gridCell.IsLookup = gridCellSession.IsLookup;
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
                if (grid.Id != null)
                {
                    int gridIndex = grid.Index();
                    GridSession gridSession = GridSessionList[gridIndex];
                    if (gridSession.GridIsEmpty() == false)
                    {
                        PropertyInfo[] propertyInfoList = UtilDal.TypeRowToPropertyList(gridSession.TypeRow);
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
                    if (gridRowSession.RowUpdate != null)
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
                    if (gridRowSession.RowInsert != null)
                    {
                        try
                        {
                            var rowNew = await UtilDal.InsertAsync(gridRowSession.RowInsert);
                            gridRowSession.Row = rowNew;

                            // Load new primary key into data grid.
                            PropertyInfo[] propertyInfoList = null;
                            GridLoad(gridIndex, rowIndex, rowNew, gridSession.TypeRow, ref propertyInfoList);

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

        private async Task GridRowSelectFirst(Grid grid)
        {
            AppInternal appInternal = UtilServer.AppInternal;
            int gridIndex = grid.Index();
            foreach (GridRowSession gridRowSession in GridSessionList[gridIndex].GridRowSessionList)
            {
                gridRowSession.IsSelect = true;
                await grid.Owner<Page>().GridRowSelectChangeAsync(grid);
                break;
            }
        }

        private void ProcessGridIsLookup()
        {
            foreach (GridItem gridItem in UtilSession.GridItemList(this))
            {
                foreach (GridRowItem gridRowItem in gridItem.GridRowList)
                {
                    foreach (GridCellItem gridCellItem in gridRowItem.GridCellList)
                    {
                        if (gridCellItem.GridCell?.IsModify == true)
                        {
                            gridCellItem.GridCellSession.IsLookup = true;
                        }
                    }
                }
            }
        }

        private async Task ProcessGridRowSelect()
        {
            foreach (GridItem gridItem in UtilSession.GridItemList(this))
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
                        gridRowItem.GridRowSession.IsSelect = false;
                    }
                    gridItem.GridRowList[rowIndexIsClick].GridRowSession.IsSelect = true;
                    await gridItem.Grid.Owner<Page>().GridRowSelectChangeAsync(gridItem.Grid);
                }
            }
        }

        public async Task ProcessAsync()
        {
            AppInternal appInternal = UtilServer.AppInternal;

            await ProcessGridSaveAsync();
            await ProcessGridRowSelect();
            ProcessGridIsLookup();

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
    }

    internal class GridCellSession
    {
        public string Text;

        public bool IsModify;

        public bool IsLookup;

        public int MergeId;

        public string Error;
    }
}
