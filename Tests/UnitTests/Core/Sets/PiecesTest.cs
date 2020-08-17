using System.Collections.Generic;
using AngouriMath.Core;
using AngouriMath.Core.Numerix;
using Xunit;

namespace UnitTests.Core
{
    public class PiecesTest
    {
        public bool In(ComplexNumber num, Piece list) => In(num, new List<Piece>{list});
        public bool In(ComplexNumber num, List<Piece> list) => new Set {Pieces = list}.Contains(num);
            //=> list.Any(c => c.Contains(new OneElementPiece(num)));
           
        [Fact]
        public void PieceInversion()
        {
            var p = Piece.Interval(3, 5).SetLeftClosed(true).SetRightClosed(true);
            var inverted = PieceFunctions.Invert(p);
            Assert.False(In(4, inverted));
            Assert.True(In(123, inverted));
        }

        [Fact]
        public void PieceInversion1()
        {
            var p = Piece.Interval(3, 5).SetLeftClosed(true).SetRightClosed(true);
            var inverted = PieceFunctions.Invert(p);
            Assert.False(In(3, inverted));
        }

        [Fact]
        public void PieceInversion2()
        {
            var p = Piece.Interval(ComplexNumber.Create(1, 1), ComplexNumber.Create(2, 2)).SetLeftClosed(true, true).SetRightClosed(true, true);
            var inverted = PieceFunctions.Invert(p);
            Assert.True(In(120, inverted));
            Assert.True(In(ComplexNumber.Create(320, -13), inverted));
        }

        [Fact]
        public void PieceSubtraction1()
        {
            var p = Piece.Interval(ComplexNumber.Create(-1, -1), ComplexNumber.Create(2, 2)).SetLeftClosed(true, true).SetRightClosed(true, true);
            var left = PieceFunctions.Subtract(p, new OneElementPiece(0));
            Assert.False(In(0, left));
        }

        [Fact]
        public void PieceSubtraction2()
        {
            // [3; 4]
            var p = Piece.Interval(3, 4).SetLeftClosed(true).SetRightClosed(true);
            // [3; 4)
            var p1 = Piece.Interval(3, 4).SetLeftClosed(true).SetRightClosed(false);

            var left = PieceFunctions.Subtract(p, p1);
            Assert.True(In(4, left));
            Assert.False(In(5, left));
            Assert.False(In(3.99, left));
        }

        [Fact]
        public void PieceIntersection1()
        {
            var p = Piece.Interval(3, 4).SetLeftClosed(true).SetRightClosed(true);
            var p1 = Piece.Interval(3.5, 4.5).SetLeftClosed(true).SetRightClosed(true);
            var p2 = Piece.Interval(3.5, 4).SetLeftClosed(true).SetRightClosed(true);
            var ints = PieceFunctions.Intersect(p, p1);
            Assert.Equal(p2, ints);
        }

        [Fact]
        public void PieceIntersection2()
        {

            var U = Piece.Interval(ComplexNumber.NegNegInfinity, ComplexNumber.PosPosInfinity);
            var p = Piece.Interval(3, 4).SetLeftClosed(true).SetRightClosed(true);
            var p1 = Piece.Interval(ComplexNumber.Create(3, 5), ComplexNumber.Create(1, 6)).SetLeftClosed(true).SetRightClosed(true);
            var p2 = Piece.Interval(3, ComplexNumber.Create(1, 6)).SetLeftClosed(false).SetRightClosed(true);
            var p3 = Piece.Element(3);
            Assert.True(U.Contains(p));
            Assert.True(U.Contains(p1));
            Assert.True(U.Contains(p2));
            Assert.True(U.Contains(p3));
        }

        [Fact]
        public void PieceIntersection3()
        {
            var p = Piece.Interval(3, 7).SetLeftClosed(true).SetRightClosed(true);
            var p1 = Piece.Interval(1, 4).SetLeftClosed(true).SetRightClosed(true);
            var p2 = PieceFunctions.Intersect(p, p1);
            var p3 = Piece.Interval(3, 4).SetLeftClosed(true).SetRightClosed(true);
            Assert.Equal(p3, p2);
        }
    }
}
