namespace Framework.Dal
{
    using Framework.Config;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata;
    using Microsoft.EntityFrameworkCore.Metadata.Conventions;
    using Microsoft.EntityFrameworkCore.Metadata.Internal;
    using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
    using Microsoft.EntityFrameworkCore.Storage;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    public static class UtilDal
    {
        /// <summary>
        /// Build model for one typeRow.
        /// </summary>
        private static IMutableModel DbContextModel(Type typeRow)
        {
            // EF Core 2.1
            var typeMappingSource = new SqlServerTypeMappingSource(new TypeMappingSourceDependencies(new ValueConverterSelector(new ValueConverterSelectorDependencies())), new RelationalTypeMappingSourceDependencies());

            var conventionSet = SqlServerConventionSetBuilder.Build();
            var builder = new ModelBuilder(conventionSet);

            // Build model
            var entity = builder.Entity(typeRow);
            SqlTableAttribute tableAttribute = (SqlTableAttribute)typeRow.GetTypeInfo().GetCustomAttribute(typeof(SqlTableAttribute));
            entity.ToTable(tableAttribute.SqlTableName, tableAttribute.SqlSchemaName); // By default EF maps sql table name to class name.
            PropertyInfo[] propertyInfoList = UtilDalType.TypeRowToPropertyInfoList(typeRow);
            bool isPrimaryKey = false; // Sql view 
            foreach (PropertyInfo propertyInfo in propertyInfoList)
            {
                SqlFieldAttribute columnAttribute = (SqlFieldAttribute)propertyInfo.GetCustomAttribute(typeof(SqlFieldAttribute));
                if (columnAttribute == null || columnAttribute.SqlFieldName == null) // Calculated column. Do not include it in sql select.
                {
                    entity.Ignore(propertyInfo.Name);
                }
                else
                {
                    if (columnAttribute.IsPrimaryKey)
                    {
                        isPrimaryKey = true;
                        entity.HasKey(propertyInfo.Name); // Prevent null exception if primary key name is not "Id".
                    }
                    entity.Property(propertyInfo.PropertyType, propertyInfo.Name).HasColumnName(columnAttribute.SqlFieldName);
                    CoreTypeMapping coreTypeMapping = typeMappingSource.FindMapping(propertyInfo.PropertyType);
                    UtilFramework.Assert(coreTypeMapping != null);
                    entity.Property(propertyInfo.PropertyType, propertyInfo.Name).HasAnnotation(CoreAnnotationNames.TypeMapping, coreTypeMapping);
                }
            }

            if (isPrimaryKey == false)
            {
                entity.HasKey(propertyInfoList.First().Name); // Prevent null exception if name of first field (of view) is not "Id".
            }

            var model = builder.Model;
            return model;
        }

        /// <summary>
        /// Returns DbContext with ConnectionString and model for one row, defined in typeRow.
        /// </summary>
        internal static DbContext DbContext(Type typeRow)
        {
            var options = new DbContextOptionsBuilder<DbContext>();
            string connectionString = ConfigFramework.ConnectionString(typeRow);
            if (connectionString == null)
            {
                throw new Exception("ConnectionString is null! (See also file: ConfigFramework.json)");
            }
            options.UseSqlServer(connectionString); // See also: ConfigFramework.json // (Data Source=localhost; Initial Catalog=Application; Integrated Security=True;)
            options.UseModel(DbContextModel(typeRow));
            DbContext result = new DbContext(options.Options);

            return result;
        }

        internal static string ExecuteParameterAdd(Type typeValue, object value, List<SqlParameter> paramList, bool isUseParam = true)
        {
            if (value != null)
            {
                UtilFramework.Assert(value.GetType() == typeValue);
            }
            if (isUseParam)
            {

            }
            else
            {

            }
            return null;
        }

        internal static async Task ExecuteAsync(string sql, List<SqlParameter> paramList, bool isFrameworkDb)
        {
            var sqlList = sql.Split(new string[] { "\r\nGO", "\nGO", "GO\r\n", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);

            string connectionString = ConfigFramework.ConnectionString(isFrameworkDb);
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                foreach (string sqlItem in sqlList)
                {
                    SqlCommand sqlCommand = new SqlCommand(sqlItem, sqlConnection);
                    if (paramList?.Count > 0)
                    {
                        sqlCommand.Parameters.AddRange(paramList.ToArray());
                    }
                    await sqlCommand.ExecuteNonQueryAsync();
                }
            }
        }

        internal static async Task ExecuteAsync(string sql, List<SqlParameter> paramList = null)
        {
            await ExecuteAsync(sql, paramList, false);
        }

        public static IQueryable Query(Type typeRow)
        {
            DbContext dbContext = DbContext(typeRow);
            IQueryable query = (IQueryable)(dbContext.GetType().GetTypeInfo().GetMethod("Set").MakeGenericMethod(typeRow).Invoke(dbContext, null));
            return query;
        }

        public static IQueryable<TRow> Query<TRow>() where TRow : Row
        {
            return (IQueryable<TRow>)Query(typeof(TRow));
        }

        /// <summary>
        /// Returns empty query to clear data grid.
        /// </summary>
        public static IQueryable<TRow> QueryEmpty<TRow>() where TRow: Row
        {
            return Enumerable.Empty<TRow>().AsQueryable();
        }

        /// <summary>
        /// Copy data row. Source and dest need not to be of same type. Only cells available on
        /// both records are copied. See also RowClone();
        /// </summary>
        internal static void RowCopy(Row rowSource, Row rowDest)
        {
            var propertyInfoDestList = UtilDalType.TypeRowToPropertyInfoList(rowDest.GetType());
            foreach (PropertyInfo propertyInfoDest in propertyInfoDestList)
            {
                string fieldName = propertyInfoDest.Name;
                PropertyInfo propertyInfoSource = rowSource.GetType().GetTypeInfo().GetProperty(fieldName);
                if (propertyInfoSource != null)
                {
                    object value = propertyInfoSource.GetValue(rowSource);
                    propertyInfoDest.SetValue(rowDest, value);
                }
            }
        }

        /// <summary>
        /// Returns true, if rows are identical.
        /// </summary>
        internal static bool RowEqual(Row rowA, Row rowB)
        {
            UtilFramework.Assert(rowA.GetType() == rowB.GetType());

            bool result = true;
            var propertyInfoList = UtilDalType.TypeRowToPropertyInfoList(rowA.GetType());
            foreach (PropertyInfo propertyInfo in propertyInfoList)
            {
                object valueA = propertyInfo.GetValue(rowA);
                object valueB = propertyInfo.GetValue(rowB);
                if (object.Equals(valueA, valueB) == false)
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Clone data row.
        /// </summary>
        internal static Row RowCopy(Row row)
        {
            Row result = (Row)UtilFramework.TypeToObject(row.GetType());
            RowCopy(row, result);
            return result;
        }

        /// <summary>
        /// Clone data row.
        /// </summary>
        internal static TRow RowCopy<TRow>(TRow row) where TRow : Row
        {
            return (TRow)RowCopy((Row)row);
        }

        /// <summary>
        /// Select data from database.
        /// </summary>
        public static async Task<List<Row>> SelectAsync(IQueryable query)
        {
            UtilFramework.LogDebug(string.Format("SELECT ({0})", query.ElementType.Name));

            var list = await query.ToDynamicListAsync();
            List<Row> result = list.Cast<Row>().ToList();
            return result;
        }

        public static async Task<List<TRow>> SelectAsync<TRow>(IQueryable<TRow> query) where TRow : Row
        {
            var list = await SelectAsync((IQueryable)query);
            List<TRow> result = list.Cast<TRow>().ToList();
            return result;
        }

        internal static IQueryable QueryFilter(IQueryable query, string fieldName, object filterValue, FilterOperator filterOperator)
        {
            string predicate = fieldName;
            switch (filterOperator)
            {
                case FilterOperator.Equal:
                    predicate += " = @0";
                    break;
                case FilterOperator.Smaller:
                    predicate += " <= @0";
                    break;
                case FilterOperator.Greater:
                    predicate += " >= @0";
                    break;
                case FilterOperator.Like:
                    predicate += ".Contains(@0)";
                    break;
                default:
                    throw new Exception("Enum unknown!");
            }
            return query.Where(predicate, filterValue);
        }

        /// <summary>
        /// Sql orderby.
        /// </summary>
        internal static IQueryable QueryOrderBy(IQueryable query, string fieldName, bool isSort)
        {
            if (isSort == true)
            {
                fieldName += " DESC";
            }
            return query.OrderBy(fieldName);
        }

        /// <summary>
        /// Sql paging.
        /// </summary>
        internal static IQueryable QuerySkipTake(IQueryable query, int skip, int take)
        {
            if (skip != 0)
            {
                query = query.Skip(skip);
            }
            query = query.Take(take);
            return query;
        }

        /// <summary>
        /// Delete data record from database.
        /// </summary>
        public static async Task Delete(Row row)
        {
            DbContext dbContext = DbContext(row.GetType());
            dbContext.Remove(row);
            int count = await dbContext.SaveChangesAsync();
            UtilFramework.Assert(count == 1, "Update failed!");
        }

        /// <summary>
        /// Insert data record. Primary key needs to be 0! Returned new row contains new primary key.
        /// </summary>
        public static async Task<TRow> InsertAsync<TRow>(TRow row) where TRow : Row
        {
            UtilFramework.LogDebug(string.Format("INSERT ({0})", row.GetType().Name));

            Row rowCopy = UtilDal.RowCopy(row);
            DbContext dbContext = DbContext(row.GetType());
            dbContext.Add(row); // Throws NullReferenceException if no primary key is defined.
            try
            {
                int count = await dbContext.SaveChangesAsync();
                UtilFramework.Assert(count == 1, "Update failed!");
                //
                // Exception: Database operation expected to affect 1 row(s) but actually affected 0 row(s). 
                // Cause: No autoincrement on Id column or no Id set by application
                //
                // Exception: The conversion of a datetime2 data type to a datetime data type resulted in an out-of-range value.
                // Cause: CSharp not nullable DateTime default value is "{1/1/0001 12:00:00 AM}" change it to nullable or set value for example to DateTime.Now
            }
            catch (Exception exception)
            {
                UtilDal.RowCopy(rowCopy, row); // In case of exception, EF might change for example auto increment id to -2147482647. Reverse it back.
                throw exception;
            }
            return row; // Return Row with new primary key.
        }

        /// <summary>
        /// Update data record on database.
        /// </summary>
        public static async Task UpdateAsync(Row row, Row rowNew)
        {
            UtilFramework.LogDebug(string.Format("UPDATE ({0})", row.GetType().Name));

            UtilFramework.Assert(row.GetType() == rowNew.GetType());
            if (UtilDal.RowEqual(row, rowNew) == false)
            {
                row = UtilDal.RowCopy(row); // Prevent modifications on SetValues(rowNew);
                DbContext dbContext = UtilDal.DbContext(row.GetType());
                var tracking = dbContext.Attach(row);
                tracking.CurrentValues.SetValues(rowNew);
                int count = await dbContext.SaveChangesAsync();
                UtilFramework.Assert(count == 1, "Update failed!");
            }
        }

        internal static string CellTextFromValue(Row row, PropertyInfo propertyInfo)
        {
            object value = propertyInfo.GetValue(row);
            string result = value?.ToString();
            return result;
        }

        internal static object CellTextToValue(string text, PropertyInfo propertyInfo)
        {
            object result = null;
            if (!string.IsNullOrEmpty(text))
            {
                Type type = UtilFramework.TypeUnderlying(propertyInfo.PropertyType);
                result = Convert.ChangeType(text, type);
            }
            return result;
        }

        /// <summary>
        /// Parse user entered text and write it to row.
        /// </summary>
        internal static void CellTextToValue(string text, PropertyInfo propertyInfo, Row row)
        {
            object value = CellTextToValue(text, propertyInfo);
            propertyInfo.SetValue(row, value);
        }
    }

    internal enum FilterOperator
    {
        None = 0,
        Equal = 1,
        Smaller = 2,
        Greater = 3,
        Like = 4
    }

    internal class UtilDalUpsert
    {
        private static string UpsertFieldNameListToCsv(string[] fieldNameList, string prefix)
        {
            string result = null;
            bool isFirst = true;
            foreach (string fieldName in fieldNameList)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    result += ", ";
                }
                result += prefix + fieldName;
            }
            return result;
        }

        private static string UpsertSelect(Type typeRow, List<Row> rowList, List<SqlParameter> paramList)
        {
            StringBuilder result = new StringBuilder();
            var fieldNameList = UtilDalType.TypeRowToSqlFieldNameList(typeRow);

            // Row
            bool isFirstRow = true;
            foreach (Row row in rowList)
            {
                if (isFirstRow)
                {
                    isFirstRow = false;
                }
                else
                {
                    result.Append(" UNION ALL\r\n");
                }
                result.Append("(SELECT");

                // Field
                bool isFirstField = true;
                foreach (var fieldName in fieldNameList)
                {
                    if (isFirstField)
                    {
                        isFirstField = false;
                    }
                    else
                    {
                        result.Append(", ");
                    }
                    result.Append($"@P{ paramList.Count }");
                    result.Append(" AS " + fieldName.SqlFieldName);

                }
                result.Append(")");
            }
            return result.ToString();
        }

        internal static async Task UpsertAsync(Type typeRow, List<Row> rowList, string[] fieldNameKeyList, string fieldNameIsExist)
        {
            string connectionString = ConfigFramework.ConnectionString(typeRow);
            string tableName = UtilDalType.TypeRowToTableName(typeRow);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                if (fieldNameIsExist != null)
                {
                    string sql = string.Format("UPDATE {0} SET {1}=1", tableName, fieldNameIsExist);
                    SqlCommand sqlCommand = new SqlCommand(sql, connection);
                    await sqlCommand.ExecuteNonQueryAsync();
                }

                string sqlUpsert = @"
                MERGE INTO FrameworkApplication AS Target
                USING ({0}) AS Source
	                ON NOT EXISTS(
                        SELECT (Source.Path)
                        EXCEPT
                        SELECT (Target.Path))
                WHEN MATCHED THEN
	                UPDATE SET 
                        Target.Text = Source.Text, Target.Path = Source.Path, Target.ApplicationTypeId = Source.ApplicationTypeId, Target.IsActive = Source.IsActive
                WHEN NOT MATCHED BY TARGET THEN
	                INSERT (Text, Path, ApplicationTypeId, IsActive)
	                VALUES (Source.Text, Source.Path, Source.ApplicationTypeId, Source.IsActive);
                ";

                foreach (Row row in rowList)
                {
                    UtilFramework.Assert(row.GetType() == typeRow);

                }
            }
        }

