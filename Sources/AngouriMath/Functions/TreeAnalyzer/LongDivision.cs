//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;
using Antlr4.Runtime.Misc;
using PeterO.Numbers;

namespace AngouriMath.Functions
{
    using static Entity;
    using static Entity.Number;
    internal static partial class TreeAnalyzer
    {
        internal sealed class PolynomialInformation
        {
            private readonly Dictionary<Variable, Dictionary<EDecimal, Entity>> monoInfo = new();
            private readonly Dictionary<Entity, Variable> replacements = new();
            private readonly Dictionary<Variable, Entity> revertReplacements = new();
            public IReadOnlyDictionary<Variable, Dictionary<EDecimal, Entity>> MonoInfo => monoInfo;
            public IReadOnlyDictionary<Entity, Variable> Replacements => replacements;
            public IReadOnlyDictionary<Variable, Entity> RevertReplacements => revertReplacements;
            public void AddReplacement(IEnumerable<Variable> existingVars, Entity value)
            {
                var variable = Variable.CreateTemp(existingVars.Concat(revertReplacements.Keys));
                replacements[value] = variable;
                revertReplacements[variable] = value;
            }
            public void AddMonoInfo(Variable variable, Dictionary<EDecimal, Entity>? powers)
            {
                if (powers is { }) monoInfo.Add(variable, powers);
            }
        }

        internal static PolynomialInformation GatherAllPossiblePolynomials(Entity expr, bool replaceVars)
        {
            // Init
            var res = new PolynomialInformation();

            if (replaceVars)
            {
                // Replace all variables we can
                foreach (var varMentioned in expr.Vars)
                    res.AddReplacement(expr.Vars, GetMinimumSubtree(expr, varMentioned));
                expr = expr.Substitute(res.Replacements);
            }

            // Gather info about each var as if this var was the only argument of the polynomial P(x)
            var children = Sumf.LinearChildren(expr);
            foreach (var varMentioned in expr.Vars)
                res.AddMonoInfo(varMentioned, Algebra.AnalyticalSolving.PolynomialSolver.GatherMonomialInformation
                   <EDecimal, PrimitiveDecimal>(children, varMentioned));
            return res;
        }

        /// <summary>
        /// A power in the one form that equal powers share. <see cref="EDecimal"/> keeps the
        /// scale it was written or computed at, and is equal only to an <see cref="EDecimal"/>
        /// of that same scale -- so <c>2</c> and <c>2.0</c> are the same number and different
        /// dictionary keys. Removing the trailing zeros is what makes them one key.
        /// </summary>
        /// <remarks>
        /// The unlimited context, so that reducing a power never rounds it. Rounding here
        /// would merge two powers that are genuinely different, which is the opposite
        /// mistake and a worse one.
        /// </remarks>
        private static EDecimal Canonical(EDecimal power) => power.Reduce(EContext.Unlimited);

        private static Dictionary<EDecimal, Entity> Canonicalize(Dictionary<EDecimal, Entity> powers)
        {
            var canonical = new Dictionary<EDecimal, Entity>();
            foreach (var pair in powers)
            {
                var power = Canonical(pair.Key);
                // Two powers of the source may reduce to the same one, and then they are one
                // monomial and their coefficients add. Overwriting instead would silently
                // drop a term.
                canonical[power] = canonical.TryGetValue(power, out var already)
                    ? already + pair.Value : pair.Value;
            }
            return canonical;
        }

        /// <summary>
        /// Divides one polynomial over another one:
        /// <a href="https://en.wikipedia.org/wiki/Polynomial_long_division"/>
        /// </summary>
        internal static (Entity Divided, Entity Remainder)? PolynomialLongDivision(Entity p, Entity q)
        {
            if (!p.Vars.Any() || !q.Vars.Any())
                return null; // There are no variables to find polynomial as

            // ---> (x^0.6 + 2x^0.3 + 1) / (x^0.3 + 1)
            var replacementInfo = GatherAllPossiblePolynomials(p + q, replaceVars: true);

            var originalP = p;
            var originalQ = q;

            // TODO: remove extra call to GatherAllPossiblePolynomials above with p + q
            // by making GatherAllPossiblePolynomials accept multiple polynomials
            p = p.Substitute(replacementInfo.Replacements);
            q = q.Substitute(replacementInfo.Replacements);

            var monoinfoP = GatherAllPossiblePolynomials(p.Expand(), replaceVars: false).MonoInfo;
            var monoinfoQ = GatherAllPossiblePolynomials(q.Expand(), replaceVars: false).MonoInfo;

            // First attempt to find polynoms
            var polyvar = monoinfoP.Keys.FirstOrDefault(monoinfoQ.ContainsKey);
            // cannot divide, return unchanged
            if (polyvar is null) return null;

            // The powers are dictionary keys, and EDecimal is equal only to an EDecimal of the
            // same scale: 2 and 2.0 are the same number and not the same key. Every power
            // arrived at by arithmetic below carries the scale of that arithmetic, so
            // 1.5 + 0.5 comes out as 2.0 and misses the 2 already in the dictionary. Reduced
            // once, here, so that every key in play is in the one canonical form and the
            // lookups below mean what they say. https://github.com/asc-community/AngouriMath/issues/751
            var powersOfP = Canonicalize(monoinfoP[polyvar]);
            var powersOfQ = Canonicalize(monoinfoQ[polyvar]);
            var maxpowP = powersOfP.Keys.Max() ?? throw new AngouriBugException("No null expected");
            var maxpowQ = powersOfQ.Keys.Max() ?? throw new AngouriBugException("No null expected");
            var maxvalP = powersOfP[maxpowP];
            var maxvalQ = powersOfQ[maxpowQ];

            // TODO: add case where all powers are non-positive
            // for now just return polynomials unchanged
            if (maxpowP.LessThan(maxpowQ)) return null;

            var result = new Dictionary<EDecimal, Entity>();
            // possibly very long process
            while (maxpowP.GreaterThanOrEquals(maxpowQ))
            {
                // KeyPair is ax^n with Key=n, Value=a
                var deltapow = Canonical(maxpowP - maxpowQ);
                var deltamul = maxvalP / maxvalQ;
                result[deltapow] = deltamul;

                foreach (var n in powersOfQ)
                {
                    var newpow = Canonical(deltapow + n.Key);
                    if (powersOfP.TryGetValue(newpow, out var existing))
                        powersOfP[newpow] = existing - deltamul * n.Value;
                    else
                        powersOfP[newpow] = -deltamul * n.Value;
                }
                _ = powersOfP.Remove(maxpowP);
                if (powersOfP.Count == 0)
                    break;

                maxpowP = powersOfP.Keys.Max() ?? throw new AngouriBugException("No null expected");
                maxvalP = powersOfP[maxpowP];
            }

            // check if all left in P is zero. If something left, division is impossible => return P / Q
            Entity rest = 0;
            foreach (var coef in powersOfP)
                if (coef.Value.Simplify() is not Integer(0) and var simplified)
                    if (coef.Key.IsZero) // Don't insert unnecessary x^0 because it's undefined for x=0
                        rest += simplified;
                    else
                        rest += simplified * MathS.Pow(polyvar, coef.Key);
            rest /= q;

            Entity res = 0;
            foreach (var pair in result)
                if (pair.Key.IsZero) // Don't insert unnecessary x^0 because it's undefined for x=0
                    res += pair.Value.Simplify(5);
                else
                    res += pair.Value.Simplify(5) * MathS.Pow(polyvar, pair.Key);
            return (res.Substitute(replacementInfo.RevertReplacements),
                    rest.Substitute(replacementInfo.RevertReplacements));
        }
    }
}
