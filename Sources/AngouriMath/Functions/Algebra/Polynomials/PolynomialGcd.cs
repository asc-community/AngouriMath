//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Multithreading;
using System;
using System.Diagnostics.CodeAnalysis;
using static AngouriMath.Entity;

namespace AngouriMath.Functions
{
    /// <summary>
    /// The greatest common divisor of two polynomials in several variables over the
    /// rationals, and the cancellation of a quotient that it makes possible:
    /// <c>(x^2 + 2xy + y^2) / (x^2 - y^2)</c> is <c>(x + y) / (x - y)</c>
    /// (<a href="https://github.com/asc-community/AngouriMath/issues/55">#55</a>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// A polynomial in several variables is a polynomial in one of them whose coefficients
    /// are polynomials in the rest, so the algorithm is the univariate one applied down a
    /// recursion on the variable count. What it cannot be is the plain Euclidean algorithm
    /// over that coefficient ring: pseudo-division multiplies through by the leading
    /// coefficient every step, and the coefficients of the remainder sequence then grow
    /// exponentially in the degree. The classic example is Knuth's, where two degree-8
    /// polynomials over the integers produce a remainder with a coefficient near 10^35.
    /// </para>
    /// <para>
    /// Two things keep that in hand, and both are needed. The greatest common divisor
    /// splits as the gcd of the contents times the gcd of the primitive parts, because the
    /// content carries everything free of the main variable and the primitive parts carry
    /// nothing of it; so the content is taken out first, recursively, in one variable
    /// fewer. And within the remainder sequence the subresultant coefficients are divided
    /// out at each step — that division is exact, since what is left is a subresultant, and
    /// it is what turns exponential growth into linear. Knuth, <i>TAOCP</i> vol. 2,
    /// §4.6.1, algorithms C and E; Geddes, Czapor and Labahn, <i>Algorithms for Computer
    /// Algebra</i>, §7.3.
    /// </para>
    /// <para>
    /// Nothing here trusts the result of that machinery. The divisor it computes is
    /// divided out of both sides and multiplied back, and the quotient is only used when
    /// both come out equal to what they started as. A divisor that is merely common and
    /// not greatest costs an incomplete cancellation; one that is not a divisor at all
    /// would be a wrong answer, and is the thing the check is there to stop.
    /// </para>
    /// </remarks>
    internal static class PolynomialGcd
    {
        /// <summary>
        /// A quotient larger than this is left alone. The bound is on node count, and is
        /// there because this runs on every quotient the simplifier constructs, not
        /// because a larger one would be wrong.
        /// </summary>
        private const int MaxComplexity = 256;

        /// <summary>
        /// Enough for any sequence a polynomial within the degree bound can produce; a
        /// cheap ceiling so that a mistake shows up as a refusal rather than a hang.
        /// </summary>
        private const int MaxSteps = 256;

