//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Collections.Generic;
using System.Linq;
using AngouriMath.Functions;
using PeterO.Numbers;

namespace AngouriMath
{
    using static Entity;
    using static Entity.Number;

    partial class MathS
    {
        /// <summary>
        /// The polynomial layer: factorisation over the rationals, multivariate greatest
        /// common divisors, resultants and discriminants.
        /// </summary>
        /// <remarks>
        /// <para>
        /// These are the operations <see cref="Entity.Simplify(int)"/>,
        /// <see cref="Entity.Solve(Variable)"/> and <see cref="Entity.Integrate(Variable)"/>
        /// already run internally, offered here directly. A caller who wants the factors, the
        /// common divisor or the eliminant — rather than whatever a simplification decides to
        /// do with them — had no way to ask for one, and no way to find out that the request
        /// was outside what the layer can do.
        /// </para>
        /// <para>
        /// <b>Every one of these answers <see langword="null"/> rather than guessing.</b>
        /// <see langword="null"/> means "I could not settle this" — the input is not a
        /// polynomial of the shape the operation needs, or it is one but past a bound the
        /// implementation carries. It never means "the answer is the input": a polynomial
        /// that does not factor comes back as itself from <see cref="Factor"/>, which is a
        /// statement that it is irreducible over the rationals, and is a different thing from
        /// a refusal. The bounds are stated on each member.
        /// </para>
        /// <para>
        /// Part of the polynomial layer of
        /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a>, item 43.
        /// </para>
        /// </remarks>
        public static class Polynomials
        {
            /// <summary>
            /// <paramref name="expr"/> written as a product of powers of polynomials
            /// irreducible over the rationals, or <see langword="null"/> where that could not
            /// be settled.
            /// </summary>
            /// <param name="expr">The polynomial to factorise.</param>
            /// <param name="variable">The variable it is a polynomial in.</param>
            /// <returns>
            /// The factorisation; the input itself where it is irreducible over the
            /// rationals, which is an answer and not a refusal; or <see langword="null"/>
            /// where <paramref name="expr"/> is not a polynomial in
            /// <paramref name="variable"/> alone with rational coefficients, where its degree
            /// is above 32, or where the factoriser declined.
            /// </returns>
            /// <remarks>
            /// <para>
            /// Zassenhaus: square-free decomposition, Berlekamp's factorisation modulo a
            /// prime, Hensel lifting to a power of it above Mignotte's bound, and
            /// recombination — and the factors are multiplied back and compared with the
            /// input before they are returned.
            /// </para>
            /// <para>
            /// <b>Univariate only.</b> A polynomial in more than one variable is refused
            /// rather than answered: <c>Factor("x * y + y", "x")</c> is <see langword="null"/>
            /// and not <c>x * y + y</c>, because handing back the input would say that
            /// <c>y * (x + 1)</c> does not exist.
            /// </para>
            /// </remarks>
            /// <example>
            /// <code>
            /// Console.WriteLine(MathS.Polynomials.Factor("x ^ 4 - 5 * x ^ 2 + 4", "x"));
            /// // (x + 1) * (x + 2) * (x - 2) * (x - 1)
            ///
            /// Console.WriteLine(MathS.Polynomials.Factor("x ^ 3 - 3 * x ^ 2 + 3 * x - 1", "x"));
            /// // (x - 1) ^ 3
            ///
            /// Console.WriteLine(MathS.Polynomials.Factor("x ^ 2 + 1", "x"));
            /// // x ^ 2 + 1        -- irreducible over Q, which is an answer
            ///
            /// Console.WriteLine(MathS.Polynomials.Factor("x * y + y", "x") is null);
            /// // True             -- more than one variable, so it declines
            /// </code>
            /// </example>
            public static Entity? Factor(Entity expr, Variable variable)
                => Assemble(PolynomialFactorization.FactorComplete(expr, variable), variable)
                   ?? FactorAfterTakingOutTheContent(expr, variable);

            /// <summary>
            /// A factorisation as an expression, or <see langword="null"/> where there was none.
            /// </summary>
            private static Entity? Assemble(
                PolynomialFactorization.Factorization? factorization, Variable variable)
            {
                if (factorization is not { } settled)
                    return null;
                Entity? product = null;
                foreach (var part in settled.Parts)
                {
                    var piece = part.Factor.ToEntity(variable);
                    if (part.Multiplicity > 1)
                        piece = piece.Pow(part.Multiplicity);
                    product = product is null ? piece : product * piece;
                }
                if (product is null)
                    return null;
                return settled.Constant.CompareTo(ERational.One) == 0
                    ? product
                    : Rational.Create(settled.Constant) * product;
            }

