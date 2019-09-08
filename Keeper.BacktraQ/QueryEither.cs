using System;

namespace Keeper.BacktraQ
{
    public static class QueryEitherExtensions
    {
        public static Query Or(this Query query, Query next)
        {
            return new Query(() => new QueryResult { Type = QueryResultType.ChoicePoint, Continuation = query, Alternate = next });
        }

        public static Query Or(this Query query, Func<Query> next)
        {
            return Or(query, Query.Wrap(next));
        }
    }
}
