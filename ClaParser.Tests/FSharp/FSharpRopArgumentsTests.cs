﻿using System;
using System.Collections.Generic;
using System.Linq;
using Chessie.ErrorHandling.CSharp;
using ClaParser.FSharp;
using ClaParser.Models;
using FSharpx;
using Microsoft.FSharp.Core;
using NFluent;
using NUnit.Framework;

namespace ClaParser.Tests.FSharp
{
	[TestFixture]
    public class FSharpRopArgumentsTests
    {
        [Test]
        public void Parse_Happy_Tests()
        {
            new List<string>().Parse().Match(
                ifSuccess: (v, _) => Check.That(v.Any()).IsFalse(),
                ifFailure: _ => Assert.Fail());

            new[] { "-x", "42" }.Parse().Match(
                ifSuccess: (v, _) => Check.That(v.First()).IsEqualTo(Tuple.Create("x", "42".Some())),
                ifFailure: _ => Assert.Fail());

            new[] { "-foo_bar", "42" }.Parse().Match(
                ifSuccess: (v, _) => Check.That(v.First()).IsEqualTo(Tuple.Create("foo_bar", "42".Some())),
                ifFailure: _ => Assert.Fail());

            new[] { "-foo bar", "42" }.Parse().Match(
                ifSuccess: (v, _) => Check.That(v.First()).IsEqualTo(Tuple.Create("foo bar", "42".Some())),
                ifFailure: _ => Assert.Fail());

            new[] { "-x" }.Parse().Match(
                ifSuccess: (v, _) => Check.That(v.First()).IsEqualTo(Tuple.Create("x", FSharpOption<string>.None)),
                ifFailure: _ => Assert.Fail());

            new[] { "-x", "-y" }.Parse().Match(
                ifSuccess: (v, _) =>
                {
                    Check.That(v.First()).IsEqualTo(Tuple.Create("x", FSharpOption<string>.None));
                    Check.That(v.Skip(1).First()).IsEqualTo(Tuple.Create("y", FSharpOption<string>.None));
                },
                ifFailure: _ => Assert.Fail());

            new[] { "--x", "42" }.Parse().Match(
                ifSuccess: (v, _) => Check.That(v.First()).IsEqualTo(Tuple.Create("x", "42".Some())),
                ifFailure: _ => Assert.Fail());

            new[] { "/x", "42" }.Parse().Match(
                ifSuccess: (v, _) => Check.That(v.First()).IsEqualTo(Tuple.Create("x", "42".Some())),
                ifFailure: _ => Assert.Fail());


            new[] { "-x", "42", "-y", "88" }.Parse().Match(
                ifSuccess: (v, _) =>
                {
                    Check.That(v.First()).IsEqualTo(Tuple.Create("x", "42".Some()));
                    Check.That(v.Skip(1).First()).IsEqualTo(Tuple.Create("y", "88".Some()));
                },
                ifFailure: _ => Assert.Fail());

            new[] { "-foo", "-y", "88" }.Parse().Match(
                ifSuccess: (v, _) =>
                {
                    Check.That(v.First()).IsEqualTo(Tuple.Create("foo", FSharpOption<string>.None));
                    Check.That(v.Skip(1).First()).IsEqualTo(Tuple.Create("y", "88".Some()));
                },
                ifFailure: _ => Assert.Fail());

            new[] { "-x1", "1", "--x2", "2", "/x3", "-x4", "4" }.Parse().Match(
                ifSuccess: (v, _) => Check.That(v).ContainsExactly(
                    Tuple.Create("x1", "1".Some()),
                    Tuple.Create("x2", "2".Some()),
                    Tuple.Create("x3", FSharpOption<string>.None),
                    Tuple.Create("x4", "4".Some())),
                ifFailure: _ => Assert.Fail());

            new[] { "-x1", "1", "--x2", "2", "x3", "-x4", "4" }.Parse().Match(
                ifSuccess: (v, _) => Assert.Fail("succeeded but should fail"),
                ifFailure: errs => Check.That(errs).ContainsExactly(ClaError.NewInvalidArgument("x3")));
        }

