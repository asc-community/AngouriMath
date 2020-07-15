using AngouriMath;
using AngouriMath.Core.Numerix;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Common
{
    [TestClass]
    public class OperatorTest
    {
        [TestMethod] public void TestEq() => Assert.AreEqual(MathS.Var("x"), MathS.Var("x"));
        [TestMethod] public void TestIneq() => Assert.AreNotEqual(MathS.Var("x"), MathS.Var("y"));
        [TestMethod]
        public void TestR() =>
            Assert.AreEqual(ComplexNumber.Create(0, 1),
                ComplexNumber.Create(0, PeterO.Numbers.EDecimal.FromInt32(1).NextPlus(MathS.Settings.DecimalPrecisionContext)));
        [TestMethod] public void TestDP() => Assert.AreEqual(-23, MathS.FromString("-23").Eval());
        [TestMethod] public void TestDM() => Assert.AreEqual(0, MathS.FromString("1 + -1").Eval());
        [TestMethod] public void TestB() => Assert.AreEqual(0, MathS.FromString("1 + (-1)").Eval());
        [TestMethod] public void TestMi() => Assert.AreEqual(1, MathS.FromString("-i^2").Eval());
        [TestMethod] public void TestMm() => Assert.AreEqual(-1, MathS.FromString("-1 * -1 * -1").Eval());
    }
}
