using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Core.Numerix;
using Xunit;

namespace UnitTests.Core
{
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

        [Fact]
        public void IndividualNumbersInIndividualOneSet()
        {
            Assert.False(A.Contains(2));
            Assert.True(A.Contains(3));
            Assert.False(A.Contains(2.9));
            Assert.True(A.Contains(4));
            Assert.True(A.Contains(5));
        }

        [Fact]
        public void InvididualNumbersInIntervalsOneSet()
        {
            Assert.Contains(Piece.ElementOrInterval(11, 13), A);
            Assert.Contains(Piece.ElementOrInterval(11, 16), A);
            Assert.Contains(Piece.ElementOrInterval(10, 13), A);
            Assert.Contains(Piece.ElementOrInterval(10, 15), A);
        }

        private readonly Set B = MathS.Sets.Empty();

        [Fact]
        public void InvididualNumbersInIntervalsTwoSets()
        {
            var C = A - B;
            Assert.False(C.Contains(11));
            Assert.True(C.Contains(10.9));
            Assert.True(C.Contains(11.1));
        }

        private readonly Set C = MathS.Sets.Empty();

        [Fact]
        public void RealIntervalDisjunctionTest()
        {
            Assert.True(C.Contains(0));
            Assert.True(C.Contains(-3));
            Assert.False(C.Contains(-10));
        }

        [Fact]
        public void SetsDisjunction()
        {
            var D = C | A;
            Assert.True(D.Contains(0));
            Assert.True(D.Contains(-3));
            Assert.True(D.Contains(18.9));
            Assert.False(D.Contains(19));
        }

        [Fact]
        public void SetsConjuction()
        {
            var D = C & A;
            Assert.True(D.Contains(5));
            Assert.False(D.Contains(-3));
            Assert.False(D.Contains(18.9));
            Assert.False(D.Contains(19));
        }

        [Fact]
        public void SetsSubtraction()
        {
            var D = C - A;
            Assert.True(D.Contains(-9.9));
            Assert.False(D.Contains(3));
            Assert.False(D.Contains(4));
            Assert.False(D.Contains(5));
        }

        private readonly Set Af = MathS.Sets.Finite(3, 4, 5);
        private readonly Set Bf = MathS.Sets.Finite(1, 2, 4);
        private readonly Set Cf = MathS.Sets.Finite(-7);
        private readonly Set Df = MathS.Sets.Finite();
        private readonly Set Ef = MathS.Sets.Finite(ComplexNumber.Create(-1, -1));
        private readonly Set Gf = MathS.Sets.Finite(ComplexNumber.Create(-1, -1));

        [Fact]
        public void SetsFiniteTestDisj()
        {
            var Q = (Af | Bf) as Set;
            Assert.Equal(5, Q?.Pieces.Count);
        }

        [Fact]
        public void SetsFiniteTestConj()
        {
            var Q = (Af & Bf) as Set;
            Assert.Equal(1, Q?.Pieces.Count);
        }

        [Fact]
        public void SetsFiniteTestSub()
        {
            var Q = (Af - Bf) as Set;
            Assert.Equal(2, Q?.Pieces.Count);
        }

        [Fact]
        public void SetsFiniteTestDisj2()
        {
            var Q = (Ef | Gf) as Set;
            Assert.Equal(1, Q?.Pieces.Count);
        }

        [Fact]
        public void SetsFiniteTestSub2()
        {
            Assert.True(Df.IsEmpty());
            Assert.True(((Set)(Df - Af)).IsEmpty());
            Assert.True(((Set)(Df - Bf)).IsEmpty());
            Assert.True(((Set)(Df - Cf)).IsEmpty());
            Assert.True(((Set)(Df - Df)).IsEmpty());
            Assert.True(((Set)(Df - Ef)).IsEmpty());
            Assert.True(((Set)(Df - Gf)).IsEmpty());
        }

        [Fact]
        public void SetsFiniteTestConj2()
        {
            var Q = (Ef & Gf) as Set;
            Assert.Equal(1, Q?.Pieces.Count);
        }
    }
}
