namespace Keeper.BacktraQ
{
    public static class QMath
    {
        public static Query Inc(Var<int> initial, Var<int> result)
        {
            return Query.Map(initial, result, x => x + 1, x => x - 1);
        }

        public static Query Add(Var<int> left, Var<int> right, Var<int> result)
        {
            return Query.Map(left, right, result, (x, y) => x + y, (x, y) => y - x, (x, y) => y - x);
        }
    }
}