        /// <summary>
        /// Puts <c>numerator / denominator</c> into lowest terms, or answers
        /// <see langword="null"/> where it is already in them, where either side is not a
        /// polynomial over the rationals, or where the machinery declines.
        /// </summary>
        /// <remarks>
        /// The answer carries the condition that the cancelled factor is nonzero. Where
        /// that factor vanishes the quotient was <c>0/0</c> and the reduced form is
        /// something definite, so dropping the condition would widen the domain and claim a
        /// value where there is none — which is how the library already answers the cases
        /// it can reach without this, <c>(x^2 - 1) / (x - 1)</c> being
        /// <c>x + 1 provided not x - 1 = 0</c>.
        /// </remarks>
        internal static bool TryCancel(Entity numerator, Entity denominator,
            [NotNullWhen(true)] out Entity? cancelled)
        {
            cancelled = null;

            // Cheap refusals first. Two polynomials with no variable in common are coprime
            // whatever they are, and there is no point parsing them to find that out.
            if (!numerator.Vars.Any(denominator.Vars.Contains))
                return false;
            if (numerator.Complexity + denominator.Complexity > MaxComplexity)
                return false;

            var variables = numerator.Vars
                .Concat(denominator.Vars)
                .Distinct()
                .OrderBy(variable => variable.Name, StringComparer.Ordinal)
                .ToArray();
            if (variables.Length > MultivariatePolynomial.MaxVariables)
                return false;
            var indices = new Dictionary<Variable, int>(variables.Length);
            for (var i = 0; i < variables.Length; i++)
                indices[variables[i]] = i;

            if (MultivariatePolynomial.TryParse(numerator, indices) is not { } top
                || MultivariatePolynomial.TryParse(denominator, indices) is not { } bottom)
                return false;
            // A constant numerator or denominator has nothing to cancel against, and a
            // zero denominator is not this function's business to rewrite.
            if (top.IsConstant || bottom.IsConstant)
                return false;

            var order = new int[variables.Length];
            for (var i = 0; i < order.Length; i++)
                order[i] = i;

            if (Gcd(top, bottom, order, 0) is not { } divisor || divisor.IsConstant)
                return false;
            if (top.DivideExact(divisor) is not { } reducedTop
                || bottom.DivideExact(divisor) is not { } reducedBottom)
                return false;

            // Multiplied back, independently of the division that produced them: a
            // cancellation is only made once it has been seen to be one.
            if (reducedTop.Multiply(divisor) is not { } checkedTop || !checkedTop.SameAs(top)
                || reducedBottom.Multiply(divisor) is not { } checkedBottom || !checkedBottom.SameAs(bottom))
                return false;

            cancelled = new Providedf(
                reducedTop.ToEntity(variables) / reducedBottom.ToEntity(variables),
                !divisor.ToEntity(variables).EqualTo(0));
            return true;
        }

        /// <summary>
        /// The greatest common divisor of two polynomials over the rationals, normalized to
        /// whole coprime coefficients with a positive leading one, or <see langword="null"/>
        /// where a step of the algorithm declined.
        /// </summary>
        internal static MultivariatePolynomial? Gcd(
            MultivariatePolynomial left, MultivariatePolynomial right, IReadOnlyList<int> variables, int depth)
        {
            if (depth > MultivariatePolynomial.MaxVariables)
                return null;
            if (left.IsZero)
                return right.Normalized();
            if (right.IsZero)
                return left.Normalized();
            if (left.IsConstant || right.IsConstant)
                return MultivariatePolynomial.One(left.VariableCount);

            // A variable occurring in only one of the two cannot occur in a common factor,
            // so the main variable is chosen among those they share. Sharing none, the two
            // lie in polynomial rings meeting only in the constants, and are coprime.
            // Among the shared ones the lowest degree goes first: the remainder sequence is
            // as long as that degree, and every step of it costs a recursion.
            var main = -1;
            var lowest = int.MaxValue;
            foreach (var variable in variables)
            {
                var here = left.DegreeIn(variable);
                var there = right.DegreeIn(variable);
                if (here == 0 || there == 0)
                    continue;
                var degree = Math.Max(here, there);
                if (degree < lowest)
                {
                    lowest = degree;
                    main = variable;
                }
            }
            if (main < 0)
                return MultivariatePolynomial.One(left.VariableCount);

            var rest = new List<int>(variables.Count - 1);
            foreach (var variable in variables)
                if (variable != main)
                    rest.Add(variable);

            if (ContentIn(left, main, rest, depth) is not { } leftContent
                || ContentIn(right, main, rest, depth) is not { } rightContent
                || left.DivideExact(leftContent) is not { } leftPrimitive
                || right.DivideExact(rightContent) is not { } rightPrimitive
                || Gcd(leftContent, rightContent, rest, depth + 1) is not { } contentGcd
                || PrimitiveGcd(leftPrimitive, rightPrimitive, main, rest, depth) is not { } primitiveGcd
                || contentGcd.Multiply(primitiveGcd) is not { } result)
                return null;
            return result.Normalized();
        }

        /// <summary>
        /// The greatest common divisor of the coefficients of <paramref name="poly"/> taken
        /// as a polynomial in <paramref name="main"/> — a polynomial in the remaining
        /// variables.
        /// </summary>
        private static MultivariatePolynomial? ContentIn(
            MultivariatePolynomial poly, int main, IReadOnlyList<int> rest, int depth)
        {
            MultivariatePolynomial? content = null;
            foreach (var coefficient in poly.CoefficientsIn(main).Values)
            {
                content = content is null ? coefficient.Normalized() : Gcd(content, coefficient, rest, depth + 1);
                if (content is null)
                    return null;
                if (content.IsConstant)
                    return MultivariatePolynomial.One(poly.VariableCount);
            }
            return content ?? MultivariatePolynomial.One(poly.VariableCount);
        }

