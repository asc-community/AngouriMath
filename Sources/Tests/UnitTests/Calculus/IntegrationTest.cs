//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using Xunit;
using AngouriMath.Extensions;

namespace AngouriMath.Tests.Calculus
{
    public sealed class IntegrationTest
    {
        // TODO: add more tests
        [Theory]
        [InlineData("2x", "x2 + C")]
        [InlineData("x2", "(1/3) * x3 + C")]
        [InlineData("x2 + x", "(1/3) * x3 + (1/2) * x2 + C")]
        [InlineData("x2 - x", "1/3 * x ^ 3 - 1/2 * x ^ 2 + C")]
        [InlineData("a / x", "ln(abs(x)) * a + C")]
        [InlineData("x cos(x)", "cos(x) + sin(x) * x + C")]
        [InlineData("sin(x)cos(x)", "sin(x) ^ 2 / 2 + C")]
        [InlineData("ln(x)", "x * (ln(x) - 1) + C")]
        [InlineData("log(a, x)", "x * (ln(x) - 1) / ln(a) + C")]
        [InlineData("e ^ x", "e ^ x + C")]
        [InlineData("a ^ x", "a ^ x / ln(a) + C")]
        [InlineData("sec(a x + b)", "1/2 * ln((1 + sin(a x + b)) / (1 - sin(a x + b))) / a + C")]
        [InlineData("csc(a x + b)", "ln(abs(tan(1/2(a x + b)))) / a + C")]
        [InlineData("C", "C x + C_1")]
        [InlineData("C C_1", "C C_1 x + C_2")]
        [InlineData("integral(x, x)", "C_1 + C * x + x ^ 3 / 6")]
        public void TestIndefinite(string initial, string expected)
        {
            Assert.Equal(MathS.Boolean.True, initial.Integrate("x").Equalizes(expected).Simplify());
        }
        [Theory]
        [InlineData("2x * e ^ (x2)", "e ^ (x2) + C")]
        [InlineData("x * e ^ (x2)", "1/2 * e ^ (x2) + C")]
        [InlineData("3 * x2 * e ^ (x3)", "e ^ (x3) + C")]
        public void TestExponentialSubstitution(string initial, string expected)
        {
            var result = initial.Integrate("x").InnerSimplified;
            var expectedResult = expected.ToEntity().InnerSimplified;
            Assert.Equal(MathS.Boolean.True, result.Equalizes(expectedResult).Simplify());
        }

        [Theory]
        [InlineData("sin(x) * cos(x)", "1/2 * sin(x)2 + C")]
        [InlineData("2x * sin(x2)", "-cos(x2) + C")]
        [InlineData("cos(x) / sin(x)", "ln(abs(sin(x))) + C")]
        public void TestTrigonometricSubstitution(string initial, string expected)
        {
            var result = initial.Integrate("x").InnerSimplified;
            var expectedResult = expected.ToEntity().InnerSimplified;
            Assert.Equal(MathS.Boolean.True, result.Equalizes(expectedResult).Simplify());
        }

        [Theory]
        [InlineData("x / (x2 + 1)", "1/2 * ln(abs(x2 + 1)) + C")]
        [InlineData("2x / (x2 + 5)", "ln(abs(x2 + 5)) + C")]
        [InlineData("x / (1 + x2)", "1/2 * ln(abs(1 + x2)) + C")]
        public void TestRationalFunctionSubstitution(string initial, string expected)
        {
            var result = initial.Integrate("x").InnerSimplified;
            var expectedResult = expected.ToEntity().InnerSimplified;
            Assert.Equal(MathS.Boolean.True, result.Equalizes(expectedResult).Simplify());
        }

        [Theory]
        [InlineData("x * (x2 + 1) ^ 3", "C + (x ^ 2 + x ^ 6 + 3/2 * x ^ 4 + x ^ 8 / 4) / 2")]
        [InlineData("3 * x2 * (x3 + 2) ^ 2", "C + 4 * x ^ 3 + 2 * x ^ 6 + x ^ 9 / 3")]
        public void TestPowerSubstitution(string initial, string expected)
        {
            var result = initial.Integrate("x").InnerSimplified;
            var expectedResult = expected.ToEntity().InnerSimplified;
            Assert.Equal(MathS.Boolean.True, result.Equalizes(expectedResult).Simplify());
        }

