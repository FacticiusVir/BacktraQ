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

            switch (initialResult.Type)
            {
                case QueryResultType.Fail:
                    return QueryResult.Fail;
                case QueryResultType.ChoicePoint:
                    return new QueryResult
                    {
                        Type = QueryResultType.ChoicePoint,
                        Continuation = new QueryPipeline(initialResult.Continuation, this.pipeline),
                        Alternate = new QueryPipeline(initialResult.Alternate, this.pipeline)
                    };
                case QueryResultType.Success:
                    return this.pipeline.Run();
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
