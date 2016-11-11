using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Keeper.LSharp
{
    public interface IQuery
    {
        QueryResult Run();

        IQuery Continuation
        {
            get;
        }

        IQuery Alternate
        {
            get;
        }
    }

    public abstract class Query
        : IQuery
    {
        public IQuery Alternate
        {
            get;
            protected set;
        }

        public IQuery Continuation
        {
            get;
            protected set;
        }

        public abstract QueryResult Run();
    }

    public abstract class Query<T>
        : IQuery
    {
        public abstract QueryResult Run();

        public T Result
        {
            get;
            protected set;
        }

        public Query<T> Continuation
        {
            get;
            protected set;
        }

        public Query<T> Alternate
        {
            get;
            protected set;
        }

        IQuery IQuery.Continuation
        {
            get
            {
                return this.Continuation;
            }
        }

        IQuery IQuery.Alternate
        {
            get
            {
                return this.Alternate;
            }
        }
    }

    public static class QueryExtensions
    {
        public static bool Succeeds(this IQuery query)
        {
            var queryStack = new Stack<IQuery>();

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

        public static IEnumerable AsEnumerable(this IQuery query)
        {
            var queryStack = new Stack<IQuery>();

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
                        yield return true;
                        break;
                }
            }
        }

        public static IEnumerable<T> AsEnumerable<T>(this Query<T> query)
        {
            bool requiresTrail = Trail.Current == null;

            try
            {
                if (requiresTrail)
                {
                    Trail.Enter();
                }

                var queryStack = new Stack<Query<T>>();

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
                            yield return currentQuery.Result;
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
