using System;
using System.Collections.Generic;
using System.Text;

namespace MathSharp
{
    public class MathSException : Exception
    {
        public MathSException(string message) : base(message)
        {
        }
    }
}
