using AngouriMath;
using static AngouriMath.Entity.Number;
using static AngouriMath.Entity;
using Xunit;
using static AngouriMath.Entity.Set;

namespace UnitTests.Core
{
    public class SetsTest
    {
        private readonly Set A = MathS.Sets.Empty;
        private readonly Set B = MathS.Sets.Empty;
        private readonly Set C = MathS.Sets.Empty;

        public SetsTest()
        {
            A = MathS.Union(A, new FiniteSet(3, 4, 5));
            A = MathS.Union(A, new Interval(10, true, 15, false));
            A = MathS.Union(A, new Interval(14, true, 19, false));
            
            B = MathS.Union(B, new FiniteSet(11));

            C = MathS.Union(C, new Interval(-10, false, 10, true));
            C = MathS.Union(C, new Interval(-3, true, 3, false));
        }

        private void AssertContains(Set set, Entity el)
            => Assert.True(set.Contains(el), $"{set} does not contain {el} but should");

        private void AssertNotContains(Set set, Entity el)
            => Assert.True(!set.Contains(el), $"{set} contains {el} but should not");

        [Fact] public void IndividualNumbersInIndividualOneSet1() => AssertNotContains(A, 2);
        [Fact] public void IndividualNumbersInIndividualOneSet2() => AssertContains(A, 3);
        [Fact] public void IndividualNumbersInIndividualOneSet3() => AssertNotContains(A, 2.9);
        [Fact] public void IndividualNumbersInIndividualOneSet4() => AssertContains(A, 4);
        [Fact] public void IndividualNumbersInIndividualOneSet5() => AssertContains(A, 5);

        [Fact]
        public void InvididualNumbersInIntervalsOneSet()
        {
            Assert.Contains(new Interval(11, true, 13, true), A);
            Assert.Contains(new Interval(11, true, 16, true), A);
            Assert.Contains(new Interval(10, true, 13, true), A);
            Assert.Contains(new Interval(10, true, 15, true), A);
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
