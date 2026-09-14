//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//
using System.Collections.Generic;
using System.Linq;
using PeterO.Numbers;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;
using static AngouriMath.Functions.Algebra.CubeRootLogarithmAnsatz;

namespace AngouriMath.Functions.Algebra
{
    /// <summary>
    /// A rational function of <c>x</c> and one square root <c>y = sqrt(P(x))</c> of a cubic or
    /// a quartic, integrated by an ansatz over logarithms of <c>A - B y</c> and arctangents of
    /// <c>A/(B y)</c> for polynomials <c>A</c> and <c>B</c>, and a rational part in <c>y</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The curve <c>y^2 = P(x)</c> is elliptic for a squarefree cubic or quartic <c>P</c>, so no
    /// substitution makes these rational; the ones that are elementary at all -- Welz's
    /// <c>(1 + x)/((x - 2) sqrt(1 + x^3))</c>, which is <c>-(2/3) atanh((1 + x)^2/(3 sqrt(1 + x^3)))</c>,
    /// Bronstein's <c>x/sqrt(x^4 + 10x^2 - 96x - 71)</c> -- are integrated by logarithms of
    /// <c>A - B y</c>. The norm <c>(A - By)(A + By) = A^2 - B^2 P</c> is where such a logarithm
    /// has its poles, so the <c>A</c> and <c>B</c> that can occur are the ones for which
    /// <c>A - By</c> vanishes at the places the integrand has poles: at a rational pole
    /// <c>a</c> with <c>P(a)</c> a square, <c>A/B</c> agrees with a branch of <c>y</c> to some
    /// order there, which is a Padé approximant of the branch's series -- Welz's
    /// <c>(1 + x)^2/3</c> is the second-order Taylor polynomial of <c>sqrt(1 + x^3)</c> at
    /// <c>2</c>, where the norm <c>(1 + x)(2 - x)^3</c> has its triple root -- and at infinity
    /// likewise for a quartic with a square leading coefficient, which is Bronstein's. Where
    /// <c>P(a)</c> is the negative of a square the branch is imaginary, the conjugate pair of
    /// logarithms is one arctangent of <c>A/(B y)</c>, and the norm is <c>A^2 + B^2 P</c>.
    /// Each Padé approximant at each place, for a few orders, is a candidate; the identity
    /// <c>sum c_k g_k' = f</c> is one in <c>Q(x)[y]/(y^2 - P)</c>, brought over one denominator
    /// and matched coefficient by coefficient, and solved exactly over the rationals.
    /// </para>
    /// <para>
    /// The rationals only: a pole where <c>P</c> is neither a square nor the negative of one
    /// wants the field of its square root, and is left for now. The answer is checked against
    /// the integrand at sampled points before it is returned.
    /// https://github.com/asc-community/AngouriMath/issues/718
    /// </para>
    /// </remarks>
    internal static class SquareRootLogarithmAnsatz
    {
        /// <summary>The largest degree of the polynomial under the root.</summary>
        private const int MaxRadicandDegree = 4;

        /// <summary>The largest degree of the denominator in <c>x</c>.</summary>
        private const int MaxDenominatorDegree = 6;

        /// <summary>The largest degree of <c>A</c>, and of <c>B</c>, in a candidate.</summary>
        private const int MaxNumeratorDegree = 4;
        private const int MaxMultiplierDegree = 2;

        /// <summary>
        /// The largest degree of <c>B</c> in a candidate at infinity, where <c>A</c> is two more:
        /// Bronstein's <c>x/sqrt(x^4 + 10x^2 - 96x - 71)</c> wants <c>B</c> of degree six.
        /// </summary>
        private const int MaxMultiplierDegreeAtInfinity = 6;

