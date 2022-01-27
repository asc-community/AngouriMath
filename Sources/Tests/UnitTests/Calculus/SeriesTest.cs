//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    public sealed class SeriesTest
    {
        [Theory]
        [InlineData("sin(t)", "(((0) + (x)) + ((0) + ((((-1) * ((x) ^ (3))) / ((3)!)) + (0)))) + (((((x) ^ (5)) / ((5)!)) + (0)) + ((((-1) * ((x) ^ (7))) / ((7)!)) + ((0) + (((x) ^ (9)) / ((9)!)))))", 10)]
        [InlineData("cos(t)", "(((1) + (0)) + ((((-1) * ((x) ^ (2))) / ((2)!)) + ((0) + (((x) ^ (4)) / ((4)!))))) + (((0) + (((-1) * ((x) ^ (6))) / ((6)!))) + ((0) + ((((x) ^ (8)) / ((8)!)) + (0))))", 10)]
        [InlineData("tan(t)", "((0) + ((x) + (0))) + ((((2) * ((x) ^ (3))) / ((3)!)) + ((0) + (((16) * ((x) ^ (5))) / ((5)!))))", 6)]
        [InlineData("cotan(1 - t)", "(cotan(1)) + (((((-1) / ((sin(1)) ^ (2))) * (-1)) * (x)) + ((((((-1) * ((((2) * (sin(1))) * ((cos(1)) * (-1))) * (-1))) / (((sin(1)) ^ (2)) ^ (2))) * (-1)) * ((x) ^ (2))) / ((2)!)))", 3)]
        [InlineData("a t3 + b t2 + c t + d", "((d) + ((c) * (x))) + (((((2) * (b)) * ((x) ^ (2))) / ((2)!)) + ((((6) * (a)) * ((x) ^ (3))) / ((3)!)))", 4)]
        public void TaylorDirect(string funcOverTRaw, string expectedRaw, int termCount)
        {
            Entity funcOverT = funcOverTRaw;
            Entity expected = expectedRaw;
            Entity actual = MathS.Series.Taylor(funcOverT, termCount, ("t", "x", "0"));
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("sin(t)", "(((0) + (x)) + ((0) + ((((-1) * ((x) ^ (3))) / ((3)!)) + (0)))) + (((((x) ^ (5)) / ((5)!)) + (0)) + ((((-1) * ((x) ^ (7))) / ((7)!)) + ((0) + (((x) ^ (9)) / ((9)!)))))", 10)]
        [InlineData("cos(t)", "(((1) + (0)) + ((((-1) * ((x) ^ (2))) / ((2)!)) + ((0) + (((x) ^ (4)) / ((4)!))))) + (((0) + (((-1) * ((x) ^ (6))) / ((6)!))) + ((0) + ((((x) ^ (8)) / ((8)!)) + (0))))", 10)]
        [InlineData("tan(t)", "((0) + ((x) + (0))) + ((((2) * ((x) ^ (3))) / ((3)!)) + ((0) + (((16) * ((x) ^ (5))) / ((5)!))))", 6)]
        [InlineData("cotan(1 - t)", "(cotan(1)) + (((((-1) / ((sin(1)) ^ (2))) * (-1)) * (x)) + ((((((-1) * ((((2) * (sin(1))) * ((cos(1)) * (-1))) * (-1))) / (((sin(1)) ^ (2)) ^ (2))) * (-1)) * ((x) ^ (2))) / ((2)!)))", 3)]
        [InlineData("a t3 + b s2 + c u + d", "((d) + ((c) * (z))) + (((((2) * (b)) * ((y) ^ (2))) / ((2)!)) + ((((6) * (a)) * ((x) ^ (3))) / ((3)!)))", 4)]
        [InlineData("(e^t)ln(1 + s)", "(0) + ((y) + ((((2) * ((x) * (y))) + ((-1) * ((y) ^ (2)))) / ((2)!)))", 3)]
        [InlineData("sin(t) + cos(s) + tan(u)", "((1) + ((x) + (z)))", 2)]
        [InlineData("cos(t)sin(s)", "((0) + (y)) + ((0) + ((((-3) * (((x) ^ (2)) * (y))) + ((-1) * ((y) ^ (3)))) / ((3)!)))", 4)]
        [InlineData("sin(t)sin(s)sin(u)sin(v)", "((0) + (0)) + ((0) + ((0) + (((24) * ((((x) * (y)) * (z)) * (w))) / ((4)!))))", 5)]
        public void MultivariableTaylorDirect(string funcOverTRaw, string expectedRaw, int termCount)
        {
            Entity funcOverT = funcOverTRaw;
            Entity expected = expectedRaw;
            Entity actual = MathS.Series.Taylor(funcOverT, termCount, ("t", "x", "0"), ("s", "y", "0"), ("u", "z", "0"), ("v", "w", "0"));
            actual.ShouldBe(expected);
        }

        [Theory]
        [InlineData(0,  6, "sin(x)", -0.5)]
        [InlineData(0,  6, "sin(x)", -0.4)]
        [InlineData(0,  6, "sin(x)", 0.3)]
        [InlineData(0,  6, "sin(x)", 0.2)]
        [InlineData(0,  4, "tan(x)", 0.1)]
        [InlineData(0,  4, "tan(x)", 0.05)]
        [InlineData(0,  4, "tan(x)", -0.1)]
        [InlineData(0,  4, "tan(x)", -0.05)]
        [InlineData(0,  6, "tanh(x)", 0.2)]
        [InlineData(-6, 6, "sin(x)", -6.5)]
        [InlineData(-6, 6, "sin(x)", -6.4)]
        [InlineData(6,  6, "sin(x)", 6.3)]
        [InlineData(6,  6, "sin(x)", 6.2)]
        [InlineData(6,  4, "tan(x)", 6.1)]
        [InlineData(6,  4, "tan(x)", 6.05)]
        [InlineData(-6, 4, "tan(x)", -6.1)]
        [InlineData(-6, 4, "tan(x)", -6.05)]
        [InlineData(6,  4, "tanh(x)", 6.2)]
        [InlineData(0,  4, "sqrt(x - 1) / e ^ (x - 1) + sin(x)", 0.1)]
        [InlineData(0,  6, "1 + x", 1)]
        public void CheckTheCorrectness(double point, int termCount, string func, double pointToCheckAt)
        {
            Entity expr = func;
            var taylor = MathS.Series.Taylor(expr, termCount, ("x", point));

            var expected = expr.Substitute("x", pointToCheckAt).EvalNumerical();
            var actual = taylor.Substitute("x", pointToCheckAt).EvalNumerical();

            var error = (expected - actual).Abs();

            Assert.True(error < 0.05, $"Error is: {error}");
        }

        [Theory]
        [InlineData(new double[3] { 0, 0, 0 }, 4, "sin(x) + cos(y) + tan(z)", new double[3] { 0.5, -0.5, 0.1 })]
        [InlineData(new double[3] { 0, 0, 0 }, 6, "tanh(z)", new double[3] { 99, 99, 0.2 })]
        [InlineData(new double[3] { 0, 0, 0 }, 4, "sqrt(x - 1) / e ^ (y - 1) + sin(z)", new double[3] { 0.1, 0.1, 0.1 })]
        [InlineData(new double[3] { -5, 5, 10 }, 4, "e^((x)(y)(z))", new double[3] { -5.5, 5.5, 9 })]
        [InlineData(new double[3] { 1, 1, 1 }, 6, "(e^x)ln(1 + y)z", new double[3] { 0.5, 0.5, 0.5 })]
        [InlineData(new double[3] { 3, 2, 1 }, 6, "((x)(y^2)(z^3)) / (1 + y)", new double[3] { 3, 1.5, 1.5 })]
        public void CheckMultivariableCorrectness(double[] point, int termCount, string func, double[] pointToCheckAt)
        {
            Entity expr = func;
            var taylor = MathS.Series.Taylor(expr, termCount, ("x", point[0]), ("y", point[1]), ("z", point[2]) );

            var expected = expr.Substitute("x", pointToCheckAt[0]).Substitute("y", pointToCheckAt[1]).Substitute("z", pointToCheckAt[2]).EvalNumerical();
            var actual = taylor.Substitute("x", pointToCheckAt[0]).Substitute("y", pointToCheckAt[1]).Substitute("z", pointToCheckAt[2]).EvalNumerical();

            var error = (expected - actual).Abs();

            Assert.True(error < 0.05, $"Error is: {error}");
        }
    }
}
