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
    /// A square root of a perfect square is the modulus, <c>sgn(P) P</c> for a real <c>P</c>, at
    /// every depth, and the rules that read a root of a quadratic must not see one:
    /// <c>1/sqrt(1 + csch(x)^2)</c> under <c>u = tanh(x)</c> is <c>sqrt(u^2)/(u^2 - 1)</c>, which
    /// the table rule for a root of a quadratic beside a linear answered with a logarithm of zero.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class RootOfAPerfectSquareIntegralTest
    {
        private static readonly double[] Points = { -2.3, -1.7, -0.4, 0.3, 0.7, 1.1, 1.9 };

        private static void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x").Substitute("C", 0);
            Assert.DoesNotContain("integral(", integral.Stringize());
            var derivative = integral.Differentiate("x");
            var original = integrand.ToEntity();
            var compared = 0;
            foreach (var at in Points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                if (got.IsNaN || want.IsNaN)
                    continue;
                compared++;
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 5, $"only {compared} of {Points.Length} points were comparable for {integrand}");
        }

        [Theory]
        [InlineData("1/sqrt(1+csch(x)^2)")]
        [InlineData("sqrt(x^2)/(x^2-1)")]
        [InlineData("sqrt(x^2)/(x+2)")]
        [InlineData("(x^2+2*x+1)^(3/2)/(x+3)")]
        [InlineData("(4*x^2-4*x+1)^(1/2)*e^x")]
        [InlineData("sqrt(sinh(x)^2)")]
        public void ARootOfAPerfectSquareIsTheModulus(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A square written with symbols: <c>a^2 + 2 a b x + b^2 x^2</c> is <c>(a + b x)^2</c>,
        /// whose discriminant <c>4a^2b^2 - 4a^2b^2</c> <see cref="Entity.InnerSimplified"/> does
        /// not collect, and whose leading coefficient <c>b^2</c> is not a number -- it is
        /// positive for a real parameter, which is what the answer's <c>provided b^2 &gt; 0</c>
        /// says. At the top the power may be any half-odd one, since the sign written there is
        /// not a factor some substitution below has to carry. Rubi's 1.2.1.9.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
        /// </summary>
        [Theory]
        [InlineData("(a^2 + 2*a*b*x + b^2*x^2)^(5/2)")]
        [InlineData("(A + B*x)*(d + pe*x)/(a^2 + 2*a*b*x + b^2*x^2)^(5/2)")]
        [InlineData("1/(a^2 + 2*a*b*x + b^2*x^2)^(3/2)")]
        public void ASquareWrittenWithSymbols(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x").Substitute("C", 0);
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("NaN", integral.Stringize());
            Entity Pin(Entity e) => e.Substitute("a", 0.9).Substitute("b", 1.7).Substitute("A", 0.7)
                .Substitute("B", 1.3).Substitute("d", 0.4).Substitute("pe", 1.1);
            var derivative = Pin(integral).Differentiate("x");
            var original = Pin(integrand.ToEntity());
            var compared = 0;
            foreach (var at in Points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                if (got.IsNaN || want.IsNaN)
                    continue;
                compared++;
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 5, $"only {compared} points were comparable for {integrand}");
        }

        [Fact]
        public void TheSignIsTheSignOfTheLinearFactor()
        {
            var integral = "sqrt(x^2)/(x+2)".ToEntity().Integrate("x").Substitute("C", 0);
            Assert.Contains(integral.Nodes, node => node is Entity.Signumf(var argument) && argument == "x".ToEntity());
            // sgn(x) (x - 2 ln(x + 2)), compared as a value on each side of the sign change.
            var difference = integral - "sgn(x) * (x - 2 * ln(x + 2))".ToEntity();
            foreach (var at in new[] { -1.5, 0.5, 3.0 })
                Assert.True(Math.Abs((double)difference.Substitute("x", at).EvalNumerical().RealPart) < 1e-12, $"at x = {at}: {integral}");
        }
    }
}
