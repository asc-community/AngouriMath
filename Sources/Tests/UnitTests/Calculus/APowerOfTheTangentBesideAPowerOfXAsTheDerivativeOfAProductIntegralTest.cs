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
    /// A sum that is the derivative of a power of x times a fractional power of a tangent or a
    /// cotangent: Rubi's 4.3.10 <c>-4 x/(b sqrt(tan(a + b x))) + x^2 sqrt(tan(a + b x)) + x^2/tan(a + b x)^(3/2)</c>
    /// is <c>(-2 x^2/(b sqrt(tan(a + b x))))'</c>, and
    /// <c>x^2/sqrt(tan(a + b x^2)) + sqrt(tan(a + b x^2))/b + x^2 tan(a + b x^2)^(3/2)</c> is
    /// <c>(x sqrt(tan(a + b x^2))/b)'</c>. The tangent's derivative is written over the square of
    /// the cosine, so the bracket of the candidate's derivative did not read beside the
    /// integrand's, and a term with no power of x stopped the second from being read at all; the
    /// first ran past twenty seconds by parts on one of its terms.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APowerOfTheTangentBesideAPowerOfXAsTheDerivativeOfAProductIntegralTest
    {
        [Theory]
        [InlineData("-4*x/(b*sqrt(tan(a + b*x))) + x^2*sqrt(tan(a + b*x)) + x^2/tan(a + b*x)^(3/2)")]
        [InlineData("x^2/sqrt(tan(a + b*x^2)) + sqrt(tan(a + b*x^2))/b + x^2*tan(a + b*x^2)^(3/2)")]
        [InlineData("4*x/(b*sqrt(cot(a + b*x))) + x^2*sqrt(cot(a + b*x)) + x^2/cot(a + b*x)^(3/2)")]
        [InlineData("x^2/sqrt(cot(a + b*x^2)) - sqrt(cot(a + b*x^2))/b + x^2*cot(a + b*x^2)^(3/2)")]
        [InlineData("-4*x/sqrt(tan(x)) + x^2*sqrt(tan(x)) + x^2/tan(x)^(3/2)")]
        [InlineData("x^2/sqrt(tan(1 + 3*x^2)) + sqrt(tan(1 + 3*x^2))/3 + x^2*tan(1 + 3*x^2)^(3/2)")]
        [InlineData("3*x^2*tan(a + b*x)^(1/3) + b*x^3*tan(a + b*x)^(-2/3)/3 + b*x^3*tan(a + b*x)^(4/3)/3")]
        public void IsTheDerivativeOfTheProduct(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 100, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -2.3, -1.2, -0.7, 0.3, 0.8, 1.3, 2.9 })
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
