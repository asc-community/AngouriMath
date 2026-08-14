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
            // Not arithmetic, so that the boolean, comparison, set, factorial and totient
            // rule sets are exercised rather than passing over inputs they cannot match.
            "a and b or a and not b",
            "x > 3 and x < 5",
            "{ 1, 2 } unite { 2, 3 }",
            // Not (x + 1)! / x!, which crashes Expand:
            // https://github.com/asc-community/AngouriMath/issues/817
            "x! * (x + 1)",
            "phi(12)",
            "tan(x) * cot(x)",
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
            Assert.NotNull(Transformation.Rationalization);
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

        // All three take a level, so all three are checked at one -- including the levels
        // outside the range the catalogue keeps built, and the negative ones Simplify passes
        // itself when it re-simplifies a candidate.
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
        [InlineData(-2)]
        [InlineData(-1)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(6)]
        public void SimplifyIsItsTransformationAtEveryLevel(int level)
            => Assert.Equal(
                Parse("sin(x) / tan(x) + a / (b / c)").Simplify(level),
                Transformation.SimplificationAtLevel(level).Apply(Parse("sin(x) / tan(x) + a / (b / c)")).Output);

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(6)]
        public void ExpandIsItsTransformationAtEveryLevel(int level)
            => Assert.Equal(
                Parse("(x + y) ^ 3 * (a + b)").Expand(level),
                Transformation.ExpansionAtLevel(level).Apply(Parse("(x + y) ^ 3 * (a + b)")).Output);

        [Theory]
        [InlineData(6)]
        [InlineData(-6)]
        public void ALevelOutsideTheCachedRangeStillWorks(int level)
        {
            // The catalogue keeps -4..4 built and constructs anything else on the spot; a
            // level outside that range must behave the same, not merely not throw.
            var built = Transformation.FactorizationAtLevel(level);
            Assert.Equal(Parse("x * y + y + 1 + x").Factorize(level), built.Apply(Parse("x * y + y + 1 + x")).Output);
        }

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
                         Transformation.Rationalization,
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
                         Transformation.Rationalization,
                         Transformation.InnerSimplification,
                     })
            {
                Assert.Equal(TransformationRelation.Equivalence, transformation.Relation);
                if (transformation.Apply(expr).Output is not { } output)
                    continue;

                // Subtracting a set from a set is elementwise, so the difference of two
                // equal sets is the set of pairwise differences and not zero. The property
                // is about expressions that denote a value; for the rest, the strongest
                // honest statement is that the two simplify to the same thing.
                if (expr is Entity.Set || output is Entity.Set)
                {
                    Assert.Equal(expr.Simplify(), output.Simplify());
                    continue;
                }

                // The property, not the printed form: subtract the two sides and simplify.
                // A domain condition may survive that -- the two sides agree only where both
                // are defined, which is exactly what SoundUnderAssumptions says -- so the
                // condition is stripped before the value is read.
                var difference = (expr - output).Simplify();
                if (difference is Entity.Providedf(var value, _))
                    difference = value;
                if (difference == 0)
                    continue;

                // Simplifying the difference to zero proves the two agree; failing to is not
                // proof that they differ, only that this simplifier could not settle it.
                // x! * (x + 1) expands to (x + 1)!, which is right, and the difference of the
                // two does not reduce. So the fallback looks for an actual counterexample
                // instead of reporting the unproven case as a defect.
                Assert.False(
                    DisagreesAtSomePoint(expr, output),
                    $"{transformation.Name} changed the value of {raw}: it gives {output.Stringize()}, "
                    + "and the two take different values at a point where both are defined");
            }
        }

        /// <summary>
        /// Whether the two take different values somewhere both are defined — a
        /// counterexample to their being the same expression written two ways.
        /// </summary>
        /// <remarks>
        /// Points where either side fails to evaluate, or comes out infinite or NaN, say
        /// nothing: a rewrite that is valid away from a pole is exactly what
        /// <see cref="Soundness.SoundUnderAssumptions"/> means, so those are passed over
        /// rather than counted against it. Small positive integers, since factorials and
        /// logarithms are only defined on part of the line and 0 and 1 are degenerate for
        /// both.
        /// </remarks>
        private static bool DisagreesAtSomePoint(Entity one, Entity another)
        {
            var variables = one.Vars.Concat(another.Vars).Distinct().ToList();
            foreach (var point in new[] { 2, 3, 5 })
            {
                Entity left = one, right = another;
                foreach (var variable in variables)
                {
                    left = left.Substitute(variable, point);
                    right = right.Substitute(variable, point);
                }

                Entity.Number difference, scale;
                try
                {
                    difference = (left - right).EvalNumerical();
                    scale = left.EvalNumerical();
                }
                catch (AngouriMath.Core.Exceptions.AngouriMathBaseException)
                {
                    continue;
                }

                if (!difference.IsFinite || !scale.IsFinite)
                    continue;

                // Relative: the values here run from single digits to factorials, and an
                // absolute threshold would either pass everything large or fail everything
                // evaluated to a hundred digits and rounded.
                if (Magnitude(difference) > 1e-9 * (1 + Magnitude(scale)))
                    return true;
            }
            return false;

            static double Magnitude(Entity.Number number)
            {
                var complex = (Entity.Number.Complex)number;
                var real = complex.RealPart.EDecimal.ToDouble();
                var imaginary = complex.ImaginaryPart.EDecimal.ToDouble();
                return Math.Sqrt(real * real + imaginary * imaginary);
            }
        }

        [Theory]
        // Genuinely different, and the check must say so -- otherwise the property test
        // above is a safety net that catches nothing.
        [InlineData("x + 1", "x + 2", true)]
        [InlineData("x ^ 2", "x ^ 3", true)]
        [InlineData("sqrt(x)", "-sqrt(x)", true)]
        // The same value written two ways, including the case Simplify cannot settle.
        [InlineData("x * 2", "2 * x", false)]
        [InlineData("x! * (x + 1)", "(x + 1)!", false)]
        [InlineData("sqrt(12) + sqrt(27)", "5 * sqrt(3)", false)]
        public void TheCounterexampleSearchFindsCounterexamplesAndOnlyThose(string one, string another, bool expected)
            => Assert.Equal(expected, DisagreesAtSomePoint(Parse(one), Parse(another)));

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
        public void RationalizationClearsASurdOutOfADenominator()
        {
            var result = Transformation.Rationalization.Apply(Parse("1 / (sqrt(3) + 5)"));

            Assert.True(result.Succeeded);
            Assert.DoesNotContain(
                result.Output!.Nodes,
                node => node is Entity.Divf(_, var denominator) && denominator.Nodes.Any(n => n is Entity.Powf));
        }

        #endregion
    }
}
