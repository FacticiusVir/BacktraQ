using System;
using System.Linq;

namespace Keeper.BacktraQ
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Running");

            const int height = 5, width = 5;

            var grid = new Var<Direction>[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    grid[x, y] = new Var<Direction>();
                }
            }

            var rnd = new Random();

            for (int y = 0; y < height; y++)
            {
                for (int row = 0; row < 3; row++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var cell = grid[x, y];

                        DrawCell(row, cell);
                    }

                    Console.WriteLine();
                }
            }
            
            Console.WriteLine("Done");
            Console.ReadLine();
        }

        private static void DrawCell(int row, Var<Direction> cell)
        {
            if (cell.HasValue)
            {
                switch (row)
                {
                    case 0:
                        if (cell.Value == Direction.Up)
                        {
                            Console.Write("┌╨┐");
                        }
                        else
                        {
                            Console.Write("┌─┐");
                        }
                        break;
                    case 1:
                        if (cell.Value == Direction.Left)
                        {
                            Console.Write("╡ │");
                        }
                        else if (cell.Value == Direction.Right)
                        {
                            Console.Write("│ ╞");
                        }
                        else
                        {
                            Console.Write("│ │");
                        }
                        break;
                    case 2:
                        if (cell.Value == Direction.Down)
                        {
                            Console.Write("└╥┘");
                        }
                        else
                        {
                            Console.Write("└─┘");
                        }
                        break;
                }
            }
            else
            {
                Console.Write("###");
            }
        }

        private enum Direction
        {
            None,
            Up,
            Down,
            Left,
            Right
        }

        private static string Format<T>(Var<VarList<T>> listVariable)
        {
            if (listVariable.HasValue)
            {
                var currentItem = listVariable.Value;

                string result = "";

                while (!currentItem.IsEmptyList)
                {
                    result += Format(currentItem.Head);

                    if (currentItem.Tail.HasValue)
                    {
                        currentItem = currentItem.Tail.Value;

                        if (!currentItem.IsEmptyList)
                        {
                            result += ", ";
                        }
                    }
                    else
                    {
                        result += "| " + currentItem.Tail.ToString();
                        currentItem = VarList<T>.EmptyList;
                    }
                }

                return $"[{result}]";
            }
            else
            {
                return listVariable.ToString();
            }
        }

        private static string Format<T>(Var<T> variable)
        {
            //return variable.HasValue
            //    ? variable.Value.ToString()
            //    : "?";

            return variable.ToString();
        }
    }
}
