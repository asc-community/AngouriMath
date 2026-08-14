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
    /// <see cref="Transformation.Canonicalization"/>: a canonical form for the commutative
    /// structure. Two expressions differing only in how their operands are arranged come out
    /// as the identical tree. https://github.com/asc-community/AngouriMath/issues/746
    /// </summary>
    /// <remarks>
    /// Everything here compares **entities**. Comparing printed forms would pass whatever the
    /// transformation did, since the printer already reorders and reassociates on its way out —
    /// `(x + y) + a` and `x + (y + a)` print alike and are different trees, which is the single
    /// most likely thing for a test in this area to get wrong.
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class CanonicalizationTest
    {
        private static Entity Canonical(string expression)
            => Transformation.Canonicalization.ApplyOrKeep(expression.ToEntity());

        /// <summary>
        /// The property the form exists for: however the operands were written, the tree is
        /// the same one.
        /// </summary>
        [Theory]
        [InlineData("x + y", "y + x")]
        [InlineData("x * y", "y * x")]
        [InlineData("x + y + a", "a + y + x")]
        [InlineData("x * y * a", "a * y * x")]
        [InlineData("1/2 - x", "-x + 1/2")]
        [InlineData("x + sqrt(a)", "sqrt(a) + x")]
        [InlineData("x * sin(y)", "sin(y) * x")]
        [InlineData("x and y", "y and x")]
        [InlineData("x or y", "y or x")]
        public void OperandOrderDoesNotSurvive(string left, string right)
            => Assert.Equal(Canonical(left), Canonical(right));

        /// <summary>
        /// Canonicalising a canonical form leaves it alone. Ordering *without* the leading
        /// normalisation does not have this property: the sort's key depends on a node's class
        /// and the normalisation changes classes, so `1/2 - x` alternates for ever.
        /// </summary>
        [Theory]
        [InlineData("1/2 - x")]
        [InlineData("0 ^ x")]
        [InlineData("x + y")]
        [InlineData("x * y * a")]
        [InlineData("sqrt(1/2) + x")]
        [InlineData("x + 1 * 1 / 2")]
        [InlineData("(x + y) * (a - 1/2)")]
        [InlineData("sin(x) + cos(x) + 1/3")]
        public void CanonicalizingTwiceIsCanonicalizingOnce(string expression)
        {
            var once = Canonical(expression);
            Assert.Equal(once, Transformation.Canonicalization.ApplyOrKeep(once));
        }

        /// <summary>
        /// It is a form, not a simplification: the value is unchanged, which is checked the way
        /// this repository checks an identity — subtract and simplify to zero.
        /// </summary>
        [Theory]
        [InlineData("1/2 - x")]
        [InlineData("x + y + a")]
        [InlineData("x * y * a")]
        [InlineData("(x + y) * (a - 1/2)")]
        [InlineData("sin(x) + cos(x) + 1/3")]
        public void TheValueIsUnchanged(string expression)
        {
            var difference = (expression.ToEntity() - Canonical(expression)).Simplify();
            while (difference is Entity.Providedf(var inner, _)) difference = inner;
            Assert.Equal(Entity.Number.Integer.Create(0), difference);
        }

        /// <summary>
        /// Nesting goes as well as order: the sort works over commutative *chains*, so it
        /// flattens as it sorts. `(x + y) + a` and `x + (y + a)` both reach `a + x + y`, and
        /// they reach it as the same tree rather than merely printing alike — which is the
        /// distinction that matters, since `InnerSimplified` alone leaves them as two trees
        /// that print identically.
        /// </summary>
        [Theory]
        [InlineData("(x + y) + a", "x + (y + a)")]
        [InlineData("(x * y) * a", "x * (y * a)")]
        [InlineData("((x + y) + a) + b", "x + (y + (a + b))")]
        [InlineData("(x + y) + a", "a + (y + x)")]
        public void AssociativityGoesToo(string left, string right)
            => Assert.Equal(Canonical(left), Canonical(right));

        /// <summary>
        /// And the pair that motivated all of this, stated as the two facts side by side: the
        /// normalisation alone leaves two trees that print the same, and the canonicalisation
        /// makes them one tree.
        /// </summary>
        [Fact]
        public void TheTwoTreesThatPrintAlikeBecomeOne()
        {
            var left = "(x + y) + a".ToEntity();
            var right = "x + (y + a)".ToEntity();
            Assert.Equal(left.InnerSimplified.Stringize(), right.InnerSimplified.Stringize());
            Assert.NotEqual(left.InnerSimplified, right.InnerSimplified);
            Assert.Equal(Canonical("(x + y) + a"), Canonical("x + (y + a)"));
        }

        /// <summary>
        /// And it is not applied by anything: asking for a simplification must not start
        /// returning canonicalised trees behind the caller's back.
        /// </summary>
        [Fact]
        public void NothingRunsItByDefault()
            => Assert.Equal("y + x".ToEntity().InnerSimplified, "y + x".ToEntity());
    }
}
