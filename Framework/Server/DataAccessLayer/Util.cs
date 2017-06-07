namespace Framework.Server.DataAccessLayer
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Reflection;

    public static class Util
    {
        public static string TableNameFromTypeRow(Type typeRow)
        {
            SqlNameAttribute attributeRow = (SqlNameAttribute)typeRow.GetTypeInfo().GetCustomAttribute(typeof(SqlNameAttribute));
            return attributeRow.SqlName;
        }

        public static string TypeRowToName(Type typeRow)
        {
            string result = null;
            if (typeRow != null)
            {
                result = typeRow.FullName;
            }
            return result;
        }

        public static Type TypeRowFromName(string typeRowName, Type typeInAssembly)
        {
            Type result = null;
            if (typeRowName != null)
            {
                result = typeInAssembly.GetTypeInfo().Assembly.GetType(typeRowName);
                if (result == null)
                {
                    throw new Exception("Type not found!");
                }
            }
            return result;
        }

        public static Type TypeRowFromTableName(string tableName, Type typeInAssembly)
        {
            foreach (Type type in typeInAssembly.GetTypeInfo().Assembly.GetTypes())
            {
                if (type.GetTypeInfo().IsSubclassOf(typeof(Row)))
                {
                    Type typeRow = type;
                    if (Util.TableNameFromTypeRow(typeRow) == tableName)
                    {
                        return typeRow;
                    }
                }
            }
            throw new Exception(string.Format("Type not found! ({0})", tableName));
        }

        public static List<Cell> ColumnList(Type typeRow)
        {
            List<Cell> result = new List<Cell>();
            if (typeRow != null)
            {
                SqlNameAttribute attributeRow = (SqlNameAttribute)typeRow.GetTypeInfo().GetCustomAttribute(typeof(SqlNameAttribute));
                foreach (PropertyInfo propertyInfo in typeRow.GetTypeInfo().GetProperties())
                {
                    SqlNameAttribute attributePropertySql = (SqlNameAttribute)propertyInfo.GetCustomAttribute(typeof(SqlNameAttribute));
                    TypeCellAttribute attributePropertyCell = (TypeCellAttribute)propertyInfo.GetCustomAttribute(typeof(TypeCellAttribute));
                    Cell cell = (Cell)Activator.CreateInstance(attributePropertyCell.TypeCell);
                    cell.Constructor(attributeRow.SqlName, attributePropertySql.SqlName, propertyInfo.Name);
                    result.Add(cell);
                }
            }
            return result;
        }

        public static List<Cell> CellList(object row)
        {
            List<Cell> result = new List<Cell>();
            result = ColumnList(row.GetType());
            foreach (Cell cell in result)
            {
                cell.Constructor(row);
            }
            return result;
        }

        /// <summary>
        /// Returns DbContext with ConnectionString and model for row defined in typeRow.
        /// </summary>
        private static DbContext DbContext(Type typeRow)
        {
            var conventionBuilder = new CoreConventionSetBuilder();
            var conventionSet = conventionBuilder.CreateConventionSet();
            var builder = new ModelBuilder(conventionSet);
            {
                var entity = builder.Entity(typeRow);
                SqlNameAttribute attributeRow = (SqlNameAttribute)typeRow.GetTypeInfo().GetCustomAttribute(typeof(SqlNameAttribute));
                entity.ToTable(attributeRow.SqlName);
                foreach (PropertyInfo propertyInfo in typeRow.GetTypeInfo().GetProperties())
                {
                    SqlNameAttribute attributeProperty = (SqlNameAttribute)propertyInfo.GetCustomAttribute(typeof(SqlNameAttribute));
                    entity.Property(propertyInfo.PropertyType, propertyInfo.Name).HasColumnName(attributeProperty.SqlName);
                }
            }
            var options = new DbContextOptionsBuilder<DbContext>();
            options.UseSqlServer(Framework.Server.ConnectionManager.ConnectionString);
            options.UseModel(builder.Model);
            DbContext result = new DbContext(options.Options);
            result.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking; // For SQL views. No primary key.
            //
            return result;
        }

        private static IQueryable SelectQuery(Type typeRow)
        {
            DbContext dbContext = DbContext(typeRow);
            dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking; // For SQL views. No primary key.
            IQueryable query = (IQueryable)(dbContext.GetType().GetTypeInfo().GetMethod("Set").MakeGenericMethod(typeRow).Invoke(dbContext, null));
            return query;
        }

        public static object[] Select(Type typeRow)
        {
            return SelectQuery(typeRow).ToDynamicArray();
        }

        public static TRow[] Select<TRow>() where TRow : Row
        {
            return Select(typeof(TRow)).Cast<TRow>().ToArray();
        }

        public static object[] Select(Type typeRow, int id)
        {
            IQueryable query = SelectQuery(typeRow);
            return query.Where("Id = @0", id).ToDynamicArray();
        }

        public static List<Row> Select(Type typeRow, string fieldNameOrderBy, bool isOrderByDesc, int pageIndex, int pageRowCount)
        {
            IQueryable query = SelectQuery(typeRow);
            if (fieldNameOrderBy != null)
            {
                string ordering = fieldNameOrderBy;
                if (isOrderByDesc)
                {
                    ordering = ordering + " DESC";
                }
                query = query.OrderBy(ordering);
            }
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
        /// Update data record on database.
        /// </summary>
        public static void Update(Row row, Row rowNew)
        {
            row = Util.Clone(row); // Prevent modifications on SetValues(rowNew);
            Framework.Util.Assert(row.GetType() == rowNew.GetType());
            DbContext dbContext = DbContext(row.GetType());
            var tracking = dbContext.Attach(row);
            tracking.CurrentValues.SetValues(rowNew);
            dbContext.SaveChanges();
        }

        /// <summary>
        /// Insert data record. Primary key needs to be 0!
        /// </summary>
        public static void Insert(Row row)
        {
            DbContext dbContext = DbContext(row.GetType());
            dbContext.Add(row);
            dbContext.SaveChanges();
        }

        /// <summary>
        /// Delete data record.
        /// </summary>
        public static void Delete(Row row)
        {
            DbContext dbContext = DbContext(row.GetType());
            dbContext.Remove(row);
            dbContext.SaveChanges();
        }

        public static T JsonObjectClone<T>(T data)
        {
            string json = JsonConvert.SerializeObject(data, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
            return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
        }

        public static object ValueToJson(object value)
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

        public static string ValueToText(object value)
        {
            string result = null;
            if (value != null)
            {
                result = value.ToString();
            }
            return result;
        }

        public static object ValueFromText(string text, Type type)
        {
            if (text == null)
            {
                return null;
            }
            if (Nullable.GetUnderlyingType(type) != null)
            {
                type = Nullable.GetUnderlyingType(type);
            }
            return Convert.ChangeType(text, type);
        }

        /// <summary>
        /// Clone data row.
        /// </summary>
        public static Row Clone(Row row)
        {
            Row result = (Row)Activator.CreateInstance(row.GetType());
            var propertyInfoList = row.GetType().GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfoList)
            {
                string fieldName = propertyInfo.Name;
                object value = propertyInfo.GetValue(row);
                propertyInfo.SetValue(result, value);
            }
            return result;
        }
    }
}
