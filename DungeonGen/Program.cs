using Keeper.BacktraQ;
using System;
using System.Diagnostics;
using System.Linq;

namespace DungeonGen
{
    class Program
    {
        private const int height = 10, width = 10;

        private const int roomCount = 40;

        static void Main(string[] args)
        {
            Console.WriteLine("Running");

            var grid = new VarGrid<Direction>(width, height);

            var initialcoord = new Var<Coord>();

            var query = PlaceFirstCell(grid, initialcoord)
                            & PlaceConnectedCells(grid, initialcoord)
                            & DrawGrid(grid);

            long startTimestamp = Stopwatch.GetTimestamp();

            if (!query.Succeeds())
            {
                Console.WriteLine("Could not generate dungeon grid.");
            }

            double duration = (Stopwatch.GetTimestamp() - startTimestamp) / (double)Stopwatch.Frequency;

            Console.WriteLine($"Query took {duration:0.0##} seconds.");

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        private static Query PlaceConnectedCells(VarGrid<Direction> grid, Var<Coord> initialcoord)
        {
            return Query.Chain((oldList, newCoord) => PlaceConnectedCell(grid, newCoord, oldList), roomCount - 1, VarList.Create(initialcoord));
        }

        private static Var<VarList<Direction>> DirectionList = VarList.Create(Direction.Up, Direction.Down, Direction.Left, Direction.Right);

        private static Query PlaceFirstCell(VarGrid<Direction> grid, Var<Coord> coord)
        {
            var x = new Var<int>();
            var y = new Var<int>();

            return Query.Random(width, x)
                    & Query.Random(height, y)
                    & Coord.Construct(x, y, coord)
                    & GetCell(grid, coord, Direction.Entrance);
        }

        private static Query PlaceConnectedCell(VarGrid<Direction> grid, Var<Coord> coord, Var<VarList<Coord>> placedList)
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

        private static Query IsCorridor(VarGrid<Direction> grid, Var<Coord> coord, Var<Direction> direction, Var<int> length)
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

        private static Query HasFreeSides(VarGrid<Direction> grid, Var<Coord> coord, int requiredCount)
        {
            var adjacent = new Var<Coord>();
            var cell = new Var<Direction>();

            var adjacencyQuery = Adjacent(coord, adjacent)
                                    & (
                                        (GetCell(grid, adjacent, cell) & cell.IsVar())
                                        | !IsInBounds(adjacent)
                                    );

            var results = new Var<VarList<Coord>>();
            var count = new Var<int>();

            return adjacencyQuery.FindAll(adjacent, results)
                            & count <= results.Length
                            & count.GreaterThanOrEqual(requiredCount);
        }

        private static Query GetCell(VarGrid<Direction> grid, Var<Coord> coord, Var<Direction> cell)
        {
            var x = new Var<int>();
            var y = new Var<int>();

            return IsInBounds(coord)
                        & Coord.Construct(x, y, coord)
                        & grid.XYth(x, y, cell);
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
            var x = new Var<int>();
            var y = new Var<int>();

            return Coord.Construct(x, y, coord)
                        & x.Between(0, width - 1)
                        & y.Between(0, height - 1);
        }

        private static void DrawCell(int row, Direction cell)
        {
            if (cell == Direction.None)
            {
                Console.Write("###");
            }
            else
            {
                switch (row)
                {
                    case 0:
                        if (cell == Direction.Up)
                        {
                            Console.Write("┌╨┐");
                        }
                        else
                        {
                            Console.Write("┌─┐");
                        }
                        break;
                    case 1:
                        if (cell == Direction.Left)
                        {
                            Console.Write("╡ │");
                        }
                        else if (cell == Direction.Right)
                        {
                            Console.Write("│ ╞");
                        }
                        else if (cell == Direction.Entrance)
                        {
                            Console.Write("│@│");
                        }
                        else
                        {
                            Console.Write("│ │");
                        }
                        break;
                    case 2:
                        if (cell == Direction.Down)
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
        }

        private enum Direction
        {
            None,
            Entrance,
            Up,
            Down,
            Left,
            Right
        }

        private static Query DrawGrid(VarGrid<Direction> grid)
        {
            var gridArray = new Var<Direction[,]>();

            return grid.ToArray(gridArray)
                    & (() =>
                    {
                        for (int y = 0; y < height; y++)
                        {
                            for (int row = 0; row < 3; row++)
                            {
                                for (int x = 0; x < width; x++)
                                {
                                    DrawCell(row, gridArray.Value[x, y]);
                                }

                                Console.WriteLine();
                            }
                        }

                        return Query.Success;
                    });
        }

        private struct Coord
        {
            public int X;
            public int Y;

            public static Query Construct(Var<int> x, Var<int> y, Var<Coord> coord)
            {
                return Query.Construct(x, y,
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
