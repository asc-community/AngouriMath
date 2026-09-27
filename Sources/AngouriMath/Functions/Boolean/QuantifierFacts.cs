//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AngouriMath.Core;
using PeterO.Numbers;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Functions.Boolean
{
    /// <summary>
    /// What the quantifiers around a statement establish while they decide it: each bound name
    /// is a member of the set it ranges over, and a claim made under a hypothesis is made where
    /// the hypothesis holds. A rule that needs one of those facts, such as that <c>p</c> is prime
    /// or that <c>0 &lt; k &lt; p</c>, asks here.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A node has no parent, so a rule inside a body cannot walk up to the quantifier that binds
    /// its names. The quantifier hands the facts down instead, for as long as it takes to decide:
    /// the set its name ranges over, and for <c>forall x in S : H implies C</c> the conjuncts of
    /// <c>H</c> while <c>C</c> is simplified. They are never in scope while <c>H</c> itself is
    /// simplified, or <c>H implies C</c> would lose its hypothesis. For <c>exists</c> and
    /// <c>exists!</c> each conjunct of the body is simplified with the ones before it in scope.
    /// Scopes nest, so the inner quantifier of <c>forall p in PP : forall k in ZZ : ...</c> knows
    /// that <c>p</c> is prime. Nothing leaves a scope: a verdict reached with the facts is the
    /// verdict on the quantified statement, which states them, and it carries no condition.
    /// </para>
    /// <para>
    /// The bound name is renamed first, to a name no other expression has, and every fact is
    /// about a renamed name. That is what keeps a verdict from escaping through a cache. A node
    /// that caches its simplification can only have read a fact if it mentions a renamed name,
    /// and every such node is built during this decision and is reachable from nothing else. A
    /// cache keyed on the structure of an expression cannot serve it elsewhere either, since no
    /// other expression has the name. For the same reason a conjunct of the hypothesis about the
    /// free names alone is not recorded: a rule reading it could simplify a node that is shared.
    /// A substitution stops at a binder, so an inner quantifier that binds the same name again
    /// is not renamed through, and the outer facts about the name do not reach it.
    /// </para>
    /// <para>
    /// Only a body in which a rule reads the facts is renamed and decided this way. Every other
    /// statement is decided as it was, under its own names.
    /// </para>
    /// https://github.com/asc-community/AngouriMath/issues/1409
    /// </remarks>
    internal static class QuantifierFacts
    {
        /// <summary>One scope: the name it renamed, where it bound one, and what it establishes.</summary>
        private sealed class Frame
        {
            internal readonly Variable? Renamed;
            internal readonly Entity[] Facts;
            internal readonly Frame? Next;
            internal Frame(Variable? renamed, Entity[] facts, Frame? next) => (Renamed, Facts, Next) = (renamed, facts, next);
        }

        [System.ThreadStatic] private static Frame? top;

        /// <summary>Numbers the renamed names, which is what makes each one unlike any other.</summary>
        [ConcurrentField] private static long lastRenamed;

        /// <summary>
        /// Decides the quantified statement with the facts in scope, or <see langword="null"/>
        /// where no rule in the body reads them, for the statement to be decided as before. The
        /// verdict is <see langword="null"/> where it is not settled, beside the body simplified
        /// as far as it went, under the name it was given.
        /// </summary>
        internal static (Entity? Verdict, Entity Body)? Decide(Quantifiers.Kind kind, Entity var, Entity over, Entity body, bool isExact)
        {
            if (var is not Variable x || over is not Set set || !body.Nodes.Any(Reads))
                return null;
            var renamed = Variable.CreateVariableUnchecked($"{x.Name}'{Interlocked.Increment(ref lastRenamed)}");
            var mark = ProofRecording.Mark();
            var enclosing = top;
            top = new Frame(renamed, new[] { renamed.In(set) }, enclosing);
            try
            {
                // One level down, so that what a rule records while the body is simplified is
                // written under the statement the quantifier records when it decides.
                var written = body.Substitute(x, renamed);
                Entity simplified;
                ProofRecording.Enter();
                try
                {
                    simplified = Simplified(kind, written, isExact);
                }
                finally
                {
                    ProofRecording.Leave();
                }
                var verdict = Quantifiers.Decide(kind, renamed, set, simplified, isExact);
                if (verdict is null)
                    ProofRecording.Rollback(mark);
                // A body the facts made True is recorded as it was written: the decision's own
                // step would otherwise read `forall k in ZZ : True`.
                else if (kind == Quantifiers.Kind.All && simplified == Entity.Boolean.True)
                    ProofRecording.Restate(mark, new Forallf(renamed, set, written),
                        written is Impliesf(var hypothesis, var claim)
                            ? $"{claim} holds for every {renamed} in {set} with {hypothesis}"
                            : $"the body holds for every {renamed} in {set}",
                        "intro");
                ProofRecording.Rename(mark, renamed, x);
                return (verdict is null ? null : Renamed(verdict, renamed, x), Renamed(simplified, renamed, x));
            }
            finally
            {
                top = enclosing;
            }
        }

        /// <summary>
        /// The body simplified with the facts in scope: a claim made under a hypothesis with the
        /// conjuncts of the hypothesis in scope as well, and the hypothesis itself with only the
        /// facts from outside it.
        /// </summary>
        private static Entity Simplified(Quantifiers.Kind kind, Entity body, bool isExact)
        {
            switch (kind, body)
            {
                // forall x in S : H implies C, and H1 implies (H2 implies C) with both in scope for C.
                case (Quantifiers.Kind.All, Impliesf(var hypothesis, var claim)):
                {
                    var assumed = hypothesis.InnerSimplified(isExact);
                    var enclosing = Establish(assumed);
                    Entity concluded;
                    try
                    {
                        concluded = Simplified(kind, claim, isExact);
                    }
                    finally
                    {
                        top = enclosing;
                    }
                    return new Impliesf(assumed, concluded).InnerSimplified(isExact);
                }
                // exists x in S : A and B and C, each conjunct with the ones before it in scope.
                case (not Quantifiers.Kind.All, Andf):
                {
                    var conjuncts = new List<Entity>();
                    var enclosing = top;
                    try
                    {
                        foreach (var conjunct in Andf.LinearChildren(body))
                        {
                            var simplified = conjunct.InnerSimplified(isExact);
                            conjuncts.Add(simplified);
                            Establish(simplified);
                        }
                    }
                    finally
                    {
                        top = enclosing;
                    }
                    return conjuncts.Aggregate((left, right) => new Andf(left, right)).InnerSimplified(isExact);
                }
                default:
                    return body.InnerSimplified(isExact);
            }
        }

        /// <summary>
        /// Puts in scope the conjuncts of <paramref name="statement"/> that mention a renamed
        /// name, and returns what was in scope before.
        /// </summary>
        private static Frame? Establish(Entity statement)
        {
            var enclosing = top;
            var facts = Andf.LinearChildren(statement).Where(MentionsARenamedName).ToArray();
            if (facts.Length > 0)
                top = new Frame(null, facts, enclosing);
            return enclosing;
        }

        /// <summary>
        /// The expression with <paramref name="from"/> read as <paramref name="to"/> everywhere,
        /// the name a quantifier binds included, which a substitution leaves alone.
        /// </summary>
        internal static Entity Renamed(Entity expression, Variable from, Variable to)
            => expression.Replace(node => node switch
            {
                Variable name when name == from => to,
                Quantifier quantifier when quantifier.Var == from => quantifier.New(to, quantifier.Over, quantifier.Body),
                _ => node
            });

        /// <summary>
        /// Whether a rule reads the facts at <paramref name="node"/>, which is what a body is
        /// renamed and decided with the facts in scope for.
        /// </summary>
        private static bool Reads(Entity node)
            => node is Dividesf(var divisor, Binomialf(var upper, _)) && divisor == upper;

        private static bool MentionsARenamedName(Entity expression)
        {
            if (top is null)
                return false;
            foreach (var name in expression.Vars)
                for (var frame = top; frame is not null; frame = frame.Next)
                    if (frame.Renamed is { } renamed && renamed == name)
                        return true;
            return false;
        }

        private static IEnumerable<Entity> InScope()
        {
            for (var frame = top; frame is not null; frame = frame.Next)
                foreach (var fact in frame.Facts)
                    yield return fact;
        }

        /// <summary>
        /// Whether <c>p divides binomial(p, k)</c> follows from the facts in scope: <c>p</c> is
        /// prime and <c>k</c> is a whole number with <c>0 &lt; k &lt; p</c>. Then <c>p</c> divides
        /// <c>p! = binomial(p, k) k! (p - k)!</c> and none of the factors of <c>k! (p - k)!</c>,
        /// each of which is below the prime <c>p</c>. Sullivan and Mackey's Prob 8.9.24.
        /// </summary>
        internal static bool PrimeDividesItsBinomial(Entity p, Entity k)
        {
            if (top is null || !IsPrime(p) || !IsWhole(k) || !IsPositive(k) || !IsPositive(p - k))
                return false;
            if (ProofRecording.Recording)
                ProofRecording.AddBelow(new Dividesf(p, new Binomialf(p, k)),
                    $"{p} is prime and 0 < {k} < {p}, so {p} divides {p}! = binomial({p}, {k}) {k}! ({p} - {k})! and none of the factors of {k}! ({p} - {k})!",
                    "Nat.Prime.dvd_choose_self", Entity.Boolean.True);
            return true;
        }

        private static bool IsPrime(Entity expression)
            => expression is Integer number ? Functions.Primes.IsPrime(number.EInteger) == true : IsIn(expression, 0);

        private static bool IsWhole(Entity expression)
            => expression is Integer || IsIn(expression, 3);

        /// <summary>
        /// Whether the facts in scope put <paramref name="element"/> in the set of numbers ranked
        /// <paramref name="rank"/> by <see cref="Rank"/>, or in one inside it.
        /// </summary>
        private static bool IsIn(Entity element, int rank)
        {
            if (!MentionsARenamedName(element))
                return false;
            foreach (var fact in InScope())
                if (fact is Inf(var member, SpecialSet within) && member == element && Rank(within) is var inner and >= 0 && inner <= rank)
                    return true;
            return false;
        }

        /// <summary>The sets of numbers, each inside the next: PP, ZZ+, ZZ*, ZZ, QQ, RR, CC.</summary>
        private static int Rank(SpecialSet set) => set switch
        {
            SpecialSet.Primes => 0,
            SpecialSet.PositiveIntegers => 1,
            SpecialSet.NonNegativeIntegers => 2,
            SpecialSet.Integers => 3,
            SpecialSet.Rationals => 4,
            SpecialSet.Reals => 5,
            SpecialSet.Complexes => 6,
            _ => -1
        };

        /// <summary>
        /// Whether <paramref name="expression"/> is positive by the facts in scope. Each fact bounds
        /// a quantity below: <c>a &gt; b</c> and <c>b &lt; a</c> say <c>a - b &gt; 0</c>, and a
        /// member of the primes, the positive or the non-negative whole numbers is at least 2, 1
        /// or 0. The expression is positive where it is such a quantity plus a constant that is
        /// not negative, or plus a positive one where the bound is not strict: <c>p - 1</c> for a
        /// prime <c>p</c> is <c>p - 2</c> plus 1.
        /// </summary>
        private static bool IsPositive(Entity expression)
        {
            if (expression is Real number)
                return number.IsPositive;
            if (!MentionsARenamedName(expression))
                return false;
            foreach (var fact in InScope())
            {
                var bound = fact switch
                {
                    Lessf(var smaller, var greater) => (Difference: greater - smaller, Strict: true),
                    Greaterf(var greater, var smaller) => (greater - smaller, true),
                    LessOrEqualf(var smaller, var greater) => (greater - smaller, false),
                    GreaterOrEqualf(var greater, var smaller) => (greater - smaller, false),
                    Inf(var member, SpecialSet.Primes) => (member - 2, false),
                    Inf(var member, SpecialSet.PositiveIntegers) => (member - 1, false),
                    Inf(var member, SpecialSet.NonNegativeIntegers) => (member, false),
                    _ => ((Entity Difference, bool Strict)?)null
                };
                if (bound is var (difference, strict) && Gap(expression, difference) is { } gap
                    && (strict ? gap.Sign >= 0 : gap.Sign > 0))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// The constant <c>a - b</c>, where the two differ by one as polynomials, and
        /// <see langword="null"/> otherwise.
        /// </summary>
        private static ERational? Gap(Entity a, Entity b)
        {
            var difference = a - b;
            var names = difference.Vars.Distinct().ToArray();
            if (names.Length > MultivariatePolynomial.MaxVariables)
                return null;
            var indices = new Dictionary<Variable, int>();
            for (var i = 0; i < names.Length; i++)
                indices[names[i]] = i;
            if (MultivariatePolynomial.TryParse(difference, indices) is not { IsConstant: true } constant)
                return null;
            return constant.IsZero ? ERational.Zero : constant.Terms.Single().Value;
        }
    }
}
