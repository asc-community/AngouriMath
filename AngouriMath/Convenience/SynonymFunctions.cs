
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



﻿using System;
using System.Collections.Generic;
 using System.Numerics;
 using AngouriMath.Core;
 using AngouriMath.Core.Numerix;
 using PeterO.Numbers;

namespace AngouriMath
{
    using FuncTable = Dictionary<string, Func<List<Entity>, Entity>>;
    using SynTable = Dictionary<string, string>;
    internal static class SynonymFunctions
    {
        /// <summary>
        /// While parsing, we want to understand functions like "sqrt". After parsing, we replace
        /// nodes with this name by the appropriate expression
        /// </summary>
        internal static readonly FuncTable SynFunctions = new FuncTable
        {
            { "gammaf", args => MathS.Gamma(args[0]) },
            { "sqrtf", args => MathS.Pow(args[0], 0.5) },
            { "sqrf", args => MathS.Pow(args[0], 2) },
            { "lnf", args => MathS.Log(MathS.e, args[0]) },
            { "secf", args => MathS.Sec(args[0]) },
            { "cosecf", args => MathS.Cosec(args[0]) },
            { "arcsecf", args => MathS.Arcsec(args[0]) },
            { "arccosecf", args => MathS.Arccosec(args[0]) },
        };

        /// <summary>
        /// Expects a tree with "sqrt" and some other unresolved functions. Returns
        /// that with all "sqrt" and other replaced
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        internal static Entity Synonymize(Entity tree)
        {
            for (int i = 0; i < tree.Children.Count; i++)
            {
                tree.Children[i] = Synonymize(tree.Children[i]);
            }
            if (SynFunctions.ContainsKey(tree.Name))
                return SynFunctions[tree.Name](tree.Children);
            else
                return tree;
        }
    }
}

namespace AngouriMath.Extensions
{
    public static class AMExtensions
    {
        public static ComplexNumber ToComplexNumber(this Complex complex)
            => ComplexNumber.Create(EDecimal.FromDouble(complex.Real), EDecimal.FromDouble(complex.Imaginary));

        public static Entity ToEntity(this string expr) => MathS.FromString(expr);
        public static Entity Simplify(this string expr) => expr.ToEntity().Simplify();
        public static ComplexNumber Eval(this string expr) => expr.ToEntity().Eval();
        public static Entity Expand(this string expr) => expr.ToEntity().Expand();
        public static Entity Collapse(this string expr) => expr.ToEntity().Collapse();
        public static Entity Substitute(this string expr, VariableEntity var, Entity value)
            => expr.ToEntity().Substitute(var, value);
        public static Set SolveEquation(this string expr, VariableEntity x)
            => expr.ToEntity().SolveEquation(x);
        public static RealNumber ToNumber(this EDecimal value) => RealNumber.Create(value);
        public static RealNumber ToNumber(this decimal value) => RealNumber.Create(EDecimal.FromDecimal(value));
        public static RealNumber ToNumber(this double value) => RealNumber.Create(EDecimal.FromDouble(value));
        public static RealNumber ToNumber(this float value) => RealNumber.Create(EDecimal.FromSingle(value));
        public static IntegerNumber ToNumber(this EInteger value) => IntegerNumber.Create(value);
        public static IntegerNumber ToNumber(this int value) => IntegerNumber.Create(value);
        public static IntegerNumber ToNumber(this long value) => IntegerNumber.Create(value);
        public static string Latexise(this string str) => str.ToEntity().Latexise();
        public static FastExpression Compile(this string str, params VariableEntity[] variables)
            => str.ToEntity().Compile(variables);

        // C# can't into templates :(
        /*

        res = ""

        for i in range(2, 10):
            res += "public static Tensor SolveSystem(this ("
            res += ", ".join(["string eq" + str(j) for j in range(1, i + 1)])
            res += ") eqs, "
            res += ", ".join(["string var" + str(j) for j in range(1, i + 1)])
            res += ")\n"
            res += "    => MathS.Equations("
            res += ", ".join(["eqs.eq" + str(j) for j in range(1, i + 1)])
            res += ").Solve("
            res += ", ".join(["var" + str(j) for j in range(1, i + 1)])
            res += ");\n\n"

        print(res)

        */
        public static Tensor SolveSystem(this (string eq1, string eq2) eqs, string var1, string var2)
            => MathS.Equations(eqs.eq1, eqs.eq2).Solve(var1, var2);

        public static Tensor SolveSystem(this (string eq1, string eq2, string eq3) eqs, string var1, string var2, string var3)
            => MathS.Equations(eqs.eq1, eqs.eq2, eqs.eq3).Solve(var1, var2, var3);

        public static Tensor SolveSystem(this (string eq1, string eq2, string eq3, string eq4) eqs, string var1, string var2, string var3, string var4)
            => MathS.Equations(eqs.eq1, eqs.eq2, eqs.eq3, eqs.eq4).Solve(var1, var2, var3, var4);

        public static Tensor SolveSystem(this (string eq1, string eq2, string eq3, string eq4, string eq5) eqs, string var1, string var2, string var3, string var4, string var5)
            => MathS.Equations(eqs.eq1, eqs.eq2, eqs.eq3, eqs.eq4, eqs.eq5).Solve(var1, var2, var3, var4, var5);

        public static Tensor SolveSystem(this (string eq1, string eq2, string eq3, string eq4, string eq5, string eq6) eqs, string var1, string var2, string var3, string var4, string var5, string var6)
            => MathS.Equations(eqs.eq1, eqs.eq2, eqs.eq3, eqs.eq4, eqs.eq5, eqs.eq6).Solve(var1, var2, var3, var4, var5, var6);

        public static Tensor SolveSystem(this (string eq1, string eq2, string eq3, string eq4, string eq5, string eq6, string eq7) eqs, string var1, string var2, string var3, string var4, string var5, string var6, string var7)
            => MathS.Equations(eqs.eq1, eqs.eq2, eqs.eq3, eqs.eq4, eqs.eq5, eqs.eq6, eqs.eq7).Solve(var1, var2, var3, var4, var5, var6, var7);

        public static Tensor SolveSystem(this (string eq1, string eq2, string eq3, string eq4, string eq5, string eq6, string eq7, string eq8) eqs, string var1, string var2, string var3, string var4, string var5, string var6, string var7, string var8)
            => MathS.Equations(eqs.eq1, eqs.eq2, eqs.eq3, eqs.eq4, eqs.eq5, eqs.eq6, eqs.eq7, eqs.eq8).Solve(var1, var2, var3, var4, var5, var6, var7, var8);

        public static Tensor SolveSystem(this (string eq1, string eq2, string eq3, string eq4, string eq5, string eq6, string eq7, string eq8, string eq9) eqs, string var1, string var2, string var3, string var4, string var5, string var6, string var7, string var8, string var9)
            => MathS.Equations(eqs.eq1, eqs.eq2, eqs.eq3, eqs.eq4, eqs.eq5, eqs.eq6, eqs.eq7, eqs.eq8, eqs.eq9).Solve(var1, var2, var3, var4, var5, var6, var7, var8, var9);

        public static Tensor SolveSystem(this Entity[] equations, params VariableEntity[] vars)
            => MathS.Equations(equations).Solve(vars);
    }
}