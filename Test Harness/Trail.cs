using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper.LSharp
{
    public class Trail
    {
        private Trail parent;
        private Stack<ChoiceFrame> frames = new Stack<ChoiceFrame>();

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

            current.ChoicePoint(null);
        }

        internal static void Exit(bool revertAll = false)
        {
            while(revertAll && Current.Depth > 0)
            {
                Current.Backtrack();
            }

            current = Current.parent;
        }

        internal void ChoicePoint(Query continuation)
        {
            this.frames.Push(new ChoiceFrame
            {
                Continuation = continuation,
                Trail = new List<Var>()
            });
        }

        internal void Log<T>(Var<T> variable)
        {
            if (this.frames.Any())
            {
                this.frames.Peek().Trail.Add(variable);
            }
        }

        internal void Cut()
        {
            if (this.frames.Any())
            {
                var frame = this.frames.Pop();

                if (this.frames.Any())
                {
                    var previousFrame = this.frames.Peek();

                    previousFrame.Trail.AddRange(frame.Trail);
                }
            }
        }

        internal Query Backtrack()
        {
            if (this.frames.Any())
            {
                var frame = this.frames.Pop();

                foreach (var variable in frame.Trail)
                {
                    variable.Reset();
                }

                return frame.Continuation;
            }
            else
            {
                return null;
            }
        }

        internal int Depth
        {
            get
            {
                return this.frames.Count;
            }
        }

        private struct ChoiceFrame
        {
            public Query Continuation;
            public List<Var> Trail;
        }
    }
}
