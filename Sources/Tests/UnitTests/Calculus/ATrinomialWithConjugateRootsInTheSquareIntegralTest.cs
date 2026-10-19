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
    /// A rational function over <c>a + b x^2 + c x^4</c> beside a root of <c>d + k x^2</c>, taken
    /// apart over the trinomial's roots in x^2 whether they are real or a conjugate pair. Over the
    /// two real quadratics a conjugate pair also factors into, the answer to
    /// <c>x^2 sqrt(d + k x^2)/(a + b x^2 + c x^4)</c> was 29,139 characters. Rubi's 1.2.2.4.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1885">#1885</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ATrinomialWithConjugateRootsInTheSquareIntegralTest
    {
        [Theory]
        // b^2 < 4 a c: the roots in x^2 are a conjugate pair.
        [InlineData("x^2*sqrt(d + k*x^2)/(a + b*x^2 + c*x^4)", 2, 1, 3)]
        [InlineData("sqrt(d + k*x^2)/(a + b*x^2 + c*x^4)", 2, 1, 3)]
        [InlineData("x^4/((a + b*x^2 + c*x^4)*sqrt(d + k*x^2))", 2, -1, 3)]
        [InlineData("(d + k*x^2)^(3/2)/(a + b*x^2 + c*x^4)", 2, 1, 3)]
        // b^2 > 4 a c: two real roots, the same writing.
        [InlineData("x^2*sqrt(d + k*x^2)/(a + b*x^2 + c*x^4)", 1, 5, 2)]
        public void OverItsRootsInTheSquare(string integrand, double a, double b, double c)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 5000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", a).Substitute("b", b).Substitute("c", c).Substitute("d", 1.9).Substitute("k", 0.7);
            var antiderivative = Pinned(integral.Substitute("C", 0));
            var derivative = antiderivative.Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in new[] { -1.5, -0.7, 0.1, 0.57, 1.4, 2.9 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
                // And the antiderivative itself is real there: the two conjugate pieces add to one.
                var value = antiderivative.Substitute("x", at).EvalNumerical();
                Assert.True(Math.Abs((double)value.ImaginaryPart) < 1e-9 * Math.Max(1, Math.Abs((double)value.RealPart)),
                    $"the antiderivative of {integrand} is {value} at x = {at}");
            }
        }
    }
}
