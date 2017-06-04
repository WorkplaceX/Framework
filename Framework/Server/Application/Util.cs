namespace Framework.Server.Application
{
    using Framework.Server.Application.Json;
    using Framework.Server.DataAccessLayer;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class GridDataServerRow
    {
        public string GridName;

        public string Index;

        public Row Row;

        public Row RowNew;
    }

    public class GridDataServer
    {
        /// <summary>
        /// (GridName, TypeRow)
        /// </summary>
        public Dictionary<string, Type> TypeRowList = new Dictionary<string, Type>();

        /// <summary>
        /// (GridName, Index). Original row.
        /// </summary>
        public Dictionary<string, Dictionary<string, GridDataServerRow>> RowList = new Dictionary<string, Dictionary<string, GridDataServerRow>>();

        public GridDataServerRow RowGet(string gridName, string index)
        {
            GridDataServerRow result = null;
            if (RowList.ContainsKey(gridName))
            {
                if (RowList[gridName].ContainsKey(index))
                {
                    result = RowList[gridName][index];
                }
            }
            return result;
        }

        public void RowSet(GridDataServerRow gridDataServerRow)
        {
            string gridName = gridDataServerRow.GridName;
            string index = gridDataServerRow.Index;
            if (!RowList.ContainsKey(gridName))
            {
                RowList[gridName] = new Dictionary<string, GridDataServerRow>();
            }
            RowList[gridName][index] = gridDataServerRow;
        }

        /// <summary>
        /// (GridName, Index, FieldName, Text). User modified text.
        /// </summary>
        public Dictionary<string, Dictionary<string, Dictionary<string, string>>> TextList = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

        /// <summary>
        /// (GridName, Index, FieldName, Text). Error attached to field.
        /// </summary>
        public Dictionary<string, Dictionary<string, Dictionary<string, string>>> ErrorFieldList = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

        /// <summary>
        /// (GridName, Index, Text). Error attached to data row.
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> ErrorRowList = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// Returns error attached to data row.
        /// </summary>
        public string ErrorRowGet(string gridName, string index)
        {
            string result = null;
            if (ErrorRowList.ContainsKey(gridName))
            {
                if (ErrorRowList[gridName].ContainsKey(index))
                {
                    result = ErrorRowList[gridName][index];
                }
            }
            return result;
        }

        /// <summary>
        /// Set error on data row.
        /// </summary>
        public void ErrorRowSet(string gridName, string index, string text)
        {
            if (!ErrorRowList.ContainsKey(gridName))
            {
                ErrorRowList[gridName] = new Dictionary<string, string>();
            }
            ErrorRowList[gridName][index] = text;
        }

        /// <summary>
        /// Gets user entered text.
        /// </summary>
        /// <returns>If null, user has not changed text.</returns>
        public string TextGet(string gridName, string index, string fieldName)
        {
            string result = null;
            if (TextList.ContainsKey(gridName))
            {
                if (TextList[gridName].ContainsKey(index))
                {
                    if (TextList[gridName][index].ContainsKey(fieldName))
                    {
                        result = TextList[gridName][index][fieldName];
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Sets user entered text.
        /// </summary>
        /// <param name="text">If null, user has not changed text.</param>
        public void TextSet(string gridName, string index, string fieldName, string text)
        {
            if (!TextList.ContainsKey(gridName))
            {
                TextList[gridName] = new Dictionary<string, Dictionary<string, string>>();
            }
            if (!TextList[gridName].ContainsKey(index))
            {
                TextList[gridName][index] = new Dictionary<string, string>();
            }
            TextList[gridName][index][fieldName] = text;
        }

        public string ErrorFieldGet(string gridName, string index, string fieldName)
        {
            string result = null;
            if (ErrorFieldList.ContainsKey(gridName))
            {
                if (ErrorFieldList[gridName].ContainsKey(index))
                {
                    if (ErrorFieldList[gridName][index].ContainsKey(fieldName))
                    {
                        result = ErrorFieldList[gridName][index][fieldName];
                    }
                }
            }
            return result;
        }

        public void ErrorFieldSet(string gridName, string index, string fieldName, string text)
        {
            if (!ErrorFieldList.ContainsKey(gridName))
            {
                ErrorFieldList[gridName] = new Dictionary<string, Dictionary<string, string>>();
            }
            if (!ErrorFieldList[gridName].ContainsKey(index))
            {
                ErrorFieldList[gridName][index] = new Dictionary<string, string>();
            }
            ErrorFieldList[gridName][index][fieldName] = text;
        }

        /// <summary>
        /// Load data from database.
        /// </summary>
        public void LoadDatabase(string gridName, Type typeRow)
        {
            TypeRowList[gridName] = typeRow;
            List<Row> rowList = DataAccessLayer.Util.Select(typeRow, 0, 15);
            Load(gridName, typeRow, rowList);
        }

        /// <summary>
        /// Load data from list.
        /// </summary>
        public void Load(string gridName, Type typeRow, List<Row> rowList)
        {
            Util.TypeRowValidate(typeRow, ref rowList);
            TextList.Remove(gridName); // Clear user modified text
            ErrorFieldList.Remove(gridName); // Clear errors attached to fields
            RowList[gridName] = new Dictionary<string, GridDataServerRow>(); // Clear data
            TypeRowList[gridName] = typeRow;
            //
            for (int index = 0; index < rowList.Count; index++)
            {
                RowSet(new GridDataServerRow() { GridName = gridName, Index = index.ToString(), Row = rowList[index], RowNew = null });
            }
        }

        /// <summary>
        /// Save data to database.
        /// </summary>
        public void SaveDatabase()
        {
            foreach (string gridName in RowList.Keys)
            {
                foreach (string index in RowList[gridName].Keys)
                {
                    var row = RowList[gridName][index];
                    if (row.RowNew != null)
                    {
                        try
                        {
                            DataAccessLayer.Util.Update(row.Row, row.RowNew);
                        }
                        catch (Exception exception)
                        {
                            ErrorRowSet(gridName, index, exception.Message);
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
            foreach (string gridName in TextList.Keys)
            {
                foreach (string index in TextList[gridName].Keys)
                {
                    var row = RowGet(gridName, index);
                    if (row != null)
                    {
                        foreach (string fieldName in TextList[gridName][index].Keys)
                        {
                            if (row.RowNew == null)
                            {
                                row.RowNew = DataAccessLayer.Util.Clone(row.Row);
                            }
                            string text = TextGet(gridName, index, fieldName);
                            if (text != null)
                            {
                                if (text == "")
                                {
                                    text = null;
                                }
                                object value;
                                try
                                {
                                    value = DataAccessLayer.Util.ValueFromText(text, row.RowNew.GetType().GetProperty(fieldName).PropertyType); // Parse text.
                                }
                                catch (Exception exception)
                                {
                                    ErrorFieldSet(gridName, index, fieldName, exception.Message);
                                    row.RowNew = null; // Do not save.
                                    break;
                                }
                                row.RowNew.GetType().GetProperty(fieldName).SetValue(row.RowNew, value);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Load data from GridDataJson to GridDataServer.
        /// </summary>
        public void LoadJson(ApplicationJson applicationJson, string gridName, Type typeInAssembly)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            //
            string typeRowName = gridDataJson.GridLoadList[gridName].TypeRowName;
            Type typeRow = DataAccessLayer.Util.TypeRowFromName(typeRowName, typeInAssembly);
            TypeRowList[gridName] = typeRow;
            //
            foreach (GridRow row in gridDataJson.RowList[gridName])
            {
                Row resultRow = (Row)Activator.CreateInstance(typeRow);
                RowSet(new GridDataServerRow() { GridName = gridName, Index = row.Index, Row = resultRow });
                foreach (var column in gridDataJson.ColumnList[gridName])
                {
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
                        ErrorFieldSet(gridName, row.Index, column.FieldName, errorFieldText);
                    }
                    // ErrorRow
                    string errorRowText = row.Error;
                    if (errorRowText != null)
                    {
                        ErrorRowSet(gridName, row.Index, errorRowText);
                    }
                    PropertyInfo propertyInfo = typeRow.GetProperty(column.FieldName);
                    object value = DataAccessLayer.Util.ValueFromText(text, propertyInfo.PropertyType);
                    propertyInfo.SetValue(resultRow, value);
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
            foreach (string gridName in gridDataJson.GridLoadList.Keys)
            {
                LoadJson(applicationJson, gridName, typeInAssembly);
            }
        }

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
        /// Copy data from class GridDataServer to class GridDataJson.
        /// </summary>
        public void SaveJson(ApplicationJson applicationJson)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            //
            if (gridDataJson.GridLoadList == null)
            {
                gridDataJson.GridLoadList = new Dictionary<string, GridLoad>();
            }
            //
            foreach (string gridName in RowList.Keys)
            {
                Type typeRow = TypeRowList[gridName];
                gridDataJson.GridLoadList[gridName] = new GridLoad() { GridName = gridName, TypeRowName = DataAccessLayer.Util.TypeRowToName(typeRow) };
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
                    GridDataServerRow row = RowList[gridName][index];
                    gridDataJson.RowList[gridName].Add(new GridRow() { Index = index.ToString() });
                    if (propertyInfoList == null && typeRow != null)
                    {
                        propertyInfoList = typeRow.GetTypeInfo().GetProperties();
                    }
                    if (propertyInfoList != null)
                    {
                        foreach (PropertyInfo propertyInfo in propertyInfoList)
                        {
                            string fieldName = propertyInfo.Name;
                            object value = propertyInfo.GetValue(row.Row);
                            string textJson = DataAccessLayer.Util.ValueToText(value); // Framework.Server.DataAccessLayer.Util.ValueToJson(value);
                            if (!gridDataJson.CellList[gridName].ContainsKey(fieldName))
                            {
                                gridDataJson.CellList[gridName][fieldName] = new Dictionary<string, GridCell>();
                            }
                            gridDataJson.CellList[gridName][fieldName][index.ToString()] = new GridCell() { T = textJson };
                        }
                    }
                }
            }
        }
    }

    public static class Util
    {
        private static List<GridColumn> GridToJsonColumnList(Type typeRow)
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

        public static void TypeRowValidate(Type typeRow, ref List<DataAccessLayer.Row> rowList)
        {
            if (rowList == null)
            {
                rowList = new List<DataAccessLayer.Row>();
            }
            foreach (DataAccessLayer.Row row in rowList)
            {
                Framework.Util.Assert(row.GetType() == typeRow);
            }
        }

        public static void GridToJson(ApplicationJson applicationJson, string gridName, Type typeRow, List<DataAccessLayer.Row> rowList)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            //
            if (gridDataJson.GridLoadList == null)
            {
                gridDataJson.GridLoadList = new Dictionary<string, GridLoad>();
            }
            gridDataJson.GridLoadList[gridName] = new GridLoad() { GridName = gridName, TypeRowName = DataAccessLayer.Util.TypeRowToName(typeRow) };
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
            gridDataJson.ColumnList[gridName] = GridToJsonColumnList(typeRow);
            // Cell
            if (gridDataJson.CellList == null)
            {
                gridDataJson.CellList = new Dictionary<string, Dictionary<string, Dictionary<string, GridCell>>>();
            }
            gridDataJson.CellList[gridName] = new Dictionary<string, Dictionary<string, GridCell>>();
            //
            PropertyInfo[] propertyInfoList = null;
            for (int index = 0; index < rowList.Count; index++)
            {
                object row = rowList[index];
                gridDataJson.RowList[gridName].Add(new GridRow() { Index = index.ToString() });
                if (propertyInfoList == null && typeRow != null)
                {
                    propertyInfoList = typeRow.GetTypeInfo().GetProperties();
                }
                if (propertyInfoList != null)
                {
                    foreach (PropertyInfo propertyInfo in propertyInfoList)
                    {
                        string fieldName = propertyInfo.Name;
                        object value = propertyInfo.GetValue(row);
                        string textJson = DataAccessLayer.Util.ValueToText(value); // Framework.Server.DataAccessLayer.Util.ValueToJson(value);
                        if (!gridDataJson.CellList[gridName].ContainsKey(fieldName))
                        {
                            gridDataJson.CellList[gridName][fieldName] = new Dictionary<string, GridCell>();
                        }
                        gridDataJson.CellList[gridName][fieldName][index.ToString()] = new GridCell() { T = textJson };
                    }
                }
            }
        }

        /// <summary>
        /// Load data into CellList. Not visual.
        /// </summary>
        public static void GridToJson(ApplicationJson applicationJson, string gridName, Type typeRow)
        {
            List<DataAccessLayer.Row> rowList = Framework.Server.DataAccessLayer.Util.Select(typeRow, 0, 15);
            GridToJson(applicationJson, gridName, typeRow, rowList);
        }

        public class Grid
        {
            public Grid(Type typeRow)
            {
                this.TypeRow = typeRow;
                this.RowList = new List<DataAccessLayer.Row>();
            }

            public List<DataAccessLayer.Row> RowList;

            public readonly Type TypeRow;
        }

        /// <summary>
        /// Returns row index. Excludes Header and Total.
        /// </summary>
        public static int? GridIndexFromJson(GridRow gridRow)
        {
            int result;
            if (int.TryParse(gridRow.Index, out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns unmodified data row.
        /// </summary>
        private static Row GridDataServerFromJson(GridDataJson gridDataJson, string gridName, GridRow gridRow, Type typeRow)
        {
            Row result = (Row)Activator.CreateInstance(typeRow);
            foreach (var column in gridDataJson.ColumnList[gridName])
            {
                string text;
                if (gridDataJson.CellList[gridName][column.FieldName][gridRow.Index].IsO)
                {
                    text = gridDataJson.CellList[gridName][column.FieldName][gridRow.Index].O;
                }
                else
                {
                    text = gridDataJson.CellList[gridName][column.FieldName][gridRow.Index].T;
                }
                PropertyInfo propertyInfo = typeRow.GetProperty(column.FieldName);
                object value = DataAccessLayer.Util.ValueFromText(text, propertyInfo.PropertyType);
                propertyInfo.SetValue(result, value);
            }
            return result;
        }

        private static void GridDataServerFromJson(ApplicationJson applicationJson, string gridName, GridDataServer result, Type typeInAssembly)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            string typeRowName = gridDataJson.GridLoadList[gridName].TypeRowName;
            Type typeRow = DataAccessLayer.Util.TypeRowFromName(typeRowName, typeInAssembly);
            result.TypeRowList[gridName] = typeRow;
            //
            foreach (GridRow gridRow in gridDataJson.RowList[gridName])
            {
                Row row = GridDataServerFromJson(gridDataJson, gridName, gridRow, typeRow);
                GridDataServerRow gridDataServerRow = new GridDataServerRow() { GridName = gridName, Index = gridRow.Index, Row = row, RowNew = null };
                result.RowSet(gridDataServerRow);
            }
            //
            foreach (GridRow gridRow in gridDataJson.RowList[gridName])
            {
                foreach (var column in gridDataJson.ColumnList[gridName])
                {
                    if (gridDataJson.CellList[gridName][column.FieldName][gridRow.Index].IsO)
                    {
                        string text = gridDataJson.CellList[gridName][column.FieldName][gridRow.Index].T;
                        if (text == null)
                        {
                            text = ""; // User changed text.
                        }
                        result.TextSet(gridName, gridRow.Index, column.FieldName, text);
                        string textError = gridDataJson.CellList[gridName][column.FieldName][gridRow.Index].E;
                        result.ErrorFieldSet(gridName, gridRow.Index, column.FieldName, textError);
                    }
                }
            }
        }

        public static GridDataServer GridDataServerFromJson(ApplicationJson applicationJson, Type typeInAssembly)
        {
            GridDataServer result = new GridDataServer();
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            foreach (string gridName in gridDataJson.GridLoadList.Keys)
            {
                GridDataServerFromJson(applicationJson, gridName, result, typeInAssembly);
            }
            return result;
        }

        public static Grid GridFromJson(ApplicationJson applicationJson, string gridName, Type typeInAssembly)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            //
            string typeRowName = gridDataJson.GridLoadList[gridName].TypeRowName;
            Type typeRow = DataAccessLayer.Util.TypeRowFromName(typeRowName, typeInAssembly);
            Grid result = new Grid(typeRow);
            foreach (GridRow row in gridDataJson.RowList[gridName])
            {
                DataAccessLayer.Row resultRow = (DataAccessLayer.Row)Activator.CreateInstance(typeRow);
                result.RowList.Add(resultRow);
                foreach (var column in gridDataJson.ColumnList[gridName])
                {
                    string text = gridDataJson.CellList[gridName][column.FieldName][row.Index].T;
                    PropertyInfo propertyInfo = typeRow.GetProperty(column.FieldName);
                    object value = DataAccessLayer.Util.ValueFromText(text, propertyInfo.PropertyType);
                    propertyInfo.SetValue(resultRow, value);
                }
            }
            return result;
        }
    }
}