        [Test]
        public void Parse_Rainy_Tests()
        {
            new[] { "x", "y", "z" }.Parse().Match(
                    ifSuccess: (v, _) => Assert.Fail("result should be bad."),
                    ifFailure: errs => Check.That(errs.First().SPrintClaError()).IsEqualTo("Expected a command but got \'x\'."));

            new[] { "-x", "y", "z" }.Parse().Match(
                ifSuccess: (v, _) => Assert.Fail("result should be bad."),
                ifFailure: errs => Check.That(errs.First().SPrintClaError()).IsEqualTo("Expected a command but got \'z\'."));

            new[] { "-" }.Parse().Match(
                ifSuccess: (v, _) => Assert.Fail("result should be bad."),
                ifFailure: errs => Check.That(errs.First().SPrintClaError()).IsEqualTo("Expected a command but got \'-\'."));

            new[] { "-x", "y", "z", "a" }.Parse().Match(
                ifSuccess: (v, _) => Assert.Fail("result should be bad."),
                ifFailure: errs => Check.That(errs.First().SPrintClaError()).IsEqualTo("Expected a command but got \'z\'."));

            new[] { "x", "y" }.Parse().Match(
                ifSuccess: (v, _) => Assert.Fail("result should be bad."),
                ifFailure: errs => Check.That(errs.First().SPrintClaError()).IsEqualTo("Expected a command but got \'x\'."));

            new[] { "-foo", "-y", "88", "bar" }.Parse().Match(
                ifSuccess: (v, _) => Assert.Fail("result should be bad."),
                ifFailure: errs => Check.That(errs.First().SPrintClaError()).IsEqualTo("Expected a command but got \'bar\'."));

            new[] { "foo" }.Parse().Match(
                ifSuccess: (v, _) => Assert.Fail("result should be bad."),
                ifFailure: errs => Check.That(errs.First().SPrintClaError()).IsEqualTo("Expected a command but got \'foo\'."));

            new[] { "//x", "42" }.Parse().Match(
                ifSuccess: (v, _) => Assert.Fail("result should be bad."),
                ifFailure: errs => Check.That(errs.First().SPrintClaError()).IsEqualTo("Expected a command but got \'//x\'."));
        }

        [Test]
        public void ParseArgs_OneRequiredCommand_ReturnsCorrectDictionary()
        {
            var defs = new[] { new ArgInfo("x", FSharpOption<string>.None, true) };

            new[] { "-x", "42" }
                .ParseArgs(defs)
                .Match(
                    ifSuccess: (dictionary, _) => Check.That(dictionary).ContainsExactly(new KeyValuePair<string, FSharpOption<string>>("x", "42".Some())),
                    ifFailure: _ => Assert.Fail());
        }

        [Test]
        public void ParseArgs_TwoRequiredCommands_ReturnsCorrectDictionary()
        {
            var defs = new[]
            {
                new ArgInfo("x", FSharpOption<string>.None, true),
                new ArgInfo("y", FSharpOption<string>.None, true),
            };

            new[] { "-x", "42", "-y", "23" }
                .ParseArgs(defs)
                .Match(
                    ifSuccess: (dictionary, _) => Check.That(dictionary).ContainsExactly(new KeyValuePair<string, FSharpOption<string>>("x", "42".Some()), new KeyValuePair<string, FSharpOption<string>>("y", "23".Some())),
                    ifFailure: _ => Assert.Fail());
        }

        [Test]
        public void ParseArgs_OptionalCommands_ReturnsCorrectDictionary()
        {
            var defs = new[]
            {
                new ArgInfo("x", FSharpOption<string>.None, false),
                new ArgInfo("y", FSharpOption<string>.None, false),
                new ArgInfo("z", FSharpOption<string>.None, false),
            };

            new[] { "-x", "42", "-y", "23" }
                .ParseArgs(defs)
                .Match(
                    ifSuccess: (dictionary, _) => Check.That(dictionary).ContainsExactly(new KeyValuePair<string, FSharpOption<string>>("x", "42".Some()), new KeyValuePair<string, FSharpOption<string>>("y", "23".Some())),
                    ifFailure: _ => Assert.Fail());
        }

        [Test]
        public void ParseArgs_OptionalAndRequiredCommands_ReturnsCorrectDictionary()
        {
            var defs = new[]
            {
                new ArgInfo("foo", FSharpOption<string>.None, true),
                new ArgInfo("x", FSharpOption<string>.None, false),
                new ArgInfo("y", FSharpOption<string>.None, false),
                new ArgInfo("z", FSharpOption<string>.None, false),
            };

            new[] { "-x", "42", "-y", "23", "-foo", "bar" }
                .ParseArgs(defs)
                .Match(
                    ifSuccess: (dictionary, _) => Check.That(dictionary).ContainsExactly(new KeyValuePair<string, FSharpOption<string>>("x", "42".Some()), new KeyValuePair<string, FSharpOption<string>>("y", "23".Some()), new KeyValuePair<string, FSharpOption<string>>("foo", "bar".Some())),
                    ifFailure: _ => Assert.Fail());
        }


        [Test]
        public void ParseArgs_RequiredCommandMissing_ReturnsCorrectMsg()
        {
            var defs = new[]
            {
                new ArgInfo("x", FSharpOption<string>.None, true),
                new ArgInfo("y", FSharpOption<string>.None, true),
            };

            new[] { "-x", "42" }
                .ParseArgs(defs)
                .Match(
                    ifSuccess: (dictionary, _) => Assert.Fail(),
                    ifFailure: errs => Check.That(errs.Select(x => x.SPrintClaError())).ContainsExactly("Required command \'y\' not in argument list."));
        }

