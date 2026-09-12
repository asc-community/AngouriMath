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

namespace AngouriMath.Functions.Algebra
{
    /// <summary>
    /// A rational function of <c>x</c> and one cube root <c>y = P(x)^(1/3)</c> of a polynomial,
    /// integrated by an ansatz over logarithms of <c>L - y</c> for linear <c>L</c>, their
    /// conjugates, and a rational part in <c>y</c> and <c>y^2</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The curve <c>y^3 = P(x)</c> has no rational parametrisation for a cubic <c>P</c>, so no
    /// substitution makes these rational; the ones that are elementary at all -- Welz's
    /// <c>x/((1 + x)(1 - x^3)^(1/3))</c>, <c>1/(x^3 - 3x^2 + 7x - 5)^(1/3)</c>,
    /// <c>(x - 1)/((x + 1)(2 + x^3)^(1/3))</c> -- are integrated by logarithms of
    /// <c>L(x) - y</c>, with <c>L</c> linear, and of their conjugates <c>L - w y</c> for the
    /// cube roots of unity <c>w</c>, which pair into <c>ln(L^2 + L y + y^2)</c> and
    /// <c>arctan((2L + y)/(sqrt(3) y))</c>. The norm <c>(L - y)(L - wy)(L - w^2 y) = L^3 - P</c>
    /// is where the logarithm has its poles, so the <c>L</c> that can occur are the ones
    /// whose cube agrees with <c>P</c> at the places the integrand has poles: the linear
    /// function through two points of the curve, or tangent to it at one. Each of those is
    /// a candidate here -- the tangent at a rational pole, at infinity, and at a root of
    /// <c>P</c>; the line through a pole and a root, or two poles -- and for an irreducible
    /// quadratic factor <c>Q</c> of the denominator with <c>P</c> constant modulo <c>Q</c>,
    /// the cube roots of that constant times a cube root of unity of <c>Q[x]/(Q)</c>, which
    /// is where <c>ln(2^(1/3) - y)</c>, <c>ln(2^(1/3) x + y)</c> and
    /// <c>ln(2^(1/3)(x - 1) - y)</c> in Welz's answers over <c>1 - x + x^2</c> come from.
    /// </para>
    /// <para>
    /// The constants those bring in are cube roots of rationals, <c>2^(1/3)</c> most often,
    /// so the coefficients live in <c>Q(c)</c> with <c>c^3</c> rational; the linear system is
    /// solved in that field exactly, as polynomials in <c>c</c> reduced modulo <c>c^3 - m</c>,
    /// rather than numerically -- a symbol for <c>c</c> would not be told from zero at
    /// <c>c^3 - 2</c>, and a floating <c>c</c> would give floating coefficients. One such
    /// constant per integrand; two independent ones would need their compositum and are
    /// declined. With symbols in the coefficients and every constant rational, the general
    /// symbolic solver is used instead.
    /// </para>
    /// <para>
    /// The identity <c>sum c_k g_k' = f</c> is an identity in the function field
    /// <c>Q(x)[y]/(y^3 - P)</c>: everything is brought over one denominator in <c>x</c>, the
    /// numerators reduced by <c>y^3 = P</c> to <c>A + B y + C y^2</c>, and the three
    /// coefficient polynomials in <c>x</c> matched term by term. The answer is checked against
    /// the integrand at sampled points before it is returned.
    /// https://github.com/asc-community/AngouriMath/issues/718
    /// </para>
    /// </remarks>
    internal static class CubeRootLogarithmAnsatz
    {
        /// <summary>The largest degree of the polynomial under the root.</summary>
        private const int MaxRadicandDegree = 4;

        /// <summary>The largest degree of the denominator in <c>x</c>.</summary>
        private const int MaxDenominatorDegree = 6;

