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
    /// A polynomial over a symbolic quartic that splits into two quadratics in a hidden variable: a
    /// palindromic one over <c>x + 1/x</c>, and one even after a shift. Rubi's 1.3.1
    /// <c>(A + B x + C x^2 + D x^3)/(a + b x + c x^2 + b x^3 + a x^4)</c> and
    /// <c>1/(4 a c + 4 c^2 x^2 + 4 c d x^3 + d^2 x^4)</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ASymbolicQuarticThatSplitsIntoTwoQuadraticsIntegralTest
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
        [InlineData("(A+B*x+G*x^2+D*x^3)/(a+b*x+c*x^2+b*x^3+a*x^4)")]
        [InlineData("1/(a+b*x+c*x^2+b*x^3+a*x^4)")]
        [InlineData("1/(4*a*c+4*c^2*x^2+4*c*d*x^3+d^2*x^4)")]
        [InlineData("x/(4*a*c+4*c^2*x^2+4*c*d*x^3+d^2*x^4)")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x").Stringize();
            // The factored quartic asked below is palindromic again; read as a fresh one, the answer
            // ran to 803,488 characters.
            Assert.True(integral.Length < 10_000, $"{integral.Length} characters");
            DifferentiatesBack(integrand, ("A", 0.6), ("B", 1.25), ("G", 0.3), ("D", 0.75), ("a", 1.4), ("b", 0.5), ("c", 3), ("d", 2));
            DifferentiatesBack(integrand, ("A", -1.5), ("B", 0.25), ("G", -0.4), ("D", -2), ("a", 0.5), ("b", -3), ("c", -1), ("d", -0.3));
        }
    }
}
