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
    /// A polynomial over a product of distinct linears beside the square root of a quadratic,
    /// above or below the bar, taken apart as the rational function it is over that root:
    /// <c>N sqrt(Q)/D</c> is <c>N Q/(D sqrt(Q))</c>, <c>P/D</c> is a polynomial plus a constant
    /// over each linear, and each piece is closed -- the polynomial over the root by the rules
    /// for it, each <c>K/((x - p) sqrt(Q))</c> by the reciprocal of its linear.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// Moses' <c>sqrt(A^2 + B^2 (1 - y^2))/(1 - y^2)</c> was declined: the Euler substitution
    /// takes its generic first substitution for a symbolic leading coefficient, with
    /// <c>sqrt(-B^2)</c> in it. Taken apart it is <c>B^2</c> over the root, which is the table's
    /// arcsine, and <c>A^2/2</c> over each of <c>1 - y</c> and <c>1 + y</c> beside it.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class RationalFunctionBesideTheRootOfAQuadraticTest
    {
        private static Entity DifferentiatesBack(string integrand, double[] points, params (string, double)[] pins)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("NaN", integral.Stringize());

            Entity answer = integral.Substitute("C", 0);
            Entity original = integrand.ToEntity();
            foreach (var (name, value) in pins)
            {
                answer = answer.Substitute(name, value);
                original = original.Substitute(name, value);
            }
            // With the symbols pinned the piecewise's conditions are decidable, and the
            // simplification picks the arm before the derivative is taken.
            var derivative = answer.Simplify().Differentiate("x");

            var compared = 0;
            foreach (var at in points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                if (got.IsNaN || want.IsNaN)
                    continue;
                compared++;
                var difference = Math.Abs((double)(got - want).RealPart)
                               + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-8,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
            Assert.True(compared >= 3,
                $"only {compared} of {points.Length} points were comparable for {integrand}");
            return integral;
        }

        /// <summary>
        /// The root above the bar and below it, with the linears written or found by
        /// factoring; checked on both sides of every pole, where the sign of each
        /// <c>x - p</c> is what makes one formula serve.
        /// </summary>
        [Theory]
        [InlineData("sqrt(4 + 9*(1 - x^2))/(1 - x^2)", new[] { -1.1, -0.7, -0.3, 0.4, 0.8, 1.1 })]
        [InlineData("sqrt(x^2 + x + 3)/(x*(x + 1))", new[] { -3.0, -1.5, -0.5, 0.4, 2.0 })]
        [InlineData("1/((x - 1)*(x + 2)*sqrt(x^2 + 1))", new[] { -3.0, -1.0, 0.3, 2.0, 3.0 })]
        [InlineData("x^2/((x - 1)*(x + 2)*sqrt(x^2 + 1))", new[] { -3.0, -1.0, 0.3, 2.0, 3.0 })]
        [InlineData("sqrt(1 + x^2)/(1 + x)", new[] { -3.0, -1.7, -0.4, 0.9, 2.6 })]
        public void TakenApartOverTheRoot(string integrand, double[] points) => DifferentiatesBack(integrand, points);

        /// <summary>
        /// Moses', with symbols: <c>B^2</c> over the root and <c>A^2/2</c> over each linear. And
        /// a symbolic quadratic under the root beside a written linear, on both signs of its
        /// constant.
        /// </summary>
        [Theory]
        [InlineData("-sqrt(A^2 + B^2*(1 - x^2))/(1 - x^2)", new[] { -1.7, -1.3, -0.5, 0.4, 1.3, 1.7 }, 1.3, 0.8, 3.0)]
        [InlineData("(x + 1)/(x*sqrt(a*x^2 + b*x + c))", new[] { -2.5, -0.7, 0.4, 1.7 }, 1.0, 1.0, 3.0)]
        [InlineData("(x + 1)/(x*sqrt(a*x^2 + b*x + c))", new[] { -4.0, -3.0, 1.6, 2.5 }, 1.0, 1.0, -3.0)]
        public void TheCoefficientsMayBeSymbols(string integrand, double[] points, double first, double second, double third)
            => DifferentiatesBack(integrand, points, ("A", first), ("B", second), ("a", first), ("b", second), ("c", third));

        /// <summary>
        /// The quadratic standing whole beside its own root, up to a constant, is one power of
        /// one base -- <c>sqrt(2) x^2 (1 - x^2)^(-3/2)</c> here -- and is answered as that, without
        /// a sign. Taken apart over <c>1 - x</c> and <c>1 + x</c> it was answered with
        /// <c>sgn(x - 1)</c> and <c>sgn(x + 1)</c> in each piece, and the derivative of a sign
        /// of a root is not read, so the by-parts remainder of Timofeev's
        /// <c>arcsin(sqrt((x - a)/(x + a)))</c>, which is this under <c>u = sqrt(1 - 2a/(x + a))</c>,
        /// made an answer nothing could check.
        /// </summary>
        [Fact]
        public void TheQuadraticBesideItsOwnRootIsOnePower()
        {
            var integral = DifferentiatesBack("x^2*(1/2 - x^2/2)^(-1/2)/(1 - x^2)", new[] { -0.8, -0.4, 0.3, 0.6, 0.9 });
            Assert.DoesNotContain("sgn(", integral.Stringize());
            DifferentiatesBack("asin(sqrt((-a + x)/(a + x)))", new[] { 1.3, 2.0, 3.5, 5.0 }, ("a", 1.0));
        }
    }
}
