//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Collections.Generic;
using System.Linq;
using AngouriMath.Core.Exceptions;
using PeterO.Numbers;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Core
{
    /// <summary>
    /// The pattern operator, <c>...</c> (or <c>…</c>): between shown terms it names the
    /// progression the shown terms determine, and the term after it names where the
    /// progression stops. <c>{1, 2, ..., n}</c> is the integers from 1 to <c>n</c>,
    /// <c>{2, 4, ..., 2 n}</c> the even ones, <c>{5, 10, 15, ...}</c> every positive multiple of
    /// five, <c>{..., -1, 0}</c> the non-positive integers; <c>1 + 2 + ... + n</c> is
    /// <c>sum(k, k, 1, n)</c> and <c>1 * 2 * ... * n</c> is <c>product(k, k, 1, n)</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>What is read, and what is refused.</b> An arithmetic progression of whole numbers,
    /// from at least two shown terms on one side of the dots: the step is the difference of the
    /// first two, and every further shown term has to be on the progression, or the input is
    /// refused with the term that is not. Three shown terms with no step in common,
    /// <c>{1, 4, 9, ...}</c>, are a guess about the writer's rule and are refused rather than
    /// answered with the smallest polynomial through them. A progression of anything but whole
    /// numbers, <c>{1/2, 1, 3/2, ..., n}</c>, is refused too: a set of those is the image of a
    /// range, which has no node yet.
    /// </para>
    /// <para>
    /// <b>What is built.</b> Nothing new: a set is <c>ZZ /\ [a; z]</c> for step one and a residue
    /// class cut by the interval, <c>{ x in ZZ : x = a (mod d) } /\ [a; z]</c>, otherwise -- both
    /// list their members when the ends are numbers, and stay written over a symbolic end; a
    /// one-sided pattern is the class cut by a ray. A sum is <c>sum(a + d k, k, 0, (z - a)/d)</c>
    /// and a product the same under <c>product</c>, with the index named <c>k</c>, or <c>k_1</c>
    /// where the terms mention <c>k</c>. The general term with an index shown, <c>a_1 + ... + a_n</c>,
    /// and the list of names of symbolic length, <c>f(x_1, ..., x_n)</c>, are the larger half of
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1437">#1437</a> and wait for
    /// v3's design of a family of names.
    /// </para>
    /// </remarks>
    internal static class PatternOperator
    {
        /// <summary>The items of a set literal, <see langword="null"/> standing for the dots.</summary>
        internal static Entity Set(IReadOnlyList<Entity?> items)
        {
            var dots = items.Select((item, index) => (item, index)).Where(pair => pair.item is null).Select(pair => pair.index).ToList();
            if (dots.Count != 1)
                throw new InvalidArgumentParseException("A set written with dots has one ... in it, between or beside the terms that show the progression");
            var before = items.Take(dots[0]).Cast<Entity>().ToList();
            var after = items.Skip(dots[0] + 1).Cast<Entity>().ToList();
            if (before.Count >= 2)
            {
                var (first, step) = Progression(before);
                if (after.Count == 0)
                    return ClassOn(first, step, from: first, to: null);
                var last = after[^1];
                if (after.Count > 1)
                    CheckOnTheProgression(last, -step, after.Take(after.Count - 1).Reverse().ToList());
                return ClassOn(first, step, from: first, to: last);
            }
            if (before.Count == 1 && after.Count >= 1)
            {
                // {a, ..., z}: one term each side names the whole numbers from a to z, step one
                // being the convention when nothing shows another.
                var last = after[^1];
                if (after.Count > 1)
                    CheckOnTheProgression(last, Integer.MinusOne, after.Take(after.Count - 1).Reverse().ToList());
                return ClassOn(before[0], Integer.One, from: before[0], to: last);
            }
            if (after.Count >= 2 && before.Count == 0)
            {
                // {..., y, z}: the step is read in the written order, and the shown terms are
                // checked backwards from the end; the set is unbounded on the side of the dots.
                var last = after[^1];
                var step = Step(after[^2], after[^1]);
                CheckOnTheProgression(last, -step, after.Take(after.Count - 1).Reverse().ToList());
                return ClassOn(last, step, from: null, to: last);
            }
            throw new InvalidArgumentParseException("A set written with dots shows at least two terms of the progression on one side of them, as {1, 2, ..., n} or {..., -1, 0}");
        }

        /// <summary>
        /// The terms shown before the dots of a sum or a product, and the one after; the terms
        /// are <see langword="null"/> where a division or a <c>mod</c> stood among the factors.
        /// </summary>
        internal static Entity SumOrProduct(IReadOnlyList<Entity>? before, Entity? last, bool product)
        {
            if (before is null)
                throw new InvalidArgumentParseException("A product written with dots is a product of its factors, as 1 * 2 * ... * n, with no division or mod among them");
            if (before.Count < 2 || last is null)
                throw new InvalidArgumentParseException(
                    product ? "A product written with dots shows at least two factors before them and the last one after, as 1 * 2 * ... * n"
                            : "A sum written with dots shows at least two terms before them and the last one after, as 1 + 2 + ... + n");
            var (first, step) = Progression(before);
            var terms = before.Append(last).ToList();
            var mentioned = terms.SelectMany(term => term.Vars);
            var k = Variable.CreateUnique(mentioned.Any() ? terms.Aggregate((a, b) => a + b) : Integer.Zero, "k");
            // With step one the index is the term itself, sum(k, k, a, z); otherwise the term
            // is a + d k from k = 0 to (z - a)/d.
            if (step == Integer.One)
                return product ? MathS.Product(k, k, first, last) : MathS.Sum(k, k, first, last);
            var term = (first + step * k).InnerSimplified;
            var count = ((last - first) / step).InnerSimplified;
            return product ? MathS.Product(term, k, Integer.Zero, count) : MathS.Sum(term, k, Integer.Zero, count);
        }

        /// <summary>
        /// The first term and the step of the arithmetic progression the shown terms are on:
        /// the step is the difference of the first two, and each further term is checked.
        /// </summary>
        private static (Entity first, Entity step) Progression(IReadOnlyList<Entity> shown)
        {
            var first = shown[0];
            var step = Step(shown[0], shown[1]);
            CheckOnTheProgression(first, step, shown.Skip(2).ToList(), offset: 2);
            return (first, step);
        }

        /// <summary>The difference of two consecutive shown terms, simplified so that <c>(k + 1) - k</c> is <c>1</c>; zero is refused.</summary>
        private static Entity Step(Entity previous, Entity next)
        {
            var step = Functions.PartialFractions.Bare((next - previous).Simplify());
            if (step.Evaled is Complex { IsZero: true })
                throw new InvalidArgumentParseException($"The terms shown, {previous} and {next}, are the same, so the dots name no progression");
            return step;
        }

        private static void CheckOnTheProgression(Entity first, Entity step, IReadOnlyList<Entity> rest, int offset = 1)
        {
            for (var i = 0; i < rest.Count; i++)
            {
                var expected = (first + step * Integer.Create(i + offset)).InnerSimplified;
                if (expected != rest[i].InnerSimplified && !(Functions.PartialFractions.Bare((expected - rest[i]).Simplify()).Evaled is Complex { IsZero: true }))
                    throw new InvalidArgumentParseException(
                        $"The terms shown are not one arithmetic progression: after {first} with step {step}, {rest[i]} was written where {expected} would be. Three or more terms with no common step name no progression, and a rule other than a common step is not guessed");
            }
        }

        /// <summary>
        /// The whole numbers congruent to <paramref name="first"/> modulo the step, between the
        /// ends that are given: a ray or a segment of the class. Refused for anything but a
        /// whole first term and step.
        /// </summary>
        private static Entity ClassOn(Entity first, Entity step, Entity? from, Entity? to)
        {
            if (step.Evaled is not Integer difference || first.Evaled is Number and not Integer)
                throw new InvalidArgumentParseException(
                    $"Only a progression of whole numbers is read from dots in a set: {first} with step {step} is not one, and the image of a range has no node yet");
            var x = Variable.CreateUnique(first + step + (from ?? Integer.Zero) + (to ?? Integer.Zero), "x");
            // A symbolic first term, {k - 2, ..., k + 2}, is a whole number by the writer's
            // intent, and the class is written with it as the residue.
            Set members = first.Evaled is Integer start
                ? Functions.ResidueClasses.Create(x, start.EInteger, difference.EInteger)
                : difference.EInteger.Abs().Equals(EInteger.One) ? MathS.Sets.Z
                : new Entity.Set.ConditionalSet(x, x.In(MathS.Sets.Z) & new Congruentf(x, first, Integer.Create(difference.EInteger.Abs())));
            // A negative step runs down: the shown first term is the largest.
            var (low, high) = difference.EInteger.Sign > 0 ? (from, to) : (to, from);
            Entity left = low ?? Real.NegativeInfinity;
            Entity right = high ?? Real.PositiveInfinity;
            var cut = new Entity.Set.Interval(left, low is not null, right, high is not null);
            return members.Intersect(cut);
        }
    }
}
