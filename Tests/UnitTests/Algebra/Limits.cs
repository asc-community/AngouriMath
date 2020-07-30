using AngouriMath;
using AngouriMath.Core.Numerix;
using AngouriMath.Experimental.Limits;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Algebra
{
    [TestClass]
    public class Limits
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
    }
}
