﻿//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using static AngouriMath.Entity.Number;
using Xunit;
using System.Collections.Generic;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;

namespace UnitTests.Algebra
{
    public sealed class SolveOneEquation
    {
        public static Variable x = nameof(x);

        /// <summary>Numerically checks if a root fits an equation</summary>
        internal static void AssertRoots(Entity equation, Entity.Variable toSub, Entity varValue, Integer? subValue = null)
        {
            subValue ??= 3;
            string eqNormal = equation.Stringize();

            var rootSimplified = varValue.Complexity > 100 ? varValue : varValue.InnerSimplified;

            equation = equation.Substitute(toSub, rootSimplified);
            // MUST be integer to correspond to integer coefficient of periodic roots
            var substitutions = new Dictionary<Entity.Variable, Integer>();
            foreach (var vr in equation.Vars)
                substitutions.Add(vr, subValue + substitutions.Count);
            equation = equation.Substitute(substitutions);
            var err = equation.EvalNumerical().Abs();
            Assert.True(err < 0.001m, $"\nError = {err.Stringize()}\n{eqNormal}\nWrong root: {toSub.Stringize()} = {rootSimplified.Stringize()}");
        }

        static void AssertRootCount(FiniteSet roots, int target)
        {
            Assert.NotEqual(-1, target);
            Assert.Equal(target, roots.Count);
        }

        void VerifySetOfRoots(Entity expr, Set roots, int rootCount, Integer? toSub)
        {
            var finiteSet = Assert.IsType<FiniteSet>(roots);
            AssertRootCount(finiteSet, rootCount);
            foreach (var root in finiteSet)
                AssertRoots(expr, x, root, toSub);
        }

        void TestSolver(Entity expr, int rootCount, Integer? toSub = null, bool testNewton = false)
        {
            using var _ = MathS.Settings.AllowNewton.Set(false);
            var rootsRaw = expr.SolveEquation(x);
            var roots = (Set)rootsRaw.InnerSimplified;
            VerifySetOfRoots(expr, roots, rootCount, toSub);

            if (!testNewton) return;
            // TODO: Increase Newton precision
            using var __ = MathS.Settings.PrecisionErrorZeroRange.Set(2e-16m);
            var ntRoots = expr.SolveNt(x);
            Assert.Equal(rootCount, ntRoots.Count);
            foreach (var root in ntRoots)
                AssertRoots(expr, x, root, toSub);
        }

        [Fact]
        public void TestPolynomialToFix()
        {
            var eq = x.Pow(2) + 2 * x + 1;
            using var _ = MathS.Settings.PrecisionErrorCommon.Set(1e-8m);
            using var __ = MathS.Settings.NewtonSolver.Set(new() { Precision = 100 });
            var roots = eq.SolveNt(x);
            // AssertRootCount(roots, 1); TODO: remove // after fix
            foreach (var root in roots)
                AssertRoots(eq, x, root);
        }

        [Theory]
        [InlineData("(x - 1) * (x - 2)", 2)]
        [InlineData("sqr(x) + 1", 2)]
        [InlineData("x2 + 2x + 2", 2)]
        [InlineData("2x2 + 4x + 2", 1)]
        [InlineData("x2 - 3x + 2", 2)]
        [InlineData("x3 + 3x2 + 3x + 1", 1)]
        [InlineData("x3 - 6x2 + 11x - 6", 3)]
        // TODO: Fix Newton Solver and set testNewton:true
        public void Polynomial(string expr, int rootCount) => TestSolver(expr, rootCount);

        [Theory]
        [InlineData("(x - goose) * (x - 3)", 2)]
        [InlineData("(x - momo) * (x - goose)", 2)]
        [InlineData("(x - goose) * (x + goose * momo) * (x - momo * 2)", 3)]
        // [InlineData("(x - goose) * (x - 2) * (sqr(x) - 4)", 3)] // TODO: Currently outputs 4 roots
        [InlineData("(x - goose) * (x - 3) * (sqr(x) - 4)", 4)]
        [InlineData("(x - goose) * (x - momo) * (x - quack) * (x - momo * goose * quack)", 4)]
        public void VarsExpanded(string expr, int rootCount)
        {
            var eq = MathS.FromString(expr);
            TestSolver(eq.Expand(), rootCount);
        }

