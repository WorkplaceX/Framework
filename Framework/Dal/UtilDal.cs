namespace Framework.Dal
{
    using Framework.Config;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata;
    using Microsoft.EntityFrameworkCore.Metadata.Conventions;
    using System;
    using System.Linq;
    using System.Reflection;

    public static class UtilDal
    {
        internal static PropertyInfo[] TypeRowToPropertyList(Type typeRow)
        {
            return typeRow.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// Build model for one typeRow.
        /// </summary>
        private static IMutableModel DbContextModel(Type typeRow)
        {
            var conventionSet = new ConventionSet();
            var builder = new ModelBuilder(conventionSet);
            // Build model
            var entity = builder.Entity(typeRow);
            SqlTableAttribute tableAttribute = (SqlTableAttribute)typeRow.GetTypeInfo().GetCustomAttribute(typeof(SqlTableAttribute));
            entity.ToTable(tableAttribute.SqlTableName, tableAttribute.SqlSchemaName); // By default EF maps sql table name to class name.
            bool isPrimaryKey = false;
            PropertyInfo[] propertyInfoList = UtilDal.TypeRowToPropertyList(typeRow);
            foreach (PropertyInfo propertyInfo in propertyInfoList)
            {
                SqlFieldAttribute columnAttribute = (SqlFieldAttribute)propertyInfo.GetCustomAttribute(typeof(SqlFieldAttribute));
                if (columnAttribute == null || columnAttribute.SqlFieldName == null) // Calculated column. Do not include it in sql select. For example button added to row.
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
                    entity.Property(propertyInfo.PropertyType, propertyInfo.Name).HasColumnName(columnAttribute.SqlFieldName);
                }
            }
            if (isPrimaryKey == false)
            {
                // No primary key defined. For example View. In order to prevent NullException when inserting row,
                // set artificial "Primary Key" on first column (not calculated column). See also method UtilDal.Insert();
                PropertyInfo propertyInfoFirst = null;
                foreach (PropertyInfo propertyInfo in propertyInfoList)
                {
                    SqlFieldAttribute columnAttribute = (SqlFieldAttribute)propertyInfo.GetCustomAttribute(typeof(SqlFieldAttribute));
                    if (columnAttribute != null && columnAttribute.SqlFieldName != null)
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
                entity.Property(propertyInfoFirst.Name).ValueGeneratedOnAdd(); // Read back auto increment key value.
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
            options.UseSqlServer(connectionString); // See also: ConnectionManagerServer.json (Data Source=localhost; Initial Catalog=Database; Integrated Security=True;)
            options.UseModel(DbContextModel(typeRow));
            DbContext result = new DbContext(options.Options);
            //
            return result;
        }

        public static IQueryable Query(Type typeRow)
        {
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
        /// Copy data row. Source and dest need not to be of same type. Only cells available on
        /// both records are copied. See also RowClone();
        /// </summary>
        internal static void RowCopy(Row rowSource, Row rowDest)
        {
            var propertyInfoDestList = UtilDal.TypeRowToPropertyList(rowDest.GetType());
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
        /// Update data record on database.
        /// </summary>
        public static void Update(Row row, Row rowNew)
        {
            UtilFramework.Assert(row.GetType() == rowNew.GetType());
            row = UtilDal.RowCopy(row); // Prevent modifications on SetValues(rowNew);
            DbContext dbContext = DbContext(row.GetType());
            var tracking = dbContext.Attach(row);
            tracking.CurrentValues.SetValues(rowNew);
            UtilFramework.Assert(dbContext.SaveChanges() == 1, "Update failed!");
        }
    }
}
