//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using static AngouriMath.Entity.Number;
using Xunit;

namespace AngouriMath.Tests.Common
{
    public sealed class OperatorTest
    {
        [Fact] public void TestEq() => Assert.Equal(MathS.Var("x"), MathS.Var("x"));
        [Fact] public void TestIneq() => Assert.NotEqual(MathS.Var("x"), MathS.Var("y"));
        [Fact]
        public void TestR() =>
            Assert.Equal(Complex.Create(0, 1),
                Complex.Create(0, PeterO.Numbers.EDecimal.FromInt32(1).NextPlus(MathS.Settings.DecimalPrecisionContext)));
        [Fact] public void TestDP() => Assert.Equal(-23, MathS.FromString("-23").EvalNumerical());
        [Fact] public void TestDM() => Assert.Equal(0, MathS.FromString("1 + -1").EvalNumerical());
        [Fact] public void TestB() => Assert.Equal(0, MathS.FromString("1 + (-1)").EvalNumerical());
        [Fact] public void TestMi() => Assert.Equal(1, MathS.FromString("-i^2").EvalNumerical());
        [Fact] public void TestMm() => Assert.Equal(-1, MathS.FromString("-1 * -1 * -1").EvalNumerical());
    }
}