        internal static Entity? Integrate(Entity expr, Variable x)
        {
            // One cube root of one polynomial, and otherwise rational in x.
            Entity? radicand = null;
            foreach (var node in expr.Nodes)
                if (node is Powf(var @base, Rational exponent) && @base.ContainsNode(x)
                    && exponent.ERational.Denominator.Equals(EInteger.FromInt32(3)))
                {
                    if (radicand is null)
                        radicand = @base;
                    else if (radicand != @base)
                        return null;
                }
                else if (node is Powf(var otherBase, var otherExponent) && otherBase.ContainsNode(x) && otherExponent is not Integer)
                    return null;
            if (radicand is null)
                return null;
            var c = Variable.CreateUnique(expr, "c");
            var field = new Field(c);
            var constants = new Constants(field);
            var p = XPoly.Read(radicand, x, field);
            if (p is null || p.Degree < 2 || p.Degree > MaxRadicandDegree)
                return null;

            var y = Variable.CreateUnique(expr, "y");
            var inY = expr.Replace(node => node is Powf(var b, Rational e) && b == radicand && e.ERational.Denominator.Equals(EInteger.FromInt32(3))
                ? MathS.Pow(y, Integer.Create(e.ERational.Numerator))
                : node);
            if (!TryReadOverThePolynomialDenominator(inY, x, y, radicand, out var numerator, out var denominatorAsWritten))
                return null;
            var e = XPoly.Read(denominatorAsWritten, x, field);
            if (e is null || e.Degree > MaxDenominatorDegree)
                return null;
            var target = YPoly.Read(numerator, x, y, p);
            if (target is null)
                return null;

            var pEntity = p.ToEntity(x);
            var pPrime = p.Derivative();

            // The places: rational poles, rational roots of P, the irreducible quadratic
            // factors of the denominator.
            var poles = RationalRoots(denominatorAsWritten, x, out var quadraticFactors);
            var roots = RationalRoots(radicand, x, out _);
            if (poles is null || roots is null)
                return null;

            var lines = new List<XPoly>();
            void Offer(Entity? line)
            {
                if (line is null)
                    return;
                if (XPoly.Read(line, x, field) is not { } read || read.IsZero || read.Degree > 1)
                    return;
                if (lines.Any(l => l.SameAs(read)))
                    return;
                lines.Add(read);
            }
            // The cube root of P at each rational pole, as a constant of the field.
            var atPoles = new List<(Entity Pole, Entity? Root)>();
            foreach (var a in poles)
            {
                var value = pEntity.Substitute(x, a).InnerSimplified.Evaled;
                Entity? root = null;
                if (value is Rational ratio)
                    root = constants.CubeRoot(ratio.ERational, c);
                else if (value is Complex { IsZero: true })
                    root = Integer.Zero;
                atPoles.Add((a, root));
            }
            // The tangent at infinity, for a cubic P with a leading coefficient whose cube root
            // is in the field.
            Entity? leadingRoot = null;
            if (p.Degree == 3 && p[3].Evaled is Rational leading)
                leadingRoot = constants.CubeRoot(leading.ERational, c);
            if (leadingRoot is not null)
            {
                Offer(leadingRoot * (x + p[2] / (3 * p[3])));
                foreach (var root in roots)
                    Offer(leadingRoot * (x - root));
            }
            for (var i = 0; i < atPoles.Count; i++)
            {
                var (a, ca) = atPoles[i];
                if (ca is null)
                    continue;
                var pa = pEntity.Substitute(x, a).InnerSimplified;
                if (pa.Evaled is not Complex { IsZero: true })
                    // The tangent at the pole: c_a (1 + P'(a)(x - a)/(3 P(a))).
                    Offer(ca * (1 + pPrime.ToEntity(x).Substitute(x, a).InnerSimplified * (x - a) / (3 * pa)));
                foreach (var root in roots)
                    if ((a - root).InnerSimplified.Evaled is not Complex { IsZero: true })
                        Offer(ca * (x - root) / (a - root));
                for (var j = i + 1; j < atPoles.Count; j++)
                {
                    var (a2, ca2) = atPoles[j];
                    if (ca2 is null || (a2 - a).InnerSimplified.Evaled is Complex { IsZero: true })
                        continue;
                    Offer(ca + (ca2 - ca) * (x - a) / (a2 - a));
                }
            }
            foreach (var quadratic in quadraticFactors)
                foreach (var line in LinesOverAQuadraticFactor(quadratic, pEntity, x, c, constants))
                    Offer(line);
            if (lines.Count == 0)
                return null;

            // The generators: each a numerator in x and y over a product of factors in x and
            // a constant, and the term it stands for.
            var generators = new List<Generator>();
            foreach (var a in poles.Concat(roots).Distinct())
                if (XPoly.Read(x - a, x, field) is { } linear)
                    generators.Add(new Generator(YPoly.Constant(XPoly.One(field)), new List<XPoly> { linear }, 1, MathS.Ln(x - a)));
            foreach (var quadratic in quadraticFactors)
                if (XPoly.Read(quadratic, x, field) is { } q)
                    generators.Add(new Generator(YPoly.Constant(q.Derivative()), new List<XPoly> { q }, 1, MathS.Ln(quadratic)));
            var sqrt3 = MathS.Sqrt(3);
            var yOnly = YPoly.Monomial(1, p);
            var ySquared = YPoly.Monomial(2, p);
            foreach (var line in lines)
            {
                var norm = line.Multiply(line).Multiply(line).Subtract(p);
                if (norm.IsZero)
                    continue;
                var lineEntity = line.ToEntity(x);
                var linePrime = line.Derivative();
                var l = YPoly.Constant(line);
                var lPrime = YPoly.Constant(linePrime);
                var pp = YPoly.Constant(p);
                var ppPrime = YPoly.Constant(pPrime);
                // L^2 + L y + y^2, and L - y.
                var conjugates = l.Multiply(l, p).Add(l.Multiply(yOnly, p)).Add(ySquared);
                var lMinusY = l.Subtract(yOnly);
                var threePLPrime = pp.Multiply(lPrime, p).Scale(3);
                generators.Add(new Generator(
                    threePLPrime.Subtract(ppPrime.Multiply(yOnly, p)).Multiply(conjugates, p),
                    new List<XPoly> { p, norm }, 3, MathS.Ln(lineEntity - y)));
                generators.Add(new Generator(
                    pp.Multiply(l.Multiply(lPrime, p).Scale(2).Add(lPrime.Multiply(yOnly, p)), p).Scale(3)
                        .Add(l.Add(yOnly.Scale(2)).Multiply(ppPrime, p).Multiply(yOnly, p))
                        .Multiply(lMinusY, p),
                    new List<XPoly> { p, norm }, 3, MathS.Ln(lineEntity * lineEntity + lineEntity * y + y * y)));
                generators.Add(new Generator(
                    yOnly.Multiply(threePLPrime.Subtract(l.Multiply(ppPrime, p)), p).Multiply(lMinusY, p),
                    new List<XPoly> { p, norm }, 2, sqrt3 * MathS.Arctan((2 * lineEntity + y) / (sqrt3 * y))));
            }
            var ePrime = e.Derivative();
            var eEntity = e.ToEntity(x);
            for (var j = 1; j <= 2; j++)
                for (var k = 0; k <= e.Degree + 2; k++)
                {
                    var xk = XPoly.Power(field, k);
                    var xkPrime = xk.Derivative();
                    var yj = j == 1 ? yOnly : ySquared;
                    // (y^j x^k / E)' = y^j [j P' x^k E + 3 P (k x^(k-1) E - x^k E')] / (3 P E^2)
                    var inside = pPrime.Multiply(xk).Multiply(e).Scale(j)
                        .Add(p.Multiply(xkPrime.Multiply(e).Subtract(xk.Multiply(ePrime))).Scale(3));
                    generators.Add(new Generator(
                        yj.Multiply(YPoly.Constant(inside), p),
                        new List<XPoly> { p, e, e }, 3, MathS.Pow(y, j) * MathS.Pow(x, k) / eEntity));
                }

            // Everything over one denominator: the product of the distinct factors, each to
            // the highest power any generator has it.
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
            Take(new List<XPoly> { e });
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
                var scaled = generator.Numerator;
                foreach (var factor in Multiplier(generator.Denominator))
                    scaled = scaled.Multiply(YPoly.Constant(factor), p);
                var column = new Dictionary<(int, int), Entity>();
                for (var j = 0; j < 3; j++)
                    foreach (var pair in scaled[j].Coefficients)
                        column[(j, pair.Key)] = field.Normalize(pair.Value / generator.Constant);
                columns.Add(column);
            }
            var targetColumn = new Dictionary<(int, int), Entity>();
            {
                var scaled = target;
                foreach (var factor in Multiplier(new List<XPoly> { e }))
                    scaled = scaled.Multiply(YPoly.Constant(factor), p);
                for (var j = 0; j < 3; j++)
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

            // Exactly over Q(c) where every entry is a number or a polynomial in c; the
            // general symbolic solver only for symbols in the coefficients, and then only
            // where no constant was needed.
            Entity[]? values;
            if (TrySolveOverTheExtension(matrix, rhs, c, constants.Modulus ?? ERational.One, out var solved))
                values = solved.Select(v => v.ToEntity(c)).ToArray();
            else if (constants.Modulus is not null || !TrySolveWithSymbols(matrix, rhs, c, out values))
                return null;

            Entity answer = Integer.Zero;
            for (var k = 0; k < generators.Count; k++)
                if (values[k] != Integer.Zero && values[k].Evaled is not Complex { IsZero: true })
                    answer += values[k] * generators[k].Term;
            var written = answer.Substitute(y, MathS.Pow(radicand, Rational.Create(1, 3)));
            if (constants.Modulus is { } m)
                written = written
                    .Replace(node => node is Powf(var b, Integer k) && b == c ? MathS.Pow(Rational.Create(m), Rational.Create(k.EInteger, EInteger.FromInt32(3))) : node)
                    .Substitute(c, MathS.Pow(Rational.Create(m), Rational.Create(1, 3)));
            written = Functions.PartialFractions.Bare(written);
            if (!Functions.PartialFractions.HoldsAtSampledPoints(written.Differentiate(x), expr, x))
                return null;
            return written;
        }