        internal static Entity? Integrate(Entity expr, Variable x)
        {
            // One square root of one polynomial, and otherwise rational in x.
            Entity? radicand = null;
            foreach (var node in expr.Nodes)
                if (node is Powf(var @base, Rational exponent) && @base.ContainsNode(x)
                    && exponent.ERational.Denominator.Equals(EInteger.FromInt32(2)))
                {
                    if (radicand is null)
                        radicand = @base;
                    else if (radicand != @base)
                        return null;
                }
                else if (node is Powf(var otherBase, var otherExponent) && otherBase.ContainsNode(x) && otherExponent is not Integer)
                    return null;
            if (radicand is null || expr.Vars.Any(v => v != x))
                return null;
            var c = Variable.CreateUnique(expr, "c");
            var field = new Field(c);
            var p = XPoly.Read(radicand, x, field);
            if (p is null || p.Degree < 3 || p.Degree > MaxRadicandDegree)
                return null;
            if (p.Coefficients.Values.Any(coefficient => coefficient.Evaled is not Rational))
                return null;

            var y = Variable.CreateUnique(expr, "y");
            var inY = expr.Replace(node => node is Powf(var b, Rational e) && b == radicand && e.ERational.Denominator.Equals(EInteger.FromInt32(2))
                ? MathS.Pow(y, Integer.Create(e.ERational.Numerator))
                : node);
            if (!TryReadOverThePolynomialDenominator(inY, x, y, radicand, 2, out var numerator, out var denominatorAsWritten))
                return null;
            var e = XPoly.Read(denominatorAsWritten, x, field);
            if (e is null || e.Degree > MaxDenominatorDegree)
                return null;
            var target = Pair.Read(numerator, x, y, p);
            if (target is null)
                return null;
            var pPrime = p.Derivative();
            var pEntity = p.ToEntity(x);

            // The places: rational poles with P a square or the negative of one there, and
            // infinity for a quartic with a square leading coefficient.
            var poles = RationalRoots(denominatorAsWritten, x, out var quadraticFactors);
            var roots = RationalRoots(radicand, x, out _);
            if (poles is null || roots is null)
                return null;
            var candidates = new List<(XPoly A, XPoly B, bool Imaginary)>();
            void Offer(XPoly a, XPoly b, bool imaginary)
            {
                if (a.IsZero || b.IsZero)
                    return;
                if (candidates.Any(pair => pair.Imaginary == imaginary && pair.A.Multiply(b).SameAs(pair.B.Multiply(a))))
                    return;
                candidates.Add((a, b, imaginary));
            }
            foreach (var a in poles)
            {
                if (a.Evaled is not Rational at)
                    continue;
                var value = pEntity.Substitute(x, a).InnerSimplified.Evaled;
                if (value is not Rational v || v.ERational.IsZero)
                    continue;
                var imaginary = v.ERational.Sign < 0;
                if (TryRationalSquareRoot(imaginary ? v.ERational.Negate() : v.ERational) is not { } s)
                    continue;
                // The series of the branch with y(a) = s (or i s), in t = x - a: y^2 = P(a + t).
                var shifted = Shifted(p, at.ERational, field);
                var series = SeriesOfTheRoot(shifted, s, imaginary, 2 * MaxNumeratorDegree + 2);
                if (series is null)
                    continue;
                foreach (var (aPoly, bPoly) in Approximants(series, field))
                    Offer(Unshifted(aPoly, at.ERational, field), Unshifted(bPoly, at.ERational, field), imaginary);
            }
            if (p.Degree == 4 && p[4].Evaled is Rational leading && leading.ERational.Sign > 0
                && TryRationalSquareRoot(leading.ERational) is { } leadingRoot)
            {
                // At infinity, in t = 1/x: y = x^2 sqrt(P(1/t) t^4), and P(1/t) t^4 is the
                // reversed polynomial, whose square root's series in t begins with the root of
                // the leading coefficient.
                var reversed = new Dictionary<int, ERational>();
                for (var k = 0; k <= 4; k++)
                    if (p[k].Evaled is Rational coefficient)
                        reversed[4 - k] = coefficient.ERational;
                var series = SeriesOfTheRoot(reversed, leadingRoot, false, 4 * MaxMultiplierDegreeAtInfinity + 4);
                if (series is not null)
                    foreach (var (aPoly, bPoly) in ApproximantsAtInfinity(series, field))
                        Offer(aPoly, bPoly, false);
            }
            if (candidates.Count == 0)
                return null;

            // The places as factors: the linear ones and the irreducible quadratics of the
            // denominator. A candidate's norm is where its logarithm has its poles, so a
            // candidate whose norm has a root anywhere else is dropped: its logarithm cannot
            // be in the answer, whose poles are the integrand's, and beside the others it
            // only makes the common denominator long.
            var linearFactors = new List<(ERational Root, XPoly Factor)>();
            foreach (var a in poles.Concat(roots).Distinct())
                if (a.Evaled is Rational root && !linearFactors.Any(pair => pair.Root.Equals(root.ERational)))
                    linearFactors.Add((root.ERational, XPoly.FromCoefficients(field, new Dictionary<int, Entity> { [0] = Rational.Create(root.ERational.Negate()), [1] = Integer.One })));
            var quadratics = new List<XPoly>();
            foreach (var quadratic in quadraticFactors)
                if (XPoly.Read(quadratic, x, field) is { } q)
                    quadratics.Add(q);
            if (FactorOverThePlaces(e, linearFactors, quadratics, field) is not (var eFactors, var eScale, var eLeftover))
                return null;
            if (eLeftover is not null)
                eFactors.Add(eLeftover);

            // The generators.
            var generators = new List<Generator>();
            foreach (var (root, linear) in linearFactors)
                generators.Add(new Generator(Pair.Constant(XPoly.One(field)).AsYPoly(p), new List<XPoly> { linear }, 1,
                    MathS.Ln(root.Sign < 0 ? x + Rational.Create(root.Negate()) : x - Rational.Create(root))));
            foreach (var q in quadratics)
                generators.Add(new Generator(Pair.Constant(q.Derivative()).AsYPoly(p), new List<XPoly> { q }, 1, MathS.Ln(q.ToEntity(x))));
            var pp = Pair.Constant(p);
            var ppPrime = Pair.Constant(pPrime);
            var yOnly = Pair.Y(field);
            foreach (var (aPoly, bPoly, imaginary) in candidates)
            {
                var a = Pair.Constant(aPoly);
                var b = Pair.Constant(bPoly);
                var aPrime = Pair.Constant(aPoly.Derivative());
                var bPrime = Pair.Constant(bPoly.Derivative());
                var aEntity = aPoly.ToEntity(x);
                var bEntity = bPoly.ToEntity(x);
                var normOfB = bPoly.Multiply(bPoly).Multiply(p);
                var norm = imaginary ? aPoly.Multiply(aPoly).Add(normOfB) : aPoly.Multiply(aPoly).Subtract(normOfB);
                if (norm.IsZero || FactorOverThePlaces(norm, linearFactors, quadratics, field) is not (var factors, var scale, null))
                    continue;
                var denominator = new List<XPoly> { p };
                denominator.AddRange(factors);
                if (!imaginary)
                {
                    // (ln(A - By))' = (A' - B'y - B P'/(2y))/(A - By); times (A + By)/(A + By)
                    // and 2P/(2P): [(2P A' - (2P B' + B P') y)(A + By)]/(2P (A^2 - B^2 P)).
                    var top = pp.Multiply(aPrime, p).Scale(2)
                        .Subtract(pp.Multiply(bPrime, p).Scale(2).Add(b.Multiply(ppPrime, p)).Multiply(yOnly, p))
                        .Multiply(a.Add(b.Multiply(yOnly, p)), p);
                    generators.Add(new Generator(top.Scale(Rational.Create(ERational.One.Divide(scale))).AsYPoly(p), denominator, 2, MathS.Ln(aEntity - bEntity * y)));
                }
                else
                {
                    // (arctan(A/(By)))' = (A' B y - A B' y - A B P'/(2y))/(A^2 + B^2 P);
                    // times 2P/(2P): y (2P (A' B - A B') - A B P')/(2P (A^2 + B^2 P)).
                    var top = pp.Multiply(aPrime.Multiply(b, p).Subtract(a.Multiply(bPrime, p)), p).Scale(2)
                        .Subtract(a.Multiply(b, p).Multiply(ppPrime, p))
                        .Multiply(yOnly, p);
                    generators.Add(new Generator(top.Scale(Rational.Create(ERational.One.Divide(scale))).AsYPoly(p), denominator, 2, MathS.Arctan(aEntity / (bEntity * y))));
                }
            }
            // The rational part: y x^k / E.
            var ePrime = e.Derivative();
            var eEntity = e.ToEntity(x);
            var firstRational = generators.Count;
            var eSquared = new List<XPoly> { p };
            eSquared.AddRange(eFactors);
            eSquared.AddRange(eFactors);
            for (var k = 0; k <= e.Degree + 2; k++)
            {
                var xk = XPoly.Power(field, k);
                // (y x^k / E)' = y [P' x^k E + 2P (k x^(k-1) E - x^k E')] / (2 P E^2)
                var inside = pPrime.Multiply(xk).Multiply(e)
                    .Add(p.Multiply(xk.Derivative().Multiply(e).Subtract(xk.Multiply(ePrime))).Scale(2));
                generators.Add(new Generator(Pair.Constant(inside).Multiply(yOnly, p).Scale(Rational.Create(ERational.One.Divide(eScale.Multiply(eScale)))).AsYPoly(p), eSquared, 2, y * MathS.Pow(x, k) / eEntity));
            }

            // Everything over one denominator.
            var common = new List<(XPoly Factor, int Power)>();
            void Take(List<XPoly> factors)
            {
                foreach (var factor in factors.Distinct())
                {
                    var count = factors.Count(f => ReferenceEquals(f, factor) || f.SameAs(factor));
                    var index = common.FindIndex(pair => pair.Factor.SameAs(factor));
                    if (index < 0)
                        common.Add((factor, count));
                    else if (common[index].Power < count)
                        common[index] = (common[index].Factor, count);
                }
            }
            foreach (var generator in generators)
                Take(generator.Denominator);
            Take(eFactors);
            List<XPoly> Multiplier(List<XPoly> own)
            {
                var factors = new List<XPoly>();
                foreach (var (factor, power) in common)
                {
                    var has = own.Count(f => f.SameAs(factor));
                    for (var i = has; i < power; i++)
                        factors.Add(factor);
                }
                return factors;
            }
            var columns = new List<Dictionary<(int, int), Entity>>();
            foreach (var generator in generators)
            {
                var scaled = Pair.FromYPoly(generator.Numerator, p);
                foreach (var factor in Multiplier(generator.Denominator))
                    scaled = scaled.Multiply(Pair.Constant(factor), p);
                var column = new Dictionary<(int, int), Entity>();
                for (var j = 0; j < 2; j++)
                    foreach (var pair in scaled[j].Coefficients)
                        column[(j, pair.Key)] = field.Normalize(pair.Value / generator.Constant);
                columns.Add(column);
            }
            var targetColumn = new Dictionary<(int, int), Entity>();
            {
                var scaled = target.Scale(Rational.Create(ERational.One.Divide(eScale)));
                foreach (var factor in Multiplier(eFactors))
                    scaled = scaled.Multiply(Pair.Constant(factor), p);
                for (var j = 0; j < 2; j++)
                    foreach (var pair in scaled[j].Coefficients)
                        targetColumn[(j, pair.Key)] = pair.Value;
            }
            var keys = columns.SelectMany(column => column.Keys).Concat(targetColumn.Keys).Distinct().ToList();
            var matrix = new Entity[keys.Count][];
            var rhs = new Entity[keys.Count];
            for (var row = 0; row < keys.Count; row++)
            {
                matrix[row] = new Entity[columns.Count];
                for (var column = 0; column < columns.Count; column++)
                    matrix[row][column] = columns[column].TryGetValue(keys[row], out var entry) ? entry : Integer.Zero;
                rhs[row] = targetColumn.TryGetValue(keys[row], out var wanted) ? wanted : Integer.Zero;
            }
            if (!Functions.PartialFractions.TrySolveLinear(matrix, rhs, out var values) || values is null)
                return null;

            Entity answer = Integer.Zero;
            for (var k = 0; k < firstRational; k++)
                if (values[k] != Integer.Zero && values[k].Evaled is not Complex { IsZero: true })
                    answer += values[k] * generators[k].Term;
            // The rational part as one quotient, cancelled against the factors of E.
            var rationalCoefficients = new Dictionary<int, Entity>();
            for (var k = firstRational; k < generators.Count; k++)
                if (values[k] != Integer.Zero && values[k].Evaled is not Complex { IsZero: true })
                    rationalCoefficients[k - firstRational] = values[k];
            if (rationalCoefficients.Count > 0)
            {
                var rationalPart = XPoly.FromCoefficients(field, rationalCoefficients);
                Entity below = Rational.Create(eScale);
                if (Rationals(rationalPart) is { } remaining)
                {
                    foreach (var factor in eFactors)
                    {
                        if (Rationals(factor) is { } divisor && remaining.Length >= divisor.Length && DivideExact(remaining, divisor) is { } quotient)
                            remaining = quotient;
                        else
                            below *= factor.ToEntity(x);
                    }
                    var cancelled = new Dictionary<int, Entity>();
                    for (var k = 0; k < remaining.Length; k++)
                        if (!remaining[k].IsZero)
                            cancelled[k] = Rational.Create(remaining[k]);
                    rationalPart = XPoly.FromCoefficients(field, cancelled);
                }
                else
                    below = denominatorAsWritten;
                answer += rationalPart.ToEntity(x) * y / below;
            }
            var written = Functions.PartialFractions.Bare(answer.Substitute(y, MathS.Sqrt(radicand)));
            if (written.Nodes.Any(node => node == MathS.NaN))
                return null;
            if (!Functions.PartialFractions.HoldsAtSampledPoints(written.Differentiate(x), expr, x))
                return null;
            return written;
        }

