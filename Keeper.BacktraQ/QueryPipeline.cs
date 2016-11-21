using System;

namespace Keeper.BacktraQ
{
    public class QueryPipeline
        : Query
    {
        private readonly Func<Query> pipeline;
        private readonly Query initial;

        public QueryPipeline(Query initial, Func<Query> pipeline)
        {
            this.initial = initial;
            this.pipeline = pipeline;
        }

        public override QueryResult Run()
        {
            var initialResult = this.initial.Run();

            switch (initialResult)
            {
                case QueryResult.Fail:
                    return QueryResult.Fail;
                case QueryResult.ChoicePoint:
                    this.Continuation = new QueryPipeline(this.initial.Continuation, this.pipeline);
                    this.Alternate = new QueryPipeline(this.initial.Alternate, this.pipeline);

                    return QueryResult.ChoicePoint;
                case QueryResult.Success:
                    var pipelineQuery = this.pipeline();
                    var pipelineResult = pipelineQuery.Run();

                    switch (pipelineResult)
                    {
                        case QueryResult.ChoicePoint:
                            this.Continuation = pipelineQuery.Continuation;
                            this.Alternate = pipelineQuery.Alternate;
                            break;
                    }

                    return pipelineResult;
                default:
                    throw new InvalidOperationException();
            }
        }

        //public static QueryPipeline<TState, TResult> Create<TState, TSubQuery, TResult>(Query<TState> initial, Query<TSubQuery> subQuery, Func<TState, TSubQuery, TResult> mapping)
        //{
        //    var pipeline = AccumulatorQuery.CreatePipeline(subQuery, mapping);

        //    return Create(initial, pipeline);
        //}

        //public static QueryPipeline<TState, TState> Create<TState>(Query<TState> initial, Func<TState, bool> predicate)
        //{
        //    var pipeline = FilterQuery.CreatePipeline(predicate);

        //    return Create(initial, pipeline);
        //}

        //public static QueryPipeline<TState, TResult> Create<TState, TResult>(Query<TState> initial, Func<TState, Query<TResult>> pipeline)
        //{
        //    return new QueryPipeline<TState, TResult>(initial, pipeline);
        //}
    }

    public static class QueryPipelineExtensions
    {
        public static Query And(this Query query, Func<Query> next)
        {
            return new QueryPipeline(query, next);
        }

        public static Query And<T>(this Query query, Func<T, Query> next, T param)
        {
            return new QueryPipeline(query, () => next(param));
        }

        public static Query And<T, V>(this Query query, Func<T, V, Query> next, T param1, V param2)
        {
            return new QueryPipeline(query, () => next(param1, param2));
        }
    }
}
