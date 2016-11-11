using System;

namespace Keeper.LSharp
{
    public class CommitContext
        : IDisposable
    {
        public CommitContext()
        {
            Trail.Enter();
        }

        public void Dispose()
        {
            Trail.Exit();
        }
    }
}
