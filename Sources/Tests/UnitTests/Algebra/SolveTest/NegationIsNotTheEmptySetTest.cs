//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Tests.Algebra.SolveTest
{
    /// <summary>
    /// The statement solver had no arm for <see cref="Notf"/>, so every negation fell to
    /// <see cref="Set.Empty"/> — a positive claim that no value satisfies the statement.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1127">#1127</a>
    /// </summary>
    /// <remarks>
    /// These assert what the answers <b>mean</b> — which values are in the set and which are not
    /// — rather than the shape they are written in, so that a later rewrite that says the same
    /// thing better does not fail them.
    /// </remarks>
    [Trait("Area", "Algebra")]
    public sealed class NegationIsNotTheEmptySetTest
    {
        private static Set Solve(string statement) => statement.ToEntity().Solve("x");

        /// <summary>The defect: none of these is unsatisfiable, so none of them is empty.</summary>
        [Theory]
        [InlineData("not (x = 1)")]
        [InlineData("not (x > 1)")]
        [InlineData("not (x >= 1)")]
        [InlineData("not (x < 1)")]
        [InlineData("not (x in RR)")]
        [InlineData("not not (x = 1)")]
        [InlineData("not (x = 1 and x = 2)")]
        [InlineData("not (x = 1 or x = 2)")]
        public void ASatisfiableNegationIsNotTheEmptySet(string statement)
        {
            var solutions = Solve(statement);
            Assert.False(solutions.IsSetEmpty, $"{statement} has solutions");
            Assert.NotEqual((Entity)Set.Empty, (Entity)solutions);
        }

        /// <summary>
        /// A negated comparison is a comparison, and answering it as one is what
        /// <see cref="Core.Transformations.RewriteRules.InequalityEquality"/> already says.
        /// </summary>
        [Theory]
        [InlineData("not (x > 1)", "x <= 1")]
        [InlineData("not (x >= 1)", "x < 1")]
        [InlineData("not (x < 1)", "x >= 1")]
        [InlineData("not (x <= 1)", "x > 1")]
        [InlineData("not not (x = 1)", "x = 1")]
        public void ANegatedComparisonIsSolvedAsTheComparisonItIs(string negated, string same)
            => Assert.Equal(Solve(same), Solve(negated));

        /// <summary>
        /// De Morgan, in the direction that reaches an arm: the negation of a conjunction is the
        /// union of the negations, and of a disjunction the intersection.
        /// </summary>
        [Theory]
        [InlineData("not (x > 1 or x < -1)", "x <= 1 and x >= -1")]
        [InlineData("not (x > 1 and x > 2)", "x <= 1 or x <= 2")]
        public void ANegatedConnectiveIsPushedInward(string negated, string same)
            => Assert.Equal(Solve(same), Solve(negated));

        /// <summary>
        /// And the answers are right about individual values, which is what "not empty" alone
        /// does not establish.
        /// </summary>
        [Theory]
        [InlineData("not (x = 1)", 2, true)]
        [InlineData("not (x = 1)", 1, false)]
        [InlineData("not (x > 1)", 0, true)]
        [InlineData("not (x > 1)", 5, false)]
        [InlineData("not (x > 1)", 1, true)]
        [InlineData("not (x >= 1)", 1, false)]
        [InlineData("not (x > 1 or x < -1)", 0, true)]
        [InlineData("not (x > 1 or x < -1)", 3, false)]
        [InlineData("not (x = 1 or x = 2)", 3, true)]
        [InlineData("not (x = 1 or x = 2)", 2, false)]
        public void ANegationsAnswerDecidesTheRightValues(string statement, int value, bool expected)
        {
            Assert.True(Solve(statement).TryContains(value, out var contains),
                $"membership of {value} in the answer to {statement} is decidable");
            Assert.Equal(expected, contains);
        }

        /// <summary>
        /// What no arm reaches is answered as written rather than as nothing:
        /// <c>{ x : not x in RR }</c> names the non-real complex numbers exactly.
        /// </summary>
        [Fact]
        public void AnUnreachedNegationIsAnsweredAsWritten()
        {
            var solutions = Solve("not (x in RR)");
            Assert.IsType<ConditionalSet>(solutions);
            Assert.True(solutions.TryContains("i".ToEntity(), out var nonReal) && nonReal);
            Assert.True(solutions.TryContains(3, out var real));
            Assert.False(real);
        }
    }
}
