using System;
using System.Linq;

namespace Keeper.BacktraQ
{
    class Program
    {
        private const int height = 5, width = 5;

        static void Main(string[] args)
        {
            Console.WriteLine("Running");

            var grid = new Var<Direction>[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    grid[x, y] = new Var<Direction>();
                }
            }

            var initialcoord = new Var<Coord>();

            var placedList = VarList<Coord>.Create(initialcoord);

            var query = Query.Create(() =>
            {
                return PlaceCell(grid, initialcoord);
            });

            for (int index = 0; index < 10; index++)
            {
                var coord = new Var<Coord>();
                var newPlacedList = new Var<VarList<Coord>>();
                var intermediaryList = VarList.Create(coord);

                query = query.And(PlaceCell, grid, coord, placedList)
                            .And(placedList.Append, intermediaryList, newPlacedList);

                placedList = newPlacedList;
            }

            foreach (var result in query.AsEnumerable())
            {
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

                Console.ReadLine();
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        private static Query PlaceCell(Var<Direction>[,] grid, Var<Coord> coord)
        {
            var x = new Var<int>();
            var y = new Var<int>();

            return Query.Random(width, x)
                    .And(Query.Random, height, y)
                    .And(() => grid[x.Value, y.Value].Unify(Direction.None))
                    .And(() => coord.Unify(new Coord { X = x.Value, Y = y.Value }));
        }

        private static Query PlaceCell(Var<Direction>[,] grid, Var<Coord> coord, Var<VarList<Coord>> placedList)
        {
            var placedLength = new Var<int>();
            var placedIndex = new Var<int>();
            var placedCoord = new Var<Coord>();
            var directionIndex = new Var<int>();
            var direction = new Var<Direction>();

            return placedList.Length(placedLength)
                .And(Query.Random, placedLength, placedIndex)
                .And(placedList.Nth, placedIndex, placedCoord)
                .And(Query.Random, 4, directionIndex)
                .And(() => direction.Unify((Direction)directionIndex.Value + 1))
                .And(() => coord.Unify(Offset(placedCoord.Value, direction.Value)))
                .And(() => Query.Create(() => IsInBounds(coord.Value)))
                .And(() => Query.Create(() => !grid[coord.Value.X, coord.Value.Y].HasValue))
                .And(() => grid[coord.Value.X, coord.Value.Y].Unify(direction));
        }

        private static Coord Offset(Coord value, Direction direction)
        {
            int xOffset = 0;
            int yOffset = 0;

            if(direction == Direction.Down)
            {
                yOffset = -1;
            }
            else if (direction == Direction.Up)
            {
                yOffset = 1;
            }
            else if (direction == Direction.Right)
            {
                xOffset = -1;
            }
            else if (direction == Direction.Left)
            {
                xOffset = 1;
            }

            return new Coord
            {
                X = value.X + xOffset,
                Y = value.Y + yOffset
            };
        }

        private static bool IsInBounds(Coord value)
        {
            return value.X >= 0
                && value.Y >= 0
                && value.X < width
                && value.Y < height;
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

        private struct Coord
        {
            public int X;
            public int Y;
        }
    }
}