        /// <summary>
        /// <paramref name="poly"/>, with rational coefficients, as its leading coefficient
        /// times the linear and quadratic factors from the lists it is divisible by, each as
        /// often as it divides, and what is left -- null where nothing is; null where a
        /// coefficient is not rational.
        /// </summary>
        private static (List<XPoly> Factors, ERational Scale, XPoly? Leftover)? FactorOverThePlaces(
            XPoly poly, List<(ERational Root, XPoly Factor)> linears, List<XPoly> quadratics, Field field)
        {
            if (Rationals(poly) is not { } current)
                return null;
            var factors = new List<XPoly>();
            foreach (var (root, factor) in linears)
                while (current.Length > 1 && DivideByLinear(current, root) is { } quotient)
                {
                    factors.Add(factor);
                    current = quotient;
                }
            foreach (var quadratic in quadratics)
                if (Rationals(quadratic) is { } divisor)
                    while (current.Length > 2 && DivideExact(current, divisor) is { } quotient)
                    {
                        factors.Add(quadratic);
                        current = quotient;
                    }
            var scale = current[current.Length - 1];
            XPoly? leftover = null;
            if (current.Length > 1)
            {
                var coefficients = new Dictionary<int, Entity>();
                for (var k = 0; k < current.Length; k++)
                    if (!current[k].IsZero)
                        coefficients[k] = Rational.Create(current[k].Divide(scale).ToLowestTerms());
                leftover = XPoly.FromCoefficients(field, coefficients);
            }
            return (factors, scale, leftover);
        }

