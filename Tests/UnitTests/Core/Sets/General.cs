using AngouriMath;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;
using static AngouriMath.MathS;
using static AngouriMath.MathS.Sets;
using AngouriMath.Extensions;

namespace UnitTests.Core.Sets
{
    public class General
    {
        [Theory]
        [InlineData(@"{}", "{}")]
        [InlineData(@"{1}", "{1}")]
        [InlineData(@"[2; 3)", "[2; 3)")]
        [InlineData(@"(2; 3)", "(2; 3)")]
        [InlineData(@"[2; 3]", "[2; 3]")]
        [InlineData(@"(2; 3]", "(2; 3]")]
        [InlineData(@"3 + {2}", "3 + {2}")]
        [InlineData(@"1 + [2; 3]", "1 + [2; 3]")]
        [InlineData(@"[{1, 2}; {3, 4}]", "[{1, 2}; {3, 4}]")]
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
        [InlineData(@"{ x : x > 0 } \/ { z : z > 0 }", @"{ u : u > 0 }")]
        [InlineData(@"{ x : x > 0 } \/ { z : z < 0 }", @"{}")]
        [InlineData(@"{ x : x >= 0 } \/ { z : z < 0 }", @"{}")]
        [InlineData(@"{ x : x > 0 } \/ { z : z <= 0 }", @"{}")]
        [InlineData(@"{ x : 0 < x } \/ { z : z <= 0 }", @"{}")]
        [InlineData(@"{ 1, 2 } \/ [0; 4]", @"[0; 4]")]
        [InlineData(@"{ 2 } \/ [0; 4]", @"[0; 4]")]
        [InlineData(@"{ 1, 2, 5 } \/ [0; 4]", @"{ 5 } \/ [0; 4]")]
        [InlineData(@"{ 2 } /\ { x : x2 = 4 }", @"{ 2 }")]
        [InlineData(@"{ x : x2 = 4 } /\ { 2 }", @"{ 2 }")]
        [InlineData(@"{ x : false }", @"{}")]
        [InlineData(@"{ x : 2 = 4 }", @"{}")]
        public void TestSimplify(string unexpected, string expected)
        {
            var actual = unexpected.ToEntity().Simplify();
            Assert.Equal(expected.ToEntity(), actual);
        }

        [Theory(Skip = "Smarter simplification algorithms needed")]
        [InlineData(@"{ x : x >= 0 } \/ { z : z <= 0 }", @"{ 0 }")]
        [InlineData(@"[0; +oo) /\ { x : x2 = 4 }", @"{ 5 } \/ [0; 4]")]
        public void TestSimplifySkip(string unexpected, string expected)
            => TestSimplify(unexpected, expected);
    }
}
