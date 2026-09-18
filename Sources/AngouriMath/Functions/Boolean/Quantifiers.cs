//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Collections.Generic;
using System.Linq;
using AngouriMath.Core;
using AngouriMath.Core.Exceptions;
using PeterO.Numbers;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Functions.Boolean
{
    /// <summary>
    /// Decides <c>forall x in S : P</c>, <c>exists x in S : P</c> and <c>exists! x in S : P</c>
    /// where a decision is available, and says nothing where it is not.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Three routes, in the order the reference teaches them. Over a <b>finite set</b> the body
    /// is evaluated at every member, which is the definition; a member the body cannot decide
    /// leaves the statement as written unless the members it can decide settle it, as one
    /// counterexample settles <c>forall</c>. Over an infinite set the first thing tried is a
    /// <b>named witness</b>: a handful of members of the set, chosen for being where a claim
    /// usually fails — 0, ±1, ±2, a half, an end of an interval, <c>i</c> in the complex numbers
    /// — and a member where the body is false disproves <c>forall</c> exactly as one where it
    /// is true proves <c>exists</c>. What a witness cannot do is prove <c>forall</c> or refute
    /// <c>exists</c>, and for that the <b>solver</b> is asked: <c>exists x in S : P</c> holds
    /// where the solution set of <c>P</c> meets <c>S</c>, and <c>forall x in S : P</c> where the
    /// solution set of <c>not P</c> does not.
    /// </para>
    /// <para>
    /// The solver route is taken only where what it answers is what was asked. Its answer to an
    /// order comparison is a set of reals, so it is asked one only over a set of reals; its
    /// default for a statement it has no arm for is the empty set, which as an answer to
    /// "where does this hold" is a claim, so it is asked only about the shapes it has arms for
    /// — comparisons of two expressions that mention the variable, joined by the connectives.
    /// Whether the solution set meets <c>S</c> is read off the two sets by
    /// <see cref="Meets"/>, and where that cannot be read — a set-builder the solver fell back to,
    /// a member whose membership is undecided — the answer is that there is none.
    /// </para>
    /// <para>
    /// Uniqueness is existence with a count: over a finite set of numbers, whose members are
    /// distinct exactly when unequal; over anything else, from the solver's solution set when it
    /// is a finite set of numbers. A set with a symbol in it is not counted, since two of its
    /// members may be one.
    /// </para>
    /// https://github.com/asc-community/AngouriMath/issues/1409
    /// </remarks>
    internal static class Quantifiers
    {
        internal enum Kind { All, Some, Unique }

        /// <summary>
        /// The truth value of the quantified statement, or <see langword="null"/> where it is
        /// not settled. The set and the body arrive already simplified.
        /// </summary>
        internal static Entity? Decide(Kind kind, Entity var, Entity over, Entity body, bool isExact)
        {
            if (var is not Variable x || over is not Set set)
                return null;
            // A statement about the members of { y in S : Q } is a statement about the members of
            // S that satisfy Q, and that shape is what the routes below read.
            if (set is ConditionalSet builder)
            {
                if (builder.DeclaredMembership is not (var declared, var rest) || builder.Var is not Variable y)
                    return null;
                var condition = rest.Substitute(y, x);
                return Decide(kind, x, declared.InnerSimplified(isExact), kind switch
                {
                    Kind.All => condition.Implies(body),
                    _ => condition & body,
                }, isExact);
            }
            if (set is SpecialSet.Booleans)
                set = new FiniteSet(Entity.Boolean.True, Entity.Boolean.False);
            if (!body.ContainsNode(x))
                return Closed(kind, set, body);
            // An equation whose two sides differ by a polynomial that expands to nothing holds
            // at every member, whatever the set: (x + 1)^2 = x^2 + 2 x + 1 is True of each.
            if (body is Equalsf(var left, var right) && IsIdentity(left, right))
                return Closed(kind, set, Entity.Boolean.True);
            if (set is FiniteSet finite)
                return OverFinite(kind, x, finite, body, isExact);
            if (set is SpecialSet integers && IsIntegerSet(integers) && kind != Kind.Unique)
            {
                // A statement about divisibility, or a congruence, in a polynomial of x with
                // whole coefficients repeats with the modulus, so the residues decide it:
                // forall n in ZZ : 6 divides n^3 + 5 n is six cases. The reference's "case
                // analysis over the classes" (Ex 6.5.14).
                if (Period(body, x) is { } period && period.CompareTo(EInteger.FromInt32(LargestPeriod)) <= 0)
                    return OverResidues(kind, x, integers, body, period, isExact);
                // A polynomial equation with no solution modulo some m has none in the whole
                // numbers: 3 x^2 - 5 y^2 = 1 is impossible modulo 5 (Ex 6.5.27). Only a refusal
                // is read off the residues; a solution modulo every m tried proves nothing.
                if (ImpossibleByResidues(kind, x, integers, body) is { } verdict)
                    return verdict;
            }
            if (Witness(kind, x, set, body, isExact) is { } byWitness)
                return byWitness;
            return BySolving(kind, x, set, body);
        }

        private static bool IsIntegerSet(SpecialSet set)
            => set.ToDomain() is Domain.Integer or Domain.NonNegativeInteger or Domain.PositiveInteger;

        /// <summary>How many residues a periodic statement is checked over before it is left as written.</summary>
        private const int LargestPeriod = 4096;

        /// <summary>
        /// The period of the body in <paramref name="x"/> over the whole numbers, where it has
        /// one: a divisibility by a whole number, or a congruence modulo one, of polynomials
        /// with whole coefficients, joined by the connectives; the least common multiple of the
        /// parts' periods.
        /// </summary>
        private static EInteger? Period(Entity body, Variable x)
        {
            switch (body)
            {
                case Entity.Boolean:
                    return EInteger.One;
                case Dividesf(Integer divisor, var dividend):
                    return divisor.EInteger.IsZero || !IsWholePolynomial(dividend) ? null : divisor.EInteger.Abs();
                case Congruentf(var left, var right, Integer modulus):
                    return modulus.EInteger.IsZero || !IsWholePolynomial(left) || !IsWholePolynomial(right) ? null : modulus.EInteger.Abs();
                case Notf(var operand):
                    return Period(operand, x);
                // A quantifier over whole numbers inside: its body is periodic in x with the
                // same period whatever the inner name takes, so the residues of x decide the
                // outer statement and the inner one is decided at each of them.
                case Quantifier { Over: SpecialSet inner } quantifier when IsIntegerSet(inner):
                    return Period(quantifier.Body, x);
                case Andf or Orf or Impliesf or Xorf:
                    if (body is not IBinaryNode { NodeFirstChild: var first, NodeSecondChild: var second })
                        return null;
                    if (Period(first, x) is not { } one || Period(second, x) is not { } another)
                        return null;
                    return one * another / one.Gcd(another);
                default:
                    return body.ContainsNode(x) ? null : EInteger.One;
            }
        }

        /// <summary>
        /// A polynomial with whole coefficients in whatever names it mentions -- which is
        /// periodic in each of them modulo anything, as long as the others stand for whole
        /// numbers.
        /// </summary>
        private static bool IsWholePolynomial(Entity expression)
        {
            if (!expression.Vars.Any())
                return expression.Evaled is Integer;
            var variables = expression.Vars.OrderBy(v => v.Name, System.StringComparer.Ordinal).ToArray();
            if (variables.Length > MultivariatePolynomial.MaxVariables)
                return false;
            var indices = new Dictionary<Variable, int>();
            for (var i = 0; i < variables.Length; i++)
                indices[variables[i]] = i;
            return MultivariatePolynomial.TryParse(expression, indices) is { HasIntegerCoefficients: true };
        }

        /// <summary>The statement at each residue, which is at every member of the set.</summary>
        private static Entity? OverResidues(Kind kind, Variable x, SpecialSet set, Entity body, EInteger period, bool isExact)
        {
            int holds = 0, fails = 0, undecided = 0;
            // From 1 for the positive whole numbers, since 0 is not one; either way every class
            // is represented once.
            var first = set.ToDomain() == Domain.PositiveInteger ? EInteger.One : EInteger.Zero;
            for (var residue = first; residue.CompareTo(first + period) < 0; residue += 1)
                switch (body.Substitute(x, Integer.Create(residue)).InnerSimplified(isExact))
                {
                    case Entity.Boolean(true): holds++; break;
                    case Entity.Boolean(false): fails++; break;
                    default: undecided++; break;
                }
            return Tally(kind, holds, fails, undecided, distinct: true);
        }

        [ConstantField]
        private static readonly int[] ModuliTried = { 2, 3, 4, 5, 7, 8, 9, 11, 13, 16 };

        /// <summary>
        /// <c>exists x, y, ... in ZZ : P = Q</c> is false where the equation has no solution
        /// modulo some small <c>m</c>, and <c>forall ... : not (P = Q)</c> true. The variables
        /// are the whole chain of quantifiers over whole numbers the body opens with.
        /// </summary>
        private static Entity? ImpossibleByResidues(Kind kind, Variable x, SpecialSet set, Entity body)
        {
            var variables = new List<Variable> { x };
            var inner = body;
            while (true)
            {
                Variable? next = null;
                Entity? deeper = null;
                if (kind == Kind.Some && inner is Existsf(Variable some, SpecialSet someOver, var someBody) && IsIntegerSet(someOver))
                    (next, deeper) = (some, someBody);
                else if (kind == Kind.All && inner is Forallf(Variable every, SpecialSet everyOver, var everyBody) && IsIntegerSet(everyOver))
                    (next, deeper) = (every, everyBody);
                if (next is null || deeper is null)
                    break;
                variables.Add(next);
                inner = deeper;
            }
            var equation = kind == Kind.Some ? inner : inner is Notf(var negated) ? negated : null;
            if (equation is not Equalsf(var left, var right))
                return null;
            var difference = (left - right).InnerSimplified;
            if (difference.Vars.Any(v => !variables.Contains(v)) || variables.Count > 3)
                return null;
            var indices = new Dictionary<Variable, int>();
            for (var i = 0; i < variables.Count; i++)
                indices[variables[i]] = i;
            if (MultivariatePolynomial.TryParse(difference, indices) is not { HasIntegerCoefficients: true } polynomial)
                return null;
            foreach (var m in ModuliTried)
            {
                var modulus = EInteger.FromInt32(m);
                if (!HasRootModulo(polynomial, variables.Count, modulus))
                    return kind == Kind.Some ? Entity.Boolean.False : Entity.Boolean.True;
            }
            return null;
        }

        private static bool HasRootModulo(MultivariatePolynomial polynomial, int variables, EInteger modulus)
        {
            var values = new EInteger[variables];
            var m = modulus.ToInt32Checked();
            var total = 1;
            for (var i = 0; i < variables; i++)
                total *= m;
            for (var index = 0; index < total; index++)
            {
                var rest = index;
                for (var i = 0; i < variables; i++)
                {
                    values[i] = EInteger.FromInt32(rest % m);
                    rest /= m;
                }
                if (polynomial.ValueModulo(values, modulus) is { IsZero: true })
                    return true;
            }
            return false;
        }

        /// <summary>Whether two sides are the same polynomial, read by expansion.</summary>
        private static bool IsIdentity(Entity left, Entity right)
        {
            var difference = (left - right).InnerSimplified;
            if (difference is Integer { IsZero: true })
                return true;
            if (difference.Complexity > LargestDifferenceRead)
                return false;
            var variables = difference.Vars.OrderBy(v => v.Name, System.StringComparer.Ordinal).ToArray();
            if (variables.Length == 0 || variables.Length > MultivariatePolynomial.MaxVariables)
                return false;
            var indices = new Dictionary<Variable, int>();
            for (var i = 0; i < variables.Length; i++)
                indices[variables[i]] = i;
            return MultivariatePolynomial.TryParse(difference, indices) is { IsZero: true };
        }

        private const int LargestDifferenceRead = 2048;

        /// <summary>
        /// A body that does not mention the variable says the same of every member, so the
        /// statement is the body wherever the set has a member -- and over the empty set every
        /// member satisfies anything and none satisfies something.
        /// </summary>
        private static Entity? Closed(Kind kind, Set set, Entity body)
        {
            if (body is not Entity.Boolean truth)
                return null;
            var empty = set switch
            {
                FiniteSet finite => finite.Count == 0,
                Interval interval => interval.IsNumeric && interval.Left.Evaled is Real from && interval.Right.Evaled is Real to
                    && (Compare(from, to) > 0 || Compare(from, to) == 0 && !(interval.LeftClosed && interval.RightClosed)),
                SpecialSet => false,
                _ => (bool?)null,
            };
            return (kind, empty) switch
            {
                (Kind.All, true) => Entity.Boolean.True,
                (Kind.Some or Kind.Unique, true) => Entity.Boolean.False,
                (Kind.All or Kind.Some, false) => truth,
                (Kind.Unique, false) => truth == Entity.Boolean.False ? Entity.Boolean.False
                    : set is FiniteSet { Count: 1 } ? Entity.Boolean.True
                    : set is FiniteSet counted && counted.All(static e => e is Number) ? Entity.Boolean.False
                    : set is Interval interval ? (Compare(interval.Left.Evaled is Real a ? a : Real.NaN, interval.Right.Evaled is Real b ? b : Real.NaN) == 0 ? Entity.Boolean.True : Entity.Boolean.False)
                    : set is SpecialSet ? Entity.Boolean.False
                    : null,
                _ => null,
            };
        }

        private static Entity? OverFinite(Kind kind, Variable x, FiniteSet set, Entity body, bool isExact)
        {
            int holds = 0, fails = 0, undecided = 0;
            foreach (var member in set.Elements)
                switch (body.Substitute(x, member).InnerSimplified(isExact))
                {
                    case Entity.Boolean(true): holds++; break;
                    case Entity.Boolean(false): fails++; break;
                    default: undecided++; break;
                }
            return Tally(kind, holds, fails, undecided, set.All(static member => member is Number));
        }

        /// <summary>What the counts say, where they say anything.</summary>
        private static Entity? Tally(Kind kind, int holds, int fails, int undecided, bool distinct)
            => kind switch
            {
                Kind.All => fails > 0 ? Entity.Boolean.False : undecided == 0 ? Entity.Boolean.True : null,
                Kind.Some => holds > 0 ? Entity.Boolean.True : undecided == 0 ? Entity.Boolean.False : null,
                // Two members where it holds refute uniqueness only if they are two members.
                Kind.Unique => holds > 1 && distinct ? Entity.Boolean.False
                    : undecided > 0 ? null
                    : holds == 0 ? Entity.Boolean.False
                    : holds == 1 ? Entity.Boolean.True
                    : null,
                _ => null,
            };

        /// <summary>
        /// A member of the set where the body decides the statement: false for
        /// <c>forall</c>, true for <c>exists</c>, or two where it is true for <c>exists!</c>.
        /// </summary>
        private static Entity? Witness(Kind kind, Variable x, Set set, Entity body, bool isExact)
        {
            var found = new List<Entity>();
            foreach (var candidate in Candidates(set))
            {
                if (!set.TryContains(candidate, out var inside) || !inside)
                    continue;
                var value = body.Substitute(x, candidate).InnerSimplified(isExact);
                switch (kind, value)
                {
                    case (Kind.All, Entity.Boolean(false)):
                        return Entity.Boolean.False;
                    case (Kind.Some, Entity.Boolean(true)):
                        return Entity.Boolean.True;
                    case (Kind.Unique, Entity.Boolean(true)):
                        if (found.Any(other => other.Evaled != candidate.Evaled))
                            return Entity.Boolean.False;
                        found.Add(candidate);
                        break;
                }
            }
            return null;
        }

        /// <summary>
        /// Members a claim over the set tends to fail at. Membership is checked by the caller,
        /// so this may offer anything.
        /// </summary>
        private static IEnumerable<Entity> Candidates(Set set)
        {
            switch (set)
            {
                case SpecialSet special:
                    foreach (var candidate in Rationals)
                        yield return candidate;
                    if (special.ToDomain() >= Domain.Real)
                    {
                        yield return MathS.Sqrt(2);
                        yield return -MathS.Sqrt(2);
                        yield return MathS.pi;
                    }
                    if (special.ToDomain() >= Domain.Complex)
                    {
                        yield return MathS.i;
                        yield return -MathS.i;
                        yield return 1 + MathS.i;
                        yield return 2 * MathS.i;
                    }
                    break;
                case Interval interval:
                    foreach (var candidate in Rationals)
                        yield return candidate;
                    foreach (var end in new[] { interval.Left, interval.Right })
                        if (end.Evaled is Real { IsFinite: true } endpoint)
                            foreach (var offset in Offsets)
                                yield return (endpoint + offset).Evaled;
                    break;
                case Unionf(Set left, Set right):
                    foreach (var candidate in Candidates(left))
                        yield return candidate;
                    foreach (var candidate in Candidates(right))
                        yield return candidate;
                    break;
                // A member of an intersection or a difference is a member of the first set.
                case Intersectionf(Set first, _):
                    foreach (var candidate in Candidates(first))
                        yield return candidate;
                    break;
                case SetMinusf(Set minuend, _):
                    foreach (var candidate in Candidates(minuend))
                        yield return candidate;
                    break;
            }
        }

        [ConstantField]
        private static readonly Entity[] Rationals =
        {
            0, 1, -1, 2, -2, 3, -3, 4, 5, 6, 7, 8, 9, 10, -10, 12, 100,
            Rational.Create(1, 2), Rational.Create(-1, 2), Rational.Create(3, 2), Rational.Create(-3, 2),
            Rational.Create(2, 3), Rational.Create(1, 10),
        };

        [ConstantField]
        private static readonly Entity[] Offsets =
        {
            0, 1, -1, 2, -2, 10, -10, Rational.Create(1, 2), Rational.Create(-1, 2),
        };

        private static Entity? BySolving(Kind kind, Variable x, Set set, Entity body)
        {
            // The inequality solver answers over the reals, so an order comparison is asked
            // only over a set of reals; an equation is answered over the complex numbers and
            // may be asked over any set.
            if (!SolverReads(body, x) || !(WithinReals(set) || Equational(body)))
                return null;
            try
            {
                switch (kind)
                {
                    case Kind.Some:
                        return Meets(body.Solve(x), set) switch
                        {
                            true => Entity.Boolean.True,
                            false => Entity.Boolean.False,
                            null => null,
                        };
                    case Kind.All:
                        return Meets(Negated(body).Solve(x), set) switch
                        {
                            true => Entity.Boolean.False,
                            false => Entity.Boolean.True,
                            null => null,
                        };
                    case Kind.Unique:
                        var solutions = body.Solve(x);
                        // Counted only where the solutions are numbers as written: a root the
                        // solver writes in radicals may be a whole number it cannot see, and a
                        // count that misses it is a wrong answer rather than none.
                        if (solutions is FiniteSet finite && finite.All(static s => s.InnerSimplified is Number))
                        {
                            int inside = 0;
                            foreach (var solution in finite.Elements)
                            {
                                if (!set.TryContains(solution, out var contains))
                                    return null;
                                if (contains)
                                    inside++;
                            }
                            return inside == 1 ? Entity.Boolean.True : Entity.Boolean.False;
                        }
                        return Meets(solutions, set) == false ? Entity.Boolean.False : null;
                }
            }
            catch (AngouriBugException) { throw; }
            catch (AngouriMathBaseException) { }
            return null;
        }

        /// <summary>
        /// The negation of the body with <c>not</c> pushed down to the comparisons, which is what
        /// the statement solver has arms for: <c>not (a implies b)</c> is <c>a and not b</c>, and
        /// a negated order comparison is the opposite one. The solver does the same for the
        /// connectives it reads and not for an implication, so it is done here first.
        /// </summary>
        private static Entity Negated(Entity body)
            => body switch
            {
                Notf(var operand) => operand,
                Andf(var left, var right) => Negated(left) | Negated(right),
                Orf(var left, var right) => Negated(left) & Negated(right),
                Impliesf(var assumption, var conclusion) => assumption & Negated(conclusion),
                Greaterf(var left, var right) => left <= right,
                GreaterOrEqualf(var left, var right) => left < right,
                Lessf(var left, var right) => left >= right,
                LessOrEqualf(var left, var right) => left > right,
                _ => !body,
            };

        /// <summary>
        /// Whether the statement solver has an arm for every part of the body, so that its answer
        /// is a solution set rather than its default.
        /// </summary>
        private static bool SolverReads(Entity body, Variable x)
            => body switch
            {
                ComparisonSign and IBinaryNode { NodeFirstChild: var left, NodeSecondChild: var right }
                    => (left.ContainsNode(x) || right.ContainsNode(x)) && Plain(left) && Plain(right),
                Andf or Orf or Impliesf => body is IBinaryNode { NodeFirstChild: var left, NodeSecondChild: var right } && SolverReads(left, x) && SolverReads(right, x),
                Notf(var operand) => SolverReads(operand, x),
                _ => false,
            };

        /// <summary>An expression the solver reads as a number: no sets, statements or binders inside.</summary>
        private static bool Plain(Entity expression)
            => expression.Nodes.All(static node => node is not (Set or Statement or Quantifier or Lambda or Providedf or Piecewise));

        /// <summary>Only equations and their connectives, which the solver answers over the complex numbers.</summary>
        private static bool Equational(Entity body)
            => body switch
            {
                Equalsf => true,
                Andf or Orf or Impliesf => body is IBinaryNode { NodeFirstChild: var left, NodeSecondChild: var right } && Equational(left) && Equational(right),
                Notf(var operand) => Equational(operand),
                _ => false,
            };

        private static bool WithinReals(Set set)
            => set switch
            {
                SpecialSet special => special.ToDomain() <= Domain.Real && special.ToDomain() != Domain.Boolean,
                Interval => true,
                FiniteSet finite => finite.All(static member => member is Real),
                Unionf(Set left, Set right) => WithinReals(left) && WithinReals(right),
                Intersectionf(Set left, Set right) => WithinReals(left) || WithinReals(right),
                SetMinusf(Set left, _) => WithinReals(left),
                _ => false,
            };

        /// <summary>
        /// Whether two sets share a member, read off their shapes; <see langword="null"/> where
        /// that cannot be read.
        /// </summary>
        private static bool? Meets(Set solutions, Set set)
        {
            switch (solutions)
            {
                case FiniteSet finite:
                    return AnyMember(finite, set);
                case Unionf(Set left, Set right):
                    return (Meets(left, set), Meets(right, set)) switch
                    {
                        (true, _) or (_, true) => true,
                        (false, false) => false,
                        _ => null,
                    };
                case Interval interval when interval.IsNumeric:
                    return MeetsInterval(interval, set);
                case SpecialSet special:
                    return set switch
                    {
                        FiniteSet finite => AnyMember(finite, special),
                        Interval interval when interval.IsNumeric => MeetsInterval(interval, special),
                        SpecialSet other => Domain.Boolean is var boolean
                            && (special.ToDomain() == boolean) == (other.ToDomain() == boolean),
                        _ => null,
                    };
                default:
                    return null;
            }
        }

        private static int Compare(Real left, Real right) => left.EDecimal.CompareTo(right.EDecimal);

        private static bool? AnyMember(FiniteSet members, Set set)
        {
            var undecided = false;
            foreach (var member in members.Elements)
                if (!set.TryContains(member, out var contains))
                    undecided = true;
                else if (contains)
                    return true;
            return undecided ? null : false;
        }

        /// <summary>Whether a numeric interval has a member in the set.</summary>
        private static bool? MeetsInterval(Interval interval, Set set)
        {
            if (interval.Left.Evaled is not Real from || interval.Right.Evaled is not Real to)
                return null;
            if (Compare(from, to) > 0 || Compare(from, to) == 0 && !(interval.LeftClosed && interval.RightClosed))
                return false;
            switch (set)
            {
                case SpecialSet special:
                    var domain = special.ToDomain();
                    if (domain == Domain.Boolean)
                        return false;
                    if (domain >= Domain.Rational)
                        return true;
                    // The integers, or those from 0 or from 1 on: the interval has one exactly when
                    // the first whole number at or after its start is at or before its end.
                    var lower = from;
                    var lowerClosed = interval.LeftClosed;
                    var least = domain == Domain.PositiveInteger ? 1 : domain == Domain.NonNegativeInteger ? 0 : (int?)null;
                    if (least is { } floor && Compare(lower, floor) < 0)
                        (lower, lowerClosed) = (Integer.Create(floor), true);
                    if (!to.IsFinite)
                        return Compare(to, 0) > 0;
                    if (!lower.IsFinite)
                        return true;
                    Real first = Integer.Create(lower.EDecimal.ToEInteger());
                    if (Compare(first, lower) < 0)
                        first = Integer.Create(((Integer)first).EInteger + 1);
                    if (Compare(first, lower) == 0 && !lowerClosed)
                        first = Integer.Create(((Integer)first).EInteger + 1);
                    return Compare(first, to) < 0 || Compare(first, to) == 0 && interval.RightClosed;
                case Interval other:
                    return MathS.Intersection(interval, other).InnerSimplified switch
                    {
                        FiniteSet finite => finite.Count > 0,
                        Interval overlap => overlap.Left.Evaled is Real a && overlap.Right.Evaled is Real b
                            && (Compare(a, b) < 0 || Compare(a, b) == 0 && overlap.LeftClosed && overlap.RightClosed),
                        _ => null,
                    };
                case FiniteSet finite:
                    return AnyMember(finite, interval);
                case Unionf(Set left, Set right):
                    return (MeetsInterval(interval, left), MeetsInterval(interval, right)) switch
                    {
                        (true, _) or (_, true) => true,
                        (false, false) => false,
                        _ => null,
                    };
                default:
                    return null;
            }
        }
    }
}