        /// <summary>The coefficients by power, where every one is rational.</summary>
        private static ERational[]? Rationals(XPoly poly)
        {
            if (poly.IsZero)
                return null;
            var result = new ERational[poly.Degree + 1];
            for (var k = 0; k <= poly.Degree; k++)
            {
                if (poly[k].Evaled is not Rational coefficient)
                    return null;
                result[k] = coefficient.ERational;
            }
            return result;
        }

        /// <summary>The quotient by <c>x - a</c>, where the remainder is zero.</summary>
        private static ERational[]? DivideByLinear(ERational[] coefficients, ERational a)
        {
            var quotient = new ERational[coefficients.Length - 1];
            var carry = ERational.Zero;
            for (var k = coefficients.Length - 1; k >= 1; k--)
            {
                carry = coefficients[k].Add(carry).ToLowestTerms();
                quotient[k - 1] = carry;
                carry = carry.Multiply(a);
            }
            return coefficients[0].Add(carry).ToLowestTerms().IsZero ? quotient : null;
        }

        /// <summary>The exact quotient by <paramref name="divisor"/>, or null.</summary>
        private static ERational[]? DivideExact(ERational[] coefficients, ERational[] divisor)
        {
            var leading = divisor[divisor.Length - 1];
            if (coefficients.Length < divisor.Length || leading.IsZero)
                return null;
            var remainder = (ERational[])coefficients.Clone();
            var quotient = new ERational[coefficients.Length - divisor.Length + 1];
            for (var k = quotient.Length - 1; k >= 0; k--)
            {
                var term = remainder[k + divisor.Length - 1].Divide(leading).ToLowestTerms();
                quotient[k] = term;
                for (var i = 0; i < divisor.Length; i++)
                    remainder[k + i] = remainder[k + i].Subtract(term.Multiply(divisor[i])).ToLowestTerms();
            }
            for (var i = 0; i < divisor.Length - 1; i++)
                if (!remainder[i].IsZero)
                    return null;
            return quotient;
        }

