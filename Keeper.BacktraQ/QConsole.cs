using System;
using static Keeper.BacktraQ.Query;

namespace Keeper.BacktraQ
{
    public static class QConsole
    {
        public static Query PrintLine(string value) => Do(() => Console.WriteLine(value));

        public static Query PrintLine(object value) => Do(() => Console.WriteLine(value));
    }
}
