using System.Collections.Generic;

using static Keeper.BacktraQ.Query;

namespace Keeper.BacktraQ
{
    public class VarGrid<T>
    {
        private readonly VarList<VarList<T>> grid;
        private readonly int width;
        private readonly int height;

        public VarGrid(int width, int height)
        {
            this.width = width;
            this.height = height;

            var columnList = new List<VarList<T>>();

            for (int x = 0; x < width; x++)
            {
                columnList.Add(VarList.Create<T>(height));
            }

            this.grid = VarList.Create(columnList.ToArray());
        }

        public Query Member(Var<T> element)
        {
            var columnVar = new Var<VarList<T>>();
            var column = new VarList<T>();

            return columnVar <= column
                    & columnVar <= grid.Member
                    & element <= column.Member;
        }
        public Query RandomMember(Var<T> element)
        {
            var column = new VarList<T>();

            return grid.RandomMember(out var columnVar)
                    & columnVar <= column
                    & element <= column.RandomMember;
        }

        public Query XYth(Var<int> x, Var<int> y, Var<T> element)
        {
            var columnVar = new Var<VarList<T>>();
            var column = new VarList<T>();

            return grid.Nth(x, columnVar)
                    & columnVar <= column
                    & column.Nth(y, element);
        }

        public Query ToArray(out Var<T[,]> array, T defaultValue = default(T)) => ToArray(NewVar(out array), defaultValue);

        public Query ToArray(Var<T[,]> array, T defaultValue = default(T))
        {
            return Wrap(() =>
            {
                var x = new Var<int>();
                var y = new Var<int>();
                var element = new Var<T>();

                if (array.HasValue)
                {
                    var arrayValue = array.Value;

                    foreach (var result in this.XYth(x, y, element))
                    {
                        if (!arrayValue[x.Value, y.Value].Equals(element.Value))
                        {
                            return Fail;
                        }
                    }
                }
                else
                {
                    var arrayValue = new T[this.width, this.height];

                    foreach (var result in this.XYth(x, y, element))
                    {
                        arrayValue[x.Value, y.Value] = element.GetValueOrDefault(defaultValue);
                    }

                    return array.Unify(arrayValue);
                }

                return Success;
            });
        }
    }
}
