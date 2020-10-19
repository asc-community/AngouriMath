using AngouriMath;
using static AngouriMath.Entity.Number;
using Xunit;
using System.Collections.Generic;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;
using AngouriMath.Extensions;

namespace UnitTests.Common
{
    public class PriorityTest
    {
        [Theory]
        [InlineData("a + b", "a + b")]
        [InlineData("a + b - c", "(a + b) - c")]
        [InlineData("a + b * c", "a + (b * c)")]
        [InlineData("a ^ b ^ c", "a ^ (b ^ c)")]
        [InlineData("a + b = c + d", "(a + b) = (c + d)")]
        [InlineData("a > b = c > d", "(a > b) = (c > d)")]
        [InlineData("a < b = c < d", "(a < b) = (c < d)")]
        [InlineData(@"A = B \/ C", @"A = (B \/ C)")]
        [InlineData(@"A = B \ C", @"A = (B \ C)")]
        [InlineData(@"B \ C = A", @"(B \ C) = A")]
        [InlineData(@"A /\ B \/ C", @"(A /\ B) \/ C")]
        [InlineData(@"A \/ B /\ C", @"A \/ (B /\ C)")]
        [InlineData(@"A \/ B = B /\ C", @"(A \/ B) = (B /\ C)")]
        [InlineData("a and b or c", "(a and b) or c")]
        [InlineData("a or b and c", "a or (b and c)")]
        [InlineData("a or b implies c or d", "(a or b) implies (c or d)")]
        [InlineData("not a or not b implies not c or not d", "((not a) or (not b)) implies ((not c) or (not d))")]
        [InlineData("a and b = c > d", "a and (b = (c > d))")]
        public void Test(string implic, string explic)
        {
            Assert.Equal(explic.ToEntity(), implic.ToEntity());
        }
    }
}
