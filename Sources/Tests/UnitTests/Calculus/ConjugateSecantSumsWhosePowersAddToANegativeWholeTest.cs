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
    /// <c>sec(y) (a + a sec(y))^m (c - c sec(y))^n</c> with <c>m + n</c> a negative whole number,
    /// for any <c>m</c>: Rubi's 4.5.2.3.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ConjugateSecantSumsWhosePowersAddToANegativeWholeTest
    {
        // The two sums multiply to -a c tan(y)^2, so they have one sign only where a and c do not:
        // with a = 2 and c = -3 the first rows are real where sec(y) > 1. The rest are complex
        // there, and are checked off the real line the same way.
        [Theory]
        [InlineData("sec(e+f*x)*(a+a*sec(e+f*x))^m*(c-c*sec(e+f*x))^(-1-m)", 300, true)]
        [InlineData("sec(e+f*x)*(a+a*sec(e+f*x))^m*(c-c*sec(e+f*x))^(-2-m)", 400, true)]
        [InlineData("sec(e+f*x)*(a+a*sec(e+f*x))^m*(c-c*sec(e+f*x))^(-3-m)", 500, true)]
        [InlineData("sec(x)*(2+2*sec(x))^(5/3)*(3-3*sec(x))^(-8/3)", 200, false)]
        [InlineData("sec(x)*(1-sec(x))^m*(1+sec(x))^(-1-m)", 200, false)]
        [InlineData("csc(x)*(2+2*csc(x))^m*(3-3*csc(x))^(-2-m)", 300, false)]
        public void DifferentiatesBack(string integrand, int atMost, bool real)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < atMost, $"{text.Length} characters of answer for {integrand}");
            Entity At(Entity expression) => expression.Substitute("C", 0).Substitute("m", "1/3".ToEntity())
                .Substitute("a", 2).Substitute("c", -3).Substitute("e", "1/5".ToEntity()).Substitute("f", "3/2".ToEntity());
            var derivative = At(integral).Differentiate("x");
            var original = At(integrand.ToEntity());
            // Points where sec (or csc) of the argument is past 1: x and 1/5 + 3/2 x in (0, pi/2).
            foreach (var at in integrand.StartsWith("csc") ? new[] { "1.1", "1.3", "1.5" } : new[] { "0.3", "0.6", "0.9" })
            {
                var want = original.Substitute("x", at.ToEntity()).EvalNumerical();
                if (real)
                    Assert.True(Math.Abs((double)want.ImaginaryPart) < 1e-12, $"{integrand} is not real at x = {at}");
                var got = derivative.Substitute("x", at.ToEntity()).EvalNumerical();
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }
    }
}
