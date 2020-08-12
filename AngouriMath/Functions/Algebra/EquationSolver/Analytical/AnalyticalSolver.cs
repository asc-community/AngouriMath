
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



using AngouriMath.Core.Exceptions;
using AngouriMath.Core.TreeAnalysis;
using AngouriMath.Functions.Algebra.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using AngouriMath.Core;
using AngouriMath.Core.Numerix;
using AngouriMath.Core.Sys.Interfaces;
using AngouriMath.Functions;
using AngouriMath.Functions.Algebra.Solver.Analytical;

namespace AngouriMath
{
    public abstract partial class Entity : ILatexiseable
    {
        /// <summary>
        /// Attempt to find analytical roots of a custom equation
        /// </summary>
        /// <param name="x"></param>
        /// <returns>
        /// Returns Set. Work with it as with a list
        /// </returns>
        public Set SolveEquation(VariableEntity x) => EquationSolver.Solve(this, x);
    }
}

namespace AngouriMath.Core.TreeAnalysis
{
    internal static partial class TreeAnalyzer
    {
        /// <summary>
        /// Searches for a subtree containing `ent` and being minimal possible size.
        /// For example, for expr = MathS.Sqr(x) + 2 * (MathS.Sqr(x) + 3) the result
        /// will be MathS.Sqr(x) while for MathS.Sqr(x) + x the minimum subtree is x.
        /// Further, it will be used for solving with variable replacing, for example,
        /// there's no pattern for solving equation like sin(x)^2 + sin(x) + 1 = 0,
        /// but we can first solve t^2 + t + 1 = 0, and then root = sin(x).
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        public static Entity GetMinimumSubtree(Entity expr, Entity ent)
        {
            // TODO: this function requires a lot of refactoring

            // The idea is the following:
            // We must get a subtree that has more occurances than 1,
            // But at the same time it should cover all references to `ent`

            bool GoodSub(Entity sub)
            {
                return expr.CountOccurances(sub.ToString()) * sub.CountOccurances(ent.ToString()) == expr.CountOccurances(ent.ToString());
                /* if found subtree contains 3 mentions of `ent` and expression consists of 2 such substress, 
                then number of mentions of `ent` in expression should be 6*/
            }


            int depth = 1;
            Entity subtree;
            Entity? best = null;
            var sub = expr.FindSubtree(ent);
            best = sub?.Parent;
            if (best is null || !GoodSub(best))
                return ent;
            else
                return best;
        }

        private static Entity GetTreeByDepth(Entity expr, Entity ent, int depth)
        {
            while (depth > 0)
            {
                foreach (var child in expr.ChildrenReadonly)
                    // We don't care about the order as once we encounter mention of `ent`,
                    // we need ALL subtrees be equal
                    if (child.SubtreeIsFound(ent))
                    {
                        expr = child;
                        break;
                    }
                depth -= 1;
                if (expr == ent)
                    return expr;
            }
            return expr;
        }

        /// <summary>
        /// Func MUST contain exactly ONE occurance of x,
        /// otherwise it won't work correctly
        /// </summary>
        /// <param name="func"></param>
        /// <param name="value"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Set FindInvertExpression(Entity func, Entity value, Entity x)
        {
            value = value.InnerSimplify();
            if (func == x)
                return new Set(value);
            if (func is NumberEntity)
                throw new MathSException("This function must contain x");
            if (func is VariableEntity)
                return new Set(func);
            if (func is OperatorEntity o)
                return InvertOperatorEntity(o, value, x);
            if (func is FunctionEntity f)
                return InvertFunctionEntity(f, value, x);

            return new Set(value);
        }

