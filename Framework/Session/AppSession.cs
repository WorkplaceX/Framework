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
            foreach (var row in rowList)
            {
                gridSession.RowSessionList.Add(new RowSession() { Row = row });
            }

            // Grid Header
            grid.Header = new GridHeader();
            grid.Header.ColumnList = new List<GridColumn>();
            foreach (PropertyInfo propertyInfo in UtilDal.TypeRowToPropertyList(typeRow))
            {
                grid.Header.ColumnList.Add(new GridColumn() { Text = propertyInfo.Name });
            }

            // Grid Row
            grid.RowList = new List<GridRow>();
            foreach (Row row in rowList)
            {
                GridRow gridRow = new GridRow();
                grid.RowList.Add(gridRow);
                gridRow.CellList = new List<GridCell>();
                foreach (PropertyInfo propertyInfo in UtilDal.TypeRowToPropertyList(typeRow))
                {
                    gridRow.CellList.Add(new GridCell() { Text = propertyInfo.GetValue(row)?.ToString() });
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
            await GridLoadAsync(grid, typeRow);
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
                    for (int rowIndex = 0; rowIndex < grid.RowList.Count; rowIndex++)
                    {
                        GridRow gridRow = grid.RowList[rowIndex];
                        RowSession rowSession = gridSession.RowSessionList[rowIndex];
                        for (int cellIndex = 0; cellIndex < grid.RowList[rowIndex].CellList.Count; cellIndex++)
                        {
                            GridCell gridCell = gridRow.CellList[cellIndex];
                            // CellSession cellSession = rowSession.CellList[cellIndex];
                            if (gridCell.IsModify)
                            {
                                PropertyInfo[] propertyInfoList = UtilDal.TypeRowToPropertyList(gridSession.TypeRow);
                                PropertyInfo propertyInfo = propertyInfoList[cellIndex];
                                if (rowSession.RowUpdate == null)
                                {
                                    rowSession.RowUpdate = UtilDal.RowCopy(rowSession.Row);

                                    object value = Convert.ChangeType(gridCell.Text, propertyInfo.PropertyType); // Parse
                                    propertyInfo.SetValue(rowSession.RowUpdate, value);
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
                        }
                        catch (Exception exception)
                        {
                            rowSession.Error = exception.Message;
                        }
                        rowSession.RowUpdate = null;
                    }
                }
            }

            // IsModify
            app.AppJson.ListAll().OfType<GridCell>().ToList().ForEach(gridCell => gridCell.IsModify = false);
        }

        private void ProcessGridRowSelect()
        {
            App app = UtilServer.App;
            foreach (var grid in app.AppJson.ListAll().OfType<Grid>())
            {
                foreach (var row in grid.RowList)
                {
                    if (row.IsClick)
                    {
                        grid.RowList.ForEach(item => item.IsSelect = false);
                        row.IsClick = false;
                        row.IsSelect = true;
                    }
                }
            }
        }

        public void Process()
        {
            ProcessGridSave();
            ProcessGridRowSelect();
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

        public string Error;

        public List<CellSession> CellList = new List<CellSession>();
    }

    internal class RowSessionInsert
    {
        public Row Row;

        public string Error;

        public List<CellSession> CellList = new List<CellSession>();
    }

    internal class CellSession
    {
        public string Error;
    }
}
