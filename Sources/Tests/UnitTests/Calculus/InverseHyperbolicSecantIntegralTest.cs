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
using static AngouriMath.Entity;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// <c>acsch(c x)</c> and <c>asech(c x)</c> with a symbolic <c>c</c>: the parser spells them
    /// <c>ln(1/(c x) + sqrt(1/(c x)^2 ± 1))</c>, and three things stood between that and an
    /// answer -- <c>(c x)^2</c> read as a power of something that is not a polynomial, the
    /// root of <c>(1 ± c^2 x^2)/(c^2 x^2)</c> not written apart because <c>c^2</c> was not a
    /// number, and <c>|c|</c> differentiated to <c>derivative(|c|, x)</c> by parts -- where the
    /// same rows with <c>c = 2</c> were answered. Rubi's 7.5.1 and 7.6.1.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class InverseHyperbolicSecantIntegralTest
    {
        /// <summary>Where <c>asech(3 x / 2)</c> is real, <c>0 &lt; c x &lt;= 1</c>, and <c>acsch</c> too.</summary>
        private static readonly double[] Points = { 0.15, 0.25, 0.4, 0.55, 0.62 };

        private static void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            // A condition on a parameter, `c^2 > 0`, holds at the c the rows are checked at.
            var bare = integral.Substitute("C", 0).Substitute("c", "3/2".ToEntity());
            var derivative = bare.Differentiate("x");
            var original = integrand.ToEntity().Substitute("c", "3/2".ToEntity());
            var compared = 0;
            foreach (var at in Points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                if (got.IsNaN || want.IsNaN)
                    continue;
                compared++;
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 4, $"only {compared} of {Points.Length} points were comparable for {integrand}");
        }

        [Theory]
        [InlineData("x*acsch(c*x)")]
        [InlineData("x^2*acsch(c*x)")]
        [InlineData("x^2*asech(c*x)")]
        [InlineData("x/sqrt(1 + 1/(c*x)^2)")]
        public void AnInverseHyperbolicSecantOrCosecantWithASymbolicCoefficient(string integrand) => DifferentiatesBack(integrand);

        [Fact]
        public void TheAnswerSaysTheParameterIsReal()
        {
            // c^2 > 0 is exactly "c is real and not zero", which is what taking sqrt(c^2 x^2)
            // for |c| |x| assumed.
            var integral = "x/sqrt(1 + 1/(c*x)^2)".ToEntity().Integrate("x");
            Assert.Contains(integral.Nodes, node => node is Providedf(_, var predicate) && predicate.Nodes.Any(inner => inner == "c^2 > 0".ToEntity()));
        }
    }
}
