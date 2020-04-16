using System;
using System.Collections.Generic;
using System.Linq;

using static Keeper.BacktraQ.Query;

namespace Keeper.BacktraQ
{
    public static class VarList
    {
        public static VarList<char> Create(string text)
        {
            return Create(text.ToCharArray());
        }

        public static VarList<T> Create<T>(params T[] items)
        {
            return Create(items.Select(x => (Var<T>)x));
        }

        public static VarList<T> Create<T>(params Var<T>[] items)
        {
            return Create(items);
        }

        public static VarList<T> Create<T>(IEnumerable<T> items, bool knownLength = true)
        {
            if (items.Any())
            {
                return new VarList<T>(items.First(), Create(items.Skip(1), knownLength));
            }
            else
            {
                return knownLength
                        ? VarList<T>.Empty
                        : new VarList<T>();
            }
        }

        public static VarList<T> Create<T>(IEnumerable<Var<T>> items, bool knownLength = true)
        {
            if (items.Any())
            {
                return new VarList<T>(items.First(), Create(items.Skip(1), knownLength));
            }
            else
            {
                return knownLength
                        ? VarList<T>.Empty
                        : new VarList<T>();
            }
        }

        public static VarList<T> Create<T>(int length)
        {
            if (length <= 0)
            {
                return VarList<T>.Empty;
            }
            else
            {
                return new VarList<T>(new Var<T>(), Create<T>(length - 1));
            }
        }
    }

    public class VarList<T>
        : Var<(Var<T> Head, VarList<T> Tail)>
    {
        public VarList()
        {
        }

        public VarList(Var<T> head, VarList<T> tail)
            : base((head, tail))
        {
        }

        private VarList(bool isEmpty)
            : base(isEmpty)
        {
        }

        public Query Head(Var<T> head) => this <= (head, new VarList<T>());

        public Query Tail(VarList<T> tail) => this <= (new Var<T>(), tail);

        public new static VarList<T> Empty => new VarList<T>(true);

        public static Query operator <=(VarList<T> variable, Func<VarList<T>, Query> bind) => bind(variable);

        public static Query operator >=(VarList<T> variable, Func<VarList<T>, Query> bind) => bind(variable);
    }

    public static class VarListExtensions
    {
        public static Query Length<T>(this VarList<T> list, Var<int> length)
        {
            return (list <= VarList<T>.Empty & length <= 0)
                        | (() =>
                        {
                            var tail = new VarList<T>();
                            var tailLength = new Var<int>();

                            return tail <= list.Tail
                                    & tailLength <= tail.Length
                                    & length <= tailLength.Inc;
                        });
        }

        public static Query Member<T>(this VarList<T> list, Var<T> element)
        {
            var tail = new VarList<T>();

            return list.Head(element)
                        | (list.Tail(tail) & (() => tail.Member(element)));
        }

        public static Query NonVarMember<T>(this VarList<T> list, Var<T> element)
            => Wrap(() =>
            {
                var tail = new VarList<T>();

                return (element <= list.Head & element.IsNonVar())
                    | (tail <= list.Tail & tail.NonVarMember(element));
            });

        public static Func<VarList<T>, Query> Append<T>(this VarList<T> initial, Var<T> item) => result => initial.Append(item, result);

        public static Query Append<T>(this VarList<T> initial, Var<T> item, VarList<T> result)
        {
            return initial.Append(VarList.Create(item), result);
        }

        public static Query Append<T>(this VarList<T> initial, VarList<T> other, VarList<T> result)
        {
            return (initial <= VarList<T>.Empty & other <= result)
                         | (() =>
                            {
                                var head = new Var<T>();
                                var initialTail = new VarList<T>();
                                var resultTail = new VarList<T>();

                                return initial <= (head, initialTail)
                                        & result <= (head, resultTail)
                                        & initialTail.Append(other, resultTail);
                            });
        }

        public static Query Nth<T>(this VarList<T> list, Var<int> index, Var<T> element)
        {
            var head = new Var<T>();
            var tail = new VarList<T>();

            return list <= (head, tail)
                        & ((head <= element & index <= 0)
                            | (() =>
                                {
                                    var innerIndex = new Var<int>();

                                    return tail.Nth(innerIndex, element)
                                            & index <= innerIndex.Inc;
                                })
                        );
        }

        public static Query RandomMember<T>(this VarList<T> list, out Var<T> member) => RandomMember(list, NewVar(out member));

        public static Query RandomMember<T>(this VarList<T> list, Var<T> member)
        {
            var listLength = new Var<int>();
            var randomIndex = new Var<int>();

            return list.Length(listLength)
                    & Query.Random(listLength, randomIndex)
                    & list.Nth(randomIndex, member);
        }

        public static IEnumerable<T> AsEnumerable<T>(this VarList<T> list)
        {
            var element = new Var<T>();

            return list.Member(element).AsEnumerable(element);
        }

        public static string AsString(this VarList<char> charList)
        {
            var charVar = new Var<char>();

            return new string(charList.Member(charVar).AsEnumerable(charVar).ToArray());
        }

        public static Query AsString(this VarList<char> charList, Var<string> value)
        {
            return Query.Wrap(() =>
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
