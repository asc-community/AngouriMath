using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core.Exceptions
{
    /// <summary>Thrown when trying to parse an invalid string</summary>
    public class ParseException : MathSException { public ParseException(string msg) : base(msg) { } }

    /// <summary>Thrown when an invalid argument passed to a AM's function</summary>
    public sealed class InvalidArgumentParseException : ParseException { public InvalidArgumentParseException(string msg) : base(msg) { } }

    /// <summary>Thrown when a wrong number of arguments are encountered when parsing a function</summary>
    public sealed class FunctionArgumentCountException : ParseException
    {
        private FunctionArgumentCountException(string msg) : base(msg) { }
        private static string CountArguments(int count, bool isAre) =>
            $"{count} argument{(count == 1 ? "" : "s")}{(isAre ? count == 1 ? " is" : " are" : "")}";
        internal static void Assert(string function, int expected, int actual)
        {
            if (expected != actual) throw new FunctionArgumentCountException(
                $"{function} should have exactly {CountArguments(expected, false)} but {CountArguments(actual, true)} provided");
        }
        internal static bool Assert(string function, (int, int) expected, int actual)
        {
            if (expected.Item1 == actual) return true;
            if (expected.Item2 == actual) return false;
            throw new FunctionArgumentCountException(
                $"{function} should have exactly {CountArguments(expected.Item1, false)} or {CountArguments(expected.Item2, false)} but {CountArguments(actual, true)} provided");
        }
    }
}
