using Keeper.BacktraQ;
using System;
using System.Linq;

namespace DungeonGen
{
    class Program
    {
        private const int height = 10, width = 10;

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

            var placedList = VarList.Create(initialcoord);

            for (int index = 0; index < 20; index++)
            {
                var coord = new Var<Coord>();
                var newPlacedList = new Var<VarList<Coord>>();

                query = query & PlaceConnectedCell(grid, coord, placedList)
                                & placedList.Append(coord, newPlacedList);

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

        private static Var<VarList<Direction>> DirectionList = VarList.Create(Direction.Up, Direction.Down, Direction.Left, Direction.Right);

        private static Query PlaceFirstCell(Var<Direction>[,] grid, Var<Coord> coord)
        {
            var x = new Var<int>();
            var y = new Var<int>();

            return Query.Random(width, x)
                    & Query.Random(height, y)
                    & Coord.Construct(x, y, coord)
                    & GetCell(grid, coord, Direction.None);
        }

        private static Query PlaceConnectedCell(Var<Direction>[,] grid, Var<Coord> coord, Var<VarList<Coord>> placedList)
        {
            var placedCoord = new Var<Coord>();
            var direction = new Var<Direction>();
            var cell = new Var<Direction>();

            return placedList.RandomMember(placedCoord)
                                & DirectionList.RandomMember(direction)
                                & Offset(placedCoord, direction, coord)
                                & IsInBounds(coord)
                                & HasFreeSides(grid, coord)
                                & GetCell(grid, coord, cell)
                                & cell.Var()
                                & cell.Unify(direction);
        }

        private static Query HasFreeSides(Var<Direction>[,] grid, Var<Coord> coord)
        {
            return Query.Create(() =>
            {
                var adjacent = new Var<Coord>();
                var cell = new Var<Direction>();

                int count = (Adjacent(coord, adjacent)
                                & ((GetCell(grid, adjacent, cell) & cell.Var()) | !IsInBounds(adjacent)))
                                .AsEnumerable(adjacent)
                                .Count();

                return count >= 3;
            });
        }

        private static Query GetCell(Var<Direction>[,] grid, Var<Coord> coord, Var<Direction> cell)
        {
            return IsInBounds(coord) & (() => grid[coord.Value.X, coord.Value.Y].Unify(cell));
        }

        private static Query Adjacent(Var<Coord> coord, Var<Coord> adjacent)
        {
            var direction = new Var<Direction>();

            return DirectionList.Member(direction)
                        & Offset(coord, direction, adjacent);
        }

        private static Query Offset(Var<Coord> oldCoord, Var<Direction> direction, Var<Coord> newCoord)
        {
            return Query.Create(() =>
            {
                Coord value = oldCoord.Value;
                Direction dirValue = direction.Value;

                int xOffset = 0;
                int yOffset = 0;

                switch (dirValue)
                {
                    case Direction.Down:
                        yOffset = -1;
                        break;
                    case Direction.Up:
                        yOffset = 1;
                        break;
                    case Direction.Right:
                        xOffset = -1;
                        break;
                    case Direction.Left:
                        xOffset = 1;
                        break;
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
                    result += currentItem.Head.ToString();

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

        private struct Coord
        {
            public int X;
            public int Y;

            public static Query Construct(Var<int> x, Var<int> y, Var<Coord> coord)
            {
                return Query.Construct(x,
                                        y,
                                        coord,
                                        (xValue, yValue) => new Coord { X = xValue, Y = yValue },
                                        coordValue => Tuple.Create(coordValue.X, coordValue.Y));
            }

            public override string ToString()
            {
                return $"X: {X}, Y: {Y}";
            }
        }
    }
}
