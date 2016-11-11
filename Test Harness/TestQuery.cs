using System;

namespace Keeper.LSharp
{
    public class TestQuery
        : Query
    {
        private readonly Func<bool> predicate;

        public TestQuery(Func<bool> predicate)
        {
            this.predicate = predicate;
        }

        public override QueryResult Run()
        {
            return this.predicate()
                ? QueryResult.Success
                : QueryResult.Fail;
        }
    }
}
