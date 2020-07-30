
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


using AngouriMath.Core.Numerix;
using System;
using System.Diagnostics;

namespace AngouriMath.Core.TreeAnalysis
{
    internal static partial class TreeAnalyzer
    {
        /// <summary>
        /// If an evaluable expression is equal to zero, <see langword="true"/>, otherwise, <see langword="false"/>
        /// For example, 1 - 1 is zero, but 1 + a is not
        /// </summary>
        internal static bool IsZero(Entity e) => MathS.CanBeEvaluated(e) && e.Eval() == 0;

        /// <summary>a ^ (-1) => 1 / a</summary>
        internal static void InvertNegativePowers(ref Entity expr)
        {
            if (expr is OperatorEntity { Name: "powf" } &&
                expr.GetChild(1) is NumberEntity { Value:IntegerNumber pow } && pow < 0)
                expr = 1 / MathS.Pow(expr.GetChild(0), -1 * pow);
            else
            {
                for (int i = 0; i < expr.ChildrenCount; i++)
                {
                    var tmp = expr.GetChild(i);
                    InvertNegativePowers(ref tmp);
                    expr.SetChild(i, tmp);
                }
            }
        }

        internal static readonly Pattern NegativeMultiplyerPattern = Patterns.any1 + Patterns.const1 * Patterns.any2;
        /// <summary>1 + (-x) => 1 - x</summary>
        internal static void InvertNegativeMultipliers(ref Entity expr)
        {
            if (NegativeMultiplyerPattern.Match(expr)
                && expr.GetChild(1).GetChild(0) is NumberEntity { Value:RealNumber { Value:var real } }
                && real > IntegerNumber.Zero)
                expr = expr.GetChild(0) - (-1 * real) * expr.GetChild(1).GetChild(1);
            else
            {
                for (int i = 0; i < expr.ChildrenCount; i++)
                {
                    var tmp = expr.GetChild(i);
                    InvertNegativePowers(ref tmp);
                    expr.SetChild(i, tmp);
                }
            }
        }
        // C for Constant
        internal static readonly Pattern FactorialDivisionPatternCC =
            Factorialf.PHang(Patterns.any1 + Patterns.const1) / Factorialf.PHang(Patterns.any1 + Patterns.const2);
        internal static readonly Pattern FactorialDivisionPatternC0 =
            Factorialf.PHang(Patterns.any1 + Patterns.const1) / Factorialf.PHang(Patterns.any1);
        internal static readonly Pattern FactorialDivisionPattern0C =
            Factorialf.PHang(Patterns.any1) / Factorialf.PHang(Patterns.any1 + Patterns.const2);

