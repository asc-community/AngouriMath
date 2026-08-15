//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Linq;
using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// The named cost models: that they are usable, that they differ from one another, and that
    /// the default one is the default rather than a copy of it.
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class CostModelTest
    {
        /// <summary>
        /// The property that makes <see cref="CostModel.Default"/> worth having: it is the very
        /// function the setting already uses, so a caller who names it explicitly changes
        /// nothing. A copy would pass the other tests here and quietly drift later.
        /// </summary>
        [Theory]
        [InlineData("a / b + b / c")]
        [InlineData("1 / (sqrt(3) + 5)")]
        [InlineData("x + 3 / 3 + x ^ 0 - log(e, e2)")]
        [InlineData("(x + y) ^ 3")]
        [InlineData("sin(x) ^ 2 + cos(x) ^ 2")]
        public void NamingTheDefaultChangesNothing(string expression)
        {
            var untouched = expression.ToEntity().Simplify().Stringize();
            using var _ = MathS.Settings.ComplexityCriteria.Set(CostModel.Default.Cost);
            var named = MathS.FromString(expression, useCache: false).Simplify().Stringize();
            Assert.Equal(untouched, named);
        }

        /// <summary>And the rate itself is the same number, not merely the same winner.</summary>
        [Theory]
        [InlineData("a / b + b / c")]
        [InlineData("1 / (sqrt(3) + 5)")]
        [InlineData("-x + 1/2")]
        public void TheDefaultCostIsTheRateTheLibraryUses(string expression)
        {
            var expr = expression.ToEntity();
            Assert.Equal(expr.SimplifiedRate, CostModel.Default.Cost(expr));
        }

        /// <summary>
        /// The documented example, which is the point of the whole feature: a different notion of
        /// simple produces a different answer.
        /// </summary>
        [Fact]
        public void FewestDivisionsClearsTheDivisionsOut()
        {
            Assert.Equal("a / b + b / c", "a / b + b / c".ToEntity().Simplify().Stringize());

            using var _ = MathS.Settings.ComplexityCriteria.Set(CostModel.FewestDivisions.Cost);
            var byFewestDivisions = MathS.FromString("a / b + b / c", useCache: false).Simplify();
            Assert.True(byFewestDivisions.Nodes.Count(node => node is Entity.Divf) < 2,
                $"expected fewer divisions, got {byFewestDivisions.Stringize()}");
        }

        /// <summary>
        /// Every model orders the obvious pair the obvious way, which is the least a cost model
        /// has to do and is worth pinning per model rather than for the default alone.
        /// </summary>
        [Theory]
        [InlineData("x")]
        [InlineData("x + y")]
        [InlineData("sin(x)")]
        public void EveryModelPrefersTheSmallerOfTwoForms(string small)
        {
            var smaller = small.ToEntity();
            Entity bigger = smaller + 0 * MathS.Var("q") * MathS.Var("r");
            foreach (var model in CostModel.All)
                Assert.True(model.Cost(smaller) < model.Cost(bigger),
                    $"{model.Name} did not prefer {smaller.Stringize()} to {bigger.Stringize()}");
        }

        /// <summary>
        /// The models genuinely disagree about the same pair, which is the whole reason for
        /// having more than one.
        /// </summary>
        /// <remarks>
        /// Two ways of writing one expression: two quotients added, or one quotient over a
        /// common denominator. The default prefers the first (24 against 33, because combining
        /// costs nodes) and counting divisions prefers the second (one division against two).
        /// </remarks>
        [Fact]
        public void TheModelsDisagreeWithEachOther()
        {
            var split = "a / b + b / c".ToEntity();
            var combined = "(a * c + b ^ 2) / (b * c)".ToEntity();

            Assert.True(CostModel.Default.Cost(split) < CostModel.Default.Cost(combined),
                "the default should prefer the split form");
            Assert.True(CostModel.FewestDivisions.Cost(combined) < CostModel.FewestDivisions.Cost(split),
                "counting divisions should prefer the common denominator");
        }

        /// <summary>
        /// The default pays nodes to clear a root out of a denominator, which is the preference
        /// #205 put there. A plain node count does not: the two forms tie on size, so the
        /// weighting is the only thing separating them.
        /// </summary>
        [Fact]
        public void OnlyTheDefaultCaresAboutARootInADenominator()
        {
            var withRadicalBelow = "1 / (sqrt(3) + 5)".ToEntity();
            var rationalised = withRadicalBelow.Simplify();

            Assert.True(CostModel.Default.Cost(rationalised) < CostModel.Default.Cost(withRadicalBelow));
            Assert.Equal(CostModel.SmallestTree.Cost(withRadicalBelow),
                CostModel.SmallestTree.Cost(rationalised));
        }

        /// <summary>
        /// A feature model still orders expressions its feature cannot tell apart, because a tie
        /// is settled by whichever candidate was generated first — an accident, not a preference.
        /// </summary>
        [Fact]
        public void AFeatureModelStillBreaksTiesBySize()
        {
            var small = "x + y".ToEntity();
            var large = "x + y + 0 * q * r * s".ToEntity();
            Assert.Equal(0, small.Nodes.Count(n => n is Entity.Divf));
            Assert.Equal(0, large.Nodes.Count(n => n is Entity.Divf));
            Assert.True(CostModel.FewestDivisions.Cost(small) < CostModel.FewestDivisions.Cost(large));
        }

        /// <summary>
        /// https://github.com/asc-community/AngouriMath/issues/950 -- the three are one value
        /// written three ways, and a model that counts divisions has to count the same number in
        /// each. It used to read the exponent's node, so the writing whose division it could not
        /// see scored cheapest, under the criterion whose whole job is removing them.
        /// </summary>
        [Fact]
        public void AFeatureModelCountsTheValueAndNotTheSpelling()
        {
            var written = "x / y".ToEntity();
            var negativePower = MathS.Pow("y".ToEntity(), -1) * "x".ToEntity();
            var unevaluatedExponent = MathS.Pow("y".ToEntity(), "1".ToEntity() * (-1)) * "x".ToEntity();

            // The middle one is the point: its exponent is a Mulf, not a negative Real.
            Assert.IsNotType<Entity.Number>(
                Assert.IsType<Entity.Powf>(unevaluatedExponent.DirectChildren[0]).Exponent);

            // One division each, so the feature term is equal and only the node term separates
            // them -- which means the written form, being smallest, must come out cheapest.
            foreach (var e in new[] { written, negativePower, unevaluatedExponent })
                Assert.True(CostModel.FewestDivisions.Cost(e) >= 1,
                    $"{e.Stringize()} is a division and must be counted as one");
            Assert.True(CostModel.FewestDivisions.Cost(written)
                        < CostModel.FewestDivisions.Cost(unevaluatedExponent),
                "the writing whose division is hardest to see must not be the cheapest");
        }

        /// <summary>The same blind spot, in the other feature model.</summary>
        [Fact]
        public void FewestRadicalsSeesARadicalWhoseExponentIsUnevaluated()
        {
            var root = "sqrt(x)".ToEntity();
            var spelledOut = MathS.Pow("x".ToEntity(), MathS.Pow(2, -1));
            Assert.True(CostModel.FewestRadicals.Cost(root) >= 1);
            Assert.True(CostModel.FewestRadicals.Cost(spelledOut) >= 1,
                "x ^ (2 ^ (-1)) is a square root however the exponent is written");
        }

        /// <summary>
        /// And the other half of #950: a model that counts <i>spelling</i> is right to read the
        /// node as written, so widening the feature models must not have widened these.
        /// </summary>
        [Fact]
        public void ASpellingModelStillCountsHowItIsWritten()
        {
            var written = "x / y".ToEntity();
            var unevaluatedExponent = MathS.Pow("y".ToEntity(), "1".ToEntity() * (-1)) * "x".ToEntity();
            Assert.True(CostModel.SmallestTree.Cost(written)
                        < CostModel.SmallestTree.Cost(unevaluatedExponent),
                "y ^ (1 * (-1)) * x really is a bigger tree, and SmallestTree counts trees");
        }

        /// <summary>They can be listed and named, which is what "as data" buys.</summary>
        [Fact]
        public void TheyCanBeListedAndNamed()
        {
            Assert.Contains(CostModel.Default, CostModel.All);
            Assert.Equal(CostModel.All.Count, CostModel.All.Select(m => m.Name).Distinct().Count());
            foreach (var model in CostModel.All)
            {
                Assert.False(string.IsNullOrWhiteSpace(model.Name));
                Assert.False(string.IsNullOrWhiteSpace(model.Description));
                Assert.Equal(model.Name, model.ToString());
            }
        }
    }
}
