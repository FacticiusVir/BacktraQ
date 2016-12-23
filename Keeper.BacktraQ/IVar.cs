namespace Keeper.BacktraQ
{
    public interface IVar<T>
    {
        Query Unify(IVar<T> other);

        Query Unify(Var<T> other);
    }
}
