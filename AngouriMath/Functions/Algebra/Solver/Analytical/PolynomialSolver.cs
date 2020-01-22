using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AngouriMath.Convenience;
using AngouriMath.Core;
using AngouriMath.Core.TreeAnalysis;

namespace AngouriMath.Functions.Algebra.AnalyticalSolver
{
    internal static class PolynomialSolver
    {
        // solves ax2 + bx + c
        private static EntitySet SolveQuadratic(Entity a, Entity b, Entity c)
        {
            EntitySet res = new EntitySet();
            var D = MathS.Sqr(b) - 4 * a * c;
            res.Add((-b - MathS.Sqrt(D)) / (2 * a));
            res.Add((-b + MathS.Sqrt(D)) / (2 * a));
            return res;
        }

        // solves ax3 + bx2 + cx + d
        private static EntitySet SolveCubic(Entity a, Entity b, Entity c, Entity d)
        {
            // en: https://en.wikipedia.org/wiki/Cubic_equation
            // ru: https://ru.wikipedia.org/wiki/%D0%A4%D0%BE%D1%80%D0%BC%D1%83%D0%BB%D0%B0_%D0%9A%D0%B0%D1%80%D0%B4%D0%B0%D0%BD%D0%BE

            // TODO (to remove sympy code!)

            EntitySet res = new EntitySet();

            if (a.entType == Entity.EntType.NUMBER &&
                b.entType == Entity.EntType.NUMBER &&
                c.entType == Entity.EntType.NUMBER &&
                d.entType == Entity.EntType.NUMBER)
            {
                var p = ((3 * a * c - MathS.Sqr(b)) / (3 * MathS.Sqr(a))).Simplify();
                var q = ((2 * MathS.Pow(b, 3) - 9 * a * b * c + 27 * MathS.Sqr(a) * d) / (27 * MathS.Pow(a, 3)));

                var Q = MathS.Pow(p / 3, 3) + MathS.Sqr(q / 2);

                var alpha = (MathS.Pow(-q / 2 + MathS.Sqrt(Q), 1.0 / 3.0));
                var beta = (MathS.Pow(-q / 2 - MathS.Sqrt(Q), 1.0 / 3.0));

                if (p.entType == Entity.EntType.NUMBER && 
                    MathS.CanBeEvaluated(alpha) &&
                    MathS.CanBeEvaluated(beta))
                {
                    var beta3 = (-q / 2 - MathS.Sqrt(Q)).Eval();
                    alpha = alpha.Eval();
                    beta = beta.Eval();
                    var p3 = (-p / 3).Eval();
                    foreach (var root in Number.GetAllRoots(beta3, 3))
                        if ((root * alpha).Eval() == p3)
                        {
                            beta = root;
                            break;
                        }
                }
                var y1 = alpha + beta;
                var y2 = (-(alpha + beta) / 2 + MathS.i * (alpha - beta) / 2 * MathS.Sqrt(3));
                var y3 = (-(alpha + beta) / 2 - MathS.i * (alpha - beta) / 2 * MathS.Sqrt(3));

                var x1 = y1 - b / (3 * a);
                var x2 = y2 - b / (3 * a);
                var x3 = y3 - b / (3 * a);

                res.Add(x1);
                res.Add(x2);
                res.Add(x3);

                return res;
            }
            else
            {
                // TO REMOVE
                var coeff = MathS.i * MathS.Sqrt(3) / 2;

                var u1 = new NumberEntity(1);
                var u2 = SySyn.Rational(-1, 2) + coeff;
                var u3 = SySyn.Rational(-1, 2) - coeff;
                var D0 = MathS.Sqr(b) - 3 * a * c;
                var D1 = 2 * MathS.Pow(b, 3) - 9 * a * b * c + 27 * MathS.Sqr(a) * d;
                var C = MathS.Pow((D1 + MathS.Sqrt(MathS.Sqr(D1) - 4 * MathS.Pow(D0, 3))) / 2, 1.0 / 3);

                foreach (var uk in new List<Entity> { u1, u2, u3 })
                    res.Add(-(b + uk * C + D0 / C / uk) / 3 / a);
                return res;
            }
        }

        // solves hx4 + ax3 + bx2 + cx + d
        private static EntitySet SolveQuartic(Entity A, Entity B, Entity C, Entity D, Entity E)
        {
            // en: https://en.wikipedia.org/wiki/Quartic_function
            // ru: https://ru.wikipedia.org/wiki/%D0%9C%D0%B5%D1%82%D0%BE%D0%B4_%D0%A4%D0%B5%D1%80%D1%80%D0%B0%D1%80%D0%B8

            EntitySet res = new EntitySet();

            var alpha = -3 * MathS.Sqr(B) / (8 * MathS.Sqr(A)) + C / A;
            var beta = MathS.Pow(B, 3) / (8 * MathS.Pow(A, 3)) - (B * C) / (2 * MathS.Sqr(A)) + D / A;
            var gamma = -3 * MathS.Pow(B, 4) / (256 * MathS.Pow(A, 4)) + MathS.Sqr(B) * C / (16 * MathS.Pow(A, 3)) - (B * D) / (4 * MathS.Sqr(A)) + E / A;

            var P = -MathS.Sqr(alpha) / 12 - gamma;
            var Q = -MathS.Pow(alpha, 3) / 108 + alpha * gamma / 3 - MathS.Sqr(beta) / 8;
            var R = -Q / 2 + MathS.Sqrt(MathS.Sqr(Q) / 4 + MathS.Pow(P, 3) / 27);
            var U = MathS.Pow(R, 1.0 / 3);
            if (MathS.CanBeEvaluated(U))
                U = U.Eval();  // further, we will compare it to 0
            var y = SySyn.Rational(-5, 6) * alpha + U + (U == 0 ? -MathS.Pow(Q, 1.0 / 3) : -P / (3 * U));
            var W = MathS.Sqrt(alpha + 2 * y);
           
            // Now we need to permutate all four combinations
            for (int s = -1; s < 1; s += 2)
                for (int t = -1; t < 1; t += 2)
                {
                    var x = -B / (4 * A) + (s * W + t * MathS.Sqrt(-(3 * alpha + 2 * y + s * 2 * beta / W))) / 2;
                    res.Add(x);
                }

           return res;
        }