            /// <summary>
            /// The factorisation of a polynomial whose coefficients in <paramref name="variable"/>
            /// are themselves polynomials, where taking out their common divisor leaves something
            /// this can factor — or <see langword="null"/> where it does not.
            /// </summary>
            /// <remarks>
            /// <para>
            /// <see cref="PolynomialFactorization"/> works over ℚ, so a coefficient that is not a
            /// rational number stops it before it starts and every polynomial in more than one
            /// variable was refused. Some of them do not need factorising over a bigger ring at
            /// all: <c>x * y + y</c> is <c>y</c> times something univariate, and only the <c>y</c>
            /// was in the way.
            /// </para>
            /// <para>
            /// So the content in <paramref name="variable"/> — the greatest common divisor of the
            /// coefficients, which is a polynomial in the other variables — is taken out first,
            /// using the same multivariate machinery <see cref="Gcd"/> is built from, and what
            /// remains goes down the ordinary path. Where the content is a constant this has
            /// nothing to offer and says so, which is the honest answer for
            /// <c>x ^ 2 - y ^ 2</c>: that one genuinely needs factorisation over ℚ(y) and is not
            /// what this does.
            /// </para>
            /// </remarks>
            private static Entity? FactorAfterTakingOutTheContent(Entity expr, Variable variable)
            {
                if (Index(expr, expr, variable) is not var (variables, index))
                    return null;
                if (variables.Count < 2)
                    return null;
                if (MultivariatePolynomial.TryParse(expr, index) is not { } poly)
                    return null;
                var main = index[variable];
                var others = new List<int>(variables.Count - 1);
                for (var i = 0; i < variables.Count; i++)
                    if (i != main)
                        others.Add(i);
                if (PolynomialGcd.ContentIn(poly, main, others, 0) is not { } content
                    || content.IsConstant)
                    return null;
                if (poly.DivideExact(content) is not { } primitive)
                    return null;
                var rest = Assemble(
                    PolynomialFactorization.FactorComplete(primitive.ToEntity(variables), variable),
                    variable);
                return rest is null ? null : content.ToEntity(variables) * rest;
            }

            /// <summary>
            /// The greatest common divisor of two polynomials, or <see langword="null"/> where
            /// that could not be settled.
            /// </summary>
            /// <param name="left">The first polynomial.</param>
            /// <param name="right">The second polynomial.</param>
            /// <returns>
            /// A polynomial dividing both, of the greatest degree that does — <c>1</c> where
            /// they are coprime — or <see langword="null"/> where either is not a polynomial
            /// with rational coefficients in at most eight variables of degree at most 127,
            /// or where the computation declined.
            /// </returns>
            /// <remarks>
            /// Multivariate, by recursion on the variables with the content taken out at each
            /// level and a subresultant remainder sequence at the bottom, so that the
            /// coefficients stay the size of the minors they are rather than compounding. The
            /// result is normalised so that its leading coefficient is positive, which is what
            /// makes two greatest common divisors of the same pair comparable; it is fixed only
            /// up to a rational factor otherwise.
            /// </remarks>
            /// <example>
            /// <code>
            /// Console.WriteLine(MathS.Polynomials.Gcd("x ^ 2 - 1", "x ^ 2 + 2 * x + 1"));
            /// // x + 1
            ///
            /// Console.WriteLine(MathS.Polynomials.Gcd("x ^ 2 - y ^ 2", "x ^ 2 - 2 * x * y + y ^ 2"));
            /// // x - y
            ///
            /// Console.WriteLine(MathS.Polynomials.Gcd("x ^ 2 + 1", "x ^ 2 + 2"));
            /// // 1                -- coprime
            /// </code>
            /// </example>
            public static Entity? Gcd(Entity left, Entity right)
            {
                if (Index(left, right) is not var (variables, index))
                    return null;
                if (MultivariatePolynomial.TryParse(left, index) is not { } first
                    || MultivariatePolynomial.TryParse(right, index) is not { } second)
                    return null;
                var order = new int[variables.Count];
                for (var i = 0; i < order.Length; i++)
                    order[i] = i;
                if (PolynomialGcd.Gcd(first, second, order, 0) is not { } divisor)
                    return null;
                return divisor.ToEntity(variables);
            }

