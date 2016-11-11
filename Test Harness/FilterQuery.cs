using System;

namespace Keeper.LSharp
{
    public static class FilterQuery
    {
        public static Func<T, FilterQuery<T>> CreatePipeline<T>(Func<T, bool> predicate)
        {
            return state => new FilterQuery<T>(state, predicate);
        }
    }

    public class FilterQuery<TState>
        : Query<TState>
    {
        private readonly TState state;
        private readonly Func<TState, bool> predicate;

        public FilterQuery(TState state, Func<TState, bool> predicate)
        {
            this.state = state;
            this.predicate = predicate;
        }

        public override QueryResult Run()
        {
            if (this.predicate(this.state))
            {
                this.Result = this.state;
                return QueryResult.Success;
            }
            else
            {
                return QueryResult.Fail;
            }
        }
    }
}
