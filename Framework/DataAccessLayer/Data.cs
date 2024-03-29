﻿namespace Framework.DataAccessLayer
{
    using Framework.Config;
    using Framework.DataAccessLayer.DatabaseMemory;
    using Framework.Json;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using static Framework.DataAccessLayer.UtilDalType;
    using System.Globalization;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Data.SqlClient;

    /// <summary>
    /// Linq to database or linq to memory.
    /// </summary>
    public enum DatabaseEnum
    {
        None = 0,

        /// <summary>
        /// Linq to database. Update and insert data with methods Data.Update(); and Data.Insert();
        /// </summary>
        Database = 1,

        /// <summary>
        /// Linq to memory shared by multiple requests (singleton scope). Update and insert data with methods Data.Update(); and Data.Insert();
        /// For update it assumes first field is primary key.
        /// </summary>
        Memory = 2,

        /// <summary>
        /// Linq to memory (request scope).
        /// </summary>
        // MemoryRequest = 3, // Replaced with Custom

        /// <summary>
        /// Linq to custom data source. For example list on ComponentJson. Update and insert data by overriding methods Gird.Update(); and Grid.Insert(); 
        /// </summary>
        Custom = 4,
    }

    /// <summary>
    /// Data access layer functions.
    /// </summary>
    public static class Data // public static class UtilDal
    {
        /// <summary>
        /// Update or insert data row.
        /// </summary>
        public static async Task UpsertAsync(Type typeRow, Row row, string[] fieldNameKeyList)
        {
            List<Row> rowList = new List<Row>
            {
                row
            };
            await UtilDalUpsert.UpsertAsync(typeRow, rowList, fieldNameKeyList);
        }

        /// <summary>
        /// Update or insert data row.
        /// </summary>
        public static async Task UpsertAsync<TRow>(TRow row, string[] fieldNameKeyList) where TRow : Row
        {
            await UpsertAsync(typeof(TRow), row, fieldNameKeyList);
        }

        /// <summary>
        /// Returns memory where rows are stored.
        /// </summary>
        public static IList MemoryRowList(Type typeRow, DatabaseEnum databaseEnum = DatabaseEnum.Memory)
        {
            switch (databaseEnum)
            {
                case DatabaseEnum.Memory:
                    return DatabaseMemory.DatabaseMemoryInternal.Instance.RowListGet(typeRow);
                default:
                    throw new Exception("DatabaseEnum not supported!");
            }
        }

        /// <summary>
        /// Returns linq to memory query.
        /// </summary>
        public static List<TRow> MemoryRowList<TRow>(DatabaseEnum databaseEnum = DatabaseEnum.Memory) where TRow : Row
        {
            switch (databaseEnum)
            {
                case DatabaseEnum.Memory:
                    return (List<TRow>)MemoryRowList(typeof(TRow));
                default:
                    throw new Exception("Scope not supported!");
            }
        }

        /// <summary>
        /// DbContext caches Model by default by DbContext type. Framework needs
        /// a Model by DbContextInternal.TypeRow.
        /// </summary>
        internal class ModelCacheKeyFactory : IModelCacheKeyFactory
        {
            public object Create(DbContext context)
            {
                DbContextInternal dbContextInternal = (DbContextInternal)context;
                return new ModelCacheKey(dbContextInternal.TypeRow, dbContextInternal.IsQuery);
            }
        }

        /// <summary>
        /// One DbContext for DbSet and one DbContext for DbQuery for one TypeRow.
        /// </summary>
        internal class ModelCacheKey
        {
            public ModelCacheKey(Type typeRow, bool isQuery)
            {
                this.TypeRow = typeRow;
                this.IsQuery = isQuery;
            }

            public readonly Type TypeRow;

            /// <summary>
            /// Gets IsQuery. If true, EF Core DbQuery is used otherwise DbSet.
            /// DbQuery is used for SELECT. DbSet for INSERT, UPDATE and DELETE.
            /// </summary>
            public readonly bool IsQuery;

            public override int GetHashCode()
            {
                return TypeRow.GetHashCode() + IsQuery.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                ModelCacheKey modelCacheKey = (ModelCacheKey)obj;
                return TypeRow == modelCacheKey.TypeRow && IsQuery == modelCacheKey.IsQuery;
            }
        }

        /// <summary>
        /// DbContext for one TypeRow.
        /// </summary>
        internal class DbContextInternal : DbContext
        {
            public DbContextInternal(string connectionString, Type typeRow, bool isQuery)
            {
                this.ConnectionString = connectionString;
                this.TypeRow = typeRow;
                this.IsQuery = isQuery;
            }

            public readonly string ConnectionString;

            public readonly Type TypeRow;

            /// <summary>
            /// Gets IsQuery. If true, EF Core DbQuery is used otherwise DbSet.
            /// DbQuery is used for SELECT. DbSet for INSERT, UPDATE and DELETE.
            /// </summary>
            public readonly bool IsQuery;

            /// <summary>
            /// Gets Query for TypeRow.
            /// </summary>
            public IQueryable Query
            {
                get
                {
                    var methodInfo = GetType().GetMethods().Where(item => item.Name == "Set" && item.IsGenericMethod).First();
                    return (IQueryable)methodInfo.MakeGenericMethod(TypeRow).Invoke(this, null);
                }
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                base.OnConfiguring(optionsBuilder);

                optionsBuilder.UseSqlServer(ConnectionString);
                optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                optionsBuilder.ReplaceService<IModelCacheKeyFactory, ModelCacheKeyFactory>();
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                var fieldList = UtilDalType.TypeRowToFieldList(TypeRow);
                SqlTableAttribute tableAttribute = (SqlTableAttribute)TypeRow.GetCustomAttribute(typeof(SqlTableAttribute));

                if (IsQuery == false)
                {
                    // Table (DbContext.DbSet)
                    var entityTypeBuilder = modelBuilder.Entity(TypeRow);
                    entityTypeBuilder.ToTable(tableAttribute.TableNameSql, tableAttribute.SchemaNameSql); // By default EF maps sql table name to class name.
                    bool isPrimaryKey = false; // Sql view 
                    foreach (var field in fieldList)
                    {
                        if (field.FieldNameSql == null) // Calculated column. Do not include it in sql select.
                        {
                            entityTypeBuilder.Ignore(field.PropertyInfo.Name);
                        }
                        else
                        {
                            if (field.IsPrimaryKey)
                            {
                                isPrimaryKey = true;
                                entityTypeBuilder.HasKey(field.PropertyInfo.Name); // Prevent null exception if primary key name is not "Id".
                            }
                            var propertyBuilder = entityTypeBuilder.Property(field.PropertyInfo.PropertyType, field.PropertyInfo.Name);
                            propertyBuilder.HasColumnName(field.FieldNameSql);
                            if (UtilDalType.FrameworkTypeFromEnum(field.FrameworkTypeEnum).SqlTypeName == "datetime")
                            {
                                // Prevent "Conversion failed when converting date and/or time from character string." exception for 
                                // sql field type "datetime" for dynamic linq where function. See also method QueryFilter();
                                propertyBuilder.HasColumnType("datetime");
                            }
                        }
                    }
                    if (isPrimaryKey == false)
                    {
                        throw new Exception("No primary key defined! See also property IsHandled."); // Did you set result.IsHandled?
                    }
                }
                else
                {
                    // Query (DbContext.DbQuery)
                    var entityTypeBuilder = modelBuilder.Entity(TypeRow);
                    entityTypeBuilder.HasNoKey();
                    entityTypeBuilder.ToView(tableAttribute.TableNameSql, tableAttribute.SchemaNameSql); // By default EF maps sql table name to class name.
                    foreach (var field in fieldList)
                    {
                        if (field.FieldNameSql == null) // Calculated column. Do not include it in sql select.
                        {
                            entityTypeBuilder.Ignore(field.PropertyInfo.Name);
                        }
                        else
                        {
                            var propertyBuilder = entityTypeBuilder.Property(field.PropertyInfo.PropertyType, field.PropertyInfo.Name);
                            propertyBuilder.HasColumnName(field.FieldNameSql);
                            if (UtilDalType.FrameworkTypeFromEnum(field.FrameworkTypeEnum).SqlTypeName == "datetime")
                            {
                                // Prevent "Conversion failed when converting date and/or time from character string." exception for 
                                // sql field type "datetime" for dynamic linq where function. See also method QueryFilter();
                                propertyBuilder.HasColumnType("datetime");
                            }
                        }
                    }
                }
            }
        }

        /*
        CREATE VIEW MyDebug AS
        SELECT 1 AS Id, 'Blue' AS Text
        UNION ALL
        SELECT NULL AS Id, NULL AS Text
        */

        /*
        public class MyDebug
        {
            public int? Id { get; set; }

            public string Text { get; set; }

            public static void Run()
            {
                foreach (var item in new DbContextDebug().MyQuery)
                {

                }
            }
        }

        internal class DbContextDebug : DbContext
        {
            public DbSet<MyDebug> MyDebug { get; set; }

            public IQueryable<MyDebug> MyQuery
            {
                get
                {
                    var result = (IQueryable)(this.GetType().GetMethods().Where(item => item.Name == "Set" && item.IsGenericMethod).First()).MakeGenericMethod(typeof(MyDebug)).Invoke(this, null);
                    return (IQueryable<MyDebug>)result;
                }
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                base.OnConfiguring(optionsBuilder);

                optionsBuilder.UseSqlServer(ConfigServer.ConnectionString(isFrameworkDb: false));
                // optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                // optionsBuilder.ReplaceService<IModelCacheKeyFactory, ModelCacheKeyFactory>();
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                // Entity model
                // var entityBuilder = modelBuilder.Entity(typeof(MyDebug));
                var entityBuilder = modelBuilder.Entity(typeof(MyDebug)).HasNoKey();
                entityBuilder.ToView("MyDebug");
                entityBuilder.Property("Id");
                entityBuilder.Property("Text");
            }
        }
        */

        /// <summary>
        /// Returns DbContext with ConnectionString and model for one row, defined in typeRow.
        /// </summary>
        internal static DbContextInternal DbContextInternalCreate(Type typeRow, bool isQuery)
        {
            string connectionString = ConfigServer.ConnectionString(typeRow);
            if (connectionString == null)
            {
                throw new Exception("ConnectionString is null! (See also file: ConfigServer.json)"); // Run command ".\wpx.cmd config ConnectionString=..."
            }

            if (UtilDalType.TypeRowIsTableNameSql(typeRow) == false)
            {
                throw new Exception("TypeRow does not have TableNameSql definition!");
            }

            return new DbContextInternal(connectionString, typeRow, isQuery);
        }

        private static string ExecuteParamAddPrivate(FrameworkTypeEnum frameworkTypeEnum, string paramName, object value, List<(FrameworkTypeEnum FrameworkTypeEnum, SqlParameter SqlParameter)> paramList)
        {
            FrameworkType frameworkType = UtilDalType.FrameworkTypeFromEnum(frameworkTypeEnum);

            // ParamName
            if (paramName == null)
            {
                paramName = $"@P{ paramList.Count }";
            }
            UtilFramework.Assert(paramName.StartsWith("@"), "Parameter does not start with @!");
            UtilFramework.Assert(paramList.Where(item => item.SqlParameter.ParameterName == paramName).Count() == 0, string.Format("ParamName already exists! ({0})", paramName));

            // Value
            if (value != null)
            {
                UtilFramework.Assert(value.GetType() == frameworkType.ValueType);
            }
            if (value is string && (string)value == "")
            {
                value = null;
            }
            if (value == null)
            {
                value = DBNull.Value;
            }

            SqlParameter parameter = new SqlParameter(paramName, frameworkType.DbType) { Value = value };
            paramList.Add((frameworkTypeEnum, parameter));

            if (frameworkType.DbType == DbType.Binary)
            {
                // Prevent error: Implicit conversion from data type nvarchar to varbinary(max) is not allowed. Use the CONVERT function to run this query
                paramName = "CONVERT(VARBINARY(MAX), " + paramName + ")";
            }

            return paramName;
        }

        /// <summary>
        /// Adds sql param.
        /// </summary>
        internal static void ExecuteParamAdd(FrameworkTypeEnum frameworkTypeEnum, string paramName, object value, List<(FrameworkTypeEnum FrameworkTypeEnum, SqlParameter SqlParameter)> paramList)
        {
            ExecuteParamAddPrivate(frameworkTypeEnum, paramName, value, paramList);
        }

        /// <summary>
        /// Adds sql param and returns new paramName. For example "@P0".
        /// </summary>
        internal static string ExecuteParamAdd(FrameworkTypeEnum frameworkTypeEnum, object value, List<(FrameworkTypeEnum FrameworkTypeEnum, SqlParameter SqlParameter)> paramList)
        {
            return ExecuteParamAddPrivate(frameworkTypeEnum, null, value, paramList);
        }

        /// <summary>
        /// Replaces sql params with text params and returns full sql for debugging.
        /// </summary>
        internal static string ExecuteParamDebug(string sql, List<(FrameworkTypeEnum FrameworkTypeEnum, SqlParameter SqlParameter)> paramList)
        {
            paramList = paramList.OrderByDescending(item => item.SqlParameter.ParameterName).ToList(); // Replace first @P100, then @P10
            foreach (var param in paramList)
            {
                FrameworkType frameworkType = UtilDalType.FrameworkTypeFromEnum(param.FrameworkTypeEnum);
                string find = param.SqlParameter.ParameterName;
                string replace = frameworkType.ValueToSqlParameterDebug(param.SqlParameter.Value);
                sql = UtilFramework.Replace(sql, find, replace);
            }
            return sql;
        }

        /// <summary>
        /// Execute sql statement. Can contain "GO" batch seperator.
        /// </summary>
        internal static async Task ExecuteNonQueryAsync(string sql, List<(FrameworkTypeEnum FrameworkTypeEnum, SqlParameter SqlParameter)> paramList, bool isFrameworkDb, int? commandTimeout = null, bool isExceptionContinue = false)
        {
            var sqlList = sql.Split(new string[] { "\r\nGO", "\nGO", "GO\r\n", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);

            string connectionString = ConfigServer.ConnectionString(isFrameworkDb);
            using SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            foreach (string sqlItem in sqlList)
            {
                SqlCommand sqlCommand = new SqlCommand(sqlItem, sqlConnection);
                if (commandTimeout.HasValue)
                {
                    sqlCommand.CommandTimeout = commandTimeout.Value;
                }
                if (paramList?.Count > 0)
                {
                    sqlCommand.Parameters.AddRange(paramList.Select(item => item.SqlParameter).ToArray());
                }
                try
                {
                    await sqlCommand.ExecuteNonQueryAsync();
                }
                catch (Exception exception)
                {
                    if (isExceptionContinue)
                    {
                        Console.WriteLine(exception.Message);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Execute sql statement. Can contain "GO" batch seperator.
        /// </summary>
        public static async Task ExecuteNonQueryAsync(string sql)
        {
            await ExecuteNonQueryAsync(sql, null, isFrameworkDb: false);
        }

        /// <summary>
        /// Returns multiple result sets from stored procedure or select.
        /// </summary>
        /// <param name="sql">For example: "SELECT 1 AS A SELECT 2 AS B"; or with parameter: "SELECT @P0 AS A";</param>
        /// <param name="paramList">See also method ExecuteParamAdd();</param>
        /// <returns>Returns (ResultSet, Row, ColumnName, Value) value list.</returns>
        internal static async Task<List<List<Dictionary<string, object>>>> ExecuteReaderMultipleAsync(string sql, List<(FrameworkTypeEnum FrameworkTypeEnum, SqlParameter SqlParameter)> paramList = null, bool isFrameworkDb = false)
        {
            List<List<Dictionary<string, object>>> result = new List<List<Dictionary<string, object>>>();
            string connectionString = ConfigServer.ConnectionString(isFrameworkDb);
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand(sql, sqlConnection);
                if (paramList?.Count > 0)
                {
                    sqlCommand.Parameters.AddRange(paramList.Select(item => item.SqlParameter).ToArray());
                }
                using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync();
                while (sqlDataReader.HasRows)
                {
                    var rowList = new List<Dictionary<string, object>>();
                    result.Add(rowList);
                    while (sqlDataReader.Read())
                    {
                        var row = new Dictionary<string, object>();
                        rowList.Add(row);
                        for (int i = 0; i < sqlDataReader.FieldCount; i++)
                        {
                            string columnName = sqlDataReader.GetName(i);
                            object value = sqlDataReader.GetValue(i);
                            if (value == DBNull.Value)
                            {
                                value = null;
                            }
                            row.Add(columnName, value);
                        }
                    }
                    sqlDataReader.NextResult();
                }
            }
            return result;
        }

        /// <summary>
        /// Execute sql statement.
        /// </summary>
        internal static async Task<List<Row>> ExecuteReaderAsync(Type typeRow, string sql, List<(FrameworkTypeEnum FrameworkTypeEnum, SqlParameter SqlParameter)> paramList = null, bool isFrameworkDb = false)
        {
            var valueList = await ExecuteReaderMultipleAsync(sql, paramList, isFrameworkDb);
            return ExecuteReaderMultipleResultCopy(typeRow, valueList, 0);
        }

        /// <summary>
        /// Execute sql statement.
        /// </summary>
        internal static async Task<List<TRow>> ExecuteReaderAsync<TRow>(string sql, List<(FrameworkTypeEnum FrameworkTypeEnum, SqlParameter SqlParameter)> paramList, bool isFrameworkDb = false) where TRow : Row
        {
            var valueList = await ExecuteReaderMultipleAsync(sql, paramList, isFrameworkDb);
            return ExecuteReaderMultipleResultCopy<TRow>(valueList, 0);
        }

        /// <summary>
        /// Convert one sql multiple result set to typed row list.
        /// </summary>
        /// <param name="valueList">Result of method ExecuteReaderAsync();</param>
        /// <param name="resultSetIndex">Sql multiple result set index.</param>
        /// <param name="typeRow">Type of row to copy to. Copy one multiple result set.</param>
        /// <returns>List of typed rows.</returns>
        internal static List<Row> ExecuteReaderMultipleResultCopy(Type typeRow, List<List<Dictionary<string, object>>> valueList, int resultSetIndex)
        {
            List<Row> result = new List<Row>();
            PropertyInfo[] propertyInfoList = UtilDalType.TypeRowToPropertyInfoList(typeRow);
            foreach (Dictionary<string, object> row in valueList[resultSetIndex])
            {
                Row rowResult = Data.RowCreate(typeRow);
                foreach (string columnName in row.Keys)
                {
                    object value = row[columnName];
                    PropertyInfo propertyInfo = propertyInfoList.Where(item => item.Name == columnName).FirstOrDefault();
                    if (propertyInfo != null)
                    {
                        propertyInfo.SetValue(rowResult, value);
                    }
                }
                result.Add(rowResult);
            }
            return result;
        }

        /// <summary>
        /// Convert sql multiple result set to one typed row list.
        /// </summary>
        /// <typeparam name="T">Type of row to copy to. Copy one multiple result set.</typeparam>
        /// <param name="valueList">Result of method Execute(); multiple result set.</param>
        /// <param name="resultSetIndex">Sql multiple result set index.</param>
        /// <returns>Type of row to copy to. Copy one multiple result set.</returns>
        internal static List<T> ExecuteReaderMultipleResultCopy<T>(List<List<Dictionary<string, object>>> valueList, int resultSetIndex) where T : Row
        {
            List<Row> result = ExecuteReaderMultipleResultCopy(typeof(T), valueList, resultSetIndex);
            return result.Cast<T>().ToList();
        }

        /// <summary>
        /// Returns linq to database query.
        /// </summary>
        public static IQueryable Query(Type typeRow, DatabaseEnum databaseEnum = DatabaseEnum.Database)
        {
            switch (databaseEnum)
            {
                case DatabaseEnum.Database:
                    return DbContextInternalCreate(typeRow, isQuery: true).Query;
                case DatabaseEnum.Memory:
                    return DatabaseMemoryInternal.Instance.RowListGet(typeRow).AsQueryable();
                case DatabaseEnum.Custom:
                    throw new Exception("Use for example ComponentJson.MyList.AsQueryable(); instead!");
                default:
                    throw new Exception("Scope not supported!");
            }
        }

        /// <summary>
        /// Returns linq to database query.
        /// </summary>
        public static IQueryable<TRow> Query<TRow>(DatabaseEnum databaseEnum = DatabaseEnum.Database) where TRow : Row
        {
            return (IQueryable<TRow>)Query(typeof(TRow), databaseEnum);
        }

        /// <summary>
        /// Returns empty query to clear data grid and keep column definition.
        /// </summary>
        public static IQueryable<TRow> QueryEmpty<TRow>() where TRow: Row
        {
            return Enumerable.Empty<TRow>().AsQueryable();
        }

        /// <summary>
        /// Copy data row. Source and dest need not to be of same type. Only cells available on
        /// both records are copied.
        /// </summary>
        public static void RowCopy(Row rowSource, Row rowDest, string fieldNameSourcePrefix = null)
        {
            var propertyInfoDestList = UtilDalType.TypeRowToPropertyInfoList(rowDest.GetType());
            foreach (PropertyInfo propertyInfoDest in propertyInfoDestList)
            {
                string fieldNameDest = propertyInfoDest.Name;
                string fieldNameSource = fieldNameSourcePrefix + fieldNameDest;
                PropertyInfo propertyInfoSource = rowSource.GetType().GetProperty(fieldNameSource);
                if (propertyInfoSource != null)
                {
                    object value = propertyInfoSource.GetValue(rowSource);
                    propertyInfoDest.SetValue(rowDest, value);
                }
            }
        }

        /// <summary>
        /// Clone data row.
        /// </summary>
        public static Row RowCopy(Row row)
        {
            Row result = (Row)UtilFramework.TypeToObject(row.GetType());
            RowCopy(row, result);
            return result;
        }

        /// <summary>
        /// Clone data row.
        /// </summary>
        public static TRow RowCopy<TRow>(TRow row) where TRow : Row
        {
            return (TRow)RowCopy((Row)row);
        }

        /// <summary>
        /// Copy data row. Source and dest need not to be of same type. Only cells available on
        /// both records are copied.
        /// </summary>
        public static TRow RowCopy<TRow>(Row row, string fieldNameSourcePrefix = null) where TRow : Row
        {
            TRow result = (TRow)(object)UtilFramework.TypeToObject(typeof(TRow));
            RowCopy(row, result, fieldNameSourcePrefix);
            return result;
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
        /// Returns new data row.
        /// </summary>
        public static Row RowCreate(Type typeRow)
        {
            return (Row)UtilFramework.TypeToObject(typeRow);
        }

        /// <summary>
        /// Execute query and select data from database.
        /// </summary>
        internal static List<Row> QueryExecute(this IQueryable query)
        {
            return query.ToDynamicList().Cast<Row>().ToList();
        }

        /// <summary>
        /// Execute query and select data from database.
        /// </summary>
        internal static List<TRow> QueryExecute<TRow>(this IQueryable<TRow> query) where TRow : Row
        {
            return query.ToDynamicList().Cast<TRow>().ToList();
        }

        /// <summary>
        /// Execute query and select data from database.
        /// </summary>
        public static Task<List<Row>> QueryExecuteAsync(this IQueryable query)
        {
            UtilFramework.LogDebug(string.Format("SELECT ({0})", query.ElementType.Name));

            return query.ToDynamicListAsync().ContinueWith(list => list.Result.Cast<Row>().ToList());
        }

        /// <summary>
        /// Execute query and select data from database.
        /// </summary>
        public static Task<List<TRow>> QueryExecuteAsync<TRow>(this IQueryable<TRow> query) where TRow : Row
        {
            return ((IQueryable)query).QueryExecuteAsync().ContinueWith(list => list.Result.Cast<TRow>().ToList());
        }

        internal static IQueryable QueryFilter(IQueryable query, string fieldName, object filterValue, FilterOperatorEnum filterOperatorEnum)
        {
            string predicate = fieldName;
            switch (filterOperatorEnum)
            {
                case FilterOperatorEnum.Equal:
                    predicate += " = @0";
                    break;
                case FilterOperatorEnum.Smaller:
                    predicate += " <= @0";
                    break;
                case FilterOperatorEnum.Greater:
                    predicate += " >= @0";
                    break;
                case FilterOperatorEnum.Like:
                    predicate += ".Contains(@0)";
                    break;
                default:
                    throw new Exception("Enum unknown!");
            }

            if (filterValue is DateTime)
            {
                // In order to prevent "Conversion failed when converting date and/or time from character string." exception for sql type "datetime"
                // dynamic linq where parameter needs to be passed as string and not as DateTime.
                //
                // SQL datetime
                // 2008-09-04 00:00:00.000
                //
                // SQL datetime2
                // 1979-02-03T00:00:00.0000000

                filterValue = ((DateTime)filterValue).ToString("yyyy-MM-dd HH:mmm:ss.fff");
            }

            return query.Where(predicate, filterValue);
        }

        /// <summary>
        /// Sql orderby.
        /// </summary>
        internal static IOrderedQueryable QueryOrderBy(IQueryable query, string fieldName, bool isSort)
        {
            if (isSort == true)
            {
                fieldName += " DESC";
            }
            return query.OrderBy(fieldName);
        }

        /// <summary>
        /// Sql orderby.
        /// </summary>
        internal static IOrderedQueryable QueryOrderByThenBy(IOrderedQueryable query, string fieldName, bool isSort)
        {
            if (isSort == true)
            {
                fieldName += " DESC";
            }
            return query.ThenBy(fieldName);
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
            DbContext dbContext = DbContextInternalCreate(row.GetType(), isQuery: false);
            dbContext.Remove(row);
            int count = await dbContext.SaveChangesAsync();
            UtilFramework.Assert(count == 1, "Update failed!");
        }

        /// <summary>
        /// Insert data record. Primary key needs to be 0! Row contains new primary key after insert.
        /// </summary>
        public static async Task InsertAsync<TRow>(TRow row, DatabaseEnum databaseEnum = DatabaseEnum.Database) where TRow : Row
        {
            UtilFramework.LogDebug(string.Format("INSERT ({0})", row.GetType().Name));

            switch (databaseEnum)
            {
                case DatabaseEnum.Database:
                    {
                        Row rowCopy = Data.RowCopy(row);
                        DbContext dbContext = DbContextInternalCreate(row.GetType(), isQuery: false);
                        dbContext.Add(row); // Throws NullReferenceException if no primary key is defined. // EF sets auto increment field to 2147482647.
                        try
                        {
                            int count = await dbContext.SaveChangesAsync(); // Override method GridInsertAsync(); for sql view.
                            UtilFramework.Assert(count == 1, "Update failed!");
                            //
                            // Exception: Database operation expected to affect 1 row(s) but actually affected 0 row(s). 
                            // Cause: No autoincrement on Id column or no Id set by application
                            //
                            // Exception: The conversion of a datetime2 data type to a datetime data type resulted in an out-of-range value.
                            // Cause: CSharp not nullable DateTime default value is "{1/1/0001 12:00:00 AM}" change it to nullable or set value for example to DateTime.Now
                        }
                        catch
                        {
                            Data.RowCopy(rowCopy, row); // In case of exception, auto increment id stays -2147482647. Reverse it back.
                            throw;
                        }
                        break;
                    }
                case DatabaseEnum.Memory:
                    {
                        var rowList = Data.MemoryRowList(row.GetType(), databaseEnum);
                        rowList.Add(row);
                        break;
                    }
                case DatabaseEnum.Custom:
                    throw new Exception("Override method Grid.Insert(); and set result.IsHandled flag for custom data source!");
                default:
                    throw new Exception("Scope not supported!");
            }
        }

        /// <summary>
        /// Update data record on database.
        /// </summary>
        public static async Task UpdateAsync(Row rowOld, Row row, DatabaseEnum databaseEnum = DatabaseEnum.Database)
        {
            UtilFramework.LogDebug(string.Format("UPDATE ({0})", rowOld.GetType().Name));

            UtilFramework.Assert(rowOld.GetType() == row.GetType());
            // if (Data.RowEqual(rowOld, row) == false) // See also: EntityState.Modified
            {
                switch (databaseEnum)
                {
                    case DatabaseEnum.Database:
                        {
                            var rowOldLocal = Data.RowCopy(rowOld); // Prevent modifications on SetValues(row);
                            DbContext dbContext = Data.DbContextInternalCreate(rowOld.GetType(), isQuery: false);
                            var tracking = dbContext.Attach(rowOldLocal);
                            tracking.CurrentValues.SetValues(row);
                            if (tracking.State == EntityState.Modified)
                            {
                                // Called by data grid.
                            }
                            if (tracking.State == EntityState.Unchanged)
                            {
                                // Called by method Data.UpdateAsync(); for table.
                                tracking.State = EntityState.Modified; 
                            }
                            int count = await dbContext.SaveChangesAsync(); // Override method GridUpdateAsync(); for sql view.
                            UtilFramework.Assert(count == 1, "Update failed!");
                            break;
                        }
                    case DatabaseEnum.Memory:
                        {
                            var rowList = Data.MemoryRowList(rowOld.GetType(), databaseEnum);
                            PropertyInfo propertyInfo = UtilDalType.TypeRowToPropertyInfoList(rowOld.GetType()).First(); // Assume first field is primary key.
                            object idNew = propertyInfo.GetValue(rowOld);
                            int updateCount = 0;
                            foreach (Row rowMemory in rowList.Cast<Row>())
                            {
                                object id = propertyInfo.GetValue(rowMemory);
                                if (object.Equals(id, idNew))
                                {
                                    Data.RowCopy(row, rowMemory);
                                    updateCount += 1;
                                }
                            }
                            if (updateCount == 0)
                            {
                                throw new Exception("Memory row could not be updated!");
                            }
                            if (updateCount > 1)
                            {
                                throw new Exception("More than one memory row updated!");
                            }
                            break;
                        }
                    case DatabaseEnum.Custom:
                        throw new Exception("Override method Grid.Update(); and set result.IsHandled flag for custom data source!");
                    default:
                        throw new Exception("Scope not supported!");
                }
            }
        }

        /// <summary>
        /// Update data record on database.
        /// </summary>
        public static Task UpdateAsync(Row row, DatabaseEnum databaseEnum = DatabaseEnum.Database)
        {
            return UpdateAsync(row, row, databaseEnum);
        }

        /// <summary>
        /// Parse user entered cell and filter text. Text can be null.
        /// </summary>
        private static object CellTextParse(Field field, string text)
        {
            object result = field.FrameworkType().CellTextParse(text);
            return result;
        }

        /// <summary>
        /// Parse user entered cell text and write it to row. Text can be null.
        /// </summary>
        internal static void CellTextParse(Field field, string text, Row row, out string errorParse)
        {
            errorParse = null;
            object value = CellTextParse(field, text);
            bool isNullable = UtilFramework.IsNullable(field.PropertyInfo.PropertyType); // Do not write value to row if type is not nullable but text is null.
            bool isPrevent = (text == null) && !isNullable;
            if (!isPrevent)
            {
                field.PropertyInfo.SetValue(row, value);
            }
        }

        /// <summary>
        /// Default parse user entered filter text. Text can be null.
        /// </summary>
        internal static void CellTextParseFilter(Field field, string text, GridFilter filter, out string errorParse)
        {
            errorParse = null;
            object filterValue = CellTextParse(field, text);
            FilterOperatorEnum filterOperatorEnum = FilterOperatorEnum.Equal;
            if (field.PropertyInfo.PropertyType == typeof(string))
            {
                filterOperatorEnum = FilterOperatorEnum.Like;
            }
            filter.ValueSet(field.PropertyInfo.Name, filterValue, filterOperatorEnum, text, isClear: text == null);
        }
    }

    public enum FilterOperatorEnum
    {
        None = 0,
        Equal = 1,
        Smaller = 2,
        Greater = 3,
        Like = 4
    }

    internal class UtilDalUpsert
    {
        internal static string UpsertFieldNameToCsvList(string[] fieldNameSqlList, string prefix)
        {
            string result = null;
            bool isFirst = true;
            foreach (string fieldName in fieldNameSqlList)
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

        internal static string UpsertFieldNameToAssignList(string[] fieldNameSqlList, string prefixTarget, string prefixSource)
        {
            string result = null;
            bool isFirst = true;
            foreach (string fieldName in fieldNameSqlList)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    result += ", ";
                }
                result += prefixTarget + fieldName + " = " + prefixSource + fieldName;
            }
            return result;
        }

        private static string UpsertSelect(Type typeRow, List<Row> rowList, List<(FrameworkTypeEnum FrameworkTypeEnum, SqlParameter SqlParameter)> paramList)
        {
            StringBuilder sqlSelect = new StringBuilder();
            var fieldList = UtilDalType.TypeRowToFieldList(typeRow);

            // Row
            bool isFirstRow = true;
            foreach (Row row in rowList)
            {
                UtilFramework.Assert(row.GetType() == typeRow);
                if (isFirstRow)
                {
                    isFirstRow = false;
                }
                else
                {
                    sqlSelect.Append(" UNION ALL\r\n");
                }

                // Field
                sqlSelect.Append("(SELECT ");
                bool isFirstField = true;
                foreach (var field in fieldList)
                {
                    if (isFirstField)
                    {
                        isFirstField = false;
                    }
                    else
                    {
                        sqlSelect.Append(", ");
                    }
                    object value = field.PropertyInfo.GetValue(row);
                    string paramName = Data.ExecuteParamAdd(field.FrameworkTypeEnum, value, paramList);
                    sqlSelect.Append(string.Format("{0} AS {1}", paramName, field.FieldNameSql));
                }
                sqlSelect.Append(")");
            }
            return sqlSelect.ToString();
        }

        internal static async Task UpsertAsync(Type typeRow, List<Row> rowList, string[] fieldNameKeyList)
        {
            if (rowList.Count == 0)
            {
                return;
            }

            string tableNameWithSchemaSql = UtilDalType.TypeRowToTableNameWithSchemaSql(typeRow);
            bool isFrameworkDb = UtilDalType.TypeRowIsFrameworkDb(typeRow);
            var fieldNameSqlList = UtilDalType.TypeRowToFieldList(typeRow).Where(item => item.IsPrimaryKey == false).Select(item => item.FieldNameSql).ToArray();

            string fieldNameKeySourceList = UpsertFieldNameToCsvList(fieldNameKeyList, "Source.");
            string fieldNameKeyTargetList = UpsertFieldNameToCsvList(fieldNameKeyList, "Target.");
            string fieldNameAssignList = UpsertFieldNameToAssignList(fieldNameSqlList, "Target.", "Source.");
            string fieldNameInsertList = UpsertFieldNameToCsvList(fieldNameSqlList, null);
            string fieldNameValueList = UpsertFieldNameToCsvList(fieldNameSqlList, "Source.");

            var paramList = new List<(FrameworkTypeEnum FrameworkTypeEnum, SqlParameter SqlParameter)>();
            string sqlSelect = UpsertSelect(typeRow, rowList, paramList);
            // string sqlDebug = Data.ExecuteParamDebug(sqlSelect, paramList);
            // var resultDebug = await Data.ExecuteReaderAsync(typeRow, sqlDebug);

            string sqlUpsert = @"
            MERGE INTO {0} AS Target
            USING ({1}) AS Source
	        ON NOT EXISTS(
                SELECT {2}
                EXCEPT
                SELECT {3})
            WHEN MATCHED THEN
	            UPDATE SET 
                    {4}
            WHEN NOT MATCHED BY TARGET THEN
	            INSERT ({5})
	            VALUES ({6});
            ";
            sqlUpsert = string.Format(sqlUpsert, tableNameWithSchemaSql, sqlSelect, fieldNameKeySourceList, fieldNameKeyTargetList, fieldNameAssignList, fieldNameInsertList, fieldNameValueList);
            // string sqlDebug = Data.ExecuteParamDebug(sqlUpsert, paramList);

            // Upsert
            await Data.ExecuteNonQueryAsync(sqlUpsert, paramList, isFrameworkDb);
        }

        internal static async Task UpsertAsync<TRow>(List<TRow> rowList, string[] fieldNameKeyList) where TRow : Row
        {
            await UpsertAsync(typeof(TRow), rowList.Cast<Row>().ToList(), fieldNameKeyList);
        }

        internal static async Task UpsertAsync<TRow>(List<TRow> rowList, string fieldNameKey) where TRow : Row
        {
            await UpsertAsync(rowList, new string[] { fieldNameKey });
        }

        /// <summary>
        /// Set IsDelete flag to false on sql table.
        /// </summary>
        internal static async Task UpsertIsDeleteAsync(Type typeRow)
        {
            string fieldNameSqlIsDelete = "IsDelete";
            string tableNameWithSchemaSql = UtilDalType.TypeRowToTableNameWithSchemaSql(typeRow);
            bool isFrameworkDb = UtilDalType.TypeRowIsFrameworkDb(typeRow);
            // IsDeletes
            string sqlIsDelete = string.Format("UPDATE {0} SET {1}=CAST(0 AS BIT)", tableNameWithSchemaSql, fieldNameSqlIsDelete);
            await Data.ExecuteNonQueryAsync(sqlIsDelete, null, isFrameworkDb);
        }

        /// <summary>
        /// Overload.
        /// </summary>
        internal static async Task UpsertIsDeleteAsync<TRow>() where TRow : Row
        {
            await UpsertIsDeleteAsync(typeof(TRow));
        }
    }

    internal class UtilDalUpsertIntegrate
    {
        internal class FieldIntegrate
        {
            /// <summary>
            /// Gets or sets Field. See also method UtilDalType.TypeRowToFieldList();
            /// </summary>
            public Field Field;

            /// <summary>
            /// Gets or sets IsKey. True, if "Id" or "IdName".
            /// </summary>
            public bool IsKey;

            /// <summary>
            /// Gets or sets IsIdName. True, if for example "TableIdName".
            /// </summary>
            public bool IsIdName;

            /// <summary>
            /// Gets or sets "IsId". True, if for example "TableId".
            /// </summary>
            public bool IsId;

            /// <summary>
            /// Gets or sets FieldNameIdSql. For example "TableId".
            /// </summary>
            public string FieldNameIdSql;

            /// <summary>
            /// Gets or sets TypeRowReference. Referenced table (or view) containing field "Id" and "Name". For example view "FrameworkTableIntegrate".
            /// </summary>
            public Type TypeRowReference;
        }

        /// <summary>
        /// Returns Integrate reference table.
        /// </summary>
        /// <param name="typeRow">Integrate table.</param>
        private static Type TypeRowReferenceIntegrate(Type typeRow, string fieldNameIdSql, List<Reference> referenceList)
        {
            var result = referenceList.SingleOrDefault(item => item.TypeRowIntegrate == typeRow && item.FieldNameIdSql == fieldNameIdSql)?.TypeRowReferenceIntegrate;
            return result;
        }

        /// <summary>
        /// Defines a reference table.
        /// </summary>
        internal class Reference
        {
            public Reference(Type typeRow, string fieldNameIdCSharp, string fieldNameIdSql, Type typeRowIntegrate, string fieldNameIdNameCSharp, string fieldNameIdNameSql, Type typeRowReference, Type typeRowReferenceIntegrate)
            {
                TypeRow = typeRow;
                FieldNameIdCSharp = fieldNameIdCSharp;
                FieldNameIdSql = fieldNameIdSql;
                TypeRowIntegrate = typeRowIntegrate;
                FieldNameIdNameCSharp = fieldNameIdNameCSharp;
                FieldNameIdNameSql = fieldNameIdNameSql;
                TypeRowReference = typeRowReference;
                TypeRowReferenceIntegrate = typeRowReferenceIntegrate;
            }

            /// <summary>
            /// Gets TypeRow. For example: "LoginUserRole"
            /// </summary>
            public readonly Type TypeRow;

            /// <summary>
            /// Gets FieldNameIdCSharp. For example: "UserId"
            /// </summary>
            public readonly string FieldNameIdCSharp;

            public readonly string FieldNameIdSql;

            /// <summary>
            /// Gets TypeRowIntegrate. For example: "LoginUserRoleIntegrate".
            /// </summary>
            public readonly Type TypeRowIntegrate;

            /// <summary>
            /// Gets FieldNameIdNameCSharp. For example "UserIdName".
            /// </summary>
            public readonly string FieldNameIdNameCSharp;

            public readonly string FieldNameIdNameSql;

            /// <summary>
            /// Gets TypeRowReference. For example: "LoginUser"
            /// </summary>
            public readonly Type TypeRowReference;

            /// <summary>
            /// Gets TypeRowReferenceIntegrate. For example: "LoginUserIntegrate"
            /// </summary>
            public readonly Type TypeRowReferenceIntegrate;
        }

        /// <summary>
        /// Returns list of FieldIntegrate for TypeRow.
        /// </summary>
        /// <param name="typeRow">Data row type.</param>
        internal static List<FieldIntegrate> FieldIntegrateList(Type typeRow, List<Reference> referenceList)
        {
            List<FieldIntegrate> result = new List<FieldIntegrate>();
            var fieldList = UtilDalType.TypeRowToFieldList(typeRow);
            var fieldNameSqlList = fieldList.Select(item => item.FieldNameSql).ToList();

            // Populate result
            foreach (var field in fieldList)
            {
                FieldIntegrate fieldIntegrate = new FieldIntegrate
                {
                    Field = field
                };
                result.Add(fieldIntegrate);
            }

            foreach (var fieldIntegrate in result)
            {
                string fieldNameSql = fieldIntegrate.Field.FieldNameSql;

                fieldIntegrate.IsKey = fieldNameSql == "Id" || fieldNameSql == "IdName";

                string lastChar = ""; // Character before "IdName".
                if (fieldNameSql.Length > "IdName".Length)
                {
                    lastChar = fieldNameSql.Substring(fieldNameSql.Length - "IdName".Length - 1, 1);
                }
                bool lastCharIsLower = lastChar == lastChar.ToLower() && lastChar.Length == 1;
                if (fieldNameSql.EndsWith("IdName") && lastCharIsLower) // Integrate naming convention.
                {
                    string fieldNameIdSql = fieldNameSql.Substring(0, fieldNameSql.Length - "Name".Length); // Integrate naming convention.
                    if (fieldNameSqlList.Contains(fieldNameIdSql))
                    {
                        UtilDalType.TypeRowToTableNameSql(typeRow, out string schemaNameSql, out string tableNameSql);
                        // Find reference table
                        Type typeRowReference = TypeRowReferenceIntegrate(typeRow, fieldNameIdSql, referenceList);

                        if (typeRowReference != null)
                        {
                            List<string> propertyNameList = UtilDalType.TypeRowToPropertyInfoList(typeRowReference).Select(item => item.Name).ToList();
                            if (propertyNameList.Contains("Id") && propertyNameList.Contains("IdName")) // Integrate naming convention.
                            {
                                // IdName
                                fieldIntegrate.IsIdName = true;
                                fieldIntegrate.TypeRowReference = typeRowReference;
                                fieldIntegrate.FieldNameIdSql = fieldNameIdSql;

                                // Id
                                var fieldIntegrateId = result.Where(item => item.Field.FieldNameSql == fieldNameIdSql).Single();
                                fieldIntegrateId.IsId = true;
                                fieldIntegrateId.TypeRowReference = typeRowReference;
                                fieldIntegrateId.FieldNameIdSql = fieldNameIdSql;
                            }
                        }
                    }
                }
            }
            return result;
        }

        private static string UpsertSelect(Type typeRow, List<Row> rowList, List<Reference> referenceList, List<(FrameworkTypeEnum FrameworkTypeEnum, SqlParameter SqlParameter)> paramList)
        {
            StringBuilder sqlSelect = new StringBuilder();
            var fieldIntegrateList = FieldIntegrateList(typeRow, referenceList);

            // Row
            bool isFirstRow = true;
            foreach (Row row in rowList)
            {
                UtilFramework.Assert(row.GetType() == typeRow);
                if (isFirstRow)
                {
                    isFirstRow = false;
                }
                else
                {
                    sqlSelect.Append(" UNION ALL\r\n");
                }

                // Field
                sqlSelect.Append("(SELECT ");
                bool isFirstField = true;
                foreach (var fieldIntegrate in fieldIntegrateList)
                {
                    bool isField = (fieldIntegrate.IsId == false && fieldIntegrate.IsIdName == false && fieldIntegrate.IsKey == false) || fieldIntegrate.IsIdName;
                    if (isField)
                    {
                        if (isFirstField)
                        {
                            isFirstField = false;
                        }
                        else
                        {
                            sqlSelect.Append(", ");
                        }
                        string fieldNameSql = fieldIntegrate.Field.FieldNameSql;
                        object value = fieldIntegrate.Field.PropertyInfo.GetValue(row);
                        string paramName = Data.ExecuteParamAdd(fieldIntegrate.Field.FrameworkTypeEnum, value, paramList);
                        if (fieldIntegrate.IsId == false && fieldIntegrate.IsIdName == false)
                        {
                            sqlSelect.Append(string.Format("{0} AS {1}", paramName, fieldIntegrate.Field.FieldNameSql));
                        }
                        else
                        {
                            if (fieldIntegrate.IsIdName)
                            {
                                string tableNameSql = UtilDalType.TypeRowToTableNameCSharp(fieldIntegrate.TypeRowReference);
                                string sqlIntegrate = string.Format("(SELECT Integrate.Id FROM {0} Integrate WHERE Integrate.IdName = {1}) AS {2}", tableNameSql, paramName, fieldIntegrate.FieldNameIdSql);
                                sqlSelect.Append(sqlIntegrate);
                            }
                        }
                    }
                }
                sqlSelect.Append(")");
            }
            return sqlSelect.ToString();
        }

        /// <summary>
        /// Set IsDelete property to true on row and to false on sql table.
        /// </summary>
        private static void IsDeleteSet(Type typeRow, List<Row> rowList)
        {
            var fieldList = UtilDalType.TypeRowToFieldListDictionary(typeRow);
            if (fieldList.TryGetValue("IsDelete", out Field field))
            {
                foreach (var row in rowList)
                {
                    field.PropertyInfo.SetValue(row, false);
                }

                // Set sql table IsDelete to true where IsIntegrate is true (if column exists)
                UtilDalUpsertIntegrate.UpsertIsDeleteAsync(typeRow).Wait();
            }
        }

        /// <summary>
        /// Sql merge into for Integrate.
        /// </summary>
        /// <param name="typeRow">Type of rowList (can be empty).</param>
        /// <param name="typeRowDest">Type underlying sql table.</param>
        /// <param name="rowList">Records to update.</param>
        /// <param name="fieldNameSqlKeyList">Key fields for record identification.</param>
        private static async Task UpsertAsync(Type typeRow, Type typeRowDest, List<Row> rowList, string[] fieldNameSqlKeyList, List<Reference> referenceList)
        {
            bool isFrameworkDb = UtilDalType.TypeRowIsFrameworkDb(typeRow);

            var fieldNameSqlListAll = FieldIntegrateList(typeRow, referenceList);

            foreach (var rowListSplit in UtilFramework.Split(rowList, 100)) // Prevent error: "The server supports a maximum of 2100 parameters"
            {
                var paramList = new List<(FrameworkTypeEnum FrameworkTypeEnum, SqlParameter SqlParameter)>();
                string sqlSelect = UpsertSelect(typeRow, rowListSplit, referenceList, paramList);

                // Update underlying sql table if sql view ends with "Integrate".
                UtilDalType.TypeRowToTableNameSql(typeRowDest, out string schemaNameSql, out string tableNameSql);
                string tableNameWithSchemaSql = UtilDalType.TableNameWithSchemaSql(schemaNameSql, tableNameSql);
                var fieldDestList = UtilDalType.TypeRowToFieldListDictionary(typeRowDest);

                var fieldNameSqlList = fieldNameSqlListAll
                    .Where(item => item.IsIdName == false && item.Field.IsPrimaryKey == false && item.IsKey == false && fieldDestList.ContainsKey(item.Field.FieldNameCSharp))
                    .Select(item => item.Field.FieldNameSql).ToArray();

                string fieldNameKeySourceList = UtilDalUpsert.UpsertFieldNameToCsvList(fieldNameSqlKeyList, "Source.");
                string fieldNameKeyTargetList = UtilDalUpsert.UpsertFieldNameToCsvList(fieldNameSqlKeyList, "Target.");
                string fieldNameAssignList = UtilDalUpsert.UpsertFieldNameToAssignList(fieldNameSqlList, "Target.", "Source.");
                string fieldNameInsertList = UtilDalUpsert.UpsertFieldNameToCsvList(fieldNameSqlList, null);
                string fieldNameValueList = UtilDalUpsert.UpsertFieldNameToCsvList(fieldNameSqlList, "Source.");

                string sqlUpsert = @"
                MERGE INTO {0} AS Target
                USING 
                
                -- Source (sqlSelect)
                ({1}) 

                AS Source
	            ON NOT EXISTS(
                    SELECT {2}
                    EXCEPT
                    SELECT {3})
                WHEN MATCHED THEN
	                UPDATE SET 
                        {4}
                WHEN NOT MATCHED BY TARGET THEN
	                INSERT ({5})
	                VALUES ({6});
                ";
                sqlUpsert = string.Format(sqlUpsert, tableNameWithSchemaSql, sqlSelect, fieldNameKeySourceList, fieldNameKeyTargetList, fieldNameAssignList, fieldNameInsertList, fieldNameValueList);
                // string sqlDebug = Data.ExecuteParamDebug(sqlUpsert, paramList);

                // Upsert
                await Data.ExecuteNonQueryAsync(sqlUpsert, paramList, isFrameworkDb); // See also method AppCli.CommandGenerateIntegrate();
            }
        }

        /// <summary>
        /// List of rows to insert or update.
        /// </summary>
        internal class UpsertItem
        {
            private UpsertItem(Type typeRow, List<Row> rowList, string[] fieldNameSqlKeyList, List<Reference> referenceList)
            {
                this.TypeRow = typeRow;
                this.RowList = rowList;
                this.FieldNameSqlKeyList = fieldNameSqlKeyList;
                this.ReferenceList = referenceList;

                foreach (var item in RowList)
                {
                    UtilFramework.Assert(item.GetType() == TypeRow);
                }
            }

            public static UpsertItem Create<TRow>(List<TRow> rowList, string[] fieldNameSqlKeyList, List<Reference> referenceList)
            {
                return new UpsertItem(typeof(TRow), rowList.Cast<Row>().ToList(), fieldNameSqlKeyList, referenceList);
            }

            /// <summary>
            /// Gets TypeRow. For example: "[dbo].[LoginUser]" or "[dbo].[LoginUserIntegrate]".
            /// </summary>
            public readonly Type TypeRow;

            /// <summary>
            /// Returns underlying sql table if sql view ends with "Integrate". For example "[dbo].[LoginUser]".
            /// </summary>
            public Type TypeRowDest(List<Assembly> assemblyList)
            {
                UtilDalType.TypeRowToTableNameSql(TypeRow, out string schemaNameSql, out string tableNameSql);
                if (tableNameSql.EndsWith("Integrate"))
                {
                    tableNameSql = tableNameSql.Substring(0, tableNameSql.Length - "Integrate".Length);
                }
                Type typeRowDest = UtilDalType.TypeRowFromTableNameSql(schemaNameSql, tableNameSql, assemblyList);

                return typeRowDest;
            }

            /// <summary>
            /// Gets RowList. Rows to insert or update.
            /// </summary>
            public readonly List<Row> RowList;

            /// <summary>
            /// Gets FieldNameSqlKeyList. Sql unique index for upsert to identify record. For example (UserId, RoleId).
            /// </summary>
            public readonly string[] FieldNameSqlKeyList;

            public readonly List<Reference> ReferenceList;

            /// <summary>
            /// Gets IsDeploy. True, if RowList is deployed to database.
            /// </summary>
            public bool IsDeploy { get; internal set; }
        }

        /// <summary>
        /// Sql merge into for Integrate.
        /// </summary>
        /// <param name="upsertList">List of rows to insert or update.</param>
        /// <param name="assemblyList">Assemblies in which to search reference tables.</param>
        internal static async Task UpsertAsync(List<UpsertItem> upsertList, List<Assembly> assemblyList, Action<Type> progressBar = null, bool isExceptionContinue = false)
        {
            upsertList = upsertList.Where(item => item.IsDeploy == false).ToList();

            // Group by TypeRow
            Dictionary<Type, List<Row>> typeRowToRowList = new Dictionary<Type, List<Row>>();
            foreach (var item in upsertList)
            {
                if (!typeRowToRowList.ContainsKey(item.TypeRow))
                {
                    typeRowToRowList[item.TypeRow] = new List<Row>();
                }
                typeRowToRowList[item.TypeRow].AddRange(item.RowList);
            }

            foreach (var item in typeRowToRowList)
            {
                Type typeRow = item.Key;
                List<Row> rowList = item.Value;

                try
                {
                    // IsDeleteSet
                    IsDeleteSet(typeRow, rowList); // One call for hierarchical Integrate which needs multiple upsert.

                    // Upsert
                    foreach (var itemUpsert in upsertList.Where(item => item.TypeRow == typeRow))
                    {
                        Type typeRowDest = itemUpsert.TypeRowDest(assemblyList);
                        progressBar?.Invoke(typeRowDest);
                        await UpsertAsync(itemUpsert.TypeRow, typeRowDest, itemUpsert.RowList, itemUpsert.FieldNameSqlKeyList, itemUpsert.ReferenceList);
                    }
                }
                catch
                {
                    if (isExceptionContinue)
                    {
                        Console.WriteLine();
                        Console.WriteLine(string.Format("Upsert failed! ({0})", typeRow.Name));
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Set IsDeploy
            foreach (var item in upsertList)
            {
                item.IsDeploy = true;
            }
        }

        /// <summary>
        /// Overload.
        /// </summary>
        internal static Task UpsertAsync(UpsertItem upsertList, List<Assembly> assemblyList)
        {
            return UpsertAsync(new List<UpsertItem>(new UpsertItem[] { upsertList }), assemblyList);
        }

        /// <summary>
        /// Set IsDelete flag to false on sql table. If sql table contains IsIntegrate column set IsDelete flag to false only on IsIntegrate data rows.
        /// </summary>
        private static async Task UpsertIsDeleteAsync(Type typeRow)
        {
            var fieldList = UtilDalType.TypeRowToFieldListDictionary(typeRow);
            if (!fieldList.ContainsKey("IsIntegrate"))
            {
                await UtilDalUpsert.UpsertIsDeleteAsync(typeRow);
            }
            else
            {
                string fieldNameSqlIsDelete = "IsDelete";
                string tableNameWithSchemaSql = UtilDalType.TypeRowToTableNameWithSchemaSql(typeRow);
                bool isFrameworkDb = UtilDalType.TypeRowIsFrameworkDb(typeRow);
                // IsDeletes
                string sqlIsDelete = string.Format("UPDATE {0} SET {1}=CAST(1 AS BIT) WHERE IsIntegrate = 1", tableNameWithSchemaSql, fieldNameSqlIsDelete);
                await Data.ExecuteNonQueryAsync(sqlIsDelete, null, isFrameworkDb);
            }
        }
    }

    /// <summary>
    /// Framework type system.
    /// </summary>
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
        NChar = 23,
        Nvarcahr = 10,
        Varchar = 11,
        Text = 12,
        Ntext = 13,
        Bit = 14,
        Money = 15,
        Smallmoney = 24,
        Decimal = 16,
        Real = 17,
        Float = 18,
        Varbinary = 19,
        Sqlvariant = 20,
        Image = 21,
        Numeric = 22, // 24
    }

    internal static class UtilDalType
    {
        /// <summary>
        /// Returns true if typeRow is declared if Framework assembly.
        /// </summary>
        internal static bool TypeRowIsFrameworkDb(Type typeRow)
        {
            return typeRow.Assembly == UtilFramework.AssemblyFramework; // Type is declared in Framework assembly.
        }

        /// <summary>
        /// Returns TybleNameCSharp declared in assembly.
        /// </summary>
        /// <returns>(TypeRow, TybleNameCSharp)</returns>
        internal static Dictionary<Type, string> TableNameCSharpList(params Assembly[] assemblyList)
        {
            var result = new Dictionary<Type, string>();
            foreach (Assembly assembly in assemblyList)
            {
                if (assembly != null)
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (type.IsSubclassOf(typeof(Row))) // TypeRow
                        {
                            Type typeRow = type;
                            string tableNameCSharp = TypeRowToTableNameCSharp(typeRow);
                            result.Add(typeRow, tableNameCSharp);
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns true if typeRow has database table.
        /// </summary>
        internal static bool TypeRowIsTableNameSql(Type typeRow)
        {
            bool result = false;
            SqlTableAttribute tableAttribute = (SqlTableAttribute)typeRow.GetCustomAttribute(typeof(SqlTableAttribute));
            if (tableAttribute != null && (tableAttribute.SchemaNameSql != null || tableAttribute.TableNameSql != null))
            {
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Returns rows defined in "Database" namespace in assemblies.
        /// </summary>
        /// <param name="assemblyList">Use method AppCli.AssemblyList(); when running in cli mode or method UtilServer.AssemblyList(); when running in web mode.</param>
        internal static List<Type> TypeRowList(List<Assembly> assemblyList)
        {
            Dictionary<string, Type> result = new Dictionary<string, Type>();
            foreach (Assembly assembly in assemblyList)
            {
                if (assembly != null)
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (type.IsSubclassOf(typeof(Row)))
                        {
                            string name = UtilFramework.TypeToName(type);
                            if (name.StartsWith("Database."))
                            {
                                string tableNameCSharp = UtilDalType.TypeRowToTableNameCSharp(type);
                                if (result.ContainsKey(tableNameCSharp))
                                {
                                    throw new Exception(string.Format("TableNameCSharp exists already in different assembly! ({0})", tableNameCSharp));

                                }
                                result.Add(tableNameCSharp, type);
                            }
                            else
                            {
                                throw new Exception(string.Format("Row class not defined in Database namespace! ({0})", UtilFramework.TypeToName(type)));
                            }
                        }
                    }
                }
            }
            return result.Values.ToList();
        }

        /// <summary>
        /// Returns row type as string. For example: "dbo.User". Omits "Database" namespace prefix.
        /// </summary>
        internal static string TypeRowToTableNameCSharp(Type typeRow)
        {
            UtilFramework.Assert(UtilFramework.IsSubclassOf(typeRow, typeof(Row)), "Wrong type!");
            string result = UtilFramework.TypeToName(typeRow);
            UtilFramework.Assert(result.StartsWith("Database."), string.Format("Move calculated row to different namespace! For example Database.Calculated. ({0})", UtilFramework.TypeToName(typeRow))); // If it is a calculated row which does not exist on database move it for example to namespace "Database.Calculated".
            UtilFramework.Assert(typeRow.BaseType == typeof(Row), string.Format("Calculated row has to derive from type Row! ({0})", UtilFramework.TypeToName(typeRow)));
            result = result.Substring("Database.".Length); // Remove "DatabaseFramework" namespace.
            return result;
        }

        /// <summary>
        /// Returns row type as string. For example: "User". Omits "Database" namespace prefix and schema.
        /// </summary>
        internal static string TypeRowToTableNameCSharpWithoutSchema(Type typeRow)
        {
            string result = TypeRowToTableNameCSharp(typeRow);
            int index = result.IndexOf(".");
            if (index != -1)
            {
                result = result.Substring(index + ".".Length);
            }
            return result;
        }

        /// <summary>
        /// Returns SchemaNameCSharp. For example: "dbo".
        /// </summary>
        internal static string TypeRowToSchemaNameCSharp(Type typeRow)
        {
            string result = TypeRowToTableNameCSharp(typeRow);
            int index = result.IndexOf(".");
            if (index == -1)
            {
                result = null; // Sql table without schema
            }
            else
            {
                result = result.Substring(0, index);
            }
            return result;
        }

        /// <summary>
        /// Returns (TypeRow, TableNameWithSchemaSql) list.
        /// </summary>
        internal static Dictionary<Type, string> TableNameSqlList(List<Assembly> assemblyList)
        {
            Dictionary<Type, string> result = new Dictionary<Type, string>();
            List<Type> typeRowList = TypeRowList(assemblyList);
            foreach (Type typeRow in typeRowList)
            {
                string tableNameWithSchemaSql = TypeRowToTableNameWithSchemaSql(typeRow);
                result.Add(typeRow, tableNameWithSchemaSql);
            }
            return result;
        }

        /// <summary>
        /// Returns (TypeRow, TableNameWithSchemaSql) list.
        /// </summary>
        internal static Dictionary<Type, (string SchemaNameSql, string TableNameSql)> TableNameWithSchemaSqlList(List<Assembly> assemblyList)
        {
            var result = new Dictionary<Type, (string SchemaNameSql, string TableNameSql)>();
            List<Type> typeRowList = TypeRowList(assemblyList);
            foreach (Type typeRow in typeRowList)
            {
                TypeRowToTableNameSql(typeRow, out string schemaNameSql, out string tableNameSql);
                result.Add(typeRow, (schemaNameSql, tableNameSql));
            }
            return result;
        }

        /// <summary>
        /// Returns true, if typeRow contains sql information.
        /// </summary>
        internal static bool TypeRowToTableNameSql(Type typeRow, out string schemaNameSql, out string tableNameSql)
        {
            SqlTableAttribute tableAttribute = (SqlTableAttribute)typeRow.GetCustomAttribute(typeof(SqlTableAttribute));
            schemaNameSql = tableAttribute?.SchemaNameSql;
            tableNameSql = tableAttribute?.TableNameSql;
            return tableAttribute != null;
        }

        /// <summary>
        /// Returns TypeRow.
        /// </summary>
        /// <param name="assemblyList">Assemblies to scan for TypeRow.</param>
        internal static Type TypeRowFromTableNameSql(string schemaNameSql, string tableNameSql, List<Assembly> assemblyList)
        {
            List<Type> result = new List<Type>();
            foreach (Assembly assembly in assemblyList)
            {
                if (assembly != null)
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (UtilFramework.IsSubclassOf(type, typeof(Row))) // TypeRow
                        {
                            if (TypeRowToTableNameSql(type, out string schemaNameSqlLocal, out string tableNameSqlLocal))
                            {
                                if (schemaNameSqlLocal == schemaNameSql && tableNameSqlLocal == tableNameSql)
                                {
                                    result.Add(type);
                                }
                            }
                        }
                    }
                }
            }
            return result.Single();
        }

        /// <summary>
        /// Returns (TypeRow, TableNameCSharp) from TableNameCSharp if declared in assembly.
        /// </summary>
        /// <param name="tableNameCSharpList">For example: "dbo.FrameworkScript"</param>
        /// <param name="assemblyList">Assemblies in which to search for TypeRow.</param>
        internal static Dictionary<Type, string> TypeRowFromTableNameCSharpList(List<string> tableNameCSharpList, List<Assembly> assemblyList)
        {
            var result = new Dictionary<Type, string>();
            tableNameCSharpList = tableNameCSharpList.Distinct().ToList();
            foreach (Assembly assembly in assemblyList)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (UtilFramework.IsSubclassOf(type, typeof(Row)))
                    {
                        string tableNameCSharp = UtilDalType.TypeRowToTableNameCSharp(type);
                        if (tableNameCSharpList.Contains(tableNameCSharp))
                        {
                            result.Add(type, tableNameCSharp);
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns FieldNameCSharp declared in assembly.
        /// </summary>
        /// <returns>(TableNameCSharp, FieldNameCSharp)</returns>
        internal static List<(string TableNameCSharp, string FieldNameCSharp)> FieldNameCSharpList(params Assembly[] assemblyList)
        {
            var result = new List<(string TableNameCSharp, string FieldNameCSharp)>();
            foreach (var item in TableNameCSharpList(assemblyList))
            {
                Type typeRow = item.Key;
                string tableNameCSharp = item.Value;
                var fieldList = UtilDalType.TypeRowToFieldList(typeRow);
                foreach (var field in fieldList)
                {
                    result.Add((tableNameCSharp, field.FieldNameCSharp));
                }
            }
            return result;
        }
            
        /// <summary>
        /// Returns sql table name with schema name.
        /// </summary>
        internal static string TableNameWithSchemaSql(string schemaNameSql, string tableNameSql)
        {
            string result = string.Format("[{0}].[{1}]", schemaNameSql, tableNameSql);
            return result;
        }

        /// <summary>
        /// See also method TypeRowIsTableNameSql();
        /// </summary>
        internal static string TypeRowToTableNameWithSchemaSql(Type typeRow)
        {
            TypeRowToTableNameSql(typeRow, out string schemaNameSql, out string tableNameSql);
            string result = TableNameWithSchemaSql(schemaNameSql, tableNameSql);
            return result;
        }

        /// <summary>
        /// See also method TypeRowToFieldList();
        /// </summary>
        internal static PropertyInfo[] TypeRowToPropertyInfoList(Type typeRow)
        {
            if (typeRow == null)
            {
                return new PropertyInfo[] { };
            }
            else
            {
                return typeRow.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            }
        }

        internal class Field
        {
            public Field(PropertyInfo propertyInfo, int sort, string fieldNameSql, bool isPrimaryKey, FrameworkTypeEnum frameworkTypeEnum)
            {
                this.PropertyInfo = propertyInfo;
                this.Sort = sort;
                this.FieldNameSql = fieldNameSql;
                this.IsPrimaryKey = isPrimaryKey;
                this.FrameworkTypeEnum = frameworkTypeEnum;
            }

            public readonly PropertyInfo PropertyInfo;

            public string FieldNameCSharp
            {
                get
                {
                    return PropertyInfo.Name;
                }
            }

            /// <summary>
            /// Gets Sort. (FieldNameCSharpSort).
            /// </summary>
            public readonly int Sort;

            public readonly string FieldNameSql;

            public readonly bool IsPrimaryKey;

            public readonly FrameworkTypeEnum FrameworkTypeEnum;

            public FrameworkType FrameworkType()
            {
                if (FrameworkTypeEnum == FrameworkTypeEnum.None)
                {
                    return UtilDalType.FrameworkTypeFromValueType(PropertyInfo.PropertyType);
                }
                return UtilDalType.FrameworkTypeFromEnum(FrameworkTypeEnum);
            }
        }

        /// <summary>
        /// Returns CSharp fields. Sequence (FieldNameCSharpSort) is identical to CSharp code typeRow property declarations.
        /// </summary>
        internal static List<Field> TypeRowToFieldList(Type typeRow)
        {
            var result = new List<Field>();
            var propertyInfoList = TypeRowToPropertyInfoList(typeRow);
            int sort = 1;
            foreach (PropertyInfo propertyInfo in propertyInfoList)
            {
                SqlFieldAttribute fieldAttribute = (SqlFieldAttribute)propertyInfo.GetCustomAttribute(typeof(SqlFieldAttribute));
                string fieldNameSql = null;
                FrameworkTypeEnum frameworkTypeEnum = FrameworkTypeEnum.None;
                bool isPrimaryKey = false;
                if (fieldAttribute != null)
                {
                    fieldNameSql = fieldAttribute.FieldNameSql;
                    frameworkTypeEnum = fieldAttribute.FrameworkTypeEnum;
                    isPrimaryKey = fieldAttribute.IsPrimaryKey;
                }
                result.Add(new Field(propertyInfo, sort, fieldNameSql, isPrimaryKey, frameworkTypeEnum));
                sort += 1;
            }
            return result;
        }

        /// <summary>
        /// Returns CSharp field list as Dictionary.
        /// </summary>
        /// <returns>(FieldNameCSharp, Field)</returns>
        internal static Dictionary<string, Field> TypeRowToFieldListDictionary(Type typeRow)
        {
            var fieldList = TypeRowToFieldList(typeRow);

            var result = new Dictionary<string, Field>();
            foreach (Field field in fieldList)
            {
                result.Add(field.PropertyInfo.Name, field);
            }
            return result;
        }

        public static Type SqlTypeToType(int sqlType)
        {
            Type type = FrameworkTypeList().Where(item => item.Value.SqlType == sqlType).Single().Value.ValueType;
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

        public static FrameworkType FrameworkTypeFromEnum(FrameworkTypeEnum frameworkTypeEnum)
        {
            UtilFramework.Assert(frameworkTypeEnum != FrameworkTypeEnum.None, "FrameworkTypeEnum not defined!");
            return FrameworkTypeList().Where(item => item.Value.FrameworkTypeEnum == frameworkTypeEnum).Single().Value;
        }

        public static FrameworkType FrameworkTypeFromValueType(Type valueType)
        {
            valueType = UtilFramework.TypeUnderlying(valueType); // int? to int.
            if (valueType == typeof(int))
            {
                return FrameworkTypeFromEnum(FrameworkTypeEnum.Int);
            }
            if (valueType == typeof(Guid))
            {
                return FrameworkTypeFromEnum(FrameworkTypeEnum.Uniqueidentifier);
            }
            if (valueType == typeof(DateTime))
            {
                return FrameworkTypeFromEnum(FrameworkTypeEnum.Datetime);
            }
            if (valueType == typeof(char))
            {
                return FrameworkTypeFromEnum(FrameworkTypeEnum.Char);
            }
            if (valueType == typeof(string))
            {
                return FrameworkTypeFromEnum(FrameworkTypeEnum.Nvarcahr);
            }
            if (valueType == typeof(bool))
            {
                return FrameworkTypeFromEnum(FrameworkTypeEnum.Bit);
            }
            if (valueType == typeof(decimal))
            {
                return FrameworkTypeFromEnum(FrameworkTypeEnum.Decimal);
            }
            if (valueType == typeof(float))
            {
                return FrameworkTypeFromEnum(FrameworkTypeEnum.Float);
            }
            throw new Exception("Type not found!");
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
                frameworkType = new FrameworkTypeNChar(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
                frameworkType = new FrameworkTypeNvarcahr(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
                frameworkType = new FrameworkTypeVarchar(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
                frameworkType = new FrameworkTypeText(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
                frameworkType = new FrameworkTypeNtext(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
                frameworkType = new FrameworkTypeBit(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
                frameworkType = new FrameworkTypeMoney(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
                frameworkType = new FrameworkTypeSmallmoney(); result.Add(frameworkType.FrameworkTypeEnum, frameworkType);
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
    }

    internal class FrameworkType
    {
        public FrameworkType(FrameworkTypeEnum frameworkTypeEnum, string sqlTypeName, int sqlType, Type valueType, DbType dbType, bool isNumber)
        {
            this.FrameworkTypeEnum = frameworkTypeEnum;
            this.SqlTypeName = sqlTypeName;
            this.SqlType = sqlType;
            this.ValueType = valueType;
            this.DbType = dbType;
            this.IsNumber = isNumber;
        }

        public readonly FrameworkTypeEnum FrameworkTypeEnum;

        /// <summary>
        /// Gets or sets SqlTypeName. For example: "int", "datetime", "datetime".
        /// </summary>
        public readonly string SqlTypeName;

        public readonly int SqlType;

        public readonly Type ValueType;

        public readonly DbType DbType;

        public readonly bool IsNumber;

        /// <summary>
        /// Convert database value to front end cell text for bool.
        /// </summary>
        private void CellTextFromValueBool(object value, ref string result)
        {
            if (value is bool valueBool)
            {
                if (valueBool)
                {
                    result = "Yes";
                }
                else
                {
                    result = "No";
                }
            }
        }

        /// <summary>
        /// Convert database value to front end cell text.
        /// </summary>
        /// <param name="value">Value is never null when this method is called.</param>
        /// <returns>Returns text to display in cell.</returns>
        protected virtual internal string CellTextFromValue(object value)
        {
            string result = value.ToString();
            CellTextFromValueBool(value, ref result);
            return result;
        }

        /// <summary>
        /// Parse user entered text to database value for bool. Text is never null.
        /// </summary>
        private object CellTextParseBool(string text)
        {
            object result = null;
            string textUpper = text?.ToUpper();
            if (textUpper.StartsWith("Y") == true)
            {
                result = true;
            }
            if (textUpper.StartsWith("N") == true)
            {
                result = false;
            }
            if (result == null)
            {
                throw new Exception(string.Format("Text was not recognized as a valid yes, no value! ({0})", text));
            }
            return result;
        }

        /// <summary>
        /// Parse user entered text to database value. Text can be null.
        /// </summary>
        protected virtual internal object CellTextParse(string text)
        {
            object result = null;
            if (text != null)
            {
                Type type = UtilFramework.TypeUnderlying(ValueType);
                if (type == typeof(bool))
                {
                    result = CellTextParseBool(text);
                }
                else
                {
                    result = Convert.ChangeType(text, type, CultureInfo.InvariantCulture);
                }
            }
            return result;
        }

        protected virtual internal string ValueToSqlParameterDebug(object value)
        {
            string result = null;
            if (value == DBNull.Value)
            {
                value = null;
            }
            if (value != null)
            {
                result = value.ToString();
            }
            if (IsNumber == false)
            {
                result = "'" + result + "'";
            }
            if (value == null)
            {
                result = "NULL";
            }
            return result;
        }

        /// <summary>
        /// Convert value to CSharp code. Value can be null.
        /// </summary>
        protected virtual internal string ValueToCSharp(object value)
        {
            string result = null;
            if (value != null)
            {
                result = value.ToString();
            }
            if (IsNumber == false)
            {
                result = "\"" + result + "\"";
            }
            if (value == null)
            {
                result = "null";
            }
            return result;
        }
    }

    internal class FrameworkTypeInt : FrameworkType
    {
        public FrameworkTypeInt()
            : base(FrameworkTypeEnum.Int, "int", 56, typeof(Int32), DbType.Int32, true)
        {

        }
    }

    internal class FrameworkTypeSmallint : FrameworkType
    {
        public FrameworkTypeSmallint()
            : base(FrameworkTypeEnum.Smallint, "smallint", 52, typeof(Int16), DbType.Int16, true)
        {

        }
    }

    internal class FrameworkTypeTinyint : FrameworkType
    {
        public FrameworkTypeTinyint()
            : base(FrameworkTypeEnum.Tinyint, "tinyint", 48, typeof(byte), DbType.Byte, true)
        {

        }
    }

    internal class FrameworkTypeBigint : FrameworkType
    {
        public FrameworkTypeBigint()
            : base(FrameworkTypeEnum.Bigint, "bigint", 127, typeof(Int64), DbType.Int64, true)
        {

        }
    }

    internal class FrameworkTypeUniqueidentifier : FrameworkType
    {
        public FrameworkTypeUniqueidentifier()
            : base(FrameworkTypeEnum.Uniqueidentifier, "uniqueidentifier", 36, typeof(Guid), DbType.Guid, false)
        {

        }

        protected internal override string ValueToCSharp(object value)
        {
            string result = "null";
            if (value is Guid guid)
            {
                result = $"Guid.Parse(\"{guid.ToString(null, CultureInfo.InvariantCulture)}\")";
            }
            return result;
        }
    }

    internal class FrameworkTypeDatetime : FrameworkType
    {
        public FrameworkTypeDatetime()
            : base(FrameworkTypeEnum.Datetime, "datetime", 61, typeof(DateTime), DbType.DateTime, false)
        {

        }

        public static string CellTextFromValue(DateTime value, bool isTime = true)
        {
            string result = null;
            result = UtilFramework.DateTimeToText(value, isTime);
            return result;
        }

        public static DateTime? CellTextParse(string text, bool isTime = true)
        {
            return UtilFramework.DateTimeFromText(text, isTime);
        }

        public static string ValueToCSharpUtil(object value)
        {
            var result = "null";
            if (value != null)
            {
                DateTime dateTime = (DateTime)value;
                string dateTimeString = UtilFramework.DateTimeToText(dateTime);
                result = $"DateTime.Parse(\"{dateTimeString}\", CultureInfo.InvariantCulture)";
            }
            return result;
        }

        protected internal override string CellTextFromValue(object value)
        {
            return FrameworkTypeDatetime.CellTextFromValue((DateTime)value);
        }

        protected internal override object CellTextParse(string text)
        {
            return FrameworkTypeDatetime.CellTextParse(text);
        }

        protected internal override string ValueToCSharp(object value)
        {
            return FrameworkTypeDatetime.ValueToCSharpUtil(value);
        }
    }

    internal class FrameworkTypeDatetime2 : FrameworkType
    {
        public FrameworkTypeDatetime2()
            : base(FrameworkTypeEnum.Datetime2, "datetime2", 42, typeof(DateTime), DbType.DateTime2, false)
        {

        }

        protected internal override string CellTextFromValue(object value)
        {
            return FrameworkTypeDatetime.CellTextFromValue((DateTime)value);
        }

        protected internal override object CellTextParse(string text)
        {
            return FrameworkTypeDatetime.CellTextParse(text);
        }

        protected internal override string ValueToCSharp(object value)
        {
            return FrameworkTypeDatetime.ValueToCSharpUtil(value);
        }
    }

    internal class FrameworkTypeDate : FrameworkType
    {
        public FrameworkTypeDate()
            : base(FrameworkTypeEnum.Date, "date", 40, typeof(DateTime), DbType.Date, false)
        {

        }

        protected internal override string CellTextFromValue(object value)
        {
            return FrameworkTypeDatetime.CellTextFromValue((DateTime)value, isTime: false);
        }

        protected internal override object CellTextParse(string text)
        {
            return FrameworkTypeDatetime.CellTextParse(text, isTime: false);
        }

        protected internal override string ValueToCSharp(object value)
        {
            return FrameworkTypeDatetime.ValueToCSharpUtil(value);
        }
    }

    internal class FrameworkTypeChar : FrameworkType
    {
        public FrameworkTypeChar()
            : base(FrameworkTypeEnum.Char, "char", 175, typeof(string), DbType.String, false)
        {

        }
    }

    internal class FrameworkTypeNChar : FrameworkType
    {
        public FrameworkTypeNChar()
            : base(FrameworkTypeEnum.NChar, "nchar", 239, typeof(string), DbType.StringFixedLength, false)
        {

        }
    }

    internal class FrameworkTypeNvarcahr : FrameworkType
    {
        public FrameworkTypeNvarcahr()
            : base(FrameworkTypeEnum.Nvarcahr, "nvarcahr", 231, typeof(string), DbType.String, false)
        {

        }
    }

    internal class FrameworkTypeVarchar : FrameworkType
    {
        public FrameworkTypeVarchar()
            : base(FrameworkTypeEnum.Varchar, "varchar", 167, typeof(string), DbType.String, false)
        {

        }
    }

    internal class FrameworkTypeText : FrameworkType // See also: https://stackoverflow.com/questions/564755/sql-server-text-type-vs-varchar-data-type
    {
        public FrameworkTypeText()
            : base(FrameworkTypeEnum.Text, "text", 35, typeof(string), DbType.String, false)
        {

        }
    }

    internal class FrameworkTypeNtext : FrameworkType
    {
        public FrameworkTypeNtext()
            : base(FrameworkTypeEnum.Ntext, "ntext", 99, typeof(string), DbType.String, false)
        {

        }
    }

    internal class FrameworkTypeBit : FrameworkType
    {
        public FrameworkTypeBit()
            : base(FrameworkTypeEnum.Bit, "bit", 104, typeof(bool), DbType.Boolean, false)
        {

        }

        protected internal override string ValueToSqlParameterDebug(object value)
        {
            string result = null;
            if (value == DBNull.Value)
            {
                value = null;
            }
            if (value != null)
            {
                UtilFramework.Assert(value.GetType() == ValueType);
                if ((bool)value == false)
                {
                    result = "CAST(0 AS BIT)";
                }
                else
                {
                    result = "CAST(1 AS BIT)";
                }
            }
            if (value == null)
            {
                result = "NULL";
            }
            return result;
        }

        protected internal override object CellTextParse(string text)
        {
            if (text?.ToLower() == "false")
            {
                return false;
            }
            if (text?.ToLower() == "true")
            {
                return true;
            }
            return base.CellTextParse(text);
        }

        protected internal override string ValueToCSharp(object value)
        {
            string result = "null";
            if ((bool?)value == false)
            {
                result = "false";
            }
            if ((bool?)value == true)
            {
                result = "true";
            }
            return result;
        }
    }

    internal class FrameworkTypeMoney : FrameworkType
    {
        public FrameworkTypeMoney()
            : base(FrameworkTypeEnum.Money, "money", 60, typeof(decimal), DbType.Decimal, true)
        {

        }
    }

    internal class FrameworkTypeSmallmoney : FrameworkType
    {
        public FrameworkTypeSmallmoney()
            : base(FrameworkTypeEnum.Smallmoney, "smallmoney", 122, typeof(decimal), DbType.Decimal, true)
        {

        }
    }

    internal class FrameworkTypeDecimal : FrameworkType
    {
        public FrameworkTypeDecimal()
            : base(FrameworkTypeEnum.Decimal, "decimal", 106, typeof(decimal), DbType.Decimal, true)
        {

        }
    }

    internal class FrameworkTypeReal : FrameworkType
    {
        public FrameworkTypeReal()
            : base(FrameworkTypeEnum.Real, "real", 59, typeof(Single), DbType.Single, true)
        {

        }
    }

    internal class FrameworkTypeFloat : FrameworkType
    {
        public FrameworkTypeFloat()
            : base(FrameworkTypeEnum.Float, "float", 62, typeof(double), DbType.Double, true)
        {

        }

        protected internal override string ValueToCSharp(object value)
        {
            string result = null;
            if (value != null)
            {
                result = ((Double)value).ToString(System.Globalization.CultureInfo.InvariantCulture); // value.ToString(); returns for example 9,5 instead of 9.5
            }
            if (value == null)
            {
                result = "null";
            }
            return result;
        }
    }

    internal class FrameworkTypeVarbinary : FrameworkType
    {
        public FrameworkTypeVarbinary()
            : base(FrameworkTypeEnum.Varbinary, "varbinary", 165, typeof(byte[]), DbType.Binary, false) // DbType.Binary?
        {

        }

        protected internal override string CellTextFromValue(object value)
        {
            return null;

            // return UtilFramework.IntToText(((byte[])value).Length) + " bytes"; // When user changes this text it gets saved to db.

            // return Convert.ToBase64String((byte[])value);
        }

        protected internal override object CellTextParse(string text)
        {
            throw new Exception("Can not parse binary!");

            // object result = null;
            // if (text != null)
            // {
            //     return Encoding.Unicode.GetBytes(text);
            // }
            // return result;
        }

        protected internal override string ValueToCSharp(object value)
        {
            string result = "null";
            if (value != null)
            {
                string text = Convert.ToBase64String(((byte[])value));
                StringBuilder stringBuilder = new StringBuilder();
                String.Join(null, "", "");
                stringBuilder.Append("Convert.FromBase64String(String.Join(null, \"\"");
                var textList = UtilFramework.SplitChunk(text, 320);
                foreach (var item in textList)
                {
                    stringBuilder.AppendLine(",");
                    stringBuilder.Append("                    ");
                    stringBuilder.Append($"\"{item}\"");
                }
                stringBuilder.Append("))");
                result = stringBuilder.ToString();
            }
            return result;
        }
    }

    internal class SqlTypeSqlvariant : FrameworkType
    {
        public SqlTypeSqlvariant()
            : base(FrameworkTypeEnum.Sqlvariant, "sql_variant", 98, typeof(object), DbType.Object, false)
        {

        }
    }

    internal class FrameworkTypeImage : FrameworkType
    {
        public FrameworkTypeImage()
            : base(FrameworkTypeEnum.Image, "image", 34, typeof(byte[]), DbType.Binary, false) // DbType.Binary?
        {

        }
    }

    internal class FrameworkTypeNumeric : FrameworkType
    {
        public FrameworkTypeNumeric()
            : base(FrameworkTypeEnum.Numeric, "numeric", 108, typeof(decimal), DbType.Decimal, true)
        {

        }
    }
}