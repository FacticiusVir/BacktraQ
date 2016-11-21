namespace Keeper.BacktraQ
{
    public interface IUnifiable<T>
    {
        bool TryUnify(T other);
    }
}
