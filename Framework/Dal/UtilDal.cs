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


        /// <summary>
        /// Update data record on database.
        /// </summary>
        public static async Task UpdateAsync(Row row, Row rowNew)
        {
            UtilFramework.LogDebug(string.Format("UPDATE ({0})", row.GetType().Name));

            UtilFramework.Assert(row.GetType() == rowNew.GetType());
            row = UtilDal.RowCopy(row); // Prevent modifications on SetValues(rowNew);
            DbContext dbContext = DbContext(row.GetType());
            var tracking = dbContext.Attach(row);
            tracking.CurrentValues.SetValues(rowNew);
            int count = await dbContext.SaveChangesAsync();
            UtilFramework.Assert(count == 1, "Update failed!");
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

        /// <summary>
        /// Parse user entered text and write it to row.
        /// </summary>
        internal static void CellTextToValue(Row row, PropertyInfo propertyInfo, string text)
        {
            object value = Convert.ChangeType(text, propertyInfo.PropertyType);
            propertyInfo.SetValue(row, value);
        }
    }
}