        /// <summary>
        /// The coefficient field: <c>Q(c)</c> with <c>c^3 = m</c> once a constant has been
        /// needed, and the rationals -- with whatever symbols the integrand carries -- before.
        /// A coefficient with <c>c</c> in it is kept as a polynomial in <c>c</c> of degree
        /// below three, so that products stay small and equal ones read equal.
        /// </summary>
        private sealed class Field
        {
            internal Field(Variable c) => C = c;

            internal Variable C { get; }
            internal RationalPolynomial? Modulus { get; set; }

            internal Entity Normalize(Entity value)
            {
                var bare = Functions.PartialFractions.Bare(value);
                if (!bare.ContainsNode(C) || Modulus is null)
                    return bare.Vars.Any() ? Canonical(bare) : bare;
                if (!PolynomialFactoring.TryGetRationalCoefficients(bare, C, 1, 0, 40, out var coefficients))
                    return bare;
                var poly = RationalPolynomial.Create(coefficients);
                if (poly.TryDivide(Modulus, out _, out var remainder))
                    poly = remainder;
                return poly.ToEntity(C);
            }

            internal static bool IsZero(Entity value)
                => value == Integer.Zero || value.Evaled is Complex { IsZero: true };

            /// <summary>
            /// A coefficient with symbols in it as one canonical polynomial in them, so that
            /// <c>q^2 (-q) q</c> and <c>-q^4</c> read equal; left as it is where it is not a
            /// polynomial in its symbols.
            /// </summary>
            private static Entity Canonical(Entity value)
            {
                var variables = value.Vars.ToList();
                var index = new Dictionary<Variable, int>();
                for (var i = 0; i < variables.Count; i++)
                    index[variables[i]] = i;
                return MultivariatePolynomial.TryParse(value, index) is { } parsed
                    ? parsed.ToEntity(variables)
                    : Functions.PartialFractions.Bare(value.Expand());
            }
        }

