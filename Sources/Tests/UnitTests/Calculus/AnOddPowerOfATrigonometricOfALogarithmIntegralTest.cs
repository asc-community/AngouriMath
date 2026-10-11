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
    /// A power of <c>x</c> times a whole power of a sine or a cosine of <c>a + b ln(k x^n)</c>, which
    /// in the logarithm is an exponential times a power of a sine or a cosine. Rubi's 4.7.5.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AnOddPowerOfATrigonometricOfALogarithmIntegralTest
    {
        [Theory]
        [InlineData("sin(ln(x))^3", 500)]
        [InlineData("sin(a + b*ln(x))^3", 2000)]
        [InlineData("x^2*sin(a + b*ln(k*x^n))^3", 3000)]
        [InlineData("sin(a + b*ln(k*x^n))^3/x^3", 3000)]
        [InlineData("x^m*cos(a + b*ln(k*x^n))^3", 3000)]
        [InlineData("sin(a + b*ln(k*x^n))^4/x^2", 4000)]
        public void DifferentiatesBack(string integrand, int atMostCharacters)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.True(integral.Stringize().Length <= atMostCharacters, $"{integral.Stringize().Length} characters");
            Entity Pinned(Entity e) => e.Substitute("a", "1/4".ToEntity()).Substitute("b", "3/2".ToEntity())
                .Substitute("k", 2).Substitute("n", 3).Substitute("m", "5/2".ToEntity());
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
