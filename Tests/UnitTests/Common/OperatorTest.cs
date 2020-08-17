using AngouriMath;
using AngouriMath.Core.Numerix;
using Xunit;

namespace UnitTests.Common
{
    public class OperatorTest
    {
        [Fact] public void TestEq() => Assert.Equal(MathS.Var("x"), MathS.Var("x"));
        [Fact] public void TestIneq() => Assert.NotEqual(MathS.Var("x"), MathS.Var("y"));
        [Fact]
        public void TestR() =>
            Assert.Equal(ComplexNumber.Create(0, 1),
                ComplexNumber.Create(0, PeterO.Numbers.EDecimal.FromInt32(1).NextPlus(MathS.Settings.DecimalPrecisionContext)));
        [Fact] public void TestDP() => Assert.Equal(-23, MathS.FromString("-23").Eval());
        [Fact] public void TestDM() => Assert.Equal(0, MathS.FromString("1 + -1").Eval());
        [Fact] public void TestB() => Assert.Equal(0, MathS.FromString("1 + (-1)").Eval());
        [Fact] public void TestMi() => Assert.Equal(1, MathS.FromString("-i^2").Eval());
        [Fact] public void TestMm() => Assert.Equal(-1, MathS.FromString("-1 * -1 * -1").Eval());
    }
}
