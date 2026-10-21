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
    /// A polynomial over a power of a symbolic quadratic beside the root of another, reduced by
    /// one solve over the polynomials in the symbols, and what is left over the quadratic split
    /// over its two roots, Rubi's writing. Rubi's 1.2.1.5:
    /// <c>(a + b x + c x^2)^(3/2)/(d + e x + f x^2)^3</c> was declined, and
    /// <c>(a + b x + c x^2)^(3/2)/(d + e x + f x^2)^2</c> was 140,668 characters.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    /// <remarks>
    /// Differentiated back at points on both sides of the quadratic's roots where they are real,
    /// with a root where the radicand is negative too, and where they are a conjugate pair:
    /// the pieces over the roots hold on either branch of the root of the radicand there.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class AQuadraticOverItsRootsBesideTheRootOfAnotherIntegralTest
    {
        // d + k x + f x^2 and d + b x + c x^2 with the roots -0.22 and -2.28, beside a radicand
        // positive everywhere.
        private static readonly (string, double)[] RealRoots =
            { ("a", 4), ("b", 5), ("c", 2), ("d", 1), ("k", 5), ("f", 2), ("g", 0.7), ("h", 1.3) };
        private static readonly double[] AroundTheRealRoots = { -3, -2.5, -1.5, -1, -0.1, 0.5, 1.5 };

        // The same roots of d + k x + f x^2, the radicand negative at -0.22 and positive at -2.28.
        private static readonly (string, double)[] ARootWhereTheRadicandIsNegative =
            { ("a", -1), ("b", 0.5), ("c", 1), ("d", 1), ("k", 5), ("f", 2), ("g", 0.7), ("h", 1.3) };
        private static readonly double[] WhereTheRadicandIsPositive = { -3, -2.5, -2, -1.5, 1, 1.5, 2.5 };

        // Both quadratics below the bar with a conjugate pair of roots.
        private static readonly (string, double)[] ComplexRoots =
            { ("a", 1), ("b", 1), ("c", 1), ("d", 2), ("k", 1), ("f", 1.5), ("g", 0.7), ("h", 1.3) };
        private static readonly double[] AlongTheLine = { -2, -1, 0, 0.5, 1.5 };

        private static void DifferentiatesBack(string integrand, Entity integral, double[] points, (string, double)[] pins)
        {
            var derivative = integral.Substitute("C", 0).Differentiate("x");
            Entity original = integrand.ToEntity();
            foreach (var (name, value) in pins)
            {
                derivative = derivative.Substitute(name, value);
                original = original.Substitute(name, value);
            }
            var compared = 0;
            foreach (var at in points)
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                if (want.IsNaN)
                    continue;
                compared++;
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 4, $"only {compared} points could be compared for {integrand}");
        }

        [Theory]
        [InlineData("1/((a + b*x + c*x^2)^(1/2)*(d + k*x + f*x^2))", 1000)]
        [InlineData("(g + h*x)/((d + k*x + f*x^2)*sqrt(a + b*x + c*x^2))", 2500)]
        [InlineData("1/((d + b*x + c*x^2)*sqrt(a + b*x + c*x^2))", 1000)]
        [InlineData("1/((a + b*x + c*x^2)^(1/2)*(d + k*x + f*x^2)^2)", 2500)]
        [InlineData("1/((d + b*x + c*x^2)^3*sqrt(a + b*x + c*x^2))", 2000)]
        [InlineData("(a + b*x + c*x^2)^(1/2)/(d + k*x + f*x^2)^2", 1500)]
        [InlineData("(a + b*x + c*x^2)^(3/2)/(d + k*x + f*x^2)^2", 7000)]
        [InlineData("(a + b*x + c*x^2)^(3/2)/(d + k*x + f*x^2)^3", 2500)]
        public void OverTheRootsOfTheQuadratic(string integrand, int longest)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < longest, $"{text.Length} characters of answer for {integrand}");
            DifferentiatesBack(integrand, integral, AroundTheRealRoots, RealRoots);
            DifferentiatesBack(integrand, integral, WhereTheRadicandIsPositive, ARootWhereTheRadicandIsNegative);
            DifferentiatesBack(integrand, integral, AlongTheLine, ComplexRoots);
        }
    }
}
