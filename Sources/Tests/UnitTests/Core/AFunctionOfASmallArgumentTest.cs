//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;
using PeterO.Numbers;
using Xunit;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// A function's value is rounded onto zero only as the residual it leaves at its own zeros, which is relative
    /// to its argument: <c>sin(pi)</c> leaves about <c>1e-100 pi</c>, where <c>sin(1e-60)</c> cancels nothing.
    /// Every one of these was 0 at the default hundred digits. References from mpmath at 80 digits.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/2074">#2074</a>
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class AFunctionOfASmallArgumentTest
    {
        [Theory]
        [InlineData("sin(10^(-60))", "1E-60")]
        [InlineData("sin(-10^(-60))", "-1E-60")]
        [InlineData("tan(10^(-60))", "1E-60")]
        [InlineData("arcsin(10^(-60))", "1E-60")]
        [InlineData("arctan(10^(-60))", "1E-60")]
        [InlineData("erf(10^(-60))", "1.128379167095512573896158903121545171688101258658E-60")]
        [InlineData("erfi(10^(-60))", "1.128379167095512573896158903121545171688101258658E-60")]
        [InlineData("Si(10^(-60))", "1E-60")]
        [InlineData("Shi(10^(-60))", "1E-60")]
        public void IsItsValue(string expression, string reference)
        {
            var expected = EDecimal.FromString(reference);
            var actual = Assert.IsType<Real>(expression.EvalNumerical()).EDecimal;
            var error = actual.Subtract(expected, EContext.ForPrecision(200)).Abs().Divide(expected.Abs(), EContext.ForPrecision(10));
            Assert.True(error.LessThan(EDecimal.FromString("1E-40")), $"{expression} is {actual}, where it is {expected}");
        }

        [Theory]
        [InlineData("sin(pi)")]
        [InlineData("sin(100*pi)")]
        [InlineData("cos(pi/2)")]
        [InlineData("tan(pi)")]
        [InlineData("arccos(1)")]
        public void TheResidualAtAZeroIsStillZero(string expression)
            => Assert.Equal(Integer.Zero, expression.EvalNumerical());
    }
}