        internal static Dictionary<int, Entity> GatherMonomialInformation(List<Entity> terms, Entity subtree)
        {
            var monomialsByPower = new Dictionary<int, Entity>();
            // here we fill the dictionary with information about monomials' coefficiants
            foreach (var child in terms)
            {
                Entity free;
                int pow;
                //{(-1) * (-1) * momo * goose * quack * quack * x * x}
                ParseMonomial(subtree, child, out free, out pow);
                if (free == null)
                    return null;
                if (!monomialsByPower.ContainsKey(pow))
                    monomialsByPower[pow] = 0;
                monomialsByPower[pow] += free;
            }
            foreach (var key in monomialsByPower.Keys.ToList())
                monomialsByPower[key] = monomialsByPower[key].Simplify();
            return monomialsByPower;
        }

        /* e. g. x or cos(x), actually, relative to what we're checking whether the equation is polynomial*/
        internal static EntitySet SolveAsPolynomial(Entity expr, Entity subtree)
        {
            expr = expr.Expand(); // (x + 1) * x => x^2 + x
            List<Entity> children;
            EntitySet res = new EntitySet();
            if (expr.entType == Entity.EntType.OPERATOR && expr.Name == "sumf" || expr.Name == "minusf")
                children = TreeAnalyzer.LinearChildren(expr, "sumf", "minusf", Const.FuncIfSum);
            else
                children = new List<Entity> { expr };
            // Check if all are like {1} * x^n & gather information about them
            var monomialsByPower = GatherMonomialInformation(children, subtree);

            if (monomialsByPower == null)
                return null; // meaning that the given equation is not polynomial

            Entity GetMonomialByPower(int power)
            {
                return monomialsByPower.ContainsKey(power) ? monomialsByPower[power] : 0;
            }

            var powers = new List<int>(monomialsByPower.Keys);
            if (powers.Count == 0)
                return null;
            powers.Sort();
            if (powers.Last() == 0)
                return null;
            if (powers.Last() > 4 && powers.Count > 2)
                return null; // So far, we can't solve equations of powers more than 4
            if (powers.Count == 1)
            {
                res.Add(0);
                return res;
            }
            else if (powers.Count == 2)
            {
                // Provided a x ^ n + b x ^ m = 0
                // a = -b x ^ (m - n)
                // (- a / b) ^ (1 / (m - n)) = x
                // x ^ (m - n) = (-a / b)
                res.AddRange(TreeAnalyzer.FindInvertExpression(MathS.Pow(subtree, powers[1] - powers[0]), (-1 * monomialsByPower[powers[0]] / monomialsByPower[powers[1]]).Simplify(), subtree));
                return res;
            }
            // By this moment we know for sure that expr's power is <= 4, that expr is not a monomial,
            // and that it consists of more than 2 monomials
            else if (powers.Last() == 2)
            {
                var a = GetMonomialByPower(2);
                var b = GetMonomialByPower(1);
                var c = GetMonomialByPower(0);

                return SolveQuadratic(a, b, c);
            }
            else if (powers.Last() == 3)
            {
                var a = GetMonomialByPower(3);
                var b = GetMonomialByPower(2);
                var c = GetMonomialByPower(1);
                var d = GetMonomialByPower(0);

                return SolveCubic(a, b, c, d);
            }
            else if (powers.Last() == 4)
            {
                var a = GetMonomialByPower(4);
                var b = GetMonomialByPower(3);
                var c = GetMonomialByPower(2);
                var d = GetMonomialByPower(1);
                var e = GetMonomialByPower(0);

                return SolveQuartic(a, b, c, d, e);
            }
            // TODO maybe throw exception here?
            // Maybe, who knows...
            else return null;
        }

        internal static void ParseMonomial(Entity aVar, Entity expr, out Entity freeMono, out int power)
        {
            freeMono = 1; // a * b
            power = 0;  // x ^ 3
            foreach(var mp in TreeAnalyzer.LinearChildren(expr, "mulf", "divf", Const.FuncIfMul))
                if (mp.entType == Entity.EntType.OPERATOR && 
                    mp.Name == "powf" && 
                    mp.Children[0] == aVar)
                {
                    // x ^ a is bad
                    if (!(mp.Children[1].entType == Entity.EntType.NUMBER))
                    {
                        freeMono = null;
                        power = 0;
                        return;
                    }

                    // x ^ 0.3 is bad
                    if (!mp.Children[1].GetValue().IsInteger())
                    {
                        freeMono = null;
                        power = 0;
                        return;
                    }

                    power += (int)Math.Round(mp.Children[1].GetValue().Re);
                }
                else if (mp == aVar)
                {
                    power++;
                }
                else
                {
                    // a ^ x, (a + x) etc. are bad
                    if (mp.FindSubtree(aVar) != null)
                    {
                        freeMono = null;
                        power = 0;
                        return;
                    }
                    freeMono *= mp;
                }
            freeMono = freeMono.Simplify();
        }
    }
}
