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
    using System.Data.SqlClient;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Reflection;
    using System.Threading.Tasks;

    public static class UtilDal
    {
        internal static PropertyInfo[] TypeRowToPropertyInfoList(Type typeRow)
        {
            return typeRow.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

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
            PropertyInfo[] propertyInfoList = TypeRowToPropertyInfoList(typeRow);
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
        private static DbContext DbContext(Type typeRow)
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

        internal static async Task ExecuteAsync(string sql, bool isFrameworkDb)
        {
            var sqlList = sql.Split(new string[] { "\r\nGO", "\nGO", "GO\r\n", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);

            string connectionString = ConfigFramework.ConnectionString(isFrameworkDb);
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                foreach (string sqlItem in sqlList)
                {
                    SqlCommand sqlCommand = new SqlCommand(sqlItem, sqlConnection);
                    await sqlCommand.ExecuteNonQueryAsync();
                }
            }
        }

        internal static async Task ExecuteAsync(string sql)
        {
            await ExecuteAsync(sql, false);
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
            var propertyInfoDestList = UtilDal.TypeRowToPropertyInfoList(rowDest.GetType());
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
            var propertyInfoList = UtilDal.TypeRowToPropertyInfoList(rowA.GetType());
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
        /// Update data record on database.
        /// </summary>
        public static async Task UpdateAsync(Row row, Row rowNew)
        {
            UtilFramework.LogDebug(string.Format("UPDATE ({0})", row.GetType().Name));

            UtilFramework.Assert(row.GetType() == rowNew.GetType());
            if (UtilDal.RowEqual(row, rowNew) == false)
            {
                row = UtilDal.RowCopy(row); // Prevent modifications on SetValues(rowNew);
                DbContext dbContext = DbContext(row.GetType());
                var tracking = dbContext.Attach(row);
                tracking.CurrentValues.SetValues(rowNew);
                int count = await dbContext.SaveChangesAsync();
                UtilFramework.Assert(count == 1, "Update failed!");
            }
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
    }

    internal enum FilterOperator
    {
        None = 0,
        Equal = 1,
        Smaller = 2,
        Greater = 3,
        Like = 4
    }
}