        internal static async Task UpsertAsync<TRow>(List<TRow> rowList, string[] fieldNameKeyList, string fieldNameIsExist = "IsExist") where TRow : Row
        {
            await UpsertAsync(typeof(TRow), rowList.Cast<Row>().ToList(), fieldNameKeyList, fieldNameIsExist);
        }

        internal static async Task UpsertAsync<TRow>(List<TRow> rowList, string fieldNameKey, string fieldNameIsExist = "IsExist") where TRow : Row
        {
            await UpsertAsync(rowList, new string[] { fieldNameKey }, fieldNameIsExist);
        }
    }

    public enum FrameworkTypeEnum
    {
        None = 0,
        Int = 1,
        Smallint = 2,
        Tinyint = 3,
        Bigint = 4,
        Uniqueidentifier = 5,
        Datetime = 6,
        Datetime2 = 7,
        Date = 8,
        Char = 9,
        Nvarcahr = 10,
        Varchar = 11,
        Text = 12,
        Ntext = 13,
        Bit = 14,
        Money = 15,
        Decimal = 16,
        Real = 17,
        Float = 18,
        Varbinary = 19,
        Sqlvariant = 20,
        Image = 21,
        Numeric = 22,
    }

    internal static class UtilDalType
    {
        /// <summary>
        /// Returns row type as string. For example: "dbo.User". Omits "Database" namespace.
        /// </summary>
        internal static string TypeRowToTableNameCSharp(Type typeRow)
        {
            string result = null;
            UtilFramework.Assert(UtilFramework.IsSubclassOf(typeRow, typeof(Row)), "Wrong type!");
            result = UtilFramework.TypeToName(typeRow);
            UtilFramework.Assert(result.StartsWith("Database.")); // If it is a calculated row which does not exist on database move it for example to namespace "Database.Calculated".
            result = result.Substring("Database.".Length); // Remove "Database" namespace.
            return result;
        }

        internal static string TypeRowToTableName(Type typeRow)
        {
            string result = null;
            SqlTableAttribute tableAttribute = (SqlTableAttribute)typeRow.GetTypeInfo().GetCustomAttribute(typeof(SqlTableAttribute));
            if (tableAttribute != null && (tableAttribute.SqlSchemaName != null || tableAttribute.SqlTableName != null))
            {
                result = string.Format("[{0}].[{1}]", tableAttribute.SqlSchemaName, tableAttribute.SqlTableName);
            }
            return result;
        }

        internal static PropertyInfo[] TypeRowToPropertyInfoList(Type typeRow)
        {
            return typeRow.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        internal static List<(PropertyInfo PropertyInfo, string SqlFieldName)> TypeRowToSqlFieldNameList(Type typeRow)
        {
            var result = new List<(PropertyInfo PropertyInfo, string SqlFieldName)>();
            var propertyInfoList = TypeRowToPropertyInfoList(typeRow);
            foreach (PropertyInfo propertyInfo in propertyInfoList)
            {
                SqlFieldAttribute fieldAttribute = (SqlFieldAttribute)propertyInfo.GetCustomAttribute(typeof(SqlFieldAttribute));
                string sqlFieldName = null;
                if (fieldAttribute != null)
                {
                    sqlFieldName = fieldAttribute.SqlFieldName;
                }
                result.Add((propertyInfo, sqlFieldName));
            }
            return result;
        }

        public static Type SqlTypeToType(int sqlType)
        {
            Type type = FrameworkTypeList().Where(item => item.Value.SqlType == sqlType).SingleOrDefault().Value?.Type;
            return type;
        }

        public static FrameworkTypeEnum SqlTypeToFrameworkTypeEnum(int sqlType)
        {
            var result = FrameworkTypeList().Where(item => item.Value.SqlType == sqlType).SingleOrDefault().Value?.FrameworkTypeEnum;
            if (result == null)
            {
                return FrameworkTypeEnum.None;
            }
            else
            {
                return (FrameworkTypeEnum)result;
            }
        }

        [ThreadStatic]
        private static Dictionary<FrameworkTypeEnum, FrameworkType> frameworkTypeList;

        private static Dictionary<FrameworkTypeEnum, FrameworkType> FrameworkTypeList()
        {
            // See also: https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-data-type-mappings
            // See also: SELECT * FROM sys.types
            // See also: https://docs.microsoft.com/en-us/sql/t-sql/data-types/data-type-conversion-database-engine

            if (frameworkTypeList == null)
            {
                Dictionary<FrameworkTypeEnum, FrameworkType> result = new Dictionary<FrameworkTypeEnum, FrameworkType>();
                FrameworkType frameworkType;
                frameworkType = new FrameworkTypeInt(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
                frameworkType = new FrameworkTypeSmallint(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
                frameworkType = new FrameworkTypeTinyint(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
                frameworkType = new FrameworkTypeBigint(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
                frameworkType = new FrameworkTypeUniqueidentifier(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
                frameworkType = new FrameworkTypeDatetime(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
                frameworkType = new FrameworkTypeDatetime2(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
                frameworkType = new FrameworkTypeDate(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
                frameworkType = new FrameworkTypeChar(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
                frameworkType = new FrameworkTypeNvarcahr(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
                frameworkType = new FrameworkTypeVarchar(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
                frameworkType = new FrameworkTypeText(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
                frameworkType = new FrameworkTypeNtext(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
                frameworkType = new FrameworkTypeBit(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
                frameworkType = new FrameworkTypeMoney(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
                frameworkType = new FrameworkTypeDecimal(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
                frameworkType = new FrameworkTypeReal(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
                frameworkType = new FrameworkTypeFloat(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
                frameworkType = new FrameworkTypeVarbinary(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
                frameworkType = new SqlTypeSqlvariant(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
                frameworkType = new FrameworkTypeImage(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
                frameworkType = new FrameworkTypeNumeric(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
                frameworkTypeList = result;
            }
            return frameworkTypeList;
        }

        internal class FrameworkType
        {
            public FrameworkType(FrameworkTypeEnum frameworkTypeEnum, string sqlTypeName, int sqlType, Type type, DbType dbType)
            {
                this.FrameworkTypeEnum = frameworkTypeEnum;
                this.SqlTypeName = sqlTypeName;
                this.SqlType = sqlType;
                this.Type = type;
                this.DbType = dbType;
            }

            public readonly FrameworkTypeEnum FrameworkTypeEnum;

            public readonly string SqlTypeName;

            public readonly int SqlType;

            public readonly Type Type;

            public readonly DbType DbType;

            protected virtual string TextFromValue(object value)
            {
                string result = value?.ToString();
                return result;
            }

            protected virtual object TextToValue(string text)
            {
                object result = null;
                if (!string.IsNullOrEmpty(text))
                {
                    Type type = UtilFramework.TypeUnderlying(Type);
                    result = Convert.ChangeType(text, type);
                }
                return result;
            }
        }

        public class FrameworkTypeInt : FrameworkType
        {
            public FrameworkTypeInt()
                : base(FrameworkTypeEnum.Int, "int", 56, typeof(Int32), DbType.Int32)
            {

            }
        }

        public class FrameworkTypeSmallint : FrameworkType
        {
            public FrameworkTypeSmallint()
                : base(FrameworkTypeEnum.Smallint, "smallint", 52, typeof(Int16), DbType.Int16)
            {

            }
        }

        public class FrameworkTypeTinyint : FrameworkType
        {
            public FrameworkTypeTinyint()
                : base(FrameworkTypeEnum.Tinyint, "tinyint", 48, typeof(byte), DbType.Byte)
            {

            }
        }

        public class FrameworkTypeBigint : FrameworkType
        {
            public FrameworkTypeBigint()
                : base(FrameworkTypeEnum.Bigint, "bigint", 127, typeof(Int64), DbType.Int64)
            {

            }
        }

        public class FrameworkTypeUniqueidentifier : FrameworkType
        {
            public FrameworkTypeUniqueidentifier()
                : base(FrameworkTypeEnum.Uniqueidentifier, "uniqueidentifier", 36, typeof(Guid), DbType.Guid)
            {

            }
        }

        public class FrameworkTypeDatetime : FrameworkType
        {
            public FrameworkTypeDatetime()
                : base(FrameworkTypeEnum.Datetime, "datetime", 61, typeof(DateTime), DbType.DateTime)
            {

            }
        }

        public class FrameworkTypeDatetime2 : FrameworkType
        {
            public FrameworkTypeDatetime2()
                : base(FrameworkTypeEnum.Datetime2, "datetime2", 42, typeof(DateTime), DbType.DateTime2)
            {

            }
        }

        public class FrameworkTypeDate : FrameworkType
        {
            public FrameworkTypeDate()
                : base(FrameworkTypeEnum.Date, "date", 40, typeof(DateTime), DbType.Date)
            {

            }
        }

        public class FrameworkTypeChar : FrameworkType
        {
            public FrameworkTypeChar()
                : base(FrameworkTypeEnum.Char, "char", 175, typeof(string), DbType.String)
            {

            }
        }

        public class FrameworkTypeNvarcahr : FrameworkType
        {
            public FrameworkTypeNvarcahr()
                : base(FrameworkTypeEnum.Nvarcahr, "nvarcahr", 231, typeof(string), DbType.String)
            {

            }
        }

        public class FrameworkTypeVarchar : FrameworkType
        {
            public FrameworkTypeVarchar()
                : base(FrameworkTypeEnum.Varchar, "varchar", 167, typeof(string), DbType.String)
            {

            }
        }

        public class FrameworkTypeText : FrameworkType // See also: https://stackoverflow.com/questions/564755/sql-server-text-type-vs-varchar-data-type
        {
            public FrameworkTypeText()
                : base(FrameworkTypeEnum.Text, "text", 35, typeof(string), DbType.String)
            {

            }
        }

        public class FrameworkTypeNtext : FrameworkType
        {
            public FrameworkTypeNtext()
                : base(FrameworkTypeEnum.Ntext, "ntext", 99, typeof(string), DbType.String)
            {

            }
        }

        public class FrameworkTypeBit : FrameworkType
        {
            public FrameworkTypeBit()
                : base(FrameworkTypeEnum.Bit, "bit", 104, typeof(bool), DbType.Boolean)
            {

            }
        }

        public class FrameworkTypeMoney : FrameworkType
        {
            public FrameworkTypeMoney()
                : base(FrameworkTypeEnum.Money, "money", 60, typeof(decimal), DbType.Decimal)
            {

            }
        }

        public class FrameworkTypeDecimal : FrameworkType
        {
            public FrameworkTypeDecimal()
                : base(FrameworkTypeEnum.Decimal, "decimal", 106, typeof(decimal), DbType.Decimal)
            {

            }
        }

        public class FrameworkTypeReal : FrameworkType
        {
            public FrameworkTypeReal()
                : base(FrameworkTypeEnum.Real, "real", 59, typeof(Single), DbType.Single)
            {

            }
        }

        public class FrameworkTypeFloat : FrameworkType
        {
            public FrameworkTypeFloat()
                : base(FrameworkTypeEnum.Float, "float", 62, typeof(double), DbType.Double)
            {

            }
        }

        public class FrameworkTypeVarbinary : FrameworkType
        {
            public FrameworkTypeVarbinary()
                : base(FrameworkTypeEnum.Varbinary, "varbinary", 165, typeof(byte[]), DbType.Binary) // DbType.Binary?
            {

            }
        }

        public class SqlTypeSqlvariant : FrameworkType
        {
            public SqlTypeSqlvariant()
                : base(FrameworkTypeEnum.Sqlvariant, "sql_variant", 98, typeof(object), DbType.Object)
            {

            }
        }

        public class FrameworkTypeImage : FrameworkType
        {
            public FrameworkTypeImage()
                : base(FrameworkTypeEnum.Image, "image", 34, typeof(byte[]), DbType.Binary) // DbType.Binary?
            {

            }
        }

        public class FrameworkTypeNumeric : FrameworkType
        {
            public FrameworkTypeNumeric()
                : base(FrameworkTypeEnum.Numeric, "numeric", 108, typeof(decimal), DbType.Decimal)
            {

            }
        }
    }
}
