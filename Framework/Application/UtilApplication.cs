namespace Framework.Application
{
    using Database.dbo;
    using Framework.Component;
    using Framework.DataAccessLayer;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// GridName is a Name or the name of a TypeRow or combined.
    /// </summary>
    public class GridName2
    {
        internal GridName2(string name, Type typeRow, bool isNameNotCombine)
        {
            this.Name = name;
            this.TypeRowInternal = typeRow;
            this.IsNameNotCombineInternal = isNameNotCombine;
            //
            ValueUpdate();
        }

        public GridName2(string name) 
            : this(name, null, false)
        {

        }

        /// <summary>
        /// Returns object GridName or GridNameTypeRow as json string.
        /// </summary>
        internal static string GridNameToJson(GridName2 gridName)
        {
            return gridName.Value;
        }

        /// <summary>
        /// Returns GridName loaded from json. It is never returns a class GridNameTypeRow.
        /// </summary>
        internal static GridName2 GridNameFromJson(string json)
        {
            GridName2 result = new GridName2(null);
            result.Value = json;
            return result;
        }

        public readonly string Name;

        internal readonly Type TypeRowInternal;

        internal readonly bool IsNameNotCombineInternal;

        private void ValueUpdate()
        {
            UtilFramework.Assert(Name == null || !Name.Contains("."));
            if (IsNameNotCombineInternal)
            {
                this.Value = Name;
            }
            else
            {
                this.Value = UtilDataAccessLayer.TypeRowToName(TypeRowInternal) + "." + Name;
            }
        }

        internal string Value;

        public override string ToString()
        {
            return base.ToString() + $" ({Value})";
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            GridName2 gridName = obj as GridName2;
            if (gridName != null)
            {
                return object.Equals(Value, gridName.Value);
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public static bool operator ==(GridName2 left, GridName2 right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(GridName2 left, GridName2 right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Gets or sets Type. Used for json serialization of derived class.
        /// </summary>
        public string Type;
    }

    public class GridNameTypeRow2 : GridName2
    {
        public GridNameTypeRow2(Type typeRow) 
            : base(null, typeRow, false)
        {

        }

        public GridNameTypeRow2(string name, Type typeRow, bool isNotCombine = false) 
            : base(name, typeRow, isNotCombine)
        {

        }

        public Type TypeRow
        {
            get
            {
                return base.TypeRowInternal;
            }
        }

        public bool IsNameNotCombine
        {
            get
            {
                return base.IsNameNotCombineInternal;
            }
        }
    }

    public class GridName2<TRow> : GridNameTypeRow2 where TRow : Row
    {
        public GridName2(string name, bool isNameNotCombine = false) 
            : base(name, typeof(TRow), isNameNotCombine)
        {

        }
    }

    public class GridName
    {
        internal GridName(string name, bool isJson)
        {
            if (isJson == false)
            {
                UtilFramework.Assert(name == null || (name.Contains(".") == false));
            }
            //
            this.Name = name;
            this.Value = name;
            this.IsJson = isJson;
        }

        public GridName(string name) 
            : this(name, false)
        {

        }

        /// <summary>
        /// Gets IsJson. Indicating value has been loaded back from json.
        /// </summary>
        internal readonly bool IsJson;

        public readonly string Name;

        /// <summary>
        /// Gets GridName without TypeRow.
        /// </summary>
        public string NameExclusive
        {
            get
            {
                string result = Value.Substring(Value.LastIndexOf(".") + ".".Length);
                if (result == "")
                {
                    result = null;
                }
                return result;
            }
        }

        /// <summary>
        /// Gets IsNameExclusive. If true, Name is not combined with TypeRow.
        /// </summary>
        public bool IsNameExclusive
        {
            get
            {
                return Value == NameExclusive;
            }
        }

        /// <summary>
        /// Gets GridName or (GridName and TypeRow) combined.
        /// </summary>
        internal string Value;

        public override string ToString()
        {
            return base.ToString() + $" ({Value})";
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            GridName gridName = obj as GridName;
            if (gridName != null)
            {
                return object.Equals(Value, gridName.Value);
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public static bool operator ==(GridName left, GridName right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(GridName left, GridName right)
        {
            return !Equals(left, right);
        }
    }

    /// <summary>
    /// Combination of GridName and TypeRow.
    /// </summary>
    public class GridNameTypeRow : GridName
    {
        public GridNameTypeRow(Type typeRow)
            : base(null)
        {
            this.TypeRow = typeRow;
            ValueInit(false);
        }

        public GridNameTypeRow(Type typeRow, string name, bool isNameExclusive = false)
            : base(name)
        {
            this.TypeRow = typeRow;
            ValueInit(isNameExclusive);
        }

        public GridNameTypeRow(Type typeRow, GridName gridName)
            : this(typeRow, gridName.NameExclusive, gridName.IsNameExclusive)
        {

        }

        private void ValueInit(bool isNameExclusive)
        {
            if (isNameExclusive == false)
            {
                Value = UtilFramework.TypeToName(TypeRow) + "." + Name;
            }
            else
            {
                Value = Name;
            }
            UtilFramework.Assert(Value != null);
            UtilFramework.Assert(isNameExclusive == IsNameExclusive);
        }

        public readonly Type TypeRow;
    }

    public class GridName<TRow> : GridNameTypeRow where TRow : Row
    {
        public GridName()
            : base(typeof(TRow))
        {

        }

        public GridName(string name, bool isNameExclusive = false)
            : base(typeof(TRow), name, isNameExclusive)
        {

        }
    }

    public enum IndexEnum
    {
        None = 0,
        Index = 1,
        Filter = 2,
        New = 3,
        Total = 4
    }

    /// <summary>
    /// Data grid row index.
    /// </summary>
    public class Index
    {
        public Index(string value)
        {
            this.Value = value;
            this.Enum = UtilApplication.IndexEnumFromText(value);
        }

        public Index(IndexEnum value)
        {
            UtilFramework.Assert(value != IndexEnum.Index);
            this.Value = UtilApplication.IndexEnumToText(value);
            this.Enum = value;
        }

        public readonly string Value;

        public readonly IndexEnum Enum;

        public override string ToString()
        {
            return base.ToString() + $" ({Value})";
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {

            Index index = obj as Index;
            if (index != null)
            {
                return object.Equals(Value, index.Value);
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public static bool operator ==(Index left, Index right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Index left, Index right)
        {
            return !Equals(left, right);
        }
    }

    /// <summary>
    /// Html cascading style sheets information.
    /// </summary>
    public class InfoCssClass
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
            this.CssClass = new InfoCssClass();
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
        public InfoCssClass CssClass;

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

        private GridNameTypeRow gridName;

        /// <summary>
        /// (FieldNameCSharp, Column).
        /// </summary>
        Dictionary<string, InfoColumn> infoColumnList = new Dictionary<string, InfoColumn>();

        internal void ColumnInit(App app, GridNameTypeRow gridName)
        {
            UtilFramework.Assert(app == App);
            this.gridName = gridName;
            //
            infoColumnList = new Dictionary<string, InfoColumn>();
            var columnList = UtilDataAccessLayer.ColumnList(gridName.TypeRow);
            foreach (Cell column in columnList)
            {
                infoColumnList[column.FieldNameCSharp] = new InfoColumn(column);
            }
            // Config from Db
            List<FrameworkConfigColumnView> configColumnList = app.DbConfigColumnList(gridName.TypeRow);
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
                app.InfoColumn(gridName, infoColumn);
            }
            // Override Column
            foreach (InfoColumn infoColumn in infoColumnList.Values)
            {
                infoColumn.ColumnInternal.InfoColumn(app, gridName, infoColumn);
            }
        }

        internal void CellInit(App app, GridNameTypeRow gridName, Row row, Index index)
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
        internal InfoColumn ColumnGet(App app, GridNameTypeRow gridName, Cell column)
        {
            UtilFramework.Assert(this.App == app && this.gridName == gridName);
            return infoColumnList[column.FieldNameCSharp];
        }

        /// <summary>
        /// Returns InfoCell.
        /// </summary>
        internal InfoCell CellGet(App app, GridNameTypeRow gridName, Cell column)
        {
            UtilFramework.Assert(this.App == app && this.gridName == gridName);
            return infoColumnList[column.FieldNameCSharp].InfoCell;
        }
    }

    public static class UtilApplication
    {
        [ThreadStatic]
        private static GridName gridNameLookup;

        public static GridName GridNameLookup
        {
            get
            {
                if (gridNameLookup == null)
                {
                    gridNameLookup = new GridName("Lookup");
                }
                return gridNameLookup;
            }
        }

        /// <summary>
        /// Bitwise (01=Select; 10=MouseOver; 11=Select and MouseOver).
        /// </summary>
        internal static bool IsSelectGet(int isSelect)
        {
            return (isSelect & 1) == 1;
        }

        /// <summary>
        /// Bitwise (01=Select; 10=MouseOver; 11=Select and MouseOver).
        /// </summary>
        internal static int IsSelectSet(int isSelect, bool value)
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
        /// Returns list of available class App.
        /// </summary>
        public static Type[] ApplicationTypeList(Type typeInAssembly)
        {
            List<Type> result = new List<Type>();
            foreach (Type itemTypeInAssembly in UtilFramework.TypeInAssemblyList(typeInAssembly))
            {
                foreach (var type in itemTypeInAssembly.GetTypeInfo().Assembly.GetTypes())
                {
                    if (UtilFramework.IsSubclassOf(type, typeof(App)))
                    {
                        result.Add(type);
                    }
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// Returns TypeRowInAssembly. This is a type in an assembly. Search for row class in this assembly when deserializing json. (For example: "dbo.Airport")
        /// </summary>
        public static Type TypeRowInAssembly(App app)
        {
            return app.GetType();
        }

        internal static string IndexEnumToText(IndexEnum indexEnum)
        {
            return indexEnum.ToString();
        }

        internal static IndexEnum IndexEnumFromText(string index)
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
