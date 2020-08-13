using System.Collections.Generic;
using AngouriMath.Core;
using AngouriMath.Core.Numerix;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Core
{
    [TestClass]
    public class PiecesTest
    {
        public bool In(ComplexNumber num, Piece list) => In(num, new List<Piece>{list});
        public bool In(ComplexNumber num, List<Piece> list) => new Set {Pieces = list}.Contains(num);
            //=> list.Any(c => c.Contains(new OneElementPiece(num)));
           
        [TestMethod]
        public void PieceInversion()
        {
            var p = Piece.Interval(3, 5).SetLeftClosed(true).SetRightClosed(true);
            var inverted = PieceFunctions.Invert(p);
            Assert.IsFalse(In(4, inverted));
            Assert.IsTrue(In(123, inverted));
        }

        [TestMethod]
        public void PieceInversion1()
        {
            var p = Piece.Interval(3, 5).SetLeftClosed(true).SetRightClosed(true);
            var inverted = PieceFunctions.Invert(p);
            Assert.IsFalse(In(3, inverted));
        }

        [TestMethod]
        public void PieceInversion2()
        {
            var p = Piece.Interval(ComplexNumber.Create(1, 1), ComplexNumber.Create(2, 2)).SetLeftClosed(true, true).SetRightClosed(true, true);
            var inverted = PieceFunctions.Invert(p);
            Assert.IsTrue(In(120, inverted));
            Assert.IsTrue(In(ComplexNumber.Create(320, -13), inverted));
        }

        [TestMethod]
        public void PieceSubtraction1()
        {
            var p = Piece.Interval(ComplexNumber.Create(-1, -1), ComplexNumber.Create(2, 2)).SetLeftClosed(true, true).SetRightClosed(true, true);
            var left = PieceFunctions.Subtract(p, new OneElementPiece(0));
            Assert.IsFalse(In(0, left));
        }

        [TestMethod]
        public void PieceSubtraction2()
        {
            // [3; 4]
            var p = Piece.Interval(3, 4).SetLeftClosed(true).SetRightClosed(true);
            // [3; 4)
            var p1 = Piece.Interval(3, 4).SetLeftClosed(true).SetRightClosed(false);

            var left = PieceFunctions.Subtract(p, p1);
            Assert.IsTrue(In(4, left));
            Assert.IsFalse(In(5, left));
            Assert.IsFalse(In(3.99, left));
        }

        [TestMethod]
        public void PieceIntersection1()
        {
            var p = Piece.Interval(3, 4).SetLeftClosed(true).SetRightClosed(true);
            var p1 = Piece.Interval(3.5, 4.5).SetLeftClosed(true).SetRightClosed(true);
            var p2 = Piece.Interval(3.5, 4).SetLeftClosed(true).SetRightClosed(true);
            var ints = PieceFunctions.Intersect(p, p1);
            Assert.AreEqual(p2, ints);
        }

        [TestMethod]
        public void PieceIntersection2()
        {

            var U = Piece.Interval(ComplexNumber.NegNegInfinity, ComplexNumber.PosPosInfinity);
            var p = Piece.Interval(3, 4).SetLeftClosed(true).SetRightClosed(true);
            var p1 = Piece.Interval(ComplexNumber.Create(3, 5), ComplexNumber.Create(1, 6)).SetLeftClosed(true).SetRightClosed(true);
            var p2 = Piece.Interval(3, ComplexNumber.Create(1, 6)).SetLeftClosed(false).SetRightClosed(true);
            var p3 = Piece.Element(3);
            Assert.IsTrue(U.Contains(p));
            Assert.IsTrue(U.Contains(p1));
            Assert.IsTrue(U.Contains(p2));
            Assert.IsTrue(U.Contains(p3));
        }

        [TestMethod]
        public void PieceIntersection3()
        {
            var p = Piece.Interval(3, 7).SetLeftClosed(true).SetRightClosed(true);
            var p1 = Piece.Interval(1, 4).SetLeftClosed(true).SetRightClosed(true);
            var p2 = PieceFunctions.Intersect(p, p1);
            var p3 = Piece.Interval(3, 4).SetLeftClosed(true).SetRightClosed(true);
            Assert.AreEqual(p3, p2);
        }
    }
}
