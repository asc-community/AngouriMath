
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
using System.Linq;
using System.Text;
using AngouriMath.Convenience;
using AngouriMath.Core;
using AngouriMath.Core.TreeAnalysis;

namespace AngouriMath.Core.TreeAnalysis
{
    internal static partial class TreeAnalyzer
    {
        /// <summary>
        /// That is realized SO badly...
        /// TODO
        /// </summary>
        /// <typeparam name="T"></typeparam>
        internal interface IPrimitive<T>
        {
            void Add(T a);
            void AddMp(T a, Number b);
            void Assign(T val);
            T GetValue();
        }
        internal class PrimitiveDouble : IPrimitive<double>
        {
            private double value = 0;
            public void Add(double a) => value += a;
            public void AddMp(double a, Number b) => Add(a * b.Re);
            public void Assign(double a) => value = a;
            public static implicit operator double(PrimitiveDouble obj) => obj.value;
            internal static IPrimitive<double> Create()
            {
                return new PrimitiveDouble();
            }
            public double GetValue() => value;
        }
        internal class PrimitiveInt : IPrimitive<int>
        {
            private int value = 0;
            public void Add(int a) => value += a;
            public void AddMp(int a, Number b) => Add((int)(a * b.Re));
            public void Assign(int a) => value = a;
            public static implicit operator int(PrimitiveInt obj) => obj.value;
            internal static IPrimitive<int> Create()
            {
                return new PrimitiveInt();
            }
            public int GetValue() => value;
        }
    }
}

namespace AngouriMath.Functions.Algebra.AnalyticalSolving
{
    /// <summary>
    /// Solves all forms of Polynomials that are trivially solved
    /// </summary>
    internal static class PolynomialSolver
    {
        /// <summary>
        /// Solves ax + b
        /// </summary>
        /// <param name="a">
        /// Coefficient of x
        /// </param>
        /// <param name="b">
        /// Free coefficient
        /// </param>
        /// <returns>
        /// Set of roots
        /// </returns>
        internal static Set SolveLinear(Entity a, Entity b)
        {
            // ax + b = 0
            // ax = -b
            // x = -b / a
            return new Set(-b / a);
        }

        /// <summary>
        /// solves ax2 + bx + c
        /// </summary>
        /// <param name="a">
        /// Coefficient of x^2
        /// </param>
        /// <param name="b">
        /// Coefficient of x
        /// </param>
        /// <param name="c">
        /// Free coefficient
        /// </param>
        /// <returns>
        /// Set of roots
        /// </returns>
        internal static Set SolveQuadratic(Entity a, Entity b, Entity c)
        {
            Set res;
            if (TreeAnalyzer.IsZero(c))
            {
                res = SolveLinear(a, b);
                res.Add(0);
                return res;
            }

            if (TreeAnalyzer.IsZero(a))
                return SolveLinear(b, c);

            res = new Set();
            var D = MathS.Sqr(b) - 4 * a * c;
            res.Add((-b - MathS.Sqrt(D)) / (2 * a));
            res.Add((-b + MathS.Sqrt(D)) / (2 * a));
            return res;
        }

