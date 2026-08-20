//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using System.Linq;
using Xunit;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// A name a binder declares is a variable, whatever that name is spelled — including the
    /// names the language reads as mathematical constants.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/984">#984</a>
    /// </summary>
    /// <remarks>
    /// The mechanism is <see cref="Entity.Constant"/>: a constant is a node and not a name, so a
    /// binder that declares <c>pi</c> holds a <see cref="Entity.Variable"/> while the rest of the
    /// language holds the constant, and no evaluation needs to be told where it is.
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class BoundConstantTest
    {
        /// <summary>
        /// The name a binder declares, as the binder itself reads it. Written rather than parsed,
        /// because parsing <c>e</c> gives the constant — which is the whole subject here — so an
        /// expectation spelled as a string could not say "a variable called e" at all.
        /// </summary>
        private static Entity.Variable BoundName(string name) =>
            Assert.IsType<Entity.Summationf>($"sum({name}, {name}, 1, 1)".ToEntity()).Var as Entity.Variable
            ?? throw new Xunit.Sdk.XunitException($"a binder over {name} did not bind a variable");

        // ---------------------------------------------------------------- the five in #984

        /// <summary>
        /// Differentiating with respect to a bound name. <c>0</c> before, because the name carried
        /// the constant's value and the constant does not depend on itself.
        /// </summary>
        [Theory]
        [InlineData("e")]
        [InlineData("pi")]
        public void DifferentiatingByABoundNameIsNotZero(string name)
        {
            var bound = BoundName(name);
            Assert.Equal(2 * bound, $"derivative({name} ^ 2, {name})".ToEntity().Simplify());
            Assert.Equal(2 * bound, $"derivative({name} ^ 2, {name})".ToEntity().Evaled);
        }

        /// <summary>A set builder's predicate is about its own name, not about the constant.</summary>
        [Theory]
        [InlineData("{ e : e > 0 }")]
        [InlineData("{ pi : pi > 0 }")]
        public void ASetBuildersPredicateSurvives(string expression) =>
            Assert.Equal(expression.ToEntity(), expression.ToEntity().Simplify());

        /// <summary>
        /// The two the issue reports on <c>Evaled</c> rather than on <c>Simplify</c>. That
        /// distinction is the shape of the defect — <c>Simplify</c> was already right and
        /// <c>Evaled</c> read the name's value — so both are asserted here for both.
        /// </summary>
        [Theory]
        [InlineData("limit(e, e, 0)", "0")]
        [InlineData("limit(pi, pi, 0)", "0")]
        [InlineData("integral(e, e, 0, 1)", "1/2")]
        [InlineData("integral(pi, pi, 0, 1)", "1/2")]
        public void ABoundConstantEvaluatesAsAVariable(string expression, string expected)
        {
            Assert.Equal(expected.ToEntity(), expression.ToEntity().Simplify());
            Assert.Equal(expected.ToEntity(), expression.ToEntity().Evaled);
        }

        // ---------------------------------------------------------------- free is unchanged

        /// <summary>A constant nobody bound is the constant, symbolically and numerically.</summary>
        [Fact]
        public void AFreeConstantIsStillTheConstant()
        {
            Assert.Equal(MathS.DecimalConst.pi, MathS.pi.Evaled.EvalNumerical().RealPart.EDecimal);
            Assert.Equal(MathS.DecimalConst.e, MathS.e.Evaled.EvalNumerical().RealPart.EDecimal);
            Assert.Equal(MathS.pi, "pi".ToEntity());
            Assert.Equal((Entity)0, "sin(pi)".ToEntity().Simplify());
            Assert.Equal((Entity)1, "ln(e)".ToEntity().Simplify());
            Assert.Equal((Entity)0, "derivative(e ^ 2, x)".ToEntity().Simplify());
            Assert.Equal("pi / 2".ToEntity(), "arccos(0)".ToEntity().Simplify());
        }

        /// <summary>
        /// Which is a claim about the representation and not only about the answers: a written
        /// constant is a <see cref="Entity.Constant"/>, and the same name in a binder is not.
        /// </summary>
        [Fact]
        public void AWrittenConstantIsAConstantNodeAndABoundOneIsNot()
        {
            Assert.IsType<Entity.Constant>(MathS.pi);
            Assert.IsType<Entity.Constant>("e".ToEntity());

            var sum = Assert.IsType<Entity.Summationf>("sum(pi, pi, 1, 3)".ToEntity());
            Assert.IsType<Entity.Variable>(sum.Var);          // exactly Variable, not Constant
            Assert.Equal(sum.Var, sum.Expression);
        }

        // ---------------------------------------------------------------- every binder

        /// <summary>
        /// All seven binders in the language, each over a constant's name and over an ordinary
        /// name, answering the same thing. A binder that bypassed the mechanism fails here rather
        /// than waiting to be noticed.
        /// </summary>
        [Theory]
        [InlineData("derivative({0} ^ 2, {0})")]
        [InlineData("integral({0} ^ 2, {0})")]
        [InlineData("integral({0} ^ 2, {0}, 0, 1)")]
        [InlineData("limit({0} ^ 2, {0}, 3)")]
        [InlineData("sum({0} ^ 2, {0}, 1, 4)")]
        [InlineData("product({0} ^ 2, {0}, 1, 4)")]
        [InlineData("apply(lambda({0}, {0} ^ 2), 3)")]
        public void EveryBinderReadsAConstantsNameAsTheNameItBinds(string shape)
        {
            var ordinary = string.Format(shape, "qq").ToEntity().Simplify();
            foreach (var name in new[] { "e", "pi" })
            {
                // Where the bound name outlives its binder -- an indefinite integral, a
                // derivative -- it is a variable, so it is renamed rather than substituted for.
                var bound = string.Format(shape, name).ToEntity().Simplify();
                var renamed = bound.Substitute(BoundName(name), MathS.Var("qq"));
                Assert.Equal(ordinary, renamed);
            }
        }

        /// <summary>A set builder is a binder too, and its predicate is not a claim about pi.</summary>
        [Fact]
        public void ASetBuilderBindsAConstantsName()
        {
            Assert.Equal("{ e : e > 0 }".ToEntity(), "{ e : e > 0 }".ToEntity().Simplify());
            var set = Assert.IsType<Entity.Set.ConditionalSet>("{ e : e > 0 }".ToEntity());
            Assert.True(set.Contains(2));
            Assert.False(set.Contains(-2));
        }

        /// <summary>A lambda binds it in the node, not only where the parser builds one.</summary>
        [Fact]
        public void ALambdaBindsAConstantsName()
        {
            Assert.Equal((Entity)6, MathS.Apply(MathS.Lambda(MathS.e, MathS.e + 1), 5).Simplify());
            Assert.Equal((Entity)6, "apply(lambda(e, e + 1), 5)".ToEntity().Simplify());
        }

        // ---------------------------------------------------------------- scope

        /// <summary>
        /// Only inside the binder that declares it. The <c>pi</c> outside the sum is the constant,
        /// and the sum's own <c>pi</c> is its index.
        /// </summary>
        [Fact]
        public void ABinderReachesNoFurtherThanItself()
        {
            Assert.Equal("6 + pi".ToEntity(), "sum(pi, pi, 1, 3) + pi".ToEntity().Simplify());
            Assert.Equal("3 * pi".ToEntity(), "sum(pi * k, k, 1, 2)".ToEntity().Simplify());
        }

        /// <summary>Nested binders, and one shadowing another over the same name.</summary>
        [Theory]
        // The inner sum is 1 + 2, a constant with respect to the outer one, three times over.
        [InlineData("sum(sum(pi, pi, 1, 2), pi, 1, 3)", "9")]
        // (1 + 3) + (2 + 3) + (3 + 3): the outer index outside the inner binder, the inner inside.
        [InlineData("sum(pi + sum(pi, pi, 1, 2), pi, 1, 3)", "15")]
        // The inner binder is over another name, so both of its terms are the outer index.
        [InlineData("sum(sum(pi, k, 1, 2), pi, 1, 3)", "12")]
        // Two different constants nested.
        [InlineData("sum(sum(e * pi, e, 1, 2), pi, 1, 3)", "18")]
        public void NestedBindersShadowInTheRightOrder(string expression, string expected) =>
            Assert.Equal(expected.ToEntity(), expression.ToEntity().Simplify());

        /// <summary>
        /// Substitution follows the same reading: substituting the constant does not reach a name
        /// a binder has taken, and does reach a free one.
        /// </summary>
        [Fact]
        public void SubstitutingTheConstantDoesNotReachABoundName()
        {
            Assert.Equal("3 + x".ToEntity(), "pi + x".ToEntity().Substitute(MathS.pi, 3).Simplify());
            Assert.Equal("sum(pi, pi, 1, 3)".ToEntity(), "sum(pi, pi, 1, 3)".ToEntity().Substitute(MathS.pi, 3));
        }

        // ---------------------------------------------------------------- capture

        /// <summary>
        /// A constant that simplification <i>produces</i> inside a binder over that name is not
        /// caught by it. <c>arccos(0)</c> is <c>pi / 2</c>, and integrating it over <c>pi</c> from
        /// 0 to 1 answered <c>1/4</c> — the integral of <c>pi / 2</c> d<c>pi</c> — because the
        /// produced constant and the bound name were one object.
        /// </summary>
        [Fact]
        public void AProducedConstantIsNotCaughtByABinderOverItsName()
        {
            Assert.Equal("pi / 2".ToEntity(), "integral(arccos(0), pi, 0, 1)".ToEntity().Simplify());
            Assert.Equal("integral(arccos(0), q, 0, 1)".ToEntity().Simplify(),
                         "integral(arccos(0), pi, 0, 1)".ToEntity().Simplify());
        }

        /// <summary>
        /// <c>ln</c> and <c>exp</c> are written with Euler's number in their own definition —
        /// <c>Ln(a)</c> is <c>Logf(e, a)</c> — and that <c>e</c> is a value rather than a mention
        /// of the name, so a binder over <c>e</c> does not reach it. Where the writer does name
        /// <c>e</c>, it binds, which is the same rule read the other way.
        /// </summary>
        [Theory]
        [InlineData("sum(ln(x), e, 1, 2)", "2 * ln(x)")]      // the body never names e
        [InlineData("sum(exp(x), e, 1, 2)", "2 * e ^ x")]     // nor does this one
        [InlineData("sum(ln(e), e, 1, 1)", "0")]              // ln(1), not log(1, 1)
        [InlineData("sum(log(e, x), e, 1, 2)", "log(1, x) + log(2, x)")]  // written, so bound
        [InlineData("sum(e ^ x, e, 1, 2)", "1 + 2 ^ x")]                  // written, so bound
        public void AnOperatorsOwnBaseIsNotANameOccurrence(string expression, string expected) =>
            Assert.Equal(expected.ToEntity(), expression.ToEntity().Simplify());

        /// <summary>
        /// The two roles are the same number and compare equal, so no rule can miss a match
        /// through the difference between them.
        /// </summary>
        [Fact]
        public void TheTwoRolesOfAConstantAreEqual()
        {
            Assert.Equal(MathS.e, Assert.IsType<Entity.Logf>(MathS.Ln(MathS.Var("x"))).Base);
            Assert.Equal("e ^ 5".ToEntity(), "e ^ 2 * exp(3)".ToEntity().Simplify());
            Assert.Equal("ln(x)".ToEntity().Simplify(), "log(e, x)".ToEntity().Simplify());
        }

        // ---------------------------------------------------------------- printing

        /// <summary>
        /// A bound name prints as it was written, and reading the printed form back gives the same
        /// expression — the binder decides what its name means when the node is built, so parsing
        /// re-establishes it.
        /// </summary>
        [Theory]
        [InlineData("sum(pi, pi, 1, 3)")]
        [InlineData("product(e, e, 1, 3)")]
        [InlineData("{ e : e > 0 }")]
        [InlineData("derivative(pi ^ 2, pi)")]
        [InlineData("integral(e ^ 2, e, 0, 1)")]
        [InlineData("limit(pi ^ 2, pi, 3)")]
        [InlineData("sum(sum(pi, pi, 1, 2), pi, 1, 3)")]
        [InlineData("lambda(e, e + 1)")]
        public void ABoundConstantPrintsAsItsNameAndReadsBack(string expression)
        {
            var parsed = expression.ToEntity();
            Assert.Contains(expression.Contains("pi") ? "pi" : "e", parsed.Stringize());
            Assert.Equal(parsed, parsed.Stringize().ToEntity());
            Assert.Equal(parsed.Simplify(), parsed.Stringize().ToEntity().Simplify());
        }

        // ---------------------------------------------------------------- unchanged elsewhere

        /// <summary>An ordinary name behaves exactly as it did, which is what all of the above is measured against.</summary>
        [Theory]
        [InlineData("derivative(k ^ 2, k)", "2 * k")]
        [InlineData("{ k : k > 0 }", "{ k : k > 0 }")]
        [InlineData("sum(k, k, 1, 3)", "6")]
        [InlineData("limit(k, k, 0)", "0")]
        [InlineData("integral(k, k, 0, 1)", "1/2")]
        public void AnOrdinaryNameIsUnchanged(string expression, string expected)
        {
            Assert.Equal(expected.ToEntity(), expression.ToEntity().Simplify());
            Assert.Equal(expected.ToEntity(), expression.ToEntity().Evaled);
        }

        /// <summary>
        /// And the imaginary unit still binds the way <a
        /// href="https://github.com/asc-community/AngouriMath/issues/976">#976</a> made it, which
        /// this reads through the same <c>Binding</c>.
        /// </summary>
        [Theory]
        [InlineData("sum(i, i, 1, 10)", "55")]
        [InlineData("sum(sqrt(-1) * i, i, 1, 10)", "55i")]
        public void TheImaginaryUnitStillBinds(string expression, string expected) =>
            Assert.Equal(expected.ToEntity(), expression.ToEntity().Simplify());

        /// <summary>
        /// A name that outlives the binder that declared it is a variable, and prints as the name
        /// it was given — so the printed form reads back as the constant rather than as that
        /// variable. Recorded rather than asserted away: it is the same for the imaginary unit
        /// since <a href="https://github.com/asc-community/AngouriMath/issues/976">#976</a>, and
        /// nothing short of a name for the escaped variable would change it.
        /// </summary>
        [Theory]
        [InlineData("derivative(pi ^ 2, pi)", "2 * pi")]
        [InlineData("derivative(e ^ 2, e)", "2 * e")]
        [InlineData("derivative(i ^ 2, i)", "2 * i")]
        public void ANameThatOutlivesItsBinderPrintsAsItself(string expression, string printed)
        {
            var answer = expression.ToEntity().Simplify();
            Assert.Equal(printed, answer.Stringize());
            Assert.NotEqual(printed.ToEntity(), answer);
        }

        /// <summary>
        /// <c>Vars</c> says which names an expression depends on, and a bound constant's name is
        /// one — it is a variable there. A free constant is not, as before.
        /// </summary>
        [Fact]
        public void VarsCountsABoundConstantsNameAndNotAFreeOne()
        {
            Assert.Empty("pi + sin(pi)".ToEntity().Vars);
            Assert.Equal(new[] { "pi" }, "sum(pi, pi, 1, 3)".ToEntity().Vars.Select(v => v.Name));
            Assert.Equal(new[] { "x" }, "pi * x".ToEntity().Vars.Select(v => v.Name));
        }
    }
}
