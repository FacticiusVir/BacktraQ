using System;

namespace Keeper.LSharp
{
    public class QueryEither
        : Query
    {
        private readonly Func<Query> alternateFunc;
        private readonly Query initial;

        public QueryEither(Query initial, Func<Query> alternate)
        {
            this.initial = initial;
            this.alternateFunc = alternate;
        }

        public override QueryResult Run()
        {
            this.Continuation = this.initial;
            this.Alternate = this.alternateFunc();

            return QueryResult.ChoicePoint;
        }
    }

    public static class QueryEitherExtensions
    {
        public static Query Or(this Query query, Func<Query> next)
        {
            return new QueryEither(query, next);
        }

        public static Query Or<T>(this Query query, Func<T, Query> next, T param)
        {
            return new QueryEither(query, () => next(param));
        }

        public static Query Or<T, V>(this Query query, Func<T, V, Query> next, T param1, V param2)
        {
            return new QueryEither(query, () => next(param1, param2));
        }
    }
}