        /// <summary>
        /// solves ax3 + bx2 + cx + d
        /// </summary>
        /// <param name="a">
        /// Coefficient of x^3
        /// </param>
        /// <param name="b">
        /// Coefficient of x^2
        /// </param>
        /// <param name="c">
        /// Coefficient of x
        /// </param>
        /// <param name="d">
        /// Free coefficient
        /// </param>
        /// <returns>
        /// Set of roots
        /// </returns>
        internal static Set SolveCubic(Entity a, Entity b, Entity c, Entity d)
        {
            // en: https://en.wikipedia.org/wiki/Cubic_equation
            // ru: https://ru.wikipedia.org/wiki/%D0%A4%D0%BE%D1%80%D0%BC%D1%83%D0%BB%D0%B0_%D0%9A%D0%B0%D1%80%D0%B4%D0%B0%D0%BD%D0%BE

            // TODO (to remove sympy code!)

            Set res;

            if (TreeAnalyzer.IsZero(d))
            {
                res = SolveQuadratic(a, b, c);
                res.Add(0);
                return res;
            }

            if (TreeAnalyzer.IsZero(a))
                return SolveQuadratic(b, c, d);

            res = new Set();

            if (MathS.CanBeEvaluated(a) &&
                MathS.CanBeEvaluated(b) &&
                MathS.CanBeEvaluated(c) &&
                MathS.CanBeEvaluated(d))
            {
                var p = ((3 * a * c - MathS.Sqr(b)) / (3 * MathS.Sqr(a))).Simplify();
                var q = ((2 * MathS.Pow(b, 3) - 9 * a * b * c + 27 * MathS.Sqr(a) * d) / (27 * MathS.Pow(a, 3)));

                var Q = MathS.Pow(p / 3, 3) + MathS.Sqr(q / 2);

                var alpha = (MathS.Pow(-q / 2 + MathS.Sqrt(Q), 1.0 / 3.0));
                var beta = (MathS.Pow(-q / 2 - MathS.Sqrt(Q), 1.0 / 3.0));

                // To find correct beta, you should find such beta that alpha + beta = -p / 3
                // Such beta always exists
                if (p.entType == Entity.EntType.NUMBER && 
                    MathS.CanBeEvaluated(alpha) &&
                    MathS.CanBeEvaluated(beta))
                {
                    var beta3 = (-q / 2 - MathS.Sqrt(Q)).Eval();
                    alpha = alpha.Eval();
                    
                    beta = beta.Eval();
                    var p3 = (-p / 3).Eval();
                    foreach (var root in Number.GetAllRoots(beta3, 3).FiniteSet())
                        if ((root * alpha).Eval() == p3)
                        {
                            beta = root;
                            break;
                        }
                }
                var y1 = alpha + beta;
                var y2 = (-(alpha + beta) / 2 + MathS.i * (alpha - beta) / 2 * MathS.Sqrt(3));
                var y3 = (-(alpha + beta) / 2 - MathS.i * (alpha - beta) / 2 * MathS.Sqrt(3));

                var x1 = y1 - b / (3 * a);
                var x2 = y2 - b / (3 * a);
                var x3 = y3 - b / (3 * a);

                res.Add(x1);
                res.Add(x2);
                res.Add(x3);

                return res;
            }
            else
            {
                // TO REMOVE
                var coeff = MathS.i * MathS.Sqrt(3) / 2;

                var u1 = new NumberEntity(1);
                var u2 = SySyn.Rational(-1, 2) + coeff;
                var u3 = SySyn.Rational(-1, 2) - coeff;
                var D0 = MathS.Sqr(b) - 3 * a * c;
                var D1 = 2 * MathS.Pow(b, 3) - 9 * a * b * c + 27 * MathS.Sqr(a) * d;
                var C = MathS.Pow((D1 + MathS.Sqrt(MathS.Sqr(D1) - 4 * MathS.Pow(D0, 3))) / 2, 1.0 / 3);

                foreach (var uk in new List<Entity> { u1, u2, u3 })
                    res.Add(-(b + uk * C + D0 / C / uk) / 3 / a);
                return res;
            }
        }

