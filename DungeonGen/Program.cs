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
            
            var query = PlaceFirstCell(grid, initialcoord)
                            & Query.Chain((oldList, newList) =>
                                    {
                                        var coord = new Var<Coord>();

                                        return PlaceConnectedCell(grid, coord, oldList)
                                                    & oldList.Append(coord, newList);
                                    }, 40, VarList.Create(initialcoord));
            
            if (query.Succeeds())
            {
                DrawGrid(grid);
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

            return placedCoord <= placedList.RandomMember
                        & direction <= DirectionList.RandomMember
                        & Offset(placedCoord, direction, coord)
                        & IsInBounds(coord)
                        & HasFreeSides(grid, coord, 3)
                        & !IsCorridor(grid, coord, direction, 3)
                        & GetCell(grid, coord, cell)
                        & cell.IsVar()
                        & cell <= direction;
        }

        private static Query IsCorridor(Var<Direction>[,] grid, Var<Coord> coord, Var<Direction> direction, Var<int> length)
        {
            return length <= 1
                | (() =>
                    {
                        var previousCoord = new Var<Coord>();
                        var previousLength = new Var<int>();

                        return Offset(previousCoord, direction, coord)
                                    & GetCell(grid, previousCoord, direction)
                                    & length <= previousLength.Inc
                                    & IsCorridor(grid, previousCoord, direction, previousLength);
                    });
        }

        private static Query HasFreeSides(Var<Direction>[,] grid, Var<Coord> coord, int requiredCount)
        {
            return Query.Create(() =>
            {
                var adjacent = new Var<Coord>();
                var cell = new Var<Direction>();

                int count = (Adjacent(coord, adjacent)
                                & ((GetCell(grid, adjacent, cell) & cell.IsVar()) | !IsInBounds(adjacent)))
                                .AsEnumerable(adjacent)
                                .Count();

                return count >= requiredCount;
            });
        }

        private static Query GetCell(Var<Direction>[,] grid, Var<Coord> coord, Var<Direction> cell)
        {
            return IsInBounds(coord) & (() => grid[coord.Value.X, coord.Value.Y] <= cell);
        }

        private static Query Adjacent(Var<Coord> coord, Var<Coord> adjacent)
        {
            var direction = new Var<Direction>();

            return direction <= DirectionList.Member
                        & Offset(coord, direction, adjacent);
        }

        private static Query Offset(Var<Coord> oldCoord, Var<Direction> direction, Var<Coord> newCoord)
        {
            return Query.Create(() =>
            {
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

                return Query.Map(oldCoord,
                                    newCoord,
                                    value => new Coord
                                    {
                                        X = value.X + xOffset,
                                        Y = value.Y + yOffset
                                    },
                                    value => new Coord
                                    {
                                        X = value.X - xOffset,
                                        Y = value.Y - yOffset
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

        private static void DrawGrid(Var<Direction>[,] grid)
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
