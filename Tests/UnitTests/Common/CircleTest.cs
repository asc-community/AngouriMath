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

        public bool FunctionsAreEqualHack(Entity eq1, Entity eq2)
        {
            var vars1 = MathS.Utils.GetUniqueVariables(eq1);
            var vars2 = MathS.Utils.GetUniqueVariables(eq2);
            vars1.Pieces.Sort((entity, entity1) => ((Entity)entity).Name.CompareTo(((Entity)entity1).Name));
            vars2.Pieces.Sort((entity, entity1) => ((Entity)entity).Name.CompareTo(((Entity)entity1).Name));
            if (vars1.Count != vars2.Count)
                return false;
            for(int i = 0; i < vars1.Count; i++)
                if (vars1.Pieces[i] != vars2.Pieces[i])
                    return false;
            for (int i = 1; i < 10; i++)
            {
                var a = eq1.DeepCopy();
                var b = eq2.DeepCopy();
                foreach (var var in vars1.FiniteSet())
                {
                    a = a.Substitute(var as VariableEntity, i);
                    b = b.Substitute(var as VariableEntity, i);
                }

                if (a.Eval() != b.Eval())
                    return false;
            }

            return true;
        }

        [TestMethod]
        public void TestLinch()
        {
            Entity expr = "x / y + x * x * y";
            Entity exprOptimized = MathS.Utils.OptimizeTree(expr);
            Assert.IsTrue(FunctionsAreEqualHack(expr, exprOptimized), "Expressions " + expr.ToString() + " and " + exprOptimized.ToString() + " are not equal");
        }

        [TestMethod]
        public void TestLinch1()
        {
            Entity expr = "x / 1 + 2";
            Entity exprOptimized = MathS.Utils.OptimizeTree(expr);
            Assert.IsTrue(FunctionsAreEqualHack(expr, exprOptimized), "Expressions " + expr.ToString() + " and " + exprOptimized.ToString() + " are not equal");
        }

        [TestMethod]
        public void TestLinch2()
        {
            Entity expr = "(x + y + x + 1 / (x + 4 + 4 + sin(x))) / (x + x + 3 / y) + 3";
            Entity exprOptimized = MathS.Utils.OptimizeTree(expr);
            Assert.IsTrue(FunctionsAreEqualHack(expr, exprOptimized), "Expressions " + expr.ToString() + " and " + exprOptimized.ToString() + " are not equal");
        }
    }
}