namespace Framework.DataAccessLayer
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
        public SqlTableAttribute(string schemaNameSql, string tableNameSql)
        {
            this.SchemaNameSql = schemaNameSql;
            this.TableNameSql = tableNameSql;
        }

        public readonly string SchemaNameSql;

        public readonly string TableNameSql;
    }

    /// <summary>
    /// Sql field name.
    /// </summary>
    public class SqlFieldAttribute : Attribute
    {
        public SqlFieldAttribute(string fieldNameSql, bool isPrimaryKey, FrameworkTypeEnum frameworkTypeEnum)
        {
            this.FieldNameSql = fieldNameSql;
            this.IsPrimaryKey = isPrimaryKey;
            this.FrameworkTypeEnum = frameworkTypeEnum;
        }

        public SqlFieldAttribute(string fieldNameSql, FrameworkTypeEnum frameworkTypeEnum)
            : this(fieldNameSql, false, frameworkTypeEnum)
        {

        }

        /// <summary>
        /// Gets or sets FieldNameSql. If null, it's a calculated field.
        /// </summary>
        public readonly string FieldNameSql;

        public readonly bool IsPrimaryKey;

        // public readonly Type TypeCell;
        /// <summary>
        /// Gets FrameworkTypeEnum. See also class FrameworkType.
        /// </summary>
        public readonly FrameworkTypeEnum FrameworkTypeEnum;
    }
}
