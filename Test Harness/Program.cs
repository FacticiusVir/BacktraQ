using System;

namespace Keeper.LSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Running");

            var ints = new[] { 1, 2, 3, 4 };

            var x = new Var<int>();
            var y = new Var<int>();

            foreach (var result in EnumerableQuery.Create(ints, x)
                                    .And(EnumerableQuery.Create, ints, y)
                                    .And(Query.Not, x.Unify(y))
                                    .AsEnumerable())
            {
                Console.WriteLine($"X = {Format(x)}, Y = {Format(y)}");
            }

            //Console.WriteLine($"X = {Format(x)}, Y = {Format(y)}");

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        private static string Format<T>(Var<T> variable)
        {
            return variable.HasValue
                ? variable.Value.ToString()
                : "?";
        }
    }
}
