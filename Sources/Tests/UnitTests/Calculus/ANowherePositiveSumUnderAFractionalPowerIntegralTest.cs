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
    /// A symbolic constant taken out of a fractional power of a sum that is nowhere positive, with
    /// the sum negated: <c>(a - a sec(u)^2)^b</c> is <c>(-a)^b (sec(u)^2 - 1)^b</c> for every real
    /// <c>a</c>, since <c>sec(u)^2 - 1</c> is not negative. Taken out for <c>a &gt; 0</c>, the answer
    /// held only where the integrand is complex and said nothing for the negative <c>a</c> where it
    /// is real. Rubi's 4.5.7.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ANowherePositiveSumUnderAFractionalPowerIntegralTest
    {
        [Theory]
        [InlineData("(a - a*sec(c + d*x)^2)^(7/2)", -1.3)]
        [InlineData("(a - a*sec(c + d*x)^2)^(1/2)", -1.3)]
        [InlineData("1/(a - a*sec(c + d*x)^2)^(1/2)", -1.3)]
        [InlineData("1/(a - a*sec(c + d*x)^2)^(7/2)", -1.3)]
        [InlineData("1/(a - a*sec(c + d*x)^2)^(7/2)", 1.3)]
        [InlineData("(a - a*csc(c + d*x)^2)^(3/2)", -1.3)]
        public void WithTheSumNegated(string integrand, double a)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.DoesNotContain("provided", text);
            Entity Pinned(Entity e) => e.Substitute("a", a).Substitute("c", 0.2).Substitute("d", 0.6);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -0.9, 0.1, 0.3, 0.7, 1.1, 2.0 })
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
    }
}