        /// <summary>
        /// A polynomial in <c>x</c> as its coefficients by power, each an entity free of
        /// <c>x</c> -- a number, or a polynomial in the field's constant and the symbols.
        /// Arithmetic here never asks the expander, whose term limit a product of a dozen
        /// factors runs past before it collects anything.
        /// </summary>
        private sealed class XPoly
        {
            private XPoly(Field field, Dictionary<int, Entity> coefficients)
            {
                Field = field;
                Coefficients = coefficients;
            }

            internal Field Field { get; }
            internal Dictionary<int, Entity> Coefficients { get; }

            internal static XPoly Zero(Field field) => new(field, new Dictionary<int, Entity>());
            internal static XPoly One(Field field) => Power(field, 0);
            internal static XPoly Power(Field field, int k) => new(field, new Dictionary<int, Entity> { [k] = Integer.One });

            internal bool IsZero => Coefficients.Count == 0;
            internal int Degree => IsZero ? -1 : Coefficients.Keys.Max();
            internal Entity this[int power] => Coefficients.TryGetValue(power, out var value) ? value : Integer.Zero;

            internal static XPoly? Read(Entity expr, Variable x, Field field)
            {
                if (!TreeAnalyzer.TryGetPolynomial(expr, x, out var read))
                    return null;
                var raw = new Dictionary<int, Entity>();
                foreach (var pair in read)
                {
                    if (pair.Key.Sign < 0 || !pair.Key.CanFitInInt32() || pair.Value.ContainsNode(x))
                        return null;
                    raw[pair.Key.ToInt32Unchecked()] = pair.Value;
                }
                return Build(field, raw);
            }

            private static XPoly Build(Field field, Dictionary<int, Entity> raw)
            {
                var coefficients = new Dictionary<int, Entity>();
                foreach (var pair in raw)
                {
                    var value = field.Normalize(pair.Value);
                    if (Field.IsZero(value))
                        continue;
                    coefficients[pair.Key] = value;
                }
                return new(field, coefficients);
            }

            internal XPoly Add(XPoly other)
            {
                var raw = new Dictionary<int, Entity>(Coefficients);
                foreach (var pair in other.Coefficients)
                    raw[pair.Key] = raw.TryGetValue(pair.Key, out var existing) ? existing + pair.Value : pair.Value;
                return Build(Field, raw);
            }

            internal XPoly Subtract(XPoly other) => Add(other.Scale(-1));

            internal XPoly Scale(Entity factor)
            {
                var raw = new Dictionary<int, Entity>();
                foreach (var pair in Coefficients)
                    raw[pair.Key] = factor * pair.Value;
                return Build(Field, raw);
            }

            internal XPoly Multiply(XPoly other)
            {
                var raw = new Dictionary<int, Entity>();
                foreach (var l in Coefficients)
                    foreach (var r in other.Coefficients)
                    {
                        var power = l.Key + r.Key;
                        var product = l.Value * r.Value;
                        raw[power] = raw.TryGetValue(power, out var existing) ? existing + product : product;
                    }
                return Build(Field, raw);
            }

            internal XPoly Derivative()
            {
                var raw = new Dictionary<int, Entity>();
                foreach (var pair in Coefficients)
                    if (pair.Key > 0)
                        raw[pair.Key - 1] = pair.Key * pair.Value;
                return Build(Field, raw);
            }

            internal bool SameAs(XPoly other)
            {
                if (Coefficients.Count != other.Coefficients.Count)
                    return false;
                foreach (var pair in Coefficients)
                    if (!other.Coefficients.TryGetValue(pair.Key, out var value)
                        || (pair.Value != value && !Field.IsZero(Field.Normalize(pair.Value - value))))
                        return false;
                return true;
            }

            internal Entity ToEntity(Variable x)
            {
                Entity result = Integer.Zero;
                foreach (var pair in Coefficients.OrderByDescending(pair => pair.Key))
                    result += pair.Key == 0 ? pair.Value : pair.Value * MathS.Pow(x, pair.Key);
                return result.InnerSimplified;
            }
        }

        /// <summary>
        /// A polynomial in <c>x</c> and <c>y</c> reduced by <c>y^3 = P</c>: three
        /// <see cref="XPoly"/>, the coefficients of <c>1</c>, <c>y</c> and <c>y^2</c>.
        /// </summary>
        private sealed class YPoly
        {
            private readonly XPoly[] parts;
            private YPoly(XPoly[] parts) => this.parts = parts;

