//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Linq;
using AngouriMath;
using AngouriMath.Core.Transformations;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Algebra
{
    /// <summary>
    /// An expression written as a single fraction: one numerator over one denominator, nothing
    /// divided inside either, nothing cancelled or multiplied out.
    /// https://github.com/asc-community/AngouriMath/issues/1239
    /// </summary>
    [Trait("Area", "Algebra")]
    public sealed class AsSingleFractionTest
    {
        /// <summary>The printed form is the point of the operation, so it is what these assert.</summary>
        [Theory]
        [InlineData("a + b/c", "(a * c + b) / c")]
        [InlineData("1 + 2/(1+t^2)", "(t ^ 2 + 3) / (t ^ 2 + 1)")]
        [InlineData("1/(t^2+1) + 1/(t+1)", "(2 + t + t ^ 2) / ((t ^ 2 + 1) * (t + 1))")]
        [InlineData("sin(x) + 1/cos(x)", "(1 + cos(x) * sin(x)) / cos(x)")]
        [InlineData("2/3 + x/2", "(4 + 3 * x) / 6")]
        [InlineData("x^(-2) + 1", "(x ^ 2 + 1) / x ^ 2")]
        [InlineData("(a/b)/(c/d)", "a * d / (b * c)")]
        public void WrittenAsOneFraction(string written, string fraction)
            => Assert.Equal(fraction, written.AsSingleFraction().Stringize());

        /// <summary>Nothing cancels, and a function's argument is not gathered.</summary>
        [Theory]
        [InlineData("x/x", "x / x")]
        [InlineData("sin(x/2) + 1/x", "(1 + sin(x / 2) * x) / x")]
        public void NothingIsCancelledAndArgumentsAreLeft(string written, string fraction)
            => Assert.Equal(fraction, written.AsSingleFraction().Stringize());

        /// <summary>With no division in it, or a number, the expression comes back as it was.</summary>
        [Theory]
        [InlineData("x")]
        [InlineData("x^2 + 1")]
        [InlineData("1/2")]
        public void WithNothingToGatherItComesBack(string written)
            => Assert.Equal(written.ToEntity(), written.ToEntity().AsSingleFraction());

        /// <summary>The same value everywhere both are defined.</summary>
        [Theory]
        [InlineData("a + b/c")]
        [InlineData("1/(t^2+1) + 1/(t+1)")]
        [InlineData("sin(x) + 1/cos(x)")]
        [InlineData("2/3 + x/2")]
        public void TheValueIsKept(string written)
        {
            var original = written.ToEntity();
            var fraction = original.AsSingleFraction();
            foreach (var (name, value) in new[] { ("a", 2), ("b", -3), ("c", 5), ("t", 3), ("x", 7) })
            {
                original = original.Substitute(name, value);
                fraction = fraction.Substitute(name, value);
            }
            Assert.Equal(original.EvalNumerical().RealPart.EDecimal.RoundToPrecision(PeterO.Numbers.EContext.ForPrecision(30)),
                fraction.EvalNumerical().RealPart.EDecimal.RoundToPrecision(PeterO.Numbers.EContext.ForPrecision(30)));
        }

        [Fact]
        public void TheTransformationIsTheMethod()
        {
            Entity written = "1 + 2/(1+t^2)";
            Assert.Equal(written.AsSingleFraction(), Transformation.AsSingleFraction.ApplyOrKeep(written));
            Assert.Equal("single-fraction", Transformation.AsSingleFraction.Name);
        }
    }
}