        /// <summary>
        /// Inverts operator and returns a set
        /// x^2 = a
        /// => x = sqrt(a)
        /// x = -sqrt(a)
        /// </summary>
        /// <param name="func"></param>
        /// <param name="value"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Set InvertOperatorEntity(OperatorEntity func, Entity value, Entity x)
        {
            Entity a, un;
            int arg;
            if (func.GetChild(0).SubtreeIsFound(x))
            {
                a = func.GetChild(1);
                un = func.GetChild(0);
                arg = 0;
            }
            else
            {
                a = func.GetChild(0);
                un = func.GetChild(1);
                arg = 1;
            }
            var n = Utils.FindNextIndex(func + value, "n");
            switch (func.Name)
            {
                case "sumf":
                    // x + a = value => x = value - a
                    return FindInvertExpression(un, value - a, x);
                case "minusf":
                    if (arg == 0)
                        // x - a = value => x = value + a
                        return FindInvertExpression(un, value + a, x);
                    else
                        // a - x = value => x = a - value
                        return FindInvertExpression(un, a - value, x);
                case "mulf":
                    // x * a = value => x = value / a
                    return FindInvertExpression(un, value / a, x);
                case "divf":
                    if (arg == 0)
                        // x / a = value => x = a * value
                        return FindInvertExpression(un, value * a, x);
                    else
                        // a / x = value => x = a / value
                        return FindInvertExpression(un, a / value, x);
                case "powf":
                    if (arg == 0)
                    {
                        // x ^ a = value => x = value ^ (1/a)
                        if (a is NumberEntity { Value:IntegerNumber { Value: var pow } })
                        {
                            var res = new Set();
                            foreach (var root in Number.GetAllRoots(1, pow).FiniteSet())
                                res.AddRange(FindInvertExpression(un, root * MathS.Pow(value, 1 / a), x));
                            return res;
                        }
                        else
                            return FindInvertExpression(un, MathS.Pow(value, 1 / a), x);
                    }
                    else
                        // a ^ x = value => x = log(a, value)
                        return FindInvertExpression(un, MathS.Log(a, value) + 2 * MathS.i * n * MathS.pi, x);
                default:
                    throw new UnknownOperatorException();
            }
        }

        /// <summary>
        /// Returns true if a is inside a rect with corners from and to,
        /// OR a is an unevaluable expression
        /// </summary>
        /// <param name="a"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private static bool EntityInBounds(Entity a, ComplexNumber from, ComplexNumber to)
        {
            if (!MathS.CanBeEvaluated(a))
                return true;
            var r = a.Eval();
            return r.Real >= from.Real &&
                   r.Imaginary >= from.Imaginary &&
                   r.Real <= to.Real &&
                   r.Imaginary <= to.Imaginary;
        }

        private static readonly ComplexNumber ArcsinFrom = ComplexNumber.Create(-MathS.DecimalConst.pi / 2, RealNumber.NegativeInfinity.Value);
        private static readonly ComplexNumber ArcsinTo = ComplexNumber.Create(MathS.DecimalConst.pi / 2, RealNumber.PositiveInfinity.Value);
        private static readonly ComplexNumber ArccosFrom = ComplexNumber.Create(0, RealNumber.NegativeInfinity.Value);
        private static readonly ComplexNumber ArccosTo = ComplexNumber.Create(MathS.DecimalConst.pi, RealNumber.PositiveInfinity.Value);
        private static readonly Set Empty = new Set();

        /// <summary>
        /// Returns a set of possible roots of a function, e. g.
        /// sin(x) = a =>
        /// x = arcsin(a) + 2 pi n
        /// x = pi - arcsin(a) + 2 pi n
        /// </summary>
        /// <param name="func"></param>
        /// <param name="value"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Set InvertFunctionEntity(FunctionEntity func, Entity value, Entity x)
        {
            Entity a = func.GetChild(0);
            int arg = func.ChildrenCount == 2 && func.GetChild(1).SubtreeIsFound(x) ? 1 : 0;
            var n = Utils.FindNextIndex(func + value, "n");
            var res = new Set();
            var pi = MathS.pi;

            static Set GetNotNullEntites(Set set)
            {
                return set.FiniteWhere(el => !(el is NumberEntity e) || e.Value.IsFinite);
            }

            switch (func.Name)
            {
                // Consider case when sin(sin(x)) where double-mention of n occures
                case "sinf":
                    {
                        // sin(x) = value => x = arcsin(value) + 2pi * n
                        res.AddRange(GetNotNullEntites(FindInvertExpression(a, MathS.Arcsin(value) + 2 * pi * n, x)));
                        // sin(x) = value => x = pi - arcsin(value) + 2pi * n
                        res.AddRange(GetNotNullEntites(FindInvertExpression(a, pi - MathS.Arcsin(value) + 2 * pi * n, x)));
                        return res;
                    }
                case "cosf":
                    {
                        // cos(x) = value => x = arccos(value) + 2pi * n
                        res.AddRange(GetNotNullEntites(FindInvertExpression(a, MathS.Arccos(value) + 2 * pi * n, x)));
                        // cos(x) = value => x = -arccos(value) + 2pi * n
                        res.AddRange(GetNotNullEntites(FindInvertExpression(a, -MathS.Arccos(value) - 2 * pi * n, x)));
                        return res;
                    }
                case "tanf":
                    {
                        var inverted = FindInvertExpression(a, MathS.Arctan(value) + pi * n, x);
                        // tan(x) = value => x = arctan(value) + pi * n
                        res.AddRange(GetNotNullEntites(inverted));
                        return res;
                    }
                case "cotanf":
                    {
                        var inverted = FindInvertExpression(a, MathS.Arccotan(value) + pi * n, x);
                        // cotan(x) = value => x = arccotan(value)
                        res.AddRange(GetNotNullEntites(inverted));
                        return res;
                    }
                case "arcsinf":
                    // arcsin(x) = value => x = sin(value)
                    if (EntityInBounds(value, ArcsinFrom, ArcsinTo))
                        return GetNotNullEntites(FindInvertExpression(a, MathS.Sin(value), x));
                    else
                        return Empty;
                case "arccosf":
                    // arccos(x) = value => x = cos(value)
                    if (EntityInBounds(value, ArccosFrom, ArccosTo))
                        return GetNotNullEntites(FindInvertExpression(a, MathS.Cos(value), x));
                    else
                        return Empty;
                case "arctanf":
                    // arctan(x) = value => x = tan(value)
                    return GetNotNullEntites(FindInvertExpression(a, MathS.Tan(value), x));
                case "arccotanf":
                    // arccotan(x) = value => x = cotan(value)
                    return GetNotNullEntites(FindInvertExpression(a, MathS.Cotan(value), x));
                case "logf":
                    Entity b = func.GetChild(1);
                    if (arg != 0)
                        // log(x, a) = value => x = a ^ value
                        return GetNotNullEntites(FindInvertExpression(b, MathS.Pow(a, value), x));
                    else
                        // log(a, x) = value => a = x ^ value => x = a ^ (1 / value)
                        return GetNotNullEntites(FindInvertExpression(a, MathS.Pow(b, 1 / value), x));
                default:
                    throw new UnknownFunctionException();
            }
        }
    }
}

