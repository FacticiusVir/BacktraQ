using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper.BacktraQ
{
    public class Trail
    {
        private readonly Trail parent;
        private readonly Stack<ChoiceFrame> frames;

        [ThreadStatic]
        private static Trail current;

        public static Trail Current => current;

        public Trail(Trail parent)
        {
            this.parent = parent;
            this.frames = new Stack<ChoiceFrame>();
        }

        internal static void Enter()
        {
            current = new Trail(Current);

            current.ChoicePoint(null);
        }

        internal static void Exit(bool revertAll = false)
        {
            var trail = current;

            if (revertAll)
            {
                while (trail.Depth > 0)
                {
                    trail.Backtrack();
                }
            }
            else if (trail.parent is Trail parent)
            {
                foreach (var frame in trail.frames.Reverse())
                {
                    parent.frames.Push(frame);
                }
            }

            current = trail.parent;
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
