using System;
using System.Collections.Generic;
using System.Linq;

namespace ClaParser.CSharp
{
    public class Notification
    {
        private readonly List<string> _errors;

        public IReadOnlyCollection<string> Errors { get { return _errors.AsReadOnly(); } }

        public Notification()
        {
            _errors = new List<string>();
        }

        public void AddError(string message)
        {
            _errors.Add(message);
        }

        public string ErrorMessage
        {
            get { return String.Join(Environment.NewLine, Errors); }
        }

        public bool IsValid { get { return !HasErrors; } }

        public bool HasErrors
        {
            get { return Errors.Any(); }
        }
    }
}