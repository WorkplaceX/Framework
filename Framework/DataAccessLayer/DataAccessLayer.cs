namespace Framework.DataAccessLayer
{
    using Framework.Application;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Base class for every database row.
    /// </summary>
    public class Row
    {
        /// <summary>
        /// Update data row on database.
        /// </summary>
        /// <param name="row">Old data row.</param>
        /// <param name="rowNew">New data row. Set properties on this rowNew, for example to read back updated content from db.</param>
        protected virtual internal void Update(Row row, Row rowNew, ApplicationEventArgument e)
        {
            UtilFramework.Assert(this == rowNew);
            if (e.App.GridData.IsModifyRowCell(e.GridName, e.Index, true)) // No update on database, if only calculated column has been modified.
            {
                UtilDataAccessLayer.Update(row, this);
            }
        }

        /// <summary>
        /// Override this method for example to save data to underlying database tables from sql view.
        /// </summary>
        protected virtual internal void Insert(Row rowNew, ApplicationEventArgument e)
        {
            UtilFramework.Assert(rowNew == this);
            if (e.App.GridData.IsModifyRowCell(e.GridName, e.Index, true)) // No insert on database, if only calculated column has been modified.
            {
                UtilDataAccessLayer.Insert(this);
            }
        }

        /// <summary>
        /// Override this method to filter detail grid when master row has been clicked.
        /// </summary>
        /// <param name="gridNameMaster">Master gridName.</param>
        /// <param name="rowMaster">Clicked master grid row.</param>
        /// <param name="isReload">If true, this grid (detail) gets reloaded. Override also method Row.Query(); to filter detail grid.</param>
        protected virtual internal void MasterIsClick(App app, GridName gridNameMaster, Row rowMaster, ref bool isReload)
        {

        }

        protected virtual internal IQueryable Query(App app, GridName gridName)
        {
            return UtilDataAccessLayer.Query(GetType());
        }

        protected virtual internal IQueryable QueryLookup(Row rowLookup, ApplicationEventArgument e)
        {
            return UtilDataAccessLayer.Query(GetType());
        }
    }

    /// <summary>
    /// Base class for every database cell.
    /// </summary>
    public class Cell
    {
        /// <summary>
        /// Constructor for column.
        /// </summary>
        internal void Constructor(string tableNameCSharp, string columnNameSql, string columnNameCSharp, Type typeRow, Type typeColumn, PropertyInfo propertyInfo)
        {
            this.TableNameCSharp = tableNameCSharp;
            this.ColumnNameSql = columnNameSql;
            this.ColumnNameCSharp = columnNameCSharp;
            this.TypeRow = typeRow;
            this.TypeColumn = typeColumn;
            this.PropertyInfo = propertyInfo;
        }

        /// <summary>
        /// Constructor for column and cell. Switch between column and cell mode. (Column mode: row = null; Cell mode: row != null).
        /// </summary>
        internal Cell Constructor(object row)
        {
            this.Row = row;
            return this;
        }

        /// <summary>
        /// Gets sql TableName.
        /// </summary>
        public string TableNameCSharp { get; private set; }


        /// <summary>
        /// Gets sql ColumnName. If null, then it's a calculated column.
        /// </summary>
        public string ColumnNameSql { get; private set; }

        /// <summary>
        /// Gets CSharp ColumnName.
        /// </summary>
        public string ColumnNameCSharp { get; private set; }

        /// <summary>
        /// Gets TypeRow.
        /// </summary>
        public Type TypeRow { get; private set; }

        /// <summary>
        /// Gets TypeColumn.
        /// </summary>
        public Type TypeColumn { get; private set; }

        internal PropertyInfo PropertyInfo { get; private set; }

        /// <summary>
        /// Gets Row. Null, if column.
        /// </summary>
        public object Row { get; private set; }

        protected virtual internal void DesignColumn(DesignColumn result, ApplicationEventArgument e)
        {

        }

        protected virtual internal void DesignCell(DesignCell result, ApplicationEventArgument e)
        {

        }

        /// <summary>
        /// Parse user entered text.
        /// </summary>
        protected virtual internal void TextParse(string text, ApplicationEventArgument e)
        {
            object value = UtilDataAccessLayer.RowValueFromText(text, Row.GetType().GetProperty(e.ColumnName).PropertyType); // Default parse text.
            Row.GetType().GetProperty(e.ColumnName).SetValue(Row, value);
        }

        /// <summary>
        /// Override for custom formatting like adding units of measurement. Called after method UtilDataAccessLayer.RowValueToText(); Inverse function is CellValueFromText.
        /// </summary>
        protected virtual internal void RowValueToText(ref string result, ApplicationEventArgument e)
        {

        }

        /// <summary>
        /// Override to parse custom formating like value with units of measurement. Called before user entered text is parsed with method UtilDataAccessLayer.ValueFromText(); Inverse function is CellValueToText.
        /// </summary>
        protected virtual internal void RowValueFromText(ref string result, ApplicationEventArgument e)
        {
            
        }

        protected virtual internal void WidthPercent(ref double widthPercent)
        {

        }

        /// <summary>
        /// Returns GridName for lookup grid window. See also method Row.QueryLookup(); to filter rows.
        /// </summary>
        protected virtual internal GridNameTypeRow Lookup(ApplicationEventArgument e)
        {
            return null;
        }

        /// <summary>
        /// Override to handle clicked Lookup row.
        /// </summary>
        /// <param name="rowLookup">LoowUp row which has been clicked.</param>
        protected virtual internal void LookupIsClick(Row rowLookup, ApplicationEventArgument e)
        {
        
        }

        /// <summary>
        /// Override this method to handle button click event. For example delete button.
        /// </summary>
        protected virtual internal void ButtonIsClick(App app, GridName gridName, Index index, Row row, string columnName, ref bool isReload)
        {

        }

        /// <summary>
        /// Gets or sets Value. Throws exception if cell is in column mode.
        /// </summary>
        public object Value
        {
            get
            {
                if (Row == null) // Column mode!
                {
                    return null;
                }
                return PropertyInfo.GetValue(Row);
            }
            set
            {
                UtilFramework.Assert(Row != null, "Column mode!");
                PropertyInfo.SetValue(Row, value);
            }
        }
    }

    /// <summary>
    /// Base class for every database cell.
    /// </summary>
    public class Cell<TRow> : Cell
    {
        public new TRow Row
        {
            get
            {
                return (TRow)base.Row;
            }
        }
    }

    /// <summary>
    /// Sql schema name and table name.
    /// </summary>
    public class SqlTableAttribute : Attribute
    {
        public SqlTableAttribute(string sqlSchemaName, string sqlTableName)
        {
            this.SqlSchemaName = sqlSchemaName;
            this.SqlTableName = sqlTableName;
        }

        public readonly string SqlSchemaName;

        public readonly string SqlTableName;
    }

    /// <summary>
    /// Sql column name.
    /// </summary>
    public class SqlColumnAttribute : Attribute
    {
        public SqlColumnAttribute(string sqlColumnName, Type typeCell)
        {
            this.SqlColumnName = sqlColumnName;
            this.TypeCell = typeCell;
        }

        public readonly string SqlColumnName;

        public readonly Type TypeCell;
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ConfigGridAttribute : Attribute
    {
        public ConfigGridAttribute(string gridName, int pageRowCount, bool pageRowCountIsNull, bool isInsert, bool isInsertIsNull)
        {
            this.GridName = gridName;
            this.pageRowCount = pageRowCount;
            this.pageRowCountIsNull = pageRowCountIsNull;
            this.isInsert = isInsert;
            this.isInsertIsNull = isInsertIsNull;
        }

        public readonly string GridName;

        private int pageRowCount;

        private bool pageRowCountIsNull; // Because of error "Attribute constructor parameter 'pageRowCount' has type 'int?', which is not a valid attribute parameter type": See also: https://stackoverflow.com/questions/3192833/why-decimal-is-not-a-valid-attribute-parameter-type

        public int? PageRowCount
        {
            get
            {
                if (pageRowCountIsNull)
                {
                    return null;
                }
                return pageRowCount;
            }
        }

        private bool isInsert;

        private bool isInsertIsNull;

        public bool? IsInsert
        {
            get
            {
                if (isInsertIsNull)
                {
                    return null;
                }
                return isInsert;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ConfigColumnAttribute : Attribute
    {
        public ConfigColumnAttribute(string gridName, string text, string description, bool isVisible, bool isVisibleIsNull, bool isReadOnly, bool isReadOnlyIsNull, double sort, bool sortIsNull, int widthPercent, bool widthPercentIsNull)
        {
            this.GridName = gridName;
            this.Text = text;
            this.Description = description;
            this.isVisible = isVisible;
            this.isVisibleIsNull = isVisibleIsNull;
            this.isReadOnly = isReadOnly;
            this.isReadOnlyIsNull = isReadOnlyIsNull;
            this.sort = sort;
            this.sortIsNull = sortIsNull;
            this.widthPercent = widthPercent;
            this.widthPercentIsNull = widthPercentIsNull;
        }

        public readonly string GridName;

        public readonly string Text;

        public readonly string Description;

        private bool isVisible;

        private bool isVisibleIsNull;

        public bool? IsVisible
        {
            get
            {
                if (isVisibleIsNull)
                {
                    return null;
                }
                return isVisible;
            }
        }

        private bool isReadOnly;

        private bool isReadOnlyIsNull;

        public bool? IsReadOnly
        {
            get
            {
                if (isReadOnlyIsNull)
                {
                    return null;
                }
                return isReadOnly;
            }
        }

        private double sort;

        private bool sortIsNull;

        public double? Sort
        {
            get
            {
                if (sortIsNull)
                {
                    return null;
                }
                return sort;
            }
        }

        private int widthPercent;

        private bool widthPercentIsNull;

        public int? WidthPercent
        {
            get
            {
                if (widthPercentIsNull)
                {
                    return null;
                }
                return widthPercent;
            }
        }
    }
}
