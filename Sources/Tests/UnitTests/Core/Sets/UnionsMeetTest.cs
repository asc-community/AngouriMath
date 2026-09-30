//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core.Sets
{
    /// <summary>
    /// Two unions of intervals and listed sets meet piece by piece, which is what a conjunction of
    /// two inequalities is solved to: Sullivan and Mackey's Prob 4.11.22 asks whether one rational
    /// inequality implies another for every positive <c>x</c>, and the <c>x</c> where it fails are
    /// such a meeting. https://github.com/asc-community/AngouriMath/issues/1409
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class UnionsMeetTest
    {
        [Theory]
        [InlineData("((0; 1) \\/ (3; 5)) /\\ ((-1; 2) \\/ (4; 6))", "(0; 1) \\/ (4; 5)")]
        [InlineData("((0; 1) \\/ (3; 5)) /\\ ([1; 3] \\/ {7})", "{ }")]
        [InlineData("((0; 1) \\/ {4}) /\\ ((1/2; 2) \\/ [4; 5))", "(1/2; 1) \\/ {4}")]
        [InlineData("((0; (5 - sqrt(21))/2) \\/ ((5 + sqrt(21))/2; +oo)) /\\ ({ (3 - sqrt(5))/2 } \\/ [(3 - sqrt(5))/2; (3 + sqrt(5))/2])", "{ }")]
        public void PieceByPiece(string meeting, string expected)
            => Assert.Equal(expected.ToEntity().InnerSimplified, meeting.ToEntity().InnerSimplified);

        /// <summary>
        /// A union of numbers is written as its disjoint pieces, the listed numbers first and the
        /// intervals in increasing order, a point at an open end closing it and an empty interval
        /// dropped, however it was grouped: two ways of writing one set come out the same.
        /// </summary>
        [Theory]
        [InlineData("{ 0, 1 } \\/ (-oo; 0) \\/ (1; +oo)", "(-oo; 0] \\/ [1; +oo)")]
        [InlineData("(2; 3) \\/ (0; 1) \\/ { 5 }", "{ 5 } \\/ (0; 1) \\/ (2; 3)")]
        [InlineData("(0; 2) \\/ ((1; 3) \\/ (5; 6))", "(0; 3) \\/ (5; 6)")]
        [InlineData("(3; 3) \\/ ((0; 1) \\/ { 7 })", "{ 7 } \\/ (0; 1)")]
        public void AUnionOfNumbersIsWrittenAsItsPieces(string written, string pieces)
            => Assert.Equal(pieces.ToEntity().InnerSimplified, written.ToEntity().InnerSimplified);

        /// <summary>What the inequality solver answers comes out as its pieces too.</summary>
        [Fact]
        public void ASolvedInequalityIsItsPieces()
            => Assert.Equal("(-oo; 0] \\/ [1; +oo)".ToEntity().InnerSimplified, "x^2 >= x".ToEntity().Solve("x"));
    }
}
