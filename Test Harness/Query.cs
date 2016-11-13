using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Keeper.LSharp
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
    }

    public static class QueryExtensions
    {
        public static bool Succeeds(this Query query)
        {
            var queryStack = new Stack<Query>();

            queryStack.Push(query);

            while (queryStack.Any())
            {
                var currentQuery = queryStack.Pop();
                var result = currentQuery.Run();

                switch (result)
                {
                    case QueryResult.ChoicePoint:
                        queryStack.Push(currentQuery.Alternate);
                        queryStack.Push(currentQuery.Continuation);
                        break;
                    case QueryResult.Success:
                        return true;
                }
            }

            return false;
        }

        public static IEnumerable AsEnumerable(this Query query)
        {
            bool requiresTrail = Trail.Current == null;

            try
            {
                if (requiresTrail)
                {
                    Trail.Enter();
                }

                var queryStack = new Stack<Query>();

                queryStack.Push(query);

                while (queryStack.Any())
                {
                    var currentQuery = queryStack.Pop();
                    var result = currentQuery.Run();

                    switch (result)
                    {
                        case QueryResult.ChoicePoint:
                            queryStack.Push(currentQuery.Alternate);
                            queryStack.Push(currentQuery.Continuation);
                            Trail.Current.ChoicePoint();
                            break;
                        case QueryResult.Success:
                            yield return null;
                            Trail.Current.Backtrack();
                            break;
                        default:
                            Trail.Current.Backtrack();
                            break;
                    }
                }
            }
            finally
            {
                if (requiresTrail)
                {
                    Trail.Exit();
                }
            }
        }
    }
}
