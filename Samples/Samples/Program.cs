using System;
using AngouriMath;
using AngouriMath.Core;
using System.Diagnostics;
using System.Numerics;
using System.Linq.Expressions;
using AngouriMath.Core.TreeAnalysis;
using AngouriMath.Functions.Algebra.AnalyticalSolving;
using AngouriMath.Convenience;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Samples
{
#pragma warning disable IDE0051
    class Program
    {
        static void Sample1()
        {
            var inp = "1 + 2 * log(9, 3)";
            var expr = MathS.FromString(inp);
            Console.WriteLine(expr.Eval());
        }
        static void Sample2()
        {
            var x = MathS.Var("x");
            var y = MathS.Var("y");
            var c = x * y + x / y;
            Console.WriteLine(MathS.Sqr(c));
        }
        static void Sample3()
        {
            var x = MathS.Var("x");
            var expr = x * 2 + MathS.Sin(x) / MathS.Sin(MathS.Pow(2, x));
            var subs = expr.Substitute(x, 0.3);
            Console.WriteLine(subs.Simplify());
        }
        static void Sample4()
        {
            var x = MathS.Var("x");
            var func = MathS.Sqr(x) + MathS.Ln(MathS.Cos(x) + 3) + 4 * x;
            var derivative = func.Derive(x);
            Console.WriteLine(derivative.Simplify());
        }
#pragma warning disable IDE0039
        static void Sample5()
        {
            var x = MathS.Var("x");
            var expr = (x + 3).Pow(x + 4);
            Func<NumberEntity, Entity> wow = v => expr.Substitute(x, v).Eval();
            Console.WriteLine(wow(4));
            Console.WriteLine(wow(5));
            Console.WriteLine(wow(6));
#pragma warning restore IDE0039
        }
        static void Sample6()
        {
            var x = MathS.Var("x");
            var y = MathS.Var("y");
            var expr = x.Pow(y) + MathS.Sqrt(x + y / 4) * (6 / x);
            Console.WriteLine(expr.Latexise());
        }
        static void Sample7()
        {
            var expr = MathS.Pow(MathS.e, MathS.pi * MathS.i);
            Console.WriteLine(expr);
            Console.WriteLine(expr.Eval());
        }
        static void Sample8()
        {
            var x = MathS.Var("x");
            var equation = (x - 1) * (x - 2) * (MathS.Sqr(x) + 1);
            Console.Write(equation.SolveNt(x));
        }
        static void Sample9()
        {
            var x = MathS.Var("x");
            var expr = MathS.Sin(x) + MathS.Sqrt(x) / (MathS.Sqrt(x) + MathS.Cos(x)) + MathS.Pow(x, 3);
            Console.WriteLine(expr.DefiniteIntegral(x, -3, 3));
            var expr2 = MathS.Sin(x);
            Console.WriteLine(expr2.DefiniteIntegral(x, 0, Math.PI));
        }
        static void Sample10()
        {
            var x = MathS.Var("x");
            var expr = MathS.Sin(x) + MathS.Sqrt(x) / (MathS.Sqrt(x) + MathS.Cos(x)) + MathS.Pow(x, 3);
            var func = expr.Compile(x);
            Console.WriteLine(func.Substitute(3));
        }
        static void Sample11()
        {
            var x = MathS.Var("x");
            var expr = (MathS.Arcsin(6 * x) + MathS.Arccos(6 * x)) - (MathS.Arctan(x) + MathS.Arccotan(x));
            Console.WriteLine(expr.Simplify());
        }
        static void Sample12()
        {
            var expr = MathS.FromString("3x3 + 2 2 2 - x(3 0.5)");
            Console.WriteLine(expr);
        }
        static void Sample13()
        {
            var x = MathS.Var("x");
            var a = MathS.Var("a");
            var b = MathS.Var("b");
            var expr = MathS.Sqrt(x) / x + a * b + b * a + (b - x) * (x + b) + 
                MathS.Arcsin(x + a) + MathS.Arccos(a + x);
            Console.WriteLine(expr.Simplify());
        }
        static void Sample14()
        {
            Entity expr = "sqr(x + y)";
            Console.WriteLine(expr.Expand().Simplify());
        }
        static void Sample15()
        {
            Entity expr = "(sin(x)2 - sin(x) + a)(b - x)((-3)x + 2 + 3x2 + (x - 3)x3)";
            foreach (var root in expr.Solve("x"))
                Console.WriteLine(root);
        }
        static void Sample16()
        {
            var x = SySyn.Symbol("x");
            var expr = SySyn.Exp(x) + x;
            Console.WriteLine(SySyn.Diff(expr));
            Console.WriteLine(SySyn.Diff(expr, x));
            Console.WriteLine(SySyn.Diff(expr, x, x));
        }

        private static void Sample17()
        {
            string x = MathS.ToBaseN(-32.25, 4);
            Console.WriteLine("-32.25(10) = " + x + "(4)");
            var y = MathS.FromBaseN("AB.3", 16);
            Console.WriteLine("AB.3(16) = " + y + "(1)");
        }

        static Complex MyFunc(Complex x)
            => x + 3 * x;

        public static readonly Tensor A = MathS.Matrix(4, 2,
            1, 2,
            3, 4,
            5, 6,
            7, 8
        );
        public static readonly Tensor B = MathS.Matrix(4, 2,
            1, 2,
            3, 4,
            5, 6,
            7, 8
        );
        public static readonly Tensor C = MathS.Matrix(2, 4,
            1, 2, 3, 4,
            5, 6, 7, 8
        );

        static void Main(string[] _)
        {
            //foreach (var ve in MathS.Quack(-0.5))
            //    Console.WriteLine(MathS.Arccos(ve).Eval());
            //return;

            //Entity v = "(3x3 + 2x2 + 2x) ^ 3 + (3x3 + 2x2 + 2x) ^ 2 + (3x3 + 2x2 + 2x) + a"; 
            //Entity v = "(3x + 2x2 + 2x) ^ 2 + (3x + 2x2 + 2x) ^ 2 + (3x + 2x2 + 2x) + 1";
            //Console.WriteLine(MathS.ToSympyCode(v));
            //Console.WriteLine(v.ToString());


            /*
            var a = new Number(24, 1);

            foreach (var r in MathS.Quack(a))
                Console.WriteLine(r.Eval());
            Console.WriteLine();
            Console.WriteLine(Complex.Acos(a));
            Console.WriteLine(-Complex.Acos(a));
            Console.WriteLine();
            Console.WriteLine();
            foreach (var r in MathS.Quack(a))
                Console.WriteLine(Complex.Cos(r.Eval()));
            Console.WriteLine();
            Console.WriteLine(Complex.Cos(Complex.Acos(a)));
            Console.WriteLine(Complex.Cos(-Complex.Acos(a)));
            //Console.WriteLine();
            return;*/
            // cos(x2 + 2x + 2) ^ 2 + cos(x2 + 2x + 2) + 3
            // cos(t + 2) ^ 2 + cos(t + 2) + 3
            // g ^ 2 + g + 3
            // g = 3
            // cos(t + 2) = 3
            // t = 5
            // 5 = x2 + 2x
            // x = 6
            //Console.WriteLine(MathS.Quack("cos(x3 + x2)2 + cos(x3 + x2)", "x"));
            //Entity q = "x2";
            Entity expr = "cos(x2 + x) - 1";
            var a = expr.Name;
            //Console.WriteLine(expr.Solve("x").Count);
            foreach (var root in expr.Solve("x"))
                Console.WriteLine(expr.Substitute("x", root).Substitute("n", 3).Eval());
            /*
            Entity toRepl = "cos(x2 + 2x + 1)"; // cos(x2 + 2x + 1) -> cos(t + 1) -> ... g
            //Entity toRepl = "cos(x)";
            Entity expr = MathS.Sqr(toRepl) + 0.3 * toRepl - 3;
            Console.WriteLine(expr.ToString().Replace("^", "**"));
            var roots = expr.Solve("x");
            foreach (var root in roots)
                //Console.WriteLine(expr.Substitute("x", root).Substitute("n", 3).Eval();
            {
                Console.WriteLine("Корень: " + root.ToString());
                var t = root.Substitute("n", 5).Eval();
                Console.WriteLine("Корень пощитанный: " + t.ToString());
                Console.WriteLine("Ашипка:" + expr.Substitute("x", t).Eval().ToString());
                Console.WriteLine();
            }*/
        }
    }
#pragma warning restore IDE0051
}
