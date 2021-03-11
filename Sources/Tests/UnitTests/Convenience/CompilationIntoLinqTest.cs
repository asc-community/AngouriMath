using AngouriMath;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Xunit;

namespace UnitTests.Convenience
{
    public sealed class CompilationIntoLinqTest
    {
        [Theory]

        // Corner
        [InlineData("5", "3")]
        [InlineData("6", "3")]
        [InlineData("pi", "3")]
        [InlineData("e", "3")]

        [InlineData("x", "3")]
        [InlineData("x", "5")]
        [InlineData("x", "5 + i")]

        // Arithmetics & powers
        [InlineData("x + 1", "3")]
        [InlineData("x + 1", "5")]
        [InlineData("x + 1", "5 + i")]
        [InlineData("x - 1", "3")]
        [InlineData("x - 1", "5")]
        [InlineData("x - 1", "5 + i")]
        [InlineData("x * 1", "3")]
        [InlineData("x * 1", "5")]
        [InlineData("x * 1", "5 + i")]
        [InlineData("x / 1", "3")]
        [InlineData("x / 1", "5")]
        [InlineData("x / 1", "5 + i")]
        [InlineData("x ^ 2", "3")]
        [InlineData("x ^ 2", "5")]
        [InlineData("x ^ 2", "5 + i")]
        [InlineData("log(2, x)", "5")]

        [InlineData("1 + x", "3")]
        [InlineData("1 + x", "5")]
        [InlineData("1 + x", "5 + i")]
        [InlineData("1 - x", "3")]
        [InlineData("1 - x", "5")]
        [InlineData("1 - x", "5 + i")]
        [InlineData("1 * x", "3")]
        [InlineData("1 * x", "5")]
        [InlineData("1 * x", "5 + i")]
        [InlineData("1 / x", "3")]
        [InlineData("1 / x", "5")]
        [InlineData("1 / x", "5 + i")]
        [InlineData("2 ^ x", "3")]
        [InlineData("2 ^ x", "5")]
        [InlineData("2 ^ x", "5 + i")]
        [InlineData("log(x, 2)", "5")]

        // Trigonometry
        [InlineData("sin(x)", "1")]
        [InlineData("sin(x + 1)", "1")]
        [InlineData("cos(x)", "1")]
        [InlineData("cos(x + 1)", "1")]
        [InlineData("tan(x)", "1")]
        [InlineData("tan(x + 1)", "1")]
        [InlineData("cotan(x)", "1")]
        [InlineData("cotan(x + 1)", "1")]
        [InlineData("arcsin(x)", "0.3")]
        [InlineData("arcsin(x + 1)", "-0.3")]
        [InlineData("arccos(x)", "1")]
        [InlineData("arccos(x + 1)", "1")]
        [InlineData("arctan(x)", "1")]
        [InlineData("arctan(x + 1)", "1")]
        [InlineData("arccotan(x)", "1")]
        [InlineData("arccotan(x + 1)", "1")]

        // Signs
        [InlineData("abs(x)", "3")]
        [InlineData("abs(x)", "-3")]
        [InlineData("abs(x)", "3 + 4i")]

        [InlineData("sgn(x)", "3")]
        [InlineData("sgn(x)", "-3")]
        [InlineData("sgn(x)", "3 + 4i")]
        public void TestComplexOneArgBasis(string exprRaw, string toSubRaw)
        {
            Entity expr = exprRaw;
            Entity toSub = toSubRaw;


            var expectedRaw = expr.Substitute("x", toSub).EvalNumerical();
            var expected = (Complex)expectedRaw;


            var toSubComplex = (Complex)toSub.EvalNumerical();
            var func = expr.Compile<Complex, Complex>("x");
            var actual = func(toSubComplex);


            var diff = expected - actual;
            var error = Complex.Abs(diff);
            Assert.True(error < 0.1, $"Error: {error}\nExpected: {expected}\nActual: {actual}");
        }

