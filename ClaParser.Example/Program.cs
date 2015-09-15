using System;
using ClaParser.Models;
using FSharpx;

namespace ClaParser.Example
{
    class Program
    {
        static void PrintSum(int x, int y)
        {
            Console.WriteLine(x + y);
        }

        static void Main(string[] args)
        {
            // Failure
            var commandLineArgs = new[] { "-x", "1", "-x", "5", "-z", "2", "-lizard", "foo", "-qwertz" };

            // Success
            //var commandLineArgs = new[] { "-x", "1", "-y", "5" };

            var defs = new[]
            {
                new ArgInfo("x", "value type integer".Some(), true),
                new ArgInfo("y", "value type integer".Some(), true),
            };

            Console.WriteLine("ERRORHANDLING WITH NOTIFICATIONS:");
            NotePattern.Run(commandLineArgs, defs, PrintSum);
            Console.WriteLine();
            Console.WriteLine("FUNCTIONAL ERRORHANDLINIG CS:");
            RopCSharp.Run(commandLineArgs, defs, PrintSum);
            Console.WriteLine();
            Console.WriteLine("FUNCTIONAL ERRORHANDLINIG FS:");
            RopFSharp.Run(commandLineArgs, defs, PrintSum);

            Console.ReadLine();
        }
    }
}
