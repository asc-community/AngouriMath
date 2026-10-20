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
    /// A rational function of the tangent beside two roots of linears in it, or one root below the
    /// bar beside a whole power of <c>a + i a tan</c>, integrated in <c>t = tan(g + h x)</c> over the
    /// partial fractions of its rational part with <c>1 + t^2</c> written as <c>(1 + i t)(1 - i t)</c>.
    /// In the root of the linears' quotient <c>1 + t^2</c> is a symbolic quartic, and every one of
    /// these ran past the budget. Rubi's 4.3.4.2 and 4.3.2.1.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ARationalFunctionOfTheTangentBesideRootsOfLinearsInItIntegralTest
    {
        [Theory]
        [InlineData("(c + d*tan(g + h*x))^(5/2)*(A + B*tan(g + h*x) + C*tan(g + h*x)^2)/sqrt(a + b*tan(g + h*x))")]
        [InlineData("(c + d*tan(g + h*x))^(5/2)*(A + B*tan(g + h*x) + C*tan(g + h*x)^2)/(a + b*tan(g + h*x))^(3/2)")]
        [InlineData("(a + b*tan(g + h*x))^(5/2)*(A + B*tan(g + h*x) + C*tan(g + h*x)^2)/(c + d*tan(g + h*x))^(5/2)")]
        [InlineData("(A + B*tan(g + h*x) + C*tan(g + h*x)^2)/((a + b*tan(g + h*x))^(3/2)*(c + d*tan(g + h*x))^(3/2))")]
        [InlineData("(A + B*tan(g + h*x) + C*tan(g + h*x)^2)/((a + b*tan(g + h*x))^(3/2)*(c + d*tan(g + h*x))^(5/2))")]
        [InlineData("1/((a + i*a*tan(g + h*x))^3*(c + d*tan(g + h*x))^(3/2))")]
        [InlineData("1/((a + i*a*tan(g + h*x))^2*(c + d*tan(g + h*x))^(5/2))")]
        [InlineData("1/((a + i*a*tan(g + h*x))^3*(c + d*tan(g + h*x))^(5/2))")]
        public void OverTheLinearsOfOnePlusTheTangentSquared(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 30000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 2.1).Substitute("d", 0.4)
                .Substitute("A", 0.9).Substitute("B", 1.7).Substitute("C", 0.6).Substitute("g", 0.2).Substitute("h", 1.1);
            var derivative = Pinned(integral).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -0.9, -0.5, 0.1, 0.4, 0.7, 1.0 })
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
