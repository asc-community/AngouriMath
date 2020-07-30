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
            => Assert.AreEqual(desiredOutput.Simplify(), Limit.ComputeLimit(expr, "x", where, appr)?.Simplify());

        public void TestLimit(Entity expr, Entity where, string desiredOutput)
            => Assert.AreEqual(desiredOutput, Limit.ComputeLimit(expr, "x", where, ApproachFrom.Left)?.Simplify().ToString());

        [TestMethod] public void Test1() => TestLimit("x", RealNumber.PositiveInfinity, ApproachFrom.Left, RealNumber.PositiveInfinity);
        [TestMethod] public void Test2() => TestLimit("-x", RealNumber.PositiveInfinity, ApproachFrom.Left, RealNumber.NegativeInfinity);
        [TestMethod] public void Test3() => TestLimit("x / (2x + 1)", RealNumber.PositiveInfinity, ApproachFrom.Left, "1/2");
        [TestMethod] public void Test4() => TestLimit("x / (a * x + 1)", RealNumber.PositiveInfinity, ApproachFrom.Left, "1/a");
        [TestMethod] public void Test5() => TestLimit("(x2 + x) / x", RealNumber.PositiveInfinity, ApproachFrom.Left, "+");
        [TestMethod] public void Test6() => TestLimit("(x2 + x) / (a * x)", RealNumber.PositiveInfinity, ApproachFrom.Left, RealNumber.PositiveInfinity / (Entity)"a");
        [TestMethod] public void Test7() => TestLimit("(x + x) / (a * x2)", RealNumber.PositiveInfinity, ApproachFrom.Left, "0");
        [TestMethod] public void Test8() => TestLimit("(x^3 + a*x^2)/(b*x^2 + 1)", "a", "2 a3 / (1 + a2 * b)");
        [TestMethod] public void Test9() => TestLimit("x / a", "1", ApproachFrom.Left, "1 / a");
    }
}
