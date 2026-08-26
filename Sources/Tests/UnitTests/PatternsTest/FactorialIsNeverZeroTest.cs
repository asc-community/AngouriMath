//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Core.Transformations;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.PatternsTest
{
    /// <summary>
    /// A factorial is never zero <b>where it is defined</b>, and at a negative integer it is not.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1081">#1081</a>
    /// </summary>
    /// <remarks>
    /// The rule attached its <i>argument's</i> domain condition rather than the factorial's, and
    /// for a bare variable that condition is <c>True</c> — which <c>Provided</c> drops. So the
    /// answer went out unconditioned and was <c>False</c> at the poles, where the statement has
    /// no truth value at all.
    /// </remarks>
    [Trait("Area", "Patterns")]
    public sealed class FactorialIsNeverZeroTest
    {
        /// <summary>
        /// Where the factorial is defined the answer is still <see langword="false"/>, so the
        /// condition narrowed the rule rather than withdrawing it.
        /// </summary>
        [Theory]
        [InlineData("3")]
        [InlineData("0")]
        [InlineData("1/2")]
        public void ItIsFalseWhereTheFactorialExists(string at)
            => Assert.Equal(
                $"({at})! = 0".ToEntity().EvalBoolean(),
                "x! = 0".ToEntity().Simplify().Substitute("x", at.ToEntity()).EvalBoolean());

        /// <summary>
        /// At a pole the statement is <c>NaN</c>, and the simplified form has to be too — this is
        /// the case the rule used to answer <see langword="false"/>.
        /// </summary>
        [Theory]
        [InlineData("-1")]
        [InlineData("-2")]
        [InlineData("-3")]
        public void ItIsUndecidedWhereTheFactorialHasAPole(string at)
        {
            var simplified = "x! = 0".ToEntity().Simplify().Substitute("x", at.ToEntity());
            Assert.False(simplified.EvaluableBoolean);
            Assert.Equal($"({at})! = 0".ToEntity().Evaled, simplified.Evaled);
        }

        /// <summary>
        /// And the rule on its own, so that a change in what the candidate search happens to pick
        /// cannot quietly stop this being about the rule.
        /// </summary>
        [Fact]
        public void TheRuleCarriesTheFactorialsOwnCondition()
        {
            var rewritten = RewriteRules.InequalityEquality.ApplyOnce("x! = 0".ToEntity());
            Assert.NotEqual("x! = 0".ToEntity(), rewritten);
            Assert.IsType<Entity.Providedf>(rewritten);
        }
    }
}
