namespace Framework.Dal.Memory
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

    /// <summary>
    /// Linq to database or linq to memory.
    /// </summary>
    public enum ScopeEnum
    {
        None = 0,

        /// <summary>
        /// Linq to database.
        /// </summary>
        Database = 1,

        /// <summary>
        /// Linq to memory shared by multiple requests (singleton scope).
        /// </summary>
        MemorySingleton = 2,

        /// <summary>
        /// Linq to memory (request scope).
        /// </summary>
        MemoryRequest = 3
    }

    internal interface IListRow : IList
    {
        MemoryInternal Memory { get; }
    }

    /// <summary>
    /// Row list with reference to memory object.
    /// </summary>
    internal class ListRow<T> : List<T>, IListRow
    {
        public ListRow(MemoryInternal memory)
        {
            this.memory = memory;
        }

        private readonly MemoryInternal memory;

        public MemoryInternal Memory
        {
            get
            {
                return memory;
            }
        }
    }

    /// <summary>
    /// Memory stores row classes.
    /// </summary>
    internal class MemoryInternal
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
        public static MemoryInternal Instance
        {
            get
            {
                return (MemoryInternal)UtilServer.Context.RequestServices.GetService(typeof(MemoryInternal)); // See also method ConfigureServices();
            }
        }

        /// <summary>
        /// Determines based on the query origin where to write back data (to database or memory).
        /// </summary>
        public static ScopeEnum ScopeEnum(IQueryable query)
        {
            var expressionVisitorScope = new ExpressionVisitorScope();
            expressionVisitorScope.Visit(query.Expression);

            ScopeEnum result = Memory.ScopeEnum.None;
            if (expressionVisitorScope.ScopeEnumList.Count == 1)
            {
                return expressionVisitorScope.ScopeEnumList.Single();
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
                ScopeEnumList.Add(ScopeEnum.Database);
            }

            // MemorySingleton
            if (node.Value.GetType().BaseType == typeof(EnumerableQuery))
            {
                var enumerable = node.Value.GetType().GetField("_enumerable", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(node.Value);
                if (enumerable is IListRow)
                {
                    MemoryInternal memory = ((IListRow)enumerable).Memory;
                    if (memory == MemoryInternal.Instance)
                    {
                        ScopeEnumList.Add(ScopeEnum.MemorySingleton);
                    }
                }
            }

            return base.VisitConstant(node);
        }

        /// <summary>
        /// Gets ScopeEnumList. Detected scope origins by ExpressionVisitor.
        /// </summary>
        public HashSet<ScopeEnum> ScopeEnumList = new HashSet<ScopeEnum>();
    }
}
