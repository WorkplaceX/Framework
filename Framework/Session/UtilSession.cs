namespace Framework.Session
{
    using Framework.Application;
    using Framework.Json;
    using Framework.Server;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using System;
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

            public GridSession GridSession;

            /// <summary>
            /// Can be null if grid has been removed from json.
            /// </summary>
            public Grid Grid;

            public List<GridRowItem> GridRowList = new List<GridRowItem>();
        }

        public class GridRowItem
        {
            public int RowIndex;

            /// <summary>
            /// Can be null if second loaded grid has less records.
            /// </summary>
            public GridRowSession GridRowSession;

            /// <summary>
            /// Can be null if not yet rendered.
            /// </summary>
            public GridRow GridRow;

            public List<GridCellItem> GridCellList = new List<GridCellItem>();
        }

        public class GridCellItem
        {
            public int CellIndex;

            public GridCellSession GridCellSession;

            public GridCell GridCell;
        }

        /// <summary>
        /// Returns (GridIndex, Grid).
        /// </summary>
        private static Dictionary<int, Grid> GridList(AppSession appSession)
        {
            var result = new Dictionary<int, Grid>();
            foreach (var grid in UtilServer.AppJson.ListAll().OfType<Grid>())
            {
                if (grid.Id != null) // Grid gets Id once it's loaded.
                {
                    int gridIndex = grid.Index();
                    UtilFramework.Assert(gridIndex < appSession.GridSessionList.Count); // Grid needs entry in session
                    result.Add(gridIndex, grid);
                }
            }
            return result;
        }

        private static T TryGetValue<T>(this List<T> list, int index)
        {
            if (list.Count > index)
            {
                return list[index];
            }
            else
            {
                return default(T);
            }
        }

        public static List<GridItem> GridItemList(AppSession appSession)
        {
            var result = new List<GridItem>();
            var gridList = GridList(appSession);
            for (int gridIndex = 0; gridIndex < appSession.GridSessionList.Count; gridIndex++)
            {
                GridSession gridSession = appSession.GridSessionList[gridIndex];
                GridItem gridItem = new GridItem();
                result.Add(gridItem);

                // Grid
                gridItem.GridIndex = gridIndex;
                gridItem.GridIndex = gridIndex;
                gridItem.GridSession = appSession.GridSessionList[gridIndex];

                gridList.TryGetValue(gridIndex, out Grid grid);
                gridItem.Grid = grid;
                int rowCount = Math.Max(appSession.GridSessionList.Count, (grid?.RowList?.Count).GetValueOrDefault());
                for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
                {
                    GridRowItem gridRowItem = new GridRowItem();
                    gridItem.GridRowList.Add(gridRowItem);
                    GridRowSession gridRowSession = gridSession.GridRowSessionList.TryGetValue(rowIndex);
                    GridRow gridRow = grid?.RowList?.TryGetValue(rowIndex);

                    // Row
                    gridRowItem.RowIndex = rowIndex;
                    gridRowItem.GridRowSession = gridRowSession;
                    gridRowItem.GridRow = gridRow;

                    int cellCount = Math.Max(gridRowSession == null ? 0 : gridRowSession.GridCellSessionList.Count, (gridRow?.CellList.Count).GetValueOrDefault());
                    for (int cellIndex = 0; cellIndex < cellCount; cellIndex++)
                    {
                        GridCellItem gridCellItem = new GridCellItem();
                        gridRowItem.GridCellList.Add(gridCellItem);
                        GridCellSession gridCellSession = gridRowSession?.GridCellSessionList.TryGetValue(cellIndex);
                        GridCell gridCell = gridRow?.CellList?.TryGetValue(cellIndex);

                        // Cell
                        gridCellItem.CellIndex = cellIndex;
                        gridCellItem.GridCellSession = gridCellSession;
                        gridCellItem.GridCell = gridCell;
                    }
                }
            }
            return result;
        }
    }
}