            internal XPoly this[int j] => parts[j];

            internal static YPoly Constant(XPoly inX) => new(new[] { inX, XPoly.Zero(inX.Field), XPoly.Zero(inX.Field) });

            /// <summary><c>y^j</c>, for <c>j</c> at most two.</summary>
            internal static YPoly Monomial(int j, XPoly p)
            {
                var parts = new[] { XPoly.Zero(p.Field), XPoly.Zero(p.Field), XPoly.Zero(p.Field) };
                parts[j] = XPoly.One(p.Field);
                return new(parts);
            }

            /// <summary>
            /// <paramref name="expr"/>, a polynomial in <paramref name="x"/> and
            /// <paramref name="y"/>, reduced; null where it is not one.
            /// </summary>
            internal static YPoly? Read(Entity expr, Variable x, Variable y, XPoly p)
            {
                if (!TreeAnalyzer.TryGetPolynomial(expr, y, out var inY))
                    return null;
                var parts = new[] { XPoly.Zero(p.Field), XPoly.Zero(p.Field), XPoly.Zero(p.Field) };
                foreach (var pair in inY)
                {
                    if (pair.Key.Sign < 0 || !pair.Key.CanFitInInt32())
                        return null;
                    var power = pair.Key.ToInt32Unchecked();
                    if (XPoly.Read(pair.Value, x, p.Field) is not { } coefficient)
                        return null;
                    for (var i = 0; i < power / 3; i++)
                        coefficient = coefficient.Multiply(p);
                    parts[power % 3] = parts[power % 3].Add(coefficient);
                }
                return new(parts);
            }

            internal YPoly Add(YPoly other) => new(new[] { parts[0].Add(other.parts[0]), parts[1].Add(other.parts[1]), parts[2].Add(other.parts[2]) });
            internal YPoly Subtract(YPoly other) => new(new[] { parts[0].Subtract(other.parts[0]), parts[1].Subtract(other.parts[1]), parts[2].Subtract(other.parts[2]) });
            internal YPoly Scale(Entity factor) => new(new[] { parts[0].Scale(factor), parts[1].Scale(factor), parts[2].Scale(factor) });

            internal YPoly Multiply(YPoly other, XPoly p)
            {
                var zero = XPoly.Zero(p.Field);
                var raw = new[] { zero, zero, zero, zero, zero };
                for (var i = 0; i < 3; i++)
                    for (var j = 0; j < 3; j++)
                        if (!parts[i].IsZero && !other.parts[j].IsZero)
                            raw[i + j] = raw[i + j].Add(parts[i].Multiply(other.parts[j]));
                // y^3 = P and y^4 = P y.
                return new(new[] { raw[0].Add(raw[3].Multiply(p)), raw[1].Add(raw[4].Multiply(p)), raw[2] });
            }
        }

        private readonly struct Generator
        {
            internal Generator(YPoly numerator, List<XPoly> denominator, int constant, Entity term)
            {
                Numerator = numerator;
                Denominator = denominator;
                Constant = constant;
                Term = term;
            }

            internal YPoly Numerator { get; }
            internal List<XPoly> Denominator { get; }
            internal int Constant { get; }
            internal Entity Term { get; }
        }

        /// <summary>
        /// <paramref name="expr"/>, rational in <paramref name="x"/> and <paramref name="y"/>,
        /// as a numerator polynomial in both over a denominator polynomial in
        /// <paramref name="x"/> alone: a power of <paramref name="y"/> below the bar is moved
        /// above it by <c>1/y = y^2/P</c>.
        /// </summary>
        private static bool TryReadOverThePolynomialDenominator(
            Entity expr, Variable x, Variable y, Entity p, out Entity numerator, out Entity denominator)
        {
            numerator = Integer.One;
            denominator = Integer.One;
            var factors = new List<(Entity Factor, bool Underneath)>();
            void Gather(Entity e, bool underneath)
            {
                switch (e)
                {
                    case Divf(var dividend, var divisor):
                        Gather(dividend, underneath);
                        Gather(divisor, !underneath);
                        break;
                    case Mulf(var l, var r):
                        Gather(l, underneath);
                        Gather(r, underneath);
                        break;
                    case Powf(var b, Integer n) when n.EInteger.Sign < 0:
                        factors.Add((MathS.Pow(b, -n), !underneath));
                        break;
                    default:
                        factors.Add((e, underneath));
                        break;
                }
            }
            Gather(expr, false);
            Entity above = Integer.One;
            Entity below = Integer.One;
            foreach (var (factor, underneath) in factors)
            {
                if (!underneath)
                {
                    above *= factor;
                    continue;
                }
                // y^n below the bar is y^(3m - n)/P^m above it.
                var power = factor == y ? 1 : factor is Powf(var b, Integer n) && b == y && n.EInteger.CanFitInInt32() ? n.EInteger.ToInt32Unchecked() : 0;
                if (power > 0)
                {
                    var m = (power + 2) / 3;
                    var left = 3 * m - power;
                    if (left > 0)
                        above *= MathS.Pow(y, left);
                    below *= MathS.Pow(p, m);
                    continue;
                }
                if (factor.ContainsNode(y))
                    return false;
                below *= factor;
            }
            numerator = above;
            denominator = below;
            return true;
        }

