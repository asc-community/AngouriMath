using AngouriMath;
using AngouriMath.Core.Numerix;
using AngouriMath.Core.TreeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Algebra
{
    [TestClass]
    public class SmartExpander
    {
        public (bool equal, RealNumber err) AreEqual(Entity expr1, Entity expr2, ComplexNumber toSub)
        {
            foreach (var var in MathS.Utils.GetUniqueVariables(expr1).FiniteSet())
                expr1 = expr1.Substitute(var.Name, toSub);
            foreach (var var in MathS.Utils.GetUniqueVariables(expr2).FiniteSet())
                expr2 = expr2.Substitute(var.Name, toSub);
            var evaled1 = expr1.Eval();
            var evaled2 = expr2.Eval();
            return (evaled1 == evaled2, (evaled1 - evaled2).Abs());
        }
        public void AssertExpander(Entity expr, params ComplexNumber[] toSubs)
        {
            MathS.Settings.MaxExpansionTermCount.Set(3000);
            var expandOver = TreeAnalyzer.SmartExpandOver(expr, entity => entity.FindSubtree("x") is { });
            if (expandOver is null)
                throw new AssertFailedException("expandOver is null");
            var expanded =
                TreeAnalyzer.MultiHangBinary(expandOver, "sumf", Const.PRIOR_SUM);
            MathS.Settings.MaxExpansionTermCount.Unset();
            foreach (var toSub in toSubs)
            {
                var (equal, err) = AreEqual(expr, expanded, toSub);
                Assert.IsTrue(equal, "E: " + err + "  toSub: " + toSub + "  expanded: " + expanded.ToString());
            }
        }
        

        private readonly ComplexNumber[] TestSet1 = { 3, 6, 8, -3, MathS.i * 2 };

        [TestMethod]
        public void TestCorner1() => AssertExpander("x", TestSet1);
        [TestMethod]
        public void TestCorner2() => AssertExpander("3", TestSet1);
        [TestMethod]
        public void TestCorner3() => AssertExpander("sin(x)", TestSet1);
        [TestMethod]
        public void TestCorner4() => AssertExpander("1 / x", TestSet1);

        [TestMethod]
        public void TestPow1() => AssertExpander("(x + 2)2", TestSet1);
        [TestMethod]
        public void TestPow2() => AssertExpander("(x + a + b)3", TestSet1);
        [TestMethod]
        public void TestPow3() => AssertExpander("(x + a + b + x2)5", TestSet1);
        [TestMethod]
        public void TestPow4() => AssertExpander("(x + a + b + x2 + x3)8", TestSet1);
        [TestMethod]
        public void TestPow5() => AssertExpander("(x + a + b + x2 + x3)8", TestSet1);
        [TestMethod]
        public void TestPow6() => AssertExpander("(x + 1)0", TestSet1);
        [TestMethod]
        public void TestPow7() => AssertExpander("(x + 1)^(-2)", TestSet1);

        [TestMethod]
        public void TestMul1() => AssertExpander("(x + 1)(x + b)", TestSet1);
        [TestMethod]
        public void TestMul2() => AssertExpander("(x + 1)(x + b + c)", TestSet1);
        [TestMethod]
        public void TestMul3() => AssertExpander("(x + 1)(x + b + x3 + x4)", TestSet1);
        [TestMethod]
        public void TestMul4() => AssertExpander("(x + 1)(x + b + x3 + x4)(x + a + x2)", TestSet1);
        [TestMethod]
        public void TestCornerMul() => AssertExpander("(x + sin(a))(sin(x2) + b)", TestSet1);

        [TestMethod]
        public void TestDiv1() => AssertExpander("(x + 1) / (x2 + x3)", TestSet1);
        [TestMethod]
        public void TestDiv2() => AssertExpander("(x + a + b + x2 + x3)/(x + b + x3 + x4)", TestSet1);
    }
}
