using Keeper.BacktraQ;
using System;
using System.Diagnostics;

using static Keeper.BacktraQ.Query;

namespace DungeonGen
{
    class Program
    {
        private const int height = 10, width = 10;

        private const int roomCount = 20;

        static void Main(string[] args)
        {
            Console.WriteLine("Running");

            var grid = new VarGrid<Direction>(width, height);

            var query = PlaceFirstCell(grid, out var initialcoord)
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
            return Chain((oldList, newCoord) => PlaceConnectedCell(grid, newCoord, oldList), roomCount - 1, VarList.Create(initialcoord));
        }

        private static Var<VarList<Direction>> DirectionList = VarList.Create(Direction.Up, Direction.Down, Direction.Left, Direction.Right);

        private static Query PlaceFirstCell(VarGrid<Direction> grid, out Var<Coord> coord)
        {
            return Random(width, out var x)
                    & Random(height, out var y)
                    & Coord.Construct(x, y, out coord)
                    & GetCell(grid, coord, Direction.Entrance);
        }

        private static Query PlaceConnectedCell(VarGrid<Direction> grid, Var<Coord> coord, Var<VarList<Coord>> placedList)
        {
            return placedList.RandomMember(out var placedCoord)
                        & DirectionList.RandomMember(out var direction)
                        & Offset(placedCoord, direction, coord)
                        & IsInBounds(coord)
                        & HasFreeSides(grid, coord, 3)
                        & !IsCorridor(grid, coord, direction, 3)
                        & GetCell(grid, coord, NewVar<Direction>(out var cell))
                        & cell.IsVar()
                        & cell <= direction;
        }

        private static Query IsCorridor(VarGrid<Direction> grid, Var<Coord> coord, Var<Direction> direction, Var<int> length)
        {
            return length <= 1
                | (() => Offset(NewVar<Coord>(out var previousCoord), direction, coord)
                                & GetCell(grid, previousCoord, direction)
                                & length <= NewVar<int>(out var previousLength).Inc
                                & IsCorridor(grid, previousCoord, direction, previousLength));
        }

        private static Query HasFreeSides(VarGrid<Direction> grid, Var<Coord> coord, int requiredCount)
        {
            var isUnset = GetCell(grid, NewVar<Coord>(out var adjacent), NewVar<Direction>(out var cell)) & cell.IsVar();

            var adjacenctFree = Adjacent(coord, adjacent)
                                    & (isUnset | !IsInBounds(adjacent));

            var results = new Var<VarList<Coord>>();
            var count = new Var<int>();

            return adjacenctFree.FindAll(adjacent, results)
                            & count <= results.Length
                            & count.GreaterThanOrEqual(requiredCount);
        }

        private static Query GetCell(VarGrid<Direction> grid, Var<Coord> coord, Var<Direction> cell)
        {
            return IsInBounds(coord)
                        & Coord.Construct(out var x, out var y, coord)
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
            return Query.Map(out var xOffset, out var yOffset, direction, (0, -1, Direction.Down), (0, 1, Direction.Up), (-1, 0, Direction.Right), (1, 0, Direction.Left))
                    & IfThen(oldCoord.IsNonVar(), Coord.Construct(out var oldX, out var oldY, oldCoord), Coord.Construct(out var newX, out var newY, newCoord))
                    & QMath.Add(oldX, xOffset, newX)
                    & QMath.Add(oldY, yOffset, newY)
                    & Coord.Construct(newX, newY, newCoord)
                    & Coord.Construct(oldX, oldY, oldCoord);
        }

        private static Query IsInBounds(Var<Coord> coord)
        {
            return Coord.Construct(out var x, out var y, coord)
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
            return grid.ToArray(out var gridArray)
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
            public readonly int X;
            public readonly int Y;
            
            public static Query Construct(Var<int> x, Var<int> y, out Var<Coord> coord) => Construct(x, y, NewVar(out coord));

            public static Query Construct(out Var<int> x, out Var<int> y, Var<Coord> coord) => Construct(NewVar(out x), NewVar(out y), coord);

            public static Query Construct(Var<int> x, Var<int> y, Var<Coord> coord)
            {
                return Query.Construct(x, y,
                                        coord,
                                        (xValue, yValue) => new Coord(xValue, yValue),
                                        coordValue => Tuple.Create(coordValue.X, coordValue.Y));
            }

            public Coord(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public override string ToString()
            {
                return $"X: {X}, Y: {Y}";
            }
        }
    }
}
