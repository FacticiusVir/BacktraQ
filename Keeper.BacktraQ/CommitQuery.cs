namespace Keeper.BacktraQ
{
    public static class CommitExtensions
    {
        public static Query Commit(this Query query)
        {
            return new Query(() =>
            {
                int trailDepth = Trail.Current.Depth;

                var subQuery = query & (() =>
                {
                    while (Trail.Current.Depth > trailDepth)
                    {
                        Trail.Current.Cut();
                    }

                    return Query.Success;
                });

                return subQuery.Run();
            });
        }
    }
}
