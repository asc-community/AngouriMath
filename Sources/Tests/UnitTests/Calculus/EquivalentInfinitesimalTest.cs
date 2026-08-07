//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Extensions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// sin(u), tan(u), arcsin(u) and arctan(u) are all equivalent to u where u vanishes, and one
    /// may be written for the other wherever the expression is a product or a quotient of them.
    /// Only the quotient had the substitution, and it was made on the function vanishing rather
    /// than on its argument vanishing, which are two different conditions.
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class EquivalentInfinitesimalTest
    {
        private static Entity Limit(string expression, string destination, ApproachFrom side)
        {
            var task = Task.Run(() =>
                expression.ToEntity().Limit("x", destination.ToEntity(), side).Simplify());
            Assert.True(task.Wait(TimeSpan.FromSeconds(30)), $"the limit of {expression} did not terminate");
            return task.Result;
        }

        private static void AssertLimit(string expression, string destination, ApproachFrom side, string expected) =>
            Assert.Equal(expected.ToEntity().Evaled, Limit(expression, destination, side).Evaled);

        /// <summary>
        /// A product takes the substitution as much as a quotient does. tan(x) * ln(x) at 0+ is
        /// the case that names this: written sin(x) * ln(x) the same limit was 0, and written
        /// with a tangent it was NaN -- the claim that it does not exist.
        /// </summary>
        [Theory]
        [InlineData("tan(x) * ln(x)", "0", ApproachFrom.Right, "0")]
        [InlineData("arcsin(x) * ln(x)", "0", ApproachFrom.Right, "0")]
        [InlineData("arctan(x) * ln(x)", "0", ApproachFrom.Right, "0")]
        [InlineData("tan(x) * ln(-x)", "0", ApproachFrom.Left, "0")]
        [InlineData("tan(2 * x) * ln(x)", "0", ApproachFrom.Right, "0")]
        public void AProductTakesTheSubstitution(string expression, string destination, ApproachFrom side, string expected) =>
            AssertLimit(expression, destination, side, expected);

        /// <summary>
        /// It is the argument that has to vanish, not the function. sin(x) vanishes at pi as
        /// surely as at 0, and there it is equivalent to pi - x rather than to x, so rewriting
        /// it as x turned a limit of -1 into pi / 0.
        /// </summary>
        [Theory]
        [InlineData("sin(x) / (x - pi)", "pi", "-1")]
        [InlineData("sin(x) / (pi - x)", "pi", "1")]
        public void TheArgumentIsWhatHasToVanish(string expression, string destination, string expected) =>
            AssertLimit(expression, destination, ApproachFrom.BothSides, expected);

        /// <summary>The quotients the substitution was already made in, unchanged.</summary>
        [Theory]
        [InlineData("sin(x) / x", "0", "1")]
        [InlineData("tan(x) / x", "0", "1")]
        [InlineData("x / sin(x)", "0", "1")]
        [InlineData("arcsin(x) / x", "0", "1")]
        [InlineData("arctan(x) / x", "0", "1")]
        [InlineData("sin(3 * x) / sin(5 * x)", "0", "3/5")]
        [InlineData("tan(x) / sin(x)", "0", "1")]
        public void TheQuotientsAreUnchanged(string expression, string destination, string expected) =>
            AssertLimit(expression, destination, ApproachFrom.BothSides, expected);

        /// <summary>
        /// A product with no vanishing argument in it must be left alone, so that the ordinary
        /// readings still answer it.
        /// </summary>
        [Theory]
        [InlineData("sin(x) * x", "0", "0")]
        [InlineData("sin(1 / x) * x", "+oo", "1")]
        [InlineData("tan(x) * cotan(x)", "0", "1")]
        public void ProductsWithoutOneAreLeftAlone(string expression, string destination, string expected) =>
            AssertLimit(expression, destination, ApproachFrom.BothSides, expected);
    }
}
