//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// What a derivative or an integral does when the thing it is taken over is not a variable.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/964">#964</a>
    /// </summary>
    /// <remarks>
    /// Differentiating with respect to a <i>subexpression</i> is a real feature
    /// (<a href="https://github.com/asc-community/AngouriMath/issues/230">#230</a>). It was done
    /// by giving the subexpression a name and differentiating over the name, which has two
    /// premises that were not checked: the subexpression must be able to <i>vary</i>, and the
    /// rename must be <i>exact</i> — nothing of the subexpression's own variables may be left
    /// over, or the leftovers are read as independent of the name they came from.
    /// Where the rename is not exact this is a change of variables, and a change of variables
    /// needs no occurrence at all.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class BoundNameTest
    {
        /// <summary>
        /// A number cannot be differentiated or integrated over. <c>derivative(x ^ 3, 3)</c>
        /// renamed the exponent and answered <c>ln(x) * x ^ 3</c>, which is the derivative of a
        /// question nobody asked.
        /// </summary>
        [Theory]
        [InlineData("derivative(x ^ 3, 3)")]
        [InlineData("derivative(x ^ 2, 2)")]
        [InlineData("integral(x ^ 2, 2)")]
        [InlineData("derivative(x ^ 2, 0)")]
        public void ANumberIsNotSomethingToDifferentiateOver(string expression) =>
            // The node, not the printed form: a declined derivative is still normalised, so
            // `x + 1` comes back as `1 + x` and comparing strings would fail on the tidying.
            AssertDeclined(expression);

        /// <summary>
        /// A subexpression that does not occur is a <b>change of variables</b> and not a rename,
        /// and it has an answer. With <c>z = x + 1</c>, <c>x ^ 2</c> is <c>(z - 1) ^ 2</c>, whose
        /// derivative is <c>2(z - 1) = 2x</c> — which is <c>(df/dx) / (dg/dx)</c> without having
        /// to invert <c>g</c>. Answering <c>0</c> asserted independence; declining said there was
        /// no answer; there is one.
        /// </summary>
        /// <remarks>
        /// Named by @Happypig375 in review on
        /// <a href="https://github.com/asc-community/AngouriMath/pull/990">#990</a>.
        /// </remarks>
        [Theory]
        [InlineData("derivative(x ^ 2, x + 1)", "2 * x")]
        [InlineData("derivative(x ^ 2, x + 1, 2)", "2")]
        [InlineData("derivative(x ^ 2, 2 * x)", "x")]
        [InlineData("derivative(x ^ 2, sin(x))", "2 * x / cos(x)")]
        [InlineData("integral(x ^ 2, x + 1)", "x ^ 3 / 3 + C")]
        public void ASubexpressionThatDoesNotOccurIsAChangeOfVariables(string expression, string expected) =>
            Assert.Equal(expected.ToEntity().Simplify(), expression.ToEntity().Simplify());

        /// <summary>
        /// And a rename that leaves the subexpression's own variables behind is not exact, so it
        /// is a change of variables too. <c>derivative(x * (x + 1), x + 1)</c> renamed one factor
        /// and read the other as independent of the name, answering <c>x</c>; with
        /// <c>z = x + 1</c> the expression is <c>(z - 1) z</c>, whose derivative is
        /// <c>2z - 1 = 2x + 1</c>.
        /// </summary>
        [Theory]
        [InlineData("derivative(x * (x + 1), x + 1)", "1 + 2 * x")]
        [InlineData("integral(x * (x + 1), x + 1)", "x ^ 3 / 3 + x ^ 2 / 2 + C")]
        public void ARenameThatLeavesTheVariableBehindIsNotExact(string expression, string expected) =>
            Assert.Equal(expected.ToEntity().Simplify(), expression.ToEntity().Simplify());

        /// <summary>
        /// What still has no answer: a subexpression of more than one variable that the rename
        /// cannot take exactly. <c>d(x + y)/d(x * y)</c> is <c>1/y</c> through <c>x</c> and
        /// <c>1/x</c> through <c>y</c>, so there is nothing to answer without a direction.
        /// </summary>
        [Theory]
        [InlineData("derivative(x ^ 2, x * y)")]
        [InlineData("derivative(x + y, x * y)")]
        [InlineData("integral(x ^ 2, x * y)")]
        public void ASubexpressionOfSeveralVariablesIsStillDeclined(string expression) =>
            AssertDeclined(expression);

        /// <summary>
        /// Declined means the node survives simplification — "I could not settle this" — rather
        /// than becoming a number.
        /// </summary>
        private static void AssertDeclined(string expression)
        {
            var simplified = expression.ToEntity().Simplify();
            Assert.True(simplified is Entity.Derivativef or Entity.Integralf,
                $"{expression} answered {simplified.Stringize()} instead of declining");
        }

        /// <summary>
        /// The feature the two checks above are protecting, which still works.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/230">#230</a>
        /// </summary>
        [Theory]
        [InlineData("derivative((x + 1) ^ 2, x + 1)", "2 * (1 + x)")]
        [InlineData("derivative(sin(x) ^ 2, sin(x))", "2 * sin(x)")]
        [InlineData("derivative(sin(x) ^ 3, sin(x))", "3 * sin(x) ^ 2")]
        [InlineData("derivative((x * y) ^ 2, x * y)", "2 * x * y")]
        [InlineData("integral((x + 1) ^ 2, x + 1)", "(x + 1) ^ 3 / 3 + C")]
        public void DifferentiatingOverASubexpressionThatOccursStillWorks(string expression, string expected) =>
            Assert.Equal(expected.ToEntity().Simplify(), expression.ToEntity().Simplify());

        /// <summary>
        /// A vector in the variable position is the gradient, and it comes from the elementwise
        /// broadcast that every binder already had — it was unreachable because the rename
        /// matched first and answered <c>0</c>.
        /// </summary>
        [Theory]
        [InlineData("derivative(x ^ 2 + y ^ 2, [x, y])", "[2 * x, 2 * y]")]
        [InlineData("derivative(x + y, [x, y])", "[1, 1]")]
        [InlineData("derivative(x ^ 2, [x, y, z])", "[2 * x, 0, 0]")]
        [InlineData("derivative(x ^ 2 + y ^ 2, [x, y]T)", "[[2 * x, 2 * y]]")]
        public void AVectorInTheVariablePositionIsTheGradient(string expression, string expected) =>
            Assert.Equal(expected.ToEntity(), expression.ToEntity().Simplify());

        /// <summary>
        /// The same for an integral: one antiderivative per component, each of them correct.
        /// <c>integral(x, [x, y]T)</c> was <c>[[C + x ^ 2, C + x * y]]</c> — the first component
        /// is not the integral of <c>x</c>.
        /// </summary>
        [Theory]
        [InlineData("integral(x, [x, y])", "[x ^ 2 / 2 + C, x * y + C]")]
        [InlineData("integral(x ^ 2 + y ^ 2, [x, y])", "[x ^ 3 / 3 + y ^ 2 * x + C, y ^ 3 / 3 + x ^ 2 * y + C]")]
        public void AVectorInTheVariablePositionIntegratesComponentwise(string expression, string expected) =>
            Assert.Equal(expected.ToEntity().Simplify(), expression.ToEntity().Simplify());

        /// <summary>
        /// What the componentwise reading is <b>not</b>, stated so that it is not mistaken for
        /// more than it is: a vector differentiated over a vector pairs component with component,
        /// which is the diagonal of the Jacobian and not the Jacobian. The shape a full Jacobian
        /// or Hessian should take is a convention this does not choose.
        /// </summary>
        [Fact]
        public void ComponentwiseIsNotAJacobian()
        {
            // [x * y, x + y] over [x, y] pairs index with index: d(xy)/dx and d(x+y)/dy.
            Assert.Equal("[y, 1]".ToEntity(), "derivative([x * y, x + y], [x, y])".ToEntity().Simplify());
            // and the second derivative is the diagonal of the Hessian, not the Hessian
            Assert.Equal("[2 * y ^ 3, 6 * x ^ 2 * y]".ToEntity(),
                "derivative(derivative(x ^ 2 * y ^ 3, [x, y]), [x, y])".ToEntity().Simplify());
        }

        /// <summary>Ordinary differentiation is untouched, which is most of what this could break.</summary>
        [Theory]
        [InlineData("derivative(x ^ 2, x)", "2 * x")]
        [InlineData("derivative(x * y, x)", "y")]
        [InlineData("derivative(y, x)", "0")]
        [InlineData("derivative([x, x ^ 2], x)", "[1, 2 * x]")]
        [InlineData("integral(x ^ 2, x)", "x ^ 3 / 3 + C")]
        [InlineData("derivative(apply(f, x), y)", "0")]
        public void OrdinaryDifferentiationIsUnaffected(string expression, string expected) =>
            Assert.Equal(expected.ToEntity().Simplify(), expression.ToEntity().Simplify());
    }
}
