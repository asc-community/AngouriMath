//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Collections.Generic;
using AngouriMath.Core;
using PeterO.Numbers;

namespace AngouriMath.Functions
{
    using static Entity;
    using static Entity.Number;

    /// <summary>
    /// The whole number every term of a sum divides by, taken out in front of it:
    /// <c>2x + 4a</c> is <c>2 * (x + 2a)</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <a href="https://github.com/asc-community/AngouriMath/issues/195">#195</a>, and the reason
    /// it says "forcefully… but not peacefully". The rules already take out a factor that appears
    /// <i>identically</i> in every term — <c>2x + 2a</c> is <c>2 * (a + x)</c> under plain
    /// <see cref="Entity.Simplify(int)"/> — and that is what "peacefully" means here. A factor that
    /// is only a common <b>divisor</b> is different: <c>2 * (x + 2 * a)</c> is one node larger than
    /// <c>4 * a + 2 * x</c>, so <c>SimplifiedRate</c> will not choose it and <c>Simplify</c> should
    /// not offer it.
    /// </para>
    /// <para>
    /// So this runs in <see cref="Entity.Factorize(int)"/> and nowhere else. That is already the
    /// forceful half of the pair — the call a user makes when they have decided they want the
    /// factored form, whatever it rates — and putting it there needs no new mode and leaves
    /// <c>Simplify</c> untouched.
    /// </para>
    /// <para>
    /// <b>Whole numbers only, and a positive one.</b> Rational coefficients have a common divisor
    /// too — <c>x/2 + a/3</c> over <c>1/6</c> — but taking it out puts a quotient outside the sum
    /// rather than a factor, which is a different rewrite and arguably the wrong direction. And the
    /// sign is left inside: <c>-2x - 4a</c> becomes <c>2 * (-x - 2a)</c> rather than
    /// <c>-2 * (x + 2a)</c>, because deciding which of those is wanted is a second question and
    /// this answers the first one.
    /// </para>
    /// </remarks>
    internal static class NumericContent
    {
        /// <summary>
        /// <paramref name="expr"/> with its numeric content in front, or the expression itself
        /// where there is none to take out.
        /// </summary>
        internal static Entity Extracted(Entity expr)
        {
            if (expr is not (Sumf or Minusf))
                return expr;

            var terms = new List<(Entity Term, bool Negated)>();
            Terms(expr, negated: false, terms);
            if (terms.Count < 2)
                return expr;

            var content = EInteger.Zero;
            var coefficients = new List<EInteger>(terms.Count);
            foreach (var (term, negated) in terms)
            {
                if (CoefficientOf(term) is not { } coefficient)
                    return expr;
                // The sign is carried here rather than by negating the term, because negating it
                // changes its shape: `-(4 * a)` comes back as a product by -1 wrapping a product
                // by 4, whose coefficient then reads as -1 and takes the content of every
                // difference down to 1. The sign belongs to the number, so it is applied to the
                // number.
                if (negated) coefficient = -coefficient;
                coefficients.Add(coefficient);
                // Gcd(0, n) is n, so seeding with zero makes the first term the running answer
                // without a special case for it, and Gcd is never negative.
                content = content.Gcd(coefficient);
            }

            // One is not a factor worth taking out, and zero cannot be: every coefficient being
            // zero means every term is, and the sum is already going to collapse without help.
            if (content.CompareTo(EInteger.One) <= 0)
                return expr;

            var reduced = Reduced(terms[0].Term, coefficients[0], content);
            for (var i = 1; i < terms.Count; i++)
                reduced += Reduced(terms[i].Term, coefficients[i], content);
            // InvertNegativeMultipliers afterwards, because a negated term inside the bracket is
            // what taking a
            // positive content out of a difference leaves: `2x - 4a` rebuilds as
            // `2 * (x + (-2) * a)`, and `a + c * b = a - (-c) * b` for a negative real c is what
            // writes it as `2 * (x - 2 * a)`. It is the
            // last step of Factorize, so nothing else would.
            return Core.Transformations.RewriteRules.InvertNegativeMultipliers.ApplyOnce(
                (Integer.Create(content) * reduced).InnerSimplified);
        }

        /// <summary>
        /// One term with the content divided out of it, written without the <c>1 *</c> that
        /// dividing a term by its own coefficient otherwise leaves.
        /// </summary>
        private static Entity Reduced(Entity term, EInteger coefficient, EInteger content)
        {
            var scaled = coefficient / content;
            var rest = WithoutCoefficient(term);
            return scaled.Equals(EInteger.One) ? rest : Integer.Create(scaled) * rest;
        }

        /// <summary>
        /// The terms of a chain of sums and differences, each carrying the sign it is under.
        /// </summary>
        /// <remarks>
        /// <c>Sumf.LinearChildren</c> descends sums only, so <c>2x - 4a</c> came back as one term
        /// and the content of a difference was never taken. Carrying the sign down instead makes
        /// <c>a - b</c> the pair <c>a</c> and <c>-b</c>, which is what it is, and a chain of any
        /// depth follows without a special case.
        /// </remarks>
        private static void Terms(Entity expr, bool negated, List<(Entity, bool)> into)
        {
            switch (expr)
            {
                case Sumf(var left, var right):
                    Terms(left, negated, into);
                    Terms(right, negated, into);
                    return;
                case Minusf(var left, var right):
                    Terms(left, negated, into);
                    Terms(right, !negated, into);
                    return;
                default:
                    into.Add((expr, negated));
                    return;
            }
        }

        /// <summary>
        /// The whole number this term is a multiple of, or <see langword="null"/> where it is not a
        /// whole multiple of anything — which is the answer that stops the whole sum.
        /// </summary>
        /// <remarks>
        /// Null rather than one, deliberately. A term with no whole coefficient does not contribute
        /// a 1 to the gcd, it makes the question unanswerable: the content of <c>2x + a/3</c> is
        /// not 1, it is a thing this does not compute, and returning 1 would say the sum has no
        /// content when what is true is that this cannot tell.
        /// </remarks>
        private static EInteger? CoefficientOf(Entity term) => term switch
        {
            Integer whole => whole.EInteger,
            Mulf(Integer whole, _) => whole.EInteger,
            Mulf(_, Integer whole) => whole.EInteger,
            // Anything else is its own coefficient of one: a bare variable, a function, a power.
            // A Rational or a Real is not, and falls through to null.
            Number => null,
            _ => EInteger.One,
        };

        /// <summary>What is left of the term once its whole coefficient is taken off.</summary>
        private static Entity WithoutCoefficient(Entity term) => term switch
        {
            Integer => Integer.One,
            Mulf(Integer, var rest) => rest,
            Mulf(var rest, Integer) => rest,
            _ => term,
        };
    }
}
