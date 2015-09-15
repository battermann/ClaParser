using System;
using System.Collections.Generic;
using System.Linq;
using Chessie.ErrorHandling.CSharp;
using ClaParser.CSharp;
using ClaParser.Models;

namespace ClaParser.Example
{
    public class RopCSharp
    {
        public static void Run(IEnumerable<string> commandLineArguments, IEnumerable<ArgInfo> defs, Action<int, int> action)
        {
            var parseToInt = new Func<string, int>(Int32.Parse);

            var valuesResult =
                from dict in commandLineArguments.ParseArgs(defs)
                from values in
                    from x in
                        from v in dict["x"].ToResult(ClaError.NewValueMissing("x"))
                        from vParsed in parseToInt.Try(v, ClaError.NewCannotParseValue("x"))
                        select vParsed
                    join y in
                        from v in dict["y"].ToResult(ClaError.NewValueMissing("y"))
                        from vParsed in parseToInt.Try(v, ClaError.NewCannotParseValue("y"))
                        select vParsed on 1 equals 1
                    select new { X = x, Y = y }
                select values;

            valuesResult.Match(
                ifSuccess: (v, list) => action(v.X, v.Y),
                ifFailure: list => list.Select(x => x.SPrintClaError()).ToList().ForEach(Console.WriteLine));
        }
    }
}
