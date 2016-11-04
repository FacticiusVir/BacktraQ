using System;

namespace Keeper.LSharp
{
    public class TupleQuery
        : AccumulatorQuery<int, int, Tuple<int, int>>
    {
        public TupleQuery(Query<int> subQuery)
            : base(subQuery, Tuple.Create)
        {
        }
    }
}
