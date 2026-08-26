//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Collections.Generic;
using AngouriMath;
using AngouriMath.Core.Transformations;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.PatternsTest
{
    /// <summary>
    /// <c>a &lt; b or a = b</c> is <c>a &lt;= b</c>, and the same law written with the comparison
    /// the other way round answers the other way round. Four of the eight arms that say this
    /// carried their neighbour's answer.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1077">#1077</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Checked against the <b>truth value</b> at sample points rather than against an expected
    /// string. A wrong rewrite here is a well-formed comparison, so a test that reads the answer
    /// records whichever answer it was given; a test that evaluates both sides cannot.
    /// </para>
    /// <para>
    /// Both operands are symbolic, which is the only way to reach these rules at all: with a
    /// number on one side, <c>2 &lt; x</c> is rewritten to <c>x &gt; 2</c> earlier in the same
    /// pass, so the disjunction is only ever seen with both halves written the same way round.
    /// That is why four wrong arms survived — and why this test would have passed on them had it
    /// used a numeric operand.
    /// </para>
    /// </remarks>
    [Trait("Area", "Patterns")]
    public sealed class OrWithEqualityTest
    {
        private static readonly string[] Shapes =
        {
            "(x < y) or (x = y)", "(y < x) or (x = y)",
            "(x > y) or (x = y)", "(y > x) or (x = y)",
            "(x = y) or (x < y)", "(x = y) or (y < x)",
            "(x = y) or (x > y)", "(x = y) or (y > x)",
        };

        // Below, on and above the diagonal: a flipped comparison agrees on the diagonal, where
        // both sides are True, and disagrees on either side of it.
        private static readonly (string X, string Y)[] Points =
        {
            ("1", "2"), ("2", "2"), ("3", "2"), ("-1", "1"), ("0", "0"), ("5", "-5"),
        };

        public static IEnumerable<object[]> Cases()
        {
            foreach (var shape in Shapes)
                foreach (var (x, y) in Points)
                    yield return new object[] { shape, x, y };
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void TheDisjunctionKeepsItsTruthValue(string shape, string x, string y)
        {
            var original = shape.ToEntity();
            var rewritten = RewriteRules.InequalityEquality.ApplyOnce(original);
            Assert.NotEqual(original, rewritten);
            Assert.Equal(
                original.Substitute("x", x.ToEntity()).Substitute("y", y.ToEntity()).EvalBoolean(),
                rewritten.Substitute("x", x.ToEntity()).Substitute("y", y.ToEntity()).EvalBoolean());
        }
    }
}
