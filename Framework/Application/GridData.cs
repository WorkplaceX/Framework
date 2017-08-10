namespace Framework.Application
{
    using Framework.DataAccessLayer;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Reflection;
    using Framework.Component;
    using Database.dbo;

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

        internal int IsSelect;

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

        public bool IsSelect;

        public bool IsModify;

        public bool IsClick;
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
        /// <summary>
        /// Returns user modified text. If null, user has not changed text.
        /// </summary>
        public string CellText(string gridName, string index, string fieldName)
        {
            string result = null;
            GridCellInternal cell = CellGet(gridName, index, fieldName);
            if (cell != null)
            {
                return cell.Text;
            }
            return result;
        }

        /// <summary>
        /// Returns list of loaded GridName.
        /// </summary>
        public List<string> GridNameList()
        {
            List<string> result = new List<string>(queryList.Keys);
            return result;
        }

        /// <summary>
        /// Returns column definitions.
        /// </summary>
        public List<Cell> ColumnList(string gridName)
        {
            Type typeRow = TypeRowGet(gridName);
            return UtilDataAccessLayer.ColumnList(typeRow);
        }

        /// <summary>
        /// Returns list of loaded row index.
        /// </summary>
        public List<string> IndexList(string gridName)
        {
            return new List<string>(rowList[gridName].Keys);
        }

        public Type TypeRow(string gridName)
        {
            return TypeRowGet(gridName);
        }

        /// <summary>
        /// (GridName, TypeRow)
        /// </summary>
        private Dictionary<string, Type> typeRowList = new Dictionary<string, Type>();

        private Type TypeRowGet(string gridName)
        {
            Type result;
            typeRowList.TryGetValue(gridName, out result);
            return result;
        }

        private void TypeRowSet(string gridName, Type typeRow)
        {
            typeRowList[gridName] = typeRow;
            if (typeRow == null)
            {
                typeRowList.Remove(gridName);
            }
        }

        /// <summary>
        /// Returns data row.
        /// </summary>
        public Row Row(string gridName, string index)
        {
            Row result = null;
            var row = RowGet(gridName, index);
            if (row != null)
            {
                result = row.Row;
            }
            return result;
        }

        /// <summary>
        /// (GridName, GridQuery).
        /// </summary>
        private Dictionary<string, GridQueryInternal> queryList = new Dictionary<string, GridQueryInternal>();

        private GridQueryInternal QueryGet(string gridName)
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
        private Dictionary<string, Dictionary<string, GridColumnInternal>> columnList = new Dictionary<string, Dictionary<string, GridColumnInternal>>();

        private GridColumnInternal ColumnGet(string gridName, string fieldName)
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
        private Dictionary<string, Dictionary<string, GridRowInternal>> rowList = new Dictionary<string, Dictionary<string, GridRowInternal>>();

        private GridRowInternal RowGet(string gridName, string index)
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

        private void RowSet(string gridName, string index, GridRowInternal gridRow)
        {
            if (!rowList.ContainsKey(gridName))
            {
                rowList[gridName] = new Dictionary<string, GridRowInternal>();
            }
            rowList[gridName][index] = gridRow;
        }

        /// <summary>
        /// (GridName, Index, FieldName, Text).
        /// </summary>
        private Dictionary<string, Dictionary<string, Dictionary<string, GridCellInternal>>> cellList = new Dictionary<string, Dictionary<string, Dictionary<string, GridCellInternal>>>();

        private GridCellInternal CellGet(string gridName, string index, string fieldName)
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
                    cellList[gridName] = new Dictionary<string, Dictionary<string, GridCellInternal>>();
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
        private string CellTextGet(string gridName, string index, string fieldName)
        {
            return CellGet(gridName, index, fieldName).Text;
        }

        /// <summary>
        /// Sets user entered text.
        /// </summary>
        /// <param name="text">If null, user has not changed text.</param>
        private void CellTextSet(string gridName, string index, string fieldName, string text)
        {
            CellGet(gridName, index, fieldName).Text = text;
        }

        /// <summary>
        /// Clear all user modified text for row.
        /// </summary>
        private void TextClear(string gridName, string index)
        {
            if (cellList.ContainsKey(gridName))
            {
                if (cellList[gridName].ContainsKey(index))
                {
                    foreach (GridCellInternal gridCell in cellList[gridName][index].Values)
                    {
                        gridCell.Text = null;
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
        private bool IsModifyRowCell(string gridName, string index)
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
        public void LoadDatabase(string gridName, List<Filter> filterList, string fieldNameOrderBy, bool isOrderByDesc, Type typeRow)
        {
            TypeRowSet(gridName, typeRow);
            List<Row> rowList = UtilDataAccessLayer.Select(typeRow, filterList, fieldNameOrderBy, isOrderByDesc, 0, 15);
            LoadRow(gridName, typeRow, rowList);
        }

        /// <summary>
        /// Load data from Sql database.
        /// </summary>
        public void LoadDatabase(string gridName, Type typeRow)
        {
            LoadDatabase(gridName, null, null, false, typeRow);
        }

        /// <summary>
        /// Load data from Sql database.
        /// </summary>
        public void LoadDatabase<TRow>(string gridName) where TRow : Row
        {
            LoadDatabase(gridName, typeof(TRow));
        }

        private void LoadDatabase(string gridName, out List<Filter> filterList)
        {
            Type typeRow = TypeRowGet(gridName);
            filterList = new List<Filter>();
            Row row = RowGet(gridName, UtilApplication.IndexEnumToText(IndexEnum.Filter)).RowFilter; // Data row with parsed filter values.
            
            foreach (Cell column in UtilDataAccessLayer.ColumnList(typeRow))
            {
                string fieldName = column.FieldNameCSharp;
                string text = CellTextGet(gridName, UtilApplication.IndexEnumToText(IndexEnum.Filter), fieldName);
                if (text == "")
                {
                    text = null;
                }
                if (text != null) // Use filter only when text set.
                {
                    if (column.FieldNameSql != null) // Do not filter on calculated column.
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
        }

        /// <summary>
        /// Reload data from database with current grid filter and current sorting.
        /// </summary>
        public void LoadDatabase(string gridName)
        {
            if (!IsErrorRowCell(gridName, UtilApplication.IndexEnumToText(IndexEnum.Filter))) // Do not reload data grid if there is text parse error in filter.
            {
                if (queryList.ContainsKey(gridName)) // Reload
                {
                    string fieldNameOrderBy = queryList[gridName].FieldNameOrderBy;
                    bool isOrderByDesc = queryList[gridName].IsOrderByDesc;
                    Type typeRow = TypeRowGet(gridName);
                    List<Filter> filterList;
                    LoadDatabase(gridName, out filterList);
                    LoadDatabase(gridName, filterList, fieldNameOrderBy, isOrderByDesc, typeRow);
                }
            }
        }

        /// <summary>
        /// Load data from list into data grid.
        /// </summary>
        public void LoadRow(string gridName, Type typeRow, List<Row> rowList)
        {
            if (typeRow == null || rowList == null)
            {
                TypeRowSet(gridName, typeRow);
                cellList.Remove(gridName);
                this.rowList.Remove(gridName);
            }
            else
            {
                foreach (Row row in rowList)
                {
                    UtilFramework.Assert(row.GetType() == typeRow);
                }
                //
                Dictionary<string, GridCellInternal> cellListFilter = null;
                if (cellList.ContainsKey(gridName))
                {
                    cellList[gridName].TryGetValue(UtilApplication.IndexEnumToText(IndexEnum.Filter), out cellListFilter); // Save filter user text.
                }
                cellList.Remove(gridName); // Clear user modified text and attached errors.
                this.rowList[gridName] = new Dictionary<string, GridRowInternal>(); // Clear data
                TypeRowSet(gridName, typeRow);
                //
                RowFilterAdd(gridName);
                for (int index = 0; index < rowList.Count; index++)
                {
                    RowSet(gridName, index.ToString(), new GridRowInternal() { Row = rowList[index], RowNew = null });
                }
                RowNewAdd(gridName);
                //
                if (cellListFilter != null)
                {
                    cellList[gridName] = new Dictionary<string, Dictionary<string, GridCellInternal>>();
                    cellList[gridName][UtilApplication.IndexEnumToText(IndexEnum.Filter)] = cellListFilter; // Load back filter user text.
                }
            }
        }

        /// <summary>
        /// Load data from single row into data grid.
        /// </summary>
        public void LoadRow(string gridName, Row row)
        {
            if (row == null)
            {
                LoadRow(gridName, null, null); // Remove data grid.
            }
            else
            {
                List<Row> rowList = new List<Row>();
                rowList.Add(row);
                LoadRow(gridName, row.GetType(), rowList);
            }
        }

        /// <summary>
        /// Add data grid filter row.
        /// </summary>
        private void RowFilterAdd(string gridName)
        {
            RowSet(gridName, UtilApplication.IndexEnumToText(IndexEnum.Filter), new GridRowInternal() { Row = null, RowNew = null });
        }

        /// <summary>
        /// Add data row of enum New to RowList.
        /// </summary>
        private void RowNewAdd(string gridName)
        {
            // (Index)
            Dictionary<string, GridRowInternal> rowListCopy = rowList[gridName];
            rowList[gridName] = new Dictionary<string, GridRowInternal>();
            // Filter
            foreach (string index in rowListCopy.Keys)
            {
                if (UtilApplication.IndexEnumFromText(index) == IndexEnum.Filter)
                {
                    RowSet(gridName, index, rowListCopy[index]);
                    break;
                }
            }
            // Index
            int indexInt = 0;
            foreach (string index in rowListCopy.Keys)
            {
                IndexEnum indexEnum = UtilApplication.IndexEnumFromText(index);
                if (indexEnum == IndexEnum.Index || indexEnum == IndexEnum.New)
                {
                    RowSet(gridName, indexInt.ToString(), rowListCopy[index]); // New becomes Index
                    indexInt += 1;
                }
            }
            // New
            RowSet(gridName, UtilApplication.IndexEnumToText(IndexEnum.New), new GridRowInternal() { Row = null, RowNew = null }); // New row
            // Total
            foreach (string index in rowListCopy.Keys)
            {
                if (UtilApplication.IndexEnumFromText(index) == IndexEnum.Total)
                {
                    RowSet(gridName, index, rowListCopy[index]);
                    break;
                }
            }
        }

        /// <summary>
        /// Save data to sql database.
        /// </summary>
        public void SaveDatabase(App app)
        {
            foreach (string gridName in rowList.Keys.ToArray())
            {
                foreach (string index in rowList[gridName].Keys.ToArray())
                {
                    IndexEnum indexEnum = UtilApplication.IndexEnumFromText(index);
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
                                        Row rowRefresh = null;
                                        row.RowNew.Update(app, row.Row, row.RowNew, ref rowRefresh);
                                        if (rowRefresh != null)
                                        {
                                            UtilDataAccessLayer.RowCopy(rowRefresh, row.RowNew);
                                        }
                                        row.RowNew.Select();
                                        ErrorRowSet(gridName, index, null);
                                        row.Row = row.RowNew;
                                        TextClear(gridName, index);
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
                                        Row rowRefresh = null;
                                        row.RowNew.Insert(app, ref rowRefresh);
                                        if (rowRefresh != null)
                                        {
                                            UtilDataAccessLayer.RowCopy(rowRefresh, row.RowNew);
                                        }
                                        row.RowNew.Select();
                                        ErrorRowSet(gridName, index, null);
                                        row.Row = row.RowNew;
                                        TextClear(gridName, index);
                                        RowNewAdd(gridName); // Make "New" to "Index" and add "New"
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
        public void TextParse(App app)
        {
            foreach (string gridName in rowList.Keys)
            {
                foreach (string index in rowList[gridName].Keys)
                {
                    if (IsModifyRowCell(gridName, index))
                    {
                        Type typeRow = TypeRowGet(gridName);
                        var row = RowGet(gridName, index);
                        if (row.Row != null)
                        {
                            UtilFramework.Assert(row.Row.GetType() == typeRow);
                        }
                        IndexEnum indexEnum = UtilApplication.IndexEnumFromText(index);
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
                            string text = CellTextGet(gridName, index, fieldName);
                            bool isModify = CellGet(gridName, index, fieldName).IsModify;
                            if (isModify)
                            {
                                try
                                {
                                    cell.CellTextParse(app, gridName, index, ref text);
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
                                        cell.CellValueFromText(app, gridName, index, ref text);
                                        app.CellValueFromText(gridName, index, cell, ref text);
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
                GridQueryInternal gridQuery = QueryGet(gridName);
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
                    GridColumnInternal gridColumn = ColumnGet(gridName, gridColumnJson.FieldName);
                    gridColumn.IsClick = gridColumnJson.IsClick;
                }
            }
        }

        /// <summary>
        /// Load data from http json request.
        /// </summary>
        public void LoadJson(AppJson appJson, string gridName, App app)
        {
            LoadJsonQuery(appJson);
            LoadJsonColumn(appJson);
            //
            GridDataJson gridDataJson = appJson.GridDataJson;
            //
            string typeRowString = gridDataJson.GridQueryList[gridName].TypeRow;
            Type typeRow = UtilDataAccessLayer.TypeRowFromName(typeRowString, UtilApplication.TypeRowInAssembly(app));
            TypeRowSet(gridName, typeRow);
            //
            foreach (GridRow row in gridDataJson.RowList[gridName])
            {
                IndexEnum indexEnum = UtilApplication.IndexEnumFromText(row.Index);
                Row resultRow = null;
                if (indexEnum == IndexEnum.Index)
                {
                    resultRow = (Row)UtilFramework.TypeToObject(typeRow);
                }
                GridRowInternal gridRow = new GridRowInternal() { Row = resultRow, IsSelect = row.IsSelect, IsClick = row.IsClick };
                RowSet(gridName, row.Index, gridRow);
                foreach (Cell cell in ColumnList(gridName))
                {
                    cell.Constructor(gridRow.Row);
                    string fieldName = cell.FieldNameCSharp;
                    //
                    CellGet(gridName, row.Index, fieldName).IsSelect = gridDataJson.CellList[gridName][fieldName][row.Index].IsSelect;
                    CellGet(gridName, row.Index, fieldName).IsClick = gridDataJson.CellList[gridName][fieldName][row.Index].IsClick;
                    CellGet(gridName, row.Index, fieldName).IsModify = gridDataJson.CellList[gridName][fieldName][row.Index].IsModify;
                    string text;
                    if (gridDataJson.CellList[gridName][cell.FieldNameCSharp][row.Index].IsO)
                    {
                        text = gridDataJson.CellList[gridName][fieldName][row.Index].O; // Original text.
                        string textModify = gridDataJson.CellList[gridName][fieldName][row.Index].T; // User modified text.
                        CellTextSet(gridName, row.Index, fieldName, textModify);
                    }
                    else
                    {
                        text = gridDataJson.CellList[gridName][fieldName][row.Index].T; // Original text.
                        CellTextSet(gridName, row.Index, fieldName, text);
                    }
                    // ErrorField
                    string errorFieldText = gridDataJson.CellList[gridName][fieldName][row.Index].E;
                    if (errorFieldText != null)
                    {
                        ErrorCellSet(gridName, row.Index, fieldName, errorFieldText);
                    }
                    // ErrorRow
                    string errorRowText = row.Error;
                    if (errorRowText != null)
                    {
                        ErrorRowSet(gridName, row.Index, errorRowText);
                    }
                    if (indexEnum == IndexEnum.Index)
                    {
                        cell.CellValueFromText(app, gridName, row.Index, ref text);
                        app.CellValueFromText(gridName, row.Index, cell, ref text);
                        object value = UtilDataAccessLayer.ValueFromText(text, cell.PropertyInfo.PropertyType);
                        cell.PropertyInfo.SetValue(resultRow, value);
                    }
                }
            }
        }

        /// <summary>
        /// Load data from GridDataJson to GridData.
        /// </summary>
        public void LoadJson(AppJson appJson, App app)
        {
            GridDataJson gridDataJson = appJson.GridDataJson;
            //
            if (gridDataJson != null)
            {
                foreach (string gridName in gridDataJson.GridQueryList.Keys)
                {
                    LoadJson(appJson, gridName, app);
                }
            }
        }

        /// <summary>
        /// Returns row's columns.
        /// </summary>
        private static List<GridColumn> TypeRowToGridColumn(App app, Type typeRow, string gridName)
        {
            var result = new List<GridColumn>();
            //
            List<FrameworkConfigColumnView> configColumnList = app.DbConfigColumnList(typeRow);
            //
            var columnList = UtilDataAccessLayer.ColumnList(typeRow);
            double widthPercentTotal = 0;
            bool isLast = false;
            //
            List<Cell> columnIsVisibleList = new List<Cell>();
            foreach (Cell column in columnList)
            {
                bool isVisible = UtilApplication.ConfigFieldNameSqlIsId(column.FieldNameSql) == false;
                FrameworkConfigColumnView configColumn = configColumnList.Where(item => item.FieldNameSql == column.FieldNameSql && item.FieldNameCSharp == column.FieldNameCSharp).FirstOrDefault();
                if (configColumn != null && configColumn.IsVisible != null)
                {
                    isVisible = configColumn.IsVisible.Value;
                }
                app.ColumnIsVisible(gridName, column, ref isVisible);
                column.ColumnIsVisible(ref isVisible);
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
                string text = column.FieldNameSql;
                if (text == null)
                {
                    text = column.FieldNameCSharp; // Calculated column has no FieldNameSql.
                }
                column.ColumnText(ref text);
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
                    result.Add(new GridColumn() { FieldName = column.FieldNameCSharp, Text = text, IsVisible = false });
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
                    GridColumnInternal gridColumn = ColumnGet(gridName, gridColumnJson.FieldName);
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
            foreach (string gridName in queryList.Keys)
            {
                GridQueryInternal gridQuery = queryList[gridName];
                GridQuery gridQueryJson = gridDataJson.GridQueryList[gridName];
                gridQueryJson.FieldNameOrderBy = gridQuery.FieldNameOrderBy;
                gridQueryJson.IsOrderByDesc = gridQuery.IsOrderByDesc;
            }
        }

        /// <summary>
        /// Render cell as Button, Html or FileUpload.
        /// </summary>
        private void SaveJsonIsButtonHtmlFileUpload(App app, string gridName, string index, Cell cell, GridCell gridCell)
        {
            gridCell.CellEnum = null;
            //
            bool result = false;
            cell.CellIsButton(app, gridName, index, ref result);
            if (result)
            {
                gridCell.CellEnum = GridCellEnum.Button;
            }
            else
            {
                result = false;
                cell.CellIsHtml(app, gridName, index, ref result);
                if (result)
                {
                    gridCell.CellEnum = GridCellEnum.Html;
                }
                else
                {
                    result = false;
                    cell.CellIsFileUpload(app, gridName, index, ref result);
                    if (result)
                    {
                        gridCell.CellEnum = GridCellEnum.FileUpload;
                    }
                }
            }
        }

        /// <summary>
        /// Copy data from class GridData to class GridDataJson.
        /// </summary>
        public void SaveJson(App app)
        {
            AppJson appJson = app.AppJson;
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
            foreach (string gridName in rowList.Keys)
            {
                Type typeRow = TypeRowGet(gridName);
                gridDataJson.GridQueryList[gridName] = new GridQuery() { GridName = gridName, TypeRow = UtilDataAccessLayer.TypeRowToName(typeRow) };
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
                gridDataJson.ColumnList[gridName] = TypeRowToGridColumn(app, typeRow, gridName);
                // Cell
                if (gridDataJson.CellList == null)
                {
                    gridDataJson.CellList = new Dictionary<string, Dictionary<string, Dictionary<string, GridCell>>>();
                }
                gridDataJson.CellList[gridName] = new Dictionary<string, Dictionary<string, GridCell>>();
                //
                PropertyInfo[] propertyInfoList = null;
                foreach (string index in rowList[gridName].Keys)
                {
                    GridRowInternal gridRow = rowList[gridName][index];
                    string errorRow = ErrorRowGet(gridName, index);
                    GridRow gridRowJson = new GridRow() { Index = index, IsSelect = gridRow.IsSelect, IsClick = gridRow.IsClick, Error = errorRow };
                    gridDataJson.RowList[gridName].Add(gridRowJson);
                    if (propertyInfoList == null && typeRow != null)
                    {
                        propertyInfoList = typeRow.GetTypeInfo().GetProperties();
                    }
                    if (propertyInfoList != null)
                    {
                        foreach (Cell cell in ColumnList(gridName))
                        {
                            cell.Constructor(gridRow.Row);
                            //
                            string fieldName = cell.FieldNameCSharp;
                            object value = null;
                            if (gridRow.Row != null)
                            {
                                value = cell.PropertyInfo.GetValue(gridRow.Row);
                            }
                            string textJson = UtilDataAccessLayer.ValueToText(value, cell.TypeField);
                            cell.CellValueToText(app, gridName, index, ref textJson); // Override text.
                            app.CellValueToText(gridName, index, cell, ref textJson); // Override text generic.
                            string text = CellTextGet(gridName, index, fieldName);
                            GridCellInternal gridCell = CellGet(gridName, index, fieldName);
                            if (!gridDataJson.CellList[gridName].ContainsKey(fieldName))
                            {
                                gridDataJson.CellList[gridName][fieldName] = new Dictionary<string, GridCell>();
                            }
                            string errorCell = ErrorCellGet(gridName, index, fieldName);
                            GridCell gridCellJson = new GridCell() { IsSelect = gridCell.IsSelect, IsClick = gridCell.IsClick, IsModify = gridCell.IsModify, E = errorCell };
                            gridDataJson.CellList[gridName][fieldName][index] = gridCellJson;
                            //
                            SaveJsonIsButtonHtmlFileUpload(app, gridName, index, cell, gridCellJson);
                            //
                            if (text == null)
                            {
                                gridCellJson.T = textJson;
                            }
                            else
                            {
                                gridCellJson.O = textJson;
                                gridCellJson.T = text;
                                gridCellJson.IsO = true;
                            }
                        }
                    }
                }
            }
            // Query removed rows. For example methos LoadRow("Table", null, null);
            foreach (string gridName in queryList.Keys)
            {
                if (!rowList.ContainsKey(gridName))
                {
                    gridDataJson.RowList[gridName] = new List<GridRow>();
                }
            }
            SaveJsonColumn(appJson);
            SaveJsonQuery(appJson);
        }
    }
}
