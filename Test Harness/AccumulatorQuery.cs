using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keeper.LSharp
{
    public class AccumulatorQuery<TState, TSubQuery, TResult>
        : Query<TState, TResult>
    {
        private readonly Func<TState, TSubQuery, TResult> mapping;
        private readonly Query<TSubQuery> subQuery;

        public AccumulatorQuery(Query<TSubQuery> subQuery, Func<TState, TSubQuery, TResult> mapping)
        {
            this.subQuery = subQuery;
            this.mapping = mapping;
        }

        public override QueryResult<TResult> Run(TState state)
        {
            return new EncapsulatedQuery(state, this.subQuery, this.mapping).Run();
        }

        internal class EncapsulatedQuery
            : Query<TResult>
        {
            private Func<TState, TSubQuery, TResult> mapping;
            private TState state;
            private Query<TSubQuery> subQuery;

            public EncapsulatedQuery(TState state, Query<TSubQuery> subQuery, Func<TState, TSubQuery, TResult> mapping)
            {
                this.state = state;
                this.subQuery = subQuery;
                this.mapping = mapping;
            }

            public override QueryResult<TResult> Run()
            {
                var secondResult = this.subQuery.Run();

                switch (secondResult.Type)
                {
                    case QueryResultType.Fail:
                        return QueryResult.Fail<TResult>();
                    case QueryResultType.ChoicePoint:
                        var continuation = new EncapsulatedQuery(this.state, secondResult.Continuation, this.mapping);
                        var alternate = new EncapsulatedQuery(this.state, secondResult.Alternate, this.mapping);

                        return QueryResult.ChoicePoint(continuation, alternate);
                    case QueryResultType.Success:
                        return QueryResult.Success(this.mapping(this.state, secondResult.Value));
                    default:
                        throw new InvalidOperationException();
                }
            }
        }
    }
}
