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
    /// <c>x^m (alpha sec(u)^p + beta sec(u)^(p - 2))</c> for <c>u = a + b ln(c x^n)</c>, and the same
    /// with cosecants, where the pair is the derivative of <c>x^(m + 1) sec(u)^(p - 2) (b n (p - 2) tan(u) - (m + 1))</c>:
    /// the reduction's remainder vanishing, <c>b^2 n^2 (p - 2)^2 + (m + 1)^2 = 0</c>, or being the
    /// integrand's other term. Rubi's 4.7.5.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class SecantPowersOfALogarithmTwoApartAsTheDerivativeOfAProductIntegralTest
    {
        private static Entity Pinned(Entity e) => e
            .Substitute("a", 0.6).Substitute("b", 0.5).Substitute("c", 1.2)
            .Substitute("n", 1.0).Substitute("m", 0.6).Substitute("p", 3.5);

        // Where the integral is answered, its derivative is the integrand at every point compared.
        private static bool DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            if (text.Contains("integral("))
                return false;
            Assert.True(text.Length < 2000, $"{text.Length} characters of answer for {integrand}");
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { 0.4, 0.9, 1.3, 1.7, 2.2, 2.9 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                if (want.IsNaN)
                    continue;
                compared++;
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-8 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand}");
            return true;
        }

        [Theory]
        [InlineData("-(1+b^2*n^2)*sec(a+b*ln(c*x^n))+2*b^2*n^2*sec(a+b*ln(c*x^n))^3")]
        [InlineData("-(1+b^2*n^2)*csc(a+b*ln(c*x^n))+2*b^2*n^2*csc(a+b*ln(c*x^n))^3")]
        [InlineData("x^m*sec(a+2*ln(c*x^(1/2*sqrt(-(1+m)^2))))^3")]
        [InlineData("x^m*csc(a+2*ln(c*x^(1/2*sqrt(-(1+m)^2))))^3")]
        [InlineData("sec(a+i*ln(c*x^n)/(n*(-2+p)))^p")]
        [InlineData("sec(a-i*ln(c*x^n)/(n*(-2+p)))^p")]
        [InlineData("csc(a+i*ln(c*x^n)/(n*(-2+p)))^p")]
        [InlineData("csc(a-i*ln(c*x^n)/(n*(-2+p)))^p")]
        [InlineData("csc(a+2*ln(c/x^(1/2*i)))^3")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
            => Assert.True(DifferentiatesBack(integrand), $"{integrand} was left unevaluated");

        // The remainder does not vanish: alone, each power is a hypergeometric function of x.
        [Theory]
        [InlineData("sec(a+b*ln(c*x^n))^p")]
        [InlineData("x*sec(a+b*ln(c*x^n))^2")]
        public void IsNotClaimedWhereTheReductionDoesNotClose(string integrand)
            => DifferentiatesBack(integrand);
    }
}
