using AngouriMath;
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
            var exprActual = MathS.FromString(expr);
            Assert.AreEqual("233/10 + 3 / 3 + i", exprActual.ToString());
        }

        [TestMethod]
        public void Test4()
        {
            MathS.FromString((MathS.Sin(x) / MathS.Cos(x)).Derive(x).ToString());
        }

        [TestMethod]
        public void Test5()
        {
            Assert.AreEqual("1", MathS.Numbers.Create(1).ToString());
            Assert.AreEqual("-1", MathS.Numbers.Create(-1).ToString());
        }

        [TestMethod]
        public void Test6()
        {
            Assert.AreEqual("i", MathS.i.ToString());
            Assert.AreEqual("-i", (-1 * MathS.i).ToString());
        }

        [TestMethod]
        public void Test7() => Assert.AreEqual(3 * x, MathS.Sin(MathS.Arcsin(x * 3)).Simplify());

        [TestMethod]
        public void Test8() => Assert.AreEqual(3 * x, MathS.Arccotan(MathS.Cotan(x * 3)).Simplify());

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
                foreach (VariableEntity var in vars1.FiniteSet())
                {
                    a = a.Substitute(var, i);
                    b = b.Substitute(var, i);
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