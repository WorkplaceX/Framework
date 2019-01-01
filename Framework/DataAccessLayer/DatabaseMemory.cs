namespace Framework.DataAccessLayer.DatabaseMemory
{
    using Framework.Server;
    using Microsoft.EntityFrameworkCore.Query.Internal;
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal interface IListRow : IList
    {
        DatabaseMemoryInternal DatabaseMemory { get; }
    }

    /// <summary>
    /// Row list with reference to memory object.
    /// </summary>
    internal class ListRow<T> : List<T>, IListRow
    {
        public ListRow(DatabaseMemoryInternal databaseMemory)
        {
            this.databaseMemory = databaseMemory;
        }

        private readonly DatabaseMemoryInternal databaseMemory;

        public DatabaseMemoryInternal DatabaseMemory
        {
            get
            {
                return databaseMemory;
            }
        }
    }

    /// <summary>
    /// Database memory stores row objects in memory.
    /// </summary>
    internal class DatabaseMemoryInternal
    {
        /// <summary>
        /// (TypeRow, List<Row>). Memory row store.
        /// </summary>
        private readonly ConcurrentDictionary<Type, IListRow> rowList = new ConcurrentDictionary<Type, IListRow>();

        /// <summary>
        /// Returns list of stored memory rows of typeRow.
        /// </summary>
        public IList RowListGet(Type typeRow)
        {
            IList result = rowList.GetOrAdd(typeRow, (Type typeRowLocal) => {
                var typeRowList = typeof(ListRow<>).MakeGenericType(typeRowLocal);
                var resultLocal = (IList)Activator.CreateInstance(typeRowList, this);
                return (IListRow)resultLocal; // resultLocal;
            });

            return result;
        }

        /// <summary>
        /// Overload.
        /// </summary>
        public List<T> RowListGet<T>()
        {
            return (List<T>)RowListGet(typeof(T));
        }

        /// <summary>
        /// Returns singleton scope memory instance.
        /// </summary>
        public static DatabaseMemoryInternal Instance
        {
            get
            {
                return (DatabaseMemoryInternal)UtilServer.Context.RequestServices.GetService(typeof(DatabaseMemoryInternal)); // See also method ConfigureServices();
            }
        }

        /// <summary>
        /// Determines based on the query origin where to write back data (to database or memory).
        /// </summary>
        public static DatabaseEnum DatabaseEnum(IQueryable query)
        {
            var expressionVisitorScope = new ExpressionVisitorScope();
            expressionVisitorScope.Visit(query.Expression);

            DatabaseEnum result = Framework.DataAccessLayer.DatabaseEnum.None;
            if (expressionVisitorScope.DatabaseEnumList.Count == 1)
            {
                return expressionVisitorScope.DatabaseEnumList.Single();
            }
            return result;
        }
    }

    /// <summary>
    /// Parse query to find out if it was built with database or memory store or combination.
    /// </summary>
    internal class ExpressionVisitorScope : ExpressionVisitor
    {
        protected override Expression VisitConstant(ConstantExpression node)
        {
            // Database
            if (node.Type.IsGenericType && node.Type.GetGenericTypeDefinition() == typeof(EntityQueryable<>))
            {
                DatabaseEnumList.Add(DatabaseEnum.Database);
            }

            // MemorySingleton
            if (node.Value.GetType().BaseType == typeof(EnumerableQuery))
            {
                var enumerable = node.Value.GetType().GetField("_enumerable", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(node.Value);
                if (enumerable is IListRow)
                {
                    DatabaseMemoryInternal databaseMemory = ((IListRow)enumerable).DatabaseMemory;
                    if (databaseMemory == DatabaseMemoryInternal.Instance)
                    {
                        DatabaseEnumList.Add(DatabaseEnum.MemorySingleton);
                    }
                }
            }

            return base.VisitConstant(node);
        }

        /// <summary>
        /// Gets DatabaseEnumList. Detected scope origins by ExpressionVisitor.
        /// </summary>
        public HashSet<DatabaseEnum> DatabaseEnumList = new HashSet<DatabaseEnum>();
    }
}
