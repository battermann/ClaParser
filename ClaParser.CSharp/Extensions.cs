using System;
using System.Collections.Generic;
using System.Linq;
using Chessie.ErrorHandling;
using Chessie.ErrorHandling.CSharp;
using FSharpx;
using Microsoft.FSharp.Core;

namespace ClaParser.CSharp
{
    public static class FuncExtensions
    {
        public static Result<TSuccess, TMessage> Try<TSource, TSuccess, TMessage>(this Func<TSource, TSuccess> parser, TSource source, TMessage msg)
        {
            return Result<TSuccess, TMessage>.Try(() => parser(source))
                .Either(
                    ifSuccess: (value, list) => Result<TSuccess, TMessage>.Succeed(value),
                    ifFailure: _ => Result<TSuccess, TMessage>.FailWith(msg));
        }
    }

    public static class ValidationExtensions
    {
        public static Result<TSuccess, TMessage> Validate<TSuccess, TMessage>(this TSuccess v, Func<TSuccess, IEnumerable<TMessage>> validator)
        {
            var errors = validator(v);
            return errors.Any()
                ? Result<TSuccess>.FailWith(errors)
                : Result<TMessage>.Succeed(v);
        }
    }

    public static class FSharpOptionExtensions
    {
        public static Result<TSuccess, TMessage> ToResult<TSuccess, TMessage>(this FSharpOption<TSuccess> option, TMessage msg)
        {
            return option.Match(
                    ifSome: Result<TSuccess, TMessage>.Succeed,
                    ifNone: () => Result<TSuccess, TMessage>.FailWith(msg));
        }
    }
}