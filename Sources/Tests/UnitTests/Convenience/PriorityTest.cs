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
        [InlineData("(a > b) = (c > d)", "(a > b) = (c > d)")]
        [InlineData("(a < b) = (c < d)", "(a < b) = (c < d)")]
        [InlineData("a > b and b = (c > d)", "a > b = (c > d)")]
        [InlineData("a < b and b = (c < d)", "a < b = (c < d)")]
        [InlineData("a > b and b = c and c > d", "a > b = c > d")]
        [InlineData("a < b and b = c and c < d", "a < b = c < d")]
        [InlineData(@"A = B \/ C", @"A = (B \/ C)")]
        [InlineData(@"A = B \ C", @"A = (B \ C)")]
        [InlineData(@"B \ C = A", @"(B \ C) = A")]
        [InlineData(@"A /\ B \/ C", @"(A /\ B) \/ C")]
        [InlineData(@"A \/ B /\ C", @"A \/ (B /\ C)")]
        [InlineData(@"A \/ B = B /\ C", @"(A \/ B) = (B /\ C)")]
        [InlineData(@"A /\ B \/ C \ D \/ E /\ F", @"(((A /\ B) \/ C) \ D) \/ (E /\ F)")]
        [InlineData(@"A * B + C - D + E * F", @"(((A * B) + C) - D) + (E * F)")]
        [InlineData(@"A and B or C xor D or E xor F and G", @"((A and B) or (C xor D)) or (E xor (F and G))")]
        [InlineData(@"(A or B) xor (B or C)", @"(A or B) xor (B or C)")]
        [InlineData("a and b or c", "(a and b) or c")]
        [InlineData("a or b and c", "a or (b and c)")]
        [InlineData("a or b implies c or d", "(a or b) implies (c or d)")]
        [InlineData("not a or not b implies not c or not d", "((not a) or (not b)) implies ((not c) or (not d))")]
        [InlineData("a = b and b > c and d", "a = b > c and d")]
        [InlineData("a provided b", "a provided b")]
        [InlineData("a provided b provided c", "a provided (b provided c)")]
        [InlineData("a provided b and c", "a provided (b and c)")]
        [InlineData("a provided b + c > 0", "a provided (b + c > 0)")]
        [InlineData("a + b provided b + c > 0", "(a + b) provided (b + c > 0)")]
        [InlineData("a + b provided b + c > 0 provided d", "(a + b) provided (((b + c) > 0) provided d)")]
        [InlineData("not a = b", "a <> b")]
        [InlineData("a = b and not b = c and c < d", "a = b <> c < d")]
        [InlineData("not a = b and not b = c", "a <> b <> c")]
        public void Test(string normalizedForm, string alternateForm)
        {
            Assert.Equal(alternateForm.ToEntity(), normalizedForm.ToEntity());
            Assert.Equal(normalizedForm, alternateForm.ToEntity().Stringize());
        }
    }
}
