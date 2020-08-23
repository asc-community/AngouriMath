
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

using System;
using System.Collections.Generic;
using System.Linq;
using AngouriMath.Core;

namespace AngouriMath.Functions.Algebra.AnalyticalSolving
{
    using static Entity;
    using static Entity.Number;
    internal static class CommonDenominatorSolver
    {
        /// <summary>
        /// All constants, no matter multiplied or divided, are numerator's coefficients:
        /// 2 * x / 3 => num: 2 / 3
        /// 
        /// All entities that contain x and have a real negative power are denominator's multipliers
        /// 2 / (x + 3) => den: [x + 3]
        /// 2 / ((x^(-1) + 3) * (x2 + 1)) => den: [x^(-1) + 3, x2 + 1]
        /// 
        /// All entities that have complex power are considered as product of an entity with a real power
        /// and an entity whole power's real part is 0
        /// x ^ (-1 + 2i) => num: x ^ (2i), den: [x]
        /// x ^ (3 - 2i) => num: x ^ (3 - 2i), den: []
        /// 
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        private static (Entity numerator, List<(Entity den, Real pow)> denominatorMultipliers) FindFractions(Entity term, Variable x)
        {
            // TODO: consider cases where we should NOT gather all powers in row
            static Entity GetPower(Entity expr) =>
                expr is Powf(var @base, var exponent) ? exponent * GetPower(@base) : 1;

            static Entity GetBase(Entity expr) =>
                expr is Powf(var @base, _) ? GetBase(@base) : expr;

            // We init numerator with 1, but once InnerSimplify is called, we get rid of 1 *
            var oneInfo = (numerator: (Entity)1, denominatorMultipliers: new List<(Entity den, Real pow)>());

            var multipliers = Mulf.LinearChildren(term);
            foreach (var multiplier in multipliers)
                if (!(multiplier.Contains(x) && GetPower(multiplier).Evaled is Real { IsPositive:false } realPart))
                    oneInfo.numerator *= multiplier;
                else
                    oneInfo.denominatorMultipliers.Add((GetBase(multiplier), realPart));
            return oneInfo;
        }

        /// <summary>
        /// Finds the best common denominator, multiplies the whole expression by that, and
        /// tries solving if the found denominator is not 1
        /// </summary>
        internal static IEnumerable<Entity>? Solve(Entity expr, Variable x) =>
            FindCD(expr, x) is { } res ? AnalyticalSolver.Solve(res, x) : null;

        private static Entity? FindCD(Entity expr, Variable x)
        {
            var terms = Sumf.LinearChildren(expr);
            var denominators = new Dictionary<string, (Entity den, Real pow)>();
            var fracs = new List<(Entity numerator, List<(Entity den, Real pow)> denominatorMultipliers)>();
            foreach (var term in terms)
            {
                var oneInfo = FindFractions(term, x);
                foreach (var denMp in oneInfo.denominatorMultipliers)
                {
                    var (den, pow) = denMp;
                    var name = den.ToString(); // TODO: Replace with faster hashing
                    if (!denominators.ContainsKey(name))
                        denominators[name] = (den, 0);
                    denominators[name] = (den, Number.Max(denominators[name].pow, -pow));
                }
                fracs.Add(oneInfo);
            }

            if (denominators.Count == 0)
                return null; // If there's no denominators or it's equal to 1, then we don't have to try to solve yet anymore

            static Dictionary<string, (Entity den, Real pow)> ToDict(List<(Entity den, Real pow)> list)
            {
                var res = new Dictionary<string, (Entity den, Real pow)>();
                foreach (var (den, pow) in list)
                    res[den.ToString()] = (den, -pow);
                return res;
            }

            var newTerms = new List<Entity>();
            foreach (var (num, dens) in fracs)
            {
                var denDict = ToDict(dens);
                Entity invertDenominator = 1;
                foreach (var mp in denominators)
                {
                    if (denDict.ContainsKey(mp.Key))
                        invertDenominator *= MathS.Pow(mp.Value.den, denominators[mp.Key].pow - denDict[mp.Key].pow);
                    else
                        invertDenominator *= MathS.Pow(mp.Value.den, denominators[mp.Key].pow);
                }
                newTerms.Add(invertDenominator * num);
            }

            return TreeAnalyzer.MultiHangBinary(newTerms, (a, b) => new Sumf(a, b)).InnerSimplify();
        }
    }
}