        /// <summary>The rational square root of a non-negative rational, where it has one.</summary>
        private static ERational? TryRationalSquareRoot(ERational value)
        {
            if (value.Sign < 0)
                return null;
            var top = value.Numerator.Sqrt();
            var bottom = value.Denominator.Sqrt();
            return top.Multiply(top).Equals(value.Numerator) && bottom.Multiply(bottom).Equals(value.Denominator)
                ? ERational.Create(top, bottom)
                : null;
        }

        /// <summary><c>P(a + t)</c> as its coefficients in <c>t</c>.</summary>
        private static Dictionary<int, ERational> Shifted(XPoly p, ERational a, Field field)
        {
            var result = new Dictionary<int, ERational>();
            var shifted = XPoly.Zero(field);
            var xPlusA = XPoly.FromCoefficients(field, new Dictionary<int, Entity> { [0] = Rational.Create(a), [1] = Integer.One });
            var power = XPoly.One(field);
            for (var k = 0; k <= p.Degree; k++)
            {
                if (p[k].Evaled is Rational coefficient)
                    shifted = shifted.Add(power.Scale(coefficient));
                power = power.Multiply(xPlusA);
            }
            foreach (var pair in shifted.Coefficients)
                if (pair.Value.Evaled is Rational r)
                    result[pair.Key] = r.ERational;
            return result;
        }

