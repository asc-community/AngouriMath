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
    /// An exponential of something of <c>1/L</c>, <c>L = c + d x</c>, beside a whole power of another
    /// linear <c>M</c>. Under <c>u = 1/L</c>, <c>M</c> is <c>((d p - q c) u + q)/d</c> times <c>L</c>,
    /// so the question in <c>u</c> is an exponential beside a rational function, which the
    /// exponential integral and the moments answer. They were declined, since only powers of
    /// <c>L</c> itself were read. Rubi's 2.3.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AnExponentialInTheReciprocalOfALinearBesideAnotherLinearIntegralTest
    {
        [Theory]
        [InlineData("f^(c/(a + b*x))*x^2", 1.1)]
        [InlineData("f^(c/(a + b*x))/x", 1.1)]
        [InlineData("f^(c/(a + b*x)^2)*x", 1.1)]
        [InlineData("f^(m + k/(c + d*x))/(g + h*x)^2", 1.1)]
        [InlineData("e^(k/(c + d*x))*(a + b*x)^2", 1.1)]
        [InlineData("e^(k/(c + d*x))/(a + b*x)", 1.1)]
        [InlineData("e^(k/(c + d*x))/(a + b*x)", -0.9)]
        [InlineData("e^(k/(c + d*x)^2)*(a + b*x)", -0.9)]
        [InlineData("f^(m + k*(a + b*x)/(c + d*x))/(g + h*x)", 1.1)]
        [InlineData("e^(1/x)/(x + 1)", 1.1)]
        public void WrittenInTheReciprocal(string integrand, double k)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 5000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4).Substitute("d", 0.3)
                .Substitute("f", 2.3).Substitute("g", 0.6).Substitute("h", 1.9).Substitute("m", 0.5).Substitute("k", k);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -0.9, -0.4, 0.3, 0.8, 1.3, 2.9 })
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
