//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using Xunit;
using AngouriMath.Extensions;

namespace AngouriMath.Tests.Common
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
        [InlineData("a provided b", "a provided b")]
        [InlineData("a provided b provided c", "(a provided b) provided c")]
        [InlineData("a provided (b provided c)", "a provided (b provided c)")]
        [InlineData("a provided b and c", "a provided (b and c)")]
        [InlineData("a provided b + c > 0", "a provided (b + c > 0)")]
        [InlineData("a + b provided b + c > 0", "(a + b) provided (b + c > 0)")]
        [InlineData("a + b provided b + c > 0 provided d", "((a + b) provided (b + c > 0)) provided d")]
        public void Test(string implic, string explic)
        {
            Assert.Equal(explic.ToEntity(), implic.ToEntity());
        }
    }
}
