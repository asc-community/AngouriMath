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
    /// The integrals that answer in the hyperbolic sine and cosine integrals: sums of
    /// exponentials of a linear, which is how <c>sinh</c> and <c>cosh</c> arrive, over a power of
    /// a linear.
    /// https://github.com/asc-community/AngouriMath/issues/1501
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class HyperbolicIntegralIntegrationTest
    {
        private static Entity DifferentiatesBack(string integrand, double[] points, params (string Name, string Value)[] pins)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Entity Pinned(Entity e) => pins.Aggregate(e, (current, pin) => current.Substitute(pin.Name, pin.Value.ToEntity()));
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-9, $"d/dx of {integral} is {got} at x = {at}, where {integrand} is {want}");
            }
            return integral;
        }

        /// <summary>Either side of 0 and of every pole below.</summary>
        private static readonly double[] AroundZero = { -1.7, 0.35, 0.9, 1.45 };

        private static readonly (string, string)[] Pins =
            { ("a", "1/3"), ("b", "2/3"), ("c", "5/4"), ("d", "1/2") };

        /// <summary>
        /// Under <c>u = c + d x</c>, each exponential is <c>u^m e^(k u)</c>, and the two of
        /// opposite rates pair: <c>A Ei(k u) + B Ei(-k u) = (A + B) Chi(k u) + (A - B) Shi(k u)</c>.
        /// Rubi's 6.1.1 and 6.2.1, <c>(c + d x)^m sinh(a + b x)^n</c> with <c>m</c> negative, and
        /// 6.7.1's products of powers, whose expansion writes one rate several ways -- and a zero
        /// rate as <c>2 b + 2 (-b)</c>.
        /// </summary>
        [Theory]
        [InlineData("sinh(x)/x")]
        [InlineData("cosh(x)/x")]
        [InlineData("sinh(2*x + 1)/(3*x + 4)")]
        [InlineData("cosh(x)/x^2")]
        [InlineData("x*sinh(x)/(x + 1)^2")]
        [InlineData("sinh(x)^2/x")]
        [InlineData("sinh(x)*cosh(x)/x")]
        [InlineData("cosh(a + b*x)/(c + d*x)")]
        [InlineData("cosh(a + b*x)*sinh(a + b*x)^3/x")]
        [InlineData("sinh(a + b*x)^2*cosh(a + b*x)^2/(c + d*x)")]
        [InlineData("(e^x + 2*e^(3*x))/x")]
        public void ASumOfExponentialsOverAPowerOfALinear(string integrand)
            => DifferentiatesBack(integrand, AroundZero, Pins);

        [Theory]
        [InlineData("sinh(x)/x", "Shi(x)")]
        [InlineData("cosh(x)/x", "Chi(x)")]
        [InlineData("sinh(2*x)/x", "Shi(2 * x)")]
        public void TheDefinitionsAreTheAnswers(string integrand, string expected)
            => Assert.Equal(expected.ToEntity() + "C".ToEntity(), integrand.ToEntity().Integrate("x"));
    }
}
