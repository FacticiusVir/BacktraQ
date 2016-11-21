using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keeper.BacktraQ
{
    public static class QMath
    {
        public static Query Inc(Var<int> initial, Var<int> result)
        {
            return Query.Create(() =>
            {
                if(initial.HasValue)
                {
                    return result.Unify(initial.Value + 1);
                }
                else if(result.HasValue)
                {
                    return initial.Unify(result.Value - 1);
                }
                else
                {
                    throw new Exception("Insufficiently instantiated terms.");
                }
            });
        }
    }
}
