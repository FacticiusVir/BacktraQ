using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper.BacktraQ
{
    public static class EnumerableQuery
    {
        public static EnumerableQuery<T> Create<T>(IEnumerable<T> enumerable, Var<T> variable)
        {
            return new EnumerableQuery<T>(enumerable, x => variable.TryUnify(x));
        }
    }

    public class EnumerableQuery<T>
        : Query
    {
        private readonly int index;
        private readonly T[] values;
        private readonly bool afterChoicePoint;
        private readonly Func<T, bool> queryAction;

        public EnumerableQuery(IEnumerable<T> enumerable, Func<T, bool> queryAction)
            : this(enumerable.ToArray(), 0, false, queryAction)
        {
        }

        private EnumerableQuery(T[] values, int index, bool afterChoicePoint, Func<T, bool> queryAction)
        {
            this.values = values;
            this.index = index;
            this.afterChoicePoint = afterChoicePoint;
            this.queryAction = queryAction;
        }

        protected internal override QueryResult Run()
        {
            if (this.index >= this.values.Length)
            {
                return QueryResult.Fail;
            }
            else if (this.index == this.values.Length - 1 || this.afterChoicePoint)
            {
                if (this.queryAction(this.values[this.index]))
                {
                    return QueryResult.Success;
                }
                else
                {
                    return QueryResult.Fail;
                }
            }
            else
            {
                return new QueryResult
                {
                    Type = QueryResultType.ChoicePoint,
                    Continuation = new EnumerableQuery<T>(this.values, this.index, true, this.queryAction),
                    Alternate = new EnumerableQuery<T>(this.values, this.index + 1, false, this.queryAction)
                };
            }
        }
    }
}
