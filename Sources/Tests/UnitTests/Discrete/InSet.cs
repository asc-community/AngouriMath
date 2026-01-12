//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using Xunit;
using AngouriMath.Extensions;
using AngouriMath;

namespace AngouriMath.Tests.Discrete
{
    public sealed class InSet
    {
        [Theory]
        [InlineData(@"x in {}", false)]
        [InlineData(@"x in { x }", true)]
        [InlineData(@"2 in { x }", null)]
        [InlineData(@"1 in { 2, 3 }", false)]
        [InlineData(@"1 in { 2, 3 } \/ { 3, 5 }", false)]
        [InlineData(@"5 in { 2, 3 } \/ { 3, 5 }", true)]
        [InlineData(@"a in { 2, 3 } \/ { 3, 5 } \/ [1; a]", true)]
        [InlineData(@"a in [1; a] \/ { 2, 3 } \/ { 3, 5 } ", true)]
        [InlineData(@"a in [0; 1] \/ [a; b] \/ [2; 3] ", true)]
        [InlineData(@"b in [0; 1] \/ [a; b] \/ [2; 3] ", true)]
        [InlineData(@"1 in [0; 1] \/ [a; b] \/ [2; 3] ", true)]
        [InlineData(@"1.5 in [0; 1] \/ [a; b] \/ [2; 3] ", null)]
        [InlineData(@"3 in [0; 1] \/ [a; b] \/ [2; 3] ", true)]
        [InlineData(@"c in [0; 1] \/ [a; b] \/ [2; 3] ", null)]
        [InlineData(@"a in { 2, 3, 5 } \/ [1; x]", null)]
        [InlineData(@"x in [2; 3]", null)]
        [InlineData(@"3 in [2; 3]", true)]
        [InlineData(@"2 in [2; 3]", true)]
        [InlineData(@"2.5 in [2; 3]", true)]
        [InlineData(@"1.9 in [2; 3]", false)]
        [InlineData(@"3.1 in [2; 3]", false)]
        public void TestInnerSimplify(string input, bool? expected)
        {
            var ent = input.ToEntity().InnerSimplified;
            Assert.Equal((Entity)(expected ?? ent), ent);
        }
    }
}
