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
        /// Add parameter to sql.
        /// </summary>
        /// <param name="isUseParameter">If true, parameter is used, otherwise parameter is passed as literal.</param>
        internal static string Parameter(object value, SqlDbType dbType, List<SqlParameter> parameterList, bool isUseParameter = true)
        {
            if (isUseParameter == true)
            {
                if (value == null)
                {
                    return "NULL";
                }
                if (value.GetType() == typeof(string))
                {
                    string valueString = (string)value;
                    valueString = valueString.Replace("'", "''"); // Escape single quote.
                    return string.Format("'{0}'", value);
                }
                if (value.GetType() == typeof(bool))
                {
                    bool valueBool = (bool)value;
                    if (valueBool == false)
                    {
                        return "0";
                    }
                    else
                    {
                        return "1";
                    }
                }
                return value.ToString();
            }
            else
            {
                string result = $"@P{ parameterList.Count }";
                if (value == null)
                {
                    value = DBNull.Value;
                }
                parameterList.Add(new SqlParameter($"P{ parameterList.Count }", dbType) { Value = value });
                return result;
            }
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
                        entity.Ignore(propertyInfo.Name);
                    }
                    else
                    {
                        // Primary key
                        if (columnAttribute == null || columnAttribute.IsPrimaryKey)
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
                    // set artificial "Primary Key" on first column. See also method UtilDataAccessLayer.Insert();
                    PropertyInfo propertyInfo = propertyInfoList.First();
                    entity.HasKey(propertyInfo.Name);
                    entity.Property(propertyInfo.Name).ValueGeneratedOnAdd(); // Read back auto increment key value. // Applies also to InMemory Rows.
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
        /// Insert data record. Primary key needs to be 0!
        /// </summary>
        public static void Insert(Row row)
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
        }

        /// <summary>
        /// Wrap SqlCommand into SqlConnection.
        /// </summary>
        private static void SqlCommand(List<string> sqlList, Action<SqlCommand> execute, bool isFrameworkDb, params SqlParameter[] paramList)
        {
            string connectionString = ConnectionManagerServer.ConnectionString(isFrameworkDb);
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                foreach (string sql in sqlList)
                {
                    using (SqlCommand sqlCommand = new SqlCommand(sql, sqlConnection))
                    {
                        sqlCommand.Parameters.AddRange(paramList);
                        execute(sqlCommand); // Call back
                    }
                }
            }
        }

        /// <summary>
        /// Read data from stored procedure or database table. Returns multiple result sets.
        /// (ResultSet, Row, ColumnName, Value).
        /// </summary>
        public static List<List<Dictionary<string, object>>> Execute(string sql, params SqlParameter[] paramList)
        {
            List<List<Dictionary<string, object>>> result = new List<List<Dictionary<string, object>>>();
            List<string> sqlList = new List<string>();
            sqlList.Add(sql);
            SqlCommand(sqlList, (sqlCommand) =>
            {
                sqlCommand.Parameters.AddRange(paramList);
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
                                row.Add(columnName, value);
                            }
                        }
                        reader.NextResult();
                    }
                }
            }, false);
            return result;
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
        /// Clone data row.
        /// </summary>
        public static Row RowClone(Row row)
        {
            Row result = (Row)UtilFramework.TypeToObject(row.GetType());
            RowCopy(row, result);
            return result;
        }

        /// <summary>
        /// Clone data row.
        /// </summary>
        public static TRow RowClone<TRow>(TRow row) where TRow : Row
        {
            return (TRow)RowClone((Row)row);
        }

        /// <summary>
        /// Copy data row. Source and dest need not to be of same type. Only cells available on
        /// both records are copied. See also RowClone();
        /// </summary>
        internal static void RowCopy(Row rowSource, Row rowDest)
        {
            var propertyInfoDestList = UtilDataAccessLayer.TypeRowToPropertyList(rowDest.GetType());
            foreach (PropertyInfo propertyInfoDest in propertyInfoDestList)
            {
                string columnName = propertyInfoDest.Name;
                PropertyInfo propertyInfoSource = rowSource.GetType().GetTypeInfo().GetProperty(columnName);
                if (propertyInfoSource != null)
                {
                    object value = propertyInfoSource.GetValue(rowSource);
                    propertyInfoDest.SetValue(rowDest, value);
                }
            }
        }

        /// <summary>
        /// Returns new data row.
        /// </summary>
        internal static Row RowCreate(Type typeRow)
        {
            return (Row)UtilFramework.TypeToObject(typeRow);
        }
    }
}