        /// <summary>
        /// solves ax4 + bx3 + cx2 + dx + e
        /// </summary>
        /// <param name="a">
        /// Coefficient of x^4
        /// </param>
        /// <param name="b">
        /// Coefficient of x^3
        /// </param>
        /// <param name="c">
        /// Coefficient of x^2
        /// </param>
        /// <param name="d">
        /// Coefficient of x
        /// </param>
        /// <param name="e">
        /// Free coefficient
        /// </param>
        /// <returns>
        /// Set of roots
        /// </returns>
        internal static Set SolveQuartic(Entity a, Entity b, Entity c, Entity d, Entity e)
        {
            // en: https://en.wikipedia.org/wiki/Quartic_function
            // ru: https://ru.wikipedia.org/wiki/%D0%9C%D0%B5%D1%82%D0%BE%D0%B4_%D0%A4%D0%B5%D1%80%D1%80%D0%B0%D1%80%D0%B8

            Set res;

            if (TreeAnalyzer.IsZero(e))
            {
                res = SolveCubic(a, b, c, d);
                res.Add(0);
                return res;
            }

            if (TreeAnalyzer.IsZero(a))
                return SolveCubic(b, c, d, e);

            
            res = new Set();

            var alpha = -3 * MathS.Sqr(b) / (8 * MathS.Sqr(a)) + c / a;
            var beta = MathS.Pow(b, 3) / (8 * MathS.Pow(a, 3)) - (b * c) / (2 * MathS.Sqr(a)) + d / a;
            var gamma = -3 * MathS.Pow(b, 4) / (256 * MathS.Pow(a, 4)) + MathS.Sqr(b) * c / (16 * MathS.Pow(a, 3)) - (b * d) / (4 * MathS.Sqr(a)) + e / a;

            if (MathS.CanBeEvaluated(beta))
                beta = beta.Eval();
            if (beta == 0)
            {
                res.FastAddingMode = true;
                for (int s = -1; s <= 1; s += 2)
                for (int t = -1; t <= 1; t += 2)
                {
                    var x = -b / 4 * a + s * MathS.Sqrt((-alpha + t * MathS.Sqrt(MathS.Sqr(alpha) - 4 * gamma)) / 2);
                    res.Add(x);
                }
                res.FastAddingMode = false;
                return res;
            }

            

            var P = -MathS.Sqr(alpha) / 12 - gamma;
            var Q = -MathS.Pow(alpha, 3) / 108 + alpha * gamma / 3 - MathS.Sqr(beta) / 8;
            var R = -Q / 2 + MathS.Sqrt(MathS.Sqr(Q) / 4 + MathS.Pow(P, 3) / 27);
            var U = MathS.Pow(R, 1.0 / 3);
            if (MathS.CanBeEvaluated(U))
                U = U.Eval();  // further, we will compare it to 0
            var y = SySyn.Rational(-5, 6) * alpha + U + (U == 0 ? -MathS.Pow(Q, 1.0 / 3) : -P / (3 * U));
            var W = MathS.Sqrt(alpha + 2 * y);
           
            // Now we need to permutate all four combinations
            res.FastAddingMode = true;  /* we are sure that there's no such root yet */
            for (int s = -1; s <= 1; s += 2)
                for (int t = -1; t <= 1; t += 2)
                {
                    var x = -b / (4 * a) + (s * W + t * MathS.Sqrt(-(3 * alpha + 2 * y + s * 2 * beta / W))) / 2;
                    res.Add(x);
                }
            res.FastAddingMode = false;
            return res;
        }

        /// <summary>
        /// So that the final list of powers contains power = 0 and all powers >= 0
        /// (e. g. if the dictionaty's keys are 3, 4, 6, the final answer will contain keys
        /// 0, 1, 3, if the dictionary's keys are -2, 0, 3, the final answer will contain keys
        /// 0, 2, 5)
        /// </summary>
        /// <param name="monomials">
        /// Dictionary to process. Key - power, value - coefficient of the corresponding term
        /// </param>
        /// <returns>
        /// Returns whether all initiall powers where > 0 (if so, x = 0 is a root)
        /// </returns>
        internal static bool ReduceCommonPower(ref Dictionary<int, Entity> monomials)
        {
            int commonPower = monomials.Keys.Min();
            if (commonPower == 0)
                return false;
            var newDict = new Dictionary<int, Entity>();
            foreach (var pair in monomials)
                newDict[pair.Key - commonPower] = pair.Value;
            monomials = newDict;
            return commonPower > 0;
        }

