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
        /// refused — which is the reason it is a node and not a parser trick. A summand that is
        /// a <em>polynomial</em> in the index is the exception, having a closed form; anything
        /// else is still carried.
        /// </summary>
        [Theory]
        [InlineData("sum(2 ^ k, k, 1, n)")]
        [InlineData("sum(1 / k, k, 1, n)")]
        [InlineData("sum(sin(k), k, 1, n)")]
        public void ASymbolicBoundIsCarried(string expression)
            => Assert.IsType<Entity.Summationf>(expression.ToEntity().Simplify());

        /// <summary>
        /// A product is carried unless its body is a single term. Where the sum takes any
        /// polynomial apart by linearity, a product has none to take it apart with.
        /// </summary>
        [Theory]
        [InlineData("product(k + 1, k, 1, n)")]
        [InlineData("product(2 ^ k, k, 1, n)")]
        public void AProductWithASymbolicBoundIsCarried(string expression)
            => Assert.IsType<Entity.Productf>(expression.ToEntity().Simplify());

        /// <summary>
        /// A polynomial summand does have a closed form, and it is given rather than carried.
        /// The value is checked against the expansion at several bounds instead of against a
        /// printed form, since what matters is that the two agree.
        /// </summary>
        [Theory]
        [InlineData("sum(k, k, 1, n)")]
        [InlineData("sum(k ^ 2, k, 1, n)")]
        [InlineData("sum(2 * k + 1, k, 1, n)")]
        public void APolynomialSummandIsGivenInClosedForm(string expression)
        {
            var closed = expression.ToEntity().Simplify();
            Assert.IsNotType<Entity.Summationf>(closed);
            foreach (var at in new[] { 0, 1, 2, 5, 9 })
                Assert.Equal(
                    expression.ToEntity().Substitute("n", at).Simplify().Evaled,
                    closed.Substitute("n", at).Simplify().Evaled);
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

        /// <summary>
        /// A declared index is an index wherever it appears, closed form included: <c>sum(i, i,
        /// 1, n)</c> is the same sum as <c>sum(k, k, 1, n)</c> and gets the same answer, rather
        /// than being read as the imaginary unit summed over itself.
        /// </summary>
        [Fact]
        public void ADeclaredImaginaryUnitIndexIsSummedAsAnIndex() =>
            Assert.Equal(
                "sum(k, k, 1, n)".ToEntity().Simplify(),
                "sum(i, i, 1, n)".ToEntity().Simplify());

        /// <summary>
        /// Too many terms is still not written out: a thousand-term sum is a correct expansion
        /// and a useless expression, and everything downstream then walks it.
        /// </summary>
        /// <remarks>
        /// It is now <em>answered</em> rather than carried, which is not the same thing as being
        /// written out — the closed form computes the value instead of building the terms, so
        /// the expression this returns is a number and not a hundred thousand of them. The
        /// assertion is therefore on the answer; that it is not an expansion follows from there
        /// being nothing to expand.
        /// </remarks>
        [Fact]
        public void AVeryLongRangeIsNotWrittenOut() =>
            Assert.Equal(
                Entity.Number.Integer.Create(5000050000L),
                "sum(k, k, 1, 100000)".ToEntity().Simplify().Evaled);

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
