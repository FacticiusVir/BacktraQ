namespace Keeper.BacktraQ
{
    public struct QueryResult
    {
        public Query Continuation;
        public Query Alternate;
        public QueryResultType Type;

        public static QueryResult Success
        {
            get
            {
                return new QueryResult
                {
                    Type = QueryResultType.Success
                };
            }
        }

        public static QueryResult Fail
        {
            get
            {
                return new QueryResult
                {
                    Type = QueryResultType.Fail
                };
            }
        }
    }

    public enum QueryResultType
    {
        Fail,
        ChoicePoint,
        Success
    }
}

//    public class QueryResult
//    {
//        public static QueryResult<T> Success<T>(T value)
//        {
//            return new QueryResult<T>
//            {
//                Type = QueryResultType.Success,
//                Value = value
//            };
//        }

//        public static QueryResult<T> ChoicePoint<T>(Query<T> continuation, Query<T> alternate)
//        {
//            return new QueryResult<T>
//            {
//                Type = QueryResultType.ChoicePoint,
//                Continuation = continuation,
//                Alternate = alternate
//            };
//        }

//        public static QueryResult<T> Fail<T>()
//        {
//            return new QueryResult<T>
//            {
//                Type = QueryResultType.Fail,
//            };
//        }
//    }

//    public class QueryResult<T>
//    {
//        public QueryResultType Type
//        {
//            get;
//            internal set;
//        }

//        public T Value
//        {
//            get;
//            internal set;
//        }

//        public Query<T> Continuation
//        {
//            get;
//            internal set;
//        }

//        public Query<T> Alternate
//        {
//            get;
//            internal set;
//        }
//    }
//}
