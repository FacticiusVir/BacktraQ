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

            var query = PlaceFirstCell(grid, initialcoord);

            var placedList = VarList<Coord>.Create(initialcoord);

            for (int index = 0; index < 9; index++)
            {
                var coord = new Var<Coord>();
                var newPlacedList = new Var<VarList<Coord>>();

                query = query.And(PlaceConnectedCell, grid, coord, placedList)
                                .And(placedList.Append, coord, newPlacedList);

                placedList = newPlacedList;
            }

            if (query.Succeeds())
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
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        private static Query PlaceFirstCell(Var<Direction>[,] grid, Var<Coord> coord)
        {
            var x = new Var<int>();
            var y = new Var<int>();

            return Query.Random(width, x)
                    .And(Query.Random, height, y)
                    .And(() => grid[x.Value, y.Value].Unify(Direction.None))
                    .And(() => coord.Unify(new Coord { X = x.Value, Y = y.Value }));
        }

        private static Query PlaceConnectedCell(Var<Direction>[,] grid, Var<Coord> coord, Var<VarList<Coord>> placedList)
        {
            var placedCoord = new Var<Coord>();
            var direction = new Var<Direction>();
            var directionList = VarList.Create(Direction.Up, Direction.Down, Direction.Left, Direction.Right);

            return RandomMember(placedList, placedCoord)
                .And(RandomMember, directionList, direction)
                .And(Offset, placedCoord, direction, coord)
                .And(IsInBounds, coord)
                .And(() => !grid[coord.Value.X, coord.Value.Y].HasValue)
                .And(() => grid[coord.Value.X, coord.Value.Y].Unify(direction));
        }
        
        private static Query RandomMember<T>(Var<VarList<T>> list, Var<T> member)
        {
            var listLength = new Var<int>();
            var randomIndex = new Var<int>();

            return list.Length(listLength)
                    .And(Query.Random, listLength, randomIndex)
                    .And(list.Nth, randomIndex, member);
        }

        private static Query Offset(Var<Coord> oldCoord, Var<Direction> direction, Var<Coord> newCoord)
        {
            return Query.Create(() =>
            {
                Coord value = oldCoord.Value;
                Direction dirValue = direction.Value;

                int xOffset = 0;
                int yOffset = 0;

                if (dirValue == Direction.Down)
                {
                    yOffset = -1;
                }
                else if (dirValue == Direction.Up)
                {
                    yOffset = 1;
                }
                else if (dirValue == Direction.Right)
                {
                    xOffset = -1;
                }
                else if (dirValue == Direction.Left)
                {
                    xOffset = 1;
                }

                return newCoord.TryUnify(new Coord
                {
                    X = value.X + xOffset,
                    Y = value.Y + yOffset
                });
            });
        }

        private static Query IsInBounds(Var<Coord> coord)
        {
            return Query.Create(() => coord.HasValue
                                        && coord.Value.X >= 0
                                        && coord.Value.Y >= 0
                                        && coord.Value.X < width
                                        && coord.Value.Y < height);
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
                        else if (cell.Value == Direction.None)
                        {
                            Console.Write("│@│");
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
