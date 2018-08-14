namespace Framework.Session
{
    using Framework.Application;
    using Framework.Json;
    using Framework.Server;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Linq;

    internal static class UtilSession
    {
        /// <summary>
        /// Serialize session state.
        /// </summary>
        public static void Serialize(AppInternal appInternal)
        {
            string json = JsonConvert.SerializeObject(appInternal.AppSession, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
            UtilServer.Session.SetString("AppSession", json);
        }

        /// <summary>
        /// Deserialize session state.
        /// </summary>
        public static void Deserialize(AppInternal appInternal)
        {
            string json = UtilServer.Session.GetString("AppSession");
            AppSession appSession;
            if (string.IsNullOrEmpty(json))
            {
                appSession = new AppSession();
            }
            else
            {
                appSession = JsonConvert.DeserializeObject<AppSession>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
            }
            appInternal.AppSession = appSession;
        }

        public class GridItem
        {
            public int GridIndex;

            public Grid Grid;

            public GridSession GridSession;

            public List<GridRowItem> GridRowList = new List<GridRowItem>();
        }

        public class GridRowItem
        {
            public int RowIndex;

            public GridRow GridRow;

            public GridRowSession GridRowSession;

            public List<GridCellItem> GridCellList = new List<GridCellItem>();
        }

        public class GridCellItem
        {
            public int CellIndex;

            public GridCell GridCell;

            public GridCellSession GridCellSession;
        }

        public static List<GridItem> GridItemList(AppSession appSession)
        {
            List<GridItem> result = new List<GridItem>();
            AppInternal appInternal = UtilServer.AppInternal;
            foreach (var grid in appInternal.AppJson.ListAll().OfType<Grid>())
            {
                if (grid.Id != null)
                {
                    int gridIndex = grid.Index();
                    GridSession gridSession = appSession.GridSessionList[gridIndex];
                    if (grid.RowList != null) // Process incoming grid. If created new it does not yet have rows rendered.
                    {
                        GridItem gridItem = new GridItem() { GridIndex = gridIndex, Grid = grid, GridSession = gridSession };
                        result.Add(gridItem);
                        for (int rowIndex = 0; rowIndex < grid.RowList.Count; rowIndex++)
                        {
                            GridRow gridRow = grid.RowList[rowIndex];
                            GridRowSession gridRowSession = gridSession.GridRowSessionList[rowIndex];
                            GridRowItem gridRowItem = new GridRowItem() { RowIndex = rowIndex, GridRow = gridRow, GridRowSession = gridRowSession };
                            gridItem.GridRowList.Add(gridRowItem);
                            for (int cellIndex = 0; cellIndex < gridRow.CellList.Count; cellIndex++)
                            {
                                GridCell gridCell = gridRow.CellList[cellIndex];
                                GridCellSession gridCellSession = gridRowSession.GridCellSessionList[cellIndex];
                                GridCellItem gridCellItem = new GridCellItem() { CellIndex = cellIndex, GridCell = gridCell, GridCellSession = gridCellSession };
                                gridRowItem.GridCellList.Add(gridCellItem);
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}
