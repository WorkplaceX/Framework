﻿namespace Framework.DataAccessLayer
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
    using System;
    using System.Collections.Generic;
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
        public string FieldName;

        public FilterOperator FilterOperator;

        public object Value;
    }

    public static class UtilDataAccessLayer
    {
        public static string TableNameFromTypeRow(Type typeRow)
        {
            SqlNameAttribute attributeRow = (SqlNameAttribute)typeRow.GetTypeInfo().GetCustomAttribute(typeof(SqlNameAttribute));
            return attributeRow.SqlName;
        }

        /// <summary>
        /// Returns row type as string. For example: "Database.dbo.User".
        /// </summary>
        public static string TypeRowToName(Type typeRow)
        {
            UtilFramework.Assert(UtilFramework.IsSubclassOf(typeRow, typeof(Row)), "Wrong type!");
            string result = null;
            if (typeRow != null)
            {
                result = typeRow.FullName;
            }
            return result;
        }

        /// <summary>
        /// Returns TypeRowList of all in code defined Row classes. Returns also framework Row classes.
        /// </summary>
        public static Type[] TypeRowList(Type typeRowInAssembly)
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
        /// <param name="typeRow">For example: "Database.dbo.User".</param>
        public static Type TypeRowFromName(string typeRow, Type typeRowInAssembly)
        {
            List<Type> result = new List<Type>();
            if (typeRow != null)
            {
                Type[] typeInAssemblyList = UtilFramework.TypeInAssemblyList(typeRowInAssembly);
                foreach (Type itemTypeInAssembly in typeInAssemblyList)
                {
                    Type type = itemTypeInAssembly.GetTypeInfo().Assembly.GetType(typeRow);
                    if (type != null)
                    {
                        result.Add(type);
                    }
                }
                if (result.Count == 0)
                {
                    UtilFramework.Assert(false, "Type not found!");
                }
                if (result.Count > 1)
                {
                    UtilFramework.Assert(false, string.Format("Row type more than once defined! ({0})", typeRow));
                }
            }
            UtilFramework.Assert(UtilFramework.IsSubclassOf(result.FirstOrDefault(), typeof(Row)), "Wrong type!");
            return result.FirstOrDefault();
        }

        [ThreadStatic]
        private static Dictionary<Type, List<Cell>> cacheColumnList;

        public static List<Cell> ColumnList(Type typeRow)
        {
            if (cacheColumnList == null)
            {
                cacheColumnList = new Dictionary<Type, List<Cell>>();
            }
            if (cacheColumnList.ContainsKey(typeRow))
            {
                foreach (Cell column in cacheColumnList[typeRow])
                {
                    column.Constructor(null); // Column mode.
                }
                return cacheColumnList[typeRow];
            }
            //
            List<Cell> result = new List<Cell>();
            if (typeRow != null)
            {
                SqlNameAttribute attributeRow = (SqlNameAttribute)typeRow.GetTypeInfo().GetCustomAttribute(typeof(SqlNameAttribute));
                foreach (PropertyInfo propertyInfo in typeRow.GetTypeInfo().GetProperties())
                {
                    SqlNameAttribute attributePropertySql = (SqlNameAttribute)propertyInfo.GetCustomAttribute(typeof(SqlNameAttribute));
                    TypeCellAttribute attributePropertyCell = (TypeCellAttribute)propertyInfo.GetCustomAttribute(typeof(TypeCellAttribute));
                    string sqlName = null;
                    if (attributePropertySql != null)
                    {
                        sqlName = attributePropertySql.SqlName;
                    }
                    Type typeCell = typeof(Cell); // Default cell api.
                    if (attributePropertyCell != null) // Reference from entity property to cell. If no cell api is defined, stick, with default cell api.
                    {
                        typeCell = attributePropertyCell.TypeCell;
                    }
                    Cell cell = (Cell)UtilFramework.TypeToObject(typeCell);
                    cell.Constructor(attributeRow.SqlName, sqlName, propertyInfo.Name, typeRow, propertyInfo.PropertyType, propertyInfo);
                    result.Add(cell);
                }
            }
            return result;
        }

        /// <summary>
        /// Returns cell list. Or column list, if row is null.
        /// </summary>
        public static List<Cell> CellList(Type typeRow, object row)
        {
            List<Cell> result = new List<Cell>();
            result = ColumnList(typeRow); // For column row is null and row.GetType() is not possible.
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
                    if (attributeProperty == null) // Calculated column. Do not include it in sql select.
                    {
                        entity.Ignore(propertyInfo.Name);
                    }
                    else
                    {
                        entity.Property(propertyInfo.PropertyType, propertyInfo.Name).HasColumnName(attributeProperty.SqlName);
                    }
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

        public static IQueryable<TRow> Select<TRow>() where TRow : Row
        {
            return (IQueryable<TRow>)SelectQuery(typeof(TRow));
        }

        public static object[] Select(Type typeRow, int id)
        {
            IQueryable query = SelectQuery(typeRow);
            return query.Where("Id = @0", id).ToDynamicArray();
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
                    filterSql += filter.FieldName;
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

        public static List<Row> Select(Type typeRow, List<Filter> filterList, string fieldNameOrderBy, bool isOrderByDesc, int pageIndex, int pageRowCount)
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
        /// Update data record on database.
        /// </summary>
        public static void Update(Row row, Row rowNew)
        {
            row = UtilDataAccessLayer.RowClone(row); // Prevent modifications on SetValues(rowNew);
            Framework.UtilFramework.Assert(row.GetType() == rowNew.GetType());
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

        public static string ValueToText(object value, Type type)
        {
            type = UtilFramework.TypeUnderlying(type);
            //
            if (type == typeof(byte[]) && value != null)
            {
                return Encoding.Unicode.GetString((byte[])value);
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
        public static object ValueFromText(string text, Type type)
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
        /// Copy data row. Source and dest need not to be of same type. Only fields available on
        /// both records are copied.
        /// </summary>
        public static void RowCopy(Row rowSource, Row rowDest)
        {
            var propertyInfoDestList = rowDest.GetType().GetProperties();
            foreach (PropertyInfo propertyInfoDest in propertyInfoDestList)
            {
                string fieldName = propertyInfoDest.Name;
                PropertyInfo propertyInfoSource = rowSource.GetType().GetTypeInfo().GetProperty(fieldName);
                object value = propertyInfoSource.GetValue(rowSource);
                propertyInfoDest.SetValue(rowDest, value);
            }
        }

        /// <summary>
        /// Returns new data row.
        /// </summary>
        public static Row RowCreate(Type typeRow)
        {
            return (Row)UtilFramework.TypeToObject(typeRow);
        }
    }
}
