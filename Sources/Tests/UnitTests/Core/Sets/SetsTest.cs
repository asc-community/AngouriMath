//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using static AngouriMath.Entity;
using Xunit;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Tests.Core
{
    public sealed class SetsTest
    {
        private static Set A = MathS.Sets.Empty;
        private static Set B = MathS.Sets.Empty;
        private static Set C = MathS.Sets.Empty;

        public SetsTest()
        {
            A = MathS.Union(A, new FiniteSet(3, 4, 5));
            A = MathS.Union(A, new Interval(10, true, 15, false));
            A = MathS.Union(A, new Interval(14, true, 19, false));
            // A = { 3, 4, 5 } \/ [10; 19)

            B = MathS.Union(B, new FiniteSet(11));

            C = MathS.Union(C, new Interval(-10, false, 10, true));
            C = MathS.Union(C, new Interval(-3, true, 3, false));
            // C = (-10; 10]
        }

        private void AssertContains(Set set, Entity el)
            => Assert.True(set.Contains(el), $"{set.Stringize()} does not contain {el.Stringize()} but should");

        private void AssertNotContains(Set set, Entity el)
            => Assert.True(!set.Contains(el), $"{set.Stringize()} contains {el.Stringize()} but should not");

        [Fact] public void IndividualNumbersInIndividualOneSet1() => AssertNotContains(A, 2);
        [Fact] public void IndividualNumbersInIndividualOneSet2() => AssertContains(A, 3);
        [Fact] public void IndividualNumbersInIndividualOneSet3() => AssertNotContains(A, 2.9);
        [Fact] public void IndividualNumbersInIndividualOneSet4() => AssertContains(A, 4);
        [Fact] public void IndividualNumbersInIndividualOneSet5() => AssertContains(A, 5);

        [Fact(Skip = "Subset needed")]
        public void InvididualNumbersInIntervalsOneSet1()
        {
            //Assert.Contains(new Interval(11, true, 13, true), A);
            //Assert.Contains(new Interval(11, true, 16, true), A);
            //Assert.Contains(new Interval(10, true, 13, true), A);
            //Assert.Contains(new Interval(10, true, 15, true), A);
        }

        public static Set C1 => A.SetSubtract(B);

        [Fact]
        public void InvididualNumbersInIntervalsTwoSets1() => Assert.False(C1.Contains(11));
        [Fact]
        public void InvididualNumbersInIntervalsTwoSets2() => Assert.True(C1.Contains(10.9));
        [Fact]
        public void InvididualNumbersInIntervalsTwoSets3() => Assert.True(C1.Contains(11.1));

        [Fact] public void RealIntervalDisjunctionTest1() => Assert.True(C.Contains(0));
        [Fact] public void RealIntervalDisjunctionTest2() => Assert.True(C.Contains(-3));
        [Fact] public void RealIntervalDisjunctionTest3() => Assert.False(C.Contains(-10));

        private static Set D => MathS.Union(C, A);

        [Fact] public void SetsDisjunction1() => Assert.True(D.Contains(0));
        [Fact] public void SetsDisjunction2() => Assert.True(D.Contains(-3));
        [Fact] public void SetsDisjunction3() => Assert.True(D.Contains(18.9));
        [Fact] public void SetsDisjunction4() => Assert.False(D.Contains(19));

        private static Set D1 => MathS.Intersection(C, A);

        [Fact] public void SetsConjuction1() => Assert.True(D1.Contains(5));
        [Fact] public void SetsConjuction2() => Assert.False(D1.Contains(-3));
        [Fact] public void SetsConjuction3() => Assert.False(D1.Contains(18.9));
        [Fact] public void SetsConjuction4() => Assert.False(D1.Contains(19));

        private static Set D2 => MathS.SetSubtraction(C, A);

        [Fact] public void SetsSubtraction1() => Assert.True(D2.Contains(-9.9));
        [Fact] public void SetsSubtraction2() => Assert.False(D2.Contains(3));
        [Fact] public void SetsSubtraction3() => Assert.False(D2.Contains(4));
        [Fact] public void SetsSubtraction4() => Assert.False(D2.Contains(3));
    }
}
