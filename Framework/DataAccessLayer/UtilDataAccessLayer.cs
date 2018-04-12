namespace Framework.DataAccessLayer
{
    using Framework.Server;
    using global::Server;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata;
    using Microsoft.EntityFrameworkCore.Metadata.Conventions;
    using Microsoft.EntityFrameworkCore.Query.Internal;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Reflection;
    using System.Text;

    public enum FilterOperator
    {
        None = 0,
        Equal = 1,
        Smaller = 2,
        Greater = 3,
        Like = 4
    }

    public class Filter
    {
        public string ColumnNameCSharp;

        public FilterOperator FilterOperator;

        public object Value;
    }

    public static class UtilDataAccessLayer
    {
        internal static PropertyInfo[] TypeRowToPropertyList(Type typeRow)
        {
            return typeRow.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance); // Exclude static GridName declarations.
        }

        /// <summary>
        /// Returns row type as string. For example: "dbo.User". Omits "Database" namespace. This is for example identical to FrameworkConfigGridView.TableNameCSharp.
        /// </summary>
        internal static string TypeRowToTableNameCSharp(Type typeRow)
        {
            string result = null;
            if (typeRow != null)
            {
                UtilFramework.Assert(UtilFramework.IsSubclassOf(typeRow, typeof(Row)), "Wrong type!");
                result = UtilFramework.TypeToName(typeRow);
                UtilFramework.Assert(result.StartsWith("Database.")); // If it is a calculated row which does not exist on database move it for example to namespace "Database.Calculated".
                result = result.Substring("Database.".Length); // Remove "Database" namespace.
            }
            return result;
        }

        /// <summary>
        /// Returns TypeRowList of all in code defined Row classes. Returns also framework Row classes.
        /// </summary>
        internal static Type[] TypeRowList(Type typeRowInAssembly)
        {
            List<Type> result = new List<Type>();
            Type[] typeInAssemblyList = UtilFramework.TypeInAssemblyList(typeRowInAssembly);
            foreach (Type itemTypeInAssembly in typeInAssemblyList)
            {
                foreach (Type type in itemTypeInAssembly.GetTypeInfo().Assembly.GetTypes())
                {
                    if (type.GetTypeInfo().IsSubclassOf(typeof(Row)))
                    {
                        result.Add(type);
                    }
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// Returns row type. Searches also for Framework tables.
        /// </summary>
        /// <param name="tableNameCSharp">For example: "Database.dbo.User".</param>
        internal static Type TypeRowFromTableNameCSharp(string tableNameCSharp, Type typeRowInAssembly)
        {
            tableNameCSharp = "Database." + tableNameCSharp;
            Type[] typeInAssemblyList = UtilFramework.TypeInAssemblyList(typeRowInAssembly);
            Type result = UtilFramework.TypeFromName(tableNameCSharp, typeInAssemblyList);
            UtilFramework.Assert(UtilFramework.IsSubclassOf(result, typeof(Row)), "Wrong type!");
            return result;
        }

        [ThreadStatic]
        private static Dictionary<Type, List<Cell>> cacheColumnList;

        internal static List<Cell> ColumnList(Type typeRow)
        {
            if (cacheColumnList == null)
            {
                cacheColumnList = new Dictionary<Type, List<Cell>>();
            }
            if (cacheColumnList.ContainsKey(typeRow))
            {
                foreach (Cell column in cacheColumnList[typeRow])
                {
                    column.ConstructorColumn(); // Column mode.
                }
                return cacheColumnList[typeRow];
            }
            //
            List<Cell> result = new List<Cell>();
            if (typeRow != null)
            {
                string tableNameCSharp = UtilDataAccessLayer.TypeRowToTableNameCSharp(typeRow);
                foreach (PropertyInfo propertyInfo in UtilDataAccessLayer.TypeRowToPropertyList(typeRow))
                {
                    SqlColumnAttribute columnAttribute = (SqlColumnAttribute)propertyInfo.GetCustomAttribute(typeof(SqlColumnAttribute));
                    string sqlColumnName = null;
                    if (columnAttribute != null)
                    {
                        sqlColumnName = columnAttribute.SqlColumnName;
                    }
                    Type typeCell = typeof(Cell); // Default cell api.
                    if (columnAttribute != null) // Reference from entity property to cell. If no cell api is defined, stick, with default cell api.
                    {
                        if (columnAttribute.TypeCell != null)
                        {
                            typeCell = columnAttribute.TypeCell; // Override default value.
                        }
                    }
                    Cell cell = (Cell)UtilFramework.TypeToObject(typeCell);
                    cell.Constructor(tableNameCSharp, sqlColumnName, propertyInfo.Name, typeRow, propertyInfo.PropertyType, propertyInfo);
                    result.Add(cell);
                }
            }
            return result;
        }

        /// <summary>
        /// Returns cell list. Or column list, if row is null.
        /// </summary>
        internal static List<Cell> CellList(Type typeRow, object row)
        {
            if (row != null)
            {
                UtilFramework.Assert(row.GetType() == typeRow);
            }
            List<Cell> result = new List<Cell>();
            result = ColumnList(typeRow); // For column row is null and row.GetType() is not possible.
            foreach (Cell cell in result)
            {
                cell.Constructor(row);
            }
            return result;
        }

        /// <summary>
        /// Build model for one typeRow or all Row classes in case of InMemory unit test mode.
        /// </summary>
        private static IMutableModel DbContextModel(Type typeRow)
        {
            List<Type> typeRowList = new List<Type>();
            UnitTestService unitTestService = UnitTestService.Instance;
            if (unitTestService.IsUnitTest)
            {
                if (unitTestService.Model != null)
                {
                    return unitTestService.Model;
                }
                typeRowList.AddRange(UtilFramework.TypeList(unitTestService.TypeInAssembly, typeof(Row))); // Add all Row classes for InMemory unit test mode.
                typeRowList.Remove(typeof(Row));
            }
            else
            {
                typeRowList.Add(typeRow);
            }
            var conventionSet = new ConventionSet();
            var builder = new ModelBuilder(conventionSet);
            // Build model
            foreach (Type itemTypeRow in typeRowList)
            {
                var entity = builder.Entity(itemTypeRow);
                SqlTableAttribute tableAttribute = (SqlTableAttribute)itemTypeRow.GetTypeInfo().GetCustomAttribute(typeof(SqlTableAttribute));
                if (tableAttribute != null) // InMemory Row for UnitTest might not have an sql table name defined.
                {
                    entity.ToTable(tableAttribute.SqlTableName, tableAttribute.SqlSchemaName); // By default EF maps sql table name to class name.
                }
                bool isPrimaryKey = false;
                PropertyInfo[] propertyInfoList = UtilDataAccessLayer.TypeRowToPropertyList(itemTypeRow);
                foreach (PropertyInfo propertyInfo in propertyInfoList)
                {
                    SqlColumnAttribute columnAttribute = (SqlColumnAttribute)propertyInfo.GetCustomAttribute(typeof(SqlColumnAttribute));
                    if (columnAttribute == null || columnAttribute.SqlColumnName == null) // Calculated column. Do not include it in sql select. For example button added to row.
                    {
                        if (columnAttribute != null && columnAttribute.IsPrimaryKey)
                        {
                            throw new Exception("Primary key can not be calculated!");
                        }
                        entity.Ignore(propertyInfo.Name);
                    }
                    else
                    {
                        // Primary key
                        if (columnAttribute != null && columnAttribute.IsPrimaryKey)
                        {
                            isPrimaryKey = true;
                            entity.HasKey(propertyInfo.Name);
                            entity.Property(propertyInfo.Name).ValueGeneratedOnAdd(); // Read back auto increment key value.
                        }
                        entity.Property(propertyInfo.PropertyType, propertyInfo.Name).HasColumnName(columnAttribute.SqlColumnName);
                    }
                }
                if (isPrimaryKey == false)
                {
                    // No primary key defined. For example View. In order to prevent NullException when inserting row,
                    // set artificial "Primary Key" on first column (not calculated column). See also method UtilDataAccessLayer.Insert();
                    PropertyInfo propertyInfoFirst = null;
                    foreach (PropertyInfo propertyInfo in propertyInfoList)
                    {
                        SqlColumnAttribute columnAttribute = (SqlColumnAttribute)propertyInfo.GetCustomAttribute(typeof(SqlColumnAttribute));
                        if (columnAttribute != null && columnAttribute.SqlColumnName != null)
                        {
                            propertyInfoFirst = propertyInfo;
                            break;
                        }
                    }
                    if (propertyInfoFirst == null)
                    {
                        propertyInfoFirst = propertyInfoList.First(); // No artificial primary key found! For example calculated rows (views) which read only and do not write back.
                    }
                    entity.HasKey(propertyInfoFirst.Name);
                    entity.Property(propertyInfoFirst.Name).ValueGeneratedOnAdd(); // Read back auto increment key value. // Applies also to InMemory Rows.
                }
            }
            var model = builder.Model;
            if (unitTestService.IsUnitTest)
            {
                unitTestService.Model = model;
            }
            return model;
        }

        /// <summary>
        /// Returns DbContext with ConnectionString and model for one row, defined in typeRow.
        /// </summary>
        private static DbContext DbContext(Type typeRow)
        {
            var options = new DbContextOptionsBuilder<DbContext>();
            if (UnitTestService.Instance.IsUnitTest == false)
            {
                string connectionString = ConnectionManagerServer.ConnectionString(typeRow);
                if (connectionString == null)
                {
                    throw new Exception("ConnectionString is null! (See also file: ConnectionManagerServer.json)");
                }
                options.UseSqlServer(connectionString); // See also: ConnectionManagerServer.json (Data Source=localhost; Initial Catalog=Database; Integrated Security=True;)
            }
            else
            {
                options.UseInMemoryDatabase(databaseName: "Memory");
            }
            options.UseModel(DbContextModel(typeRow));
            DbContext result = new DbContext(options.Options);
            //
            return result;
        }

        public static IQueryable Query(Type typeRow)
        {
            UtilFramework.LogDebug(string.Format("QUERY ({0})", UtilDataAccessLayer.TypeRowToTableNameCSharp(typeRow)));
            //
            DbContext dbContext = DbContext(typeRow);
            dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking; // For SQL views. No primary key.
            IQueryable query = (IQueryable)(dbContext.GetType().GetTypeInfo().GetMethod("Set").MakeGenericMethod(typeRow).Invoke(dbContext, null));
            return query;
        }

        public static IQueryable<TRow> Query<TRow>() where TRow : Row
        {
            return (IQueryable<TRow>)Query(typeof(TRow));
        }

        /// <summary>
        /// Returns true, if query is a database query. Otherwise it is a linq to memory query.
        /// </summary>
        internal static bool QueryProviderIsDatabase(IQueryable query)
        {
            return query.Provider.GetType() == typeof(EntityQueryProvider);
        }

        /// <summary>
        /// Add filter to query.
        /// </summary>
        private static void Select(List<Filter> filterList, ref IQueryable query)
        {
            if (filterList != null && filterList.Count > 0)
            {
                string filterSql = null;
                List<object> parameterList = new List<object>();
                int i = 0;
                foreach (Filter filter in filterList)
                {
                    if (filterSql != null)
                    {
                        filterSql += " AND ";
                    }
                    filterSql += filter.ColumnNameCSharp;
                    switch (filter.FilterOperator)
                    {
                        case FilterOperator.Equal:
                            filterSql += " = @" + i.ToString();
                            break;
                        case FilterOperator.Smaller:
                            filterSql += " <= @" + i.ToString();
                            break;
                        case FilterOperator.Greater:
                            filterSql += " >= @" + i.ToString();
                            break;
                        case FilterOperator.Like:
                            filterSql += ".Contains(@" + i.ToString() + ")";
                            break;
                        default:
                            throw new Exception("Enum unknowen!");
                    }
                    parameterList.Add(filter.Value);
                    i += 1;
                }
                query = query.Where(filterSql, parameterList.ToArray());
            }
        }

        internal static List<Row> Select(Type typeRow, List<Filter> filterList, string columnNameOrderBy, bool isOrderByDesc, int pageIndex, int pageRowCount, IQueryable query = null)
        {
            UtilFramework.LogDebug(string.Format("SELECT ({0})",  UtilDataAccessLayer.TypeRowToTableNameCSharp(typeRow)));
            //
            UtilFramework.Assert(query.ElementType == typeRow);
            if (query == null)
            {
                query = Query(typeRow);
            }
            if (columnNameOrderBy != null)
            {
                string ordering = columnNameOrderBy;
                if (isOrderByDesc)
                {
                    ordering = ordering + " DESC";
                }
                query = query.OrderBy(ordering);
            }
            Select(filterList, ref query);
            query = query.Skip(pageIndex * pageRowCount).Take(pageRowCount);
            object[] resultArray = query.ToDynamicArray().ToArray();
            List<Row> result = new List<Row>();
            foreach (var row in resultArray)
            {
                result.Add((Row)row);
            }
            return result;
        }

        /// <summary>
        /// Compares values of two rows.
        /// </summary>
        /// <returns>Returns true, if rows are equal.</returns>
        private static bool IsRowEqual(Row rowA, Row rowB)
        {
            bool result = true;
            var columnList = UtilDataAccessLayer.ColumnList(rowA.GetType());
            foreach (var column in columnList)
            {
                object valueA = column.PropertyInfo.GetValue(rowA);
                object valueB = column.PropertyInfo.GetValue(rowB);
                if (!object.Equals(valueA, valueB))
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Update data record on database.
        /// </summary>
        public static void Update(Row row, Row rowNew)
        {
            UtilFramework.LogDebug(string.Format("UPDATE ({0})", UtilDataAccessLayer.TypeRowToTableNameCSharp(row.GetType())));
            //
            UtilFramework.Assert(row.GetType() == rowNew.GetType());
            if (!IsRowEqual(row, rowNew)) // Rows are equal for example after user reverted input after error.
            {
                row = UtilDataAccessLayer.RowClone(row); // Prevent modifications on SetValues(rowNew);
                DbContext dbContext = DbContext(row.GetType());
                var tracking = dbContext.Attach(row);
                tracking.CurrentValues.SetValues(rowNew);
                UtilFramework.Assert(dbContext.SaveChanges() == 1, "Update failed!");
            }
        }

        /// <summary>
        /// Insert data record. Primary key needs to be 0! Returned new row contains new primary key.
        /// </summary>
        public static TRow Insert<TRow>(TRow row) where TRow : Row
        {
            UtilFramework.LogDebug(string.Format("INSERT ({0})", UtilDataAccessLayer.TypeRowToTableNameCSharp(row.GetType())));
            //
            Row rowClone = UtilDataAccessLayer.RowClone(row);
            DbContext dbContext = DbContext(row.GetType());
            dbContext.Add(row); // Throws NullReferenceException if no primary key is defined.
            try
            {
                dbContext.SaveChanges();
                //
                // Exception: Database operation expected to affect 1 row(s) but actually affected 0 row(s). 
                // Cause: No autoincrement on Id column or no Id set by application
                //
                // Exception: The conversion of a datetime2 data type to a datetime data type resulted in an out-of-range value.
                // Cause: CSharp not nullable DateTime default value is "{1/1/0001 12:00:00 AM}" change it to nullable or set value for example to DateTime.Now
            }
            catch (Exception exception)
            {
                UtilDataAccessLayer.RowCopy(rowClone, row); // In case of exception, EF might change for example auto incremental id to -2147482647. Reverse it back.
                throw exception;
            }
            return row; // Return Row with new primary key.
        }

        /// <summary>
        /// Wrap SqlCommand into SqlConnection.
        /// </summary>
        /// <param name="sqlList">List of sql statements.</param>
        /// <param name="isFrameworkDb">Execute sql on Appliation or Framework database.</param>
        /// <param name="paramList">Do not use if multiple sqlList.</param>
        /// <param name="isUseParam">If false, parameters are replaced with sql text.</param>
        /// <param name="execute">Callback method.</param>
        internal static void Execute(List<string> sqlList, bool isFrameworkDb, List<SqlParameter> paramList, bool isUseParam, Action<SqlCommand> execute)
        {
            if (sqlList.Count > 1)
            {
                UtilFramework.Assert(paramList.Count == 0, "No paramList if multiple sql statement!");
            }
            string connectionString = ConnectionManagerServer.ConnectionString(isFrameworkDb);
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("ConnectionString missing!");
            }
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                foreach (string sql in sqlList)
                {
                    string sqlForeach = sql;
                    ExecuteParameterReplace(ref sqlForeach, paramList, isUseParam);
                    using (SqlCommand sqlCommand = new SqlCommand(sqlForeach, sqlConnection))
                    {
                        sqlCommand.Parameters.AddRange(paramList.ToArray());
                        execute(sqlCommand); // Call back
                    }
                }
            }
        }

        /// <summary>
        /// Returns parameter value as sql text.
        /// </summary>
        private static string ExecuteParameterToSqlText(SqlParameter parameter)
        {
            if (parameter.Value == DBNull.Value)
            {
                return "NULL";
            }
            if (parameter.Value is int)
            {
                return parameter.Value.ToString();
            }
            if (parameter.Value is bool)
            {
                if ((bool)parameter.Value == true)
                {
                    return "1";
                }
                else
                {
                    return "0";
                }
            }
            if (parameter.Value is string)
            {
                string valueString = parameter.Value.ToString().Replace("'", "''"); // Escape single quote.
                return string.Format("'{0}'", valueString);
            }
            throw new Exception("Type unknown!");
        }

        /// <summary>
        /// Replace parameter name with actual parameter text value. Be aware of sql injection when using with sql parameter type of text.
        /// </summary>
        private static void ExecuteParameterReplace(ref string sql, List<SqlParameter> paramList)
        {
            paramList = paramList.OrderByDescending(item => item.ParameterName).ToList(); // Replace first long names.
            for (int i = 0; i < paramList.Count; i++)
            {
                SqlParameter parameter = paramList[i];
                string parameterText = ExecuteParameterToSqlText(parameter);
                UtilFramework.Assert(parameterText.Contains("@P") == false); // Parameter value does not contain parameter name.
                string parameterName = parameter.ParameterName;
                UtilFramework.Assert(sql.Contains(parameterName), "Parameter not found in sql statement!");
                sql = sql.Replace(parameterName, parameterText);
            }
        }

        /// <summary>
        /// Add sql parameter to list.
        /// </summary>
        /// <param name="name">Parameter name. For example: @Text</param>
        /// <param name="value">Parameter value.</param>
        internal static void ExecuteParameterAdd(string name, object value, SqlDbType dbType, List<SqlParameter> paramList)
        {
            UtilFramework.Assert(name.StartsWith("@"), "Parameter does not start with @!");
            UtilFramework.Assert(paramList.Where(item => item.ParameterName == name).Count() == 0, string.Format("ParameterName already exists! ({0})", name));
            if (value == null)
            {
                value = DBNull.Value;
            }
            SqlParameter parameter = new SqlParameter(name, dbType) { Value = value };
            paramList.Add(parameter);
        }

        /// <summary>
        /// Returns parameter name or value as sql text.
        /// </summary>
        /// <param name="isUseParam">If true, method returns parameter name. For example @P0. If false, method returns value as sql text. For example: 'Name'</param>
        internal static string ExecuteParameterAdd(object value, SqlDbType dbType, List<SqlParameter> paramList, bool isUseParam = true)
        {
            string result;
            string name = $"@P{ paramList.Count }";
            ExecuteParameterAdd(name, value, dbType, paramList);
            if (isUseParam)
            {
                result = name;
            }
            else
            {
                result = ExecuteParameterToSqlText(paramList[paramList.Count - 1]);
                paramList.RemoveAt(paramList.Count - 1);
            }
            return result;
        }

        private static void ExecuteParameterReplace(ref string sql, List<SqlParameter> paramList, bool isUseParam)
        {
            if (isUseParam == false)
            {
                ExecuteParameterReplace(ref sql, paramList);
                paramList.Clear();
            }
        }
        /// <summary>
        /// Read data from stored procedure or database table. Returns multiple result sets.
        /// </summary>
        /// <param name="sql">For example: ExecuteReader("SELECT 1 AS A SELECT 2 AS B"); or with parameter: ExecuteReader("SELECT @P0 AS A");</param>
        /// <param name="paramList">See also method ExecuteParameterAdd();</param>
        /// <param name="isUseParam">If false, sql parameters are replaced with sql text. Use for example to debug. Be aware of SQL injection!</param>
        /// <returns>Returns (ResultSet, Row, ColumnName, Value).</returns>
        public static List<List<Dictionary<string, object>>> ExecuteReader(string sql, List<SqlParameter> paramList, bool isUseParam = true)
        {
            List<List<Dictionary<string, object>>> result = new List<List<Dictionary<string, object>>>();
            List<string> sqlList = new List<string>();
            sqlList.Add(sql);
            Execute(sqlList, false, paramList, isUseParam, (sqlCommand) =>
            {
                using (SqlDataReader reader = sqlCommand.ExecuteReader())
                {
                    while (reader.HasRows)
                    {
                        var rowList = new List<Dictionary<string, object>>();
                        result.Add(rowList);
                        while (reader.Read())
                        {
                            var row = new Dictionary<string, object>();
                            rowList.Add(row);
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string columnName = reader.GetName(i);
                                object value = reader.GetValue(i);
                                if (value == DBNull.Value)
                                {
                                    value = null;
                                }
                                row.Add(columnName, value);
                            }
                        }
                        reader.NextResult();
                    }
                }
            });
            return result;
        }

        public static List<List<Dictionary<string, object>>> ExecuteReader(string sql)
        {
            return ExecuteReader(sql, new List<SqlParameter>());
        }

        /// <summary>
        /// Execute non query sql statement. For example: INSERT INTO MyTable (Name) SELECT @P0 or EXEC MyStoredProc
        /// </summary>
        /// <returns>Returns rows affected.</returns>
        public static int ExecuteNonQuery(string sql, List<SqlParameter> paramList, bool isUseParam = true)
        {
            List<string> sqlList = new List<string>(new string[] { sql });
            int result = 0;
            Execute(sqlList, false, paramList, isUseParam, (sqlCommand) =>
            {
                result = sqlCommand.ExecuteNonQuery();
            });
            return result;
        }

        public static int ExecuteNonQuery(string sql)
        {
            return ExecuteNonQuery(sql, new List<SqlParameter>());
        }

        /// <summary>
        /// Convert one sql multiple result set to typed row list.
        /// </summary>
        /// <param name="valueList">Result of method Execute();</param>
        /// <param name="resultSetIndex">Sql multiple result set index.</param>
        /// <param name="typeRow">Type of row to copy to. Copy one multiple result set.</param>
        /// <returns>List of typed rows.</returns>
        public static List<object> ExecuteResultCopy(List<List<Dictionary<string, object>>> valueList, int resultSetIndex, Type typeRow)
        {
            List<object> result = new List<object>();
            PropertyInfo[] propertyInfoList = UtilDataAccessLayer.TypeRowToPropertyList(typeRow);
            foreach (Dictionary<string, object> row in valueList[resultSetIndex])
            {
                Row rowResult = UtilDataAccessLayer.RowCreate(typeRow);
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
        public static List<T> ExecuteResultCopy<T>(List<List<Dictionary<string, object>>> valueList, int resultSetIndex) where T : Row
        {
            List<object> result = ExecuteResultCopy(valueList, resultSetIndex, typeof(T));
            return result.Cast<T>().ToList();
        }

        /// <summary>
        /// Delete data record from database.
        /// </summary>
        public static void Delete(Row row)
        {
            DbContext dbContext = DbContext(row.GetType());
            dbContext.Remove(row);
            dbContext.SaveChanges();
        }

        internal static object ValueToJson(object value)
        {
            object result = value;
            if (value != null)
            {
                if (value.GetType() == typeof(int))
                {
                    result = Convert.ChangeType(value, typeof(double));
                }
            }
            return result;
        }

        internal static string RowValueToText(object value, Type type)
        {
            type = UtilFramework.TypeUnderlying(type);
            //
            if (type == typeof(byte[]) && value != null)
            {
                return Encoding.Unicode.GetString((byte[])value);
            }
            if (type == typeof(DateTime) && value != null)
            {
                return string.Format("{0:yyyy-MM-dd}", value);
            }
            if (value != null)
            {
                return value.ToString();
            }
            return null;
        }

        /// <summary>
        /// Parse user entered text.
        /// </summary>
        internal static object RowValueFromText(string text, Type type)
        {
            Type typeUnderlying = Nullable.GetUnderlyingType(type);
            if (text == null && typeUnderlying != null) // Type is nullable
            {
                return null;
            }
            if (typeUnderlying != null)
            {
                type = typeUnderlying;
            }
            //
            if (type == typeof(byte[]) && text != null)
            {
                string base64 = "base64,";
                if (text.StartsWith("data:") && text.Contains(base64))
                {
                    text = text.Substring(text.IndexOf(base64) + base64.Length);
                    return Convert.FromBase64String(text);
                }
                return Encoding.Unicode.GetBytes(text);
            }
            //
            if (text == null && type.GetTypeInfo().IsValueType)
            {
                return UtilFramework.TypeToObject(type); // For example Int32
            }
            //
            if (type == typeof(Guid) || typeUnderlying == typeof(Guid))
            {
                return Guid.Parse(text);
            }
            if (type == typeof(string) && text == "")
            {
                text = null;
            }
            if (type.IsValueType && text == "")
            {
                return null;
            }
            return Convert.ChangeType(text, type);
        }

        /// <summary>
        /// Clone data row. Like method RowCopy(); but source and dest are of same type.
        /// </summary>
        public static Row RowClone(Row row)
        {
            Row result = (Row)UtilFramework.TypeToObject(row.GetType());
            RowCopy(row, result);
            return result;
        }

        /// <summary>
        /// Clone data row. Like method RowCopy(); but source and dest are of same type.
        /// </summary>
        public static TRow RowClone<TRow>(TRow row) where TRow : Row
        {
            return (TRow)RowClone((Row)row);
        }

        /// <summary>
        /// Copy data row. Source and dest need not to be of same type. Only cells available on
        /// both records are copied. See also RowClone();
        /// </summary>
        public static void RowCopy(Row rowSource, Row rowDest, string columnNamePrefix = null)
        {
            var propertyInfoDestList = UtilDataAccessLayer.TypeRowToPropertyList(rowDest.GetType());
            foreach (PropertyInfo propertyInfoDest in propertyInfoDestList)
            {
                string columnName = columnNamePrefix + propertyInfoDest.Name;
                PropertyInfo propertyInfoSource = rowSource.GetType().GetTypeInfo().GetProperty(columnName);
                if (propertyInfoSource != null)
                {
                    object value = propertyInfoSource.GetValue(rowSource);
                    propertyInfoDest.SetValue(rowDest, value);
                }
            }
        }

        /// <summary>
        /// Returns true, if row and rowNew have different values.
        /// </summary>
        internal static bool RowIsModify(Row row, Row rowNew)
        {
            UtilFramework.Assert(row.GetType() == rowNew.GetType());
            //
            bool result = false;
            var propertyInfoDestList = UtilDataAccessLayer.TypeRowToPropertyList(row.GetType());
            foreach (PropertyInfo propertyInfoDest in propertyInfoDestList)
            {
                string columnName = propertyInfoDest.Name;
                PropertyInfo propertyInfoSource = row.GetType().GetTypeInfo().GetProperty(columnName);
                object value = propertyInfoSource.GetValue(row);
                object valueNew = propertyInfoSource.GetValue(rowNew);
                if (!object.Equals(value, valueNew))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Copy data row. Source and dest need not to be of same type. Only cells available on
        /// both records are copied. See also RowClone();
        /// </summary>
        /// <param name="columnNamePrefix">Used for example if sql view prefixes underling tables uniquely.</param>
        public static TRowDest RowCopy<TRowDest>(Row rowSource, string columnNamePrefix = null) where TRowDest : Row
        {
            TRowDest rowDest = UtilDataAccessLayer.RowCreate<TRowDest>();
            RowCopy(rowSource, rowDest, columnNamePrefix);
            return rowDest;
        }

        /// <summary>
        /// Returns new data row.
        /// </summary>
        public static Row RowCreate(Type typeRow)
        {
            return (Row)UtilFramework.TypeToObject(typeRow);
        }

        /// <summary>
        /// Returns new data row.
        /// </summary>
        public static TRowType RowCreate<TRowType>() where TRowType : Row
        {
            return (TRowType)RowCreate(typeof(TRowType));
        }
    }
}
