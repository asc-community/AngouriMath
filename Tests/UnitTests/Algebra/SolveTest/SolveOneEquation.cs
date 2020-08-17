using AngouriMath;
using AngouriMath.Core;
using Xunit;
using System;
using System.Collections.Generic;
using AngouriMath.Core.Numerix;
using System.Linq;

namespace UnitTests.Algebra
{
    public class SolveOneEquation
    {
        public static Entity.Variable x = nameof(x);

        /// <summary>Numerically checks if a root fits an equation</summary>
        internal static void AssertRoots(Entity equation, Entity.Variable toSub, Entity varValue, IntegerNumber? subValue = null)
        {
            subValue ??= 3;
            string eqNormal = equation.ToString();
            equation = equation.Substitute(toSub, varValue);
            // MUST be integer to correspond to integer coefficient of periodic roots
            var substitutions = new Dictionary<Entity.Variable, IntegerNumber>();
            foreach (var vr in equation.Vars)
                substitutions.Add(vr, subValue + substitutions.Count);
            equation = equation.Substitute(substitutions);
            var err = Entity.Number.Abs(equation.Eval());
            Assert.True(err < 0.001m, $"\nError = {err}\n{eqNormal}\nWrong root: {toSub} = {varValue}");
        }

        static void AssertRootCount(Set roots, int target)
        {
            Assert.NotEqual(Set.PowerLevel.INFINITE, roots.Power);
            Assert.Equal(target, roots.Count);
        }

        void TestSolver(Entity expr, int rootCount, IntegerNumber? toSub = null)
        {
            var roots = expr.SolveEquation(x);
            AssertRootCount(roots, rootCount);
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr, x, root, toSub);
        }

        [Fact]
        public void Test1()
        {
            var eq = (x - 1) * (x - 2);
            var roots = eq.SolveNt(x);
            AssertRootCount(roots, 2);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }
        [Fact]
        public void Test2()
        {
            // TODO: Remove this line when precision is increased
            MathS.Settings.PrecisionErrorZeroRange.As(2e-16m, () =>
            {
                var eq = MathS.Sqr(x) + 1;
                var roots = eq.SolveNt(x);
                AssertRootCount(roots, 2);
                foreach (var root in roots.FiniteSet())
                    AssertRoots(eq, x, root);
            });
        }
        [Fact]
        public void Test4()
        {
            var eq = x.Pow(2) + 2 * x + 1;
            var roots = MathS.Settings.PrecisionErrorCommon.As(1e-8m, () =>
                MathS.Settings.NewtonSolver.As(new NewtonSetting() {Precision = 100}, () =>
                    eq.SolveNt(x)
                ));
            // AssertRootCount(roots, 1); TODO: remove // after fix
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }

        [Fact]
        public void Test5()
        {
            // solve x2 + 2x + 2
            var eq = x.Pow(2) + 2 * x + 2;
            var roots = eq.SolveEquation(x);
            var r1 = MathS.FromString("-1 + 1i").Simplify();
            var r2 = MathS.FromString("-1 - 1i").Simplify();
            AssertRootCount(roots, 2);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
            var s = new Set();
        }

        [Fact]
        public void Test6()
        {
            // solve 2x2 + 4x + 2
            var eq = 2 * x.Pow(2) + 4 * x + 2;
            var roots = eq.SolveEquation(x);
            AssertRootCount(roots, 1);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }

        [Fact]
        public void Test7()
        {
            // solve x2 - 3x + 2
            var eq = x.Pow(2) - 3 * x + 2;
            var roots = eq.SolveEquation(x);
            AssertRootCount(roots, 2);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }

        [Fact]
        public void Test8()
        {
            // solve x3 + 3x2 + 3x + 1
            var eq = x.Pow(3) + 3 * x.Pow(2) + 3 * x + 1;
            var roots = eq.SolveEquation(x);
            AssertRootCount(roots, 1);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }

        [Fact]
        public void Test9()
        {
            // solve x3 - 6x2 + 11x - 6
            var eq = x.Pow(3) - 6 * x.Pow(2) + 11 * x - 6;
            var roots = eq.SolveEquation(x);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }
        [Fact]
        public void TestVars2()
        {
            var goose = MathS.Var("goose");
            var eq = ((x - goose) * (x - 3)).Expand();
            var roots = eq.SolveEquation(x);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }
        [Fact]
        public void TestVars4mp()
        {
            var goose = MathS.Var("goose");
            var eq = ((x - goose) * (x - 3) * (MathS.Sqr(x) - 4));
            var roots = eq.SolveEquation(x);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }

        [Fact]
        public void MomoTest()
        {
            Entity expr = "1/210 - (17*x)/210 + (101*x^2)/210 - (247*x^3)/210 + x^4";
            var sols = expr.SolveEquation(x);
            AssertRootCount(sols, 4);
            foreach (var root in sols.FiniteSet())
                AssertRoots(expr, x, root);
        }

        [Fact]
        public void TestVars3()
        {
            var goose = MathS.Var("goose");
            var momo = MathS.Var("momo");
            var eq = ((x - goose) * (x + goose * momo) * (x - momo * 2)).Expand();
            var roots = eq.SolveEquation(x);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }
        [Fact]
        public void TestVars4()
        {
            var goose = MathS.Var("goose");
            var momo = MathS.Var("momo");
            var quack = MathS.Var("quack");
            var eq = ((x - goose) * (x - momo) * (x - quack) * (x - momo * goose * quack)).Expand();
            var roots = eq.SolveEquation(x);
            Assert.NotNull(roots);
            AssertRootCount(roots, 4);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }
        [Fact]
        public void TestVars2_0()
        {
            var goose = MathS.Var("goose");
            var momo = MathS.Var("momo");
            var eq = ((x - momo) * (x - goose)).Expand();
            var roots = eq.SolveEquation(x);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }
        [Fact]
        public void TestReduce()
        {
            Entity expr = "3x5 + 5x3";
            var roots = expr.SolveEquation(x);
            AssertRootCount(roots, 3);
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr, x, root);
        }

        [Theory]
        [InlineData("sin", 8)]
        [InlineData("cos", 8)]
        [InlineData("tan", 4)]
        [InlineData("cotan", 4)]
        [InlineData("arcsin", 4)]
        // TODO: arccos return analytically correct answer, but incorrect when substituting variables (as there are some Y such there is no X that arccos(X) = Y)
        // [InlineData("arccos", 4)]
        [InlineData("arctan", 4)]
        [InlineData("arccotan", 4)]
        public void InvertedFunctionTests(string func, int rootAmount)
        {
            Entity toRepl = func + "(x2 + 3)";
            Entity expr = MathS.Sqr(toRepl) + 0.3 * toRepl - 0.1 * MathS.Var("a");
            var roots = expr.SolveEquation(x);
            AssertRootCount(roots, rootAmount);
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr.Substitute("a", 5), x, root.Substitute("n_1", 3).Substitute("a", 5));
        }

        private readonly List<ComplexNumber> KeyPoints = new List<ComplexNumber>
        {
            ComplexNumber.Create(0, 1),
            ComplexNumber.Create(1, 0),
            ComplexNumber.Create(-3, -3),
            ComplexNumber.Create(2, 2),
            ComplexNumber.Create(13, 13),
            ComplexNumber.Create(-9, +7),
            ComplexNumber.Create(0.5m, -0.5m),
            ComplexNumber.Create(-0.5m, 0.5m),
        };

        [Fact]
        public void TestLogs()
        {
            Entity eqs = "log(x, 32) - 5";
            var roots = eqs.SolveEquation(x);
            AssertRootCount(roots, 1);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eqs, x, root);
        }

        [Fact]
        public void TestFerrari1()
        {
            Entity eq = "x4 - x2 + 1";
            var roots = eq.SolveEquation(x);
            AssertRootCount(roots, 4);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }

        [Fact]
        public void TestFerrari2()
        {
            Entity eq = "x4 - x + 1";
            var roots = eq.SolveEquation(x);
            AssertRootCount(roots, 4);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }
        [Fact]
        public void TestFerrari3()
        {
            Entity eq = "x4 - x3 + 1";
            var roots = eq.SolveEquation(x);
            AssertRootCount(roots, 4);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }
        [Fact]
        public void TestFerrari4()
        {
            Entity eq = "x4 - x2 + x - x3 + 1";
            var roots = eq.SolveEquation(x);
            AssertRootCount(roots, 4);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }
        [Fact]
        public void TestFerrari5()
        {
            Entity eq = "(x2 - 1)2 - 2";
            var roots = eq.SolveEquation(x);
            AssertRootCount(roots, 4);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }

        [Fact]
        public void TestFerrari6()
        {
            Entity eq = "x4 - 2x2 - 1";
            var roots = eq.SolveEquation(x);
            AssertRootCount(roots, 4);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }

        [Fact]
        public void TestFerrari7()
        {
            Entity eq = "x4 - 2x2 - 2";
            var roots = eq.SolveEquation(x);
            AssertRootCount(roots, 4);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }

        [Fact]
        public void Test1px()
        {
            Entity expr = "pi - 1^x";
            expr.SolveEquation(x); // Check if it doesn't hang
        }

        [Fact]
        public void ExpSimpl()
        {
            Entity expr = "x^4 * x^y - 2";
            var roots = expr.SolveEquation(x);
            AssertRootCount(roots, 1);
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr, x, root);
        }

        [Fact]
        public void TestLinearTrigRoots1()
        {
            Entity expr = "sin(x) + cos(x) - 1";
            var roots = expr.SolveEquation(x);
            AssertRootCount(roots, 2);
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr, x, root);
        }

        [Fact]
        public void TestLinearTrigRoots2()
        {
            Entity expr = "sin(x) + cos(x) - 0.5";
            var roots = expr.SolveEquation(x);
            AssertRootCount(roots, 2);
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr, x, root);
        }

        [Fact]
        public void TestLinearTrigRoots3()
        {
            Entity expr = "sin(x) + cos(x) - 2";
            var roots = expr.SolveEquation(x);
            AssertRootCount(roots, 2);
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr, x, root);
        }

        [Fact]
        public void TestLinearTrigRoots4()
        {
            Entity expr = "sin(x)^2 + cos(x) - 1";
            var roots = expr.SolveEquation(x);
            AssertRootCount(roots, 3);
            
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr, x, root);
            // 2 pi n, -pi/2 + 2 pi n, pi/2 + 2 pi n
        }

        void TestLinearTrig5Func(Entity expr)
        {
            var roots = expr.SolveEquation(x);
            AssertRootCount(roots, 4);
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr, x, root);
        }

        [Fact]
        public void TestLinearTrigRoots5_1()
            => TestLinearTrig5Func("3 * sin(2 * x + 1) - sin(x) - a");

        [Fact]
        public void TestLinearTrigRoots5_2()
            => TestLinearTrig5Func("3 * sin(1 + 2 * x) - sin(x) - a");

        [Fact]
        public void TestLinearTrigRoots5_3()
            => TestLinearTrig5Func("3 * sin(1 + x * 2) - sin(x) - a");

        [Fact]
        public void TestLinearTrigRoots5_4()
            => TestLinearTrig5Func("3 * sin(x * 2 + 1) - sin(x) - a");

        [Fact]
        public void TestLinearTrigRoots5_5()
            => TestLinearTrig5Func("3 * cos(2 * x + 1) - cos(x) - a");

        [Fact]
        public void TestLinearTrigRoots5_6()
            => TestLinearTrig5Func("3 * cos(1 + 2 * x) - cos(x) - a");

        [Fact]
        public void TestLinearTrigRoots5_7()
            => TestLinearTrig5Func("3 * cos(1 + x * 2) - cos(x) - a");

        [Fact]
        public void TestLinearTrigRoots5_8()
            => TestLinearTrig5Func("3 * cos(x * 2 + 1) - cos(x) - a");
        [Fact]
        public void TestLinearTrigRootsMomosIssue()
            => TestLinearTrig5Func("sin(2x + 2) + sin(x + 1) - a");

        [Fact]
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
                var roots = expr.SolveEquation(x);
                foreach (var root in roots.FiniteSet())
                    AssertRoots(expr, x, root);
            }
        }

        [Fact]
        public void TestLinearTrigRoots6()
        {
            Entity expr = "sin(2*x + 1) - sin(x) - 1";
            var roots = expr.SolveEquation(x);
            AssertRootCount(roots, 4);
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr, x, root);
        }

        [Fact]
        public void TestCDSolver1()
        {
            Entity expr = "(x - b) / (x + a) + c";
            var roots = expr.SolveEquation(x);
            AssertRootCount(roots, 1);
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr, x, root);
        }

        [Fact]
        public void TestCDSolver2()
        {
            Entity expr = "(x - b) / (x + a) + c / (x + a)";
            var roots = expr.SolveEquation(x);
            AssertRootCount(roots, 1);
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr, x, root);
        }

        [Fact]
        public void TestCDSolver3()
        {
            Entity expr = "(x - b) / (x + a) + c / (x + a)2";
            var roots = expr.SolveEquation(x);
            AssertRootCount(roots, 2);
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr, x, root);
        }

        [Fact]
        public void TestCDSolver4()
        => TestSolver("(x - b) / (x + a) + c + (x - c) / (x + d)", 2, 11);

        [Fact]
        public void TestFractionedPoly1()
            => TestSolver("x + sqr(x + a) + c", 2);

        [Fact]
        public void TestFractionedPoly2()
            => TestSolver("x + sqr(x^0.1 + a) + c", 0);

        
        [Fact]
        public void TestFractionedPoly3()
            => TestSolver("(x + 6)^(1/6) + x + x3 + a", 0);

        [Fact]
        public void TestFractionedPoly4()
            => TestSolver("sqrt(x + 1) + sqrt(x + 2) + a + x", 0);

        [Fact]
        public void TestFractionedPoly5()
            => TestSolver("(x + 1)^(1/3) - x - a", 3);
    }
}


