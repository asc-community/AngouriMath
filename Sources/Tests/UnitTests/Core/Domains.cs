//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core
{
    public sealed class Domains
    {
        [Theory]
        [InlineData("domain(sqrt(-3), RR)")]
        [InlineData("domain(sqrt(3), ZZ)")]
        [InlineData("domain(sqrt(3), QQ)")]
        [InlineData("1 / 0")]
        [InlineData("domain(true and false, CC)")]
        [InlineData("domain(domain(sqrt(-3), RR) + 3, CC)")]
        [InlineData("domain(sqrt(4 / 9), ZZ)")]
        [InlineData("domain(1 / 2, ZZ)")]
        public void CheckNaN(string expr)
            => Assert.Equal(MathS.NaN, expr.EvalNumerical());

        [Theory]
        [InlineData("domain(sqrt(4), RR)")]
        [InlineData("domain(sqrt(4), QQ)")]
        [InlineData("domain(sqrt(4), ZZ)")]
        [InlineData("domain(sqrt(4 / 9), QQ)")]
        [InlineData("domain(sqrt(-3), CC)")]
        [InlineData("domain(3 / 5, CC)")]
        [InlineData("domain(3 / 5, RR)")]
        [InlineData("domain(3 / 5, QQ)")]
        public void CheckNotNaN(string expr)
            => Assert.NotEqual(MathS.NaN, expr.EvalNumerical());
    }
}
