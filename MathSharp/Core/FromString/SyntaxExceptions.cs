using System;
using System.Collections.Generic;
using System.Text;

namespace MathSharp.Core.FromString
{
    public abstract class SyntaxException : Exception
    {
        protected SyntaxException(string msg) : base(msg) { }
    }
    public class ParseException : SyntaxException
    {
        public ParseException(string msg) : base(msg) { }
    }
    public class LexicException : SyntaxException
    {
        public LexicException(string msg) : base(msg) { }
    }
}
