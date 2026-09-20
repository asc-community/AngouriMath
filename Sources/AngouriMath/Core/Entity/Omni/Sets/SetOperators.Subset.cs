//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Collections.Generic;
using System.Linq;
using AngouriMath.Core.Exceptions;
using AngouriMath.Functions.Boolean;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Core.Sets
{
    internal static partial class SetOperators
    {
        /// <summary>
        /// Whether every member of <paramref name="sub"/> is a member of <paramref name="super"/>,
        /// as a truth value, or <see langword="null"/> where that is not settled. Both sets arrive
        /// already simplified. The routes, in order: what the two shapes say outright, what the
        /// set algebra says about a union, an intersection, a difference or a power set on either
        /// side, the members of a finite set one by one, the chain the special sets sit on, the
        /// ends of two intervals, the difference <c>sub \ super</c> where it evaluates, and last
        /// the statement <c>forall x in sub : x in super</c>, which the quantifier decision
        /// answers with the solver where the membership reads as a comparison.
        /// https://github.com/asc-community/AngouriMath/issues/1409
        /// </summary>
        internal static Entity? Subset(Entity sub, Entity super, bool isExact)
            => SubsetVerdict(sub, super, isExact) switch
            {
                true => Entity.Boolean.True,
                false => Entity.Boolean.False,
                null => null,
            };

        /// <summary>
        /// The two sides may be symbols standing for sets, <c>A /\ B subset A</c>, which the
        /// algebra decides without a member in sight; the routes that look at members need
        /// both sides to be sets.
        /// </summary>
        private static bool? SubsetVerdict(Entity sub, Entity super, bool isExact)
        {
            if (sub == super || sub is Set { IsSetEmpty: true })
                return true;
            if (super is Set { IsSetEmpty: true })
                return sub is FiniteSet or Interval { IsNumeric: true } or SpecialSet ? false : null;

            // The algebra, read off the operators before any member is looked at. Each arm
            // states an equivalence, or an implication that settles the answer one way and
            // leaves the other open.
            switch (sub, super)
            {
                case (Unionf(var a, var b), _):
                    return Both(SubsetVerdict(a, super, isExact), SubsetVerdict(b, super, isExact));
                case (_, Intersectionf(var a, var b)):
                    return Both(SubsetVerdict(sub, a, isExact), SubsetVerdict(sub, b, isExact));
                case (Powersetf(var a), Powersetf(var b)):
                    return SubsetVerdict(a, b, isExact);
            }
            switch (sub, super)
            {
                case (Intersectionf(var a, var b), _) when SubsetVerdict(a, super, isExact) == true || SubsetVerdict(b, super, isExact) == true:
                    return true;
                case (SetMinusf(var a, _), _) when SubsetVerdict(a, super, isExact) == true:
                    return true;
                case (_, Unionf(var a, var b)) when SubsetVerdict(sub, a, isExact) == true || SubsetVerdict(sub, b, isExact) == true:
                    return true;
                case (_, SetMinusf(var a, var b)) when SubsetVerdict(sub, a, isExact) == true
                    && (sub.Intersect(b).InnerSimplified(isExact) is Set met && met.IsSetEmpty):
                    return true;
            }
            if (sub is not Set subSet || super is not Set superSet)
                return null;
            return ByMembers(subSet, superSet, isExact);
        }

        private static bool? ByMembers(Set sub, Set super, bool isExact)
        {
            // A set builder whose predicate the solver settles to a list is that list: { x in RR :
            // x^2 = 1 } is {-1, 1}, and is then checked member by member.
            if (sub is ConditionalSet builder && Listed(builder) is { } solved)
                return Members(solved, super);
            switch (sub, super)
            {
                case (FiniteSet finite, _):
                    return Members(finite, super);
                case (SpecialSet a, SpecialSet b):
                    return RankInTheNumberChain(a) is { } rankA && RankInTheNumberChain(b) is { } rankB ? rankA <= rankB
                        : a is SpecialSet.Booleans || b is SpecialSet.Booleans ? false : null;
                case (Interval { IsNumeric: true } interval, SpecialSet special):
                    return IntervalWithin(interval, special);
                case (SpecialSet special, Interval { IsNumeric: true } interval):
                    // A number set is unbounded, so only the whole line holds one, and only a
                    // real one: the complex numbers are not on the line.
                    return RankInTheNumberChain(special) is { } rank
                        ? rank <= 4 && interval.Left.Evaled == Real.NegativeInfinity && interval.Right.Evaled == Real.PositiveInfinity
                        : null;
                case (Interval { IsNumeric: true } a, Interval { IsNumeric: true } b):
                    return IntervalWithin(a, b);
                case (Interval { IsNumeric: true } interval, FiniteSet):
                    // A proper interval has more members than any list; a single point is one.
                    return interval.Left.Evaled == interval.Right.Evaled
                        ? interval.LeftClosed && interval.RightClosed ? Members(new FiniteSet(interval.Left), super) : true
                        : false;
                case (SpecialSet, FiniteSet):
                    return false;
            }

            // The difference evaluates where the operators have an arm for the pair: empty is
            // yes, a listed member is no.
            if (sub.SetSubtract(super).InnerSimplified(isExact) is Set difference && !ReferenceEquals(difference, sub) && difference is not SetMinusf)
            {
                if (difference.IsSetEmpty)
                    return true;
                if (difference is FiniteSet { Count: > 0 } listed && listed.All(static member => member is Number))
                    return false;
            }

            // What is left is the definition: every x in sub is in super, put to the quantifier
            // decision with the membership spelled as the comparison it is where sub ranges over
            // a set the solver reads.
            var x = Variable.CreateTemp(sub.Vars.Concat(super.Vars));
            var membership = (MembershipAsComparison(x, super, sub) ?? x.In(super)).InnerSimplified;
            return Quantifiers.Decide(Quantifiers.Kind.All, x, sub, membership, isExact) switch
            {
                Entity.Boolean truth => truth == Entity.Boolean.True,
                _ => null,
            };
        }

        private static bool? Both(bool? left, bool? right)
            => (left, right) switch
            {
                (true, true) => true,
                (false, _) or (_, false) => false,
                _ => null,
            };

        /// <summary>
        /// The members of <c>{ x in S : p }</c> as a list, where <c>p</c> is an equation the
        /// solver answers with a finite set of numbers and each is decidably in or out of
        /// <c>S</c>; <see langword="null"/> otherwise.
        /// </summary>
        private static FiniteSet? Listed(ConditionalSet builder)
        {
            if (builder.DeclaredMembership is not (Set declared, var rest) || builder.Var is not Variable x)
                return null;
            if (!Quantifiers.Equational(rest) || !Quantifiers.SolverReads(rest, x))
                return null;
            try
            {
                if (rest.Solve(x) is not FiniteSet solutions || !solutions.All(static s => s.Evaled is Number))
                    return null;
                var members = new List<Entity>();
                foreach (var solution in solutions)
                {
                    var value = solution.InnerSimplified;
                    if (!declared.TryContains(value, out var inside))
                        return null;
                    if (inside)
                        members.Add(value);
                }
                return new FiniteSet(members);
            }
            catch (AngouriBugException) { throw; }
            catch (AngouriMathBaseException) { return null; }
        }

        /// <summary>Every listed member is in the set, or one is not, or one is undecided.</summary>
        private static bool? Members(FiniteSet finite, Set super)
        {
            var undecided = false;
            foreach (var member in finite)
            {
                if (!super.TryContains(member, out var contains))
                    undecided = true;
                else if (!contains)
                    return false;
            }
            return undecided ? null : true;
        }

        private static bool? IntervalWithin(Interval interval, SpecialSet special)
        {
            var left = (Real)interval.Left.Evaled;
            var right = (Real)interval.Right.Evaled;
            var single = left == right;
            if (single && !(interval.LeftClosed && interval.RightClosed))
                return true;
            if (single)
                return special.TryContains(left, out var contains) ? contains : null;
            if (left > right)
                return true;
            // A proper interval holds a number of every kind below the reals, so it sits inside
            // the reals and the complex numbers and inside nothing narrower.
            return RankInTheNumberChain(special) is { } rank ? rank >= 4 : special is SpecialSet.Booleans ? false : null;
        }

        private static bool? IntervalWithin(Interval a, Interval b)
        {
            var (aLeft, aRight) = ((Real)a.Left.Evaled, (Real)a.Right.Evaled);
            var (bLeft, bRight) = ((Real)b.Left.Evaled, (Real)b.Right.Evaled);
            if (aLeft > aRight || aLeft == aRight && !(a.LeftClosed && a.RightClosed))
                return true;
            var leftFits = aLeft > bLeft || aLeft == bLeft && (b.LeftClosed || !a.LeftClosed);
            var rightFits = aRight < bRight || aRight == bRight && (b.RightClosed || !a.RightClosed);
            return leftFits && rightFits;
        }

        /// <summary>
        /// <c>x in super</c> as the comparison the solver reads, for an <c>x</c> ranging over
        /// <paramref name="sub"/>: a listed set is one equality per member, an interval is its
        /// two bounds, and a number set on the chain
        /// is decided against the set <c>x</c> already ranges over -- wider is <c>True</c>, and an
        /// integer set inside another is the bound that separates them. <see langword="null"/>
        /// where the membership has no such spelling.
        /// </summary>
        private static Entity? MembershipAsComparison(Variable x, Set super, Set sub)
        {
            var over = sub is ConditionalSet { DeclaredMembership: (var declared, _) } ? declared as Set : sub;
            switch (super)
            {
                case ConditionalSet { DeclaredMembership: (var declaredOfSuper, var rest), Var: Variable y } builder
                    when declaredOfSuper is Set declaredSet:
                    // x in { y in S : p } is x in S and p(x); the first conjunct is read the
                    // same way, and where it has no reading the membership has none.
                    return MembershipAsComparison(x, declaredSet, sub) is { } inTheDeclared
                        ? inTheDeclared & rest.Substitute(y, x)
                        : null;
                case FiniteSet { Count: > 0 } listed when listed.All(static member => member is Number):
                    // A listed set of numbers is the disjunction of equalities, which the solver
                    // answers over any set.
                    return listed.Select(member => x.Equalizes(member)).Aggregate(static (a, b) => a | b);
                case Interval { IsNumeric: true } interval:
                    return (interval.LeftClosed ? x >= interval.Left : x > interval.Left)
                        & (interval.RightClosed ? x <= interval.Right : x < interval.Right);
                case SpecialSet special when over is SpecialSet range
                    && RankInTheNumberChain(special) is { } target && RankInTheNumberChain(range) is { } from:
                    if (from <= target)
                        return Entity.Boolean.True;
                    // ZZ+ inside ZZ* or ZZ is x >= 1; ZZ* inside ZZ is x >= 0. Below the integers
                    // there is no bound that separates a set from the one above it.
                    return (special, from) switch
                    {
                        (SpecialSet.PositiveIntegers, <= 2) => x >= Integer.One,
                        (SpecialSet.NonNegativeIntegers, 2) => x >= Integer.Zero,
                        _ => null,
                    };
                default:
                    return null;
            }
        }
    }
}
