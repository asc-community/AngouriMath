using AngouriMath;
using AngouriMath.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using AngouriMath.Functions.Algebra.Solver.Analytical;

namespace UnitTests.Algebra
{
    [TestClass]
    public class SolveOneEquation
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
            Assert.IsTrue(roots.Count == 2);
            AssertRoots(eq, x, roots[0]);
            AssertRoots(eq, x, roots[1]);
        }

        [TestMethod]
        public void Test6()
        {
            // solve 2x2 + 4x + 2
            var eq = 2 * x.Pow(2) + 4 * x + 2;
            var roots = eq.Solve("x");
            Assert.IsTrue(roots.Count == 1);
            AssertRoots(eq, x, roots[0]);
        }

        [TestMethod]
        public void Test7()
        {
            // solve x2 - 3x + 2
            var eq = x.Pow(2) - 3 * x + 2;
            var roots = eq.Solve("x");
            Assert.IsTrue(roots.Count == 2);
            AssertRoots(eq, x, roots[0]);
            AssertRoots(eq, x, roots[1]);
        }

        [TestMethod]
        public void Test8()
        {
            // solve x3 + 3x2 + 3x + 1
            var eq = x.Pow(3) + 3 * x.Pow(2) + 3 * x + 1;
            var roots = eq.Solve("x");
            Assert.IsTrue(roots.Count == 1);
            AssertRoots(eq, x, roots[0]);
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
        [TestMethod]
        public void TestReduce()
        {
            Entity expr = "3x5 + 5x3";
            var roots = expr.Solve("x");
            Assert.IsTrue(roots.Count == 3);
            AssertRoots(expr, x, roots[0]);
            AssertRoots(expr, x, roots[1]);
            AssertRoots(expr, x, roots[2]);
        }

        public void InvertedFunctionTests(string func, int rootAmount)
        {
            Entity toRepl = func + "(x2 + 3)";
            Entity expr = MathS.Sqr(toRepl) + 0.3 * toRepl - 0.1 * MathS.Var("a");
            var roots = expr.Solve(x);
            Assert.IsTrue(roots.Count == rootAmount);
            foreach (var root in roots)
                AssertRoots(expr.Substitute("a", 5), x, root.Substitute("n", 3).Substitute("a", 5));
        }

        [TestMethod]
        public void TestRepl1()
            => InvertedFunctionTests("sin", 8);
        [TestMethod]
        public void TestRepl2()
            => InvertedFunctionTests("cos", 8);
        [TestMethod]
        public void TestRepl3()
            => InvertedFunctionTests("tan", 4);
        [TestMethod]
        public void TestRepl4()
            => InvertedFunctionTests("cotan", 4);
        [TestMethod]
        public void TestRepl5()
            => InvertedFunctionTests("arcsin", 4);

        // TODO: arccos return analytically correct answer, but incorrect when substituting variables (as there are some Y such there is no X that arccos(X) = Y)
        //[TestMethod]
        //public void TestRepl6()
        //    => InvertedFunctionTests("arccos", 4);
        [TestMethod]
        public void TestRepl7()
            => InvertedFunctionTests("arctan", 4);
        [TestMethod]
        public void TestRepl8()
            => InvertedFunctionTests("arccotan", 4);


        private readonly List<Number> KeyPoints = new List<Number>
        {
            new Number(0, 1),
            new Number(1, 0),
            new Number(-3, -3),
            new Number(2, 2),
            new Number(13, 13),
            new Number(-9, +7),
            new Number(0.5, -0.5),
            new Number(-0.5, 0.5),
        };

        [TestMethod]
        public void TestInvertSin()
        {
            foreach (var kp in KeyPoints)
            {
                var roots = Trigonometry.FindInvertSin(kp);
                Assert.IsTrue(roots[0].Sin().Eval() == kp, kp.ToString());
                Assert.IsTrue(roots[1].Sin().Eval() == kp, kp.ToString());
            }
        }

        [TestMethod]
        public void TestLogs()
        {
            Entity eqs = "log(32, x) - 5";
            var roots = eqs.SolveEquation("x");
            Assert.IsTrue(roots.Count == 1);
            AssertRoots(eqs, "x", roots[0]);
        }

        [TestMethod]
        public void TestFerrari1()
        {
            Entity eq = "x4 - x2 + 1";
            var roots = eq.SolveEquation("x");
            Assert.IsTrue(roots.Count == 4);
            foreach (var root in roots)
                AssertRoots(eq, "x", root);
        }

        [TestMethod]
        public void TestFerrari2()
        {
            Entity eq = "x4 - x + 1";
            var roots = eq.SolveEquation("x");
            Assert.IsTrue(roots.Count == 4);
            foreach (var root in roots)
                AssertRoots(eq, "x", root);
        }
        [TestMethod]
        public void TestFerrari3()
        {
            Entity eq = "x4 - x3 + 1";
            var roots = eq.SolveEquation("x");
            Assert.IsTrue(roots.Count == 4);
            foreach (var root in roots)
                AssertRoots(eq, "x", root);
        }
        [TestMethod]
        public void TestFerrari4()
        {
            Entity eq = "x4 - x2 + x - x3 + 1";
            var roots = eq.SolveEquation("x");
            Assert.IsTrue(roots.Count == 4);
            foreach (var root in roots)
                AssertRoots(eq, "x", root);
        }
        [TestMethod]
        public void TestFerrari5()
        {
            Entity eq = "(x2 - 1)2 - 2";
            var roots = eq.SolveEquation("x");
            Assert.IsTrue(roots.Count == 4);
            foreach (var root in roots)
                AssertRoots(eq, "x", root);
        }

        [TestMethod]
        public void TestFerrari6()
        {
            Entity eq = "x4 - 2x2 - 1";
            var roots = eq.SolveEquation("x");
            Assert.IsTrue(roots.Count == 4);
            foreach (var root in roots)
                AssertRoots(eq, "x", root);
        }

        [TestMethod]
        public void TestFerrari7()
        {
            Entity eq = "x4 - 2x2 - 2";
            var roots = eq.SolveEquation("x");
            Assert.IsTrue(roots.Count == 4);
            foreach (var root in roots)
                AssertRoots(eq, "x", root);
        }

        [TestMethod]
        public void Test1px()
        {
            Entity expr = "pi - 1^x";
            expr.SolveEquation("x"); // Check if it doesn't hang
        }

        [TestMethod]
        public void ExpSimpl()
        {
            Entity expr = "x^4 * x^y - 2";
            var roots = expr.SolveEquation("x");
            Assert.IsTrue(roots.Count == 1);
            AssertRoots(expr, "x", roots[0]);
        }
    }
}

