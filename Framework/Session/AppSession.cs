namespace Framework.Session
{
    using Framework.Application;
    using Framework.Dal;
    using Framework.Json;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;

    internal class AppSession
    {
        public List<GridSession> GridList = new List<GridSession>();

        public void Load(App app, Grid grid, IQueryable<App> query)
        {

        }

        public void Load(App app, Grid grid, Type typeRow)
        {

        }

        public void Process(App app)
        {
            foreach (var grid in app.AppJson.ListAll().OfType<Grid>())
            {
                int gridIndex = grid.Id - 1;
                for (int rowIndex = 0; rowIndex < grid.RowList.Count; rowIndex++)
                {
                    for (int cellIndex = 0; cellIndex < grid.RowList[rowIndex].CellList.Count; cellIndex++)
                    {
                        if (grid.RowList[rowIndex].CellList[cellIndex].IsModify)
                        {

                        }
                    }
                }
            }
        }
    }

    internal class GridSession
    {
        public List<RowSession> RowList = new List<RowSession>();

        public List<Row> RowInserList = new List<Row>();
    }

    internal class RowSession
    {
        public Row Row;

        public Row RowUpdate;

        public string Error;

        public List<CellSession> CellList = new List<CellSession>();
    }

    internal class CellSession
    {
        public string Error;
    }
}
