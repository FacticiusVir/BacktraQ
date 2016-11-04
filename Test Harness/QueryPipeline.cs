using System;

namespace Keeper.LSharp
{
    public static class QueryPipeline
    {
        public static QueryPipeline<TState, TResult> Create<TState, TResult>(Query<TState> initial, Query<TState, TResult> continuation)
        {
            return new QueryPipeline<TState, TResult>(initial, continuation);
        }
    }

    public class QueryPipeline<TState, TResult>
        : Query<TResult>
    {
        private readonly Query<TState, TResult> continuation;
        private readonly Query<TState> initial;

        public QueryPipeline(Query<TState> initial, Query<TState, TResult> continuation)
        {
            this.initial = initial;
            this.continuation = continuation;
        }

        public override QueryResult<TResult> Run()
        {
            var initialResult = this.initial.Run();

            switch (initialResult.Type)
            {
                case QueryResultType.Fail:
                    return QueryResult.Fail<TResult>();
                case QueryResultType.ChoicePoint:
                    var continuation = new QueryPipeline<TState, TResult>(initialResult.Continuation, this.continuation);
                    var alternate = new QueryPipeline<TState, TResult>(initialResult.Alternate, this.continuation);

                    return QueryResult.ChoicePoint(continuation, alternate);
                case QueryResultType.Success:
                    return this.continuation.Run(initialResult.Value);
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
