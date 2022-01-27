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
        [InlineData("2x", "x2")]
        [InlineData("x2", "(1/3) * x3")]
        [InlineData("x2 + x", "(1/3) * x3 + (1/2) * x2")]
        [InlineData("x2 - x", "1/3 * x ^ 3 - 1/2 * x ^ 2")]
        [InlineData("a / x", "ln(x) * a")]
        [InlineData("x cos(x)", "cos(x) + sin(x) * x")]
        [InlineData("sin(x)cos(x)", "-1/4 cos(2x)")]
        [InlineData("ln(x)", "x * (ln(x) - 1)")]
        [InlineData("log(a, x)", "x * (ln(x) - 1) / ln(a)")]
        [InlineData("e ^ x", "e ^ x")]
        [InlineData("a ^ x", "a ^ x / ln(a)")]
        [InlineData("sec(a x + b)", "1/2 * ln((1 + sin(a x + b)) / (1 - sin(a x + b))) / a")]
        [InlineData("csc(a x + b)", "ln(tan(1/2(a x + b))) / a")]
        public void TestIndefinite(string initial, string expected)
        {
            Assert.Equal(expected.ToEntity().Simplify(), initial.Integrate("x").Simplify());
        }

        static readonly Entity.Variable x = nameof(x);
#pragma warning disable CS0618 // Type or member is obsolete
        [Fact]
        public void Test1()
        {
            var expr = x;

            Assert.True((MathS.Compute.DefiniteIntegral(expr, x, 0, 1).RealPart - 1.0/2).Abs() < 0.1);
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
