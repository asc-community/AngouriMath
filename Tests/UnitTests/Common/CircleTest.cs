using AngouriMath;
using AngouriMath.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Common
{
    [TestClass]
    public class CircleTest
    {
        public static readonly VariableEntity x = MathS.Var("x");

        [TestMethod]
        public void Test1()
        {
            const string expr = "1 + 2 * log(2, 3)";
            Assert.AreEqual(expr, MathS.FromString(expr).ToString());
        }

        [TestMethod]
        public void Test2()
        {
            const string expr = "2 ^ 3 + sin(3)";
            Assert.AreEqual(expr, MathS.FromString(expr).ToString());
        }

        [TestMethod]
        public void Test3()
        {
            const string expr = "23.3 + 3 / 3 + i";
            Assert.AreEqual(expr, MathS.FromString(expr).ToString());
        }

        [TestMethod]
        public void Test4()
        {
            MathS.FromString((MathS.Sin(x) / MathS.Cos(x)).Derive(x).ToString());
        }

        [TestMethod]
        public void Test5()
        {
            Assert.IsTrue(MathS.Num(1).ToString() == "1");
            Assert.IsTrue(MathS.Num(-1).ToString() == "-1");
        }

        [TestMethod]
        public void Test6()
        {
            Assert.IsTrue(MathS.i.ToString() == "i");
            Assert.IsTrue((-1 * MathS.i).ToString() == "-i");
        }

        [TestMethod]
        public void Test7()
        {
            Assert.IsTrue(MathS.Sin(MathS.Arcsin(x * 3)).Simplify() == 3 * x);
        }

        [TestMethod]
        public void Test8()
        {
            Assert.IsTrue(MathS.Arccotan(MathS.Cotan(x * 3)).Simplify() == 3 * x);
        }

        // TODO: the test
        [TestMethod]
        public void TestSys()
        {
            /*
            Entity expr = "sqrt(x) * sin(y) + a ^ (-1) * x * 5 - x ^ 3 * sin(y) ^ 0.2 - 2 + a ^ 4";
            var pi = MathS.GatherAllPossiblePolynomials(expr);
            Console.WriteLine("Replacements:");
            foreach (var r in pi.replacementInfo)
                Console.WriteLine(r.Key + ": " + r.Value.ToString());
            Console.WriteLine();
            Console.WriteLine("Mono info:");
            foreach (var r in pi.monoInfo)
            {
                Console.WriteLine("  " + r.Key.ToString() + ":");
                foreach (var p in r.Value)
                    Console.WriteLine("    " + p.Key.ToString() + " => " + p.Value.ToString());
            }
            */
        }
    }
}