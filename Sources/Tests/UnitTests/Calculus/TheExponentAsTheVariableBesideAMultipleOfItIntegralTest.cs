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
    /// <c>e^P P'</c> over a power of the quadratic <c>P</c>, under <c>u = P</c>, with a constant multiple
    /// of <c>P</c> read as that multiple of <c>u</c>: taking the leading coefficient's power out from
    /// below the bar hands on <c>e^P P'/(P/c)^2</c>, and <c>e^(a + b x + c x^2) (b + 2 c x)/(a + b x + c x^2)^2</c>
    /// went on to the ansatz and ran out of time. Rubi's 2.3.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class TheExponentAsTheVariableBesideAMultipleOfItIntegralTest
    {
        [Theory]
        [InlineData("e^(a + b*x + c*x^2)*(b + 2*c*x)/(a + b*x + c*x^2)^2")]
        [InlineData("e^(a + b*x + c*x^2)*(b + 2*c*x)/(a + b*x + c*x^2)^3")]
        [InlineData("e^(a + b*x + c*x^2)*(b + 2*c*x)/(a + b*x + c*x^2)^4")]
        [InlineData("e^(1 + x + 3*x^2)*(1 + 6*x)/(1 + x + 3*x^2)^2")]
        public void APowerOfTheExponentBelowTheBar(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 20000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", -1.3).Substitute("b", 0.7).Substitute("c", 0.4);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -2.7, -1.2, -0.5, 0.3, 0.8, 1.7, 2.9 })
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
