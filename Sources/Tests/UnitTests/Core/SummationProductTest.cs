//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

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
    /// The index is mostly written <c>k</c>, but <c>i</c> works too and the cases below say so.
    /// <c>i</c> is the imaginary unit — decided in the lexer, so it cannot be a variable anywhere
    /// in the language — and naming it as an index used to bind nothing and sum nothing. It is now
    /// read as the declaration it plainly is, and only inside the operator that declares it.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/976">#976</a>
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
        /// <c>i</c> is the imaginary unit, and the lexer decides that — so naming it as the index
        /// used to sum nothing. Naming it is now taken as the declaration it obviously is.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/976">#976</a>
        /// </summary>
        [Theory]
        [InlineData("sum(i, i, 1, 10)", "55")]
        [InlineData("sum(i ^ 2, i, 1, 4)", "30")]
        [InlineData("product(i, i, 1, 5)", "120")]
        public void TheImaginaryUnitCanBeAnIndexWhenItIsDeclaredAsOne(string expression, string expected) =>
            Assert.Equal(expected.ToEntity().Evaled, expression.ToEntity().Simplify().Evaled);

        /// <summary>
        /// The other half, and the one that would make the change above a wrong answer if it
        /// failed: <c>i</c> is reinterpreted only where it is the index. Anywhere else — inside
        /// the same sum, or outside it — it is still the imaginary unit.
        /// </summary>
        [Theory]
        [InlineData("sum(i * k, k, 1, 3)", "6i")]
        [InlineData("sum(k + i, k, 1, 2)", "3 + 2i")]
        [InlineData("sum(i, i, 1, 3) + i", "6 + i")]
        public void ElsewhereItIsStillTheImaginaryUnit(string expression, string expected) =>
            Assert.Equal(expected.ToEntity().Evaled, expression.ToEntity().Simplify().Evaled);

        /// <summary>A declared index with a symbolic bound is still carried, not guessed at.</summary>
        [Fact]
        public void ADeclaredImaginaryUnitIndexWithASymbolicBoundIsCarried() =>
            Assert.IsType<Entity.Summationf>("sum(i, i, 1, n)".ToEntity().Simplify());

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
