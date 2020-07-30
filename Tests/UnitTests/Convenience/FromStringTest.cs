using AngouriMath;
using AngouriMath.Core.Numerix;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Convenience
{
    [TestClass]
    public class FromStringTest
    {
        public static readonly VariableEntity x = MathS.Var("x");
        [TestMethod]
        public void Test1() => Assert.AreEqual(2, MathS.FromString("1 + 1").Eval());
        [TestMethod]
        public void Test2() => Assert.AreEqual(0, MathS.FromString("sin(0)").Eval());
        [TestMethod]
        public void Test3() => Assert.AreEqual(2, MathS.FromString("log(2, 4)").Eval());
        [TestMethod]
        public void Test4() => Assert.AreEqual(MathS.Cos(MathS.Var("x")), MathS.FromString("sin(x)").Derive(MathS.Var("x")).Simplify());
        [TestMethod]
        public void Test5() => Assert.AreEqual(7625597484987L, MathS.FromString("3 ^ 3 ^ 3").Eval());
        [TestMethod]
        public void Test6() => Assert.AreEqual(6, MathS.FromString("2 + 2 * 2").Eval());
        [TestMethod]
        public void Test7() =>
            // Only needed for Mac
            MathS.Settings.PrecisionErrorZeroRange.As(2e-16m, () =>
                Assert.AreEqual(MathS.i, MathS.FromString("x^2+1").SolveNt(MathS.Var("x")).Pieces[0])
            );
        [TestMethod]
        public void Test8() => Assert.AreEqual(1, MathS.FromString("cos(sin(0))").Eval());
        [TestMethod]
        public void Test9() => Assert.AreEqual(ComplexNumber.Create(4, 1), MathS.FromString("2i + 2 * 2 - 1i").Eval());
        [TestMethod]
        public void Test10() => Assert.AreEqual(-1, MathS.FromString("i^2").Eval());
        [TestMethod]
        public void Test11() => Assert.AreEqual(3, MathS.FromString("x^2-1").Substitute(MathS.Var("x"), 2).Eval());
        [TestMethod]
        public void Test12() => Assert.AreEqual((MathS.pi / 2).Eval(), MathS.FromString("arcsin(x)").Substitute(MathS.Var("x"), 1.0).Eval());
        [TestMethod]
        public void Test13()
        {
            Assert.AreEqual(MathS.Sqr(x), MathS.FromString("x2"));
            Assert.AreEqual(2 * x, MathS.FromString("2x"));
        }
        [TestMethod]
        public void Test14() => Assert.AreEqual(9, MathS.FromString("3 2").Eval());
        [TestMethod]
        public void Test15() => Assert.AreEqual(5 * x, MathS.FromString("x(2 + 3)").Simplify());
        [TestMethod]
        public void Test16()
        {
            var y = MathS.Var("y");
            Assert.AreEqual(x * y, MathS.FromString("x y"));
        }
        [TestMethod]
        public void Test17() => Assert.AreEqual(x * MathS.Sqrt(3), MathS.FromString("x sqrt(3)"));
        [TestMethod]
        public void Test18() => Assert.AreEqual(MathS.Factorial(x), MathS.FromString("x!"));
        [TestMethod]
        public void Test19() => Assert.ThrowsException<AngouriMath.Core.FromString.ParseException>(() => MathS.FromString("x!!"));
        [TestMethod]
        public void Test20() => Assert.AreEqual(MathS.Factorial(MathS.Sin(x)), MathS.FromString("sin(x)!"));
        [TestMethod]
        public void Test21() => Assert.AreEqual(MathS.Pow(2, MathS.Factorial(3)), MathS.FromString("2^3!"));
        [TestMethod]
        public void Test22() => Assert.AreEqual(MathS.Pow(MathS.Factorial(2), MathS.Factorial(3)), MathS.FromString("2!^3!"));
        [TestMethod]
        public void Test23() => Assert.AreEqual(MathS.Pow(MathS.Factorial(2), MathS.Factorial(x + 2)), MathS.FromString("2!^(x+2)!"));
        [TestMethod]
        public void Test24() => Assert.AreEqual(-MathS.Factorial(1), MathS.FromString("-1!"));
        [TestMethod]
        public void Test25() => Assert.AreEqual(MathS.Factorial(-1), MathS.FromString("(-1)!"));
        [TestMethod]
        public void TestSys()
        {
            Entity expr = "x2";
            Assert.AreEqual(MathS.Sqr(x), expr);
        }
    }
}
