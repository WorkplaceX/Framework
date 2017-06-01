namespace Framework.Server.Application
{
    using Framework.Server.Application.Json;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

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
