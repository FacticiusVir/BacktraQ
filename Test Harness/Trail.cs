using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper.LSharp
{
    public class Trail
    {
        private Trail parent;
        private Stack<List<Var>> varTrail = new Stack<List<Var>>();

        [ThreadStatic]
        private static Trail current;

        public static Trail Current
        {
            get
            {
                return current;
            }
        }

        public Trail(Trail parent)
        {
            this.parent = parent;
        }

        internal static void Enter()
        {
            current = new Trail(Current);
        }

        internal static void Exit()
        {
            current = Current.parent;
        }

        internal void ChoicePoint()
        {
            this.varTrail.Push(new List<Var>());
        }

        internal void Log<T>(Var<T> variable)
        {
            if (this.varTrail.Any())
            {
                this.varTrail.Peek().Add(variable);
            }
        }

        internal void Backtrack()
        {
            if (this.varTrail.Any())
            {
                var trail = this.varTrail.Pop();

                foreach (var variable in trail)
                {
                    variable.Reset();
                }
            }
        }
    }
}
