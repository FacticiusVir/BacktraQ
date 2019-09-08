using System;

namespace Keeper.BacktraQ
{
    public static class QueryPipelineExtensions
    {
        public static Query And(this Query query, Query next)
        {
            return new Query(() =>
            {
                var initialResult = query.Run();

                switch (initialResult.Type)
                {
                    case QueryResultType.Fail:
                        return QueryResult.Fail;
                    case QueryResultType.ChoicePoint:
                        return new QueryResult
                        {
                            Type = QueryResultType.ChoicePoint,
                            Continuation = initialResult.Continuation & next,
                            Alternate = initialResult.Alternate & next
                        };
                    case QueryResultType.Success:
                        return next.Run();
                    default:
                        throw new InvalidOperationException();
                }
            });
        }

        public static Query And(this Query query, Func<Query> next)
        {
            return And(query, Query.Wrap(next));
        }
    }
}
