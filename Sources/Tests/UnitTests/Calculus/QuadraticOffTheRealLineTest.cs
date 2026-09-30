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
    /// The table integrals over a quadratic <c>Q</c> -- of <c>1/sqrt(Q)</c>, <c>1/Q</c> and
    /// <c>1/Q^n</c> -- chose their form by the sign of the leading coefficient or of the
    /// discriminant, and had no arm for one off the real line.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1598">#1598</a>
    /// </summary>
    /// <remarks>
    /// <c>i &lt; 0</c> and <c>i &gt; 0</c> are each NaN, so <c>1/(x^2 + i)</c> was answered
    /// <c>NaN + C</c>, and with a real sign in front and the rest of the quadratic complex, the
    /// arcsine taken for it differentiated back to minus the integrand on part of the line. Every
    /// integrand here is complex along the whole line, so the check compares complex values.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class QuadraticOffTheRealLineTest
    {
        private static void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var written = integral.Stringize();
            Assert.DoesNotContain("integral(", written);
            Assert.DoesNotContain("NaN", written);

            var derivative = integral.Substitute("C", 0).Differentiate("x")
                .Substitute("a", 1.3).Substitute("c", 0.2).Substitute("d", 0.9);
            var original = integrand.ToEntity()
                .Substitute("a", 1.3).Substitute("c", 0.2).Substitute("d", 0.9);
            var compared = 0;
            foreach (var at in new[] { -2.3, -1.1, -0.4, 0.31, 0.83, 1.7, 2.9 })
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                if (got.IsNaN || want.IsNaN)
                    continue;
                compared++;
                var difference = got - want;
                var distance = Math.Abs((double)difference.RealPart) + Math.Abs((double)difference.ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(distance / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
            Assert.True(compared >= 5,
                $"only {compared} points were comparable for {integrand}");
        }

        /// <summary>A coefficient that is not real, where every arm was a sign and none held.</summary>
        [Theory]
        [InlineData("1/(x^2 + i)")]
        [InlineData("1/(i*x^2 + 1)")]
        [InlineData("1/(x^2 + x + i)")]
        [InlineData("x/(i*x^2 + x + 1)")]
        [InlineData("1/(x^2 + i)^2")]
        [InlineData("1/(x^2 + (1 + i)*x + 1)^2")]
        [InlineData("1/sqrt(i*x^2 + 1)")]
        [InlineData("sqrt(i*x^2 + 1)")]
        public void ANonRealCoefficientIsAnswered(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// With a symbol beside the imaginary unit, the arms were <c>i a &lt; 0</c> and
        /// <c>i a &gt; 0</c>, and neither holds for a real <c>a</c> but zero.
        /// </summary>
        [Theory]
        [InlineData("1/sqrt(i*a*x^2 + 1)")]
        [InlineData("1/(i*a*x^2 + 1)")]
        public void ASymbolBesideTheImaginaryUnitIsAnswered(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A real negative leading coefficient with the rest of the quadratic complex took the
        /// arcsine, which differentiated back to minus the integrand below -2 for the first of
        /// these and on half the points for the others.
        /// </summary>
        [Theory]
        [InlineData("1/sqrt(-x^2 + (1 + i)*x + 1)")]
        [InlineData("1/sqrt(-x^2 + (3 - 2*i)*x - 1)")]
        [InlineData("1/sqrt(-2*x^2 - 5*i*x + 3*i)")]
        [InlineData("sqrt(-x^2 + (1 + i)*x + 1)")]
        public void ARealSignBesideComplexTermsIsAnswered(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Reached through a substitution: the product of the two roots, and Rubi's 4.3.2.1 row
        /// 260, which were answered <c>NaN + C</c> once they came to one of the table integrals.
        /// </summary>
        [Theory]
        [InlineData("1/(sqrt(x)*sqrt(1 + i*x))")]
        [InlineData("sqrt(x)/sqrt(1 + i*x)")]
        [InlineData("sqrt(tan(c + d*x))/(a + i*a*tan(c + d*x))^(5/2)")]
        public void ReachedThroughASubstitution(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A real quadratic keeps its sign arms, and the forms that are real where it is: no arm
        /// for a value off the real line is added where nothing in it is off the real line.
        /// </summary>
        [Theory]
        [InlineData("1/sqrt(a*x^2 + 1)")]
        [InlineData("1/(a*x^2 + x + 1)")]
        [InlineData("1/(a*x^2 + 1)^2")]
        public void ARealQuadraticKeepsItsSignArms(string integrand)
        {
            var written = integrand.ToEntity().Integrate("x").Stringize();
            Assert.DoesNotContain(" in ", written);
            Assert.Contains("> 0", written);
        }
    }
}
