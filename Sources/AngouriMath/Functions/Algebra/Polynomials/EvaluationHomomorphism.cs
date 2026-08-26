//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using PeterO.Numbers;
using System.Collections.Generic;

namespace AngouriMath.Functions
{
    /// <summary>
    /// Deciding that a polynomial in several variables <b>does not factor</b>, by evaluating
    /// every variable but one at an integer point and asking the one-variable factoriser.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Substituting integers for variables is a ring homomorphism, so a factorisation survives
    /// it: if <c>f = g·h</c> then <c>f(x, a) = g(x, a)·h(x, a)</c>. Degrees survive it too, as
    /// long as the leading coefficient in <c>x</c> does not vanish at the point — and degrees in
    /// <c>x</c> add, so if the total is preserved then neither part can have lost any. So <b>an
    /// image that is irreducible, of the same degree in <c>x</c>, is a proof that the polynomial
    /// it came from is irreducible</b>.
    /// </para>
    /// <para>
    /// The converse is not true and is not claimed. An image factors more readily than its
    /// source — <c>x^2 + y</c> at <c>y = 4</c> is <c>x^2 + 4</c>, still irreducible, but at
    /// <c>y = -4</c> it is <c>(x-2)(x+2)</c> — so a reducible image says nothing at all, and
    /// several points are tried before giving up. This decides one direction and declines the
    /// other, which is why it can be asked first and cheaply.
    /// </para>
    /// <para>
    /// <b>Why it is worth having next to <see cref="KroneckerFactorization"/>.</b> The
    /// substitution's image has degree <c>Π (d_i + 1) - 1</c>, a product, so it leaves the
    /// one-variable factoriser's reach after very few variables and the answer is a refusal —
    /// <c>x^2 + y^2 + z^2 + w^2 + 1</c> is past it. An evaluation image has degree <c>d_main</c>
    /// and does not grow with the variable count at all, so the polynomials this settles are
    /// exactly the ones the substitution cannot reach. It answers only "it does not factor", but
    /// since <a href="https://github.com/asc-community/AngouriMath/pull/1059">#1059</a> that is
    /// an answer rather than a refusal.
    /// </para>
    /// <para>
    /// <b>The precondition is checked and not assumed.</b> The argument above is about factors
    /// of positive degree in <c>x</c>; a factor that is free of <c>x</c> — the content — is
    /// invisible to it, and <c>y·(x + 1)</c> would be certified irreducible on an image of
    /// <c>2x + 2</c> whose primitive part is <c>x + 1</c>. So the content in the main variable is
    /// computed, and anything but a constant declines. The caller has a path that takes the
    /// content out and asks again.
    /// </para>
    /// <para>
    /// <b>This is not Hensel lifting</b>, and does not pretend to be the piece of
    /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a> tier 1 that is
    /// still outstanding. It is the first step of that algorithm — choosing an evaluation point
    /// whose image keeps the degree and stays square-free — used for the one conclusion that
    /// needs no lifting. Lifting a *reducible* image back to a factorisation of the source is the
    /// rest of it, and is a different piece of work.
    /// </para>
    /// </remarks>
    internal static class EvaluationHomomorphism
    {
        /// <summary>
        /// Points tried, in this order. Small values keep the image's coefficients small, which
        /// is what the one-variable factoriser's own bound is about; 0 is first because it makes
        /// the image the coefficient of the constant term and is by far the cheapest, and both
        /// signs are tried because a sign is exactly what decides <c>x^2 ± 4</c>.
        /// </summary>
        [ConstantField] private static readonly int[] Points = { 0, 1, -1, 2, -2, 3, -3, 5 };

        /// <summary>
        /// Combinations of <see cref="Points"/> tried before giving up. A point is a vector over
        /// every variable but the main one, so the space is exponential in the variable count and
        /// is walked in a fixed, small number of steps rather than enumerated.
        /// </summary>
        private const int MaxAttempts = 24;