        /// <summary>(x + a)! / (x + b)! -> (x+b+1)*(x+b+2)*...*(x+a)</summary>
        internal static void ExpandFactorialDivisions(ref Entity expr)
        {
            (Entity any1, Entity const1, Entity const2)? consts = null;
            if (FactorialDivisionPatternCC.Match(expr))
                consts = (expr.GetChild(0).GetChild(0).GetChild(0),
                          expr.GetChild(0).GetChild(0).GetChild(1),
                          expr.GetChild(1).GetChild(0).GetChild(1));
            else if (FactorialDivisionPatternC0.Match(expr))
                consts = (expr.GetChild(0).GetChild(0).GetChild(0),
                          expr.GetChild(0).GetChild(0).GetChild(1),
                          new NumberEntity(0));
            else if (FactorialDivisionPattern0C.Match(expr))
                consts = (expr.GetChild(1).GetChild(0).GetChild(0),
                          new NumberEntity(0),
                          expr.GetChild(1).GetChild(0).GetChild(1));
            if (consts is (var x, NumberEntity { Value: var num }, NumberEntity { Value: var den })
                && num - den is IntegerNumber { Value: var diff }
                && !diff.IsZero && diff.Abs() < 20) // We don't want to expand (x+100)!/x!
                if (diff > 0) // e.g. (x+3)!/x! = (x+1)(x+2)(x+3)
                {
                    expr = Add(x, den + 1);
                    for (var i = 2; i <= diff; i++)
                        expr *= Add(x, den + i);
                } else // e.g. x!/(x+3)! = 1/(x+1)/(x+2)/(x+3)
                {
                    diff = -diff;
                    expr = 1 / Add(x, num + 1);
                    for (var i = 2; i <= diff; i++)
                        expr /= Add(x, num + i);
                }
            static Entity Add(Entity a, ComplexNumber b) =>
                b == IntegerNumber.Zero ? a : a + new NumberEntity(b);
        }
        // (x-1)!*x = x!
        // x!*(x+1) = (x+1)!
        // etc.
        internal static readonly Pattern FactorialMultiplicationPatternCC =
            Factorialf.PHang(Patterns.any1 + Patterns.const1) * (Patterns.any1 + Patterns.const2);
        internal static readonly Pattern FactorialMultiplicationPatternC0 =
            Factorialf.PHang(Patterns.any1 + Patterns.const1) * Patterns.any1;
        internal static readonly Pattern FactorialMultiplicationPattern0C =
            Factorialf.PHang(Patterns.any1) * (Patterns.any1 + Patterns.const2);
        // https://en.wikipedia.org/wiki/Reflection_formula
        // (z-1)! (-z)! -> Γ(z) Γ(1 - z) = π/sin(π z), z ∉ ℤ // actually, when z ∈ ℤ, both sides include division by zero, so we can still replace
        // Replace z with -z => z! (-z-1)! = π/sin(-π z)
        // TODO: Modify the complexity criteria to rank non-elementary functions more complex than elementary functions
        //       so that this formula can be used to simplify
        internal static readonly Pattern FactorialReflectionFormula =
            Factorialf.PHang(Patterns.any1) * Factorialf.PHang(Patterns.any2);
        // TODO: Other than the reflection formula,
        // (z-1)! (z-1/2)! -> Γ(z) Γ(z + 1/2) = 2^(1 - 2 z) sqrt(π) Γ(2 z) -> 2^(1 - 2 z) sqrt(π) (2 z - 1)!
        // is also another possible simplification
        /// <summary>(x-1)! x -> x!, x! (x+1) -> (x+1)!, etc.<para>as well as z! (-z-1)! -> -π/sin(π z)</para></summary>
        internal static void CollapseFactorialMultiplications(ref Entity expr)
        {
            (Entity newFact, Entity const1, Entity const2)? consts = null;
            if (FactorialMultiplicationPatternCC.Match(expr))
                consts = (expr.GetChild(1), expr.GetChild(0).GetChild(0).GetChild(1), expr.GetChild(1).GetChild(1));
            else if (FactorialMultiplicationPatternC0.Match(expr))
                consts = (expr.GetChild(1), expr.GetChild(0).GetChild(0).GetChild(1), new NumberEntity(0));
            else if (FactorialMultiplicationPattern0C.Match(expr))
                consts = (expr.GetChild(1), new NumberEntity(0), expr.GetChild(1).GetChild(1));
            if (consts is (var x, NumberEntity { Value: var factConst }, NumberEntity { Value: var @const })
                && factConst + 1 == @const)
            {
                expr = Factorialf.Hang(x);
                return;
            }
            // This currently makes formulae more complex with the current complexity criteria. See above note
            // if (FactorialReflectionFormula.Match(expr))
            // {
            //     Entity? z = null;
            //     // InnerSimplify() cannot simplify x + 1 - 1 -> x
            //     if ((-expr.Children[0].Children[0] - 1).Simplify() == expr.Children[1].Children[0])
            //         z = expr.Children[0].Children[0];
            //     else if ((-expr.Children[1].Children[0] - 1).Simplify() == expr.Children[0].Children[0])
            //         z = expr.Children[1].Children[0];
            //     if (z is { })
            //         expr = (MathS.pi / MathS.Sin(-MathS.pi * z)).Simplify();
            // }
        }
    }
}