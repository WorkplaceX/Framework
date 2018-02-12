namespace Framework.Application
{
    using Framework.DataAccessLayer;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Reflection;
    using Framework.Component;

    internal class GridRowInternal
    {
        public Row Row;

        /// <summary>
        /// Gets or sets RowNew. This is the row to update or insert on database.
        /// </summary>
        public Row RowNew;

        /// <summary>
        /// Filter with parsed and valid parameters.
        /// </summary>
        public Row RowFilter; // List<Row> for multiple parameters.

        /// <summary>
        /// Gets or sets error attached to row.
        /// </summary>
        public string Error;

        /// <summary>
        /// Bitwise (01=Select; 10=MouseOver; 11=Select and MouseOver).
        /// </summary>
        internal int IsSelect;

        internal bool IsSelectGet()
        {
            return UtilApplication.IsSelectGet(IsSelect);
        }

        internal void IsSelectSet(bool value)
        {
            IsSelect = UtilApplication.IsSelectSet(IsSelect, value);
        }

        internal bool IsClick;
    }

    internal class GridCellInternal
    {
        /// <summary>
        /// Gets or sets user modified text.
        /// </summary>
        public string Text;

        /// <summary>
        /// Gets or sets error attached to cell.
        /// </summary>
        public string Error;

        /// <summary>
        /// Gets or sets IsModify. Text has been modified on last request.
        /// </summary>
        public bool IsModify;

        /// <summary>
        /// Gets or sets IsDeleteKey. Sent by client indicating user pressed delete or backspace button.
        /// </summary>
        public bool IsDeleteKey;

        /// <summary>
        /// Gets or sets IsParseSuccess. For internal use only. Never sent to client. Indicates user modified text could be parsed successfully.
        /// </summary>
        public bool IsParseSuccess;

        public bool IsLookup;

        public string GridNameLookup;

        /// <summary>
        /// Gets or sets FocusId. GridCell can be displayed by multiple GridField. Focus has the one with FocusId. Used to show or hide Lookup.
        /// </summary>
        public int? FocusId;

        /// <summary>
        /// Gets FocusIdRequest. Sent by client if it got focus.
        /// </summary>
        public int? FocusIdRequest;

        /// <summary>
        /// Gets or sets IsOriginal. If true, Text has been modified by user. Original text is stored in TextOriginal.
        /// </summary>
        public bool IsOriginal;

        /// <summary>
        /// Gets or sets TextOriginal. See also field IsOriginal.
        /// </summary>
        public string TextOriginal;

        public bool IsClick;

        public string PlaceHolder;
    }

    internal class GridColumnInternal
    {
        /// <summary>
        /// Gets or sets ColumnName. For internal use only. Never sent to client.
        /// </summary>
        public string ColumnName;

        public bool IsClick;
    }

    internal class GridQueryInternal
    {
        public string ColumnNameOrderBy;

        public bool IsOrderByDesc;

        public int PageIndex;

        public int PageHorizontalIndex;

        /// <summary>
        /// Gets or sets IsInsert. This data is internal and never sent to client. Indicating method GridData.RowNewAdd(); has to be called once Config is loaded.
        /// </summary>
        public bool IsInsert;
    }

    public class GridData
    {
        internal GridData(App app)
        {
            this.App = app;
            this.Config = new ConfigInternal(App);
        }

        internal readonly App App;

        internal readonly ConfigInternal Config;

        /// <summary>
        /// Returns list of loaded GridName.
        /// </summary>
        internal List<GridName> GridNameList()
        {
            List<GridName> result = new List<GridName>(rowList.Keys);
            return result;
        }

        /// <summary>
        /// Returns column definitions.
        /// </summary>
        internal List<Cell> ColumnList(GridName gridName)
        {
            Type typeRow = TypeRowGet(gridName);
            return UtilDataAccessLayer.ColumnList(typeRow);
        }

        /// <summary>
        /// Returns list of loaded row index.
        /// </summary>
        internal List<Index> IndexList(GridName gridName)
        {
            return new List<Index>(rowList[gridName].Keys);
        }

        /// <summary>
        /// Returns TypeRow of loaded data. (Works also if load selected no rows).
        /// </summary>
        internal Type TypeRow(GridName gridName)
        {
            return TypeRowGet(gridName);
        }

        /// <summary>
        /// Returns GridNameTypeRow. This is GridName with TypeRow definition.
        /// </summary>
        internal GridNameTypeRow GridNameTypeRow(GridName gridName)
        {
            GridNameTypeRow result = null;
            Type typeRow = TypeRow(gridName);
            if (typeRow != null)
            {
                result = new GridNameTypeRow(typeRow, gridName);
            }
            return result;
        }

        /// <summary>
        /// (GridName, TypeRow). See also: GridDataJson.GridQueryList.
        /// </summary>
        private Dictionary<GridName, Type> typeRowList = new Dictionary<GridName, Type>();

        private Type TypeRowGet(GridName gridName)
        {
            Type result = null;
            if (gridName != null)
            {
                typeRowList.TryGetValue(gridName, out result);
            }
            return result;
        }

        private void TypeRowSet(GridNameTypeRow gridName)
        {
            typeRowList[gridName] = gridName.TypeRow;
            if (gridName.TypeRow == null)
            {
                typeRowList.Remove(gridName);
            }
        }

        internal List<GridNameTypeRow> GridNameTypeRowList()
        {
            List<GridNameTypeRow> result = new List<GridNameTypeRow>();
            foreach (var item in typeRowList)
            {
                GridNameTypeRow gridNameTypeRow = new GridNameTypeRow(item.Value, item.Key);
                result.Add(gridNameTypeRow);
            }
            return result;
        }

        /// <summary>
        /// Returns data row.
        /// </summary>
        public Row RowGet(GridName gridName, Index index)
        {
            var row = RowInternalGet(gridName, index);
            switch (index.Enum)
            {
                case IndexEnum.Index:
                    if (row.RowNew != null)
                    {
                        return row.RowNew;
                    }
                    return row.Row;
                case IndexEnum.Filter:
                    return row.RowFilter;
                case IndexEnum.New:
                    return row.RowNew;
                default:
                    throw new Exception("Enum unknown!");
            }
        }

        /// <summary>
        /// Select a row of grid and return newly selected row.
        /// </summary>
        internal Row RowSelect(GridName gridName, Index index)
        {
            Row result = null;
            if (rowList.ContainsKey(gridName))
            {
                foreach (Index key in rowList[gridName].Keys)
                {
                    if (key == index)
                    {
                        rowList[gridName][key].IsSelectSet(true);
                        UtilFramework.Assert(result == null);
                        result = rowList[gridName][key].Row;
                    }
                    else
                    {
                        rowList[gridName][key].IsSelectSet(false);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns selected data row.
        /// </summary>
        public Row RowSelected(GridName gridName)
        {
            Row result = null;
            if (rowList.ContainsKey(gridName))
            {
                foreach (var item in rowList[gridName].Values)
                {
                    if (item.IsSelectGet())
                    {
                        result = item.Row;
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns selected data row.
        /// </summary>
        public TRow RowSelected<TRow>(GridName<TRow> gridName) where TRow : Row
        {
            Row result = null;
            if (rowList.ContainsKey(gridName))
            {
                foreach (var item in rowList[gridName].Values)
                {
                    if (item.IsSelectGet())
                    {
                        result = item.Row;
                        break;
                    }
                }
            }
            return (TRow)result;
        }

        /// <summary>
        /// Returns index of selected data row.
        /// </summary>
        internal Index RowSelectedIndex(GridName gridName)
        {
            Index result = null;
            if (rowList.ContainsKey(gridName))
            {
                foreach (Index index in rowList[gridName].Keys)
                {
                    if (rowList[gridName][index].IsSelectGet())
                    {
                        result = index;
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// (GridName, GridQuery).
        /// </summary>
        private Dictionary<GridName, GridQueryInternal> queryList = new Dictionary<GridName, GridQueryInternal>();

        internal GridQueryInternal QueryGet(GridName gridName)
        {
            if (!queryList.ContainsKey(gridName))
            {
                queryList[gridName] = new GridQueryInternal();
            }
            return queryList[gridName];
        }

        /// <summary>
        /// (GridName, ColumnName, GridColumn).
        /// </summary>
        private Dictionary<GridName, Dictionary<string, GridColumnInternal>> columnList = new Dictionary<GridName, Dictionary<string, GridColumnInternal>>();

        private GridColumnInternal ColumnGet(GridName gridName, string columnName)
        {
            if (!columnList.ContainsKey(gridName))
            {
                columnList[gridName] = new Dictionary<string, GridColumnInternal>();
            }
            if (!columnList[gridName].ContainsKey(columnName))
            {
                columnList[gridName][columnName] = new GridColumnInternal();
            }
            //
            return columnList[gridName][columnName];
        }

        internal List<GridColumnInternal> ColumnInternalList(GridName gridName)
        {
            if (!columnList.ContainsKey(gridName))
            {
                columnList[gridName] = new Dictionary<string, GridColumnInternal>();
            }
            return new List<GridColumnInternal>(columnList[gridName].Values);
        }

        /// <summary>
        /// (GridName, Index). Original row as loaded from json.
        /// </summary>
        private Dictionary<GridName, Dictionary<Index, GridRowInternal>> rowList = new Dictionary<GridName, Dictionary<Index, GridRowInternal>>();

        internal Dictionary<Index, GridRowInternal> RowList(GridName gridName)
        {
            return rowList[gridName];
        }

        internal string SelectGridName;

        internal Index SelectIndex;

        internal string SelectColumnName;

        private GridRowInternal RowInternalGet(GridName gridName, Index index)
        {
            GridRowInternal result = null;
            if (rowList.ContainsKey(gridName))
            {
                if (rowList[gridName].ContainsKey(index))
                {
                    result = rowList[gridName][index];
                }
            }
            return result;
        }

        private void RowSet(GridName gridName, Index index, GridRowInternal gridRow)
        {
            if (!rowList.ContainsKey(gridName))
            {
                rowList[gridName] = new Dictionary<Index, GridRowInternal>();
            }
            rowList[gridName][index] = gridRow;
        }

        /// <summary>
        /// (GridName, Index, ColumnName, GridCellInternal).
        /// </summary>
        private Dictionary<GridName, Dictionary<Index, Dictionary<string, GridCellInternal>>> cellList = new Dictionary<GridName, Dictionary<Index, Dictionary<string, GridCellInternal>>>();

        internal void CellAll(Action<GridCellInternal, AppEventArg> callback)
        {
            foreach (GridName gridName in cellList.Keys)
            {
                foreach (Index index in cellList[gridName].Keys)
                {
                    foreach (string columnName in cellList[gridName][index].Keys)
                    {
                        callback(cellList[gridName][index][columnName], new AppEventArg(App, gridName, index, columnName));
                    }
                }
            }
        }

        /// <summary>
        /// Set IsLookup flag on cell.
        /// </summary>
        internal void LookupOpen(GridName gridName, Index index, string columnName, GridName gridNameLookup)
        {
            CellAll((GridCellInternal gridCell, AppEventArg e) =>
            {
                gridCell.IsLookup = false;
                gridCell.GridNameLookup = null;
            });
            CellAll((GridCellInternal gridCell, AppEventArg e) =>
            {
                bool isLookup = gridName == e.GridName && index == e.Index && columnName == e.ColumnName;
                if (isLookup)
                {
                    gridCell.IsLookup = true;
                    gridCell.GridNameLookup = gridNameLookup.Name;
                }
            });
        }

        /// <summary>
        /// Set IsLookup flag to false.
        /// </summary>
        internal void LookupClose()
        {
            CellAll((GridCellInternal gridCell, AppEventArg e) =>
            {
                gridCell.IsLookup = false;
                gridCell.GridNameLookup = null;
            });
        }

        internal GridCellInternal CellGet(GridName gridName, Index index, string columnName)
        {
            GridCellInternal result = null;
            if (cellList.ContainsKey(gridName))
            {
                if (cellList[gridName].ContainsKey(index))
                {
                    if (cellList[gridName][index].ContainsKey(columnName))
                    {
                        result = cellList[gridName][index][columnName];
                    }
                }
            }
            if (result == null)
            {
                result = new GridCellInternal();
                if (!cellList.ContainsKey(gridName))
                {
                    cellList[gridName] = new Dictionary<Index, Dictionary<string, GridCellInternal>>();
                }
                if (!cellList[gridName].ContainsKey(index))
                {
                    cellList[gridName][index] = new Dictionary<string, GridCellInternal>();
                }
                cellList[gridName][index][columnName] = result;
            }
            return result;
        }

        /// <summary>
        /// Returns error attached to data row.
        /// </summary>
        private string ErrorRowGet(GridName gridName, Index index)
        {
            return RowInternalGet(gridName, index).Error;
        }

        /// <summary>
        /// Set error on data row.
        /// </summary>
        internal void ErrorRowSet(GridName gridName, Index index, string text)
        {
            RowInternalGet(gridName, index).Error = text;
        }

        /// <summary>
        /// Gets user entered text.
        /// </summary>
        /// <returns>If null, user has not changed text.</returns>
        internal string CellTextGet(GridName gridName, Index index, string columnName)
        {
            return CellGet(gridName, index, columnName).Text;
        }

        /// <summary>
        /// Sets user entered text.
        /// </summary>
        /// <param name="text">If null, user has not changed text.</param>
        private void CellTextSet(GridName gridName, Index index, string columnName, string text, bool isOriginal, string textOriginal)
        {
            GridCellInternal cell = CellGet(gridName, index, columnName);
            cell.Text = text;
            cell.IsOriginal = isOriginal;
            cell.TextOriginal = textOriginal;
        }

        /// <summary>
        /// Set IsModify flag.
        /// </summary>
        internal void CellIsModifySet(GridName gridName, Index index, string columnName)
        {
            GridRowInternal gridRow = RowInternalGet(gridName, index);
            if (index.Enum == IndexEnum.Index && gridRow.RowNew == null)
            {
                gridRow.RowNew = UtilDataAccessLayer.RowClone(gridRow.Row);
            }
            GridCellInternal gridCell = CellGet(gridName, index, columnName);
            gridCell.IsModify = true;
        }

        /// <summary>
        /// Clear all user modified text for row.
        /// </summary>
        private void CellTextClear(GridName gridName, Index index)
        {
            if (cellList.ContainsKey(gridName))
            {
                if (cellList[gridName].ContainsKey(index))
                {
                    foreach (GridCellInternal cell in cellList[gridName][index].Values)
                    {
                        cell.Text = null;
                        cell.IsOriginal = false;
                    }
                }
            }
        }

        private string ErrorCellGet(GridName gridName, Index index, string columnName)
        {
            return CellGet(gridName, index, columnName).Error;
        }

        private void ErrorCellSet(GridName gridName, Index index, string columnName, string text)
        {
            CellGet(gridName, index, columnName).Error = text;
        }

        /// <summary>
        /// Returns true, if data row contains text parse error.
        /// </summary>
        private bool IsErrorRowCell(GridName gridName, Index index)
        {
            bool result = false;
            if (cellList.ContainsKey(gridName))
            {
                if (cellList[gridName].ContainsKey(index))
                {
                    foreach (string columnName in cellList[gridName][index].Keys)
                    {
                        if (cellList[gridName][index][columnName].Error != null)
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns true, if text on row has been modified.
        /// </summary>
        /// <param name="isCalculatedColumnExclude">If true, columns without ColumnNameSql are ignored for result. Used for example if query provider is database.</param>
        internal bool IsModifyRowCell(GridName gridName, Index index, bool isCalculatedColumnExclude)
        {
            bool result = false;
            Type typeRow = TypeRowGet(gridName);
            if (cellList.ContainsKey(gridName))
            {
                if (cellList[gridName].ContainsKey(index))
                {
                    foreach (Cell column in UtilDataAccessLayer.ColumnList(typeRow))
                    {
                        if (isCalculatedColumnExclude == false || (column.ColumnNameSql != null)) // Exclude calculated column.
                        {
                            string columnName = column.ColumnNameCSharp;
                            if (cellList[gridName][index].ContainsKey(columnName))
                            {
                                if (cellList[gridName][index][columnName].IsModify)
                                {
                                    result = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Load data from Sql database.
        /// </summary>
        internal void LoadDatabase(GridNameTypeRow gridName, List<Filter> filterList, string columnNameOrderBy, bool isOrderByDesc, bool isLookup, Row rowLookup, Index indexLookup)
        {
            TypeRowSet(gridName);
            Row rowTable = UtilDataAccessLayer.RowCreate(gridName.TypeRow);
            IQueryable query;
            if (isLookup == false)
            {
                query = rowTable.Query(App, gridName);
            }
            else
            {
                query = rowTable.QueryLookup(rowLookup, new AppEventArg(App, gridName, indexLookup, null));
            }
            List<Row> rowList = new List<Row>();
            if (query != null)
            {
                Config.LoadDatabaseConfig(gridName);
                int pageRowCount = Config.ConfigGridGet(gridName).PageRowCount;
                rowList = UtilDataAccessLayer.Select(gridName.TypeRow, filterList, columnNameOrderBy, isOrderByDesc, 0, pageRowCount, query);
            }
            LoadRow(gridName, rowList);
        }

        /// <summary>
        /// Use this method for detail grid. See also method Row.MasterIsClick();
        /// </summary>
        public void LoadDatabaseInit(GridNameTypeRow gridName)
        {
            List<Row> rowList = new List<Row>();
            LoadRow(gridName, rowList);
        }

        /// <summary>
        /// Load data from Sql database.
        /// </summary>
        internal void LoadDatabase(GridNameTypeRow gridName)
        {
            LoadDatabase(gridName, null, null, false, false, null, null);
        }

        /// <summary>
        /// Parse user entered grid filter row from json.
        /// </summary>
        /// <param name="isExcludeCalculatedColumn">Exclude columns without ColumnNameSql from returned filterList.</param>
        private void LoadDatabaseFilterList(GridName gridName, out List<Filter> filterList, bool isExcludeCalculatedColumn)
        {
            Type typeRow = TypeRowGet(gridName);
            filterList = new List<Filter>();
            Row rowFilter = RowInternalGet(gridName, Index.Filter).RowFilter; // Data row with parsed filter values.
            Row rowFilterDefault = UtilDataAccessLayer.RowCreate(typeRow);
            foreach (Cell column in UtilDataAccessLayer.ColumnList(typeRow))
            {
                string columnName = column.ColumnNameCSharp;
                object value = column.Constructor(rowFilter).Value;
                object valueDefault = column.Constructor(rowFilterDefault).Value;
                if (!object.Equals(value, valueDefault)) // Use filter only when value set.
                {
                    if (isExcludeCalculatedColumn == false || (column.ColumnNameSql != null)) // Do not filter on calculated column if query provider is database.
                    {
                        FilterOperator filterOperator = FilterOperator.Equal;
                        if (value is string)
                        {
                            filterOperator = FilterOperator.Like;
                        }
                        else
                        {
                            string text = CellTextGet(gridName, Index.Filter, columnName);
                            if (text.Contains(">"))
                            {
                                filterOperator = FilterOperator.Greater;
                            }
                            if (text.Contains("<"))
                            {
                                filterOperator = FilterOperator.Greater;
                            }
                        }
                        filterList.Add(new Filter() { ColumnName = columnName, FilterOperator = filterOperator, Value = value });
                    }
                }
            }
        }

        /// <summary>
        /// Reload data from database with current grid filter and current sorting.
        /// </summary>
        public void LoadDatabaseReload(GridName gridName)
        {
            if (!IsErrorRowCell(gridName, Index.Filter)) // Do not reload data grid if there is text parse error in filter.
            {
                Type typeRow = TypeRowGet(gridName);
                Row rowTable = UtilDataAccessLayer.RowCreate(typeRow);
                IQueryable query = rowTable.Query(App, gridName);
                List<Row> rowList = new List<Row>();
                List<Filter> filterList = null;
                if (query != null)
                {
                    string ColumnNameOrderBy = null;
                    bool isOrderByDesc = false;
                    int pageIndex = 0;
                    if (queryList.ContainsKey(gridName)) // Reload
                    {
                        ColumnNameOrderBy = queryList[gridName].ColumnNameOrderBy;
                        isOrderByDesc = queryList[gridName].IsOrderByDesc;
                        pageIndex = queryList[gridName].PageIndex;
                        bool isExcludeCalculatedColumn = false; // UtilDataAccessLayer.QueryProviderIsDatabase(query); // If IQueryable.Provider is database, exclude columns without ColumnNameSql.
                        LoadDatabaseFilterList(gridName, out filterList, isExcludeCalculatedColumn);
                    }
                    Config.LoadDatabaseConfig(gridName);
                    int pageRowCount = Config.ConfigGridGet(GridNameTypeRow(gridName)).PageRowCount;
                    rowList = UtilDataAccessLayer.Select(typeRow, filterList, ColumnNameOrderBy, isOrderByDesc, pageIndex, pageRowCount, query);
                    if (pageIndex > 0 && rowList.Count == 0) // Page end reached.
                    {
                        queryList[gridName].PageIndex -= 1;
                        pageIndex = queryList[gridName].PageIndex;
                        rowList = UtilDataAccessLayer.Select(typeRow, filterList, ColumnNameOrderBy, isOrderByDesc, pageIndex, 15, query);
                    }
                }
                LoadRow(new GridNameTypeRow(typeRow, gridName), rowList);
            }
        }

        /// <summary>
        /// Load data directly from list into data grid. Returns false, if data grid has been removed.
        /// </summary>
        internal bool LoadRow(GridNameTypeRow gridName, List<Row> rowList)
        {
            bool result = gridName.TypeRow != null && rowList != null;
            if (result == false)
            {
                TypeRowSet(gridName);
                cellList.Remove(gridName);
                this.rowList.Remove(gridName);
            }
            else
            {
                foreach (Row row in rowList)
                {
                    UtilFramework.Assert(row.GetType() == gridName.TypeRow);
                }
                //
                Dictionary<string, GridCellInternal> cellListFilter = null;
                GridRowInternal rowFilter = null;
                if (cellList.ContainsKey(gridName))
                {
                    cellList[gridName].TryGetValue(Index.Filter, out cellListFilter); // Save filter user text.
                    this.rowList[gridName].TryGetValue(Index.Filter, out rowFilter); // Save parsed row.
                }
                cellList.Remove(gridName); // Clear user modified text and attached errors.
                this.rowList[gridName] = new Dictionary<Index, GridRowInternal>(); // Clear data
                TypeRowSet(gridName);
                //
                RowFilterAdd(gridName);
                for (int index = 0; index < rowList.Count; index++)
                {
                    RowSet(gridName, new Index(index.ToString()), new GridRowInternal() { Row = rowList[index], RowNew = null });
                }
                QueryGet(gridName).IsInsert = true; // Postpone call of method RowNewAdd(gridName); till Config is loaded.
                //
                if (cellListFilter != null)
                {
                    cellList[gridName] = new Dictionary<Index, Dictionary<string, GridCellInternal>>();
                    cellList[gridName][Index.Filter] = cellListFilter; // Load back filter user text.
                    this.rowList[gridName][Index.Filter] = rowFilter;
                }
            }
            return result;
        }

        /// <summary>
        /// Load data from single row into data grid.
        /// </summary>
        internal void LoadRow(GridNameTypeRow gridName, Row row)
        {
            if (row == null)
            {
                LoadRow(gridName, (List<Row>)null); // Remove data grid.
            }
            else
            {
                List<Row> rowList = new List<Row>();
                rowList.Add(row);
                LoadRow(gridName, rowList);
            }
        }

        /// <summary>
        /// Add data grid filter row.
        /// </summary>
        private void RowFilterAdd(GridName gridName)
        {
            Type typeRow = this.TypeRowGet(gridName);
            Row rowFilter = UtilDataAccessLayer.RowCreate(typeRow);
            RowSet(gridName, Index.Filter, new GridRowInternal() { RowFilter = rowFilter });
        }

        /// <summary>
        /// Add data row of enum New to RowList.
        /// </summary>
        internal void RowNewAdd(GridName gridName)
        {
            // (Index)
            Dictionary<Index, GridRowInternal> rowListCopy = rowList[gridName];
            rowList[gridName] = new Dictionary<Index, GridRowInternal>();
            // Filter
            foreach (Index index in rowListCopy.Keys)
            {
                if (index.Enum == IndexEnum.Filter)
                {
                    RowSet(gridName, index, rowListCopy[index]);
                    break;
                }
            }
            // Index
            int indexInt = 0;
            foreach (Index index in rowListCopy.Keys)
            {
                IndexEnum indexEnum = index.Enum;
                if (indexEnum == IndexEnum.Index || indexEnum == IndexEnum.New)
                {
                    RowSet(gridName, new Index(indexInt.ToString()), rowListCopy[index]); // New becomes Index
                    indexInt += 1;
                }
            }
            // New
            Type typeRow = this.TypeRowGet(gridName);
            Row rowNew = UtilDataAccessLayer.RowCreate(typeRow);
            RowSet(gridName, Index.New, new GridRowInternal() { Row = null, RowNew = rowNew }); // New row
            // Total
            foreach (Index index in rowListCopy.Keys)
            {
                if (index.Enum == IndexEnum.Total)
                {
                    RowSet(gridName, index, rowListCopy[index]);
                    break;
                }
            }
        }

        /// <summary>
        /// Select last row of data grid.
        /// </summary>
        private void SaveDatabaseSelectGridLastIndex(GridName gridName)
        {
            UtilFramework.Assert(SelectGridName == GridName.ToJson(gridName));
            Index indexLast = null;
            foreach (Index index in rowList[gridName].Keys)
            {
                if (index.Enum == IndexEnum.Index)
                {
                    indexLast = index;
                }
            }
            UtilFramework.Assert(indexLast != null);
            SelectIndex = indexLast;
        }

        /// <summary>
        /// Save data to sql database.
        /// </summary>
        internal void SaveDatabase()
        {
            foreach (GridName gridName in rowList.Keys.ToArray())
            {
                foreach (Index index in rowList[gridName].Keys.ToArray())
                {
                    IndexEnum indexEnum = index.Enum;
                    if (indexEnum == IndexEnum.Index || indexEnum == IndexEnum.New) // Exclude Filter and Total.
                    {
                        if (!IsErrorRowCell(gridName, index)) // No save if data row has text parse error!
                        {
                            if (IsModifyRowCell(gridName, index, false)) // Only save row if user modified row on latest request.
                            {
                                var row = rowList[gridName][index];
                                if (indexEnum == IndexEnum.Index) // Database Update
                                {
                                    try
                                    {
                                        row.RowNew.Update(row.Row, row.RowNew, new AppEventArg(App, gridName, index, null));
                                        ErrorRowSet(gridName, index, null);
                                        row.Row = row.RowNew;
                                        CellTextClear(gridName, index);
                                    }
                                    catch (Exception exception)
                                    {
                                        ErrorRowSet(gridName, index, UtilFramework.ExceptionToText(exception));
                                    }
                                }
                                if (indexEnum == IndexEnum.New) // Database Insert
                                {
                                    try
                                    {
                                        row.RowNew.Insert(row.RowNew, new AppEventArg(App, gridName, index, null));
                                        ErrorRowSet(gridName, index, null);
                                        row.Row = row.RowNew;
                                        CellTextClear(gridName, index);
                                        RowNewAdd(gridName); // User entered text in "New" row. Make "New" to "Index" and add "New". No Config check. It has to be Grid.IsInsert once we reached this point.
                                        SaveDatabaseSelectGridLastIndex(gridName);
                                    }
                                    catch (Exception exception)
                                    {
                                        ErrorRowSet(gridName, index, UtilFramework.ExceptionToText(exception));
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Set error on IsModify cell which could not be saved, because of an other error.
                            foreach (string columnName in cellList[gridName][index].Keys)
                            {
                                GridCellInternal cell = cellList[gridName][index][columnName];
                                if (cell.IsModify)
                                {
                                    if (ErrorCellGet(gridName, index, columnName) == null)
                                    {
                                        ErrorCellSet(gridName, index, columnName, "Not saved! Fix other error.");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Parse user modified input text. See also method TextSet(); when parse error occurs method ErrorSet(); is called for the cell.
        /// </summary>
        /// <param name="isFilterParse">Parse grid filter also if user made no modifications.</param>
        internal void TextParse(bool isFilterParse = false)
        {
            UtilFramework.LogDebug(string.Format("TextParse"));
            //
            foreach (GridName gridName in rowList.Keys)
            {
                foreach (Index index in rowList[gridName].Keys)
                {
                    if (IsModifyRowCell(gridName, index, false) || (index.Enum == IndexEnum.Filter && isFilterParse))
                    {
                        Type typeRow = TypeRowGet(gridName);
                        var row = RowInternalGet(gridName, index);
                        if (row.Row != null)
                        {
                            UtilFramework.Assert(row.Row.GetType() == typeRow);
                        }
                        IndexEnum indexEnum = index.Enum;
                        Row rowWrite;
                        switch (indexEnum)
                        {
                            case IndexEnum.Index:
                                rowWrite = UtilDataAccessLayer.RowClone(row.Row);
                                row.RowNew = rowWrite; // Row = Original; RowNew = Modified.
                                break;
                            case IndexEnum.New:
                                rowWrite = row.RowNew;
                                break;
                            case IndexEnum.Filter:
                                rowWrite = row.RowFilter;
                                break;
                            default:
                                throw new Exception("Enum unknown!");
                        }
                        foreach (Cell cell in ColumnList(gridName))
                        {
                            cell.Constructor(rowWrite);
                            string columnName = cell.ColumnNameCSharp;
                            //
                            GridCellInternal cellInternal = CellGet(gridName, index, columnName);
                            bool isDeleteKey = cellInternal.IsDeleteKey; // User hit delete or backspace key.
                            string text = cellInternal.Text;
                            bool isModify = cellInternal.IsModify;
                            if (isModify || cellInternal.Error != null) // Cell IsModify or has an error from a previous request.
                            {
                                try
                                {
                                    var appEventArg  = new AppEventArg(App, gridName, index, columnName);
                                    App.CellTextParse(cell, ref text, isDeleteKey, appEventArg);
                                    cell.TextParse(text, appEventArg); // Write to row
                                    text = text == "" ? null : text;
                                    //
                                    string textCompare = null;
                                    {
                                        if (index.Enum == IndexEnum.Index)
                                        {
                                            textCompare = UtilDataAccessLayer.RowValueToText(cell.Value, cell.TypeColumn);
                                        }
                                        else
                                        {
                                            if (text != null) // Not default value has been written to Row.
                                            {
                                                textCompare = UtilDataAccessLayer.RowValueToText(cell.Value, cell.TypeColumn);
                                            }
                                            else
                                            {
                                                textCompare = CellTextGet(gridName, index, columnName); // Do not read value from row for filter! It could be non nullable int. Do not show "0" in filter.
                                            }
                                        }
                                        textCompare = textCompare == "" ? null : textCompare;
                                    }
                                    if (textCompare != text) // For example user entered "8.". It would be overwritten on screen with "8".
                                    {
                                        ErrorCellSet(gridName, index, columnName, "Value invalid!");
                                        row.RowNew = null; // Do not save.
                                        break;
                                    }
                                    else
                                    {
                                        cellInternal.IsParseSuccess = true;
                                    }
                                }
                                catch (Exception exception)
                                {
                                    ErrorCellSet(gridName, index, columnName, exception.Message);
                                    row.RowNew = null; // Do not save.
                                    break;
                                }
                                ErrorCellSet(gridName, index, columnName, null); // Clear error.
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Load Query data from json.
        /// </summary>
        private void LoadJsonQuery(AppJson appJson)
        {
            GridDataJson gridDataJson = appJson.GridDataJson;
            //
            foreach (string gridName in gridDataJson.GridQueryList.Keys)
            {
                GridQuery gridQueryJson = gridDataJson.GridQueryList[gridName];
                GridQueryInternal gridQuery = QueryGet(GridName.FromJson(gridName));
                gridQuery.ColumnNameOrderBy = gridQueryJson.ColumnNameOrderBy;
                gridQuery.IsOrderByDesc = gridQueryJson.IsOrderByDesc;
                gridQuery.PageIndex = gridQueryJson.PageIndex;
            }
        }

        /// <summary>
        /// Load GridColumn data from json.
        /// </summary>
        private void LoadJsonColumn(AppJson appJson)
        {
            GridDataJson gridDataJson = appJson.GridDataJson;
            //
            foreach (string gridName in gridDataJson.ColumnList.Keys)
            {
                foreach (GridColumn gridColumnJson in gridDataJson.ColumnList[gridName])
                {
                    GridColumnInternal gridColumn = ColumnGet(GridName.FromJson(gridName), gridColumnJson.ColumnName);
                    gridColumn.ColumnName = gridColumnJson.ColumnName;
                    gridColumn.IsClick = gridColumnJson.IsClick;
                }
            }
        }

        /// <summary>
        /// Load data from http json request.
        /// </summary>
        internal void LoadJson(GridName gridName)
        {
            UtilFramework.LogDebug(string.Format("DataFromJson ({0})", GridName.ToJson(gridName)));
            //
            GridDataJson gridDataJson = App.AppJson.GridDataJson;
            //
            string tableNameCSharp = gridDataJson.GridQueryList[GridName.ToJson(gridName)].TypeRow;
            Type typeRow = UtilDataAccessLayer.TypeRowFromTableNameCSharp(tableNameCSharp, UtilApplication.TypeRowInAssembly(App));
            TypeRowSet(new GridNameTypeRow(typeRow, gridName));
            //
            foreach (GridRow rowJson in gridDataJson.RowList[GridName.ToJson(gridName)])
            {
                Index rowIndex = new Index(rowJson.Index);
                IndexEnum indexEnum = rowIndex.Enum;
                GridRowInternal gridRow = new GridRowInternal() { IsSelect = rowJson.IsSelect, IsClick = rowJson.IsClick };
                Row resultRow = (Row)UtilFramework.TypeToObject(typeRow);
                switch (indexEnum)
                {
                    case IndexEnum.Index:
                        gridRow.Row = resultRow;
                        break;
                    case IndexEnum.Filter:
                        gridRow.RowFilter = resultRow;
                        break;
                    case IndexEnum.New:
                        gridRow.RowNew = resultRow;
                        break;
                    default:
                        throw new Exception("Enum unknown!");
                }
                RowSet(gridName, rowIndex, gridRow);
                foreach (Cell cell in ColumnList(gridName))
                {
                    Row row = this.RowGet(gridName, rowIndex);
                    cell.Constructor(row);
                    string columnName = cell.ColumnNameCSharp;
                    //
                    GridCell gridCell = gridDataJson.CellList[GridName.ToJson(gridName)][columnName][rowJson.Index];
                    GridCellInternal gridCellInternal = CellGet(gridName, rowIndex, columnName);
                    gridCellInternal.IsClick = gridCell.IsClick;
                    gridCellInternal.IsModify = gridCell.IsModify;
                    gridCellInternal.IsDeleteKey = gridCell.IsDeleteKey;
                    gridCellInternal.IsLookup = gridCell.IsLookup;
                    gridCellInternal.GridNameLookup = gridCell.GridNameLookup;
                    gridCellInternal.FocusId = gridCell.FocusId;
                    gridCellInternal.FocusIdRequest = gridCell.FocusIdRequest;
                    gridCellInternal.PlaceHolder = gridCell.PlaceHolder;
                    string text;
                    if (gridCell.IsO == true)
                    {
                        text = gridCell.O; // Original text.
                        string textModify = gridCell.T; // User modified text.
                        CellTextSet(gridName, rowIndex, columnName, textModify, true, text);
                    }
                    else
                    {
                        text = gridCell.T; // Original text.
                        CellTextSet(gridName, rowIndex, columnName, text, false, null);
                    }
                    // ErrorCell
                    string errorCellText = gridCell.E;
                    if (errorCellText != null)
                    {
                        ErrorCellSet(gridName, rowIndex, columnName, errorCellText);
                    }
                    // ErrorRow
                    string errorRowText = rowJson.Error;
                    if (errorRowText != null)
                    {
                        ErrorRowSet(gridName, rowIndex, errorRowText);
                    }
                    App.CellRowValueFromText(cell, ref text, new AppEventArg(App, gridName, rowIndex, cell.ColumnNameCSharp));
                    cell.RowValueFromText(ref text, new AppEventArg(App, gridName, rowIndex, null));
                    if (rowIndex.Enum == IndexEnum.Index)
                    {
                        object value = UtilDataAccessLayer.RowValueFromText(text, cell.PropertyInfo.PropertyType);
                        cell.PropertyInfo.SetValue(resultRow, value);
                    }
                    else
                    {
                        // Do not load text into RowFilter. RowFilter could have non nullable int. But filter is null!
                    }
                }
            }
        }

        /// <summary>
        /// Load data from GridDataJson to GridData.
        /// </summary>
        internal void LoadJson()
        {
            GridDataJson gridDataJson = App.AppJson.GridDataJson;
            //
            if (gridDataJson != null)
            {
                LoadJsonQuery(App.AppJson);
                LoadJsonColumn(App.AppJson);
                //
                foreach (string gridName in gridDataJson.GridQueryList.Keys)
                {
                    LoadJson(GridName.FromJson(gridName));
                }
                //
                SelectGridName = gridDataJson.SelectGridName;
                SelectIndex = new Index(gridDataJson.SelectIndex);
                SelectColumnName = gridDataJson.SelectColumnName;
            }
        }

        /// <summary>
        /// Returns row's columns.
        /// </summary>
        private static List<GridColumn> TypeRowToGridColumn(App app, GridNameTypeRow gridName)
        {
            var result = new List<GridColumn>();
            //
            var config = app.GridData.Config;
            var columnList = UtilDataAccessLayer.ColumnList(gridName.TypeRow);
            double widthPercentTotal = 0;
            bool isLast = false;
            //
            List<Cell> columnIsVisibleList = new List<Cell>();
            foreach (Cell column in columnList)
            {
                bool isVisible = config.ConfigColumnGet(gridName, column).IsVisible;
                if (isVisible)
                {
                    columnIsVisibleList.Add(column);
                }
            }
            //
            int i = -1;
            foreach (Cell column in columnList)
            {
                // Text
                string text = config.ConfigColumnGet(gridName, column).Text;
                //
                bool isVisible = columnIsVisibleList.Contains(column);
                if (isVisible)
                {
                    i = i + 1;
                    isLast = column == columnIsVisibleList.LastOrDefault();
                    double widthPercentAvg = Math.Round(((double)100 - widthPercentTotal) / ((double)columnIsVisibleList.Count - (double)i), 2);
                    double widthPercent = widthPercentAvg;
                    column.WidthPercent(ref widthPercent);
                    widthPercent = Math.Round(widthPercent, 2);
                    if (isLast)
                    {
                        widthPercent = 100 - widthPercentTotal;
                    }
                    else
                    {
                        if (widthPercentTotal + widthPercent > 100)
                        {
                            widthPercent = widthPercentAvg;
                        }
                    }
                    widthPercentTotal = widthPercentTotal + widthPercent;
                    result.Add(new GridColumn() { ColumnName = column.ColumnNameCSharp, Text = text, WidthPercent = widthPercent, IsVisible = true });
                }
                else
                {
                    result.Add(new GridColumn() { ColumnName = column.ColumnNameCSharp, Text = null, IsVisible = false });
                }
            }
            return result;
        }

        /// <summary>
        /// Save column state to Json.
        /// </summary>
        private void SaveJsonColumn(AppJson appJson)
        {
            GridDataJson gridDataJson = appJson.GridDataJson;
            //
            foreach (string gridName in gridDataJson.ColumnList.Keys)
            {
                foreach (GridColumn gridColumnJson in gridDataJson.ColumnList[gridName])
                {
                    GridColumnInternal gridColumn = ColumnGet(GridName.FromJson(gridName), gridColumnJson.ColumnName);
                    gridColumnJson.IsClick = gridColumn.IsClick;
                }
            }
        }

        /// <summary>
        /// Save Query back to Json.
        /// </summary>
        private void SaveJsonQuery(AppJson appJson)
        {
            GridDataJson gridDataJson = appJson.GridDataJson;
            //
            foreach (GridName gridName in queryList.Keys)
            {
                GridQueryInternal gridQuery = queryList[gridName];
                GridQuery gridQueryJson = gridDataJson.GridQueryList[GridName.ToJson(gridName)];
                gridQueryJson.ColumnNameOrderBy = gridQuery.ColumnNameOrderBy;
                gridQueryJson.IsOrderByDesc = gridQuery.IsOrderByDesc;
                gridQueryJson.PageIndex = gridQuery.PageIndex;
            }
        }

        private void SaveJsonSelect(AppJson appJson)
        {
            appJson.GridDataJson.SelectGridName = SelectGridName;
            appJson.GridDataJson.SelectIndex = SelectIndex?.Value;
            appJson.GridDataJson.SelectColumnName = SelectColumnName;
        }

        /// <summary>
        /// Render cell as Button, Html or FileUpload.
        /// </summary>
        private void SaveJsonIsButtonHtmlFileUpload(GridNameTypeRow gridName, Index index, Cell cell, GridCell gridCell)
        {
            ConfigCell configCell = App.GridData.Config.ConfigCellGet(gridName, index, cell);
            //
            gridCell.CellEnum = configCell.CellEnum;
            gridCell.CssClass = configCell.CssClass.ToHtml();
        }

        /// <summary>
        /// Copy data from class GridData to class GridDataJson.
        /// </summary>
        internal void SaveJson()
        {
            AppJson appJson = App.AppJson;
            //
            if (appJson.GridDataJson == null)
            {
                appJson.GridDataJson = new GridDataJson();
                appJson.GridDataJson.ColumnList = new Dictionary<string, List<GridColumn>>();
                appJson.GridDataJson.RowList = new Dictionary<string, List<GridRow>>();
            }
            GridDataJson gridDataJson = appJson.GridDataJson;
            //
            if (gridDataJson.GridQueryList == null)
            {
                gridDataJson.GridQueryList = new Dictionary<string, GridQuery>();
            }
            //
            foreach (GridName gridName in rowList.Keys)
            {
                UtilFramework.LogDebug(string.Format("DataToJson ({0})", GridName.ToJson(gridName)));
                //
                Type typeRow = TypeRowGet(gridName);
                GridNameTypeRow gridNameTypeRow = new GridNameTypeRow(typeRow, gridName);
                //
                gridDataJson.GridQueryList[GridName.ToJson(gridName)] = new GridQuery() { GridName = GridName.ToJson(gridName), TypeRow = UtilDataAccessLayer.TypeRowToTableNameCSharp(typeRow) };
                // Row
                if (gridDataJson.RowList == null)
                {
                    gridDataJson.RowList = new Dictionary<string, List<GridRow>>();
                }
                gridDataJson.RowList[GridName.ToJson(gridName)] = new List<GridRow>();
                // Column
                if (gridDataJson.ColumnList == null)
                {
                    gridDataJson.ColumnList = new Dictionary<string, List<GridColumn>>();
                }
                gridDataJson.ColumnList[GridName.ToJson(gridName)] = TypeRowToGridColumn(App, gridNameTypeRow);
                // Cell
                if (gridDataJson.CellList == null)
                {
                    gridDataJson.CellList = new Dictionary<string, Dictionary<string, Dictionary<string, GridCell>>>();
                }
                gridDataJson.CellList[GridName.ToJson(gridName)] = new Dictionary<string, Dictionary<string, GridCell>>();
                //
                PropertyInfo[] propertyInfoList = null;
                foreach (Index index in rowList[gridName].Keys)
                {
                    GridRowInternal gridRow = rowList[gridName][index];
                    Row row = RowGet(gridName, index);
                    string errorRow = ErrorRowGet(gridName, index);
                    GridRow gridRowJson = new GridRow() { Index = index.Value, IsSelect = gridRow.IsSelect, IsClick = gridRow.IsClick, Error = errorRow };
                    gridRowJson.IsFilter = index.Enum == IndexEnum.Filter;
                    gridDataJson.RowList[GridName.ToJson(gridName)].Add(gridRowJson);
                    if (propertyInfoList == null && typeRow != null)
                    {
                        propertyInfoList = UtilDataAccessLayer.TypeRowToPropertyList(typeRow);
                    }
                    if (propertyInfoList != null)
                    {
                        foreach (Cell cell in ColumnList(gridName))
                        {
                            cell.Constructor(row);
                            //
                            string columnName = cell.ColumnNameCSharp;
                            object value = null;
                            if (row != null)
                            {
                                value = cell.PropertyInfo.GetValue(row);
                            }
                            string textJson;
                            if (index.Enum == IndexEnum.Index)
                            {
                                textJson = UtilDataAccessLayer.RowValueToText(value, cell.TypeColumn);
                                App.CellRowValueToText(cell, ref textJson, new AppEventArg(App, gridName, index, columnName)); // Override text generic.
                                cell.RowValueToText(ref textJson, new AppEventArg(App, gridName, index, columnName)); // Override text.
                            }
                            else
                            {
                                textJson = CellTextGet(gridName, index, columnName); // Filter could bo not nullable int. Do not show "0" in filter.
                            }
                            ConfigCell configCell = App.GridData.Config.ConfigCellGet(gridNameTypeRow, index, cell);
                            if (configCell.CellEnum == GridCellEnum.Button && textJson == null && cell.TypeColumn == typeof(string))
                            {
                                textJson = "Button"; // Default text for button.
                            }
                            GridCellInternal gridCellInternal = CellGet(gridName, index, columnName);
                            if (!gridDataJson.CellList[GridName.ToJson(gridName)].ContainsKey(columnName))
                            {
                                gridDataJson.CellList[GridName.ToJson(gridName)][columnName] = new Dictionary<string, GridCell>();
                            }
                            string errorCell = ErrorCellGet(gridName, index, columnName);
                            GridCell gridCellJson = new GridCell();
                            gridDataJson.CellList[GridName.ToJson(gridName)][columnName][index.Value] = gridCellJson;
                            //
                            SaveJsonIsButtonHtmlFileUpload(gridNameTypeRow, index, cell, gridCellJson);
                            //
                            if (gridCellInternal.IsOriginal == false || gridCellInternal.IsParseSuccess)
                            {
                                gridCellJson.T = textJson;
                            }
                            else
                            {
                                gridCellJson.O = textJson;
                                gridCellJson.T = gridCellInternal.Text; // Never overwrite user entered text.
                                gridCellJson.IsO = true;
                            }
                            gridCellJson.PlaceHolder = configCell.PlaceHolder;
                            gridCellJson.IsClick = gridCellInternal.IsClick;
                            gridCellJson.IsModify = gridCellInternal.IsModify;
                            gridCellJson.E = errorCell;
                            gridCellJson.IsLookup = gridCellInternal.IsLookup;
                            gridCellJson.GridNameLookup = gridCellInternal.GridNameLookup;
                            gridCellJson.FocusId = gridCellInternal.FocusId;
                            gridCellJson.FocusIdRequest = gridCellInternal.FocusIdRequest;
                        }
                    }
                }
            }
            // Query removed rows. For example methos LoadRow("Table", null, null);
            foreach (GridName gridName in queryList.Keys)
            {
                if (!rowList.ContainsKey(gridName))
                {
                    gridDataJson.ColumnList[GridName.ToJson(gridName)] = new List<GridColumn>(); // Remove GridColumn (header).
                    gridDataJson.RowList[GridName.ToJson(gridName)] = new List<GridRow>();
                }
            }
            SaveJsonColumn(appJson);
            SaveJsonQuery(appJson);
            SaveJsonSelect(appJson);
        }
    }
}
