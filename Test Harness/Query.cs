namespace Keeper.LSharp
{
    public abstract class Query<T>
    {
        public abstract QueryResult<T> Run();
    }

    public abstract class Query<TState, TResult>
    {
        public abstract QueryResult<TResult> Run(TState state);
    }
}
