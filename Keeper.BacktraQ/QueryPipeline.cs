using System;

namespace Keeper.BacktraQ
{
    public class QueryPipeline
        : Query
    {
        private readonly Query pipeline;
        private readonly Query initial;

        public QueryPipeline(Query initial, Query pipeline)
        {
            this.initial = initial;
            this.pipeline = pipeline;
        }

        protected internal override QueryResult Run()
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
                    var pipelineQuery = this.pipeline;
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
    }

    public static class QueryPipelineExtensions
    {
        public static Query And(this Query query, Query next)
        {
            return new QueryPipeline(query, next);
        }

        public static Query And(this Query query, Func<Query> next)
        {
            return new QueryPipeline(query, Query.Create(next));
        }
    }
}
