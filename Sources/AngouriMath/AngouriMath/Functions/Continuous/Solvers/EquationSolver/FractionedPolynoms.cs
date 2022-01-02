//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Multithreading;

namespace AngouriMath.Functions.Algebra.AnalyticalSolving
{
    using static Entity;
    using static Entity.Number;
    internal static class FractionedPolynoms
    {
        internal static bool TrySolve(Entity expr, Variable x, out Set dst)
        {
            dst = Set.Empty;
            var children = TreeAnalyzer.GatherLinearChildrenOverSumAndExpand(
                expr, entity => entity.ContainsNode(x)
            );

            if (children is null)
                return false;

            Entity normalPolynom = 0;
            var fractioned = new List<(Entity multiplier, List<(Entity main, Integer pow)> fracs)>();

            // Use PowerRules to replace sqrt(f(x))^2 => f(x)
            foreach (var child in children.Select(c => c.InnerSimplified.Replace(Patterns.PowerRules)))
            {
                (Entity multiplier, List<(Entity main, Integer pow)> fracs) potentialFraction;
                potentialFraction.multiplier = 1;
                potentialFraction.fracs = new List<(Entity main, Integer pow)>();
                foreach (var mpChild in Mulf.LinearChildren(child))
                {
                    if (!(mpChild is Powf(var @base, Number num and not Integer))) // (x + 3) ^ 3
                    {
                        potentialFraction.multiplier *= mpChild;
                        continue;
                    }
                    if (num is not Rational fracNum)
                        return false; // (x + 1)^0.2348728
                    var newChild = MathS.Pow(@base, fracNum.ERational.Numerator).InnerSimplified;
                    var den = fracNum.ERational.Denominator;
                    potentialFraction.fracs.Add((newChild, den));

                    MultithreadingFunctional.ExitIfCancelled();
                }

                if (potentialFraction.fracs.Count > 0)
                    fractioned.Add(potentialFraction);
                else
                    normalPolynom += child;
            }

            if (fractioned.Count == 0)
                return false; // means that one can either be solved polynomially or unsolvable at all

            // starting from i = 1 check if all are equal to [0]
            static bool BasesAreEqual(List<(Entity main, Integer pow)> f1,
                List<(Entity main, Integer pow)> f2)
            {
                if (f1.Count != f2.Count)
                    return false;
                for (int i = 0; i < f1.Count; i++)
                    if (f1[i].main != f2[i].main || f1[i].pow != f2[i].pow)
                        return false;
                return true;
            }
            for (int i = 1; i < fractioned.Count; i++)
            {
                if (BasesAreEqual(fractioned[i].fracs, fractioned[0].fracs))
                {
                    var were = fractioned[0];
                    fractioned[0] = (were.multiplier + fractioned[i].multiplier, were.fracs);
                }
                else
                    return false;
            }

            var (multiplier, fracs) = fractioned[0];

            var lcm = fracs.Select(c => c.pow.EInteger).Aggregate((aggregate, current) => aggregate.Lcm(current));
            var intLcm = Integer.Create(lcm);

            //                        "-" to compensate sum: x + sqrt(x + 1) = 0 => x = -sqrt(x+1)
            var mp = MathS.Pow(-multiplier, intLcm).InnerSimplified;
            foreach (var (main, pow) in fracs)
                mp *= MathS.Pow(main, Integer.Create(lcm.Divide(pow.EInteger)));

            MultithreadingFunctional.ExitIfCancelled();

            var finalExpr = MathS.Pow(normalPolynom, intLcm) - mp;

            dst = (Set)AnalyticalEquationSolver.Solve(finalExpr, x).InnerSimplified;
            return true;
        }
    }
}
