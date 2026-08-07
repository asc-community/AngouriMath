//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Linq;
using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.PatternsTest
{
    /// <summary>
    /// A three-term sum that is a perfect square was left as written when one of its terms
    /// is a radical: <c>1 + sqrt(2x) + x/2</c> is <c>(1 + sqrt(x/2))^2</c>.
    /// https://github.com/asc-community/AngouriMath/issues/176
    /// https://github.com/asc-community/AngouriMath/issues/203
    /// </summary>
    [Trait("Area", "PatternsTest")]
    public sealed class CollapsePerfectSquareTest
    {
        private static readonly double[] Points = { 0.3, 1.4, 2.7, 5.1, 8.6 };

        /// <summary>
        /// Sampled rather than compared as text: the square can be spelled several equal
        /// ways -- sqrt(x/2) and sqrt(2x)/2 are one number -- and what matters is that the
        /// collapsed form is the same function, not how it prints.
        /// </summary>
        private static void AssertSameValue(string original, Entity collapsed)
        {
            var compared = 0;
            foreach (var point in Points)
            {
                double before, after;
                try
                {
                    before = original.ToEntity().Substitute("x", point).EvalNumerical()
                        .RealPart.EDecimal.ToDouble();
                    after = collapsed.Substitute("x", point).EvalNumerical()
                        .RealPart.EDecimal.ToDouble();
                }
                catch { continue; }
                if (double.IsNaN(before) || double.IsNaN(after)) continue;
                compared++;
                var scale = Math.Max(Math.Max(Math.Abs(before), Math.Abs(after)), 1e-12);
                Assert.True(Math.Abs(before - after) <= 1e-9 * scale,
                    $"{original} collapsed to {collapsed.Stringize()}, which at x = {point} "
                    + $"is {after} rather than {before}");
            }
            Assert.True(compared > 0, $"{original}: no point was comparable, so nothing was checked");
        }

        [Theory]
        [InlineData("1 + sqrt(2 * x) + x / 2")]
        [InlineData("1 + 2 * sqrt(x) + x")]
        [InlineData("4 + 4 * sqrt(x) + x")]
        public void ASumWithARadicalCollapsesToASquare(string expr)
        {
            var collapsed = expr.ToEntity().Factorize();
            Assert.Contains(collapsed.Nodes,
                node => node is Entity.Powf(_, Entity.Number.Integer(2)));
            AssertSameValue(expr, collapsed);
        }

        /// <summary>
        /// A sum that is not a square must be left alone -- the cross term has to be
        /// exactly twice the product of the two roots, and a rule that rounded that off
        /// would be inventing an identity.
        /// </summary>
        [Theory]
        [InlineData("1 + 3 * sqrt(x) + x")]
        [InlineData("1 + sqrt(x) + x")]
        [InlineData("1 + sqrt(2 * x) + x / 3")]
        public void ASumThatIsNotASquareIsLeftAlone(string expr)
            => AssertSameValue(expr, expr.ToEntity().Factorize());

        /// <summary>
        /// <c>sqrt(u)^2</c> is <c>u</c> for every complex <c>u</c>, but <c>sqrt(u^2)</c> is
        /// not <c>u</c> -- it is <c>u</c> only where <c>u</c> is non-negative. So a
        /// polynomial trinomial must not be collapsed through this route, which would be
        /// #752 all over again: at <c>x = -3</c>, <c>x^2 + 2x + 1</c> is 4 and
        /// <c>(sqrt(x^2) + 1)^2</c> is 16.
        /// </summary>
        /// <summary>
        /// Not a square over the complex plane, though it reads like one: sqrt(x)*sqrt(y)
        /// is sqrt(x*y) only for suitable branches. At x = y = -1 the sum is 0 while
        /// (sqrt(x) + sqrt(y))^2 is -4, so collapsing it would be #752 in a new place.
        /// </summary>
        [Fact]
        public void ATwoVariableSumThatOnlyLooksLikeASquareIsLeftAlone()
        {
            var collapsed = "x + 2 * sqrt(x * y) + y".ToEntity().Factorize();
            var at = collapsed.Substitute("x", -1).Substitute("y", -1).EvalNumerical();
            Assert.True(Math.Abs(at.RealPart.EDecimal.ToDouble()) < 1e-9,
                $"x + 2*sqrt(x*y) + y collapsed to {collapsed.Stringize()}, which at x = y = -1 "
                + $"is {at.Stringize()} rather than 0");
        }

        [Theory]
        [InlineData("x ^ 2 + 2 * x + 1", -3.0)]
        [InlineData("x ^ 2 - 2 * x + 1", -3.0)]
        public void APolynomialTrinomialKeepsItsValueAtANegativePoint(string expr, double point)
        {
            var collapsed = expr.ToEntity().Factorize();
            var before = expr.ToEntity().Substitute("x", point).EvalNumerical()
                .RealPart.EDecimal.ToDouble();
            var after = collapsed.Substitute("x", point).EvalNumerical()
                .RealPart.EDecimal.ToDouble();
            Assert.True(Math.Abs(before - after) < 1e-9,
                $"{expr} collapsed to {collapsed.Stringize()}, which at x = {point} is {after} rather than {before}");
        }
    }
}
