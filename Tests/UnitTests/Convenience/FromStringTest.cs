using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Core.FromString;
using AngouriMath.Core.Numerix;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Convenience
{
    [TestClass]
    public class FromStringTest
    {
        public static readonly Entity.Variable x = MathS.Var(nameof(x));
        public static readonly Entity.Variable y = MathS.Var(nameof(y));
        [TestMethod] public void WrongNumbersOfArgs0() => Assert.ThrowsException<FunctionArgumentCountException>(
            () => (Entity)"limitleft()", "limitleft should have exactly 3 arguments but 0 arguments are provided");
        [TestMethod] public void WrongNumbersOfArgs1() => Assert.ThrowsException<FunctionArgumentCountException>(
            () => (Entity)"derivative(3)", "derivative should have exactly 3 arguments but 1 argument is provided");
        [TestMethod] public void WrongNumbersOfArgs2() => Assert.ThrowsException<FunctionArgumentCountException>(
            () => (Entity)"ln(3, 5)", "ln should have exactly 1 argument but 2 arguments are provided");
        [TestMethod] public void WrongNumbersOfArgs3() => Assert.ThrowsException<FunctionArgumentCountException>(
            () => (Entity)"sin(3, 5, 8)", "sin should have exactly 1 argument but 3 arguments are provided");
        [TestMethod] public void WrongNumbersOfArgsLog0() => Assert.ThrowsException<FunctionArgumentCountException>(
            () => (Entity)"log()", "log should have 1 argument or 2 arguments but 0 arguments are provided");
        [TestMethod] public void WrongNumbersOfArgsLog3() => Assert.ThrowsException<FunctionArgumentCountException>(
            () => (Entity)"log(1, 1, 1)", "log should have 1 argument or 2 arguments but 3 arguments are provided");
        [TestMethod] public void Test1() => Assert.AreEqual(2, MathS.FromString("1 + 1").Eval());
        [TestMethod] public void Test2() => Assert.AreEqual(0, MathS.FromString("sin(0)").Eval());
        [TestMethod] public void Test3() => Assert.AreEqual(2, MathS.FromString("log(2, 4)").Eval());
        [TestMethod] public void Test4() => Assert.AreEqual(2, MathS.FromString("log(100)").Eval());
        [TestMethod] public void Test5() => Assert.AreEqual(MathS.Cos(x), MathS.FromString("sin(x)").Derive(x).Simplify());
        [TestMethod] public void Test6() => Assert.AreEqual(7625597484987L, MathS.FromString("3 ^ 3 ^ 3").Eval());
        [TestMethod] public void Test7() => Assert.AreEqual(6, MathS.FromString("2 + 2 * 2").Eval());
        [TestMethod] public void Test8() =>
            // Only needed for Mac
            MathS.Settings.PrecisionErrorZeroRange.As(2e-16m, () =>
                Assert.AreEqual(MathS.i, MathS.FromString("x^2+1").SolveNt(x).Pieces[0])
            );
        [TestMethod] public void Test9() => Assert.AreEqual(1, MathS.FromString("cos(sin(0))").Eval());
        [TestMethod] public void Test10() => Assert.AreEqual(ComplexNumber.Create(4, 1), MathS.FromString("2i + 2 * 2 - 1i").Eval());
        [TestMethod] public void Test11() => Assert.AreEqual(-1, MathS.FromString("i^2").Eval());
        [TestMethod] public void Test12() => Assert.AreEqual(3, MathS.FromString("x^2-1").Substitute(x, 2).Eval());
        [TestMethod] public void Test13() => Assert.AreEqual(MathS.DecimalConst.pi / 2, MathS.FromString("arcsin(x)").Substitute(x, 1).Eval());
        [TestMethod] public void Test14() => Assert.AreEqual(MathS.Sqr(x), MathS.FromString("x2"));
        [TestMethod] public void Test15() =>  Assert.AreEqual(2 * x, MathS.FromString("2x"));
        [TestMethod] public void Test16() => Assert.AreEqual(9, MathS.FromString("3 2").Eval());
        [TestMethod] public void Test17() => Assert.AreEqual(5 * x, MathS.FromString("x(2 + 3)").Simplify());
        [TestMethod] public void Test18() => Assert.AreEqual(x * y, MathS.FromString("x y"));
        [TestMethod] public void Test19() => Assert.AreEqual(x * MathS.Sqrt(3), MathS.FromString("x sqrt(3)"));
        [TestMethod] public void Test20() => Assert.AreEqual(MathS.Factorial(x), MathS.FromString("x!"));
        [TestMethod] public void Test21() => Assert.ThrowsException<ParseException>(() => MathS.FromString("x!!"));
        [TestMethod] public void Test22() => Assert.AreEqual(MathS.Factorial(MathS.Sin(x)), MathS.FromString("sin(x)!"));
        [TestMethod] public void Test23() => Assert.AreEqual(MathS.Pow(2, MathS.Factorial(3)), MathS.FromString("2^3!"));
        [TestMethod] public void Test24() => Assert.AreEqual(MathS.Pow(MathS.Factorial(2), MathS.Factorial(3)), MathS.FromString("2!^3!"));
        [TestMethod] public void Test25() => Assert.AreEqual(MathS.Pow(MathS.Factorial(2), MathS.Factorial(x + 2)), MathS.FromString("2!^(x+2)!"));
        [TestMethod] public void Test26() => Assert.AreEqual(-MathS.Factorial(1), MathS.FromString("-1!"));
        // TODO: "-" inverts an expression instead of parsing it as a number
        [TestMethod] public void Test27() => Assert.AreEqual(MathS.Factorial(-1).ToString(), "(-1)!");
        [TestMethod] public void TestSys() => Assert.AreEqual(MathS.Sqr(x), "x2");
        [TestMethod] public void Test28() 
            => Assert.AreEqual(MathS.Derivative("x + 1", x), "derivative(x + 1, x, 1)");
        [TestMethod] public void Test29() 
            => Assert.AreEqual(MathS.Derivative("x + 1", x, 5), "derivative(x + 1, x, 5)");
        [TestMethod] public void Test30()
            => Assert.AreEqual(MathS.Integral("x + 1", x), "integral(x + 1, x, 1)");
        [TestMethod] public void Test31()
            => Assert.AreEqual(MathS.Integral("x + y", x, 3), "integral(x + y, x, 3)");
        [TestMethod] public void Test32()
            => Assert.AreEqual(MathS.Limit("x + y", x, 3), "limit(x + y, x, 3)");
        [TestMethod] public void Test33()
            => Assert.AreEqual(MathS.Limit("x + y", x, 3, AngouriMath.Limits.ApproachFrom.Left), "limitleft(x + y, x, 3)");
    }
}
