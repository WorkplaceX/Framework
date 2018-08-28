namespace Framework.Session
{
    using Framework.Application;
    using Framework.Dal;
    using Framework.Json;
    using Framework.Server;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

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

            public List<GridColumnItem> GridColumnItemList = new List<GridColumnItem>();

            public List<GridRowItem> GridRowList = new List<GridRowItem>();
        }

        public class GridColumnItem
        {
            public int CellIndex;

            public GridColumnSession GridColumnSession;

            public GridColumn GridColumn;

            public PropertyInfo PropertyInfo;
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

            public PropertyInfo PropertyInfo;

            public string FieldName
            {
                get
                {
                    return PropertyInfo.Name;
                }
            }
        }

        /// <summary>
        /// Returns (GridIndex, Grid).
        /// </summary>
        private static Dictionary<int, Grid> GridList(AppSession appSession)
        {
            var result = new Dictionary<int, Grid>();
            foreach (var grid in UtilServer.AppJson.ListAll().OfType<Grid>())
            {
                if (grid.Index != null) // Grid gets Id once it's loaded.
                {
                    int gridIndex = UtilSession.GridToIndex(grid);
                    UtilFramework.Assert(gridIndex < appSession.GridSessionList.Count); // Grid needs entry in session
                    result.Add(gridIndex, grid);
                }
            }
            return result;
        }

        private static T TryGetValue<T>(this List<T> list, int index)
        {
            if (list.Count > index && index >= 0)
            {
                return list[index];
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// Returns incoming data grid (json) and outgoing grid (session) as one data structure.
        /// Incoming data grid (json) has lower priority. It gets reset once new data has been loaded into grid (session).
        /// </summary>
        public static List<GridItem> GridItemList()
        {
            AppSession appSession = UtilServer.AppSession;

            var result = new List<GridItem>();
            var gridList = GridList(appSession);
            for (int gridIndex = 0; gridIndex < appSession.GridSessionList.Count; gridIndex++)
            {
                GridSession gridSession = appSession.GridSessionList[gridIndex];
                GridItem gridItem = new GridItem();
                result.Add(gridItem);

                // Set Grid
                gridItem.GridIndex = gridIndex;
                gridItem.GridIndex = gridIndex;
                gridItem.GridSession = appSession.GridSessionList[gridIndex];

                gridList.TryGetValue(gridIndex, out Grid grid);
                gridItem.Grid = grid;

                var propertyInfoList = UtilDalType.TypeRowToPropertyInfoList(gridItem.GridSession.TypeRow);
                gridItem.GridColumnItemList = new List<GridColumnItem>();
                for (int cellIndex = 0; cellIndex < propertyInfoList.Length; cellIndex++)
                {
                    GridColumnItem gridColumnItem = new GridColumnItem();
                    gridItem.GridColumnItemList.Add(gridColumnItem);

                    // Set Column
                    gridColumnItem.CellIndex = cellIndex;
                    gridColumnItem.GridColumnSession = gridSession.GridColumnSessionList[cellIndex];
                    gridColumnItem.GridColumn = grid?.ColumnList?.TryGetValue(cellIndex - gridSession.OffsetColumn);
                    gridColumnItem.PropertyInfo = propertyInfoList[cellIndex];
                }

                for (int rowIndex = 0; rowIndex < gridSession.GridRowSessionList.Count; rowIndex++)
                {
                    GridRowItem gridRowItem = new GridRowItem();
                    gridItem.GridRowList.Add(gridRowItem);
                    GridRowSession gridRowSession = gridSession.GridRowSessionList.TryGetValue(rowIndex);
                    GridRow gridRow = grid?.RowList?.TryGetValue(rowIndex);

                    // Set Row
                    gridRowItem.RowIndex = rowIndex;
                    gridRowItem.GridRowSession = gridRowSession;
                    gridRowItem.GridRow = gridRow;

                    int offsetColumn = gridItem.GridSession.OffsetColumn;
                    int cellCount = Math.Max(gridRowSession == null ? 0 : gridRowSession.GridCellSessionList.Count, (gridRow?.CellList.Count).GetValueOrDefault() + offsetColumn);
                    for (int cellIndex = 0; cellIndex < cellCount; cellIndex++)
                    {
                        GridCellItem gridCellItem = new GridCellItem();
                        gridRowItem.GridCellList.Add(gridCellItem);
                        GridCellSession gridCellSession = gridRowSession?.GridCellSessionList.TryGetValue(cellIndex);
                        GridCell gridCell = gridRow?.CellList?.TryGetValue(cellIndex - offsetColumn);

                        // Set Cell
                        gridCellItem.CellIndex = cellIndex;
                        gridCellItem.GridCellSession = gridCellSession;
                        gridCellItem.GridCell = gridCell;
                        if (gridCellSession != null)
                        {
                            gridCellItem.PropertyInfo = propertyInfoList[cellIndex];
                        }
                    }
                }
            }
            return result;
        }

        public static int GridToIndex(Grid grid)
        {
            return (int)grid.Index;
        }

        public static Grid GridFromIndex(int gridIndex)
        {
            return UtilServer.AppJson.ListAll().OfType<Grid>().Where(item => item.Index == gridIndex).Single();
        }

        public static GridSession GridSessionFromIndex(int gridIndex)
        {
            AppSession appSession = UtilServer.AppSession;
            return appSession.GridSessionList[gridIndex];
        }

        public static GridSession GridSessionFromGrid(Grid grid)
        {
            int gridIndex = GridToIndex(grid);
            return GridSessionFromIndex(gridIndex);
        }

        public static Grid GridSessionToGrid(GridSession gridSession)
        {
            int gridIndex = GridSessionToIndex(gridSession);
            return GridFromIndex(gridIndex);
        }

        public static int GridSessionToIndex(GridSession gridSession)
        {
            AppSession appSession = UtilServer.AppSession;
            return appSession.GridSessionList.IndexOf(gridSession);
        }

        public static int GridRowToIndex(Grid grid, Row row)
        {
            int result = -1;

            AppSession appSession = UtilServer.AppSession;
            int gridIndex = GridToIndex(grid);

            for (int rowIndex = 0; rowIndex < appSession.GridSessionList[gridIndex].GridRowSessionList.Count; rowIndex++)
            {
                GridRowSession gridRowSession = appSession.GridSessionList[gridIndex].GridRowSessionList[rowIndex];
                if (gridRowSession.Row == row)
                {
                    result = gridIndex;
                    break;
                }
            }

            UtilFramework.Assert(result != -1, "Grid not found!");
            return result;
        }

        public static Row GridRowFromIndex(int gridIndex, int rowIndex)
        {
            AppSession appSession = UtilServer.AppSession;
            return appSession.GridSessionList[gridIndex].GridRowSessionList[rowIndex].Row;
        }

        public static int GridFieldNameToCellIndex(Grid grid, string fieldName)
        {
            int result = -1;
            AppSession appSession = UtilServer.AppSession;
            int gridIndex = GridToIndex(grid);
            for (int cellIndex = 0; cellIndex < appSession.GridSessionList[gridIndex].GridColumnSessionList.Count; cellIndex++)
            {
                string fieldNameItem = appSession.GridSessionList[gridIndex].GridColumnSessionList[cellIndex].FieldName;
                if (fieldNameItem == fieldName)
                {
                    result = cellIndex;
                }
            }

            UtilFramework.Assert(result != -1, "FieldName not found!");
            return result;
        }

        public static string GridFieldNameFromCellIndex(int gridIndex, int cellIndex)
        {
            AppSession appSession = UtilServer.AppSession;
            return appSession.GridSessionList[gridIndex].GridColumnSessionList[cellIndex].FieldName;
        }

        public static GridCell GridCellFromIndex(int gridIndex, int rowIndex, int cellIndex)
        {
            Grid grid = GridFromIndex(gridIndex);
            return grid.RowList[rowIndex].CellList[cellIndex];
        }

        /// <summary>
        /// Reject incoming data grid.
        /// </summary>
        public static void GridReset(Grid grid)
        {
            grid.ColumnList = null;
            grid.RowList = null;
            grid.IsClickEnum = GridIsClickEnum.None;
            grid.List.Clear();
            grid.LookupCellIndex = null;
            grid.LookupGridIndex = null;
            grid.LookupRowIndex = null;
        }
    }
}
