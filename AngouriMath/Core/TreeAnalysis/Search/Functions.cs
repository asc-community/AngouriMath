using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core.TreeAnalysis
{
    internal static partial class TreeAnalyzer
    {
        internal static bool IsZero(Entity e) => MathS.CanBeEvaluated(e) && e.Eval() == 0;
        internal static void InvertNegativePowers(ref Entity expr)
        {
            if (expr.entType == Entity.EntType.OPERATOR &&
                expr.Name == "powf" &&
                expr.Children[1].entType == Entity.EntType.NUMBER &&
                expr.Children[1].GetValue().IsInteger() &&
                !expr.Children[1].GetValue().IsNatural())
                expr = 1 / MathS.Pow(expr.Children[0], (-1 * expr.Children[1].GetValue()));
            else
            {
                for (int i = 0; i < expr.Children.Count; i++)
                {
                    var tmp = expr.Children[i];
                    InvertNegativePowers(ref tmp);
                    expr.Children[i] = tmp;
                }
            }
        }

        internal static readonly Pattern NegativeMultiplyerPattern = Patterns.any1 + Patterns.const1 * Patterns.any2;

        internal static void InvertNegativeMultipliers(ref Entity expr)
        {
            if (NegativeMultiplyerPattern.Match(expr) &&
                !expr.Children[1].Children[0].GetValue().IsComplex() &&
                expr.Children[1].Children[0].GetValue().Re > 0)
                expr = expr.Children[0] - (-1 * expr.Children[1].Children[0].GetValue()) * expr.Children[1].Children[1];
            else
            {
                for (int i = 0; i < expr.Children.Count; i++)
                {
                    var tmp = expr.Children[i];
                    InvertNegativePowers(ref tmp);
                    expr.Children[i] = tmp;
                }
            }
        }
    }
}
