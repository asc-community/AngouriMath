//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// <c>asech(a + b x)</c> and <c>acsch(a + b x)</c> with symbolic <c>a</c> and <c>b</c> beside
    /// a power of <c>x</c>: by parts leaves <c>1/(x (a + b x)^2 sqrt(1/(a + b x)^2 - 1))</c>,
    /// and five things stood between that and an answer -- the remainder carried three
    /// levels down by taking its constants out, its denominator expanded because the root
    /// beside the polynomial was read as a constant Yun's factorisation could not place,
    /// <c>(a + b x)^2</c> rewritten monic beside a root that kept the other spelling, the
    /// radicand written in two orders, and the even power of a linear below the bar that no
    /// rule wrote apart. Rubi's 7.5.2 and 7.6.2, and 7.5.1's <c>(a + b asech(c x))/(d + e x^2)^2</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class InverseHyperbolicOfALinearIntegralTest
    {
        /// <summary>Where <c>asech(1/3 + x/2)</c> is real: <c>0 &lt; 1/3 + x/2 &lt;= 1</c>.</summary>
        private static readonly double[] Points = { 0.15, 0.4, 0.7, 1.0, 1.25 };

        private static void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            var bare = integral.Substitute("C", 0).Substitute("a", "1/3".ToEntity()).Substitute("b", "1/2".ToEntity())
                .Substitute("c", "3/2".ToEntity()).Substitute("d", "2/3".ToEntity()).Substitute("pe", "5/4".ToEntity());
            var derivative = bare.Differentiate("x");
            var original = integrand.ToEntity().Substitute("a", "1/3".ToEntity()).Substitute("b", "1/2".ToEntity())
                .Substitute("c", "3/2".ToEntity()).Substitute("d", "2/3".ToEntity()).Substitute("pe", "5/4".ToEntity());
            var compared = 0;
            foreach (var at in Points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                if (got.IsNaN || want.IsNaN)
                    continue;
                compared++;
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 4, $"only {compared} of {Points.Length} points were comparable for {integrand}");
        }

        [Theory]
        [InlineData("asech(a+b*x)/x^2")]
        [InlineData("x*asech(a+b*x)")]
        [InlineData("x^3*asech(a+b*x)")]
        [InlineData("acsch(a+b*x)/x^2")]
        [InlineData("x^3*acsch(a+b*x)")]
        public void AnInverseHyperbolicSecantOrCosecantOfALinear(string integrand) => DifferentiatesBack(integrand);

        /// <summary>The remainder by parts leaves, on its own, and Rubi's 7.5.1 row over a quadratic.</summary>
        [Theory]
        [InlineData("1/(x*(a+b*x)^2*sqrt(1/(a+b*x)^2-1))")]
        [InlineData("x*(a+b*asech(c*x))/(d+pe*x^2)^2")]
        public void TheRemainderAndTheQuadraticDenominator(string integrand) => DifferentiatesBack(integrand);
    }
}
