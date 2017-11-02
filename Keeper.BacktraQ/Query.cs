using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Keeper.BacktraQ
{
    public abstract class Query
        : IEnumerable
    {
        protected internal abstract QueryResult Run();

        public static Query Create(Action action, Action rollback = null)
        {
            return new ActionQuery(action, rollback);
        }

        public static Var<T> NewVar<T>(out Var<T> variable)
        {
            return variable = new Var<T>();
        }

        public static Var<VarList<T>> NewList<T>(out Var<VarList<T>> list)
        {
            return list = new Var<VarList<T>>();
        }

        public Query FindAll<T>(Var<T> variable, Var<VarList<T>> results)
        {
            return Query.Create(() =>
            {
                return results.Unify(this.AsVarList(variable));
            });
        }

        public static Query Create(Func<bool> predicate)
        {
            return new TestQuery(predicate);
        }

        public static Query IfThen(Query condition, Query action)
        {
            return IfThen(condition, action, Query.Success);
        }

        public static Query IfThen(Query condition, Query action, Query elseAction)
        {
            return Query.Create(() =>
            {
                if (condition.Succeeds(true))
                {
                    return action;
                }
                else
                {
                    return elseAction;
                }
            });
        }

        public static Query All(params Query[] goals)
        {
            return All((IEnumerable<Query>)goals);
        }

        public static Query All(IEnumerable<Query> goals)
        {
            var result = goals.First();

            foreach (var goal in goals.Skip(1))
            {
                result = result.And(goal);
            }

            return result;
        }


        public static Query Any(params Query[] options)
        {
            return Any((IEnumerable<Query>)options);
        }

        public static Query Any(IEnumerable<Query> options)
        {
            var result = options.First();

            foreach (var alternative in options.Skip(1))
            {
                result = result.Or(alternative);
            }

            return result;
        }

        public static Query Chain<T>(Func<Var<VarList<T>>, Var<T>, Query> subQuery, int repetitions, Var<VarList<T>> input, Var<VarList<T>> output = null)
        {
            return Chain((oldList, newList) =>
            {
                var newItem = new Var<T>();

                return subQuery(oldList, newItem)
                            & oldList.Append(newItem, newList);
            }, repetitions, input, output);
        }

        public static Query Chain<T>(Func<Var<T>, Var<T>, Query> subQuery, int repetitions, Var<T> input, Var<T> output = null)
        {
            if (repetitions == 0)
            {
                return Query.Success;
            }

            output = output ?? new Var<T>();

            var intermediary = new Var<T>();

            var queryAccumulator = subQuery(input, intermediary);

            for (int index = 1; index < repetitions; index++)
            {
                input = intermediary;
                intermediary = new Var<T>();

                queryAccumulator = queryAccumulator & subQuery(input, intermediary);
            }

            return intermediary <= output & queryAccumulator;
        }

        public static Query Map<T, V>(Var<T> left, Var<V> right, Func<T, V> map, Func<V, T> unmap = null)
        {
            return Query.Create(() =>
            {
                if (left.HasValue)
                {
                    return right <= map(left.Value);
                }
                else if (right.HasValue && unmap != null)
                {
                    return left <= unmap(right.Value);
                }
                else
                {
                    throw new Exception("Insufficiently instantiated terms.");
                }
            });
        }

        public static Query Map<T, V, W>(Var<T> left, Var<V> right, Var<W> result, Func<T, V, W> map, Func<V, W, T> unmapLeft, Func<T, W, V> unmapRight)
        {
            return Query.Create(() =>
            {
                if (left.HasValue & right.HasValue)
                {
                    return result <= map(left.Value, right.Value);
                }
                else if (right.HasValue && result.HasValue)
                {
                    return left <= unmapLeft(right.Value, result.Value);
                }
                else if (left.HasValue && result.HasValue)
                {
                    return right <= unmapRight(left.Value, result.Value);
                }
                else
                {
                    throw new Exception("Insufficiently instantiated terms.");
                }
            });
        }

        public static Query Map<T, V, W>(out Var<T> t, out Var<V> v, Var<W> w, params (T, V, W)[] mappings) => Map(NewVar(out t), NewVar(out v), w, mappings);

        public static Query Map<T, V, W>(Var<T> t, Var<V> v, Var<W> w, params (T, V, W)[] mappings)
        {
            return Query.Any(mappings.Select(mapping =>
            {
                return t <= mapping.Item1
                        & v <= mapping.Item2
                        & w <= mapping.Item3;
            }));
        }

        public static Query Construct<T, V, W>(out Var<T> left, out Var<V> right, Var<W> result, Func<T, V, W> construct, Func<W, Tuple<T, V>> deconstruct = null) => Construct(NewVar(out left), NewVar(out right), result, construct, deconstruct);

        public static Query Construct<T, V, W>(Var<T> left, Var<V> right, Var<W> result, Func<T, V, W> construct, Func<W, Tuple<T, V>> deconstruct = null)
        {
            return Query.Create(() =>
            {
                if (left.HasValue & right.HasValue)
                {
                    return result <= construct(left.Value, right.Value);
                }
                else if (result.HasValue && deconstruct != null)
                {
                    var values = deconstruct(result.Value);

                    return left <= values.Item1
                            & right <= values.Item2;
                }
                else
                {
                    throw new Exception("Insufficiently instantiated terms.");
                }
            });
        }

        public static Query Create(Func<Query> query)
        {
            return new PassthroughQuery(query);
        }

        public static Query Success
        {
            get
            {
                return new TestQuery(() => true);
            }
        }

        public static Query Fail
        {
            get
            {
                return new TestQuery(() => false);
            }
        }

        public static Query Random(out Var<int> bound, Var<int> value) =>  Random(NewVar(out bound), value);

        public static Query Random(Var<int> bound, out Var<int> value) =>  Random(bound, NewVar(out value));
        
        public static Query Random(out Var<int> bound, out Var<int> value) => Random(NewVar(out bound), NewVar(out value));

        public static Query Random(Var<int> bound, Var<int> value)
        {
            return Create(() => bound.HasValue)
                .And(() =>
                    {
                        var sequence = Enumerable.Range(0, bound.Value).ToList();

                        Shuffle(sequence);

                        return EnumerableQuery.Create(sequence, value);
                    });
        }

        public static Query Not(Query query)
        {
            return new TestQuery(() => !query.Succeeds(true));
        }

        private static Random rnd = new Random();

        public static void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.AsEnumerable().GetEnumerator();
        }

        public static Query operator &(Query left, Query right)
        {
            return left.And(right);
        }

        public static Query operator &(Query left, Func<Query> right)
        {
            return left.And(right);
        }

        public static Query operator |(Query left, Query right)
        {
            return left.Or(right);
        }

        public static Query operator |(Query left, Func<Query> right)
        {
            return left.Or(right);
        }

        public static Query operator !(Query query)
        {
            return Not(query);
        }
    }

    public static class QueryExtensions
    {
        public static bool Succeeds(this Query query, bool revertAll = false)
        {
            foreach (var result in query.AsEnumerable(revertAll))
            {
                return true;
            }

            return false;
        }

        public static Var<VarList<T>> AsVarList<T>(this Query query, Var<T> resultVariable)
        {
            var resultList = new List<Var<T>>();

            foreach (var result in query)
            {
                if (resultVariable.HasValue)
                {
                    resultList.Add(resultVariable.Value);
                }
                else if (resultVariable.IsBound)
                {
                    resultList.Add(resultVariable.Dereference());
                }
                else
                {
                    resultList.Add(new Var<T>());
                }
            }

            return VarList.Create(resultList.ToArray());
        }

        public static IEnumerable<T> AsEnumerable<T>(this Query query, Var<T> resultVariable)
        {
            foreach (var result in query)
            {
                yield return resultVariable.Value;
            }
        }

        public static IEnumerable AsEnumerable(this Query query, bool revertAll = false)
        {
            try
            {
                Trail.Enter();

                Query nextQuery = query;

                while (nextQuery != null)
                {
                    var currentQuery = nextQuery;
                    var result = currentQuery.Run();

                    switch (result.Type)
                    {
                        case QueryResultType.ChoicePoint:
                            nextQuery = result.Continuation;
                            Trail.Current.ChoicePoint(result.Alternate);
                            break;
                        case QueryResultType.Success:
                            yield return null;
                            nextQuery = Trail.Current.Backtrack();
                            break;
                        default:
                            nextQuery = Trail.Current.Backtrack();
                            break;
                    }
                }
            }
            finally
            {
                Trail.Exit(revertAll);
            }
        }
    }
}
