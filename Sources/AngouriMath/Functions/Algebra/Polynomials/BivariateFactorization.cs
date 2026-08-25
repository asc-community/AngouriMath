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
    /// Factorisation of a polynomial in two variables, by reducing it to one variable and
    /// putting the answer back.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kronecker's substitution.</b> A factor of a polynomial of degree <c>d</c> in <c>x</c>
    /// has degree at most <c>d</c> in <c>x</c>, so with <c>s = d + 1</c> the map
    /// <c>x^i y^j -> t^(i + s*j)</c> is injective on every monomial that can appear in the
    /// polynomial or in any of its factors: <c>i</c> is the remainder and <c>j</c> the quotient of
    /// the exponent by <c>s</c>, and neither can be confused with another pair. So a factorisation
    /// of the one-variable image can be read back, and each subset of its irreducible factors
    /// names a candidate.
    /// </para>
    /// <para>
    /// <b>A candidate is a guess and is checked by division.</b> The image can factor further than
    /// the polynomial does — the substitution is injective on monomials, not on factorisations —
    /// so a subset whose product reads back as a polynomial need not divide the original. Every
    /// one is tested with exact division before it is kept, which is why this cannot answer
    /// wrongly: the worst it does is fail to find a factorisation that exists and say so.
    /// </para>
    /// <para>
    /// <b>What it will not do.</b> The image has degree <c>d + s * e</c> for a polynomial of
    /// degree <c>e</c> in <c>y</c>, and the one-variable factoriser stops at
    /// <see cref="IntegerPolynomial.MaxDegree"/> — so this reaches bidegrees like (2, 10),
    /// (3, 7) and (5, 4) and refuses beyond them. The recombination is over subsets, so the
    /// number of irreducible factors of the image is capped as well. Both limits are refusals,
    /// never wrong answers, and neither is the algorithm one would write to lift this ceiling:
    /// that is Hensel lifting with an evaluation homomorphism, and it is a different piece of
    /// work (<a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a> item 43).
    /// </para>
    /// </remarks>
    internal static class BivariateFactorization
    {
        /// <summary>
        /// Beyond this the subset search is refused rather than paid for: the recombination is
        /// over every subset of the image's irreducible factors, and a polynomial this reducible
        /// is not what the substitution is for.
        /// </summary>
        private const int MaxImageFactors = 12;

        /// <summary>
        /// The factors of <paramref name="poly"/> in <paramref name="x"/> and <paramref name="y"/>,
        /// each to the first power and each of positive degree in <paramref name="x"/>, or
        /// <see langword="null"/> where nothing could be settled. A single factor means the
        /// polynomial did not factor, which is an answer.
        /// </summary>
        internal static IReadOnlyList<MultivariatePolynomial>? Factor(
            MultivariatePolynomial poly, int x, int y)
        {
            var degreeInX = poly.DegreeIn(x);
            var degreeInY = poly.DegreeIn(y);
            if (poly.IsZero || degreeInX < 1 || degreeInY < 1)
                return null;

            var stride = degreeInX + 1;
            var imageDegree = degreeInX + stride * degreeInY;
            if (imageDegree > IntegerPolynomial.MaxDegree)
                return null;

            if (ToImage(poly, x, y, stride, imageDegree) is not { } image)
                return null;
            if (PolynomialFactorization.FactorPrimitive(image.PrimitivePart()) is not { } parts)
                return null;

            // The multiplicities are flattened: a square in the image may or may not be a square
            // in two variables, and the recombination settles that by division rather than by
            // carrying the exponent across the substitution.
            var irreducibles = new List<IntegerPolynomial>();
            foreach (var part in parts)
                for (var i = 0; i < part.Multiplicity; i++)
                {
                    if (irreducibles.Count == MaxImageFactors)
                        return null;
                    irreducibles.Add(part.Factor);
                }
            if (irreducibles.Count < 2)
                return new[] { poly };

            return Recombine(poly, irreducibles, x, y, stride);
        }

        /// <summary>
        /// The one-variable image, with the denominators cleared — the constant they came to is
        /// not wanted, since a rational multiple of a factor is the same factor.
        /// </summary>
        private static IntegerPolynomial? ToImage(
            MultivariatePolynomial poly, int x, int y, int stride, int imageDegree)
        {
            var coefficients = new ERational[imageDegree + 1];
            for (var i = 0; i < coefficients.Length; i++)
                coefficients[i] = ERational.Zero;

            foreach (var byX in poly.CoefficientsIn(x))
                foreach (var byY in byX.Value.CoefficientsIn(y))
                {
                    // Anything left is a third variable, and this is the two-variable case.
                    if (!byY.Value.IsConstant)
                        return null;
                    var at = byX.Key + stride * byY.Key;
                    if (at > imageDegree)
                        return null;
                    coefficients[at] = coefficients[at].Add(byY.Value.CoefficientOf(0));
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

        private static EInteger Lcm(EInteger left, EInteger right)
            => left.Divide(left.Gcd(right)).Multiply(right);

        /// <summary>
        /// Two variables again, reading each exponent as its remainder and quotient by
        /// <paramref name="stride"/> — or <see langword="null"/> where a power is past what a
        /// packed monomial holds.
        /// </summary>
        private static MultivariatePolynomial? FromImage(
            IntegerPolynomial image, int variableCount, int x, int y, int stride)
        {
            var result = MultivariatePolynomial.Zero(variableCount);
            for (var power = 0; power <= image.Degree; power++)
            {
                if (image[power].IsZero)
                    continue;
                var inX = power % stride;
                var inY = power / stride;
                if (inX > MultivariatePolynomial.MaxDegree || inY > MultivariatePolynomial.MaxDegree)
                    return null;
                if (MultivariatePolynomial.Monomial(variableCount, x).Power(inX) is not { } partX
                    || MultivariatePolynomial.Monomial(variableCount, y).Power(inY) is not { } partY
                    || partX.Multiply(partY) is not { } monomial)
                    return null;
                result = result.Add(monomial.ScaleBy(ERational.Create(image[power], EInteger.One)));
            }
            return result.IsZero ? null : result;
        }

        /// <summary>
        /// Every subset of the image's irreducible factors, smallest first, kept where its
        /// product divides what is left of the polynomial.
        /// </summary>
        /// <remarks>
        /// Smallest first so that what is taken out is irreducible: a subset that divides and
        /// whose proper subsets do not is a factor with nothing inside it. The loop restarts
        /// after each success because the remaining polynomial has changed.
        /// </remarks>
        private static IReadOnlyList<MultivariatePolynomial>? Recombine(
            MultivariatePolynomial poly, List<IntegerPolynomial> irreducibles, int x, int y, int stride)
        {
            var found = new List<MultivariatePolynomial>();
            var remaining = poly;
            var available = new List<IntegerPolynomial>(irreducibles);

            var progress = true;
            while (progress && available.Count > 0)
            {
                progress = false;
                for (var size = 1; size <= available.Count / 2 && !progress; size++)
                    foreach (var subset in Subsets(available.Count, size))
                    {
                        var product = IntegerPolynomial.One;
                        foreach (var index in subset)
                            if (product.Multiply(available[index]) is { } multiplied)
                                product = multiplied;
                            else
                                return null;
                        if (FromImage(product, poly.VariableCount, x, y, stride) is not { } candidate
                            || candidate.DegreeIn(x) < 1
                            || remaining.DivideExact(candidate) is not { } quotient)
                            continue;
                        found.Add(candidate.Normalized());
                        remaining = quotient;
                        for (var i = subset.Count - 1; i >= 0; i--)
                            available.RemoveAt(subset[i]);
                        progress = true;
                        break;
                    }
            }

            if (found.Count == 0)
                return new[] { poly };
            if (!remaining.IsConstant)
                found.Add(remaining.Normalized());
            // Nothing is returned that does not multiply back to what was asked about.
            var check = MultivariatePolynomial.One(poly.VariableCount);
            foreach (var factor in found)
                if (check.Multiply(factor) is { } multiplied)
                    check = multiplied;
                else
                    return null;
            return DividesBackExactly(check, poly) ? found : null;
        }

        /// <summary>
        /// Whether the factors multiply back to the polynomial up to a rational constant, which
        /// is as far as a factorisation is ever fixed.
        /// </summary>
        private static bool DividesBackExactly(MultivariatePolynomial product, MultivariatePolynomial poly)
            => !product.IsZero
               && poly.DivideExact(product) is { } quotient
               && quotient.IsConstant;

        /// <summary>The index subsets of a given size, in a fixed order.</summary>
        private static IEnumerable<List<int>> Subsets(int count, int size)
        {
            var chosen = new List<int>(size);
            return Walk(0);

            IEnumerable<List<int>> Walk(int from)
            {
                if (chosen.Count == size)
                {
                    yield return new List<int>(chosen);
                    yield break;
                }
                for (var index = from; index < count; index++)
                {
                    chosen.Add(index);
                    foreach (var subset in Walk(index + 1))
                        yield return subset;
                    chosen.RemoveAt(chosen.Count - 1);
                }
            }
        }
    }
}
