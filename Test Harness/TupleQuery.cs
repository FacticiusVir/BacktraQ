using System;

namespace Keeper.LSharp
{
    public class TupleInjectorQuery
        : Query<int, Tuple<int, int>>
    {
        private readonly Query<int> subQuery;

        public TupleInjectorQuery(Query<int> subQuery)
        {
            this.subQuery = subQuery;
        }

        public override QueryResult<Tuple<int, int>> Run(int state)
        {
            return new TupleQuery(state, this.subQuery).Run();
        }
    }

    public class TupleQuery
        : Query<Tuple<int, int>>
    {
        private readonly int firstValue;
        private readonly Query<int> secondQuery;

        public TupleQuery(int firstValue, Query<int> secondQuery)
        {
            this.firstValue = firstValue;
            this.secondQuery = secondQuery;
        }

        public override QueryResult<Tuple<int, int>> Run()
        {
            var secondResult = this.secondQuery.Run();

            switch (secondResult.Type)
            {
                case QueryResultType.Fail:
                    return QueryResult.Fail<Tuple<int, int>>();
                case QueryResultType.ChoicePoint:
                    var continuation = new TupleQuery(this.firstValue, secondResult.Continuation);
                    var alternate = new TupleQuery(this.firstValue, secondResult.Alternate);

                    return QueryResult.ChoicePoint(continuation, alternate);
                case QueryResultType.Success:
                    return QueryResult.Success(Tuple.Create(this.firstValue, secondResult.Value));
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
