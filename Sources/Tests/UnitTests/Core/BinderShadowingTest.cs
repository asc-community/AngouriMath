//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// A binder handed <c>i</c> as the name it binds reads it as that name, and every binder in
    /// the language does it the same way.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/976">#976</a>
    /// </summary>
    /// <remarks>
    /// <c>e</c> and <c>pi</c> need none of this and are here to say why: they are
    /// <see cref="Entity.Variable"/>s that carry a value, so a binder shadows them by
    /// construction. <c>i</c> is a number — the lexer decides it, <c>NUMBER: ... | 'i'</c> — so
    /// without this it is the one name in the language that no binder can bind.
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class BinderShadowingTest
    {
        /// <summary>
        /// Every binder, given the same expression written over <c>i</c> and over an ordinary
        /// name, answers the same thing.
        /// </summary>
        [Theory]
        [InlineData("sum({0}, {0}, 1, 10)")]
        [InlineData("product({0}, {0}, 1, 5)")]
        [InlineData("integral({0}, {0})")]
        [InlineData("integral({0}, {0}, 0, 1)")]
        [InlineData("integral({0} ^ 2 + 1, {0}, 0, 2)")]
        [InlineData("limit({0}, {0}, 0)")]
        [InlineData("limit(sin({0}) / {0}, {0}, 0)")]
        [InlineData("derivative({0} ^ 2, {0})")]
        [InlineData("apply(lambda({0}, {0} + 1), 3)")]
        public void ABinderOverIAnswersWhatTheSameBinderOverKAnswers(string shape)
        {
            var overK = string.Format(shape, "k").ToEntity().Simplify();
            var overI = string.Format(shape, "i").ToEntity().Simplify();
            // Renamed rather than compared as they stand: the two answers are the same
            // expression written over different names wherever the name survives into it.
            Assert.Equal(overK, overI.Substitute(Entity.Variable.CreateVariableUnchecked("i"), "k"));
        }

        /// <summary>
        /// <c>2i</c> is one token — the lexer's <c>NUMBER</c> ends <c>'i'?</c> — so it arrives as a
        /// single number with nothing in it to rename. Under a binder that names <c>i</c> it is
        /// the writer's <c>2</c> beside the writer's <c>i</c>, and <c>2k</c> there is a product.
        /// </summary>
        [Theory]
        [InlineData("sum(2i, i, 1, 3)", "12")]
        [InlineData("product(2i, i, 1, 3)", "48")]
        [InlineData("sum(3 + 2i, i, 1, 3)", "21")]
        [InlineData("integral(2i, i, 0, 1)", "1")]
        public void AWrittenCoefficientOnTheBoundNameIsAProduct(string expression, string expected) =>
            Assert.Equal(expected.ToEntity().Evaled, expression.ToEntity().Simplify().Evaled);

        /// <summary>
        /// The half that makes the above a fix rather than a new defect: <c>i</c> is read as a
        /// name only inside the binder that declares it.
        /// </summary>
        /// <remarks>
        /// The last case is the one Happypig375 asked about on
        /// <a href="https://github.com/asc-community/AngouriMath/issues/979">#979</a>, and it is
        /// what SymPy answers for <c>Sum(sqrt(-1) * i, (i, 1, 10))</c>.
        /// </remarks>
        [Theory]
        [InlineData("sum(i * k, k, 1, 3)", "6i")]
        [InlineData("integral(i * k, k, 0, 2)", "2i")]
        [InlineData("limit(i * k, k, 2)", "2i")]
        [InlineData("sum(i, i, 1, 3) + i", "6 + i")]
        [InlineData("sum(sqrt(-1) * i, i, 1, 10)", "55i")]
        public void ElsewhereItIsStillTheImaginaryUnit(string expression, string expected) =>
            Assert.Equal(expected.ToEntity().Evaled, expression.ToEntity().Simplify().Evaled);

        /// <summary>A set builder binds its name too, and used to answer <c>NaN</c> over this one.</summary>
        [Fact]
        public void ASetBuilderOverIIsTheSetItDescribes()
        {
            var set = (Entity.Set)"{ i : i > 0 }".ToEntity();
            Assert.True(set.Contains(5));
            Assert.False(set.Contains(-5));
        }

        /// <summary>
        /// A lambda's parameter is typed <see cref="Entity.Variable"/>, so it is the one binder
        /// that cannot be handed the imaginary unit at all — it threw rather than answering.
        /// </summary>
        [Fact]
        public void ALambdaOverIBindsIt()
        {
            var lambda = Assert.IsType<Entity.Lambda>("lambda(i, i + 1)".ToEntity());
            Assert.IsType<Entity.Variable>(lambda.Parameter);
            Assert.Equal(4, "apply(lambda(i, i + 1), 3)".ToEntity().Simplify().EvalNumerical());
        }

        /// <summary>
        /// Read at the node and not at the way in, so the constructor gets it as well as
        /// <see cref="MathS.Sum(Entity, Entity, Entity, Entity)"/> and the parser.
        /// </summary>
        [Fact]
        public void TheNodeItselfReadsTheName()
        {
            Assert.Equal(55, new Entity.Summationf(MathS.i, MathS.i, 1, 10).Simplify().EvalNumerical());
            Assert.Equal(120, new Entity.Productf(MathS.i, MathS.i, 1, 5).Simplify().EvalNumerical());
            Assert.Equal(55, MathS.Sum(MathS.i, MathS.i, 1, 10).Simplify().EvalNumerical());
        }

        /// <summary>
        /// One reading for the whole binder, not one per position: the bounds are inside it too.
        /// </summary>
        [Fact]
        public void TheWholeBinderReadsTheNameTheSameWay()
        {
            var sum = Assert.IsType<Entity.Summationf>(MathS.Sum(MathS.i, MathS.i, 1, MathS.i));
            Assert.IsType<Entity.Variable>(sum.Var);
            Assert.Equal(sum.Var, sum.Expression);
            Assert.Equal(sum.Var, sum.To);
        }

        /// <summary>
        /// Nothing was added for <c>e</c> and <c>pi</c> because a binder already shadows them:
        /// they are variables that carry a value, so the name in the index position binds, and
        /// these have always worked.
        /// </summary>
        [Theory]
        [InlineData("sum(e, e, 1, 3)", "6")]
        [InlineData("product(pi, pi, 1, 4)", "24")]
        [InlineData("integral(e, e, 0, 1)", "1/2")]
        public void ANamedConstantThatIsAVariableNeededNothing(string expression, string expected) =>
            Assert.Equal(expected.ToEntity(), expression.ToEntity().Simplify());

        /// <summary>
        /// And where they do <i>not</i> work, which is recorded here rather than believed away.
        /// </summary>
        /// <remarks>
        /// A bound <c>e</c> keeps the constant's value, because the bound name and the constant
        /// are the same object: <c>Variable.ConstantList</c> is keyed by name, so a variable
        /// called <c>e</c> is <c>2.718…</c> wherever something reads its value. Nothing
        /// short of a representation that tells a bound name from a constant one fixes it,
        /// which is why this change is about the one constant that is not a variable — for
        /// <c>i</c> the bound name and the unit are distinguishable, and that is the whole reason
        /// the fix above is possible at all.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/984">#984</a>
        /// </remarks>
        [Theory]
        [InlineData("{ e : e > 0 }", "{ e : True }")]          // { e : e > 0 }
        public void ABoundNamedConstantStillCarriesItsValue(string expression, string expected) =>
            Assert.Equal(expected.ToEntity(), expression.ToEntity().Simplify());

        /// <summary>
        /// Differentiating over a bound named constant works, and it is worth its own test
        /// because it did not: it answered <c>0</c> until the bound name stopped being read as
        /// the number the constant evaluates to.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/964">#964</a>
        /// </summary>
        [Theory]
        [InlineData("derivative(e ^ 2, e)", "2 * e")]
        [InlineData("derivative(pi ^ 2, pi)", "2 * pi")]
        public void ABoundNamedConstantCanBeDifferentiatedOver(string expression, string expected) =>
            Assert.Equal(expected.ToEntity(), expression.ToEntity().Simplify());

        /// <summary>The same, on the path that shows it most plainly.</summary>
        [Fact]
        public void ABoundNamedConstantEvaluatesToTheConstant()
        {
            // Simplify gets this right -- it is 0 -- and Evaled does not, which is the shape of
            // the defect: the bound name is only a constant once something reads its value.
            Assert.Equal("limit(e, e, 0)".ToEntity().Simplify(), (Entity)0);
            Assert.Equal(MathS.DecimalConst.e, "limit(e, e, 0)".ToEntity().Evaled.EvalNumerical().RealPart.EDecimal);
        }

    }
}