        /// <summary>
        /// Whether <paramref name="poly"/> is irreducible over the rationals as a polynomial of
        /// positive degree in <paramref name="main"/>. <see langword="false"/> means <b>not
        /// settled</b> — it never means reducible.
        /// </summary>
        internal static bool CertifiesIrreducible(MultivariatePolynomial poly, int main)
        {
            var degree = poly.DegreeIn(main);
            if (degree < 1 || poly.IsZero)
                return false;

            var others = new List<int>();
            for (var variable = 0; variable < poly.VariableCount; variable++)
                if (variable != main && poly.DegreeIn(variable) > 0)
                    others.Add(variable);
            if (others.Count == 0)
                return false;

            // A factor free of the main variable is not something an image in that variable can
            // see, so this only speaks for a polynomial that has none.
            if (PolynomialGcd.ContentIn(poly, main, others, 0) is not { IsConstant: true })
                return false;

            var point = new int[others.Count];
            for (var attempt = 0; attempt < MaxAttempts; attempt++)
            {
                if (!NextPoint(point, attempt, others.Count))
                    return false;
                if (ToImage(poly, main, others, point) is not { } image)
                    continue;
                // The leading coefficient in the main variable vanished at this point, so the
                // image has lost degree and its factorisation says nothing about the source.
                if (image.Degree != degree)
                    continue;
                if (PolynomialFactorization.FactorPrimitive(image.PrimitivePart()) is not { } parts)
                    continue;
                if (parts.Count == 1 && parts[0].Multiplicity == 1 && parts[0].Factor.Degree == degree)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// The <paramref name="attempt"/>th point, as a mixed-radix numeral over
        /// <see cref="Points"/> — so the first few vary the last variable, which is the cheapest
        /// thing to vary, and every coordinate is eventually reached.
        /// </summary>
        private static bool NextPoint(int[] point, int attempt, int count)
        {
            var left = attempt;
            for (var i = count - 1; i >= 0; i--)
            {
                point[i] = Points[left % Points.Length];
                left /= Points.Length;
            }
            // Past the last numeral the sequence would repeat, so there is nothing more to try.
            return left == 0;
        }

        /// <summary>
        /// <paramref name="poly"/> with every variable but <paramref name="main"/> replaced by
        /// its coordinate of <paramref name="point"/>, with the denominators cleared — the
        /// constant they came to is not wanted, a rational multiple of a factor being the same
        /// factor.
        /// </summary>
        private static IntegerPolynomial? ToImage(
            MultivariatePolynomial poly, int main, IReadOnlyList<int> others, IReadOnlyList<int> point)
        {
            var degree = poly.DegreeIn(main);
            var coefficients = new ERational[degree + 1];
            for (var i = 0; i < coefficients.Length; i++)
                coefficients[i] = ERational.Zero;

            foreach (var pair in poly.CoefficientsIn(main))
            {
                if (pair.Key >= coefficients.Length)
                    return null;
                if (Evaluate(pair.Value, others, point, 0) is not { } value)
                    return null;
                coefficients[pair.Key] = coefficients[pair.Key].Add(value);
            }

            var denominator = EInteger.One;
            foreach (var coefficient in coefficients)
                denominator = Lcm(denominator, coefficient.Denominator);
            var whole = new EInteger[coefficients.Length];
            for (var i = 0; i < whole.Length; i++)
                whole[i] = coefficients[i].Numerator
                    .Multiply(denominator.Divide(coefficients[i].Denominator));
            return IntegerPolynomial.Create(whole);
        }

        /// <summary>
        /// One variable at a time, Horner-free: each coefficient is evaluated at the remaining
        /// variables and weighted by the point raised to that exponent. What is left when every
        /// variable has been taken has to be a constant, and anything else declines.
        /// </summary>
        private static ERational? Evaluate(
            MultivariatePolynomial poly, IReadOnlyList<int> others, IReadOnlyList<int> point, int depth)
        {
            if (depth == others.Count)
                return poly.IsZero ? ERational.Zero
                    : poly.IsConstant ? poly.CoefficientOf(0) : (ERational?)null;

            var total = ERational.Zero;
            var at = EInteger.FromInt32(point[depth]);
            foreach (var pair in poly.CoefficientsIn(others[depth]))
            {
                if (Evaluate(pair.Value, others, point, depth + 1) is not { } inner)
                    return null;
                total = total.Add(inner.Multiply(ERational.Create(at.Pow(pair.Key), EInteger.One)))
                    .ToLowestTerms();
            }
            return total;
        }

        private static EInteger Lcm(EInteger left, EInteger right)
            => left.Divide(left.Gcd(right)).Multiply(right);
    }
}