        /// <summary>
        /// The rational roots of <paramref name="poly"/> in <paramref name="x"/>, and its
        /// irreducible quadratic factors over the rationals; where the coefficients carry
        /// symbols, the roots of its written linear factors and no quadratics.
        /// </summary>
        private static List<Entity>? RationalRoots(Entity poly, Variable x, out List<Entity> quadratics)
        {
            quadratics = new List<Entity>();
            var roots = new List<Entity>();
            if (!poly.ContainsNode(x))
                return roots;
            if (PolynomialFactorization.FactorComplete(poly, x) is { } factorization)
            {
                foreach (var part in factorization.Parts)
                {
                    var f = part.Factor;
                    if (f.Degree == 1)
                        roots.Add(Rational.Create(ERational.Create(f[0], f[1]).Negate().ToLowestTerms()));
                    else if (f.Degree == 2)
                        quadratics.Add(RationalPolynomial.FromInteger(f).ToEntity(x));
                }
                return roots;
            }
            foreach (var factor in Mulf.LinearChildren(poly))
            {
                var @base = factor is Powf(var b, Integer) ? b : factor;
                if (TreeAnalyzer.TryGetPolyLinear(@base, x, out var a, out var c) && a.Evaled is not Complex { IsZero: true })
                    roots.Add((-c / a).InnerSimplified);
            }
            return roots;
        }

        /// <summary>
        /// For an irreducible quadratic factor <c>Q = x^2 + p x + q</c> of the denominator with
        /// <c>P</c> constant modulo <c>Q</c>, the linear <c>L</c> with <c>L^3 = P</c> modulo
        /// <c>Q</c>: the cube root of that constant times each cube root of unity of
        /// <c>Q[x]/(Q)</c>, where the field has them.
        /// </summary>
        private static IEnumerable<Entity> LinesOverAQuadraticFactor(Entity quadratic, Entity p, Variable x, Variable c, Constants constants)
        {
            if (!PolynomialFactoring.TryGetRationalCoefficients(quadratic, x, 1, 2, 2, out var qCoefficients)
                || !PolynomialFactoring.TryGetRationalCoefficients(p, x, 1, 0, MaxRadicandDegree, out var pCoefficients))
                yield break;
            var q = RationalPolynomial.Create(qCoefficients);
            q = q.ScaleBy(ERational.One.Divide(q.Leading));
            if (!RationalPolynomial.Create(pCoefficients).TryDivide(q, out _, out var remainder) || remainder.Degree > 0 || remainder.IsZero)
                yield break;
            var m = remainder[0];
            var rootOfM = constants.CubeRoot(m, c);
            if (rootOfM is null)
                yield break;
            // Cube roots of unity u x + v in Q[x]/(x^2 + p x + q): v/u = (p +- sqrt((4q - p^2)/3))/2
            // and u^3 = 1/(p q - 3 t q + t^3) with t = v/u.
            var pp = q[1];
            var qq = q[0];
            yield return rootOfM;
            var discriminantOverThree = qq.Multiply(ERational.FromInt32(4)).Subtract(pp.Multiply(pp)).Divide(ERational.FromInt32(3)).ToLowestTerms();
            if (!TryRationalSquareRoot(discriminantOverThree, out var s))
                yield break;
            foreach (var sign in new[] { 1, -1 })
            {
                var t = pp.Add(s.Multiply(ERational.FromInt32(sign))).Divide(ERational.FromInt32(2)).ToLowestTerms();
                var cubeOfU = ERational.One.Divide(pp.Multiply(qq).Subtract(t.Multiply(qq).Multiply(ERational.FromInt32(3))).Add(t.Multiply(t).Multiply(t))).ToLowestTerms();
                if (!TryRationalCubeRoot(cubeOfU, out var u))
                    continue;
                var v = t.Multiply(u).ToLowestTerms();
                yield return rootOfM * (Rational.Create(u) * x + Rational.Create(v));
            }
        }

        private static bool TryRationalSquareRoot(ERational value, out ERational root)
        {
            root = ERational.Zero;
            if (value.Sign < 0)
                return false;
            var n = value.Numerator.Sqrt();
            var d = value.Denominator.Sqrt();
            if (!n.Multiply(n).Equals(value.Numerator) || !d.Multiply(d).Equals(value.Denominator))
                return false;
            root = ERational.Create(n, d).ToLowestTerms();
            return true;
        }

        private static bool TryRationalCubeRoot(ERational value, out ERational root)
        {
            root = ERational.Zero;
            var negative = value.Sign < 0;
            var magnitude = value.Abs();
            var n = magnitude.Numerator.Root(3);
            var d = magnitude.Denominator.Root(3);
            if (!n.Pow(3).Equals(magnitude.Numerator) || !d.Pow(3).Equals(magnitude.Denominator))
                return false;
            root = ERational.Create(negative ? n.Negate() : n, d).ToLowestTerms();
            return true;
        }

