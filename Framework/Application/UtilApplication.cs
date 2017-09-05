namespace Framework.Application
{
    using Database.dbo;
    using Framework.Component;
    using Framework.DataAccessLayer;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public enum IndexEnum
    {
        None = 0,
        Index = 1,
        Filter = 2,
        New = 3,
        Total = 4
    }

    /// <summary>
    /// Html cascading style sheets information.
    /// </summary>
    public class InfoCss
    {
        private List<string> valueList = new List<string>();

        public void Add(string value)
        {
            if (!valueList.Contains(value))
            {
                valueList.Add(value);
            }
        }

        public void Remove(string value)
        {
            if (valueList.Contains(value))
            {
                valueList.Remove(value);
            }
        }

        public string ToHtml()
        {
            if (valueList.Count == 0)
            {
                return null;
            }
            StringBuilder result = new StringBuilder();
            bool isFirst = false;
            foreach (var value in valueList)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    result.Append(" ");
                }
                result.Append(value);
            }
            return result.ToString();
        }
    }

    public class InfoColumn
    {
        internal InfoColumn(Cell column)
        {
            this.ColumnInternal = column;
        }

        internal readonly Cell ColumnInternal;

        /// <summary>
        /// Gets or sets Text. This is the grid column header.
        /// </summary>
        public string Text;

        /// <summary>
        /// Gets or sets IsReadOnly. Column read only flag.
        /// </summary>
        public bool IsReadOnly;

        /// <summary>
        /// Gets or sets IsVisible. If false, data is not shown but still transfered to client.
        /// </summary>
        public bool IsVisible;

        /// <summary>
        /// Gets WidthPercent. This is the column width.
        /// </summary>
        public double WidthPercent
        {
            get;
            internal set;
        }

        internal InfoCell InfoCell;
    }

    public class InfoCell
    {
        public InfoCell()
        {
            this.Css = new InfoCss();
        }

        /// <summary>
        /// Gets or sets IsReadOnly.
        /// </summary>
        public bool IsReadOnly;

        /// <summary>
        /// Gets or sets CellEnum. If not rendered as default (null), cell can be rendered as Button, Html or FileUpload.
        /// </summary>
        public GridCellEnum? CellEnum;

        /// <summary>
        /// Gets or sets InfoCss. Html cascading style sheets information for cell.
        /// </summary>
        public InfoCss Css;

        /// <summary>
        /// Gets or sets PlaceHolder. For example "Search" for filter or "New" for new row, when no text is displayed in input field.
        /// </summary>
        public string PlaceHolder;
    }

    public class Info
    {
        internal Info(App app)
        {
            this.App = app;
        }

        internal readonly App App;

        private string gridName;

        private Type TypeRow;

        /// <summary>
        /// (FieldNameCSharp, Column).
        /// </summary>
        Dictionary<string, InfoColumn> infoColumnList = new Dictionary<string, InfoColumn>();

        internal void ColumnInit(App app, string gridName, Type typeRow)
        {
            UtilFramework.Assert(app == App);
            this.gridName = gridName;
            this.TypeRow = typeRow;
            //
            infoColumnList = new Dictionary<string, InfoColumn>();
            var columnList = UtilDataAccessLayer.ColumnList(typeRow);
            foreach (Cell column in columnList)
            {
                infoColumnList[column.FieldNameCSharp] = new InfoColumn(column);
            }
            // Config from Db
            List<FrameworkConfigColumnView> configColumnList = app.DbConfigColumnList(typeRow);
            // Text
            foreach (InfoColumn infoColumn in infoColumnList.Values)
            {
                // Text
                string text = infoColumn.ColumnInternal.FieldNameSql;
                if (text == null)
                {
                    text = infoColumn.ColumnInternal.FieldNameCSharp; // Calculated column has no FieldNameSql.
                }
                infoColumn.Text = text;
            }
            // IsVisible
            foreach (InfoColumn infoColumn in infoColumnList.Values)
            {
                bool isVisible = UtilApplication.ConfigFieldNameSqlIsId(infoColumn.ColumnInternal.FieldNameSql) == false;
                FrameworkConfigColumnView configColumn = configColumnList.Where(item => item.FieldNameSql == infoColumn.ColumnInternal.FieldNameSql && item.FieldNameCSharp == infoColumn.ColumnInternal.FieldNameCSharp).FirstOrDefault();
                if (configColumn != null && configColumn.IsVisible != null)
                {
                    isVisible = configColumn.IsVisible.Value;
                }
                infoColumn.IsVisible = isVisible;
            }
            // Override App
            foreach (InfoColumn infoColumn in infoColumnList.Values)
            {
                app.InfoColumn(gridName, typeRow, infoColumn);
            }
            // Override Column
            foreach (InfoColumn infoColumn in infoColumnList.Values)
            {
                infoColumn.ColumnInternal.InfoColumn(app, gridName, typeRow, infoColumn);
            }
        }

        internal void CellInit(App app, string gridName, Type typeRow, Row row, string index)
        {
            foreach (InfoColumn infoColumn in infoColumnList.Values)
            {
                infoColumn.InfoCell = new InfoCell();
            }
            //
            foreach (string fieldNameCSharp in infoColumnList.Keys)
            {
                InfoColumn infoColumn = infoColumnList[fieldNameCSharp];
                Cell cell = infoColumn.ColumnInternal;
                UtilFramework.Assert(cell.Row == null);
                try
                {
                    cell.Constructor(row); // Column to cell;
                    infoColumn.InfoCell.PlaceHolder = app.GridData.CellGet(gridName, index, fieldNameCSharp).PlaceHolder; // PlaceHolder loaded back from json request.
                    if (infoColumn.IsVisible)
                    {
                        app.InfoCell(gridName, index, cell, infoColumn.InfoCell);
                        cell.InfoCell(app, gridName, index, infoColumn.InfoCell);
                    }
                }
                finally
                {
                    cell.Constructor(null);
                }
            }
        }

        /// <summary>
        /// Returns InfoColumn.
        /// </summary>
        internal InfoColumn ColumnGet(App app, string gridName, Type typeRow, Cell column)
        {
            UtilFramework.Assert(this.App == app && this.gridName == gridName && this.TypeRow == typeRow);
            return infoColumnList[column.FieldNameCSharp];
        }

        /// <summary>
        /// Returns InfoCell.
        /// </summary>
        internal InfoCell CellGet(App app, string gridName, Type typeRow, Cell column)
        {
            UtilFramework.Assert(this.App == app && this.gridName == gridName && this.TypeRow == typeRow);
            return infoColumnList[column.FieldNameCSharp].InfoCell;
        }
    }

    public static class UtilApplication
    {
        /// <summary>
        /// Bitwise (01=Select; 10=MouseOver; 11=Select and MouseOver).
        /// </summary>
        public static bool IsSelectGet(int isSelect)
        {
            return (isSelect & 1) == 1;
        }

        /// <summary>
        /// Bitwise (01=Select; 10=MouseOver; 11=Select and MouseOver).
        /// </summary>
        public static int IsSelectSet(int isSelect, bool value)
        {
            if (value)
            {
                isSelect = isSelect | 1;
            }
            else
            {
                isSelect = isSelect & 2;
            }
            return isSelect;
        }

        /// <summary>
        /// Returns TypeRowInAssembly. This is a type in an assembly. Search for row class in this assembly when deserializing json. (For example: "dbo.Airport")
        /// </summary>
        public static Type TypeRowInAssembly(App app)
        {
            return app.GetType();
        }

        public static string IndexEnumToText(IndexEnum indexEnum)
        {
            return indexEnum.ToString();
        }

        public static IndexEnum IndexEnumFromText(string index)
        {
            if (IndexEnumToText(IndexEnum.Filter) == index)
            {
                return IndexEnum.Filter;
            }
            if (IndexEnumToText(IndexEnum.New) == index)
            {
                return IndexEnum.New;
            }
            if (IndexEnumToText(IndexEnum.Total) == index)
            {
                return IndexEnum.Total;
            }
            int indexInt;
            if (int.TryParse(index, out indexInt))
            {
                return IndexEnum.Index;
            }
            return IndexEnum.None;
        }

        /// <summary>
        /// Returns true, if column name contains "Id" according default naming convention.
        /// </summary>
        public static bool ConfigFieldNameSqlIsId(string fieldNameSql)
        {
            bool result = false;
            if (fieldNameSql != null)
            {
                int index = 0;
                while (index != -1)
                {
                    index = fieldNameSql.IndexOf("Id", index);
                    if (index != -1)
                    {
                        index += "Id".Length;
                        if (index < fieldNameSql.Length)
                        {
                            string text = fieldNameSql.Substring(index, 1);
                            if (text.ToUpper() == text)
                            {
                                result = true;
                                break;
                            }
                        }
                        else
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }
            return result;
        }
    }
}
