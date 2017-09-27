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

        public bool IsLookup;

        /// <summary>
        /// Gets or sets FocusId. GridCell can be displayed by multiple GridField. Focus has the one with FocusId. Used to show or hide Lookup.
        /// </summary>
        public int? FocusId;

        /// <summary>
        /// Gets FocusIdRequest. Sent by client if it got focus.
        /// </summary>
        public int? FocusIdRequest;

        /// <summary>
        /// Gets or sets IsOriginal. Text is modified by user. Original text is stored in TextOriginal.
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
        public bool IsClick;
    }

    internal class GridQueryInternal
    {
        public string FieldNameOrderBy;

        public bool IsOrderByDesc;
    }

    public class GridData
    {
        internal GridData(App app)
        {
            this.App = app;
        }

        internal readonly App App;

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
        /// (GridName, TypeRow)
        /// </summary>
        private Dictionary<GridName, Type> typeRowList = new Dictionary<GridName, Type>();

        private Type TypeRowGet(GridName gridName)
        {
            Type result;
            typeRowList.TryGetValue(gridName, out result);
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

        /// <summary>
        /// Returns data row.
        /// </summary>
        public Row Row(GridName gridName, Index index)
        {
            Row result = null;
            var row = RowGet(gridName, index);
            if (row != null)
            {
                result = row.Row;
                if (result == null)
                {
                    result = row.RowNew;
                }
            }
            return result;
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

        private GridQueryInternal QueryGet(GridName gridName)
        {
            if (!queryList.ContainsKey(gridName))
            {
                queryList[gridName] = new GridQueryInternal();
            }
            return queryList[gridName];
        }

        /// <summary>
        /// (GridName, FieldName, GridColumn).
        /// </summary>
        private Dictionary<GridName, Dictionary<string, GridColumnInternal>> columnList = new Dictionary<GridName, Dictionary<string, GridColumnInternal>>();

        private GridColumnInternal ColumnGet(GridName gridName, string fieldName)
        {
            if (!columnList.ContainsKey(gridName))
            {
                columnList[gridName] = new Dictionary<string, GridColumnInternal>();
            }
            if (!columnList[gridName].ContainsKey(fieldName))
            {
                columnList[gridName][fieldName] = new GridColumnInternal();
            }
            //
            return columnList[gridName][fieldName];
        }

        /// <summary>
        /// (GridName, Index). Original row as loaded from json.
        /// </summary>
        private Dictionary<GridName, Dictionary<Index, GridRowInternal>> rowList = new Dictionary<GridName, Dictionary<Index, GridRowInternal>>();

        private string selectGridName;

        private Index selectIndex;

        private string selectFieldName;

        private GridRowInternal RowGet(GridName gridName, Index index)
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
        /// (GridName, Index, FieldName, GridCellInternal).
        /// </summary>
        private Dictionary<GridName, Dictionary<Index, Dictionary<string, GridCellInternal>>> cellList = new Dictionary<GridName, Dictionary<Index, Dictionary<string, GridCellInternal>>>();

        internal void CellAll(Action<GridCellInternal> callback)
        {
            foreach (GridName gridName in cellList.Keys)
            {
                foreach (Index index in cellList[gridName].Keys)
                {
                    foreach (string fieldName in cellList[gridName][index].Keys)
                    {
                        callback(cellList[gridName][index][fieldName]);
                    }
                }
            }
        }

        /// <summary>
        /// Set IsLookup flag on cell.
        /// </summary>
        internal void LookupOpen(GridName gridName, Index index, string fieldName)
        {
            int lookupCount = 0;
            foreach (GridName gridNameItem in cellList.Keys)
            {
                foreach (Index indexItem in cellList[gridNameItem].Keys)
                {
                    foreach (string fieldNameItem in cellList[gridNameItem][indexItem].Keys)
                    {
                        bool isLookup = gridName == gridNameItem && index == indexItem && fieldName == fieldNameItem;
                        if (isLookup)
                        {
                            lookupCount += 1;
                        }
                        if (lookupCount > 1)
                        {
                            isLookup = false;
                        }
                        cellList[gridNameItem][indexItem][fieldNameItem].IsLookup = isLookup;
                    }
                }
            }
        }

        /// <summary>
        /// Set IsLookup flag to false.
        /// </summary>
        internal void LookupClose()
        {
            foreach (GridName gridNameItem in cellList.Keys)
            {
                foreach (Index indexItem in cellList[gridNameItem].Keys)
                {
                    foreach (string fieldNameItem in cellList[gridNameItem][indexItem].Keys)
                    {
                        cellList[gridNameItem][indexItem][fieldNameItem].IsLookup = false;
                    }
                }
            }
            //
            LoadRow(new GridNameTypeRow(null, UtilApplication.GridNameLookup), (List<Row>)null);
        }

        internal GridCellInternal CellGet(GridName gridName, Index index, string fieldName)
        {
            GridCellInternal result = null;
            if (cellList.ContainsKey(gridName))
            {
                if (cellList[gridName].ContainsKey(index))
                {
                    if (cellList[gridName][index].ContainsKey(fieldName))
                    {
                        result = cellList[gridName][index][fieldName];
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
                cellList[gridName][index][fieldName] = result;
            }
            return result;
        }

        /// <summary>
        /// Returns error attached to data row.
        /// </summary>
        private string ErrorRowGet(GridName gridName, Index index)
        {
            return RowGet(gridName, index).Error;
        }

        /// <summary>
        /// Set error on data row.
        /// </summary>
        internal void ErrorRowSet(GridName gridName, Index index, string text)
        {
            RowGet(gridName, index).Error = text;
        }

        /// <summary>
        /// Gets user entered text.
        /// </summary>
        /// <returns>If null, user has not changed text.</returns>
        private string CellTextGet(GridName gridName, Index index, string fieldName)
        {
            return CellGet(gridName, index, fieldName).Text;
        }

        /// <summary>
        /// Sets user entered text.
        /// </summary>
        /// <param name="text">If null, user has not changed text.</param>
        private void CellTextSet(GridName gridName, Index index, string fieldName, string text, bool isOriginal, string textOriginal)
        {
            GridCellInternal cell = CellGet(gridName, index, fieldName);
            cell.Text = text;
            cell.IsOriginal = isOriginal;
            cell.TextOriginal = textOriginal;
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

        private string ErrorCellGet(GridName gridName, Index index, string fieldName)
        {
            return CellGet(gridName, index, fieldName).Error;
        }

        private void ErrorCellSet(GridName gridName, Index index, string fieldName, string text)
        {
            CellGet(gridName, index, fieldName).Error = text;
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
                    foreach (string fieldName in cellList[gridName][index].Keys)
                    {
                        if (cellList[gridName][index][fieldName].Error != null)
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
        private bool IsModifyRowCell(GridName gridName, Index index)
        {
            bool result = false;
            Type typeRow = TypeRowGet(gridName);
            if (cellList.ContainsKey(gridName))
            {
                if (cellList[gridName].ContainsKey(index))
                {
                    foreach (Cell column in UtilDataAccessLayer.ColumnList(typeRow))
                    {
                        if (column.FieldNameSql != null) // Exclude calculated column
                        {
                            string fieldName = column.FieldNameCSharp;
                            if (cellList[gridName][index].ContainsKey(fieldName))
                            {
                                if (cellList[gridName][index][fieldName].IsModify)
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
        internal void LoadDatabase(GridNameTypeRow gridName, List<Filter> filterList, string fieldNameOrderBy, bool isOrderByDesc)
        {
            TypeRowSet(gridName);
            Row rowTable = UtilDataAccessLayer.RowCreate(gridName.TypeRow);
            IQueryable query = rowTable.Where(App, gridName);
            List<Row> rowList = new List<Row>();
            if (query != null)
            {
                rowList = UtilDataAccessLayer.Select(gridName.TypeRow, filterList, fieldNameOrderBy, isOrderByDesc, 0, 15, query);
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
        public void LoadDatabase(GridNameTypeRow gridName)
        {
            LoadDatabase(gridName, null, null, false);
        }

        /// <summary>
        /// Parse user entered grid filter row from json.
        /// </summary>
        private void LoadDatabaseFilterList(GridName gridName, out List<Filter> filterList)
        {
            Type typeRow = TypeRowGet(gridName);
            filterList = new List<Filter>();
            Row rowFilter = RowGet(gridName, new Index(IndexEnum.Filter)).RowFilter; // Data row with parsed filter values.
            foreach (Cell column in UtilDataAccessLayer.ColumnList(typeRow))
            {
                string fieldName = column.FieldNameCSharp;
                string text = CellTextGet(gridName, new Index(IndexEnum.Filter), fieldName);
                if (text == "")
                {
                    text = null;
                }
                if (text != null) // Use filter only when text set.
                {
                    if (column.FieldNameSql != null) // Do not filter on calculated column.
                    {
                        if (rowFilter != null) // RowFilter is null, if user made no modifications. See also: GridData.TextParse(isFilterParse: true);
                        {
                            object value = rowFilter.GetType().GetProperty(fieldName).GetValue(rowFilter);
                            FilterOperator filterOperator = FilterOperator.Equal;
                            if (value is string)
                            {
                                filterOperator = FilterOperator.Like;
                            }
                            else
                            {
                                if (text.Contains(">"))
                                {
                                    filterOperator = FilterOperator.Greater;
                                }
                                if (text.Contains("<"))
                                {
                                    filterOperator = FilterOperator.Greater;
                                }
                            }
                            filterList.Add(new Filter() { FieldName = fieldName, FilterOperator = filterOperator, Value = value });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reload data from database with current grid filter and current sorting.
        /// </summary>
        internal void LoadDatabaseReload(GridName gridName)
        {
            if (!IsErrorRowCell(gridName, new Index(IndexEnum.Filter))) // Do not reload data grid if there is text parse error in filter.
            {
                Type typeRow = TypeRowGet(gridName);
                Row rowTable = UtilDataAccessLayer.RowCreate(typeRow);
                IQueryable query = rowTable.Where(App, gridName);
                List<Row> rowList = new List<Row>();
                List<Filter> filterList = null;
                if (query != null)
                {
                    string fieldNameOrderBy = null;
                    bool isOrderByDesc = false;
                    if (queryList.ContainsKey(gridName)) // Reload
                    {
                        fieldNameOrderBy = queryList[gridName].FieldNameOrderBy;
                        isOrderByDesc = queryList[gridName].IsOrderByDesc;
                        LoadDatabaseFilterList(gridName, out filterList);
                    }
                    rowList = UtilDataAccessLayer.Select(typeRow, filterList, fieldNameOrderBy, isOrderByDesc, 0, 15, query);
                }
                LoadRow(new GridNameTypeRow(typeRow, gridName), rowList);
            }
        }

        /// <summary>
        /// Load data directly from list into data grid. Returns false, if data grid has been removed.
        /// </summary>
        public bool LoadRow(GridNameTypeRow gridName, List<Row> rowList)
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
                if (cellList.ContainsKey(gridName))
                {
                    cellList[gridName].TryGetValue(new Index(IndexEnum.Filter), out cellListFilter); // Save filter user text.
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
                RowNewAdd(gridName);
                //
                if (cellListFilter != null)
                {
                    cellList[gridName] = new Dictionary<Index, Dictionary<string, GridCellInternal>>();
                    cellList[gridName][new Index(IndexEnum.Filter)] = cellListFilter; // Load back filter user text.
                }
            }
            return result;
        }

        /// <summary>
        /// Load data from single row into data grid.
        /// </summary>
        public void LoadRow(GridNameTypeRow gridName, Row row)
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
            RowSet(gridName, new Index(IndexEnum.Filter), new GridRowInternal() { Row = null, RowNew = null });
        }

        /// <summary>
        /// Add data row of enum New to RowList.
        /// </summary>
        private void RowNewAdd(GridName gridName)
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
            RowSet(gridName, new Index(IndexEnum.New), new GridRowInternal() { Row = null, RowNew = rowNew }); // New row
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
            UtilFramework.Assert(selectGridName == gridName.Value);
            Index indexLast = null;
            foreach (Index index in rowList[gridName].Keys)
            {
                if (index.Enum == IndexEnum.Index)
                {
                    indexLast = index;
                }
            }
            UtilFramework.Assert(indexLast != null);
            selectIndex = indexLast;
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
                            if (IsModifyRowCell(gridName, index)) // Only save row if user modified row on latest request.
                            {
                                var row = rowList[gridName][index];
                                if (row.Row != null && row.RowNew != null) // Database Update
                                {
                                    try
                                    {
                                        row.RowNew.Update(App, row.Row, row.RowNew);
                                        ErrorRowSet(gridName, index, null);
                                        row.Row = row.RowNew;
                                        CellTextClear(gridName, index);
                                    }
                                    catch (Exception exception)
                                    {
                                        ErrorRowSet(gridName, index, UtilFramework.ExceptionToText(exception));
                                    }
                                }
                                if (row.Row == null && row.RowNew != null) // Database Insert
                                {
                                    try
                                    {
                                        row.RowNew.Insert(App);
                                        ErrorRowSet(gridName, index, null);
                                        row.Row = row.RowNew;
                                        CellTextClear(gridName, index);
                                        RowNewAdd(gridName); // Make "New" to "Index" and add "New"
                                        SaveDatabaseSelectGridLastIndex(gridName);
                                    }
                                    catch (Exception exception)
                                    {
                                        ErrorRowSet(gridName, index, UtilFramework.ExceptionToText(exception));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Parse user modified input text. See also method TextSet(); when parse error occurs method ErrorSet(); is called for the field.
        /// </summary>
        /// <param name="isFilterParse">Parse grid filter also if user made no modifications.</param>
        internal void TextParse(bool isFilterParse = false)
        {
            foreach (GridName gridName in rowList.Keys)
            {
                foreach (Index index in rowList[gridName].Keys)
                {
                    if (IsModifyRowCell(gridName, index) || (index.Enum == IndexEnum.Filter && isFilterParse))
                    {
                        Type typeRow = TypeRowGet(gridName);
                        var row = RowGet(gridName, index);
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
                                row.RowNew = rowWrite;
                                break;
                            case IndexEnum.New:
                                rowWrite = UtilDataAccessLayer.RowCreate(typeRow);
                                row.RowNew = rowWrite;
                                break;
                            case IndexEnum.Filter:
                                rowWrite = UtilDataAccessLayer.RowCreate(typeRow);
                                row.RowFilter = rowWrite;
                                break;
                            default:
                                throw new Exception("Enum unknown!");
                        }
                        foreach (Cell cell in ColumnList(gridName))
                        {
                            cell.Constructor(rowWrite);
                            string fieldName = cell.FieldNameCSharp;
                            //
                            GridCellInternal cellInternal = CellGet(gridName, index, fieldName);
                            string text = cellInternal.Text;
                            bool isModify = cellInternal.IsModify;
                            if (isModify)
                            {
                                try
                                {
                                    cell.CellTextParse(App, gridName, index, ref text);
                                }
                                catch (Exception exception)
                                {
                                    ErrorCellSet(gridName, index, fieldName, exception.Message);
                                    row.RowNew = null; // Do not save.
                                    break;
                                }
                            }
                            if (text != null)
                            {
                                if (text == "")
                                {
                                    text = null;
                                }
                                if (!(text == null && indexEnum == IndexEnum.Filter)) // Do not parse text null for filter.
                                {
                                    object value;
                                    try
                                    {
                                        cell.CellValueFromText(App, gridName, index, ref text);
                                        App.CellValueFromText(gridName, index, cell, ref text);
                                        value = UtilDataAccessLayer.ValueFromText(text, rowWrite.GetType().GetProperty(fieldName).PropertyType); // Parse text.
                                    }
                                    catch (Exception exception)
                                    {
                                        ErrorCellSet(gridName, index, fieldName, exception.Message);
                                        row.RowNew = null; // Do not save.
                                        break;
                                    }
                                    rowWrite.GetType().GetProperty(fieldName).SetValue(rowWrite, value);
                                }
                            }
                            ErrorCellSet(gridName, index, fieldName, null); // Clear error.
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
                GridQueryInternal gridQuery = QueryGet(new GridName(gridName, true));
                gridQuery.FieldNameOrderBy = gridQueryJson.FieldNameOrderBy;
                gridQuery.IsOrderByDesc = gridQueryJson.IsOrderByDesc;
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
                    GridColumnInternal gridColumn = ColumnGet(new GridName(gridName, true), gridColumnJson.FieldName);
                    gridColumn.IsClick = gridColumnJson.IsClick;
                }
            }
        }

        /// <summary>
        /// Load data from http json request.
        /// </summary>
        internal void LoadJson(GridName gridName)
        {
            GridDataJson gridDataJson = App.AppJson.GridDataJson;
            //
            string typeRowString = gridDataJson.GridQueryList[gridName.Value].TypeRow;
            Type typeRow = UtilDataAccessLayer.TypeRowFromName(typeRowString, UtilApplication.TypeRowInAssembly(App));
            TypeRowSet(new GridNameTypeRow(typeRow, gridName));
            //
            foreach (GridRow row in gridDataJson.RowList[gridName.Value])
            {
                Index rowIndex = new Index(row.Index);
                IndexEnum indexEnum = rowIndex.Enum;
                GridRowInternal gridRow = new GridRowInternal() { IsSelect = row.IsSelect, IsClick = row.IsClick };
                Row resultRow = null;
                if (indexEnum == IndexEnum.Index)
                {
                    resultRow = (Row)UtilFramework.TypeToObject(typeRow);
                    gridRow.Row = resultRow;
                }
                if (indexEnum == IndexEnum.New)
                {
                    resultRow = (Row)UtilFramework.TypeToObject(typeRow);
                    gridRow.RowNew = resultRow;
                }
                RowSet(gridName, rowIndex, gridRow);
                foreach (Cell cell in ColumnList(gridName))
                {
                    cell.Constructor(gridRow.Row);
                    string fieldName = cell.FieldNameCSharp;
                    //
                    GridCell gridCell = gridDataJson.CellList[gridName.Value][fieldName][row.Index];
                    GridCellInternal gridCellInternal = CellGet(gridName, rowIndex, fieldName);
                    gridCellInternal.IsClick = gridCell.IsClick;
                    gridCellInternal.IsModify = gridCell.IsModify;
                    gridCellInternal.IsLookup = gridCell.IsLookup;
                    gridCellInternal.FocusId = gridCell.FocusId;
                    gridCellInternal.FocusIdRequest = gridCell.FocusIdRequest;
                    gridCellInternal.PlaceHolder = gridCell.PlaceHolder;
                    string text;
                    if (gridDataJson.CellList[gridName.Value][cell.FieldNameCSharp][row.Index].IsO)
                    {
                        text = gridDataJson.CellList[gridName.Value][fieldName][row.Index].O; // Original text.
                        string textModify = gridDataJson.CellList[gridName.Value][fieldName][row.Index].T; // User modified text.
                        CellTextSet(gridName, rowIndex, fieldName, textModify, true, text);
                    }
                    else
                    {
                        text = gridDataJson.CellList[gridName.Value][fieldName][row.Index].T; // Original text.
                        CellTextSet(gridName, rowIndex, fieldName, text, false, null);
                    }
                    // ErrorField
                    string errorFieldText = gridDataJson.CellList[gridName.Value][fieldName][row.Index].E;
                    if (errorFieldText != null)
                    {
                        ErrorCellSet(gridName, rowIndex, fieldName, errorFieldText);
                    }
                    // ErrorRow
                    string errorRowText = row.Error;
                    if (errorRowText != null)
                    {
                        ErrorRowSet(gridName, rowIndex, errorRowText);
                    }
                    if (indexEnum == IndexEnum.Index || indexEnum == IndexEnum.New)
                    {
                        cell.CellValueFromText(App, gridName, rowIndex, ref text);
                        App.CellValueFromText(gridName, rowIndex, cell, ref text);
                        object value = UtilDataAccessLayer.ValueFromText(text, cell.PropertyInfo.PropertyType);
                        cell.PropertyInfo.SetValue(resultRow, value);
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
                    LoadJson(new GridName(gridName, true));
                }
                //
                selectGridName = gridDataJson.SelectGridName;
                selectIndex = new Index(gridDataJson.SelectIndex);
                selectFieldName = gridDataJson.SelectFieldName;
            }
        }

        /// <summary>
        /// Returns row's columns.
        /// </summary>
        private static List<GridColumn> TypeRowToGridColumn(App app, GridNameTypeRow gridName, Info info)
        {
            var result = new List<GridColumn>();
            //
            var columnList = UtilDataAccessLayer.ColumnList(gridName.TypeRow);
            double widthPercentTotal = 0;
            bool isLast = false;
            //
            List<Cell> columnIsVisibleList = new List<Cell>();
            foreach (Cell column in columnList)
            {
                bool isVisible = info.ColumnGet(app, gridName, column).IsVisible;
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
                string text = info.ColumnGet(app, gridName, column).Text;
                //
                bool isVisible = columnIsVisibleList.Contains(column);
                if (isVisible)
                {
                    i = i + 1;
                    isLast = column == columnIsVisibleList.LastOrDefault();
                    double widthPercentAvg = Math.Round(((double)100 - widthPercentTotal) / ((double)columnIsVisibleList.Count - (double)i), 2);
                    double widthPercent = widthPercentAvg;
                    column.ColumnWidthPercent(ref widthPercent);
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
                    result.Add(new GridColumn() { FieldName = column.FieldNameCSharp, Text = text, WidthPercent = widthPercent, IsVisible = true });
                }
                else
                {
                    result.Add(new GridColumn() { FieldName = column.FieldNameCSharp, Text = null, IsVisible = false });
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
                    GridColumnInternal gridColumn = ColumnGet(new GridName(gridName, true), gridColumnJson.FieldName);
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
                GridQuery gridQueryJson = gridDataJson.GridQueryList[gridName.Value];
                gridQueryJson.FieldNameOrderBy = gridQuery.FieldNameOrderBy;
                gridQueryJson.IsOrderByDesc = gridQuery.IsOrderByDesc;
            }
        }

        private void SaveJsonSelect(AppJson appJson)
        {
            appJson.GridDataJson.SelectGridName = selectGridName;
            appJson.GridDataJson.SelectIndex = selectIndex?.Value;
            appJson.GridDataJson.SelectFieldName = selectFieldName;
        }

        /// <summary>
        /// Render cell as Button, Html or FileUpload.
        /// </summary>
        private void SaveJsonIsButtonHtmlFileUpload(GridNameTypeRow gridName, Index index, Cell cell, GridCell gridCell, Info info)
        {
            InfoCell infoCell = info.CellGet(App, gridName, cell);
            //
            gridCell.CellEnum = infoCell.CellEnum;
            gridCell.CssClass = infoCell.CssClass.ToHtml();
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
                Type typeRow = TypeRowGet(gridName);
                GridNameTypeRow gridNameTypeRow = new GridNameTypeRow(typeRow, gridName);
                //
                Info info = new Info(App);
                info.ColumnInit(App, gridNameTypeRow);
                //
                gridDataJson.GridQueryList[gridName.Value] = new GridQuery() { GridName = gridName.Value, TypeRow = UtilDataAccessLayer.TypeRowToName(typeRow) };
                // Row
                if (gridDataJson.RowList == null)
                {
                    gridDataJson.RowList = new Dictionary<string, List<GridRow>>();
                }
                gridDataJson.RowList[gridName.Value] = new List<GridRow>();
                // Column
                if (gridDataJson.ColumnList == null)
                {
                    gridDataJson.ColumnList = new Dictionary<string, List<GridColumn>>();
                }
                gridDataJson.ColumnList[gridName.Value] = TypeRowToGridColumn(App, gridNameTypeRow, info);
                // Cell
                if (gridDataJson.CellList == null)
                {
                    gridDataJson.CellList = new Dictionary<string, Dictionary<string, Dictionary<string, GridCell>>>();
                }
                gridDataJson.CellList[gridName.Value] = new Dictionary<string, Dictionary<string, GridCell>>();
                //
                PropertyInfo[] propertyInfoList = null;
                foreach (Index index in rowList[gridName].Keys)
                {
                    GridRowInternal gridRow = rowList[gridName][index];
                    Row row = gridRow.Row;
                    if (row == null)
                    {
                        row = gridRow.RowNew;
                    }
                    info.CellInit(App, gridNameTypeRow, row, index);
                    string errorRow = ErrorRowGet(gridName, index);
                    GridRow gridRowJson = new GridRow() { Index = index.Value, IsSelect = gridRow.IsSelect, IsClick = gridRow.IsClick, Error = errorRow };
                    gridRowJson.IsFilter = index.Enum == IndexEnum.Filter;
                    gridDataJson.RowList[gridName.Value].Add(gridRowJson);
                    if (propertyInfoList == null && typeRow != null)
                    {
                        propertyInfoList = typeRow.GetTypeInfo().GetProperties();
                    }
                    if (propertyInfoList != null)
                    {
                        foreach (Cell cell in ColumnList(gridName))
                        {
                            cell.Constructor(row);
                            //
                            string fieldName = cell.FieldNameCSharp;
                            object value = null;
                            if (row != null)
                            {
                                value = cell.PropertyInfo.GetValue(row);
                            }
                            string textJson = UtilDataAccessLayer.ValueToText(value, cell.TypeField);
                            InfoCell infoCell = info.CellGet(App, gridNameTypeRow, cell);
                            if (infoCell.CellEnum == GridCellEnum.Button && textJson == null)
                            {
                                textJson = "Button"; // Default text for button.
                            }
                            cell.CellValueToText(App, gridName, index, ref textJson); // Override text.
                            App.CellValueToText(gridName, index, cell, ref textJson); // Override text generic.
                            GridCellInternal gridCellInternal = CellGet(gridName, index, fieldName);
                            if (!gridDataJson.CellList[gridName.Value].ContainsKey(fieldName))
                            {
                                gridDataJson.CellList[gridName.Value][fieldName] = new Dictionary<string, GridCell>();
                            }
                            string errorCell = ErrorCellGet(gridName, index, fieldName);
                            GridCell gridCellJson = new GridCell();
                            gridDataJson.CellList[gridName.Value][fieldName][index.Value] = gridCellJson;
                            //
                            SaveJsonIsButtonHtmlFileUpload(gridNameTypeRow, index, cell, gridCellJson, info);
                            //
                            if (gridCellInternal.IsOriginal == false)
                            {
                                gridCellJson.T = textJson;
                            }
                            else
                            {
                                gridCellJson.O = textJson;
                                gridCellJson.T = gridCellInternal.Text; // Never overwrite user entered text.
                                gridCellJson.IsO = true;
                            }
                            gridCellJson.PlaceHolder = infoCell.PlaceHolder;
                            gridCellJson.IsClick = gridCellInternal.IsClick;
                            gridCellJson.IsModify = gridCellInternal.IsModify;
                            gridCellJson.E = errorCell;
                            gridCellJson.IsLookup = gridCellInternal.IsLookup;
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
                    gridDataJson.ColumnList[gridName.Value] = new List<GridColumn>(); // Remove GridColumn (header).
                    gridDataJson.RowList[gridName.Value] = new List<GridRow>();
                }
            }
            SaveJsonColumn(appJson);
            SaveJsonQuery(appJson);
            SaveJsonSelect(appJson);
        }
    }
}