        /// <summary>
        /// The cube roots of rationals an integrand needs, as elements of one field
        /// <c>Q(c)</c> with <c>c^3 = m</c>: a rational where the value is a cube, and
        /// otherwise a rational multiple of <c>c</c> or <c>c^2</c> where the value's cube-free
        /// part is that of <c>m</c> or of <c>m^2</c>. A value that would need a second
        /// generator is declined.
        /// </summary>
        private sealed class Constants
        {
            private readonly Field coefficients;
            private ERational? modulus;
            internal Constants(Field coefficients) => this.coefficients = coefficients;

            /// <summary><c>m</c>, once a cube root of a rational that is not a cube has been asked for.</summary>
            internal ERational? Modulus
            {
                get => modulus;
                private set
                {
                    modulus = value;
                    if (value is not null)
                        coefficients.Modulus = RationalPolynomial.Create(new[] { value.Negate(), ERational.Zero, ERational.Zero, ERational.One });
                }
            }

            internal Entity? CubeRoot(ERational value, Variable c)
            {
                if (value.IsZero)
                    return Integer.Zero;
                if (TryRationalCubeRoot(value, out var rational))
                    return Rational.Create(rational);
                var negative = value.Sign < 0;
                var magnitude = value.Abs();
                // n/d = n d^2 / d^3, so the root is (n d^2)^(1/3)/d.
                var whole = magnitude.Numerator.Multiply(magnitude.Denominator.Multiply(magnitude.Denominator));
                if (!TryCubeFreePart(whole, out var cube, out var cubeFree))
                    return null;
                Entity? root = null;
                if (Modulus is null)
                {
                    // 4^(1/3) is 2^(2/3): a square cube-free part names the smaller generator.
                    var squareRoot = cubeFree.Sqrt();
                    if (squareRoot.Multiply(squareRoot).Equals(cubeFree) && !squareRoot.Equals(EInteger.One))
                    {
                        Modulus = ERational.FromEInteger(squareRoot);
                        root = c * c;
                    }
                    else
                    {
                        Modulus = ERational.FromEInteger(cubeFree);
                        root = c;
                    }
                }
                else if (Modulus!.Numerator.Equals(cubeFree))
                    root = c;
                else if (TryCubeFreePart(Modulus!.Numerator.Pow(2), out var cubeOfSquare, out var cubeFreeOfSquare) && cubeFreeOfSquare.Equals(cubeFree))
                    // m^2 = s^3 r, so r^(1/3) = c^2 / s.
                    root = c * c / Integer.Create(cubeOfSquare);
                if (root is null)
                    return null;
                var scale = ERational.Create(negative ? cube.Negate() : cube, magnitude.Denominator).ToLowestTerms();
                return Rational.Create(scale) * root;
            }

            /// <summary><paramref name="whole"/> as <c>cube^3 * cubeFree</c>, by trial division.</summary>
            private static bool TryCubeFreePart(EInteger whole, out EInteger cube, out EInteger cubeFree)
            {
                cube = EInteger.One;
                cubeFree = EInteger.One;
                var remaining = whole;
                var divisor = EInteger.FromInt32(2);
                var bound = EInteger.FromInt32(10_000);
                while (divisor.CompareTo(bound) <= 0 && divisor.Multiply(divisor).Multiply(divisor).CompareTo(remaining) <= 0)
                {
                    var count = 0;
                    while (remaining.Remainder(divisor).IsZero)
                    {
                        remaining = remaining.Divide(divisor);
                        count++;
                    }
                    for (var i = 0; i < count / 3; i++)
                        cube = cube.Multiply(divisor);
                    for (var i = 0; i < count % 3; i++)
                        cubeFree = cubeFree.Multiply(divisor);
                    divisor = divisor.Add(EInteger.One);
                }
                if (divisor.CompareTo(bound) > 0)
                    return false;
                cubeFree = cubeFree.Multiply(remaining);
                return true;
            }
        }

