using System;

namespace Keeper.BacktraQ
{
    public class ActionQuery
        : Query
    {
        private readonly Action action;
        private readonly Action rollback;

        private bool hasRun;
        
        public ActionQuery(Action action, Action rollback = null)
        {
            this.action = action;
            this.rollback = rollback;
        }

        protected internal override QueryResult Run()
        {
            this.action();

            var rollbackQuery = this.rollback != null
                                    ? new ActionQuery(this.rollback, null)
                                    : Fail;

            return new QueryResult()
            {
                Type = QueryResultType.ChoicePoint,
                Continuation = Success,
                Alternate = rollbackQuery
            };
        }
    }
}