        /// <summary>A polynomial in <c>t = x - a</c> written in <c>x</c>.</summary>
        private static XPoly Unshifted(XPoly inT, ERational a, Field field)
        {
            var xMinusA = XPoly.FromCoefficients(field, new Dictionary<int, Entity> { [0] = Rational.Create(a.Negate()), [1] = Integer.One });
            var result = XPoly.Zero(field);
            var power = XPoly.One(field);
            for (var k = 0; k <= inT.Degree; k++)
            {
                result = result.Add(power.Scale(inT[k]));
                power = power.Multiply(xMinusA);
            }
            return result;
        }

        /// <summary>
        /// The series of the square root of <paramref name="coefficients"/> (a polynomial in
        /// <c>t</c>) with constant term <paramref name="s"/>, to <paramref name="terms"/> terms;
        /// for an imaginary branch the series of the root of the negated polynomial, whose
        /// constant term is <paramref name="s"/> as well -- <c>y = i s (1 + ...)</c>, and the
        /// factor <c>i</c> is the arctangent's.
        /// </summary>
        private static List<ERational>? SeriesOfTheRoot(Dictionary<int, ERational> coefficients, ERational s, bool imaginary, int terms)
        {
            ERational At(int k) => coefficients.TryGetValue(k, out var v) ? (imaginary ? v.Negate() : v) : ERational.Zero;
            if (s.IsZero || !At(0).Equals(s.Multiply(s)))
                return null;
            var series = new List<ERational> { s };
            var twoS = s.Multiply(ERational.FromInt32(2));
            for (var k = 1; k < terms; k++)
            {
                var sum = ERational.Zero;
                for (var i = 1; i < k; i++)
                    sum = sum.Add(series[i].Multiply(series[k - i])).ToLowestTerms();
                series.Add(At(k).Subtract(sum).Divide(twoS).ToLowestTerms());
            }
            return series;
        }

