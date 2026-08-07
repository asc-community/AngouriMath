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

namespace AngouriMath.Tests.Algebra.SolveTest
{
    /// <summary>
    /// A polynomial whose roots are found numerically must be answered with its roots and
    /// not with one entry per starting point the search happened to use. No issue is filed
    /// for this; it was found while checking the answers of another fix.
    /// </summary>
    [Trait("Area", "Algebra")]
    public sealed class DuplicateRootTest
    {
        /// <summary>
        /// The search starts from a grid, so the same root reached from different starting
        /// points came back agreeing to sixteen significant digits and differing after
        /// that, and each of those counted as an answer of its own: x^5 + 3x + 1 was
        /// answered with 28 roots, four of them -0.83907243306660750, -0.83907243306660761,
        /// -0.83907243306660773 and -0.83907243306660784.
        /// </summary>
        [Theory]
        [InlineData("x ^ 5 + 3 * x + 1", 5)]
        [InlineData("x ^ 6 + x + 1", 6)]
        [InlineData("x ^ 5 + x + 1", 5)]
        [InlineData("x ^ 5 - 5 * x + 2", 5)]
        [InlineData("x ^ 6 - 3 * x ^ 2 + 1", 6)]
        [InlineData("x ^ 9 - x - 1", 9)]
        public void APolynomialHasAsManyRootsAsItsDegree(string expression, int degree) =>
            Assert.Equal(degree, RootsOf(expression).Count);

        // The polynomials answered exactly, by radicals rather than by iteration, are not
        // touched by any of this and must keep the roots they had.
        [Theory]
        [InlineData("x ^ 2 - 4", 2)]
        [InlineData("x ^ 3 - 1", 3)]
        [InlineData("x ^ 5 - 1", 5)]
        [InlineData("x ^ 8 - 1", 8)]
        [InlineData("x ^ 7 - 2", 7)]
        [InlineData("x ^ 4 - 10 * x ^ 3 + 35 * x ^ 2 - 50 * x + 24", 4)]
        public void TheOnesSolvedExactlyAreUnaffected(string expression, int degree) =>
            Assert.Equal(degree, RootsOf(expression).Count);

        // Whichever candidate is kept from a group has to be one, so collapsing them may
        // not hand back something that fails the equation.
        [Theory]
        [InlineData("x ^ 5 + 3 * x + 1")]
        [InlineData("x ^ 6 + x + 1")]
        [InlineData("x ^ 9 - x - 1")]
        public void EveryRootKeptIsStillARoot(string expression)
        {
            var expr = expression.ToEntity();
            foreach (var root in RootsOf(expression))
            {
                var residual = expr.Substitute("x", root).EvalNumerical();
                Assert.True(
                    residual.Abs().EDecimal.ToDouble() < 1e-8,
                    $"{root.Stringize()} leaves {residual.Stringize()}");
            }
        }

        // No two of the answers may be the same root written twice.
        [Theory]
        [InlineData("x ^ 5 + 3 * x + 1")]
        [InlineData("x ^ 6 + x + 1")]
        [InlineData("x ^ 9 - x - 1")]
        [InlineData("x ^ 5 - 5 * x + 2")]
        public void NoTwoAnswersAreTheSameNumber(string expression)
        {
            var values = RootsOf(expression)
                .Select(root => (Entity.Number.Complex)root.EvalNumerical())
                .Select(value => (Re: value.RealPart.EDecimal.ToDouble(), Im: value.ImaginaryPart.EDecimal.ToDouble()))
                .ToList();
            for (var i = 0; i < values.Count; i++)
                for (var j = i + 1; j < values.Count; j++)
                    Assert.True(
                        Math.Max(Math.Abs(values[i].Re - values[j].Re), Math.Abs(values[i].Im - values[j].Im)) > 1e-9,
                        $"{values[i]} and {values[j]} are the same root of {expression}");
        }

        private static Entity.Set.FiniteSet RootsOf(string expression) =>
            Assert.IsType<Entity.Set.FiniteSet>(expression.ToEntity().SolveEquation("x"));
    }
}
