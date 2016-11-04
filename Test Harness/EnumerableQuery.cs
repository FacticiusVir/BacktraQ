using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper.LSharp
{
    public static class EnumerableQuery
    {
        public static EnumerableQuery<T> Create<T>(IEnumerable<T> enumerable)
        {
            return new EnumerableQuery<T>(enumerable);
        }
    }

    public class EnumerableQuery<T>
        : Query<T>
    {
        private readonly int index;
        private readonly T[] values;
        private readonly bool afterChoicePoint;

        public EnumerableQuery(IEnumerable<T> enumerable)
            : this(enumerable.ToArray(), 0, false)
        {
        }

        private EnumerableQuery(T[] values, int index, bool afterChoicePoint)
        {
            this.values = values;
            this.index = index;
            this.afterChoicePoint = afterChoicePoint;
        }

        public override QueryResult<T> Run()
        {
            if(this.index >= this.values.Length)
            {
                return QueryResult.Fail<T>();
            }
            else if(this.index == this.values.Length - 1 || this.afterChoicePoint)
            {
                return QueryResult.Success(this.values[this.index]);
            }
            else
            {
                return QueryResult.ChoicePoint(
                                        new EnumerableQuery<T>(this.values, this.index, true),
                                        new EnumerableQuery<T>(this.values, this.index + 1, false));
            }
        }
    }
}
