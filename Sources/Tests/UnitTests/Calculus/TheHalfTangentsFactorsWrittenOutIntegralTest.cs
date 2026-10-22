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
    /// Under <c>u = tanh(x/2)</c> the parts of an integrand are homogenised apart, and a factor they
    /// share was left hidden inside the expanded quartic of one of them: Rubi's 6.7.1
    /// <c>sech(x)^2/(a + c sech(x) + b tanh(x))</c> is <c>(1 + u^2)^2</c> over <c>(1 + u^2)^2</c> times a
    /// quartic holding <c>1 + u^2</c> once more, and the rational integrator wrote 1.38 million
    /// characters in twenty-two seconds, past the corpus's budget. Written out, it is
    /// <c>1/((1 + u^2) Q(u))</c> for a quadratic Q.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class TheHalfTangentsFactorsWrittenOutIntegralTest
    {
        [Theory]
        // Rubi 6.7.1:910.
        [InlineData("sech(x)^2/(a + c*sech(x) + b*tanh(x))")]
        [InlineData("csch(x)^2/(a + c*csch(x) + b*coth(x))")]
        public void IsAnsweredShortly(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 5000, $"{integrand} came back in {text.Length} characters");
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in new[] { -2.2, -0.9, 0.3, 1.4, 2.1 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                Assert.False(got.IsNaN, $"d/dx of the antiderivative of {integrand} has no value at x = {at}, where the integrand is {want}");
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }
    }
}
