
/* Copyright (c) 2019-2020 Angourisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
 * is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using AngouriMath.Core;
using NumericsComplex = System.Numerics.Complex;
using PeterO.Numbers;
using System.Linq;

namespace AngouriMath.Extensions
{
    using static Entity;
    using static Entity.Number;
    public static class AngouriMathExtensions
    {
        public static FiniteSet ToFiniteSet(this System.Collections.Generic.IEnumerable<Entity> expr) 
            => new FiniteSet(expr.Select(c => SetPiece.Element(c)));
        public static Entity ToEntity(this string expr) => MathS.FromString(expr);
        public static Entity Simplify(this string expr) => expr.ToEntity().Simplify();
        public static Complex Eval(this string expr) => expr.ToEntity().EvalNumerical();
        public static Entity Expand(this string expr) => expr.ToEntity().Expand();
        public static Entity Factorize(this string expr) => expr.ToEntity().Factorize();
        public static Entity Substitute(this string expr, Variable var, Entity value)
            => expr.ToEntity().Substitute(var, value);
        public static Set SolveEquation(this string expr, Variable x)
            => expr.ToEntity().SolveEquation(x);
        public static Integer ToNumber(this int value) => Integer.Create(value);
        public static Integer ToNumber(this long value) => Integer.Create(value);
        public static Integer ToNumber(this EInteger value) => Integer.Create(value);
        public static Real ToNumber(this float value) => Real.Create(EDecimal.FromSingle(value));
        public static Real ToNumber(this double value) => Real.Create(EDecimal.FromDouble(value));
        public static Real ToNumber(this decimal value) => Real.Create(EDecimal.FromDecimal(value));
        public static Real ToNumber(this EDecimal value) => Real.Create(value);
        public static Boolean ToBoolean(this bool value) => Boolean.Create(value);
        public static Complex ToNumber(this NumericsComplex complex)
            => Complex.Create(EDecimal.FromDouble(complex.Real), EDecimal.FromDouble(complex.Imaginary));
        public static string Latexise(this string str) => str.ToEntity().Latexise();
        public static FastExpression Compile(this string str, params Variable[] variables)
            => str.ToEntity().Compile(variables);
        public static Entity Derive(this string str, Variable x)
            => str.ToEntity().Derive(x);

        /*

            Utils/generate_tuples.bat to regenerate this block

        */

        public static Tensor? SolveSystem(this (string eq1, string eq2) eqs, string var1, string var2)
            => MathS.Equations(eqs.eq1, eqs.eq2).Solve(var1, var2);

        public static Tensor? SolveSystem(this (string eq1, string eq2, string eq3) eqs, string var1, string var2, string var3)
            => MathS.Equations(eqs.eq1, eqs.eq2, eqs.eq3).Solve(var1, var2, var3);

        public static Tensor? SolveSystem(this (string eq1, string eq2, string eq3, string eq4) eqs, string var1, string var2, string var3, string var4)
            => MathS.Equations(eqs.eq1, eqs.eq2, eqs.eq3, eqs.eq4).Solve(var1, var2, var3, var4);

        public static Tensor? SolveSystem(this (string eq1, string eq2, string eq3, string eq4, string eq5) eqs, string var1, string var2, string var3, string var4, string var5)
            => MathS.Equations(eqs.eq1, eqs.eq2, eqs.eq3, eqs.eq4, eqs.eq5).Solve(var1, var2, var3, var4, var5);

        public static Tensor? SolveSystem(this (string eq1, string eq2, string eq3, string eq4, string eq5, string eq6) eqs, string var1, string var2, string var3, string var4, string var5, string var6)
            => MathS.Equations(eqs.eq1, eqs.eq2, eqs.eq3, eqs.eq4, eqs.eq5, eqs.eq6).Solve(var1, var2, var3, var4, var5, var6);

        public static Tensor? SolveSystem(this (string eq1, string eq2, string eq3, string eq4, string eq5, string eq6, string eq7) eqs, string var1, string var2, string var3, string var4, string var5, string var6, string var7)
            => MathS.Equations(eqs.eq1, eqs.eq2, eqs.eq3, eqs.eq4, eqs.eq5, eqs.eq6, eqs.eq7).Solve(var1, var2, var3, var4, var5, var6, var7);

        public static Tensor? SolveSystem(this (string eq1, string eq2, string eq3, string eq4, string eq5, string eq6, string eq7, string eq8) eqs, string var1, string var2, string var3, string var4, string var5, string var6, string var7, string var8)
            => MathS.Equations(eqs.eq1, eqs.eq2, eqs.eq3, eqs.eq4, eqs.eq5, eqs.eq6, eqs.eq7, eqs.eq8).Solve(var1, var2, var3, var4, var5, var6, var7, var8);

        public static Tensor? SolveSystem(this (string eq1, string eq2, string eq3, string eq4, string eq5, string eq6, string eq7, string eq8, string eq9) eqs, string var1, string var2, string var3, string var4, string var5, string var6, string var7, string var8, string var9)
            => MathS.Equations(eqs.eq1, eqs.eq2, eqs.eq3, eqs.eq4, eqs.eq5, eqs.eq6, eqs.eq7, eqs.eq8, eqs.eq9).Solve(var1, var2, var3, var4, var5, var6, var7, var8, var9);
    }
}