using System;
using System.Collections.Generic;
using System.Text;
using AngouriMath.Core.Exceptions;

namespace AngouriMath
{
    public class MathSException : SysException
    {
        public MathSException(string message) : base(message)
        {
        }
    }

    public class SolvingException : MathSException
    {
        public SolvingException(string message) : base(message)
        {
        }
    }
}
