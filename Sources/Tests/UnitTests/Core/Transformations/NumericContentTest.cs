//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Transformations;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    /// <summary>
    /// The whole number every term of a sum divides by, taken out in front of it —
    /// <a href="https://github.com/asc-community/AngouriMath/issues/195">#195</a>'s "forcefully".
    /// </summary>
    /// <remarks>
    /// <para>
    /// The issue asks for <c>2x + 2a</c> to be collected "forcefully… but not peacefully", and that
    /// half stopped being true some time ago: an <i>identical</i> factor comes out under plain
    /// <see cref="Entity.Simplify(int)"/>. What is forceful is a common <b>divisor</b>, because
    /// <c>2 * (x + 2 * a)</c> is a node larger than <c>4 * a + 2 * x</c> and the cost model will
    /// not choose it.
    /// </para>
    /// <para>
    /// So it runs in <see cref="Entity.Factorize(int)"/> and not in <c>Simplify</c>. Both halves of
    /// that are asserted here — the second is the one that would go wrong quietly.
    /// </para>
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class NumericContentTest
    {
        [Theory]
        [InlineData("2 * x + 4 * a", "2 * (x + 2 * a)")]
        [InlineData("6 * x + 9 * a", "3 * (2 * x + 3 * a)")]
        [InlineData("2 * x + 4", "2 * (x + 2)")]
        [InlineData("4 * x + 6 * y + 10 * z", "2 * (2 * x + 3 * y + 5 * z)")]
        [InlineData("2 * sin(x) + 4 * cos(x)", "2 * (sin(x) + 2 * cos(x))")]
        public void FactorizeTakesTheContentOut(string expression, string expected)
            => Assert.Equal(expected.ToEntity(), expression.ToEntity().Factorize());

        /// <summary>
        /// A difference is a sum of a negated term, and the sign is carried as a sign rather than
        /// by negating the term.
        /// </summary>
        /// <remarks>
        /// Negating the term changes its shape — <c>-(4 * a)</c> comes back as a product by -1
        /// wrapping a product by 4, whose coefficient then reads as -1 and takes the content of
        /// every difference down to 1. That is what the first version did, and every one of these
        /// came back unchanged.
        /// </remarks>
        [Theory]
        [InlineData("2 * x - 4 * a", "2 * (x - 2 * a)")]
        [InlineData("-2 * x - 4 * a", "2 * (-x - 2 * a)")]
        [InlineData("6 * x - 9 * a", "3 * (2 * x - 3 * a)")]
        public void ADifferenceIsASumOfANegatedTerm(string expression, string expected)
            => Assert.Equal(expected.ToEntity(), expression.ToEntity().Factorize());

        /// <summary>
        /// <b>What it leaves alone, which is most things.</b>
        /// </summary>
        /// <remarks>
        /// A content of one is not a factor. A coefficient that is not a whole number makes the
        /// question unanswerable rather than answering one: the content of <c>2x + a/3</c> is not
        /// 1, it is a thing this does not compute, and taking 1 out would say there was nothing to
        /// take.
        /// </remarks>
        [Theory]
        [InlineData("2 * x + 3 * a")]
        [InlineData("x + a")]
        [InlineData("x / 2 + a / 3")]
        [InlineData("2 * x + a / 3")]
        public void WhereThereIsNoContentNothingMoves(string expression)
            => Assert.Equal(expression.ToEntity(), expression.ToEntity().Factorize());

        /// <summary>
        /// <b><c>Simplify</c> is untouched, which is the whole point of putting this in
        /// <c>Factorize</c>.</b>
        /// </summary>
        /// <remarks>
        /// The peaceful behaviour is asserted as it is: <c>2x + 2a</c> is collected because the
        /// factor is identical, and <c>2x + 4a</c> is not because it is only a divisor and the
        /// result is larger. If a later change makes <c>Simplify</c> take the content out, this is
        /// what says so.
        /// </remarks>
        [Theory]
        [InlineData("2 * x + 4 * a", "4 * a + 2 * x")]
        [InlineData("6 * x + 9 * a", "9 * a + 6 * x")]
        [InlineData("4 * x + 6 * y + 10 * z", "4 * x + 6 * y + 10 * z")]
        [InlineData("2 * x + 2 * a", "2 * (a + x)")]
        public void SimplifyStaysPeaceful(string expression, string expected)
            => Assert.Equal(expected.ToEntity(), expression.ToEntity().Simplify());

        /// <summary>
        /// The transformation on its own, since it is offered as one.
        /// </summary>
        [Fact]
        public void ItIsAlsoATransformationOfItsOwn()
        {
            var taken = Transformation.NumericContentExtraction;
            Assert.Equal("numeric-content", taken.Name);
            Assert.Equal(TransformationRelation.Equivalence, taken.Relation);
            Assert.Equal(Soundness.Sound, taken.Soundness);
            Assert.Equal("2 * (x + 2 * a)".ToEntity(), taken.ApplyOrKeep("2 * x + 4 * a".ToEntity()));
            // And it steps aside rather than refusing, so that composing it cannot lose the
            // answer the step before it gave: `Then` reads a null from its second half as the
            // whole chain having no answer.
            Assert.Equal("x + a".ToEntity(), taken.ApplyOrKeep("x + a".ToEntity()));
            Assert.NotNull(taken.Apply("x + a".ToEntity()).Output);
        }

        /// <summary>
        /// It is the last step of factorisation, so what it takes out is on top of everything the
        /// rules and the polynomial layer already did.
        /// </summary>
        /// <remarks>
        /// Compared as printed forms, which is the wrong default and right here: the two are the
        /// same product associated differently, and what this asserts is that the content came out
        /// on top of the factorisation rather than which way the resulting chain is nested.
        /// </remarks>
        [Fact]
        public void ItComesAfterTheRestOfFactorization()
            => Assert.Equal("2 * (x - 1) * (x + 1)",
                "2 * x ^ 2 - 2".ToEntity().Factorize().Stringize());
    }
}