        [Theory]
        [InlineData("(x - goose) * (x - 3)", 2)]
        [InlineData("(x - momo) * (x - goose)", 2)]
        [InlineData("(x - goose) * (x + goose * momo) * (x - momo * 2)", 3)]
        // [InlineData("(x - goose) * (x - 2) * (sqr(x) - 4)", 3)] // TODO: Currently outputs 4 roots
        [InlineData("(x - goose) * (x - 3) * (sqr(x) - 4)", 4)]
        [InlineData("(x - goose) * (x - momo) * (x - quack) * (x - momo * goose * quack)", 4)]
        public void Vars(string expr, int rootCount)
        {
            var eq = MathS.FromString(expr);
            TestSolver(eq, rootCount);
        }

        // TODO: Fix Newton Solver and set testNewton:true
        [Fact] public void MomoTest() => TestSolver("1/210 - (17*x)/210 + (101*x^2)/210 - (247*x^3)/210 + x^4", 4);
        // TODO: Fix Newton Solver and set testNewton:true
        [Fact] public void Logs() => TestSolver("log(x, 32) - 5", 1);
        [Fact] public void PiM1PowX() => TestSolver("pi - 1^x", 0, testNewton: true); // Check if it doesn't hang
        // TODO: Fix Newton Solver and set testNewton:true
        [Fact] public void ExpSimpl() => TestSolver("x^4 * x^y - 2", 1);

        [Theory]
        [InlineData("3x5 + 5x3", 3)]
        [InlineData("3x10 + 5x6", 5)]
        // Wolfram Alpha goes nuts, LOL: https://www.wolframalpha.com/input/?i=3x%5E5+%2B+5x%5E3+%3D-+a
        // [InlineData("3x5 + 5x3 + a")] // TODO: To doose (honk honk)
        // TODO: Fix Newton Solver and set testNewton:true
        public void Reduce(string expr, int rootCount) => TestSolver(expr, rootCount);

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
        public void InvertedFunctions(string func, int rootAmount)
        {
            Entity toRepl = func + "(x2 + 3)";
            Entity expr = MathS.Sqr(toRepl) + 0.3 * toRepl - 0.1 * MathS.Var("a");
            var roots = expr.SolveEquation(x);
            roots = (Set)roots.InnerSimplified;
            var finite = Assert.IsType<FiniteSet>(roots);
            AssertRootCount(finite, rootAmount);
            foreach (var root in finite)
                AssertRoots(expr.Substitute("a", 5), x, root.Substitute("n_1", 3).Substitute("a", 5));
        }

        [Theory]
        [InlineData("x4 - x2 + 1")]
        [InlineData("x4 - x + 1")]
        [InlineData("x4 - x3 + 1")]
        [InlineData("x4 - x2 + x - x3 + 1")]
        [InlineData("(x2 - 1)2 - 2")]
        [InlineData("x4 - 2x2 - 1")]
        [InlineData("x4 - 2x2 - 2")]
        // TODO: Fix Newton Solver and set testNewton:true
        public void Ferrari(string input) => TestSolver(input, 4);

        [Theory]
        [InlineData("sin(x) + cos(x) - 1", 2)]
        [InlineData("sin(x) + cos(x) - 0.5", 2)]
        [InlineData("sin(x) + cos(x) - 2", 2)]
        [InlineData("sin(x)^2 + cos(x) - 1", 3)] // 2 pi n, -pi/2 + 2 pi n, pi/2 + 2 pi n
        [InlineData("3 * cos(x * 2 + 1) - cos(x) - a", 4)]
        [InlineData("sin(2x + 2) + sin(x + 1) - a", 4)] // Momo's Issue
        [InlineData("3 * sin(2 * x) - sin(x) - a", 4)]
        [InlineData("sec(2x + c) + cosec(2x + c) + c", 4)]
        [InlineData("sec(2x) + cot(2x + c) + c", 4)]
        public void LinearTrigRoots(string expr, int rootCount) => TestSolver(expr, rootCount);

        [Theory]
        [InlineData("(x - b) / (x + a) + c", 1)]
        [InlineData("(x - b) / (x + a) + c / (x + a)", 1)]
        [InlineData("(x - b) / (x + a) + c / (x + a)2", 2)]
        [InlineData("(x - b) / (x + a) + c + (x - c) / (x + d)", 2, 11)]
        public void CDSolver(string expr, int rootCount, int? toSub = null) => TestSolver(expr, rootCount, toSub);

