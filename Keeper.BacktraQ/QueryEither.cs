using System;

namespace Keeper.BacktraQ
{
    public class QueryEither
        : Query
    {
        private readonly Query initial;
        private readonly Query alternate;

        public QueryEither(Query initial, Query alternate)
        {
            this.initial = initial;
            this.alternate = alternate;
        }

        protected internal override QueryResult Run()
        {
            return new QueryResult
            {
                Type = QueryResultType.ChoicePoint,
                Continuation = this.initial,
                Alternate = this.alternate
            };
        }
    }

    public static class QueryEitherExtensions
    {
        public static Query Or(this Query query, Query next)
        {
            return new QueryEither(query, next);
        }

        public static Query Or(this Query query, Func<Query> next)
        {
            return new QueryEither(query, Query.Create(next));
        }
    }
}
