using AngouriMath;
using AngouriMath.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTests
{
    [TestClass]
    public class SolveTest
    {
        public static VariableEntity x = "x";

        /// <summary>
        /// Numerically checks if a root fits an equation
        /// </summary>
        /// <param name="equation"></param>
        /// <param name="toSub"></param>
        /// <param name="varValue"></param>
        public void AssertRoots(Entity equation, VariableEntity toSub, Entity varValue)
        {
            string LimitString(string s)
            {
                if (s.Length < 30)
                    return s;
                else
                    return s.Substring(0, 10) + "..." + s.Substring(s.Length - 10, 10);
            }
            string eqNormal = equation.ToString();
            var err = CheckRoots(equation, toSub, varValue);
            Assert.IsTrue(err < 0.001, "Error is : " + err + "  " + LimitString(eqNormal) + "  wrong root is " + toSub.Name + " = " + LimitString(varValue.ToString()));
        }

        public double CheckRoots(Entity equation, VariableEntity toSub, Entity varValue)
        {
            equation = equation.Substitute(toSub, varValue);
            var allVars = MathS.GetUniqueVariables(equation);
            
            foreach (var vr in allVars)
                equation = equation.Substitute(vr.Name, 3 + MathS.i
                    /* doesn't matter what to sub*/);

            return Number.Abs(equation.Eval());
        }

        [TestMethod]
        public void Test1()
        {
            var eq = (x - 1) * (x - 2);
            var roots = eq.SolveNt(x);
            Assert.IsTrue(roots.Count == 2);
            var s = roots[0] + roots[1];
            Assert.IsTrue(s == 3);
        }
        [TestMethod]
        public void Test2()
        {
            var eq = MathS.Sqr(x) + 1;
            var roots = eq.SolveNt(x);
            Assert.IsTrue(roots.Count == 2);
            Assert.IsTrue(roots[0] == MathS.i);
            Assert.IsTrue(roots[1] == new Number(0, -1));
        }
        [TestMethod]
        public void Test3()
        {
            var eq = new NumberEntity(1);
            var roots = eq.SolveNt(x);
            Assert.IsTrue(roots.Count == 0);
        }
        [TestMethod]
        public void Test4()
        {
            var eq = x.Pow(2) + 2 * x + 1;
            MathS.EQUALITY_THRESHOLD = 1.0e-6;
            var roots = eq.SolveNt(x, precision: 100);
            MathS.EQUALITY_THRESHOLD = 1.0e-11;
            Assert.IsTrue(roots.Count == 1);
        }

        [TestMethod]
        public void Test5()
        {
            // solve x2 + 2x + 2
            var eq = x.Pow(2) + 2 * x + 2;
            var roots = eq.Solve("x");
            var r1 = MathS.FromString("-1 + 1i").Simplify();
            var r2 = MathS.FromString("-1 - 1i").Simplify();

            Assert.IsTrue(roots.Count == 2 &&
                ((roots[0] == r1 && roots[1] == r2) || (roots[0] == r2 && roots[1] == r1)),
            string.Format("roots: {0}, expected: [-1 - 1i, -1 + 1i]", roots));
        }

        [TestMethod]
        public void Test6()
        {
            // solve 2x2 + 4x + 2
            var eq = 2 * x.Pow(2) + 4 * x + 2;
            var roots = eq.Solve("x");
            Assert.IsTrue(roots.Count == 1 && roots[0] == -1, string.Format("roots: {0}, expected: [-1]", roots));
        }

        [TestMethod]
        public void Test7()
        {
            // solve x2 - 3x + 2
            var eq = x.Pow(2) - 3 * x + 2;
            var roots = eq.Solve("x");

            Assert.IsTrue(roots.Count == 2 &&
                ((roots[0] == 1 && roots[1] == 2) || (roots[0] == 2 && roots[1] == 1)),
                 string.Format("roots: {0}, expected: [1, 2]", roots));
        }

        [TestMethod]
        public void Test8()
        {
            // solve x3 + 3x2 + 3x + 1
            var eq = x.Pow(3) + 3 * x.Pow(2) + 3 * x + 1;
            var roots = eq.Solve("x");
            Assert.IsTrue(roots.Count == 1 && roots[0] == -1, string.Format("roots: {0}, expected: [-1]", roots));
        }

        [TestMethod]
        public void Test9()
        {
            // solve x3 - 6x2 + 11x - 6
            var eq = x.Pow(3) - 6 * x.Pow(2) + 11 * x - 6;
            var roots = eq.Solve("x");
            AssertRoots(eq, x, roots[0]);
            AssertRoots(eq, x, roots[1]);
            AssertRoots(eq, x, roots[2]);
        }
        [TestMethod]
        public void TestAllNumbers3()
        {
            var rand = new Random(24 /* seed should be specified due to required determinism*/ );
            for (int i = 0; i < 30; i++)
            {
                var expr = (x - rand.Next(0, 10)) *
                           (x - rand.Next(0, 10)) *
                           (x - rand.Next(0, 10));
                var newexpr = expr.Expand();
                foreach (var root in newexpr.Solve(x))
                    AssertRoots(newexpr, x, root);
            }
        }
        [TestMethod]
        public void TestAllNumbers3complex()
        {
            var rand = new Random(24 /* seed should be specified due to required determinism*/ );
            for (int i = 0; i < 30; i++)
            {
                var expr = (x - (rand.Next(0, 10) + MathS.i * rand.Next(0, 10))) *
                           (x - (rand.Next(0, 10) + MathS.i * rand.Next(0, 10))) *
                           (x - (rand.Next(0, 10) + MathS.i * rand.Next(0, 10)));
                var newexpr = expr.Expand();
                foreach (var root in newexpr.Solve(x))
                    AssertRoots(newexpr, x, root);
            }
        }
        [TestMethod]
        public void TestAllNumbers3complexCount()
        {
            var rand = new Random(24 /* seed should be specified due to required determinism*/ );
            int WA = 0;
            for (int i = 0; i < 30; i++)
            {
                var expr = (x - (rand.Next(0, 10) + MathS.i * rand.Next(0, 10))) *
                           (x - (rand.Next(0, 10) + MathS.i * rand.Next(0, 10))) *
                           (x - (rand.Next(0, 10) + MathS.i * rand.Next(0, 10)));
                var newexpr = expr.Expand();
                foreach (var root in newexpr.Solve(x))
                    WA += CheckRoots(newexpr, x, root) > 0.001 ? 1 : 0;
            }
            Assert.IsTrue(WA == 0, "WA count: " + WA);
        }
        [TestMethod]
        public void TestAllNumbers4()
        {
            var rand = new Random(24 /* seed should be specified due to required determinism*/ );
            for (int i = 0; i < 30; i++)
            {
                var expr = (x - rand.Next(0, 10)) *
                           (x - rand.Next(0, 10)) *
                           (x - rand.Next(0, 10)) *
                           (x - rand.Next(0, 10));
                var newexpr = expr.Expand();
                var roots = newexpr.Solve(x);
                Assert.IsTrue(roots.Count > 0);
                foreach (var root in roots)
                    AssertRoots(newexpr, x, root);
            }
        }
        [TestMethod]
        public void TestAllNumbers4complex()
        {
            var rand = new Random(24 /* seed should be specified due to required determinism*/ );
            for (int i = 0; i < 30; i++)
            {
                var expr = (x - (rand.Next(0, 10) + MathS.i * rand.Next(0, 10))) *
                           (x - (rand.Next(0, 10) + MathS.i * rand.Next(0, 10))) *
                           (x - (rand.Next(0, 10) + MathS.i * rand.Next(0, 10))) *
                           (x - (rand.Next(0, 10) + MathS.i * rand.Next(0, 10)));
                var newexpr = expr.Expand();
                foreach (var root in newexpr.Solve(x))
                    AssertRoots(newexpr, x, root);
            }
        }
        [TestMethod]
        public void TestVars2()
        {
            var goose = MathS.Var("goose");
            var eq = ((x - goose) * (x - 3)).Expand();
            var roots = eq.Solve(x);
            AssertRoots(eq, x, roots[0]);
            AssertRoots(eq, x, roots[1]);
        }
        [TestMethod]
        public void TestVars4mp()
        {
            var goose = MathS.Var("goose");
            var eq = ((x - goose) * (x - 3) * (MathS.Sqr(x) - 4));
            var roots = eq.Solve(x);
            AssertRoots(eq, x, roots[0]);
            AssertRoots(eq, x, roots[1]);
            AssertRoots(eq, x, roots[2]);
            AssertRoots(eq, x, roots[3]);
        }
        [TestMethod]
        public void TestVars3()
        {
            var goose = MathS.Var("goose");
            var momo = MathS.Var("momo");
            var eq = ((x - goose) * (x + goose * momo) * (x - momo * 2)).Expand();
            var roots = eq.Solve(x);
            AssertRoots(eq, x, roots[0]);
            AssertRoots(eq, x, roots[1]);
            AssertRoots(eq, x, roots[2]);
        }
        [TestMethod]
        public void TestVars4()
        {
            var goose = MathS.Var("goose");
            var momo = MathS.Var("momo");
            var quack = MathS.Var("quack");
            var eq = ((x - goose) * (x - momo) * (x - quack) * (x - momo * goose * quack)).Expand();
            var roots = eq.Solve(x);
            Assert.IsNotNull(roots, "roots is null");
            Assert.IsTrue(roots.Count == 4, "count of roots is less (" + roots.Count + ") than should be");
            AssertRoots(eq, x, roots[0]);
            AssertRoots(eq, x, roots[1]);
            AssertRoots(eq, x, roots[2]);
            AssertRoots(eq, x, roots[3]);
        }
        [TestMethod]
        public void TestVars2_0()
        {
            var goose = MathS.Var("goose");
            var momo = MathS.Var("momo");
            var eq = ((x - momo) * (x - goose)).Expand();
            var roots = eq.Solve(x);
            AssertRoots(eq, x, roots[0]);
            AssertRoots(eq, x, roots[1]);
        }
    }
}
