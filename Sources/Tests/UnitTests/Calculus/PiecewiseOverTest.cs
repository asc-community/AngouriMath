//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Linq;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// An expression with floors and ceilings in it over a bounded range, as the piecewise it is there: the pieces
    /// partition the range, their ends are exact, and an end belongs to the piece whose value it has -- or null, where
    /// that cannot be proved. The expressions are the ones
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1807">#1807</a> names.
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class PiecewiseOverTest
    {
        private static Entity.Piecewise? Pieces(string expr, string from, string to)
            => MathS.ExperimentalFeatures.PiecewiseOver(expr.ToEntity(), "x", from.ToEntity(), to.ToEntity());

        /// <summary>Each piece as <c>value on interval</c>, with the interval as the library writes it.</summary>
        private static string Written(Entity.Piecewise pieces)
            => string.Join(", ", pieces.Cases.Select(@case => $"{@case.Expression.Stringize()} for {@case.Predicate.Stringize()}"));

        [Theory]
        [InlineData("floor(x^2)", "0", "2",
            "0 for x in [0; 1), 1 for x in [1; sqrt(2)), 2 for x in [sqrt(2); sqrt(3)), 3 for x in [sqrt(3); 2), 4 for x = 2")]
        [InlineData("floor(x^2/2)", "0", "3",
            "0 for x in [0; sqrt(2)), 1 for x in [sqrt(2); 2), 2 for x in [2; sqrt(6)), 3 for x in [sqrt(6); 2 * sqrt(2)), 4 for x in [2 * sqrt(2); 3]")]
        // The argument touches 1, -1 and 0 at its turning points, where its derivative is 0: the value there is the
        // one the point has, and the pieces either side of it are one value.
        [InlineData("floor(sin(pi*x*(4 - x)/2))", "0", "4",
            "0 for x in [0; 2 - sqrt(3)), 1 for x = 2 - sqrt(3), 0 for x in (2 - sqrt(3); 2 - sqrt(2)], -1 for x in (2 - sqrt(2); 2), "
            + "0 for x = 2, -1 for x in (2; 2 + sqrt(2)), 0 for x in [2 + sqrt(2); 2 + sqrt(3)), 1 for x = 2 + sqrt(3), 0 for x in (2 + sqrt(3); 4]")]
        [InlineData("ceil(sin(pi*x*(4 - x)/2))", "0", "4",
            "0 for x = 0, 1 for x in (0; 2 - sqrt(2)), 0 for x in [2 - sqrt(2); 1), -1 for x = 1, 0 for x in (1; 3), -1 for x = 3, "
            + "0 for x in (3; 2 + sqrt(2)], 1 for x in (2 + sqrt(2); 4), 0 for x = 4")]
        [InlineData("floor(sin(pi*x/2))", "0", "4",
            "0 for x in [0; 1), 1 for x = 1, 0 for x in (1; 2], -1 for x in (2; 4), 0 for x = 4")]
        // Two steps, the pieces their common refinement, and the runs of one value joined.
        [InlineData("floor(x^2) + ceil(x)", "-1", "2",
            "0 for x in [-1; 0], 1 for x in (0; 1), 2 for x = 1, 3 for x in (1; sqrt(2)), 4 for x in [sqrt(2); sqrt(3)), 5 for x in [sqrt(3); 2), 6 for x = 2")]
        // What is beside the step is carried into each piece.
        [InlineData("x*floor(x^2)", "0", "2",
            "0 for x in [0; 1), x for x in [1; sqrt(2)), x * 2 for x in [sqrt(2); sqrt(3)), x * 3 for x in [sqrt(3); 2), x * 4 for x = 2")]
        [InlineData("floor(e^x)", "0", "1", "1 for x in [0; ln(2)), 2 for x in [ln(2); 1]")]
        public void IsThePiecewiseItIsOverTheRange(string expr, string from, string to, string expected)
        {
            var pieces = Pieces(expr, from, to);
            Assert.NotNull(pieces);
            Assert.Equal(expected, Written(pieces!));
        }

        /// <summary>
        /// The piecewise has the expression's value at every point of the range, a point inside a piece and each end
        /// alike: exact substitution, which is what <c>floor(sin(pi x/2))</c> at 4 got wrong in 2.5.0.
        /// </summary>
        [Theory]
        [InlineData("floor(sin(pi*x*(4 - x)/2))", "0", "4", new[] { "0", "1/10", "2 - sqrt(3)", "1/2", "1", "2", "5/2", "3", "2 + sqrt(2)", "4" })]
        [InlineData("floor(x^2) + ceil(x)", "-1", "2", new[] { "-1", "-1/2", "0", "1/2", "1", "sqrt(2)", "3/2", "sqrt(3)", "2" })]
        public void HasTheExpressionsValueAtEachPoint(string expr, string from, string to, string[] points)
        {
            var pieces = Pieces(expr, from, to);
            Assert.NotNull(pieces);
            foreach (var point in points)
            {
                var want = expr.ToEntity().Substitute("x", point.ToEntity()).Simplify();
                var got = pieces!.Substitute("x", point.ToEntity()).Simplify();
                Assert.Equal(want, got);
            }
        }

        /// <summary>
        /// Declined, not answered with a piece missing: a pole of the argument in the range, a floor inside a floor, a
        /// range that is not one, and an argument with another symbol in it.
        /// </summary>
        [Theory]
        [InlineData("floor(1/x)", "-1", "1")]
        [InlineData("floor(floor(x)/2 + x)", "0", "3")]
        [InlineData("floor(x^2)", "2", "0")]
        [InlineData("floor(x^2)", "0", "a")]
        [InlineData("floor(a*x)", "0", "1")]
        public void IsNullWhereThePiecesCannotBeProvedComplete(string expr, string from, string to)
            => Assert.Null(Pieces(expr, from, to));
    }
}
