using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private static EntitySet SolveQubic(Entity a, Entity b, Entity c, Entity d)
        {
            // en: https://en.wikipedia.org/wiki/Cubic_equation
            // ru: https://ru.wikipedia.org/wiki/%D0%A4%D0%BE%D1%80%D0%BC%D1%83%D0%BB%D0%B0_%D0%9A%D0%B0%D1%80%D0%B4%D0%B0%D0%BD%D0%BE

            EntitySet res = new EntitySet();

            var p = ((3 * a * c - MathS.Sqr(b)) / (3 * MathS.Sqr(a)));
            var q = ((2 * MathS.Pow(b, 3) - 9 * a * b * c + 27 * MathS.Sqr(a) * d) / (27 * MathS.Pow(a, 3)));

            var Q = (MathS.Pow(p / 3, 3) + MathS.Sqr(q / 2));

            var alpha = (MathS.Pow(-q / 2 + MathS.Sqrt(Q), 1.0 / 3.0));
            var beta = (MathS.Pow(-q / 2 - MathS.Sqrt(Q), 1.0 / 3.0));

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

        // solves hx4 + ax3 + bx2 + cx + d
        private static EntitySet SolveQuartic(Entity h, Entity a, Entity b, Entity c, Entity d)
        {
            // en: https://en.wikipedia.org/wiki/Quartic_function
            // ru: https://ru.wikipedia.org/wiki/%D0%9C%D0%B5%D1%82%D0%BE%D0%B4_%D0%A4%D0%B5%D1%80%D1%80%D0%B0%D1%80%D0%B8

            EntitySet res = new EntitySet();
            // transform to the form x4 + ax3 + bx2 + cx + d
            a = a / h;
            b = b / h;
            c = c / h;
            d = d / h;
            // find roots of equation y3 - b * y2 + (ac - 4d) * y - (a2 * d + 4bd - c2)

            var roots = SolveQubic(1, b, (a * c - 4 * d), (MathS.Sqr(a) * d + 4 * b * d - MathS.Sqr(c)));
            //TO DO: peek any root from [roots] to solve equation

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

                return SolveQubic(a, b, c, d);
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
            expr = expr.Simplify(); // x * x => x ^ 2
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
