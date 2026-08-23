//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Collections.Generic;
using PeterO.Numbers;
using AngouriMath;
using AngouriMath.Extensions;
using Xunit;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Tests.Algebra
{
    [Trait("Area", "Algebra")]
    public sealed class SolveInequality
    {
        [Theory]
        [InlineData("x > 0", "(0; +oo)")]
        [InlineData("x < 0", "(-oo; 0)")]
        [InlineData("x > 0 and x < 0", "{}")]
        [InlineData("(x - 2)(x + 2) > 0", @"(-oo; -2) \/ (2; +oo)")]
        [InlineData("(x - 2)(x + 2) < 0", "(-2; 2)")]
        [InlineData("(x - 2)(x + 2) <= 0", "[-2; 2]")]
        // The symbolic coefficient is no longer pinned by its printed form here. It was, for
        // two releases, as evidence of https://github.com/asc-community/AngouriMath/issues/757
        // -- first as [a; -a] and then as the empty (sqrt(a^2); -sqrt(a^2)) -- and each of
        // those expectations recorded a wrong answer rather than a right one. It is now
        // checked for what it has to be true of instead, in
        // <see cref="ASymbolicCoefficientGivesAnIntervalOrderedForEitherSign"/>.
        public void Test(string initial, string expected)
        {
            Variable x = "x";
            Entity initialEnt = initial;
            Entity solutions = initialEnt.Solve(x);
            var expectedEntity = expected.ToEntity().InnerSimplified;
            var actualEntity = solutions.Simplify().InnerSimplified;
            Assert.Equal(expectedEntity, actualEntity);
        }

        /// <summary>
        /// Checked by which points the answer holds rather than by how it prints:
        /// <c>1/2 * abs(X)</c> and <c>abs(X) / 2</c> are one number written two ways, and a
        /// structural comparison calls that a disagreement.
        /// </summary>
        private static void AssertSolvesLike(string inequality, string value)
        {
            Variable x = "x";
            var symbolic = (Set)((Entity)inequality).Solve(x).Substitute("a", value.ToEntity()).Simplify();
            var truth = ((Entity)inequality).Substitute("a", value.ToEntity());
            foreach (var point in new Entity[] { -7, -3, -2, -1, "-1/2".ToEntity(), 0,
                                                 "1/2".ToEntity(), 1, 2, 3, 7 })
                Assert.True(symbolic.Contains(point) == truth.Substitute(x, point).EvalBoolean(),
                    $"solving {inequality} symbolically and then setting a = {value} gives "
                    + $"{symbolic}, which disagrees with the inequality itself at x = {point}");
        }

        [Theory]
        [InlineData("(x - a)(x + a) <= 0", "3")]
        [InlineData("(x - a)(x + a) <= 0", "-3")]
        [InlineData("(x - a)(x + a) <= 0", "1/2")]
        [InlineData("(x - a)(x + a) <= 0", "-1/2")]
        [InlineData("(x - a)(x + a) < 0", "3")]
        [InlineData("(x - a)(x + a) < 0", "-3")]
        [InlineData("(x - a)(x + a) > 0", "3")]
        [InlineData("(x - a)(x + a) > 0", "-3")]
        [InlineData("(x - a)(x + a) >= 0", "3")]
        [InlineData("(x - a)(x + a) >= 0", "-3")]
        public void ASymbolicCoefficientGivesAnIntervalOrderedForEitherSign(string inequality, string value)
            => AssertSolvesLike(inequality, value);

        /// <summary>
        /// The roots need not be symmetric about zero, and need not both be symbolic, for the
        /// ordering to be undecidable -- <c>(x - 1)(x - a)</c> turns on whether <c>a</c> is
        /// above or below 1.
        /// </summary>
        [Theory]
        [InlineData("(x - a)(x - 2*a) <= 0", "3")]
        [InlineData("(x - a)(x - 2*a) <= 0", "-3")]
        [InlineData("(x - a)(x - 2*a) < 0", "3")]
        [InlineData("(x - a)(x - 2*a) < 0", "-3")]
        [InlineData("(x - 1)(x - a) <= 0", "3")]
        [InlineData("(x - 1)(x - a) <= 0", "-3")]
        [InlineData("(x - 1)(x - a) <= 0", "1/2")]
        [InlineData("(x - 1)(x - a) < 0", "3")]
        [InlineData("(x - 1)(x - a) < 0", "-3")]
        [InlineData("x^2 - a^2 <= 0", "3")]
        [InlineData("x^2 - a^2 <= 0", "-3")]
        [InlineData("x^2 - a^2 < 0", "3")]
        [InlineData("x^2 - a^2 < 0", "-3")]
        public void AsymmetricAndOneSidedRootsAreOrderedToo(string inequality, string value)
            => AssertSolvesLike(inequality, value);

        /// <summary>
        /// A double root, where the two endpoints coincide and the span between them is empty.
        /// The ordering arithmetic collapses to a point, which is what these have to be:
        /// <c>(x - a)^2 &lt;= 0</c> holds only at <c>a</c>, and <c>&lt; 0</c> nowhere. Before,
        /// each came back as an interval built from an unevaluated discriminant.
        /// </summary>
        [Theory]
        [InlineData("(x - a)^2 <= 0", "3")]
        [InlineData("(x - a)^2 <= 0", "-3")]
        [InlineData("(x - a)^2 < 0", "3")]
        [InlineData("(x - a)^2 > 0", "3")]
        [InlineData("(x - a)^2 >= 0", "3")]
        [InlineData("x^2 - 2*a*x + a^2 < 0", "3")]
        [InlineData("x^2 - 2*a*x + a^2 > 0", "-3")]
        public void ADoubleRootCollapsesToThePoint(string inequality, string value)
            => AssertSolvesLike(inequality, value);


        [Theory]
        [InlineData("x > a")]
        [InlineData("x < a")]
        [InlineData("x <= a")]
        [InlineData("x >= a")]
        [InlineData("(x + a)(x + b) >= 0")]
        [InlineData("(x + a)(x + b) > 0")]
        // These two were skipped as "Piecewise required" and no longer are. They wanted the
        // smaller of two symbolic roots as the left endpoint, and that has a closed form --
        // https://github.com/asc-community/AngouriMath/issues/757 -- so no case split was
        // needed to answer them after all.
        [InlineData("(x + a)(x + b) <= 0")]
        [InlineData("(x + a)(x + b) < 0")]
        public void AutoTest(string inequality, string setToCheck = "{ -5, -3, 0, 3, 5 }")
        {
            FiniteSet checkpoints = (FiniteSet)setToCheck.Simplify();
            var roots = (Set)inequality.Solve("x").Substitute("a", -10).Substitute("b", 10).Simplify();
            foreach (var cp in checkpoints)
                Assert.True(roots.Contains(cp) == inequality.Substitute("x", cp).Substitute("a", -10).Substitute("b", 10).EvalBoolean(), 
                    $"{roots} doesn't contain {cp}");
        }

        /// <summary>
        /// A symbol on the right-hand side moves the roots rather than merely permuting them,
        /// so whether the quadratic has real roots at all turns on the sign of <c>a + 1/4</c>,
        /// and where it has none the answer is the whole line or nothing rather than an
        /// interval between endpoints. These were skipped as "Piecewise required" since 1.2 and
        /// are answered by the case split on the signs of the leading coefficient and the
        /// discriminant.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/762">#762</a>
        /// </summary>
        [Theory]
        [InlineData("(x + 1)(x + 2) < a")]
        [InlineData("(x + 1)(x + 2) <= a")]
        [InlineData("(x + 1)(x + 2) > a")]
        [InlineData("(x + 1)(x + 2) >= a")]
        public void AShiftedRightHandSideIsAnsweredToo(string inequality, string setToCheck = "{ -5, -3, 0, 3, 5 }")
            => AutoTest(inequality, setToCheck);

        /// <summary>
        /// Whether a parabola lies above zero between its roots or outside them is the sign of
        /// its leading coefficient, and the solver tested <c>a is Real { IsNegative: true }</c>
        /// -- which a symbol fails, so every symbolic leading coefficient was read as positive
        /// and <c>a*x^2 - 1 &lt; 0</c> came back with the complement of its solution set. Where
        /// the coefficient is symbolic, so is the discriminant's sign, and a negative one means
        /// no real roots at all and no endpoints to lie between.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/762">#762</a>
        /// </summary>
        [Theory]
        [InlineData("a*x^2 - 1 < 0", "3")]
        [InlineData("a*x^2 - 1 < 0", "-3")]
        [InlineData("a*x^2 - 1 < 0", "1/2")]
        [InlineData("a*x^2 - 1 < 0", "-1/2")]
        [InlineData("a*x^2 - 1 <= 0", "3")]
        [InlineData("a*x^2 - 1 <= 0", "-3")]
        [InlineData("a*x^2 - 1 > 0", "3")]
        [InlineData("a*x^2 - 1 > 0", "-3")]
        [InlineData("a*x^2 - 1 >= 0", "3")]
        [InlineData("a*x^2 - 1 >= 0", "-3")]
        [InlineData("a*x^2 - a < 0", "3")]
        [InlineData("a*x^2 - a < 0", "-3")]
        [InlineData("a*x^2 - a > 0", "3")]
        [InlineData("a*x^2 - a > 0", "-3")]
        [InlineData("a*x^2 - a >= 0", "1/2")]
        [InlineData("a*x^2 - a >= 0", "-1/2")]
        public void ASymbolicLeadingCoefficientPicksItsOwnBranch(string inequality, string value)
            => AssertSolvesLike(inequality, value);

        /// <summary>
        /// A symbolic coefficient on a linear inequality has the same defect in miniature:
        /// <c>a*x + b &gt; 0</c> is <c>x &gt; -b/a</c> for a positive <c>a</c> and
        /// <c>x &lt; -b/a</c> for a negative one, and for <c>a = 0</c> it is not an inequality
        /// in <c>x</c> at all.
        /// </summary>
        [Theory]
        [InlineData("a*x - 1 > 0", "3")]
        [InlineData("a*x - 1 > 0", "-3")]
        [InlineData("a*x + 2 < 0", "3")]
        [InlineData("a*x + 2 < 0", "-3")]
        [InlineData("a*x - 1 >= 0", "1/2")]
        [InlineData("a*x - 1 >= 0", "-1/2")]
        public void ASymbolicLinearCoefficientPicksItsOwnDirection(string inequality, string value)
            => AssertSolvesLike(inequality, value);

        /// <summary>
        /// A quadratic that never reaches zero is above it everywhere or below it everywhere,
        /// and which of those is again the sign of the leading coefficient. Returning the empty
        /// set regardless answered <c>x^2 + 1 &gt; 0</c> with nothing, where it holds at every
        /// real x. Whether this was noticed at all depended on how far the radical simplified:
        /// <c>sqrt(-4)</c> is the literal <c>2i</c> and <c>sqrt(-12)</c> is a product that is
        /// not, so <c>x^2 + 1</c> was recognised and <c>3*x^2 + 1</c> was not -- the
        /// discriminant is read directly now.
        /// </summary>
        [Theory]
        [InlineData("x^2 + 1 > 0", true)]
        [InlineData("3*x^2 + 1 > 0", true)]
        [InlineData("x^2 + x + 1 > 0", true)]
        [InlineData("x^2 + 1 >= 0", true)]
        [InlineData("x^2 + 1 < 0", false)]
        [InlineData("3*x^2 + 1 <= 0", false)]
        [InlineData("-x^2 - 1 < 0", true)]
        [InlineData("-x^2 - 1 > 0", false)]
        [InlineData("-3*x^2 - 1 <= 0", true)]
        public void AQuadraticThatNeverReachesZeroHoldsEverywhereOrNowhere(string inequality, bool everywhere)
        {
            Variable x = "x";
            var solutions = (Set)((Entity)inequality).Solve(x).Simplify();
            foreach (var point in new Entity[] { -7, -1, 0, "1/2".ToEntity(), 3, 7 })
                Assert.True(solutions.Contains(point) == everywhere,
                    $"{inequality} was solved as {solutions}, which "
                    + (everywhere ? "does not hold" : "holds") + $" at x = {point}");
        }

        /// <summary>
        /// A univariate polynomial inequality of degree three or more, answered by the sign
        /// table over its irreducible factors. Checked by which points the answer holds at
        /// rather than by how it prints, since the endpoints of a cubic's intervals are
        /// radicals with several equally correct writings.
        /// </summary>
        private static void AssertHoldsExactlyWhereItShould(string inequality, params double[] extraPoints)
        {
            Variable x = "x";
            var solutions = (Set)((Entity)inequality).Solve(x).Simplify();
            var points = new List<Entity>();
            for (var numerator = -60; numerator <= 60; numerator++)
                points.Add(Number.Rational.Create(EInteger.FromInt32(numerator), EInteger.FromInt32(17)));
            foreach (var extra in extraPoints)
                points.Add(Number.Rational.Create(EInteger.FromInt64((long)System.Math.Round(extra * 1000)), EInteger.FromInt32(1000)));
            foreach (var point in points)
            {
                var truth = ((Entity)inequality).Substitute(x, point).EvalBoolean();
                Assert.True(solutions.Contains(point) == truth,
                    $"{inequality} was solved as {solutions}, which "
                    + (truth ? "excludes" : "includes") + $" x = {point} where the inequality "
                    + (truth ? "holds" : "does not hold"));
            }
        }

        /// <summary>
        /// Degree three and above used to be refused outright -- "Only linear and quadratic
        /// polynomial inequalities are supported". The sign table answers them by factoring
        /// into irreducibles over Q and reading the sign between consecutive roots, and the
        /// discriminant of each factor is what says how many real roots there are to look
        /// for. <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a>,
        /// item 43.
        /// </summary>
        [Theory]
        // Splits into linear factors.
        [InlineData("x^3 - x > 0")]
        [InlineData("x^3 - x < 0")]
        [InlineData("x^3 - x >= 0")]
        [InlineData("x^3 - x <= 0")]
        [InlineData("x^4 - 5*x^2 + 4 > 0")]
        [InlineData("x^4 - 5*x^2 + 4 < 0")]
        // A repeated factor, where the sign does not change across the root.
        [InlineData("(x - 1)^2 * (x + 2) > 0")]
        [InlineData("(x - 1)^2 * (x + 2) < 0")]
        [InlineData("(x - 1)^2 * (x + 2) >= 0")]
        [InlineData("x^2 * (x - 3) < 0")]
        // An irreducible quadratic factor with real roots: its discriminant is positive, so
        // it contributes two endpoints that are not rational.
        [InlineData("x^3 - 2*x + 1 > 0")]
        [InlineData("x^3 - 2*x + 1 < 0")]
        // An irreducible quadratic factor with no real roots at all, which contributes a
        // constant sign and no endpoint.
        [InlineData("(x - 1) * (x^2 + 1) > 0")]
        [InlineData("(x^2 + x + 1) * (x + 2) < 0")]
        [InlineData("x^4 + x^2 + 1 > 0")]
        [InlineData("x^4 + x^2 + 1 < 0")]
        // An irreducible cubic: one real root where the discriminant is negative, three
        // where it is positive.
        [InlineData("x^3 - 2 > 0")]
        [InlineData("x^3 - 2 < 0")]
        [InlineData("x^3 - 3*x + 1 > 0")]
        [InlineData("x^3 - 3*x + 1 < 0")]
        // A negative leading coefficient, and denominators in the coefficients.
        [InlineData("-x^3 + x > 0")]
        [InlineData("x^3/2 - x/8 > 0")]
        // Degree five and six, still splitting into factors of degree at most three.
        [InlineData("x^5 - 5*x^3 + 4*x > 0")]
        [InlineData("(x^2 - 2) * (x^2 - 3) * (x + 1) > 0")]
        public void AHigherDegreePolynomialInequalityIsAnswered(string inequality)
            => AssertHoldsExactlyWhereItShould(inequality);
    }
}