        /// <summary>
        /// The Padé approximants <c>A/B</c> of the series, one per pair of degrees: the
        /// polynomials with <c>A - B y = O(t^(deg A + deg B + 1))</c>.
        /// </summary>
        private static IEnumerable<(XPoly A, XPoly B)> Approximants(List<ERational> series, Field field)
        {
            for (var degreeB = 0; degreeB <= MaxMultiplierDegree; degreeB++)
                for (var degreeA = 0; degreeA <= MaxNumeratorDegree; degreeA++)
                {
                    var order = degreeA + degreeB + 1;
                    if (order > series.Count)
                        continue;
                    // Coefficient of t^k in A - B y, for k < order: a_k - sum_i b_i y_(k-i) = 0.
                    var unknowns = degreeA + 1 + degreeB + 1;
                    var rows = new List<Entity[]>();
                    for (var k = 0; k < order; k++)
                    {
                        var row = new Entity[unknowns];
                        for (var i = 0; i < unknowns; i++)
                            row[i] = Integer.Zero;
                        if (k <= degreeA)
                            row[k] = Integer.One;
                        for (var i = 0; i <= degreeB; i++)
                            if (k - i >= 0)
                                row[degreeA + 1 + i] = Rational.Create(series[k - i].Negate());
                        rows.Add(row);
                    }
                    if (Solve(rows, unknowns, degreeA + 1, degreeB, field) is var (a, b) && a is not null && b is not null)
                        yield return (a, b);
                }
        }

        /// <summary>
        /// The approximants at infinity: <c>A(x) - B(x) y(x) = O(x^(-order))</c> for
        /// <c>y = x^2 (s + s_1/x + s_2/x^2 + ...)</c>, with the degree of <c>A</c> two more than
        /// that of <c>B</c>.
        /// </summary>
        private static IEnumerable<(XPoly A, XPoly B)> ApproximantsAtInfinity(List<ERational> series, Field field)
        {
            for (var degreeB = 0; degreeB <= MaxMultiplierDegreeAtInfinity; degreeB++)
            {
                var degreeA = degreeB + 2;
                // In t = 1/x: A = sum a_i t^(-i), B y = sum_i b_i t^(-i) sum_j s_j t^(j-2). The
                // coefficient of t^(-m) for m from degreeA down to degreeA - (degreeA + degreeB):
                // a_m - sum_i b_i s_(i + 2 - m) = 0 -- as many equations as unknowns less one.
                var unknowns = degreeA + 1 + degreeB + 1;
                var rows = new List<Entity[]>();
                for (var m = degreeA; m > degreeA - (unknowns - 1); m--)
                {
                    var row = new Entity[unknowns];
                    for (var i = 0; i < unknowns; i++)
                        row[i] = Integer.Zero;
                    if (m >= 0 && m <= degreeA)
                        row[m] = Integer.One;
                    for (var i = 0; i <= degreeB; i++)
                    {
                        var j = i + 2 - m;
                        if (j >= 0 && j < series.Count)
                            row[degreeA + 1 + i] = Rational.Create(series[j].Negate());
                        else if (j >= series.Count)
                            goto next;
                    }
                    rows.Add(row);
                }
                if (Solve(rows, unknowns, degreeA + 1, degreeB, field) is var (a, b) && a is not null && b is not null)
                    yield return (a, b);
                next:;
            }
        }

