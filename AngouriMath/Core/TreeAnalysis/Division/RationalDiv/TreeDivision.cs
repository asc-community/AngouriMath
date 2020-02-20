using AngouriMath.Core.TreeAnalysis.Division.RationalDiv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AngouriMath.Core.TreeAnalysis
{
    internal static partial class TreeAnalyzer
    {
        internal static void FindDivisors(ref Entity expr, Func<Entity, Entity, bool> cond)
        {
            for (int i = 0; i < expr.Children.Count; i++)
            {
                // TODO
                var temporaryElement = expr.Children[i];
                FindDivisors(ref temporaryElement, cond);
                expr.Children[i] = temporaryElement;
            }
            if (expr.entType == Entity.EntType.OPERATOR && expr.Name == "divf")
                if (cond(expr.Children[0], expr.Children[1]))
                    expr = DividePolynoms(expr.Children[0], expr.Children[1]);
        }

        internal static Entity DividePolynoms(Entity p, Entity q)
        {
            // ---> (x^0.6 + 2x^0.3 + 1) / (x^0.3 + 1)
            var replacementInfo = GatherAllPossiblePolynomials(p + q, replaceVars: true).replacementInfo;

            var originalP = p;
            var originalQ = q;

            // TODO remove extra copy
            p = p.DeepCopy();
            q = q.DeepCopy();

            foreach (var pair in replacementInfo)
            {
                FindAndReplace(ref p, pair.Value, new VariableEntity(PolyInfo.NewVarName(pair.Key)));
                FindAndReplace(ref q, pair.Value, new VariableEntity(PolyInfo.NewVarName(pair.Key)));
            }

            var monoinfoQ = GatherAllPossiblePolynomials(q.Expand(), replaceVars: false).monoInfo;
            var monoinfoP = GatherAllPossiblePolynomials(p.Expand(), replaceVars: false).monoInfo;

            string polyvar = null;

            // TODO use Linq to find polyvar
            // First attempt to find polynoms
            foreach(var pair in monoinfoQ)
            {
                if(pair.Value != null && monoinfoP.ContainsKey(pair.Key) && monoinfoP[pair.Key] != null)
                {
                    polyvar = pair.Key;
                    break;
                }
            }
            /*
            if (string.IsNullOrEmpty(polyvar))
            {
                // Second attempt to find polynoms
                foreach (var pair in monoinfoQ)
                {
                    if (pair.Value != null && (!monoinfoP.ContainsKey(pair.Key) || monoinfoP[pair.Key] != null))
                    {
                        polyvar = pair.Key;
                        break;
                    }
                }
            }
            */
            // cannot divide, return unchanged
            if (string.IsNullOrEmpty(polyvar)) return originalP / originalQ;

            var maxpowQ = monoinfoQ[polyvar].Keys.Max();
            var maxpowP = monoinfoP[polyvar].Keys.Max();
            var maxvalQ = monoinfoQ[polyvar][maxpowQ];
            var maxvalP = monoinfoP[polyvar][maxpowP];

            var result = new Dictionary<double, Entity>();

            // TODO: add case where all powers are non-positive
            // for now just return polynomials unchanged
            if (maxpowP < maxpowQ) return originalP / originalQ;

            // possibly very long process
            while (maxpowP >= maxpowQ)
            {
                // KeyPair is ax^n with Key=n, Value=a
                double deltapow = maxpowP - maxpowQ;
                Entity deltamul = maxvalP / maxvalQ;
                result[deltapow] = deltamul;

                foreach (var n in monoinfoQ[polyvar])
                {
                    // TODO: precision loss can happen here. MUST be fixed somehow
                    double newpow = deltapow + n.Key;
                    if (!monoinfoP[polyvar].ContainsKey(newpow))
                    {
                        monoinfoP[polyvar][newpow] = -deltamul * n.Value;
                    }
                    else
                    { 
                        monoinfoP[polyvar][newpow] -= deltamul * n.Value;
                    }
                    /*
                    monoinfoP[polyvar][newpow] = monoinfoP[polyvar][newpow].SimplifyIntelli();
                    if(monoinfoP[polyvar][newpow] == 0)
                        monoinfoP[polyvar].Remove(newpow);
                    */
                }
                if (monoinfoP[polyvar].ContainsKey(maxpowP))
                    monoinfoP[polyvar].Remove(maxpowP);
                if (monoinfoP[polyvar].Count == 0)
                    break;

                maxpowP = monoinfoP[polyvar].Keys.Max();
                maxvalP = monoinfoP[polyvar][maxpowP];
            }

            // check if all left in P is zero. If something left, division is impossible => return P / Q
            var Zero = new NumberEntity(0);
            foreach(var coef in monoinfoP[polyvar])
            {
                var simplified = coef.Value.Simplify();
                if (simplified != Zero)
                    return originalP / originalQ;
            }

            Entity res = new Number(0);
            foreach(var pair in result)
            {
                res += pair.Value.Simplify(5) * MathS.Pow(new VariableEntity(polyvar), pair.Key);
            }
            // TODO: we know that variable is the same but with suffux '_r'. This foreach loop can be speeded-up
            foreach (var subst in replacementInfo)
            {
                FindAndReplace(ref res, new VariableEntity(PolyInfo.NewVarName(subst.Key)), subst.Value);
            }
            return res;
        }
    }
}