            /// <summary>
            /// The resultant of two polynomials with respect to one variable — the condition
            /// on the others under which the two have a common root in it — or
            /// <see langword="null"/> where that could not be settled.
            /// </summary>
            /// <param name="left">The first polynomial.</param>
            /// <param name="right">The second polynomial.</param>
            /// <param name="eliminate">The variable to eliminate between them.</param>
            /// <returns>
            /// A polynomial in the remaining variables, vanishing exactly where the two have a
            /// common <paramref name="eliminate"/> — or where both of their leading
            /// coefficients in it vanish, which is why an eliminant can carry a root the
            /// original pair does not have. <see langword="null"/> where either argument is
            /// not a polynomial with rational coefficients in at most eight variables of
            /// degree at most 127, where the sum of the two degrees in
            /// <paramref name="eliminate"/> is above 40, or where the elimination ran past its
            /// budget.
            /// </returns>
            /// <remarks>
            /// The determinant of the Sylvester matrix, computed as one by fraction-free
            /// elimination. The remainder-sequence formulations are faster and each carries a
            /// sign convention that is easy to get wrong and gives a plausible-looking wrong
            /// answer when it is; taken as a determinant the convention falls out of the
            /// matrix instead of being imposed on it.
            /// </remarks>
            /// <example>
            /// <code>
            /// // Eliminating y between a circle and a line leaves the condition on x.
            /// Console.WriteLine(MathS.Polynomials.Resultant("x ^ 2 + y ^ 2 - 1", "x + y - 1", "y"));
            /// // 2 * x ^ 2 - 2 * x
            ///
            /// // Two polynomials share a root exactly when their resultant vanishes.
            /// Console.WriteLine(MathS.Polynomials.Resultant("x ^ 2 - 1", "x - a", "x"));
            /// // a ^ 2 - 1
            /// </code>
            /// </example>
            public static Entity? Resultant(Entity left, Entity right, Variable eliminate)
            {
                if (Index(left, right, eliminate) is not var (variables, index))
                    return null;
                if (MultivariatePolynomial.TryParse(left, index) is not { } first
                    || MultivariatePolynomial.TryParse(right, index) is not { } second)
                    return null;
                var main = index[eliminate];
                var others = new List<int>(variables.Count - 1);
                for (var i = 0; i < variables.Count; i++)
                    if (i != main)
                        others.Add(i);
                if (PolynomialResultant.Resultant(first, second, main, others) is not { } resultant)
                    return null;
                return resultant.ToEntity(variables);
            }

            /// <summary>
            /// The discriminant of a polynomial with respect to one variable — vanishing
            /// exactly where it has a repeated root in that variable — or
            /// <see langword="null"/> where that could not be settled.
            /// </summary>
            /// <param name="expr">The polynomial.</param>
            /// <param name="variable">The variable to take the discriminant in.</param>
            /// <returns>
            /// A polynomial in the remaining variables, or <see langword="null"/> under the
            /// same conditions as <see cref="Resultant"/> — this is
            /// <c>(-1)^(n(n-1)/2) Res(f, f') / lc(f)</c>, so it declines wherever that
            /// resultant does.
            /// </returns>
            /// <remarks>
            /// The sign convention is the usual one, so that a quadratic gives
            /// <c>b^2 - 4ac</c> rather than its negative, and a cubic <c>x^3 + px + q</c>
            /// gives <c>-4p^3 - 27q^2</c>. For a polynomial with real coefficients the sign
            /// counts real roots up to degree three: a quadratic has two where it is positive
            /// and none where it is negative, and a cubic three and one respectively. It stops
            /// deciding at degree four.
            /// </remarks>
            /// <example>
            /// <code>
            /// Console.WriteLine(MathS.Polynomials.Discriminant("a * x ^ 2 + b * x + c", "x"));
            /// // -4 * a * c + b ^ 2
            ///
            /// Console.WriteLine(MathS.Polynomials.Discriminant("x ^ 3 - 3 * x + 1", "x"));
            /// // 81               -- positive, so all three roots are real
            ///
            /// Console.WriteLine(MathS.Polynomials.Discriminant("x ^ 2 - 2 * x + 1", "x"));
            /// // 0                -- a repeated root
            /// </code>
            /// </example>
            public static Entity? Discriminant(Entity expr, Variable variable)
            {
                if (Index(expr, expr, variable) is not var (variables, index))
                    return null;
                if (MultivariatePolynomial.TryParse(expr, index) is not { } poly)
                    return null;
                var main = index[variable];
                var others = new List<int>(variables.Count - 1);
                for (var i = 0; i < variables.Count; i++)
                    if (i != main)
                        others.Add(i);
                if (PolynomialResultant.Discriminant(poly, main, others) is not { } discriminant)
                    return null;
                return discriminant.ToEntity(variables);
            }