        /// <summary>
        /// A nonzero solution of the homogeneous system, with the leading coefficient of
        /// <c>B</c> fixed at one and, failing that, each other unknown in turn.
        /// </summary>
        private static (XPoly?, XPoly?) Solve(List<Entity[]> rows, int unknowns, int aCount, int degreeB, Field field)
        {
            for (var fixedAt = unknowns - 1; fixedAt >= 0; fixedAt--)
            {
                var others = Enumerable.Range(0, unknowns).Where(i => i != fixedAt).ToList();
                var matrix = rows.Select(row => others.Select(i => row[i]).ToArray()).ToArray();
                var rhs = rows.Select(row => (-row[fixedAt]).InnerSimplified).ToArray();
                if (!Functions.PartialFractions.TrySolveLinear(matrix, rhs, out var values) || values is null)
                    continue;
                var all = new Entity[unknowns];
                for (var i = 0; i < others.Count; i++)
                    all[others[i]] = values[i];
                all[fixedAt] = Integer.One;
                var a = new Dictionary<int, Entity>();
                var b = new Dictionary<int, Entity>();
                for (var i = 0; i < aCount; i++)
                    if (all[i].Evaled is not Complex { IsZero: true })
                        a[i] = all[i];
                for (var i = 0; i <= degreeB; i++)
                    if (all[aCount + i].Evaled is not Complex { IsZero: true })
                        b[i] = all[aCount + i];
                if (a.Count == 0 || b.Count == 0)
                    continue;
                return (XPoly.FromCoefficients(field, a), XPoly.FromCoefficients(field, b));
            }
            return (null, null);
        }

        /// <summary>
        /// <c>a + b y</c> for <see cref="XPoly"/> <c>a</c> and <c>b</c>, reduced by <c>y^2 = P</c>.
        /// </summary>
        private sealed class Pair
        {
            private readonly XPoly[] parts;
            private Pair(XPoly a, XPoly b) => parts = new[] { a, b };

            internal XPoly this[int j] => parts[j];

            internal static Pair Constant(XPoly inX) => new(inX, XPoly.Zero(inX.Field));
            internal static Pair Y(Field field) => new(XPoly.Zero(field), XPoly.One(field));

            internal static Pair? Read(Entity expr, Variable x, Variable y, XPoly p)
            {
                if (!TreeAnalyzer.TryGetPolynomial(expr, y, out var inY))
                    return null;
                var a = XPoly.Zero(p.Field);
                var b = XPoly.Zero(p.Field);
                foreach (var pair in inY)
                {
                    if (pair.Key.Sign < 0 || !pair.Key.CanFitInInt32())
                        return null;
                    var power = pair.Key.ToInt32Unchecked();
                    if (XPoly.Read(pair.Value, x, p.Field) is not { } coefficient)
                        return null;
                    for (var i = 0; i < power / 2; i++)
                        coefficient = coefficient.Multiply(p);
                    if (power % 2 == 0) a = a.Add(coefficient);
                    else b = b.Add(coefficient);
                }
                return new(a, b);
            }

            internal Pair Add(Pair other) => new(parts[0].Add(other.parts[0]), parts[1].Add(other.parts[1]));
            internal Pair Subtract(Pair other) => new(parts[0].Subtract(other.parts[0]), parts[1].Subtract(other.parts[1]));
            internal Pair Scale(Entity factor) => new(parts[0].Scale(factor), parts[1].Scale(factor));

            internal Pair Multiply(Pair other, XPoly p)
                => new(parts[0].Multiply(other.parts[0]).Add(parts[1].Multiply(other.parts[1]).Multiply(p)),
                       parts[0].Multiply(other.parts[1]).Add(parts[1].Multiply(other.parts[0])));

            // The cube-root ansatz's generator carries a YPoly; a pair is one with no y^2 part.
            internal YPoly AsYPoly(XPoly p) => YPoly.Linear(parts[0], parts[1]);

            internal static Pair FromYPoly(YPoly poly, XPoly p) => new(poly[0], poly[1]);
        }
    }
}
