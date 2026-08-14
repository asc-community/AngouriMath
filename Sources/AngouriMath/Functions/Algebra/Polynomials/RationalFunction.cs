//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using PeterO.Numbers;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Functions
{
    /// <summary>
    /// A canonical form for rational functions over <c>Q</c>: two expressions denoting the
    /// same quotient of polynomials become the identical tree, so that deciding whether they
    /// are equal is a structural comparison rather than a search.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the part of the language where a canonical form is <i>possible</i>. There is
    /// none for the whole of it — zero-equivalence is undecidable once <c>pi</c>, the
    /// exponential, the trigonometric functions and <c>abs</c> are in play (Richardson, 1968)
    /// — so the boundary is in the signature: a refusal means "not a rational function over
    /// <c>Q</c>, and no canonical form is claimed", never a normalisation that resembles one.
    /// <c>Docs/Contributing/CanonicalForm.md</c> is the specification.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/934">#934</a>.
    /// </para>
    /// <para>
    /// Four steps, and only the first is new. The expression is gathered into a single
    /// quotient — nothing else in the library does that, and without it <c>1/x + 1/y</c> and
    /// <c>(x + y)/(x*y)</c> could never meet. Then the numerator and denominator are divided
    /// by their multivariate greatest common divisor, which
    /// <see cref="PolynomialGcd"/> computes and verifies. Then both are scaled so that the
    /// denominator's leading coefficient is one, which is what makes <c>2x/(4y)</c> and
    /// <c>x/(2y)</c> the same tree. Coefficients are in lowest terms throughout.
    /// </para>
    /// <para>
    /// <b>The domain is preserved rather than assumed away.</b> Cancelling a common factor
    /// widens the domain — <c>x/x</c> is not <c>1</c> — so where a factor of positive degree
    /// is cancelled the answer carries the condition that it is nonzero, which is what the
    /// library already does elsewhere and what keeps "equal trees means equal expressions"
    /// true rather than nearly true. Gathering over a common denominator does not widen
    /// anything: a sum is defined exactly where its terms are, and the product of the
    /// denominators vanishes exactly where one of them does.
    /// </para>
    /// </remarks>
    internal static class RationalFunction
    {
        /// <summary>
        /// A quotient larger than this is left alone. On node count, and a refusal rather
        /// than an approximation.
        /// </summary>
        private const int MaxComplexity = 256;

        /// <summary>
        /// Raising to a power is where a gathered quotient explodes, so the exponent is
        /// bounded before <see cref="MultivariatePolynomial.Power"/> is asked; that has its
        /// own bound on the number of terms, which catches the rest.
        /// </summary>
        private const int MaxExponent = 32;

        /// <summary>
        /// <paramref name="expr"/> as a canonical quotient of polynomials over <c>Q</c>, or
        /// <see langword="false"/> where it is not a rational function over <c>Q</c> in its
        /// free variables, or where a bound is reached.
        /// </summary>
        internal static bool TryCanonicalise(Entity expr, [NotNullWhen(true)] out Entity? canonical)
        {
            canonical = null;
            if (expr.Complexity > MaxComplexity)
                return false;

            var variables = expr.Vars
                .OrderBy(variable => variable.Name, StringComparer.Ordinal)
                .ToArray();
            if (variables.Length > MultivariatePolynomial.MaxVariables)
                return false;
            var indices = new Dictionary<Variable, int>(variables.Length);
            for (var i = 0; i < variables.Length; i++)
                indices[variables[i]] = i;
            var variableCount = variables.Length;

            if (!TryGather(expr, indices, variableCount, out var numerator, out var denominator))
                return false;
            // A vanishing denominator is not a rational function, and a vanishing numerator
            // is zero however it was written.
            if (denominator.IsZero)
                return false;
            if (numerator.IsZero)
            {
                canonical = Integer.Create(0);
                return true;
            }

            var order = new int[variableCount];
            for (var i = 0; i < order.Length; i++)
                order[i] = i;

            var cancelled = MultivariatePolynomial.One(variableCount);
            if (variableCount > 0
                && PolynomialGcd.Gcd(numerator, denominator, order, 0) is { } divisor
                && !divisor.IsConstant)
            {
                if (numerator.DivideExact(divisor) is not { } reducedTop
                    || denominator.DivideExact(divisor) is not { } reducedBottom)
                    return false;
                // Multiplied back independently of the division that produced them, as
                // PolynomialGcd does for the same reason: an incomplete cancellation is a
                // tolerable answer and a wrong one is not.
                if (reducedTop.Multiply(divisor) is not { } checkedTop || !checkedTop.SameAs(numerator)
                    || reducedBottom.Multiply(divisor) is not { } checkedBottom
                    || !checkedBottom.SameAs(denominator))
                    return false;
                numerator = reducedTop;
                denominator = reducedBottom;
                cancelled = divisor;
            }

            // Scaled so the denominator leads with one, under the same lexicographic monomial
            // order the Gröbner solver uses. Without this, 2x/(4y) and x/(2y) are different
            // trees for one function: their greatest common divisor is fixed only up to a
            // unit, and nothing obliges the machinery to take the 2 out.
            var leading = denominator.LeadingCoefficient(MonomialOrder.Lexicographic);
            if (leading.IsZero)
                return false;
            if (leading.CompareTo(ERational.One) != 0)
            {
                var inverse = ERational.One.Divide(leading).ToLowestTerms();
                numerator = numerator.ScaleBy(inverse);
                denominator = denominator.ScaleBy(inverse);
            }

            // The denominator is now monic, so a constant one is exactly 1 and is dropped.
            var quotient = denominator.IsConstant
                ? numerator.ToEntity(variables)
                : numerator.ToEntity(variables) / denominator.ToEntity(variables);

            canonical = cancelled.IsConstant
                ? quotient
                : new Providedf(quotient, !cancelled.ToEntity(variables).EqualTo(0));
            return true;
        }

        /// <summary>
        /// <paramref name="expr"/> as a single quotient of polynomials, gathering a sum of
        /// quotients over a common denominator. The denominator is never zero and never
        /// simplified away; it is <c>1</c> for a polynomial.
        /// </summary>
        /// <remarks>
        /// The common denominator is the product rather than the least common multiple. Both
        /// are correct and the product is cheaper to build; what it costs is a larger
        /// intermediate, which the greatest common divisor then removes — so the answer is the
        /// same and only the work in between differs.
        /// </remarks>
        private static bool TryGather(
            Entity expr, IReadOnlyDictionary<Variable, int> indices, int variableCount,
            [NotNullWhen(true)] out MultivariatePolynomial? numerator,
            [NotNullWhen(true)] out MultivariatePolynomial? denominator)
        {
            numerator = denominator = null;

            // A polynomial is its own numerator, and this is the common case, so it is tried
            // before the expression is taken apart.
            if (MultivariatePolynomial.TryParse(expr, indices) is { } whole)
            {
                numerator = whole;
                denominator = MultivariatePolynomial.One(variableCount);
                return true;
            }

            switch (expr)
            {
                case Sumf(var left, var right):
                    return TryCombine(left, right, subtract: false, indices, variableCount,
                        out numerator, out denominator);

                case Minusf(var left, var right):
                    return TryCombine(left, right, subtract: true, indices, variableCount,
                        out numerator, out denominator);

                case Mulf(var left, var right):
                {
                    if (!TryGather(left, indices, variableCount, out var leftTop, out var leftBottom)
                        || !TryGather(right, indices, variableCount, out var rightTop, out var rightBottom))
                        return false;
                    if (leftTop.Multiply(rightTop) is not { } top
                        || leftBottom.Multiply(rightBottom) is not { } bottom)
                        return false;
                    numerator = top;
                    denominator = bottom;
                    return true;
                }

                case Divf(var left, var right):
                {
                    if (!TryGather(left, indices, variableCount, out var leftTop, out var leftBottom)
                        || !TryGather(right, indices, variableCount, out var rightTop, out var rightBottom))
                        return false;
                    // Dividing by a quotient that is identically zero is not a rational
                    // function, and inverting it here would quietly produce one.
                    if (rightTop.IsZero)
                        return false;
                    if (leftTop.Multiply(rightBottom) is not { } top
                        || leftBottom.Multiply(rightTop) is not { } bottom)
                        return false;
                    numerator = top;
                    denominator = bottom;
                    return true;
                }

                case Powf(var @base, Integer power):
                {
                    var exponent = power.EInteger;
                    if (exponent.Abs().CompareTo(EInteger.FromInt32(MaxExponent)) > 0)
                        return false;
                    if (!TryGather(@base, indices, variableCount, out var baseTop, out var baseBottom))
                        return false;
                    var magnitude = exponent.Abs().ToInt32Checked();
                    var negative = exponent.Sign < 0;
                    if (negative && baseTop.IsZero)
                        return false;
                    var top = negative ? baseBottom : baseTop;
                    var bottom = negative ? baseTop : baseBottom;
                    if (top.Power(magnitude) is not { } raisedTop
                        || bottom.Power(magnitude) is not { } raisedBottom)
                        return false;
                    numerator = raisedTop;
                    denominator = raisedBottom;
                    return true;
                }

                default:
                    return false;
            }
        }

        /// <summary>
        /// A sum or a difference of two quotients, over the product of their denominators.
        /// </summary>
        private static bool TryCombine(
            Entity left, Entity right, bool subtract,
            IReadOnlyDictionary<Variable, int> indices, int variableCount,
            [NotNullWhen(true)] out MultivariatePolynomial? numerator,
            [NotNullWhen(true)] out MultivariatePolynomial? denominator)
        {
            numerator = denominator = null;
            if (!TryGather(left, indices, variableCount, out var leftTop, out var leftBottom)
                || !TryGather(right, indices, variableCount, out var rightTop, out var rightBottom))
                return false;
            if (leftTop.Multiply(rightBottom) is not { } crossLeft
                || rightTop.Multiply(leftBottom) is not { } crossRight
                || leftBottom.Multiply(rightBottom) is not { } bottom)
                return false;
            numerator = subtract ? crossLeft.Subtract(crossRight) : crossLeft.Add(crossRight);
            denominator = bottom;
            return true;
        }
    }
}
