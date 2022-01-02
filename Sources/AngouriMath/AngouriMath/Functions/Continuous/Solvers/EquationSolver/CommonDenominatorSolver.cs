//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

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
        /// </summary>
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
                if (!(multiplier.ContainsNode(x) && GetPower(multiplier).Evaled is Real { IsPositive:false } realPart))
                    oneInfo.numerator *= multiplier;
                else
                    oneInfo.denominatorMultipliers.Add((GetBase(multiplier), realPart));
            return oneInfo;
        }

        /// <summary>
        /// Finds the best common denominator, multiplies the whole expression by that, and
        /// tries solving if the found denominator is not 1
        /// </summary>
        internal static bool TrySolveGCD(Entity expr, Variable x, out Set dst)
        {
            if (TryGCDSolve(expr, x, out var res))
            {
                dst = (Set)AnalyticalEquationSolver.Solve(res, x).InnerSimplified;
                return true;
            }
            else
            {
                dst = Set.Empty;
                return false;
            }
        }

        private static bool TryGCDSolve(Entity expr, Variable x, out Entity res)
        {
            res = MathS.NaN;
            var terms = Sumf.LinearChildren(expr);
            var denominators = new Dictionary<string, (Entity den, Real pow)>();
            var fracs = new List<(Entity numerator, List<(Entity den, Real pow)> denominatorMultipliers)>();
            foreach (var term in terms)
            {
                var oneInfo = FindFractions(term, x);
                foreach (var denMp in oneInfo.denominatorMultipliers)
                {
                    var (den, pow) = denMp;
                    var name = den.Stringize(); // TODO: Replace with faster hashing
                    if (!denominators.ContainsKey(name))
                        denominators[name] = (den, 0);
                    denominators[name] = (den, Number.Max(denominators[name].pow, -pow));
                }
                fracs.Add(oneInfo);
            }

            // If there's no denominators or it's equal to 1, then we don't have to try to solve yet anymore
            if (denominators.Count == 0)
                return false;

            var newTerms = new List<Entity>();
            foreach (var (num, dens) in fracs)
            {
                var denDict = new Dictionary<string, (Entity den, Real pow)>();
                foreach (var (den, pow) in dens)
                    denDict[den.Stringize()] = (den, -pow);
                Entity invertDenominator = 1;
                foreach (var mp in denominators)
                    invertDenominator *= MathS.Pow(mp.Value.den, 
                        denDict.TryGetValue(mp.Key, out var denPow)
                        ? denominators[mp.Key].pow - denPow.pow
                        : denominators[mp.Key].pow);
                newTerms.Add(invertDenominator * num);
            }

            res = TreeAnalyzer.MultiHangBinary(newTerms, (a, b) => new Sumf(a, b)).InnerSimplified;
            return true;
        }
    }
}
