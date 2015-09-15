using System.Collections.Generic;
using Chessie.ErrorHandling;

namespace ClaParser.CSharp
{
    public static class Result<T>
    {
        public static Result<T, TMessage> FailWith<TMessage>(TMessage msg)
        {
            return Result<T, TMessage>.FailWith(msg);
        }

        public static Result<T, TMessage> FailWith<TMessage>(IEnumerable<TMessage> msgs)
        {
            return Result<T, TMessage>.FailWith(msgs);
        }

        public static Result<TSuccess, T> Succeed<TSuccess>(TSuccess value)
        {
            return Result<TSuccess, T>.Succeed(value);
        }
    }
}