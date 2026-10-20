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
    /// Two linears whose product is a multiple of the radicand of <c>arsinh(c x)</c>,
    /// <c>(d + i c d x)(f - i c f x) = d f (1 + c^2 x^2)</c>, under powers a whole number apart:
    /// with the root of the radicand 3/2 or more below the bar the whole power above it is
    /// written out as monomials over the root, and with both above the bar the smaller power is
    /// paired. Each ran past the budget. Rubi's 7.1.4.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APairOfLinearsOverARadicandAsMonomialsIntegralTest
    {
        [Theory]
        [InlineData("(a+b*asinh(c*x))*sqrt(f-i*c*f*x)/(d+i*c*d*x)^(3/2)")]
        [InlineData("(f-i*c*f*x)^(3/2)*(a+b*asinh(c*x))/(d+i*c*d*x)^(3/2)")]
        [InlineData("(f-i*c*f*x)^(3/2)*(a+b*asinh(c*x))/(d+i*c*d*x)^(5/2)")]
        [InlineData("(f-i*c*f*x)^(5/2)*(a+b*asinh(c*x))/(d+i*c*d*x)^(3/2)")]
        [InlineData("(f-i*c*f*x)^(5/2)*(a+b*asinh(c*x))/(d+i*c*d*x)^(5/2)")]
        [InlineData("(d+i*c*d*x)^(5/2)*(a+b*asinh(c*x))/(f-i*c*f*x)^(3/2)")]
        [InlineData("(d+i*c*d*x)^(3/2)*(a+b*asinh(c*x))/(f-i*c*f*x)^(3/2)")]
        [InlineData("(a+b*asinh(c*x))*sqrt(d+i*c*d*x)/(f-i*c*f*x)^(3/2)")]
        [InlineData("(d+i*c*d*x)^(5/2)*(a+b*asinh(c*x))/(f-i*c*f*x)^(5/2)")]
        [InlineData("(d+i*c*d*x)^(3/2)*(a+b*asinh(c*x))/(f-i*c*f*x)^(5/2)")]
        [InlineData("(d+i*c*d*x)^(3/2)*(f-i*c*f*x)^(5/2)*(a+b*asinh(c*x))")]
        [InlineData("(d+i*c*d*x)^(3/2)*(f-i*c*f*x)^(5/2)*(a+b*asinh(c*x))^2")]
        public void AnsweredAndDifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Entity Pinned(Entity e) => e.Substitute("a", 0.9).Substitute("b", 1.7).Substitute("c", 0.6).Substitute("d", -1.3).Substitute("f", 0.8);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -1.1, -0.4, 0.3, 0.7, 1.2, 2.1 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                if (want.IsNaN)
                    continue;
                compared++;
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(difference / scale < 1e-9, $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand}");
        }
    }
}
