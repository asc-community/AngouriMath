//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Linq;
using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Algebra.SolveTest
{
    /// <summary>
    /// An equation whose unknown stands inside a derivative, an integral, a limit, a sum
    /// or a product, where the operator is taken over some other name.
    /// https://github.com/asc-community/AngouriMath/issues/964
    /// </summary>
    /// <remarks>
    /// Such an operator is evaluated by deciding that the unknown does not move with the
    /// name the operator is taken over: <c>derivative(y, x)</c> is 0 because <c>y</c> is
    /// not <c>x</c>. A root that mentions <c>x</c> denies exactly that, so it cannot be
    /// asserted on the strength of a derivation that assumed it.
    /// </remarks>
    [Trait("Area", "Algebra")]
    public sealed class CalculusEquationTest
    {
        /// <summary>
        /// The answer must not be a finite set holding something that is not a root. The
        /// membership is checked by substituting back and simplifying, which is what the
        /// issue did by hand.
        /// </summary>
        private static void AssertEveryRootSatisfies(string equation, string unknown)
        {
            var expr = equation.ToEntity();
            if (expr.SolveEquation(unknown) is not Entity.Set.FiniteSet finite)
                return;
            foreach (var root in finite)
                Assert.True(
                    expr.Substitute(unknown, root).Simplify() == 0,
                    $"{root.Stringize()} is not a root of {equation}; it leaves "
                    + $"{expr.Substitute(unknown, root).Simplify().Stringize()}");
        }

        [Theory]
        [InlineData("derivative(y, x) + y - x", "y")]
        [InlineData("derivative(y, x) + y ^ 2 - x", "y")]
        [InlineData("integral(y, x) + y - x", "y")]
        [InlineData("limit(y, x, 0) + y - x", "y")]
        [InlineData("sum(y, k, 1, 3) + y - k", "y")]
        public void NoAssertedNonRoot(string equation, string unknown)
            => AssertEveryRootSatisfies(equation, unknown);

        /// <summary>
        /// The empty set says there is no such <c>y</c>, which is as false as naming one.
        /// The condition itself is the only one of the three that is true.
        /// </summary>
        [Theory]
        [InlineData("derivative(y, x) + y - x", "y")]
        [InlineData("integral(y, x) + y - x", "y")]
        [InlineData("limit(y, x, 0) + y - x", "y")]
        [InlineData("sum(y, k, 1, 3) + y - k", "y")]
        [InlineData("product(y, k, 1, 3) + y - k", "y")]
        public void UnsolvedRatherThanEmpty(string equation, string unknown)
            => Assert.IsType<Entity.Set.ConditionalSet>(equation.ToEntity().SolveEquation(unknown));

        /// <summary>An equation and the same equation written <c>= 0</c> answer alike.</summary>
        [Theory]
        [InlineData("derivative(y, x) + y - x", "y")]
        [InlineData("limit(y, x, 0) + y - x", "y")]
        public void StatementFormAnswersTheSame(string equation, string unknown)
            => Assert.IsType<Entity.Set.ConditionalSet>(
                (equation + " = 0").ToEntity().Solve(unknown));

        /// <summary>
        /// Where no root mentions the name the operator is taken over, nothing was assumed
        /// that the answer then denies, and the answer stands. <c>d(y*x)/dx</c> is <c>y</c>
        /// for a <c>y</c> free of <c>x</c>, and <c>1/2</c> is free of <c>x</c>.
        /// </summary>
        [Theory]
        [InlineData("derivative(y * x, x) + y - 1", "y", "1/2")]
        [InlineData("derivative(y ^ 2, y) - 2", "y", "1")]
        [InlineData("limit(y * x, x, 2) - 4", "y", "2")]
        [InlineData("sum(y, k, 1, 3) - 6", "y", "2")]
        public void AnUnthreatenedRootIsStillReturned(string equation, string unknown, string root)
        {
            var finite = Assert.IsType<Entity.Set.FiniteSet>(equation.ToEntity().SolveEquation(unknown));
            Assert.Contains(root.ToEntity(), finite.Select(e => e.Simplify()));
        }
    }
}
