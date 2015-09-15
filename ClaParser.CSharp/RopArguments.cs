using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Chessie.ErrorHandling;
using Chessie.ErrorHandling.CSharp;
using ClaParser.Models;
using FSharpx;
using Microsoft.FSharp.Core;

namespace ClaParser.CSharp
{
   public static class RopArguments
    {
        public static Result<Dictionary<string, FSharpOption<string>>, ClaError> ParseArgs(this IEnumerable<string> commandLineArgs, IEnumerable<ArgInfo> defs)
        {
            var definedCmds = defs.Select(x => x.command);
            var requiredCmds = defs.Where(x => x.required).Select(x => x.command);

            return 
                from parsedArgs in commandLineArgs.Parse()
                from dict in 
                    from check1 in parsedArgs.Validate(CheckArgsContainNoDuplicates(definedCmds))
                    join check2 in parsedArgs.Validate(CheckAllRequiredArgsExist(requiredCmds)) on 1 equals 1
                    join check3 in parsedArgs.Validate(CheckAllArgsAreDefined(definedCmds)) on 1 equals 1
                    select check3.ToDictionary(x => x.Item1, x => x.Item2)
                select dict;
        }

        private static FSharpOption<string> TryParseCommand(this string cmd)
        {
            var m = Regex.Match(cmd, @"^(?:-{1,2}|\/)(?<command>[a-zA-Z0-9]+.*)$", RegexOptions.IgnoreCase);
            return m.Success
                ? m.Groups["command"].Value.ToLower().Some()
                : FSharpOption<string>.None;
        }

        internal static Result<List<Tuple<string, FSharpOption<string>>>, ClaError> Parse(this IEnumerable<string> args)
        {
            // non-recursive is much faster than recursive implementation in C#
            Func<Result<List<Tuple<string, FSharpOption<string>>>, ClaError>, Tuple<FSharpOption<string>, string>, Result<List<Tuple<string, FSharpOption<string>>>, ClaError>> folder =
                (acc, t) => 
                    from accValue in acc
                    from accValueNew in t.Item1.Select(cmd =>
                            t.Item2.TryParseCommand().HasValue()
                                ? Tuple.Create(cmd, FSharpOption<string>.None)
                                : Tuple.Create(cmd, t.Item2.ToLower().Some()))
                        .ToResult(ClaError.NewInvalidArgument(t.Item2))
                        .Select(xs => accValue.Concat(new[] { xs }).ToList())
                    select accValueNew;

            // first arg has to be a command
            var emptyOrError = args.Take(1).All(x => x.TryParseCommand().HasValue())
                ? Result<ClaError>.Succeed(new List<Tuple<string, FSharpOption<string>>>())
                : Result<List<Tuple<string, FSharpOption<string>>>>.FailWith(ClaError.NewInvalidArgument(args.First()));

            return args
                .Zip(args.Skip(1).Concat(args.Take(1)), (fst, snd) => Tuple.Create(fst.TryParseCommand(), snd))
                .Where(pair => pair.Item1.HasValue() || !pair.Item2.TryParseCommand().HasValue())
                .Aggregate(emptyOrError, folder);
        }

        private static readonly Func<IEnumerable<string>, Func<IEnumerable<Tuple<string, FSharpOption<string>>>, IEnumerable<ClaError>>> CheckArgsContainNoDuplicates =
            definedCommands => args =>
                args
                .Where(x => definedCommands.Contains(x.Item1))
                .GroupBy(x => x.Item1)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key)
                .Distinct()
                .Select(ClaError.NewDuplicateCommand);

        private static readonly Func<IEnumerable<string>, Func<IEnumerable<Tuple<string, FSharpOption<string>>>, IEnumerable<ClaError>>> CheckAllArgsAreDefined =
            definedCommands => args =>
                args
                .Select(x => x.Item1)
                .Distinct()
                .Where(x => !definedCommands.Contains(x))
                .Select(ClaError.NewUnknownCommand);

        private static readonly Func<IEnumerable<string>, Func<IEnumerable<Tuple<string, FSharpOption<string>>>, IEnumerable<ClaError>>> CheckAllRequiredArgsExist =
            requiredCommands => args =>
                requiredCommands
                .Where(required => !args.Select(p => p.Item1).Contains(required))
                .Select(ClaError.NewRequiredCommandMissing);
    }
}
