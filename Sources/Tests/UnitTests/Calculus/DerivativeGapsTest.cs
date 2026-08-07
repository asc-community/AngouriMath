//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// Two ways a derivative could come back unusable: as an unevaluated node, or with a
    /// condition on it that does not belong. Both were found by differentiating
    /// antiderivatives back and checking them at points.
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class DerivativeGapsTest
    {
        [Fact]
        public void SignumHasADerivative() =>
            Assert.Equal("0 provided not x = 0".ToEntity(), "sgn(x)".ToEntity().Differentiate("x"));

        // The antiderivative of abs(x) is sgn(x) * x^2 / 2. Differentiating it back used to
        // produce derivative(sgn(x), x) and stop, so the answer could not be checked or
        // used numerically.
        [Theory]
        [InlineData("abs(x)")]
        [InlineData("abs(x) + x")]
        [InlineData("abs(x + 1)")]
        [InlineData("sgn(x) * x")]
        public void AntiderivativesOfAbsoluteValuesDifferentiateBack(string integrand)
        {
            var f = integrand.ToEntity();
            var derivative = f.Integrate("x").Substitute("C", 0).Differentiate("x");
            foreach (var point in new[] { 0.37, 1.41, 2.71 })
            {
                var expected = f.Substitute("x", point).EvalNumerical().RealPart.EDecimal.ToDouble();
                var actual = derivative.Substitute("x", point).EvalNumerical().RealPart.EDecimal.ToDouble();
                Assert.Equal(expected, actual, 8);
            }
        }

        // A constant exponent that happens to be written as a sum is still constant. Read
        // as non-constant it took the logarithmic rule, which needs a positive base, so
        // this came back undefined for x < 1.
        [Fact]
        public void ConstantExponentWrittenAsASumUsesThePowerRule() =>
            Assert.Equal("(x - 1) ^ 4".ToEntity(),
                "(x - 1) ^ (4 + 1) / (4 + 1)".ToEntity().Differentiate("x").Simplify());

        [Fact]
        public void AntiderivativeOfAPowerDifferentiatesBackEverywhere()
        {
            var f = "(x - 1) ^ 4".ToEntity();
            var derivative = f.Integrate("x").Substitute("C", 0).Differentiate("x");
            // 0.37 is below the base's root, which is where the spurious condition bit.
            foreach (var point in new[] { 0.37, 1.41, 2.71 })
            {
                var expected = f.Substitute("x", point).EvalNumerical().RealPart.EDecimal.ToDouble();
                var actual = derivative.Substitute("x", point).EvalNumerical().RealPart.EDecimal.ToDouble();
                Assert.Equal(expected, actual, 8);
            }
        }

        // A genuinely symbolic exponent must still take the logarithmic rule, condition
        // and all.
        [Theory]
        [InlineData("x ^ n", "x ^ n * n / x provided x > 0")]
        [InlineData("2 ^ x", "ln(2) * 2 ^ x")]
        // Compared as printed text: what is under test is that the logarithmic rule and
        // its condition are still used, and the tree differs from the parsed expectation
        // only in how the condition nests.
        public void SymbolicExponentsAreUnaffected(string input, string expected) =>
            Assert.Equal(expected, input.ToEntity().Differentiate("x").Simplify().Stringize());
    }
}
