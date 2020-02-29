using Keeper.BacktraQ;
using System;
using System.Diagnostics;

using static Keeper.BacktraQ.Query;

namespace DungeonGen
{
    class Program
    {
        private const int height = 8, width = 20;

        private const int roomCount = 60;

        static void Main(string[] args)
        {
            Console.WriteLine("Running");

            var grid = new VarGrid<Direction>(width, height);

            var query = PlaceFirstCell(grid, out var initialCoord)
                            & PlaceConnectedCells(grid, initialCoord)
                            & DrawGrid(grid);

            long startTimestamp = Stopwatch.GetTimestamp();

            if (!query.Succeeds())
            {
                Console.WriteLine("Could not generate dungeon grid.");
            }

            double duration = (Stopwatch.GetTimestamp() - startTimestamp) / (double)Stopwatch.Frequency;

            Console.WriteLine($"Query took {duration:0.0##} seconds.");

            Console.WriteLine("Done");
        }

        private static Query PlaceConnectedCells(VarGrid<Direction> grid, Var<(Var<int>, Var<int>)> initialcoord)
        {
            return QueryExtensions.Chain((oldList, newCoord) => PlaceConnectedCell(grid, newCoord, oldList), roomCount - 1, VarList.Create(initialcoord));
        }

        private static readonly Var<VarList<Direction>> DirectionList = VarList.Create(Direction.Up, Direction.Down, Direction.Left, Direction.Right);

        private static Query PlaceFirstCell(VarGrid<Direction> grid, out Var<(Var<int>, Var<int>)> coord)
        {
            return Random(width, out var x)
                    & Random(height, out var y)
                    & NewVar(out coord) <= (x, y)
                    & GetCell(grid, coord, Direction.Entrance);
        }

        private static Query PlaceConnectedCell(VarGrid<Direction> grid, Var<(Var<int>, Var<int>)> coord, Var<VarList<(Var<int>, Var<int>)>> placedList)
        {
            return NewVar<int, int>(out var placedCoord) <= placedList.RandomMember
                        & NewVar<Direction>(out var direction) <= DirectionList.RandomMember
                        & Offset(placedCoord, direction, coord)
                        & IsInBounds(coord)
                        & HasFreeSides(grid, coord, 3)
                        & !IsCorridor(grid, coord, direction, 3)
                        & GetCell(grid, coord, NewVar<Direction>(out var cell))
                        & cell.IsVar()
                        & cell <= direction;
        }

        private static Query IsCorridor(VarGrid<Direction> grid, Var<(Var<int>, Var<int>)> coord, Var<Direction> direction, Var<int> length)
        {
            return length <= 1
                | (() => Offset(NewVar<int, int>(out var previousCoord), direction, coord)
                                & GetCell(grid, previousCoord, direction)
                                & length <= NewVar<int>(out var previousLength).Inc
                                & IsCorridor(grid, previousCoord, direction, previousLength));
        }

        private static Query HasFreeSides(VarGrid<Direction> grid, Var<(Var<int>, Var<int>)> coord, int requiredCount)
        {
            var isUnset = GetCell(grid, NewVar<int, int>(out var adjacent), NewVar<Direction>(out var cell)) & cell.IsVar();

            var adjacenctFree = Adjacent(coord, adjacent)
                                    & (isUnset | !IsInBounds(adjacent));

            return NewList<int, int>(out var results) <= adjacenctFree.FindAll(adjacent)
                        & NewVar<int>(out var count) <= results.Length
                        & count.GreaterThanOrEqual(requiredCount);
        }

        private static Query GetCell(VarGrid<Direction> grid, Var<(Var<int>, Var<int>)> coord, Var<Direction> cell)
        {
            return IsInBounds(coord)
                        & coord.Deconstruct(out var x, out var y)
                        & grid.XYth(x, y, cell);
        }

        private static Query Adjacent(Var<(Var<int>, Var<int>)> coord, Var<(Var<int>, Var<int>)> adjacent) =>
            NewVar<Direction>(out var direction) <= DirectionList.Member
                & Offset(coord, direction, adjacent);

        private static Query Offset(Var<(Var<int>, Var<int>)> oldCoord, Var<Direction> direction, Var<(Var<int>, Var<int>)> newCoord)
        {
            return Map(out var xOffset, out var yOffset, direction, (0, -1, Direction.Down), (0, 1, Direction.Up), (-1, 0, Direction.Right), (1, 0, Direction.Left))
                    & oldCoord.Deconstruct(out var oldX, out var oldY)
                    & newCoord.Deconstruct(out var newX, out var newY)
                    & newX <= oldX.Add(xOffset)
                    & newY <= oldY.Add(yOffset);
        }

    private static Query IsInBounds(Var<(Var<int>, Var<int>)> coord)
    {
        return coord.Deconstruct(out var x, out var y)
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
}
}
