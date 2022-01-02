//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Diagnostics.CodeAnalysis;
using PeterO.Numbers;

namespace AngouriMath.Functions
{
    using static Entity;
    internal static partial class TreeAnalyzer
    {
        /// <summary>
        /// Finds all monomials with respect to the power of <paramref name="variable"/>
        /// </summary>
        internal static bool TryGetPolynomial(Entity expr, Variable variable, 
            [NotNullWhen(true)] out Dictionary<EInteger, Entity>? dst)
        {
            var children = Sumf.LinearChildren(expr.Expand());
            dst = Algebra.AnalyticalSolving.PolynomialSolver.GatherMonomialInformation
                <EInteger, PrimitiveInteger>(children, variable);
            return dst is not null;
        }

        /// <summary>a x + b</summary>
        internal static bool TryGetPolyLinear(Entity expr, Variable variable,
            [NotNullWhen(true)] out Entity? a,
            [NotNullWhen(true)] out Entity? b)
        { 
            a = b = null;
            if (TryGetPolynomial(expr, variable, out var monoInfo) 
                && AssertIsValidNormalPoly(monoInfo, 1, out var res))
            {
                a = res[1];
                b = res[0];
                return true;
            }
            return false;
        }

        /// <summary>a x ^ 2 + b x + c</summary>
        internal static bool TryGetPolyQuadratic(Entity expr, Variable variable,
            [NotNullWhen(true)] out Entity? a,
            [NotNullWhen(true)] out Entity? b,
            [NotNullWhen(true)] out Entity? c)
        {
            a = b = c = null;
            if (TryGetPolynomial(expr, variable, out var monoInfo)
                && AssertIsValidNormalPoly(monoInfo, 2, out var res))
            {
                a = res[2];
                b = res[1];
                c = res[0];
                return true;
            }
            return false;
        }

        private static bool AssertIsValidNormalPoly(Dictionary<EInteger, Entity> polyInfo, 
            int maxPower, out Entity[] monoByPower)
        {
            monoByPower = new Entity[maxPower + 1];
            foreach (var pair in polyInfo)
            {
                if (!pair.Key.CanFitInInt32())
                    return false;
                var asInt = pair.Key.ToInt32Unchecked();
                if (asInt < 0)
                    return false;
                if (asInt > maxPower)
                    return false;
                monoByPower[asInt] = pair.Value;
            }
            for (int i = 0; i < monoByPower.Length; i++)
                if (monoByPower[i] is null)
                    monoByPower[i] = 0;
            return true;
        }
    }
}
