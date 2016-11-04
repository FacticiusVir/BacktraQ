using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper.LSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var queryStack = new Stack<Query<Tuple<int, int>>>();

            var enumQuery = EnumerableQuery.Create(new[] { 1, 2, 3, 4 });

            var tuplePipeline = QueryPipeline.Create(enumQuery, new TupleQuery(enumQuery));

            queryStack.Push(QueryPipeline.Create(tuplePipeline, new FilterQuery()));

            while (queryStack.Any())
            {
                var query = queryStack.Pop();

                var result = query.Run();

                switch (result.Type)
                {
                    case QueryResultType.Fail:
                        Console.WriteLine($"Fail / {queryStack.Count}");
                        break;
                    case QueryResultType.ChoicePoint:
                        queryStack.Push(result.Alternate);
                        queryStack.Push(result.Continuation);
                        Console.WriteLine($"ChoicePoint / {queryStack.Count}");
                        break;
                    case QueryResultType.Success:
                        Console.WriteLine($"{result.Value} / {queryStack.Count}");
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
