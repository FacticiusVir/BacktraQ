using System;

namespace Keeper.BacktraQ
{
    public class QueryEither
        : Query
    {
        private readonly Query alternateFunc;
        private readonly Query initial;

        public QueryEither(Query initial, Query alternate)
        {
            this.initial = initial;
            this.alternateFunc = alternate;
        }

        protected internal override QueryResult Run()
        {
            this.Continuation = this.initial;
            this.Alternate = this.alternateFunc;

            return QueryResult.ChoicePoint;
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
