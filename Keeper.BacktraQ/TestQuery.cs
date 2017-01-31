using System;

namespace Keeper.BacktraQ
{
    public class TestQuery
        : Query
    {
        private readonly Func<bool> predicate;

        public TestQuery(Func<bool> predicate)
        {
            this.predicate = predicate;
        }

        protected internal override QueryResult Run()
        {
            return this.predicate()
                ? QueryResult.Success
                : QueryResult.Fail;
        }
    }
}
