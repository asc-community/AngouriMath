
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



﻿using AngouriMath.Core.Exceptions;
using AngouriMath.Core.TreeAnalysis;
using AngouriMath.Functions.Algebra.AnalyticalSolving;
using AngouriMath.Functions.Algebra.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
 using AngouriMath.Core.Sys.Interfaces;

namespace AngouriMath
{
    public abstract partial class Entity : ILatexiseable
    {
        /// <summary>
        /// Attempt to find analytical roots of a custom equation
        /// </summary>
        /// <param name="x"></param>
        /// <returns>
        /// Returns EntitySet. Work with it as with a list
        /// </returns>
        [ObsoleteAttribute("This method will soon be deprecated. Use SolveEquation instead.")]
        public EntitySet Solve(VariableEntity x) => EquationSolver.Solve(this, x);

        /// <summary>
        /// Attempt to find analytical roots of a custom equation
        /// </summary>
        /// <param name="x"></param>
        /// <returns>
        /// Returns EntitySet. Work with it as with a list
        /// </returns>
        public EntitySet SolveEquation(VariableEntity x) => EquationSolver.Solve(this, x);
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
            Entity best = null;
            while ((subtree = GetTreeByDepth(expr, ent, depth)) != ent)
            {
                if (subtree.Children.Count == 0) return subtree;

                depth++;
                if (GoodSub(subtree))
                    best = subtree;
            }
            return best ?? ent;
        }
        
