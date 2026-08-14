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

namespace AngouriMath.Tests.Core.Transformations
{
    /// <summary>
    /// <see cref="RewriteRules.NumericNeat"/> inverted the sign where both operands of a sum
    /// or a difference were negative numerals: `-1 + -1` came back as `2`.
    /// https://github.com/asc-community/AngouriMath/issues/936
    /// </summary>
    /// <remarks>
    /// <para>
    /// The value is asserted rather than the shape, because the shape is the rule's business
    /// — it exists to write `-1 + -1` as `-(1 + 1)` — and the value is the part that was wrong.
    /// </para>
    /// <para>
    /// It has to be asserted against the <b>rule set applied alone</b>. Through `Simplify` the
    /// defect was invisible: the rule only ever fires on numerals, and on numerals evaluation
    /// has already produced the right answer before this is consulted, so every corpus in
    /// `work/` passed throughout. A rule that only fires where something else covers for it can
    /// be arbitrarily wrong and never show, until a caller applies it on its own — which the
    /// transformation layer now lets them do.
    /// </para>
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class NumericNeatSignTest
    {
        private static Entity Rewritten(string expression)
            => RewriteRules.NumericNeat.ApplyOnce(expression.ToEntity());

        [Theory]
        [InlineData("-1 + -1", -2)]
        [InlineData("-2 + -3", -5)]
        [InlineData("-1/2 + -3/2", -2)]
        [InlineData("-1 - (-1)", 0)]
        [InlineData("-3 - (-1)", -2)]
        [InlineData("-1 - (-3)", 2)]
        public void TheValueSurvivesTheRewrite(string expression, int expected)
        {
            var value = Rewritten(expression).EvalNumerical();
            Assert.Equal(expected, value.RealPart.EDecimal.ToDouble(), 9);
            Assert.Equal(0, value.ImaginaryPart.EDecimal.ToDouble(), 9);
        }

        /// <summary>
        /// The branches beside the two that were wrong, so that a later correction cannot fix
        /// one sign by breaking another. Each is a mixed-sign case and each was already right.
        /// </summary>
        [Theory]
        [InlineData("x + -1", "x - 1")]
        [InlineData("-1 + x", "x - 1")]
        [InlineData("x - (-1)", "x + 1")]
        [InlineData("-1 - x", "-(x + 1)")]
        public void TheMixedSignBranchesAreUnchanged(string expression, string expected)
            => Assert.Equal(expected.ToEntity(), Rewritten(expression));

        /// <summary>
        /// And the whole pipeline still answers what it always did — which is the point about
        /// why this hid for so long, stated as a test rather than as a remark.
        /// </summary>
        [Theory]
        [InlineData("-1 + -1", -2)]
        [InlineData("-2 + -3", -5)]
        [InlineData("-1 - (-1)", 0)]
        public void SimplifyWasNeverAffected(string expression, int expected)
            => Assert.Equal(
                Entity.Number.Integer.Create(expected), expression.ToEntity().Simplify());
    }
}
