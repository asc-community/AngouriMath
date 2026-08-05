//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Core.Exceptions;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// https://github.com/asc-community/AngouriMath/issues/381
    /// </summary>
    public sealed class CharacteristicPolynomialTest
    {
        private static Entity Expanded(Entity.Matrix matrix, string variable)
            => MathS.CharacteristicPolynomial(matrix, variable).Expand().Simplify();

        // det(x*I - A) written out by hand for each of these.
        [Theory]
        [InlineData("[[1, 2], [3, 4]]", "x ^ 2 - 5 * x - 2")]
        [InlineData("[[2, 0], [0, 3]]", "x ^ 2 - 5 * x + 6")]
        [InlineData("[[0, 1], [-1, 0]]", "x ^ 2 + 1")]
        [InlineData("[[1, 0, 0], [0, 1, 0], [0, 0, 1]]", "x ^ 3 - 3 * x ^ 2 + 3 * x - 1")]
        [InlineData("[[2, 1], [1, 2]]", "x ^ 2 - 4 * x + 3")]
        public void MatchesTheHandComputedPolynomial(string matrix, string expected) =>
            Assert.Equal(Entity.Number.Integer.Create(0),
                (Expanded((Entity.Matrix)matrix.ToEntity(), "x") - expected.ToEntity()).Expand().Simplify());

        // The point of the polynomial: its roots are the eigenvalues. A diagonal matrix wears
        // them on its face, so it is the case that can be checked without solving anything.
        [Theory]
        [InlineData("[[5, 0], [0, 7]]", 5, 7)]
        [InlineData("[[-1, 0], [0, 4]]", -1, 4)]
        public void DiagonalEntriesAreRoots(string matrix, int first, int second)
        {
            var polynomial = MathS.CharacteristicPolynomial((Entity.Matrix)matrix.ToEntity(), "x");
            Assert.Equal(Entity.Number.Integer.Create(0), polynomial.Substitute("x", first).Simplify());
            Assert.Equal(Entity.Number.Integer.Create(0), polynomial.Substitute("x", second).Simplify());
        }

        // Monic, which is what distinguishes det(x*I - A) from det(A - x*I): for an odd-sized
        // matrix the other convention would put a -1 in front of the leading term.
        [Theory]
        [InlineData("[[1, 2], [3, 4]]", 2)]
        [InlineData("[[1, 2, 3], [4, 5, 6], [7, 8, 10]]", 3)]
        public void IsMonic(string matrix, int degree)
        {
            var polynomial = Expanded((Entity.Matrix)matrix.ToEntity(), "x");
            // The leading coefficient is what survives when the polynomial is divided by x^n
            // and x is sent to infinity.
            Assert.Equal(Entity.Number.Integer.Create(1),
                (polynomial / MathS.Pow("x", degree)).Limit("x", Entity.Number.Real.PositiveInfinity).Simplify());
        }

        // Symbolic entries are as good as numeric ones: this is det(x*I - A) either way.
        [Fact]
        public void WorksOnSymbolicEntries() =>
            Assert.Equal(Entity.Number.Integer.Create(0),
                (Expanded((Entity.Matrix)"[[a, b], [c, d]]".ToEntity(), "x")
                 - "x ^ 2 - (a + d) * x + a * d - b * c".ToEntity()).Expand().Simplify());

        [Fact]
        public void NonSquareMatrixIsRefused() =>
            Assert.Throws<InvalidMatrixOperationException>(
                () => MathS.CharacteristicPolynomial((Entity.Matrix)"[[1, 2, 3], [4, 5, 6]]".ToEntity(), "x"));
    }
}
