using AngouriMath;
using static AngouriMath.Entity.Number;
using static AngouriMath.Entity;
using Xunit;

namespace UnitTests.Core
{
    public class SetsTest
    {
        private readonly Set A = MathS.Sets.Empty();
        private readonly Set B = MathS.Sets.Empty();
        private readonly Set C = MathS.Sets.Empty();

        public SetsTest()
        {
            A.AddElements(3, 4, 5);
            A.AddInterval(MathS.Sets.Interval(10, 15).SetLeftClosed(true).SetRightClosed(false));
            A.AddInterval(MathS.Sets.Interval(14, 19).SetLeftClosed(true).SetRightClosed(false));
            A.AddInterval(MathS.Sets.Interval(Complex.Create(8, 3), Complex.Create(11, 5)).SetLeftClosed(true).SetRightClosed(false));
            A.AddInterval(MathS.Sets.Interval(Complex.Create(3, 51), Complex.Create(3, 61)));

            B.AddElements(11);

            C.AddInterval(MathS.Sets.Interval(-10, 10).SetLeftClosed(false));
            C.AddInterval(MathS.Sets.Interval(-3, 3).SetRightClosed(false));
            C.AddInterval(MathS.Sets.Interval(-3 * MathS.i, 3 * MathS.i).SetRightClosed(true, false));
        }

        [Fact]
        public void IndividualNumbersInIndividualOneSet()
        {
            Assert.DoesNotContain((Entity)2, A);
            Assert.Contains((Entity)3, A);
            Assert.DoesNotContain((Entity)2.9, A);
            Assert.Contains((Entity)4, A);
            Assert.Contains((Entity)5, A);
        }

        [Fact]
        public void InvididualNumbersInIntervalsOneSet()
        {
            Assert.Contains(new Interval(11, 13), A);
            Assert.Contains(new Interval(11, 16), A);
            Assert.Contains(new Interval(10, 13), A);
            Assert.Contains(new Interval(10, 15), A);
        }

        

        [Fact]
        public void InvididualNumbersInIntervalsTwoSets()
        {
            var C = A - B;
            Assert.False(C.ContainsNode(11));
            Assert.True(C.ContainsNode(10.9));
            Assert.True(C.ContainsNode(11.1));
        }

        

        [Fact]
        public void RealIntervalDisjunctionTest()
        {
            Assert.True(C.ContainsNode(0));
            Assert.True(C.ContainsNode(-3));
            Assert.False(C.ContainsNode(-10));
        }

        [Fact]
        public void SetsDisjunction()
        {
            var D = C | A;
            Assert.True(D.ContainsNode(0));
            Assert.True(D.ContainsNode(-3));
            Assert.True(D.ContainsNode(18.9));
            Assert.False(D.ContainsNode(19));
        }

        [Fact]
        public void SetsConjuction()
        {
            var D = C & A;
            Assert.True(D.ContainsNode(5));
            Assert.False(D.ContainsNode(-3));
            Assert.False(D.ContainsNode(18.9));
            Assert.False(D.ContainsNode(19));
        }

        [Fact]
        public void SetsSubtraction()
        {
            var D = C - A;
            Assert.True(D.ContainsNode(-9.9));
            Assert.False(D.ContainsNode(3));
            Assert.False(D.ContainsNode(4));
            Assert.False(D.ContainsNode(5));
        }

        private readonly Set Af = MathS.Sets.Finite(3, 4, 5);
        private readonly Set Bf = MathS.Sets.Finite(1, 2, 4);
        private readonly Set Cf = MathS.Sets.Finite(-7);
        private readonly Set Df = MathS.Sets.Finite();
        private readonly Set Ef = MathS.Sets.Finite(Complex.Create(-1, -1));
        private readonly Set Gf = MathS.Sets.Finite(Complex.Create(-1, -1));

        [Fact]
        public void SetsFiniteTestDisj()
        {
            var Q = Assert.IsType<Set>(Af | Bf);
            Assert.Equal(5, Q.Pieces.Count);
        }

        [Fact]
        public void SetsFiniteTestConj()
        {
            var Q = Assert.IsType<Set>(Af & Bf);
            Assert.Single(Q.Pieces);
        }

        [Fact]
        public void SetsFiniteTestSub()
        {
            var Q = Assert.IsType<Set>(Af - Bf);
            Assert.Equal(2, Q.Pieces.Count);
        }

        [Fact]
        public void SetsFiniteTestDisj2()
        {
            var Q = Assert.IsType<Set>(Ef | Gf);
            Assert.Single(Q.Pieces);
        }

        [Fact]
        public void SetsFiniteTestSub2()
        {
            Assert.True(Df.IsEmpty);
            Assert.True(((Set)(Df - Af)).IsEmpty);
            Assert.True(((Set)(Df - Bf)).IsEmpty);
            Assert.True(((Set)(Df - Cf)).IsEmpty);
            Assert.True(((Set)(Df - Df)).IsEmpty);
            Assert.True(((Set)(Df - Ef)).IsEmpty);
            Assert.True(((Set)(Df - Gf)).IsEmpty);
        }

        [Fact]
        public void SetsFiniteTestConj2()
        {
            var Q = Assert.IsType<Set>(Ef & Gf);
            Assert.Single(Q.Pieces);
        }
    }
}