        [Theory]
        [InlineData("sin(2x + 3)", "-1/2 * cos(2x + 3) + C")]
        [InlineData("cos(3x - 1)", "1/3 * sin(3x - 1) + C")]
        [InlineData("e ^ (2x)", "1/2 * e ^ (2x) + C")]
        public void TestLinearSubstitution(string initial, string expected)
        {
            var result = initial.Integrate("x").InnerSimplified;
            var expectedResult = expected.ToEntity().InnerSimplified;
            Assert.Equal(MathS.Boolean.True, result.Equalizes(expectedResult).Simplify());
        }

        [Theory]
        [InlineData("x / sqrt(x2 + 1)", "sqrt(x2 + 1) + C")]
        [InlineData("x * sqrt(x2 + 1)", "1/3 * (x2 + 1) ^ (3/2) + C")]
        public void TestSquareRootSubstitution(string initial, string expected)
        {
            var result = initial.Integrate("x").InnerSimplified;
            var expectedResult = expected.ToEntity().InnerSimplified;
            Assert.Equal(MathS.Boolean.True, result.Equalizes(expectedResult).Simplify());
        }

        [Theory]
        [InlineData("x2 * e ^ (x3)", "1/3 * e ^ (x3) + C")]
        [InlineData("sin(x) ^ 2 * cos(x)", "1/3 * sin(x) ^ 3 + C")]
        public void TestCompositeSubstitution(string initial, string expected)
        {
            var result = initial.Integrate("x").InnerSimplified;
            var expectedResult = expected.ToEntity().InnerSimplified;
            Assert.Equal(MathS.Boolean.True, result.Equalizes(expectedResult).Simplify());
        }

        [Theory]
        [InlineData("1 / (x * ln(x))", "ln(abs(ln(x))) + C")]
        public void TestLogarithmicSubstitution(string initial, string expected)
        {
            var result = initial.Integrate("x").InnerSimplified;
            var expectedResult = expected.ToEntity().InnerSimplified;
            Assert.Equal(MathS.Boolean.True, result.Equalizes(expectedResult).Simplify());
        }

        [Theory]
        [InlineData("tan(x)", "-ln(abs(cos(x))) + C")]
        [InlineData("sec(x) * tan(x)", "sec(x) + C")]
        public void TestAdvancedTrigSubstitution(string initial, string expected)
        {
            var result = initial.Integrate("x").InnerSimplified;
            var expectedResult = expected.ToEntity().InnerSimplified;
            Assert.Equal(MathS.Boolean.True, result.Equalizes(expectedResult).Simplify());
        }

        [Fact]
        public void TestSubstitutionWithVariable()
        {
            Entity expr = "2 * x * e ^ (x ^ 2)";
            var result = expr.Integrate("x").InnerSimplified;
            var expected = "e ^ (x ^ 2) + C".ToEntity().InnerSimplified;
            Assert.Equal(MathS.Boolean.True, result.Equalizes(expected).Simplify());
        }

        [Fact]
        public void TestSubstitutionDoesNotApplyWhenNotNeeded()
        {
            // Simple polynomial shouldn't use substitution
            Entity expr = "x ^ 2";
            var result = expr.Integrate("x").Simplify();
            Assert.NotNull(result);
            Assert.DoesNotContain("Integral", result.ToString());
        }

        [Fact]
        public void TestSubstitutionWithMultipleTerms()
        {
            // Should split sum first, then apply substitution to each term
            Entity expr = "2 * x * e ^ (x ^ 2) + x ^ 2";
            var result = expr.Integrate("x").Simplify();
            Assert.NotNull(result);
            Assert.DoesNotContain("Integral", result.ToString());
        }

        [Fact]
        public void TestNoInfiniteRecursion()
        {
            // Expression that substitution can't handle shouldn't cause infinite recursion
            Entity expr = "e ^ (e ^ x)";
            var result = expr.Integrate("x");
            // Should either return a valid result or an Integralf node
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("x * (x ^ 2 + 1) ^ 10", "C + (5 * (x^18 + x^4) + x^22 / 11 + x^2 + x^20 + 42 * (x^10 + x^12) + 15 * (x^16 + x^6) + 30 * (x^14 + x^8)) / 2")] // differs from C + (x^2+1)^11 / 22 by -1/22
        [InlineData("x * (x ^ 2 + 1) ^ n", "1/(2(n+1)) * (x^2 + 1) ^ (n+1) + C")]
        public void TestHighPowerSubstitution(string initial, string expected)
        {
            var result = initial.Integrate("x").Simplify();
            var expectedResult = expected.ToEntity().Simplify();
            Assert.Equal(MathS.Boolean.True, result.Equalizes(expectedResult).Simplify());
        }