        /// <summary>
        /// Tries to solve as polynomial
        /// </summary>
        /// <param name="expr">
        /// Polynomial of an expression
        /// </param>
        /// <param name="subtree">
        /// The expression the polynomial of (e. g. cos(x)^2 + cos(x) + 1 is a polynomial of cos(x))
        /// </param>
        /// <returns>
        /// a finite Set if successful,
        /// null otherwise
        /// </returns>
        internal static Set SolveAsPolynomial(Entity expr, Entity subtree)
        {
            // Here we find all terms
            expr = expr.Expand(); // (x + 1) * x => x^2 + x
            List<Entity> children;
            Set res = new Set();
            if (expr.entType == Entity.EntType.OPERATOR && expr.Name == "sumf" || expr.Name == "minusf")
                children = TreeAnalyzer.LinearChildren(expr, "sumf", "minusf", Const.FuncIfSum);
            else
                children = new List<Entity> { expr };
            // Check if all are like {1} * x^n & gather information about them
            var monomialsByPower = GatherMonomialInformation<int>(children, subtree);

            if (monomialsByPower == null)
                return null; // meaning that the given equation is not polynomial

            Entity GetMonomialByPower(int power)
            {
                return monomialsByPower.ContainsKey(power) ? monomialsByPower[power] : 0;
            }
            if (ReduceCommonPower(ref monomialsByPower)) // x5 + x3 + x2 - common power is 2, one root is 0, then x3 + x + 1
                res.Add(0);
            var powers = new List<int>(monomialsByPower.Keys);
            var gcdPower = Utils.GCD(powers.ToArray());
            // // //



            // Change all replacements, x6 + x3 + 1 => x2 + x + 1
            if (gcdPower != 1)
            {
                for (int i = 0; i < powers.Count; i++)
                    powers[i] /= gcdPower;

                var newMonom = new Dictionary<int, Entity>();
                foreach (var pair in monomialsByPower)
                    newMonom[pair.Key / gcdPower] = pair.Value;
                monomialsByPower = newMonom;
            }
            // // //



            // if we had x^6 + x^3 + 1, we replace it with x^2 + x + 1 and find all cubic roots of the final answer
            Set FinalPostProcess(Set set)
            {
                if (gcdPower != 1)
                {
                    var newSet = new Set();
                    foreach (var root in set.FiniteSet())
                    foreach (var coef in Number.GetAllRoots(1, gcdPower).FiniteSet())
                        newSet.Add(coef * MathS.Pow(root, MathS.Num(1.0) / gcdPower));
                    set = newSet;
                }
                return set;
            }

            if (powers.Count == 0)
                return null;
            powers.Sort();
            if (powers.Last() == 0)
                return FinalPostProcess(res);
            if (powers.Last() > 4 && powers.Count > 2)
                return null; // So far, we can't solve equations of powers more than 4
            if (powers.Count == 1)
            {
                res.Add(0);
                return FinalPostProcess(res);
            }
            else if (powers.Count == 2)
            {
                // Provided a x ^ n + b = 0
                // a = -b x ^ n
                // (- a / b) ^ (1 / n) = x
                // x ^ n = (-a / b)
                res.AddRange(TreeAnalyzer.FindInvertExpression(MathS.Pow(subtree, powers[1]), (-1 * monomialsByPower[powers[0]] / monomialsByPower[powers[1]]).Simplify(), subtree));
                return FinalPostProcess(res);
            }
            // By this moment we know for sure that expr's power is <= 4, that expr is not a monomial,
            // and that it consists of more than 2 monomials
            else if (powers.Last() == 2)
            {
                var a = GetMonomialByPower(2);
                var b = GetMonomialByPower(1);
                var c = GetMonomialByPower(0);

                res.AddRange(SolveQuadratic(a, b, c));
                return FinalPostProcess(res);
            }
            else if (powers.Last() == 3)
            {
                var a = GetMonomialByPower(3);
                var b = GetMonomialByPower(2);
                var c = GetMonomialByPower(1);
                var d = GetMonomialByPower(0);

                res.AddRange(SolveCubic(a, b, c, d));
                return FinalPostProcess(res);
            }
            else if (powers.Last() == 4)
            {
                var a = GetMonomialByPower(4);
                var b = GetMonomialByPower(3);
                var c = GetMonomialByPower(2);
                var d = GetMonomialByPower(1);
                var e = GetMonomialByPower(0);

                res.AddRange(SolveQuartic(a, b, c, d, e));
                return FinalPostProcess(res);
            }
            return null;
        }
    
