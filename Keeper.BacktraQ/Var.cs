using System;

namespace Keeper.BacktraQ
{
    public abstract class Var
    {
        internal abstract void Reset();
    }

    public class Var<T>
        : Var
    {
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

        public Query Unify(Var<T> other)
        {
            return new TestQuery(() => this.TryUnify(other));
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
                    return derefThis.Value.Equals(derefOther.Value);
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
    }

    public enum VarState
    {
        Empty,
        Reference,
        Value
    }
}
