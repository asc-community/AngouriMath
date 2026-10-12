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
    /// A cosecant of twice the argument beside functions of the argument, written as
    /// <c>1/(2 sin cos)</c>: <c>csc(2x) ln(tan(x))</c> is <c>ln(tan(x))</c> beside its derivative.
    /// Rubi's 4.7.7.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ACosecantOfADoubleArgumentIntegralTest
    {
        [Theory]
        [InlineData("csc(2*x)*ln(tan(x))", 200)]
        [InlineData("csc(2*x)*ln(cot(x))", 200)]
        [InlineData("csc(2*x)*ln(tan(x))^2", 200)]
        [InlineData("csc(2*x)*ln(a*tan(x))", 400)]
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
