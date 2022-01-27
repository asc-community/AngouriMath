//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using Xunit;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Tests.Core.Sets
{
    public sealed class FiniteAndFinite
    {
        private readonly Set A = new FiniteSet(1,   3, 5);
        private readonly Set B = new FiniteSet(1,      5);
        private readonly Set C = new FiniteSet(  2, 3);

        private void Test(Entity actual, params Entity[] exp)
        {
            var finite = Assert.IsType<FiniteSet>(actual.InnerSimplified);
            Assert.Equal(new FiniteSet(exp), finite);
        }

        [Fact] public void Union1() => Test(A.Unite(B), 1, 3, 5);
        [Fact] public void Union2() => Test(B.Unite(A), 1, 3, 5);
        [Fact] public void Union3() => Test(A.Unite(C), 1, 2, 3, 5);
        [Fact] public void Union4() => Test(C.Unite(A), 1, 2, 3, 5);
        [Fact] public void Union5() => Test(B.Unite(C), 1, 2, 3, 5);
        [Fact] public void Union6() => Test(C.Unite(B), 1, 2, 3, 5);

        [Fact] public void Intersection1() => Test(A.Intersect(B), 1, 5);
        [Fact] public void Intersection2() => Test(B.Intersect(A), 1, 5);
        [Fact] public void Intersection3() => Test(A.Intersect(C), 3);
        [Fact] public void Intersection4() => Test(C.Intersect(A), 3);
        [Fact] public void Intersection5() => Test(B.Intersect(C));
        [Fact] public void Intersection6() => Test(C.Intersect(B));

        [Fact] public void Subtraction1() => Test(A.SetSubtract(B), 3);
        [Fact] public void Subtraction2() => Test(B.SetSubtract(A));
        [Fact] public void Subtraction3() => Test(A.SetSubtract(C), 1, 5);
        [Fact] public void Subtraction4() => Test(C.SetSubtract(A), 2);
        [Fact] public void Subtraction5() => Test(B.SetSubtract(C), 1, 5);
        [Fact] public void Subtraction6() => Test(C.SetSubtract(B), 2, 3);
    }
}
