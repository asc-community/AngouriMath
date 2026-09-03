//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.PatternsTest
{
    /// <summary>
    /// <c>sqrt(5 + 2*sqrt(6))</c> is <c>sqrt(2) + sqrt(3)</c>: a radical under a radical, written
    /// without the nesting.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/717">#717</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Squaring <c>sqrt(x) + sqrt(y)</c> gives <c>x + y + 2*sqrt(x*y)</c>, so matching that
    /// against <c>a + b*sqrt(c)</c> makes <c>x</c> and <c>y</c> the roots of
    /// <c>t^2 - a*t + b^2*c/4</c>. They are rational exactly when <c>a^2 - b^2*c</c> is the
    /// square of a rational, which is the whole test — decidable in exact arithmetic, not a
    /// search.
    /// </para>
    /// <para>
    /// The claim under test is <b>the answer squared is the radicand</b>, checked exactly rather
    /// than at sample points, since that is the identity itself and not a consequence of it.
    /// </para>
    /// </remarks>
    [Trait("Area", "Patterns")]
    public sealed class DenestRadicalTest
    {
        /// <summary>Squares the answer and compares it with what was under the root.</summary>
        private static void DenestsTo(string nested, string expected)
        {
            var got = nested.ToEntity().Simplify();
            Assert.Equal(expected.ToEntity().Simplify(), got);

            var radicand = ((Entity.Powf)nested.ToEntity()).Base;
            Assert.Equal(Entity.Number.Integer.Create(0), (got * got - radicand).Simplify());
        }

        /// <summary>The textbook ones, and the shape the issue names.</summary>
        [Theory]
        [InlineData("sqrt(5 + 2 * sqrt(6))", "sqrt(3) + sqrt(2)")]
        [InlineData("sqrt(7 - 4 * sqrt(3))", "2 - sqrt(3)")]
        [InlineData("sqrt(9 + 4 * sqrt(5))", "sqrt(5) + 2")]
        [InlineData("sqrt(11 + 6 * sqrt(2))", "3 + sqrt(2)")]
        [InlineData("sqrt(6 - 2 * sqrt(5))", "sqrt(5) - 1")]
        public void ANestedRadicalComesApart(string nested, string expected)
            => DenestsTo(nested, expected);

        /// <summary>
        /// The sign of the inner coefficient chooses between the sum and the difference, since
        /// squaring either gives <c>a + |b|*sqrt(c)</c>.
        /// </summary>
        [Theory]
        [InlineData("sqrt(7 - 4 * sqrt(3))", 0.2679491924311227)]
        [InlineData("sqrt(6 - 2 * sqrt(5))", 1.2360679774997896)]
        [InlineData("sqrt(5 + 2 * sqrt(6))", 3.1462643699419726)]
        public void TheAnswerHasTheValueTheRadicalHad(string nested, double value)
            => Assert.Equal(value, (double)nested.ToEntity().Simplify().EvalNumerical().RealPart, 12);

        /// <summary>
        /// <b>Where the discriminant is not a square there is nothing to find</b>, and the radical
        /// stays as written rather than acquiring a worse form.
        /// </summary>
        [Theory]
        [InlineData("sqrt(1 + sqrt(2))")]
        [InlineData("sqrt(3 + sqrt(5))")]
        [InlineData("sqrt(1 + 2 * sqrt(7))")]
        public void ARadicandWithNoRationalSplitIsLeftAlone(string nested)
            => Assert.Equal(nested.ToEntity(), nested.ToEntity().Simplify());

        /// <summary>
        /// A negative <c>a</c> is refused: the radicand is then negative wherever the identity
        /// would apply, and the two sides sit on different sides of the branch cut.
        /// </summary>
        [Fact]
        public void ANegativeOuterTermIsRefused()
            => Assert.Null(AngouriMath.Functions.Patterns.DenestRadical("-5 + 2 * sqrt(6)".ToEntity()));

        /// <summary>
        /// What the helper declines outright, so that the rule cannot fire on it: a radicand that
        /// is not <c>a + b*sqrt(c)</c> at all.
        /// </summary>
        [Theory]
        [InlineData("6")]
        [InlineData("x + sqrt(2)")]
        [InlineData("5 + 2 * sqrt(6) + sqrt(7)")]
        [InlineData("5 + 2 * sqrt(4)")]
        public void WhatIsNotANestedRadicalIsDeclined(string radicand)
            => Assert.Null(AngouriMath.Functions.Patterns.DenestRadical(radicand.ToEntity()));

        /// <summary>
        /// <b>The inner root is read however the other rules have left it.</b> They gather
        /// <c>2 * sqrt(2)</c> into <c>2 ^ (3/2)</c>, so a reader that insisted on a written
        /// <c>sqrt</c> would decline a radicand it can denest — the two spellings must agree.
        /// </summary>
        [Fact]
        public void AGatheredRootIsStillARoot()
            => Assert.Equal(
                AngouriMath.Functions.Patterns.DenestRadical("3 + 2 * sqrt(2)".ToEntity()),
                AngouriMath.Functions.Patterns.DenestRadical("3 + 2^(3/2)".ToEntity()));

        /// <summary>
        /// The neighbouring radical rules keep working: one takes a whole power out from under a
        /// root, this one takes a root out from under another, and they do not fight.
        /// </summary>
        [Theory]
        [InlineData("sqrt(4)", "2")]
        [InlineData("sqrt(8)", "2 * sqrt(2)")]
        [InlineData("sqrt(2)", "sqrt(2)")]
        [InlineData("sqrt(x)", "sqrt(x)")]
        public void TheNeighbouringRadicalRulesAreUnchanged(string input, string expected)
            => Assert.Equal(expected.ToEntity().Simplify(), input.ToEntity().Simplify());

        /// <summary>
        /// <b>A denesting that is not shorter is not taken</b>, which is `Simplify` choosing by
        /// size rather than this rule declining. <c>sqrt(2 + sqrt(3))</c> does denest — to
        /// <c>(sqrt(6) + sqrt(2))/2</c> — and the nested form is the smaller of the two, so that
        /// is what comes back.
        /// </summary>
        [Fact]
        public void ADenestingThatIsLongerIsNotChosen()
        {
            Assert.NotNull(AngouriMath.Functions.Patterns.DenestRadical("2 + sqrt(3)".ToEntity()));
            Assert.Equal("sqrt(2 + sqrt(3))".ToEntity(), "sqrt(2 + sqrt(3))".ToEntity().Simplify());
        }

        /// <summary>
        /// A boundary recorded rather than explained. The rule fires on this radicand — applying
        /// the set directly gives <c>sqrt(2) + 1</c>, which is the shorter form by the metric
        /// that ranks candidates — yet <c>Simplify</c> does not offer it among its alternatives
        /// and returns the nested form.
        /// </summary>
        /// <remarks>
        /// It is not the perfect-square collapse re-nesting the radicand: measured,
        /// <c>CollapseToPerfectSquare</c> answers null for <c>3 + 2*sqrt(2)</c>, so that
        /// hypothesis is out rather than unexamined. The cause is somewhere in how the pipeline
        /// reaches this shape, and is not established — which is why this is a test of what
        /// happens and not a claim about why.
        /// </remarks>
        [Fact]
        public void OneRadicandTheRuleAnswersButSimplifyDoesNotTake()
        {
            Assert.Equal("sqrt(2) + 1".ToEntity(), AngouriMath.Functions.Patterns.PowerRules("sqrt(3 + 2 * sqrt(2))".ToEntity()));
            Assert.Equal("sqrt(3 + 2 * sqrt(2))".ToEntity(), "sqrt(3 + 2 * sqrt(2))".ToEntity().Simplify());
        }

        /// <summary>Both spellings of the rule answer alike, which the agreement test also asks.</summary>
        [Theory]
        [InlineData("sqrt(5 + 2 * sqrt(6))")]
        [InlineData("sqrt(3 + 2 * sqrt(2))")]
        [InlineData("sqrt(2 + sqrt(3))")]
        [InlineData("sqrt(1 + sqrt(2))")]
        [InlineData("sqrt(8)")]
        public void TheSwitchAndTheDataAgree(string input)
            => Assert.Equal(
                AngouriMath.Functions.Patterns.PowerRules(input.ToEntity()),
                AngouriMath.Core.Transformations.Matching.MatchedRules.Power.ApplyHere(input.ToEntity()));
    }
}