        private static MultivariatePolynomial? PrimitivePartIn(
            MultivariatePolynomial poly, int main, IReadOnlyList<int> rest, int depth)
            => ContentIn(poly, main, rest, depth) is { } content && poly.DivideExact(content) is { } primitive
                ? primitive.Normalized()
                : null;

        /// <summary>
        /// The subresultant polynomial remainder sequence of two polynomials already free
        /// of content in <paramref name="main"/>. Its last nonzero member is the greatest
        /// common divisor up to a factor free of <paramref name="main"/>, which the
        /// primitive part then removes.
        /// </summary>
        private static MultivariatePolynomial? PrimitiveGcd(
            MultivariatePolynomial left, MultivariatePolynomial right, int main, IReadOnlyList<int> rest, int depth)
        {
            if (left.DegreeIn(main) < right.DegreeIn(main))
                (left, right) = (right, left);
            var one = MultivariatePolynomial.One(left.VariableCount);
            var previousLead = one;
            var scale = one;
            for (var step = 0; step < MaxSteps; step++)
            {
                MultithreadingFunctional.ExitIfCancelled();
                var delta = left.DegreeIn(main) - right.DegreeIn(main);
                if (PseudoRemainder(left, right, main) is not { } remainder)
                    return null;
                if (remainder.IsZero)
                    return PrimitivePartIn(right, main, rest, depth);
                // A remainder free of the main variable means the two primitive parts have
                // no factor carrying it, and being primitive they have no other.
                if (remainder.DegreeIn(main) == 0)
                    return MultivariatePolynomial.One(left.VariableCount);

                // The division below is the whole point of the subresultant sequence: what
                // it leaves is a subresultant, so it comes out exact, and the coefficients
                // stay the size of the subresultants instead of compounding.
                if (scale.Power(delta) is not { } scalePower
                    || previousLead.Multiply(scalePower) is not { } factor
                    || remainder.DivideExact(factor) is not { } next)
                    return null;

                left = right;
                right = next;
                previousLead = left.LeadingCoefficientIn(main);
                if (delta == 1)
                    scale = previousLead;
                else if (delta > 1)
                {
                    if (previousLead.Power(delta) is not { } raised
                        || scale.Power(delta - 1) is not { } divisor
                        || raised.DivideExact(divisor) is not { } updated)
                        return null;
                    scale = updated;
                }
            }
            return null;
        }

        /// <summary>
        /// <c>lc(divisor) ^ (deg(dividend) - deg(divisor) + 1) * dividend</c> reduced modulo
        /// <paramref name="divisor"/>, all with respect to <paramref name="main"/>. The
        /// power in front is what makes every division by the leading coefficient come out
        /// whole, so that no denominator ever enters the coefficient ring.
        /// </summary>
        private static MultivariatePolynomial? PseudoRemainder(
            MultivariatePolynomial dividend, MultivariatePolynomial divisor, int main)
        {
            var divisorDegree = divisor.DegreeIn(main);
            var divisorLead = divisor.LeadingCoefficientIn(main);
            var remainder = dividend;
            var outstanding = dividend.DegreeIn(main) - divisorDegree + 1;
            for (var step = 0; step < MaxSteps; step++)
            {
                if (remainder.IsZero || remainder.DegreeIn(main) < divisorDegree)
                    break;
                MultithreadingFunctional.ExitIfCancelled();
                var shift = remainder.DegreeIn(main) - divisorDegree;
                if (divisorLead.Multiply(remainder) is not { } scaled
                    || remainder.LeadingCoefficientIn(main).Multiply(divisor) is not { } cancelling
                    || cancelling.ShiftedBy(main, shift) is not { } shifted)
                    return null;
                remainder = scaled.Subtract(shifted);
                outstanding--;
            }
            if (remainder.IsZero)
                return remainder;
            for (var i = 0; i < outstanding; i++)
            {
                if (divisorLead.Multiply(remainder) is not { } scaled)
                    return null;
                remainder = scaled;
            }
            return remainder;
        }
    }
}
