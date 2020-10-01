
/* Copyright (c) 2019-2020 Angourisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
 * is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Reflection;

namespace AngouriMath.Core.Exceptions
{
    /// <summary>If one was thrown, the exception is probably not foreseen by AM. Report it is an issue</summary>
    public class AngouriBugException : Exception { public AngouriBugException(string msg) : base(msg + "\n please report about it to the official repository") { } }

    /// <summary>If one is thrown, the user's input is invalid</summary>
    public class MathSException : ArgumentException { public MathSException(string message) : base(message) { } }

    /// <summary>Thrown when an invalid node or combination of nodes in the expression tree is encountered</summary>
    public class TreeException : MathSException { public TreeException(string message) : base(message) { } }

    /// <summary>Thrown when trying to compile and a node cannot be compiled</summary>
    public class UncompilableNodeException : TreeException { public UncompilableNodeException(string message) : base(message) { } }

    /// <summary>Thrown when trying to parse an invalid string</summary>
    public class ParseException : MathSException { public ParseException(string msg) : base(msg) { } }

    /// <summary>Is thrown when something cannot be collapsed into a single number or boolean</summary>
    public class CannotEvalException : MathSException { public CannotEvalException(string msg) : base(msg) { } }

    /// <summary>Thrown when a wrong number of arguments are encountered when parsing a function</summary>
    public class FunctionArgumentCountException : ParseException
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

    /// <summary> Cannot figure out whether the entity is in the set </summary>
    public class ElementInSetAmbiguousException : MathSException { public ElementInSetAmbiguousException(string msg) : base(msg) { } }

    public class FutureReleaseException : NotImplementedException
    {
        private FutureReleaseException(string msg) : base(msg) {}

        internal static Exception Raised(string feature, string plannedVersion)
        {
            var currVersion = Assembly.GetExecutingAssembly().GetName().Version;
            var vers = new Version(plannedVersion);
            if (currVersion > vers)
                return new AngouriBugException($"{feature} was planned for {plannedVersion} but hasn't been released by {currVersion}");
            else
                return new FutureReleaseException($"Feature {feature} will be completed by {plannedVersion}. You are on {currVersion}");
        }
    }
}