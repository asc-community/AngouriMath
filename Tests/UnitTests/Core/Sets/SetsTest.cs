using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Core.Numerix;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Core
{
    [TestClass]
    public class SetsTest
    {
        private readonly Set A = MathS.Sets.Empty();

        public SetsTest()
        {
            A.AddElements(3, 4, 5);
            A.AddInterval(MathS.Sets.Interval(10, 15).SetLeftClosed(true).SetRightClosed(false));
            A.AddInterval(MathS.Sets.Interval(14, 19).SetLeftClosed(true).SetRightClosed(false));
            A.AddInterval(MathS.Sets.Interval(ComplexNumber.Create(8, 3), ComplexNumber.Create(11, 5)).SetLeftClosed(true).SetRightClosed(false));
            A.AddInterval(MathS.Sets.Interval(ComplexNumber.Create(3, 51), ComplexNumber.Create(3, 61)));

            B.AddElements(11);

            C.AddInterval(MathS.Sets.Interval(-10, 10).SetLeftClosed(false));
            C.AddInterval(MathS.Sets.Interval(-3, 3).SetRightClosed(false));
            C.AddInterval(MathS.Sets.Interval(- 3 * MathS.i, 3 * MathS.i).SetRightClosed(true, false));
        }

        [TestMethod]
        public void IndividualNumbersInIndividualOneSet()
        {
            Assert.IsFalse(A.Contains(2));
            Assert.IsTrue(A.Contains(3));
            Assert.IsFalse(A.Contains(2.9));
            Assert.IsTrue(A.Contains(4));
            Assert.IsTrue(A.Contains(5));
        }

        [TestMethod]
        public void InvididualNumbersInIntervalsOneSet()
        {
            Assert.IsTrue(A.Contains(Piece.Interval(11, 13)));
            Assert.IsTrue(A.Contains(Piece.Interval(11, 16)));
            Assert.IsTrue(A.Contains(Piece.Interval(10, 13)));
            Assert.IsTrue(A.Contains(Piece.Interval(10, 15)));
        }

        private readonly Set B = MathS.Sets.Empty();

        [TestMethod]
        public void InvididualNumbersInIntervalsTwoSets()
        {
            var C = A - B;
            Assert.IsFalse(C.Contains(11));
            Assert.IsTrue(C.Contains(10.9));
            Assert.IsTrue(C.Contains(11.1));
        }

        private readonly Set C = MathS.Sets.Empty();

        [TestMethod]
        public void RealIntervalDisjunctionTest()
        {
            Assert.IsTrue(C.Contains(0));
            Assert.IsTrue(C.Contains(-3));
            Assert.IsFalse(C.Contains(-10));
        }

        [TestMethod]
        public void SetsDisjunction()
        {
            var D = C | A;
            Assert.IsTrue(D.Contains(0));
            Assert.IsTrue(D.Contains(-3));
            Assert.IsTrue(D.Contains(18.9));
            Assert.IsFalse(D.Contains(19));
        }

        [TestMethod]
        public void SetsConjuction()
        {
            var D = C & A;
            Assert.IsTrue(D.Contains(5));
            Assert.IsFalse(D.Contains(-3));
            Assert.IsFalse(D.Contains(18.9));
            Assert.IsFalse(D.Contains(19));
        }

        [TestMethod]
        public void SetsSubtraction()
        {
            var D = C - A;
            Assert.IsTrue(D.Contains(-9.9));
            Assert.IsFalse(D.Contains(3));
            Assert.IsFalse(D.Contains(4));
            Assert.IsFalse(D.Contains(5));
        }

        private readonly Set Af = MathS.Sets.Finite(3, 4, 5);
        private readonly Set Bf = MathS.Sets.Finite(1, 2, 4);
        private readonly Set Cf = MathS.Sets.Finite(-7);
        private readonly Set Df = MathS.Sets.Finite();
        private readonly Set Ef = MathS.Sets.Finite(ComplexNumber.Create(-1, -1));
        private readonly Set Gf = MathS.Sets.Finite(ComplexNumber.Create(-1, -1));

        [TestMethod]
        public void SetsFiniteTestDisj()
        {
            var Q = (Af | Bf) as Set;
            Assert.AreEqual(5, Q?.Pieces.Count);
        }

        [TestMethod]
        public void SetsFiniteTestConj()
        {
            var Q = (Af & Bf) as Set;
            Assert.AreEqual(1, Q?.Pieces.Count);
        }

        [TestMethod]
        public void SetsFiniteTestSub()
        {
            var Q = (Af - Bf) as Set;
            Assert.AreEqual(2, Q?.Pieces.Count);
        }

        [TestMethod]
        public void SetsFiniteTestDisj2()
        {
            var Q = (Ef | Gf) as Set;
            Assert.AreEqual(1, Q?.Pieces.Count);
        }

        [TestMethod]
        public void SetsFiniteTestSub2()
        {
            Assert.IsTrue(Df.IsEmpty());
            Assert.IsTrue(((Set)(Df - Af)).IsEmpty());
            Assert.IsTrue(((Set)(Df - Bf)).IsEmpty());
            Assert.IsTrue(((Set)(Df - Cf)).IsEmpty());
            Assert.IsTrue(((Set)(Df - Df)).IsEmpty());
            Assert.IsTrue(((Set)(Df - Ef)).IsEmpty());
            Assert.IsTrue(((Set)(Df - Gf)).IsEmpty());
        }

        [TestMethod]
        public void SetsFiniteTestConj2()
        {
            var Q = (Ef & Gf) as Set;
            Assert.AreEqual(1, Q?.Pieces.Count);
        }
    }
}
