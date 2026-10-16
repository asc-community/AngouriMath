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
    /// A rational function times the logarithm of a nested product of powers of linears, by parts,
    /// the logarithm's derivative read off as written: Rubi's 3.2.3
    /// <c>ln(e (f (a + b x)^p (c + d x)^q)^r)</c> alone and over powers of linears.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ARationalFunctionTimesALogarithmOfPowersOfLinearsIntegralTest
    {
        private static void DifferentiatesBack(string integrand, params (string, double)[] pins)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());

            var pinned = integral.Substitute("C", 0);
            Entity original = integrand.ToEntity();
            foreach (var (name, value) in pins)
            {
                pinned = pinned.Substitute(name, value);
                original = original.Substitute(name, value);
            }
            var derivative = pinned.Differentiate("x");
            foreach (var at in new[] { -2.3, -0.7, 0.4, 1.9 })
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(difference / scale < 1e-8,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }

        // d with the sign that keeps the argument real at every sampled x, and |d| below 2/2.3^2
        // so that the inverse sine and cosine stay inside [-1, 1]; off it the exact checks hold too.
        [Theory]
        [InlineData("ln(k*(f*(a+b*x)^p*(c+d*x)^q)^r)")]
        [InlineData("x*ln(k*(f*(a+b*x)^p*(c+d*x)^q)^r)")]
        [InlineData("ln(k*(f*(a+b*x)^p*(c+d*x)^q)^r)/(a+b*x)^5")]
        [InlineData("ln(k*(f*(a+b*x)^p*(c+d*x)^q)^r)/(g+h*x)^3")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            DifferentiatesBack(integrand, ("a", 3), ("b", 0.7), ("c", 1.25), ("d", 0.5), ("f", 1.4), ("k", 2), ("p", 1.5), ("q", -0.3), ("r", 2.5), ("g", 3), ("h", 0.75));
            DifferentiatesBack(integrand, ("a", 2.5), ("b", -0.3), ("c", 4), ("d", 1.5), ("f", 0.5), ("k", 3), ("p", -2), ("q", 1), ("r", -0.5), ("g", 3.5), ("h", 0.2));
        }
    }
}
