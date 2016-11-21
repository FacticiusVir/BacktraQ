using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper.BacktraQ
{
    public struct VarList<T>
        : IUnifiable<VarList<T>>
    {
        private VarList(Var<T> head, Var<VarList<T>> tail)
        {
            this.Head = head;
            this.Tail = tail;
        }

        public Var<T> Head
        {
            get;
            private set;
        }

        public Var<VarList<T>> Tail
        {
            get;
            private set;
        }

        public bool IsEmptyList
        {
            get
            {
                return this.Head == null || this.Tail == null;
            }
        }

        public static VarList<T> Create()
        {
            return new VarList<T>(new Var<T>(), new Var<VarList<T>>());
        }

        public static VarList<T> Create(Var<T> head, VarList<T> tail)
        {
            return new VarList<T>(head, tail);
        }

        public static Var<VarList<T>> Create(params T[] items)
        {
            return Create((IEnumerable<T>)items);
        }

        public static Var<VarList<T>> Create(IEnumerable<T> items, bool knownLength = true)
        {
            if (items.Any())
            {
                var tail = Create(items.Skip(1), knownLength);

                return new VarList<T>(items.First(), tail);
            }
            else
            {
                return knownLength
                        ? EmptyList
                        : new Var<VarList<T>>();
            }
        }

        public static VarList<T> EmptyList
        {
            get
            {
                return new VarList<T>();
            }
        }

        public bool TryUnify(VarList<T> other)
        {
            if (this.IsEmptyList || other.IsEmptyList)
            {
                return this.IsEmptyList && other.IsEmptyList;
            }
            else
            {
                return this.Head.TryUnify(other.Head) && this.Tail.TryUnify(other.Tail);
            }
        }
    }

    public static class VarExtensions
    {
        public static Query Length<T>(this Var<VarList<T>> list, Var<int> length)
        {
            return list.Unify(VarList<T>.EmptyList).And(length.Unify, 0)
                        .Or(() =>
                        {
                            var innerLength = new Var<int>();
                            var listValue = VarList<T>.Create();

                            return list.Unify(listValue)
                                    .And(Length, listValue.Tail, innerLength)
                                    .And(QMath.Inc, innerLength, length);
                        });
        }

        public static Query Member<T>(this Var<VarList<T>> list, Var<T> element)
        {
            return Query.Create(() =>
            {
                var listValue = VarList<T>.Create();

                return list.Unify(listValue)
                            .And(() =>
                                element.Unify(listValue.Head)
                                    .Or(Member, listValue.Tail, element));
            });
        }
    }
}
