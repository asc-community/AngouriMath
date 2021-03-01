/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using AngouriMath.Core.Multithreading;
using PeterO.Numbers;

namespace AngouriMath.Functions
{
    using static AngouriMath.Entity.Set;
    using static Entity;
    using static Entity.Number;
    internal static class Simplificator
    {
        internal static Entity PickSimplest(Entity one, Entity another)
            => one.SimplifiedRate < another.SimplifiedRate ? one : another;

        /// <summary>See more details in <see cref="Entity.Simplify(int)"/></summary>
        internal static Entity Simplify(Entity expr, int level) => Alternate(expr, level).First().InnerSimplified;

        internal static Entity SimplifyChildren(Entity expr)
        {
            return expr.Replace(Patterns.InvertNegativePowers)
                       .Replace(Patterns.InvertNegativeMultipliers).Replace(
                        Patterns.SortRules(TreeAnalyzer.SortLevel.HIGH_LEVEL)
                        )
                .InnerSimplified.Replace(Patterns.CommonRules).InnerSimplified;
        }

        /// <summary>Finds all alternative forms of an expression</summary>
        internal static IEnumerable<Entity> Alternate(Entity src, int level)
        {
            if (src is FiniteSet ss)
                return new[] { ss.Apply(ent => ent.Simplify()).InnerSimplified };
            if (src is Number or Variable or Entity.Boolean)
                return new[] { src };
            var stage1 = src.InnerSimplified;

#if DEBUG
            if (MathS.Diagnostic.CatchOnSimplify.Value(stage1))
                throw new MathS.Diagnostic.DiagnosticCatchException();
#endif

            if (stage1 is Number or Variable or Entity.Boolean)
                return new[] { stage1 };

            // List of criteria for expr's complexity
            var history = new SortedDictionary<double, HashSet<Entity>>();
            void AddHistory(Entity expr)
            {
                void __IterAddHistory(Entity expr)
                {
                    var refexpr = expr.Replace(Patterns.SortRules(TreeAnalyzer.SortLevel.HIGH_LEVEL)).InnerSimplified;
                    var compl1 = refexpr.SimplifiedRate;
                    var compl2 = expr.SimplifiedRate;
                    var n = compl1 > compl2 ? expr : refexpr;
                    var ncompl = Math.Min(compl2, compl1);
                    if (history.TryGetValue(ncompl, out var ncomplList))
                        ncomplList.Add(n);
                    else 
                        history[ncompl] = new HashSet<Entity> { n };
                }
                __IterAddHistory(expr);
                __IterAddHistory(expr.Replace(Patterns.InvertNegativePowers));

                MultithreadingFunctional.ExitIfCancelled();
            }

            AddHistory(stage1);
            var res = stage1;

            for (int i = 0; i < Math.Abs(level); i++)
            {
                var sortLevel = i switch
                {
                    1 => TreeAnalyzer.SortLevel.MIDDLE_LEVEL,
                    2 => TreeAnalyzer.SortLevel.LOW_LEVEL,
                    _ => TreeAnalyzer.SortLevel.HIGH_LEVEL
                };
                res = res.Replace(Patterns.SortRules(sortLevel)).InnerSimplified;
                if (res.Nodes.Any(child => child is Powf))
                    AddHistory(res = res.Replace(Patterns.PowerRules).InnerSimplified);

#if DEBUG
                if (MathS.Diagnostic.CatchOnSimplify.Value(res)) throw new MathS.Diagnostic.DiagnosticCatchException();
#endif

                AddHistory(res = SimplifyChildren(res));

#if DEBUG
                if (MathS.Diagnostic.CatchOnSimplify.Value(res)) throw new MathS.Diagnostic.DiagnosticCatchException();
#endif

                AddHistory(res = res.Replace(Patterns.InvertNegativePowers).Replace(Patterns.DivisionPreparingRules).InnerSimplified);

#if DEBUG
                if (MathS.Diagnostic.CatchOnSimplify.Value(res)) throw new MathS.Diagnostic.DiagnosticCatchException();
#endif

                AddHistory(res = res.Replace(Patterns.PolynomialLongDivision).InnerSimplified);

#if DEBUG
                if (MathS.Diagnostic.CatchOnSimplify.Value(res)) throw new MathS.Diagnostic.DiagnosticCatchException();
#endif
                AddHistory(res = res.Replace(Patterns.NormalTrigonometricForm).InnerSimplified);
                AddHistory(res = res.Replace(e => Patterns.FractionCommonDenominatorRules(e, sortLevel)).InnerSimplified);
                AddHistory(res = res.Replace(Patterns.PowerRules).InnerSimplified);
#if DEBUG
                if (MathS.Diagnostic.CatchOnSimplify.Value(res)) throw new MathS.Diagnostic.DiagnosticCatchException();
#endif

                if (res.Nodes.Any(child => child is TrigonometricFunction))
                {
                    var res1 = res.Replace(Patterns.ExpandTrigonometricRules).InnerSimplified;
                    AddHistory(res = res.Replace(Patterns.TrigonometricRules).Replace(Patterns.CommonRules).InnerSimplified);
                    AddHistory(res1);
                    res = PickSimplest(res, res1);
                    AddHistory(res = res.Replace(Patterns.CollapseTrigonometricFunctions).Replace(Patterns.TrigonometricRules));
                }

#if DEBUG
                if (MathS.Diagnostic.CatchOnSimplify.Value(res)) throw new MathS.Diagnostic.DiagnosticCatchException();
#endif

                if (res.Nodes.Any(child => child is Statement))
                {
                    AddHistory(res = res.Replace(Patterns.BooleanRules).InnerSimplified);
                }

#if DEBUG
                if (MathS.Diagnostic.CatchOnSimplify.Value(res)) throw new MathS.Diagnostic.DiagnosticCatchException();
#endif

                if (res.Nodes.Any(child => child is ComparisonSign))
                {
                    AddHistory(res = res.Replace(Patterns.InequalityEqualityRules).InnerSimplified);
                }

#if DEBUG
                if (MathS.Diagnostic.CatchOnSimplify.Value(res)) throw new MathS.Diagnostic.DiagnosticCatchException();
#endif

                if (res.Nodes.Any(child => child is Factorialf))
                {
                    AddHistory(res = res.Replace(Patterns.ExpandFactorialDivisions).InnerSimplified);
                    AddHistory(res = res.Replace(Patterns.FactorizeFactorialMultiplications).InnerSimplified);
                }

#if DEBUG
                if (MathS.Diagnostic.CatchOnSimplify.Value(res)) throw new MathS.Diagnostic.DiagnosticCatchException();
#endif

                if (res.Nodes.Any(child => child is Powf or Logf))
                    AddHistory(res = res.Replace(Patterns.PowerRules).InnerSimplified);

#if DEBUG
                if (MathS.Diagnostic.CatchOnSimplify.Value(res)) throw new MathS.Diagnostic.DiagnosticCatchException();
#endif

                if (res.Nodes.Any(child => child is Set))
                {
                    var replaced = res.Replace(Patterns.SetOperatorRules);

#if DEBUG
                    if (MathS.Diagnostic.CatchOnSimplify.Value(res)) throw new MathS.Diagnostic.DiagnosticCatchException();
#endif

                    AddHistory(res = replaced.InnerSimplified);
                }

#if DEBUG
                if (MathS.Diagnostic.CatchOnSimplify.Value(res)) throw new MathS.Diagnostic.DiagnosticCatchException();
#endif

                if (res.Nodes.Any(child => child is Phif))
                    AddHistory(res = res.Replace(Patterns.PhiFunctionRules).InnerSimplified);

#if DEBUG 
                if (MathS.Diagnostic.CatchOnSimplify.Value(res)) throw new MathS.Diagnostic.DiagnosticCatchException(); 
#endif

                Entity? possiblePoly = null;
                foreach (var var in res.Vars)
                    if (TryPolynomial(res, var, out var resPoly)
                        && (possiblePoly is null || resPoly.Complexity < possiblePoly.Complexity))
                        AddHistory(possiblePoly = resPoly);
                if (possiblePoly is { } && possiblePoly.Complexity < res.Complexity)
                    res = possiblePoly;

#if DEBUG
                if (MathS.Diagnostic.CatchOnSimplify.Value(res)) throw new MathS.Diagnostic.DiagnosticCatchException();
#endif

                AddHistory(res = res.Replace(Patterns.CommonRules));

#if DEBUG
                if (MathS.Diagnostic.CatchOnSimplify.Value(res)) throw new MathS.Diagnostic.DiagnosticCatchException();
#endif

                AddHistory(res = res.Replace(Patterns.NumericNeatRules));

#if DEBUG
                if (MathS.Diagnostic.CatchOnSimplify.Value(res)) throw new MathS.Diagnostic.DiagnosticCatchException();
#endif
                /*
                This was intended to simplify expressions as polynomials over nodes, some kind of
                greatest common node and simplifying over it. However, the current algorithm does
                not solve this issue completely and yet too slow to be accepted.

                AddHistory(res = TreeAnalyzer.Factorize(res));
                */

                res = history[history.Keys.Min()].First();
            }
            if (level > 0) // if level < 0 we don't check whether expanded version is better
            {
                AddHistory(res.Expand().Simplify(-level));

#if DEBUG
                if (MathS.Diagnostic.CatchOnSimplify.Value(res)) throw new MathS.Diagnostic.DiagnosticCatchException();
#endif

                AddHistory(res.Factorize().Simplify(-level));

#if DEBUG
                if (MathS.Diagnostic.CatchOnSimplify.Value(res)) throw new MathS.Diagnostic.DiagnosticCatchException();
#endif
            }

            return history.Values.SelectMany(x => x);
        }

