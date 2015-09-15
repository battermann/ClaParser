using System;
using System.Collections.Generic;
using System.Linq;
using ClaParser.CSharp;
using ClaParser.Models;
using NFluent;
using NUnit.Framework;

namespace ClaParser.Tests.CSharp
{
    [TestFixture]
    public class NoteArgumentsTests
    {
        [TestCase("-x;42", "x=42", 1)]
        [TestCase("-foo_bar;42", "foo_bar=42", 1)]
        [TestCase("-foo bar;42", "foo bar=42", 1)]
        [TestCase("-x", "x=", 1)]
        [TestCase("-x;-y", "x=;y=", 2)]
        [TestCase("--x;42", "x=42", 1)]
        [TestCase("/x;42", "x=42", 1)]
        [TestCase("-x;42;-y;88", "x=42;y=88", 2)]
        [TestCase("-foo;-y;88", "foo=;y=88", 2)]
        public void Parse_Happy_Tests(string args, string expected, int expectedCount)
        {
            var a = new NoteArguments(args.Split(';'), new List<ArgInfo>());
            var note = new Notification();
            var parsed = a.Parse(note);

            var expectedParsed = expected.Split(';').Select(x => x.Split('='));

            Check.That(parsed.Count()).IsEqualTo(expectedCount);
            for (var i = 0; i < parsed.Count(); i++)
            {
                Check.That(parsed.ElementAt(i)).ContainsExactly(expectedParsed.ElementAt(i));
            }
            Check.That(note.IsValid).IsTrue();
        }

        [TestCase("x;y", "Expected a command but got \'x\'.")]
        [TestCase("//x;42", "Expected a command but got \'//x\'.")]
        [TestCase("-", "Expected a command but got \'-\'.")]
        [TestCase("-foo;-y;88;bar", "Expected a command but got \'bar\'.")]
        [TestCase("foo", "Expected a command but got \'foo\'.")]
        [TestCase("-x;y;z", "Expected a command but got \'z\'.")]
        public void Parse_Rainy_Tests(string args, string err)
        {
            var a = new NoteArguments(args.Split(';'), new List<ArgInfo>());
            var note = new Notification();
            var parsed = a.Parse(note);

            Check.That(note.HasErrors).IsTrue();
            Check.That(note.Errors.Count()).IsEqualTo(1);
            Check.That(note.Errors.First()).IsEqualTo(err);
        }

        [Test]
        public void Dictionary_invalid_throws()
        {
            var args = new NoteArguments(new[] {"-x", "42"}, new List<ArgInfo>());
            Check.ThatCode(() => args.Dictionary).Throws<Exception>();
        }
    }
}
