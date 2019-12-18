namespace Framework.Session
{
    using Framework.Application;
    using Framework.DataAccessLayer;
    using Framework.Json;
    using Framework.Server;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static Framework.DataAccessLayer.UtilDalType;

    internal static class UtilSession
    {
        /// <summary>
        /// Serialize session state.
        /// </summary>
        public static void Serialize(AppInternal appInternal)
        {
            UtilStopwatch.TimeStart("SerializeSession");
            string json = JsonConvert.SerializeObject(appInternal.AppSession, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
            UtilServer.Session.SetString("AppSession", json);
            UtilStopwatch.TimeStop("SerializeSession");
        }

        /// <summary>
        /// Deserialize session state.
        /// </summary>
        public static void Deserialize(AppInternal appInternal)
        {
            UtilStopwatch.TimeStart("Deserialize");
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
            UtilStopwatch.TimeStop("Deserialize");
        }

        public class GridItem
        {
            /// <summary>
            /// Grid session index.
            /// </summary>
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

            public Field Field;
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

            public Field Field;

            public string FieldName
            {
                get
                {
                    return Field.PropertyInfo.Name;
                }
            }
        }

        /// <summary>
        /// Returns (GridIndex, Grid).
        /// </summary>
        private static Dictionary<int, Grid> GridList(AppSession appSession)
        {
            var result = new Dictionary<int, Grid>();
            foreach (var grid in UtilServer.AppJson.ComponentListAll().OfType<Grid>())
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

                List<Field> fieldList = null;
                gridItem.GridColumnItemList = new List<GridColumnItem>();
                if (gridItem.GridSession.TypeRow != null)
                {
                    fieldList = UtilDalType.TypeRowToFieldList(gridItem.GridSession.TypeRow);
                    for (int cellIndex = 0; cellIndex < fieldList.Count; cellIndex++)
                    {
                        GridColumnItem gridColumnItem = new GridColumnItem();
                        gridItem.GridColumnItemList.Add(gridColumnItem);

                        // Set Column
                        gridColumnItem.CellIndex = cellIndex;
                        gridColumnItem.GridColumnSession = gridSession.GridColumnSessionList[cellIndex]; // Outgoing Column (Session)
                        gridColumnItem.Field = fieldList[cellIndex];
                    }
                }

                var config = new UtilColumnIndexConfig(gridItem);
                foreach (GridColumnItem gridColumnItem in gridItem.GridColumnItemList)
                {
                    int cellIndex = gridColumnItem.CellIndex;
                    if (config.IndexToIndexConfigExist(cellIndex))
                    {
                        int cellIndexConfig = config.IndexToIndexConfig(cellIndex);
                        gridColumnItem.GridColumn = grid?.ColumnList?.TryGetValue(cellIndexConfig - gridSession.OffsetColumn); // Incoming Column (Json)
                    }
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
                        GridCellSession gridCellSession = gridRowSession?.GridCellSessionList[cellIndex];
                        GridCell gridCell = null;
                        if (config.IndexToIndexConfigExist(cellIndex))
                        {
                            int cellIndexConfig = config.IndexToIndexConfig(cellIndex);
                            gridCell = gridRow?.CellList?.TryGetValue(cellIndexConfig - offsetColumn);
                        }

                        // Set Cell
                        gridCellItem.CellIndex = cellIndex;
                        gridCellItem.GridCellSession = gridCellSession; // Outgoing Cell (Session)
                        gridCellItem.GridCell = gridCell; // Incoming Cell (Json)
                        if (gridCellSession != null)
                        {
                            gridCellItem.Field = fieldList[cellIndex];
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
            return UtilServer.AppJson.ComponentListAll().OfType<Grid>().Where(item => item.Index == gridIndex).Single();
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

        public static GridRowSession GridRowSessionFromIndex(int gridIndex, int rowIndex)
        {
            AppSession appSession = UtilServer.AppSession;
            return appSession.GridSessionList[gridIndex].GridRowSessionList[rowIndex];
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

    /// <summary>
    /// Map column index to configured column index (IsVisible, Sort).
    /// </summary>
    internal class UtilColumnIndexConfig
    {
        public UtilColumnIndexConfig(UtilSession.GridItem gridItem)
        {
            List<UtilSession.GridColumnItem> result = new List<UtilSession.GridColumnItem>();
            foreach (UtilSession.GridColumnItem gridColumnItem in gridItem.GridColumnItemList)
            {
                if (gridColumnItem.GridColumnSession.IsVisible)
                {
                    result.Add(gridColumnItem);
                }
            }
            result = result
                .OrderBy(item => item.GridColumnSession.Sort)
                .ThenBy(item => item.Field.Sort).ToList(); // Make it deterministic if multiple columns have same Sort.

            for (int indexConfig = 0; indexConfig < result.Count; indexConfig++)
            {
                UtilSession.GridColumnItem item = result[indexConfig];
                int index = gridItem.GridColumnItemList.IndexOf(item);
                AddIndex(index, indexConfig);
            }
        }

        private Dictionary<int, int> indexToIndexConfigList = new Dictionary<int, int>();

        private Dictionary<int, int> indexConfigToIndexList = new Dictionary<int, int>();

        private void AddIndex(int index, int? indexConfig)
        {
            if (indexConfig != null)
            {
                this.indexToIndexConfigList.Add(index, indexConfig.Value);
                this.indexConfigToIndexList.Add(indexConfig.Value, index);
            }
        }

        public int IndexConfigToIndex(int index)
        {
            return this.indexConfigToIndexList[index];
        }

        /// <summary>
        /// Returns true, if column IsVisible.
        /// </summary>
        public bool IndexToIndexConfigExist(int index)
        {
            return this.indexToIndexConfigList.ContainsKey(index);
        }

        public int IndexToIndexConfig(int index)
        {
            return this.indexToIndexConfigList[index];
        }

        /// <summary>
        /// Gets Count. Is less than GridColumnItemList.Count, if some columns are IsVisible false.
        /// </summary>
        public int Count
        {
            get
            {
                return this.indexToIndexConfigList.Count;
            }
        }

        /// <summary>
        /// Returns list with configuration (IsVisible, Sort).
        /// </summary>
        public List<T> ConfigList<T>(List<T> list)
        {
            List<T> listLocal = new List<T>(list);
            list.Clear();
            foreach (var item in this.indexToIndexConfigList)
            {
                list.Add(listLocal[item.Key]);
            }
            return list;
        }
    }
}
