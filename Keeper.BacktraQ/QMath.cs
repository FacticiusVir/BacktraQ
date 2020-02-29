using System;

namespace Keeper.BacktraQ
{
    public static class QMath
    {
        public static Query Inc(this Var<int> initial, Var<int> result)
        {
            return Query.Map(initial, result, x => x + 1, x => x - 1);
        }

        public static Func<Var<int>, Query> Add(this Var<int> left, Var<int> right) => result => Add(left, right, result);

        public static Query Add(Var<int> left, Var<int> right, out Var<int> result) => Add(left, right, Query.NewVar(out result));

        public static Query Add(Var<int> left, Var<int> right, Var<int> result) => Query.Map(left, right, result, (x, y) => x + y, (x, y) => y - x, (x, y) => y - x);

        public static Query LessThan(this Var<int> left, Var<int> right)
        {
            return Query.When(() =>
            {
                return left.Value < right.Value;
            });
        }

        public static Query LessThanOrEqual(this Var<int> left, Var<int> right)
        {
            return Query.When(() =>
            {
                return left.Value <= right.Value;
            });
        }

        public static Query GreaterThan(this Var<int> left, Var<int> right)
        {
            return Query.When(() =>
            {
                return left.Value > right.Value;
            });
        }

        public static Query GreaterThanOrEqual(this Var<int> left, Var<int> right)
        {
            return Query.When(() =>
            {
                return left.Value >= right.Value;
            });
        }

        public static Query Between(this Var<int> value, Var<int> lower, Var<int> upper)
        {
            return lower.LessThanOrEqual(upper)
                        & (() =>
                        {
                            if (value.HasValue)
                            {
                                return lower.LessThanOrEqual(value)
                                        & value.LessThanOrEqual(upper);
                            }
                            else
                            {
                                var nextLower = new Var<int>();

                                return value <= lower
                                            | (nextLower <= lower.Inc & value.Between(nextLower, upper));
                            }
                        });
        }
    }
}
