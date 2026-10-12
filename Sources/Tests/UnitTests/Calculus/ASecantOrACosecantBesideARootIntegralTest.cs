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
    /// A secant or a cosecant beside a root, read as the cosine or the sine the other way up:
    /// <c>sec(x) sqrt(sec(x) + tan(x))</c> is <c>(2 sqrt(sec(x) + tan(x)))'</c>, and
    /// <c>sqrt(1 + tan(x)) csc(x) sec(x)</c> is a function of the tangent. Rubi's 4.7.7.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ASecantOrACosecantBesideARootIntegralTest
    {
        [Theory]
        [InlineData("sec(x)*sqrt(sec(x) + tan(x))", 100)]
        [InlineData("sec(x)*sqrt(sec(x) - tan(x))", 100)]
        [InlineData("csc(x)*sqrt(csc(x) + cot(x))", 100)]
        [InlineData("sec(a + b*x)*(sec(a + b*x) + tan(a + b*x))^c", 200)]
        [InlineData("sqrt(1 + tan(x))*csc(x)*sec(x)", 400)]
        public void DifferentiatesBack(string integrand, int atMost)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < atMost, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", 2).Substitute("b", 3).Substitute("c", "5/4".ToEntity());
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in new[] { "0.35", "0.6", "1.1", "1.4" })
            {
                var want = original.Substitute("x", at.ToEntity()).EvalNumerical();
                var got = derivative.Substitute("x", at.ToEntity()).EvalNumerical();
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }
    }
}
