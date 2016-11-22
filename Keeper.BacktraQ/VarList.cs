using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper.BacktraQ
{
    public static class VarList
    {
        public static Var<VarList<T>> Create<T>(params T[] items)
        {
            return VarList<T>.Create(items.Select(x => (Var<T>)x));
        }

        public static Var<VarList<T>> Create<T>(params Var<T>[] items)
        {
            return VarList<T>.Create(items);
        }
    }

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

        public static VarList<T> Create(Var<T> head, Var<VarList<T>> tail)
        {
            return new VarList<T>(head, tail);
        }

        public static Var<VarList<T>> Create(params Var<T>[] items)
        {
            return Create((IEnumerable<Var<T>>)items);
        }

        public static Var<VarList<T>> Create(IEnumerable<Var<T>> items, bool knownLength = true)
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

        public static Query Append<T>(this Var<VarList<T>> initial, Var<VarList<T>> other, Var<VarList<T>> result)
        {
            return initial.Unify(VarList<T>.EmptyList).And(other.Unify, result)
                        .Or(() =>
                            {
                                var initialList = VarList<T>.Create();
                                var resultList = VarList<T>.Create();

                                return initial.Unify(initialList)
                                        .And(result.Unify, resultList)
                                        .And(initialList.Head.Unify, resultList.Head)
                                        .And(Append, initialList.Tail, other, resultList.Tail);
                            });
        }

        public static Query Nth<T>(this Var<VarList<T>> list, Var<int> index, Var<T> element)
        {
            return Query.Create(() =>
            {
                var listValue = VarList<T>.Create();

                return list.Unify(listValue)
                            .And(() =>
                                listValue.Head.Unify(element)
                                    .And(index.Unify, 0)
                                    .Or(() =>
                                    {
                                        var innerIndex = new Var<int>();

                                        return listValue.Tail.Nth(innerIndex, element)
                                                .And(QMath.Inc, innerIndex, index);
                                    })
                            );
            });
        }
    }
}
