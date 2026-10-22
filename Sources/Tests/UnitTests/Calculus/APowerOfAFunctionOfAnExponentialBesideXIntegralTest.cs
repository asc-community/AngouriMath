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
    /// A power that is not whole of a function of one exponential, beside a polynomial in x, by the
    /// ansatz <c>F = W P/(t^m E)</c> with <c>t = e^(s x)</c>: Rubi's 6.7.1
    /// <c>sqrt(csch(x)) (x cosh(x) - 4 sech(x) tanh(x))</c> is
    /// <c>(2 x/sqrt(csch(x)) - 4 sech(x)/csch(x)^(3/2))'</c>, and by parts and the split left terms that are
    /// not elementary apart. The answer keeps the integrand's own power, and holds where <c>sinh(x) &lt; 0</c>
    /// too.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APowerOfAFunctionOfAnExponentialBesideXIntegralTest
    {
        [Theory]
        // Rubi 6.7.1:1243.
        [InlineData("sqrt(csch(x))*(x*cosh(x) - 4*sech(x)*tanh(x))")]
        [InlineData("sqrt(csch(a + x))*((a + x)*cosh(a + x) - 4*sech(a + x)*tanh(a + x))")]
        public void IsAnsweredOnBothSidesOfZero(string integrand)
            => Check(integrand.ToEntity());

        [Fact]
        public void AnswersTheDerivativeOfOneOfTheSameShape()
            // Quadratic in x: (sqrt(csch(x)) (x^2 sinh(x) + x cosh(x)))', whose terms are not elementary apart.
            => Check("sqrt(csch(x))*(x^2*sinh(x) + x*cosh(x))".ToEntity().Differentiate("x"));

        private static void Check(Entity integrand)
        {
            var integral = integrand.Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 2000, $"{integrand} came back in {text.Length} characters");
            Entity Pinned(Entity e) => e.Substitute("a", 0.3);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand);
            foreach (var at in new[] { -2.2, -0.9, 0.4, 1.4, 2.1 })
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
