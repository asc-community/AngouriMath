//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Collections.Generic;
using AngouriMath.Core.Transformations;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    /// <summary>
    /// The cycle that made <c>Common</c> the one rule set with no fixed point, written down as the
    /// shapes it went through. <a href="https://github.com/asc-community/AngouriMath/issues/1056">#1056</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="RuleSetTerminationTest"/> holds the general claim over a generated corpus; this
    /// holds the particular one, because the corpus that finds a cycle is not the thing that says
    /// which rewrite closed it. Printed forms are not enough — two of the three trees printed the
    /// same string — so every assertion here is against a tree.
    /// </para>
    /// <para>
    /// The cycle was
    /// <c>Mulf(-1/2, x)</c> → <c>Mulf(-1, Divf(x, 2))</c> → <c>Divf(Mulf(-1, x), 2)</c> → back,
    /// turned by three rules of which two are exact inverses on this shape. The positive case
    /// never cycled: <c>x / 2</c> is a quotient over a leaf and so does not re-enter the
    /// collection rule's pattern at all. The loop existed only because a negation is spelled as a
    /// product.
    /// </para>
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class CommonTerminatesTest
    {
        /// <summary>
        /// Iterated on its own, with no normalisation to lean on, the set reaches a fixed point.
        /// </summary>
        [Theory]
        [InlineData("-x * 1/2")]
        [InlineData("-1/2 * x")]
        [InlineData("-x / 2")]
        [InlineData("-2 * x / 4")]
        [InlineData("-x * 1/3")]
        [InlineData("x * 1/2")]
        public void CommonReachesAFixedPoint(string source)
        {
            var current = source.ToEntity();
            var seen = new List<Entity> { current };
            for (var pass = 0; pass < 32; pass++)
            {
                var applied = RewriteRules.Common.ApplyOnce(current);
                if (applied.Equals(current))
                    return;
                Assert.DoesNotContain(applied, seen);
                seen.Add(applied);
                current = applied;
            }

            Assert.True(false, "no fixed point in 32 passes: "
                + string.Join(" -> ", seen.ConvertAll(e => e.Stringize())));
        }

        /// <summary>
        /// The rule that gave way, at the shape it gives way on and at the shape it does not. It
        /// collects a numeric factor out of a quotient's numerator, and <c>-1</c> is the sign
        /// rather than a factor to collect — so that one case is declined and every other
        /// coefficient still collects.
        /// </summary>
        [Theory]
        [InlineData("(-1 * x) / 2", null)]
        [InlineData("(-2 * x) / 4", "-1/2 * x")]
        [InlineData("(2 * x) / 4", "1/2 * x")]
        [InlineData("(1 * x) / 2", "1/2 * x")]
        [InlineData("(-3 * x) / 2", "-3/2 * x")]
        public void TheCollectionRuleDeclinesTheSignAndNothingElse(string source, string? expected)
        {
            var rule = System.Linq.Enumerable.Single(
                RewriteRules.Common.Rules,
                r => r.Name == "a-numeric-quotient-of-a-numeric-multiple-collects-its-numbers");
            var applied = rule.TryApply(source.ToEntity());

            if (expected is null)
                Assert.Null(applied);
            else
                Assert.Equal(expected.ToEntity(), applied);
        }
    }
}
