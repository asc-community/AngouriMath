//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath;
using AngouriMath.Core.Transformations;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.PatternsTest
{
    /// <summary>
    /// <c>ln(1/b) = -ln(b)</c> was applied for every <c>b</c>, and it is false on the negative
    /// reals: the principal argument does not negate with its logarithm. At <c>b = -0.63</c>,
    /// <c>ln(1/b)</c> is <c>0.462 + pi*i</c> and <c>-ln(b)</c> is <c>0.462 - pi*i</c>.
    /// https://github.com/asc-community/AngouriMath/issues/1062
    /// </summary>
    /// <remarks>
    /// <para>
    /// The same mistake as the logarithm gathering ten lines below it in <c>PowerRules</c>,
    /// which was guarded while these three arms were not — the third time in this file's history
    /// that a branch-cut fix has stopped at the rule it was reported against and left its
    /// neighbours alone, after
    /// <a href="https://github.com/asc-community/AngouriMath/issues/752">#752</a> and
    /// <a href="https://github.com/asc-community/AngouriMath/issues/801">#801</a>.
    /// </para>
    /// <para>
    /// Asserted against <see cref="RewriteRules.Power"/> rather than against
    /// <see cref="Entity.Simplify"/>, because <c>Simplify</c> never picked this branch and so
    /// would pass whatever the rule did. That is what made the defect invisible to
    /// <c>boundcheck</c>, which measures <c>Simplify</c>.
    /// </para>
    /// </remarks>
    [Trait("Area", "PatternsTest")]
    public sealed class ReciprocalLogarithmBranchTest
    {
        /// <summary>
        /// The rule set must not change the value at a negative argument. Compared against the
        /// expression it came from rather than against an expected form: what matters is that
        /// rewriting did not move the point.
        /// </summary>
        [Theory]
        [InlineData("ln(1 / x)")]
        [InlineData("log(2, 1 / x)")]
        [InlineData("log(1 / x, 2)")]
        [InlineData("log(1 / x, 1 / y)")]
        public void RewritingKeepsTheValueAtANegativeArgument(string expr)
        {
            var rewritten = RewriteRules.Power.ApplyOnce(expr.ToEntity());
            foreach (var (x, y) in new[] { (-0.63, -1.7), (-1.0, -2.0), (-2.5, -0.4), (-0.25, 3.0) })
            {
                var before = expr.ToEntity().Substitute("x", x).Substitute("y", y).EvalNumerical();
                var after = rewritten.Substitute("x", x).Substitute("y", y).EvalNumerical();
                var difference =
                    Math.Abs(before.RealPart.EDecimal.ToDouble() - after.RealPart.EDecimal.ToDouble())
                    + Math.Abs(before.ImaginaryPart.EDecimal.ToDouble() - after.ImaginaryPart.EDecimal.ToDouble());
                Assert.True(difference < 1e-9,
                    $"{expr} was rewritten to {rewritten.Stringize()}, which at x = {x}, y = {y} "
                    + $"is {after.Stringize()} rather than {before.Stringize()}");
            }
        }

        /// <summary>
        /// The exact point the issue names, stated as itself rather than as a tolerance: the
        /// imaginary parts must not be each other's negation.
        /// </summary>
        [Fact]
        public void TheArgumentDoesNotNegateWithItsLogarithm()
        {
            var at = Entity.Number.Real.Create(PeterO.Numbers.EDecimal.FromString("-0.63"));
            var direct = "ln(1 / x)".ToEntity().Substitute("x", at).EvalNumerical();
            var negated = "-ln(x)".ToEntity().Substitute("x", at).EvalNumerical();
            Assert.True(
                Math.Abs(direct.ImaginaryPart.EDecimal.ToDouble()
                         + negated.ImaginaryPart.EDecimal.ToDouble()) < 1e-9,
                "the premise of this test has changed: the two are supposed to differ by the "
                + $"sign of the imaginary part, and they are {direct.Stringize()} and {negated.Stringize()}");
            Assert.NotEqual(direct.Stringize(), negated.Stringize());
        }

        /// <summary>
        /// The rule must keep firing where it is sound, or this would be a fix that removes an
        /// answer. A positive real argument is safe, and that is the condition the guard asks.
        /// </summary>
        [Theory]
        [InlineData("ln(1 / 2.5)", "-ln(5/2)")]
        [InlineData("log(3, 1 / 2.5)", "-log(3, 5/2)")]
        public void TheSoundCasesStillRewrite(string expr, string expected)
            => Assert.Equal(expected.ToEntity(), RewriteRules.Power.ApplyOnce(expr.ToEntity()));
    }
}
