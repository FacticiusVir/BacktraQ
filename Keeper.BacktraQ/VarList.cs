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
        public static Query UnifyHead<T>(this Var<VarList<T>> list, Var<T> head)
        {
            var listValue = VarList<T>.Create();

            return list <= listValue & listValue.Head <= head;
        }

        public static Query UnifyTail<T>(this Var<VarList<T>> list, Var<VarList<T>> tail)
        {
            var listValue = VarList<T>.Create();

            return list <= listValue & listValue.Tail <= tail;
        }

        public static Query Unify<T>(this Var<VarList<T>> list, Var<T> head, Var<VarList<T>> tail)
        {
            var listValue = VarList<T>.Create();

            return list <= listValue
                        & listValue.Head <= head
                        & listValue.Tail <= tail;
        }


        public static Query Length<T>(this Var<VarList<T>> list, Var<int> length)
        {
            return (list <= VarList<T>.EmptyList & length <= 0)
                        | (() =>
                        {
                            var tail = new Var<VarList<T>>();
                            var tailLength = new Var<int>();

                            return list.UnifyTail(tail)
                                    & tail.Length(tailLength)
                                    & QMath.Inc(tailLength, length);
                        });
        }

        public static Query Member<T>(this Var<VarList<T>> list, Var<T> element)
        {
            var tail = new Var<VarList<T>>();

            return list.UnifyHead(element)
                        | (list.UnifyTail(tail) & (() => tail.Member(element)));
        }

        public static Query NonVarMember<T>(this Var<VarList<T>> list, Var<T> element)
        {
            return Query.Create(() =>
            {
                var tail = VarList.Create<T>();

                return Query.Any(list.UnifyHead(element) & element.IsNonVar(),
                                    list.UnifyTail(tail) & tail.IsNonVar());
            });
        }

        public static Query Append<T>(this Var<VarList<T>> initial, Var<T> item, Var<VarList<T>> result)
        {
            return initial.Append(VarList.Create(item), result);
        }

        public static Query Append<T>(this Var<VarList<T>> initial, Var<VarList<T>> other, Var<VarList<T>> result)
        {
            return (initial <= VarList<T>.EmptyList & other <= result)
                         | (() =>
                            {
                                var head = new Var<T>();
                                var initialTail = new Var<VarList<T>>();
                                var resultTail = new Var<VarList<T>>();

                                return initial.Unify(head, initialTail)
                                        & result.Unify(head, resultTail)
                                        & initialTail.Append(other, resultTail);
                            });
        }

        public static Query Nth<T>(this Var<VarList<T>> list, Var<int> index, Var<T> element)
        {
            var head = new Var<T>();
            var tail = new Var<VarList<T>>();

            return list.Unify(head, tail)
                        & ((head <= element & index <= 0)
                            | (() =>
                                {
                                    var innerIndex = new Var<int>();

                                    return tail.Nth(innerIndex, element)
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

        public static IEnumerable<T> AsEnumerable<T>(this Var<VarList<T>> list)
        {
            var element = new Var<T>();

            return list.Member(element).AsEnumerable(element);
        }

        public static IEnumerable<Var<T>> AsVarEnumerable<T>(this Var<VarList<T>> list)
        {
            if (!list.HasValue)
            {
                throw new InvalidOperationException();
            }
            else if ((list <= VarList<T>.EmptyList).Succeeds())
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
                    return charList <= VarList.Create(value.Value);
                }
                else
                {
                    var charVar = new Var<char>();

                    return value <= new string(charList.Member(charVar).AsEnumerable(charVar).ToArray());
                }
            });
        }
    }
}
