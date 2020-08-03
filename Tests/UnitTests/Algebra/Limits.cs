using AngouriMath;
using AngouriMath.Core.Numerix;
using AngouriMath.Limits;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Algebra
{
    [TestClass]
    internal class LimitFunctional
    {
        public void TestLimit(Entity expr, Entity where, ApproachFrom appr, Entity desiredOutput)
            => Assert.AreEqual(desiredOutput.Simplify(), MathS.Limits.Compute(expr, "x", where, appr)?.Simplify());

        [TestMethod] public void Test1() => TestLimit("x", RealNumber.PositiveInfinity, ApproachFrom.Left, RealNumber.PositiveInfinity);
        [TestMethod] public void Test2() => TestLimit("-x", RealNumber.PositiveInfinity, ApproachFrom.Left, RealNumber.NegativeInfinity);
        [TestMethod] public void Test3() => TestLimit("x / (2x + 1)", RealNumber.PositiveInfinity, ApproachFrom.Left, "1/2");
        [TestMethod] public void Test4() => TestLimit("x / (a * x + 1)", RealNumber.PositiveInfinity, ApproachFrom.Left, "1/a");
        [TestMethod] public void Test5() => TestLimit("(x2 + x) / x", RealNumber.PositiveInfinity, ApproachFrom.Left, RealNumber.PositiveInfinity);
        [TestMethod] public void Test6() => TestLimit("(x2 + x) / (a * x)", RealNumber.PositiveInfinity, ApproachFrom.Left, RealNumber.PositiveInfinity / (Entity)"a");
        [TestMethod] public void Test7() => TestLimit("(x + x) / (a * x2)", RealNumber.PositiveInfinity, ApproachFrom.Left, "0");
        [TestMethod] public void Test8() => TestLimit("(x^3 + a*x^2)/(b*x^2 + 1)", "a", ApproachFrom.Left,"2 a3 / (1 + a2 * b)");
        [TestMethod] public void Test9() => TestLimit("x / a", "1", ApproachFrom.Left, "1 / a");
        [TestMethod] public void Test10()
        {
            Assert.Inconclusive("Under work");
            TestLimit("sin(x) / x", "0", ApproachFrom.Left, "1");
        }
        [TestMethod] public void Test11()
        {
            Assert.Inconclusive("Under work");
            TestLimit("a * sin(x) / x", "0", ApproachFrom.Left, "1");
        }

        [TestMethod] public void Test12() => TestLimit("a x", RealNumber.PositiveInfinity, ApproachFrom.Left, RealNumber.PositiveInfinity * (Entity)"a");
        [TestMethod] public void Test13() => TestLimit("a x2 + b / x", RealNumber.PositiveInfinity, ApproachFrom.Left, RealNumber.PositiveInfinity * (Entity)"a");
        [TestMethod] public void Test14() => TestLimit("a x ^ (-2) + b / x", RealNumber.PositiveInfinity, ApproachFrom.Left, "0");
        [TestMethod] public void Test15() => TestLimit("1 / (x^2 + x)", "-1", ApproachFrom.Left, RealNumber.PositiveInfinity);
        [TestMethod] public void Test16() => TestLimit("1 / x", "0", ApproachFrom.Left, RealNumber.NegativeInfinity);
        [TestMethod] public void Test17() => TestLimit("1 / x", "0", ApproachFrom.Right, RealNumber.PositiveInfinity);
        [TestMethod] public void Test18() => TestLimit("ln(x)", "0", ApproachFrom.Right, RealNumber.NegativeInfinity);
        [TestMethod] public void Test19() => TestLimit("ln(x)", "0", ApproachFrom.Left, RealNumber.NegativeInfinity);
        [TestMethod] public void Test20() => TestLimit("ln(x)", RealNumber.PositiveInfinity, ApproachFrom.Left, RealNumber.PositiveInfinity);
        [TestMethod] public void Test21() => TestLimit("ln(x)", RealNumber.NegativeInfinity, ApproachFrom.Right, RealNumber.PositiveInfinity);
        [TestMethod] public void Test22() => TestLimit("ln(1 / (x - a))", "a", ApproachFrom.Right, RealNumber.PositiveInfinity);
        [TestMethod] public void Test23() => TestLimit("log(b, x)", 0, ApproachFrom.Right, RealNumber.NegativeInfinity);
        [TestMethod] public void Test24() => TestLimit("ln(x^4 - x^2)", RealNumber.PositiveInfinity, ApproachFrom.Left, RealNumber.PositiveInfinity);
        [TestMethod] public void Test25() => TestLimit("ln((x - 1) / (2x + 1))", RealNumber.PositiveInfinity, ApproachFrom.Right, MathS.Ln(0.5));
        [TestMethod] public void Test26() => TestLimit("log(x, x^2)", 0, ApproachFrom.Right, 2);
        [TestMethod] public void Test27() => TestLimit("log(1/x, x^2)", 0, ApproachFrom.Right, -2);
        [TestMethod] public void Test28() => TestLimit("log(x, x^(-2))", 0, ApproachFrom.Right, -2);
        [TestMethod] public void Test29() => TestLimit("log(1/x, x^(-2))", 0, ApproachFrom.Right, 2);
        [TestMethod] public void Test30() => TestLimit("log(x, x)", 0, ApproachFrom.Right, 1);
        [TestMethod] public void Test31() => TestLimit("ln(ln((e^2*x + t) / (x + 1)))", RealNumber.PositiveInfinity, ApproachFrom.Left, MathS.Ln(2));
        [TestMethod] public void Test32() => TestLimit("log((2x - 1)/(x + 1), (x - 1)/(2x - 1))", RealNumber.PositiveInfinity, ApproachFrom.Left, -1);

        [TestMethod]
        public void TestComplicated()
        {
            Entity subExpr = "(a * x2 + b x) / (c x2 - 3)";
            Entity expr = MathS.Sqrt(subExpr * 3 / MathS.Sin(subExpr) + MathS.Sin("d"));
            VariableEntity x = "x";
            Entity dest = RealNumber.PositiveInfinity;
            var limit = MathS.Limits.Compute(expr, x, dest, ApproachFrom.Left);
            Assert.IsNotNull(limit);
            Assert.AreEqual("sqrt(a / c * 3 / sin(a / c) + sin(d))", limit?.ToString());
        }
    }
}
