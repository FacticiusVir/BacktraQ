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

            queryStack.Push(QueryPipeline.Create(enumQuery, new TupleInjectorQuery(enumQuery)));

            while (queryStack.Any())
            {
                var query = queryStack.Pop();

                var result = query.Run();

                if (result.Type == QueryResultType.ChoicePoint)
                {
                    queryStack.Push(result.Alternate);
                    queryStack.Push(result.Continuation);
                    Console.WriteLine("ChoicePoint");
                }
                else if (result.Type == QueryResultType.Success)
                {
                    Console.WriteLine(result.Value);
                }
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