        [Theory]
        [InlineData("x + sqrt(x^0.1 + a) + c", 0)]
        [InlineData("(x + 6)^(1/6) + x + x3 + a", 0)]
        [InlineData("sqrt(x + 1) + sqrt(x + 2) + a + x", 0)]
        [InlineData("(x + 1)^(1/3) - x - a", 3)]
        public void FractionedPoly(string expr, int rootCount) => TestSolver(expr, rootCount);

        [Fact(Skip = "Piecewise required")]
        public void FractionedPolyTestPiecewise() => TestSolver("x + sqrt(x + a) + c", 2);

        [Theory]
        [InlineData("sgn(x) + 1")]
        [InlineData("sgn(x) - 1")]
        [InlineData("sgn(x) + 1 / sqrt(2) + i / sqrt(2)")]
        [InlineData("sgn(x) + 1 / sqrt(2) - i / sqrt(2)")]
        [InlineData("sgn(x) - 1 / sqrt(2) - i / sqrt(2)")]
        [InlineData("sgn(x) - 1 / sqrt(2) + i / sqrt(2)")]
        public void SignumTest(string expr) => TestSolver(expr, 1, toSub: 4);

        [Theory]
        [InlineData("abs(x) - 5", 5)]
        [InlineData("abs(x) - a", 4)]
        [InlineData("abs(x) - a", 3)]
        [InlineData("abs(x) - a", 2)]
        [InlineData("abs(x) - a", 10)]
        public void AbsTest(string expr, int value) => TestSolver(expr, rootCount: 1, toSub: value);

        [Fact(Skip = "Piecewise required")]
        public void Abs0RootsTest1() => AbsTest("abs(x) + 5", 3);

        [Fact(Skip = "Piecewise required")]
        public void Abs0RootsTest2() => AbsTest("abs(x) - i", 3);

        [Fact(Skip = "Piecewise required")]
        public void Sign0RootsTest1() => SignumTest("sgn(x) + 14");

        [Fact(Skip = "Piecewise required")]
        public void Sign0RootsTest2() => SignumTest("sgn(x) + 1.1");
        [Fact(Skip = "Piecewise required")]
        public void Sign0RootsTest3() => SignumTest("sgn(x) + i + 1");

        [Theory]
        [InlineData("4^x - a", 1, 3)]
        [InlineData("a^x + (a^2)^x - c", 2)]
        [InlineData("e^x + (e2)^x - 1", 2)]
        [InlineData("2 ^ (x sin(x)) + 4 ^ (x sin(x)) + c", 0)]
        [InlineData("2^x - 4^x", 1)]
        public void TestExponentialSolver(string equation, int rootCount, int? toSub = null)
            => TestSolver(equation, rootCount, toSub);

        [Theory(Skip = "Exponentiation works unexpectedly")]
        [InlineData("4^x + 2^x - a", 2, 3)]
        /*
         
        The issue here is the case, say, 3 ^ (2 i pi).
        Ideally, it would work as e ^ (ln(3) 2 i pi) = 
        (e ^ (2 i pi)) ^ ln(3) = 1 ^ ln(3) = 1
        Which is not the case for either AM or WA,
        as for this expression both work like this
        https://www.wolframalpha.com/input/?i=3+%5E+%282+pi+i%29
         
         */
        [InlineData("1 + 2 ^ x + 4 ^ x + 8 ^ x - c", 3)]
        public void TestExponentialSolverSkipped(string equation, int rootCount, int? toSub = null)
            => TestSolver(equation, rootCount, toSub);

        [Theory]
        [InlineData("(x / 3)^2 - 4", 2)]
        [InlineData("(3 / x)^2 - 4", 2)]
        [InlineData("(x - 3)^2 - 4", 2)]
        [InlineData("(3 - x)^2 - 4", 2)]
        [InlineData("arccos(x)2 - 1", 1)]
        [InlineData("x! - 1", 0)]
        [InlineData("limit(x, x, y)2 - 2", 0)]
        [InlineData("sec(x) - a", 2)]
        [InlineData("csc(x) - a", 2)]
        [InlineData("arcsec(x) - a", 1, 3)]
        [InlineData("arccsc(x) - a", 1, 1)]
        public void TestInvertNodes(string equation, int rootCount, int? toSub = null)
            => TestSolver(equation, rootCount, toSub);
    }
}