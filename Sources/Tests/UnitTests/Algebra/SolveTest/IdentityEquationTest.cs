//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Algebra
{
    /// <summary>
    /// An equation that puts no condition on the variable is satisfied by every value of it,
    /// not by none. Answering such an equation with the empty set, or with the single root 0,
    /// is what made a system containing one come back as null from
    /// <see cref="Core.EquationSystem.Solve"/> --
    /// https://github.com/asc-community/AngouriMath/issues/550.
    /// </summary>
    public sealed class IdentityEquationTest
    {
        [Theory]
        [InlineData("x - x = 0")]
        [InlineData("0 = 0")]
        [InlineData("2 * x - 2 * x = 0")]
        [InlineData("x + 1 - x - 1 = 0")]
        [InlineData("x ^ 2 - x ^ 2 = 0")]
        [InlineData("x ^ 2 + x = x + x ^ 2")]
        public void EveryValueSatisfiesAnIdentity(string equation) =>
            Assert.Equal(MathS.Sets.C, equation.ToEntity().Solve("x"));

        [Theory]
        [InlineData("1 = 0")]
        [InlineData("x - x + 1 = 0")]
        [InlineData("x - x = 1")]
        [InlineData("2 = 3")]
        public void NoValueSatisfiesAContradiction(string equation) =>
            Assert.Equal(MathS.Sets.Empty, equation.ToEntity().Solve("x"));

        // A term that cancelled must not be counted towards the degree either. x^2 - x^2 + x + 1
        // is linear, and reading it as a quadratic with a zero leading coefficient is how the
        // wrong root of x - x arose.
        [Theory]
        [InlineData("x - 3 = 0", new[] { 3.0 })]
        [InlineData("x ^ 2 - 4 = 0", new[] { 2.0, -2.0 })]
        [InlineData("x ^ 2 - x ^ 2 + x + 1 = 0", new[] { -1.0 })]
        [InlineData("x ^ 2 - x ^ 2 + x ^ 2 - 4 = 0", new[] { 2.0, -2.0 })]
        [InlineData("x ^ 3 - x = 0", new[] { 0.0, 1.0, -1.0 })]
        public void OrdinaryEquationsKeepTheirRoots(string equation, double[] expected)
        {
            var roots = (Entity.Set.FiniteSet)equation.ToEntity().Solve("x");
            Assert.Equal(expected.Length, roots.Count);
            foreach (var root in roots)
                Assert.Contains(expected, e =>
                    System.Math.Abs(e - root.EvalNumerical().RealPart.EDecimal.ToDouble()) < 1e-9);
        }
    }
}
