﻿namespace Framework.Server.Application
{
    using Framework.Server.Application.Json;
    using Framework.Server.DataAccessLayer;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class GridDataProcessRow
    {
        public string GridName;

        public string Index;

        public Row Row;

        public Row RowNew;
    }

    public class GridDataProcess
    {
        /// <summary>
        /// (GridName, TypeRow)
        /// </summary>
        public Dictionary<string, Type> TypeRowList = new Dictionary<string, Type>();

        /// <summary>
        /// (GridName, Index). Original row.
        /// </summary>
        public Dictionary<string, Dictionary<string, GridDataProcessRow>> RowList = new Dictionary<string, Dictionary<string, GridDataProcessRow>>();

        public GridDataProcessRow RowGet(string gridName, string index)
        {
            GridDataProcessRow result = null;
            if (RowList.ContainsKey(gridName))
            {
                if (RowList[gridName].ContainsKey(index))
                {
                    result = RowList[gridName][index];
                }
            }
            return result;
        }

        public void RowSet(GridDataProcessRow gridDataProcessRow)
        {
            string gridName = gridDataProcessRow.GridName;
            string index = gridDataProcessRow.Index;
            if (!RowList.ContainsKey(gridName))
            {
                RowList[gridName] = new Dictionary<string, GridDataProcessRow>();
            }
            RowList[gridName][index] = gridDataProcessRow;
        }

        /// <summary>
        /// (GridName, Index, FieldName, Text). User modified text.
        /// </summary>
        public Dictionary<string, Dictionary<string, Dictionary<string, string>>> TextList = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

        /// <summary>
        /// (GridName, Index, FieldName, Text). Error attached to field.
        /// </summary>
        public Dictionary<string, Dictionary<string, Dictionary<string, string>>> ErrorList = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

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

        public string ErrorGet(string gridName, string index, string fieldName)
        {
            string result = null;
            if (ErrorList.ContainsKey(gridName))
            {
                if (ErrorList[gridName].ContainsKey(index))
                {
                    if (ErrorList[gridName][index].ContainsKey(fieldName))
                    {
                        result = ErrorList[gridName][index][fieldName];
                    }
                }
            }
            return result;
        }

        public void ErrorSet(string gridName, string index, string fieldName, string text)
        {
            if (!ErrorList.ContainsKey(gridName))
            {
                ErrorList[gridName] = new Dictionary<string, Dictionary<string, string>>();
            }
            if (!ErrorList[gridName].ContainsKey(index))
            {
                ErrorList[gridName][index] = new Dictionary<string, string>();
            }
            ErrorList[gridName][index][fieldName] = text;
        }

        /// <summary>
        /// Load data from database.
        /// </summary>
        public void LoadDatabase(string gridName, Type typeRow)
        {
            TypeRowList[gridName] = typeRow;
            List<Row> rowList = DataAccessLayer.Util.Select(typeRow, 0, 15);
            TextList.Remove(gridName); // Clear user modified text.
            ErrorList.Remove(gridName); // Clear errors attached to fields.
            RowList.Remove(gridName);
            for (int index = 0; index < rowList.Count; index++)
            {
                RowSet(new GridDataProcessRow() { GridName = gridName, Index = index.ToString(), Row = rowList[index], RowNew = null });
            }
        }

        /// <summary>
        /// Parse user input text. See also method TextSet(); when parse error occurs method ErrorSet(); is called for the field.
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
                                    ErrorSet(gridName, index, fieldName, exception.Message);
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
                            string fieldName = row.Row.GetType().GetProperties().First().Name; // First field
                            ErrorSet(gridName, index, fieldName, exception.Message);
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

        public static void GridToJson(JsonApplication jsonApplication, string gridName, Type typeRow, List<DataAccessLayer.Row> rowList)
        {
            GridData gridData = jsonApplication.GridData;
            //
            if (gridData.GridLoadList == null)
            {
                gridData.GridLoadList = new Dictionary<string, GridLoad>();
            }
            gridData.GridLoadList[gridName] = new GridLoad() { GridName = gridName, TypeRowName = DataAccessLayer.Util.TypeRowToName(typeRow) };
            // Row
            if (gridData.RowList == null)
            {
                gridData.RowList = new Dictionary<string, List<GridRow>>();
            }
            gridData.RowList[gridName] = new List<GridRow>();
            // Column
            if (gridData.ColumnList == null)
            {
                gridData.ColumnList = new Dictionary<string, List<GridColumn>>();
            }
            gridData.ColumnList[gridName] = GridToJsonColumnList(typeRow);
            // Cell
            if (gridData.CellList == null)
            {
                gridData.CellList = new Dictionary<string, Dictionary<string, Dictionary<string, GridCell>>>();
            }
            gridData.CellList[gridName] = new Dictionary<string, Dictionary<string, GridCell>>();
            //
            PropertyInfo[] propertyInfoList = null;
            for (int index = 0; index < rowList.Count; index++)
            {
                object row = rowList[index];
                gridData.RowList[gridName].Add(new GridRow() { Index = index.ToString() });
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
                        if (!gridData.CellList[gridName].ContainsKey(fieldName))
                        {
                            gridData.CellList[gridName][fieldName] = new Dictionary<string, GridCell>();
                        }
                        gridData.CellList[gridName][fieldName][index.ToString()] = new GridCell() { T = textJson };
                    }
                }
            }
        }

        /// <summary>
        /// Load data into CellList. Not visual.
        /// </summary>
        public static void GridToJson(JsonApplication jsonApplication, string gridName, Type typeRow)
        {
            List<DataAccessLayer.Row> rowList = Framework.Server.DataAccessLayer.Util.Select(typeRow, 0, 15);
            GridToJson(jsonApplication, gridName, typeRow, rowList);
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
        private static Row GridDataProcessFromJson(GridData gridData, string gridName, GridRow gridRow, Type typeRow)
        {
            Row result = (Row)Activator.CreateInstance(typeRow);
            foreach (var column in gridData.ColumnList[gridName])
            {
                string text;
                if (gridData.CellList[gridName][column.FieldName][gridRow.Index].IsO)
                {
                    text = gridData.CellList[gridName][column.FieldName][gridRow.Index].O;
                }
                else
                {
                    text = gridData.CellList[gridName][column.FieldName][gridRow.Index].T;
                }
                PropertyInfo propertyInfo = typeRow.GetProperty(column.FieldName);
                object value = DataAccessLayer.Util.ValueFromText(text, propertyInfo.PropertyType);
                propertyInfo.SetValue(result, value);
            }
            return result;
        }

        private static void GridDataProcessFromJson(JsonApplication jsonApplication, string gridName, GridDataProcess result, Type typeInAssembly)
        {
            GridData gridData = jsonApplication.GridData;
            string typeRowName = gridData.GridLoadList[gridName].TypeRowName;
            Type typeRow = DataAccessLayer.Util.TypeRowFromName(typeRowName, typeInAssembly);
            result.TypeRowList[gridName] = typeRow;
            //
            foreach (GridRow gridRow in gridData.RowList[gridName])
            {
                Row row = GridDataProcessFromJson(gridData, gridName, gridRow, typeRow);
                GridDataProcessRow gridDataProcessRow = new GridDataProcessRow() { GridName = gridName, Index = gridRow.Index, Row = row, RowNew = null };
                result.RowSet(gridDataProcessRow);
            }
            //
            foreach (GridRow gridRow in gridData.RowList[gridName])
            {
                foreach (var column in gridData.ColumnList[gridName])
                {
                    if (gridData.CellList[gridName][column.FieldName][gridRow.Index].IsO)
                    {
                        string text = gridData.CellList[gridName][column.FieldName][gridRow.Index].T;
                        if (text == null)
                        {
                            text = ""; // User changed text.
                        }
                        result.TextSet(gridName, gridRow.Index, column.FieldName, text);
                        string textError = gridData.CellList[gridName][column.FieldName][gridRow.Index].E;
                        result.ErrorSet(gridName, gridRow.Index, column.FieldName, textError);
                    }
                }
            }
        }

        public static GridDataProcess GridDataProcessFromJson(JsonApplication jsonApplication, Type typeInAssembly)
        {
            GridDataProcess result = new GridDataProcess();
            GridData gridData = jsonApplication.GridData;
            foreach (string gridName in gridData.GridLoadList.Keys)
            {
                GridDataProcessFromJson(jsonApplication, gridName, result, typeInAssembly);
            }
            return result;
        }

        public static Grid GridFromJson(JsonApplication jsonApplication, string gridName, Type typeInAssembly)
        {
            GridData gridData = jsonApplication.GridData;
            //
            string typeRowName = gridData.GridLoadList[gridName].TypeRowName;
            Type typeRow = DataAccessLayer.Util.TypeRowFromName(typeRowName, typeInAssembly);
            Grid result = new Grid(typeRow);
            foreach (GridRow row in gridData.RowList[gridName])
            {
                DataAccessLayer.Row resultRow = (DataAccessLayer.Row)Activator.CreateInstance(typeRow);
                result.RowList.Add(resultRow);
                foreach (var column in gridData.ColumnList[gridName])
                {
                    string text = gridData.CellList[gridName][column.FieldName][row.Index].T;
                    PropertyInfo propertyInfo = typeRow.GetProperty(column.FieldName);
                    object value = DataAccessLayer.Util.ValueFromText(text, propertyInfo.PropertyType);
                    propertyInfo.SetValue(resultRow, value);
                }
            }
            return result;
        }
    }
}