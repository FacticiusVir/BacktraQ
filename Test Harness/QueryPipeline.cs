using System;

namespace Keeper.LSharp
{
    public class QueryPipeline
        : Query
    {
        private readonly Func<IQuery> pipeline;
        private readonly IQuery initial;

        public QueryPipeline(IQuery initial, Func<IQuery> pipeline)
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

        public static QueryPipeline<TState, TResult> Create<TState, TSubQuery, TResult>(Query<TState> initial, Query<TSubQuery> subQuery, Func<TState, TSubQuery, TResult> mapping)
        {
            var pipeline = AccumulatorQuery.CreatePipeline(subQuery, mapping);

            return Create(initial, pipeline);
        }

        public static QueryPipeline<TState, TState> Create<TState>(Query<TState> initial, Func<TState, bool> predicate)
        {
            var pipeline = FilterQuery.CreatePipeline(predicate);

            return Create(initial, pipeline);
        }

        public static QueryPipeline<TState, TResult> Create<TState, TResult>(Query<TState> initial, Func<TState, Query<TResult>> pipeline)
        {
            return new QueryPipeline<TState, TResult>(initial, pipeline);
        }
    }

    public class QueryPipeline<TState, TResult>
        : Query<TResult>
    {
        private readonly Func<TState, Query<TResult>> pipeline;
        private readonly Query<TState> initial;

        public QueryPipeline(Query<TState> initial, Func<TState, Query<TResult>> pipeline)
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
                    this.Continuation = new QueryPipeline<TState, TResult>(this.initial.Continuation, this.pipeline);
                    this.Alternate = new QueryPipeline<TState, TResult>(this.initial.Alternate, this.pipeline);

                    return QueryResult.ChoicePoint;
                case QueryResult.Success:
                    var pipelineQuery = this.pipeline(this.initial.Result);
                    var pipelineResult = pipelineQuery.Run();

                    switch (pipelineResult)
                    {
                        case QueryResult.ChoicePoint:
                            this.Continuation = pipelineQuery.Continuation;
                            this.Alternate = pipelineQuery.Alternate;
                            break;
                        case QueryResult.Success:
                            this.Result = pipelineQuery.Result;
                            break;
                    }

                    return pipelineResult;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