        /// <summary>
        /// Sorts an expression into a polynomial.
        /// See more at <see cref="MathS.TryPolynomial"/>
        /// </summary>
        internal static bool TryPolynomial(Entity expr, Variable variable,
            [System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
            out Entity? dst)
        {
            dst = null;
            var children = Sumf.LinearChildren(expr.Expand());
            var monomialsByPower = Algebra.AnalyticalSolving.PolynomialSolver.GatherMonomialInformation
                <EInteger, TreeAnalyzer.PrimitiveInteger>(children, variable);
            if (monomialsByPower == null)
                return false;
            var res = BuildPoly(monomialsByPower, variable);
            if (res is null)
                return false;
            dst = res;
            return true;
        }
        internal static Entity? BuildPoly(Dictionary<EInteger, Entity> monomialsByPower, Variable x)
        {
            var terms = new List<Entity>();
            foreach (var index in monomialsByPower.Keys.OrderByDescending(x => x))
            {
                var pair = new KeyValuePair<EInteger, Entity>(index, monomialsByPower[index]);
                if (pair.Key.IsZero)
                {
                    terms.Add(pair.Value.InnerSimplified);
                    continue;
                }

                var px = pair.Key.Equals(EInteger.One) ? x : MathS.Pow(x, pair.Key);
                terms.Add(pair.Value == 1 ? px : pair.Value.InnerSimplified * px);
            }

            if (terms.Count == 0)
                return null;
            var dst = terms[0];
            for (int i = 1; i < terms.Count; i++)
                if (terms[i] is Mulf(Real { IsNegative: true } r, var m))
                    dst -= -r * m;
                else
                    dst += terms[i];
            return dst.InnerSimplified;
        }

        internal static Entity ParaphraseInterval(Entity entity, Entity left, bool leftClosed, Entity right, bool rightClosed)
        {
            var leftCon = ConditionallyGreater(entity, left);
            if (leftClosed)
                leftCon |= entity.Equalizes(left);
            var rightCon = ConditionallyGreater(right, entity);
            if (rightClosed)
                rightCon |= entity.Equalizes(right);
            return leftCon & rightCon;
        }

        internal static Entity ConditionallyGreater(Entity left, Entity right)
        {
            return SimplifyChildren(left - right) > 0;
        }
    }
}