        [Test]
        public void ParseArgs_DuplicateArg_ReturnsCorrectMsg()
        {
            var defs = new[]
            {
                new ArgInfo("x", FSharpOption<string>.None, true),
            };

            new[] { "-x", "42", "-x", "24" }
                .ParseArgs(defs)
                .Match(
                    ifSuccess: (dictionary, _) => Assert.Fail(),
                    ifFailure: errs => Check.That(errs.Select(x => x.SPrintClaError())).ContainsExactly("Duplicate command \'x\'."));
        }

        [Test]
        public void ParseArgs_TwoRequiredCommandsMissing_ReturnsCorrectMsg()
        {
            var defs = new[]
            {
                new ArgInfo("x", FSharpOption<string>.None, true),
                new ArgInfo("y", FSharpOption<string>.None, true),
                new ArgInfo("z", FSharpOption<string>.None, true),
            };

            new[] { "-x", "42" }
                .ParseArgs(defs)
                .Match(
                    ifSuccess: (dictionary, _) => Assert.Fail(),
                    ifFailure: errs => Check.That(errs.Select(x => x.SPrintClaError())).ContainsExactly("Required command \'y\' not in argument list.", "Required command \'z\' not in argument list."));
        }

        [Test]
        public void ParseArgs_FirstArgNoCommand_ReturnsCorrectMsg()
        {
            var defs = new[]
            {
                new ArgInfo("x", FSharpOption<string>.None, true),
            };

            new[] { "x" }
                .ParseArgs(defs)
                .Match(
                    ifSuccess: (dictionary, _) => Assert.Fail(),
                    ifFailure: errs => Check.That(errs.Select(x => x.SPrintClaError())).ContainsExactly("Expected a command but got \'x\'."));
        }

        [Test]
        public void ParseArgs_ValueMissing_ValueEmpty()
        {
            var defs = new[]
            {
                new ArgInfo("x", FSharpOption<string>.None, true),
                new ArgInfo("y", FSharpOption<string>.None, true),
                new ArgInfo("z", FSharpOption<string>.None, false),
            };

            new[] { "-x", "-y", "42", "-z" }
                .ParseArgs(defs)
                .Match(
                    ifSuccess: (dictionary, _) => Check.That(dictionary).ContainsExactly(new KeyValuePair<string, FSharpOption<string>>("x", FSharpOption<string>.None), new KeyValuePair<string, FSharpOption<string>>("y", "42".Some()), new KeyValuePair<string, FSharpOption<string>>("z", FSharpOption<string>.None)),
                    ifFailure: _ => Assert.Fail());
        }

        [Test]
        public void ParseArgs_CommandNotDefined_ReturnsCorrectMsg()
        {
            var defs = new[]
            {
                new ArgInfo("x", FSharpOption<string>.None, false),
            };

            new[] { "-y", "lizard" }
                .ParseArgs(defs)
                .Match(
                    ifSuccess: (dictionary, _) => Assert.Fail(),
                    ifFailure: errs => Check.That(errs.Select(x => x.SPrintClaError()))
                        .ContainsExactly("Command 'y' not defined."));
        }

        [Test]
        public void ParseArgs_RequiredCommandMissingAndInalidArg_ReturnsCorrectMsg()
        {
            var defs = new[]
            {
                new ArgInfo("x", FSharpOption<string>.None, true),
                new ArgInfo("y", FSharpOption<string>.None, false),
                new ArgInfo("z", FSharpOption<string>.None, false),
            };

            new[] { "-y", "lizard", "-a", "Anton", "-a", "Anton" }
                .ParseArgs(defs)
                .Match(
                    ifSuccess: (dictionary, _) => Assert.Fail(),
                    ifFailure: errs => Check.That(errs.Select(x => x.SPrintClaError()))
                        .ContainsExactly("Required command \'x\' not in argument list.", "Command \'a\' not defined."));
        }

        [Test]
        public void ParseArgs_DuplicateArgAndRequiredCommandMissingAndCommandNotDefined_ReturnsCorrectMsg()
        {
            var defs = new[]
            {
                new ArgInfo("x", FSharpOption<string>.None, true),
                new ArgInfo("y", FSharpOption<string>.None, false),
                new ArgInfo("z", FSharpOption<string>.None, false),
            };

            new[] { "-y", "lizard", "-a", "Anton", "-z", "Zealot", "-z", "Zoo" }
                .ParseArgs(defs)
                .Match(
                    ifSuccess: (dictionary, _) => Assert.Fail("should fail but was success."),
                    ifFailure: errs => Check.That(errs).ContainsExactly(
                        ClaError.NewDuplicateCommand("z"), 
                        ClaError.NewRequiredCommandMissing("x"), 
                        ClaError.NewUnknownCommand("a")));
        }
    }
}