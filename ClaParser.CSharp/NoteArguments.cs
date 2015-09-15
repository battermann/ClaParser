using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ClaParser.Models;

namespace ClaParser.CSharp
{
    public class NoteArguments
    {
        private readonly IEnumerable<string> _commandLineArguments;
        private readonly IEnumerable<string> _requiredCommands;
        private readonly IEnumerable<string> _definedCommands;
        private Dictionary<string, string> _dictionary;

        public NoteArguments(IEnumerable<string> commandLineArguments, IEnumerable<ArgInfo> defs)
        {
            _commandLineArguments = commandLineArguments;
            _requiredCommands = defs.Where(x => x.required).Select(x => x.command);
            _definedCommands = defs.Select(x => x.command);
        }

        public Dictionary<string, string> Dictionary
        {
            get
            {
                return _dictionary ?? (_dictionary = CreateDictionary());
            }
        }

        private Dictionary<string, string> CreateDictionary()
        {
            Check();
            return _dictionary;
        }

        private void Check()
        {
            var note = Validation();
            if (note.HasErrors)
                throw new ArgumentException(note.ErrorMessage);
        }

        public Notification Validation()
        {
            var note = new Notification();

            var parsed = Parse(note);

            if (note.HasErrors)
                return note;

            CheckNoDuplicates(parsed, note);
            CheckAllRequired(parsed, note);
            CheckAllDefined(parsed, note);

            if (note.IsValid)
                _dictionary = parsed.ToDictionary(x => x.First(), x => x.Skip(1).First());

            return note;
        }

        private static readonly Regex Regex = new Regex(@"^(?:-{1,2}|\/)(?<command>[a-zA-Z0-9]+.*)$", RegexOptions.IgnoreCase);

        private static bool TryParseCommand(string arg, out string cmd)
        {
            cmd = null;
            var m = Regex.Match(arg);
            if (m.Success)
                cmd = m.Groups["command"].Value.ToLower();
            return m.Success;
        }

        private static bool IsCommand(string arg)
        {
            return Regex.IsMatch(arg);
        }

        internal IEnumerable<string[]> Parse(Notification note)
        {
            var parsed = new List<string[]>();

            var length = _commandLineArguments.Count();

            for (var i = 0; i < length; i++)
            {
                var fst = _commandLineArguments.ElementAt(i);
                var snd = i < length - 1 ? _commandLineArguments.ElementAt(i + 1) : _commandLineArguments.ElementAt(0);
                var sndIsCmd = IsCommand(snd);

                string cmd;

                if (TryParseCommand(fst, out cmd))
                {
                    var value = sndIsCmd
                        ? String.Empty
                        : snd;

                    parsed.Add(new[] { cmd, value });
                    continue;
                }

                if (sndIsCmd)
                    continue;

                note.AddError(String.Format("Expected a command but got \'{0}\'.", i == 0 ? fst : snd));
                break;
            }

            return parsed;
        }

        private void CheckNoDuplicates(IEnumerable<string[]> args, Notification note)
        {
            var duplicates = args
                .Where(x => _definedCommands.Contains(x.First()))
                .GroupBy(x => x.First())
                .Where(x => x.Count() > 1)
                .Select(x => x.Key)
                .Distinct();

            foreach (var duplicate in duplicates)
            {
                note.AddError(String.Format("Duplicate command \'{0}\'.", duplicate));
            }
        }

        private void CheckAllDefined(IEnumerable<string[]> args, Notification note)
        {
            foreach (var arg in args.Select(x => x.First()).Distinct().Where(arg => !_definedCommands.Contains(arg)))
            {
                note.AddError(String.Format("Command \'{0}\' not defined.", arg));
            }
        }

        private void CheckAllRequired(IEnumerable<string[]> args, Notification note)
        {
            foreach (var arg in _requiredCommands.Where(x => !args.Select(arg => arg.First()).Contains(x)))
            {
                note.AddError(String.Format("Required command \'{0}\' not in argument list.", arg));
            }
        }
    }
}
