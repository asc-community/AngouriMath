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
    /// The derivative of a sine raised from nothing times powers of the cosine, the secant and sums
    /// in them, or of a cosine beside the sine, alone or beside x by parts: Rubi's 4.2.2.1, 4.5.1.2,
    /// 4.5.4.2 and 4.7.7.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ASineRaisedFromNothingBesidePowersOfTheCosineIntegralTest
    {
        // Points where the cosine is negative too, where the integrand is complex and the answer's
        // principal powers still differentiate to it.
        [Theory]
        [InlineData("(a+a*cos(c+d*x))^(3/2)/cos(c+d*x)^(5/4)")]
        [InlineData("(a+a*cos(c+d*x))^(3/2)*sec(c+d*x)^(5/4)")]
        [InlineData("(a+a*sec(c+d*x))^(3/2)/sec(c+d*x)^(1/4)")]
        [InlineData("(a+a*sec(c+d*x))^n*(-a*A*n-a*Q*(1+n)*sec(c+d*x))/(a*(1+n)*sec(c+d*x)^n)+sec(c+d*x)^(-1-n)*(a+a*sec(c+d*x))^n*(A+Q*sec(c+d*x)^2)")]
        [InlineData("x*(b+a*sin(x))/(a+b*sin(x))^2")]
        [InlineData("x*(b+a*cos(c+d*x))/(a+b*cos(c+d*x))^2")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.3).Substitute("d", 0.75)
                .Substitute("A", 0.6).Substitute("Q", 1.25).Substitute("n", 0.4);
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
        public void IsTheRaisedProductInClosedForm()
        {
            // 4 a^2 sin/(cos^(1/4) sqrt(a + a cos)), Rubi's, with no half angle in it.
            var integral = "(a+a*cos(x))^(3/2)/cos(x)^(5/4)".ToEntity().Integrate("x").Stringize();
            Assert.DoesNotContain("x / 2", integral);
            Assert.Contains("cos(x) ^ (-1/4)", integral);
        }
    }
}
