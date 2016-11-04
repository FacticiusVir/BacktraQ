using System;

namespace Keeper.LSharp
{
    public class FilterQuery
        : Query<Tuple<int, int>, Tuple<int, int>>
    {
        public override QueryResult<Tuple<int, int>> Run(Tuple<int, int> state)
        {
            if (state.Item1 <= state.Item2)
            {
                return QueryResult.Success(state);
            }
            else
            {
                return QueryResult.Fail<Tuple<int, int>>();
            }
        }
    }
}
