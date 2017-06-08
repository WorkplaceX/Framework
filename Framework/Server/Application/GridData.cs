namespace Framework.Server.Application
{
    using Framework.Server.Application.Json;
    using Framework.Server.DataAccessLayer;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Reflection;

    public class GridRowServer
    {
        public Row Row;

        public Row RowNew;

        /// <summary>
        /// Filter with parsed and valid parameters.
        /// </summary>
        public Row RowFilter; // List<Row> for multiple parameters.

        /// <summary>
        /// Gets or sets error attached to row.
        /// </summary>
        public string Error;

        internal int IsSelect;

        internal bool IsClick;
    }

    internal class GridCellServer
    {
        /// <summary>
        /// Gets or sets user modified text.
        /// </summary>
        public string Text;

        /// <summary>
        /// Gets or sets error attached to cell.
        /// </summary>
        public string Error;

        public bool IsSelect;

        public bool IsModify;

        public bool IsClick;
    }

    internal class GridColumnServer
    {
        public bool IsClick;
    }

    internal class GridQueryServer
    {
        public string FieldNameOrderBy;

        public bool IsOrderByDesc;
    }

    public class GridDataServer
    {
        /// <summary>
        /// (GridName, TypeRow)
        /// </summary>
        private Dictionary<string, Type> TypeRowList = new Dictionary<string, Type>();

        public Type TypeRowGet(string gridName)
        {
            Type result;
            TypeRowList.TryGetValue(gridName, out result);
            return result;
        }

        private Dictionary<string, GridQueryServer> QueryList = new Dictionary<string, GridQueryServer>();

        private GridQueryServer QueryGet(string gridName)
        {
            if (!QueryList.ContainsKey(gridName))
            {
                QueryList[gridName] = new GridQueryServer();
            }
            return QueryList[gridName];
        }

        private Dictionary<string, Dictionary<string, GridColumnServer>> ColumnList = new Dictionary<string, Dictionary<string, GridColumnServer>>();

        private GridColumnServer ColumnGet(string gridName, string fieldName)
        {
            if (!ColumnList.ContainsKey(gridName))
            {
                ColumnList[gridName] = new Dictionary<string, GridColumnServer>();
            }
            if (!ColumnList[gridName].ContainsKey(fieldName))
            {
                ColumnList[gridName][fieldName] = new GridColumnServer();
            }
            //
            return ColumnList[gridName][fieldName];
        }

        /// <summary>
        /// (GridName, Index). Original row.
        /// </summary>
        private Dictionary<string, Dictionary<string, GridRowServer>> RowList = new Dictionary<string, Dictionary<string, GridRowServer>>();

        public GridRowServer RowGet(string gridName, string index)
        {
            GridRowServer result = null;
            if (RowList.ContainsKey(gridName))
            {
                if (RowList[gridName].ContainsKey(index))
                {
                    result = RowList[gridName][index];
                }
            }
            return result;
        }

        private void RowSet(string gridName, string index, GridRowServer gridRowServer)
        {
            if (!RowList.ContainsKey(gridName))
            {
                RowList[gridName] = new Dictionary<string, GridRowServer>();
            }
            RowList[gridName][index] = gridRowServer;
        }

        /// <summary>
        /// (GridName, Index, FieldName, Text).
        /// </summary>
        private Dictionary<string, Dictionary<string, Dictionary<string, GridCellServer>>> CellList = new Dictionary<string, Dictionary<string, Dictionary<string, GridCellServer>>>();

        private GridCellServer CellGet(string gridName, string index, string fieldName)
        {
            GridCellServer result = null;
            if (CellList.ContainsKey(gridName))
            {
                if (CellList[gridName].ContainsKey(index))
                {
                    if (CellList[gridName][index].ContainsKey(fieldName))
                    {
                        result = CellList[gridName][index][fieldName];
                    }
                }
            }
            if (result == null)
            {
                result = new GridCellServer();
                if (!CellList.ContainsKey(gridName))
                {
                    CellList[gridName] = new Dictionary<string, Dictionary<string, GridCellServer>>();
                }
                if (!CellList[gridName].ContainsKey(index))
                {
                    CellList[gridName][index] = new Dictionary<string, GridCellServer>();
                }
                CellList[gridName][index][fieldName] = result;
            }
            return result;
        }

        /// <summary>
        /// Returns error attached to data row.
        /// </summary>
        private string ErrorRowGet(string gridName, string index)
        {
            return RowGet(gridName, index).Error;
        }

        /// <summary>
        /// Set error on data row.
        /// </summary>
        private void ErrorRowSet(string gridName, string index, string text)
        {
            RowGet(gridName, index).Error = text;
        }

        /// <summary>
        /// Gets user entered text.
        /// </summary>
        /// <returns>If null, user has not changed text.</returns>
        private string TextGet(string gridName, string index, string fieldName)
        {
            return CellGet(gridName, index, fieldName).Text;
        }

        /// <summary>
        /// Sets user entered text.
        /// </summary>
        /// <param name="text">If null, user has not changed text.</param>
        private void TextSet(string gridName, string index, string fieldName, string text)
        {
            CellGet(gridName, index, fieldName).Text = text;
        }

        /// <summary>
        /// Clear all user modified text for row.
        /// </summary>
        private void TextClear(string gridName, string index)
        {
            if (CellList.ContainsKey(gridName))
            {
                if (CellList[gridName].ContainsKey(index))
                {
                    foreach (GridCellServer gridCellServer in CellList[gridName][index].Values)
                    {
                        gridCellServer.Text = null;
                    }
                }
            }
        }

        private string ErrorCellGet(string gridName, string index, string fieldName)
        {
            return CellGet(gridName, index, fieldName).Error;
        }

        private void ErrorCellSet(string gridName, string index, string fieldName, string text)
        {
            CellGet(gridName, index, fieldName).Error = text;
        }

        /// <summary>
        /// Returns true, if data row contains text parse error.
        /// </summary>
        private bool IsErrorRowCell(string gridName, string index)
        {
            bool result = false;
            if (CellList.ContainsKey(gridName))
            {
                if (CellList[gridName].ContainsKey(index))
                {
                    foreach (string fieldName in CellList[gridName][index].Keys)
                    {
                        if (CellList[gridName][index][fieldName].Error != null)
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
        private bool IsModifyRowCell(string gridName, string index)
        {
            bool result = false;
            if (CellList.ContainsKey(gridName))
            {
                if (CellList[gridName].ContainsKey(index))
                {
                    foreach (string fieldName in CellList[gridName][index].Keys)
                    {
                        if (CellList[gridName][index][fieldName].IsModify)
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
        /// Load data from database.
        /// </summary>
        public void LoadDatabase(string gridName, List<Filter> filterList, string fieldNameOrderBy, bool isOrderByDesc, Type typeRow)
        {
            TypeRowList[gridName] = typeRow;
            List<Row> rowList = DataAccessLayer.Util.Select(typeRow, filterList, fieldNameOrderBy, isOrderByDesc, 0, 15);
            LoadRow(gridName, typeRow, rowList);
        }

        private void LoadDatabase(string gridName, out List<Filter> filterList)
        {
            filterList = new List<Filter>();
            Row row = RowGet(gridName, Util.IndexEnumToString(IndexEnum.Filter)).RowFilter; // Data row with parsed filter values.
            foreach (string fieldName in ColumnList[gridName].Keys)
            {
                string text = TextGet(gridName, Util.IndexEnumToString(IndexEnum.Filter), fieldName);
                if (text == "")
                {
                    text = null;
                }
                if (text != null) // Use filter only when text set.
                {
                    object value = row.GetType().GetProperty(fieldName).GetValue(row);
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

        /// <summary>
        /// Load data from database with current grid filter and current sorting.
        /// </summary>
        public void LoadDatabase(string gridName)
        {
            if (!IsErrorRowCell(gridName, Util.IndexEnumToString(IndexEnum.Filter))) // Do not reload data grid if there is text parse error in filter.
            {
                string fieldNameOrderBy = QueryList[gridName].FieldNameOrderBy;
                bool isOrderByDesc = QueryList[gridName].IsOrderByDesc;
                Type typeRow = TypeRowGet(gridName);
                List<Filter> filterList;
                LoadDatabase(gridName, out filterList);
                LoadDatabase(gridName, filterList, fieldNameOrderBy, isOrderByDesc, typeRow);
            }
        }

        /// <summary>
        /// Load data from list.
        /// </summary>
        public void LoadRow(string gridName, Type typeRow, List<Row> rowList)
        {
            if (rowList == null)
            {
                rowList = new List<Row>();
            }
            foreach (Row row in rowList)
            {
                Framework.Util.Assert(row.GetType() == typeRow);
            }
            //
            Dictionary<string, GridCellServer> cellListFilter = null;
            if (CellList.ContainsKey(gridName))
            {
                CellList[gridName].TryGetValue(Util.IndexEnumToString(IndexEnum.Filter), out cellListFilter); // Save filter user text.
            }
            CellList.Remove(gridName); // Clear user modified text and attached errors.
            RowList[gridName] = new Dictionary<string, GridRowServer>(); // Clear data
            TypeRowList[gridName] = typeRow;
            //
            RowFilterAdd(gridName);
            for (int index = 0; index < rowList.Count; index++)
            {
                RowSet(gridName, index.ToString(), new GridRowServer() { Row = rowList[index], RowNew = null });
            }
            RowNewAdd(gridName);
            //
            if (cellListFilter != null)
            {
                CellList[gridName] = new Dictionary<string, Dictionary<string, GridCellServer>>();
                CellList[gridName][Util.IndexEnumToString(IndexEnum.Filter)] = cellListFilter; // Load back filter user text.
            }
        }

        /// <summary>
        /// Add data grid filter row.
        /// </summary>
        private void RowFilterAdd(string gridName)
        {
            RowSet(gridName, Util.IndexEnumToString(IndexEnum.Filter), new GridRowServer() { Row = null, RowNew = null });
        }

        /// <summary>
        /// Add data row of enum New to RowList.
        /// </summary>
        private void RowNewAdd(string gridName)
        {
            // (Index)
            Dictionary<string, GridRowServer> rowListCopy = RowList[gridName];
            RowList[gridName] = new Dictionary<string, GridRowServer>();
            // Filter
            foreach (string index in rowListCopy.Keys)
            {
                if (Util.IndexToIndexEnum(index) == IndexEnum.Filter)
                {
                    RowSet(gridName, index, rowListCopy[index]);
                    break;
                }
            }
            // Index
            int indexInt = 0;
            foreach (string index in rowListCopy.Keys)
            {
                IndexEnum indexEnum = Util.IndexToIndexEnum(index);
                if (indexEnum == IndexEnum.Index || indexEnum == IndexEnum.New)
                {
                    RowSet(gridName, indexInt.ToString(), rowListCopy[index]); // New becomes Index
                    indexInt += 1;
                }
            }
            // New
            RowSet(gridName, Util.IndexEnumToString(IndexEnum.New), new GridRowServer() { Row = null, RowNew = null }); // New row
            // Total
            foreach (string index in rowListCopy.Keys)
            {
                if (Util.IndexToIndexEnum(index) == IndexEnum.Total)
                {
                    RowSet(gridName, index, rowListCopy[index]);
                    break;
                }
            }
        }

        /// <summary>
        /// Save data to database.
        /// </summary>
        public void SaveDatabase()
        {
            foreach (string gridName in RowList.Keys.ToArray())
            {
                foreach (string index in RowList[gridName].Keys.ToArray())
                {
                    IndexEnum indexEnum = Util.IndexToIndexEnum(index);
                    if (indexEnum == IndexEnum.Index || indexEnum == IndexEnum.New) // Exclude Filter and Total.
                    {
                        if (!IsErrorRowCell(gridName, index)) // No save if data row has text parse error!
                        {
                            if (IsModifyRowCell(gridName, index)) // Only save row if user modified row on latest request.
                            {
                                var row = RowList[gridName][index];
                                if (row.Row != null && row.RowNew != null) // Database Update
                                {
                                    try
                                    {
                                        DataAccessLayer.Util.Update(row.Row, row.RowNew);
                                        ErrorRowSet(gridName, index, null);
                                        row.Row = row.RowNew;
                                        TextClear(gridName, index);
                                    }
                                    catch (Exception exception)
                                    {
                                        ErrorRowSet(gridName, index, Framework.Util.ExceptionToText(exception));
                                    }
                                }
                                if (row.Row == null && row.RowNew != null) // Database Insert
                                {
                                    try
                                    {
                                        DataAccessLayer.Util.Insert(row.RowNew);
                                        ErrorRowSet(gridName, index, null);
                                        row.Row = row.RowNew;
                                        TextClear(gridName, index);
                                        RowNewAdd(gridName); // Make "New" to "Index" and add "New"
                                    }
                                    catch (Exception exception)
                                    {
                                        ErrorRowSet(gridName, index, Framework.Util.ExceptionToText(exception));
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
        public void TextParse()
        {
            foreach (string gridName in RowList.Keys)
            {
                foreach (string index in RowList[gridName].Keys)
                {
                    if (IsModifyRowCell(gridName, index))
                    {
                        Type typeRow = TypeRowGet(gridName);
                        var row = RowGet(gridName, index);
                        if (row.Row != null)
                        {
                            Framework.Util.Assert(row.Row.GetType() == typeRow);
                        }
                        IndexEnum indexEnum = Util.IndexToIndexEnum(index);
                        Row rowWrite;
                        switch (indexEnum)
                        {
                            case IndexEnum.Index:
                                rowWrite = DataAccessLayer.Util.RowClone(row.Row);
                                row.RowNew = rowWrite;
                                break;
                            case IndexEnum.New:
                                rowWrite = DataAccessLayer.Util.RowCreate(typeRow);
                                row.RowNew = rowWrite;
                                break;
                            case IndexEnum.Filter:
                                rowWrite = DataAccessLayer.Util.RowCreate(typeRow);
                                row.RowFilter = rowWrite;
                                break;
                            default:
                                throw new Exception("Enum unknown!");
                        }
                        foreach (string fieldName in CellList[gridName][index].Keys)
                        {
                            string text = TextGet(gridName, index, fieldName);
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
                                        value = DataAccessLayer.Util.ValueFromText(text, rowWrite.GetType().GetProperty(fieldName).PropertyType); // Parse text.
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
        private void LoadJsonQuery(ApplicationJson applicationJson)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            //
            foreach (string gridName in gridDataJson.GridQueryList.Keys)
            {
                GridQuery gridQuery = gridDataJson.GridQueryList[gridName];
                GridQueryServer gridQueryServer = QueryGet(gridName);
                gridQueryServer.FieldNameOrderBy = gridQuery.FieldNameOrderBy;
                gridQueryServer.IsOrderByDesc = gridQuery.IsOrderByDesc;
            }
        }

        /// <summary>
        /// Load GridColumn data from json.
        /// </summary>
        private void LoadJsonColumn(ApplicationJson applicationJson)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            //
            foreach (string gridName in gridDataJson.ColumnList.Keys)
            {
                foreach (GridColumn gridColumn in gridDataJson.ColumnList[gridName])
                {
                    GridColumnServer gridColumnServer = ColumnGet(gridName, gridColumn.FieldName);
                    gridColumnServer.IsClick = gridColumn.IsClick;
                }
            }
        }

        /// <summary>
        /// Load data from GridDataJson to GridDataServer.
        /// </summary>
        public void LoadJson(ApplicationJson applicationJson, string gridName, Type typeInAssembly)
        {
            LoadJsonQuery(applicationJson);
            LoadJsonColumn(applicationJson);
            //
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            //
            string typeRowName = gridDataJson.GridQueryList[gridName].TypeRowName;
            Type typeRow = DataAccessLayer.Util.TypeRowFromName(typeRowName, typeInAssembly);
            TypeRowList[gridName] = typeRow;
            //
            foreach (GridRow row in gridDataJson.RowList[gridName])
            {
                IndexEnum indexEnum = Util.IndexToIndexEnum(row.Index);
                Row resultRow = null;
                if (indexEnum == IndexEnum.Index)
                {
                    resultRow = (Row)Activator.CreateInstance(typeRow);
                }
                GridRowServer gridRowServer = new GridRowServer() { Row = resultRow, IsSelect = row.IsSelect, IsClick = row.IsClick };
                RowSet(gridName, row.Index, gridRowServer);
                foreach (var column in gridDataJson.ColumnList[gridName])
                {
                    CellGet(gridName, row.Index, column.FieldName).IsSelect = gridDataJson.CellList[gridName][column.FieldName][row.Index].IsSelect;
                    CellGet(gridName, row.Index, column.FieldName).IsClick = gridDataJson.CellList[gridName][column.FieldName][row.Index].IsClick;
                    CellGet(gridName, row.Index, column.FieldName).IsModify = gridDataJson.CellList[gridName][column.FieldName][row.Index].IsModify;
                    string text;
                    if (gridDataJson.CellList[gridName][column.FieldName][row.Index].IsO)
                    {
                        text = gridDataJson.CellList[gridName][column.FieldName][row.Index].O; // Original text.
                        string textModify = gridDataJson.CellList[gridName][column.FieldName][row.Index].T; // User modified text.
                        TextSet(gridName, row.Index, column.FieldName, textModify);
                    }
                    else
                    {
                        text = gridDataJson.CellList[gridName][column.FieldName][row.Index].T; // Original text.
                    }
                    // ErrorField
                    string errorFieldText = gridDataJson.CellList[gridName][column.FieldName][row.Index].E;
                    if (errorFieldText != null)
                    {
                        ErrorCellSet(gridName, row.Index, column.FieldName, errorFieldText);
                    }
                    // ErrorRow
                    string errorRowText = row.Error;
                    if (errorRowText != null)
                    {
                        ErrorRowSet(gridName, row.Index, errorRowText);
                    }
                    if (indexEnum == IndexEnum.Index)
                    {
                        PropertyInfo propertyInfo = typeRow.GetProperty(column.FieldName);
                        object value = DataAccessLayer.Util.ValueFromText(text, propertyInfo.PropertyType);
                        propertyInfo.SetValue(resultRow, value);
                    }
                }
            }
        }

        /// <summary>
        /// Load data from GridDataJson to GridDataServer.
        /// </summary>
        public void LoadJson(ApplicationJson applicationJson, Type typeInAssembly)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            //
            foreach (string gridName in gridDataJson.GridQueryList.Keys)
            {
                LoadJson(applicationJson, gridName, typeInAssembly);
            }
        }

        /// <summary>
        /// Returns row's columns.
        /// </summary>
        private static List<GridColumn> TypeRowToGridColumn(Type typeRow)
        {
            var result = new List<GridColumn>();
            //
            var cellList = Framework.Server.DataAccessLayer.Util.ColumnList(typeRow);
            double widthPercentTotal = 0;
            bool isLast = false;
            for (int i = 0; i < cellList.Count; i++)
            {
                // Text
                string text = cellList[i].FieldNameSql;
                cellList[i].ColumnText(ref text);
                // WidthPercent
                isLast = i == cellList.Count;
                double widthPercentAvg = Math.Round(((double)100 - widthPercentTotal) / ((double)cellList.Count - (double)i), 2);
                double widthPercent = widthPercentAvg;
                cellList[i].ColumnWidthPercent(ref widthPercent);
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
                result.Add(new GridColumn() { FieldName = cellList[i].FieldNameSql, Text = text, WidthPercent = widthPercent });
            }
            return result;
        }

        /// <summary>
        /// Save column state to Json.
        /// </summary>
        private void SaveJsonColumn(ApplicationJson applicationJson)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            //
            foreach (string gridName in gridDataJson.ColumnList.Keys)
            {
                foreach (GridColumn gridColumn in gridDataJson.ColumnList[gridName])
                {
                    GridColumnServer gridColumnServer = ColumnGet(gridName, gridColumn.FieldName);
                    gridColumn.IsClick = gridColumnServer.IsClick;
                }
            }
        }

        /// <summary>
        /// Save Query back to Json.
        /// </summary>
        private void SaveJsonQuery(ApplicationJson applicationJson)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            //
            foreach (string gridName in QueryList.Keys)
            {
                GridQueryServer gridQueryServer = QueryList[gridName];
                GridQuery gridQuery = gridDataJson.GridQueryList[gridName];
                gridQuery.FieldNameOrderBy = gridQueryServer.FieldNameOrderBy;
                gridQuery.IsOrderByDesc = gridQueryServer.IsOrderByDesc;
            }
        }

        /// <summary>
        /// Copy data from class GridDataServer to class GridDataJson.
        /// </summary>
        public void SaveJson(ApplicationJson applicationJson)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            //
            if (gridDataJson.GridQueryList == null)
            {
                gridDataJson.GridQueryList = new Dictionary<string, GridQuery>();
            }
            //
            foreach (string gridName in RowList.Keys)
            {
                Type typeRow = TypeRowList[gridName];
                gridDataJson.GridQueryList[gridName] = new GridQuery() { GridName = gridName, TypeRowName = DataAccessLayer.Util.TypeRowToName(typeRow) };
                // Row
                if (gridDataJson.RowList == null)
                {
                    gridDataJson.RowList = new Dictionary<string, List<GridRow>>();
                }
                gridDataJson.RowList[gridName] = new List<GridRow>();
                // Column
                if (gridDataJson.ColumnList == null)
                {
                    gridDataJson.ColumnList = new Dictionary<string, List<GridColumn>>();
                }
                gridDataJson.ColumnList[gridName] = TypeRowToGridColumn(typeRow);
                // Cell
                if (gridDataJson.CellList == null)
                {
                    gridDataJson.CellList = new Dictionary<string, Dictionary<string, Dictionary<string, GridCell>>>();
                }
                gridDataJson.CellList[gridName] = new Dictionary<string, Dictionary<string, GridCell>>();
                //
                PropertyInfo[] propertyInfoList = null;
                foreach (string index in RowList[gridName].Keys)
                {
                    GridRowServer gridRowServer = RowList[gridName][index];
                    string errorRow = ErrorRowGet(gridName, index);
                    GridRow gridRow = new GridRow() { Index = index, IsSelect = gridRowServer.IsSelect, IsClick = gridRowServer.IsClick, Error = errorRow };
                    gridDataJson.RowList[gridName].Add(gridRow);
                    if (propertyInfoList == null && typeRow != null)
                    {
                        propertyInfoList = typeRow.GetTypeInfo().GetProperties();
                    }
                    if (propertyInfoList != null)
                    {
                        foreach (PropertyInfo propertyInfo in propertyInfoList)
                        {
                            string fieldName = propertyInfo.Name;
                            object value = null;
                            if (gridRowServer.Row != null)
                            {
                                value = propertyInfo.GetValue(gridRowServer.Row);
                            }
                            string textJson = DataAccessLayer.Util.ValueToText(value);
                            string text = TextGet(gridName, index, fieldName);
                            GridCellServer gridCellServer = CellGet(gridName, index, fieldName);
                            if (!gridDataJson.CellList[gridName].ContainsKey(fieldName))
                            {
                                gridDataJson.CellList[gridName][fieldName] = new Dictionary<string, GridCell>();
                            }
                            string errorCell = ErrorCellGet(gridName, index, fieldName);
                            GridCell gridCell = new GridCell() { IsSelect = gridCellServer.IsSelect, IsClick = gridCellServer.IsClick, IsModify = gridCellServer.IsModify, E = errorCell };
                            gridDataJson.CellList[gridName][fieldName][index] = gridCell;
                            if (text == null)
                            {
                                gridCell.T = textJson;
                            }
                            else
                            {
                                gridCell.O = textJson;
                                gridCell.T = text;
                                gridCell.IsO = true;
                            }
                        }
                    }
                }
            }
            SaveJsonColumn(applicationJson);
            SaveJsonQuery(applicationJson);
        }
    }
}
