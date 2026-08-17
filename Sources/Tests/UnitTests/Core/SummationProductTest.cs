//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Linq;
using AngouriMath;
using AngouriMath.Extensions;
using Xunit;
using static AngouriMath.MathS;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// <c>sum</c> and <c>product</c> — the first operators in the library that bind a variable
    /// over a range. <a href="https://github.com/asc-community/AngouriMath/issues/248">#248</a>
    /// </summary>
    /// <remarks>
    /// The index is written <c>k</c> in most of what follows, but <c>i</c> works too: it lexes as
    /// the imaginary unit, and declaring it as the index shadows the constant inside the body.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/976">#976</a> asked for that,
    /// and the tests at the bottom of this file are what it means.
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class SummationProductTest
    {
        [Theory]
        [InlineData("sum(k, k, 1, 10)", "55")]
        [InlineData("sum(k, k, 1, 3)", "6")]
        [InlineData("sum(k^2, k, 1, 4)", "30")]
        [InlineData("product(k, k, 1, 5)", "120")]
        [InlineData("product(k, k, 1, 3)", "6")]
        public void ConcreteBoundsAreWrittenOut(string expression, string expected) =>
            Assert.Equal(expected.ToEntity().Evaled, expression.ToEntity().Simplify().Evaled);

        /// <summary>An index the body never mentions still runs the right number of times.</summary>
        [Fact]
        public void AnIndexTheBodyIgnoresStillCounts() =>
            Assert.Equal(MathS.Boolean.True,
                "sum(x, k, 1, 3)".ToEntity().Simplify().EqualTo(3 * Var("x")).Simplify());

        /// <summary>
        /// An empty range is the operator's identity, and the two differ — stated because getting
        /// it from the accumulator's initial value by accident would read as a coincidence.
        /// </summary>
        [Theory]
        [InlineData("sum(k, k, 5, 1)", "0")]
        [InlineData("product(k, k, 5, 1)", "1")]
        public void AnEmptyRangeIsTheIdentity(string expression, string expected) =>
            Assert.Equal(expected.ToEntity().Evaled, expression.ToEntity().Simplify().Evaled);

        /// <summary>
        /// A symbolic bound has no finite expansion, so the operator is carried rather than
        /// refused — which is the reason it is a node and not a parser trick.
        /// </summary>
        [Fact]
        public void ASymbolicBoundIsCarried()
        {
            var summation = "sum(k, k, 1, n)".ToEntity().Simplify();
            Assert.IsType<Entity.Summationf>(summation);
        }

        /// <summary>
        /// **The binder property.** The index is bound, so substituting it from outside must not
        /// reach inside — this is what #878 was about getting wrong for set-builders.
        /// </summary>
        [Fact]
        public void SubstitutingTheIndexFromOutsideDoesNothing()
        {
            var summation = "sum(k, k, 1, n)".ToEntity();
            Assert.Equal(summation, summation.Substitute("k", 5));
        }

        /// <summary>And a free variable in the bounds is substitutable, which is the other half.</summary>
        [Fact]
        public void SubstitutingABoundIsAnOrdinarySubstitution() =>
            Assert.Equal(((Entity)6).Evaled,
                "sum(k, k, 1, n)".ToEntity().Substitute("n", 3).Simplify().Evaled);

        /// <summary>
        /// <c>i</c> lexes as the imaginary unit, and declaring it as the index shadows that: the
        /// sum every textbook writes with <c>i</c> is the sum a reader means by it. #976
        /// </summary>
        [Theory]
        [InlineData("sum(i, i, 1, 10)", "55")]
        [InlineData("sum(i^2, i, 1, 4)", "30")]
        [InlineData("product(i, i, 1, 4)", "24")]
        public void AnIndexNamedIShadowsTheImaginaryUnit(string expression, string expected) =>
            Assert.Equal(expected.ToEntity().Evaled, expression.ToEntity().Simplify().Evaled);

        /// <summary>
        /// The same through the C# surface, which is why the shadowing lives in <c>MathS.Sum</c>
        /// rather than in the grammar action: <c>"i"</c> converts to an entity by being parsed, so
        /// this call arrives exactly as the parsed text does, and the two must not disagree.
        /// </summary>
        [Fact]
        public void TheApiShadowsItToo() =>
            Assert.Equal(((Entity)55).Evaled, Sum("i", "i", 1, 10).Simplify().Evaled);

        /// <summary>
        /// The shadowing reaches an imaginary <i>literal</i> too, because <c>2i</c> is one number
        /// token rather than a product: answering this <c>6i</c> while <c>sum(2 * i, i, 1, 3)</c>
        /// is <c>12</c> would be one expression answered two ways.
        /// </summary>
        [Theory]
        [InlineData("sum(2i, i, 1, 3)")]
        [InlineData("sum(2 * i, i, 1, 3)")]
        public void AnImaginaryLiteralIsTwiceTheIndexToo(string expression) =>
            Assert.Equal(((Entity)12).Evaled, expression.ToEntity().Simplify().Evaled);

        /// <summary>
        /// And nothing is shadowed where <c>i</c> was not declared: the constant is still the
        /// constant when the index is something else, which is the half that could regress silently.
        /// </summary>
        [Fact]
        public void TheImaginaryUnitSurvivesAnIndexNamedSomethingElse() =>
            Assert.Equal((3 * MathS.i).Evaled, "sum(i, k, 1, 3)".ToEntity().Simplify().Evaled);

        /// <summary>
        /// What is shadowed is the <i>name</i>, so an expression that denotes the constant without
        /// naming it still denotes it — the same way shadowing works anywhere else. This is the case
        /// that says the rule is syntactic rather than a hunt for the value.
        /// </summary>
        [Fact]
        public void AnExpressionThatOnlyEqualsTheImaginaryUnitIsNotTheName() =>
            Assert.Equal((3 * MathS.i).Evaled, "sum(sqrt(-1), i, 1, 3)".ToEntity().Simplify().Evaled);

        /// <summary>
        /// The index is a name once it is declared, so it is bound like any other index — the
        /// property <see cref="SubstitutingTheIndexFromOutsideDoesNothing"/> pins for <c>k</c>.
        /// </summary>
        [Fact]
        public void TheShadowedIndexIsBoundLikeAnyOther()
        {
            var summation = "sum(i, i, 1, n)".ToEntity();
            var index = Assert.Single(summation.Vars.Where(variable => variable.Name == "i"));
            Assert.Equal(summation, summation.Substitute(index, 5));
        }

        /// <summary>
        /// The bounds are outside the binder — they are written in the scope the declaration is
        /// made in — so the imaginary unit there is still the imaginary unit, and a range that ends
        /// at it has no integer bound and is carried.
        /// </summary>
        [Fact]
        public void TheBoundsAreOutsideTheBinder()
        {
            var summation = Assert.IsType<Entity.Summationf>("sum(i, i, 1, i)".ToEntity().Simplify());
            Assert.Equal(MathS.i, summation.To);
            Assert.IsType<Entity.Variable>(summation.Var);
        }

        /// <summary>
        /// Too many terms is left unexpanded: a thousand-term sum is a correct expansion and a
        /// useless expression, and everything downstream then walks it.
        /// </summary>
        [Fact]
        public void AVeryLongRangeIsNotWrittenOut() =>
            Assert.IsType<Entity.Summationf>("sum(k, k, 1, 100000)".ToEntity().Simplify());

        [Theory]
        [InlineData("sum(k, k, 1, n)")]
        [InlineData("product(k, k, 1, n)")]
        // A shadowed index prints as `i`, which reads as the imaginary unit anywhere else -- so the
        // round trip only closes because parsing the printed form shadows it again. #976
        [InlineData("sum(i, i, 1, n)")]
        [InlineData("product(i^2, i, 1, n)")]
        public void ItPrintsAndParsesBackToItself(string expression)
        {
            var original = expression.ToEntity();
            Assert.Equal(original, original.Stringize().ToEntity());
        }

        [Fact]
        public void ItLatexizesAsTheOperatorItIs()
        {
            Assert.Contains(@"\sum_{", Sum("k", "k", 1, "n").Latexize());
            Assert.Contains(@"\prod_{", Product("k", "k", 1, "n").Latexize());
        }

        [Fact]
        public void ItExportsToSymPy()
        {
            Assert.Contains("sympy.Sum(", ToSympyCode(Sum("k", "k", 1, "n")));
            Assert.Contains("sympy.Product(", ToSympyCode(Product("k", "k", 1, "n")));
        }
    }
}
