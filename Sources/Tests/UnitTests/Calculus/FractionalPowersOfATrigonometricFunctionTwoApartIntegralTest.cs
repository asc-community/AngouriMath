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
    /// A sum of powers of one sine, cosine, secant or cosecant, not whole and two apart, by the
    /// reduction formula, and beside x by parts once: Rubi's 4.1.10, 4.2.10 and 4.5.4.1, each term
    /// of which is elliptic on its own.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class FractionalPowersOfATrigonometricFunctionTwoApartIntegralTest
    {
        // Points on both sides of the function's zeros: the identities hold for the principal
        // powers where the integrand is complex too.
        [Theory]
        [InlineData("x/csc(c+d*x)^(3/2)-1/3*x*sqrt(csc(c+d*x))")]
        [InlineData("x/csc(c+d*x)^(7/2)-5/21*x*sqrt(csc(c+d*x))")]
        [InlineData("x/sin(c+d*x)^(7/2)+3/5*x*sqrt(sin(c+d*x))")]
        [InlineData("x/sec(x)^(5/2)-3/5*x/sqrt(sec(x))")]
        [InlineData("x/cos(x)^(7/2)+3/5*x*sqrt(cos(x))")]
        [InlineData("(3+3*sec(c+d*x)^2)/sec(c+d*x)^(1/2)")]
        [InlineData("1/csc(x)^(7/2)-5/21*sqrt(csc(x))")]
        [InlineData("(a+b*x)*(cos(x)^(1/3)+2*cos(x)^(-5/3))")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.3).Substitute("d", 0.75);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -2.3, -1.1, -0.7, 0.4, 1.9, 2.8 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                if (want.IsNaN)
                    continue;
                compared++;
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand}");
        }

        [Fact]
        public void IsNotClaimedBesideTheSquareOfX()
        {
            // By parts the remainder is x times the sum's antiderivative, elliptic again.
            var integral = "x^2*(1/sec(x)^(3/2)-1/3*sqrt(sec(x)))".ToEntity().Integrate("x").Stringize();
            Assert.Contains("integral(", integral);
        }
    }
}
