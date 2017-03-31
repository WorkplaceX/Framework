namespace Framework.Server.DataAccessLayer
{
    using System;

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
        internal void Constructor(object row)
        {
            this.row = row;
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
        /// Gets sql FieldName.
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
        /// Gets sql FieldName.
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

        protected virtual internal void ColumnIsVisible(ref bool isVisible)
        {

        }

        protected virtual internal void ColumnIsReadOnly(ref bool isReadOnly)
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
            this.TypeCell = typeCell;
        }

        public readonly Type TypeCell;
    }
}
