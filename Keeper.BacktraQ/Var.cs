using System;

namespace Keeper.BacktraQ
{
    public abstract class Var
    {
        protected static int count = 0;

        internal abstract void Reset();

        public static void Optional<T>(ref Var<T> variable)
        {
            variable = variable ?? new Var<T>();
        }
    }

    public class Var<T>
        : Var, IVar<T>
    {
        private readonly int index = System.Threading.Interlocked.Increment(ref count);
        private T value;
        private Var<T> reference;

        public Var()
        {
            this.State = VarState.Empty;
        }

        public VarState State
        {
            get;
            private set;
        }

        public bool HasValue
        {
            get
            {
                if (this.State == VarState.Reference)
                {
                    return this.reference.HasValue;
                }
                else
                {
                    return this.State == VarState.Value;
                }
            }
        }

        public T Value
        {
            get
            {
                switch (this.State)
                {
                    case VarState.Value:
                        return this.value;
                    case VarState.Reference:
                        return this.reference.Value;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        internal override void Reset()
        {
            this.State = VarState.Empty;
            this.value = default(T);
            this.reference = null;
        }

        private Var<T> Dereference()
        {
            if (this.State == VarState.Reference)
            {
                return this.reference.Dereference();
            }
            else
            {
                return this;
            }
        }

        public static implicit operator Var<T>(T value)
        {
            return new Var<T>
            {
                value = value,
                State = VarState.Value
            };
        }

        public bool TryUnify(Var<T> other)
        {
            var derefThis = this.Dereference();
            var derefOther = other.Dereference();

            if (derefThis == derefOther)
            {
                return true;
            }

            if (derefThis.HasValue)
            {
                if (derefOther.HasValue)
                {
                    //TODO Implement general type unification
                    var unifiable = derefThis.value as IUnifiable<T>;

                    if (unifiable != null)
                    {
                        return unifiable.TryUnify(derefOther.Value);
                    }
                    else
                    {
                        return derefThis.Value.Equals(derefOther.Value);
                    }
                }
                else
                {
                    return other.TryUnify(this);
                }
            }

            Trail.Current?.Log(derefThis);

            derefThis.reference = derefOther;
            derefThis.State = VarState.Reference;

            return true;
        }

        public override string ToString()
        {
            if (this.HasValue)
            {
                return this.Value.ToString();
            }
            else
            {
                return "#" + this.Dereference().index;
            }
        }

        Query IVar<T>.Unify(IVar<T> other)
        {
            return other.Unify(this);
        }

        Query IVar<T>.Unify(Var<T> other)
        {
            return this.Unify(other);
        }

        public static Query operator <=(Var<T> left, Var<T> right)
        {
            return left.Unify(right);
        }

        public static Query operator <=(Var<T> variable, Func<Var<T>, Query> bind)
        {
            return bind(variable);
        }

        public static Query operator >=(Var<T> left, Var<T> right)
        {
            return left.Unify(right);
        }

        public static Query operator >=(Var<T> variable, Func<Var<T>, Query> bind)
        {
            return bind(variable);
        }
    }

    public static class VarExtensions
    {
        public static Query Unify<T>(this Var<T> variable, T value)
        {
            return variable == null
                ? Query.Success
                : Query.Create(() => variable.TryUnify(value));
        }

        public static Query Unify<T>(this Var<T> variable, Var<T> other)
        {
            return variable == null
                ? Query.Success
                : Query.Create(() => variable.TryUnify(other));
        }

        public static Query IsVar<T>(this Var<T> variable)
        {
            return variable == null
                ? Query.Success
                : Query.Create(() => !variable.HasValue);
        }

        public static Query IsNonVar<T>(this Var<T> variable)
        {
            return variable == null
                ? Query.Fail
                : Query.Create(() => variable.HasValue);
        }
    }

    public enum VarState
    {
        Empty,
        Reference,
        Value
    }
}
