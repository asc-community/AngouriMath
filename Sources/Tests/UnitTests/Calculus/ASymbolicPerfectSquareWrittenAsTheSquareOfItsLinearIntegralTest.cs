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
    /// The exponential substitution makes of <c>1/(b cosh(x) + c sinh(x) + sqrt(b^2 - c^2))^4</c> a
    /// power of a quadratic that is a perfect square only as a value, which the Hermite reduction
    /// ground on for twenty seconds before the table answered a piecewise on its discriminant: it is
    /// written as the square of its linear. Rubi's 6.7.1.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ASymbolicPerfectSquareWrittenAsTheSquareOfItsLinearIntegralTest
    {
        private static string AnsweredAndCheckedBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            // b > c, so that the root is real.
            Entity Pinned(Entity e) => e.Substitute("b", 2.7).Substitute("c", 0.4);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -1.9, -0.7, 0.3, 0.8, 1.3, 2.1 })
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
            return text;
        }

        // Over the square of the linear, with no piecewise on a discriminant that is zero.
        [Theory]
        [InlineData("1/(b*cosh(x) + c*sinh(x) + sqrt(b^2 - c^2))^4")]
        [InlineData("1/(b*cosh(x) + c*sinh(x) + sqrt(b^2 - c^2))^3")]
        [InlineData("1/(b*cosh(x) + c*sinh(x) + sqrt(b^2 - c^2))^2")]
        [InlineData("1/(b*cosh(x) + c*sinh(x) - sqrt(b^2 - c^2))^4")]
        [InlineData("256*x^3*((b - c)/(b + c) + 4*sqrt(b^2 - c^2)*x/(2*b + 2*c) + x^2)^(-4)")]
        public void WritesTheSquareOfTheLinear(string integrand)
            => Assert.DoesNotContain("piecewise(", AnsweredAndCheckedBack(integrand));
    }
}