        /// <summary>
        /// Finds all terms of a polynomial
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="terms"></param>
        /// <param name="subtree"></param>
        /// <returns></returns>
        internal static Dictionary<T, Entity> GatherMonomialInformation<T>(List<Entity> terms, Entity subtree)
        {
            terms = terms.Select(t => t.Collapse().InnerSimplify()).ToList();

            var monomialsByPower = new Dictionary<T, Entity>();
            // here we fill the dictionary with information about monomials' coefficiants
            foreach (var child in terms)
            {
                // TODO
                Entity free;
                object pow;
                if (typeof(T) == typeof(double))
                    pow = new TreeAnalyzer.PrimitiveDouble();
                else
                    pow = new TreeAnalyzer.PrimitiveInt();
                
                TreeAnalyzer.IPrimitive<T> q = pow as TreeAnalyzer.IPrimitive<T>;
                ParseMonomial<T>(subtree, child, out free, ref q);
                if (free == null)
                    return null;
                if (!monomialsByPower.ContainsKey(q.GetValue()))
                    monomialsByPower[q.GetValue()] = 0;
                monomialsByPower[q.GetValue()] += free;
            }
            // TODO: do we need to simplify all values of monomialsByPower?
            return monomialsByPower;
        }


        internal static void ParseMonomial<T>(Entity aVar, Entity expr, out Entity freeMono, ref TreeAnalyzer.IPrimitive<T> power)
        {
            if (expr.FindSubtree(aVar) == null)
            {
                freeMono = expr;
                return;
            }

            freeMono = 1; // a * b

            bool allowFloat = typeof(T) == typeof(double);
            foreach (var mp in TreeAnalyzer.LinearChildren(expr, "mulf", "divf", Const.FuncIfMul))
                if (mp.entType == Entity.EntType.OPERATOR &&
                    mp.Name == "powf")
                {
                    // x ^ a is bad
                    if (mp.Children[1].entType != Entity.EntType.NUMBER)
                    {
                        freeMono = null;
                        return;
                    }

                    // x ^ 0.3 is bad
                    if (!allowFloat && !mp.Children[1].GetValue().IsInteger())
                    {
                        freeMono = null;
                        return;
                    }

                    if (mp == aVar)
                    {
                        if (allowFloat)
                            (power as TreeAnalyzer.PrimitiveDouble).Add(mp.Children[1].GetValue().Re);
                        else
                            (power as TreeAnalyzer.PrimitiveInt).Add((int)Math.Round(mp.Children[1].GetValue().Re));
                    }
                    else
                    {
                        if (!MathS.CanBeEvaluated(mp.Children[1]))
                        {
                            freeMono = null;
                            return;
                        }
                        Entity tmpFree;
                        // TODO
                        object pow;
                        if (typeof(T) == typeof(double))
                            pow = new TreeAnalyzer.PrimitiveDouble();
                        else
                            pow = new TreeAnalyzer.PrimitiveInt();
                        TreeAnalyzer.IPrimitive<T> q = pow as TreeAnalyzer.IPrimitive<T>;
                        ParseMonomial<T>(aVar, mp.Children[0], out tmpFree, ref q);
                        if (tmpFree == null)
                        {
                            freeMono = null;
                            return;
                        }
                        else
                        {
                            // Can we eval it right here?
                            mp.Children[1] = mp.Children[1].Eval();
                            freeMono *= MathS.Pow(tmpFree, mp.Children[1]);
                            power.AddMp(q.GetValue(), mp.Children[1].GetValue());
                        }
                    }
                }
                else if (mp == aVar)
                {
                    if (allowFloat)
                        (power as TreeAnalyzer.PrimitiveDouble).Add(1);
                    else
                        (power as TreeAnalyzer.PrimitiveInt).Add(1);
                }
                else
                {
                    // a ^ x, (a + x) etc. are bad
                    if (mp.FindSubtree(aVar) != null)
                    {
                        freeMono = null;
                        return;
                    }
                    freeMono *= mp;
                }
            // TODO: do we need to simplify freeMono?
        }
    }
}
