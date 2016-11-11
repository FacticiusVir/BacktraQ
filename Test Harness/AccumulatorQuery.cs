using System;

namespace Keeper.LSharp
{
    public static class AccumulatorQuery
    {
        public static Func<TState, Query<TResult>> CreatePipeline<TState, TSubQuery, TResult>(Query<TSubQuery> subQuery, Func<TState, TSubQuery, TResult> mapping)
        {
            return state => new AccumulatorQuery<TState, TSubQuery, TResult>(state, subQuery, mapping);
        }
    }

    public class AccumulatorQuery<TState, TSubQuery, TResult>
        : Query<TResult>
    {
        private readonly TState state;
        private readonly Query<TSubQuery> subQuery;
        private readonly Func<TState, TSubQuery, TResult> mapping;
        
        public AccumulatorQuery(TState state, Query<TSubQuery> subQuery, Func<TState, TSubQuery, TResult> mapping)
        {
            this.state = state;
            this.subQuery = subQuery;
            this.mapping = mapping;
        }

        public override QueryResult Run()
        {
            var secondResult = this.subQuery.Run();

            switch (secondResult)
            {
                case QueryResult.Fail:
                    return QueryResult.Fail;
                case QueryResult.ChoicePoint:
                    this.Continuation = new AccumulatorQuery<TState, TSubQuery, TResult>(this.state, this.subQuery.Continuation, this.mapping);
                    this.Alternate = new AccumulatorQuery<TState, TSubQuery, TResult>(this.state, this.subQuery.Alternate, this.mapping);

                    return QueryResult.ChoicePoint;
                case QueryResult.Success:
                    this.Result = this.mapping(this.state, this.subQuery.Result);

                    return QueryResult.Success;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
