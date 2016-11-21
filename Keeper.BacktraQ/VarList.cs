namespace Keeper.BacktraQ
{
    public class VarList<T>
    {
        public VarList()
        {
            this.Head = new Var<T>();
            this.Tail = new Var<VarList<T>>();
        }

        private VarList(Var<T> head, Var<VarList<T>> tail)
        {
            this.Head = head;
            this.Tail = tail;
        }

        public Var<T> Head
        {
            get;
            private set;
        }

        public Var<VarList<T>> Tail
        {
            get;
            private set;
        }

        public bool IsEmptyList
        {
            get
            {
                return this.Head == null || this.Tail == null;
            }
        }

        public Query Length(Var<int> length)
        {
            if (this.IsEmptyList)
            {
                return length.Unify(0);
            }

            throw new System.NotImplementedException();
        }

        private static VarList<T> emptyList = new VarList<T>(null, null);

        public static VarList<T> EmptyList
        {
            get
            {
                return emptyList;
            }
        }
    }
}
