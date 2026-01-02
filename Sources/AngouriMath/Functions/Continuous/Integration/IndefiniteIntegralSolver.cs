//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

namespace AngouriMath.Functions.Algebra
{
    internal static class IndefiniteIntegralSolver
    {
        internal static Entity? SolveBySplittingSum(Entity expr, Entity.Variable x)
        {
            var splitted = TreeAnalyzer.GatherLinearChildrenOverSumAndExpand(expr, e => e.ContainsNode(x));
            if (splitted is null || splitted.Count < 2) return null; // nothing to do, let other solvers do the work   
            splitted[0] = Integration.ComputeIndefiniteIntegral(splitted[0], x); // base case for aggregate
            var result = splitted.Aggregate((e1, e2) => e1 + Integration.ComputeIndefiniteIntegral(e2, x));
            return result;
        }

        internal static Entity? SolveAsPolynomialTerm(Entity expr, Entity.Variable x) => expr switch
        {
            Entity.Mulf(var m1, var m2) => 
                !m1.ContainsNode(x) ? 
                    m1 * Integration.ComputeIndefiniteIntegral(m2, x) : 
                !m2.ContainsNode(x) ?
                    m2 * Integration.ComputeIndefiniteIntegral(m1, x) :
                null,

            Entity.Divf(var div, var over) =>
                !div.ContainsNode(x) ?
                    over is Entity.Powf(var @base, var power) ?
                        div * Integration.ComputeIndefiniteIntegral(MathS.Pow(@base, -power), x) :
                        div * Integration.ComputeIndefiniteIntegral(MathS.Pow(over, -1), x) :
                !over.ContainsNode(x) ?
                    Integration.ComputeIndefiniteIntegral(div, x) / over :
                null,

            Entity.Powf(var @base, var power) =>
                !power.ContainsNode(x) && @base == x ?
                    power == -1 ?
                        MathS.Ln(@base) : // TODO: here should be ln(abs(x)) but for now I left it as is
                        MathS.Pow(x, power + 1) / (power + 1) :     
                    null,

            Entity.Variable(var v) =>
                v == x ? MathS.Pow(x, 2) / 2 : v * x,

            _ => null
        };

        internal static Entity? SolveIntegratingByParts(Entity expr, Entity.Variable x)
        {
            static Entity? IntegrateByParts(Entity v, Entity u, Entity.Variable x, int currentRecursion = 0)
            {
                if (v == 0) return 0;
                if (currentRecursion == MathS.Settings.MaxExpansionTermCount) return null;

                var integral = Integration.ComputeIndefiniteIntegral(u, x);
                var differential = v.Differentiate(x);
                var result = IntegrateByParts(differential, integral, x, currentRecursion + 1);
                return (result is null) ? null : v * integral - result;
            }

            if (expr is Entity.Mulf(var f, var g))
            {
                if (MathS.TryPolynomial(f, x, out var fPoly))
                {
                    return IntegrateByParts(fPoly, g, x);
                }
                if (MathS.TryPolynomial(g, x, out var gPoly))
                {
                    return IntegrateByParts(gPoly, f, x);
                }
                else return null;
            }
            else return null;
        }

        internal static Entity? SolveLogarithmic(Entity expr, Entity.Variable x) => expr switch
        {
            Entity.Logf(var @base, var arg) =>
                @base.ContainsNode(x) ?
                    Integration.ComputeIndefiniteIntegral(MathS.Ln(arg) / MathS.Ln(@base), x) :
                arg is Entity.Powf(var y, var pow) ? // log(b, y^p) = ln(y^p) / ln(b) = ln(p) / ln(b) * ln(y)
                    Integration.ComputeIndefiniteIntegral(pow / MathS.Ln(@base) * MathS.Ln(y), x) :
                    null,

            _ => null
        };

        internal static Entity? SolveExponential(Entity expr, Entity.Variable x) => expr switch
        {
            Entity.Powf(var @base, var pow) =>
                @base.ContainsNode(x) ?
                    Integration.ComputeIndefiniteIntegral(MathS.Pow(MathS.e, MathS.Ln(@base) * pow), x) :
                    null,

            _ => null
        };
    }
}
