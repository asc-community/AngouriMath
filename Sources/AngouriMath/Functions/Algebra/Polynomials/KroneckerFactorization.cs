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
    /// Factorisation of a polynomial in any number of variables, by reducing it to one variable
    /// and putting the answer back.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kronecker's substitution, in mixed radix.</b> A factor of a polynomial has degree at
    /// most <c>d_i</c> in each variable <c>v_i</c>, because a factor divides it. So with radices
    /// <c>d_i + 1</c> and place values <c>s_0 = 1</c>, <c>s_(i+1) = s_i * (d_i + 1)</c>, the map
    /// <c>v_0^e_0 · … · v_(k-1)^e_(k-1) -> t^(Σ e_i · s_i)</c> writes each exponent as one digit
    /// of a numeral and is therefore injective on every monomial that can appear in the
    /// polynomial or in any of its factors. A factorisation of the one-variable image can be read
    /// back digit by digit, and each subset of its irreducible factors names a candidate.
    /// </para>
    /// <para>
    /// <b>A candidate is a guess and is checked by division.</b> The image can factor further
    /// than the polynomial does — the substitution is injective on monomials, not on
    /// factorisations — so a subset whose product reads back as a polynomial need not divide the
    /// original. Every one is tested with exact division before it is kept, which is why this
    /// cannot answer wrongly: the worst it does is fail to find a factorisation that exists and
    /// say so.
    /// </para>
    /// <para>
    /// <b>What it will not do.</b> The image has degree <c>Π (d_i + 1) - 1</c>, a *product* and
    /// not a sum, and the one-variable factoriser stops at
    /// <see cref="IntegerPolynomial.MaxDegree"/> — so the ceiling closes quickly as variables are
    /// added. Two variables reach bidegrees like (2, 10), (3, 7) and (5, 4); three variables of
    /// degree 2 fit (27 ≤ 32) and four do not (81); and a quadratic in eight variables is far
    /// past it. The recombination is over subsets, so the number of irreducible factors of the
    /// image is capped as well. Both limits are refusals, never wrong answers, and neither is the
    /// algorithm one would write to lift them: that is Hensel lifting with an evaluation
    /// homomorphism, and it is a different piece of work
    /// (<a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a> item 43).
    /// </para>
    /// </remarks>
    internal static class KroneckerFactorization
    {
        /// <summary>
        /// Beyond this the subset search is refused rather than paid for: the recombination is
        /// over every subset of the image's irreducible factors, and a polynomial this reducible
        /// is not what the substitution is for.
        /// </summary>
        private const int MaxImageFactors = 12;

        /// <summary>
        /// The factors of <paramref name="poly"/> in <paramref name="main"/> and whatever other
        /// variables it has, each to the first power and each of positive degree in
        /// <paramref name="main"/>, or <see langword="null"/> where nothing could be settled. A
        /// single factor means the polynomial did not factor, which is an answer.
        /// </summary>
        internal static IReadOnlyList<MultivariatePolynomial>? Factor(
            MultivariatePolynomial poly, int main)
        {
            if (poly.IsZero || poly.DegreeIn(main) < 1)
                return null;

            // The main variable is placed first so that its exponent is the lowest digit; the
            // rest follow in index order, and a variable the polynomial does not use is left out
            // rather than given a radix of one.
            var order = new List<int> { main };
            for (var variable = 0; variable < poly.VariableCount; variable++)
                if (variable != main && poly.DegreeIn(variable) > 0)
                    order.Add(variable);
            if (order.Count < 2)
                return null;

            var radices = new int[order.Count];
            var places = new int[order.Count];
            long size = 1;
            for (var i = 0; i < order.Count; i++)
            {
                radices[i] = poly.DegreeIn(order[i]) + 1;
                places[i] = (int)size;
                size *= radices[i];
                if (size > IntegerPolynomial.MaxDegree + 1)
                    return null;
            }
            var imageDegree = (int)size - 1;

            if (ToImage(poly, order, places, radices, imageDegree) is not { } image)
                return null;
            if (PolynomialFactorization.FactorPrimitive(image.PrimitivePart()) is not { } parts)
                return null;

            // The multiplicities are flattened: a square in the image may or may not be a square
            // in the original, and the recombination settles that by division rather than by
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

            return Recombine(poly, irreducibles, main, order, places, radices);
        }

        /// <summary>
        /// The one-variable image, with the denominators cleared — the constant they came to is
        /// not wanted, since a rational multiple of a factor is the same factor.
        /// </summary>
        private static IntegerPolynomial? ToImage(
            MultivariatePolynomial poly, IReadOnlyList<int> order,
            IReadOnlyList<int> places, IReadOnlyList<int> radices, int imageDegree)
        {
            var coefficients = new ERational[imageDegree + 1];
            for (var i = 0; i < coefficients.Length; i++)
                coefficients[i] = ERational.Zero;

            if (!Peel(poly, 0, 0))
                return null;

            var denominator = EInteger.One;
            foreach (var coefficient in coefficients)
                denominator = Lcm(denominator, coefficient.Denominator);
            var whole = new EInteger[coefficients.Length];
            for (var i = 0; i < whole.Length; i++)
                whole[i] = coefficients[i].Numerator
                    .Multiply(denominator.Divide(coefficients[i].Denominator));
            return IntegerPolynomial.Create(whole);

            // One variable at a time, adding that exponent's digit to the place already
            // accumulated. What is left when every variable has been peeled has to be a constant.
            bool Peel(MultivariatePolynomial rest, int depth, int at)
            {
                if (depth == order.Count)
                {
                    if (!rest.IsConstant)
                        return false;
                    coefficients[at] = coefficients[at].Add(rest.CoefficientOf(0));
                    return true;
                }
                foreach (var pair in rest.CoefficientsIn(order[depth]))
                {
                    if (pair.Key >= radices[depth])
                        return false;
                    if (!Peel(pair.Value, depth + 1, at + places[depth] * pair.Key))
                        return false;
                }
                return true;
            }
        }

        private static EInteger Lcm(EInteger left, EInteger right)
            => left.Divide(left.Gcd(right)).Multiply(right);

        /// <summary>
        /// Reading each exponent back as one digit of the numeral — or <see langword="null"/>
        /// where a power is past what a packed monomial holds.
        /// </summary>
        private static MultivariatePolynomial? FromImage(
            IntegerPolynomial image, int variableCount, IReadOnlyList<int> order,
            IReadOnlyList<int> places, IReadOnlyList<int> radices)
        {
            var result = MultivariatePolynomial.Zero(variableCount);
            for (var power = 0; power <= image.Degree; power++)
            {
                if (image[power].IsZero)
                    continue;
                var monomial = MultivariatePolynomial.One(variableCount);
                for (var i = 0; i < order.Count; i++)
                {
                    var digit = power / places[i] % radices[i];
                    if (digit > MultivariatePolynomial.MaxDegree)
                        return null;
                    if (MultivariatePolynomial.Monomial(variableCount, order[i]).Power(digit)
                            is not { } part
                        || monomial.Multiply(part) is not { } multiplied)
                        return null;
                    monomial = multiplied;
                }
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
            MultivariatePolynomial poly, List<IntegerPolynomial> irreducibles, int main,
            IReadOnlyList<int> order, IReadOnlyList<int> places, IReadOnlyList<int> radices)
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
                        if (FromImage(product, poly.VariableCount, order, places, radices)
                                is not { } candidate
                            || candidate.DegreeIn(main) < 1
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
