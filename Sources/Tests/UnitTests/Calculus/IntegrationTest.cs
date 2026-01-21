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
        [InlineData("e^e^x", "integral(e ^ e ^ x, x)")] // don't recurse infinitely
        public void TestIndefinite(string initial, string expected)
        {
            Assert.Equal(MathS.Boolean.True, initial.Integrate("x").EqualTo(expected).Simplify());
        }
        [Theory]
        [InlineData("2x * e ^ (x2)", "e ^ (x2) + C")]
        [InlineData("x * e ^ (x2)", "1/2 * e ^ (x2) + C")]
        [InlineData("3 * x2 * e ^ (x3)", "e ^ (x3) + C")]
        [InlineData("2 * x * e ^ (x ^ 2) + x ^ 2", "e^x^2 + x^3/3 + C")]
        public void TestExponentialSubstitution(string initial, string expected)
        {
            var result = initial.Integrate("x").InnerSimplified;
            var expectedResult = expected.ToEntity().InnerSimplified;
            Assert.Equal(MathS.Boolean.True, result.EqualTo(expectedResult).Simplify());
        }

        [Theory]
        [InlineData("sin(x) * cos(x)", "1/2 * sin(x)2 + C")]
        [InlineData("2x * sin(x2)", "-cos(x2) + C")]
        [InlineData("cos(x) / sin(x)", "ln(abs(sin(x))) + C")]
        public void TestTrigonometricSubstitution(string initial, string expected)
        {
            var result = initial.Integrate("x").InnerSimplified;
            var expectedResult = expected.ToEntity().InnerSimplified;
            Assert.Equal(MathS.Boolean.True, result.EqualTo(expectedResult).Simplify());
        }

        [Theory]
        [InlineData("x / (x2 + 1)", "1/2 * ln(abs(x2 + 1)) + C")]
        [InlineData("2x / (x2 + 5)", "ln(abs(x2 + 5)) + C")]
        [InlineData("x / (1 + x2)", "1/2 * ln(abs(1 + x2)) + C")]
        public void TestRationalFunctionSubstitution(string initial, string expected)
        {
            var result = initial.Integrate("x").InnerSimplified;
            var expectedResult = expected.ToEntity().InnerSimplified;
            Assert.Equal(MathS.Boolean.True, result.EqualTo(expectedResult).Simplify());
        }

        [Theory]
        [InlineData("x * (x2 + 1) ^ 3", "C + (x ^ 2 + x ^ 6 + 3/2 * x ^ 4 + x ^ 8 / 4) / 2")]
        [InlineData("3 * x2 * (x3 + 2) ^ 2", "C + -5/4 * x ^ 6 + (x ^ 3 + 2) * (x ^ 6 / 2 + 2 * x ^ 3)")]
        public void TestPowerSubstitution(string initial, string expected)
        {
            var result = initial.Integrate("x").InnerSimplified;
            var expectedResult = expected.ToEntity().InnerSimplified;
            Assert.Equal(MathS.Boolean.True, result.EqualTo(expectedResult).Simplify());
        }

        [Theory]
        [InlineData("sin(2x + 3)", "-1/2 * cos(2x + 3) + C")]
        [InlineData("cos(3x - 1)", "1/3 * sin(3x - 1) + C")]
        [InlineData("e ^ (2x)", "1/2 * e ^ (2x) + C")]
        public void TestLinearSubstitution(string initial, string expected)
        {
            var result = initial.Integrate("x").InnerSimplified;
            var expectedResult = expected.ToEntity().InnerSimplified;
            Assert.Equal(MathS.Boolean.True, result.EqualTo(expectedResult).Simplify());
        }

        [Theory]
        [InlineData("x / sqrt(x2 + 1)", "sqrt(x2 + 1) + C")]
        [InlineData("x * sqrt(x2 + 1)", "1/3 * (x2 + 1) ^ (3/2) + C")]
        public void TestSquareRootSubstitution(string initial, string expected)
        {
            var result = initial.Integrate("x").InnerSimplified;
            var expectedResult = expected.ToEntity().InnerSimplified;
            Assert.Equal(MathS.Boolean.True, result.EqualTo(expectedResult).Simplify());
        }

        [Theory]
        [InlineData("x2 * e ^ (x3)", "1/3 * e ^ (x3) + C")]
        [InlineData("sin(x) ^ 2 * cos(x)", "1/3 * sin(x) ^ 3 + C")]
        public void TestCompositeSubstitution(string initial, string expected)
        {
            var result = initial.Integrate("x").InnerSimplified;
            var expectedResult = expected.ToEntity().InnerSimplified;
            Assert.Equal(MathS.Boolean.True, result.EqualTo(expectedResult).Simplify());
        }

        [Theory]
        [InlineData("1 / (x * ln(x))", "ln(abs(ln(x))) + C")]
        public void TestLogarithmicSubstitution(string initial, string expected)
        {
            var result = initial.Integrate("x").InnerSimplified;
            var expectedResult = expected.ToEntity().InnerSimplified;
            Assert.Equal(MathS.Boolean.True, result.EqualTo(expectedResult).Simplify());
        }

        [Theory]
        [InlineData("tan(x)", "-ln(abs(cos(x))) + C")]
        [InlineData("sec(x) * tan(x)", "sec(x) + C")]
        public void TestAdvancedTrigSubstitution(string initial, string expected)
        {
            var result = initial.Integrate("x").InnerSimplified;
            var expectedResult = expected.ToEntity().InnerSimplified;
            Assert.Equal(MathS.Boolean.True, result.EqualTo(expectedResult).Simplify());
        }

        [Theory]
        [InlineData("abs(x)", "signum(x) * x^2 / 2 + C")]
        [InlineData("abs(x + 0)", "signum(x) * x^2 / 2 + C")]
        [InlineData("abs(2x)", "signum(2x) * (2x)^2 / 4 + C")]
        [InlineData("abs(x + 3)", "signum(x + 3) * (x + 3)^2 / 2 + C")]
        [InlineData("abs(3x - 1)", "signum(3x - 1) * (3x - 1)^2 / 6 + C")]
        [InlineData("signum(x)", "abs(x) + C")]
        [InlineData("signum(2x)", "abs(2x) / 2 + C")]
        [InlineData("signum(x + 5)", "abs(x + 5) + C")]
        [InlineData("signum(4x - 2)", "abs(4x - 2) / 4 + C")]
        public void TestAbsAndSignumIntegration(string initial, string expected)
        {
            var result = initial.Integrate("x").InnerSimplified;
            var expectedResult = expected.ToEntity().InnerSimplified;
            Assert.Equal(MathS.Boolean.True, result.EqualTo(expectedResult).Simplify());
        }

        [Theory]
        [InlineData("ln(abs(x))", "x * (ln(abs(x)) - 1) + C")] // Direct pattern: x * (ln|x| - 1)
        [InlineData("ln(abs(2x))", "2 * x / 2 * (ln(abs(2 * x)) - 1) + C")] // Direct pattern with linear arg
        [InlineData("ln(abs(x + 3))", "(x + 3) * (ln(abs(x + 3)) - 1) + C")] // Direct pattern with linear arg
        [InlineData("ln(abs(3x - 1))", "(3 * x - 1) / 3 * (ln(abs(3 * x - 1)) - 1) + C")] // Direct pattern with linear arg
        [InlineData("ln(abs(x + 5))", "(x + 5) * (ln(abs(x + 5)) - 1) + C")] // Direct pattern with linear arg
        [InlineData("ln(abs(2x + 4))", "(2 * x + 4) / 2 * (ln(abs(2 * x + 4)) - 1) + C")] // Direct pattern with linear arg
        [InlineData("ln(abs(x)) / x", "integral(ln(abs(x)) / x, x)")] // Unsolvable - logarithmic integral Li(x), not expressible in elementary functions
        [InlineData("ln(abs(x)) * ln(abs(x))", "ln(abs(x)) * x * (ln(abs(x)) - 1) - (x * (ln(abs(x)) - 1) + -x) + C")]
        [InlineData("x * ln(abs(x^2))", "x ^ 2 * (ln(abs(x ^ 2)) - 1) / 2 + C")] // Solvable via u-substitution
        [InlineData("ln(abs(sin(x))) * cos(x)", "sin(x) * (ln(abs(sin(x))) - 1) + C")] // Solvable via u-substitution
        [InlineData("abs(x) * ln(abs(x))", "integral(abs(x) * ln(abs(x)), x)")] // Unsolvable - requires piecewise handling and integration by parts
        [InlineData("signum(x) * ln(abs(x))", "abs(x) * ln(abs(x)) - abs(x) + C")] // Solvable! sgn(x) = x/|x|, reduces to clever substitution
        [InlineData("ln(abs(abs(x)))", "x * (ln(abs(x)) - 1) + C")] // Simplifies abs(abs(x)) → abs(x), then integrates
        [InlineData("abs(ln(abs(x)))", "integral(abs(ln(abs(x))), x)")] // Unsolvable - nested absolute value with logarithm, no elementary form
        public void TestLnAbsIntegration(string initial, string expected)
        {
            var result = initial.Integrate("x").InnerSimplified;
            Assert.Equal(expected, result.Stringize());
        }

        [Theory]
        [InlineData("x * abs(x^2)", "signum(x^2) * (x^2)^2 / 4 + C")]
        [InlineData("2x * abs(x^2 + 1)", "signum(x^2 + 1) * (x^2 + 1)^2 / 2 + C")]
        [InlineData("x * signum(x^2)", "abs(x^2) / 2 + C")]
        [InlineData("2x * signum(x^2 + 3)", "abs(x^2 + 3) + C")]
        [InlineData("cos(x) * abs(sin(x))", "signum(sin(x)) * sin(x)^2 / 2 + C")]
        [InlineData("cos(x) * signum(sin(x))", "abs(sin(x)) + C")]
        [InlineData("sin(x) * abs(cos(x))", "signum(cos(x)) * cos(x)^2 / (-2) + C")]
        [InlineData("sin(x) * signum(cos(x))", "abs(cos(x)) / (-1) + C")]
        public void TestAbsAndSignumWithSubstitution(string initial, string expected)
        {
            var result = initial.Integrate("x").InnerSimplified;
            var expectedResult = expected.ToEntity().InnerSimplified;
            Assert.Equal(MathS.Boolean.True, result.EqualTo(expectedResult).Simplify());
        }

        [Theory]
        [InlineData("x * (x ^ 2 + 1) ^ 10", "C + (5 * (x^18 + x^4) + x^22 / 11 + x^2 + x^20 + 42 * (x^10 + x^12) + 15 * (x^16 + x^6) + 30 * (x^14 + x^8)) / 2")] // differs from C + (x^2+1)^11 / 22 by -1/22
        [InlineData("x * (x ^ 2 + 1) ^ n", "1/(2(n+1)) * (x^2 + 1) ^ (n+1) + C")]
        public void TestHighPowerSubstitution(string initial, string expected)
        {
            var result = initial.Integrate("x").Simplify();
            var expectedResult = expected.ToEntity().Simplify();
            Assert.Equal(MathS.Boolean.True, result.EqualTo(expectedResult).Simplify());
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
            Assert.Equal(MathS.Boolean.True, result.EqualTo(expectedResult).Simplify());
        }

        [Fact]
        public void TestLnAbsSquared()
        {
            // Test that ln(abs(x))^2 can be integrated without stack overflow
            // ∫ln²(abs(x)) dx = x·ln²(abs(x)) - 2x·ln(abs(x)) + 2x + C
            //                 = x(ln²(abs(x)) - 2ln(abs(x)) + 2) + C
            
            var expr = "ln(abs(x)) ^ 2".ToEntity();
            var result = expr.Integrate("x").Simplify(1);
            Assert.Equal("C + x * (ln(abs(x)) ^ 2 - ln(abs(x)) - ln(abs(x))) + 2 * x", result.Stringize());

            // Verify the result by differentiation
            var derivative = result.Differentiate("x"); // TODO: Make this simplify to expr with Simplify()
            foreach (var point in new[] { -3, 7 }) // TODO: Implement and test for "provided x in R" in result
                Assert.Equal(expr.Substitute(x, point).Evaled, derivative.Substitute(x, point).Evaled);
        }

        [Theory(Skip = "TODO: integration by parts multiple times")]
        [InlineData("ln(abs(x)) ^ 3", "C + x * (ln(abs(x)) ^ 3 - ln(abs(x)) ^ 2 - ln(abs(x)) ^ 2 - ln(abs(x)) ^ 2) + 6 * (x * (ln(abs(x)) - 1) + -x)")] // Triple integration by parts
        [InlineData("e^x * sin(x)", "-1/2 * cos(x) * e ^ x + 1/2 * sin(x) * e ^ x + C")] // Classic integration by parts
        [InlineData("e^x * cos(x)", "1/2 * cos(x) * e ^ x + 1/2 * sin(x) * e ^ x + C")] // Classic integration by parts
        [InlineData("arctan(x)", "x * arctan(x) - 1/2 * ln(abs(x ^ 2 + 1)) + C")] // Integration by parts with 1 * arctan(x)
        [InlineData("arcsin(x)", "x * arcsin(x) + sqrt(1 - x ^ 2) + C")] // Integration by parts with 1 * arcsin(x)
        [InlineData("arccos(x)", "x * arccos(x) - sqrt(1 - x ^ 2) + C")] // Integration by parts with 1 * arccos(x)
        public void TestIntegrationByPartsNonPolynomial(string initial, string expected)
        {
            var result = initial.Integrate("x").InnerSimplified;
            var expectedResult = expected.ToEntity().InnerSimplified;
            Assert.Equal(MathS.Boolean.True, result.EqualTo(expectedResult).Simplify());
        }

        [Theory(Skip = "TODO: integration by parts multiple times")]
        [InlineData("sin(ln(abs(x)))", "x / 2 * (sin(ln(abs(x))) - cos(ln(abs(x)))) + C")] // Integration by parts twice
        [InlineData("cos(ln(abs(x)))", "x / 2 * (sin(ln(abs(x))) + cos(ln(abs(x)))) + C")] // Integration by parts twice
        [InlineData("ln(abs(x)) * sin(x)", "-ln(abs(x)) * cos(x) + sin(x) + C")] // Integration by parts
        [InlineData("ln(abs(x)) * cos(x)", "ln(abs(x)) * sin(x) + cos(x) + C")] // Integration by parts
        public void TestIntegrationByPartsMixed(string initial, string expected)
        {
            var result = initial.Integrate("x").InnerSimplified;
            var expectedResult = expected.ToEntity().InnerSimplified;
            Assert.Equal(MathS.Boolean.True, result.EqualTo(expectedResult).Simplify());
        }

        [Theory]
        [InlineData("e^x * x^2", "e ^ x * (x ^ 2 - 2 * (x - 1)) + C")] // Polynomial times exponential (should use recursive polynomial IBP)
        [InlineData("x^3 * sin(x)", "-cos(x) * x ^ 3 + 6 * cos(x) * x + (-6) * sin(x) + 3 * sin(x) * x ^ 2 + C")] // Polynomial times trig (should use recursive polynomial IBP)
        // [InlineData("x * ln(abs(x)) ^ 2", "x ^ 2 / 2 * (ln(abs(x)) ^ 2 - ln(abs(x)) - ln(abs(x))) + x ^ 2 + C")] // TODO: ln(abs(x)) ^ 2 needs integration by parts
        public void TestPolynomialIntegrationByParts(string initial, string expected)
        {
            var result = initial.Integrate("x").InnerSimplified;
            var expectedResult = expected.ToEntity().InnerSimplified;
            Assert.Equal(MathS.Boolean.True, result.EqualTo(expectedResult).Simplify());
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
