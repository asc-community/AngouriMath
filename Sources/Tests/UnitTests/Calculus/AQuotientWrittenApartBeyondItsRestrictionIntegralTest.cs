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
    /// <c>x^(4k - 1) (a + b arcsec(c x))/sqrt(1 - c^4 x^4)</c> and the arccosecant's, by parts: the
    /// remainder holds a root of <c>1 - 1/(c x)^2</c>, which is written apart as
    /// <c>sqrt((c x)^2 - 1)/|c x|</c>, and what the chain answers for it holds for
    /// <c>|c x| &gt;= 1</c> only. Returned without that condition, the answer was wrong for
    /// <c>|c x| &lt; 1</c>, where the integrand is complex; off it, it is the negative of one, and
    /// <c>sgn((c x)^2 - 1)</c> in front makes it one on both sides. Rubi's 5.5.1 and 5.6.1.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AQuotientWrittenApartBeyondItsRestrictionIntegralTest
    {
        [Theory]
        [InlineData("x^3*(a + b*arcsec(c*x))/sqrt(1 - c^4*x^4)")]
        [InlineData("x^7*(a + b*arcsec(c*x))/sqrt(1 - c^4*x^4)")]
        [InlineData("x^11*(a + b*arcsec(c*x))/sqrt(1 - c^4*x^4)")]
        [InlineData("x^3*(a + b*arccsc(c*x))/sqrt(1 - c^4*x^4)")]
        [InlineData("x^7*(a + b*arccsc(c*x))/sqrt(1 - c^4*x^4)")]
        [InlineData("x^11*(a + b*arccsc(c*x))/sqrt(1 - c^4*x^4)")]
        // And with the root above the bar, which parts leaves the same root of 1 - 1/(c x)^2 beside.
        [InlineData("x^3*(a + b*arcsec(c*x))*sqrt(1 - c^4*x^4)")]
        [InlineData("x^3*(a + b*arccsc(c*x))*sqrt(1 - c^4*x^4)")]
        public void HoldsOnBothSidesOfTheUnitModulus(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.DoesNotContain("provided", text);
            // c = 1/2 puts |c x| = 1 at x = 2, and c = -13/10 at |x| = 10/13, a negative c.
            Compare(integrand, integral, c: 0.5, inside: new[] { -1.7, -0.9, -0.3, 0.4, 1.1, 1.9 }, outside: new[] { -3.3, -2.2, 2.3, 4.1 });
            Compare(integrand, integral, c: -1.3, inside: new[] { -0.7, -0.35, 0.2, 0.6 }, outside: new[] { -2.1, -0.9, 0.85, 1.7 });
        }

        [Theory]
        [InlineData("x^3*(a + b*arcsec(c*x))/sqrt(1 - c^4*x^4)")]
        [InlineData("x^3*(a + b*arccsc(c*x))/sqrt(1 - c^4*x^4)")]
        public void TheIntegrandHasNoValueWhereTheSignTurns(string integrand)
        {
            // At |c x| = 1 the integrand is 0/0, so the domain parts there and a sign that turns
            // there leaves an antiderivative on each side: Rubi's has the same factor,
            // sqrt(1 - c^2 x^2)/(c x sqrt(1 - 1/(c^2 x^2))).
            var original = integrand.ToEntity().Substitute("a", 1.3).Substitute("b", 0.6).Substitute("c", 0.5);
            Assert.True(original.Substitute("x", 2).EvalNumerical().IsNaN);
            Assert.True(original.Substitute("x", -2).EvalNumerical().IsNaN);
        }

        private static void Compare(string integrand, Entity integral, double c, double[] inside, double[] outside)
        {
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.6).Substitute("c", c);
            // Pinned before the differentiation, so that sgn((c x)^2 - 1)' is zero off its roots.
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in inside)
                Assert.True(Math.Abs(c * at) < 1, $"{at} is not inside for c = {c}");
            foreach (var at in outside)
                Assert.True(Math.Abs(c * at) > 1, $"{at} is not outside for c = {c}");
            // In decimals, as the integrator's own checks compare: downcast to rationals, the
            // integrand at c = -1.3, x = 0.2 has no value in the legacy evaluation.
            using var _ = MathS.Settings.DowncastingEnabled.Set(false);
            foreach (var at in inside.Concat(outside))
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                // The integrand is complex on both sides: compared as complex values.
                Assert.False(want.IsNaN, $"the integrand {integrand} has no value at x = {at}, c = {c}");
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, c = {c}, where the integrand is {want}");
            }
        }
    }
}
