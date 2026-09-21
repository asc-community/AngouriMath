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
    /// Three routes, in the order Sullivan and Mackey's proofs book (#1409) teaches them. Over a <b>finite set</b> the body
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
            // One read per decision, and nothing recorded or allocated unless a scope is open.
            if (!ProofRecording.Recording)
                return DecideBy(kind, var, over, body, isExact, out _);
            ProofRecording.Enter();
            var mark = ProofRecording.Mark();
            try
            {
                var verdict = DecideBy(kind, var, over, body, isExact, out var by);
                // A decision that came to nothing leaves nothing behind: what was tried on
                // the way is not part of any proof.
                if (verdict is null)
                    ProofRecording.Rollback(mark);
                else if (by is var (rule, lemma))
                    ProofRecording.Add(Quantified(kind, var, over, body), rule, lemma, verdict);
                return verdict;
            }
            finally
            {
                ProofRecording.Leave();
            }
        }

        private static Entity Quantified(Kind kind, Entity var, Entity over, Entity body)
            => kind switch
            {
                Kind.All => new Forallf(var, over, body),
                Kind.Some => new Existsf(var, over, body),
                _ => new ExistsUniquef(var, over, body),
            };

        /// <summary>
        /// <see cref="Decide"/> proper, naming the rule that decided in <paramref name="by"/> --
        /// as the reference names it, and as the Lean 4 tactic or lemma a checker would use --
        /// and <see langword="null"/> for a route that only asked another statement.
        /// </summary>
        private static Entity? DecideBy(Kind kind, Entity var, Entity over, Entity body, bool isExact, out (string Rule, string Lemma)? by)
        {
            by = null;
            if (var is not Variable x || over is not Set set)
                return null;
            // A statement about the members of { y in S : Q } is a statement about the members of
            // S that satisfy Q, and that shape is what the routes below read.
            if (set is ConditionalSet builder)
            {
                if (builder.DeclaredMembership is not (var declared, var rest) || builder.Var is not Variable y)
                    return null;
                var condition = rest.Substitute(y, x);
                by = ("the members of { y in S : Q } are the members of S with Q", "Set.mem_setOf_eq");
                return Decide(kind, x, declared.InnerSimplified(isExact), kind switch
                {
                    Kind.All => condition.Implies(body),
                    _ => condition & body,
                }, isExact);
            }
            if (set is SpecialSet.Booleans)
                set = new FiniteSet(Entity.Boolean.True, Entity.Boolean.False);
            if (!body.ContainsNode(x))
            {
                by = ("the body does not mention the name, so it says the same of every member", "forall_const");
                return Closed(kind, set, body);
            }
            // The whole numbers from m are ZZ* shifted by m, so a statement over ZZ+ /\ [4; +oo)
            // is the statement about 4 + t over ZZ*, where every route below reads the set.
            if (set is Intersectionf cut && LeastMember(cut) is { } start && IsUnboundedAbove(cut))
            {
                var t = Variable.CreateUnique(body + start, "t");
                by = ($"the whole numbers from {start} are ZZ* shifted by {start}", "Nat.le_induction");
                return Decide(kind, t, MathS.Sets.NonNegativeIntegers, body.Substitute(x, start + t).InnerSimplified(isExact), isExact);
            }
            // A piecewise body -- what a closed form comes as, sum(k, k, 1, n) being
            // (n + n^2)/2 provided n >= 0 -- is the case whose condition holds at every member of
            // the set, or the next case where a condition holds at none: over ZZ+ the condition
            // n >= 0 is the membership itself, and the statement is about the closed form.
            // https://github.com/asc-community/AngouriMath/issues/1409
            if (body is Piecewise piecewise)
            {
                foreach (var @case in piecewise.Cases)
                {
                    var holds = @case.Predicate == Entity.Boolean.True ? Entity.Boolean.True : Decide(Kind.All, x, set, @case.Predicate.InnerSimplified(isExact), isExact);
                    if (holds == Entity.Boolean.True)
                    {
                        by = ($"the case {@case.Predicate} of the piecewise body holds at every member, so the body is that case", "if_pos");
                        return Decide(kind, x, set, @case.Expression.InnerSimplified(isExact), isExact);
                    }
                    var probed = ProofRecording.Mark();
                    var somewhere = Decide(Kind.Some, x, set, @case.Predicate.InnerSimplified(isExact), isExact);
                    ProofRecording.Rollback(probed);
                    if (somewhere != Entity.Boolean.False)
                        break;
                }
            }
            // forall b in B : exists a in A : f(a) = b is the statement that B lies in the image
            // of A under f -- surjectivity onto B (Sullivan and Mackey's Def 7.4.1) -- and the image is
            // a set the library computes: listed over a listed A, an interval by interval
            // arithmetic, so the subset decision settles it where the image evaluates.
            // https://github.com/asc-community/AngouriMath/issues/1409
            if (kind == Kind.All && body is Existsf(Variable a, Set domain, Equalsf(var lhs, var rhs))
                && (lhs == x && !rhs.ContainsNode(x) && rhs.ContainsNode(a) ? rhs : rhs == x && !lhs.ContainsNode(x) && lhs.ContainsNode(a) ? lhs : null) is { } map)
            {
                var image = new IndexedUnionf(a, domain, new FiniteSet(map)).InnerSimplified(isExact);
                if (image is not IndexedUnionf && image is Set imageSet && Core.Sets.SetOperators.Subset(set, imageSet, isExact) is { } covered)
                {
                    by = ($"every b has an a with f(a) = b exactly when the set lies in the image of f, which is {image}", "Set.range_subset_iff");
                    return covered;
                }
            }
            // An equation whose two sides differ by a polynomial that expands to nothing holds
            // at every member, whatever the set: (x + 1)^2 = x^2 + 2 x + 1 is True of each.
            if (body is Equalsf(var left, var right))
            {
                var tried = ProofRecording.Mark();
                if (IsIdentity(left, right, x, set, isExact) is { } identity)
                {
                    by = ("the two sides differ by a polynomial that expands to nothing", "ring");
                    return Closed(kind, set, Entity.Boolean.True) is Entity.Boolean(true) ? identity : Closed(kind, set, Entity.Boolean.True);
                }
                ProofRecording.Rollback(tried);
            }
            if (set is FiniteSet finite)
            {
                by = ($"evaluated at each of the {finite.Count} members", "decide");
                return OverFinite(kind, x, finite, body, isExact);
            }
            if (set is SpecialSet integers && IsIntegerSet(integers) && kind != Kind.Unique)
            {
                // A statement about divisibility, or a congruence, in a polynomial of x with
                // whole coefficients repeats with the modulus, so the residues decide it:
                // forall n in ZZ : 6 divides n^3 + 5 n is six cases. Sullivan and Mackey's "case
                // analysis over the classes" (Ex 6.5.14).
                if (Period(body, x, integers) is { } period && period.CompareTo(EInteger.FromInt32(LargestPeriod)) <= 0)
                {
                    by = ($"the statement repeats modulo {period}, so its {period} residues decide it", "decide");
                    return OverResidues(kind, x, integers, body, period, isExact);
                }
                // A polynomial equation with no solution modulo some m has none in the whole
                // numbers: 3 x^2 - 5 y^2 = 1 is impossible modulo 5 (Ex 6.5.27). Only a refusal
                // is read off the residues; a solution modulo every m tried proves nothing.
                if (ImpossibleByResidues(kind, x, integers, body) is { } verdict)
                {
                    by = ("the equation has no solution modulo a small m, so none in the whole numbers", "decide");
                    return verdict;
                }
            }
            // A statement about a sum or a product up to x, over the whole numbers from some
            // least one, is proved by induction: it holds at the least member, and holding at
            // x it holds at x + 1, where the sum to x + 1 is the sum to x and one more term.
            // Sullivan and Mackey's chapter 5. https://github.com/asc-community/AngouriMath/issues/1409
            if (kind == Kind.All && LeastMember(set) is { } least && ByInduction(x, set, least, body, isExact) is { } byInduction)
            {
                by = byInduction == Entity.Boolean.False
                    ? ($"false at the least member, {least}", "decide")
                    : ($"by induction from {least}: the base case, and the sum to {x} + 1 unfolded by one term with the hypothesis for the sum to {x}", "Nat.le_induction");
                return byInduction;
            }
            // And an inequality with an exponential or a factorial in x, by induction with the
            // step read off a multiple of the hypothesis: P(x + 1) = c P(x) + D with c >= 0 and
            // D >= 0 keeps P >= 0 (Sullivan and Mackey's Ex 5.3.2, Prob 5.7.2, 5.7.8).
            if (kind == Kind.All && LeastMember(set) is { } first && ByInductionOnAnInequality(x, set, first, body, isExact) is { } byGrowth)
            {
                by = byGrowth == Entity.Boolean.False
                    ? ($"false at the least member, {first}", "decide")
                    : ($"by induction from {first}: the base case, and P({x} + 1) = c P({x}) + D with c >= 0 and D >= 0", "Nat.le_induction");
                return byGrowth;
            }
            if (Witness(kind, x, set, body, isExact) is { } byWitness)
            {
                by = (byWitness == Entity.Boolean.False && kind == Kind.All ? "a member at which the body is false"
                    : byWitness == Entity.Boolean.True && kind == Kind.Some ? "a member at which the body is true"
                    : "two members at which the body is true", "exact ⟨_, by decide⟩");
                return byWitness;
            }
            if (BySolving(kind, x, set, body) is { } bySolving)
            {
                by = ((kind, bySolving == Entity.Boolean.True) switch
                {
                    (Kind.All, true) => "the negation of the body has no solution in the set",
                    (Kind.All, false) => "the negation of the body has a solution in the set",
                    (Kind.Some, true) => "the body's solutions meet the set",
                    (Kind.Some, false) => "the body has no solution in the set",
                    (_, true) => "the body has exactly one solution in the set",
                    _ => "the body has no solution, or more than one, in the set",
                }, "nlinarith");
                return bySolving;
            }
            return null;
        }

        /// <summary>
        /// The least member of a set of whole numbers bounded below -- <c>ZZ+</c>, <c>ZZ*</c>,
        /// either cut by an interval -- or <see langword="null"/> where the set is something else.
        /// </summary>
        internal static Integer? LeastMember(Set set)
        {
            switch (set)
            {
                case SpecialSet special:
                    return special.ToDomain() switch
                    {
                        Domain.PositiveInteger => Integer.One,
                        Domain.NonNegativeInteger => Integer.Zero,
                        _ => null,
                    };
                case Intersectionf(Set left, Set right):
                    {
                        var (integers, cut) = (left, right) switch
                        {
                            (SpecialSet whole, Interval interval) when IsIntegerSet(whole) => (whole, interval),
                            (Interval interval, SpecialSet whole) when IsIntegerSet(whole) => (whole, interval),
                            _ => (null, null),
                        };
                        if (integers is null || cut is null || cut.Left.Evaled is not Real { IsFinite: true } low)
                            return null;
                        var floor = low.EDecimal.RoundToExponent(0, ERounding.Floor).ToEInteger();
                        // The first whole number at or after the end: the end itself when it
                        // is whole and included, the next one otherwise.
                        var first = Integer.Create(low.EDecimal.IsInteger() && cut.LeftClosed ? floor : floor + 1);
                        var own = LeastMember(integers);
                        return own is not null && own > first ? own : first;
                    }
                default:
                    return null;
            }
        }

        /// <summary>
        /// <c>forall x in S : sum(f, k, a, x) = g</c> by induction from the least member of
        /// <c>S</c>: the statement at that member evaluates to <see langword="true"/>, and the
        /// statement at <c>x + 1</c>, with the sum to <c>x + 1</c> unfolded to the sum to <c>x</c>
        /// plus <c>f(x + 1)</c> and that sum then replaced by <c>g</c> -- the hypothesis -- holds
        /// at every member. A product unfolds with a factor. Only the shape with one sum or
        /// product, standing alone on a side of the equation, is read; a statement that fails at
        /// the least member is <see langword="false"/>.
        /// </summary>
        private static Entity? ByInduction(Variable x, Set set, Integer least, Entity body, bool isExact)
        {
            if (body is not Equalsf(var left, var right))
                return null;
            var (accumulated, closed) = (left, right) switch
            {
                (Summationf or Productf, _) when !right.ContainsNode(left) => (left, right),
                (_, Summationf or Productf) when !left.ContainsNode(right) => (right, left),
                _ => (null, null),
            };
            if (accumulated is null || closed is null)
                return null;
            var (term, index, from, to) = accumulated switch
            {
                Summationf sum => (sum.Expression, sum.Var, sum.From, sum.To),
                Productf product => (product.Expression, product.Var, product.From, product.To),
                _ => throw new AngouriBugException("A sum or a product was matched above"),
            };
            // The upper bound is x, or x plus a whole number after a shift of the set.
            if (WholeGap(to, x) is null || from.ContainsNode(x) || term.ContainsNode(x) || index is not Variable k)
                return null;
            var atLeast = body.Substitute(x, least).InnerSimplified(isExact);
            var tried = ProofRecording.Mark();
            if (atLeast is Equalsf(var l, var r) && IsIdentity(l, r, x, set, isExact) is { } identity)
                atLeast = identity;
            else
                ProofRecording.Rollback(tried);
            if (atLeast is Entity.Boolean or Providedf)
                ProofRecording.AddBelow(body.Substitute(x, least), $"the base case, at {least}, by evaluation", "decide", atLeast);
            if (atLeast == Entity.Boolean.False)
                return Entity.Boolean.False;
            if (Truth(atLeast) is not { } baseCondition)
                return null;
            var next = x + Integer.One;
            var oneMore = term.Substitute(k, to.Substitute(x, next));
            var unfolded = accumulated is Summationf ? closed + oneMore : closed * oneMore;
            var step = new Equalsf(unfolded, closed.Substitute(x, next)).InnerSimplified(isExact);
            if (Truth(Decide(Kind.All, x, set, step, isExact)) is not { } stepCondition)
                return null;
            return Entity.Boolean.True.Provided((baseCondition & stepCondition).InnerSimplified(isExact));
        }

        private static bool IsUnboundedAbove(Intersectionf cut)
            => (cut.Left as Interval ?? cut.Right as Interval) is { } interval && interval.Right.Evaled is Real { EDecimal: var end } && end.IsInfinity() && !end.IsNegative;

        /// <summary>
        /// <c>forall x in S : L > R</c> (or <c>&gt;=</c>, <c>&lt;</c>, <c>&lt;=</c>) with an exponential
        /// or a factorial in <c>x</c>, from the least member of <c>S</c>: it holds there, and
        /// with <c>P = L - R</c>, <c>P(x + 1) = c P(x) + D</c> for a multiplier <c>c &gt;= 0</c> --
        /// <c>1</c>, the base of an exponential, <c>x + 1</c> beside a factorial -- and a
        /// remainder <c>D &gt;= 0</c> read by the sign calculus, so that <c>P(x) &gt;= 0</c> carries to
        /// <c>x + 1</c>; strictly, where <c>c &gt; 0</c> or <c>D &gt; 0</c>.
        /// </summary>
        private static Entity? ByInductionOnAnInequality(Variable x, Set set, Integer least, Entity body, bool isExact)
        {
            var (positive, strict) = body switch
            {
                Greaterf(var l, var r) => (l - r, true),
                GreaterOrEqualf(var l, var r) => (l - r, false),
                Lessf(var l, var r) => (r - l, true),
                LessOrEqualf(var l, var r) => (r - l, false),
                _ => (null, false),
            };
            if (positive is null || !positive.Nodes.Any(node => node is Powf(_, var e) && e.ContainsNode(x) || node is Factorialf f && f.ContainsNode(x)))
                return null;
            // The sign calculus asks the decision about polynomials only, so the step never asks
            // for an inequality of this shape again; the guard is against that ceasing to hold.
            if (inductionDepth > 0)
                return null;
            inductionDepth++;
            try
            {
                return ByInductionOnAnInequalityCore(x, set, least, body, positive, strict, isExact);
            }
            finally
            {
                inductionDepth--;
            }
        }

        [System.ThreadStatic] private static int inductionDepth;

        private static Entity? ByInductionOnAnInequalityCore(Variable x, Set set, Integer least, Entity body, Entity positive, bool strict, bool isExact)
        {
            var atLeast = body.Substitute(x, least).InnerSimplified(isExact);
            if (atLeast is Entity.Boolean)
                ProofRecording.AddBelow(body.Substitute(x, least), $"the base case, at {least}, by evaluation", "decide", atLeast);
            if (atLeast == Entity.Boolean.False)
                return Entity.Boolean.False;
            if (atLeast != Entity.Boolean.True)
                return null;
            // Read off the shape first: 3^(-t - 1) + t^3 is positive on ZZ* term by term.
            if (Sign(positive, x, set, isExact) is { } direct && (!strict || direct == Signum.Positive))
                return Entity.Boolean.True;
            var next = positive.Substitute(x, x + Integer.One);
            foreach (var c in Multipliers(positive, x))
            {
                var tried = ProofRecording.Mark();
                if (Sign(c, x, set, isExact) is { } multiplier
                    && Sign(Collected(next - c * positive), x, set, isExact) is { } remainder
                    && (!strict || multiplier == Signum.Positive || remainder == Signum.Positive))
                    return Entity.Boolean.True;
                ProofRecording.Rollback(tried);
            }
            return null;
        }

        /// <summary>The multipliers tried for the step: one, the whole base of each exponential in <paramref name="x"/>, and <c>x + 1</c> beside a factorial.</summary>
        private static IEnumerable<Entity> Multipliers(Entity positive, Variable x)
        {
            yield return Integer.One;
            foreach (var @base in positive.Nodes.OfType<Powf>().Where(p => p.Exponent.ContainsNode(x)).Select(p => p.Base).Where(b => b.Evaled is Integer).Distinct())
                yield return @base;
            if (positive.Nodes.Any(node => node is Factorialf f && f.ContainsNode(x)))
                yield return x + Integer.One;
        }

        /// <summary>
        /// The expression as a polynomial over its atoms, collected: <c>3^(x + 1) - 3 * 3^x</c> is
        /// <c>0</c>, <c>(x + 1)! - (x + 1) x!</c> is <c>0</c>, <c>2^(x + 1) - 2^x</c> is <c>2^x</c>.
        /// </summary>
        private static Entity Collected(Entity expr)
        {
            var atoms = new Dictionary<Entity, Variable>();
            var atomized = Atomized(expr.InnerSimplified, atoms, FactorialsUnfolded(expr.InnerSimplified));
            var variables = atomized.Vars.OrderBy(v => v.Name, System.StringComparer.Ordinal).ToArray();
            var indices = new Dictionary<Variable, int>();
            for (var i = 0; i < variables.Length; i++)
                indices[variables[i]] = i;
            var collected = variables.Length <= MultivariatePolynomial.MaxVariables && MultivariatePolynomial.TryParse(atomized, indices) is { } polynomial
                ? polynomial.ToEntity(variables)
                : atomized;
            foreach (var pair in atoms)
                collected = collected.Substitute(pair.Value, pair.Key);
            return collected.InnerSimplified;
        }

        private enum Signum { Positive, NonNegative }

        /// <summary>
        /// Whether the expression is positive, or non-negative, at every member of the set, read
        /// off its shape: a positive base to any power, a factorial of a whole number, sums and
        /// products of these, and a polynomial in <paramref name="x"/> decided over the set.
        /// <see langword="null"/> where neither is known.
        /// </summary>
        private static Signum? Sign(Entity expr, Variable x, Set set, bool isExact)
        {
            switch (expr)
            {
                case Real real:
                    return real.IsPositive ? Signum.Positive : real.IsNegative ? null : Signum.NonNegative;
                case Number:
                    return null;
                case Variable v when v == x:
                    return LeastMember(set) is { } least ? least.IsPositive ? Signum.Positive : least.IsNegative ? null : Signum.NonNegative : null;
                case Sumf(var a, var b):
                    return (Sign(a, x, set, isExact), Sign(b, x, set, isExact)) switch
                    {
                        (null, _) or (_, null) => SignByAtoms(expr, x, set, isExact),
                        (Signum.Positive, _) or (_, Signum.Positive) => Signum.Positive,
                        _ => Signum.NonNegative,
                    };
                case Minusf:
                    return SignOfPolynomial(expr, x, set, isExact) ?? SignByAtoms(expr, x, set, isExact);
                case Mulf(var a, var b):
                    return (Sign(a, x, set, isExact), Sign(b, x, set, isExact)) switch
                    {
                        (null, _) or (_, null) => null,
                        (Signum.Positive, Signum.Positive) => Signum.Positive,
                        _ => Signum.NonNegative,
                    };
                case Divf(var a, var b):
                    return (Sign(a, x, set, isExact), Sign(b, x, set, isExact)) switch
                    {
                        (null, _) or (_, null) or (_, Signum.NonNegative) => null,
                        (Signum.Positive, Signum.Positive) => Signum.Positive,
                        _ => Signum.NonNegative,
                    };
                // A positive base to any real power; the exponent is real where it mentions
                // nothing but x, which ranges over whole numbers.
                case Powf(var @base, var exponent) when exponent.Vars.All(v => v == x):
                    return Sign(@base, x, set, isExact) switch
                    {
                        Signum.Positive => Signum.Positive,
                        Signum.NonNegative when exponent.Evaled is Integer { IsNegative: false } => Signum.NonNegative,
                        _ => exponent.Evaled is Integer { EInteger.IsEven: true } && @base.Vars.All(v => v == x) ? Signum.NonNegative : null,
                    };
                case Factorialf(var argument) when argument.Vars.All(v => v == x) && IsWholePolynomial(argument):
                    return Sign(argument, x, set, isExact) is not null ? Signum.Positive : null;
                default:
                    return SignOfPolynomial(expr, x, set, isExact);
            }
        }

        /// <summary>
        /// A polynomial in <paramref name="x"/> and nothing else, decided over the set by the
        /// routes that read one, which do not come back here; <see langword="null"/> for anything else.
        /// </summary>
        private static Signum? SignOfPolynomial(Entity expr, Variable x, Set set, bool isExact)
        {
            if (!expr.Vars.All(v => v == x) || !expr.Vars.Any() || MultivariatePolynomial.TryParse(expr, new Dictionary<Variable, int> { [x] = 0 }) is null)
                return null;
            var tried = ProofRecording.Mark();
            if (Decide(Kind.All, x, set, new Greaterf(expr, Integer.Zero).InnerSimplified(isExact), isExact) is Entity.Boolean(true))
                return Signum.Positive;
            ProofRecording.Rollback(tried);
            if (Decide(Kind.All, x, set, new GreaterOrEqualf(expr, Integer.Zero).InnerSimplified(isExact), isExact) is Entity.Boolean(true))
                return Signum.NonNegative;
            ProofRecording.Rollback(tried);
            return null;
        }

        /// <summary>
        /// The sign of a sum read term by term: each term is a product of atoms -- exponentials
        /// and factorials in <paramref name="x"/> -- times a polynomial in <paramref name="x"/>,
        /// the polynomials are collected per atom product, and the sum is non-negative where
        /// every atom product and its collected coefficient are: <c>(n + 1) n! - 2 n!</c> is
        /// <c>(n - 1) n!</c>, non-negative on <c>ZZ+</c>.
        /// </summary>
        private static Signum? SignByAtoms(Entity expr, Variable x, Set set, bool isExact)
        {
            var groups = new Dictionary<Entity, Entity>();
            foreach (var term in Sumf.LinearChildren(expr))
            {
                Entity atomic = Integer.One;
                Entity coefficient = Integer.One;
                foreach (var factor in Mulf.LinearChildren(term))
                    if (factor.Vars.All(v => v == x) && MultivariatePolynomial.TryParse(factor, new Dictionary<Variable, int> { [x] = 0 }) is not null)
                        coefficient *= factor;
                    else
                        atomic *= factor;
                atomic = atomic.InnerSimplified;
                groups[atomic] = groups.TryGetValue(atomic, out var sum) ? sum + coefficient : coefficient;
            }
            if (groups.Count < 2 && expr is not Minusf)
                return null;
            var result = Signum.NonNegative;
            foreach (var pair in groups)
            {
                var atomSign = pair.Key == Integer.One ? Signum.Positive : Sign(pair.Key, x, set, isExact);
                var collected = pair.Value.InnerSimplified;
                var coefficientSign = collected is Number ? Sign(collected, x, set, isExact) : SignOfPolynomial(collected, x, set, isExact);
                if (atomSign is null || coefficientSign is null)
                    return null;
                if (atomSign == Signum.Positive && coefficientSign == Signum.Positive)
                    result = Signum.Positive;
            }
            return result;
        }

        /// <summary>
        /// The condition under which a decision is <see langword="true"/> -- <c>True</c> for
        /// <c>True</c> itself, the predicate for <c>True provided P</c> -- or
        /// <see langword="null"/> for anything else.
        /// </summary>
        private static Entity? Truth(Entity? decision)
            => decision switch
            {
                Entity.Boolean(true) => Entity.Boolean.True,
                Providedf(Entity.Boolean(true), var predicate) => predicate,
                _ => null,
            };

        internal static bool IsIntegerSet(SpecialSet set)
            => set.ToDomain() is Domain.Integer or Domain.NonNegativeInteger or Domain.PositiveInteger;

        /// <summary>How many residues a periodic statement is checked over before it is left as written.</summary>
        private const int LargestPeriod = 4096;

        /// <summary>
        /// The period of the body in <paramref name="x"/> over the whole numbers, where it has
        /// one: a divisibility by a whole number, or a congruence modulo one, of polynomials
        /// with whole coefficients, joined by the connectives; the least common multiple of the
        /// parts' periods.
        /// </summary>
        private static EInteger? Period(Entity body, Variable x, SpecialSet set)
        {
            switch (body)
            {
                case Entity.Boolean:
                    return EInteger.One;
                case Dividesf(Integer divisor, var dividend):
                    return divisor.EInteger.IsZero ? null : PeriodModulo(dividend, divisor.EInteger.Abs(), x, set);
                case Congruentf(var left, var right, Integer modulus):
                    return modulus.EInteger.IsZero ? null : PeriodModulo(left - right, modulus.EInteger.Abs(), x, set);
                case Notf(var operand):
                    return Period(operand, x, set);
                // A quantifier over whole numbers inside: its body is periodic in x with the
                // same period whatever the inner name takes, so the residues of x decide the
                // outer statement and the inner one is decided at each of them.
                case Quantifier { Over: SpecialSet inner } quantifier when IsIntegerSet(inner):
                    return Period(quantifier.Body, x, set);
                case Andf or Orf or Impliesf or Xorf:
                    if (body is not IBinaryNode { NodeFirstChild: var first, NodeSecondChild: var second })
                        return null;
                    if (Period(first, x, set) is not { } one || Period(second, x, set) is not { } another)
                        return null;
                    return one * another / one.Gcd(another);
                default:
                    return body.ContainsNode(x) ? null : EInteger.One;
            }
        }

        /// <summary>
        /// The period, in <paramref name="x"/>, of an expression's residue modulo
        /// <paramref name="modulus"/>: the modulus for a polynomial with whole coefficients,
        /// and with a power <c>a^e</c> of a whole base coprime to the modulus among the terms --
        /// <c>7^n - 4^n</c> modulo <c>3</c>, <c>2^n + 1</c> modulo <c>7</c> -- the least common
        /// multiple with the order of each base, since <c>a^(e + k) = a^e</c> modulo <c>m</c>
        /// where <c>a^k = 1</c>. Over the non-negative whole numbers only, where the powers are
        /// whole; <see langword="null"/> for a base sharing a factor with the modulus, whose
        /// powers repeat only eventually. Sullivan and Mackey's Ex 5.3.7 and §5.2.4 Try 3.
        /// </summary>
        private static EInteger? PeriodModulo(Entity expr, EInteger modulus, Variable x, SpecialSet set)
        {
            if (IsWholePolynomial(expr))
                return modulus;
            if (set.ToDomain() is not (Domain.NonNegativeInteger or Domain.PositiveInteger))
                return null;
            var period = modulus;
            var replaced = expr;
            var count = 0;
            foreach (var power in expr.Nodes.OfType<Powf>().Where(p => p.Exponent.ContainsNode(x)).Distinct())
            {
                if (power.Base.Evaled is not Integer { EInteger: var @base } || !power.Exponent.Vars.All(v => v == x) || !IsWholePolynomial(power.Exponent))
                    return null;
                if (!@base.Gcd(modulus).Equals(EInteger.One))
                    return null;
                var order = EInteger.One;
                var value = @base.Mod(modulus);
                while (!value.Equals(EInteger.One) && order.CompareTo(modulus) < 0)
                {
                    value = value * @base % modulus;
                    order += 1;
                }
                if (!value.Equals(EInteger.One))
                    return null;
                period = period * order / period.Gcd(order);
                replaced = replaced.Substitute(power, Variable.CreateUnique(expr, "power_" + count++));
            }
            return IsWholePolynomial(replaced) ? period : null;
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
        /// <summary>
        /// <c>True</c> where the two sides differ by a polynomial that expands to nothing,
        /// <c>True provided P</c> where they do once quotients are put over one denominator
        /// and <c>P</c> says the denominator's factors in the free names are not zero, and
        /// <see langword="null"/> otherwise. A factor in the quantified name has to be decided
        /// non-zero over the set, or the identity is not claimed.
        /// </summary>
        private static Entity? IsIdentity(Entity left, Entity right, Variable x, Set set, bool isExact)
        {
            var difference = (left - right).InnerSimplified;
            if (difference is Integer { IsZero: true })
                return Entity.Boolean.True;
            if (difference.Complexity > LargestDifferenceRead)
                return null;
            // Whatever is not a polynomial connective -- 2^(n + 1), n!, sin(x) -- is an atom the
            // polynomial is over: -(1 - 2^(n + 1)) = 2^(n + 1) - 1 is p - p in one atom, and a
            // polynomial that is zero in its atoms is zero at every value of them.
            var atoms = new Dictionary<Entity, Variable>();
            difference = Atomized(difference, atoms, FactorialsUnfolded(difference));
            Entity condition = Entity.Boolean.True;
            if (difference.Nodes.Any(node => node is Divf))
            {
                // A difference of quotients is zero where its numerator over the common
                // denominator is, and where that denominator is not: n/(n + 1) + 1/((n + 1)(n + 2))
                // = (n + 1)/(n + 2) at every whole n >= 1, and sum(k x^k, k, 1, n)'s closed form
                // over (1 - x)^2 holds provided x != 1.
                var (numerator, denominator) = SingleQuotient.Of(difference);
                foreach (var factor in Mulf.LinearChildren(denominator).Distinct())
                {
                    var written = factor;
                    foreach (var pair in atoms)
                        written = written.Substitute(pair.Value, pair.Key);
                    if (written.Evaled is Number)
                        continue;
                    if (written.ContainsNode(x))
                    {
                        if (Decide(Kind.Some, x, set, new Equalsf(written, Integer.Zero).InnerSimplified(isExact), isExact) is not Entity.Boolean(false))
                            return null;
                        continue;
                    }
                    condition &= new Notf(new Equalsf(written, Integer.Zero));
                }
                difference = numerator;
            }
            if (!IsZeroPolynomial(difference))
                return null;
            var simplified = condition.InnerSimplified(isExact);
            return simplified == Entity.Boolean.True ? Entity.Boolean.True : Entity.Boolean.True.Provided(simplified);
        }

        private static bool IsZeroPolynomial(Entity expr)
        {
            var variables = expr.Vars.OrderBy(v => v.Name, System.StringComparer.Ordinal).ToArray();
            if (variables.Length == 0 || variables.Length > MultivariatePolynomial.MaxVariables)
                return false;
            var indices = new Dictionary<Variable, int>();
            for (var i = 0; i < variables.Length; i++)
                indices[variables[i]] = i;
            return MultivariatePolynomial.TryParse(expr, indices) is { IsZero: true };
        }

        /// <summary>The whole number by which <paramref name="left"/> exceeds <paramref name="right"/> identically, or <see langword="null"/>.</summary>
        private static Integer? WholeGap(Entity left, Entity right)
        {
            var difference = (left - right).InnerSimplified;
            if (difference is Integer whole)
                return whole;
            var atOrigin = difference;
            foreach (var v in difference.Vars)
                atOrigin = atOrigin.Substitute(v, Integer.Zero);
            return atOrigin.Evaled is Integer gap && IsZeroPolynomial(difference - gap) ? gap : null;
        }

        /// <summary>
        /// Each factorial whose argument is a smaller factorial's argument plus a whole number,
        /// as that factorial times the numbers in between: <c>(n + 2)!</c> beside <c>n!</c> is
        /// <c>(n + 2) (n + 1) n!</c>, so that the two are one atom rather than two.
        /// </summary>
        private static Dictionary<Entity, Entity> FactorialsUnfolded(Entity expr)
        {
            var unfolded = new Dictionary<Entity, Entity>();
            var factorials = expr.Nodes.OfType<Factorialf>().Distinct().ToList();
            foreach (var factorial in factorials)
            {
                Factorialf @base = factorial;
                foreach (var other in factorials)
                    if (WholeGap(@base.Argument, other.Argument) is { IsNegative: false, IsZero: false } gap && gap.EInteger.CompareTo(EInteger.FromInt32(LargestFactorialGap)) <= 0)
                        @base = other;
                if (ReferenceEquals(@base, factorial) || WholeGap(factorial.Argument, @base.Argument) is not { } distance)
                    continue;
                Entity product = @base;
                for (var j = 1; j <= distance.EInteger.ToInt32Checked(); j++)
                    product *= @base.Argument + Integer.Create(j);
                unfolded[factorial] = product;
            }
            return unfolded;
        }

        private const int LargestFactorialGap = 16;

        /// <summary>
        /// The expression with every maximal subterm that is not a sum, a difference, a product,
        /// a whole power or a division replaced by one variable per distinct subterm.
        /// </summary>
        private static Entity Atomized(Entity expr, Dictionary<Entity, Variable> atoms, Dictionary<Entity, Entity> unfolded)
        {
            switch (expr)
            {
                case Number or Variable:
                    return expr;
                case Sumf(var a, var b):
                    return Atomized(a, atoms, unfolded) + Atomized(b, atoms, unfolded);
                case Minusf(var a, var b):
                    return Atomized(a, atoms, unfolded) - Atomized(b, atoms, unfolded);
                case Mulf(var a, var b):
                    return Atomized(a, atoms, unfolded) * Atomized(b, atoms, unfolded);
                case Divf(var a, var b):
                    return Atomized(a, atoms, unfolded) / Atomized(b, atoms, unfolded);
                case Powf(var a, Integer n):
                    return n.IsNegative ? Integer.One / Atomized(a, atoms, unfolded).Pow(-n) : Atomized(a, atoms, unfolded).Pow(n);
                case Factorialf when unfolded.TryGetValue(expr, out var product):
                    return Atomized(product, atoms, unfolded);
                // A power whose exponent carries a whole term, 2^(n + 1), is the power without
                // it times a number, 2 * 2^n, so that it is the atom 2^n is.
                case Powf(var a, var e) when Sumf.LinearChildren(e).Any(t => t.Evaled is Integer):
                    {
                        var shift = Sumf.LinearChildren(e).Select(t => t.Evaled).OfType<Integer>().Aggregate((Integer)Integer.Zero, (acc, t) => (Integer)(acc + t));
                        var rest = Sumf.LinearChildren(e).Where(t => t.Evaled is not Integer).Aggregate((Entity)Integer.Zero, (acc, t) => acc + t).InnerSimplified;
                        var scaled = shift.IsNegative ? Integer.One / Atomized(a, atoms, unfolded).Pow(-shift) : Atomized(a, atoms, unfolded).Pow(shift);
                        return Atomized(a.Pow(rest), atoms, unfolded) * scaled;
                    }
                // And a whole multiple in the exponent is a whole power of the atom: x^(2 n) is (x^n)^2.
                case Powf(var a, Mulf(Integer { IsNegative: false, IsZero: false } times, var r)):
                    return Atomized(a.Pow(r), atoms, unfolded).Pow(times);
                default:
                    if (!atoms.TryGetValue(expr, out var atom))
                        atoms[expr] = atom = Variable.CreateUnique(expr, "atom_" + atoms.Count);
                    return atom;
            }
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
                // The whole numbers from some least one, cut by an interval: empty exactly
                // when the interval ends before the least one.
                Intersectionf intersection when LeastMember(intersection) is { } least
                    => (intersection.Left as Interval ?? intersection.Right as Interval) is { } cut
                        ? cut.Right.Evaled is Real to && (Compare(least, to) > 0 || Compare(least, to) == 0 && !cut.RightClosed)
                        : (bool?)null,
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
        internal static bool SolverReads(Entity body, Variable x)
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
        internal static bool Equational(Entity body)
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
                    // The primes: the interval has one exactly when the first prime at or after
                    // its start is at or before its end, and there is always a next prime.
                    if (domain == Domain.Prime)
                    {
                        if (!to.IsFinite)
                            return Compare(to, 0) > 0;
                        var start = !from.IsFinite ? EInteger.FromInt32(2) : from.EDecimal.RoundToExponent(EInteger.Zero, ERounding.Ceiling).ToEInteger();
                        if (from.IsFinite && !interval.LeftClosed && from.EDecimal.CompareTo(EDecimal.FromEInteger(start)) == 0)
                            start += 1;
                        if (Functions.Primes.NextPrime(start) is not { } prime)
                            return null;
                        Real found = Integer.Create(prime);
                        return Compare(found, to) < 0 || Compare(found, to) == 0 && interval.RightClosed;
                    }
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
