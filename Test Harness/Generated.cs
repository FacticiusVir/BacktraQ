using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper.BacktraQ
{
    public static class Generated
    {
        public static Generated<T> Random<T>(params T[] items)
        {
            return Random((IEnumerable<T>)items);
        }

        public static Generated<T> Random<T>(IEnumerable<T> items)
        {
            return new Generated<T>(VarList.Create(items.ToArray()).RandomMember);
        }
    }

    public class Generated<T>
        : IVar<T>
    {
        private Var<T> value;
        private Func<Var<T>, Query> generate;

        public Generated(Func<Var<T>, Query> generate)
        {
            this.value = new Var<T>();
            this.generate = generate;
        }

        public Query Unify(Var<T> value)
        {
            return this.value.Unify(value)
                        .And(this.generate, this.value);
        }

        public Query Unify(IVar<T> other)
        {
            return other.Unify(this.value)
                        .And(this.generate, this.value);
        }

        public bool HasValue
        {
            get
            {
                return this.value.HasValue;
            }
        }

        public T Value
        {
            get
            {
                if (!this.value.HasValue)
                {
                    this.generate(this.value).Commit().Succeeds();
                }

                return this.value.Value;
            }
        }
    }
}
