//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Collections.Generic;
using System.Linq;
using AngouriMath.Functions.Boolean;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Functions.Algebra.AnalyticalSolving
{
    /// <summary>
    /// <c>{ n in ZZ : 2^n > n^2 }</c>, an inequality with an exponential or a factorial over
    /// the whole numbers, is searched over a window from the least member and its tails are
    /// proved: the members are the points of the window where it holds, and the whole numbers
    /// from the start of the final run where the quantifier decides it holds there -- by
    /// induction, which is how Sullivan and Mackey do it (Ex 5.3.2: <c>{0, 1} \/ ZZ /\ [5; +oo)</c>).
    /// Over <c>ZZ</c> the other tail is proved the same way, downwards. A tail the quantifier
    /// leaves undecided leaves the set as written: a window is evidence about the window.
    /// https://github.com/asc-community/AngouriMath/issues/1409
    /// </summary>
    internal static class ThresholdSearch
    {
        /// <summary>How far from the least member, or from zero, the window reaches.</summary>
        private const int Window = 64;

        internal static Set? Solve(Variable x, Set integers, ComparisonSign inequality)
        {
            if (inequality is Equalsf || inequality.Vars.Any(v => v != x))
                return null;
            if (!inequality.Nodes.Any(node => node is Powf(_, var e) && e.ContainsNode(x) || node is Factorialf f && f.ContainsNode(x)))
                return null;
            if (integers is not SpecialSet special || !Quantifiers.IsIntegerSet(special))
                return null;
            var least = Quantifiers.LeastMember(special);
            var from = least is null ? -Window : least.EInteger.ToInt32Checked();
            var truth = new List<bool>();
            for (var n = from; n <= Window; n++)
            {
                if (inequality.Substitute(x, Integer.Create(n)).Evaled is not Entity.Boolean(var holds))
                    return null;
                truth.Add(holds);
            }
            // The final run, and the first one over ZZ, are what the quantifier is asked about.
            var last = truth.Count - 1;
            var tailStart = last;
            while (tailStart > 0 && truth[tailStart - 1] == truth[last])
                tailStart--;
            var upwards = truth[last] ? inequality : Negated(inequality);
            if (!Holds(upwards, x, from + tailStart, downwards: false))
                return null;
            var headEnd = 0;
            if (least is null)
            {
                while (headEnd < tailStart - 1 && truth[headEnd + 1] == truth[0])
                    headEnd++;
                if (!Holds(truth[0] ? inequality : Negated(inequality), x, from + headEnd, downwards: true))
                    return null;
            }
            // One run over the whole window, proved on every side, is the whole set.
            if (tailStart == 0 && truth[last])
                return special;
            if (tailStart == 0)
                return Empty;
            var listed = new List<Entity>();
            for (var i = least is null ? headEnd + 1 : 0; i < tailStart; i++)
                if (truth[i])
                    listed.Add(Integer.Create(from + i));
            Set result = new FiniteSet(listed);
            if (least is null && truth[0])
                result = result.Unite(special.Intersect(new Interval(Real.NegativeInfinity, false, Integer.Create(from + headEnd), true)));
            if (truth[last])
                result = result.Unite(special.Intersect(new Interval(Integer.Create(from + tailStart), true, Real.PositiveInfinity, false)));
            return result;
        }

        /// <summary>Whether the statement holds at every whole number from <paramref name="start"/> on, or down from it.</summary>
        private static bool Holds(Entity statement, Variable x, int start, bool downwards)
        {
            var t = Variable.CreateUnique(statement, "t");
            Entity shifted = downwards ? Integer.Create(start) - t : Integer.Create(start) + t;
            var body = statement.Substitute(x, shifted).InnerSimplified;
            return Quantifiers.Decide(Quantifiers.Kind.All, t, MathS.Sets.NonNegativeIntegers, body, true) is Entity.Boolean(true);
        }

        private static ComparisonSign Negated(ComparisonSign inequality)
            => inequality switch
            {
                Greaterf(var a, var b) => new LessOrEqualf(a, b),
                GreaterOrEqualf(var a, var b) => new Lessf(a, b),
                Lessf(var a, var b) => new GreaterOrEqualf(a, b),
                LessOrEqualf(var a, var b) => new Greaterf(a, b),
                _ => throw new Core.Exceptions.AngouriBugException("An equation is refused above"),
            };
    }
}
