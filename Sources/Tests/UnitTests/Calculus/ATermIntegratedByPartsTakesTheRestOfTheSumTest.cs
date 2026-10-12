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
    /// A sum whose terms are not elementary apart, integrated by parts against the power of
    /// <c>x</c> in a term, with what parts leaves asked together with the other terms:
    /// <c>sqrt(csc(x)) (x cos(x) - 4 sec(x) tan(x))</c> is
    /// <c>(2 x sqrt(sin(x)) - 4 sin(x)^(3/2)/cos(x))'</c>. Rubi's 4.7.7.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ATermIntegratedByPartsTakesTheRestOfTheSumTest
    {
        [Theory]
        [InlineData("sqrt(csc(x))*(x*cos(x) - 4*sec(x)*tan(x))", 100, "0.2 0.6 0.9 2.3")]
        [InlineData("x*cos(x)/sqrt(sin(x)) - 4*sqrt(sin(x))/cos(x)^2", 100, "0.2 0.6 0.9 2.3")]
        [InlineData("x*sin(x)/sqrt(cos(x)) + 4*sqrt(cos(x))/sin(x)^2", 100, "0.2 0.6 0.9 1.3")]
        [InlineData("sqrt(csc(2*x + 1))*(x*cos(2*x + 1) - 2*sec(2*x + 1)*tan(2*x + 1))", 150, "0.2 0.35 0.6 0.9")]
        [InlineData("x*cos(x)*e^sin(x) + e^sin(x)", 50, "0.35 1.1 2.3 4.4")]
        [InlineData("x^2*cos(x)*e^sin(x) + 2*x*e^sin(x)", 50, "0.35 1.1 2.3 4.4")]
        public void DifferentiatesBack(string integrand, int atMost, string points)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < atMost, $"{text.Length} characters of answer for {integrand}");
            var derivative = integral.Substitute("C", 0).Differentiate("x");
            var original = integrand.ToEntity();
            foreach (var at in points.Split(' '))
            {
                var want = original.Substitute("x", at.ToEntity()).EvalNumerical();
                Assert.True(Math.Abs((double)want.ImaginaryPart) < 1e-12, $"{integrand} is not real at x = {at}");
                var got = derivative.Substitute("x", at.ToEntity()).EvalNumerical();
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }

        /// <summary>
        /// What parts leaves is <c>-2 sqrt(sin(x))</c>, and beside <c>3 sqrt(sin(x))</c> it is
        /// <c>sqrt(sin(x))</c>, which is not elementary: declined, not answered.
        /// </summary>
        [Fact]
        public void ARemainderThatIsNotElementaryIsDeclined()
            => Assert.Contains("integral(", "x*cos(x)/sqrt(sin(x)) + 3*sqrt(sin(x))".ToEntity().Integrate("x").Stringize());
    }
}
