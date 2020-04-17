using System;
using System.Runtime.CompilerServices;

namespace Keeper.BacktraQ
{
    public abstract class Var
    {
        protected static int count = 0;

        internal abstract void Reset();

        internal abstract bool TryUnify(Var other);

        public static void Optional<T>(ref Var<T> variable)
        {
            variable ??= new Var<T>();
        }
    }

    public class Var<T>
        : Var
    {
        private readonly int index = System.Threading.Interlocked.Increment(ref count);
        private T value;
        private Var<T> reference;

        public Var()
        {
            this.State = VarState.Undefined;
        }

        protected Var(bool isEmpty)
        {
            this.State = isEmpty ? VarState.Empty : VarState.Undefined;
        }

        protected Var(T value)
        {
            this.value = value;
            this.State = VarState.Value;
        }

        public VarState State
        {
            get;
            private set;
        }

        public static Var<T> Empty => new Var<T> { State = VarState.Empty };

        public bool HasValue =>
            (this.State == VarState.Reference && this.reference.HasValue)
            || this.State == VarState.Value;

        public T Value
        {
            get
            {
                return this.State switch
                {
                    VarState.Value => this.value,
                    VarState.Reference => this.reference.Value,
                    _ => throw new InvalidOperationException(),
                };
            }
        }

        public T GetValueOrDefault(T defaultValue = default) => this.HasValue ? this.Value : defaultValue;

        public bool IsBound => this.State != VarState.Undefined;

        internal override void Reset()
        {
            this.State = VarState.Undefined;
            this.value = default;
            this.reference = null;
        }

        internal Var<T> Dereference() => this.State == VarState.Reference ? this.reference.Dereference() : this;

        public static implicit operator Var<T>(T value) => new Var<T>(value);

        internal override bool TryUnify(Var other) => this.TryUnify((Var<T>)other);

        public bool TryUnify(Var<T> other)
        {
            var derefThis = this.Dereference();
            var derefOther = other.Dereference();

            if (object.ReferenceEquals(derefThis, derefOther))
            {
                return true;
            }

            bool thisIsEmpty = derefThis.State == VarState.Empty;
            bool otherIsEmpty = derefOther.State == VarState.Empty;

            if (thisIsEmpty ^ otherIsEmpty)
            {
                if (derefThis.State == VarState.Undefined)
                {
                    derefThis.State = VarState.Empty;

                    Trail.Current?.Log(derefThis);

                    return true;
                }
                else if (derefOther.State == VarState.Undefined)
                {
                    derefOther.State = VarState.Empty;

                    Trail.Current?.Log(derefOther);

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (thisIsEmpty && otherIsEmpty)
            {
                return true;
            }

            if (derefThis.HasValue)
            {
                if (derefOther.HasValue)
                {
                    //TODO Implement general type unification

                    if (derefThis.value is IUnifiable<T> unifiable)
                    {
                        return unifiable.TryUnify(derefOther.Value);
                    }
                    else if (derefThis.value is ITuple tuple)
                    {
                        var otherTuple = (ITuple)derefOther.value;

                        bool allUnify = true;

                        for (int fieldIndex = 0; fieldIndex < tuple.Length; fieldIndex++)
                        {
                            var thisFieldValue = tuple[fieldIndex];
                            var otherFieldValue = otherTuple[fieldIndex];

                            if (thisFieldValue is Var fieldVar)
                            {
                                allUnify &= fieldVar.TryUnify((Var)otherFieldValue);
                            }
                            else
                            {
                                allUnify &= thisFieldValue.Equals(otherFieldValue);
                            }

                            if (!allUnify)
                            {
                                break;
                            }
                        }

                        return allUnify;
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
            else if (this.State == VarState.Empty)
            {
                return "[]";
            }
            else
            {
                return "#" + this.Dereference().index;
            }
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
        public static Query Deconstruct<T, U>(this Var<(Var<T>, Var<U>)> variable, out Var<T> first, out Var<U> second)
            => variable <= (Query.NewVar(out first), Query.NewVar(out second));

        public static Query Unify<T>(this Var<T> variable, T value)
        {
            return variable == null
                ? Query.Success
                : Query.When(() => variable.TryUnify(value));
        }

        public static Query Unify<T>(this Var<T> variable, Var<T> other)
        {
            return variable == null
                ? Query.Success
                : Query.When(() => variable.TryUnify(other));
        }

        public static Query IsVar<T>(this Var<T> variable)
        {
            return variable == null
                ? Query.Success
                : Query.When(() => !variable.HasValue);
        }

        public static Query IsNonVar<T>(this Var<T> variable)
        {
            return variable == null
                ? Query.Fail
                : Query.When(() => variable.HasValue);
        }
    }

    public enum VarState
    {
        Undefined,
        Empty,
        Reference,
        Value
    }
}
