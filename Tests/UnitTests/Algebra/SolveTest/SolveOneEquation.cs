using AngouriMath;
using AngouriMath.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using AngouriMath.Core.Numerix;

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
        public static void AssertRoots(Entity equation, VariableEntity toSub, Entity varValue)
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
            Assert.IsTrue(err < 0.001m, "Error is : " + err + "  " + LimitString(eqNormal) + "  wrong root is " + toSub.Name + " = " + LimitString(varValue.ToString()));
        }

        public static decimal CheckRoots(Entity equation, VariableEntity toSub, Entity varValue)
        {
            equation = equation.Substitute(toSub, varValue);
            var allVars = MathS.Utils.GetUniqueVariables(equation);
            
            foreach (var vr in allVars.FiniteSet())
                equation = equation.Substitute(vr.Name, 3
                    /* MUST be integer to correspond to integer coefficient of periodic roots*/);

            return Number.Abs(equation.Eval());
        }

        public static void AssertRootCount(Set roots, int target)
        {
            Assert.IsFalse(roots.Power == Set.PowerLevel.INFINITE, "Set of roots must be finite");
            Assert.IsTrue(roots.Count == target, string.Format("Number of roots must be equal {0} but is {1}", target, roots.Count));
        }

        [TestMethod]
        public void Test1()
        {
            var eq = (x - 1) * (x - 2);
            var roots = eq.SolveNt(x);
            AssertRootCount(roots, 2);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }
        [TestMethod]
        public void Test2()
        {
            var eq = MathS.Sqr(x) + 1;
            var roots = eq.SolveNt(x);
            AssertRootCount(roots, 2);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }
        [TestMethod]
        public void Test3()
        {
            var eq = new NumberEntity(1);
            var roots = eq.SolveNt(x);
            AssertRootCount(roots, 0);
        }
        [TestMethod]
        public void Test4()
        {
            var eq = x.Pow(2) + 2 * x + 1;
            MathS.Settings.PrecisionErrorCommon.Set(1e-6m);
            var roots = eq.SolveNt(x, precision: 100);
            MathS.Settings.PrecisionErrorCommon.Unset();
            AssertRootCount(roots, 1);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }

        [TestMethod]
        public void Test5()
        {
            // solve x2 + 2x + 2
            var eq = x.Pow(2) + 2 * x + 2;
            var roots = eq.SolveEquation("x");
            var r1 = MathS.FromString("-1 + 1i").Simplify();
            var r2 = MathS.FromString("-1 - 1i").Simplify();
            AssertRootCount(roots, 2);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
            var s = new Set();
        }

        [TestMethod]
        public void Test6()
        {
            // solve 2x2 + 4x + 2
            var eq = 2 * x.Pow(2) + 4 * x + 2;
            var roots = eq.SolveEquation("x");
            AssertRootCount(roots, 1);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }

        [TestMethod]
        public void Test7()
        {
            // solve x2 - 3x + 2
            var eq = x.Pow(2) - 3 * x + 2;
            var roots = eq.SolveEquation("x");
            AssertRootCount(roots, 2);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }

        [TestMethod]
        public void Test8()
        {
            // solve x3 + 3x2 + 3x + 1
            var eq = x.Pow(3) + 3 * x.Pow(2) + 3 * x + 1;
            var roots = eq.SolveEquation("x");
            AssertRootCount(roots, 1);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }

        [TestMethod]
        public void Test9()
        {
            // solve x3 - 6x2 + 11x - 6
            var eq = x.Pow(3) - 6 * x.Pow(2) + 11 * x - 6;
            var roots = eq.SolveEquation("x");
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }
        [TestMethod]
        public void TestVars2()
        {
            var goose = MathS.Var("goose");
            var eq = ((x - goose) * (x - 3)).Expand();
            var roots = eq.SolveEquation(x);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }
        [TestMethod]
        public void TestVars4mp()
        {
            var goose = MathS.Var("goose");
            var eq = ((x - goose) * (x - 3) * (MathS.Sqr(x) - 4));
            var roots = eq.SolveEquation(x);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }

        [TestMethod]
        public void MomoTest()
        {
            Entity expr = "1/210 - (17*x)/210 + (101*x^2)/210 - (247*x^3)/210 + x^4";
            var sols = expr.SolveEquation("x");
            AssertRootCount(sols, 4);
            foreach (var root in sols.FiniteSet())
                AssertRoots(expr, x, root);
        }

        [TestMethod]
        public void TestVars3()
        {
            var goose = MathS.Var("goose");
            var momo = MathS.Var("momo");
            var eq = ((x - goose) * (x + goose * momo) * (x - momo * 2)).Expand();
            var roots = eq.SolveEquation(x);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }
        [TestMethod]
        public void TestVars4()
        {
            var goose = MathS.Var("goose");
            var momo = MathS.Var("momo");
            var quack = MathS.Var("quack");
            var eq = ((x - goose) * (x - momo) * (x - quack) * (x - momo * goose * quack)).Expand();
            var roots = eq.SolveEquation(x);
            Assert.IsNotNull(roots, "roots is null");
            AssertRootCount(roots, 4);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }
        [TestMethod]
        public void TestVars2_0()
        {
            var goose = MathS.Var("goose");
            var momo = MathS.Var("momo");
            var eq = ((x - momo) * (x - goose)).Expand();
            var roots = eq.SolveEquation(x);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }
        [TestMethod]
        public void TestReduce()
        {
            Entity expr = "3x5 + 5x3";
            var roots = expr.SolveEquation("x");
            AssertRootCount(roots, 3);
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr, x, root);
        }

        public void InvertedFunctionTests(string func, int rootAmount)
        {
            Entity toRepl = func + "(x2 + 3)";
            Entity expr = MathS.Sqr(toRepl) + 0.3 * toRepl - 0.1 * MathS.Var("a");
            var roots = expr.SolveEquation(x);
            AssertRootCount(roots, rootAmount);
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr.Substitute("a", 5), x, root.Substitute("n_1", 3).Substitute("a", 5));
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
            new ComplexNumber(0, 1),
            new ComplexNumber(1, 0),
            new ComplexNumber(-3, -3),
            new ComplexNumber(2, 2),
            new ComplexNumber(13, 13),
            new ComplexNumber(-9, +7),
            new ComplexNumber(0.5, -0.5),
            new ComplexNumber(-0.5, 0.5),
        };

        [TestMethod]
        public void TestLogs()
        {
            Entity eqs = "log(x, 32) - 5";
            var roots = eqs.SolveEquation("x");
            AssertRootCount(roots, 1);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eqs, x, root);
        }

        [TestMethod]
        public void TestFerrari1()
        {
            Entity eq = "x4 - x2 + 1";
            var roots = eq.SolveEquation("x");
            AssertRootCount(roots, 4);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, "x", root);
        }

        [TestMethod]
        public void TestFerrari2()
        {
            Entity eq = "x4 - x + 1";
            var roots = eq.SolveEquation("x");
            AssertRootCount(roots, 4);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, "x", root);
        }
        [TestMethod]
        public void TestFerrari3()
        {
            Entity eq = "x4 - x3 + 1";
            var roots = eq.SolveEquation("x");
            AssertRootCount(roots, 4);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, "x", root);
        }
        [TestMethod]
        public void TestFerrari4()
        {
            Entity eq = "x4 - x2 + x - x3 + 1";
            var roots = eq.SolveEquation("x");
            AssertRootCount(roots, 4);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, "x", root);
        }
        [TestMethod]
        public void TestFerrari5()
        {
            Entity eq = "(x2 - 1)2 - 2";
            var roots = eq.SolveEquation("x");
            AssertRootCount(roots, 4);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, "x", root);
        }

        [TestMethod]
        public void TestFerrari6()
        {
            Entity eq = "x4 - 2x2 - 1";
            var roots = eq.SolveEquation("x");
            AssertRootCount(roots, 4);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, "x", root);
        }

        [TestMethod]
        public void TestFerrari7()
        {
            Entity eq = "x4 - 2x2 - 2";
            var roots = eq.SolveEquation("x");
            AssertRootCount(roots, 4);
            foreach (var root in roots.FiniteSet())
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
            AssertRootCount(roots, 1);
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr, x, root);
        }

        [TestMethod]
        public void TestLinearTrigRoots1()
        {
            Entity expr = "sin(x) + cos(x) - 1";
            var roots = expr.SolveEquation("x");
            AssertRootCount(roots, 2);
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr, x, root);
        }

        [TestMethod]
        public void TestLinearTrigRoots2()
        {
            Entity expr = "sin(x) + cos(x) - 0.5";
            var roots = expr.SolveEquation("x");
            AssertRootCount(roots, 2);
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr, x, root);
        }

        [TestMethod]
        public void TestLinearTrigRoots3()
        {
            Entity expr = "sin(x) + cos(x) - 2";
            var roots = expr.SolveEquation("x");
            AssertRootCount(roots, 2);
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr, x, root);
        }

        [TestMethod]
        public void TestLinearTrigRoots4()
        {
            Entity expr = "sin(x)^2 + cos(x) - 1";
            var roots = expr.SolveEquation("x");
            AssertRootCount(roots, 3);
            
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr, x, root);
            // 2 pi n, -pi/2 + 2 pi n, pi/2 + 2 pi n
        }

        public void TestLinearTrig5Func(Entity expr)
        {
            var roots = expr.SolveEquation("x");
            AssertRootCount(roots, 4);
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr, x, root);
        }

        [TestMethod]
        public void TestLinearTrigRoots5_1()
            => TestLinearTrig5Func("3 * sin(2 * x + 1) - sin(x) - a");

        [TestMethod]
        public void TestLinearTrigRoots5_2()
            => TestLinearTrig5Func("3 * sin(1 + 2 * x) - sin(x) - a");

        [TestMethod]
        public void TestLinearTrigRoots5_3()
            => TestLinearTrig5Func("3 * sin(1 + x * 2) - sin(x) - a");

        [TestMethod]
        public void TestLinearTrigRoots5_4()
            => TestLinearTrig5Func("3 * sin(x * 2 + 1) - sin(x) - a");

        [TestMethod]
        public void TestLinearTrigRoots5_5()
            => TestLinearTrig5Func("3 * cos(2 * x + 1) - cos(x) - a");

        [TestMethod]
        public void TestLinearTrigRoots5_6()
            => TestLinearTrig5Func("3 * cos(1 + 2 * x) - cos(x) - a");

        [TestMethod]
        public void TestLinearTrigRoots5_7()
            => TestLinearTrig5Func("3 * cos(1 + x * 2) - cos(x) - a");

        [TestMethod]
        public void TestLinearTrigRoots5_8()
            => TestLinearTrig5Func("3 * cos(x * 2 + 1) - cos(x) - a");
        [TestMethod]
        public void TestLinearTrigRootsMomosIssue()
            => TestLinearTrig5Func("sin(2x + 2) + sin(x + 1) - a");

        [TestMethod]
        public void TestLinearTrigRoots7()
        {
            var exprs = new List<Entity>() {
                "3 * sin(2 * x) - sin(x) - a",
                "3 * sin(x * 2) - sin(x) - a",
                "3 * sin(1 + x) - sin(x) - a",
                "3 * sin(x + 1) - sin(x) - a",
                "3 * cos(2 * x) - cos(x) - a",
                "3 * cos(x * 2) - cos(x) - a",
                "3 * cos(1 + x) - cos(x) - a",
                "3 * cos(x + 1) - cos(x) - a",
            };
            foreach (var expr in exprs)
            {
                var roots = expr.SolveEquation("x");
                foreach (var root in roots.FiniteSet())
                    AssertRoots(expr, x, root);
            }
        }

        [TestMethod]
        public void TestLinearTrigRoots6()
        {
            Entity expr = "sin(2*x + 1) - sin(x) - 1";
            var roots = expr.SolveEquation("x");
            AssertRootCount(roots, 4);
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr, x, root);
        }
    }
}


