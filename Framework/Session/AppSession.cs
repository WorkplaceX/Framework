namespace Framework.Session
{
    using Framework.Application;
    using Framework.Dal;
    using Framework.Json;
    using Framework.Server;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Reflection;
    using System.Threading.Tasks;

    internal class AppSession
    {
        public List<GridSession> GridSessionList = new List<GridSession>();

        public void GridLoad(Grid grid, List<Row> rowList, Type typeRow)
        {
            if (grid.Id == null)
            {
                GridSessionList.Add(new GridSession());
                grid.Id = GridSessionList.Count;
            }
            int gridIndex = (int)grid.Id - 1;
            GridSession gridSession = GridSessionList[gridIndex];
            gridSession.TypeRow = typeRow;
            gridSession.RowSessionList.Clear();
            gridSession.RowInserList.Clear();
            PropertyInfo[] propertyInfoList = UtilDal.TypeRowToPropertyList(typeRow);
            foreach (var row in rowList)
            {
                RowSession rowSession = new RowSession();
                gridSession.RowSessionList.Add(rowSession);
                rowSession.Row = row;

                foreach (PropertyInfo propertyInfo in propertyInfoList)
                {
                    string text = UtilDal.CellTextFromValue(rowSession.Row, propertyInfo);
                    CellSession cellSession = new CellSession();
                    rowSession.CellSessionList.Add(cellSession);
                    cellSession.Text = text;
                }
            }
        }

        public async Task GridLoadAsync(Grid grid, IQueryable query)
        {
            var list = await query.ToDynamicListAsync();
            List<Row> rowList = list.Cast<Row>().ToList();
            GridLoad(grid, rowList, query.ElementType);
        }

        public async Task GridLoadAsync(Grid grid, Type typeRow)
        {
            var query = UtilDal.Query(typeRow);
            await GridLoadAsync(grid, query);
        }

        /// <summary>
        /// Refresh rows and cells of each data grid.
        /// </summary>
        private void GridRender()
        {
            App app = UtilServer.App;
            foreach (Grid grid in app.AppJson.ListAll().OfType<Grid>())
            {
                GridSession gridSession = GridSessionList[(int)grid.Id - 1];
                PropertyInfo[] propertyInfoList = UtilDal.TypeRowToPropertyList(gridSession.TypeRow);


                // Grid Header
                grid.Header = new GridHeader();
                grid.Header.ColumnList = new List<GridColumn>();
                foreach (PropertyInfo propertyInfo in propertyInfoList)
                {
                    grid.Header.ColumnList.Add(new GridColumn() { Text = propertyInfo.Name });
                }

                // Grid Row, Cell
                grid.RowList = new List<GridRow>();
                foreach (RowSession rowSession in gridSession.RowSessionList)
                {
                    GridRow gridRow = new GridRow();
                    grid.RowList.Add(gridRow);
                    gridRow.IsSelect = rowSession.IsSelect;
                    gridRow.CellList = new List<GridCell>();
                    for (int cellIndex = 0; cellIndex < propertyInfoList.Length; cellIndex++)
                    {
                        PropertyInfo propertyInfo = propertyInfoList[cellIndex];
                        CellSession cellSession = rowSession.CellSessionList[cellIndex];

                        GridCell gridCell = new GridCell();
                        gridRow.CellList.Add(gridCell);
                        gridCell.Text = cellSession.Text;
                        gridCell.IsModify = cellSession.IsModify;
                    }
                }
            }
        }

        private void ProcessGridSave()
        {
            App app = UtilServer.App;

            // RowUpdate
            foreach (var grid in app.AppJson.ListAll().OfType<Grid>())
            {
                if (grid.Id != null)
                {
                    int gridIndex = (int)grid.Id - 1;
                    GridSession gridSession = GridSessionList[gridIndex];
                    PropertyInfo[] propertyInfoList = UtilDal.TypeRowToPropertyList(gridSession.TypeRow);
                    if (grid.RowList != null) // Process incoming grid. Has no rows rendered if new created.
                    {
                        for (int rowIndex = 0; rowIndex < grid.RowList?.Count; rowIndex++)
                        {
                            GridRow gridRow = grid.RowList[rowIndex];
                            RowSession rowSession = gridSession.RowSessionList[rowIndex];
                            for (int cellIndex = 0; cellIndex < grid.RowList[rowIndex].CellList.Count; cellIndex++)
                            {
                                GridCell gridCell = gridRow.CellList[cellIndex];
                                CellSession cellSession = rowSession.CellSessionList[cellIndex];
                                if (gridCell.IsModify)
                                {
                                    cellSession.IsModify = true;
                                    cellSession.Text = gridCell.Text;
                                    PropertyInfo propertyInfo = propertyInfoList[cellIndex];
                                    if (rowSession.RowUpdate == null)
                                    {
                                        rowSession.RowUpdate = UtilDal.RowCopy(rowSession.Row);
                                    }
                                    UtilDal.CellTextToValue(rowSession.RowUpdate, propertyInfo, gridCell.Text); // Parse user entered text.
                                }
                            }
                        }
                    }
                }
            }

            // Row Save
            foreach (GridSession gridSession in GridSessionList)
            {
                foreach (RowSession rowSession in gridSession.RowSessionList)
                {
                    if (rowSession.RowUpdate != null)
                    {
                        try
                        {
                            UtilDal.Update(rowSession.Row, rowSession.RowUpdate);
                            rowSession.Row = rowSession.RowUpdate;
                            rowSession.RowUpdate = null;
                            foreach (CellSession cellSession in rowSession.CellSessionList)
                            {
                                cellSession.IsModify = false;
                            }
                        }
                        catch (Exception exception)
                        {
                            rowSession.Error = exception.Message;
                        }
                        rowSession.RowUpdate = null;
                    }
                }
            }
        }

        private void ProcessGridRowSelect()
        {
            App app = UtilServer.App;
            foreach (var grid in app.AppJson.ListAll().OfType<Grid>())
            {
                if (grid.Id != null)
                {
                    int gridIndex = (int)grid.Id - 1;
                    if (grid.RowList != null) // Process incoming grid. If created new it does not yet have rows rendered.
                    {
                        // Get IsClick
                        int rowIndexIsClick = -1;
                        for (int rowIndex = 0; rowIndex < grid.RowList.Count; rowIndex++)
                        {
                            GridRow gridRow = grid.RowList[rowIndex];
                            if (gridRow.IsClick)
                            {
                                rowIndexIsClick = rowIndex;
                                break;
                            }
                        }

                        // Set IsSelect
                        if (rowIndexIsClick != -1)
                        {
                            foreach (RowSession rowSession in GridSessionList[gridIndex].RowSessionList)
                            {
                                rowSession.IsSelect = false;
                            }
                            GridSessionList[gridIndex].RowSessionList[rowIndexIsClick].IsSelect = true;
                        }
                    }
                }
            }
        }

        public void Process()
        {
            ProcessGridSave();
            ProcessGridRowSelect();

            GridRender();
        }
    }

    internal class GridSession
    {
        public Type TypeRow;

        public List<RowSession> RowSessionList = new List<RowSession>();

        public List<Row> RowInserList = new List<Row>();
    }

    internal class RowSession
    {
        public Row Row;

        public Row RowUpdate;

        public bool IsSelect;

        public string Error;

        public List<CellSession> CellSessionList = new List<CellSession>();
    }

    internal class RowSessionInsert
    {
        public Row Row;

        public string Error;

        public List<CellSession> CellList = new List<CellSession>();
    }

    internal class CellSession
    {
        public string Text;

        public bool IsModify;

        public string Error;
    }
}