        private static Entity GetTreeByDepth(Entity expr, Entity ent, int depth)
        {
            while(depth > 0)
            {
                foreach (var child in expr.Children)
                    // We don't care about the order as once we encounter mention of `ent`,
                    // we need ALL subtrees be equal
                    if (child.FindSubtree(ent) != null)
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
        public static EntitySet FindInvertExpression(Entity func, Entity value, Entity x)
        {
            value = MathS.CanBeEvaluated(value) ? value.Eval() : value;
            if (func == x)
                return new EntitySet(value);
            if (func.entType == Entity.EntType.NUMBER)
                throw new MathSException("This function must contain x");
            if (func.entType == Entity.EntType.VARIABLE)
                return new EntitySet(func);
            if (func.entType == Entity.EntType.OPERATOR)
                return InvertOperatorEntity(func as OperatorEntity, value, x);
            if (func.entType == Entity.EntType.FUNCTION)
                return InvertFunctionEntity(func as FunctionEntity, value, x);

            return new EntitySet(value);
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
        public static EntitySet InvertOperatorEntity(OperatorEntity func, Entity value, Entity x)
        {
            Entity a, un;
            int arg;
            if (func.Children[0].FindSubtree(x) != null)
            {
                a = func.Children[1];
                un = func.Children[0];
                arg = 0;
            }
            else
            {
                a = func.Children[0];
                un = func.Children[1];
                arg = 1;
            }
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
                        if (a.entType == Entity.EntType.NUMBER && a.GetValue().IsInteger())
                        {
                            var res = new EntitySet();
                            foreach (var root in Number.GetAllRoots(1, (int)(a.GetValue().Re)))
                                res.AddRange(FindInvertExpression(un, root * MathS.Pow(value, 1 / a), x));
                            return res;
                        }
                        else
                            return FindInvertExpression(un, MathS.Pow(value, 1 / a), x);
                    }
                    else
                        // a ^ x = value => x = log(value, a)
                        return FindInvertExpression(un, MathS.Log(value, a), x);
                default:
                    throw new SysException("Unknown operator");
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
        private static bool EntityInBounds(Entity a, Number from, Number to)
        {
            if (!MathS.CanBeEvaluated(a))
                return true;
            var r = a.Eval();
            return r.Re >= from.Re &&
                   r.Im >= from.Im &&
                   r.Re <= to.Re &&
                   r.Im <= to.Im;
        }

        private static readonly Number ArcsinFrom = new Number(-Math.PI / 2, -double.MaxValue);
        private static readonly Number ArcsinTo = new Number(+Math.PI / 2, double.MaxValue);
        private static readonly Number ArccosFrom = new Number(0, -double.MaxValue);
        private static readonly Number ArccosTo = new Number(Math.PI, double.MaxValue);
        private static readonly EntitySet Empty = new EntitySet();

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
        public static EntitySet InvertFunctionEntity(FunctionEntity func, Entity value, Entity x)
        {
            Entity a = func.Children[0];
            Entity b = func.Children.Count == 2 ? func.Children[1] : null;
            int arg = func.Children.Count == 2 && func.Children[1].FindSubtree(x) != null ? 1 : 0;
            var n = MathS.Var("n");
            var res = new EntitySet();
            var pi = MathS.pi;

            EntitySet GetNotNullEntites(EntitySet set)
            {
                return new EntitySet(set.Where(el => el.entType != Entity.EntType.NUMBER || !el.GetValue().IsNull));
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
                    if (arg == 0)
                        // log(x, a) = value => x = a ^ value
                        return GetNotNullEntites(FindInvertExpression(a, MathS.Pow(b, value), x));
                    else
                        // log(a, x) = value => a = x ^ value => x = a ^ (1 / value)
                        return GetNotNullEntites(FindInvertExpression(b, MathS.Pow(a, 1 / value), x));
                default:
                    throw new SysException("Unknown function");
            }
        }
    }
}

namespace AngouriMath.Functions.Algebra.AnalyticalSolving
{
    using PatType = Entity.PatType;
    internal static class AnalyticalSolver
    {
        /// <summary>
        /// Equation solver
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="x"></param>
        /// <param name="dst"></param>
        internal static void Solve(Entity expr, VariableEntity x, EntitySet dst)
            => Solve(expr, x, dst, compensateSolving: false);
        internal static void Solve(Entity expr, VariableEntity x, EntitySet dst, bool compensateSolving)
        {
            if (expr == x)
            {
                dst.Add(0);
                return;
            }

            EntitySet res = PolynomialSolver.SolveAsPolynomial(expr.DeepCopy(), x);
            if (res != null)
            {
                dst.AddRange(res);
                return;
            }

            if (expr.entType == Entity.EntType.OPERATOR)
            {
                switch (expr.Name)
                {
                    case "mulf":
                        Solve(expr.Children[0], x, dst);
                        Solve(expr.Children[1], x, dst);
                        return;
                    case "divf":
                        Solve(expr.Children[0], x, dst);
                        return;
                    case "powf":
                        Solve(expr.Children[0], x, dst);
                        return;
                    case "minusf":
                        if (expr.Children[1].FindSubtree(x) == null && compensateSolving)
                        {
                            if (expr.Children[0] == x)
                            {
                                dst.Add(expr.Children[1]);
                                return;
                            }
                            var subs = 0;
                            Entity lastChild = null;
                            foreach (var child in expr.Children[0].Children)
                            {
                                if (child.FindSubtree(x) != null)
                                {
                                    subs += 1;
                                    lastChild = child;
                                }
                            }
                            if (subs != 1)
                                break;
                            var resInverted = TreeAnalyzer.FindInvertExpression(expr.Children[0], expr.Children[1], lastChild);
                            foreach (var result in resInverted)
                                Solve(lastChild - result, x, dst, compensateSolving: true);
                            //foreach (var block in res.Select(r => (lastChild - r).Solve(x)))
                            //    dst.AddRange(block);
                            return;
                        }
                        break;
                }
            }
            else if (expr.entType == Entity.EntType.FUNCTION)
            {
                dst.AddRange(TreeAnalyzer.InvertFunctionEntity(expr as FunctionEntity, 0, x));
                return;
            }


            // Here we generate a unique variable name
            var uniqVars = MathS.GetUniqueVariables(expr);
            uniqVars.Sort((a, b) => b.Name.Length.CompareTo(a.Name.Length));
            VariableEntity newVar = uniqVars[0].Name + "quack";
            // // //

            // Here we find all possible replacements
            var replacements = new List<Tuple<Entity, Entity>>();
            replacements.Add(new Tuple<Entity, Entity>(TreeAnalyzer.GetMinimumSubtree(expr, x), expr));
            foreach (var alt in expr.Alternate(4))
                replacements.Add(new Tuple<Entity, Entity>(TreeAnalyzer.GetMinimumSubtree(alt, x), alt));
            // // //

            // Here we find one that has at least one solution
            EntitySet solutions = null;
            Entity bestReplacement = null;
            foreach (var replacement in replacements)
            {
                if (replacement.Item1 == x)
                    continue;
                var newExpr = replacement.Item2.DeepCopy();
                TreeAnalyzer.FindAndReplace(ref newExpr, replacement.Item1, newVar);
                solutions = newExpr.SolveEquation(newVar);
                if (solutions.Count > 0/* && !newExpr.Children.Any(c => c == newVar)*/)
                {
                    bestReplacement = replacement.Item1;
                    break;
                }
            }
            // // //

            // Here we get back to our initial variable
            if (bestReplacement != null)
            {
                EntitySet newDst = new EntitySet();
                foreach (var solution in solutions)
                {
                    var str = bestReplacement.ToString();
                        if (!compensateSolving || bestReplacement - solution != expr)
                        Solve(bestReplacement - solution, x, newDst, compensateSolving: true);
                }
                dst.AddRange(newDst);
            }
            // // //
            

            if (dst.Count == 0) // if nothing has been found so far
            {
                EntitySet allVars = new EntitySet();
                TreeAnalyzer.GetUniqueVariables(expr, allVars);
                if (allVars.Count == 1)
                    dst.Merge(expr.SolveNt(x));
            }
        }
    }
}