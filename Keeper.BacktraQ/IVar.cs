namespace Keeper.BacktraQ
{
    public interface IVar<T>
    {
        bool HasValue { get; }

        T Value { get; }

        Query Unify(IVar<T> other);

        Query Unify(Var<T> other);
    }
}