        [Theory]
        [InlineData("1", "0", "1", "1")]
        [InlineData("x", "0", "1", "1/2")]
        [InlineData("sin(x)", "-1", "1", "0")]
        [InlineData("cos(x)", "0", "pi", "0")]
        [InlineData("1/(x^2+1)", "-oo", "+oo", "pi")]
        public void TestDefinite(string initial, string from, string to, string expected)
        {
            Assert.Equal(expected, initial.Integrate("x", from, to).Simplify().Stringize());
        }

        [Theory] // TODO: Some of these results can be further simplified, e.g. (4 + 2 * x) / 2
        [InlineData("1 / (x^2 + 1)", "arctan(x) + C")] // no linear term
        [InlineData("1 / (x^2 + 4)", "1/2 * arctan(x/2) + C")] // no linear term
        [InlineData("1 / (4*x^2 + 1)", "1/2 * arctan(2*x) + C")] // no linear term
        [InlineData("2 / (x^2 + 1)", "2 * arctan(x) + C")] // no linear term
        [InlineData("1 / (x^2 - 1)", "1/2 * ln(abs((x - 1) / (x + 1))) + C")] // no linear term
        [InlineData("1 / (x^2 - 4)", "ln(abs(1 + (-8) / (4 + 2 * x))) / 4 + C")] // no linear term
        [InlineData("1 / (9 - x^2)", "ln(abs(1 + (-12) / (6 + (-2) * x))) / 6 + C")] // no linear term
        [InlineData("1 / (x^2 + 2*x + 1)", "-1 / (x + 1) + C")] // perfect square
        [InlineData("1 / (x^2 + 4*x + 4)", "(-2) / (2 * x + 4) + C")] // perfect square
        [InlineData("2 / (x^2 - 6*x + 9)", "(-4) / (2 * x - 6) + C")] // perfect square
        [InlineData("1 / (x^2 + 2*x + 2)", "arctan(x + 1) + C")] // complete the square via arctan
        [InlineData("1 / (x^2 + 4*x + 5)", "arctan((4 + 2 * x) / 2) + C")] // complete the square via arctan
        [InlineData("1 / (x^2 - 2*x + 2)", "arctan(x - 1) + C")] // complete the square via arctan
        [InlineData("3 / (x^2 + 6*x + 10)", "3 * arctan((6 + 2 * x) / 2) + C")] // complete the square via arctan
        [InlineData("1 / (x^2 + 2*x - 3)", "ln(abs(1 + (-8) / (6 + 2 * x))) / 4 + C")] // complete the square via log
        [InlineData("1 / (x^2 - 2*x - 3)", "1/4 * ln(abs((x - 3) / (x + 1))) + C")] // complete the square via log
        [InlineData("2 / (x^2 + 4*x - 5)", "1/3 * ln(abs(1 + (-12) / (10 + 2 * x))) + C")] // complete the square via log
        [InlineData("1 / (2*x^2 + 3*x + 1)", "ln(abs(1 + -1/2 / (x + 1))) + C")] // factorable
        [InlineData("1 / (3*x^2 + 5*x + 2)", "ln(abs(1 + -1/3 / (x + 1))) + C")] // factorable
        public void TestQuadraticDenominator(string initial, string expected)
        {
            var result = initial.Integrate("x").InnerSimplified;
            var expectedResult = expected.ToEntity().InnerSimplified;
            Assert.Equal(MathS.Boolean.True, result.Equalizes(expectedResult).Simplify());
        }

        static readonly Entity.Variable x = nameof(x);
#pragma warning disable CS0618 // Type or member is obsolete
        [Fact]
        public void Test1()
        {
            var expr = x;

            Assert.True((MathS.Compute.DefiniteIntegral(expr, x, 0, 1).RealPart - 1.0 / 2).Abs() < 0.1);
        }
        [Fact]
        public void Test2()
        {
            var expr = MathS.Sin(x);
            Assert.Equal(0, MathS.Compute.DefiniteIntegral(expr, x, -1, 1));
        }
        [Fact]
        public void Test3()
        {
            var expr = MathS.Sin(x);
            Assert.True(MathS.Compute.DefiniteIntegral(expr, x, 0, 3).RealPart > 1.5);
        }
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
