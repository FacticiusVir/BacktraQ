using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper.BacktraQ
{
    public static class VarList
    {
        public static Var<VarList<T>> Create<T>()
        {
            return VarList<T>.Create();
        }

        public static Var<VarList<char>> Create(string text)
        {
            return Create(text.ToCharArray());
        }

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

    public static class VarListExtensions
    {
        public static Query UnifyHead<T>(this IVar<VarList<T>> list, Var<T> head)
        {
            var listValue = VarList<T>.Create();

            return list.Unify(listValue)
                        & listValue.Head.Unify(head);
        }

        public static Query UnifyTail<T>(this IVar<VarList<T>> list, Var<VarList<T>> tail)
        {
            var listValue = VarList<T>.Create();

            return list.Unify(listValue)
                        & listValue.Tail.Unify(tail);
        }

        public static Query Unify<T>(this IVar<VarList<T>> list, Var<T> head, Var<VarList<T>> tail)
        {
            var listValue = VarList<T>.Create();

            return list.Unify(listValue)
                        & listValue.Head.Unify(head)
                        & listValue.Tail.Unify(tail);
        }


        public static Query Length<T>(this Var<VarList<T>> list, Var<int> length)
        {
            return list.Unify(VarList<T>.EmptyList).And(length.Unify(0))
                        .Or(() =>
                        {
                            var innerLength = new Var<int>();
                            var listValue = VarList<T>.Create();

                            return list.Unify(listValue)
                                    & Length(listValue.Tail, innerLength)
                                    & QMath.Inc(innerLength, length);
                        });
        }

        public static Query Member<T>(this IVar<VarList<T>> list, Var<T> element)
        {
            var tail = VarList.Create<T>();

            return list.UnifyHead(element)
                        | (list.UnifyTail(tail) & Query.Create(() => tail.Member(element)));
        }

        public static Query NonVarMember<T>(this Var<VarList<T>> list, Var<T> element)
        {
            return Query.Create(() =>
            {
                var tail = VarList.Create<T>();

                return Query.Any(list.UnifyHead(element) & element.NonVar(),
                                    list.UnifyTail(tail) & tail.NonVar());
            });
        }

        public static Query Append<T>(this Var<VarList<T>> initial, Var<T> item, Var<VarList<T>> result)
        {
            return initial.Append(VarList.Create(item), result);
        }

        public static Query Append<T>(this Var<VarList<T>> initial, Var<VarList<T>> other, Var<VarList<T>> result)
        {
            return initial.Unify(VarList<T>.EmptyList) & (other.Unify(result))
                         | Query.Create(() =>
                            {
                                var initialList = VarList<T>.Create();
                                var resultList = VarList<T>.Create();

                                return initial.Unify(initialList)
                                        & result.Unify(resultList)
                                        & initialList.Head.Unify(resultList.Head)
                                        & initialList.Tail.Append(other, resultList.Tail);
                            });
        }

        public static Query Nth<T>(this Var<VarList<T>> list, Var<int> index, Var<T> element)
        {
            var listValue = VarList<T>.Create();

            return list.Unify(listValue)
                        & ((listValue.Head.Unify(element)
                            & index.Unify(0))
                            | (() =>
                            {
                                var innerIndex = new Var<int>();

                                return listValue.Tail.Nth(innerIndex, element)
                                        & QMath.Inc(innerIndex, index);
                            })
                        );
        }

        public static Query RandomMember<T>(this Var<VarList<T>> list, Var<T> member)
        {
            var listLength = new Var<int>();
            var randomIndex = new Var<int>();

            return list.Length(listLength)
                    & Query.Random(listLength, randomIndex)
                    & list.Nth(randomIndex, member);
        }

        public static IEnumerable<T> AsEnumerable<T>(this IVar<VarList<T>> list)
        {
            var element = new Var<T>();

            return list.Member(element).AsEnumerable(element);
        }

        public static IEnumerable<Var<T>> AsVarEnumerable<T>(this IVar<VarList<T>> list)
        {
            if (!list.HasValue)
            {
                throw new InvalidOperationException();
            }
            else if (list.Unify(VarList<T>.EmptyList).Succeeds())
            {
                yield break;
            }
            else
            {
                var head = new Var<T>();
                var tail = new Var<VarList<T>>();

                list.Unify(head, tail);

                yield return head;

                foreach (var tailVar in tail.AsVarEnumerable())
                {
                    yield return tailVar;
                }
            }
        }

        public static string AsString(this Var<VarList<char>> charList)
        {
            var charVar = new Var<char>();

            return new string(charList.Member(charVar).AsEnumerable(charVar).ToArray());
        }

        public static Query AsString(this Var<VarList<char>> charList, Var<string> value)
        {
            return Query.Create(() =>
            {
                if (value.HasValue)
                {
                    return charList.Unify(VarList.Create(value.Value));
                }
                else
                {
                    var charVar = new Var<char>();

                    return value.Unify(new string(charList.Member(charVar).AsEnumerable(charVar).ToArray()));
                }
            });
        }
    }
}
