namespace Framework.Dal
{
    using System;

    /// <summary>
    /// Base class for every database row. (Table and view).
    /// </summary>
    public class Row
    {

    }

    /// <summary>
    /// Base class for every database cell.
    /// </summary>
    public class Cell
    {
        /// <summary>
        /// Gets Row. Null, if column definition.
        /// </summary>
        public object Row { get; private set; }
    }

    /// <summary>
    /// Base class for every database cell.
    /// </summary>
    public class Cell<TRow> : Cell where TRow : Row
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
    /// Sql field name.
    /// </summary>
    public class SqlFieldAttribute : Attribute
    {
        public SqlFieldAttribute(string sqlFieldName, Type typeCell, bool isPrimaryKey, FrameworkTypeEnum frameworkTypeEnum)
        {
            this.SqlFieldName = sqlFieldName;
            this.IsPrimaryKey = isPrimaryKey;
            this.TypeCell = typeCell;
            this.FrameworkTypeEnum = frameworkTypeEnum;
        }

        public SqlFieldAttribute(string sqlFieldName, Type typeCell, FrameworkTypeEnum frameworkTypeEnum)
            : this(sqlFieldName, typeCell, false, frameworkTypeEnum)
        {

        }

        /// <summary>
        /// Gets or sets SqlFieldName. If null, it's a calculated field.
        /// </summary>
        public readonly string SqlFieldName;

        public readonly bool IsPrimaryKey;

        public readonly Type TypeCell;

        /// <summary>
        /// Gets FrameworkTypeEnum. See also class FrameworkType.
        /// </summary>
        public readonly FrameworkTypeEnum FrameworkTypeEnum;
    }
}
