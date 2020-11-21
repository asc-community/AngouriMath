
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

using System.Collections.Generic;
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
