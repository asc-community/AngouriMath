using AngouriMath;
using AngouriMath.Core.Numerix;
using AngouriMath.Core.TreeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace UnitTests.PatternsTest
{
    [TestClass]
    public class SmartExpander
    {
        public (bool equal, ComplexNumber eval1, ComplexNumber eval2, RealNumber err) AreEqual(Entity expr1, Entity expr2, ComplexNumber toSub)
        {
            foreach (var var in expr1.Vars)
                expr1 = expr1.Substitute(var, toSub);
            foreach (var var in expr2.Vars)
                expr2 = expr2.Substitute(var, toSub);
            var evaled1 = expr1.Eval();
            var evaled2 = expr2.Eval();
            return (evaled1 == evaled2, evaled1, evaled2, (evaled1 - evaled2).Abs());
        }
        public void AssertExpander(Entity expr, ComplexNumber[] toSubs, bool nullExpansion = false)
        {
            var expanded = MathS.Settings.MaxExpansionTermCount.As(3000, () =>
            {
                var expandOver = TreeAnalyzer.SmartExpandOver(expr, entity => entity.Vars.Contains("x"));
                if (expandOver is null)
                    throw new AssertFailedException("expandOver is null");
                return TreeAnalyzer.MultiHangBinary(expandOver, (a, b) => new Entity.Sumf(a, b));
            });
            foreach (var toSub in toSubs)
            {
                if (nullExpansion) Assert.AreEqual(expr, expanded); // Nodes should be same
                else Assert.AreNotEqual(expr, expanded); // Nodes should be different
                var (equal, expected, actual, err) = AreEqual(expr, expanded, toSub);
                Assert.IsTrue(equal, $"\nexpected: {expected}\nactual: {actual}\nerror: {err}\ntoSub: {toSub}\nexpanded: {expanded}");
            }
        }
        

        private readonly ComplexNumber[] TestSet1 = { 3, 6, 8, -3, MathS.i * 2 };

        [TestMethod] public void TestCorner1() => AssertExpander("x", TestSet1, true);
        [TestMethod] public void TestCorner2() => AssertExpander("3", TestSet1, true);
        [TestMethod] public void TestCorner3() => AssertExpander("sin(x)", TestSet1, true);
        [TestMethod] public void TestCorner4() => AssertExpander("1 / x", TestSet1, true);

        [TestMethod] public void TestPow1() => AssertExpander("(x + 2)2", TestSet1);
        [TestMethod] public void TestPow2() => AssertExpander("(x + a + b)3", TestSet1);
        [TestMethod] public void TestPow3() => AssertExpander("(x + a + b + x2)5", TestSet1);
        [TestMethod] public void TestPow4() => AssertExpander("(x + a + b + x2 + x3)8", TestSet1);
        [TestMethod] public void TestPow5() => AssertExpander("(x + a + b + x2 + x3)8", TestSet1);
        [TestMethod] public void TestPow6() => AssertExpander("(x + 1)0", TestSet1, true);
        [TestMethod] public void TestPow7() => AssertExpander("(x + 1)^(-2)", TestSet1, true);

        [TestMethod] public void TestMul1() => AssertExpander("(x + 1)(x + b)", TestSet1);
        [TestMethod] public void TestMul2() => AssertExpander("(x + 1)(x + b + c)", TestSet1);
        [TestMethod] public void TestMul3() => AssertExpander("(x + 1)(x + b + x3 + x4)", TestSet1);
        [TestMethod] public void TestMul4() => AssertExpander("(x + 1)(x + b + x3 + x4)(x + a + x2)", TestSet1);
        [TestMethod] public void TestCornerMul() => AssertExpander("(x + sin(a))(sin(x2) + b)", TestSet1);
        [TestMethod] public void Factorial1() => AssertExpander("x!/(x+3)!", TestSet1);
        [TestMethod] public void Factorial2() => AssertExpander("(x+16)!/(x+6)!", TestSet1);

        [TestMethod] public void TestDiv1() => AssertExpander("(x + 1) / (x2 + x3)", TestSet1);
        [TestMethod] public void TestDiv2() => AssertExpander("(x + a + b + x2 + x3)/(x + b + x3 + x4)", TestSet1);
    }
}
