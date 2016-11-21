using System;

namespace Keeper.BacktraQ
{
    public class CommitQuery
        : Query
    {
        private readonly Query goal;

        public CommitQuery(Query goal)
        {
            this.goal = goal;
        }

        public override QueryResult Run()
        {
            int trailDepth = Trail.Current.Depth;

            var subQuery = goal.And(() => new TestQuery(() =>
            {
                while (Trail.Current.Depth > trailDepth)
                {
                    Trail.Current.Cut();
                }

                return true;
            }));

            var result = subQuery.Run();

            this.Continuation = subQuery.Continuation;
            this.Alternate = subQuery.Alternate;

            return result;
        }
    }

    public static class CommitExtensions
    {
        public static Query Commit(this Query query)
        {
            return new CommitQuery(query);
        }
    }
}