        /// <summary>
        /// Gaussian elimination over <c>Q(c)</c>, <c>c^3 = m</c>, on a system whose entries are
        /// polynomials in <paramref name="c"/> with rational coefficients. A row reducing to
        /// <c>0 = nonzero</c> declines the system; an unknown no row pivots on is zero.
        /// </summary>
        private static bool TrySolveOverTheExtension(
            Entity[][] matrix, Entity[] rhs, Variable c, ERational m, out RationalPolynomial[] values)
        {
            values = System.Array.Empty<RationalPolynomial>();
            var modulus = RationalPolynomial.Create(new[] { m.Negate(), ERational.Zero, ERational.Zero, ERational.One });
            var rows = rhs.Length;
            var width = matrix[0].Length;
            var a = new RationalPolynomial[rows][];
            var b = new RationalPolynomial[rows];
            for (var row = 0; row < rows; row++)
            {
                a[row] = new RationalPolynomial[width];
                for (var column = 0; column < width; column++)
                    if (Read(matrix[row][column], c, modulus) is { } entry)
                        a[row][column] = entry;
                    else
                        return false;
                if (Read(rhs[row], c, modulus) is { } wanted)
                    b[row] = wanted;
                else
                    return false;
            }

            RationalPolynomial Reduce(RationalPolynomial poly)
                => poly.TryDivide(modulus, out _, out var remainder) ? remainder : poly;

            var pivotColumnOfRow = new int[rows];
            var rank = 0;
            for (var column = 0; column < width && rank < rows; column++)
            {
                var pivot = -1;
                for (var row = rank; row < rows; row++)
                    if (!a[row][column].IsZero)
                    {
                        pivot = row;
                        break;
                    }
                if (pivot < 0)
                    continue;
                (a[pivot], a[rank]) = (a[rank], a[pivot]);
                (b[pivot], b[rank]) = (b[rank], b[pivot]);
                if (!RationalPolynomial.TryBezout(a[rank][column], modulus, out var inverse, out _))
                    return false;
                inverse = Reduce(inverse);
                for (var k = column; k < width; k++)
                    a[rank][k] = Reduce(a[rank][k].Multiply(inverse));
                b[rank] = Reduce(b[rank].Multiply(inverse));
                for (var row = 0; row < rows; row++)
                {
                    if (row == rank || a[row][column].IsZero)
                        continue;
                    var factor = a[row][column];
                    for (var k = column; k < width; k++)
                        a[row][k] = Reduce(a[row][k].Subtract(factor.Multiply(a[rank][k])));
                    b[row] = Reduce(b[row].Subtract(factor.Multiply(b[rank])));
                }
                pivotColumnOfRow[rank] = column;
                rank++;
            }
            for (var row = rank; row < rows; row++)
                if (!b[row].IsZero)
                    return false;
            values = new RationalPolynomial[width];
            for (var column = 0; column < width; column++)
                values[column] = RationalPolynomial.Zero;
            for (var row = 0; row < rank; row++)
                values[pivotColumnOfRow[row]] = b[row];
            return true;
        }

        /// <summary>
        /// The system with symbols in its entries: solved exactly with the symbols pinned to
        /// two sets of rationals, and where the two solutions agree the coefficients are
        /// constants and that is the answer -- <c>1/(x (x^2 - q))^(1/3)</c> is a quarter of a
        /// logarithm and three quarters of another for every <c>q</c>. Where they differ the
        /// symbolic solver is asked, on the columns the pinned solution needed and no others.
        /// </summary>
        private static bool TrySolveWithSymbols(Entity[][] matrix, Entity[] rhs, Variable c, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Entity[]? values)
        {
            values = null;
            var symbols = matrix.SelectMany(row => row).Concat(rhs).SelectMany(entry => entry.Vars).Where(v => v != c).Distinct().ToList();
            Entity[][] Pinned(Entity[][] source, int seed)
                => source.Select(row => row.Select(entry => Pin(entry, symbols, seed)).ToArray()).ToArray();
            Entity[] PinnedRhs(int seed) => rhs.Select(entry => Pin(entry, symbols, seed)).ToArray();
            if (!TrySolveOverTheExtension(Pinned(matrix, 0), PinnedRhs(0), c, ERational.One, out var first)
                || !TrySolveOverTheExtension(Pinned(matrix, 1), PinnedRhs(1), c, ERational.One, out var second))
                return false;
            var width = matrix[0].Length;
            var agree = true;
            for (var k = 0; k < width && agree; k++)
                agree = first[k].SameAs(second[k]);
            if (agree)
            {
                values = first.Select(v => v.ToEntity(c)).ToArray();
                return true;
            }
            var support = Enumerable.Range(0, width).Where(k => !first[k].IsZero || !second[k].IsZero).ToList();
            var reduced = matrix.Select(row => support.Select(k => row[k]).ToArray()).ToArray();
            if (!Functions.PartialFractions.TrySolveLinear(reduced, (Entity[])rhs.Clone(), out var onSupport))
                return false;
            values = new Entity[width];
            for (var k = 0; k < width; k++)
                values[k] = Integer.Zero;
            for (var i = 0; i < support.Count; i++)
                values[support[i]] = onSupport[i];
            return true;
        }

        /// <summary>Every symbol replaced by a rational, distinct per symbol and per seed, off the integers.</summary>
        private static Entity Pin(Entity entry, List<Variable> symbols, int seed)
        {
            for (var i = 0; i < symbols.Count; i++)
                entry = entry.Substitute(symbols[i], Rational.Create(ERational.Create(EInteger.FromInt32(7 + 4 * i + 3 * seed), EInteger.FromInt32(3 + seed))));
            return Functions.PartialFractions.Bare(entry);
        }

        private static RationalPolynomial? Read(Entity entry, Variable c, RationalPolynomial modulus)
        {
            if (!PolynomialFactoring.TryGetRationalCoefficients(entry, c, 1, 0, 12, out var coefficients))
                return null;
            var poly = RationalPolynomial.Create(coefficients);
            return poly.TryDivide(modulus, out _, out var remainder) ? remainder : poly;
        }
    }
}
