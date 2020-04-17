using System;
using static Keeper.BacktraQ.Query;

namespace Keeper.BacktraQ.Prolog
{
    class Program
    {
        static void Main(string[] args)
        {
            var value = new VarList<char>();
            var value2 = new VarList<char>();

            var parser = (Phrase)"(" + value + ", " + value2 + ")";

            parser.AsString("(test, abc)").Succeeds();

            Console.WriteLine(value.AsString());
            Console.WriteLine(value2.AsString());
        }
    }
}
