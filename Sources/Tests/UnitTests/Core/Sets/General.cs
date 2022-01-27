//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using Xunit;
using AngouriMath.Extensions;

namespace AngouriMath.Tests.Core.Sets
{
    public sealed class General
    {
        [Theory]
        [InlineData(@"{}", "{}")]
        [InlineData(@"{1}", "{1}")]
        [InlineData(@"[2; 3)", "[2; 3)")]
        [InlineData(@"(2; 3)", "(2; 3)")]
        [InlineData(@"[2; 3]", "[2; 3]")]
        [InlineData(@"(2; 3]", "(2; 3]")]
        [InlineData(@"3 + {2}", "{5}")]
        [InlineData(@"1 + [2; 3]", "[3; 4]")]
        [InlineData(@"[{1, 2}; {3, 4}]", "{[1; 3], [1; 4], [2; 3], [2; 4]}")]
        [InlineData(@"{[1; 2], [3; 4]}", "{[1; 2], [3; 4]}")]
        [InlineData(@"{ x : x }", "{ x : x }")]
        [InlineData(@"{ 1, 2, 3 } \/ { 4 }", "{ 1, 2, 3, 4 }")]
        [InlineData(@"{ 1, 2, 3 } \/ { 4, 5 }", "{ 1, 2, 3, 4, 5 }")]
        [InlineData(@"{ 1, 2, 3 } /\ { 2, 3 }", "{ 2, 3 }")]
        [InlineData(@"A /\ A", "A")]
        [InlineData(@"A \/ A", "A")]
        [InlineData(@"A \ A", "{}")]
        [InlineData(@"{ 1 } \/ { 1 }", "{ 1 }")]
        [InlineData(@"[2; 3] \/ [3; 5]", "[2; 5]")]
        [InlineData(@"[2; 3) \/ (3; 5]", @"[2; 3) \/ (3; 5]")]
        [InlineData(@"[2; 3] \/ (3; 5]", @"[2; 5]")]
        [InlineData(@"[3; 5] \/ [2; 3)", @"[2; 5]")]
        [InlineData(@"[3; 5] /\ [5; 7)", @"{ 5 }")]
        [InlineData(@"[3; 5] /\ [3; 7)", @"[3; 5]")]
        [InlineData(@"[3; 5] /\ [3; 5)", @"[3; 5)")]
        [InlineData(@"[3; 5] \ [5; 6]", @"[3; 5)")]
        [InlineData(@"[3; 5] \ [4; 6]", @"[3; 4)")]
        [InlineData(@"{ 1, 2, 3, 4 } \ { 1, 3 }", @"{ 2, 4 }")]
        [InlineData(@"{ 1, 2 } \ [2; 2]", @"{ 1 }")]
        [InlineData(@"[2; 2)", @"{}")]
        [InlineData(@"(2; 2)", @"{}")]
        [InlineData(@"(2; 2]", @"{}")]
        [InlineData(@"[2; 2]", @"{ 2 }")]
        [InlineData(@"{ x : x > 0 } /\ { z : z > 0 }", @"{ u : u > 0 }")]
        [InlineData(@"{ x : x > 0 } /\ { z : z < 0 }", @"{}")]
        [InlineData(@"{ x : x >= 0 } /\ { z : z < 0 }", @"{}")]
        [InlineData(@"{ x : x > 0 } /\ { z : z <= 0 }", @"{}")]
        [InlineData(@"{ x : 0 < x } /\ { z : z <= 0 }", @"{}")]
        [InlineData(@"{ 1, 2 } \/ [0; 4]", @"[0; 4]")]
        [InlineData(@"{ 2 } \/ [0; 4]", @"[0; 4]")]
        [InlineData(@"{ 1, 2, 5 } \/ [0; 4]", @"{ 5 } \/ [0; 4]")]
        [InlineData(@"{ 2 } /\ { x : x2 = 4 }", @"{ 2 }")]
        [InlineData(@"{ x : x2 = 4 } /\ { 2 }", @"{ 2 }")]
        [InlineData(@"{ x : false }", @"{}")]
        [InlineData(@"{ x : 2 = 4 }", @"{}")]
        [InlineData(@"[ { sqrt(3), sqrt(5) }; sqrt(10) ]", @"{ [ sqrt(3); sqrt(10) ], [ sqrt(5); sqrt(10) ] }")]
        [InlineData(@"[ sqrt(3); { sqrt(5), sqrt(10) } ]", @"{ [ sqrt(3); sqrt(5) ], [ sqrt(3); sqrt(10) ] }")]
        [InlineData(@"[ { sqrt(2), sqrt(3) }; { sqrt(5), sqrt(10) } ]", @"{ [ sqrt(2); sqrt(5) ], [ sqrt(3); sqrt(5) ], [ sqrt(2); sqrt(10) ], [ sqrt(3); sqrt(10) ] }")]
        public void TestSimplify(string unsimplified, string simplified)
        {
            var actual = unsimplified.ToEntity();
            actual = actual.Simplify();
            Assert.Equal(simplified.ToEntity(), actual);
        }

        [Theory(Skip = "Not working for now")]
        [InlineData(@"{ x : x >= 0 } \/ { z : z <= 0 }", @"RR")]
        [InlineData(@"[0; +oo) /\ { x : x2 = 4 }", @"{ 2 }")]
        public void TestSimplifySkip(string unexpected, string expected)
            => TestSimplify(unexpected, expected);
    }
}