            /// <summary>
            /// The square-free part of a polynomial — the same polynomial with every repeated
            /// factor reduced to a single one, so that it has the same roots and each of them
            /// once — or <see langword="null"/> where that could not be settled.
            /// </summary>
            /// <param name="expr">The polynomial.</param>
            /// <param name="variable">The variable it is a polynomial in.</param>
            /// <returns>
            /// The polynomial divided by its greatest common divisor with its own derivative,
            /// normalised to a positive leading coefficient and no common factor among its
            /// coefficients; or <see langword="null"/> where <paramref name="expr"/> is not a
            /// polynomial in <paramref name="variable"/> alone with rational coefficients, or
            /// where its degree is above 32.
            /// </returns>
            /// <remarks>
            /// Univariate, and over the rationals. This is what to take before looking for
            /// roots: the multiplicities are the part a root-finder gets wrong, and dividing
            /// them out costs one greatest common divisor.
            /// </remarks>
            /// <example>
            /// <code>
            /// Console.WriteLine(MathS.Polynomials.SquareFreePart("(x - 1) ^ 3 * (x + 2) ^ 2", "x"));
            /// // x ^ 2 + x - 2    -- which is (x - 1)(x + 2)
            ///
            /// Console.WriteLine(MathS.Polynomials.SquareFreePart("x ^ 2 + 1", "x"));
            /// // x ^ 2 + 1        -- already square-free
            /// </code>
            /// </example>
            public static Entity? SquareFreePart(Entity expr, Variable variable)
            {
                if (!PolynomialFactoring.TryGetRationalCoefficients(
                        expr, variable, leastTerms: 1, leastDegree: 1,
                        IntegerPolynomial.MaxDegree, out var rational))
                    return null;
                var denominator = EInteger.One;
                foreach (var coefficient in rational)
                    denominator = coefficient.Denominator
                        .Divide(coefficient.Denominator.Gcd(denominator)).Multiply(denominator);
                var whole = new EInteger[rational.Length];
                for (var i = 0; i < whole.Length; i++)
                    whole[i] = rational[i].Numerator.Multiply(denominator.Divide(rational[i].Denominator));
                var primitive = IntegerPolynomial.Create(whole).PrimitivePart();
                var repeated = IntegerPolynomial.Gcd(primitive, primitive.Derivative());
                if (primitive.DivideExact(repeated) is not { } distinct)
                    return null;
                return distinct.PrimitivePart().ToEntity(variable);
            }

            /// <summary>
            /// The variables of the arguments, in a fixed order, with the position each one
            /// occupies in a packed monomial — or <see langword="null"/> where there are more
            /// of them than the representation has room for.
            /// </summary>
            /// <remarks>
            /// Sorted by name rather than left in the order the expressions happen to mention
            /// them, so that the answer does not depend on which argument came first.
            /// </remarks>
            private static (IReadOnlyList<Variable> Variables, Dictionary<Variable, int> Index)? Index(
                Entity left, Entity right, Variable? required = null)
            {
                var names = new SortedSet<Variable>(Comparer<Variable>.Create(
                    static (a, b) => string.CompareOrdinal(a.Name, b.Name)));
                foreach (var variable in left.Vars)
                    names.Add(variable);
                foreach (var variable in right.Vars)
                    names.Add(variable);
                if (required is { } named)
                    names.Add(named);
                if (names.Count == 0 || names.Count > MultivariatePolynomial.MaxVariables)
                    return null;
                var variables = names.ToList();
                var index = new Dictionary<Variable, int>(variables.Count);
                for (var i = 0; i < variables.Count; i++)
                    index[variables[i]] = i;
                return (variables, index);
            }
        }
    }
}
