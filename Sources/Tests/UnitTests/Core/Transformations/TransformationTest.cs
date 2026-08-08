//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;
using System.Linq;
using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Core.Transformations;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    /// <summary>
    /// The transformation layer: that it does what it says, that the 1.x methods built on
    /// it still answer what they answered, and that it is honest about what it could not do.
    /// </summary>
    [Trait("Area", "Transformations")]
    public sealed class TransformationTest
    {
        /// <summary>
        /// Ordinary textbook-sized expressions, of the kinds the rule sets below are about.
        /// </summary>
        public static readonly IEnumerable<object[]> Corpus = new[]
        {
            "x + 0",
            "x * 1",
            "x - x",
            "(x + 1) ^ 2",
            "(x + y) * (x - y)",
            "x * y + y + x + 1",
            "sin(x) ^ 2 + cos(x) ^ 2",
            "a / b / c",
            "1 / (sqrt(3) + 5)",
            "(x ^ 3 + 3 * x ^ 2 * y + 3 * x * y ^ 2 + y ^ 3) / (x + y)",
            "2 * x + 3 * x",
            "sqrt(12) + sqrt(27)",
        }.Select(x => new object[] { x }).ToArray();

        private static Entity Parse(string raw) => MathS.FromString(raw);

        #region The abstraction itself

        [Fact]
        public void ATransformationReportsWhatItIsAndWhatItDid()
        {
            var result = Transformation.Expansion.Apply(Parse("(x + 1) ^ 2"));

            Assert.True(result.Succeeded);
            Assert.True(result.Changed);
            Assert.Equal(Transformation.Expansion, result.Transformation);
            Assert.Equal(TransformationRelation.Equivalence, result.Relation);
            Assert.Equal(Soundness.SoundUnderAssumptions, result.Soundness);
            Assert.Equal(Parse("(x + 1) ^ 2"), result.Input);
        }

        [Fact]
        public void AFixedPointSucceedsWithoutChangingAnything()
        {
            // Nothing in the common rules matches a bare variable, so the pass answers with
            // what it was given. That is a fixed point, not a failure.
            var result = Transformation.Rewriting(RewriteRules.Common).Apply(Parse("x"));

            Assert.True(result.Succeeded);
            Assert.False(result.Changed);
            Assert.Equal(Parse("x"), result.Output);
        }

        [Fact]
        public void ApplyOrKeepHandsBackTheInputWhereThereIsNoAnswer()
        {
            var unanswerable = Transformation.Integration("x").Apply(Parse("e ^ (x ^ 2)"));
            Assert.False(unanswerable.Succeeded);
            Assert.Equal(Parse("e ^ (x ^ 2)"), unanswerable.OutputOrInput);
        }

        [Fact]
        public void ATransformationRefusesANullInput()
            => Assert.Throws<ArgumentNullException>(() => Transformation.Simplification.Apply(null!));

        #endregion

        #region Composition

        [Fact]
        public void AChainRunsItsPartsInOrder()
        {
            var expandThenFactor = Transformation.Expansion.Then(Transformation.Factorization);
            var factorThenExpand = Transformation.Factorization.Then(Transformation.Expansion);

            // Not an inverse pair, and the names say which way round each one is.
            Assert.Equal("expand[2] then factorize", Shorten(expandThenFactor.Name));
            Assert.Equal("factorize then expand[2]", Shorten(factorThenExpand.Name));

            static string Shorten(string name)
                => name.Replace("rewrite[PerfectSquare] then rewrite[Factorization] then inner-simplify x2", "factorize");
        }

        [Fact]
        public void AChainWithADerivationInItIsNotAnEquivalence()
        {
            var chain = Transformation.Expansion.Then(Transformation.Differentiation("x"));

            Assert.Equal(TransformationRelation.Equivalence, Transformation.Expansion.Relation);
            Assert.Equal(TransformationRelation.Derivation, chain.Relation);
        }

        [Fact]
        public void AChainCarriesTheWeakerOfTheTwoJustifications()
        {
            // Substitution is unconditional; expansion is not. Composing them cannot make
            // the pair unconditional again.
            var chain = Transformation.Substitution("x", 3).Then(Transformation.Expansion);

            Assert.Equal(Soundness.Sound, Transformation.Substitution("x", 3).Soundness);
            Assert.Equal(Soundness.SoundUnderAssumptions, chain.Soundness);
        }

        [Fact]
        public void ANoAnswerAnywhereInAChainIsANoAnswerForTheChain()
        {
            // The integral has no closed form, so nothing downstream of it can have run.
            var chain = Transformation.Integration("x").Then(Transformation.Simplification);

            Assert.False(chain.Apply(Parse("e ^ (x ^ 2)")).Succeeded);
        }

        [Fact]
        public void RepeatingSomethingZeroTimesChangesNothing()
            => Assert.Equal(
                Parse("(x + 1) ^ 2"),
                Transformation.Expansion.Repeat(0).Apply(Parse("(x + 1) ^ 2")).Output);

        [Fact]
        public void HittingTheBoundBeforeStabilisingIsReportedAsNoAnswer()
        {
            // One iteration expands the square, so a single application cannot yet show
            // that another one would change nothing.
            var tooFewIterations = Transformation.Expansion.UntilStable(1);
            Assert.False(tooFewIterations.Apply(Parse("(x + 1) ^ 2")).Succeeded);

            // Given room, it settles.
            var enough = Transformation.Expansion.UntilStable(16);
            Assert.True(enough.Apply(Parse("(x + 1) ^ 2")).Succeeded);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void UntilStableRefusesAnUnboundedLoop(int bound)
            => Assert.Throws<ArgumentOutOfRangeException>(() => Transformation.Simplification.UntilStable(bound));

        #endregion

        #region The rule registry

        [Fact]
        public void TheRegistryIsEnumerableAndItsOrderIsFixed()
        {
            Assert.NotEmpty(RewriteRules.All);
            Assert.Equal(RewriteRules.All.Select(r => r.Name), RewriteRules.All.Select(r => r.Name));
            Assert.Equal(RewriteRules.CanonicalOrder, RewriteRules.All[0]);
            Assert.Equal(RewriteRules.All.Count, RewriteRules.All.Select(r => r.Name).Distinct().Count());
        }

        [Fact]
        public void EveryRuleSetSaysWhatItIsAndWhatItClaims()
        {
            foreach (var ruleSet in RewriteRules.All)
            {
                Assert.False(string.IsNullOrWhiteSpace(ruleSet.Name));
                Assert.False(string.IsNullOrWhiteSpace(ruleSet.Description));
                // Nothing in the registry may quietly claim a proof it has not got.
                Assert.NotEqual(Soundness.Sound, ruleSet.Soundness);
            }
        }

        [Fact]
        public void TheRegistryAndTheCatalogueAreBothFullyBuilt()
        {
            // A smoke check, and it is here because the failure it catches is not a failing
            // assertion: RewriteRuleSet builds its transformation on demand precisely so
            // that constructing a rule set does not run Transformation's static
            // initialiser, which reads the registry back. Tie those two together and
            // whichever type is touched second reads the other's fields before they are
            // set, so the entries below come out null and every call that reaches them
            // dies -- in whichever order the process happened to load them.
            foreach (var ruleSet in RewriteRules.All)
                Assert.NotNull(ruleSet.AsTransformation());

            Assert.NotNull(Transformation.Simplification);
            Assert.NotNull(Transformation.Expansion);
            Assert.NotNull(Transformation.Factorization);
            Assert.NotNull(Transformation.Normalization);
            Assert.NotNull(Transformation.Rationalisation);
            Assert.NotNull(Transformation.InnerSimplification);
        }

        [Theory]
        [MemberData(nameof(Corpus))]
        public void NoRuleSetRewritesInACycle(string raw)
        {
            var expr = Parse(raw);
            foreach (var ruleSet in RewriteRules.All)
                Assert.True(
                    ruleSet.AsTransformation().UntilStable(32).Apply(expr).Succeeded,
                    $"{ruleSet.Name} did not reach a fixed point on {raw} within 32 passes");
        }

        [Theory]
        [MemberData(nameof(Corpus))]
        public void ARuleSetGivesTheSameAnswerEveryTime(string raw)
        {
            var expr = Parse(raw);
            foreach (var ruleSet in RewriteRules.All)
                Assert.Equal(ruleSet.ApplyOnce(expr), ruleSet.ApplyOnce(expr));
        }

        #endregion

        #region The 1.x methods answer what they answered

        [Theory]
        [MemberData(nameof(Corpus))]
        public void SimplifyIsItsTransformation(string raw)
            => Assert.Equal(Parse(raw).Simplify(), Transformation.Simplification.Apply(Parse(raw)).Output);

        [Theory]
        [MemberData(nameof(Corpus))]
        public void ExpandIsItsTransformation(string raw)
            => Assert.Equal(Parse(raw).Expand(), Transformation.Expansion.Apply(Parse(raw)).Output);

        [Theory]
        [MemberData(nameof(Corpus))]
        public void FactorizeIsItsTransformation(string raw)
            => Assert.Equal(Parse(raw).Factorize(), Transformation.Factorization.Apply(Parse(raw)).Output);

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void FactorizeRunsThePassAsManyTimesAsItIsAsked(int level)
            => Assert.Equal(
                Parse("x * y + y + 1 + x").Factorize(level),
                Transformation.FactorizationAtLevel(level).Apply(Parse("x * y + y + 1 + x")).Output);

        [Theory]
        [MemberData(nameof(Corpus))]
        public void DifferentiateIsItsTransformation(string raw)
            => Assert.Equal(
                Parse(raw).Differentiate("x"),
                Transformation.Differentiation("x").Apply(Parse(raw)).Output);

        [Theory]
        [InlineData("x")]
        [InlineData("sin(x)")]
        [InlineData("1 / x")]
        public void IntegrateIsItsTransformationPlusTheConstant(string raw)
        {
            var antiderivative = Transformation.Integration("x").Apply(Parse(raw)).Output;
            Assert.NotNull(antiderivative);
            Assert.Equal(Parse(raw).Integrate("x"), antiderivative! + (Entity)"C");
        }

        [Theory]
        [InlineData("sin(x) / x", "0")]
        [InlineData("(1 + 1 / x) ^ x", "+oo")]
        public void LimitIsItsTransformationTidiedUp(string raw, string destination)
        {
            var computed = Transformation.LimitAt("x", destination, ApproachFrom.BothSides).Apply(Parse(raw)).Output;
            Assert.NotNull(computed);
            Assert.Equal(Parse(raw).Limit("x", destination, ApproachFrom.BothSides), computed!.InnerSimplified);
        }

        [Theory]
        [MemberData(nameof(Corpus))]
        public void SubstituteIsItsTransformation(string raw)
            => Assert.Equal(
                Parse(raw).Substitute("x", 3),
                Transformation.Substitution("x", 3).Apply(Parse(raw)).Output);

        #endregion

        #region Honesty

        [Fact]
        public void AnIntegralWithNoClosedFormHasNoAnswerRatherThanAWrongOne()
        {
            var result = Transformation.Integration("x").Apply(Parse("e ^ (x ^ 2)"));

            Assert.False(result.Succeeded);
            Assert.Null(result.Output);

            // The 1.x method makes the same claim in the shape its callers expect: an
            // unevaluated node, which is "I could not settle this" and not NaN.
            var legacy = Parse("e ^ (x ^ 2)").Integrate("x");
            Assert.IsType<Entity.Integralf>(legacy);
            Assert.False(legacy.IsNaN);
        }

        [Fact]
        public void ALimitThatCannotBeSettledHasNoAnswerRatherThanNaN()
        {
            // The documented unevaluated case: see the example on Entity.Limit(Variable, Entity).
            var expr = Parse("sin(x * a) / x");
            var result = Transformation.LimitAt("x", "+oo", ApproachFrom.BothSides).Apply(expr);

            Assert.False(result.Succeeded);

            var legacy = expr.Limit("x", "+oo", ApproachFrom.BothSides);
            Assert.IsType<Entity.Limitf>(legacy);
            Assert.False(legacy.IsNaN);
        }

        #endregion

        #region Determinism and the relation each transformation claims

        [Theory]
        [MemberData(nameof(Corpus))]
        public void TheSameTransformationOnTheSameInputGivesTheSameAnswer(string raw)
        {
            var expr = Parse(raw);
            foreach (var transformation in new[]
                     {
                         Transformation.Simplification,
                         Transformation.Expansion,
                         Transformation.Factorization,
                         Transformation.Normalization,
                         Transformation.Rationalisation,
                         Transformation.InnerSimplification,
                     })
                Assert.Equal(transformation.Apply(expr).Output, transformation.Apply(expr).Output);
        }

        [Theory]
        [MemberData(nameof(Corpus))]
        public void AnEquivalenceTransformationDoesNotChangeTheValue(string raw)
        {
            var expr = Parse(raw);
            foreach (var transformation in new[]
                     {
                         Transformation.Expansion,
                         Transformation.Factorization,
                         Transformation.Normalization,
                         Transformation.Rationalisation,
                         Transformation.InnerSimplification,
                     })
            {
                Assert.Equal(TransformationRelation.Equivalence, transformation.Relation);
                if (transformation.Apply(expr).Output is not { } output)
                    continue;
                // The property, not the printed form: subtract the two sides and simplify.
                // A domain condition may survive that -- the two sides agree only where both
                // are defined, which is exactly what SoundUnderAssumptions says -- so the
                // condition is stripped before the value is read.
                var difference = (expr - output).Simplify();
                if (difference is Entity.Providedf(var value, _))
                    difference = value;
                Assert.True(
                    difference == 0,
                    $"{transformation.Name} changed the value of {raw}: difference simplified to {difference}");
            }
        }

        [Fact]
        public void SubstitutionIsTheOneThingHereThatNeedsNoAssumptions()
        {
            var substitution = Transformation.Substitution("x", 3);
            Assert.Equal(Soundness.Sound, substitution.Soundness);
            // and it is a different object, not another way of writing the same one
            Assert.Equal(TransformationRelation.Derivation, substitution.Relation);
        }

        [Theory]
        [MemberData(nameof(Corpus))]
        public void SimplifyingAnAlreadySimplifiedExpressionChangesNothing(string raw)
        {
            var once = Parse(raw).Simplify();
            Assert.Equal(once, once.Simplify());
        }

        #endregion

        #region The transformations this layer added

        [Fact]
        public void NormalizationMakesTwoArrangementsOfOneExpressionTheSameTree()
        {
            var oneWay = Transformation.Normalization.Apply(Parse("x + y + z")).Output;
            var another = Transformation.Normalization.Apply(Parse("z + y + x")).Output;

            Assert.Equal(oneWay, another);
            // and it is not simplification: the two were already as short as they get
            Assert.NotEqual(Parse("x + y + z"), Parse("z + y + x"));
        }

        [Fact]
        public void RationalisationClearsASurdOutOfADenominator()
        {
            var result = Transformation.Rationalisation.Apply(Parse("1 / (sqrt(3) + 5)"));

            Assert.True(result.Succeeded);
            Assert.DoesNotContain(
                result.Output!.Nodes,
                node => node is Entity.Divf(_, var denominator) && denominator.Nodes.Any(n => n is Entity.Powf));
        }

        #endregion
    }
}
