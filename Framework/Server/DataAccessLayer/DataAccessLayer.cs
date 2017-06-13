namespace Framework.Server.DataAccessLayer
{
    using Framework.Server.Application;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Base class for every database row.
    /// </summary>
    public class Row
    {
        protected virtual bool IsReadOnly()
        {
            return false;
        }
    }

    /// <summary>
    /// Base class for every database field.
    /// </summary>
    public class Cell
    {
        /// <summary>
        /// Constructor for column.
        /// </summary>
        internal void Constructor(string tableNameSql, string fieldNameSql, string fieldNameCSharp)
        {
            this.tableNameSql = tableNameSql;
            this.fieldNameSql = fieldNameSql;
            this.fieldNameCSharp = fieldNameCSharp;
        }

        /// <summary>
        /// Constructor for column and cell.
        /// </summary>
        internal Cell Constructor(object row)
        {
            this.row = row;
            return this;
        }

        private string tableNameSql;

        /// <summary>
        /// Gets sql TableName.
        /// </summary>
        public string TableName
        {
            get
            {
                return tableNameSql;
            }
        }

        private string fieldNameSql;

        /// <summary>
        /// Gets sql FieldName. If null, then it's a calculated column.
        /// </summary>
        public string FieldNameSql
        {
            get
            {
                return fieldNameSql;
            }
        }

        private string fieldNameCSharp;

        /// <summary>
        /// Gets Csharp FieldName.
        /// </summary>
        public string FieldNameCSharp
        {
            get
            {
                return fieldNameCSharp;
            }
        }

        private object row;

        public object Row
        {
            get
            {
                return row;
            }
        }

        protected virtual internal void CellIsReadOnly(ref bool isReadOnly)
        {

        }

        protected virtual internal void ColumnText(ref string text)
        {

        }

        protected virtual internal void ColumnWidthPercent(ref double widthPercent)
        {

        }

        protected virtual internal void LookUp(out Type typeRow, out List<DataAccessLayer.Row> rowList)
        {
            typeRow = null;
            rowList = null;
        }

        protected virtual internal void ColumnIsVisible(ref bool isVisible)
        {

        }

        protected virtual internal void ColumnIsReadOnly(ref bool isReadOnly)
        {

        }

        /// <summary>
        /// Returns true, if data call is to be rendered as button.
        /// </summary>
        protected virtual internal bool ColumnIsButton()
        {
            return GetType().GetTypeInfo().GetMethod(nameof(CellProcessButtonIsClick), BindingFlags.Instance | BindingFlags.NonPublic).DeclaringType == GetType(); // Method ProcessButtonIsClick(); is overwritten.
        }

        protected virtual internal void CellProcessButtonIsClick(PageGrid pageGrid, string gridName, string index, string fieldName)
        {

        }
    }

    /// <summary>
    /// Base class for every database field.
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
    /// Sql table name and field name.
    /// </summary>
    public class SqlNameAttribute : Attribute
    {
        public SqlNameAttribute(string sqlName)
        {
            this.SqlName = sqlName;
        }

        public readonly string SqlName;
    }

    public class TypeCellAttribute : Attribute
    {
        public TypeCellAttribute(Type typeCell)
        {
            Framework.Util.Assert(typeCell.GetTypeInfo().IsSubclassOf(typeof(Cell)));
            this.TypeCell = typeCell;
        }

        public readonly Type TypeCell;
    }
}