        [Theory]
        [InlineData("x + y", "1")]
        [InlineData("x + y", "2")]
        [InlineData("x - y", "1")]
        [InlineData("x - y", "2")]
        [InlineData("x / y", "1")]
        [InlineData("x / y", "2")]
        [InlineData("x * y", "1")]
        [InlineData("x * y", "2")]
        [InlineData("x ^ y", "1")]
        [InlineData("x ^ y", "2")]
        [InlineData("y + 2x", "1")]
        [InlineData("y^x", "2")]
        [InlineData("sin(x + y) + sin(2x + y) + cos(y x - x)", "2")]
        public void TestComplexTwoArgs(string exprRaw, string toSubRaw)
        {
            Entity expr = exprRaw;
            Entity toSub = toSubRaw;


            var expectedRaw = expr.Substitute("x", toSub).Substitute("y", toSub + 1).EvalNumerical();
            var expected = (Complex)expectedRaw;


            var toSubComplex = (Complex)toSub.EvalNumerical();
            var func = expr.Compile<Complex, Complex, Complex>("x", "y");
            var actual = func(toSubComplex, toSubComplex + 1);


            var diff = expected - actual;
            var error = Complex.Abs(diff);
            Assert.True(error < 0.1, $"Error: {error}\nExpected: {expected}\nActual: {actual}");
        }

        

        [Theory]

        // Corner
        [InlineData("5", "3")]
        [InlineData("6", "3")]
        [InlineData("pi", "3")]
        [InlineData("e", "3")]

        [InlineData("x", "3")]

        // Arithmetics & powers
        [InlineData("x + 1", "3")]
        [InlineData("x + 1", "5")]
        [InlineData("x - 1", "3")]
        [InlineData("x - 1", "5")]
        [InlineData("x * 1", "3")]
        [InlineData("x * 1", "5")]
        [InlineData("x / 1", "3")]
        [InlineData("x / 1", "5")]
        [InlineData("x ^ 2", "3")]
        [InlineData("x ^ 2", "5")]
        [InlineData("log(2, x)", "5")]

        [InlineData("1 + x", "3")]
        [InlineData("1 + x", "5")]
        [InlineData("1 - x", "3")]
        [InlineData("1 - x", "5")]
        [InlineData("1 * x", "3")]
        [InlineData("1 * x", "5")]
        [InlineData("1 / x", "3")]
        [InlineData("1 / x", "5")]
        [InlineData("2 ^ x", "3")]
        [InlineData("2 ^ x", "5")]
        [InlineData("log(x, 2)", "5")]

        // Trigonometry
        [InlineData("sin(x)", "1")]
        [InlineData("sin(x + 1)", "1")]
        [InlineData("cos(x)", "1")]
        [InlineData("cos(x + 1)", "1")]
        [InlineData("tan(x)", "1")]
        [InlineData("tan(x + 1)", "1")]
        [InlineData("cotan(x)", "1")]
        [InlineData("cotan(x + 1)", "1")]
        [InlineData("arcsin(x)", "0.3")]
        [InlineData("arcsin(x + 1)", "-0.3")]
        [InlineData("arccos(x)", "0.3")]
        [InlineData("arccos(x / 2)", "1")]
        [InlineData("arctan(x)", "1")]
        [InlineData("arctan(x / 2)", "1")]
        [InlineData("arccotan(x / 3)", "1")]
        [InlineData("arccotan(x / 2)", "1")]

        // Signs
        [InlineData("abs(x)", "3")]
        [InlineData("abs(x)", "-3")]

        [InlineData("sgn(x)", "3")]
        [InlineData("sgn(x)", "-3")]
        public void TestDoubleOneArg(string exprRaw, string toSubRaw)
        {
            Entity expr = exprRaw;
            Entity toSub = toSubRaw;


            var expectedRaw = expr.Substitute("x", toSub).EvalNumerical();
            var expected = (double)expectedRaw;


            var toSubComplex = (double)toSub.EvalNumerical();
            var func = expr.Compile<double, double>("x");
            var actual = func(toSubComplex);


            var diff = expected - actual;
            var error = Math.Abs(diff);
            Assert.True(error < 0.1, $"Error: {error}\nExpected: {expected}\nActual: {actual}");
        }

        [Theory]
        [InlineData("2 + x + 3 + 5", "3")]
        [InlineData("4 / x", "2")]
        [InlineData("2 ^ x + 3", "5")]
        [InlineData("-x", "5")]
        [InlineData("3 - x", "5")]
        public void TestIntOneArg(string exprRaw, string toSubRaw)
        {
            Entity expr = exprRaw;
            Entity toSub = toSubRaw;


            var expectedRaw = expr.Substitute("x", toSub).EvalNumerical();
            var expected = (int)expectedRaw;


            var toSubComplex = (int)toSub.EvalNumerical();
            var func = expr.Compile<int, int>("x");
            var actual = func(toSubComplex);


            Assert.Equal(expected, actual);
        }
    }
}
