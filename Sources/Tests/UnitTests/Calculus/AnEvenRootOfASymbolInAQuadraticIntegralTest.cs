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
    /// A quadratic with an even root of a symbol among its coefficients is off the real line on
    /// half of that symbol's values, as one with <c>i</c> in it is, and takes the arm there:
    /// <c>1/(b (c + d x)^2 + sqrt(-a))</c> has the discriminant <c>4 sqrt(-a) b d^2</c>, which has no
    /// sign for <c>a &gt; 0</c>, and the answer had no value for any positive <c>a</c>. Rubi's 1.3.1.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AnEvenRootOfASymbolInAQuadraticIntegralTest
    {
        [Theory]
        [InlineData("1/(b*(c + d*x)^2 + sqrt(-a))", "1.3")]
        [InlineData("1/(b*(c + d*x)^2 + sqrt(-a))", "-1.3")]
        [InlineData("1/(x^2 + sqrt(a))", "1.3")]
        [InlineData("1/(x^2 + sqrt(a))", "-1.3")]
        [InlineData("x/(x^2 + x + sqrt(a))", "-1.3")]
        public void DifferentiatesBackOnEitherSideOfTheRoot(string integrand, string a)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Entity Pinned(Entity e) => e.Substitute("a", a.ToEntity()).Substitute("b", 0.7).Substitute("c", 0.4).Substitute("d", 0.3);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -1.7, -0.6, 0.3, 0.8, 1.3, 2.9 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                if (want.IsNaN)
                    continue;
                var got = derivative.Substitute("x", at).EvalNumerical();
                compared++;
                var size = Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart) < 1e-9 * size,
                    $"d/dx of the antiderivative of {integrand} at a = {a} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand} at a = {a}");
        }

        // An odd root is real here, (-8)^(1/3) = -2, and leaves the arms on the sign as they were.
        [Fact]
        public void AnOddRootTakesNoArmOffTheRealLine()
            => Assert.DoesNotContain("in RR", "1/(x^2 + a^(1/3))".ToEntity().Integrate("x").Stringize());
    }
}
