using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Keeper.BacktraQ
{
    public abstract class Query
    {
        public Query Alternate
        {
            get;
            protected set;
        }

        public Query Continuation
        {
            get;
            protected set;
        }

        public abstract QueryResult Run();

        protected QueryResult InvokeAsPassthrough(Query subQuery)
        {
            var result = subQuery.Run();

            this.Continuation = subQuery.Continuation;
            this.Alternate = subQuery.Alternate;

            return result;
        }

        public static Query Create(Func<bool> predicate)
        {
            return new TestQuery(predicate);
        }

        public static Query Create(Func<Query> query)
        {
            return new PassthroughQuery(query);
        }

        public static Query Not(Query query)
        {
            return new TestQuery(() => !query.Succeeds(true));
        }
    }

    public static class QueryExtensions
    {
        public static bool Succeeds(this Query query, bool revertAll = false)
        {
            foreach(var result in query.AsEnumerable(revertAll))
            {
                return true;
            }

            return false;
        }

        public static IEnumerable AsEnumerable(this Query query, bool revertAll = false)
        {
            try
            {
                Trail.Enter();

                Query nextQuery = query;

                while (nextQuery != null)
                {
                    var currentQuery = nextQuery;
                    var result = currentQuery.Run();

                    switch (result)
                    {
                        case QueryResult.ChoicePoint:
                            nextQuery = currentQuery.Continuation;
                            Trail.Current.ChoicePoint(currentQuery.Alternate);
                            break;
                        case QueryResult.Success:
                            yield return null;
                            nextQuery = Trail.Current.Backtrack();
                            break;
                        default:
                            nextQuery = Trail.Current.Backtrack();
                            break;
                    }
                }
            }
            finally
            {
                Trail.Exit(revertAll);
            }
        }
    }
}
