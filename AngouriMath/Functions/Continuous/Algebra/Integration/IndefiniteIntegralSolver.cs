using PeterO.Numbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AngouriMath.Functions.Algebra
{
    static class IndefiniteIntegralSolver
    {
        internal static Entity? SolveBySplittingSum(Entity expr, Entity.Variable x)
        {
            var splitted = TreeAnalyzer.GatherLinearChildrenOverSumAndExpand(expr, e => e.Contains(x));
            if (splitted is null || splitted.Count < 2) return null; // nothing to do, let other solvers do the work   
            splitted[0] = Integration.ComputeIndefiniteIntegral(splitted[0], x); // base case for aggregate
            var result = splitted.Aggregate((e1, e2) => e1 + Integration.ComputeIndefiniteIntegral(e2, x));
            return result;
        }

        internal static Entity? SolveAsPolynomialTerm(Entity expr, Entity.Variable x) => expr switch
        {
            Entity.Mulf(var m1, var m2) => 
                !m1.Contains(x) ? 
                    m1 * Integration.ComputeIndefiniteIntegral(m2, x) : 
                !m2.Contains(x) ?
                    m2 * Integration.ComputeIndefiniteIntegral(m1, x) :
                null,

            Entity.Divf(var div, var over) =>
                !div.Contains(x) ?
                    over is Entity.Powf(var @base, var power) ?
                        div * Integration.ComputeIndefiniteIntegral(MathS.Pow(@base, -power), x) :
                        div * Integration.ComputeIndefiniteIntegral(MathS.Pow(over, -1), x) :
                !over.Contains(x) ?
                    Integration.ComputeIndefiniteIntegral(div, x) / over :
                null,

            Entity.Powf(var @base, var power) =>
                !power.Contains(x) && @base == x ?
                    power == -1 ?
                        MathS.Ln(@base) : // TODO: here should be ln(abs(x)) but for now I left it as is
                        MathS.Pow(x, power + 1) / (power + 1) :     
                    null,

            Entity.Variable(var v) =>
                v == x ? MathS.Pow(x, 2) / 2 : v * x,

            _ => null
        };
    }
}
