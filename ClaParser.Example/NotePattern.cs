using System;
using System.Collections.Generic;
using System.Linq;
using ClaParser.CSharp;
using ClaParser.Models;

namespace ClaParser.Example
{
    public static class NotePattern
    {
        public static void Run(IEnumerable<string> commandLineArguments, IEnumerable<ArgInfo> defs, Action<int, int> action)
        {
            var args = new NoteArguments(commandLineArguments, defs);
            var note = args.Validation();

            if (!note.IsValid)
            {
                note.Errors.ToList().ForEach(Console.WriteLine);
                return;
            }

            var result = args.Dictionary;

            int x;
            if (!Int32.TryParse(result["x"], out x))
            {
                Console.WriteLine("Could not parse x");
                return;
            }

            int y;
            if (!Int32.TryParse(result["y"], out y))
            {
                Console.WriteLine("Could not parse y");
                return;
            }

            action(x, y);
        }
    }
}
