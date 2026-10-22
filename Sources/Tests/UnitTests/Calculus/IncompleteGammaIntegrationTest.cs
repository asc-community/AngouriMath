//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Linq;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// The upper incomplete gamma function of a linear, beside a power of <c>x</c> or a polynomial, by parts: the
    /// remainder <c>Q u^(n - 1) e^(-u)</c> is a sum of derivatives of <c>Gamma(n + k, u)</c>. Rubi's 8.6.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/2075">#2075</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class IncompleteGammaIntegrationTest
    {
        private static Entity DifferentiatesBack(string integrand, double[] points)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Entity Pinned(Entity e) => Pins.Aggregate(e, (current, pin) => current.Substitute(pin.Name, pin.Value.ToEntity()));
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(difference / scale < 1e-9, $"d/dx of {integral} is {got} at x = {at}, where {integrand} is {want}");
            }
            return integral;
        }

        /// <summary>Where every integrand here is real.</summary>
        private static readonly double[] Positive = { 0.35, 0.9, 1.45, 2.3 };

        /// <summary>
        /// And on the cut, at <c>x = -1.7</c>, for a whole order beside a whole power, where no power in the answer
        /// takes the real root that an exact exponent of odd denominator takes on a negative base, against the
        /// principal branch the function takes.
        /// </summary>
        private static readonly double[] AcrossTheCut = { -1.7, 0.35, 0.9, 1.45 };

        private static readonly (string Name, string Value)[] Pins =
            { ("a", "1/3"), ("b", "2/3"), ("c", "5/4"), ("d", "1/2"), ("m", "3/7"), ("n", "2/5") };

        /// <summary>
        /// Without an offset, beside <c>(d x)^m</c> for any <c>m</c> but -1: <c>x^(m + 1) Gamma(n, b x)/(m + 1)</c>
        /// less <c>x^m (b x)^(-m) Gamma(m + n + 1, b x)/(b (m + 1))</c>, where <c>x^m (b x)^(-m)</c> is <c>b^(-m)</c>
        /// for a whole <c>m</c> only.
        /// </summary>
        [Theory]
        [InlineData("gamma(n, 2*x)/x^4")]
        [InlineData("x^m*gamma(n, b*x)")]
        [InlineData("(d*x)^m*gamma(n, b*x)")]
        [InlineData("x^(3/2)*gamma(1/3, 2*x)")]
        [InlineData("sqrt(x)*gamma(n, a*x)")]
        public void BesideAPowerOfAMultipleOfX(string integrand) => DifferentiatesBack(integrand, Positive);

        [Theory]
        [InlineData("gamma(0, a*x)")]
        [InlineData("x^2*gamma(0, a*x)")]
        [InlineData("x*gamma(-1, a*x)")]
        [InlineData("gamma(0, a*x)/x^2")]
        [InlineData("gamma(-2, a*x)/x^3")]
        [InlineData("(c + d*x)*gamma(-1, a + b*x)")]
        public void AWholeOrderBesideAWholePowerAcrossTheCut(string integrand) => DifferentiatesBack(integrand, AcrossTheCut);

        /// <summary>
        /// With an offset, beside a polynomial written in powers of <c>u = a + b x</c>: Rubi's
        /// <c>(c + d x)^m Gamma(n, a + b x)</c>, in its <c>b c - a d</c>.
        /// </summary>
        [Theory]
        [InlineData("gamma(n, a + b*x)")]
        [InlineData("gamma(0, a + b*x)")]
        [InlineData("(c + d*x)^2*gamma(n, a + b*x)")]
        [InlineData("(c + d*x)*gamma(-1, a + b*x)")]
        [InlineData("x^2*gamma(n, a + b*x)")]
        [InlineData("x*(x + 1)*gamma(n, a + b*x)")]
        public void BesideAPolynomialWithAnOffset(string integrand) => DifferentiatesBack(integrand, Positive);

        /// <summary>
        /// As the function it is, rather than expanded: <c>Gamma(100, a x)</c> is a polynomial of degree 99 times
        /// <c>e^(-a x)</c>, and a hundred rounds of parts.
        /// </summary>
        [Fact]
        public void AHighPowerIsAnsweredInClosedForm()
            => Assert.Contains("gamma(100, a * x)", DifferentiatesBack("x^100*gamma(-1, a*x)", Positive).Stringize());

        /// <summary>
        /// Not beside <c>1/x</c>, whose integral is a hypergeometric function, nor beside a power of a different
        /// linear, which Rubi leaves unintegrated too.
        /// </summary>
        [Theory]
        [InlineData("gamma(n, a*x)/x")]
        [InlineData("gamma(n, a + b*x)/(c + d*x)")]
        public void IsDeclinedWhereItIsNotAGammaFunction(string integrand)
            => Assert.Contains("integral(", integrand.ToEntity().Integrate("x").Stringize());
    }
}
