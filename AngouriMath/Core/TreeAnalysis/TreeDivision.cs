
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
using System.Linq;
using AngouriMath.Core.Numerix;
using PeterO.Numbers;

namespace AngouriMath.Core.TreeAnalysis
{
    using static Entity;
    internal static partial class TreeAnalyzer
    {
        /// <summary>Returns information about all monomials of an expression</summary>
        internal static PolyInfo GatherAllPossiblePolynomials(Entity expr, bool replaceVars)
        {
            // Init
            var res = new PolyInfo();

            if (replaceVars)
            {
                // Replace all variables we can
                foreach (var varMentioned in expr.Vars)
                    res.AddReplacement(varMentioned, GetMinimumSubtree(expr, varMentioned));
                expr = expr.Substitute(res.Replacements);
            }

            // Gather info about each var as if this var was the only argument of the polynomial P(x)
            var children = Sumf.LinearChildren(expr);
            foreach (var varMentioned in expr.Vars)
                res.AddMonoInfo(varMentioned,
                    Functions.Algebra.AnalyticalSolving.PolynomialSolver.GatherMonomialInformation<EDecimal>(children, varMentioned));
            return res;
        }
        internal class PolyInfo
        {
            readonly Dictionary<Var, Dictionary<EDecimal, Entity>> monoInfo = new();
            readonly Dictionary<Var, Entity> revertReplacements = new();
            readonly Dictionary<Entity, Var> replacements = new();
            public IReadOnlyDictionary<Var, Dictionary<EDecimal, Entity>> MonoInfo => monoInfo;
            public IReadOnlyDictionary<Entity, Var> Replacements => replacements;
            public IReadOnlyDictionary<Var, Entity> RevertReplacements => revertReplacements;
            public void AddReplacement(Var variable, Entity value)
            {
                variable = new(variable.Name + "_r");
                replacements.Add(value, variable);
                revertReplacements.Add(variable, value);
            }
            public void AddMonoInfo(Var variable, Dictionary<EDecimal, Entity>? powers)
            {
                if (powers is { }) monoInfo.Add(variable, powers);
            }
        }
        /// <summary>
        /// Divides one polynom over another one
        /// </summary>
        internal static (Entity Divided, Entity Remainder) DivideAndRemainderPolynoms(Entity p, Entity q)
        {
            // ---> (x^0.6 + 2x^0.3 + 1) / (x^0.3 + 1)
            var replacementInfo = GatherAllPossiblePolynomials(p + q, replaceVars: true);

            var originalP = p;
            var originalQ = q;

            // TODO remove extra copy by making GatherAllPossiblePolynomials accept multiple polynomials
            p = p.Substitute(replacementInfo.Replacements);
            q = q.Substitute(replacementInfo.Replacements);

            var monoinfoQ = GatherAllPossiblePolynomials(q.Expand(), replaceVars: false).MonoInfo;
            var monoinfoP = GatherAllPossiblePolynomials(p.Expand(), replaceVars: false).MonoInfo;

            Var? polyvar = null;

            // TODO use Linq to find polyvar
            // First attempt to find polynoms
            foreach (var pair in monoinfoQ)
            {
                if (pair.Value != null && monoinfoP.ContainsKey(pair.Key) && monoinfoP[pair.Key] != null)
                {
                    polyvar = pair.Key;
                    break;
                }
            }
            // cannot divide, return unchanged
            if (polyvar is null) return (Divided: originalP / originalQ, Remainder: 0);

            var maxpowQ = monoinfoQ[polyvar].Keys.Max();
            var maxpowP = monoinfoP[polyvar].Keys.Max();
            var maxvalQ = monoinfoQ[polyvar][maxpowQ];
            var maxvalP = monoinfoP[polyvar][maxpowP];

            var result = new Dictionary<EDecimal, Entity>();

            // TODO: add case where all powers are non-positive
            // for now just return polynomials unchanged
            if (maxpowP.LessThan(maxpowQ)) return (Divided: originalP / originalQ, Remainder: 0);

            // possibly very long process
            while (maxpowP.GreaterThanOrEquals(maxpowQ))
            {
                // KeyPair is ax^n with Key=n, Value=a
                EDecimal deltapow = maxpowP - maxpowQ;
                Entity deltamul = maxvalP / maxvalQ;
                result[deltapow] = deltamul;

                foreach (var n in monoinfoQ[polyvar])
                {
                    // TODO: precision loss can happen here. MUST be fixed somehow
                    EDecimal newpow = deltapow + n.Key;
                    if (!monoinfoP[polyvar].ContainsKey(newpow))
                    {
                        monoinfoP[polyvar][newpow] = -deltamul * n.Value;
                    }
                    else
                    {
                        monoinfoP[polyvar][newpow] -= deltamul * n.Value;
                    }
                }
                if (monoinfoP[polyvar].ContainsKey(maxpowP))
                    monoinfoP[polyvar].Remove(maxpowP);
                if (monoinfoP[polyvar].Count == 0)
                    break;

                maxpowP = monoinfoP[polyvar].Keys.Max();
                maxvalP = monoinfoP[polyvar][maxpowP];
            }

            // check if all left in P is zero. If something left, division is impossible => return P / Q

            Entity rest = 0;
            Entity Zero = 0;
            foreach (var coef in monoinfoP[polyvar])
            {
                var simplified = coef.Value.Simplify();
                if (simplified != Zero)
                    rest += simplified * MathS.Pow(polyvar, coef.Key);
            }

            rest /= q;

            Entity res = IntegerNumber.Create(0);

            foreach (var pair in result)
            {
                res += pair.Value.Simplify(5) * MathS.Pow(polyvar, pair.Key);
            }
            res = res.Substitute(replacementInfo.RevertReplacements);
            rest = rest.Substitute(replacementInfo.RevertReplacements);
            return (res, rest);
        }
    }
}
