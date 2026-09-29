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
    /// A rational function beside the square root of a quadratic with another quadratic below
    /// the bar: <c>(g + h x)/(A sqrt(B))</c> in closed form, and what the partial fractions take
    /// apart into it. Rubi's 1.2.1.6, and its 4.3.9 and 4.4.9, which the tangent brings to it
    /// with <c>A = 1 + t^2</c>, all with symbols for coefficients, which is what the rotation of
    /// the tangent could not take.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// Checked by differentiating back with the symbols pinned, at points where the integrand is
    /// real, never against a printed form: the answers are arctangents of a linear over the root
    /// with a nested root in their coefficients, whose shape says nothing about whether they
    /// differentiate back.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class QuadraticBesideARootIntegralTest
    {
        private static readonly (string, double)[] Pins =
            { ("a", 2), ("b", 1), ("c", 3), ("d", 5), ("k", 1), ("f", 2), ("g", 0.7), ("h", 1.3) };

        private static void DifferentiatesBackPinned(string integrand, double[] points, params (string, double)[] pins)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());

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
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                if (got.IsNaN || want.IsNaN)
                    continue;
                compared++;
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
            Assert.True(compared >= 3, $"only {compared} points could be compared for {integrand}");
        }

        /// <summary>
        /// A linear over a quadratic beside the root of another: two arctangents of a linear
        /// over the root, whose coefficients hold <c>sqrt(P^2 - E F)</c>.
        /// </summary>
        [Theory]
        [InlineData("1/((1 + x^2)*sqrt(a + b*x + c*x^2))")]
        [InlineData("x/((1 + x^2)*sqrt(a + b*x + c*x^2))")]
        [InlineData("(g + h*x)/((d + k*x + f*x^2)*sqrt(a + b*x + c*x^2))")]
        [InlineData("(g + h*x)/((d - f*x^2)*sqrt(a + b*x + c*x^2))")]
        public void ALinearOverAQuadratic(string integrand)
            => DifferentiatesBackPinned(integrand, new[] { 0.3, 0.7, 1.2, -0.5 }, Pins);

        /// <summary>
        /// Under the tangent: <c>1/sqrt(a + b tan(x) + c tan(x)^2)</c> is
        /// <c>1/((1 + t^2) sqrt(a + b t + c t^2))</c>, and the powers of the tangent and the
        /// cotangent beside it leave a polynomial part, a power of <c>t</c> below the bar, or a
        /// power of the root's own quadratic, each closed beside the root.
        /// </summary>
        [Theory]
        [InlineData("1/sqrt(a + b*tan(x) + c*tan(x)^2)")]
        [InlineData("tan(x)^5*sqrt(a + b*tan(x) + c*tan(x)^2)")]
        [InlineData("cot(x)^3*sqrt(a + b*tan(x) + c*tan(x)^2)")]
        [InlineData("cot(x)^2/sqrt(a + b*tan(x) + c*tan(x)^2)")]
        [InlineData("cot(x)^3/(a + b*tan(x) + c*tan(x)^2)^(3/2)")]
        [InlineData("cot(x)^5/sqrt(a + b*cot(x) + c*cot(x)^2)")]
        public void UnderTheTangent(string integrand)
            => DifferentiatesBackPinned(integrand, new[] { 0.3, 0.7, 1.1, 1.4 }, Pins);

        /// <summary>Rubi's 1.2.1.6: a polynomial part beside the linear over the quadratic.</summary>
        [Theory]
        [InlineData("x^2*(a + c*x^2)^(3/2)/(d + k*x + f*x^2)")]
        [InlineData("x*(a + b*x + c*x^2)^(3/2)/(d - f*x^2)")]
        [InlineData("x^2/((a + b*x + c*x^2)^(1/2)*(d + k*x + f*x^2))")]
        public void APolynomialPartBeside(string integrand)
            => DifferentiatesBackPinned(integrand, new[] { 0.3, 0.7, 1.2, -0.5 }, Pins);

        /// <summary>
        /// Over a power of the quadratic, a power at a time: the derivative of
        /// <c>(p + q x) sqrt(B)/A^(k - 1)</c> takes one away, and what it leaves goes down to the
        /// closed form over the first. The cube is numeric: with six symbols its coefficients
        /// grow past the bound the reduction declines at.
        /// </summary>
        [Theory]
        [InlineData("1/((1 + x^2)^2*sqrt(1 - x^2))")]
        [InlineData("(g + h*x)/((d + k*x + f*x^2)^2*sqrt(a + b*x + c*x^2))")]
        [InlineData("(7 + 13*x)/((5 + x + 2*x^2)^3*sqrt(2 + x + 3*x^2))")]
        [InlineData("x^3/((d + f*x^2)^2*sqrt(a + b*x + c*x^2))")]
        public void OverAPowerOfTheQuadratic(string integrand)
            => DifferentiatesBackPinned(integrand, new[] { 0.3, 0.7, 0.9, -0.5 }, Pins);

        /// <summary>
        /// Beside a half-odd power of <c>a + a sec(x)</c>, the half-angle tangent leaves a power
        /// of <c>(c + d) + (d - c) t^2</c> below the bar, which ran out Rubi's budget in 4.5.2.1.
        /// </summary>
        [Theory]
        [InlineData("sqrt(a + a*sec(x))/(c + d*sec(x))^2")]
        [InlineData("(a + a*sec(x))^(3/2)/(c + d*sec(x))^2")]
        [InlineData("sqrt(a + a*sec(x))/(c + d*sec(x))^3")]
        public void BesideAHalfOddPowerOfOnePlusASecant(string integrand)
            => DifferentiatesBackPinned(integrand, new[] { 0.3, 0.7, 1.1, -0.4 }, ("a", 1.3), ("c", 2), ("d", 0.7));
    }
}