namespace AngouriMath.Functions.Algebra.AnalyticalSolving
{
    internal static class AnalyticalSolver
    {
        private static Entity TryDowncast(Entity equation, VariableEntity x, Entity root)
        {
            if (!MathS.CanBeEvaluated(root))
                return root;
            var preciseValue = root.Eval();
            var downcasted = MathS.Settings.FloatToRationalIterCount.As(20, () =>
                MathS.Settings.PrecisionErrorZeroRange.As(1e-7m, () =>
                    ComplexNumber.Create(preciseValue.Real, preciseValue.Imaginary)));
            var errorExpr = equation.Substitute(x, downcasted);
            if (!MathS.CanBeEvaluated(errorExpr))
                return root;
            var error = errorExpr.Eval();

            static bool ComplexRational(ComplexNumber a)
                => a.Real is RationalNumber && a.Imaginary is RationalNumber;

            var innerSimplified = root.InnerSimplify();

            return Number.IsZero(error) && ComplexRational(downcasted) ? downcasted : innerSimplified;
        }


        /// <summary>
        /// Equation solver
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="x"></param>
        /// <param name="dst"></param>
        internal static void Solve(Entity expr, VariableEntity x, Set dst)
            => Solve(expr, x, dst, compensateSolving: false);
        internal static void Solve(Entity expr, VariableEntity x, Set dst, bool compensateSolving)
        {
            if (expr == x)
            {
                dst.Add(0);
                return;
            }

            // Applies an attempt to downcast roots
            void DestinationAddRange(Set toAdd)
            {
                toAdd.FiniteApply(ent => TryDowncast(expr, x, ent));
                dst.AddRange(toAdd);
            }

            Set? res = PolynomialSolver.SolveAsPolynomial(expr, x);
            if (res is { })
            {
                res.FiniteApply(e => e.InnerSimplify());
                DestinationAddRange(res);
                return;
            }

            if (expr is OperatorEntity)
            {
                switch (expr.Name)
                {
                    case "mulf":
                        Solve(expr.GetChild(0), x, dst);
                        Solve(expr.GetChild(1), x, dst);
                        return;
                    case "divf":

                        static bool IsSetNumeric(Set a)
                            => a.Select(piece => piece.LowerBound().Item1).All(MathS.CanBeEvaluated);

                        var zeroNumerators = new Set();
                        Solve(expr.GetChild(0), x, zeroNumerators);
                        if (!IsSetNumeric(zeroNumerators))
                        {
                            dst.AddRange(zeroNumerators);
                            return;
                        }
                        var zeroDenominators = new Set();
                        Solve(expr.GetChild(1), x, zeroDenominators);
                        if (!IsSetNumeric(zeroDenominators))
                        {
                            dst.AddRange(zeroNumerators);
                            return;
                        }
                        dst.AddRange((Set)(zeroNumerators & !zeroDenominators));
                        return;
                    case "powf":
                        Solve(expr.GetChild(0), x, dst);
                        return;
                    case "minusf":
                        if (!expr.GetChild(1).SubtreeIsFound(x) && compensateSolving)
                        {
                            if (expr.GetChild(0) == x)
                            {
                                dst.Add(expr.GetChild(1));
                                return;
                            }
                            var subs = 0;
                            Entity? lastChild = null;
                            foreach (var child in expr.GetChild(0).ChildrenReadonly)
                            {
                                if (child.SubtreeIsFound(x))
                                {
                                    subs += 1;
                                    lastChild = child;
                                }
                            }
                            if (subs != 1 || lastChild is null)
                                break;
                            var resInverted = TreeAnalyzer.FindInvertExpression(expr.GetChild(0), expr.GetChild(1), lastChild);
                            foreach (var result in resInverted.FiniteSet())
                                Solve(lastChild - result, x, dst, compensateSolving: true);
                            return;
                        }
                        break;
                }
            }
            else if (expr is FunctionEntity f)
            {
                DestinationAddRange(TreeAnalyzer.InvertFunctionEntity(f, 0, x));
                return;
            }

            // if the replacement isn't one-variable one,
            // then solving over replacements is already useless,
            // so we skip this part and go to other solvers
            if (!compensateSolving)
            {
                // Here we generate a unique variable name
                var uniqVars = MathS.Utils.GetUniqueVariables(expr);
                uniqVars.Pieces.Sort((a, b) => ((Entity)b).Name.Length.CompareTo(((Entity)a).Name.Length));
                VariableEntity newVar = ((Entity)uniqVars.Pieces[0]).Name + "quack";
                // // //


                // Here we find all possible replacements
                var replacements = new List<(Entity, Entity)>
                {
                    (TreeAnalyzer.GetMinimumSubtree(expr, x), expr)
                };
                foreach (var alt in expr.Alternate(4).FiniteSet())
                {
                    if (!alt.SubtreeIsFound(x))
                        return; // in this case there is either 0 or +oo solutions
                    replacements.Add((TreeAnalyzer.GetMinimumSubtree(alt, x), alt));
                }
                // // //

                // Here we find one that has at least one solution

                foreach (var replacement in replacements)
                {
                    Set? solutions = null;
                    if (replacement.Item1 == x)
                        continue;
                    var newExpr = replacement.Item2.DeepCopy();
                    TreeAnalyzer.FindAndReplace(ref newExpr, replacement.Item1, newVar);
                    solutions = newExpr.SolveEquation(newVar);
                    if (!solutions.IsEmpty())
                    {
                        var bestReplacement = replacement.Item1;

                        // Here we are trying to solve for this replacement
                        Set newDst = new Set();
                        foreach (var solution in solutions.FiniteSet())
                        {
                            var str = bestReplacement.ToString();
                            // TODO: make a smarter comparison than just comparison of complexities of two expressions
                            // The idea is  
                            // similarToPrevious = ((bestReplacement - solution) - expr).Simplify() == 0
                            // But Simplify costs us too much time
                            var similarToPrevious = (bestReplacement - solution).Complexity() >= expr.Complexity();
                            if (!compensateSolving || !similarToPrevious)
                                Solve(bestReplacement - solution, x, newDst, compensateSolving: true);
                        }
                        DestinationAddRange(newDst);
                        if (!dst.IsEmpty())
                            break;
                        // // //
                    }
                }
                // // //
            }

            // if no replacement worked, try trigonometry solver
            if (dst.IsEmpty())
            {
                var trigexpr = expr.DeepCopy();
                res = TrigonometricSolver.SolveLinear(trigexpr, x);
                if (res != null)
                {
                    DestinationAddRange(res);
                    return;
                }
            }
            // // //

            // if no trigonometric rules helped, common denominator might help
            if (dst.IsEmpty())
            {
                res = CommonDenominatorSolver.Solve(expr, x);
                if (res != null)
                {
                    DestinationAddRange(res);
                    return;
                }
            }
            // // //


            // if we have fractioned polynomials
            if (dst.IsEmpty())
            {
                res = FractionedPolynoms.Solve(expr, x);
                if (res != null)
                {
                    DestinationAddRange(res);
                    return;
                }
            }
            // // //

            // TODO: Solve factorials (Needs Lambert W function)
            // https://mathoverflow.net/a/28977

            // if nothing has been found so far
            if (dst.IsEmpty() && MathS.Settings.AllowNewton)
            {
                Set allVars = new Set();
                TreeAnalyzer._GetUniqueVariables(expr, allVars);
                if (allVars.Count == 1)
                    DestinationAddRange(expr.SolveNt(x));
            }
        }
    }
}