//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//
using HonkSharp.Fluency;
using static AngouriMath.Entity;
using System.Linq;
using System.Collections.Generic;
using PeterO.Numbers;

namespace AngouriMath.Functions.Algebra
{
    internal static class IndefiniteIntegralSolver
    {
        internal static Entity? SolveBySplittingSum(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            // The terms as written first, and expanded only if one of those fails: expanding
            // `(1 + 1/x)/(x + ln(x))^(3/2)` writes it as two terms, neither of which is the
            // substitution `u = x + ln(x)` the unexpanded one is, and Bronstein's
            // `1/x + (1 + 1/x)/(x + ln(x))^(3/2)` was declined for that while each of its two
            // terms alone was answered.
            // And a sum over a denominator likewise, each written term over it with a factor
            // written on both sides cancelled: `(ln(x^2 + 1) (x^2 + 1) - x^2)/(2 (x^2 + 1))` is
            // what parts leaves from `x ln(1 + x^2) arctan(x)`, and expanded its first term is
            // `x^2 ln(x^2 + 1)/(2 (x^2 + 1))` and its second `ln(x^2 + 1)/(2 (x^2 + 1))`, neither
            // elementary, where as written it is `ln(x^2 + 1)/2`.
            var (writtenSum, over) = expr is Divf(var writtenNumerator, var writtenDenominator) && writtenNumerator is Sumf or Minusf && writtenDenominator.ContainsNode(x)
                ? (writtenNumerator, writtenDenominator) : (expr, Number.Integer.One as Entity);
            if (writtenSum is Sumf or Minusf)
            {
                // In the order the expanding gather uses -- the terms in the variable, then
                // what is free of it as one term -- so that the answer reads the same either way.
                // And only for terms of modest size: a partial-fraction term whose coefficient
                // is a page of unsimplified symbols took sixty seconds of every rule
                // simplifying the page for itself, where the expanding gather below tidies
                // its terms as it goes and takes half a second on the same sum.
                var inTheVariable = Sumf.LinearChildren(writtenSum).Where(term => term.ContainsNode(x)).ToList();
                var free = Sumf.LinearChildren(writtenSum).Where(term => !term.ContainsNode(x)).ToList();
                var asWritten = new List<Entity>(inTheVariable);
                if (free.Count > 0)
                    asWritten.Add(free.Aggregate((l, r) => l + r));
                if (over != Number.Integer.One)
                    asWritten = asWritten.Select(term => CancelCommonFactors(term, over)).ToList();
                if (asWritten.Count >= 2 && asWritten.All(term => term.Complexity <= LargestTermTakenAsWritten)
                    && Integrated(asWritten) is { } termByTerm)
                    return termByTerm;
            }
            // A sum above the bar that is the derivative of a factor below it is not expanded:
            // `(e^x + e^-x)/(e^x - e^-x)^4` -- `cosh/sinh^4` as the library writes it -- is
            // `f'/f^4`, one substitution, and expanded it is two terms whose antiderivatives
            // carry logarithms that cancel only together. Beside another factor the pair is
            // what parts reads: Timofeev's `arccot(cosh x) cosh x/sinh^4 x` was expanded into
            // two products first, each of them a step of parts with a logarithm-laden `v` and
            // a remainder that took thirty seconds to decline.
            if (IsADerivativeOfAFactorBelowTheBar(expr, x))
                return null;
            // A logarithm or an inverse trigonometric function times a rational function of x
            // goes to parts against the whole rational function before the rational function
            // is expanded into terms: each term's antiderivative keeps a logarithm the whole's
            // does not, and a step of parts against a term is a remainder with that logarithm
            // beside the derivative of the other function, a dilogarithm's shape that every
            // rule below spends itself declining. Timofeev's `arccot(cosh x) cosh x/sinh^4 x`
            // under `u = e^x` is `8 arccot((u + 1/u)/2) (u^4 + u^2)/(u^2 - 1)^4`, answered by one
            // step of parts against the rational function and ten seconds of declines on its
            // two terms first. The same stance as the polynomial times a rational function of
            // an exponential, at the top of the chain. Where parts is allowed at all: a step
            // of parts hands its own integral down with parts switched off, and Timofeev's
            // `arcsin(sinh x) sech^4 x` -- `arcsin((u - 1/u)/2) u^3/(u^2 + 1)^4` -- has a first
            // pairing that differentiates `u^3` and is meant to decline in a moment, where
            // parts on what it hands down was a two-minute search.
            if (integrateByParts && IsAnInverseFunctionTimesARationalFunction(expr, x) && SolveIntegratingByParts(expr, x) is { } byParts)
                return byParts;
            var splitted = TreeAnalyzer.GatherLinearChildrenOverSumAndExpand(expr, e => e.ContainsNode(x));
            if (splitted is null || splitted.Count < 2) return null; // nothing to do, let other solvers do the work
            // Each expanded term with a factor written on both sides of its bar cancelled:
            // `x sqrt(1 - x^2)/(sqrt(1 - x^2)(2 + 2x sqrt(1 - x^2)))` is what the expansion makes
            // of one term of the by-parts remainder of `arctan(x + sqrt(1 - x^2))`, and the
            // chain's own normalisation collects that root over itself into `(1 - x^2)^0`, a
            // factor no rule that reads a root of a quadratic sees through.
            var cancelledTerms = splitted.Select(term =>
            {
                var (above, below) = Functions.SingleQuotient.Of(term);
                return below == Number.Integer.One ? term : CancelCommonFactors(above, below);
            }).ToList();
            if (Integrated(cancelledTerms) is { } termByExpandedTerm)
                return termByExpandedTerm;
            // The expanded terms gathered again by what is not polynomial in them, and each
            // group asked as one term: Hearn's
            // `(-2 sqrt(1 + x^3) + 5x^4 sqrt(1 + x^3) - 3x^2 sqrt(1 - 2x + x^5))/(2 sqrt(1 + x^3) sqrt(1 - 2x + x^5))`
            // expands to `-1/sqrt(P)`, `5x^4/(2 sqrt(P))` and `-3x^2/(2 sqrt(1 + x^3))`, of which
            // the first two are not elementary apart and are `P'/(2 sqrt(P))` together. Only
            // where the gathering joins something and separates something: with every term
            // in a group of its own this is the split above, which has just failed, and with
            // every term in one group it is the integrand. And asked, not volunteered: each
            // group is a search of its own, and one level down the sums are what by parts
            // and the substitutions hand on, where the groups cost sixteen seconds of the
            // corpus for nothing.
            if (!Integration.AnsweringTheQuestionAsked)
                return null;
            // What is polynomial in a term is what has no root of the variable in it: a
            // logarithm or an exponential of x is an indeterminate as far as gathering and
            // cancelling go. Bronstein's
            // `(x^2 + 2x ln(x) + ln(x)^2 + (1 + x) sqrt(x + ln(x)))/(x^3 + 2x^2 ln(x) + x ln(x)^2)`
            // expands to three terms with no root and one with, and the three are `1/x`
            // together -- `(x + L)^2` over `x (x + L)^2` with `L` for the logarithm -- and
            // nothing apart.
            var groups = new List<(Entity Radicals, Entity Below, Entity Polynomial)>();
            foreach (var term in splitted)
            {
                var (above, below) = Functions.SingleQuotient.Of(term);
                Entity polynomial = Number.Integer.One;
                Entity radicals = Number.Integer.One;
                foreach (var factor in Mulf.LinearChildren(above))
                    if (!factor.ContainsNode(x) || !HasARadicalOf(factor, x))
                        polynomial = polynomial * factor;
                    else
                        radicals = radicals * factor;
                var index = groups.FindIndex(group => group.Radicals == radicals && group.Below == below);
                if (index < 0)
                    groups.Add((radicals, below, polynomial));
                else
                    groups[index] = (radicals, below, groups[index].Polynomial + polynomial);
            }
            // Two groups at least: one group is the integrand gathered back into itself, and
            // that has been asked.
            if (groups.Count >= splitted.Count || groups.Count < 2)
                return null;
            return Integrated(groups.Select(group =>
            {
                // Each group's quotient cancelled with the functions of x as indeterminates,
                // and its denominator written in square-free factors the same way, so that
                // `x (x + ln(x))^2` is seen as the factors it is.
                var (top, bottom) = CancelledWithFunctionsAsIndeterminates(group.Polynomial.InnerSimplified, group.Below, x, expr);
                return ((top * group.Radicals).InnerSimplified / bottom).InnerSimplified;
            }).ToList());

            Entity? Integrated(List<Entity> terms)
                => terms.Select(e => Integration.ComputeAsAQuestionOfItsOwn(e, x, integrateByParts)).Aggregate((e1, e2) => (e1, e2) switch {
                    (null, _) or (_, null) => null,
                    (var int1, var int2) => int1 + int2
                });
        }

        /// <summary>The largest term a sum is split over as written, before the expanding gather.</summary>
        private const int LargestTermTakenAsWritten = 60;

        /// <summary>
        /// Whether the integrand is one logarithm or inverse trigonometric function of
        /// <paramref name="x"/>, or a whole power of one, times a rational function of
        /// <paramref name="x"/> that is not a polynomial.
        /// </summary>
        private static bool IsAnInverseFunctionTimesARationalFunction(Entity expr, Entity.Variable x)
        {
            var (above, below) = Functions.SingleQuotient.Of(expr);
            if (!below.ContainsNode(x))
                return false;
            var inverse = 0;
            foreach (var factor in Mulf.LinearChildren(above).Concat(Mulf.LinearChildren(below)))
            {
                if (!factor.ContainsNode(x))
                    continue;
                var @base = factor is Powf(var inner, Number.Integer) ? inner : factor;
                if (@base is Logf or Arcsinf or Arccosf or Arctanf or Arccotanf or Arcsecantf or Arccosecantf)
                    inverse++;
                else if (!IsRationalIn(factor, x))
                    return false;
            }
            return inverse == 1;
        }

        /// <summary>
        /// An integrand that is a constant multiple of <c>D'/D</c> for a sum <c>D</c> below the
        /// bar holding a root, decided at sampled points and answered as that multiple of
        /// <c>ln(D)</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Hearn's <c>x((x^2 - 1) sqrt(x^2 - 4) + (x^2 - 4) sqrt(x^2 - 1))/((x^2 - 1)(x^2 - 4)(1 + sqrt(x^2 - 4) + sqrt(x^2 - 1)))</c>
        /// is <c>(x/sqrt(x^2 - 4) + x/sqrt(x^2 - 1))/(1 + sqrt(x^2 - 4) + sqrt(x^2 - 1))</c>, the
        /// derivative of its denominator over it, and the substitution that reads
        /// <c>u = D</c> was declined for the quotient by <c>D'</c> it could not simplify --
        /// <c>(x^2 - 1) sqrt(x^2 - 4)/((x^2 - 1)(x^2 - 4))</c> is <c>1/sqrt(x^2 - 4)</c> only
        /// with the root's square read as the radicand -- and the search around it ran past
        /// the budget. The quotient <c>f D/D'</c> is evaluated at points instead: where it is
        /// the same rational number at each, <c>f</c> is that number times <c>D'/D</c>, and the
        /// answer is checked against the integrand at sampled points before it is returned,
        /// as the ansätze check theirs.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveALogarithmicDerivativeOfARadicalSum(Entity expr, Entity.Variable x)
        {
            var (_, below) = Functions.SingleQuotient.Of(expr);
            if (below == Number.Integer.One)
                return null;
            foreach (var factor in Mulf.LinearChildren(below))
            {
                if (factor is not (Sumf or Minusf) || !factor.ContainsNode(x) || !HasARadicalOf(factor, x))
                    continue;
                var derivative = factor.Differentiate(x).InnerSimplified;
                if (derivative is Providedf(var bare, _))
                    derivative = bare;
                var quotient = expr * factor / derivative;
                Number? constant = null;
                var agreed = 0;
                foreach (var at in new[] { "2.29", "3.43", "5.71", "0.37", "-2.61", "-4.13" })
                {
                    Entity value;
                    try
                    {
                        value = quotient.Substitute(x, Number.Real.Create(EDecimal.FromString(at))).EvalNumerical();
                    }
                    catch (System.Exception)
                    {
                        continue;
                    }
                    if (value is not Number.Real real || !real.EDecimal.IsFinite)
                        continue;
                    if (constant is null)
                    {
                        // The number as a rational, where it is one within the evaluation's
                        // precision; otherwise this is not the shape.
                        if (Number.Real.Create(real.EDecimal) is not Number.Rational rational)
                            break;
                        constant = rational;
                        agreed = 1;
                        continue;
                    }
                    if ((real - constant).Evaled is not Number.Real realDifference || constant.Evaled is not Number.Real realConstant
                        || realDifference.EDecimal.Abs().CompareTo(EDecimal.FromString("1e-20").Multiply(EDecimal.Max(EDecimal.One, realConstant.EDecimal.Abs()))) > 0)
                    {
                        agreed = 0;
                        break;
                    }
                    agreed++;
                }
                if (constant is null || agreed < 3 || TreeAnalyzer.IsZero(constant))
                    continue;
                Entity answer = constant == Number.Integer.One ? MathS.Ln(factor) : constant * MathS.Ln(factor);
                if (Functions.PartialFractions.HoldsAtSampledPoints(answer.Differentiate(x), expr, x))
                    return answer;
            }
            return null;
        }

        /// <summary>
        /// The signs and moduli of one real-valued argument among the factors of a product
        /// gathered into a power of the argument and at most one sign: <c>|a|^k</c> is
        /// <c>sgn(a)^k a^k</c> for a real <c>a</c>, so <c>sgn(a)^m |a|^k</c> is
        /// <c>sgn(a)^((m + k) mod 2) a^k</c>, and <c>sgn(a)^2</c> is <c>1</c> -- away from
        /// <c>a = 0</c>, which is the generic case every rule answers in.
        /// </summary>
        /// <remarks>
        /// The secant substitution answers <c>1/(x^2 (x^2 - 1)^(5/2))</c> in <c>|x|</c> and
        /// <c>sgn(x)</c>, and a step of parts against it -- Timofeev's
        /// <c>arccsc(x)/(x^2 (x^2 - 1)^(5/2))</c> -- leaves a remainder with <c>|x|^4</c>,
        /// <c>|x|^2</c> and <c>sgn(x)/|x|</c> in it that no rule reads, where it is
        /// <c>x^4</c>, <c>x^2</c> and <c>1/x</c>. Applied with the gathering of powers on every
        /// entry to the chain, for the same reason: the shape is the integrator's own.
        /// </remarks>
        internal static Entity GatherSignsAndModuli(Entity node, Entity.Variable x)
        {
            if (node is not (Mulf or Divf))
                return node;
            var signs = new Dictionary<Entity, EInteger>();
            var moduli = new Dictionary<Entity, EInteger>();
            var rest = new List<Entity>();
            foreach (var factor in Mulf.LinearChildren(node))
            {
                var (@base, exponent) = factor is Powf(var inner, Number.Integer whole) ? (inner, whole.EInteger) : (factor, EInteger.One);
                if (@base is Signumf(var signed) && signed.ContainsNode(x))
                    signs[signed] = signs.TryGetValue(signed, out var m) ? m.Add(exponent) : exponent;
                else if (@base is Absf(var measured) && measured.ContainsNode(x))
                    moduli[measured] = moduli.TryGetValue(measured, out var k) ? k.Add(exponent) : exponent;
                else
                    rest.Add(factor);
            }
            // Something to gather: a modulus beside a sign of the same argument, or a power
            // of either beyond the first.
            var arguments = signs.Keys.Concat(moduli.Keys).Distinct().ToList();
            if (!arguments.Any(argument =>
                    signs.TryGetValue(argument, out var m) && moduli.ContainsKey(argument)
                    || signs.TryGetValue(argument, out m) && m.Abs().CompareTo(EInteger.One) > 0
                    || moduli.TryGetValue(argument, out var k) && k.Abs().CompareTo(EInteger.One) > 0))
                return node;
            Entity? rebuilt = null;
            void Multiply(Entity factor) => rebuilt = rebuilt is null ? factor : rebuilt * factor;
            foreach (var argument in arguments)
            {
                signs.TryGetValue(argument, out var m);
                moduli.TryGetValue(argument, out var k);
                m ??= EInteger.Zero;
                k ??= EInteger.Zero;
                if (!TreeAnalyzer.IsRealValued(argument, x))
                {
                    // Not gathered: |a| is sgn(a) a only for a real a.
                    if (!m.IsZero) Multiply(m.Equals(EInteger.One) ? MathS.Signum(argument) : MathS.Pow(MathS.Signum(argument), Number.Integer.Create(m)));
                    if (!k.IsZero) Multiply(k.Equals(EInteger.One) ? MathS.Abs(argument) : MathS.Pow(MathS.Abs(argument), Number.Integer.Create(k)));
                    continue;
                }
                if (!m.Add(k).IsEven)
                    Multiply(MathS.Signum(argument));
                if (!k.IsZero)
                    Multiply(k.Equals(EInteger.One) ? argument : MathS.Pow(argument, Number.Integer.Create(k)));
            }
            foreach (var factor in rest)
                Multiply(factor);
            return rebuilt ?? Number.Integer.One;
        }

        /// <summary>
        /// Whether a sum among the factors above the bar is a constant multiple of the
        /// derivative of a factor below it that is not a polynomial in <paramref name="x"/>.
        /// </summary>
        private static bool IsADerivativeOfAFactorBelowTheBar(Entity expr, Entity.Variable x)
        {
            var (above, below) = Functions.SingleQuotient.Of(expr);
            if (below == Number.Integer.One)
                return false;
            var sums = Mulf.LinearChildren(above).Where(factor => factor is Sumf or Minusf && factor.ContainsNode(x)).ToList();
            if (sums.Count == 0)
                return false;
            foreach (var factor in Mulf.LinearChildren(below))
            {
                var @base = factor is Powf(var inner, Number.Integer) ? inner : factor;
                if (!@base.ContainsNode(x) || TreeAnalyzer.TryGetPolynomial(@base, x, out _))
                    continue;
                var derivative = @base.Differentiate(x).InnerSimplified;
                if (derivative is Providedf(var bare, _))
                    derivative = bare;
                foreach (var sum in sums)
                {
                    var ratio = (sum / derivative).InnerSimplified;
                    if (ratio.ContainsNode(x) && ratio.Complexity <= LargestTermTakenAsWritten)
                        ratio = ratio.Simplify();
                    if (ratio is Providedf(var bareRatio, _))
                        ratio = bareRatio;
                    if (!ratio.ContainsNode(x))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// A quotient whose numerator and denominator are polynomials in <paramref name="x"/>
        /// and in the functions of <paramref name="x"/> in them, cancelled by their greatest
        /// common divisor with those functions taken for indeterminates, and the denominator
        /// written in its square-free factors the same way; the quotient so written is asked.
        /// </summary>
        /// <remarks>
        /// <c>(x^2 + 2x sin(x) + sin(x)^2)/(x + sin(x))^2</c> is <c>1</c>, and no rule saw that:
        /// split, its three terms are not elementary apart, and as it stands nothing reads a
        /// quotient of polynomials in a sine. With <c>S</c> for the sine it is
        /// <c>(x + S)^2/(x + S)^2</c>. Exact in the generic case, as everywhere in this
        /// integrator: the cancelled factor is taken nonzero. Only where the writing changes
        /// something, so that it cannot ask what it was asked.
        /// </remarks>
        internal static Entity? SolveByCancellingWithFunctionsAsIndeterminates(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            if (!TryReadAsQuotient(expr, out var numerator, out var denominator) || !denominator.ContainsNode(x)
                || !denominator.Nodes.Any(node => node.ContainsNode(x) && node is not (Variable or Sumf or Minusf or Mulf or Divf or Powf(_, Number.Integer))))
                return null;
            var (top, bottom) = CancelledWithFunctionsAsIndeterminates(numerator, denominator, x, expr);
            if (top == numerator && bottom == denominator)
                return null;
            var written = Functions.PartialFractions.Bare(top / bottom);
            if (written == expr)
                return null;
            return Integration.AnsweringTheQuestionAsked
                ? Integration.ComputeAsAQuestionOfItsOwn(written, x, integrateByParts)
                : Integration.ComputeIndefiniteIntegral(written, x, integrateByParts);
        }

        /// <summary>Whether <paramref name="expr"/> holds a fractional power of something in <paramref name="x"/>.</summary>
        private static bool HasARadicalOf(Entity expr, Entity.Variable x)
            => expr.Nodes.Any(node => node is Powf(var @base, Number.Rational power) && power is not Number.Integer && @base.ContainsNode(x));

        /// <summary>
        /// <paramref name="numerator"/> over <paramref name="denominator"/> with every function
        /// of <paramref name="x"/> that is not a polynomial in it -- a logarithm, an
        /// exponential, a trigonometric function -- taken for an indeterminate: the two
        /// cancelled by their greatest common divisor as polynomials in <paramref name="x"/>
        /// and those, and the denominator written as its square-free factorisation, each
        /// factor to its multiplicity, each sum written in the order <paramref name="writtenIn"/>
        /// writes it where it writes it at all. Unchanged where either is not such a
        /// polynomial, or where nothing cancels and nothing factors.
        /// </summary>
        /// <remarks>
        /// The generic case, as everywhere in this integrator: the cancelled factor is taken
        /// nonzero, and the identity <c>ln(x) = L</c> is not used -- a cancellation valid for
        /// every value of <c>L</c> is valid for that one.
        /// </remarks>
        private static (Entity Numerator, Entity Denominator) CancelledWithFunctionsAsIndeterminates(Entity numerator, Entity denominator, Entity.Variable x, Entity writtenIn)
        {
            if (!denominator.ContainsNode(x))
                return (numerator, denominator);
            var atoms = new Dictionary<Entity, Entity.Variable>();
            // Each name unique against the expression and against the atoms named before it.
            var named = numerator + denominator;
            Entity WithAtoms(Entity expr) => expr.Replace(node =>
            {
                if (!node.ContainsNode(x) || node is Variable or Sumf or Minusf or Mulf or Divf
                    || node is Powf(_, Number.Integer { IsNegative: false }))
                    return node;
                if (!atoms.TryGetValue(node, out var atom))
                {
                    atom = Variable.CreateUnique(named, "atom");
                    named = named + atom;
                    atoms[node] = atom;
                }
                return atom;
            });
            var top = WithAtoms(numerator);
            var bottom = WithAtoms(denominator);
            if (top.Nodes.Any(node => node is Powf(_, Number.Rational r) && r is not Number.Integer)
                || bottom.Nodes.Any(node => node is Powf(_, Number.Rational r) && r is not Number.Integer))
                return (numerator, denominator);
            var variables = top.Vars.Concat(bottom.Vars).Distinct().OrderBy(v => v.Name, System.StringComparer.Ordinal).ToList();
            if (variables.Count == 0 || variables.Count > MultivariatePolynomial.MaxVariables)
                return (numerator, denominator);
            var indices = new Dictionary<Variable, int>();
            for (var i = 0; i < variables.Count; i++)
                indices[variables[i]] = i;
            if (MultivariatePolynomial.TryParse(top, indices) is not { } above || MultivariatePolynomial.TryParse(bottom, indices) is not { } below
                || below.IsConstant)
                return (numerator, denominator);
            var order = Enumerable.Range(0, variables.Count).ToList();
            var cancelled = false;
            if (Functions.PolynomialGcd.Gcd(above, below, order, 0) is { IsConstant: false } divisor
                && above.DivideExact(divisor) is { } reducedAbove && below.DivideExact(divisor) is { } reducedBelow)
            {
                above = reducedAbove;
                below = reducedBelow;
                cancelled = true;
            }
            // The square-free factorisation of the denominator in x: Yun's, with the gcds
            // this file already has.
            var factors = new List<(MultivariatePolynomial Factor, int Multiplicity)>();
            var xIndex = indices.TryGetValue(x, out var xi) ? xi : -1;
            if (xIndex >= 0 && Functions.PolynomialGcd.Gcd(below, below.DerivativeIn(xIndex), order, 0) is { IsConstant: false } repeated
                && below.DivideExact(repeated) is { } squareFree)
            {
                var c = repeated;
                var w = squareFree;
                var multiplicity = 1;
                while (!w.IsConstant && multiplicity < 16)
                {
                    if (Functions.PolynomialGcd.Gcd(w, c, order, 0) is not { } y || w.DivideExact(y) is not { } z || c.DivideExact(y) is not { } nextC)
                    {
                        factors.Clear();
                        break;
                    }
                    if (!z.IsConstant)
                        factors.Add((z, multiplicity));
                    w = y;
                    c = nextC;
                    multiplicity++;
                }
                if (factors.Count > 0 && !c.IsConstant)
                    factors.Clear();
            }
            // Written back, and a sum that the integrand already writes in some order is
            // written in that order: the factor `ln(x) + x` beside the root of `x + ln(x)` is
            // not the substitution the root is, and the same sum is.
            var writtenSums = writtenIn.Nodes.Where(node => node is Sumf or Minusf)
                .Select(node => (Node: node, Terms: new HashSet<Entity>(Sumf.LinearChildren(node)))).ToList();
            // Nothing cancelled and nothing factored: the quotient as it was, and not the
            // same quotient respelled, which a rule asking whether anything changed would take
            // for a change.
            // ...or the denominator collapsed to one term as a polynomial in the atoms where
            // it was written as a sum: `cosh(x) + sinh(x)` is `(A + B)/2 + (A - B)/2` for the
            // exponentials `A` and `B`, which is `A`, and Timofeev's `e^(m x)/(cosh(x) + sinh(x))`
            // was declined as a quotient by a sum.
            var collapsed = denominator is Sumf or Minusf && below.Terms.Count() == 1;
            var factored = factors.Count >= 2 || factors.Any(pair => pair.Multiplicity > 1);
            if (!cancelled && !factored && !collapsed)
                return (numerator, denominator);
            // Written back outermost first: an atom holds the atoms inside it by name.
            Entity Back(Entity mapped)
            {
                foreach (var pair in atoms.Reverse())
                    mapped = mapped.Substitute(pair.Value, pair.Key);
                return mapped.Replace(node =>
                {
                    if (node is not (Sumf or Minusf))
                        return node;
                    var terms = new HashSet<Entity>(Sumf.LinearChildren(node));
                    foreach (var (written, writtenTerms) in writtenSums)
                        if (writtenTerms.SetEquals(terms))
                            return written;
                    return node;
                });
            }
            Entity factoredBelow;
            if (factors.Count > 0)
            {
                // The content -- the rational the factors were normalised by -- goes in front.
                MultivariatePolynomial? product = MultivariatePolynomial.One(variables.Count);
                foreach (var (factor, multiplicity) in factors)
                    for (var i = 0; i < multiplicity && product is not null; i++)
                        product = product.Multiply(factor);
                if (product is null || below.DivideExact(product) is not { IsConstant: true } content)
                    factoredBelow = Back(below.ToEntity(variables));
                else
                {
                    factoredBelow = content.ToEntity(variables);
                    foreach (var (factor, multiplicity) in factors)
                    {
                        var written = Back(factor.ToEntity(variables));
                        factoredBelow = factoredBelow == Number.Integer.One ? (multiplicity == 1 ? written : MathS.Pow(written, multiplicity))
                            : factoredBelow * (multiplicity == 1 ? written : MathS.Pow(written, multiplicity));
                    }
                }
            }
            else
                factoredBelow = Back(below.ToEntity(variables));
            // The numerator as it was written where nothing cancelled in it: rebuilt from its
            // monomials it is expanded, and `(1 + x) sqrt(x + ln(x))` expanded is two terms
            // neither of which is the substitution the product is.
            return (cancelled ? Back(above.ToEntity(variables)) : numerator, factoredBelow);
        }

        /// <summary>
        /// A quotient of polynomials, split into two smaller quotients and integrated in two
        /// parts — at a rational root of the denominator where it has one, and otherwise at a
        /// coprime pair of its irreducible factors.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The rules for a linear or a quadratic denominator answer those in one piece, so
        /// what is left over are the denominators of degree three and up, which nothing
        /// read at all: 1/(x^3 + 1) had no antiderivative. Splitting at the root -1 leaves
        /// (1/3)/(x + 1) and (2 - x)/(3(x^2 - x + 1)), and both of those are already
        /// integrable, the second by the rule for a linear numerator over a quadratic.
        /// Each step takes a degree off the denominator, so this ends.
        /// </para>
        /// <para>
        /// The split at a root reaches a denominator only where it has a rational one, which
        /// left 1/(x^4 + 3x^2 + 2) unevaluated although it is (x^2 + 1)(x^2 + 2) and both of
        /// those are integrated by the rule above. <see cref="Functions.PartialFractions"/>
        /// splits those, and is tried when the split at a root either does not apply or
        /// applies and leaves something that will not integrate — it takes the denominator
        /// apart differently, so a failure of the one is no evidence about the other.
        /// </para>
        /// <para>
        /// Both of those factor over the rationals and stop where the rationals do, which left
        /// x^2/(x^4 + 1) unevaluated: x^4 + 1 is irreducible over Q, and over the reals it is
        /// (x^2 - sqrt(2)x + 1)(x^2 + sqrt(2)x + 1). The third step
        /// (<see cref="Functions.PartialFractions.TrySplitBiquadraticOverTheReals"/>) reads a
        /// biquadratic denominator that way. It is last because it is the only one that
        /// introduces a radical, and a denominator that factors over Q should be taken apart in
        /// exact arithmetic by one of the two above.
        /// </para>
        /// <para>
        /// <b>All three of those want a proper fraction</b>, and each declines an improper one
        /// rather than dividing it out — so <c>x^2/(x + 1)</c> had no antiderivative although
        /// it is <c>x - 1 + 1/(x + 1)</c> and every piece of that is read. The division is the
        /// first step here for that reason, and it is not new code:
        /// <see cref="TreeAnalyzer.PolynomialLongDivision"/> has done it all along for the
        /// simplifier's own rule set, and the integrator simply never asked it.
        /// </para>
        /// </remarks>
        internal static Entity? SolveByPartialFractions(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            if (!TryReadAsQuotient(expr, out var numerator, out var denominator))
                return null;

            // The helper answers null for a fraction that is already proper, so this cannot
            // fire on one and recurse into the problem it started from. The check on the
            // quotient is the second half of that guarantee: a division that came back with
            // nothing taken out would hand the same fraction on and not terminate.
            // And only for a fraction that is improper by its degrees: asked to divide `x^2`
            // by `(a + b x)(c + d x)(f/g + x)^2`, the division read the divisor through its
            // other spelling and came back with a quotient of the first degree carrying a
            // condition, and the two halves integrated to a page of piecewise.
            if (!IsProperByDegree(numerator, denominator, x)
                && TreeAnalyzer.PolynomialLongDivision(numerator, denominator, genericCase: true, inTermsOf: x) is var (quotient, properPart)
                && quotient.Evaled != Entity.Number.Integer.Create(0)
                && Integration.ComputeIndefiniteIntegral(quotient, x, integrateByParts) is { } wholePart
                && Integration.ComputeIndefiniteIntegral(properPart, x, integrateByParts) is { } fractionPart)
                return wholePart + fractionPart;

            // A numerator that is a constant multiple of the denominator's derivative is the
            // logarithm of the denominator, whatever the denominator is: Welz's
            // `(3 - 3x + 30x^2 + 160x^3)/(9 + 24x - 12x^2 + 80x^3 + 320x^4)` is `ln(D)/8`, and
            // the substitution rule found it as `u = D` while sums were still its candidates.
            if (denominator.ContainsNode(x)
                && TreeAnalyzer.PolynomialLongDivision(numerator, denominator.Differentiate(x).InnerSimplified, genericCase: true, inTermsOf: x)
                    is var (multiple, leftover)
                && !multiple.ContainsNode(x)
                && (leftover is Divf(var leftoverTop, _) ? leftoverTop : leftover).InnerSimplified.Evaled is Number.Complex { IsZero: true })
                return (multiple * MathS.Ln(denominator)).InnerSimplified;

            // Written linear factors with symbols in their coefficients, two or more, one of
            // them to a power: decomposed over the written factors, the coefficients read
            // off derivatives at the roots and each a line. First, before the respellings
            // below and the Hermite reduction, whose solve hands back its coefficients as
            // quotients of determinants -- `1/((a + b x)(f + g x)^3)` came out twenty
            // kilobytes long that way -- and before a respelling sends the quotient round
            // the chain, where the substitution search answers `x^2/((a + b x)(c + d x)(f + g x)^2)`
            // as a page of piecewise.
            if (Mulf.LinearChildren(denominator).Any(f => f.ContainsNode(x) && f is Powf(_, Number.Integer { EInteger.Sign: > 0 } e) && e != Number.Integer.One)
                && IsAProductOfSymbolicLinearFactors(denominator, x)
                && Functions.PartialFractions.TrySplitOverWrittenFactors(numerator, denominator, x, out var overSymbolicLinears)
                && IntegratedTermByTerm(overSymbolicLinears, x, integrateByParts) is { } overTheLinears)
                return overTheLinears;

            // Every rule below reads the denominator **as written**: the Hermite reduction wants
            // its repeated factor written as a power, the coprime split wants two written blocks.
            // A denominator whose written factors hide either -- `(1 + t^2)(1 - 2t - 2t^3 - t^4)`
            // is `(1 + t^2)^2 (1 - 2t - t^2)`, and Euler's substitution produces exactly that --
            // is written in its irreducible factors over the rationals first, equal ones
            // gathered into one power, and asked again in that spelling -- of this rule, not of
            // the chain: the substitution rule in front of it simplifies the integrand once per
            // candidate subtree, and a denominator written as five factors is five candidates
            // more than one written out, each a rational function for the simplifier to
            // factor. `1/(x^6 - 1)` did not return in thirty seconds through the chain and is
            // under a second this way, with the chain behind it for a spelling this rule does
            // not answer on its own -- one irreducible factor to the first power is the table's.
            // Once: the spelling this produces refactors to itself.
            // A written sum with a symbol among its coefficients that they all share is that
            // symbol times a polynomial over the rationals, and is written so first: `(a u + a)`
            // beside `1 - u^2` shares a root with it, which the symbolic split declines, and a
            // symbolic sextic is nothing the refactoring below reads, where `a` times a rational
            // one is. Once: a primitive factor has no content to take out.
            if (WithTheContentOutOfEachSumFactor(denominator, x) is { } primitive
                && (SolveByPartialFractions(numerator / primitive, x, integrateByParts)
                    ?? Integration.ComputeIndefiniteIntegral(numerator / primitive, x, integrateByParts)) is { } overPrimitives)
                return overPrimitives;

            if (TryWriteInIrreducibleFactors(denominator, x) is { } refactored
                && (SolveByPartialFractions(numerator / refactored, x, integrateByParts)
                    ?? Integration.ComputeIndefiniteIntegral(numerator / refactored, x, integrateByParts)) is { } overIrreducibles)
                return overIrreducibles;

            // And a written base that is a polynomial with rational coefficients spelled some
            // other way -- `2 + (u^2 - 3)^2 - (u^2 - 3)`, which is what the linear-radical
            // substitution makes of `1 + x + 2x^2` under `u = sqrt(3 - 2x)` -- is written as its
            // polynomial, once: the splits below read the bases as written, and Welz's
            // `1/((3 - 2x)^(11/2) (1 + x + 2x^2)^5)` was answered as `1/(u^10 (u^4 - 7u^2 + 14)^5)`
            // and declined as it came. For a polynomial numerator only, since the respelling
            // is asked of the whole chain and a numerator with a root in it is not this
            // rule's to begin with.
            if (Functions.PolynomialFactoring.TryGetRationalCoefficients(numerator, x, 1, 0, 64, out _)
                && TryWriteBasesAsPolynomials(denominator, x) is { } respelled
                && (SolveByPartialFractions(numerator / respelled, x, integrateByParts)
                    ?? Integration.ComputeIndefiniteIntegral(numerator / respelled, x, integrateByParts)) is { } overPolynomials)
                return overPolynomials;

            // A denominator with a written repeated factor takes the Hermite reduction first:
            // the rational part of the answer in one linear solve, and what is left is a proper
            // fraction over a squarefree denominator for the splits below. `(1 + x^2)/(x (1 + x^3)^2)`
            // reached the same answer through them after seventeen seconds of peeling and
            // re-factoring; this way it is under a second.
            if (Mulf.LinearChildren(denominator).Any(f => f.ContainsNode(x) && f is Powf(_, Number.Integer { EInteger.Sign: > 0 } e) && e != Number.Integer.One)
                && IntegrateByAnsatz(null, numerator / denominator, x) is { } byHermite)
                return byHermite;

            // Splitting into coprime blocks comes before peeling one root off, and the order is
            // load-bearing rather than a preference.
            //
            // Both succeed on a denominator with two repeated factors, so whichever runs first
            // decides, and the one that was first is the expensive one. It takes a single root
            // out and hands the remainder on **expanded** — for 1/((1+x)^3 * (2+x)^4) that is a
            // degree-6 denominator written out — so every level of the recursion re-factorises
            // what the level above had already factored, and the whole cost is in there.
            // Measured, integrating the pieces each splitter produces for that integrand:
            //
            //     peel one root, then the remainder        33,318 ms
            //     split into coprime blocks, both halves       39 ms
            //
            // Neither splitter is slow in itself: both return in under 16 ms. It is what they
            // hand on that differs, and the coprime split hands on two smaller problems whose
            // denominators are each a single repeated factor, where peeling hands on one problem
            // barely smaller than the original.
            // https://github.com/asc-community/AngouriMath/issues/1235
            if (Functions.PartialFractions.TrySplitIntoCoprimeParts(
                    numerator, denominator, x, out var left, out var right)
                && Integration.ComputeIndefiniteIntegral(left, x, integrateByParts) is { } overOne
                && Integration.ComputeIndefiniteIntegral(right, x, integrateByParts) is { } overOther)
                return overOne + overOther;

            // Still needed, and not only as a fallback: the coprime split declines a denominator
            // that is a single repeated factor, or that has no second coprime block to split off,
            // and those are exactly what the recursion above descends into.
            if (Functions.PolynomialFactoring.TrySplitOffRationalRoot(
                    numerator, denominator, x, out var simple, out var restNumerator, out var restDenominator)
                && Integration.ComputeIndefiniteIntegral(simple, x, integrateByParts) is { } first
                && Integration.ComputeIndefiniteIntegral(restNumerator / restDenominator, x, integrateByParts) is { } rest)
                return first + rest;

            if (Functions.PartialFractions.TrySplitBiquadraticOverTheReals(
                    numerator, denominator, x, out var overOneReal, out var overOtherReal)
                && Integration.ComputeIndefiniteIntegral(overOneReal, x, integrateByParts) is { } realFirst
                && Integration.ComputeIndefiniteIntegral(overOtherReal, x, integrateByParts) is { } realRest)
                return realFirst + realRest;

            // The two splits above stop where exact rational arithmetic does, and a symbol is
            // not a rational. A polynomial over a power of a linear with a symbol in it --
            // `(c + d x)/(a + b x)^2` -- is one substitution from a sum of powers.
            if (IntegrateAPolynomialOverAPowerOfALinear(numerator, denominator, x) is { } overALinearPower)
                return overALinearPower;

            // And a denominator *written* as a product of distinct linear and quadratic
            // factors -- `(x + a)(x^2 + b)` -- is decomposed by undetermined coefficients,
            // checked, and each piece is a shape the rules here read.
            if (Functions.PartialFractions.TrySplitOverWrittenFactors(numerator, denominator, x, out var overWrittenFactors)
                && IntegratedTermByTerm(overWrittenFactors, x, integrateByParts) is { } termByTerm)
            {
                // Checked: the decomposition's terms integrate through the rules below and,
                // with coefficients that are pages of symbols, one such sum came back wrong
                // with every term right on its own.
                // https://github.com/asc-community/AngouriMath/issues/1369
                if (!overWrittenFactors.Vars.Any(symbol => symbol != x)
                    || Functions.PartialFractions.DerivativeHoldsAtSampledPoints(termByTerm, overWrittenFactors, x))
                    return termByTerm;
            }

            // Last, because everything above answers in exact arithmetic where it can: a
            // binomial denominator the splits above could not take apart -- `x^3 + 2`, `x^5 + 1`,
            // `a x^3 - b` -- decomposed at its roots of unity in closed form.
            if (IntegrateAPolynomialOverABinomial(numerator, denominator, x) is { } atTheRootsOfUnity)
                return atTheRootsOfUnity;

            return null;
        }

        /// <summary>
        /// A polynomial over a power of a linear, <c>P(x)/(a + b x)^k</c> with <c>k >= 2</c>,
        /// under <c>t = a + b x</c>: <c>P((t - a)/b) t^(-k) / b</c> is a sum of powers of
        /// <c>t</c>, each a power rule or a logarithm.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>(c + d x)/(3 + 5 x)^2</c> came out and <c>(c + d x)/(a + b x)^2</c> did not: the
        /// first has a rational root to split at, the second has none, and no other rule read
        /// the shape. <c>1/(a + b x)^2</c> was answered by the quadratic rule, as a piecewise on
        /// a discriminant that is identically zero. This is what the symbolic partial-fraction
        /// split hands on for a repeated linear factor, and it was declined there for want of
        /// this.
        /// </para>
        /// <para>
        /// After the splits over the rationals, so that a numeric coefficient keeps the answer
        /// it had. <c>b</c> is divided by, in the generic case as everywhere here; a decidably
        /// zero <c>b</c> is not a linear at all and is declined.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        private static Entity? IntegrateAPolynomialOverAPowerOfALinear(Entity numerator, Entity denominator, Entity.Variable x)
        {
            if (denominator is not Powf(var linear, Number.Integer exponent)
                || !exponent.EInteger.CanFitInInt32() || exponent.EInteger.ToInt32Unchecked() is var k && k < 2)
                return null;
            if (!TreeAnalyzer.TryGetPolyLinear(linear, x, out var b, out var a)
                || b.Evaled is Number.Complex { IsZero: true })
                return null;
            if (!TreeAnalyzer.TryGetPolynomial(numerator, x, out var above))
                return null;
            foreach (var term in above)
                if (term.Key.Sign < 0 || term.Value.ContainsNode(x))
                    return null;

            var t = Variable.CreateUnique(numerator + denominator, "t");
            var inT = numerator.Substitute(x, (t - a) / b).Expand();
            if (!TreeAnalyzer.TryGetPolynomial(inT, t, out var powersOfT))
                return null;

            Entity total = 0;
            foreach (var term in powersOfT)
            {
                if (!term.Key.CanFitInInt32())
                    return null;
                var power = term.Key.ToInt32Unchecked() - k;
                total += power == -1
                    ? term.Value * IntegralPatterns.AntiderivativeLog(t)
                    : term.Value * MathS.Pow(t, power + 1) / (power + 1);
            }
            return (total / b).Substitute(t, linear).InnerSimplified;
        }

        /// <summary>
        /// A polynomial over a binomial <c>a x^n + b</c>, <c>n >= 3</c>, decomposed at the
        /// <c>n</c>-th roots of <c>-b/a</c> and integrated term by term in closed form.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>1/(x^3 + 1)</c> had an antiderivative and <c>1/(x^3 + 2)</c> did not: the first
        /// factors over the rationals and the coprime split takes it apart, the second does not
        /// and nothing further was tried. The same for <c>1/(x^5 + 1)</c>, <c>1/(x^6 + 2)</c> and
        /// every <c>1/(a x^n + b)</c> with a symbol in it, which no split over the rationals can
        /// reach at all. Rubi's test suite has these by the dozen.
        /// </para>
        /// <para>
        /// <b>The decomposition is a formula, not a computation.</b> Writing the denominator as
        /// <c>a (x^n - c)</c> with <c>c = -b/a</c> and <c>rho^n = c</c>, the roots are
        /// <c>rho e^(i theta_k)</c> and the residue of <c>x^m/(x^n - c)</c> at one of them is
        /// <c>rho^(m+1-n) e^(i (m+1-n) theta_k) / n</c>. A root on the real line gives
        /// <c>rho^(m+1-n) cos((m+1-n) theta) / n</c> over <c>x - rho cos(theta)</c>; a conjugate
        /// pair gives, over <c>x^2 - 2 rho cos(theta) x + rho^2</c>,
        /// </para>
        /// <code>
        ///     (2 rho^(m+1-n) / n) (cos((m+1-n) theta) x - rho cos((m-n) theta))
        /// </code>
        /// <para>
        /// and each of those is a logarithm plus an arctangent, written out here rather than
        /// handed back to the integrator: the quadratic is <c>(x - h)^2 + k^2</c> with
        /// <c>h = rho cos(theta)</c> and <c>k = rho sin(theta)</c>, and
        /// <c>int (P x + Q) / ((x - h)^2 + k^2) dx</c> is
        /// <c>(P/2) ln((x - h)^2 + k^2) + ((Q + P h)/k) arctan((x - h)/k)</c>. Handing the pieces
        /// back would have the quadratic rule decide the sign of a discriminant that is
        /// <c>-4 rho^2 sin^2(theta)</c> and cannot be read as negative once <c>rho</c> is a
        /// symbol, and answer with a piecewise for what is one branch.
        /// </para>
        /// <para>
        /// <b>Which roots, by the sign of <c>c</c>.</b> For <c>c &gt; 0</c> the real
        /// <c>rho = c^(1/n)</c> puts the roots at <c>2 pi k / n</c>; for <c>c &lt; 0</c> it is
        /// <c>rho = (-c)^(1/n)</c> and they sit at <c>pi (2k + 1) / n</c>, so that every
        /// <c>rho</c> and every angle is real and the answer is real on the real line. A symbol
        /// has no sign to read, and takes the first form with <c>rho = c^(1/n)</c>: the
        /// factorisation <c>x^n - c = prod (x - rho zeta_k)</c> is an identity for any
        /// <c>rho</c> with <c>rho^n = c</c>, so the antiderivative is correct for every
        /// <c>c</c> and happens to be written through a complex <c>rho</c> where <c>c</c> is
        /// negative -- which is what Rubi's own answer for <c>1/(a + b x^3)</c> does with its
        /// <c>(a/b)^(1/3)</c>, and is the generic case this integrator gives elsewhere.
        /// </para>
        /// <para>
        /// After every split over the rationals, so that a denominator which factors exactly
        /// keeps the exact answer it had. A proper fraction only; an improper one has been
        /// divided out above.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        private static Entity? IntegrateAPolynomialOverABinomial(Entity numerator, Entity denominator, Entity.Variable x)
        {
            if (!TreeAnalyzer.TryGetPolynomial(denominator, x, out var below) || below.Count != 2
                || !below.TryGetValue(EInteger.Zero, out var b))
                return null;
            var degree = below.Keys.First(power => !power.IsZero);
            if (!degree.CanFitInInt32() || degree.ToInt32Unchecked() is var n && (n < 3 || n > MaximumBinomialDegree))
                return null;
            var a = below[degree];
            if (a.ContainsNode(x) || b.ContainsNode(x)
                || a.Evaled is Number.Complex { IsZero: true } || b.Evaled is Number.Complex { IsZero: true })
                return null;
            if (!TreeAnalyzer.TryGetPolynomial(numerator, x, out var above))
                return null;
            foreach (var term in above)
                if (term.Key.Sign < 0 || term.Key.CompareTo(degree) >= 0 || term.Value.ContainsNode(x))
                    return null;

            // a x^n + b = a (x^n - c). The roots are rho e^(i pi t), for t = (2k + offset)/n.
            var c = (-b / a).InnerSimplified;
            var offset = 0;
            Entity rho;
            // A symbol has no sign to read, but it has a spelling: `x^3 + a` arrives as
            // `c = -a`, and taking `rho = a^(1/3)` with the roots at the odd multiples is the
            // real answer for the sign the integrand was evidently written for, where
            // `(-a)^(1/3)` would be a complex one for it. Either is correct; this one is the
            // one a reader expects.
            if (c.Evaled is Number.Real { IsFinite: true } sign && sign < 0
                || c is Mulf(var leading, _) && leading.Evaled is Number.Real { IsFinite: true } negative && negative < 0)
            {
                rho = MathS.Pow(-c, Number.Rational.Create(1, n));
                offset = 1;
            }
            else
                rho = MathS.Pow(c, Number.Rational.Create(1, n));
            rho = rho.InnerSimplified;

            Entity total = 0;
            for (var k = 0; k < n; k++)
            {
                var twiceKPlusOffset = 2 * k + offset;
                // Past pi the roots are the conjugates of the ones before it, and each pair is
                // taken once, at its representative below pi.
                if (twiceKPlusOffset > n)
                    break;
                var theta = MathS.pi * Number.Rational.Create(twiceKPlusOffset, n);
                foreach (var term in above)
                {
                    var m = term.Key.ToInt32Unchecked();
                    var coefficient = term.Value;
                    // rho^(m+1-n) / n, the modulus of the residue at every root.
                    var modulus = (coefficient * MathS.Pow(rho, m + 1 - n) / n).InnerSimplified;
                    if (twiceKPlusOffset == 0)
                        total += modulus * IntegralPatterns.AntiderivativeLog(x - rho);
                    else if (twiceKPlusOffset == n)
                        total += modulus * ((m + 1 - n) % 2 == 0 ? 1 : -1) * IntegralPatterns.AntiderivativeLog(x + rho);
                    else
                    {
                        var p = (2 * modulus * MathS.Cos((m + 1 - n) * theta)).InnerSimplified;
                        var q = (-2 * modulus * rho * MathS.Cos((m - n) * theta)).InnerSimplified;
                        var h = (rho * MathS.Cos(theta)).InnerSimplified;
                        var kappa = (rho * MathS.Sin(theta)).InnerSimplified;
                        var quadratic = MathS.Sqr(x) - 2 * h * x + MathS.Sqr(rho);
                        total += p / 2 * MathS.Ln(quadratic) + (q + p * h) / kappa * MathS.Arctan((x - h) / kappa);
                    }
                }
            }
            return (total / a).InnerSimplified;
        }

        /// <summary>
        /// The largest binomial degree <see cref="IntegrateAPolynomialOverABinomial"/> takes on.
        /// Every root past the first two is a logarithm and an arctangent, so the answer's size
        /// is linear in the degree; the cap keeps a stray <c>x^1000 + 1</c> from being answered
        /// with five hundred of each. Rubi's suite goes to twelve.
        /// </summary>
        private const int MaximumBinomialDegree = 24;

        /// <summary>
        /// <c>N/(c g(x))</c> integrated as <c>(1/c) * N/g(x)</c>, where <c>c</c> is whatever part
        /// of the denominator is free of the variable.
        /// </summary>
        /// <remarks>
        /// The constant is a constant wherever it stands, and the two branches above take it out
        /// only when it is the *whole* of one side — so <c>a * (1/(1 + x^3))</c> was answered and
        /// <c>1/(a*(1 + x^3))</c>, the same number, was not. Everything downstream reads the
        /// denominator as a polynomial over the rationals, and a symbolic factor stops it being
        /// one, so leaving the factor in place is not a neutral choice.
        ///
        /// It terminates because the denominator handed on has strictly fewer factors, and
        /// declines at once where there is no constant factor to take.
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        private static Entity? TakeConstantFactorOutOfDenominator(
            Entity numerator, Entity denominator, Entity.Variable x, bool integrateByParts)
        {
            Entity? constantPart = null;
            Entity? variablePart = null;
            foreach (var factor in Entity.Mulf.LinearChildren(denominator))
                if (!factor.ContainsNode(x))
                    constantPart = constantPart is null ? factor : constantPart * factor;
                // A whole power of a polynomial with a symbolic leading coefficient is that
                // coefficient's power times the power of the monic polynomial: `t^2/(b - d t^2)^3`
                // is `1/(-d)^3` times `t^2/(t^2 - b/d)^3`, and the rational rules with a symbol
                // in the leading place answer the first power and not the third -- the
                // quotient of two linear radicals hands on exactly this for Hearn's
                // `sqrt(a + b x) sqrt(c + d x)`. The first power is left as written, its table
                // rules reading the coefficient where it is. Built without a stray `1 *`, since
                // the table rules read the shape as written.
                else if (factor is Powf(var polynomial, Number.Integer power) && power.EInteger.CompareTo(EInteger.FromInt32(2)) >= 0
                         && TreeAnalyzer.TryGetPolynomial(polynomial, x, out var read) && read.Count > 1
                         && read.Keys.Max() is { } top && read[top] is var leading && !leading.ContainsNode(x) && leading.Evaled is not Number)
                {
                    var leadingPower = MathS.Pow(leading, power);
                    constantPart = constantPart is null ? leadingPower : constantPart * leadingPower;
                    Entity? monic = null;
                    foreach (var pair in read.OrderBy(pair => pair.Key))
                    {
                        Entity coefficient = pair.Key.Equals(top) ? Number.Integer.One : (pair.Value / leading).InnerSimplified;
                        var degree = pair.Key.ToInt32Checked();
                        Entity term = degree == 0 ? coefficient
                            : coefficient == Number.Integer.One ? (degree == 1 ? x : MathS.Pow(x, degree))
                            : coefficient * (degree == 1 ? x : MathS.Pow(x, degree));
                        monic = monic is null ? term : monic + term;
                    }
                    var monicPower = MathS.Pow(monic!, power);
                    variablePart = variablePart is null ? monicPower : variablePart * monicPower;
                }
                else
                    variablePart = variablePart is null ? factor : variablePart * factor;

            // Only where the denominator genuinely mixes the two. A denominator that is entirely
            // constant, or entirely in the variable, is one of the branches below's to answer,
            // and taking this one would hand on `N/1` and go round again.
            if (constantPart is null || variablePart is null)
                return null;

            return Integration.ComputeIndefiniteIntegral(numerator / variablePart, x, integrateByParts)
                ?.Pipe(i => i / constantPart);
        }

        /// <summary>
        /// The same for a numerator that is a product with a constant factor in it:
        /// <c>sqrt(2) sqrt(1 - t^2)/(t^2 + 1)</c> is <c>sqrt(2)</c> times a quotient Euler's
        /// substitution answers, and with the root of two inside the quotient -- a radical
        /// beside the radical -- it and the rules beside it declined. The tangent
        /// substitution hands on exactly that for Timofeev's <c>sqrt(cot(2x)/cot(x))</c>.
        /// </summary>
        private static Entity? TakeConstantFactorOutOfNumerator(
            Entity numerator, Entity denominator, Entity.Variable x, bool integrateByParts)
        {
            Entity constantPart = Number.Integer.One;
            Entity variablePart = Number.Integer.One;
            foreach (var factor in Entity.Mulf.LinearChildren(numerator))
                if (factor.ContainsNode(x))
                    variablePart *= factor;
                else
                    constantPart *= factor;
            if (constantPart == Number.Integer.One || variablePart == Number.Integer.One)
                return null;
            // A rational constant is read by every rule that reads the quotient, and is left
            // where it is: taken out, the `-1` in front of what the tangent substitution makes
            // of Timofeev's 568 sent the quotient down a search that did not return, where the
            // rule for combining radicals answers it with the sign in place in three seconds.
            if (constantPart.Evaled is Number.Rational)
                return null;
            // As a question of its own: the quotient without its constant is the question
            // asked, strictly smaller and no continuation of any search, and one level down
            // the rules scoped to the top would not see it.
            return Integration.ComputeAsAQuestionOfItsOwn(variablePart / denominator, x, integrateByParts)
                ?.Pipe(i => constantPart * i);
        }

        /// <summary>
        /// Reads <paramref name="expr"/> as a numerator over a denominator, in either of the two
        /// spellings a quotient has here.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>1/(1 + x^3)</c> is a <c>Divf</c> and <c>(1 + x^3)^(-1)</c> is a <c>Powf</c>, and
        /// they are the same integrand. <see cref="SolveByPartialFractions"/> matched the first
        /// and declined the second, so which of them was asked decided whether the integral came
        /// back — and the second is not an exotic way to write it: it is what
        /// <see cref="SolveAsPolynomialTerm"/> builds whenever it takes a factor out of a
        /// quotient. That is why <c>a/(1 + x^3)</c> had no antiderivative while
        /// <c>1/(1 + x^3)</c> and <c>a * (1/(1 + x^3))</c> both did.
        /// </para>
        /// <para>
        /// <b>And the third spelling, a product with a negative power in it.</b>
        /// <c>3 u (4 - u^3)^(-1)</c> is what <c>Simplify</c> makes of <c>3u/(4 - u^3)</c>, and it
        /// is neither a <c>Divf</c> nor a bare <c>Powf</c>, so it was declined here -- and then
        /// taken by integration by parts with <c>v' = (4 - u^3)^(-1)</c>, which integrates to a
        /// hundred-node sum of logarithms and arctangents that the search then spent thirty
        /// seconds failing to integrate against <c>3u</c>. Read as the quotient it is, the same
        /// integrand is answered in a fifth of a second. The factors are gathered and one quotient
        /// rebuilt from them, which is what makes the verdict independent of how the product
        /// happens to be associated.
        /// </para>
        /// <para>
        /// <b>Only a negative whole power.</b> A positive one is not a quotient; a fractional or
        /// symbolic exponent is not one either, and <c>(a/b)^(1/2)</c> is not <c>sqrt(a)/sqrt(b)</c>
        /// on the branch cut. Nothing here rewrites a quotient back into a power, so this cannot
        /// re-enter the mutual recursion <see cref="Integration.Normalized"/> records.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        /// <summary>
        /// A rational function with rational coefficients whose denominator the splits above
        /// could not take apart -- irreducible over the rationals past degree two, or with
        /// real factors that carry the roots of something worse -- integrated by the Hermite
        /// reduction and the Rothstein–Trager resultant, in real terms. See
        /// <see cref="RothsteinTrager"/>. After <see cref="SolveByPartialFractions"/>, so that
        /// everything that answers keeps the form it gives.
        /// </summary>
        internal static Entity? SolveByRothsteinTrager(Entity expr, Entity.Variable x)
        {
            if (!TryReadAsQuotient(expr, out var numerator, out var denominator))
                return null;
            return RothsteinTrager.Integrate(numerator, denominator, x);
        }

        private static bool TryReadAsQuotient(Entity expr, out Entity numerator, out Entity denominator)
        {
            // An exponent read evaluated: the normalisation writes `(u^2 - 1)^5` below the bar
            // as `(u^2 - 1)^(5 * (-1))`, a power whose exponent is a product of two numbers and
            // not a number, and `1/((1 - x)^(7/2) x^5)` was declined for it one level down.
            switch (expr)
            {
                case Entity.Divf(var dividend, var divisor):
                    (numerator, denominator) = (dividend, divisor);
                    return true;
                // A whole power of a quotient is the quotient of the powers: `(u/(1 - 2u^2))^2`,
                // which is how the simplifier writes the image of `sqrt((1 + x)/(3 + 2x))`
                // under the root as the variable, read as nothing before.
                case Entity.Powf(Entity.Divf(var dividend, var divisor), var exponent) when exponent.Evaled is Number.Integer { EInteger.Sign: > 0 } power && power != Number.Integer.One:
                    (numerator, denominator) = (MathS.Pow(dividend, power), MathS.Pow(divisor, power));
                    return true;
                case Entity.Powf(var @base, var exponent) when exponent.Evaled is Number.Integer { EInteger.Sign: < 0 } power:
                    // Written out rather than as base^1 when the power is -1, since everything
                    // downstream reads the denominator as a polynomial and a redundant power of
                    // one is a shape it would have to see through.
                    var reciprocated = -power;
                    (numerator, denominator) = (Number.Integer.One,
                        reciprocated == Number.Integer.One ? @base : MathS.Pow(@base, reciprocated));
                    return true;
                case Entity.Mulf:
                    Entity above = Number.Integer.One;
                    Entity below = Number.Integer.One;
                    foreach (var factor in Entity.Mulf.LinearChildren(expr))
                        switch (factor)
                        {
                            case Entity.Powf(var @base, var exponent) when exponent.Evaled is Number.Integer { EInteger.Sign: < 0 } power:
                                var positive = -power;
                                below *= positive == Number.Integer.One ? @base : MathS.Pow(@base, positive);
                                break;
                            case Entity.Powf(Entity.Divf(var dividend, var divisor), var exponent) when exponent.Evaled is Number.Integer { EInteger.Sign: > 0 } power:
                                above *= MathS.Pow(dividend, power);
                                below *= MathS.Pow(divisor, power);
                                break;
                            case Entity.Divf(var dividend, var divisor):
                                above *= dividend;
                                below *= divisor;
                                break;
                            default:
                                above *= factor;
                                break;
                        }
                    if (below == Number.Integer.One)
                    {
                        (numerator, denominator) = (expr, Number.Integer.One);
                        return false;
                    }
                    (numerator, denominator) = (above, below);
                    return true;
                default:
                    (numerator, denominator) = (expr, Number.Integer.One);
                    return false;
            }
        }

        internal static Entity? SolveAsPolynomialTerm(Entity expr, Entity.Variable x, bool integrateByParts = true) => expr switch
        {
            Entity.Mulf(var m1, var m2) =>
                !m1.ContainsNode(x) ?
                    Integration.ComputeIndefiniteIntegral(m2, x, integrateByParts)?.Pipe(i => m1 * i) :
                !m2.ContainsNode(x) ?
                    Integration.ComputeIndefiniteIntegral(m1, x, integrateByParts)?.Pipe(i => m2 * i) :
                null,

            Entity.Divf(var div, var over) =>
                // A denominator that is partly constant is split first, and the order matters.
                // The branch below turns `c/g(x)` into `c * g(x)^(-1)`, and a power is a shape
                // the rational rules do not read -- so `1/(a*(1 + x^3))` went that way and was
                // declined, although `a * (1/(1 + x^3))`, the same number, is taken apart by the
                // Mulf case above. Taking the constant out is both cheaper and more decisive.
                TakeConstantFactorOutOfDenominator(div, over, x, integrateByParts) is { } withoutIt ?
                    withoutIt :
                TakeConstantFactorOutOfNumerator(div, over, x, integrateByParts) is { } withoutItAbove ?
                    withoutItAbove :
                !div.ContainsNode(x) ?
                    // The exponent negated as a number rather than as a tree: `-power` on the
                    // node `2` is `2 * (-1)`, and `sin(x)^(2 * (-1))` is a shape the closed rule
                    // for a power of the sine does not read, so `c/sin(x)^2` went to the
                    // half-angle substitution for what is `-c cot(x)`.
                    over is Entity.Powf(var @base, var power) ?
                        Integration.ComputeIndefiniteIntegral(MathS.Pow(@base, (-power).InnerSimplified), x, integrateByParts)?.Pipe(i => div * i) :
                        Integration.ComputeIndefiniteIntegral(MathS.Pow(over, -1), x, integrateByParts)?.Pipe(i => div * i) :
                !over.ContainsNode(x) ?
                    Integration.ComputeIndefiniteIntegral(div, x, integrateByParts)?.Pipe(i => i / over) :
                null,

            Entity.Powf(var @base, var power) =>
                !power.ContainsNode(x) && @base == x ?
                    IntegrateAPowerOfTheVariable(@base, power, x) :
                    null,

            Entity.Variable v =>
                v == x ? MathS.Pow(x, 2) / 2 : v * x,

            _ => null
        };

        /// <summary>
        /// A polynomial numerator over a denominator with a power of a linear in it beside a
        /// factor that is not a polynomial -- <c>P(x)/((a + b x)^k S(x))</c> -- with the
        /// polynomial written in powers of the linear at its root: what the power divides goes
        /// over <c>S</c> alone, and each lower power over its own power of the linear.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>P(x) = p_0 + p_1 (a + b x) + ... + (a + b x)^k Q(x)</c> is a polynomial identity,
        /// its coefficients the Taylor coefficients of <c>P</c> at <c>-a/b</c> over the powers
        /// of <c>b</c>, so the integrand is <c>Q/S + sum p_j /((a + b x)^(k-j) S)</c> exactly and
        /// everywhere. <c>(A + B x + C x^2 + D x^3)/((a + b x) sqrt(c + d x))</c> was answered
        /// through the substitution <c>u = sqrt(c + d x)</c> term by term, a cubic in
        /// <c>u^2 - c</c> over a symbolic quadratic in <c>u</c> each time, in a hundred kilobytes
        /// of piecewise that did not evaluate within the corpus's budget; reduced, it is a
        /// quadratic over the root, three powers, and one <c>p_0/((a + b x) sqrt(c + d x))</c>.
        /// </para>
        /// <para>
        /// Only beside a factor that is not a polynomial in <c>x</c>: a quotient of polynomials
        /// is the partial fractions' and they do this and more.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveByReducingThePolynomialOverALinearFactor(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            if (!TryReadAsQuotient(expr, out var numerator, out var denominator))
                return null;
            if (!TreeAnalyzer.TryGetPolynomial(numerator, x, out var polynomial) || polynomial.Count == 0
                || polynomial.Keys.Any(power => power.Sign < 0) || polynomial.Values.Any(coefficient => coefficient.ContainsNode(x)))
                return null;
            var degree = polynomial.Keys.Max()!;
            if (!degree.CanFitInInt32() || degree.ToInt32Unchecked() < 1)
                return null;

            Entity? linear = null;
            var multiplicity = 0;
            Entity rest = Number.Integer.One;
            var somethingElse = false;
            foreach (var factor in Mulf.LinearChildren(denominator))
            {
                if (!factor.ContainsNode(x))
                {
                    rest *= factor;
                    continue;
                }
                var (@base, power) = factor is Powf(var repeated, Number.Integer { EInteger.Sign: > 0 } times) && times.EInteger.CanFitInInt32()
                    ? (repeated, times.EInteger.ToInt32Unchecked()) : (factor, 1);
                if (linear is null && TreeAnalyzer.TryGetPolynomial(@base, x, out var read) && read.Count > 0
                    && read.Keys.Max()!.Equals(EInteger.One) && read.Values.All(coefficient => !coefficient.ContainsNode(x)))
                {
                    linear = @base;
                    multiplicity = power;
                    continue;
                }
                if (!TreeAnalyzer.TryGetPolynomial(factor, x, out _))
                    somethingElse = true;
                rest *= factor;
            }
            if (linear is null || !somethingElse)
                return null;

            if (!TreeAnalyzer.TryGetPolynomial(linear, x, out var line))
                return null;
            var beta = line[EInteger.One];
            var alpha = line.TryGetValue(EInteger.Zero, out var constantTerm) ? constantTerm : Number.Integer.Zero;
            var root = Functions.PartialFractions.Bare((-alpha / beta).InnerSimplified);
            var count = degree.ToInt32Unchecked() + 1;
            if (Functions.PartialFractions.TaylorCoefficientsAtTheRoot(numerator, root, count, x) is not { } coefficients)
                return null;

            Entity total = Number.Integer.Zero;
            for (var order = 0; order < multiplicity && order < count; order++)
            {
                if (coefficients[order] == Number.Integer.Zero)
                    continue;
                var over = multiplicity - order;
                var coefficient = Functions.PartialFractions.InLowestTermsOverTheSymbols(coefficients[order] / MathS.Pow(beta, order));
                var piece = coefficient / ((over == 1 ? linear : MathS.Pow(linear, over)) * rest);
                if (Integration.ComputeIndefiniteIntegral(piece, x, integrateByParts) is not { } integrated)
                    return null;
                total += integrated;
            }
            Entity quotient = Number.Integer.Zero;
            var offset = x - root;
            for (var order = multiplicity; order < count; order++)
            {
                if (coefficients[order] == Number.Integer.Zero)
                    continue;
                var coefficient = Functions.PartialFractions.InLowestTermsOverTheSymbols(coefficients[order] / MathS.Pow(beta, multiplicity));
                var shift = order - multiplicity;
                quotient += shift == 0 ? coefficient : shift == 1 ? coefficient * offset : coefficient * MathS.Pow(offset, shift);
            }
            if (quotient != Number.Integer.Zero)
            {
                var expanded = Functions.PartialFractions.Bare(quotient.Expand().InnerSimplified);
                if (Integration.ComputeIndefiniteIntegral(expanded / rest, x, integrateByParts) is not { } integrated)
                    return null;
                total += integrated;
            }
            return total;
        }

        /// <summary>
        /// A polynomial in <c>x^2</c> over <c>a + b x^2 + c x^4</c> with a symbol in it, by the
        /// two roots in <c>x^2</c>: with <c>q = sqrt(b^2 - 4 a c)</c> and
        /// <c>r = (-b ± q)/(2c)</c>, <c>(d + e x^2)/(a + b x^2 + c x^4)</c> is
        /// <c>(d + e r_1)/(q (x^2 - r_1)) - (d + e r_2)/(q (x^2 - r_2))</c>, and
        /// <c>1/(x^2 - r)</c> is <c>atan(x/sqrt(-r))/sqrt(-r)</c> for any complex <c>r</c>. A
        /// higher even degree is divided down first, and an odd numerator is left to the
        /// substitution <c>u = x^2</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The partial fractions read written factors, and <c>a + b x^2 + c x^4</c> is written
        /// as one; over the rationals it is factored, over symbols it was declined, and Rubi's
        /// quartic files -- <c>(d x)^m (a + b x^2 + c x^4)^p</c> and the next four -- lost the
        /// even numerators to it: <c>x^2/(a + b x^2 + c x^4)</c>, <c>(d + e x^2)/(...)</c>.
        /// </para>
        /// <para>
        /// Exact wherever the two roots differ, which is the generic case; a discriminant
        /// that is zero as written declines, the square of a quadratic being another shape.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveAnEvenPolynomialOverASymbolicBiquadratic(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            if (!TryReadAsQuotient(expr, out var numerator, out var denominator))
                return null;
            if (!TreeAnalyzer.TryGetPolynomial(denominator, x, out var below) || below.Count == 0
                || !below.ContainsKey(EInteger.FromInt32(4)) || below.Keys.Any(power => !(power.IsZero || power.Equals(EInteger.FromInt32(2)) || power.Equals(EInteger.FromInt32(4))))
                || below.Values.Any(coefficient => coefficient.ContainsNode(x))
                || !denominator.Vars.Any(v => v != x))
                return null;
            var a = below.TryGetValue(EInteger.Zero, out var a0) ? a0 : Number.Integer.Zero;
            var b = below.TryGetValue(EInteger.FromInt32(2), out var b0) ? b0 : Number.Integer.Zero;
            var c = below[EInteger.FromInt32(4)];
            if (a == Number.Integer.Zero || a.Evaled is Number.Complex { IsZero: true })
                return null;
            if (!TreeAnalyzer.TryGetPolynomial(numerator, x, out var above) || above.Count == 0
                || above.Keys.Any(power => power.Sign < 0 || !power.IsEven) || above.Values.Any(coefficient => coefficient.ContainsNode(x)))
                return null;

            var discriminant = Functions.PartialFractions.Bare((b * b - 4 * a * c).Simplify());
            if (discriminant == Number.Integer.Zero || discriminant.Evaled is Number.Complex { IsZero: true })
                return null;

            // The numerator in w = x^2, divided down by c w^2 + b w + a to a remainder d + e w.
            var degree = above.Keys.Max()!.ToInt32Checked() / 2;
            var inW = new Entity[degree + 1];
            for (var k = 0; k <= degree; k++)
                inW[k] = above.TryGetValue(EInteger.FromInt32(2 * k), out var at) ? at : Number.Integer.Zero;
            Entity polynomialPart = Number.Integer.Zero;
            for (var k = degree; k >= 2; k--)
            {
                var lead = Functions.PartialFractions.InLowestTermsOverTheSymbols(inW[k] / c);
                if (lead == Number.Integer.Zero)
                    continue;
                polynomialPart += lead * MathS.Pow(x, 2 * (k - 2));
                inW[k - 1] = Functions.PartialFractions.InLowestTermsOverTheSymbols(inW[k - 1] - lead * b);
                inW[k - 2] = Functions.PartialFractions.InLowestTermsOverTheSymbols(inW[k - 2] - lead * a);
            }
            var d = inW[0];
            var e = degree >= 1 ? inW[1] : Number.Integer.Zero;

            var q = MathS.Sqrt(discriminant);
            var firstRoot = (-b + q) / (2 * c);
            var secondRoot = (-b - q) / (2 * c);
            Entity total = Number.Integer.Zero;
            if (polynomialPart != Number.Integer.Zero)
            {
                if (Integration.ComputeIndefiniteIntegral(polynomialPart, x, integrateByParts) is not { } whole)
                    return null;
                total += whole;
            }
            // `1/(x^2 - r)` is `atan(x/s)/s` with `s = sqrt(-r)` for every complex `r` but zero:
            // `d/dx atan(x/s)/s` is `1/(s^2 + x^2)` whatever `s` is, and for a positive `r` the
            // arctangent of an imaginary argument is the hyperbolic one, `-atanh(x/sqrt(r))/sqrt(r)`,
            // with a constant imaginary part on each side of the poles that a derivative does not
            // see. The roots are conjugate where the discriminant is negative -- the ordinary
            // case with real coefficients -- and the two terms then sum to a real function; a
            // piecewise on the sign of a root, as the quadratic rule would write, has no
            // value there at all.
            foreach (var (root, sign) in new[] { (firstRoot, 1), (secondRoot, -1) })
            {
                var coefficient = ((d + e * root) / q).InnerSimplified;
                if (coefficient == Number.Integer.Zero)
                    continue;
                var s = MathS.Sqrt(-root);
                var part = MathS.Arctan(x / s) / s;
                total += sign == 1 ? coefficient * part : -coefficient * part;
            }
            return total;
        }

        /// <summary>
        /// A product of powers of the variable and of constant multiples of it,
        /// <c>(c x)^m (d x)^k x^n</c> times a constant, with at least one written multiple:
        /// <c>(c x)^m (d x)^k x^(n+1)/(m + k + n + 1)</c>, and the logarithm where the sum of
        /// the exponents is minus one.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The written power is kept as it is, which is what makes this exact: for every
        /// <c>x</c> but zero, <c>d/dx (c x)^m</c> is <c>m (c x)^m / x</c>, since
        /// <c>(c x)^(m-1)</c> is <c>(c x)^m / (c x)</c> whatever the branch, and each factor
        /// contributes its exponent over <c>x</c>. Opening <c>(c x)^m</c> into <c>c^m x^m</c>
        /// would need <c>c</c> or <c>x</c> positive. Rubi's <c>(e x)^m (a + b x^n)^p (c + d x^n)^q</c>
        /// with symbolic <c>m</c> and <c>n</c>, expanded, is a sum of these and was declined
        /// term by term, the power rule reading <c>x^p</c> and nothing else.
        /// </para>
        /// <para>
        /// A power of <c>x</c> alone is left to the power rule, which answers it as before.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveAProductOfPowersOfTheVariable(Entity expr, Entity.Variable x)
        {
            Entity constant = Number.Integer.One;
            Entity written = Number.Integer.One;
            Entity plain = Number.Integer.Zero;
            Entity exponents = Number.Integer.Zero;
            var multiples = 0;
            foreach (var factor in Mulf.LinearChildren(expr))
            {
                if (!factor.ContainsNode(x))
                {
                    constant *= factor;
                    continue;
                }
                if (factor == x)
                {
                    plain += Number.Integer.One;
                    continue;
                }
                if (factor is not Powf(var @base, var power) || power.ContainsNode(x))
                    return null;
                // A whole power of a power is the power of the product of the exponents,
                // exactly: `(x^n)^2` is `x^n x^n`. The expansion of `(a + b x^n)^2` writes it so.
                while (power is Number.Integer && @base is Powf(var inner, var innerPower) && !innerPower.ContainsNode(x))
                {
                    power = (innerPower * power).InnerSimplified;
                    @base = inner;
                }
                if (@base == x)
                {
                    plain += power;
                    continue;
                }
                if (@base is not Mulf(var left, var right)
                    || !(left == x && !right.ContainsNode(x) || right == x && !left.ContainsNode(x)))
                    return null;
                written *= MathS.Pow(@base, power);
                exponents += power;
                multiples++;
            }
            if (multiples == 0)
                return null;
            // Where the exponents sum to minus one the integrand is `K/x` for the constant
            // `K = (c x)^m (d x)^k x^(n+1)`, whose derivative is `K (m + k + n + 1)/x`, zero.
            // Simplified rather than normalised, since the sum is of symbols that cancel:
            // `(c x)^m / x^(m+1)` has `m - (m + 1) + 1`, which the normalisation leaves as
            // written and which would have gone on to divide by itself.
            var raised = Functions.PartialFractions.Bare((plain + 1).Simplify());
            var overAll = Functions.PartialFractions.Bare((exponents + raised).Simplify());
            if (overAll == Number.Integer.Zero || overAll.Evaled is Number.Complex { IsZero: true })
                return constant * written * MathS.Pow(x, raised) * IntegralPatterns.AntiderivativeLog(x);
            return constant * written * MathS.Pow(x, raised) / overAll;
        }

        /// <summary>
        /// <c>x^p</c> for a <paramref name="power"/> that does not hold the variable: the power
        /// rule, or the logarithm where the power rule would divide by zero.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>The exponent is normalised before it is read.</b> <c>x^(-3 + 1 + 1)</c> is
        /// <c>x^(-1)</c>, but the sum is not the integer, so comparing the written form against
        /// <c>-1</c> missed the logarithmic case and applied the power rule to it — producing
        /// <c>x^(-3 + 1 + 1 + 1)/(-3 + 1 + 1 + 1)</c>, which is <c>x^0/0</c>, which is
        /// <c>NaN</c>. That is a claim that no antiderivative exists where one plainly does, and
        /// it reached the caller: <c>(a^2 + 2abx^2 + b^2x^4)^3/x^7</c> came back <c>NaN + C</c>
        /// while the same integrand written <c>(a + bx^2)^6/x^7</c> was answered.
        /// https://github.com/asc-community/AngouriMath/issues/1258
        /// </para>
        /// <para>
        /// <b>And normalised again on the way out</b>, which is the other half and the half that
        /// explains where such an exponent comes from. This rule used to return <c>p + 1</c>
        /// standing as a sum, so an answer handed back in as an integrand — which is exactly what
        /// repeated integration by parts does — accumulated one <c>+ 1</c> per round until a
        /// round landed on <c>-1</c> spelled as a sum. The rule was feeding itself the one input
        /// it could not read.
        /// </para>
        /// <para>
        /// A <b>symbolic</b> exponent is left to the power rule as before. <c>int x^n dx</c> is
        /// <c>x^(n+1)/(n+1)</c> for every <c>n</c> but <c>-1</c>, and this does not decide
        /// whether an undecidable <c>n</c> is that one; what it fixes is an exponent that
        /// <em>is</em> decidable and was read as though it were not.
        /// </para>
        /// </remarks>
        private static Entity IntegrateAPowerOfTheVariable(Entity @base, Entity power, Entity.Variable x)
        {
            var exponent = power.InnerSimplified;
            if (exponent == -1)
                return IntegralPatterns.AntiderivativeLog(@base);
            var raised = (exponent + 1).InnerSimplified;
            return MathS.Pow(x, raised) / raised;
        }

        /// <summary>
        /// Whether <paramref name="factor"/> is one of the functions integration by parts
        /// differentiates rather than integrates when the other factor is a polynomial.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the <b>L</b> and the <b>I</b> of LIATE, which both come before <b>A</b>. The
        /// rule below implemented only the L, and the reason it gives for the logarithm is the
        /// same one that holds for an inverse trigonometric function: differentiating it turns it
        /// into something algebraic that cancels against the integrated polynomial and ends,
        /// where integrating it puts the original integral back in front of us.
        /// </para>
        /// <para>
        /// <c>x * atan(x)</c> is the case: differentiating <c>atan</c> gives <c>1/(1 + x^2)</c>
        /// and what is left is <c>x^2/(2(1 + x^2))</c>, which is answered; integrating it first
        /// gives an antiderivative holding <c>x*atan(x)</c> again, which is where the search went
        /// instead and why it was left unevaluated.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        private static bool IsDifferentiatedBeforeAPolynomial(Entity factor)
            => factor switch
            {
                Logf or Entity.Arcsinf or Entity.Arccosf
                    or Entity.Arctanf or Entity.Arccotanf
                    or Entity.Arcsecantf or Entity.Arccosecantf => true,
                // And a whole power of one, which is the same function for this purpose:
                // differentiating ln(x)^2 gives 2ln(x)/x, whose x cancels against the integrated
                // polynomial exactly as ln(x)'s does, leaving x*ln(x) -- one step simpler, and
                // answered by another round of parts.
                //
                // A positive whole exponent, because that is when differentiating lowers the
                // power by one and the descent is finite. A negative or fractional one is a
                // different integrand with no reason to be the factor differentiated.
                Powf(var repeated, Number.Integer power) when power.EInteger.Sign > 0
                    => IsDifferentiatedBeforeAPolynomial(repeated),
                _ => false
            };

        internal static Entity? SolveIntegratingByParts(Entity expr, Entity.Variable x)
        {
            // The measure the nested call below decreases on. Read once, since every step
            // compares against the integrand this call started from rather than against its own
            // immediate predecessor -- what has to shrink is the distance to an answer, and a
            // chain of steps that each shrink by nothing would otherwise be admitted one at a
            // time.
            var wholeSize = expr.Nodes.Count();
            // And the second component of that measure: the highest power of a logarithm or
            // inverse trigonometric factor in the integrand. `x*arctan(x)^2` leaves
            // `x^2*arctan(x)/(1 + x^2)` after one step, which has more nodes and needs one more
            // round of parts to finish, so on node count alone it was declined; on the power it
            // is one step smaller, 2 to 1, and that is the descent the step made. Ordered with
            // the power first: a step that lowers it may grow the expression, and a step that
            // keeps it must shrink the expression, so the pair is well-founded and the runaway
            // the node count was put in for is still stopped by it -- an integrand with no such
            // factor has power zero on both sides and is measured by nodes alone as before.
            // https://github.com/asc-community/AngouriMath/issues/718
            var wholePower = HighestDifferentiatedPower(expr);

            // Standard integration by parts for polynomial × function
            // The antiderivative of u is fixed up to a constant, and the constant is chosen so
            // that what v' divides by divides it: against `arctan(x)^2 ln(1 + x^2)` the
            // antiderivative of `x` is `(1 + x^2)/2` and not `x^2/2`, and the remainder is
            // `arctan(x) ln(1 + x^2) + x arctan(x)^2`, two products each answered by parts, where
            // with `x^2/2` it was two quotients by `1 + x^2` that are elementary only together.
            // Rubi's `x^m` rules do the same. For a polynomial antiderivative and a polynomial
            // divisor, where the division leaves a constant.
            static Entity WithTheConstantMatchedTo(Entity antiderivative, Entity derivativeOfV, Variable x)
            {
                if (!TreeAnalyzer.TryGetPolynomial(antiderivative, x, out _))
                    return antiderivative;
                var divisors = Sumf.LinearChildren(Functions.PartialFractions.Bare(derivativeOfV))
                    .Select(term => Functions.SingleQuotient.Of(term).Denominator)
                    .Where(below => below.ContainsNode(x) && TreeAnalyzer.TryGetPolynomial(below, x, out _))
                    .Distinct().ToList();
                if (divisors.Count != 1)
                    return antiderivative;
                if (TreeAnalyzer.PolynomialLongDivision(antiderivative, divisors[0], genericCase: true, inTermsOf: x) is var (_, leftOver)
                    && Functions.SingleQuotient.Of(leftOver) is var (leftOverTop, _) && !leftOverTop.ContainsNode(x)
                    && leftOverTop.Evaled is Number.Complex { IsZero: false } constant)
                    return (antiderivative + (-constant).Evaled).InnerSimplified;
                return antiderivative;
            }

            static Entity? IntegrateByPartsPolynomial(Entity polynomialToDifferentiate, Entity toIntegrate, Variable x, int currentRecursion = 0)
            {
                if (polynomialToDifferentiate == 0) return 0;
                if (currentRecursion == MathS.Settings.MaxExpansionTermCount) return null;

                // A power of the sine times a power of the cosine goes to its closed rule
                // directly, since that rule answers only at the top and this is one level
                // down: `u sin(u)^3/cos(u)^2` is one step of parts against `sin^3/cos^2`, which
                // the rule answers at once and the chain below it spent nine seconds declining
                // through the half-angle substitution.
                var integral = SolveByTrigonometricPowerSubstitution(toIntegrate, x, asked: true)
                    ?? Integration.ComputeIndefiniteIntegral(toIntegrate, x, false);
                if (integral is null) return null;
                var differential = polynomialToDifferentiate.Differentiate(x);
                var result = IntegrateByPartsPolynomial(differential, integral, x, currentRecursion + 1);
                return (result is null) ? null : polynomialToDifferentiate * integral - result;
            }

            // Generalized integration by parts: tries once with v and u both being integrable
            // ∫ v·u dx = v·∫u dx - ∫(v'·∫u dx) dx
            // Only attempts if both v and u can be integrated
            static Entity? TryIntegrateByPartsOnce(Entity v, Entity u, Variable x, int wholeSize, int wholePower)
            {
                // Try to integrate u -- inner-simplified, as the public entry simplifies what
                // the integrator returns: the table answers `x/sqrt(1 + x^2)` as a piecewise on
                // a coefficient's sign whose conditions are decided, `0 = 0`, and a remainder
                // holding that piecewise is read by nothing.
                var integralOfU = Integration.ComputeIndefiniteIntegral(u, x, false)?.InnerSimplified;
                if (integralOfU is null) return null;

                // Differentiate v
                var derivativeOfV = v.Differentiate(x).InnerSimplified;
                if (derivativeOfV is Providedf(var inner, _)) derivativeOfV = inner; // TODO: signularities ignored but not handled properly
                if (derivativeOfV == Integer.Zero)
                    return v * integralOfU; // If v is constant, we're done
                // The antiderivative of u is fixed up to a constant, and the constant is
                // chosen so that what v' divides by divides it: against `arctan(x)^2 ln(1 + x^2)`
                // the antiderivative of `x` is `(1 + x^2)/2` and not `x^2/2`, and the remainder is
                // `arctan(x) ln(1 + x^2) + x arctan(x)^2`, two products each answered by parts,
                // where with `x^2/2` it was two quotients by `1 + x^2` that are elementary only
                // together. Rubi's `x^m` rules do the same.
                integralOfU = WithTheConstantMatchedTo(integralOfU, derivativeOfV, x);

                // Try to integrate the remaining term: v' · ∫u dx
                var remaining = (derivativeOfV * integralOfU).Simplify(1);
                if (remaining is Providedf(var inner_, _)) remaining = inner_; // TODO: signularities ignored but not handled properly
                // **By parts is allowed on what is left**, and that is the whole of this change.
                // One step of parts often leaves an integral that needs another: `x * ln(x)^2`
                // leaves `x * ln(x)`, which is answered by parts and by nothing else, so with
                // parts switched off here the outer integral was declined for want of the inner
                // one. Every power of a logarithm times a polynomial was out of reach that way.
                //
                // It was switched off because this recursion used to run away -- `x * ln(x)`
                // went round until the stack ran out. What stopped that was choosing the right
                // factor to differentiate (the L-before-A of LIATE, above), and since then the
                // integrator has grown two bounds that hold whatever the rules do: a set of
                // integrands already on the stack, which declines a repeat immediately, and a
                // ceiling on the depth of the descent. Those are what makes this safe now, and
                // they are checked on every entry rather than trusted to a flag.
                // https://github.com/asc-community/AngouriMath/issues/718
                // ...but only where what is left is **strictly smaller** than what we started
                // with. That is the decrease that makes the descent finite, and it is also what
                // keeps the search from widening: `sin(x)/(x^2 + 1)^2` has no elementary
                // antiderivative, so every solver runs to exhaustion on it, and letting parts
                // recurse without a measure took it from about a second to 83 -- caught by
                // `AnIntegralWithNoAnswerStillFinishesQuickly`, which exists for exactly this.
                // A power of a logarithm does decrease: `x * ln(x)^2` leaves `x * ln(x)`.
                // The power admits a larger remainder only while a factor is left for the next
                // round to differentiate. Once it has fallen to zero the remainder is whatever
                // the derivative and the integral made together -- for `asec(x)^2` times a
                // radical, a hundred-node radical expression -- and a by-parts search on that
                // is the runaway the node bound exists for: measured at thirty seconds for a
                // decline, where the node bound alone declined it in one.
                var remainingPower = HighestDifferentiatedPower(remaining);
                var partsOnTheRemainder = remaining.Nodes.Count() < wholeSize
                    || (remainingPower >= 1 && remainingPower < wholePower);
                // Spelled as the product its own next step reads, where there is one. `Simplify`
                // writes `2 arctan(x)/(1 + x^2) * x^2/2` as `arctan(x) x^2/(x^2 + 1)`, a
                // quotient, and this rule runs on a product. `x*arctan(x)^2` is answered by
                // exactly this and by nothing else.
                //
                // Only where what is left beside the factor is **rational** in the variable,
                // which is where the next round is a division and a table lookup. With a
                // radical there instead -- `asec(x)^2` times `(x^2 - 1)^(3/2)/x^5` -- the
                // re-spelled remainder handed a by-parts search a hundred-node radical
                // expression at every level below, and a one-second decline became thirty.
                if (partsOnTheRemainder
                    && TryRegroupAroundTheDifferentiatedFactor(remaining) is var (next, rest)
                    && next is not null && rest is not null
                    && IsRationalIn(rest, x))
                    remaining = next * rest;
                // A remainder that is a rational function of x and one root of a quadratic goes
                // to Euler's substitution directly, before the chain: `ln(x^2 + sqrt(1 - x^2))`
                // against 1 leaves exactly that, and the chain spent five seconds of
                // substitutions and reductions on it before Euler answered it in half of one.
                // From the top only: a nested step's remainder answered this way let a doomed
                // search above it carry on, thirty seconds where the gate leaves it at one.
                var remainingIntegral = (Integration.AnsweringTheQuestionAsked ? SolveByEulerSubstitution(remaining, x) : null)
                    ?? Integration.ComputeIndefiniteIntegral(remaining, x, partsOnTheRemainder);
                if (remainingIntegral is null) return null;

                return v * integralOfU - remainingIntegral;
            }

            Entity? TrySplit(Entity f, Entity g)
            {
                // Case 0: a logarithm times a polynomial. Only one of the two orders
                // terminates. Differentiating the polynomial, which is what Case 1 does,
                // leaves the logarithm to be integrated, and the antiderivative of ln(x)
                // holds an x*ln(x) that puts the original integral back in front of us --
                // that is how integral(x * ln(x), x) recursed until the stack ran out.
                // Differentiating the logarithm instead turns it into 1/x, which cancels
                // against the integrated polynomial and ends. (The L-before-A of LIATE.)
                if (IsDifferentiatedBeforeAPolynomial(f) && MathS.TryPolynomial(g, x, out _)
                    && TryIntegrateByPartsOnce(f, g, x, wholeSize, wholePower) is { } logFirstF) return logFirstF;
                if (IsDifferentiatedBeforeAPolynomial(g) && MathS.TryPolynomial(f, x, out _)
                    && TryIntegrateByPartsOnce(g, f, x, wholeSize, wholePower) is { } logFirstG) return logFirstG;

                // Case 1: One term is polynomial - use recursive polynomial integration by parts
                if (MathS.TryPolynomial(f, x, out var fPoly)) return IntegrateByPartsPolynomial(fPoly, g, x);
                if (MathS.TryPolynomial(g, x, out var gPoly)) return IntegrateByPartsPolynomial(gPoly, f, x);

                // Case 2: Neither is polynomial - try single-step integration by parts
                // This handles cases like ln(abs(x)) × ln(abs(x))
                // Try both orderings: f as v, g as u OR g as v, f as u
                //
                // **Only with a factor worth differentiating** -- a logarithm or an inverse
                // function, whose derivative is algebraic, or an exponential, whose integral
                // is itself and which the cyclic cases need. With neither, the step trades one
                // product of transcendental factors for another of the same kind with a
                // derivative in it, and searching that is where a decline went to spend thirty
                // seconds: `-3 tan(x)/(4 sec(x)^2 + 5 tan(x)^2)` against `1/sin(x)^2`, both
                // integrable, neither the right thing to differentiate. Measured on the Rubi
                // sample with the restriction and without: the same answers, eight seconds less.
                bool IsAnExponential(Entity factor)
                    => factor is Powf(var @base, var power) && !@base.ContainsNode(x) && power.ContainsNode(x);
                if (!IsDifferentiatedBeforeAPolynomial(f) && !IsDifferentiatedBeforeAPolynomial(g)
                    && !IsAnExponential(f) && !IsAnExponential(g))
                    return null;
                if (TryIntegrateByPartsOnce(f, g, x, wholeSize, wholePower) is { } result1) return result1;
                if (TryIntegrateByPartsOnce(g, f, x, wholeSize, wholePower) is { } result2) return result2;
                return null;
            }

            if (expr is Entity.Mulf(var f, var g))
            {
                if (TrySplit(f, g) is { } atTheTop) return atTheTop;

                // **And the same again with the factors regrouped.** The split above is the top
                // `Mulf` node's two children, which is a fact about how the product was written
                // rather than about the integrand: `x * cos(x) * sin(x)` parses left-associated,
                // so the two children are `x * cos(x)` and `sin(x)` -- neither a polynomial, and
                // none of the three cases above finds anything. `x * (cos(x) * sin(x))` is the
                // same function with the same factors, and it came out, because there the
                // polynomial is a child.
                //
                // So the factors are gathered and cut once more, polynomial on one side and the
                // rest on the other, which is the split every one of these cases is looking for
                // and the only one associativity can hide. Tried second, so an integrand that
                // was answered before is answered the same way.
                // https://github.com/asc-community/AngouriMath/issues/718
                if (TryRegroupAroundThePolynomial(expr, x) is var (polynomialPart, restPart)
                    && polynomialPart is not null && restPart is not null
                    && TrySplit(polynomialPart, restPart) is { } regrouped)
                    return regrouped;
            }

            // **A polynomial over something else is the polynomial times its reciprocal**, and
            // Case 1 reads a product: `x/cos(x)^2` reached nothing where `x*sec(x)^2` is one
            // step of parts, and it is what `arcsin(x)/sqrt(1 - x^2)^3` becomes under
            // `x = sin(u)`. Asked, not volunteered, like the regrouping below and for the same
            // reason: the reciprocal opens a search of its own.
            // Not over an algebraic function: a polynomial over a radical or over another
            // polynomial is the rational integrator's or Euler's, both of which have already
            // declined it by the time this runs, and a step of parts against it opens their
            // search again a level down -- one second more, measured, on a decline.
            if (Integration.AnsweringTheQuestionAsked
                && expr is Divf(var above, var below) && below.ContainsNode(x) && !IsAlgebraicIn(below, x)
                && TryRegroupAroundThePolynomialOverTheBar(above, below, x) is var (polynomialAbove, restOfTheQuotient)
                && polynomialAbove is not null && restOfTheQuotient is not null
                && TrySplit(polynomialAbove, restOfTheQuotient) is { } overTheBar)
                return overTheBar;

            // **And once more around the factor LIATE differentiates first, wherever it sits.**
            // The block above only runs on a product, so `arcsin(x)/(x^2*sqrt(1 - x^2))` never
            // reached this rule at all -- its top node is a quotient. Written as the product it
            // is, `arcsin(x) * (1/(x^2*sqrt(1 - x^2)))`, the same integrand came out. A quotient
            // is a product with reciprocals in it, and reading it as one is the whole of this.
            //
            // The cut is the logarithm or inverse trigonometric factor against everything else,
            // because that is the factor whose derivative is algebraic and cancels; taking it
            // out of the middle of a product is the same defect as taking the polynomial out,
            // with a different factor doing the work. `x*arcsin(x)/sqrt(1 - x^2)` needs both
            // moves at once: read as a product, and cut at the arcsine rather than at the top.
            //
            // **Two bounds, and both were measured rather than added for safety.** Without them
            // this is worth six more answers on the Rubi sample and forty per cent of its wall
            // clock, which is what https://github.com/asc-community/AngouriMath/issues/1265 is
            // about. With them it is worth the same six answers and three per cent.
            //
            // The first is scope: asked, not volunteered. The split is offered only for the
            // caller's own integrand, never for one another rule produced on the way somewhere.
            // A by-parts step produces quotients holding logarithms by the dozen, and each of
            // those opening a fresh by-parts attempt is where the time went --
            // `cos(x)^3*ln(sin(x))` is answered in 1.7 s by master and took 45 s without this,
            // although the rule never fires on it at the top at all.
            //
            // The second is in the regrouping: exactly one factor to differentiate, never two.
            if (Integration.AnsweringTheQuestionAsked
                && TryRegroupAroundTheDifferentiatedFactor(expr) is var (differentiated, others)
                && differentiated is not null && others is not null
                && TrySplit(differentiated, others) is { } byLiate)
                return byLiate;

            // **Two factors to differentiate, taken together**, against an algebraic rest whose
            // integral is closed: `x arctan(x) ln(x + sqrt(1 + x^2))/sqrt(1 + x^2)` is the product
            // of the two transcendental factors against `x/sqrt(1 + x^2)`, whose integral is
            // `sqrt(1 + x^2)`, and the remainder `sqrt(1 + x^2) (f g)'` is `arctan(x)` plus
            // `ln(x + sqrt(1 + x^2))/sqrt(1 + x^2)`, each with one such factor and each answered.
            // The regrouping above refuses two for the reason in its remark -- choosing one
            // to differentiate is a guess, and the remainder still holds the other -- which
            // taking both together does not have: the product's derivative has one factor
            // per term. Only against an algebraic rest, which is where the integral of the
            // rest is closed and the remainder is smaller than what it came from.
            // The remainder's terms are asked at the top, each being a term of a sum asked at
            // the top and each holding one transcendental factor where the integrand held two:
            // `arctan(x)` alone is by parts against one, which answers only at the top.
            if (Integration.AnsweringTheQuestionAsked
                && TryRegroupAroundBothDifferentiatedFactors(expr) is var (bothOfThem, algebraicRest)
                && bothOfThem is not null && algebraicRest is not null
                && IsAlgebraicIn(algebraicRest, x)
                && Integration.ComputeIndefiniteIntegral(algebraicRest, x, false)?.InnerSimplified is { } integralOfTheRest)
            {
                // The derivative as differentiation writes it: simplified first, `1 + x^2` is
                // `x^2 + 1`, a spelling the substitution for `ln(x + sqrt(1 + x^2))` then did not
                // read against the `sqrt(1 + x^2)` beside it.
                var derivativeOfBoth = bothOfThem.Differentiate(x);
                integralOfTheRest = WithTheConstantMatchedTo(integralOfTheRest, derivativeOfBoth, x);
                var remaining = (derivativeOfBoth * integralOfTheRest).Simplify(1);
                if (remaining is Providedf(var bareRemaining, _))
                    remaining = bareRemaining;
                // Distributed over the sum the product rule left -- the derivative of the pair
                // is a sum and the integral of the rest multiplies it -- and no further: written
                // out in full, `arctan(x) (1 + x/sqrt(x^2 + 1))` comes apart into two terms
                // neither of which is elementary, where together they are `arctan(x)`.
                var terms = DistributedOverTheSum(remaining);
                Entity? total = null;
                foreach (var written in terms)
                {
                    // With the radicals differentiation left beside each other combined and
                    // cancelled first -- `(sqrt(x^2 + 1) + x)/(x + sqrt(x^2 + 1))` is one, and a
                    // term carrying it was thirty seconds of search where `arctan(x)` alone is
                    // a step of parts.
                    var term = CombineRadicalsIn(written, x);
                    if (Integration.ComputeAsAQuestionOfItsOwn(term, x, integrateByParts: true) is not { } termIntegral)
                    {
                        total = null;
                        break;
                    }
                    total = total is null ? termIntegral : total + termIntegral;
                }
                if (total is not null)
                    return bothOfThem * integralOfTheRest - total;
            }

            // **A bare logarithm or inverse function of something that is not linear**, by
            // parts against 1: `int f(g) dx = x f(g) - int x g' f'(g) dx`, and the remainder is
            // algebraic. A linear argument is the table's; anything else reached no rule at all,
            // since this one runs on a product and the integrand is a single node.
            // `arctan(x sqrt(1 - x^2))` had no antiderivative and is one step of this.
            // Asked, not volunteered, like the regrouping above: the remainder can be a radical
            // the search spends seconds on, and a rule that produced this shape on its way
            // somewhere is not owed that search.
            // And with two such factors, one of them differentiated and the other integrated
            // beside the rest, either way round: `x ln(1 + x^2) arctan(x)` is not elementary
            // by the two together -- the remainder holds `ln(1 + x^2)/(1 + x^2)`, which is not
            // -- and is by parts against `x ln(1 + x^2)`, whose integral is a substitution,
            // with the arctangent differentiated: the remainder is then rational and a
            // logarithm over a quadratic. Measured on the total power, which the step lowers.
            if (Integration.AnsweringTheQuestionAsked
                && TryRegroupAroundBothDifferentiatedFactors(expr) is var (twoOfThem, restBesideThem)
                && twoOfThem is not null && restBesideThem is not null)
                foreach (var differentiatedOne in Mulf.LinearChildren(twoOfThem))
                {
                    var integratedOne = Mulf.LinearChildren(twoOfThem).First(factor => factor != differentiatedOne);
                    var integrated = restBesideThem == Integer.One ? integratedOne : integratedOne * restBesideThem;
                    if (TryIntegrateByPartsOnce(differentiatedOne, integrated, x, wholeSize, wholePower) is { } oneOfTheTwo)
                        return oneOfTheTwo;
                }

            if (Integration.AnsweringTheQuestionAsked && IsDifferentiatedBeforeAPolynomial(expr)
                && expr is not Powf
                && !(expr.DirectChildren.LastOrDefault() is { } argument && TreeAnalyzer.TryGetPolyLinear(argument, x, out _, out _))
                && TryIntegrateByPartsOnce(expr, Integer.One, x, wholeSize, wholePower) is { } againstOne)
                return againstOne;

            // Special case for powers of integrable functions, try integration by parts on base × base
            // e.g., ln(abs(x))^2 = ln(abs(x)) × ln(abs(x))
            if (expr is Powf(var @base, Integer(2)) && TryIntegrateByPartsOnce(@base, @base, x, wholeSize, wholePower) is { } result) return result;

            return null;
        }

        /// <summary>
        /// Whether <paramref name="expr"/> is built from <paramref name="x"/> by the four
        /// operations and rational powers alone: no function of <paramref name="x"/> anywhere.
        /// </summary>
        private static bool IsAlgebraicIn(Entity expr, Entity.Variable x)
            => expr.Nodes.All(node =>
                !node.ContainsNode(x)
                || node is Variable or Sumf or Minusf or Mulf or Divf
                || node is Powf(_, var power) && power is Number.Rational);

        /// <summary>
        /// Whether <paramref name="expr"/> is a quotient of polynomials in <paramref name="x"/>,
        /// read in either spelling a quotient has here.
        /// </summary>
        private static bool IsRationalIn(Entity expr, Entity.Variable x)
        {
            if (!TryReadAsQuotient(expr, out var above, out var below))
                return TreeAnalyzer.TryGetPolynomial(expr, x, out _);
            return TreeAnalyzer.TryGetPolynomial(above, x, out _) && TreeAnalyzer.TryGetPolynomial(below, x, out _);
        }

        /// <summary>
        /// The total power of the factors <see cref="IsDifferentiatedBeforeAPolynomial"/>
        /// recognises among the factors of <paramref name="expr"/> -- of each term of it, the
        /// largest, where it is a sum or a sum over a denominator -- or zero where there is
        /// none: the first component of the measure integration by parts descends on.
        /// </summary>
        /// <remarks>
        /// The total and not the highest: <c>arctan(x) ln(1 + x^2)</c> is two, and one step of
        /// parts leaves <c>2x^2 arctan(x)/(1 + x^2) - x ln(1 + x^2)/(1 + x^2)</c>, each term of
        /// which is one -- the step differentiated one of the two away, and that is the
        /// descent it made, where the highest power read one on both sides and declined the
        /// larger remainder. A step that lowers the total may grow the expression, and a
        /// step that keeps it must shrink the expression, as before.
        /// </remarks>
        private static int HighestDifferentiatedPower(Entity expr)
        {
            var (numerator, denominator) = expr is Divf(var top, var bottom) ? (top, bottom) : (expr, Number.Integer.One as Entity);
            var highest = 0;
            foreach (var term in Sumf.LinearChildren(numerator))
            {
                var total = 0;
                foreach (var (factor, underneath) in FactorsOfTheIntegrand(denominator == Number.Integer.One ? term : term / denominator))
                {
                    if (underneath || !IsDifferentiatedBeforeAPolynomial(factor))
                        continue;
                    total += factor is Powf(_, Number.Integer exponent) && exponent.EInteger.CanFitInInt32()
                        ? exponent.EInteger.ToInt32Unchecked()
                        : 1;
                }
                if (total > highest)
                    highest = total;
            }
            return highest;
        }

        /// <summary>
        /// The factors of <paramref name="expr"/> read as a product — a quotient contributing its
        /// denominator's factors as reciprocals — cut into the one
        /// <see cref="IsDifferentiatedBeforeAPolynomial"/> recognises and everything else.
        /// </summary>
        /// <remarks>
        /// Both halves <see langword="null"/> when there is no such factor, or when it is the
        /// whole integrand and there is nothing to put on the other side. One factor is taken,
        /// not all of them: it is the one that gets differentiated, and differentiating a product
        /// of two logarithms is not a step towards anything.
        /// </remarks>
        private static (Entity? Differentiated, Entity? Others) TryRegroupAroundTheDifferentiatedFactor(Entity expr)
        {
            var factors = FactorsOfTheIntegrand(expr);
            if (factors.Count < 2)
                return (null, null);

            // Exactly one, and that is a bound rather than a tidiness. With two logarithms or
            // inverse trigonometric factors there is no reason to differentiate one rather than
            // the other, so the choice is a guess -- and what is left still holds the other one,
            // so the step has not made the problem smaller. Measured on
            // `x*arctan(x)*ln(x + sqrt(1 + x^2))/sqrt(1 + x^2)`: declined in 66 ms without this
            // rule and in 39 s with it choosing one of the two.
            if (factors.Count(pair => !pair.Underneath && IsDifferentiatedBeforeAPolynomial(pair.Factor)) != 1)
                return (null, null);

            Entity? differentiated = null;
            Entity? above = null;
            Entity? below = null;
            foreach (var (factor, underneath) in factors)
                if (differentiated is null && !underneath && IsDifferentiatedBeforeAPolynomial(factor))
                    differentiated = factor;
                else if (underneath)
                    below = below is null ? factor : below * factor;
                else
                    above = above is null ? factor : above * factor;

            if (differentiated is null || (above is null && below is null))
                return (null, null);
            // **Put the rest back together as one quotient**, numerator over denominator, rather
            // than as a product of reciprocals. Which of the two it is decides whether the rules
            // below read it: `1/(x^2*sqrt(1 - x^2))` is answered and
            // `(1/x^2) * (1/sqrt(1 - x^2))` is not, and handing on the second shape means this
            // rule reproduces, inside itself, exactly the spelling defect it exists to remove.
            var others = below is null ? above! : above is null ? 1 / below : above / below;
            return (differentiated, others);
        }

        /// <summary>
        /// <paramref name="expr"/> as the terms of its top-level sum, with a product of a sum
        /// and other factors distributed one level: <c>(a + b) c</c> is <c>a c</c> and <c>b c</c>,
        /// and <c>a</c> is left as written.
        /// </summary>
        private static List<Entity> DistributedOverTheSum(Entity expr)
        {
            if (expr is Sumf or Minusf)
                return Sumf.LinearChildren(expr).ToList();
            var factors = Mulf.LinearChildren(expr).ToList();
            var sum = factors.FirstOrDefault(factor => factor is Sumf or Minusf);
            if (sum is null)
                return new List<Entity> { expr };
            Entity rest = Number.Integer.One;
            foreach (var factor in factors)
                if (!ReferenceEquals(factor, sum))
                    rest = rest * factor;
            return Sumf.LinearChildren(sum).Select(term => term * rest).ToList();
        }

        /// <summary>
        /// The two factors LIATE would differentiate, as one product, against the rest of the
        /// integrand as one quotient -- <c>1</c> where there is nothing beside them, since
        /// <c>arctan(x) ln(1 + x^2)</c> against <c>1</c> is one step of parts whose remainder,
        /// <c>x ln(1 + x^2)/(1 + x^2) + 2x^2 arctan(x)/(1 + x^2)</c>, is two terms each of which
        /// is answered on its own; <c>(null, null)</c> where there are not exactly two above
        /// the bar.
        /// </summary>
        private static (Entity? Both, Entity? Others) TryRegroupAroundBothDifferentiatedFactors(Entity expr)
        {
            var factors = FactorsOfTheIntegrand(expr);
            if (factors.Count < 2)
                return (null, null);
            if (factors.Count(pair => !pair.Underneath && IsDifferentiatedBeforeAPolynomial(pair.Factor)) != 2)
                return (null, null);
            Entity? both = null;
            Entity? above = null;
            Entity? below = null;
            foreach (var (factor, underneath) in factors)
                if (!underneath && IsDifferentiatedBeforeAPolynomial(factor))
                    both = both is null ? factor : both * factor;
                else if (underneath)
                    below = below is null ? factor : below * factor;
                else
                    above = above is null ? factor : above * factor;
            if (both is null)
                return (null, null);
            if (above is null && below is null)
                return (both, Integer.One);
            var others = below is null ? above! : above is null ? 1 / below : above / below;
            return (both, others);
        }

        /// <summary>
        /// <paramref name="expr"/> read as a product of factors, each paired with whether it sits
        /// under a division bar.
        /// </summary>
        /// <remarks>
        /// <c>a/(b*c)</c> comes back as <c>a</c> above and <c>b</c>, <c>c</c> below — which is
        /// what lets a rule that reasons about factors see through a quotient, where
        /// <see cref="Mulf.LinearChildren"/> stops at it. The flag is kept rather than folded
        /// into a reciprocal so that the caller can rebuild one quotient instead of a product of
        /// them.
        /// </remarks>
        private static List<(Entity Factor, bool Underneath)> FactorsOfTheIntegrand(Entity expr)
        {
            var factors = new List<(Entity, bool)>();
            Gather(expr, underneath: false);
            return factors;

            void Gather(Entity node, bool underneath)
            {
                switch (node)
                {
                    case Mulf(var left, var right):
                        Gather(left, underneath);
                        Gather(right, underneath);
                        break;
                    case Divf(var numerator, var denominator):
                        Gather(numerator, underneath);
                        Gather(denominator, !underneath);
                        break;
                    default:
                        factors.Add((node, underneath));
                        break;
                }
            }
        }

        /// <summary>
        /// The factors of <paramref name="expr"/> cut into the polynomial ones and the rest,
        /// where that cut is not the one the top <c>Mulf</c> node already makes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Both halves <see langword="null"/> when there is nothing new to offer: fewer than three
        /// factors, or all of them polynomial, or none of them. In those cases the top node's own
        /// split is either the same cut or the only one there is.
        /// </para>
        /// <para>
        /// A factor free of the variable counts as polynomial, since it is a constant here and
        /// belongs with the part that gets differentiated — where it survives one step and
        /// vanishes from the recursion, rather than riding along inside an integrand.
        /// </para>
        /// </remarks>
        private static (Entity? Polynomial, Entity? Remainder) TryRegroupAroundThePolynomial(Entity expr, Entity.Variable x)
        {
            var factors = Mulf.LinearChildren(expr).ToList();
            if (factors.Count < 3)
                return (null, null);

            Entity? polynomial = null;
            Entity? rest = null;
            foreach (var factor in factors)
                if (MathS.TryPolynomial(factor, x, out _) || !factor.ContainsNode(x))
                    polynomial = polynomial is null ? factor : polynomial * factor;
                else
                    rest = rest is null ? factor : rest * factor;
            return polynomial is null || rest is null ? (null, null) : (polynomial, rest);
        }

        /// <summary>
        /// The polynomial factors of a quotient's numerator against the rest of the quotient:
        /// <c>u sec(u)^2/tan(u)^2</c> is <c>u</c> and <c>sec(u)^2/tan(u)^2</c>. The polynomial
        /// has to be in <paramref name="x"/>, or there is nothing to differentiate.
        /// </summary>
        private static (Entity? Polynomial, Entity? Remainder) TryRegroupAroundThePolynomialOverTheBar(
            Entity above, Entity below, Entity.Variable x)
        {
            Entity? polynomial = null;
            Entity? rest = null;
            foreach (var factor in Mulf.LinearChildren(above))
                if (MathS.TryPolynomial(factor, x, out _) || !factor.ContainsNode(x))
                    polynomial = polynomial is null ? factor : polynomial * factor;
                else
                    rest = rest is null ? factor : rest * factor;
            if (polynomial is null || !polynomial.ContainsNode(x))
                return (null, null);
            return (polynomial, rest is null ? Integer.One / below : rest / below);
        }

        internal static Entity? SolveLogarithmic(Entity expr, Entity.Variable x, bool integrateByParts = true) => expr switch
        {
            Entity.Logf(var @base, var arg) =>
                @base.ContainsNode(x) ?
                    Integration.ComputeIndefiniteIntegral(MathS.Ln(arg) / MathS.Ln(@base), x, integrateByParts) :
                arg is Entity.Powf(var y, var pow) ? // log(b, y^p) = ln(y^p) / ln(b) = ln(p) / ln(b) * ln(y)
                    Integration.ComputeIndefiniteIntegral(pow / MathS.Ln(@base) * MathS.Ln(y), x, integrateByParts) :
                    SolveALogarithmOrAnArctangentOfAPolynomial(expr, x),

            Entity.Arctanf or Entity.Arccotanf => SolveALogarithmOrAnArctangentOfAPolynomial(expr, x),

            _ => null
        };

        /// <summary>
        /// The logarithm, arctangent or arccotangent of a polynomial in <paramref name="x"/>,
        /// by parts against one, closed: <c>x f(P) - int x f'(P) P'</c>, whose remainder is a
        /// rational function -- <c>x P'/P</c>, <c>x P'/(1 + P^2)</c> -- and goes to the rational
        /// integrator directly.
        /// </summary>
        /// <remarks>
        /// By parts against one is a rule of the top: a nested step's remainder answered that
        /// way let a doomed search above it carry on. So <c>ln(1 + x^2)</c> one level down had no
        /// antiderivative, and every remainder that held it -- what parts leaves from
        /// <c>arctan(x) ln(1 + x^2)</c> with the arctangent differentiated, for one -- was
        /// declined for want of it. This is the same step for the one shape whose remainder is
        /// closed, and it is closed itself, so it is answered at any depth.
        /// </remarks>
        private static Entity? SolveALogarithmOrAnArctangentOfAPolynomial(Entity expr, Entity.Variable x)
        {
            Entity argument;
            Entity derivativeOfTheFunction;
            switch (expr)
            {
                case Logf(var @base, var arg) when @base == MathS.e:
                    argument = arg;
                    derivativeOfTheFunction = 1 / arg;
                    break;
                case Arctanf(var arg):
                    argument = arg;
                    derivativeOfTheFunction = 1 / (1 + MathS.Sqr(arg));
                    break;
                case Arccotanf(var arg):
                    argument = arg;
                    derivativeOfTheFunction = -1 / (1 + MathS.Sqr(arg));
                    break;
                default:
                    return null;
            }
            // A linear argument is the table's, and answered more shortly there.
            if (!argument.ContainsNode(x) || !TreeAnalyzer.TryGetPolynomial(argument, x, out var read)
                || TreeAnalyzer.TryGetPolyLinear(argument, x, out _, out _) || read.Keys.Any(k => !k.CanFitInInt32()))
                return null;
            var remainder = Functions.PartialFractions.Bare(x * argument.Differentiate(x) * derivativeOfTheFunction);
            if (!TryReadAsQuotient(remainder, out var above, out var below)
                || !TreeAnalyzer.TryGetPolynomial(above, x, out _) || !TreeAnalyzer.TryGetPolynomial(below, x, out _))
                return null;
            var rational = above / below;
            var integratedRemainder = SolveByPartialFractions(rational, x, integrateByParts: false)
                ?? IntegralPatterns.TryStandardIntegrals(rational, x)
                ?? SolveByRothsteinTrager(rational, x);
            if (integratedRemainder is null)
                return null;
            var answer = (x * expr - integratedRemainder).InnerSimplified;
            return answer.Nodes.Any(node => node is Number.Complex { IsNaN: true }) ? null : answer;
        }

        internal static Entity? SolveExponential(Entity expr, Entity.Variable x, bool integrateByParts = true) => expr switch
        {
            Entity.Powf(var @base, var pow) =>
                @base.ContainsNode(x) ?
                    Integration.ComputeIndefiniteIntegral(MathS.Pow(MathS.e, MathS.Ln(@base) * pow), x, integrateByParts) :
                    null,

            _ => null
        };

        /// <summary>
        /// <c>(a + b sin(c x + d))^n</c> or the same with a cosine, for <c>a^2 = b^2</c> and
        /// <c>n</c> a positive half-integer, in closed form.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>sqrt(1 + sin(x))</c> had no antiderivative. It is <c>-2 cos(x)/sqrt(1 + sin(x))</c>:
        /// differentiating that gives <c>(2 sin (1 + sin) + cos^2)/(1 + sin)^(3/2)</c>, and with
        /// <c>cos^2 = 1 - sin^2 = (1 - sin)(1 + sin)</c> the numerator is <c>(1 + sin)^2</c>. That
        /// cancellation is what <c>a^2 = b^2</c> buys, and it is the whole of the rule.
        /// </para>
        /// <para>
        /// Above the base case, one step of by parts lowers <c>n</c> by one:
        /// </para>
        /// <code>
        ///     int (a + b sin)^n = -b cos (a + b sin)^(n-1)/(c n) + (a (2n - 1)/n) int (a + b sin)^(n-1)
        /// </code>
        /// <para>
        /// which is checked by differentiating the first term and using <c>b^2 cos^2 =
        /// (a - b sin)(a + b sin)</c>. Applied until <c>n</c> is <c>1/2</c>, so
        /// <c>(1 - sin(2x/3))^(5/2)</c> is two steps and a base. A cosine is the same rule
        /// with <c>sin</c> replaced by <c>-cos</c> in the first term, since <c>d cos = -sin</c>.
        /// </para>
        /// <para>
        /// <b>No condition is owed.</b> <c>a + b sin</c> is never negative when <c>a^2 = b^2</c>,
        /// so its square root is real wherever the integrand is, and the base-case answer is a
        /// single antiderivative across the zeros of <c>1 + sin</c>: both it and the integrand
        /// vanish there. <c>a^2 = b^2</c> is required decidably, so a symbolic <c>a</c> beside a
        /// numeric <c>b</c> is not this shape. Rubi's rule for the same case is the source.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveAHalfPowerOfOnePlusASine(Entity expr, Entity.Variable x)
        {
            if (expr is not Powf(var @base, Number.Rational exponent) || exponent is Number.Integer
                || !exponent.ERational.Denominator.Equals(EInteger.FromInt32(2)) || exponent.ERational.Sign <= 0)
                return null;
            // a + b f(u), for f a sine or cosine of something linear in x, read off the sum.
            Entity a = 0;
            Entity? b = null;
            Entity? argument = null;
            var isSine = false;
            foreach (var term in Sumf.LinearChildren(@base))
            {
                if (!term.ContainsNode(x))
                {
                    a += term;
                    continue;
                }
                Entity coefficient = 1;
                Entity? function = null;
                foreach (var factor in Mulf.LinearChildren(term))
                    if (!factor.ContainsNode(x))
                        coefficient *= factor;
                    else if (function is null && factor is Sinf or Cosf)
                        function = factor;
                    else
                        return null;
                if (function is null || b is not null)
                    return null;
                b = coefficient;
                isSine = function is Sinf;
                argument = function.DirectChildren.First();
            }
            if (b is null || argument is null)
                return null;
            if (!TreeAnalyzer.TryGetPolyLinear(argument, x, out var rate, out _)
                || rate.Evaled is Number.Complex { IsZero: true })
                return null;
            // a^2 = b^2, decided rather than assumed.
            if ((MathS.Sqr(a) - MathS.Sqr(b)).Evaled is not Number.Complex { IsZero: true })
                return null;

            var n = exponent.ERational;
            // The direction of the first by-parts term: b cos for a sine, -b sin for a cosine.
            var complement = isSine ? MathS.Cos(argument) : -MathS.Sin(argument);
            Entity Step(ERational power)
            {
                var previous = Number.Rational.Create(power.Subtract(ERational.One));
                var current = Number.Rational.Create(power);
                if (power.CompareTo(ERational.Create(1, 2)) == 0)
                    return -2 * b * complement / (rate * MathS.Sqrt(@base));
                return -b * complement * MathS.Pow(@base, previous) / (rate * current)
                    + a * (2 * current - 1) / current * Step(power.Subtract(ERational.One));
            }
            return Step(n).InnerSimplified;
        }

        /// <summary>
        /// A whole power of the secant or cosecant, brought down two at a time by the standard
        /// reduction until the power rule for the first or the zeroth takes over.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>1/cos(x)^2</c> was answered and <c>1/cos(x)^6</c> was not. The rule for powers of
        /// sine and cosine reads a <b>positive</b> exponent, so a negative one — which is what a
        /// secant or cosecant is — matched nothing above the square, and the square was answered
        /// only because it is a standard integral in its own right.
        /// </para>
        /// <para>
        /// This is Rubi's rule 4.5.1.1, ported:
        /// <code>
        /// Int[(b*csc[c+d*x])^n] := -b*Cos[c+d*x]*(b*Csc[c+d*x])^(n-1)/(d*(n-1))
        ///                          + b^2*(n-2)/(n-1)*Int[(b*Csc[c+d*x])^(n-2)]
        ///     /; GtQ[n,1] &amp;&amp; IntegerQ[2*n]
        /// </code>
        /// which for the secant reads
        /// <c>∫sec^n = sec^(n-2) tan/(a(n-1)) + ((n-2)/(n-1)) ∫sec^(n-2)</c>, and for the
        /// cosecant the same with <c>-cot</c> and the sign of the first term flipped. Each step
        /// takes two off the exponent, so it ends at <c>n = 1</c> — a standard integral here
        /// already — or at <c>n = 0</c>, which is <c>x</c>.
        /// </para>
        /// <para>
        /// <b>Read through both spellings.</b> A secant is <c>sec(u)</c>, and it is also
        /// <c>cos(u)^(-n)</c> and <c>1/cos(u)^n</c>; all three arrive here, and answering one of
        /// them and not the others is the defect this file has had five times over.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveBySecantPowerReduction(Entity expr, Entity.Variable x)
        {
            if (!TryReadReciprocalTrigonometricPower(expr, out var argument, out var power, out var isSecant))
                return null;
            if (power < 2)
                return null;   // the first power and the zeroth are answered without a recursion
            // Asked, not volunteered. Answering a power of the secant that some other rule
            // produced on its way somewhere lets that rule's search carry on instead of
            // stopping, and that -- rather than any work this does -- is what took
            // `sec(x)^6*tan(x)^3` from a 637 ms decline to not returning in 400 s.
            // https://github.com/asc-community/AngouriMath/issues/1265
            if (!Integration.AnsweringTheQuestionAsked)
                return null;
            if (!TreeAnalyzer.TryGetPolyLinear(argument, x, out var rate, out _) || rate.Evaled == 0)
                return null;

            var reciprocal = isSecant ? MathS.Sec(argument) : MathS.Cosec(argument);
            var partner = isSecant ? MathS.Tan(argument) : MathS.Cotan(argument);

            // The recurrence run out here rather than through the integrator. Every step is
            // determined -- the power comes down by two and the coefficient is (n-2)/(n-1) --
            // so there is nothing to search for, and asking the integrator instead would put the
            // rule's own descent behind the gate above and stop it after one step.
            //
            //   sec^(n-2) tan / (a(n-1))  +  ((n-2)/(n-1)) * the same at n-2
            //  -csc^(n-2) cot / (a(n-1))  +  ((n-2)/(n-1)) * the same at n-2
            Entity total = 0;
            Entity carried = 1;
            var remaining = power;
            while (remaining >= 2)
            {
                var lowered = remaining - 2 == 1 ? reciprocal : MathS.Pow(reciprocal, remaining - 2);
                var boundary = lowered * partner / (rate * Number.Integer.Create(remaining - 1));
                if (!isSecant)
                    boundary = -boundary;
                total += carried * boundary;
                carried *= Number.Rational.Create(remaining - 2, remaining - 1);
                remaining -= 2;
            }

            // What the recurrence lands on: sec^1 and csc^1 are the standard integrals this
            // library already carries, and sec^0 is x. Written out rather than asked for, so
            // that the rule is closed over its own recursion.
            var rest = remaining == 0
                ? x
                : isSecant
                    ? MathS.Hyperbolic.Artanh(MathS.Sin(argument)) / rate
                    : IntegralPatterns.AntiderivativeLog(MathS.Tan(argument / 2)) / rate;

            return total + carried * rest;
        }

        /// <summary>
        /// An integrand that is a power of the sine times a power of the cosine, of one common
        /// argument linear in the variable, turned into a polynomial by whichever of
        /// <c>u = cos</c>, <c>u = sin</c> and <c>u = tan</c> the two exponents admit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Everything built from the six trigonometric functions is a <c>sin^p cos^q</c></b>,
        /// and reading it that way is what makes one rule out of a family Rubi spreads over
        /// several sections: <c>tan^m sec^n</c> is <c>sin^m cos^(-m-n)</c>, <c>cot^m csc^n</c> is
        /// <c>cos^m sin^(-m-n)</c>, a power of the secant alone is <c>cos^(-n)</c>. The exponents
        /// are integers of either sign, so the same three cases below cover all of them.
        /// </para>
        /// <para>
        /// <b>Which substitution, and why exactly these three.</b> Each one has to leave a
        /// <i>Laurent polynomial</i> — a sum of powers of <c>u</c>, integrated term by term —
        /// because that is what makes the rule closed. It asks the integrator nothing.
        /// </para>
        /// <list type="bullet">
        /// <item><description>
        /// <c>p</c> odd and at least one: <c>u = cos</c>, <c>du = -sin dx</c>, so one sine goes
        /// into <c>du</c> and <c>sin^(p-1)</c> is <c>(1 - u^2)^((p-1)/2)</c>. Leaves
        /// <c>-(1 - u^2)^((p-1)/2) u^q</c>, and <c>q</c> may be anything.
        /// </description></item>
        /// <item><description>
        /// <c>q</c> odd and at least one: <c>u = sin</c>, the mirror of it.
        /// </description></item>
        /// <item><description>
        /// both even and <c>p + q</c> at most <c>-2</c>: <c>u = tan</c>, under which
        /// <c>cos^2 = 1/(1 + u^2)</c> and <c>dx = du/(1 + u^2)</c>, leaving
        /// <c>u^p (1 + u^2)^(-(p+q)/2 - 1)</c> — a polynomial exactly when <c>p + q</c> is at
        /// most <c>-2</c>, which is the condition. <c>tan^2 sec^4</c> is this case and neither
        /// of the others.
        /// </description></item>
        /// </list>
        /// <para>
        /// <b>What is deliberately left out</b>: both exponents even with <c>p + q</c> at least
        /// zero, where the tangent substitution leaves a negative power of <c>1 + u^2</c> rather
        /// than a polynomial. Those are the ordinary <c>sin^2 cos^4</c> shapes, which the
        /// power-reduction rules already answer, so the boundary costs nothing. A power of the
        /// secant or cosecant alone reaches <see cref="SolveBySecantPowerReduction"/> first,
        /// which gives a shorter answer for it.
        /// </para>
        /// <para>
        /// <b>Asked, not volunteered</b>, for the reason in
        /// <a href="https://github.com/asc-community/AngouriMath/issues/1265">#1265</a>: a rule
        /// that answers a sub-integral which used to come back unanswered lets the search that
        /// asked for it carry on, and that cost lands on integrands the rule never fires on.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveByTrigonometricPowerSubstitution(Entity expr, Entity.Variable x, bool asked = false)
        {
            if (!asked && !Integration.AnsweringTheQuestionAsked)
                return null;
            if (!TryReadSineCosinePowers(expr, out var argument, out var sinePower, out var cosinePower, out var factor))
                return null;
            if (!TreeAnalyzer.TryGetPolyLinear(argument, x, out var rate, out _) || rate.Evaled == 0)
                return null;
            return IntegrateAPowerOfSineTimesAPowerOfCosine(argument, sinePower, cosinePower, factor, rate);
        }

        /// <summary>
        /// <c>int factor * sin(argument)^p cos(argument)^q dx</c>, where <paramref name="rate"/> is
        /// the derivative of the argument — the closed part of
        /// <see cref="SolveByTrigonometricPowerSubstitution"/>, separated so that a rule which
        /// <i>produces</i> such an integrand can use it without going back through the chain.
        /// </summary>
        /// <remarks>
        /// <see langword="null"/> where none of the three substitutions applies, which is the same
        /// boundary the rule above declines at.
        /// </remarks>
        private static Entity? IntegrateAPowerOfSineTimesAPowerOfCosine(
            Entity argument, ERational sinePower, ERational cosinePower, Entity factor, Entity rate)
        {
            // What is left after the substitution: u^leading times (1 + insideSign*u^2) raised to
            // expansionPower, all times sign -- which is what the substitution's own derivative
            // contributes, negative for the cosine and positive for the sine and the tangent.
            //
            // The exponent that decides the case must be a whole odd or even number, because it
            // is what the binomial expansion runs over. The *other* exponent -- `leading` -- need
            // only be rational: it rides through untouched and the power rule takes u^(-3/2) as
            // readily as u^(-3). That asymmetry is the whole of the fractional support.
            Entity u;
            ERational leading;
            int expansionPower, insideSign, sign;
            if (IsOddAndPositive(sinePower))
                (u, expansionPower, leading, insideSign, sign) =
                    (MathS.Cos(argument), (WholeOf(sinePower) - 1) / 2, cosinePower, -1, -1);
            else if (IsOddAndPositive(cosinePower))
                (u, expansionPower, leading, insideSign, sign) =
                    (MathS.Sin(argument), (WholeOf(cosinePower) - 1) / 2, sinePower, -1, 1);
            else if (IsEvenWhole(sinePower) && IsEvenWhole(cosinePower)
                     && WholeOf(sinePower) + WholeOf(cosinePower) <= -2)
                (u, expansionPower, leading, insideSign, sign) =
                    (MathS.Tan(argument),
                     -(WholeOf(sinePower) + WholeOf(cosinePower)) / 2 - 1, sinePower, 1, 1);
            // Neither substitution leaves a polynomial, and with both exponents whole there is
            // still a closed answer -- it just holds a logarithm, which the recurrences reach and
            // a rearranged polynomial cannot.
            else if (IsWholeAndFitsAnInt(sinePower, out var wholeSine)
                     && IsWholeAndFitsAnInt(cosinePower, out var wholeCosine))
                return (factor * IntegrateSineCosineByReduction(argument, wholeSine, wholeCosine) / rate)
                    .InnerSimplified;
            else
                return null;

            // The binomial expansion, each term integrated by the power rule -- or by the
            // logarithm at the one exponent where the power rule would divide by zero, which is
            // reachable here whenever `leading` is a negative odd whole number.
            Entity total = 0;
            var coefficient = EInteger.One;
            for (var i = 0; i <= expansionPower; i++)
            {
                var exponent = leading + ERational.FromInt32(2 * i);
                var raised = exponent + ERational.One;
                var term = raised.IsZero
                    ? IntegralPatterns.AntiderivativeLog(u)
                    : MathS.Pow(u, Number.Rational.Create(raised)) / Number.Rational.Create(raised);
                var alternating = insideSign < 0 && i % 2 != 0 ? -1 : 1;
                total += Number.Integer.Create(coefficient) * alternating * term;
                // The next binomial coefficient from this one: C(k, i+1) = C(k, i) * (k-i)/(i+1).
                coefficient = coefficient * (expansionPower - i) / (i + 1);
            }

            return (factor * sign * total / rate).InnerSimplified;
        }

        /// <summary>
        /// <c>int sin(t)^p cos(t)^q dt</c> for whole exponents of any sign, by the four standard
        /// recurrences, ending on the nine integrands with both exponents in <c>{-1, 0, 1}</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>This is the boundary the polynomial route stops at.</b> The three substitutions in
        /// <see cref="IntegrateAPowerOfSineTimesAPowerOfCosine"/> each leave a Laurent polynomial,
        /// so they answer the cases where one exponent is odd and positive, or where both are even
        /// and sum to at most <c>-2</c>. Everything else needs a <b>logarithm</b> somewhere, and
        /// no rearrangement of a polynomial produces one: <c>sin^2 cos^(-3)</c> is
        /// <c>x^2/sqrt(1 + x^2)</c> seen through the tangent substitution, and its antiderivative
        /// holds an inverse hyperbolic sine.
        /// </para>
        /// <para>
        /// The four recurrences, each of them one integration by parts written out:
        /// </para>
        /// <code>
        ///  q down   I(p,q) =  sin^(p+1) cos^(q-1)/(p+q)   + ((q-1)/(p+q))     I(p, q-2)
        ///  p down   I(p,q) = -sin^(p-1) cos^(q+1)/(p+q)   + ((p-1)/(p+q))     I(p-2, q)
        ///  q up     I(p,q) = -sin^(p+1) cos^(q+1)/(q+1)   + ((p+q+2)/(q+1))   I(p, q+2)
        ///  p up     I(p,q) =  sin^(p+1) cos^(q+1)/(p+1)   + ((p+q+2)/(p+1))   I(p+2, q)
        /// </code>
        /// <para>
        /// <b>Termination is <c>|p| + |q|</c>, which every branch below lowers by two</b> until
        /// both exponents are in <c>{-1, 0, 1}</c>. The downward pair divides by <c>p + q</c> and
        /// the upward pair by <c>q + 1</c> and <c>p + 1</c>; the ordering is chosen so that the
        /// branch taken never has a zero divisor. Raising is tried first for exactly that reason —
        /// <c>q + 1</c> is zero only at <c>q = -1</c>, which is already in range, so an exponent
        /// at most <c>-2</c> can always be raised, where <c>p + q</c> can vanish at any size.
        /// </para>
        /// <para>
        /// It is <b>closed</b>: the recursion is on two integers walking towards a fixed set, and
        /// it asks the integrator nothing.
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </para>
        /// </remarks>
        private static Entity IntegrateSineCosineByReduction(Entity argument, int sinePower, int cosinePower)
        {
            var sine = MathS.Sin(argument);
            var cosine = MathS.Cos(argument);

            if (sinePower >= -1 && sinePower <= 1 && cosinePower >= -1 && cosinePower <= 1)
                return (sinePower, cosinePower) switch
                {
                    (0, 0) => argument,
                    (1, 0) => -cosine,
                    (0, 1) => sine,
                    (1, 1) => MathS.Sqr(sine) / 2,
                    (1, -1) => -IntegralPatterns.AntiderivativeLog(cosine),      // int tan
                    (-1, 1) => IntegralPatterns.AntiderivativeLog(sine),         // int cot
                    (-1, -1) => IntegralPatterns.AntiderivativeLog(MathS.Tan(argument)),
                    // int sec and int csc, both as an inverse hyperbolic tangent. `ln|tan(t/2)|`
                    // is the usual form of the second and is avoided deliberately: it is the one
                    // base case whose argument is not `t` itself, and a caller substituting the
                    // trigonometric functions back cannot express a half-angle in terms of them.
                    // `-artanh(cos t)` is the same function and is symmetric with the first.
                    (0, -1) => MathS.Hyperbolic.Artanh(sine),
                    (-1, 0) => -MathS.Hyperbolic.Artanh(cosine),
                    _ => throw new Core.Exceptions.AngouriBugException(
                        "every pair in the base range is listed above"),
                };

            var sum = sinePower + cosinePower;
            Entity Power(Entity of, int to) => MathS.Pow(of, Number.Integer.Create(to));
            Entity Carried(int numerator, int denominator, int nextSine, int nextCosine)
                => Number.Rational.Create(numerator, denominator)
                 * IntegrateSineCosineByReduction(argument, nextSine, nextCosine);

            // Raising first, because an exponent at most -2 can always be raised and lowering can
            // be blocked by p + q being zero -- `tan(t)^2` is `sin^2 cos^(-2)`, the smallest case
            // where it is.
            if (cosinePower <= -2)
                return -Power(sine, sinePower + 1) * Power(cosine, cosinePower + 1)
                       / Number.Integer.Create(cosinePower + 1)
                     + Carried(sum + 2, cosinePower + 1, sinePower, cosinePower + 2);
            if (sinePower <= -2)
                return Power(sine, sinePower + 1) * Power(cosine, cosinePower + 1)
                       / Number.Integer.Create(sinePower + 1)
                     + Carried(sum + 2, sinePower + 1, sinePower + 2, cosinePower);
            // Both exponents are now at least -1, so p + q is zero only if one of them is -1 and
            // the other 1 -- both in range, and so already answered above.
            if (cosinePower >= 2)
                return Power(sine, sinePower + 1) * Power(cosine, cosinePower - 1)
                       / Number.Integer.Create(sum)
                     + Carried(cosinePower - 1, sum, sinePower, cosinePower - 2);
            return -Power(sine, sinePower - 1) * Power(cosine, cosinePower + 1)
                   / Number.Integer.Create(sum)
                 + Carried(sinePower - 1, sum, sinePower - 2, cosinePower);
        }

        /// <summary>
        /// A rational function of <c>x^n</c> times <c>(c + d x^n)^(k - 1/n)</c> for a whole
        /// <c>k</c>, rationalised by <c>u = x/(c + d x^n)^(1/n)</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>du/dx</c> is <c>c (c + d x^n)^(-(n + 1)/n)</c>, so <c>dx/(c + d x^n)^(1/n)</c> is
        /// <c>(c + d x^n) du/c</c>; and <c>u^n = x^n/(c + d x^n)</c> gives <c>x^n = c u^n/(1 - d u^n)</c>
        /// and <c>c + d x^n = c/(1 - d u^n)</c>, so everything in <c>x^n</c> is rational in
        /// <c>u^n</c> and the integrand is a rational function of <c>u</c>. Timofeev's
        /// <c>1/((1 + x^4)(2 + x^4)^(1/4))</c> is <c>1/(1 + u^4)</c> under it, and his
        /// <c>(1 + x^4)^(3/4)/(2 + x^4)^2</c>, whose root is <c>(1 + x^4)^(1 - 1/4)</c>, is
        /// rational the same way; Welz's <c>1/((1 - x^3)(a + b x^3)^(1/3))</c> is
        /// <c>1/(1 - (a + b) u^3)</c>. Rubi's rule for <c>(c + d x^n)^p/(a + b x^n)</c> at
        /// <c>p = -1/n</c> is this substitution.
        /// </para>
        /// <para>
        /// The identity holds wherever the root is real, which is where the answer is
        /// asked; <c>u</c> goes back in as <c>x (c + d x^n)^(-1/n)</c>.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveByDividingByTheRoot(Entity expr, Entity.Variable x)
        {
            // One fractional power of a binomial `c + d x^n` with exponent `k - 1/n`, and the
            // rest a rational function of x^n.
            Entity? radicand = null;
            Entity? c = null, d = null;
            var n = 0;
            var k = EInteger.Zero;
            Entity rest = Number.Integer.One;
            var (above, below) = Functions.SingleQuotient.Of(expr);
            foreach (var (side, isBelow) in new[] { (above, false), (below, true) })
                foreach (var factor in Mulf.LinearChildren(side))
                {
                    if (factor is Powf(var @base, Number.Rational exponent) && exponent is not Number.Integer && @base.ContainsNode(x))
                    {
                        var signed = isBelow ? exponent.ERational.Negate() : exponent.ERational;
                        if (radicand is not null || !TryReadAsABinomialIn(@base, x, out var readC, out var readD, out var readN)
                            || readN < 2 || !signed.Denominator.Equals(EInteger.FromInt32(readN)))
                            return null;
                        // The numerator of the exponent is k n - 1.
                        var shifted = signed.Numerator.Add(EInteger.One);
                        if (!shifted.Remainder(EInteger.FromInt32(readN)).IsZero)
                            return null;
                        radicand = @base;
                        c = readC;
                        d = readD;
                        n = readN;
                        k = shifted.Divide(EInteger.FromInt32(readN));
                        continue;
                    }
                    if (factor.ContainsNode(x) && !IsRationalIn(factor, x))
                        return null;
                    rest = rest == Number.Integer.One ? (isBelow ? 1 / factor : factor) : isBelow ? rest / factor : rest * factor;
                }
            if (radicand is null || c is null || d is null || TreeAnalyzer.IsZero(c) || TreeAnalyzer.IsZero(d))
                return null;
            // The rest as a rational function of x^n: written in a stand-in for x^n where every
            // power of x is a multiple of n, and declined otherwise.
            var xToTheN = Variable.CreateUnique(expr, "x_n");
            var inXToTheN = rest.Replace(node =>
                node is Powf(var xAgain, Number.Integer power) && xAgain == x && power.EInteger.Remainder(EInteger.FromInt32(n)).IsZero
                    ? MathS.Pow(xToTheN, Number.Integer.Create(power.EInteger.Divide(EInteger.FromInt32(n))))
                    : node);
            if (inXToTheN.ContainsNode(x))
                return null;
            var u = Variable.CreateUnique(expr, "u_root");
            var uToTheN = MathS.Pow(u, n);
            var oneMinusDUToTheN = 1 - d * uToTheN;
            var binomialInU = c / oneMinusDUToTheN;
            var integrand = Functions.SingleQuotient.Combine(
                inXToTheN.Substitute(xToTheN, c * uToTheN / oneMinusDUToTheN)
                * (k.IsZero ? Number.Integer.One : MathS.Pow(binomialInU, Number.Integer.Create(k)))
                / oneMinusDUToTheN).Simplify();
            if (integrand is Providedf(var inner, _))
                integrand = inner;
            if (integrand.ContainsNode(x) || integrand.Nodes.Any(node => node == MathS.NaN))
                return null;
            if (Integration.ComputeAsAQuestionOfItsOwn(integrand, u, integrateByParts: false) is not { } result)
                return null;
            var answer = result.Substitute(u, x * MathS.Pow(radicand, Number.Rational.Create(-1, n)));
            return answer.Nodes.Any(node => node == MathS.NaN) ? null : answer;
        }

        /// <summary>
        /// Reads <paramref name="expr"/> as <c>c + d x^n</c> with <c>c</c> and <c>d</c> free of
        /// <paramref name="x"/>.
        /// </summary>
        private static bool TryReadAsABinomialIn(Entity expr, Entity.Variable x, out Entity c, out Entity d, out int n)
        {
            c = d = Number.Integer.Zero;
            n = 0;
            if (!TreeAnalyzer.TryGetPolynomial(expr, x, out var monomials) || monomials.Count != 2
                || !monomials.TryGetValue(EInteger.Zero, out var constant))
                return false;
            foreach (var pair in monomials)
                if (!pair.Key.IsZero)
                {
                    if (!pair.Key.CanFitInInt32() || pair.Value.ContainsNode(x))
                        return false;
                    n = pair.Key.ToInt32Checked();
                    d = pair.Value;
                }
            c = constant;
            return !c.ContainsNode(x) && n >= 1;
        }

        /// <summary>
        /// A <b>binomial differential</b> <c>x^m (a + b x^n)^(p/q)</c>, in the two of Chebyshev's
        /// three cases that are not a whole power: <c>(m + 1)/n</c> whole, or
        /// <c>(m + 1)/n + p/q</c> whole.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>x^2 sqrt(1 + x^3)</c> came out and <c>x^5 sqrt(1 + x^3)</c> did not, and the two are
        /// the same substitution. Under <c>u = (a + b x^n)^(1/q)</c> the first leaves a monomial
        /// in <c>u</c>, which the general substitution finds because <c>x^(n-1)</c> is <c>du</c>
        /// up to a constant; the second leaves a <b>polynomial</b>, which it does not, because
        /// what multiplies <c>du</c> there is <c>x^3</c> rather than a constant.
        /// </para>
        /// <para>
        /// Writing <c>s = (m + 1)/n</c> and substituting:
        /// </para>
        /// <code>
        ///     x^n = (u^q - a)/b       x^m dx = (q/(n b)) ((u^q - a)/b)^(s-1) u^(q-1) du
        ///     int x^m (a + b x^n)^(p/q) dx = (q/(n b)) int ((u^q - a)/b)^(s-1) u^(p+q-1) du
        /// </code>
        /// <para>
        /// which is a polynomial in <c>u</c> when <c>s</c> is a whole number of at least one,
        /// expanded by the binomial theorem and integrated term by term; and a <b>rational
        /// function</b> of <c>u</c> when <c>s</c> is a whole number of at most zero, which the
        /// rational integrator answers — <c>sqrt(1 + x^3)/x</c> is <c>(2/3) int u^2/(u^2 - 1) du</c>,
        /// and <c>1/(x sqrt(1 - x^3))</c> is <c>(2/3) int 1/(u^2 - 1) du</c>. Both were declined
        /// for <c>s = 0</c>, which the rule read as "not a polynomial" and left at that.
        /// </para>
        /// <para>
        /// <b>The third case comes down to the second.</b> Where <c>s + p/q</c> is whole instead,
        /// <c>x = 1/y</c> turns <c>x^m (a + b x^n)^(p/q) dx</c> into
        /// <c>-y^m' (b + a y^n)^(p/q) dy</c> with <c>m' = -m - 2 - n p/q</c>, a whole number, and
        /// <c>(m' + 1)/n = -(s + p/q)</c>, whole — so it is the second case in <c>y</c>, with the
        /// roles of <c>a</c> and <c>b</c> exchanged, and <c>y = 1/x</c> put back afterwards.
        /// <c>x^6 (3 + 4x^4)^(1/4)</c> and <c>(x^3 - 1)/(2 + x^3)^(1/3)</c> are this. Chebyshev
        /// proved there is no fourth case: outside these the integrand has no elementary
        /// antiderivative at all, which is worth knowing before anyone goes looking.
        /// </para>
        /// <para>
        /// Closed in one step: the polynomial case is a sum of powers, and the rational case
        /// goes to the rational integrator directly, not back into the chain -- which is what
        /// lets it be volunteered at any depth rather than asked at the top only
        /// (<a href="https://github.com/asc-community/AngouriMath/issues/1265">#1265</a>):
        /// <c>tan(x)/sqrt(1 + sec(x)^3)</c> is <c>1/(u sqrt(1 + u^3))</c> under <c>u = sec(x)</c>,
        /// one level down, and was declined there.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveABinomialDifferential(Entity expr, Entity.Variable x)
        {
            if (!TryReadABinomialDifferential(expr, x, out var power, out var exponent,
                    out var inner, out var free, out var leading, out var factor))
                return null;

            // The second case: s = (m + 1)/n whole.
            if ((power + 1) % inner == 0)
                return IntegrateABinomialDifferentialInTheSecondCase(
                    power, inner, exponent, free, leading, x, factor);

            // The third: s + p/q whole, taken to the second by x = 1/y.
            var sPlusP = ERational.Create(power + 1, inner).Add(exponent);
            if (!sPlusP.IsInteger())
                return null;
            var nTimesP = exponent.Multiply(EInteger.FromInt32(inner));
            if (!nTimesP.IsInteger() || !nTimesP.Numerator.CanFitInInt32())
                return null;
            var reflectedPower = -power - 2 - nTimesP.ToLowestTerms().Numerator.ToInt32Unchecked();
            var y = Variable.CreateUnique(expr, "y_binom");
            if (IntegrateABinomialDifferentialInTheSecondCase(
                    reflectedPower, inner, exponent, leading, free, y, Number.Integer.MinusOne) is not { } inY)
                return null;
            return (factor * inY.Substitute(y, 1 / x)).InnerSimplified;
        }

        /// <summary>
        /// <c>int factor * v^m (a + b v^n)^(p/q) dv</c> with <c>(m + 1)/n</c> whole: a polynomial
        /// in <c>u = (a + b v^n)^(1/q)</c> expanded term by term for <c>s >= 1</c>, and a rational
        /// function of <c>u</c> handed to the rational integrator for <c>s &lt;= 0</c>.
        /// </summary>
        private static Entity? IntegrateABinomialDifferentialInTheSecondCase(
            int power, int inner, ERational exponent, ERational free, ERational leading,
            Entity.Variable v, Entity factor)
        {
            if ((power + 1) % inner != 0)
                return null;
            var s = (power + 1) / inner;
            var q = exponent.Denominator.ToInt32Checked();
            var p = exponent.Numerator.ToInt32Checked();
            var a = Number.Rational.Create(free);
            var b = Number.Rational.Create(leading);
            var u = Variable.CreateUnique(v + factor, "u_binom");
            var outside = Number.Integer.Create(q)
                        / (Number.Integer.Create(inner) * MathS.Pow(b, Number.Integer.Create(s)));
            var bracket = (a + b * MathS.Pow(v, Number.Integer.Create(inner))).InnerSimplified;
            var back = MathS.Pow(bracket, Number.Rational.Create(EInteger.One, exponent.Denominator));

            if (s < 1)
            {
                // (q/(n b^s)) int u^(p+q-1) / (u^q - a)^(1-s) du: a rational function of u.
                var overPower = 1 - s;
                Entity denominator = overPower == 1 ? MathS.Pow(u, q) - a : MathS.Pow(MathS.Pow(u, q) - a, overPower);
                Entity numerator = p + q - 1 == 0 ? Number.Integer.One : MathS.Pow(u, p + q - 1);
                // To the rational integrator directly rather than back into the chain, so that
                // this closes in one step wherever it is asked -- below the secant substitution
                // for `tan(x)/sqrt(1 + sec(x)^3)`, which is `1/(u sqrt(1 + u^3))` one level down
                // -- instead of opening the search that had it confined to the top level.
                var rational = (numerator / denominator).InnerSimplified;
                if ((IntegralPatterns.TryStandardIntegrals(rational, u)
                     ?? SolveByPartialFractions(rational, u, integrateByParts: false)
                     ?? SolveByRothsteinTrager(rational, u)) is not { } inU)
                    return null;
                return (factor * outside * inU.Substitute(u, back)).InnerSimplified;
            }

            // (q/(n b^s)) * int (u^q - a)^(s-1) u^(p+q-1) du, the binomial expanded.
            Entity total = 0;
            var binomial = EInteger.One;
            for (var i = 0; i <= s - 1; i++)
            {
                // Term i of (u^q - a)^(s-1) is C(s-1, i) u^(q i) (-a)^(s-1-i).
                var raised = q * i + p + q;
                if (raised == 0)
                    return null;   // the power rule would divide by zero; not a polynomial after all
                total += Number.Integer.Create(binomial)
                       * MathS.Pow(-a, Number.Integer.Create(s - 1 - i))
                       * MathS.Pow(u, Number.Integer.Create(raised)) / Number.Integer.Create(raised);
                binomial = binomial * (s - 1 - i) / (i + 1);
            }
            return (factor * outside * total.Substitute(u, back)).InnerSimplified;
        }

        /// <summary>
        /// Reads <paramref name="expr"/> as a rational multiple of <c>x^m (a + b x^n)^(p/q)</c>,
        /// with <c>q</c> above one and <c>n</c> at least two.
        /// </summary>
        /// <remarks>
        /// A whole exponent on the bracket is a polynomial and wants expanding rather than this;
        /// <c>n = 1</c> is a radical of something linear, which
        /// <see cref="SolveByLinearRadicalSubstitution"/> answers in its own terms and more
        /// shortly.
        /// </remarks>
        private static bool TryReadABinomialDifferential(
            Entity expr, Entity.Variable x, out int power, out ERational exponent,
            out int inner, out ERational free, out ERational leading, out Entity factor)
        {
            power = 0;
            exponent = ERational.Zero;
            inner = 0;
            free = ERational.Zero;
            leading = ERational.Zero;
            factor = 1;

            Entity? bracket = null;
            var exponentFound = ERational.Zero;
            var powerFound = 0;
            Entity constantFactor = 1;
            if (!Read(expr, 1) || bracket is null)
                return false;

            var lowest = exponentFound.ToLowestTerms();
            if (lowest.Denominator.Equals(EInteger.One)
                || !lowest.Numerator.CanFitInInt32() || !lowest.Denominator.CanFitInInt32())
                return false;

            // `a + b x^n`: one term free of the variable and one monomial in it.
            Entity? constantPart = null;
            Entity? monomial = null;
            foreach (var term in Sumf.LinearChildren(bracket))
                if (!term.ContainsNode(x))
                    constantPart = constantPart is null ? term : constantPart + term;
                else if (monomial is null)
                    monomial = term;
                else
                    return false;
            if (constantPart is null || monomial is null)
                return false;
            if (!TryReadAMonomial(monomial, x, out var degree, out var coefficient) || degree < 2)
                return false;
            if (constantPart.Evaled is not Number.Rational constantValue
                || coefficient.Evaled is not Number.Rational coefficientValue
                || coefficientValue.ERational.IsZero)
                return false;

            power = powerFound;
            exponent = lowest;
            inner = degree;
            free = constantValue.ERational;
            leading = coefficientValue.ERational;
            factor = constantFactor;
            return true;

            bool Read(Entity node, int multiplicity)
            {
                switch (node)
                {
                    case Variable v when v == x:
                        powerFound += multiplicity;
                        return true;
                    case Mulf(var left, var right):
                        return Read(left, multiplicity) && Read(right, multiplicity);
                    case Divf(var above, var below):
                        return Read(above, multiplicity) && Read(below, -multiplicity);
                    case Powf(var @base, Number.Integer whole)
                        when @base == x && whole.EInteger.CanFitInInt32():
                        powerFound += multiplicity * whole.EInteger.ToInt32Checked();
                        return true;
                    case Powf(var @base, Number.Rational raised) when @base.ContainsNode(x):
                        if (bracket is not null && bracket != @base)
                            return false;
                        bracket = @base;
                        exponentFound += ERational.FromInt32(multiplicity) * raised.ERational;
                        return true;
                    case Number.Rational rational when node is not Powf:
                        constantFactor = multiplicity > 0
                            ? constantFactor * MathS.Pow(rational, multiplicity)
                            : constantFactor / MathS.Pow(rational, -multiplicity);
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>Reads <c>c * x^n</c>, giving the degree and the coefficient.</summary>
        private static bool TryReadAMonomial(Entity term, Entity.Variable x, out int degree, out Entity coefficient)
        {
            degree = 0;
            coefficient = 1;
            switch (term)
            {
                case Variable v when v == x:
                    degree = 1;
                    return true;
                case Powf(var @base, Number.Integer whole) when @base == x && whole.EInteger.CanFitInInt32():
                    degree = whole.EInteger.ToInt32Checked();
                    return true;
                case Mulf(var left, var right) when !left.ContainsNode(x):
                    if (!TryReadAMonomial(right, x, out degree, out var fromRight))
                        return false;
                    coefficient = left * fromRight;
                    return true;
                case Mulf(var left, var right) when !right.ContainsNode(x):
                    if (!TryReadAMonomial(left, x, out degree, out var fromLeft))
                        return false;
                    coefficient = right * fromLeft;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// A polynomial times an exponential times a sine or a cosine —
        /// <c>P(x) e^(a x) cos(b x)</c> and its kin — integrated by the repeated by-parts that
        /// this shape is the textbook case for, run out here rather than through the chain.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>e^x cos(x)</c> came out and <c>x e^x cos(x)</c> did not. The polynomial by-parts
        /// integrates its <c>dv</c> with by-parts switched off, and <c>e^x cos(x)</c> is answered
        /// by <b>nothing but</b> by-parts — the cyclic one, where the integral comes back a
        /// multiple of itself. So the outer integral was declined for want of the inner one.
        /// </para>
        /// <para>
        /// <b>Switching that flag on is not the fix, and this is measured.</b> It buys these
        /// integrands and takes <c>x tan(x)^3 sec(x)^4</c> from 20 ms to <b>94 seconds</b> to
        /// decline — one test, the whole of a 76% slowdown in the calculus suite. The cost and
        /// the gain both arrive on the <i>second</i> round, integrating the antiderivative the
        /// first produced, so no size measure separates them: what separates them is whether the
        /// search succeeds, and that is only known by running it.
        /// https://github.com/asc-community/AngouriMath/issues/1265
        /// </para>
        /// <para>
        /// So the family is named instead of searched for. <c>int e^(a x)(c cos(b x) + d sin(b x))
        /// dx</c> is closed:
        /// </para>
        /// <code>
        ///     e^(ax) [ (a c - b d) cos(bx) + (b c + a d) sin(bx) ] / (a^2 + b^2)
        /// </code>
        /// <para>
        /// and it stays in the same shape, so integrating <c>x^k</c> against it by parts lowers
        /// <c>k</c> by one and leaves the same shape again. That terminates in <c>k + 1</c> steps
        /// with no search at all, which is what makes this <b>closed</b>.
        /// </para>
        /// <para>
        /// <c>a = 0</c> and <c>b = 0</c> are both admitted — a polynomial times a bare sine, or a
        /// bare exponential — though those already came out, so what this adds is the two
        /// together. Both zero is a polynomial and is declined, since it is the power rule's.
        /// </para>
        /// <para>
        /// <b>Asked, not volunteered</b>:
        /// <a href="https://github.com/asc-community/AngouriMath/issues/1265">#1265</a>.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveAPolynomialTimesAnExponentialAndATrigonometric(Entity expr, Entity.Variable x)
        {
            if (!Integration.AnsweringTheQuestionAsked)
                return null;
            if (!TryReadAnExponentialTimesATrigonometric(expr, x,
                    out var polynomial, out var rate, out var frequency, out var sinePower, out var cosinePower))
                return null;

            // One sine or one cosine, or neither, is the shape the loop closes. A power of either,
            // or both together, is a sum of multiple angles -- `sin^3 u = (3 sin u - sin 3u)/4` --
            // and each term of that sum is the shape again, at its own frequency, so the loop
            // runs once per term and the answers add. `e^u sin(u)^3` is what
            // `e^(arcsin(x)) x^3/sqrt(1 - x^2)` becomes under `x = sin(u)`, and it was declined
            // for the cube.
            if (sinePower + cosinePower <= 1)
                return IntegrateAPolynomialTimesAnExponentialTimesOneTrigonometric(
                    polynomial, rate, frequency, sinePower == 1 ? 0 : 1, sinePower == 1 ? 1 : 0, x);

            Entity total = 0;
            foreach (var (multiple, isSine, coefficient) in MultipleAngles(sinePower, cosinePower))
            {
                var term = IntegrateAPolynomialTimesAnExponentialTimesOneTrigonometric(
                    (coefficient * polynomial).InnerSimplified, rate, (multiple * frequency).InnerSimplified,
                    isSine ? 0 : 1, isSine ? 1 : 0, x);
                if (term is null)
                    return null;
                total += term;
            }
            return total.InnerSimplified;
        }

        /// <summary>
        /// <c>sin^s cos^c</c> of one argument as a sum of sines and cosines of its multiples:
        /// each entry is a multiple, whether it is a sine, and its coefficient. Built by
        /// multiplying one sine or cosine in at a time through the product-to-sum identities,
        /// which keeps the count of terms linear in the powers.
        /// </summary>
        private static List<(int Multiple, bool IsSine, Entity Coefficient)> MultipleAngles(int sinePower, int cosinePower)
        {
            var terms = new Dictionary<(int, bool), Entity> { [(0, false)] = Number.Integer.One };
            void Add(int k, bool isSine, Entity coefficient)
            {
                // sin(-k t) = -sin(k t), cos(-k t) = cos(k t), sin(0) = 0.
                if (k < 0)
                {
                    k = -k;
                    if (isSine) coefficient = -coefficient;
                }
                if (k == 0 && isSine)
                    return;
                terms[(k, isSine)] = terms.TryGetValue((k, isSine), out var already) ? already + coefficient : coefficient;
            }
            for (var i = 0; i < sinePower + cosinePower; i++)
            {
                var bySine = i < sinePower;
                var next = new Dictionary<(int, bool), Entity>();
                var previous = terms;
                terms = next;
                foreach (var pair in previous)
                {
                    var (k, isSine) = pair.Key;
                    var half = (pair.Value / 2).InnerSimplified;
                    if (bySine)
                    {
                        // cos(k t) sin(t) = (sin((k+1) t) - sin((k-1) t))/2
                        // sin(k t) sin(t) = (cos((k-1) t) - cos((k+1) t))/2
                        if (!isSine) { Add(k + 1, true, half); Add(k - 1, true, -half); }
                        else { Add(k - 1, false, half); Add(k + 1, false, -half); }
                    }
                    else
                    {
                        // cos(k t) cos(t) = (cos((k-1) t) + cos((k+1) t))/2
                        // sin(k t) cos(t) = (sin((k+1) t) + sin((k-1) t))/2
                        if (!isSine) { Add(k - 1, false, half); Add(k + 1, false, half); }
                        else { Add(k + 1, true, half); Add(k - 1, true, half); }
                    }
                }
            }
            return terms.Select(pair => (pair.Key.Item1, pair.Key.Item2, pair.Value.InnerSimplified))
                        .Where(term => term.Item3 != Number.Integer.Zero)
                        .ToList();
        }

        /// <summary>
        /// <c>int P(x) e^(a x) (c cos(b x) + d sin(b x)) dx</c>, closed by as many rounds of parts
        /// as <c>P</c> has degree.
        /// </summary>
        private static Entity? IntegrateAPolynomialTimesAnExponentialTimesOneTrigonometric(
            Entity polynomial, Entity rate, Entity frequency, Entity onTheCosine, Entity onTheSine, Entity.Variable x)
        {
            // a^2 + b^2, which is zero when both are, and then the integrand is a polynomial --
            // and when the rate is the frequency times i: `e^(-i u) cos(u)` is
            // `(1 + e^(-2 i u))/2`, a resonance the formula divides by zero on, and it answered
            // NaN for `e^(-i arctan(a x))/(c + a^2 c x^2)^(3/2)` under `x = tan(u)/a`.
            var scale = (MathS.Sqr(rate) + MathS.Sqr(frequency)).InnerSimplified;
            var bothZero = rate.Evaled is Number.Complex { IsZero: true } && frequency.Evaled is Number.Complex { IsZero: true };
            if (bothZero)
                return null;
            var exponential = MathS.Pow(MathS.e, rate * x);
            var cosine = MathS.Cos(frequency * x);
            var sine = MathS.Sin(frequency * x);
            if (scale == 0 || scale.Evaled is Number.Complex { IsZero: true })
            {
                // Resonant: by Euler, `c cos(bx) + d sin(bx)` is
                // `(c - i d) e^(i b x)/2 + (c + i d) e^(-i b x)/2`, and each term beside
                // `P(x) e^(a x)` is a polynomial times one exponential -- one of them, with
                // `a = -i b` or `a = i b`, a polynomial alone -- which are closed on their own.
                // `e^(i arctan(a x))/sqrt(c + a^2 c x^2)` under `x = tan(u)/a` is `e^(i u) cos(u)`.
                var plus = ((onTheCosine - MathS.i * onTheSine) / 2).InnerSimplified;
                var minus = ((onTheCosine + MathS.i * onTheSine) / 2).InnerSimplified;
                Entity resonant = Number.Integer.Zero;
                foreach (var (coefficient, exponent) in new[] { (plus, (rate + MathS.i * frequency).InnerSimplified), (minus, (rate - MathS.i * frequency).InnerSimplified) })
                {
                    if (coefficient.Evaled is Number.Complex { IsZero: true })
                        continue;
                    var term = exponent.Evaled is Number.Complex { IsZero: true } ? polynomial : polynomial * MathS.Pow(MathS.e, exponent * x);
                    if (Integration.ComputeIndefiniteIntegral(term.InnerSimplified, x, integrateByParts: false) is not { } termIntegral || termIntegral.Nodes.Any(node => node == MathS.NaN))
                        return null;
                    resonant += coefficient * termIntegral;
                }
                return resonant.InnerSimplified;
            }

            // Integrating `e^(ax)(c cos + d sin)` leaves the same shape, so one step of by parts
            // against `P(x)` lowers its degree and leaves this loop's own state: a polynomial and
            // the pair of coefficients. `total` collects the boundary terms.
            Entity total = 0;
            var carried = polynomial;
            var c = onTheCosine;
            var d = onTheSine;
            for (var step = 0; ; step++)
            {
                if (step > MaximumByPartsSteps)
                    return null;   // the degree should fall every round; if it has not, decline
                // int e^(ax) cos(bx) = e^(ax)(a cos + b sin)/(a^2+b^2), and
                // int e^(ax) sin(bx) = e^(ax)(a sin - b cos)/(a^2+b^2). Together, for
                // `c cos + d sin`, the cosine collects `a c - b d` and the sine `b c + a d`.
                var nextC = ((rate * c - frequency * d) / scale).InnerSimplified;
                var nextD = ((frequency * c + rate * d) / scale).InnerSimplified;
                var antiderivative = exponential * (nextC * cosine + nextD * sine);

                total += carried * antiderivative;
                var derivative = carried.Differentiate(x).InnerSimplified;
                if (derivative == 0)
                    return total.InnerSimplified;
                // `- int P'(x) * (that) dx`, which is this loop again with the sign folded in.
                carried = (-derivative).InnerSimplified;
                c = nextC;
                d = nextD;
            }
        }

        /// <summary>
        /// How many rounds of by parts are admitted before the rule gives up on its own
        /// termination. The degree of the polynomial falls every round, so a polynomial this rule
        /// accepted cannot reach it; it is a backstop against a <c>Differentiate</c> that does not
        /// lower the degree rather than a bound on anything expected.
        /// </summary>
        private const int MaximumByPartsSteps = 64;

        /// <summary>
        /// The largest total power of the sine and the cosine the rule expands into multiple
        /// angles; past it the sum has more terms than any answer is worth.
        /// </summary>
        private const int MaximumTrigonometricPower = 8;

        /// <summary>
        /// Reads <paramref name="expr"/> as <c>P(x) e^(a x) (c cos(b x) + d sin(b x))</c>, where
        /// <c>P</c> is a polynomial and <c>a</c> and <c>b</c> are free of the variable.
        /// </summary>
        /// <remarks>
        /// A base other than <c>e</c> is read through: <c>2^x</c> is <c>e^(x ln 2)</c>, so
        /// <c>2^x x cos(x)</c> is this shape and was declined for the spelling alone.
        /// </remarks>
        private static bool TryReadAnExponentialTimesATrigonometric(
            Entity expr, Entity.Variable x, out Entity polynomial,
            out Entity rate, out Entity frequency, out int sinePower, out int cosinePower)
        {
            polynomial = 1;
            rate = 0;
            frequency = 0;
            sinePower = 0;
            cosinePower = 0;

            Entity? rateFound = null;
            Entity? frequencyFound = null;
            var cosines = 0;
            var sines = 0;
            Entity polynomialPart = 1;
            if (!Read(expr, 1))
                return false;
            // Powers of the sine and the cosine, of one argument, above the bar: a reciprocal
            // sine or cosine is a different integrand. A secant or a cosecant *below* the bar is
            // a cosine or a sine above it and is read as one.
            if (cosines < 0 || sines < 0 || cosines + sines > MaximumTrigonometricPower)
                return false;
            if (!MathS.TryPolynomial(polynomialPart, x, out var asPolynomial)
                && polynomialPart.ContainsNode(x))
                return false;

            polynomial = polynomialPart.ContainsNode(x) ? asPolynomial! : polynomialPart;
            rate = rateFound ?? 0;
            frequency = frequencyFound ?? 0;
            sinePower = sines;
            cosinePower = cosines;
            // A bare polynomial is the power rule's, and a polynomial times a bare exponential or
            // a bare sine already came out; what is new is the two together. Reading them all the
            // same way costs nothing and keeps the rule one thing.
            return rateFound is not null || frequencyFound is not null;

            bool Read(Entity node, int multiplicity)
            {
                switch (node)
                {
                    case Sinf(var argument) when multiplicity == 1:
                        sines++;
                        return AgreesOnTheFrequency(argument);
                    case Cosf(var argument) when multiplicity == 1:
                        cosines++;
                        return AgreesOnTheFrequency(argument);
                    case Cosecantf(var argument) when multiplicity == -1:
                        sines++;
                        return AgreesOnTheFrequency(argument);
                    case Secantf(var argument) when multiplicity == -1:
                        cosines++;
                        return AgreesOnTheFrequency(argument);
                    case Powf(Sinf(var argument), Number.Integer power) when multiplicity == 1 && power.EInteger.Sign > 0 && power.EInteger.CanFitInInt32():
                        sines += power.EInteger.ToInt32Unchecked();
                        return AgreesOnTheFrequency(argument);
                    case Powf(Cosf(var argument), Number.Integer power) when multiplicity == 1 && power.EInteger.Sign > 0 && power.EInteger.CanFitInInt32():
                        cosines += power.EInteger.ToInt32Unchecked();
                        return AgreesOnTheFrequency(argument);
                    case Powf(Cosecantf(var argument), Number.Integer power) when multiplicity == -1 && power.EInteger.Sign > 0 && power.EInteger.CanFitInInt32():
                        sines += power.EInteger.ToInt32Unchecked();
                        return AgreesOnTheFrequency(argument);
                    case Powf(Secantf(var argument), Number.Integer power) when multiplicity == -1 && power.EInteger.Sign > 0 && power.EInteger.CanFitInInt32():
                        cosines += power.EInteger.ToInt32Unchecked();
                        return AgreesOnTheFrequency(argument);
                    case Powf(var @base, var power)
                        when !@base.ContainsNode(x) && power.ContainsNode(x) && @base != 0:
                        // e^(ax), and any other base through its logarithm.
                        if (!TreeAnalyzer.TryGetPolyLinear(power, x, out var slope, out var offset))
                            return false;
                        var thisRate = (slope * (@base == MathS.e ? 1 : MathS.Ln(@base))).InnerSimplified;
                        rateFound = rateFound is null
                            ? (multiplicity * thisRate).InnerSimplified
                            : (rateFound + multiplicity * thisRate).InnerSimplified;
                        // The constant part of the exponent is a factor, not a rate -- and there
                        // is none to take when the exponent has no constant part. `MathS.Pow`
                        // leaves `b^0` written out, and a factor of `d^0` standing in front of
                        // the polynomial is enough to stop `TryPolynomial` reading it, so
                        // `d^x x cos(x)` was declined where `2^x x cos(x)` came out: with a
                        // numeric base the same dead factor is folded away and with a symbolic
                        // one it is not. `d^x cos(x)` hid it, because a polynomial part free of
                        // the variable is never read as a polynomial at all.
                        // https://github.com/asc-community/AngouriMath/issues/718
                        if (offset.Evaled is not Number.Integer(0))
                            polynomialPart *= MathS.Pow(@base, offset * multiplicity);
                        return true;
                    case Mulf(var left, var right):
                        return Read(left, multiplicity) && Read(right, multiplicity);
                    case Divf(var above, var below):
                        return Read(above, multiplicity) && Read(below, -multiplicity);
                    default:
                        if (multiplicity != 1)
                            return false;
                        polynomialPart *= node;
                        return true;
                }
            }

            bool AgreesOnTheFrequency(Entity argument)
            {
                if (!TreeAnalyzer.TryGetPolyLinear(argument, x, out var slope, out var offset)
                    || offset.Evaled is not Number.Integer(0))
                    return false;   // a phase would want the angle-sum identity first
                if (frequencyFound is not null && frequencyFound != slope)
                    return false;
                frequencyFound = slope;
                return true;
            }
        }

        /// <summary>
        /// A power of the variable times an odd half-power of a quadratic without a linear term —
        /// <c>x^m (a + b x^2)^(k/2)</c> with <c>k</c> odd — turned into a power of the sine times
        /// a power of the cosine, which
        /// <see cref="IntegrateAPowerOfSineTimesAPowerOfCosine"/> answers in closed form.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>1/(1 + x^2)^(3/2)</c>, <c>x^5/sqrt(5 + x^2)</c>, <c>1/(x^2 sqrt(1 - x^2))</c> and
        /// their kin had no antiderivative. A square root of a quadratic is the largest single
        /// class the Rubi suites leave unanswered here, and this is the part of it that comes out
        /// <b>closed</b>.
        /// </para>
        /// <para>
        /// <b>Why the trigonometric substitution rather than Euler's.</b> Euler's rationalises the
        /// radical and lands on a rational function, which then has to be integrated by the whole
        /// chain — a speculative hand-off that
        /// <a href="https://github.com/asc-community/AngouriMath/issues/1244">#1244</a> measured
        /// at tens of seconds on integrands it never answers. This lands on <c>sin^p cos^q</c>,
        /// which is a Laurent polynomial after one more substitution and is integrated term by
        /// term without asking the integrator anything. Same class of integrand; one of the two
        /// routes is closed and the other is a search.
        /// </para>
        /// <para>
        /// <b>Three substitutions, chosen by the two signs.</b> With <c>r = sqrt(|a/b|)</c>:
        /// </para>
        /// <list type="bullet">
        /// <item><description>
        /// <c>a &gt; 0, b &gt; 0</c>: <c>x = r tan(t)</c>, under which <c>a + b x^2 = a sec(t)^2</c>
        /// and <c>dx = r sec(t)^2 dt</c>, leaving <c>sin(t)^m cos(t)^(-m-k-2)</c>.
        /// </description></item>
        /// <item><description>
        /// <c>a &gt; 0, b &lt; 0</c>: <c>x = r sin(t)</c>, under which <c>a + b x^2 = a cos(t)^2</c>
        /// and <c>dx = r cos(t) dt</c>, leaving <c>sin(t)^m cos(t)^(k+1)</c>.
        /// </description></item>
        /// <item><description>
        /// <c>a &lt; 0, b &gt; 0</c>: <c>x = r sec(t)</c>, added in
        /// <a href="https://github.com/asc-community/AngouriMath/pull/1277">#1277</a>. Its domain
        /// is two intervals rather than one, and the sign that makes the second one right is
        /// written out in <see cref="IntegrateAPowerTimesARadicalQuadratic"/>.
        /// </description></item>
        /// </list>
        /// <para>
        /// Both signs negative is a radicand negative everywhere, with no real integrand to
        /// integrate, and there is no fourth case.
        /// </para>
        /// <para>
        /// <b>Coming back.</b> The answer arrives in <c>sin(t)</c>, <c>cos(t)</c> and
        /// <c>tan(t)</c>, each of which is algebraic in <c>x</c> under the substitution, and in
        /// <c>t</c> itself wherever the recursion bottoms out on <c>int 1 dt</c> — which is where
        /// an <c>arcsin</c> or an <c>arctan</c> enters an otherwise algebraic answer.
        /// </para>
        /// <para>
        /// <b>Volunteered, and deliberately so</b> — this is the one rule of the five that is not
        /// scoped to <see cref="Integration.AnsweringTheQuestionAsked"/>. Scope is right for a
        /// rule whose answers only ever let some other search carry on into ground that was
        /// doomed, which is what
        /// <a href="https://github.com/asc-community/AngouriMath/issues/1265">#1265</a> is about.
        /// It is wrong for this one, because the sub-integrals it answers are the ones integration
        /// by parts asks for and then uses: <c>int x arcsin(x) dx</c> leaves
        /// <c>int x^2/sqrt(1 - x^2) dx</c>, which is squarely this rule's and was refused for
        /// sitting one level down. Ungating it is worth <b>ten</b> more of the Rubi sample and
        /// measures free — same wall clock, same timeouts, and the same 28.8 s over a probe of
        /// integrands that are declined either way.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveARadicalOfAQuadraticAsTrigonometric(
            Entity expr, Entity.Variable x, bool aWholePowerCounts = false)
        {
            if (!TryReadAPowerTimesARadicalQuadratic(expr, x, out var power, out var half,
                    out var constant, out var middle, out var quadratic, out var factor))
                return null;
            // An odd count is a genuine radical and is this rule's straight away. An even one is a
            // rational function, which partial fractions answers in a form people expect -- so it
            // reaches this only from the second call site, below that rule, with what it declined.
            // `1/(x (1 + x^2)^2)` is the shape that gets here: a repeated irreducible quadratic
            // beside a negative power of the variable, which the rational split does not take
            // apart and the tangent substitution turns into `sin^(-1) cos^3`.
            if (half % 2 == 0 && !aWholePowerCounts)
                return null;

            if (middle.IsZero)
                return TimesTheFactor(factor,
                    IntegrateAPowerTimesARadicalQuadratic(expr, x, power, half, constant, quadratic));

            // **Completing the square, and then the same rule again.** `y = x + b/(2c)` turns
            // `a + b x + c x^2` into `A + c y^2` with `A = a - b^2/(4c)`, which is the shape
            // above. What the shift costs is the power of the variable outside the radical:
            // `x^m` is `(y - h)^m`, a sum of `m + 1` terms each of which is this rule's shape
            // again, so the answer is their sum. That needs `m` to be a non-negative whole
            // number -- a negative one is not a finite sum -- which is why a negative power
            // beside a shifted quadratic is declined rather than shifted.
            if (power < 0)
                return null;
            var shift = middle / (ERational.FromInt32(2) * quadratic);
            var shiftedConstant = constant - middle * middle / (ERational.FromInt32(4) * quadratic);
            var shifted = (x + Number.Rational.Create(shift)).InnerSimplified;

            Entity total = 0;
            var binomial = EInteger.One;
            for (var j = power; j >= 0; j--)
            {
                var piece = IntegrateAPowerTimesARadicalQuadratic(
                    expr, shifted, j, half, shiftedConstant, quadratic);
                if (piece is null)
                    return null;
                total += Number.Integer.Create(binomial)
                       * MathS.Pow(-Number.Rational.Create(shift), power - j)
                       * piece;
                // C(m, j-1) from C(m, j): multiply by j and divide by m - j + 1.
                binomial = binomial * j / (power - j + 1);
            }
            return TimesTheFactor(factor, total);
        }

        /// <summary>Multiplies an answer by the constant taken out of the integrand, or keeps a decline.</summary>
        private static Entity? TimesTheFactor(Entity factor, Entity? answer)
            => answer is null ? null : (factor * answer).InnerSimplified;

        /// <summary>
        /// <c>int y^m (A + c y^2)^(k/2) dy</c>, written back in terms of
        /// <paramref name="inTermsOf"/> -- which is the variable itself where the quadratic had no
        /// linear term, and the shifted variable where it did.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Where the answer holds.</b> Each of the three substitutions is a bijection only on
        /// the interval where the radicand is positive, and that is where the integrand is real
        /// in the first place: everywhere for <c>A, c &gt; 0</c>, between the two roots for
        /// <c>c &lt; 0</c>, and outside them for <c>A &lt; 0</c>. The answer is an antiderivative
        /// on each such interval, which is the standing caveat on a trigonometric substitution
        /// and is not something this writes as a condition.
        /// </para>
        /// </remarks>
        private static Entity? IntegrateAPowerTimesARadicalQuadratic(
            Entity expr, Entity inTermsOf, int power, int half, ERational constant, ERational quadratic)
        {
            // Both coefficients decided and neither zero. Their two signs pick one of the three
            // substitutions, and there is no fourth: `A + c y^2` with both positive is
            // `A(1 + tan^2)`, with `c` negative it is `A(1 - sin^2)`, and with `A` negative it is
            // `|A|(sec^2 - 1)`. Both negative is a radicand that is negative everywhere, which is
            // not this rule's business.
            if (quadratic.IsZero || constant.IsZero
                || (constant.Sign < 0 && quadratic.Sign < 0))
                return null;

            var secant = constant.Sign < 0;
            // **The secant branch only for a genuine radical**, and that is a correctness bound
            // rather than a scope one. It is the one substitution whose domain is two intervals,
            // and both of them exclude `|y| < r`. Where the power is odd the integrand excludes
            // that interval too -- it is a square root of something negative there -- so the
            // answer is valid wherever the integrand is. Where the power is *even* the integrand
            // is an ordinary polynomial, real on the whole line, and an answer built from
            // `sqrt(y^2 - r^2)` is wrong on the middle: `(2x + 3x^2)^2` came back with the sign
            // reversed at `x = -0.5`, which is a wrong answer and not a missing condition.
            if (secant && half % 2 == 0)
                return null;
            var a = Number.Rational.Create(constant.Abs());
            var b = Number.Rational.Create(quadratic);
            var r = MathS.Sqrt(Number.Rational.Create(constant.Abs() / quadratic.Abs()));

            var t = Variable.CreateUnique(expr, "t_trig");
            // The three substitutions and what each leaves, with `r = sqrt(|A/c|)`:
            //
            //   A > 0, c > 0   y = r tan(t)   dy = r sec^2 dt   ->  sin^m cos^(-m-k-2)
            //   A > 0, c < 0   y = r sin(t)   dy = r cos dt     ->  sin^m cos^(k+1)
            //   A < 0, c > 0   y = r sec(t)   dy = r sec tan dt ->  sin^(k+1) cos^(-m-k-2)
            //
            // and the constant outside is `r^(m+1) |A|^(k/2)` in all three.
            var sinePower = ERational.FromInt32(secant ? half + 1 : power);
            var cosinePower = ERational.FromInt32(
                secant || quadratic.Sign > 0 ? -power - half - 2 : half + 1);
            var coefficient = MathS.Pow(r, power + 1) * MathS.Pow(a, Number.Rational.Create(half, 2));

            if (IntegrateAPowerOfSineTimesAPowerOfCosine(t, sinePower, cosinePower, 1, 1) is not { } inT)
                return null;

            // Back in terms of the variable. Each of the three is algebraic under the
            // substitution, so the answer stays algebraic except where the recursion bottoms out
            // on `int 1 dt` -- which is where the inverse function below enters.
            //
            // Under the secant substitution `tan(t)^2` is `y^2/r^2 - 1`, which is the radicand
            // over `|A|`, so the radical the integrand came in with is what goes back in.
            var radicand = ((secant ? -a : a) + b * MathS.Sqr(inTermsOf)).InnerSimplified;
            var radical = MathS.Sqrt(radicand);
            // **The secant branch is written for the right interval and reflected onto the left.**
            // It is the one substitution whose domain is two intervals, and the two are not the
            // same calculation: `(a tan(t)^2)^(k/2)` is `a^(k/2) |tan(t)|^k`, and `tan(t)` is
            // negative for `t` in `(pi/2, pi)`, which is where `y < -r` lands. Writing `tan(t)^k`
            // there is wrong by a sign for odd `k`, and the sign it is wrong by depends on the
            // power outside the radical as well, so patching the three replacements one at a time
            // does not converge -- measured, and it does not.
            //
            // What is true without cases: the integrand `y^m (b y^2 - a)^(k/2)` is even or odd
            // under `y -> -y` according to `m`, so an antiderivative `F` valid for `y > r` gives
            // the whole answer as `sign(y)^(m+1) F(|y|)`. So everything below is written in
            // `|y|`, where the substitution's own interval is the principal one and every sign is
            // positive, and the reflection is applied once at the end.
            var absolute = MathS.Abs(inTermsOf);
            var (sine, cosine, tangent) =
                secant
                ? (radical / (absolute * MathS.Sqrt(b)),
                   r / absolute,
                   radical / MathS.Sqrt(a))
                : quadratic.Sign > 0
                ? (inTermsOf * MathS.Sqrt(b) / radical,
                   MathS.Sqrt(a) / radical,
                   inTermsOf * MathS.Sqrt(b) / MathS.Sqrt(a))
                : (inTermsOf * MathS.Sqrt(-b) / MathS.Sqrt(a),
                   radical / MathS.Sqrt(a),
                   inTermsOf * MathS.Sqrt(-b) / radical);

            // `t` itself appears whenever the reduction bottoms out on `int 1 dt`, and that is
            // where an inverse trigonometric function enters an answer that is otherwise
            // algebraic -- `int sqrt(1 - x^2)/x^2 dx` holds an arcsine for exactly this reason.
            var angle = secant
                ? MathS.Arccos(r / absolute)
                : quadratic.Sign > 0
                ? MathS.Arctan(inTermsOf * MathS.Sqrt(b) / MathS.Sqrt(a))
                : MathS.Arcsin(inTermsOf * MathS.Sqrt(-b) / MathS.Sqrt(a));

            // **Two passes, and the order is load-bearing.** `Replace` rewrites a node's children
            // before the node, so a single pass that also matched a bare `t` turned `tan(t)` into
            // `tan(arccos(...))` -- the parent no longer matched once its child had been replaced.
            // That answer is correct and cannot be evaluated: `EvalNumerical` throws on it, so a
            // caller gets an antiderivative it can differentiate and not one it can use. The
            // trigonometric functions of `t` go first, and only what is left of `t` becomes the
            // angle. https://github.com/asc-community/AngouriMath/issues/718
            var answer = inT
                .Replace(node => node switch
                {
                    Sinf(var inner) when inner == t => sine,
                    Cosf(var inner) when inner == t => cosine,
                    Tanf(var inner) when inner == t => tangent,
                    _ => node
                })
                .Replace(node => node is Variable v && v == t ? angle : node);
            if (answer.ContainsNode(t))
                return null;
            // The reflection, once: `sign(y)^(m+1)`, which is `sign(y)` for an even power outside
            // the radical and nothing for an odd one. A factor constant on each interval changes
            // an antiderivative by a constant there and nothing else.
            if (secant && power % 2 == 0)
                answer *= MathS.Signum(inTermsOf);
            return (coefficient * answer).InnerSimplified;
        }

        /// <summary>
        /// Reads <paramref name="expr"/> as a rational multiple of
        /// <c>x^m (a + b x^2)^(k/2)</c> with <c>k</c> odd, however the radical is spelled.
        /// </summary>
        /// <remarks>
        /// The half-power is what makes this rule's rather than the ordinary power rule's: an even
        /// <c>k</c> is a whole power of a polynomial, which expanding answers. The quadratic has
        /// no linear term, so a shifted one — <c>x/sqrt(1 + x + x^2)</c> — is not read here and
        /// wants completing the square first, which is its own step.
        /// </remarks>
        private static bool TryReadAPowerTimesARadicalQuadratic(
            Entity expr, Entity.Variable x, out int power, out int half,
            out ERational constant, out ERational middle, out ERational quadratic, out Entity factor)
        {
            power = 0;
            half = 0;
            constant = ERational.Zero;
            middle = ERational.Zero;
            quadratic = ERational.Zero;
            factor = 1;

            Entity? radicandFound = null;
            var halfFound = 0;
            var powerFound = 0;
            Entity constantFactor = 1;
            var read = Read(expr, 1);
            if (!read || radicandFound is null)
                return false;
            if (!TreeAnalyzer.TryGetPolyQuadratic(radicandFound, x, out var square, out var linear, out var free))
                return false;
            if (free.Evaled is not Number.Rational freeValue
                || linear.Evaled is not Number.Rational linearValue
                || square.Evaled is not Number.Rational squareValue)
                return false;

            power = powerFound;
            half = halfFound;
            constant = freeValue.ERational;
            middle = linearValue.ERational;
            quadratic = squareValue.ERational;
            factor = constantFactor;
            return !quadratic.IsZero;

            bool Read(Entity node, int multiplicity)
            {
                switch (node)
                {
                    case Variable v when v == x:
                        powerFound += multiplicity;
                        return true;
                    case Mulf(var left, var right):
                        return Read(left, multiplicity) && Read(right, multiplicity);
                    case Divf(var above, var below):
                        return Read(above, multiplicity) && Read(below, -multiplicity);
                    // A whole power of the quadratic, counted on the same scale -- `Q^2` is
                    // `Q^(4/2)`. Whether an even count is acceptable is the caller's to decide,
                    // and the two callers decide differently: a radical is this rule's before
                    // anything else has tried, and a whole power is only its if the rational
                    // machinery has already declined.
                    case Powf(var @base, Number.Integer whole)
                        when @base.ContainsNode(x) && @base != x && whole.EInteger.CanFitInInt32():
                        if (radicandFound is not null && radicandFound != @base)
                            return false;
                        radicandFound = @base;
                        halfFound += multiplicity * 2 * whole.EInteger.ToInt32Checked();
                        return true;
                    // The radical, in any odd half power: sqrt(Q), Q^(3/2), 1/Q^(5/2).
                    case Powf(var @base, Number.Rational exponent)
                        when @base.ContainsNode(x)
                             && exponent.ERational.Denominator.Equals(EInteger.FromInt32(2))
                             && exponent.ERational.Numerator.CanFitInInt32():
                        if (radicandFound is not null && radicandFound != @base)
                            return false;
                        radicandFound = @base;
                        halfFound += multiplicity * exponent.ERational.Numerator.ToInt32Checked();
                        return true;
                    // The quadratic written out rather than raised, which is the power one.
                    case Sumf or Minusf when node.ContainsNode(x):
                        if (radicandFound is not null && radicandFound != node)
                            return false;
                        radicandFound = node;
                        halfFound += multiplicity * 2;
                        return true;
                    // A whole power of the variable, written as one.
                    case Powf(var @base, Number.Integer exponent)
                        when @base == x && exponent.EInteger.CanFitInInt32():
                        powerFound += multiplicity * exponent.EInteger.ToInt32Checked();
                        return true;
                    case Number.Rational rational when node is not Powf:
                        constantFactor = multiplicity > 0
                            ? constantFactor * MathS.Pow(rational, multiplicity)
                            : constantFactor / MathS.Pow(rational, -multiplicity);
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Reads <paramref name="expr"/> as a rational multiple of <c>sin(u)^p cos(u)^q</c> over
        /// one common argument, however the six trigonometric functions spell it.
        /// </summary>
        /// <remarks>
        /// A tangent contributes <c>(+1, -1)</c>, a secant <c>(0, -1)</c>, a cosecant
        /// <c>(-1, 0)</c> and a cotangent <c>(-1, +1)</c>, so a product of any of them over one
        /// argument is a pair of integers. Anything else in the product — a second argument, a
        /// non-integer power, a function of the variable that is not one of the six — makes the
        /// whole read fail, because a rule that guesses at the part it did not recognise answers
        /// a question it was not asked.
        /// </remarks>
        private static bool TryReadSineCosinePowers(
            Entity expr, out Entity argument, out ERational sinePower, out ERational cosinePower, out Entity factor)
        {
            Entity? common = null;
            var sine = ERational.Zero;
            var cosine = ERational.Zero;
            Entity constant = 1;
            var read = Read(expr, ERational.One);
            argument = common ?? 0;
            sinePower = sine;
            cosinePower = cosine;
            factor = constant;
            return read && common is not null && !(sine.IsZero && cosine.IsZero);

            bool Agrees(Entity candidate)
            {
                if (common is null)
                {
                    common = candidate;
                    return true;
                }
                return common == candidate;
            }

            bool Read(Entity node, ERational multiplicity)
            {
                switch (node)
                {
                    case Sinf(var a): sine += multiplicity; return Agrees(a);
                    case Cosf(var a): cosine += multiplicity; return Agrees(a);
                    case Tanf(var a): sine += multiplicity; cosine -= multiplicity; return Agrees(a);
                    case Cotanf(var a): sine -= multiplicity; cosine += multiplicity; return Agrees(a);
                    case Secantf(var a): cosine -= multiplicity; return Agrees(a);
                    case Cosecantf(var a): sine -= multiplicity; return Agrees(a);
                    case Mulf(var left, var right):
                        return Read(left, multiplicity) && Read(right, multiplicity);
                    case Divf(var above, var below):
                        return Read(above, multiplicity) && Read(below, -multiplicity);
                    // A rational exponent, not only a whole one: `sin(x)/sqrt(cos(x)^3)` is
                    // `sin cos^(-3/2)`, and the substitution that answers it does not care
                    // whether the exponent it carries along is whole — the power rule takes
                    // <c>u^(-3/2)</c> as readily as <c>u^(-3)</c>. Which of the three cases
                    // applies still turns on a whole exponent, and that is checked there.
                    // Not a fractional power of an even power, though: `(sin^2)^(3/2)` is
                    // `|sin|^3`, and read as `sin^3` it was integrated as one, wrong on every
                    // other half-turn; `sqrt(a sin^2)^5`, `1/sqrt(a cot^2)`, `(csc^2)^(3/2)`,
                    // `x sqrt(sin^2)` were four of Rubi's answered so.
                    // https://github.com/asc-community/AngouriMath/issues/1387
                    case Powf(var @base, Number.Rational power) when power is not Number.Integer && HasAnEvenPowerOfATrigonometricFunction(@base):
                        return false;
                    case Powf(var @base, Number.Rational power):
                        return Read(@base, multiplicity * power.ERational);
                    // A rational factor rides along; anything else is declined rather than
                    // carried, since carrying it would claim the rest of the product is
                    // trigonometric when it has not been read. A rational *factor* and a
                    // rational *exponent* are told apart by the case above matching first.
                    case Number.Rational rational when node is not Powf:
                        constant = IsWholeAndFitsAnInt(multiplicity, out var times)
                            ? times > 0
                                ? constant * MathS.Pow(rational, times)
                                : constant / MathS.Pow(rational, -times)
                            : constant * MathS.Pow(rational, Number.Rational.Create(multiplicity));
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Whether an even whole power of a sine, cosine, tangent, cotangent, secant or
        /// cosecant stands anywhere in <paramref name="expr"/>: a fractional power of it is a
        /// power of the function's modulus, not of the function.
        /// </summary>
        private static bool HasAnEvenPowerOfATrigonometricFunction(Entity expr)
            => expr.Nodes.Any(node => node is Powf(TrigonometricFunction, Number.Integer even) && even.EInteger.IsEven && !even.EInteger.IsZero);

        /// <summary>
        /// Whether <paramref name="value"/> is a whole number small enough to count with, and
        /// what that number is.
        /// </summary>
        private static bool IsWholeAndFitsAnInt(ERational value, out int whole)
        {
            var lowest = value.ToLowestTerms();
            if (lowest.Denominator.Equals(EInteger.One) && lowest.Numerator.CanFitInInt32())
            {
                whole = lowest.Numerator.ToInt32Checked();
                return true;
            }
            whole = 0;
            return false;
        }

        /// <summary>
        /// Whether <paramref name="value"/> is an odd whole number of at least one — the exponent
        /// that lets one factor of the function go into <c>du</c> and leaves an even power, which
        /// is a polynomial in the other function.
        /// </summary>
        private static bool IsOddAndPositive(ERational value)
            => IsWholeAndFitsAnInt(value, out var whole) && whole >= 1 && whole % 2 != 0;

        /// <summary>Whether <paramref name="value"/> is an even whole number.</summary>
        private static bool IsEvenWhole(ERational value)
            => IsWholeAndFitsAnInt(value, out var whole) && whole % 2 == 0;

        /// <summary>
        /// The whole number <paramref name="value"/> is, for a value one of the two tests above
        /// has already said is whole.
        /// </summary>
        private static int WholeOf(ERational value)
            => value.ToLowestTerms().Numerator.ToInt32Checked();

        /// <summary>
        /// Reads <paramref name="expr"/> as a whole power of a secant or a cosecant, however it
        /// is written: as the node, as a negative power of cosine or sine, or as one over a
        /// positive power of them.
        /// </summary>
        private static bool TryReadReciprocalTrigonometricPower(
            Entity expr, out Entity argument, out int power, out bool isSecant)
        {
            argument = 0;
            power = 0;
            isSecant = false;
            switch (expr)
            {
                case Secantf(var secantArgument):
                    (argument, power, isSecant) = (secantArgument, 1, true);
                    return true;
                case Cosecantf(var cosecantArgument):
                    (argument, power, isSecant) = (cosecantArgument, 1, false);
                    return true;
                case Powf(Secantf(var raisedSecant), Number.Integer secantPower)
                    when secantPower.EInteger.Sign > 0 && secantPower.EInteger.CanFitInInt32():
                    (argument, power, isSecant) =
                        (raisedSecant, secantPower.EInteger.ToInt32Checked(), true);
                    return true;
                case Powf(Cosecantf(var raisedCosecant), Number.Integer cosecantPower)
                    when cosecantPower.EInteger.Sign > 0 && cosecantPower.EInteger.CanFitInInt32():
                    (argument, power, isSecant) =
                        (raisedCosecant, cosecantPower.EInteger.ToInt32Checked(), false);
                    return true;
                // cos(u)^(-n) is sec(u)^n, and sin(u)^(-n) is csc(u)^n.
                case Powf(Cosf(var loweredCosine), Number.Integer negativeCosine)
                    when negativeCosine.EInteger.Sign < 0 && negativeCosine.EInteger.CanFitInInt32():
                    (argument, power, isSecant) =
                        (loweredCosine, -negativeCosine.EInteger.ToInt32Checked(), true);
                    return true;
                case Powf(Sinf(var loweredSine), Number.Integer negativeSine)
                    when negativeSine.EInteger.Sign < 0 && negativeSine.EInteger.CanFitInInt32():
                    (argument, power, isSecant) =
                        (loweredSine, -negativeSine.EInteger.ToInt32Checked(), false);
                    return true;
                // And one over a positive power, which is the same thing spelled as a quotient.
                case Divf(var one, var below) when one == Number.Integer.One:
                    return TryReadTrigonometricDenominator(below, ref argument, ref power, ref isSecant);
                default:
                    return false;
            }
        }

        private static bool TryReadTrigonometricDenominator(
            Entity below, ref Entity argument, ref int power, ref bool isSecant)
        {
            switch (below)
            {
                case Cosf(var cosine):
                    (argument, power, isSecant) = (cosine, 1, true);
                    return true;
                case Sinf(var sine):
                    (argument, power, isSecant) = (sine, 1, false);
                    return true;
                case Powf(Cosf(var cosine), Number.Integer cosinePower)
                    when cosinePower.EInteger.Sign > 0 && cosinePower.EInteger.CanFitInInt32():
                    (argument, power, isSecant) =
                        (cosine, cosinePower.EInteger.ToInt32Checked(), true);
                    return true;
                case Powf(Sinf(var sine), Number.Integer sinePower)
                    when sinePower.EInteger.Sign > 0 && sinePower.EInteger.CanFitInInt32():
                    (argument, power, isSecant) =
                        (sine, sinePower.EInteger.ToInt32Checked(), false);
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// An integrand holding an inverse trigonometric function of the variable itself,
        /// integrated by the substitution that undoes it: <c>x = sin(u)</c> for <c>arcsin(x)</c>,
        /// <c>x = tan(u)</c> for <c>arctan(x)</c>, <c>x = cos(u)</c> and <c>x = sec(u)</c> for the
        /// other two.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>e^(arcsin(x)) x^3/sqrt(1 - x^2)</c> had no antiderivative. Under <c>x = sin(u)</c> it
        /// is <c>e^u sin(u)^3</c>, which the product-to-sum rewrite and the closed
        /// exponential-times-trigonometric rule answer between them; <c>x arcsec(x)/sqrt(x^2 - 1)</c>
        /// is <c>u sec(u)^2</c> under <c>x = sec(u)</c>, one step of parts. The general
        /// substitution does not find these because it substitutes for a subtree and asks what
        /// is left, and what is left here is <c>x</c> itself, which only the <b>inverse</b>
        /// substitution removes -- the same reason <see cref="SolveByLogarithmSubstitution"/>
        /// exists beside it.
        /// </para>
        /// <para>
        /// <b>The radical goes by construction.</b> <c>sqrt(1 - x^2)</c> under <c>x = sin(u)</c>
        /// is <c>cos(u)</c> on the principal branch, where <c>u</c> lies in <c>[-pi/2, pi/2]</c>
        /// and the cosine is not negative; substituting and simplifying instead would leave
        /// <c>sqrt(1 - sin(u)^2)</c>, which the simplifier is right not to call <c>cos(u)</c>.
        /// So <c>(1 - x^2)^(k/2)</c> becomes <c>cos(u)^k</c> directly, and likewise
        /// <c>(1 + x^2)^(k/2)</c> into <c>sec(u)^k</c> for the tangent and <c>(x^2 - 1)^(k/2)</c>
        /// into <c>tan(u)^k</c> for the secant, each on its principal branch. That branch is the
        /// one the inverse function is defined on, so the answer holds wherever the integrand
        /// is read through it.
        /// </para>
        /// <para>
        /// Exactly one inverse function, of the variable or of a linear <c>c x + d</c> in it,
        /// and nothing left in <c>x</c> afterwards; otherwise declined. Handed to the
        /// integrator in <c>u</c>, where the trigonometric rules are. With the linear,
        /// <c>x = (sin(u) - d)/c</c> and <c>dx = cos(u) du/c</c>, and the radical that goes
        /// is of a constant multiple of <c>1 - (c x + d)^2</c> -- Rubi's
        /// <c>(d - c^2 d x^2)^p (a + b arcsin(c x))^n</c> -- taken as that multiple's root
        /// times the cosine, the generic case. A power of <c>a + b arcsin(c x)</c> counts
        /// as a power of the inverse: <c>x arcsin(a x)^2</c> under the sine is
        /// <c>u^2 sin(u) cos(u)/a^2</c>, parts twice against <c>sin(2u)</c>, where parts in
        /// <c>x</c> stalled on <c>x^2 arcsin(a x)/sqrt(1 - a^2 x^2)</c>.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveByInverseTrigonometricSubstitution(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            // The one inverse function, of the variable or of a linear in it.
            Entity? inverse = null;
            foreach (var node in expr.Nodes)
            {
                if (node is not (Arcsinf or Arccosf or Arctanf or Arcsecantf or Arccosecantf or Arccotanf))
                    continue;
                if (inverse is not null && inverse != node)
                    return null;
                inverse = node;
            }
            if (inverse is null)
                return null;
            var argument = inverse.DirectChildren.First();
            if (!argument.ContainsNode(x))
                return null;
            Entity slope = Number.Integer.One, offset = Number.Integer.Zero;
            var linear = argument != x;
            if (linear)
            {
                if (!TreeAnalyzer.TryGetPolyLinear(argument, x, out var readSlope, out var readOffset)
                    || readSlope is null || readOffset is null || TreeAnalyzer.IsZero(readSlope) || readSlope.Evaled is Number.Complex { IsZero: true })
                    return null;
                (slope, offset) = (readSlope, readOffset);
            }

            var u = Variable.CreateUnique(expr, "u_inv");
            // The sign of u, where the root's sign is that: a constant on each half of the
            // range, carried through the integration as a symbol and written back as the sign
            // of x, which it is for the cosecant and the cotangent.
            var signOfU = Variable.CreateUnique(expr, "s_inv");
            // x in terms of u, dx/du, the quadratic whose root goes by construction and what it
            // becomes, and the way back.
            Entity xInU, dxdu, radicandBase, root;
            // Written for the argument; with a linear argument, x itself is `(L - d)/c` and
            // dx is `dL/c`.
            switch (inverse)
            {
                case Arcsinf:
                    (xInU, dxdu, radicandBase, root) = (MathS.Sin(u), MathS.Cos(u), 1 - MathS.Sqr(argument), MathS.Cos(u));
                    break;
                case Arccosf:
                    (xInU, dxdu, radicandBase, root) = (MathS.Cos(u), -MathS.Sin(u), 1 - MathS.Sqr(argument), MathS.Sin(u));
                    break;
                case Arctanf:
                    (xInU, dxdu, radicandBase, root) = (MathS.Tan(u), MathS.Sqr(MathS.Sec(u)), 1 + MathS.Sqr(argument), MathS.Sec(u));
                    break;
                case Arccotanf:
                    // The range here is (-pi/2, pi/2], where the cosecant is not negative on
                    // the positive half and negative on the other: the root is kept as `|csc|`,
                    // which is `csc(u) sgn(u)`, so the answer holds on both.
                    (xInU, dxdu, radicandBase, root) = (MathS.Cotan(u), -MathS.Sqr(MathS.Cosec(u)), 1 + MathS.Sqr(argument), MathS.Cosec(u) * signOfU);
                    break;
                case Arccosecantf:
                    // arccsc has the range [-pi/2, 0) ∪ (0, pi/2], where the cotangent has the
                    // sign of u: `sqrt(x^2 - 1)` is `cot(u) sgn(u)`.
                    (xInU, dxdu, radicandBase, root) = (MathS.Cosec(u), -MathS.Cosec(u) * MathS.Cotan(u), MathS.Sqr(argument) - 1, MathS.Cotan(u) * signOfU);
                    break;
                default:
                    (xInU, dxdu, radicandBase, root) = (MathS.Sec(u), MathS.Sec(u) * MathS.Tan(u), MathS.Sqr(argument) - 1, MathS.Tan(u));
                    break;
            }
            if (linear)
            {
                xInU = ((xInU - offset) / slope).InnerSimplified;
                dxdu = dxdu / slope;
            }

            // What the substitution is for: a radical of the quadratic it removes, or an
            // exponential of the inverse function, which it turns into `e^u`. With neither the
            // integrand is by parts' -- `x arcsin(x)` is `u sin(u) cos(u)` under the sine, an
            // answer in `sin(2 arcsin(x))` where parts gives one in `x` -- and the time spent
            // here is spent on nothing: `x arctan(x)^2 ln(1 + x^2)` cost 0.6 s of it.
            var radicalsRemoved = 0;
            var rewritten = expr
                .Substitute(inverse, u)
                .Replace(node =>
                {
                    // A whole power of the quadratic from the second up counts as well: parts
                    // in x on `x (d - c^2 d x^2)^3 (a + b arcsin(c x))` leaves a seventh-degree
                    // polynomial over the root and went past its budget there, where under
                    // the sine it is a polynomial in the sine and cosine beside `a + b u`.
                    var wholePower = node is Powf(var wholeBase, Number.Integer whole) && whole.EInteger.CompareTo(EInteger.FromInt32(2)) >= 0
                        && whole.EInteger.CompareTo(EInteger.FromInt32(64)) <= 0 && wholeBase.ContainsNode(x) ? whole.EInteger.ToInt32Unchecked() : 0;
                    if (wholePower > 0 && node is Powf(var quadratic, _) && TreeAnalyzer.TryGetPolyQuadratic(quadratic, x, out _, out _, out _)
                        && TryReadAConstantMultipleOf(quadratic, radicandBase, x) is { } wholeMultiple)
                    {
                        radicalsRemoved++;
                        return MathS.Pow(wholeMultiple, wholePower) * MathS.Pow(root, 2 * wholePower);
                    }
                    if (!TryReadAHalfPower(node, out var @base, out var numerator))
                        return node;
                    if (IsTheSameQuadratic(@base, radicandBase, x))
                    {
                        radicalsRemoved++;
                        return MathS.Pow(root, numerator);
                    }
                    // A constant multiple of the quadratic, `d - c^2 d x^2` for `1 - (c x)^2`:
                    // the multiple's root times the root of the quadratic, the generic case.
                    if (linear || TreeAnalyzer.TryGetPolyQuadratic(@base, x, out _, out _, out _))
                    {
                        if (TryReadAConstantMultipleOf(@base, radicandBase, x) is { } multiple)
                        {
                            radicalsRemoved++;
                            return MathS.Pow(multiple, Number.Rational.Create(numerator, 2)) * MathS.Pow(root, numerator);
                        }
                    }
                    return node;
                });
            var exponentialOfTheInverse = expr.Nodes.Any(node =>
                node is Powf(var @base, var power) && !@base.ContainsNode(x) && power.ContainsNode(inverse));
            // Or a power of the inverse function above the first: parts on `x^3 arccsc(x)^2`
            // leaves `x^2 arccsc(x)/sqrt(x^2 - 1)` and stalls, where under `x = csc(u)` it is
            // `-u^2 csc(u)^4 cot(u)`, two steps of parts against a power of the cosecant.
            // Or of a function of it alone -- `(a + b arcsin(c x))^2` -- which is a power of
            // u after the substitution.
            var powerOfTheInverse = expr.Nodes.Any(node =>
                node is Powf(var @base, Number.Integer power) && power.EInteger.CompareTo(EInteger.One) > 0
                    && @base.ContainsNode(inverse) && !@base.Substitute(inverse, u).ContainsNode(x));
            if (radicalsRemoved == 0 && !exponentialOfTheInverse && !powerOfTheInverse)
                return null;
            // The cosecant and the cotangent carry the sign of u into the root, and a first
            // power of either beside a root is parts' -- `arccot(x)/(1 + x^2)^(3/2)` is
            // `x arccot(x)/sqrt(1 + x^2) + 1/sqrt(1 + x^2)` there, with no sign in it.
            if (inverse is Arccosecantf or Arccotanf && !exponentialOfTheInverse && !powerOfTheInverse)
                return null;
            // And nothing else of `x` under a root, which the construction did not reach:
            // `x^3 arcsin(x)/sqrt(1 - x^4)` under the sine is a root of `1 - sin(u)^4`, worse
            // than what it came from, and 1.4 s of declining it.
            if (rewritten.Nodes.Any(node =>
                    node is Powf(var @base, Number.Rational power) && power is not Number.Integer && @base.ContainsNode(x)))
                return null;
            rewritten = rewritten.Substitute(x, xInU);
            if (rewritten.ContainsNode(x))
                return null;

            // As one quotient with the common factors cancelled: `x/sqrt(1 - x^2)` under the
            // sine leaves `sin(u) cos(u)/cos(u)`, and the closed exponential-times-trigonometric
            // rule reads a cosine above the bar and not one below it.
            var (numerator, denominator) = Functions.SingleQuotient.Of((rewritten * dxdu).InnerSimplified);
            var integrand = CancelCommonFactors(numerator, denominator);
            if (integrand is Providedf(var inner, _))
                integrand = inner;
            // A cosecant or a secant left below the bar is a sine or a cosine above it:
            // `arccsc(x)^4/(x^2 sqrt(x^2 - 1))` under the cosecant is `-u^4/csc(u)`, which parts
            // does not read, and `-u^4 sin(u)`, which it does.
            if (integrand.Nodes.Any(node => node is Cosecantf or Secantf))
            {
                var asSinesAndCosines = integrand.Replace(node => node switch
                {
                    Cosecantf(var argument) => 1 / MathS.Sin(argument),
                    Secantf(var argument) => 1 / MathS.Cos(argument),
                    _ => node,
                });
                var (top, bottom) = Functions.SingleQuotient.Of(Functions.SingleQuotient.Combine(asSinesAndCosines));
                integrand = CancelCommonFactors(top, bottom);
                if (integrand is Providedf(var innerCombined, _))
                    integrand = innerCombined;
            }

            // The sign squared is one: an even power of it, from a whole power of the
            // quadratic, is folded before the question is asked, since `e^u sin(u)^4/s^6` is
            // not the exponential-times-trigonometric rule's and `e^u sin(u)^4` is.
            if (integrand.ContainsNode(signOfU))
                integrand = integrand
                    .Replace(node => node is Powf(var b, Number.Integer k) && b == signOfU ? (k.EInteger.IsEven ? Number.Integer.One : signOfU) : node)
                    .InnerSimplified;
            // The same question in another variable, not a step in the search for it: asked at
            // the top when this was, so the closed rules that answer only at the top --
            // `e^u sin(u)^3` is the exponential-times-trigonometric rule's -- are consulted.
            if (Integration.ComputeAsAQuestionOfItsOwn(integrand, u, integrateByParts) is not { } result)
                return null;
            if (result.ContainsNode(signOfU))
                // The sign squared is one, and the sign itself is the sign of x.
                result = result
                    .Replace(node => node is Powf(var b, Number.Integer k) && b == signOfU ? (k.EInteger.IsEven ? Number.Integer.One : signOfU) : node)
                    .Substitute(signOfU, MathS.Signum(x));
            return TrigonometryOfTheInverseInX(result.Substitute(u, inverse), inverse, argument);
        }

        /// <summary>
        /// The constant <c>k</c> with <paramref name="candidate"/> equal to
        /// <c>k</c> times <paramref name="wanted"/> as functions of <paramref name="x"/>,
        /// decided at sampled points and then read exactly from the quotient, else null.
        /// </summary>
        private static Entity? TryReadAConstantMultipleOf(Entity candidate, Entity wanted, Entity.Variable x)
        {
            if (!AreProportionalAtSampledPoints(candidate, wanted, x))
                return null;
            var ratio = Functions.PartialFractions.Bare((candidate / wanted).Simplify());
            if (ratio.ContainsNode(x) || ratio.Nodes.Any(node => node == MathS.NaN))
                return null;
            // A multiple that is known negative is refused: `sqrt(k Q)` is `sqrt(k) sqrt(Q)`
            // for a positive k and for a symbol taken as one, and for `k = -1` with a negative
            // Q it is off by a sign -- `arcosh(a x)^2/sqrt(1 - a^2 x^2)` came back with `i a`
            // below where the integrand is real.
            if (ratio.Evaled is Number.Complex evaluated && (evaluated is not Number.Real || evaluated is Number.Real { IsNegative: true }))
                return null;
            return ratio;
        }

        /// <summary>
        /// An exponential of <c>i</c> times an inverse trigonometric function, written
        /// algebraically: <c>e^(i arctan(L))</c> is <c>(1 + i L)/sqrt(1 + L^2)</c>,
        /// <c>e^(i arcsin(L))</c> is <c>sqrt(1 - L^2) + i L</c> and <c>e^(i arccos(L))</c> is
        /// <c>L + i sqrt(1 - L^2)</c>, each on the principal branch for a real <c>L</c>, and
        /// <c>e^(n i f(L))</c> is that to the <c>n</c>th, for a rational <c>n</c>. Rubi's
        /// <c>e^(i arctan(a x))/sqrt(c + a^2 c x^2)</c> is then <c>(1 + i a x)/(sqrt(c) (1 + a^2 x^2))</c>,
        /// a rational function; under <c>x = tan(u)/a</c> it was <c>e^(i u) sec(u)</c>, which
        /// nothing in <c>u</c> reads. The integrand as rewritten is asked as a question of
        /// its own, and the answer checked at sampled points where a symbol is involved.
        /// </summary>
        internal static Entity? SolveByWritingAnExponentialOfAnInverseAlgebraically(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            var rewrote = false;
            var rewritten = expr.Replace(node =>
            {
                if (node is not Powf(var @base, var exponent) || @base != MathS.e || !exponent.ContainsNode(x))
                    return node;
                // The exponent as n i times one inverse function of a real argument.
                Entity? inverse = null;
                foreach (var inner in exponent.Nodes)
                    if (inner is Arctanf or Arcsinf or Arccosf)
                    {
                        if (inverse is not null && inverse != inner)
                            return node;
                        inverse = inner;
                    }
                if (inverse is null || inverse.DirectChildren.First() is not { } argument || !argument.ContainsNode(x))
                    return node;
                var placeholder = Variable.CreateUnique(exponent, "u_exp");
                var inThePlaceholder = exponent.Replace(inner => inner == inverse ? placeholder : inner);
                if (inThePlaceholder.ContainsNode(x)
                    || !TreeAnalyzer.TryGetPolyLinear(inThePlaceholder, placeholder, out var coefficient, out var constantPart)
                    || coefficient is null || constantPart is null || constantPart.Evaled is not Number.Complex { IsZero: true })
                    return node;
                if ((coefficient / MathS.i).InnerSimplified.Evaled is not Number.Rational n)
                    return node;
                rewrote = true;
                // The tangent's as two powers, `(1 + i L)^n (1 + L^2)^(-n/2)`, which the
                // radical rules read where a power of the quotient is one node to them.
                if (inverse is Arctanf)
                {
                    var halfPower = Number.Rational.Create(n.ERational.Negate().Divide(2));
                    return (n == Number.Integer.One ? 1 + MathS.i * argument : MathS.Pow(1 + MathS.i * argument, n)) * MathS.Pow(1 + MathS.Sqr(argument), halfPower);
                }
                Entity unit = inverse is Arcsinf
                    ? MathS.Sqrt(1 - MathS.Sqr(argument)) + MathS.i * argument
                    : argument + MathS.i * MathS.Sqrt(1 - MathS.Sqr(argument));
                return n == Number.Integer.One ? unit : MathS.Pow(unit, n);
            });
            if (!rewrote)
                return null;
            // A fractional power of a constant multiple of the quadratic the rewrite brought
            // in, as the multiple's power times the quadratic's, the generic case, so that
            // `(1 + a^2 x^2)^(-1/2)` and `(c + a^2 c x^2)^(-1/2)` meet as one power:
            // `e^(i arctan(a x))/sqrt(c + a^2 c x^2)` is `(1 + i a x)/(sqrt(c) (1 + a^2 x^2))`.
            var quadratics = new List<Entity>();
            foreach (var node in rewritten.Nodes)
                if (node is Powf(var q, Number.Rational r) && r is not Number.Integer && q.ContainsNode(x) && TreeAnalyzer.TryGetPolyQuadratic(q, x, out _, out _, out _)
                    && !quadratics.Contains(q))
                    quadratics.Add(q);
            if (quadratics.Count > 1)
            {
                var first = quadratics[0];
                rewritten = rewritten.Replace(node =>
                    node is Powf(var q, Number.Rational r) && r is not Number.Integer && q != first && quadratics.Contains(q)
                    && TryReadAConstantMultipleOf(q, first, x) is { } multiple
                        ? MathS.Pow(multiple, r) * MathS.Pow(first, r)
                        : node);
            }
            if (Integration.ComputeAsAQuestionOfItsOwn(rewritten.InnerSimplified, x, integrateByParts) is not { } answer)
                return null;
            if (answer.Nodes.Any(node => node == MathS.NaN))
                return null;
            // Checked at sampled points where a symbol is involved -- a point where the
            // derivative cannot be evaluated, a fractional power of a complex number at a
            // symbol, is not a verdict -- and the simplified answer must not be NaN either:
            // a piecewise with complex coefficients simplified to NaN for
            // `x e^(-2 i arctan(a + b x))`, and an answer that simplifies to a claim of
            // non-existence is not given.
            if (expr.Vars.Any(symbol => symbol != x))
            {
                try
                {
                    if (!Functions.PartialFractions.HoldsAtSampledPoints(answer.Differentiate(x), expr, x))
                        return null;
                }
                catch (Core.Exceptions.CannotEvalException)
                {
                    return null;
                }
            }
            if (answer.Nodes.Any(node => node is Piecewise) && answer.Simplify().Nodes.Any(node => node == MathS.NaN))
                return null;
            return answer;
        }

        /// <summary>
        /// An integrand holding an inverse hyperbolic function of a linear in the variable,
        /// integrated by the substitution that undoes it. The inverse hyperbolic functions
        /// are not nodes here -- <c>arsinh(L)</c> is written <c>ln(L + sqrt(L^2 + 1))</c>,
        /// <c>arcosh(L)</c> is <c>ln(L + sqrt(L^2 - 1))</c> and <c>artanh(L)</c> is
        /// <c>ln((1 + L)/(1 - L))/2</c> -- so the logarithm is read for the function it is,
        /// and under <c>L = sinh(u)</c>, <c>cosh(u)</c> or <c>tanh(u)</c> it is <c>u</c>, the
        /// radical of <c>L^2 + 1</c> is <c>cosh(u)</c>, of <c>L^2 - 1</c> is <c>sinh(u)</c>
        /// (for <c>u</c> not negative, where the principal <c>arcosh</c> lies) and
        /// <c>1 - L^2</c> is <c>sech(u)^2</c>; a constant multiple of the quadratic is the
        /// multiple's power times that, the generic case. What is left is a function of
        /// <c>u</c> and of exponentials of it, which the exponential rules read: Rubi's
        /// <c>x arsinh(a x)</c> is <c>u sinh(2u)/(2 a^2)</c>, parts once, where parts in
        /// <c>x</c> left the logarithm's derivative as a quotient of radicals.
        /// The way back writes <c>e^u</c> as <c>L + sqrt(L^2 + 1)</c> and <c>e^(-u)</c> as
        /// <c>sqrt(L^2 + 1) - L</c>, on the principal branch.
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </summary>
        internal static Entity? SolveByInverseHyperbolicSubstitution(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            // The one inverse hyperbolic function in the integrand, read off its logarithm.
            Entity? inverse = null;
            Entity? argument = null;
            var kind = 0;   // 1 sinh, 2 cosh, 3 tanh
            foreach (var node in expr.Nodes)
            {
                if (node is not Logf(var @base, var antilogarithm) || @base != MathS.e || !antilogarithm.ContainsNode(x))
                    continue;
                var read = ReadAnInverseHyperbolic(antilogarithm, x);
                if (read is null)
                    return null;   // a logarithm of something else: not this route's
                if (inverse is not null && inverse != node)
                    return null;
                (inverse, argument, kind) = (node, read.Value.Argument, read.Value.Kind);
            }
            if (inverse is null || argument is null)
                return null;
            if (!TreeAnalyzer.TryGetPolyLinear(argument, x, out var slope, out var offset) || slope is null || offset is null
                || TreeAnalyzer.IsZero(slope) || slope.Evaled is Number.Complex { IsZero: true })
                return null;
            // The tangent's logarithm carries a half in front: `ln((1 + L)/(1 - L))/2` is u, and
            // the logarithm alone is 2u.
            var u = Variable.CreateUnique(expr, "u_inv");
            var expU = MathS.Pow(MathS.e, u);
            var expMinusU = MathS.Pow(MathS.e, -u);
            var sinh = (expU - expMinusU) / 2;
            var cosh = (expU + expMinusU) / 2;
            Entity argumentInU, dArgument, radicandBase, root;
            switch (kind)
            {
                case 1:
                    (argumentInU, dArgument, radicandBase, root) = (sinh, cosh, MathS.Sqr(argument) + 1, cosh);
                    break;
                case 2:
                    (argumentInU, dArgument, radicandBase, root) = (cosh, sinh, MathS.Sqr(argument) - 1, sinh);
                    break;
                default:
                    (argumentInU, dArgument, radicandBase, root) = (sinh / cosh, 1 / MathS.Sqr(cosh), 1 - MathS.Sqr(argument), 1 / cosh);
                    break;
            }
            var logarithmInU = kind == 3 ? 2 * u : u;

            var radicalsRemoved = 0;
            var rewritten = expr
                .Substitute(inverse, logarithmInU)
                .Replace(node =>
                {
                    if (node is Powf(var wholeBase, Number.Integer whole) && whole.EInteger.CompareTo(EInteger.FromInt32(2)) >= 0
                        && whole.EInteger.CompareTo(EInteger.FromInt32(64)) <= 0 && wholeBase.ContainsNode(x)
                        && TreeAnalyzer.TryGetPolyQuadratic(wholeBase, x, out _, out _, out _)
                        && TryReadAConstantMultipleOf(wholeBase, radicandBase, x) is { } wholeMultiple)
                    {
                        radicalsRemoved++;
                        return MathS.Pow(wholeMultiple, whole) * MathS.Pow(root, 2 * whole.EInteger.ToInt32Unchecked());
                    }
                    if (!TryReadAHalfPower(node, out var @base, out var numerator))
                        return node;
                    if (IsTheSameQuadratic(@base, radicandBase, x))
                    {
                        radicalsRemoved++;
                        return MathS.Pow(root, numerator);
                    }
                    if (TreeAnalyzer.TryGetPolyQuadratic(@base, x, out _, out _, out _) && TryReadAConstantMultipleOf(@base, radicandBase, x) is { } multiple)
                    {
                        radicalsRemoved++;
                        return MathS.Pow(multiple, Number.Rational.Create(numerator, 2)) * MathS.Pow(root, numerator);
                    }
                    return node;
                });
            var powerOfTheInverse = expr.Nodes.Any(node =>
                node is Powf(var @base, Number.Integer power) && power.EInteger.CompareTo(EInteger.One) > 0
                    && @base.ContainsNode(inverse) && !@base.Substitute(inverse, u).ContainsNode(x));
            var exponentialOfTheInverse = expr.Nodes.Any(node =>
                node is Powf(var @base, var power) && !@base.ContainsNode(x) && power.ContainsNode(inverse));
            // Any other root of x is one the construction did not reach, and worse in u.
            if (rewritten.Nodes.Any(node =>
                    node is Powf(var @base, Number.Rational power) && power is not Number.Integer && @base.ContainsNode(x)))
                return null;
            // Taken whenever the inverse is there, unlike the trigonometric route: parts in x
            // does not read the logarithm's derivative, a quotient of radicals, so there is no
            // answer in x to prefer -- unless another function of x stands beside it, which
            // the exponentials of u do not read: `x arctan(x) arsinh(x)/sqrt(1 + x^2)` is
            // `u sinh(u) arctan(sinh(u))` under the sine, ten seconds of search that found
            // nothing, where parts in x answers it in seventy milliseconds.
            _ = (radicalsRemoved, powerOfTheInverse, exponentialOfTheInverse);
            if (rewritten.Nodes.Any(node => node is Function && node is not Powf && node != inverse && node.ContainsNode(x)))
                return null;
            rewritten = rewritten.Substitute(x, ((argumentInU - offset) / slope).InnerSimplified);
            if (rewritten.ContainsNode(x))
                return null;
            var (numerator, denominator) = Functions.SingleQuotient.Of((rewritten * dArgument / slope).InnerSimplified);
            var integrand = CancelCommonFactors(numerator, denominator);
            if (integrand is Providedf(var inner, _))
                integrand = inner;
            // A product of powers of the hyperbolic functions is a product of powers of sums
            // of exponentials, which no rule reads whole; expanded, it is a sum of `u^n e^(k u)`
            // terms, each closed. Only where the denominator is free of u, since expanding a
            // quotient's numerator alone helps nothing.
            if (integrand is not Divf(_, var below) || !below.ContainsNode(u))
            {
                var expanded = integrand.Expand();
                if (expanded is Sumf)
                    integrand = expanded;
            }
            if (Integration.ComputeAsAQuestionOfItsOwn(integrand, u, integrateByParts) is not { } result)
                return null;
            // The way back: u is the inverse function, e^u and e^(-u) its two exponentials in
            // the argument, on the principal branch.
            Entity plus, minus;
            switch (kind)
            {
                case 1:
                    (plus, minus) = (argument + MathS.Sqrt(MathS.Sqr(argument) + 1), MathS.Sqrt(MathS.Sqr(argument) + 1) - argument);
                    break;
                case 2:
                    (plus, minus) = (argument + MathS.Sqrt(MathS.Sqr(argument) - 1), argument - MathS.Sqrt(MathS.Sqr(argument) - 1));
                    break;
                default:
                    // e^(-u) as the reciprocal, not as the root of the reciprocal: off the real
                    // domain the principal roots of a quotient and of its reciprocal are not
                    // reciprocals, and the check at sampled points found the difference.
                    (plus, minus) = (MathS.Sqrt((1 + argument) / (1 - argument)), 1 / MathS.Sqrt((1 + argument) / (1 - argument)));
                    break;
            }
            var back = result
                .Replace(node => node switch
                {
                    Powf(var b, var e) when b == MathS.e && e == u => plus,
                    Powf(var b, var e) when b == MathS.e && e.ContainsNode(u) && TreeAnalyzer.TryGetPolyLinear(e, u, out var k, out var m) && k is { } && m is { } && !k.ContainsNode(u) && !m.ContainsNode(u)
                        => k.Evaled is Number.Integer { EInteger.Sign: < 0 } ? MathS.Pow(MathS.e, m) * MathS.Pow(minus, -k) : MathS.Pow(MathS.e, m) * MathS.Pow(plus, k),
                    _ => node,
                })
                .Substitute(u, kind == 3 ? inverse / 2 : inverse);
            if (back.ContainsNode(u) || back.Nodes.Any(node => node == MathS.NaN))
                return null;
            // Checked on the real domain of the inverse, where the identities the route rests
            // on hold -- with the symbols pinned to values near one, the argument is inside
            // (-1, 1) at small x for the tangent and past 1 at large x for the cosine -- and
            // then at the default points as well, some of them off that domain: an integrand
            // can be real there through two imaginary factors cancelling, `e^artanh(a x)` over
            // `sqrt(1 - a^2 x^2)` past `a x = 1`, and an answer that holds only on the domain
            // is a wrong answer at every such point. Declined rather than given there.
            var points = kind switch
            {
                2 => new[] { "1.43", "3.17", "2.2", "5.1" },
                3 => new[] { "0.29", "-0.61", "0.13", "-0.4" },
                _ => null,
            };
            var derivative = back.Differentiate(x);
            try
            {
                if (points is { } && !Functions.PartialFractions.HoldsAtSampledPoints(derivative, expr, x, points))
                    return null;
                if (!Functions.PartialFractions.HoldsAtSampledPoints(derivative, expr, x))
                    return null;
            }
            catch (Core.Exceptions.CannotEvalException)
            {
                return null;
            }
            return back;
        }

        /// <summary>
        /// The argument and the kind (1 <c>arsinh</c>, 2 <c>arcosh</c>, 3 <c>artanh</c>) of the
        /// inverse hyperbolic function whose logarithm has <paramref name="antilogarithm"/>
        /// as its argument -- <c>L + sqrt(L^2 + 1)</c>, <c>L + sqrt(L^2 - 1)</c> or
        /// <c>(1 + L)/(1 - L)</c> for a linear <c>L</c> -- else null.
        /// </summary>
        private static (Entity Argument, int Kind)? ReadAnInverseHyperbolic(Entity antilogarithm, Entity.Variable x)
        {
            if (antilogarithm is Sumf(var left, var right))
            {
                foreach (var (linear, radical) in new[] { (left, right), (right, left) })
                {
                    if (!TryReadAHalfPower(radical, out var radicand, out var numeratorOfHalf) || numeratorOfHalf != 1 || !linear.ContainsNode(x))
                        continue;
                    if (!TreeAnalyzer.TryGetPolyLinear(linear, x, out var slope, out _) || slope is null || TreeAnalyzer.IsZero(slope))
                        continue;
                    if (IsTheSameQuadraticOnceSimplified(radicand, MathS.Sqr(linear) + 1, x))
                        return (linear, 1);
                    if (IsTheSameQuadraticOnceSimplified(radicand, MathS.Sqr(linear) - 1, x))
                        return (linear, 2);
                }
                return null;
            }
            if (antilogarithm is Divf(var above, var below)
                && TreeAnalyzer.TryGetPolyLinear(above, x, out var slopeAbove, out var offsetAbove) && slopeAbove is { } && offsetAbove is { }
                && TreeAnalyzer.TryGetPolyLinear(below, x, out var slopeBelow, out var offsetBelow) && slopeBelow is { } && offsetBelow is { }
                && VanishesOnceSimplified(slopeAbove + slopeBelow)
                && VanishesOnceSimplified(offsetAbove - 1)
                && VanishesOnceSimplified(offsetBelow - 1))
                return ((slopeAbove * x).InnerSimplified, 3);
            return null;

            // Zero as written, evaluated, or once simplified: `a + (-a)` for a symbol a.
            static bool VanishesOnceSimplified(Entity expr)
                => expr.InnerSimplified.Evaled is Number.Complex { IsZero: true }
                    || expr.Vars.Any() && Functions.PartialFractions.Bare(expr.Simplify()).Evaled is Number.Complex { IsZero: true };

            // The same quadratic coefficient by coefficient, with symbols: `(c x)^2 + 1`
            // against `c^2 x^2 + 1`, which the evaluated difference does not settle.
            static bool IsTheSameQuadraticOnceSimplified(Entity candidate, Entity wanted, Entity.Variable x)
                => TreeAnalyzer.TryGetPolyQuadratic(candidate, x, out var a, out var b, out var c)
                   && TreeAnalyzer.TryGetPolyQuadratic(wanted, x, out var a2, out var b2, out var c2)
                   && a is { } && b is { } && c is { } && a2 is { } && b2 is { } && c2 is { }
                   && VanishesOnceSimplified(a - a2) && VanishesOnceSimplified(b - b2) && VanishesOnceSimplified(c - c2);
        }

        /// <summary>
        /// A trigonometric function of the inverse function of <paramref name="x"/> written
        /// in <paramref name="x"/>, on the principal branch: <c>cos(arcsin(x))</c> is
        /// <c>sqrt(1 - x^2)</c> there, since the cosine is not negative on
        /// <c>[-pi/2, pi/2]</c>; <c>tan(arcsec(x))</c> is <c>x sqrt(1 - 1/x^2)</c>, the sine
        /// not negative on <c>[0, pi]</c>; and the cosine of <c>arccot(x)</c>, whose range
        /// here is <c>(-pi/2, pi/2]</c>, is <c>|x|/sqrt(1 + x^2)</c>. Whatever is not one of
        /// these compositions, a multiple angle among them, is left as it is.
        /// </summary>
        private static Entity TrigonometryOfTheInverseInX(Entity result, Entity inverse, Entity x)
        {
            Entity sine, cosine;
            switch (inverse)
            {
                case Arcsinf:
                    (sine, cosine) = (x, MathS.Sqrt(1 - MathS.Sqr(x)));
                    break;
                case Arccosf:
                    (sine, cosine) = (MathS.Sqrt(1 - MathS.Sqr(x)), x);
                    break;
                case Arctanf:
                    (sine, cosine) = (x / MathS.Sqrt(1 + MathS.Sqr(x)), 1 / MathS.Sqrt(1 + MathS.Sqr(x)));
                    break;
                case Arccotanf:
                    (sine, cosine) = (MathS.Abs(x) / (x * MathS.Sqrt(1 + MathS.Sqr(x))), MathS.Abs(x) / MathS.Sqrt(1 + MathS.Sqr(x)));
                    break;
                case Arcsecantf:
                    (sine, cosine) = (MathS.Sqrt(1 - 1 / MathS.Sqr(x)), 1 / x);
                    break;
                case Arccosecantf:
                    (sine, cosine) = (1 / x, MathS.Sqrt(1 - 1 / MathS.Sqr(x)));
                    break;
                default:
                    return result;
            }
            var written = result.Replace(node => node switch
            {
                Sinf(var argument) when argument == inverse => sine,
                Cosf(var argument) when argument == inverse => cosine,
                Tanf(var argument) when argument == inverse => sine / cosine,
                Cotanf(var argument) when argument == inverse => cosine / sine,
                Secantf(var argument) when argument == inverse => 1 / cosine,
                Cosecantf(var argument) when argument == inverse => 1 / sine,
                _ => node,
            });
            return written == result ? result : written.InnerSimplified;
        }

        /// <summary>
        /// <c>base^(n/2)</c> in either of its spellings, <c>(base)^(3/2)</c> or <c>sqrt(base)^3</c>:
        /// the second is the first on the principal branch, since <c>Log(sqrt(z)) = Log(z)/2</c>
        /// exactly, so <c>(z^(1/2))^n = z^(n/2)</c> for every integer <c>n</c>.
        /// </summary>
        private static bool TryReadAHalfPower(Entity node, out Entity @base, out int numerator)
        {
            @base = 0;
            numerator = 0;
            switch (node)
            {
                case Powf(var inner, Number.Rational half)
                    when half is not Number.Integer
                         && half.ERational.Denominator.Equals(EInteger.FromInt32(2))
                         && half.ERational.Numerator.CanFitInInt32():
                    @base = inner;
                    numerator = half.ERational.Numerator.ToInt32Unchecked();
                    return true;
                case Powf(Powf(var inner, Number.Rational half), Number.Integer power)
                    when half is not Number.Integer
                         && half.ERational.Denominator.Equals(EInteger.FromInt32(2))
                         && half.ERational.Numerator.CanFitInInt32()
                         && power.EInteger.CanFitInInt32():
                    @base = inner;
                    numerator = half.ERational.Numerator.ToInt32Unchecked() * power.EInteger.ToInt32Unchecked();
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Whether <paramref name="candidate"/> is the quadratic <paramref name="wanted"/> in
        /// <paramref name="x"/>, by coefficients rather than by spelling.
        /// </summary>
        private static bool IsTheSameQuadratic(Entity candidate, Entity wanted, Entity.Variable x)
            => TreeAnalyzer.TryGetPolyQuadratic(candidate, x, out var a, out var b, out var c)
               && TreeAnalyzer.TryGetPolyQuadratic(wanted, x, out var a2, out var b2, out var c2)
               && (a - a2).Evaled is Number.Complex { IsZero: true }
               && (b - b2).Evaled is Number.Complex { IsZero: true }
               && (c - c2).Evaled is Number.Complex { IsZero: true };

        /// <summary>
        /// An integrand that is a function of <c>ln(x)</c> and of nothing else, integrated by the
        /// substitution <c>u = ln(x)</c>, under which <c>dx</c> is <c>e^u du</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>sin(ln(x))</c> had no antiderivative, and it is one substitution away from one that
        /// does: under <c>u = ln(x)</c> it becomes <c>e^u sin(u)</c>, which is the cyclic
        /// by-parts integral and is closed by
        /// <see cref="SolveAPolynomialTimesAnExponentialAndATrigonometric"/>. The same for
        /// <c>ln(x)^n</c> beside anything, and for <c>e^(1/ln(x))</c> and its kin.
        /// </para>
        /// <para>
        /// <b>The substitution is an inverse one</b>, which is what makes it different from every
        /// other substitution here and why the general one does not find it. The others divide by
        /// <c>du/dx</c> and ask what is left; this one goes the other way — <c>x = e^u</c>, so
        /// <c>dx</c> is <c>e^u du</c>, and the exponential is <b>introduced</b> rather than
        /// cancelled. That only pays because the integrator answers exponentials times almost
        /// anything.
        /// </para>
        /// <para>
        /// <b>The test is the rewrite itself</b>: replace every <c>ln(x)</c> and see whether an
        /// <c>x</c> survives. <c>ln(x) + x</c> keeps one and is declined, which is right — it is
        /// not a function of the logarithm alone. A different base is read through first, since
        /// <c>log(b, x)</c> is <c>ln(x)/ln(b)</c> and declining it for the spelling would be the
        /// defect this file has had repeatedly.
        /// </para>
        /// <para>
        /// <b>Where the answer holds.</b> <c>u = ln(x)</c> is a bijection from the positive reals
        /// to the whole line, so the answer is an antiderivative for <c>x &gt; 0</c> — which is
        /// where an integrand built from <c>ln(x)</c> is real in the first place. Nothing is
        /// assumed that the integrand did not already assume.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveByLogarithmSubstitution(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            // A logarithm to another base first: log(b, x) is ln(x)/ln(b), and a rule that
            // declined it for having a different node would be answering the spelling.
            expr = expr.Replace(node =>
                node is Logf(var @base, var argument) && !@base.ContainsNode(x) && @base != MathS.e
                    ? MathS.Ln(argument) / MathS.Ln(@base)
                    : node);

            var logarithm = MathS.Ln(x);
            if (!expr.ContainsNode(logarithm))
                return null;

            var u = Variable.CreateUnique(expr, "u_log");
            var inU = expr.Substitute(logarithm, u);
            if (inU.ContainsNode(x))
                return null;

            var integrand = Functions.SingleQuotient.Combine(inU * MathS.Pow(MathS.e, u)).InnerSimplified;
            if (integrand is Providedf(var inner, _))
                integrand = inner;

            return Integration.ComputeIndefiniteIntegral(integrand, u, integrateByParts) is { } result
                ? result.Substitute(u, MathS.Ln(x))
                : null;
        }

        /// <summary>
        /// A quotient of two <b>homogeneous</b> polynomials in <c>sin(u)</c> and <c>cos(u)</c>,
        /// integrated by <c>t = tan(u)</c> — which turns it into a rational function of <c>t</c>
        /// whenever the two degrees differ by an even number.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>1/(cos(x) + sin(x))^6</c> took <b>231 seconds</b> to be declined and
        /// <c>1/(b^2 cos(x)^2 + a^2 sin(x)^2)</c> was one of the Rubi sample's three timeouts.
        /// Both are this shape, and so is <c>sin(x)/(cos(x)^3 + sin(x)^3)</c>.
        /// </para>
        /// <para>
        /// <b>Why the degrees decide it.</b> Writing <c>t = tan(u)</c> and <c>c = cos(u)</c>, a
        /// homogeneous polynomial of degree <c>n</c> in the pair is <c>c^n</c> times a polynomial
        /// in <c>t</c> alone. So a quotient of degrees <c>n</c> over <c>d</c> is
        /// <c>c^(n-d) N(t)/D(t)</c>, and with <c>c^2 = 1/(1 + t^2)</c> and
        /// <c>dx = dt/(a(1 + t^2))</c>:
        /// </para>
        /// <code>
        ///     f dx = [N(t)/D(t)] (1 + t^2)^((d - n)/2 - 1) dt / a
        /// </code>
        /// <para>
        /// which is rational exactly when <c>d - n</c> is even. Odd is a genuine boundary rather
        /// than a first cut: there the substitution leaves a square root of <c>1 + t^2</c> behind,
        /// which is a different problem and not a rational one.
        /// </para>
        /// <para>
        /// This is the third of Bioche's rules, and it is the one the other two do not cover:
        /// <see cref="SolveByTangentSubstitution"/> answers an integrand that is a function of
        /// <c>tan</c> <i>alone</i>, and <c>1/(b^2 cos^2 + a^2 sin^2)</c> is not — it is a function
        /// of <c>tan</c> times <c>sec^2</c>, which is what the degree difference of two is saying.
        /// </para>
        /// <para>
        /// <b>Where the answer holds.</b> <c>t = tan(u)</c> is a bijection on each interval
        /// between the poles of the tangent, so the answer is an antiderivative on each of them —
        /// the standing caveat on this substitution, shared with the half-angle one, and not
        /// something this writes as a condition.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveByHomogeneousTrigonometricSubstitution(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            if (!TryReadAsQuotient(expr, out var numerator, out var denominator))
                return null;
            if (!TryReadAHomogeneousTrigonometricPolynomial(numerator, x, out var above, out var aboveDegree, out var argument)
                || !TryReadAHomogeneousTrigonometricPolynomial(denominator, x, out var below, out var belowDegree, out var otherArgument))
                return null;
            // A side free of the variable is degree zero and names no argument -- `1/(cos + sin)^2`
            // is the common case and was declined for it. Only one side may be silent; if both
            // are, there is no integrand here.
            argument ??= otherArgument;
            otherArgument ??= argument;
            if (argument is null || argument != otherArgument)
                return null;
            var difference = belowDegree - aboveDegree;
            if (difference % 2 != 0)
                return null;   // an odd difference leaves a root of 1 + t^2, which is not rational
            if (!TreeAnalyzer.TryGetPolyLinear(argument, x, out var rate, out _) || rate.Evaled == 0)
                return null;

            var t = Variable.CreateUnique(expr, "t_homog");
            var inT = Functions.SingleQuotient.Combine(
                above.Substitute(HomogeneousTangent, t).Substitute(HomogeneousCosine, 1)
                / below.Substitute(HomogeneousTangent, t).Substitute(HomogeneousCosine, 1)
                * MathS.Pow(1 + MathS.Sqr(t), Number.Integer.Create(difference / 2 - 1))
                / rate).Simplify();
            if (inT is Providedf(var inner, _))
                inT = inner;
            if (inT.ContainsNode(x))
                return null;

            return Integration.ComputeIndefiniteIntegral(inT, t, integrateByParts) is { } result
                ? result.Substitute(t, MathS.Tan(argument))
                : null;
        }

        /// <summary>The two placeholders a homogeneous polynomial is rewritten over.</summary>
        [ConstantField] private static readonly Entity.Variable HomogeneousTangent =
            Entity.Variable.CreateVariableOrConstant("__homogeneous_tangent");

        /// <summary>See <see cref="HomogeneousTangent"/>.</summary>
        [ConstantField] private static readonly Entity.Variable HomogeneousCosine =
            Entity.Variable.CreateVariableOrConstant("__homogeneous_cosine");

        /// <summary>
        /// Reads <paramref name="expr"/> as a homogeneous polynomial in <c>sin(u)</c> and
        /// <c>cos(u)</c> over one common argument, rewritten with <c>sin(u)</c> as
        /// <c>tangent * cosine</c> and <c>cos(u)</c> as <c>cosine</c> so that the caller can put
        /// <c>cosine</c> to one and read off the polynomial in the tangent.
        /// </summary>
        /// <remarks>
        /// Homogeneous means every term has the same total degree, and that is checked rather
        /// than assumed: <c>1 + cos(u)</c> has terms of degree zero and one and is declined, which
        /// is right — it is not this substitution's, and the half-angle one answers it.
        /// </remarks>
        private static bool TryReadAHomogeneousTrigonometricPolynomial(
            Entity expr, Entity.Variable x, out Entity rewritten, out int degree, out Entity? argument)
        {
            rewritten = expr;
            degree = 0;
            argument = null;

            var found = (Entity?)null;
            var theDegree = (int?)null;
            // The terms with their degrees, for the parity case below.
            var terms = new List<(Entity Term, int Degree)>();
            var expanded = expr.Expand();
            foreach (var term in Entity.Sumf.LinearChildren(expanded))
            {
                var thisDegree = 0;
                foreach (var factor in Entity.Mulf.LinearChildren(term))
                {
                    var (piece, power) = factor is Powf(var @base, Number.Integer whole)
                                         && whole.EInteger.CanFitInInt32()
                        ? (@base, whole.EInteger.ToInt32Checked())
                        : (factor, 1);
                    // The secant and the cosecant are the cosine and the sine to the power
                    // minus one, and the tangent and the cotangent are of degree zero, so a
                    // sum with them in it is homogeneous or not by the same count:
                    // Timofeev's `1/(2 sec(x) + sin(x))^2` has `4 sec^2 + 4 sec sin + sin^2`
                    // below, of degrees -2, 0 and 2, homogeneous up to parity.
                    var (argumentOfThis, degreeOfThis) = piece switch
                    {
                        Sinf(var a) => (a, power),
                        Cosf(var a) => (a, power),
                        Secantf(var a) => (a, -power),
                        Cosecantf(var a) => (a, -power),
                        Tanf(var a) => (a, 0),
                        Cotanf(var a) => (a, 0),
                        _ => (null, 0),
                    };
                    // A trigonometric function free of x is a coefficient: `cos(a)` beside
                    // `sin(x)`, as the unifier writes `sin(x - a)` out, is not a second argument.
                    if (argumentOfThis is null || !argumentOfThis.ContainsNode(x))
                    {
                        if (piece.ContainsNode(x))
                            return false;   // anything else in the term is not this shape
                        continue;
                    }
                    if (found is not null && found != argumentOfThis) return false;
                    found = argumentOfThis;
                    thisDegree += degreeOfThis;
                }
                terms.Add((term, thisDegree));
                if (theDegree is null)
                    theDegree = thisDegree;
                else if (theDegree != thisDegree)
                    theDegree = -1;   // not homogeneous as written; the parity case below
            }
            if (theDegree is null)
                return false;
            if (found is null)
            {
                // No sine or cosine at all: a constant, which is homogeneous of degree zero and
                // needs no rewriting. It names no argument, and the caller takes the other side's.
                degree = 0;
                return !expr.ContainsNode(x);
            }

            // **Homogeneous up to parity is homogeneous**: a term two degrees short of the
            // highest is the same term times `sin^2 + cos^2`, which is one. `sin + sin^2 cos`
            // has degrees one and three and is `sin (sin^2 + cos^2) + sin^2 cos`, of degree
            // three throughout; it is what `cos(x)/(sin(x)(2 + sin(2x)))` has below the bar once
            // its arguments are unified, and it was declined here for the spelling. A term an
            // odd number short is a genuine boundary -- no power of one bridges it.
            var highest = terms.Max(pair => pair.Degree);
            if (theDegree == -1)
            {
                if (terms.Any(pair => (highest - pair.Degree) % 2 != 0))
                    return false;
                var pythagorean = MathS.Sqr(MathS.Sin(found)) + MathS.Sqr(MathS.Cos(found));
                Entity raised = Number.Integer.Zero;
                foreach (var (term, thisDegree) in terms)
                {
                    var levels = (highest - thisDegree) / 2;
                    var lifted = levels == 0 ? term : term * MathS.Pow(pythagorean, levels);
                    raised = raised == Number.Integer.Zero ? lifted : raised + lifted;
                }
                expanded = raised.Expand();
            }

            degree = highest;
            argument = found;
            rewritten = expanded.Replace(node => node switch
            {
                Sinf(var a) when a == found => HomogeneousTangent * HomogeneousCosine,
                Cosf(var a) when a == found => HomogeneousCosine,
                Secantf(var a) when a == found => 1 / HomogeneousCosine,
                Cosecantf(var a) when a == found => 1 / (HomogeneousTangent * HomogeneousCosine),
                Tanf(var a) when a == found => HomogeneousTangent,
                Cotanf(var a) when a == found => 1 / HomogeneousTangent,
                _ => node
            });
            return true;
        }

        /// <summary>
        /// An integrand that is a function of <c>tan(x)</c> and of nothing else, integrated by
        /// the substitution <c>u = tan(x)</c>, under which <c>dx</c> is <c>du/(1 + u^2)</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The substitution that turns every rational function of the tangent into a rational
        /// function, and <c>int sqrt(tan(x))</c> — the last of the five integrals
        /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a> lists — into
        /// <c>int sqrt(u)/(1 + u^2) du</c>.
        /// </para>
        /// <para>
        /// <b>Why this is not a candidate in <see cref="SolveBySubstitution"/>.</b> That one
        /// divides the integrand by <c>du/dx</c> and asks whether any <c>x</c> is left, which
        /// works when the substitution survives the division. Here it does not:
        /// <c>sqrt(tan(x))</c> over the derivative of <c>sqrt(tan(x))</c> is
        /// <c>2 tan(x) cos(x)^2</c>, which is <c>sin(2x)</c> and is simplified to it — a correct
        /// answer to a question that has stopped being about the tangent. Rewriting the integrand
        /// asks a different question and does not lose the shape.
        /// </para>
        /// <para>
        /// The test is the rewrite itself: replace every <c>tan(x)</c> and see whether an
        /// <c>x</c> survives. <c>tan(x) + x</c> keeps one and is declined, which is right — it is
        /// not a function of the tangent alone. <b>The cotangent is covered</b>, by writing it as
        /// <c>1/tan</c> on the way in: it is the same function the other way up, and declining it
        /// for having its own node meant <c>tan(x)^2</c> was answered and <c>cot(x)^2</c> was not.
        /// </para>
        /// <para>
        /// <b>No condition is owed by the substitution</b>, but the answer inherits the
        /// tangent's: <c>u = tan(x)</c> is undefined exactly where the integrand is, since the
        /// integrand is a function of it, and <c>1 + u^2</c> is never zero for real <c>u</c>.
        /// </para>
        /// </remarks>
        internal static Entity? SolveByTangentSubstitution(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            // The cotangent first, because it is the tangent written the other way up and this
            // rule used to decline it for no better reason than the node being a different one.
            // `tan(x)^2` was answered and `cot(x)^2` was not, though the second is `1/tan(x)^2`.
            //
            // cot(u) = 1/tan(u) wherever either is defined: both are cos(u)/sin(u), and the
            // points where the quotient is written differently -- tan undefined at odd multiples
            // of pi/2, where cot is zero -- are removable in the same way for both, so this
            // neither widens nor narrows the domain of an integrand built from them.
            expr = expr.Replace(node =>
                node is Cotanf(var cotangentArgument)
                    ? 1 / MathS.Tan(cotangentArgument)
                    : node);

            // An odd root of a product of powers of the sine and the cosine is the product of
            // the roots, exactly on the reals: the odd root is the real one, an odd function,
            // so `(ab)^(1/3)` is `a^(1/3) b^(1/3)` whatever the signs, and `(s^k)^(p/q)` is
            // `s^(kp/q)` under the same convention. Timofeev's
            // `((sin/cos^7)^(1/3) - 3 tan)/(cos^5 sin)^(2/3)` is
            // `(sin^(1/3) cos^(-7/3) - 3 tan)/(cos^(10/3) sin^(2/3))` so, a polynomial in the
            // cube root of the tangent under the substitution, and was a search past its
            // budget as written. Only here, where the factors are read next: split in the
            // open chain the same roots sent `(sin cos^2)^(1/3)` into a thirty-second search.
            expr = expr.Replace(node =>
                node is Powf(var product, Number.Rational fraction) && fraction is not Number.Integer
                && !fraction.ERational.Denominator.IsEven && product is Mulf or Divf
                && WithTheOddRootOnEachFactor(product, fraction, x) is { } apart
                    ? apart
                    : node);

            var tangent = MathS.Tan(x);
            // Or a sine or a cosine under a root, for the writing by the sign below; a rational
            // function of those is the half-angle substitution's.
            if (!expr.ContainsNode(tangent) && !(HasARadicalOf(expr, x) && expr.Nodes.Any(node => node is Sinf or Cosf && node.ContainsNode(x))))
                return null;

            // An even power of the secant, cosine, sine or cosecant of x is a rational function
            // of the tangent, exactly and wherever it is defined: `sec^2 = 1 + tan^2`,
            // `cos^2 = 1/(1 + tan^2)`, `sin^2 = tan^2/(1 + tan^2)`. Timofeev's
            // `(sec^2 - 3 tan sqrt(4 sec^2 + 5 tan^2))/(sin^2 (4 sec^2 + 5 tan^2)^(3/2))` is
            // `((1 + u^2) - 3u sqrt(4 + 9u^2))/(u^2 (4 + 9u^2)^(3/2))` under the tangent, and was
            // declined for the secant and the sine that survived the rewriting. An odd power is
            // not a function of the tangent -- its sign is not -- and is left, to survive and
            // decline as before.
            var secantSquared = 1 + MathS.Sqr(tangent);
            expr = expr.Replace(node => node switch
            {
                Powf(Secantf(var a), Number.Integer even) when a == x && even.EInteger.IsEven && even.EInteger.Sign > 0
                    => MathS.Pow(secantSquared, even.EInteger.ToInt32Checked() / 2),
                Powf(Cosf(var a), Number.Integer even) when a == x && even.EInteger.IsEven && even.EInteger.Sign > 0
                    => MathS.Pow(secantSquared, -(even.EInteger.ToInt32Checked() / 2)),
                Powf(Sinf(var a), Number.Integer even) when a == x && even.EInteger.IsEven && even.EInteger.Sign > 0
                    => MathS.Pow(MathS.Sqr(tangent) / secantSquared, even.EInteger.ToInt32Checked() / 2),
                Powf(Cosecantf(var a), Number.Integer even) when a == x && even.EInteger.IsEven && even.EInteger.Sign > 0
                    => MathS.Pow(secantSquared / MathS.Sqr(tangent), even.EInteger.ToInt32Checked() / 2),
                _ => node,
            });

            // The double angle is the tangent's too, exactly: `sin(2x) = 2 tan/(1 + tan^2)`,
            // `cos(2x) = (1 - tan^2)/(1 + tan^2)`, `tan(2x) = 2 tan/(1 - tan^2)` and the
            // cotangent its reciprocal -- Timofeev's `sqrt(tan(x) tan(2x))` and `sqrt(cot(2x)/cot(x))`.
            static bool IsTwice(Entity argument, Entity.Variable x)
                => TreeAnalyzer.TryGetPolyLinear(argument, x, out var slope, out var intercept)
                   && slope.Evaled == Number.Integer.Create(2) && intercept.Evaled == Number.Integer.Zero;
            expr = expr.Replace(node => node switch
            {
                Sinf(var a) when IsTwice(a, x) => 2 * tangent / secantSquared,
                Cosf(var a) when IsTwice(a, x) => (1 - MathS.Sqr(tangent)) / secantSquared,
                Tanf(var a) when IsTwice(a, x) => 2 * tangent / (1 - MathS.Sqr(tangent)),
                Cotanf(var a) when IsTwice(a, x) => (1 - MathS.Sqr(tangent)) / (2 * tangent),
                Cotanf(var a) when a == x => 1 / tangent,
                _ => node,
            });

            var uSub = Variable.CreateUnique(expr, "u_tan");
            var inU = expr.Substitute(tangent, uSub);
            if (inU.ContainsNode(x))
            {
                // An odd power of the sine or the cosine is its sign times a function of the
                // tangent: `cos(x) = sgn(cos(x))/sqrt(1 + tan^2)` and `sin(x) = tan(x) cos(x)`,
                // the sign a constant between the zeros of the cosine. Timofeev's
                // `sin(x)^7/sin(2x)^(7/2)` is `sgn(cos(x)) u^(7/2)/(2^(7/2) (1 + u^2))` so, a
                // binomial under `t = sqrt(u)`; the sign goes back in as `sgn(cos(x))`. Only
                // where that leaves the tangent alone and something algebraic in it.
                var sign = Variable.CreateUnique(expr, "sgn_cos");
                var rootOfSecantSquared = MathS.Sqrt(secantSquared);
                var odd = expr.Replace(node => node switch
                {
                    Cosf(var a) when a == x => sign / rootOfSecantSquared,
                    Sinf(var a) when a == x => sign * tangent / rootOfSecantSquared,
                    Powf(Cosf(var a), Number.Integer k) when a == x && k.EInteger.CanFitInInt32() && !k.EInteger.IsEven
                        => MathS.Pow(sign / rootOfSecantSquared, k),
                    Powf(Sinf(var a), Number.Integer k) when a == x && k.EInteger.CanFitInInt32() && !k.EInteger.IsEven
                        => MathS.Pow(sign * tangent / rootOfSecantSquared, k),
                    _ => node,
                }).Substitute(tangent, uSub);
                if (odd.ContainsNode(x) || !IsAlgebraicIn(odd, uSub))
                    return null;
                // The sign is 1 or -1, and the integrand is even or odd in it: with the sign
                // one, the function of u to integrate, and the sign in front where the two
                // differ. Decided at a point, since the two are the same expression up to it.
                var withOne = Functions.PartialFractions.Bare(odd.Substitute(sign, Number.Integer.One));
                var withMinusOne = Functions.PartialFractions.Bare(odd.Substitute(sign, Number.Integer.MinusOne));
                // With every symbol pinned to a rational too: Moses's `sqrt(A^2 + B^2 sin(x)^2)/sin(x)`
                // was an exception out of the evaluation, with A and B still in it.
                var at = Number.Rational.Create(37, 100);
                var atOne = withOne.Substitute(uSub, at);
                var atMinusOne = withMinusOne.Substitute(uSub, at);
                var pinned = 0;
                foreach (var symbol in atOne.Vars.Concat(atMinusOne.Vars).Distinct().ToList())
                {
                    var value = Number.Rational.Create(7 + 4 * pinned++, 3);
                    atOne = atOne.Substitute(symbol, value);
                    atMinusOne = atMinusOne.Substitute(symbol, value);
                }
                if (atOne.Evaled is not Number.Complex valueWithOne || atMinusOne.Evaled is not Number.Complex valueWithMinusOne)
                    return null;
                bool oddInTheSign;
                if ((valueWithOne - valueWithMinusOne).Abs() < 1e-9 * (1 + valueWithOne.Abs()))
                    oddInTheSign = false;
                else if ((valueWithOne + valueWithMinusOne).Abs() < 1e-9 * (1 + valueWithOne.Abs()))
                    oddInTheSign = true;
                else
                    return null;
                // A fractional power of a quotient by `1 + u^2`, or of a positive number times
                // something, comes apart exactly: the denominator is positive and so is the
                // number, and a positive real factor leaves the argument of the rest alone.
                for (var round = 0; round < 4; round++)
                {
                    var apart = withOne.Replace(node =>
                    {
                        if (node is not Powf(var @base, Number.Rational fraction) || fraction is Number.Integer || !@base.ContainsNode(uSub))
                            return node;
                        var (top, bottom) = Functions.SingleQuotient.Of(@base);
                        if (bottom != Number.Integer.One && IsPositiveForReal(bottom, uSub))
                            return MathS.Pow(top, fraction) * MathS.Pow(bottom, Number.Rational.Create(fraction.ERational.Negate()));
                        if (@base is Mulf && Mulf.LinearChildren(@base).FirstOrDefault(factor => !factor.ContainsNode(uSub) && factor.Evaled is Number.Real { IsPositive: true }) is { } positive)
                        {
                            var rest = Mulf.LinearChildren(@base).Where(factor => factor != positive).Aggregate(Number.Integer.One as Entity, (product, factor) => product * factor);
                            return MathS.Pow(positive, fraction) * MathS.Pow(rest, fraction);
                        }
                        return node;
                    });
                    if (apart == withOne)
                        break;
                    withOne = apart;
                }
                var signed = Functions.SingleQuotient.Combine(withOne / secantSquared.Substitute(tangent, uSub)).Simplify();
                if (signed is Providedf(var bareSigned, _))
                    signed = bareSigned;
                if (signed.ContainsNode(x) || signed.Nodes.Any(node => node == MathS.NaN))
                    return null;
                if (Integration.ComputeIndefiniteIntegral(signed, uSub, integrateByParts) is not { } signedResult
                    || signedResult.Nodes.Any(node => node == MathS.NaN))
                    return null;
                var back = signedResult.Substitute(uSub, tangent);
                return oddInTheSign ? MathS.Signum(MathS.Cos(x)) * back : back;
            }

            // Combined into one quotient, not merely inner-simplified. Writing the cotangent as
            // 1/tan puts a quotient inside a quotient -- cot(x)^2 arrives as (1/u)^2/(1 + u^2) --
            // and `InnerSimplified` leaves that nesting standing. Everything downstream wants a
            // single Divf of two polynomials, and it is the difference between an answer and
            // none: `1/(u^2*(1 + u^2))` is integrated and `(1/u)^2/(1 + u^2)`, which is the same
            // expression, is not. The half-angle and exponential substitutions comb their
            // integrands for the same reason.
            var integrand = Functions.SingleQuotient
                .Combine(inU / (1 + MathS.Sqr(uSub))).Simplify();
            if (integrand is Providedf(var withoutCondition, _))
                integrand = withoutCondition;
            return Integration.ComputeIndefiniteIntegral(integrand, uSub, integrateByParts) is { } result
                ? result.Substitute(uSub, tangent)
                : null;
        }
        /// <summary>
        /// <c>(prod f_i^(k_i))^(p/q)</c>, <c>q</c> odd, as <c>prod f_i^(k_i p/q)</c> where every
        /// <c>f_i</c> is a trigonometric function of <paramref name="x"/> itself or free of it,
        /// and the powers of the sine and the cosine that come out sum to an even integer --
        /// then the product is <c>tan^a (1 + tan^2)^k</c> with <c>k</c> whole, a function of the
        /// tangent. Null otherwise: <c>(2 sin/cos^3)^(1/3)</c> is <c>(2 u (1 + u^2))^(1/3)</c> read
        /// whole and is answered so, where <c>2^(1/3) sin^(1/3)/cos</c> is not a function of
        /// the tangent and goes the long way round.
        /// </summary>
        private static Entity? WithTheOddRootOnEachFactor(Entity product, Number.Rational fraction, Entity.Variable x)
        {
            Entity? rebuilt = null;
            var total = ERational.Zero;
            foreach (var factor in Mulf.LinearChildren(product))
            {
                var @base = factor;
                var exponent = EInteger.One;
                while (@base is Powf(var inner, Number.Integer whole))
                {
                    exponent = exponent.Multiply(whole.EInteger);
                    @base = inner;
                }
                var trigonometric = @base switch
                {
                    Sinf(var a) => a,
                    Cosf(var a) => a,
                    Tanf(var a) => a,
                    Secantf(var a) => a,
                    Cosecantf(var a) => a,
                    Cotanf(var a) => a,
                    _ => null,
                };
                if (trigonometric is not null ? trigonometric != x : @base.ContainsNode(x))
                    return null;
                var each = fraction.ERational.Multiply(ERational.FromEInteger(exponent));
                // The sine and the cosine count one, their reciprocals minus one, and the
                // tangent, a sine over a cosine, nothing.
                total = total.Add(each.Multiply(@base switch
                {
                    Sinf or Cosf => ERational.One,
                    Secantf or Cosecantf => ERational.FromInt32(-1),
                    _ => ERational.Zero,
                }));
                var power = MathS.Pow(@base, Number.Rational.Create(each));
                rebuilt = rebuilt is null ? power : rebuilt * power;
            }
            var reduced = total.ToLowestTerms();
            return reduced.Denominator.Equals(EInteger.One) && reduced.Numerator.IsEven ? rebuilt : null;
        }

        /// <summary>
        /// An integrand holding a fractional power of something <b>linear</b> in the variable,
        /// turned into a rational function by <c>u^q = a*x + b</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Setting <c>u = (a*x + b)^(1/q)</c> gives <c>x = (u^q - b)/a</c> and
        /// <c>dx = (q/a) u^(q-1) du</c>, and every occurrence of the radical becomes a power of
        /// <c>u</c>. A quotient of polynomials in <c>x</c> and that radical is then a quotient of
        /// polynomials in <c>u</c>, which the rational integrator answers.
        /// </para>
        /// <para>
        /// <b>Why the general substitution does not already do this, which is the point.</b>
        /// <see cref="SolveBySubstitution"/> rewrites <em>sub-expressions</em>: it finds
        /// <c>1 + x</c> inside <c>x/sqrt(1 + x)</c>, replaces that, and is left with a bare
        /// <c>x</c> it cannot express, so it declines. This substitutes for <b>x itself</b>, so
        /// there is nothing left behind to fail on. <c>x/sqrt(1 + x)</c> and
        /// <c>x/sqrt(2 - 3x)</c> had no antiderivative for exactly that reason.
        /// </para>
        /// <para>
        /// <b>One q for the whole integrand.</b> Where several radicals share a base —
        /// <c>sqrt(x + 1)</c> beside <c>(x + 1)^(1/3)</c> — the exponent denominators are taken
        /// together by their least common multiple, so one substitution clears both. Radicals over
        /// <em>different</em> linear bases are not handled: <c>sqrt(1 - x) + sqrt(1 + x)</c> needs
        /// two substitutions at once and is declined rather than half-rewritten.
        /// </para>
        /// <para>
        /// <b>No condition is owed by the rewrite.</b> <c>a</c> is non-zero, since a zero
        /// coefficient is not a linear expression in <c>x</c> and the reader below rejects it,
        /// and <c>u^q = a*x + b</c> is invertible wherever the radical it came from is defined.
        /// The answer inherits the radical's own domain and adds nothing.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveByLinearRadicalSubstitution(Entity expr, Entity.Variable x, bool integrateByParts, bool variableIsNonnegative = false)
        {
            // Every fractional power in the tree whose base is linear in x, collected with the
            // base it is over so that radicals over different bases are told apart.
            //
            // **Two bases, when both are square roots.** `sqrt(u)/(u sqrt(1 + u))` -- what
            // `sqrt(1 + tanh(4x))` becomes under `u = e^(8x)` -- was declined here for the second
            // base, and it is one substitution: with `s = sqrt(u)` the other root is
            // `sqrt(1 + s^2)`, a root of a quadratic, which the rules for those answer. So a
            // second base is admitted where every radical is a square root; its radicals are
            // substituted rather than built, and what they become is the next rule's. A third
            // base, or a cube root beside a second base, is declined as before.
            //
            // And only where the two bases are not both negative anywhere on the reals, unless
            // the caller knows its variable to be non-negative. Where both are, the integrand
            // is real -- `sqrt(x - 1) sqrt(x - 2)` below 1 is `-sqrt((1 - x)(2 - x))` -- and the
            // answer, built through an imaginary `u`, is not its antiderivative there: measured
            // by quadrature over [-1.5, 0.3], -3.66 against the answer's -4.68, with the two
            // agreeing to twelve digits above 2. The exponential substitution's `u = e^(k x)`
            // never is negative, and it says so.
            Entity? radicalBase = null;
            Entity? otherBase = null;
            var denominators = new List<int>();
            var allSquareRoots = true;
            foreach (var node in expr.Nodes)
            {
                if (node is not Powf(var @base, Number.Rational exponent) || exponent is Number.Integer)
                    continue;
                if (!@base.ContainsNode(x))
                    continue;
                if (!exponent.ERational.Denominator.Equals(EInteger.FromInt32(2)))
                    allSquareRoots = false;
                // A radical over something that is not linear is **skipped rather than refused**,
                // and that is where the nested ones come from. `sqrt(x + sqrt(1 + x))` holds two:
                // the inner one is over something linear and is what the substitution is for, and
                // the outer one is over a sum holding the inner one. Declining because of the
                // outer one refused the whole family -- and it need not, because the substitution
                // eliminates `x` from it anyway: with `u^2 = 1 + x` the outer base is `u^2 + u - 1`
                // and the integrand becomes `2u sqrt(u^2 + u - 1)`, which the quadratic radical
                // rule answers. The check that no `x` survives, below, is what makes skipping safe
                // rather than a guess. https://github.com/asc-community/AngouriMath/issues/718
                // A quotient of two linears is a base of its own kind: `sqrt((1 + x)/(3 + 2x))`
                // under `u = sqrt((1 + x)/(3 + 2x))` has `x = (3u^2 - 1)/(1 - 2u^2)`, rational,
                // and the answer holds wherever the root is real -- below `-3/2` too, where
                // both linears are negative and the root of each is not. Split into a root
                // over a root it is declined for that, and rightly.
                if (!TreeAnalyzer.TryGetPolyLinear(@base, x, out var slope, out _) || slope.Evaled == 0)
                {
                    if (radicalBase is null && IsAQuotientOfLinears(@base, x))
                    {
                        radicalBase = @base;
                        if (!exponent.ERational.Denominator.CanFitInInt32())
                            return null;
                        denominators.Add(exponent.ERational.Denominator.ToInt32Checked());
                    }
                    continue;
                }
                if (radicalBase is null)
                    radicalBase = @base;
                else if (radicalBase != @base)
                {
                    if (otherBase is null)
                        otherBase = @base;
                    else if (otherBase != @base)
                        return null;   // three different bases at once
                    continue;
                }
                if (!exponent.ERational.Denominator.CanFitInInt32())
                    return null;
                denominators.Add(exponent.ERational.Denominator.ToInt32Checked());
            }
            if (radicalBase is null || denominators.Count == 0)
                return null;
            if (otherBase is not null && !allSquareRoots)
                return null;
            if (otherBase is not null && !variableIsNonnegative
                && !AtMostOneIsNegativeOnTheReals(new List<Entity> { radicalBase, otherBase }, x))
                return null;

            var q = denominators.Aggregate(1, Lcm);
            if (q < 2 || q > 12)   // beyond this the rewritten polynomial is not worth building
                return null;

            var u = Variable.CreateUnique(expr, "u_rad");
            Entity xInU;
            Entity dx;
            if (TreeAnalyzer.TryGetPolyLinear(radicalBase, x, out var a, out var b))
            {
                // x = (u^q - b) / a, and dx = (q/a) u^(q-1) du.
                xInU = (MathS.Pow(u, q) - b) / a;
                dx = Number.Integer.Create(q) / a * MathS.Pow(u, q - 1);
            }
            else
            {
                if (otherBase is not null)
                    return null;
                // u^q = (a x + b)/(c x + d), so x = (d u^q - b)/(a - c u^q).
                var (above, below) = Functions.SingleQuotient.Of(radicalBase);
                if (!TreeAnalyzer.TryGetPolyLinear(above, x, out a, out b) || !TreeAnalyzer.TryGetPolyLinear(below, x, out var c, out var d))
                    return null;
                xInU = ((d * MathS.Pow(u, q) - b) / (a - c * MathS.Pow(u, q))).InnerSimplified;
                dx = xInU.Differentiate(u).InnerSimplified;
            }

            // Each radical becomes its power of u **by construction** rather than by substituting
            // and then simplifying. Substituting alone turns (x + 1)^(1/2) into ((u^6))^(1/2), and
            // the simplifier is right to refuse to call that u^3 — it is |u^3| on the reals and
            // worse off them. Building u^3 directly is sound here because u is the principal
            // q-th root of a*x + b by definition of the substitution, and it is the difference
            // between answering 1/(sqrt(x+1) + (x+1)^(1/3)) and handing it back.
            var rewritten = expr.Replace(node =>
                node is Powf(var radical, Number.Rational e) && e is not Number.Integer
                && radical == radicalBase
                && e.ERational.Denominator.CanFitInInt32()
                && e.ERational.Numerator.CanFitInInt32()
                    ? MathS.Pow(u, q / e.ERational.Denominator.ToInt32Checked()
                                   * e.ERational.Numerator.ToInt32Checked())
                    : node);

            rewritten = rewritten.Substitute(x, xInU);
            if (rewritten.ContainsNode(x))
                return null;

            var integrand = Functions.SingleQuotient.Combine((rewritten * dx).Simplify());
            if (integrand is Providedf(var inner, _))
                integrand = inner;
            // For an even q the principal root is not negative wherever it is real, and a root
            // holding a power of u gives that power up: `1/sqrt(t + t^(3/2))` under `u = sqrt(t)`
            // is `2u/sqrt(u^2 + u^3)`, a root of a cubic, and is `2/sqrt(1 + u)`.
            if (q % 2 == 0)
                integrand = FactorANonnegativeVariableOutOfRadicals(integrand, u);

            return Integration.ComputeIndefiniteIntegral(integrand, u, integrateByParts) is { } result
                ? result.Substitute(u, MathS.Pow(radicalBase, Number.Rational.Create(1, q)))
                : null;
        }

        /// <summary>
        /// Fractional powers of two different linears in the variable, <c>(a x + b)^(p/q)</c>
        /// beside <c>(c x + d)^(r/q)</c>, rationalised by <c>t = ((a x + b)/(c x + d))^(1/q)</c>
        /// where every term of the integrand carries a whole power of the second linear once
        /// the first is written through <c>t</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// With <c>t^q = (a x + b)/(c x + d)</c>, <c>x = (d t^q - b)/(a - c t^q)</c> and
        /// <c>c x + d = (a d - b c)/(a - c t^q)</c>, both rational in <c>t</c>; and
        /// <c>(a x + b)^(p/q) = t^p (c x + d)^(p/q)</c>. So each term of the integrand is a
        /// power of <c>t</c> times a power of the second linear, and where that power's
        /// numerator is the same modulo <c>q</c> across the terms above the line and across
        /// those below, the powers left are whole and the integrand is rational in <c>t</c>.
        /// <c>1/((x - 1)^4 (x + 1)^2)^(1/3)</c> is <c>(x - 1)^(-4/3) (x + 1)^(-2/3)</c>: with
        /// <c>t^3 = (x - 1)/(x + 1)</c> it is <c>t^(-4) (x + 1)^(-2)</c>, and
        /// <c>x (1 + x)^(2/3) sqrt(1 - x)</c> over a sum of two such products is the same with
        /// <c>q = 6</c>, every product there carrying <c>(1 - x)^(7/6)</c>.
        /// </para>
        /// <para>
        /// <b>Where it holds: everywhere.</b> <c>t</c> is written back as the quotient of the
        /// two principal roots, <c>(a x + b)^(1/q) / (c x + d)^(1/q)</c>, and not as the root of
        /// the quotient: a whole power of a quotient is the quotient of the powers, so
        /// <c>t^p</c> is <c>(a x + b)^(p/q) / (c x + d)^(p/q)</c> for every complex <c>x</c>,
        /// where the root of the quotient agrees with that only where the two linears have the
        /// same sign. Written the first way the answer to Timofeev's 315 held on <c>(-1, 1)</c>
        /// and failed above 1, where <c>sqrt(1 - x)</c> is imaginary.
        /// </para>
        /// <para>
        /// After <see cref="SolveByLinearRadicalSubstitution"/>, which answers one linear base
        /// and declines two of different roots; this is those.
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </para>
        /// </remarks>
        internal static Entity? SolveByAQuotientOfTwoLinearRadicals(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            Entity? first = null;
            Entity? second = null;
            var denominators = new List<int>();
            foreach (var node in expr.Nodes)
            {
                if (node is not Powf(var @base, Number.Rational exponent) || exponent is Number.Integer)
                    continue;
                if (!@base.ContainsNode(x))
                    continue;
                if (!TreeAnalyzer.TryGetPolyLinear(@base, x, out var slope, out _) || slope.Evaled == 0)
                    return null;
                if (!exponent.ERational.Denominator.CanFitInInt32() || !exponent.ERational.Numerator.CanFitInInt32())
                    return null;
                denominators.Add(exponent.ERational.Denominator.ToInt32Checked());
                if (first is null)
                    first = @base;
                else if (first != @base)
                {
                    if (second is null)
                        second = @base;
                    else if (second != @base)
                        return null;
                }
            }
            if (first is null || second is null)
                return null;
            var q = denominators.Aggregate(1, Lcm);
            if (q < 2 || q > 12)
                return null;
            if (!TreeAnalyzer.TryGetPolyLinear(first, x, out var a, out var b) || !TreeAnalyzer.TryGetPolyLinear(second, x, out var c, out var d))
                return null;
            // Two proportional linears are one radical with a constant in it, and not this
            // rule's: their determinant a d - b c is zero, the second linear in t is 0/(a - c t^q)
            // and everything it multiplies vanished -- Rubi's
            // `sin(a + b (c + d x)^(1/3))/(c e + d e x)^(1/3)` came back as `0 provided ...`.
            // https://github.com/asc-community/AngouriMath/issues/1386
            var determinant = (a * d - b * c).InnerSimplified;
            if (determinant.Evaled is Number.Complex { IsZero: true }
                || determinant.Vars.Any() && Functions.PartialFractions.Bare(determinant.Simplify()).Evaled is Number.Complex { IsZero: true })
                return null;

            var t = Variable.CreateUnique(expr, "t_rad");
            var w = Variable.CreateUnique(expr, "w_rad");
            var tq = MathS.Pow(t, q);
            var xInT = ((d * tq - b) / (a - c * tq)).InnerSimplified;
            var secondInT = ((a * d - b * c) / (a - c * tq)).InnerSimplified;
            var dx = xInT.Differentiate(t).InnerSimplified;

            // Each radical becomes its powers by construction, as in the linear rule: the
            // first linear's (p/q')-th power is t^k w^k and the second's is w^k, k = p q/q',
            // with w standing for the second linear's q-th root.
            var rewritten = expr.Replace(node =>
            {
                if (node is not Powf(var radical, Number.Rational e) || e is Number.Integer || (radical != first && radical != second))
                    return node;
                var k = q / e.ERational.Denominator.ToInt32Checked() * e.ERational.Numerator.ToInt32Checked();
                return radical == first ? MathS.Pow(t, k) * MathS.Pow(w, k) : MathS.Pow(w, k);
            });
            rewritten = rewritten.Substitute(x, xInT);
            if (rewritten.ContainsNode(x))
                return null;
            // The powers of w read off the terms as written first, which keeps a power of a
            // quadratic the derivative of x(t) carries as the power it is: for Hearn's
            // `sqrt(a + b x) sqrt(c + d x)` the rational function is `t^2 / (b - d t^2)^3`, and
            // read after simplification it is over a quartic times the quadratic, which the
            // rational rules with symbols in the coefficients do not take apart. Where a term
            // holds w other than as a factor, the terms are read as polynomials in w after
            // simplification, as before.
            var root = Number.Rational.Create(1, q);
            var back = MathS.Pow(first, root) / MathS.Pow(second, root);
            foreach (var simplified in new[] { false, true })
            {
                var product = rewritten * dx;
                if (simplified)
                {
                    product = product.Simplify();
                    if (product is Providedf(var bareProduct, _))
                        product = bareProduct;
                }
                var (above, below) = Functions.SingleQuotient.Of(Functions.SingleQuotient.Combine(product));
                var aboveTerms = simplified ? TermsAsPolynomialInW(above, w) : TermsByFactorsOfW(above, w);
                var belowTerms = simplified ? TermsAsPolynomialInW(below, w) : TermsByFactorsOfW(below, w);
                if (aboveTerms is null || belowTerms is null)
                    continue;
                // The powers of w above the line agree modulo q, and so do those below, and the
                // two residues agree: then every power left after t^q's worth is taken is whole.
                if (Residue(aboveTerms.Select(term => term.Power), q) is not { } residueAbove
                    || Residue(belowTerms.Select(term => term.Power), q) is not { } residueBelow || residueAbove != residueBelow)
                    continue;
                Entity rebuilt = WithWholePowersOfTheSecond(aboveTerms, residueAbove, q, secondInT)
                    / WithWholePowersOfTheSecond(belowTerms, residueBelow, q, secondInT);
                // A rational function of t; the condition the rebuilding attaches -- a power of
                // a quotient is defined where the quotient is -- is the substitution's, not the
                // integrand's, whose own domain the answer inherits.
                var integrand = Functions.SingleQuotient.Combine(simplified ? rebuilt.Simplify() : rebuilt.InnerSimplified);
                if (integrand is Providedf(var rational, _))
                    integrand = rational;
                if (integrand.ContainsNode(w))
                    continue;
                // t as the quotient of the two principal roots, not the root of the quotient:
                // then t^p is (a x + b)^(p/q) over (c x + d)^(p/q) for every complex x, and the
                // identity the rewriting rests on holds off the real line too.
                if (Integration.ComputeIndefiniteIntegral(integrand, t, integrateByParts) is { } result)
                    return result.Substitute(t, back);
            }
            return null;

            // Each additive term as its power of w and the rest, w taken as a factor of the
            // term and nowhere else; null where a term holds w inside a sum or a power.
            static List<(EInteger Power, Entity Coefficient)>? TermsByFactorsOfW(Entity expr, Variable w)
            {
                var terms = new List<(EInteger, Entity)>();
                foreach (var term in Sumf.LinearChildren(expr))
                {
                    var power = EInteger.Zero;
                    Entity? coefficient = null;
                    foreach (var factor in Mulf.LinearChildren(term))
                    {
                        if (factor == w)
                            power = power.Add(1);
                        else if (factor is Powf(var b, Number.Integer e) && b == w)
                            power = power.Add(e.EInteger);
                        else if (factor.ContainsNode(w))
                            return null;
                        else
                            coefficient = coefficient is null ? factor : coefficient * factor;
                    }
                    terms.Add((power, coefficient ?? Number.Integer.One));
                }
                return terms;
            }

            static List<(EInteger Power, Entity Coefficient)>? TermsAsPolynomialInW(Entity expr, Variable w)
                => TreeAnalyzer.TryGetPolynomial(expr, w, out var monomials)
                    ? monomials.Select(pair => (pair.Key, pair.Value)).ToList()
                    : null;

            static int? Residue(IEnumerable<EInteger> exponents, int q)
            {
                int? residue = null;
                foreach (var exponent in exponents)
                {
                    if (!exponent.CanFitInInt32())
                        return null;
                    var r = ((exponent.ToInt32Checked() % q) + q) % q;
                    if (residue is null)
                        residue = r;
                    else if (residue != r)
                        return null;
                }
                return residue ?? 0;
            }

            // w^e with e = residue + m q is (second)^m, the residue's worth cancelling between
            // the two lines.
            static Entity WithWholePowersOfTheSecond(List<(EInteger Power, Entity Coefficient)> terms, int residue, int q, Entity secondInT)
            {
                Entity? sum = null;
                foreach (var (power, coefficient) in terms)
                {
                    var m = (power.ToInt32Checked() - residue) / q;
                    Entity term = m == 0 ? coefficient : coefficient * MathS.Pow(secondInT, m);
                    sum = sum is null ? term : sum + term;
                }
                return sum ?? Number.Integer.Zero;
            }
        }

        /// <summary>
        /// Whether <paramref name="expr"/> is a quotient of two polynomials linear in
        /// <paramref name="x"/>, the one below with a slope.
        /// </summary>
        private static bool IsAQuotientOfLinears(Entity expr, Entity.Variable x)
        {
            var (above, below) = Functions.SingleQuotient.Of(expr);
            return below != Number.Integer.One
                && TreeAnalyzer.TryGetPolyLinear(above, x, out _, out _)
                && TreeAnalyzer.TryGetPolyLinear(below, x, out var slope, out _)
                && slope.Evaled is not Number.Complex { IsZero: true };
        }

        /// <summary>
        /// Whether <paramref name="expr"/> is a polynomial in one sine or one cosine of one
        /// argument holding <paramref name="x"/>, and nothing else of <paramref name="x"/>.
        /// </summary>
        private static bool IsAPolynomialInOneTrigonometricFunction(Entity expr, Entity.Variable x)
        {
            var functions = expr.Nodes.Where(node => node is Sinf or Cosf && node.ContainsNode(x)).Distinct().ToList();
            if (functions.Count == 2)
            {
                // A polynomial in the sine and the cosine of one argument with the cosine only
                // in even powers is a polynomial in the sine, by Pythagoras, and the other way
                // about: Timofeev's `sqrt(3 cos(x)^2 - sin(x)^2)` is `sqrt(3 - 4 sin(x)^2)`,
                // and beside `cos(3x)` it was declined for the two functions under the root.
                if (TrigonometricArgument(functions[0]) != TrigonometricArgument(functions[1]) || functions[0].GetType() == functions[1].GetType())
                    return false;
                var first = Variable.CreateUnique(expr, "trig_1");
                var second = Variable.CreateUnique(expr, "trig_2");
                var inBoth = expr.Substitute(functions[0], first).Substitute(functions[1], second);
                if (inBoth.ContainsNode(x))
                    return false;
                var expanded = inBoth.Expand();
                return IsEvenIn(expanded, first, second) || IsEvenIn(expanded, second, first);

                static bool IsEvenIn(Entity polynomial, Entity.Variable even, Entity.Variable other)
                    => TreeAnalyzer.TryGetPolynomial(polynomial, even, out var read)
                       && read.All(pair => pair.Key.IsEven && pair.Key.Sign >= 0 && TreeAnalyzer.TryGetPolynomial(pair.Value, other, out _));
            }
            if (functions.Count != 1)
                return false;
            var placeholder = Variable.CreateUnique(expr, "trig");
            var inPlaceholder = expr.Substitute(functions[0], placeholder);
            return !inPlaceholder.ContainsNode(x) && TreeAnalyzer.TryGetPolynomial(inPlaceholder, placeholder, out _);
        }

        /// <summary>
        /// An integrand whose trigonometric functions have <b>different multiples</b> of one
        /// argument — <c>sin(x)/cos(2x)</c>, <c>cos(x)/(sin(x) tan(x/2))</c> — rewritten so that
        /// every one of them is of the same argument, and handed on.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Every trigonometric rule here reads one argument: a quotient of homogeneous
        /// polynomials in <c>sin(u)</c> and <c>cos(u)</c>, a function of <c>tan(u)</c>, the
        /// half-angle substitution in <c>u</c>. An integrand with <c>x</c> and <c>2x</c> in it
        /// is none of those and every one of them declined it — while the same integrand with
        /// <c>cos(2x)</c> written as <c>1 - 2 sin(x)^2</c> came out. Nothing was missing but the
        /// rewriting. Product-to-sum, beside this, goes the other way, and fires on a product of
        /// two different arguments; this fires on anything else built from them.
        /// </para>
        /// <para>
        /// The common argument is <c>g x</c> with <c>g</c> the greatest common divisor of the
        /// slopes, so <c>x</c> and <c>x/2</c> share <c>x/2</c> and <c>2x</c> and <c>3x</c> share
        /// <c>x</c>. Each function of <c>n g x</c> is then a polynomial in <c>sin(g x)</c> and
        /// <c>cos(g x)</c> through the Chebyshev recurrences, <c>cos(n t) = T_n(cos t)</c> and
        /// <c>sin(n t) = sin(t) U_(n-1)(cos t)</c>, which are identities and owe no condition;
        /// a tangent, secant or cosecant is the quotient of those it stands for. Slopes are
        /// rational and the multiples are capped, since a function of <c>50x</c> beside one of
        /// <c>x</c> is a degree-fifty polynomial nobody wants.
        /// </para>
        /// <para>
        /// It terminates: what it hands on has one argument, on which this rule declines.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveByUnifyingTrigonometricArguments(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            // Rational in the trigonometric functions, and no more: a radical of one, or a
            // fractional power, is a different integrand, and the expanded polynomial under it
            // is one no rule reads. `sqrt(cos(x) sin(x)^3)` beside `sin(2x)` and
            // `(2 - 3 sin(x)^2)^(3/5) sin(4x)` were each five seconds spent to decline once the
            // rewriting let them through; each declined in a tenth of that before.
            //
            // And bounded in the degree the rewriting produces: a power of a sum holding a
            // function of `3x`, raised to the fifth, is a polynomial of degree fifteen in the
            // sine and cosine, and `1/(cos(x) + cos(3x))^5` went the same way for it.
            var slopes = new List<Number.Rational>();
            var arguments = new HashSet<(ERational Slope, Entity Offset)>();
            var anOffset = false;
            foreach (var node in expr.Nodes)
            {
                if (!node.ContainsNode(x))
                    continue;
                switch (node)
                {
                    case Variable or Sumf or Minusf or Mulf or Divf:
                    case Sinf or Cosf or Tanf or Cotanf or Secantf or Cosecantf:
                    case Powf(_, Number.Integer):
                        break;
                    // A fractional power of a polynomial in *one* function of one argument is
                    // let through: `(2 - 3 sin(x)^2)^(3/5) sin(4x)` is, under the sine, a sum of
                    // binomial differentials once the multiple is written out, and the binomial
                    // rule answers those at any depth now. A root of a product of two functions
                    // is not -- `sqrt(cos(x) sin(x)^3)` beside `sin(2x)` was thirty seconds to
                    // decline let through -- and nor is anything else.
                    case Powf(var radicalBase, Number.Rational) when IsAPolynomialInOneTrigonometricFunction(radicalBase, x):
                        break;
                    default:
                        return null;
                }
                if (TrigonometricArgument(node) is not { } argument || !argument.ContainsNode(x))
                    continue;
                // An offset free of x is allowed, and written out by the addition formula
                // below: Timofeev's `tan(x) tan(x - a)` is a rational function of `tan(x)` once
                // `tan(x - a)` is `(tan(x) - tan(a))/(1 + tan(x) tan(a))`.
                if (!TreeAnalyzer.TryGetPolyLinear(argument, x, out var slope, out var offset)
                    || offset.ContainsNode(x)
                    || slope.Evaled is not Number.Rational rational || rational.IsZero)
                    return null;
                slopes.Add(rational);
                arguments.Add((rational.ERational, offset.InnerSimplified));
                if (offset.Evaled is not Number.Complex { IsZero: true })
                    anOffset = true;
            }
            if (slopes.Count < 2 || arguments.Count < 2)
                return null;

            // g = gcd of the numerators over the lcm of the denominators, so that every slope is
            // a whole multiple of it; nothing to do when they are all the same multiple.
            var numerators = EInteger.Zero;
            var denominators = EInteger.One;
            foreach (var slope in slopes)
            {
                numerators = numerators.Gcd(slope.ERational.Numerator.Abs());
                denominators = denominators.Multiply(slope.ERational.Denominator)
                    .Divide(denominators.Gcd(slope.ERational.Denominator));
            }
            var common = ERational.Create(numerators, denominators);
            var multiples = new HashSet<int>();
            foreach (var slope in slopes)
            {
                // Reduced before its numerator is read: `Divide` leaves `2/2` as it is.
                var multiple = slope.ERational.Divide(common).ToLowestTerms();
                if (!multiple.IsInteger() || !multiple.Numerator.Abs().CanFitInInt32())
                    return null;
                var n = multiple.Numerator.Abs().ToInt32Unchecked();
                if (n > MaximumUnifiedMultiple)
                    return null;
                multiples.Add(n);
            }
            if (multiples.Count < 2 && !anOffset)
                return null;
            if (ExpandedDegree(expr, x, common) > MaximumUnifiedDegree)
                return null;

            var theta = (Number.Rational.Create(common) * x).InnerSimplified;

            // Up before down. With only the argument and its double present, an integrand that
            // is even in the smaller one is a function of the doubled one alone, at half the
            // degree the other direction gives; that is tried first and the rest falls through.
            if (!anOffset && multiples.Count == 2 && multiples.Contains(1) && multiples.Contains(2)
                && TryRaiseToTheDoubledArgument(expr, x, theta) is { } raised)
            {
                var (raisedNumerator, raisedDenominator) = Functions.SingleQuotient.Of(raised.InnerSimplified);
                if (Integration.ComputeIndefiniteIntegral(
                        CancelCommonFactors(raisedNumerator, raisedDenominator), x, integrateByParts: false) is { } fromAbove)
                    return fromAbove;
            }

            var sine = MathS.Sin(theta);
            var cosine = MathS.Cos(theta);
            var rewritten = expr.Replace(node =>
            {
                if (TrigonometricArgument(node) is not { } argument || !argument.ContainsNode(x)
                    || !TreeAnalyzer.TryGetPolyLinear(argument, x, out var slope, out var offset)
                    || slope.Evaled is not Number.Rational rational)
                    return node;
                var multiple = rational.ERational.Divide(common).ToLowestTerms();
                var n = multiple.Numerator.Abs().ToInt32Unchecked();
                var negative = multiple.Numerator.Sign < 0;
                var (sin, cos) = SineAndCosineOfAMultiple(n, sine, cosine);
                if (negative)
                    sin = -sin;
                if (offset.Evaled is not Number.Complex { IsZero: true })
                {
                    // sin(u + c) = sin(u) cos(c) + cos(u) sin(c), cos(u + c) = cos(u) cos(c) - sin(u) sin(c).
                    var (sinOfOffset, cosOfOffset) = (MathS.Sin(offset).InnerSimplified, MathS.Cos(offset).InnerSimplified);
                    (sin, cos) = (sin * cosOfOffset + cos * sinOfOffset, cos * cosOfOffset - sin * sinOfOffset);
                }
                return node switch
                {
                    Sinf => sin,
                    Cosf => cos,
                    Tanf => sin / cos,
                    Cotanf => cos / sin,
                    Secantf => 1 / cos,
                    Cosecantf => 1 / sin,
                    _ => node
                };
            });
            // As one quotient with the common factors cancelled, before it is handed on. The
            // rewriting leaves a quotient of quotients with a factor to cancel -- `tan(x)/tan(2x)`
            // becomes `(s/c) / (2sc/(2c^2 - 1))` -- and on that shape, or on the single quotient
            // with the `s` still in it, the search ran for twenty-five seconds to decline what is
            // `1 - 1/(2c^2)` and answered in a quarter of one. `Simplify` is not the tool: it
            // folds `2sc` back into `sin(2x)` and hands this rule its own input.
            var (numerator, denominator) = Functions.SingleQuotient.Of(rewritten.InnerSimplified);
            var cancelled = CancelCommonFactors(numerator, denominator);
            // A radicand in both functions with one of them in even powers only is written
            // in the other by Pythagoras, in each way that is open where both are even:
            // Timofeev's `sin(5x)/(5 cos(x)^2 + 9 sin(x)^2)^(5/2)` is a polynomial in the
            // cosine times the sine over `(9 - 4 cos(x)^2)^(5/2)`, `u = cos(x)`'s at once, and
            // handed on with the two functions under the root it ran the budget out.
            foreach (var spelling in RadicandsInOneFunction(cancelled, sine, cosine))
                if (Integration.ComputeIndefiniteIntegral(spelling, x, integrateByParts: false) is { } answer)
                    return answer;
            return null;
        }

        /// <summary>
        /// <paramref name="expr"/> with every fractional-power base that is a polynomial in
        /// <paramref name="sine"/> and <paramref name="cosine"/> together, one of them in even
        /// powers only, written in the other; the spelling in the cosine first and then in
        /// the sine where both are open, and <paramref name="expr"/> itself where there is
        /// nothing to write.
        /// </summary>
        private static IEnumerable<Entity> RadicandsInOneFunction(Entity expr, Entity sine, Entity cosine)
        {
            var bases = expr.Nodes.Where(node => node is Powf(var @base, Number.Rational r) && r is not Number.Integer
                && @base.ContainsNode(sine) && @base.ContainsNode(cosine)).Select(node => ((Powf)node).Base).Distinct().ToList();
            if (bases.Count == 0)
            {
                yield return expr;
                yield break;
            }
            var s = Variable.CreateUnique(expr, "s_rad");
            var c = Variable.CreateUnique(expr, "c_rad");
            Entity? InTheOther(Entity @base, Entity.Variable even, Entity.Variable other, Entity evenSquaredAs)
            {
                var inBoth = @base.Substitute(sine, s).Substitute(cosine, c).Expand();
                if (!TreeAnalyzer.TryGetPolynomial(inBoth, even, out var read)
                    || !read.All(pair => pair.Key.IsEven && pair.Key.Sign >= 0 && TreeAnalyzer.TryGetPolynomial(pair.Value, other, out _)))
                    return null;
                Entity written = Number.Integer.Zero;
                foreach (var pair in read)
                {
                    var half = pair.Key.ToInt32Checked() / 2;
                    written += pair.Value * (half == 0 ? Number.Integer.One : MathS.Pow(evenSquaredAs, half));
                }
                return written.Expand().InnerSimplified.Substitute(s, sine).Substitute(c, cosine);
            }
            var any = false;
            foreach (var (even, other, evenSquaredAs) in new[] { (s, c, 1 - MathS.Sqr(c)), (c, s, 1 - MathS.Sqr(s)) })
            {
                var respelled = new Dictionary<Entity, Entity>();
                foreach (var @base in bases)
                    if (InTheOther(@base, even, other, evenSquaredAs) is { } written && written != @base)
                        respelled[@base] = written;
                if (respelled.Count == 0)
                    continue;
                any = true;
                yield return expr.Replace(node => node is Powf(var @base, Number.Rational r) && r is not Number.Integer && respelled.TryGetValue(@base, out var written)
                    ? MathS.Pow(written, r) : node);
            }
            if (!any)
                yield return expr;
        }

        /// <summary>
        /// <paramref name="expr"/>, whose trigonometric functions are of <paramref name="theta"/>
        /// and of <c>2 theta</c>, rewritten in <c>sin(2 theta)</c> and <c>cos(2 theta)</c> alone
        /// — or <see langword="null"/> where it is not even in <paramref name="theta"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>sin^2 = (1 - C)/2</c>, <c>cos^2 = (1 + C)/2</c> and <c>sin cos = S/2</c>, with
        /// <c>S</c> and <c>C</c> the sine and cosine of the doubled argument; so a monomial
        /// <c>sin^p cos^q</c> is a rational function of <c>S</c> and <c>C</c> exactly when
        /// <c>p + q</c> is even, negative powers included, and an expression built from such
        /// monomials by sums, products and quotients is one too. Going up to the doubled argument
        /// where that holds gives polynomials of half the degree the other direction does:
        /// <c>sec(2t)/(1 + sec(t)^2 + 3 tan(t))</c> is answered in a third of a second written in
        /// <c>2t</c> and declined after five written in <c>t</c>.
        /// </para>
        /// </remarks>
        private static Entity? TryRaiseToTheDoubledArgument(Entity expr, Entity.Variable x, Entity theta)
        {
            var sine = MathS.Sin(theta);
            var cosine = MathS.Cos(theta);
            var doubledSine = MathS.Sin((2 * theta).InnerSimplified);
            var doubledCosine = MathS.Cos((2 * theta).InnerSimplified);

            // A monomial sin^p cos^q of the small argument, as a function of the doubled one:
            // (sin cos)^k times an even power of whichever is left, with k the smaller of the two
            // exponents so that the leftover power is p - q or q - p, even by the parity check.
            Entity? Monomial(int p, int q)
            {
                if ((p + q) % 2 != 0)
                    return null;
                // Either exponent will do for k, since p - q is even; the one that leaves the
                // smaller exponents behind gives the tidier form -- sec^2 as 2/(1 + C) rather
                // than as 2(1 - C)/S^2.
                static int Size(int k, int p, int q) => System.Math.Abs(k) + System.Math.Abs(p - k) + System.Math.Abs(q - k);
                var k = Size(p, p, q) <= Size(q, p, q) ? p : q;
                Entity result = k == 0 ? Number.Integer.One : MathS.Pow(doubledSine / 2, k);
                if (p != k)
                    result *= MathS.Pow((1 - doubledCosine) / 2, (p - k) / 2);
                if (q != k)
                    result *= MathS.Pow((1 + doubledCosine) / 2, (q - k) / 2);
                return result;
            }

            Entity? Raise(Entity node)
            {
                if (!node.ContainsNode(x))
                    return node;
                switch (node)
                {
                    case Sumf(var a, var b):
                        return Raise(a) is { } summandA && Raise(b) is { } summandB ? summandA + summandB : null;
                    case Minusf(var a, var b):
                        return Raise(a) is { } ma && Raise(b) is { } mb ? ma - mb : null;
                    case Divf(var a, var b):
                        return Raise(a) is { } da && Raise(b) is { } db ? da / db : null;
                    case Mulf:
                    {
                        // The sine and cosine powers of the small argument gathered across the
                        // product, so that sin * cos pairs off; every other factor on its own.
                        int p = 0, q = 0;
                        Entity rest = Number.Integer.One;
                        foreach (var factor in Mulf.LinearChildren(node))
                        {
                            var (@base, power) = factor is Powf(var pb, Number.Integer pe) && pe.EInteger.CanFitInInt32()
                                ? (pb, pe.EInteger.ToInt32Unchecked())
                                : (factor, 1);
                            if (@base == sine) p += power;
                            else if (@base == cosine) q += power;
                            else if (@base is Tanf(var ta) && ta == theta) { p += power; q -= power; }
                            else if (@base is Cotanf(var ca) && ca == theta) { p -= power; q += power; }
                            else if (@base is Secantf(var sa) && sa == theta) q -= power;
                            else if (@base is Cosecantf(var csa) && csa == theta) p -= power;
                            else if (Raise(factor) is { } raised) rest *= raised;
                            else return null;
                        }
                        return Monomial(p, q) is { } monomial ? rest * monomial : null;
                    }
                    case Powf(var @base, Number.Integer exponent) when exponent.EInteger.CanFitInInt32():
                    {
                        var n = exponent.EInteger.ToInt32Unchecked();
                        if (@base == sine) return Monomial(n, 0);
                        if (@base == cosine) return Monomial(0, n);
                        if (@base is Tanf(var ta) && ta == theta) return Monomial(n, -n);
                        if (@base is Cotanf(var ca) && ca == theta) return Monomial(-n, n);
                        if (@base is Secantf(var sa) && sa == theta) return Monomial(0, -n);
                        if (@base is Cosecantf(var csa) && csa == theta) return Monomial(-n, 0);
                        return Raise(@base) is { } rb ? MathS.Pow(rb, exponent) : null;
                    }
                    case Sinf(var a) when a == theta: return null;
                    case Cosf(var a) when a == theta: return null;
                    case Tanf(var a) when a == theta: return Monomial(1, -1);
                    case Cotanf(var a) when a == theta: return Monomial(-1, 1);
                    case Secantf(var a) when a == theta: return null;
                    case Cosecantf(var a) when a == theta: return null;
                    // Of the doubled argument, everything in its sine and cosine, which is what
                    // the rules below read.
                    case Tanf(var a) when a == doubledSine.DirectChildren.First(): return doubledSine / doubledCosine;
                    case Cotanf(var a) when a == doubledSine.DirectChildren.First(): return doubledCosine / doubledSine;
                    case Secantf(var a) when a == doubledSine.DirectChildren.First(): return 1 / doubledCosine;
                    case Cosecantf(var a) when a == doubledSine.DirectChildren.First(): return 1 / doubledSine;
                    default:
                        // A function of the doubled argument, or anything else, stays as it is;
                        // a bare sine or cosine of the small argument is odd and was declined above.
                        return node.DirectChildren.Any(child => child.ContainsNode(x) && Raise(child) is null) ? null : node;
                }
            }

            return Raise(expr);
        }

        /// <summary>
        /// <paramref name="numerator"/> over <paramref name="denominator"/> with every factor
        /// that appears in both, to a whole power, divided out by the smaller of the two powers.
        /// Factors are compared as written, which is what a rewriting that built both sides from
        /// the same pieces needs and no more.
        /// </summary>
        private static Entity CancelCommonFactors(Entity numerator, Entity denominator)
        {
            static Dictionary<Entity, int> Powers(Entity side)
            {
                var powers = new Dictionary<Entity, int>();
                foreach (var factor in Mulf.LinearChildren(side))
                {
                    // Nested whole powers folded, so that `((2t + 1)^2)^2` and `(2t + 1)^4` are
                    // one factor: a substitution that squares what it built leaves the first.
                    var @base = factor;
                    var power = 1;
                    while (@base is Powf(var inner, Number.Integer e) && e.EInteger.CanFitInInt32())
                    {
                        power *= e.EInteger.ToInt32Unchecked();
                        @base = inner;
                    }
                    powers[@base] = powers.TryGetValue(@base, out var already) ? already + power : power;
                }
                return powers;
            }
            static Entity Rebuild(Dictionary<Entity, int> powers)
            {
                Entity built = Number.Integer.One;
                foreach (var pair in powers)
                {
                    if (pair.Value == 0)
                        continue;
                    var factor = pair.Value == 1 ? pair.Key : MathS.Pow(pair.Key, pair.Value);
                    built = built == Number.Integer.One ? factor : built * factor;
                }
                return built;
            }

            var above = Powers(numerator);
            var below = Powers(denominator);
            foreach (var pair in above.ToList())
                if (below.TryGetValue(pair.Key, out var underneath) && pair.Value > 0 && underneath > 0)
                {
                    var shared = System.Math.Min(pair.Value, underneath);
                    above[pair.Key] = pair.Value - shared;
                    below[pair.Key] = underneath - shared;
                }
            var top = Rebuild(above);
            var bottom = Rebuild(below);
            return bottom == Number.Integer.One ? top : top / bottom;
        }

        /// <summary>
        /// The largest multiple <see cref="SolveByUnifyingTrigonometricArguments"/> expands;
        /// past it the Chebyshev polynomials are longer than any answer they lead to.
        /// </summary>
        private const int MaximumUnifiedMultiple = 6;

        /// <summary>
        /// The largest degree, in the sine and cosine of the common argument, that the rewriting
        /// is allowed to produce anywhere in the integrand.
        /// </summary>
        private const int MaximumUnifiedDegree = 16;

        /// <summary>
        /// An upper bound on the degree in <c>sin(g x)</c> and <c>cos(g x)</c> that rewriting
        /// <paramref name="expr"/> to the common slope <paramref name="common"/> produces: a
        /// function of <c>n g x</c> is degree <c>n</c>, a product adds, a power multiplies, a sum
        /// or quotient takes the larger side.
        /// </summary>
        private static int ExpandedDegree(Entity expr, Entity.Variable x, ERational common)
        {
            if (!expr.ContainsNode(x))
                return 0;
            switch (expr)
            {
                case Sumf or Minusf or Divf:
                    return expr.DirectChildren.Max(child => ExpandedDegree(child, x, common));
                case Mulf:
                    return expr.DirectChildren.Sum(child => ExpandedDegree(child, x, common));
                case Powf(var @base, Number.Integer power) when power.EInteger.CanFitInInt32():
                    return ExpandedDegree(@base, x, common) * System.Math.Abs(power.EInteger.ToInt32Unchecked());
                default:
                    if (TrigonometricArgument(expr) is { } argument
                        && TreeAnalyzer.TryGetPolyLinear(argument, x, out var slope, out _)
                        && slope.Evaled is Number.Rational rational)
                    {
                        var multiple = rational.ERational.Divide(common).ToLowestTerms();
                        return multiple.Numerator.Abs().CanFitInInt32() ? multiple.Numerator.Abs().ToInt32Unchecked() : int.MaxValue / 4;
                    }
                    return 1;
            }
        }

        private static Entity? TrigonometricArgument(Entity node) => node switch
        {
            Sinf(var a) => a,
            Cosf(var a) => a,
            Tanf(var a) => a,
            Cotanf(var a) => a,
            Secantf(var a) => a,
            Cosecantf(var a) => a,
            _ => null
        };

        /// <summary>
        /// <c>sin(n t)</c> and <c>cos(n t)</c> as polynomials in <paramref name="sine"/> and
        /// <paramref name="cosine"/>, by the Chebyshev recurrences.
        /// </summary>
        private static (Entity Sine, Entity Cosine) SineAndCosineOfAMultiple(int n, Entity sine, Entity cosine)
        {
            // T_0 = 1, T_1 = c, T_(k+1) = 2c T_k - T_(k-1); U_0 = 1, U_1 = 2c, likewise.
            Entity tPrevious = 1, tCurrent = cosine;
            Entity uPrevious = 1, uCurrent = 2 * cosine;
            if (n == 0)
                return (0, 1);
            for (var k = 1; k < n; k++)
            {
                (tPrevious, tCurrent) = (tCurrent, (2 * cosine * tCurrent - tPrevious).InnerSimplified);
                (uPrevious, uCurrent) = (uCurrent, (2 * cosine * uCurrent - uPrevious).InnerSimplified);
            }
            // sin(n t) = sin(t) U_(n-1)(cos t): after the loop uPrevious is U_(n-1).
            return ((sine * uPrevious).InnerSimplified, tCurrent);
        }

        /// <summary>
        /// <c>N B^r</c>, for a fractional <c>r</c> and an <c>N</c> and a <c>B</c> that are
        /// polynomials in <paramref name="x"/> and the functions of it in them, answered as
        /// <c>P(x) B^(r + 1)</c> for a polynomial <c>P</c> where there is one: the derivative of
        /// that is <c>B^r (P' B + (r + 1) P B')</c>, and <c>P' B + (r + 1) P B' = N</c> is a linear
        /// system in the coefficients of <c>P</c> once the functions of <c>x</c> are taken for
        /// indeterminates.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Bronstein's <c>(5x^2 + 3 (e^x + x)^(1/3) + e^x (3x + 2x^2))/(x (e^x + x)^(1/3))</c> is
        /// <c>3/x + (5x + e^x (2x + 3))/(e^x + x)^(1/3)</c>, and the second is
        /// <c>3x (e^x + x)^(2/3)</c>: with <c>P = c_0 + c_1 x</c> and <c>r = -1/3</c>,
        /// <c>c_1 (e^x + x) + (2/3)(c_0 + c_1 x)(e^x + 1) = 5x + e^x (2x + 3)</c> is
        /// <c>c_1 = 3</c>, <c>c_0 = 0</c>. Nothing else read it: the substitution
        /// <c>u = e^x + x</c> wants <c>1 + e^x</c> beside the root and finds <c>5x + e^x (2x + 3)</c>.
        /// </para>
        /// <para>
        /// The degree of <c>P</c> is the degree of <c>N</c> in <c>x</c>, the functions of <c>x</c>
        /// held constant, and one more; the system is exact, over the rationals, and its
        /// solution is checked by differentiating back at sampled points, since an identity
        /// between two functions taken for independent indeterminates -- <c>e^x</c> and
        /// <c>e^(2x)</c>, say -- can fail to be found and cannot be found wrongly, but a
        /// derivative the reader did not expect can. Closed, and volunteered at any depth.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveByAPolynomialTimesAPowerOfTheBase(Entity expr, Entity.Variable x)
        {
            var (numerator, denominator) = Functions.SingleQuotient.Of(expr);
            Entity? radicalBase = null;
            Number.Rational? exponent = null;
            Entity rest = Number.Integer.One;
            Entity constantBelow = Number.Integer.One;
            foreach (var (side, isBelow) in new[] { (numerator, false), (denominator, true) })
                foreach (var factor in Mulf.LinearChildren(side))
                {
                    if (factor is Powf(var @base, Number.Rational power) && power is not Number.Integer && @base.ContainsNode(x))
                    {
                        if (radicalBase is not null)
                            return null;
                        radicalBase = @base;
                        exponent = isBelow ? Number.Rational.Create(power.ERational.Negate()) : power;
                        continue;
                    }
                    if (!factor.ContainsNode(x))
                    {
                        if (isBelow) constantBelow = constantBelow * factor;
                        else rest = rest * factor;
                        continue;
                    }
                    if (isBelow || HasARadicalOf(factor, x))
                        return null;
                    rest = rest * factor;
                }
            if (radicalBase is null || exponent is null || HasARadicalOf(radicalBase, x)
                || !radicalBase.Nodes.Any(node => node.ContainsNode(x) && node is not (Variable or Sumf or Minusf or Mulf or Divf or Powf(_, Number.Integer))))
                return null;

            // The degree of N in x with the functions of x held constant.
            var atoms = new Dictionary<Entity, Entity.Variable>();
            var named = expr;
            Entity WithAtoms(Entity e) => e.Replace(node =>
            {
                if (!node.ContainsNode(x) || node is Variable or Sumf or Minusf or Mulf or Divf || node is Powf(_, Number.Integer { IsNegative: false }))
                    return node;
                if (!atoms.TryGetValue(node, out var atom))
                {
                    atom = Variable.CreateUnique(named, "atom");
                    named = named + atom;
                    atoms[node] = atom;
                }
                return atom;
            });
            var n = WithAtoms(rest);
            if (!TreeAnalyzer.TryGetPolynomial(n, x, out var nRead) || nRead.Keys.Any(k => !k.CanFitInInt32()))
                return null;
            var degree = (nRead.Count == 0 ? 0 : nRead.Keys.Max()!.ToInt32Unchecked()) + 1;
            if (degree > 4)
                return null;

            // P' B + (r + 1) P B' - N, linear in the unknowns; written with the functions of x
            // as indeterminates, as one polynomial, whose every coefficient must vanish.
            var unknowns = new List<Entity.Variable>();
            for (var k = 0; k <= degree; k++)
            {
                var unknown = Variable.CreateUnique(named, "c" + k);
                named = named + unknown;
                unknowns.Add(unknown);
            }
            Entity polynomial = Number.Integer.Zero;
            for (var k = 0; k <= degree; k++)
                polynomial = polynomial + unknowns[k] * (k == 0 ? Number.Integer.One : k == 1 ? x : MathS.Pow(x, k));
            var rPlusOne = Number.Rational.Create(exponent.ERational.Add(ERational.One));
            var identity = polynomial.Differentiate(x) * radicalBase + rPlusOne * polynomial * radicalBase.Differentiate(x) - rest / constantBelow;
            var (top, bottom) = Functions.SingleQuotient.Of(Functions.PartialFractions.Bare(WithAtoms(Functions.PartialFractions.Bare(identity))));
            if (bottom.ContainsNode(x) && !TreeAnalyzer.TryGetPolynomial(bottom, x, out _))
                return null;
            var variables = top.Vars.Where(v => !unknowns.Contains(v)).OrderBy(v => v.Name, System.StringComparer.Ordinal).ToList();
            if (variables.Count == 0 || variables.Count > MultivariatePolynomial.MaxVariables)
                return null;
            var indices = new Dictionary<Variable, int>();
            for (var i = 0; i < variables.Count; i++)
                indices[variables[i]] = i;
            // top = E_0 + sum_k c_k E_k.
            Entity AtUnit(int which)
            {
                var e = top;
                for (var k = 0; k <= degree; k++)
                    e = e.Substitute(unknowns[k], k == which ? Number.Integer.One : Number.Integer.Zero);
                return Functions.PartialFractions.Bare(e);
            }
            if (MultivariatePolynomial.TryParse(AtUnit(-1), indices) is not { } constantPart)
                return null;
            var columns = new List<MultivariatePolynomial>();
            for (var k = 0; k <= degree; k++)
            {
                if (MultivariatePolynomial.TryParse(AtUnit(k), indices) is not { } column)
                    return null;
                columns.Add(column.Subtract(constantPart));
            }
            var monomials = columns.SelectMany(column => column.Terms.Select(term => term.Key)).Concat(constantPart.Terms.Select(term => term.Key)).Distinct().ToList();
            var matrix = new Entity[monomials.Count][];
            var rhs = new Entity[monomials.Count];
            for (var row = 0; row < monomials.Count; row++)
            {
                matrix[row] = new Entity[degree + 1];
                for (var k = 0; k <= degree; k++)
                    matrix[row][k] = columns[k].Terms.FirstOrDefault(term => term.Key == monomials[row]).Value is { } c ? Number.Rational.Create(c) : Number.Integer.Zero;
                rhs[row] = constantPart.Terms.FirstOrDefault(term => term.Key == monomials[row]).Value is { } d ? Number.Rational.Create(d.Negate()) : Number.Integer.Zero;
            }
            if (!Functions.PartialFractions.TrySolveLinear(matrix, rhs, out var values) || values is null)
                return null;
            Entity p = Number.Integer.Zero;
            for (var k = 0; k <= degree; k++)
                if (values[k].Evaled is Number.Complex { IsZero: false })
                    p = p + values[k] * (k == 0 ? Number.Integer.One : k == 1 ? x : MathS.Pow(x, k));
            if (p == Number.Integer.Zero)
                return null;
            var answer = (p * MathS.Pow(radicalBase, rPlusOne)).InnerSimplified;
            // Checked as a fact about the function, since two functions taken for
            // independent indeterminates may not be.
            if (!Functions.PartialFractions.HoldsAtSampledPoints(answer.Differentiate(x), expr, x))
                return null;
            return answer;
        }

        /// <summary>
        /// An integrand <c>e^h R</c>, with <c>R</c> rational in <c>x</c> and <c>h</c> rational
        /// in <c>x</c> or absent, integrated by the ansatz <c>F = e^h N/D</c>: <c>D</c> read
        /// off the denominator of <c>R</c>, <c>N</c> a polynomial of unknown coefficients, and
        /// <c>F' = e^h R</c> a linear system in them.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>e^x x/(1 + x)^2</c> is <c>(e^x/(1 + x))'</c>; <c>e^(x^2)(1 + 2x^2)</c> is
        /// <c>(x e^(x^2))'</c>; <c>(4x^5 - 1)/(1 + x + x^5)^2</c> is <c>(-x/(1 + x + x^5))'</c>.
        /// None had an antiderivative: the first two are not a polynomial times an exponential,
        /// which is the shape by parts reads, and the third has a denominator nothing factors.
        /// Each is the derivative of something of the same shape, and that is what is looked
        /// for -- Liouville's theorem says the elementary antiderivative of <c>e^h R</c>, where
        /// there is one, is <c>e^h</c> times a rational function, so an ansatz that fails here
        /// fails because there is no such antiderivative and not because the shape was wrong.
        /// For <c>h = 0</c> this is the rational part of the Hermite reduction, with the
        /// logarithmic part required to vanish; a denominator with a repeated factor and a
        /// logarithmic part beside it is left to the splits, as before.
        /// </para>
        /// <para>
        /// <b>Which <c>D</c>.</b> Differentiating raises the multiplicity of every factor of the
        /// denominator by one, so <c>D</c> is the denominator of <c>R</c> with each factor's
        /// written power lowered by one, and then the denominator itself, and <c>1</c> where the
        /// denominator is <c>1</c>. With <c>h = p/q</c> the identity is
        /// </para>
        /// <code>
        ///     ((N' D - N D') q^2 + (p' q - p q') N D) D_R  =  N_R D^2 q^2
        /// </code>
        /// <para>
        /// a polynomial identity in <c>x</c>, linear in the coefficients of <c>N</c>, solved by
        /// the same elimination the symbolic partial-fraction split uses and checked the same way
        /// before anything is returned: at sampled points with every symbol pinned. The degree
        /// of <c>N</c> is bounded by the degrees in that identity, with a little to spare;
        /// an unknown the system does not need is zero.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveByExponentialAnsatz(Entity expr, Entity.Variable x)
        {
            // A sum whose terms share one exponential is read whole -- `e^(x^2) + 2x^2 e^(x^2)`
            // is `(x e^(x^2))'` and neither term is elementary on its own, so splitting the sum
            // first, which linearity does, loses it.
            Entity? exponent = null;
            Entity rest = 0;
            foreach (var term in Sumf.LinearChildren(expr))
            {
                if (ReadOneExponentialTimesTheRest(term, x) is not var (h, r) || h is null)
                    return null;
                if (exponent is null)
                    exponent = h;
                else if (exponent != h)
                    return null;
                rest += r;
            }
            return exponent is null || IntegrateByAnsatz(exponent, rest, x) is not { } answer ? null : InTheBaseWritten(answer, expr, x);
        }

        /// <summary>
        /// <paramref name="answer"/> with the exponential written in the base the integrand
        /// wrote it in: the ansatz reads <c>F^u</c> as <c>e^(u ln F)</c> and builds its answer
        /// on that, and <c>F^(a + b x + c x^3)/ln(F)</c> is the answer to give for Rubi's
        /// <c>F^(a + b x + c x^3)(b + 3 c x^2)</c>, not <c>e^((a + b x + c x^3) ln F)/ln(F)</c>.
        /// </summary>
        private static Entity InTheBaseWritten(Entity answer, Entity integrand, Entity.Variable x)
        {
            var written = new Dictionary<Entity, Entity>();
            foreach (var node in integrand.Nodes)
                if (node is Powf(var @base, var power) && @base != MathS.e && !@base.ContainsNode(x) && power.ContainsNode(x) && power is not Number)
                {
                    written[(power * MathS.Ln(@base)).InnerSimplified] = node;
                    written[(-power * MathS.Ln(@base)).InnerSimplified] = MathS.Pow(@base, -power);
                }
            return written.Count == 0 ? answer
                : answer.Replace(node => node is Powf(var e, var exponent) && e == MathS.e && written.TryGetValue(exponent, out var asWritten) ? asWritten : node);
        }

        /// <summary>
        /// <c>e^(a x)</c> times a rational function of <c>sin(x)</c> and <c>cos(x)</c>, closed by
        /// an ansatz in the half-angle tangent: <c>F = e^(a x) P(t)/Q(t)</c> with <c>t = tan(x/2)</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Timofeev's <c>e^x (1 - sin(x))/(1 - cos(x))</c> had no antiderivative, and it is
        /// <c>-e^x cot(x/2)</c>. Nothing here reads it: the exponential times a trigonometric
        /// rule wants a polynomial in sine and cosine, the half-angle substitution wants no
        /// exponential, and by parts goes round in a circle. Under <c>t = tan(x/2)</c> the
        /// trigonometric part is a rational function <c>r(t)</c>, and the integrand is
        /// <c>e^(a x) r(t)</c> with <c>dt/dx = (1 + t^2)/2</c>; differentiating the ansatz,
        /// </para>
        /// <code>
        ///     F' = e^(a x) [a P/Q + (P'Q - P Q') (1 + t^2)/(2 Q^2)]
        /// </code>
        /// <para>
        /// and asking that it equal <c>e^(a x) n(t)/d(t)</c> is one polynomial identity,
        /// <c>[a P Q + (P'Q - P Q')(1 + t^2)/2] d = n Q^2</c>, linear in the coefficients of
        /// <c>P</c>. <c>Q</c> is tried from <c>d</c>'s written factors, as the exponential ansatz
        /// tries its denominators; the identity is exact, so a solution is an answer and the
        /// absence of one a decline. Timofeev's has <c>r = (1 - t)^2/(2 t^2)</c>, <c>Q = t</c>,
        /// <c>P = -1</c>.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveByExponentialHalfAngleAnsatz(Entity expr, Entity.Variable x)
        {
            if (!Integration.AnsweringTheQuestionAsked)
                return null;
            if (ReadOneExponentialTimesTheRest(expr, x) is not var (exponent, rest) || exponent is null)
                return null;
            if (!TreeAnalyzer.TryGetPolyLinear(exponent, x, out var a, out _) || a.ContainsNode(x) || a.Evaled is Number.Complex { IsZero: true })
                return null;
            // The rest is built from sin(x) and cos(x) by the field operations, and no other
            // function of x: written over the two, then in the half-angle tangent.
            var sine = MathS.Sin(x);
            var cosine = MathS.Cos(x);
            var overTheTwo = rest.Replace(node => node switch
            {
                Tanf(var arg) when arg == x => sine / cosine,
                Cotanf(var arg) when arg == x => cosine / sine,
                Secantf(var arg) when arg == x => 1 / cosine,
                Cosecantf(var arg) when arg == x => 1 / sine,
                _ => node
            });
            if (overTheTwo.Nodes.Any(node => node.ContainsNode(x) && node is not (Variable or Sumf or Minusf or Mulf or Divf or Sinf or Cosf)
                                              && !(node is Powf(_, Number.Integer))))
                return null;
            if (overTheTwo.Nodes.Any(node => node is Sinf(var arg) && arg != x || node is Cosf(var arg2) && arg2 != x))
                return null;

            var t = Variable.CreateUnique(expr, "t_half");
            var tSquared = MathS.Sqr(t);
            var inT = Functions.SingleQuotient.Combine(
                overTheTwo.Substitute(sine, 2 * t / (1 + tSquared)).Substitute(cosine, (1 - tSquared) / (1 + tSquared))).Simplify();
            if (inT is Providedf(var inner, _))
                inT = inner;
            if (inT.ContainsNode(x))
                return null;
            var (n, d) = Functions.SingleQuotient.Of(inT);
            if (!TreeAnalyzer.TryGetPolynomial(n, t, out var nRead) || !TreeAnalyzer.TryGetPolynomial(d, t, out var dRead))
                return null;
            var degreeN = nRead.Count == 0 ? 0 : nRead.Keys.Max()!.ToInt32Checked();
            var degreeD = dRead.Count == 0 ? 0 : dRead.Keys.Max()!.ToInt32Checked();
            if (degreeN > MaximumAnsatzDegree || degreeD > MaximumAnsatzDegree)
                return null;

            var half = Number.Rational.Create(1, 2);
            foreach (var q in CandidateDenominators(d, t))
            {
                var degreeQ = TreeAnalyzer.TryGetPolynomial(q, t, out var qRead) && qRead.Count > 0
                    ? qRead.Keys.Max()!.ToInt32Checked() : 0;
                // The bracket has degree deg P + max(deg Q + 1, ...) against n Q^2 / d.
                var degreeP = System.Math.Max(degreeQ, degreeQ + degreeN - degreeD + 2);
                if (degreeP > MaximumAnsatzDegree)
                    continue;
                var qPrime = q.Differentiate(t);
                var columns = new List<Dictionary<EInteger, Entity>>();
                var failed = false;
                for (var k = 0; k <= degreeP; k++)
                {
                    Entity tk = k == 0 ? Number.Integer.One : k == 1 ? t : MathS.Pow(t, k);
                    Entity tkPrime = k == 0 ? Number.Integer.Zero : k == 1 ? Number.Integer.One : k * MathS.Pow(t, k - 1);
                    var bracket = a * tk * q + (tkPrime * q - tk * qPrime) * (1 + tSquared) * half;
                    if (!TreeAnalyzer.TryGetPolynomial(Functions.PartialFractions.Bare((bracket * d).Expand()), t, out var column))
                    {
                        failed = true;
                        break;
                    }
                    columns.Add(column);
                }
                if (failed)
                    continue;
                if (!TreeAnalyzer.TryGetPolynomial(Functions.PartialFractions.Bare((n * MathS.Sqr(q)).Expand()), t, out var target))
                    continue;
                var monomials = columns.SelectMany(column => column.Keys).Concat(target.Keys).Distinct().OrderBy(k => k).ToList();
                var matrix = new Entity[monomials.Count][];
                var rhs = new Entity[monomials.Count];
                for (var row = 0; row < monomials.Count; row++)
                {
                    matrix[row] = new Entity[columns.Count];
                    for (var k = 0; k < columns.Count; k++)
                        matrix[row][k] = columns[k].TryGetValue(monomials[row], out var entry) ? entry : Number.Integer.Zero;
                    rhs[row] = target.TryGetValue(monomials[row], out var wanted) ? wanted : Number.Integer.Zero;
                }
                if (!Functions.PartialFractions.TrySolveLinear(matrix, rhs, out var values) || values is null)
                    continue;
                Entity polynomial = Number.Integer.Zero;
                for (var k = 0; k < values.Length; k++)
                {
                    var value = values[k].InnerSimplified;
                    if (value.Evaled is Number.Complex { IsZero: true })
                        continue;
                    polynomial += value * (k == 0 ? Number.Integer.One : k == 1 ? t : MathS.Pow(t, k));
                }
                var tangent = MathS.Tan(x / 2);
                var answer = (MathS.Pow(MathS.e, exponent) * polynomial / q).Substitute(t, tangent);
                // The solve can answer a system it only nearly satisfies; the derivative decides.
                if (!Functions.PartialFractions.HoldsAtSampledPoints(answer.Differentiate(x), expr, x))
                    continue;
                return answer.InnerSimplified;
            }
            return null;
        }

        /// <summary>
        /// An exponential times a rational function of <c>x</c> and <c>ln(x)</c>, with the
        /// logarithm allowed in the exponent too, closed by an ansatz
        /// <c>F = e^h P(x, L)/(D(x) L^k)</c> with <c>L = ln(x)</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Hearn's <c>(-1 + (1 - x) ln(x))/(e^x ln(x)^2)</c> is <c>(x e^(-x)/ln(x))'</c> and Hebisch's
        /// <c>e^(x + 1/ln(x)) (-1 + (1 + x) ln(x)^2)/ln(x)^2</c> is <c>(x e^(x + 1/ln(x)))'</c>; neither
        /// had an antiderivative, and neither is reached by parts or by any substitution,
        /// since the logarithm is not a whole subtree to replace. They are the exponential
        /// ansatz one level up the tower: <c>x</c> and <c>ln(x)</c> are algebraically
        /// independent, so with <c>L</c> standing for <c>ln(x)</c> and <c>L' = 1/x</c>, the
        /// derivative of the ansatz divided by <c>e^h</c> is a rational function of <c>x</c> and
        /// <c>L</c>, and asking it to equal the integrand's is one polynomial identity in the
        /// two -- linear in the coefficients of <c>P</c>, one equation per monomial
        /// <c>x^i L^j</c>. The identity is exact, so a solution is an answer and its absence a
        /// decline; the derivative of what comes out is checked against the integrand at
        /// sampled points all the same.
        /// </para>
        /// <para>
        /// The denominator is tried from the integrand's: its <c>x</c>-part as the exponential
        /// ansatz tries its denominators, times <c>L^k</c> for every <c>k</c> from zero up to
        /// the written power, smallest first so that the answer carries no common factor. Degrees are bounded like the other
        /// ansatz's, and the columns are built one monomial at a time by differentiating the
        /// candidate and clearing one common denominator, so nothing here is expanded with
        /// unknowns in it.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveByLogarithmTowerAnsatz(Entity expr, Entity.Variable x)
        {
            if (!Integration.AnsweringTheQuestionAsked)
                return null;
            var logarithm = MathS.Ln(x);
            if (!expr.ContainsNode(logarithm))
                return null;
            if (ReadOneExponentialTimesTheRest(expr, x) is not var (exponent, rest) || exponent is null)
                return null;

            // Rational in x and L, and nothing else of x, on both sides.
            var L = Variable.CreateUnique(expr, "L_tower");
            var restInL = rest.Substitute(logarithm, L);
            var exponentInL = exponent.Substitute(logarithm, L);
            static bool IsRationalInBoth(Entity e, Entity.Variable x, Entity.Variable L)
                => e.Nodes.All(node => !(node.ContainsNode(x) || node.ContainsNode(L))
                    || node is Variable or Sumf or Minusf or Mulf or Divf
                    || node is Powf(_, Number.Integer));
            if (!IsRationalInBoth(restInL, x, L) || !IsRationalInBoth(exponentInL, x, L))
                return null;
            if (!exponentInL.ContainsNode(x) && !exponentInL.ContainsNode(L))
                return null;

            var (above, below) = Functions.SingleQuotient.Of(restInL.InnerSimplified);
            if (!TryReadInBoth(above, x, L, out var aboveRead) || !TryReadInBoth(below, x, L, out var belowRead))
                return null;
            var degreeAboveX = aboveRead.Keys.Max(k => k.Item1);
            var degreeAboveL = aboveRead.Keys.Max(k => k.Item2);
            var degreeBelowX = belowRead.Keys.Max(k => k.Item1);
            var degreeBelowL = belowRead.Keys.Max(k => k.Item2);
            if (degreeAboveX > MaximumAnsatzDegree || degreeBelowX > MaximumAnsatzDegree
                || degreeAboveL > MaximumTowerDegree || degreeBelowL > MaximumTowerDegree)
                return null;

            // The denominator's x-part and L-part, read off the written factors.
            Entity xPart = Number.Integer.One;
            var lPower = 0;
            foreach (var factor in Mulf.LinearChildren(below))
            {
                if (factor == L) { lPower += 1; continue; }
                if (factor is Powf(var b, Number.Integer e) && b == L && e.EInteger.CanFitInInt32()) { lPower += e.EInteger.ToInt32Unchecked(); continue; }
                if (factor.ContainsNode(L))
                    return null;   // L inside a written factor with x: not a shape this reads
                xPart = xPart * factor;
            }

            var hPrime = exponentInL.Differentiate(x) + exponentInL.Differentiate(L) / x;
            var degreePX = System.Math.Max(degreeBelowX, degreeBelowX + degreeAboveX - degreeBelowX + 2);
            var degreePL = System.Math.Max(lPower, degreeAboveL + 1);
            if (degreePX > MaximumAnsatzDegree || degreePL > MaximumTowerDegree)
                return null;

            // The smallest power of L first, so that the answer found is the one without a
            // common factor of L above and below.
            var lPowers = new List<int>();
            for (var k = 0; k <= lPower; k++)
                lPowers.Add(k);
            foreach (var d in CandidateDenominators(xPart, x))
                foreach (var k in lPowers)
                {
                    var denominator = (k == 0 ? d : d * MathS.Pow(L, k));
                    // The derivative of e^h x^i L^j / (d L^k), over e^h, has denominator dividing
                    // x d^2 L^(k+1) times whatever h' brought; clearing by that and the
                    // integrand's denominator gives polynomials on both sides.
                    var (hAbove, hBelow) = Functions.SingleQuotient.Of(hPrime.InnerSimplified);
                    var clearing = x * MathS.Sqr(denominator) * L * hBelow;
                    var columns = new List<Dictionary<(int, int), Entity>>();
                    var monomialsOfP = new List<(int, int)>();
                    var failed = false;
                    for (var i = 0; i <= degreePX && !failed; i++)
                        for (var j = 0; j <= degreePL; j++)
                        {
                            // The candidate with its powers of L folded into one, so that its
                            // derivative is a Laurent monomial and not a quotient rule on L/L.
                            Entity candidate = MathS.Pow(x, i) * (j - k == 0 ? Number.Integer.One : MathS.Pow(L, j - k)) / d;
                            var derivative = hPrime * candidate + candidate.Differentiate(x) + candidate.Differentiate(L) / x;
                            var cleared = Functions.PartialFractions.Bare(
                                Functions.SingleQuotient.Combine(derivative * clearing * below).Simplify().Expand());
                            if (!TryReadInBoth(cleared, x, L, out var column))
                            {
                                failed = true;
                                break;
                            }
                            columns.Add(column);
                            monomialsOfP.Add((i, j));
                        }
                    if (failed)
                        continue;
                    if (!TryReadInBoth(Functions.PartialFractions.Bare((above * clearing).Simplify().Expand()), x, L, out var target))
                        continue;

                    var monomials = columns.SelectMany(c => c.Keys).Concat(target.Keys).Distinct().OrderBy(m => m).ToList();
                    var matrix = new Entity[monomials.Count][];
                    var rhs = new Entity[monomials.Count];
                    for (var row = 0; row < monomials.Count; row++)
                    {
                        matrix[row] = new Entity[columns.Count];
                        for (var c = 0; c < columns.Count; c++)
                            matrix[row][c] = columns[c].TryGetValue(monomials[row], out var entry) ? entry : Number.Integer.Zero;
                        rhs[row] = target.TryGetValue(monomials[row], out var wanted) ? wanted : Number.Integer.Zero;
                    }
                    if (!Functions.PartialFractions.TrySolveLinear(matrix, rhs, out var values) || values is null)
                        continue;
                    Entity numerator = Number.Integer.Zero;
                    for (var c = 0; c < values.Length; c++)
                    {
                        var value = values[c].InnerSimplified;
                        if (value.Evaled is Number.Complex { IsZero: true })
                            continue;
                        var (i, j) = monomialsOfP[c];
                        numerator += value * MathS.Pow(x, i) * MathS.Pow(L, j);
                    }
                    if (numerator.Evaled is Number.Complex { IsZero: true })
                        continue;
                    // The generic case, as the integrator answers everywhere: the condition
                    // simplifying attaches, `not ln(x) = 0`, is the integrand's own pole.
                    var answer = Functions.PartialFractions.Bare(
                        (MathS.Pow(MathS.e, exponent) * (numerator / denominator).Substitute(L, logarithm)).InnerSimplified);
                    if (!Functions.PartialFractions.HoldsAtSampledPoints(answer.Differentiate(x), expr, x))
                        continue;
                    return answer;
                }
            return null;
        }

        /// <summary>The largest power of the logarithm the tower ansatz reads or tries.</summary>
        private const int MaximumTowerDegree = 4;

        /// <summary>
        /// A product of powers times a sum that is the derivative of the product with some of
        /// the powers raised by one: <c>e^x x^2 ln(x)^2 (3 + (3 + x) ln(x))</c> is
        /// <c>(e^x x^3 ln(x)^3)'</c>, and Rubi's
        /// <c>F^(c (a + b x)) x^m ln(d x)^n (p + p n + p (1 + m + b c x ln F) ln(d x))</c> is
        /// <c>(p F^(c (a + b x)) x^(m + 1) ln(d x)^(n + 1))'</c>, for symbols <c>m</c> and <c>n</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The product rule on <c>G = K prod f_i^(e_i)</c> gives <c>G' = G sum e_i f_i'/f_i</c>,
        /// so an integrand of that shape is a product of the same powers, each lowered where
        /// its base's derivative divides it, times a sum. Read the other way: the sum in the
        /// integrand names the powers to raise. Every subset of the powers with a base holding
        /// <c>x</c> and an exponent free of it is tried raised by one, the exponentials kept,
        /// and the integrand divided by the candidate's derivative must be a constant: decided
        /// at sampled points first, with every symbol pinned, and where it is, the constant is
        /// the quotient of the two sums simplified -- <c>p</c> above -- and the answer is checked
        /// by differentiating it back. Nothing else reads a symbolic exponent: the Risch-Norman
        /// ansatz wants whole powers of its monomials, and splitting the sum loses it, since
        /// <c>F^(c(a + bx)) x^m ln(dx)^n</c> on its own is not elementary.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveAsTheDerivativeOfAProductOfPowers(Entity expr, Entity.Variable x)
        {
            if (!Integration.AnsweringTheQuestionAskedOrOneBelow)
                return null;
            Entity constant = Number.Integer.One;
            Entity? bracket = null;
            var raisable = new List<(Entity Base, Entity Exponent)>();
            var logarithmicDerivativesKept = new List<Entity>();
            Entity kept = Number.Integer.One;
            foreach (var (factor, underneath) in FactorsOfTheIntegrand(expr))
            {
                if (!factor.ContainsNode(x))
                {
                    constant = underneath ? constant / factor : constant * factor;
                    continue;
                }
                if (factor is Sumf or Minusf)
                {
                    if (bracket is not null || underneath)
                        return null;
                    bracket = factor;
                    continue;
                }
                if (factor is Powf(var @base, var exponent) && !exponent.ContainsNode(x))
                {
                    raisable.Add((@base, underneath ? (-exponent).InnerSimplified : exponent));
                    continue;
                }
                if (factor is Powf(var constantBase, var power) && !constantBase.ContainsNode(x))
                {
                    var h = constantBase == MathS.e ? power : power * MathS.Ln(constantBase);
                    var derivative = Functions.PartialFractions.Bare(h.Differentiate(x).InnerSimplified);
                    logarithmicDerivativesKept.Add(underneath ? -derivative : derivative);
                    kept = underneath ? kept / factor : kept * factor;
                    continue;
                }
                raisable.Add((factor, underneath ? Number.Integer.MinusOne : Number.Integer.One));
            }
            if (bracket is null || raisable.Count == 0 || raisable.Count > 4)
                return null;
            for (var subset = 1; subset < 1 << raisable.Count; subset++)
            {
                Entity candidate = kept;
                Entity productOfRaisedBases = Number.Integer.One;
                Entity logarithmicDerivative = Number.Integer.Zero;
                foreach (var derivative in logarithmicDerivativesKept)
                    logarithmicDerivative += derivative;
                for (var i = 0; i < raisable.Count; i++)
                {
                    var (@base, exponent) = raisable[i];
                    var raised = (subset & (1 << i)) != 0;
                    var newExponent = raised ? (exponent + 1).InnerSimplified : exponent;
                    if (raised)
                        productOfRaisedBases *= @base;
                    if (newExponent.Evaled is Number.Complex { IsZero: true })
                        continue;   // a power raised to nothing: not a factor of the candidate
                    candidate *= newExponent == Number.Integer.One ? @base : MathS.Pow(@base, newExponent);
                    logarithmicDerivative += newExponent * @base.Differentiate(x) / @base;
                }
                if (candidate == kept)
                    continue;
                // The bracket the candidate's derivative has, against the integrand's: the
                // candidate is the integrand's powers with the raised bases in besides, so
                // G' is (integrand without its bracket) (product of raised bases) (sum of the
                // logarithmic derivatives), and the constant is the quotient of the brackets.
                var bracketOfTheDerivative = Functions.PartialFractions.Bare((productOfRaisedBases * logarithmicDerivative).InnerSimplified);
                if (!AreProportionalAtSampledPoints(bracket, bracketOfTheDerivative, x))
                    continue;
                // The constant is the quotient of one monomial's coefficients, the two
                // brackets read as polynomials in x and in the transcendental atoms holding
                // it -- `ln(d x)` -- each an indeterminate; or, where they do not read so,
                // the quotient of the brackets simplified.
                if (!TryReadTheRatioOfBrackets(bracket, bracketOfTheDerivative, x, out var k))
                {
                    k = Functions.SingleQuotient.Combine(bracket / bracketOfTheDerivative).Simplify();
                    if (k is Providedf(var inner, _))
                        k = inner;
                }
                k = Functions.PartialFractions.Bare((constant * k).InnerSimplified);
                if (k.ContainsNode(x) || k.Nodes.Any(node => node == MathS.NaN))
                    continue;
                var answer = (k * candidate).InnerSimplified;
                bool holds;
                using (MathS.Settings.DowncastingEnabled.Set(false))
                    holds = Functions.PartialFractions.HoldsAtSampledPoints(answer.Differentiate(x), expr, x);
                if (holds)
                    return answer;
            }
            return null;
        }

        /// <summary>
        /// The constant <c>k</c> with <paramref name="left"/> equal to <c>k</c> times
        /// <paramref name="right"/>, read off one monomial with both written as polynomials
        /// in <paramref name="x"/> and in every atom holding it -- a logarithm, an exponential
        /// -- as an indeterminate; false where either does not read so, or the monomial is
        /// missing from <paramref name="left"/>. Whether the constant holds for every
        /// monomial is the caller's to check.
        /// </summary>
        private static bool TryReadTheRatioOfBrackets(Entity left, Entity right, Entity.Variable x, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Entity? k)
        {
            k = null;
            var atoms = new List<(Entity Atom, Entity.Variable Symbol)>();
            Entity everything = left + right;
            Entity InAtoms(Entity entry) => entry.Replace(node =>
            {
                if (!node.ContainsNode(x) || node == x || node is Sumf or Minusf or Mulf or Divf || node is Powf(_, Number.Integer { EInteger.Sign: >= 0 }))
                    return node;
                foreach (var (atom, symbol) in atoms)
                    if (atom == node)
                        return symbol;
                var fresh = Variable.CreateUnique(everything, "t_atom");
                atoms.Add((node, fresh));
                everything += fresh;
                return fresh;
            });
            var leftInAtoms = InAtoms(left);
            var rightInAtoms = InAtoms(right);
            var variables = new List<Entity.Variable> { x };
            variables.AddRange(atoms.Select(pair => pair.Symbol));
            if (!TryReadInAll(leftInAtoms, variables, out var leftRead) || !TryReadInAll(rightInAtoms, variables, out var rightRead))
                return false;
            // A monomial whose coefficient on the right is a number first, so that the
            // constant is the left coefficient over it and not a quotient of symbols to be
            // cancelled; the quotient is simplified either way, and it is small.
            foreach (var numbersFirst in new[] { true, false })
                foreach (var pair in rightRead)
                {
                    if (pair.Value.Evaled is Number.Complex { IsZero: true } || numbersFirst != pair.Value.Evaled is Number.Rational)
                        continue;
                    if (!leftRead.TryGetValue(pair.Key, out var above))
                        return false;
                    k = Functions.PartialFractions.Bare((above / pair.Value).Simplify());
                    return true;
                }
            return false;
        }

        /// <summary>
        /// Whether <paramref name="left"/> is a constant multiple of <paramref name="right"/>,
        /// decided at sampled points with every symbol pinned; false where fewer than two
        /// points evaluate.
        /// </summary>
        private static bool AreProportionalAtSampledPoints(Entity left, Entity right, Entity.Variable x)
        {
            // Sampled in decimals: a pinned symbol is a small rational, and a symbolic
            // exponent pinned so makes `(F^(g (e + f x)))^n` an exact rational power whose
            // numerator has more digits than there is time for.
            using var _ = MathS.Settings.DowncastingEnabled.Set(false);
            var parameters = left.Vars.Concat(right.Vars).Where(v => v != x).Distinct().ToList();
            var pinned = 0;
            foreach (var parameter in parameters)
            {
                var fraction = (pinned % 3) switch { 0 => "1.37", 1 => "2.71", _ => "0.83" };
                var value = Number.Real.Create(EDecimal.FromString(fraction).Add(EDecimal.FromInt32(pinned)));
                left = left.Substitute(parameter, value);
                right = right.Substitute(parameter, value);
                pinned++;
            }
            Number.Complex? ratio = null;
            var compared = 0;
            foreach (var at in new[] { "0.29", "1.43", "3.17", "0.61" })
            {
                var point = Number.Real.Create(EDecimal.FromString(at));
                var l = left.Substitute(x, point).EvalNumerical();
                var r = right.Substitute(x, point).EvalNumerical();
                if (l.IsNaN || r.IsNaN || r.Abs().EDecimal.CompareTo(EDecimal.FromString("1e-30")) < 0)
                    continue;
                var here = l / r;
                if (ratio is null)
                    ratio = here;
                else
                {
                    var difference = (here - ratio).Abs().EDecimal;
                    var scale = EDecimal.Max(EDecimal.One, ratio.Abs().EDecimal);
                    if (difference.CompareTo(scale.Multiply(EDecimal.FromString("1e-9"))) > 0)
                        return false;
                }
                compared++;
            }
            return compared >= 2;
        }

        /// <summary>
        /// A rational function of <c>x</c> and of exponentials and logarithms built over it,
        /// integrated by the Risch-Norman ansatz: <c>F = P/Q + sum c_j ln(q_j)</c> with <c>P</c> a
        /// polynomial of unknown coefficients in <c>x</c> and the transcendental monomials, <c>Q</c>
        /// tried from the integrand's denominator, and the <c>q_j</c> its factors.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Hearn's <c>e^(1 - x e^(x^2) + 2x^2)(x + 2x^3)/(1 - x e^(x^2))^2</c> is
        /// <c>(e^(1 - x e^(x^2))/(1 - x e^(x^2)))'</c>, and his
        /// <c>e^(x^2)/x + 2x e^(x^2) ln(x) + (ln(x) - 2)/(x + ln(x)^2)^2 + (1 + 1/x + 2 ln(x)/x)/(x + ln(x)^2)</c>
        /// is <c>(e^(x^2) ln(x) - ln(x)/(x + ln(x)^2) + ln(x + ln(x)^2))'</c>. Neither had an
        /// antiderivative: the first is an exponential of something with an exponential in it,
        /// and the second a sum whose terms are not elementary apart -- <c>e^(x^2)/x</c> is
        /// not -- so that every split loses it. With <c>t_1 = e^(x^2)</c>, <c>t_2 = ln(x)</c>,
        /// <c>t_3 = e^(1 - x t_1 + 2x^2)</c> for indeterminates, each with the derivative it has
        /// -- <c>2x t_1</c>, <c>1/x</c>, <c>(4x - t_1 - 2x^2 t_1) t_3</c> -- the integrand is a
        /// rational function of <c>x</c> and the <c>t</c>, and Liouville's theorem says its
        /// elementary antiderivative, where there is one, is a rational function of the same
        /// plus logarithms with constant coefficients. The rational part's denominator divides
        /// the integrand's with each factor's power lowered by one, the exponential monomials
        /// excepted, which may stand to any power; the logarithms' arguments are the
        /// denominator's factors. So <c>F' = f</c>, over one denominator, is a polynomial
        /// identity in <c>x</c> and the <c>t</c>, linear in the unknown coefficients of <c>P</c>
        /// and the <c>c_j</c>: one equation per monomial, solved by the elimination the other
        /// ansätze use. The identity is exact, so a solution is an answer and its absence a
        /// decline; the derivative of what comes out is checked against the integrand at
        /// sampled points all the same. This is the parallel Risch algorithm of Norman and
        /// Moore as a heuristic, with the degrees bounded as the tower ansätze bound theirs.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveByRischNormanAnsatz(Entity expr, Entity.Variable x)
        {
            if (!Integration.AnsweringTheQuestionAsked)
                return null;
            // The transcendental monomials: every exponential and natural logarithm of
            // something in x, innermost first, each read in the indeterminates of the ones
            // inside it.
            var towers = expr.Nodes
                .Where(node => node.ContainsNode(x) && (node is Powf(var b, var e) && b == MathS.e && e.ContainsNode(x)
                                                       || node is Logf(var lb, var arg) && lb == MathS.e && arg.ContainsNode(x)))
                .Distinct()
                .OrderBy(node => node.Nodes.Count())
                .ToList();
            if (towers.Count == 0 || towers.Count > 3)
                return null;
            // Only a tower the closed rules do not read: an exponential of something that is
            // not linear in x -- of a polynomial, or of something with an exponential or a
            // logarithm in it -- or a logarithm of something other than x. Exponentials of
            // linears, however many, are the exponential substitution's, and `ln(x)` beside
            // them the logarithm tower's; this rule before the splits took twenty seconds on
            // `e^x (1 + sinh(x))/(1 + cosh(x))` to decline what those answer in a moment.
            static bool IsPlain(Entity tower, Entity.Variable x)
                => tower is Powf(_, var g) && TreeAnalyzer.TryGetPolyLinear(g, x, out _, out _)
                   || tower is Logf(_, var argument) && argument == x;
            if (towers.All(tower => IsPlain(tower, x)))
                return null;
            var monomials = new List<(Entity.Variable Indeterminate, Entity Written, Entity Derivative)>();
            var map = new Dictionary<Entity, Entity>();
            Entity InIndeterminates(Entity e) => e.Replace(node => map.TryGetValue(node, out var t) ? t : node);
            foreach (var tower in towers)
            {
                var written = InIndeterminates(tower);
                if (map.ContainsKey(written))
                    continue;
                var t = Variable.CreateUnique(expr, $"t{monomials.Count + 1}_rn");
                // t' in x and the indeterminates so far: e^g has g' e^g, and ln(g) has g'/g.
                Entity derivative = written switch
                {
                    Powf(_, var g) => Derived(g) * t,
                    Logf(_, var g) => Derived(g) / g,
                    _ => Number.Integer.Zero,
                };
                map[written] = t;
                monomials.Add((t, written, derivative));
            }
            Entity Derived(Entity g)
            {
                var result = g.Differentiate(x);
                foreach (var (t, _, derivative) in monomials)
                    if (g.ContainsNode(t))
                        result += g.Differentiate(t) * derivative;
                return result.InnerSimplified;
            }
            var variables = new List<Entity.Variable> { x };
            variables.AddRange(monomials.Select(m => m.Indeterminate));
            var integrand = InIndeterminates(expr);
            if (integrand.Nodes.Any(node => variables.Any(node.ContainsNode) && node is not (Variable or Sumf or Minusf or Mulf or Divf) && node is not Powf(_, Number.Integer)))
                return null;
            // With x itself in it beside the towers: a function of exponentials alone --
            // `e^x sech(e^x)` -- is the exponential substitution's, and three seconds of a
            // system that has no arctangent to offer.
            if (!integrand.ContainsNode(x))
                return null;
            // Rational in everything: read as a quotient of polynomials, cancelled by the
            // greatest common divisor with the indeterminates as variables -- a sum brought
            // over one bar carries every term's denominator, and the degrees decide the size
            // of the system.
            var (above, below) = Functions.SingleQuotient.Of(Functions.SingleQuotient.Combine(integrand).InnerSimplified);
            if (Functions.PolynomialGcd.TryCancel(above.Expand().InnerSimplified, below.Expand().InnerSimplified, out var cancelled) && cancelled is not null)
                (above, below) = Functions.SingleQuotient.Of(Functions.PartialFractions.Bare(cancelled));
            if (!TryReadInAll(above, variables, out var aboveRead) || !TryReadInAll(below, variables, out var belowRead))
                return null;
            var degreeAbove = aboveRead.Keys.Select(k => k.Max()).DefaultIfEmpty(0).Max();
            var degreeBelow = belowRead.Keys.Select(k => k.Max()).DefaultIfEmpty(0).Max();
            if (degreeAbove > MaximumAnsatzDegree || degreeBelow > MaximumAnsatzDegree)
                return null;

            // The denominator's factors with their powers, the exponential indeterminates
            // apart, which may stand to any power in the answer's denominator: of each term of
            // the integrand as written, the highest power of each factor -- the least common
            // multiple, where bringing the sum over one bar multiplies the terms' denominators
            // together.
            var factors = new List<(Entity Factor, int Power)>();
            foreach (var term in Sumf.LinearChildren(integrand))
            {
                var (_, termBelow) = Functions.SingleQuotient.Of(term);
                foreach (var factor in Mulf.LinearChildren(termBelow))
                {
                    var (@base, power) = factor is Powf(var inner, Number.Integer whole) && whole.EInteger.CanFitInInt32() ? (inner, whole.EInteger.ToInt32Unchecked()) : (factor, 1);
                    if (!variables.Any(@base.ContainsNode))
                        continue;
                    if (@base is Variable v && monomials.Any(m => m.Indeterminate == v && m.Written is Powf))
                        continue;
                    var index = factors.FindIndex(f => f.Factor == @base);
                    if (index >= 0)
                        factors[index] = (@base, System.Math.Max(factors[index].Power, power));
                    else
                        factors.Add((@base, power));
                }
            }
            // The derivative of a generator, with the tower's derivatives in it, over the
            // indeterminates: symbolic in the indeterminates, since each has the derivative
            // the chain rule gives it.
            Entity Differentiated(Entity generator)
            {
                var result = generator.Differentiate(x);
                foreach (var (t, _, derivative) in monomials)
                    if (generator.ContainsNode(t))
                        result += generator.Differentiate(t) * derivative;
                return result;
            }
            var exponentialIndeterminates = monomials.Where(m => m.Written is Powf).Select(m => m.Indeterminate).ToList();
            // Q: the factors with their powers lowered by one, and then as they are; each
            // exponential indeterminate to every power from zero up to the numerator's degree
            // in it plus one, since a power of an exponential above the bar is one below in
            // the answer -- Hearn's has e^(2x^2) above and 1/e^(2x^2) in its answer.
            var qCandidates = new List<Entity>();
            foreach (var lowered in new[] { true, false })
            {
                Entity q = Number.Integer.One;
                foreach (var (factor, power) in factors)
                {
                    var p = lowered ? power - 1 : power;
                    if (p > 0)
                        q = q * (p == 1 ? factor : MathS.Pow(factor, p));
                }
                qCandidates.Add(q);
            }
            qCandidates = qCandidates.Distinct().ToList();
            var exponentialBounds = exponentialIndeterminates.Select(t =>
            {
                var index = variables.IndexOf(t);
                var aboveDegree = aboveRead.Keys.Select(k => k[index]).DefaultIfEmpty(0).Max();
                return System.Math.Min(MaximumTowerDegree, aboveDegree + 2);
            }).ToList();
            var logarithmGenerators = factors.Select(f => f.Factor).Distinct().ToList();
            foreach (var q in qCandidates)
                foreach (var exponentPowers in Combinations(exponentialBounds))
                {
                    Entity denominator = q;
                    for (var i = 0; i < exponentialIndeterminates.Count; i++)
                        if (exponentPowers[i] > 0)
                            denominator = denominator * MathS.Pow(exponentialIndeterminates[i], exponentPowers[i]);
                    // The generators: x^a prod t^b over the denominator, and the logarithms of
                    // the denominator's factors. Everything is brought over one denominator,
                    // `denominator^2 * below` times the logarithms' arguments and the ln-towers'
                    // arguments (whose derivatives have them below), by exact products of
                    // polynomials: (m/Q)' is (m' Q - m Q')/Q^2 and (ln q)' is q'/q, so the
                    // columns are polynomials without a quotient to simplify.
                    var monomialGenerators = new List<Entity>();
                    var bounds = new List<int>();
                    for (var i = 0; i < variables.Count; i++)
                    {
                        var index = i;
                        var degree = aboveRead.Keys.Concat(belowRead.Keys).Select(k => k[index]).DefaultIfEmpty(0).Max();
                        bounds.Add(i == 0 ? System.Math.Min(MaximumAnsatzDegree, degree + 1) : System.Math.Min(MaximumTowerDegree, degree + 1));
                    }
                    foreach (var powers in Combinations(bounds))
                    {
                        Entity monomial = Number.Integer.One;
                        for (var i = 0; i < variables.Count; i++)
                            if (powers[i] > 0)
                                monomial = monomial == Number.Integer.One ? MathS.Pow(variables[i], powers[i]) : monomial * MathS.Pow(variables[i], powers[i]);
                        monomialGenerators.Add(monomial);
                    }
                    if (monomialGenerators.Count + logarithmGenerators.Count > 200)
                        continue;
                    Entity towersBelow = Number.Integer.One;
                    foreach (var (_, written, _) in monomials)
                        if (written is Logf(_, var g))
                            towersBelow = towersBelow * g;
                    Entity logArguments = Number.Integer.One;
                    foreach (var argument in logarithmGenerators)
                        logArguments = logArguments * argument;
                    var denominatorPrime = Differentiated(denominator);
                    var generators = new List<Entity>();
                    var columns = new List<Dictionary<int[], Entity>>();
                    var failed = false;
                    foreach (var monomial in monomialGenerators)
                    {
                        generators.Add(monomial / denominator);
                        var numerator = (Differentiated(monomial) * denominator - monomial * denominatorPrime) * below * logArguments * towersBelow;
                        if (!TryReadInAll(Functions.PartialFractions.Bare(numerator.Expand().InnerSimplified), variables, out var column))
                        {
                            failed = true;
                            break;
                        }
                        columns.Add(column);
                    }
                    if (!failed)
                        foreach (var argument in logarithmGenerators)
                        {
                            generators.Add(MathS.Ln(argument));
                            Entity others = Number.Integer.One;
                            foreach (var other in logarithmGenerators)
                                if (other != argument)
                                    others = others * other;
                            var numerator = Differentiated(argument) * MathS.Sqr(denominator) * below * others * towersBelow;
                            if (!TryReadInAll(Functions.PartialFractions.Bare(numerator.Expand().InnerSimplified), variables, out var column))
                            {
                                failed = true;
                                break;
                            }
                            columns.Add(column);
                        }
                    if (failed)
                        continue;
                    var targetNumerator = above * MathS.Sqr(denominator) * logArguments * towersBelow;
                    if (!TryReadInAll(Functions.PartialFractions.Bare(targetNumerator.Expand().InnerSimplified), variables, out var target))
                        continue;
                    var keys = columns.SelectMany(c => c.Keys).Concat(target.Keys).Distinct(new IntArrayComparer()).ToList();
                    var matrix = new Entity[keys.Count][];
                    var rhs = new Entity[keys.Count];
                    for (var row = 0; row < keys.Count; row++)
                    {
                        matrix[row] = new Entity[columns.Count];
                        for (var c = 0; c < columns.Count; c++)
                            matrix[row][c] = columns[c].TryGetValue(keys[row], out var entry) ? entry : Number.Integer.Zero;
                        rhs[row] = target.TryGetValue(keys[row], out var wanted) ? wanted : Number.Integer.Zero;
                    }
                    if (!Functions.PartialFractions.TrySolveLinear(matrix, rhs, out var values) || values is null)
                        continue;
                    Entity answer = Number.Integer.Zero;
                    for (var c = 0; c < values.Length; c++)
                    {
                        var value = values[c].InnerSimplified;
                        if (value.Evaled is Number.Complex { IsZero: true })
                            continue;
                        answer += value * generators[c];
                    }
                    if (answer == Number.Integer.Zero)
                        continue;
                    // Back in the functions, outermost first.
                    for (var i = monomials.Count - 1; i >= 0; i--)
                        answer = answer.Substitute(monomials[i].Indeterminate, monomials[i].Written);
                    answer = Functions.PartialFractions.Bare(answer.InnerSimplified);
                    if (!Functions.PartialFractions.HoldsAtSampledPoints(answer.Differentiate(x), expr, x))
                        continue;
                    return answer;
                }
            return null;

            static IEnumerable<int[]> Combinations(List<int> bounds)
            {
                var current = new int[bounds.Count];
                while (true)
                {
                    yield return (int[])current.Clone();
                    var i = 0;
                    while (i < bounds.Count)
                    {
                        current[i]++;
                        if (current[i] <= bounds[i])
                            break;
                        current[i] = 0;
                        i++;
                    }
                    if (i == bounds.Count)
                        yield break;
                }
            }
        }

        private sealed class IntArrayComparer : IEqualityComparer<int[]>
        {
            public bool Equals(int[]? a, int[]? b) => a is not null && b is not null && a.SequenceEqual(b);
            public int GetHashCode(int[] a)
            {
                var hash = 17;
                foreach (var v in a)
                    hash = hash * 31 + v;
                return hash;
            }
        }

        /// <summary>
        /// <paramref name="expr"/> as a polynomial in all of <paramref name="variables"/> with
        /// coefficients free of them, keyed by the powers in the variables' order.
        /// </summary>
        private static bool TryReadInAll(Entity expr, List<Entity.Variable> variables, out Dictionary<int[], Entity> read)
        {
            read = new Dictionary<int[], Entity>(new IntArrayComparer());
            var result = read;
            bool Read(Entity e, int index, int[] prefix)
            {
                if (index == variables.Count)
                {
                    if (variables.Any(e.ContainsNode))
                        return false;
                    result[prefix] = result.TryGetValue(prefix, out var already) ? already + e : e;
                    return true;
                }
                if (!TreeAnalyzer.TryGetPolynomial(e, variables[index], out var monomials))
                    return false;
                foreach (var pair in monomials)
                {
                    if (pair.Key.Sign < 0 || !pair.Key.CanFitInInt32())
                        return false;
                    var next = (int[])prefix.Clone();
                    next[index] = pair.Key.ToInt32Unchecked();
                    if (!Read(pair.Value, index + 1, next))
                        return false;
                }
                return true;
            }
            if (!Read(expr, 0, new int[variables.Count]))
                return false;
            if (read.Count == 0)
                read[new int[variables.Count]] = Number.Integer.Zero;
            return true;
        }

        /// <summary>
        /// A rational function of <c>x</c> and of <c>sin(x)</c> and <c>cos(x)</c> together, with
        /// an exponential <c>e^(a x)</c> in front or not, closed by the ansatz
        /// <c>F = e^(a x) P/Q</c> with <c>P</c> and <c>Q</c> polynomials in the three.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Timofeev's <c>x^2/(x cos(x) - sin(x))^2</c> is <c>((x sin(x) + cos(x))/(x cos(x) - sin(x)))'</c>
        /// and <c>(2x + sin(2x))/(cos(x) + x sin(x))^2</c> is <c>(2x sin(x)/(x sin(x) + cos(x)))'</c>;
        /// neither had an antiderivative. Nothing reads them: the half-angle substitution
        /// wants no bare <c>x</c>, by parts goes round in a circle, and no subtree is a
        /// substitution. They are the logarithm tower's ansatz with the sine and cosine for
        /// the logarithm. The ring of polynomials in <c>x</c>, <c>sin(x)</c> and <c>cos(x)</c>
        /// is <c>Q[x, s, c]/(s^2 + c^2 - 1)</c>, an integral domain with the basis
        /// <c>x^i c^k</c>, <c>x^i s c^k</c>, and it is closed under the derivative --
        /// <c>(s c^k)' = c^(k+1) - k (1 - c^2) c^(k-1)</c> -- so <c>F' = N/D</c> is the identity
        /// <c>(a P Q + P' Q - P Q') D = N Q^2</c> in that ring, one equation per basis element
        /// and linear in the coefficients of <c>P</c>. Exact, so a solution is an answer and
        /// its absence a decline; the derivative of what comes out is checked against the
        /// integrand at sampled points all the same. <c>Q</c> is tried from the integrand's
        /// written denominator as the other ansätze try theirs, the factors with their powers
        /// lowered by one and then as they are. The half-angle tangent was tried for the
        /// tower first, and every substitution of it leaves powers of <c>1 + t^2</c> above and
        /// below that nothing cancels; the ring needs no substitution.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveByTrigonometricTowerAnsatz(Entity expr, Entity.Variable x)
        {
            if (!Integration.AnsweringTheQuestionAsked)
                return null;
            var sine = MathS.Sin(x);
            var cosine = MathS.Cos(x);
            if (!expr.ContainsNode(sine) && !expr.ContainsNode(cosine)
                && !expr.Nodes.Any(node => node is Tanf(var a) && a == x || node is Cotanf(var b) && b == x
                                          || node is Secantf(var c) && c == x || node is Cosecantf(var d) && d == x))
                return null;
            if (ReadOneExponentialTimesTheRest(expr, x) is not var (exponent, rest))
                return null;
            Entity a = Number.Integer.Zero;
            if (exponent is not null)
            {
                if (!TreeAnalyzer.TryGetPolyLinear(exponent, x, out var slope, out _) || slope.ContainsNode(x))
                    return null;
                a = slope;
            }
            var overTheTwo = rest.Replace(node => node switch
            {
                Tanf(var arg) when arg == x => sine / cosine,
                Cotanf(var arg) when arg == x => cosine / sine,
                Secantf(var arg) when arg == x => 1 / cosine,
                Cosecantf(var arg) when arg == x => 1 / sine,
                _ => node
            });
            // Rational in x and the two, and of the two only with the argument x or a whole
            // multiple of it; and x bare somewhere, or this is the half-angle substitution's
            // and it has declined.
            if (overTheTwo.Nodes.Any(node => node.ContainsNode(x) && node is not (Variable or Sumf or Minusf or Mulf or Divf or Sinf or Cosf)
                                              && !(node is Powf(_, Number.Integer))))
                return null;
            var s = Variable.CreateUnique(expr, "s_tower");
            var c = Variable.CreateUnique(expr, "c_tower");
            // A multiple angle is a polynomial in the two: `sin(2x)` is `2 s c`, and
            // Timofeev's `(2x + sin(2x))/(cos(x) + x sin(x))^2` is `(2x sin(x)/(x sin(x) + cos(x)))'`.
            static bool IsAWholeMultiple(Entity argument, Entity.Variable x, out int k)
            {
                k = 0;
                if (!TreeAnalyzer.TryGetPolyLinear(argument, x, out var slope, out var intercept)
                    || intercept.Evaled is not Number.Complex { IsZero: true }
                    || slope.Evaled is not Number.Integer whole || whole.EInteger.Sign <= 0 || whole.EInteger.CompareTo(EInteger.FromInt32(MaximumTrigonometricTowerDegree)) > 0)
                    return false;
                k = whole.EInteger.ToInt32Checked();
                return true;
            }
            var inTheThree = overTheTwo.Replace(node =>
            {
                if (node is Sinf(var argument) && IsAWholeMultiple(argument, x, out var k))
                    return MultipleAngle(k, s, c).Sine;
                if (node is Cosf(var argument2) && IsAWholeMultiple(argument2, x, out var k2))
                    return MultipleAngle(k2, s, c).Cosine;
                return node;
            });
            if (inTheThree.Nodes.Any(node => node is Sinf(var arg) && arg.ContainsNode(x) || node is Cosf(var arg2) && arg2.ContainsNode(x)))
                return null;
            if (!inTheThree.ContainsNode(x) || !(inTheThree.ContainsNode(s) || inTheThree.ContainsNode(c)))
                return null;

            var (above, below) = Functions.SingleQuotient.Of(inTheThree);
            if (!TrigonometricPolynomial.TryRead(above, x, s, c, out var n) || !TrigonometricPolynomial.TryRead(below, x, s, c, out var d))
                return null;
            if (n.DegreeInX > MaximumTrigonometricTowerDegree || d.DegreeInX > MaximumTrigonometricTowerDegree
                || n.TrigonometricDegree > MaximumTrigonometricTowerDegree || d.TrigonometricDegree > MaximumTrigonometricTowerDegree)
                return null;

            // The denominators tried, from the written factors that hold any of the three.
            var factors = Mulf.LinearChildren(below).Where(f => f.ContainsNode(x) || f.ContainsNode(s) || f.ContainsNode(c)).ToList();
            var candidates = new List<Entity>();
            Entity lowered = Number.Integer.One;
            foreach (var factor in factors)
                if (factor is Powf(var @base, Number.Integer power) && power.EInteger.Sign > 0 && power.EInteger.CanFitInInt32())
                {
                    var one = power.EInteger.ToInt32Unchecked() - 1;
                    if (one > 0)
                        lowered *= one == 1 ? @base : MathS.Pow(@base, one);
                }
            candidates.Add(lowered);
            if (factors.Count > 0)
                candidates.Add(below);

            foreach (var candidate in candidates.Distinct())
            {
                if (!TrigonometricPolynomial.TryRead(candidate, x, s, c, out var q))
                    continue;
                var degreePX = System.Math.Max(q.DegreeInX, n.DegreeInX + 2);
                var degreePT = System.Math.Max(q.TrigonometricDegree, n.TrigonometricDegree + 1);
                if (degreePX > MaximumTrigonometricTowerDegree || degreePT > MaximumTrigonometricTowerDegree + 1)
                    continue;
                var qPrime = q.Derivative();
                var qSquared = q.Times(q);
                var target = n.Times(qSquared);
                var columns = new List<TrigonometricPolynomial>();
                var monomialsOfP = new List<(int X, int C, int S)>();
                for (var i = 0; i <= degreePX; i++)
                    for (var k = 0; k <= degreePT; k++)
                        for (var flag = 0; flag <= 1; flag++)
                        {
                            if (k + flag > degreePT)
                                continue;
                            var m = TrigonometricPolynomial.Monomial(i, k, flag);
                            // (a m Q + m' Q - m Q') D, the coefficient of this unknown.
                            var bracket = m.Derivative().Times(q).Minus(m.Times(qPrime));
                            if (a.Evaled is not Number.Complex { IsZero: true })
                                bracket = bracket.Plus(m.Times(q).Scaled(a));
                            columns.Add(bracket.Times(d));
                            monomialsOfP.Add((i, k, flag));
                        }
                var keys = columns.SelectMany(column => column.Terms.Keys).Concat(target.Terms.Keys).Distinct().OrderBy(key => key).ToList();
                var matrix = new Entity[keys.Count][];
                var rhs = new Entity[keys.Count];
                for (var row = 0; row < keys.Count; row++)
                {
                    matrix[row] = new Entity[columns.Count];
                    for (var column = 0; column < columns.Count; column++)
                        matrix[row][column] = columns[column].Terms.TryGetValue(keys[row], out var entry) ? entry : Number.Integer.Zero;
                    rhs[row] = target.Terms.TryGetValue(keys[row], out var wanted) ? wanted : Number.Integer.Zero;
                }
                if (!Functions.PartialFractions.TrySolveLinear(matrix, rhs, out var values) || values is null)
                    continue;
                Entity numerator = Number.Integer.Zero;
                for (var column = 0; column < values.Length; column++)
                {
                    var value = values[column].InnerSimplified;
                    if (value.Evaled is Number.Complex { IsZero: true })
                        continue;
                    var (i, k, flag) = monomialsOfP[column];
                    numerator += value * TrigonometricPolynomial.Monomial(i, k, flag).ToEntity(x, s, c);
                }
                if (numerator.Evaled is Number.Complex { IsZero: true })
                    continue;
                var quotient = (numerator / candidate).Substitute(s, sine).Substitute(c, cosine);
                var answer = Functions.PartialFractions.Bare(
                    (exponent is null ? quotient : MathS.Pow(MathS.e, exponent) * quotient).InnerSimplified);
                if (!Functions.PartialFractions.HoldsAtSampledPoints(answer.Differentiate(x), expr, x))
                    continue;
                return answer;
            }
            return null;
        }

        /// <summary>The largest degree in x or in the two the trigonometric tower ansatz reads or tries.</summary>
        private const int MaximumTrigonometricTowerDegree = 6;

        /// <summary>
        /// <c>sin(k x)</c> and <c>cos(k x)</c> as polynomials in <paramref name="s"/> and
        /// <paramref name="c"/>, by <c>sin((k + 1)x) = sin(kx) cos(x) + cos(kx) sin(x)</c> and
        /// <c>cos((k + 1)x) = cos(kx) cos(x) - sin(kx) sin(x)</c>.
        /// </summary>
        private static (Entity Sine, Entity Cosine) MultipleAngle(int k, Entity.Variable s, Entity.Variable c)
        {
            Entity sine = s;
            Entity cosine = c;
            for (var i = 1; i < k; i++)
                (sine, cosine) = (sine * c + cosine * s, cosine * c - sine * s);
            return (sine, cosine);
        }

        /// <summary>
        /// A polynomial in <c>x</c>, <c>sin(x)</c> and <c>cos(x)</c> with every <c>sin^2</c>
        /// written as <c>1 - cos^2</c>, so that the basis is <c>x^i cos^k</c> and
        /// <c>x^i sin cos^k</c>: keyed by the degree in x, the degree in the cosine and the
        /// sine's presence, with coefficients free of the three. Two are equal as functions
        /// exactly when their terms are.
        /// </summary>
        private sealed class TrigonometricPolynomial
        {
            internal readonly Dictionary<(int X, int C, int S), Entity> Terms = new();

            internal int DegreeInX => Terms.Count == 0 ? 0 : Terms.Keys.Max(key => key.X);
            internal int TrigonometricDegree => Terms.Count == 0 ? 0 : Terms.Keys.Max(key => key.C + key.S);

            internal static TrigonometricPolynomial Monomial(int i, int k, int flag)
            {
                var monomial = new TrigonometricPolynomial();
                monomial.Terms[(i, k, flag)] = Number.Integer.One;
                return monomial;
            }

            private void Add((int X, int C, int S) key, Entity coefficient)
            {
                var sum = Terms.TryGetValue(key, out var already) ? (already + coefficient).InnerSimplified : coefficient;
                if (sum.Evaled is Number.Complex { IsZero: true })
                    Terms.Remove(key);
                else
                    Terms[key] = sum;
            }

            internal TrigonometricPolynomial Plus(TrigonometricPolynomial other)
            {
                var sum = new TrigonometricPolynomial();
                foreach (var pair in Terms) sum.Add(pair.Key, pair.Value);
                foreach (var pair in other.Terms) sum.Add(pair.Key, pair.Value);
                return sum;
            }

            internal TrigonometricPolynomial Minus(TrigonometricPolynomial other) => Plus(other.Scaled(Number.Integer.MinusOne));

            internal TrigonometricPolynomial Scaled(Entity by)
            {
                var scaled = new TrigonometricPolynomial();
                foreach (var pair in Terms) scaled.Add(pair.Key, (by * pair.Value).InnerSimplified);
                return scaled;
            }

            internal TrigonometricPolynomial Times(TrigonometricPolynomial other)
            {
                var product = new TrigonometricPolynomial();
                foreach (var leftPair in Terms)
                    foreach (var rightPair in other.Terms)
                    {
                        var (left, right) = (leftPair.Key, rightPair.Key);
                        var coefficient = (leftPair.Value * rightPair.Value).InnerSimplified;
                        var i = left.X + right.X;
                        var k = left.C + right.C;
                        if (left.S + right.S == 2)
                        {
                            // sin^2 is 1 - cos^2.
                            product.Add((i, k, 0), coefficient);
                            product.Add((i, k + 2, 0), (-coefficient).InnerSimplified);
                        }
                        else
                            product.Add((i, k, left.S + right.S), coefficient);
                    }
                return product;
            }

            /// <summary>The derivative in x, with <c>(sin cos^k)' = cos^(k+1) - k (1 - cos^2) cos^(k-1)</c>.</summary>
            internal TrigonometricPolynomial Derivative()
            {
                var derivative = new TrigonometricPolynomial();
                foreach (var pair in Terms)
                {
                    var (i, k, flag) = pair.Key;
                    var coefficient = pair.Value;
                    if (i > 0)
                        derivative.Add((i - 1, k, flag), (i * coefficient).InnerSimplified);
                    if (flag == 0)
                    {
                        // (cos^k)' = -k sin cos^(k-1)
                        if (k > 0)
                            derivative.Add((i, k - 1, 1), (-k * coefficient).InnerSimplified);
                    }
                    else
                    {
                        derivative.Add((i, k + 1, 0), coefficient);
                        if (k > 0)
                        {
                            derivative.Add((i, k - 1, 0), (-k * coefficient).InnerSimplified);
                            derivative.Add((i, k + 1, 0), (k * coefficient).InnerSimplified);
                        }
                    }
                }
                return derivative;
            }

            internal Entity ToEntity(Entity.Variable x, Entity.Variable s, Entity.Variable c)
            {
                Entity sum = Number.Integer.Zero;
                foreach (var pair in Terms.OrderBy(pair => pair.Key))
                {
                    var (i, k, flag) = pair.Key;
                    Entity term = pair.Value;
                    if (i > 0) term *= i == 1 ? x : MathS.Pow(x, i);
                    if (flag == 1) term *= s;
                    if (k > 0) term *= k == 1 ? c : MathS.Pow(c, k);
                    sum = sum == Number.Integer.Zero ? term : sum + term;
                }
                return sum;
            }

            /// <summary>
            /// <paramref name="expr"/>, a polynomial in the three as written, read into the
            /// basis; <see langword="false"/> where it is not one.
            /// </summary>
            internal static bool TryRead(Entity expr, Entity.Variable x, Entity.Variable s, Entity.Variable c, out TrigonometricPolynomial read)
            {
                read = new TrigonometricPolynomial();
                if (!TreeAnalyzer.TryGetPolynomial(expr.Expand(), x, out var inX))
                    return false;
                foreach (var pairInX in inX)
                {
                    var powerOfX = pairInX.Key;
                    var coefficientInX = pairInX.Value;
                    if (powerOfX.Sign < 0 || !powerOfX.CanFitInInt32())
                        return false;
                    if (!TreeAnalyzer.TryGetPolynomial(coefficientInX, s, out var inS))
                        return false;
                    foreach (var pairInS in inS)
                    {
                        var powerOfS = pairInS.Key;
                        var coefficientInS = pairInS.Value;
                        if (powerOfS.Sign < 0 || !powerOfS.CanFitInInt32())
                            return false;
                        if (!TreeAnalyzer.TryGetPolynomial(coefficientInS, c, out var inC))
                            return false;
                        foreach (var pair in inC)
                        {
                            var powerOfC = pair.Key;
                            var coefficient = pair.Value;
                            if (powerOfC.Sign < 0 || !powerOfC.CanFitInInt32() || coefficient.ContainsNode(x) || coefficient.ContainsNode(s) || coefficient.ContainsNode(c))
                                return false;
                            // sin^(2m + f) is (1 - cos^2)^m sin^f.
                            var j = powerOfS.ToInt32Unchecked();
                            var term = Monomial(powerOfX.ToInt32Unchecked(), powerOfC.ToInt32Unchecked(), j % 2).Scaled(coefficient);
                            var oneMinusCosineSquared = new TrigonometricPolynomial();
                            oneMinusCosineSquared.Terms[(0, 0, 0)] = Number.Integer.One;
                            oneMinusCosineSquared.Terms[(0, 2, 0)] = Number.Integer.MinusOne;
                            for (var m = 0; m < j / 2; m++)
                                term = term.Times(oneMinusCosineSquared);
                            read = read.Plus(term);
                        }
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// <paramref name="expr"/> as a polynomial in <paramref name="x"/> and <paramref name="L"/>
        /// with coefficients free of both, keyed by the pair of degrees.
        /// </summary>
        private static bool TryReadInBoth(Entity expr, Entity.Variable x, Entity.Variable L, out Dictionary<(int, int), Entity> read)
        {
            read = new Dictionary<(int, int), Entity>();
            if (!TreeAnalyzer.TryGetPolynomial(expr, x, out var inX))
                return false;
            foreach (var pair in inX)
            {
                if (pair.Key.Sign < 0 || !pair.Key.CanFitInInt32())
                    return false;
                if (!TreeAnalyzer.TryGetPolynomial(pair.Value, L, out var inL))
                    return false;
                foreach (var inner in inL)
                {
                    if (inner.Key.Sign < 0 || !inner.Key.CanFitInInt32() || inner.Value.ContainsNode(x) || inner.Value.ContainsNode(L))
                        return false;
                    var key = (pair.Key.ToInt32Unchecked(), inner.Key.ToInt32Unchecked());
                    read[key] = read.TryGetValue(key, out var already) ? already + inner.Value : inner.Value;
                }
            }
            if (read.Count == 0)
                read[(0, 0)] = Number.Integer.Zero;
            return true;
        }

        /// <summary>
        /// <paramref name="term"/> as one exponential of something in <paramref name="x"/> times
        /// everything else, or a <see langword="null"/> exponent where there is not exactly one.
        /// </summary>
        private static (Entity? Exponent, Entity Remainder)? ReadOneExponentialTimesTheRest(Entity term, Entity.Variable x)
        {
            Entity? exponent = null;
            Entity rest = Number.Integer.One;
            foreach (var (factor, underneath) in FactorsOfTheIntegrand(term))
            {
                if (factor is Powf(var @base, var power) && !@base.ContainsNode(x) && power.ContainsNode(x)
                    && power is not Number)
                {
                    if (exponent is not null)
                        return null;
                    var h = @base == MathS.e ? power : power * MathS.Ln(@base);
                    exponent = (underneath ? -h : h).InnerSimplified;
                    continue;
                }
                rest = underneath ? rest / factor : rest * factor;
            }
            return (exponent, rest);
        }

        /// <summary>
        /// The ansatz of <see cref="SolveByExponentialAnsatz"/> for <c>e^h R</c>, or for
        /// <c>R</c> alone when <paramref name="h"/> is <see langword="null"/>.
        /// </summary>
        internal static Entity? IntegrateByAnsatz(Entity? h, Entity rational, Entity.Variable x)
        {
            var (above, below) = Functions.SingleQuotient.Of(rational.InnerSimplified);
            if (!TreeAnalyzer.TryGetPolynomial(above, x, out var numeratorRead)
                || !TreeAnalyzer.TryGetPolynomial(below, x, out var denominatorRead)
                || numeratorRead.Count == 0 || denominatorRead.Count == 0)
                return null;
            foreach (var pair in numeratorRead.Concat(denominatorRead))
                if (pair.Key.Sign < 0 || pair.Value.ContainsNode(x))
                    return null;

            Entity p = 0, q = 1;
            if (h is not null)
            {
                (p, q) = Functions.SingleQuotient.Of(h.InnerSimplified);
                if (!TreeAnalyzer.TryGetPolynomial(p, x, out var pRead) || !TreeAnalyzer.TryGetPolynomial(q, x, out var qRead))
                    return null;
                foreach (var pair in pRead.Concat(qRead))
                    if (pair.Key.Sign < 0 || pair.Value.ContainsNode(x))
                        return null;
            }

            var degreeAbove = (int)numeratorRead.Keys.Max()!.ToInt32Checked();
            var degreeBelow = (int)denominatorRead.Keys.Max()!.ToInt32Checked();
            if (degreeAbove > MaximumAnsatzDegree || degreeBelow > MaximumAnsatzDegree)
                return null;

            foreach (var d in CandidateDenominators(below, x))
            {
                if (IntegrateByAnsatzOver(h, p, q, above, below, d, x, degreeAbove, degreeBelow) is { } answer)
                    return answer;
            }
            return null;
        }

        /// <summary>
        /// Whether <c>sum values_k columns_k = target</c> holds coefficient by coefficient in
        /// rational arithmetic; false where a coefficient or a value is not rational.
        /// </summary>
        private static bool IdentityHoldsExactly(List<Dictionary<EInteger, Entity>> columns, Entity[] values, Dictionary<EInteger, Entity> target)
        {
            var sums = new Dictionary<EInteger, ERational>();
            for (var k = 0; k < columns.Count; k++)
            {
                if (values[k].Evaled is not Number.Rational value)
                    return false;
                if (value.ERational.IsZero)
                    continue;
                foreach (var pair in columns[k])
                {
                    if (pair.Value.Evaled is not Number.Rational coefficient)
                        return false;
                    var term = coefficient.ERational.Multiply(value.ERational);
                    sums[pair.Key] = sums.TryGetValue(pair.Key, out var so) ? so.Add(term).ToLowestTerms() : term.ToLowestTerms();
                }
            }
            foreach (var pair in target)
            {
                if (pair.Value.Evaled is not Number.Rational wanted)
                    return false;
                var have = sums.TryGetValue(pair.Key, out var sum) ? sum : ERational.Zero;
                if (!have.Subtract(wanted.ERational).ToLowestTerms().IsZero)
                    return false;
                sums.Remove(pair.Key);
            }
            return sums.Values.All(sum => sum.IsZero);
        }

        /// <summary>The product of two polynomials given as coefficients by power, each coefficient simplified.</summary>
        private static Dictionary<EInteger, Entity> PolynomialProduct(Dictionary<EInteger, Entity> left, Dictionary<EInteger, Entity> right)
        {
            // Over the rationals in rational arithmetic: sixty columns of degree a hundred
            // and twenty as expressions simplified coefficient by coefficient did not return.
            if (AsRationals(left) is { } leftRationals && AsRationals(right) is { } rightRationals)
            {
                var rationalProduct = new Dictionary<EInteger, ERational>();
                foreach (var l in leftRationals)
                    foreach (var r in rightRationals)
                    {
                        var power = l.Key + r.Key;
                        var term = l.Value.Multiply(r.Value);
                        rationalProduct[power] = rationalProduct.TryGetValue(power, out var so) ? so.Add(term).ToLowestTerms() : term.ToLowestTerms();
                    }
                var written = new Dictionary<EInteger, Entity>();
                foreach (var pair in rationalProduct)
                    if (!pair.Value.IsZero)
                        written[pair.Key] = Number.Rational.Create(pair.Value);
                return written;
            }
            var sums = new Dictionary<EInteger, List<Entity>>();
            foreach (var l in left)
                foreach (var r in right)
                {
                    var power = l.Key + r.Key;
                    if (!sums.TryGetValue(power, out var terms))
                        sums[power] = terms = new List<Entity>();
                    terms.Add(l.Value * r.Value);
                }
            var product = new Dictionary<EInteger, Entity>();
            foreach (var pair in sums)
            {
                Entity sum = pair.Value[0];
                for (var i = 1; i < pair.Value.Count; i++)
                    sum += pair.Value[i];
                var simplified = sum.InnerSimplified;
                if (simplified != Number.Integer.Zero && simplified.Evaled is not Number.Complex { IsZero: true })
                    product[pair.Key] = simplified;
            }
            return product;
        }

        /// <summary>The coefficients as rationals, or null where one is not.</summary>
        private static Dictionary<EInteger, ERational>? AsRationals(Dictionary<EInteger, Entity> poly)
        {
            var rationals = new Dictionary<EInteger, ERational>();
            foreach (var pair in poly)
            {
                if (pair.Value.Evaled is not Number.Rational rational)
                    return null;
                rationals[pair.Key] = rational.ERational;
            }
            return rationals;
        }

        private static Dictionary<EInteger, Entity> PolynomialSum(Dictionary<EInteger, Entity> left, Dictionary<EInteger, Entity> right)
        {
            var sum = new Dictionary<EInteger, Entity>(left);
            foreach (var pair in right)
            {
                var value = sum.TryGetValue(pair.Key, out var so) ? (so + pair.Value).InnerSimplified : pair.Value;
                if (value == Number.Integer.Zero || value.Evaled is Number.Complex { IsZero: true })
                    sum.Remove(pair.Key);
                else
                    sum[pair.Key] = value;
            }
            return sum;
        }

        private static Dictionary<EInteger, Entity> PolynomialDifference(Dictionary<EInteger, Entity> left, Dictionary<EInteger, Entity> right)
        {
            var negated = new Dictionary<EInteger, Entity>();
            foreach (var pair in right)
                negated[pair.Key] = (-pair.Value).InnerSimplified;
            return PolynomialSum(left, negated);
        }

        private static Dictionary<EInteger, Entity> PolynomialDerivative(Dictionary<EInteger, Entity> poly)
        {
            var derivative = new Dictionary<EInteger, Entity>();
            foreach (var pair in poly)
                if (!pair.Key.IsZero)
                    derivative[pair.Key - 1] = (Number.Integer.Create(pair.Key) * pair.Value).InnerSimplified;
            return derivative;
        }

        /// <summary>
        /// The denominators the ansatz tries, in order: the denominator with every written power
        /// lowered by one, and the denominator as it is.
        /// </summary>
        private static IEnumerable<Entity> CandidateDenominators(Entity denominator, Entity.Variable x)
        {
            var variableFactors = Mulf.LinearChildren(denominator).Where(f => f.ContainsNode(x)).ToList();
            // One written power on its own is tried at every level below it: with an exponential
            // in front the antiderivative's denominator need not be one below the integrand's --
            // `e^(1/x)(1 + x)/x^4` is `(-e^(1/x)(1 - x + x^2)/x^2)'`, two below.
            if (variableFactors.Count == 1
                && variableFactors[0] is Powf(var only, Number.Integer onlyPower)
                && onlyPower.EInteger.Sign > 0 && onlyPower.EInteger.CanFitInInt32())
            {
                for (var level = onlyPower.EInteger.ToInt32Unchecked() - 1; level >= 0; level--)
                    yield return level == 0 ? Number.Integer.One : level == 1 ? only : MathS.Pow(only, level);
                yield return denominator;
                yield break;
            }
            Entity lowered = Number.Integer.One;
            foreach (var factor in variableFactors)
                if (factor is Powf(var @base, Number.Integer power) && power.EInteger.Sign > 0)
                {
                    var one = power.EInteger.Subtract(EInteger.One);
                    if (one.IsZero)
                        continue;
                    lowered *= one.Equals(EInteger.One) ? @base : MathS.Pow(@base, Number.Integer.Create(one));
                }
            yield return lowered;
            if (denominator.ContainsNode(x))
                yield return denominator;
        }

        /// <summary>
        /// <paramref name="denominator"/> with each written factor taken into its irreducible
        /// factors over the rationals and equal factors gathered into one power, or
        /// <see langword="null"/> where the written factors already have that structure -- the
        /// same count of distinct factors with the same multiplicities -- so that a
        /// denominator which is already factored keeps its own spelling.
        /// </summary>
        /// <summary>
        /// <paramref name="denominator"/> with each written base that is a polynomial in
        /// <paramref name="x"/> with rational coefficients, of degree at least two and not a
        /// sum of monomials as it stands, written as that sum; null where every base already is.
        /// </summary>
        private static Entity? TryWriteBasesAsPolynomials(Entity denominator, Entity.Variable x)
        {
            var changed = false;
            Entity product = Number.Integer.One;
            foreach (var factor in Mulf.LinearChildren(denominator))
            {
                var (@base, power) = factor is Powf(var b, Number.Integer e) ? (b, (Entity)e) : (factor, Number.Integer.One);
                if (@base.ContainsNode(x) && @base is not Variable && !IsASumOfMonomials(@base, x)
                    && Functions.PolynomialFactoring.TryGetRationalCoefficients(@base, x, 1, 2, 64, out var coefficients))
                {
                    var written = Functions.RationalPolynomial.Create(coefficients).ToEntity(x);
                    if (written != @base)
                    {
                        changed = true;
                        @base = written;
                    }
                }
                product *= power == Number.Integer.One ? @base : MathS.Pow(@base, power);
            }
            return changed ? product : null;
        }

        /// <summary>Whether <paramref name="expr"/> is written as a sum of rational multiples of powers of <paramref name="x"/>.</summary>
        private static bool IsASumOfMonomials(Entity expr, Entity.Variable x)
        {
            foreach (var term in Sumf.LinearChildren(expr))
            {
                var bare = term is Mulf(var l, var r) && !l.ContainsNode(x) ? r : term is Mulf(var l2, var r2) && !r2.ContainsNode(x) ? l2 : term;
                if (bare.ContainsNode(x) && bare != x && bare is not Powf(Variable, Number.Integer))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// <paramref name="denominator"/> with the content of every written sum among its
        /// factors taken out in front of it: <c>(a u + a)(1 - u^2)</c> is <c>a (u + 1)(1 - u^2)</c>,
        /// and <c>-a t^6 - 2a t^5 - a t^4 + a t^2 + 2a t + a</c> is <c>a</c> times the
        /// polynomial over the rationals. Null where no factor has one.
        /// </summary>
        /// <remarks>
        /// The content is the gcd of the coefficients as polynomials in the symbols, and a
        /// factor whose coefficients share one is a constant times a smaller polynomial: with
        /// the constant written inside, <c>(a u + a)</c> was a symbolic linear whose root the
        /// factor <c>1 - u^2</c> beside it shares, which the symbolic split declines, and the
        /// sextic the half-angle makes of <c>tan(x)/(a + a csc(x))</c> was a polynomial with a
        /// symbol in every coefficient, which nothing factors -- where over the rationals it is
        /// <c>(t - 1)(t + 1)^3 (t^2 + 1)</c>. Rubi's <c>a + a csc(x)</c> and the like are what
        /// this is for.
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        private static Entity? WithTheContentOutOfEachSumFactor(Entity denominator, Entity.Variable x)
        {
            var changed = false;
            Entity product = Number.Integer.One;
            foreach (var factor in Mulf.LinearChildren(denominator))
            {
                // Whole powers only: `(c a + d a x)^(1/3)` is `a^(1/3) (c + d x)^(1/3)` for a
                // positive `a` and not for a negative one, and a symbol is neither.
                var (@base, power) = factor is Powf(var b, Number.Integer p) ? (b, p) : (factor, Number.Integer.One);
                if (@base is not (Sumf or Minusf) || !@base.ContainsNode(x) || !@base.Vars.Any(v => v != x)
                    || !TreeAnalyzer.TryGetPolynomial(@base, x, out var read) || read.Count < 2
                    || read.Keys.Any(degree => degree.Sign < 0) || read.Values.Any(coefficient => coefficient.ContainsNode(x)))
                {
                    product *= factor;
                    continue;
                }
                var variables = @base.Vars.OrderBy(v => v.Name, System.StringComparer.Ordinal).ToList();
                if (variables.Count > MultivariatePolynomial.MaxVariables)
                {
                    product *= factor;
                    continue;
                }
                var indices = new Dictionary<Variable, int>();
                for (var i = 0; i < variables.Count; i++)
                    indices[variables[i]] = i;
                var rest = Enumerable.Range(0, variables.Count).Where(i => variables[i] != x).ToList();
                // The lowest power of x every term holds comes out too: `a u^4 + b u^3` is
                // `u^3 (a u + b)`, which is what the sine substitution makes of `cot^3/(a + b csc)`,
                // and neither the refactoring over the rationals nor the symbolic split reads
                // a symbolic polynomial for the monomial it is a multiple of.
                var lowest = read.Keys.Min()!;
                if (MultivariatePolynomial.TryParse(@base, indices) is not { } polynomial
                    || Functions.PolynomialGcd.ContentIn(polynomial, indices[x], rest, 0) is not { } content
                    || content.IsConstant && lowest.IsZero
                    || polynomial.DivideExact(content) is not { } primitive)
                {
                    product *= factor;
                    continue;
                }
                var monomial = lowest.IsZero ? Number.Integer.One : lowest.Equals(EInteger.One) ? x : MathS.Pow(x, Number.Integer.Create(lowest));
                var primitivePart = primitive.ToEntity(variables);
                if (!lowest.IsZero && TreeAnalyzer.PolynomialLongDivision(primitivePart, monomial, genericCase: true, inTermsOf: x) is var (divided, remainder)
                    && (remainder is Divf(var top, _) ? top : remainder).Evaled is Number.Complex { IsZero: true })
                    primitivePart = divided.InnerSimplified;
                else if (!lowest.IsZero)
                {
                    product *= factor;
                    continue;
                }
                Entity constantPart = content.IsConstant ? monomial : lowest.IsZero ? content.ToEntity(variables) : content.ToEntity(variables) * monomial;
                product *= power == Number.Integer.One ? constantPart * primitivePart : MathS.Pow(constantPart, power) * MathS.Pow(primitivePart, power);
                changed = true;
            }
            return changed ? product : null;
        }

        private static Entity? TryWriteInIrreducibleFactors(Entity denominator, Entity.Variable x)
        {
            // Only where the written bases, each taken once, share a factor among them or
            // repeat one inside -- a product that is not squarefree -- can the spelling be
            // hiding what the rules below want written; written coprime squarefree bases to
            // whatever powers are the Hermite reduction's and the coprime split's as they
            // stand. One gcd decides that, where factoring every rational denominator cost
            // `(4x^5 - 1)/(x^5 + x + 1)^2` four times its answer.
            if (!HasARepeatedFactor(SquarefreePartAsWritten(denominator, x), x))
                return null;
            var writtenPowers = new List<int>();
            var gathered = new Dictionary<Entity, int>();
            Entity constant = Number.Integer.One;
            foreach (var factor in Mulf.LinearChildren(denominator))
            {
                if (!factor.ContainsNode(x))
                {
                    constant *= factor;
                    continue;
                }
                var (@base, power) = factor is Powf(var b, Number.Integer e) && e.EInteger.Sign > 0 && e.EInteger.CanFitInInt32()
                    ? (b, e.EInteger.ToInt32Unchecked())
                    : (factor, 1);
                writtenPowers.Add(power);
                if (Functions.PolynomialFactorization.FactorComplete(@base, x) is not { } factorization)
                {
                    // Not a polynomial with rational coefficients: kept as written.
                    gathered[@base] = gathered.TryGetValue(@base, out var so) ? so + power : power;
                    continue;
                }
                if (factorization.Constant.CompareTo(ERational.One) != 0)
                    constant *= MathS.Pow(Number.Rational.Create(factorization.Constant), power);
                foreach (var part in factorization.Parts)
                {
                    var piece = part.Factor.ToEntity(x);
                    gathered[piece] = gathered.TryGetValue(piece, out var sofar) ? sofar + part.Multiplicity * power : part.Multiplicity * power;
                }
            }
            if (gathered.Count == 0)
                return null;
            var same = gathered.Count == writtenPowers.Count
                && gathered.Values.OrderBy(v => v).SequenceEqual(writtenPowers.OrderBy(v => v));
            if (same)
                return null;
            Entity product = constant.InnerSimplified;
            foreach (var pair in gathered)
            {
                Entity factor = pair.Value == 1 ? pair.Key : MathS.Pow(pair.Key, pair.Value);
                product = product == Number.Integer.One ? factor : product * factor;
            }
            return product;
        }

        /// <summary>
        /// Whether the polynomial <paramref name="expr"/> in <paramref name="x"/>, with rational
        /// coefficients, shares a factor of positive degree with its derivative.
        /// <see langword="false"/> where it is not such a polynomial.
        /// </summary>
        private static bool HasARepeatedFactor(Entity expr, Entity.Variable x)
        {
            if (!Functions.PolynomialFactoring.TryGetRationalCoefficients(
                    expr, x, leastTerms: 2, leastDegree: 2, IntegerPolynomial.MaxDegree, out var rational))
                return false;
            var denominator = EInteger.One;
            foreach (var coefficient in rational)
                denominator = denominator.Multiply(coefficient.Denominator).Divide(denominator.Gcd(coefficient.Denominator));
            var whole = new EInteger[rational.Length];
            for (var i = 0; i < whole.Length; i++)
                whole[i] = rational[i].Numerator.Multiply(denominator.Divide(rational[i].Denominator));
            var poly = IntegerPolynomial.Create(whole);
            return IntegerPolynomial.Gcd(poly, poly.Derivative()).Degree > 0;
        }

        /// <summary>
        /// The product of the distinct written factors of <paramref name="denominator"/>, each
        /// to the first power: the denominator of the logarithmic part of a Hermite reduction.
        /// </summary>
        private static Entity SquarefreePartAsWritten(Entity denominator, Entity.Variable x)
        {
            Entity product = Number.Integer.One;
            foreach (var factor in Mulf.LinearChildren(denominator))
            {
                if (!factor.ContainsNode(x))
                    continue;
                var @base = factor is Powf(var b, Number.Integer power) && power.EInteger.Sign > 0 ? b : factor;
                product = product == Number.Integer.One ? @base : product * @base;
            }
            return product;
        }

        private static Entity? IntegrateByAnsatzOver(
            Entity? h, Entity p, Entity q, Entity above, Entity below, Entity d, Entity.Variable x,
            int degreeAbove, int degreeBelow)
        {
            var degreeD = TreeAnalyzer.TryGetPolynomial(d, x, out var dRead) && dRead.Count > 0
                ? (int)dRead.Keys.Max()!.ToInt32Checked() : 0;
            var degreeQ = TreeAnalyzer.TryGetPolynomial(q, x, out var qRead) && qRead.Count > 0
                ? (int)qRead.Keys.Max()!.ToInt32Checked() : 0;
            // Hermite's bound is `deg N < deg D`; with an exponential in front the numerator can
            // run past it by as much as the integrand's own excess and the exponent's
            // denominator -- `e^(x^2)(1 + 2x^2)` has `N = x` over `D = 1`. Two to spare either
            // way, since an unknown the system does not need is set to zero and costs nothing.
            var degreeN = System.Math.Max(degreeD - 1,
                degreeD + System.Math.Max(degreeAbove - degreeBelow, 0) + degreeQ + 2);
            if (degreeN > MaximumAnsatzDegree)
                return null;

            // With no exponential this is the Hermite reduction in full: `R = (N/D)' + M/D_1`,
            // with `D_1` the product of the distinct factors and `M` a second unknown
            // polynomial, so that a logarithmic part beside the rational one does not defeat
            // the ansatz -- `(1 + x^2 + x^4)/((1 + x^2)(4 + x^2)^2)` has both. What is left,
            // `M/D_1`, is a proper fraction over a squarefree denominator, which the splits
            // answer. With an exponential there is no logarithmic part to look for.
            Entity? squarefree = null;
            var degreeM = -1;
            if (h is null)
            {
                squarefree = SquarefreePartAsWritten(below, x);
                var degreeSquarefree = TreeAnalyzer.TryGetPolynomial(squarefree, x, out var sRead) && sRead.Count > 0
                    ? (int)sRead.Keys.Max()!.ToInt32Checked() : 0;
                if (degreeSquarefree > 0 && squarefree != below)
                    degreeM = degreeSquarefree - 1;
                else
                    squarefree = null;
            }

            // The identity, linear in the unknown coefficients, is built **one column at a
            // time** as a numeric polynomial rather than once with the unknowns in it: with
            // fourteen unknowns over a degree-eleven denominator the single symbolic expansion
            // did not return, and the same fourteen numeric ones take a moment. The identity is
            //
            //     [(N' D - N D') q^2 + (p' q - p q') N D] D_R  =  N_R D^2 q^2            (exponential)
            //     (N' D - N D') D_1 D_R + M D^2 D_R           =  N_R D^2 D_1            (Hermite)
            //
            // so the column for the k-th coefficient of N is the left side with N = x^k, the
            // column for the j-th of M is x^j D^2 D_R, and the right side is the constant.
            // In coefficient arithmetic rather than by expanding the products as expressions:
            // the expander estimates a product's terms before it collects them and declines
            // past two thousand, and `(u^4 - 7u^2 + 14)^10` beside its own powers is past that
            // long before the twenty-odd terms it collects to -- Welz's
            // `1/((3 - 2x)^(11/2) (1 + x + 2x^2)^5)` under `u = sqrt(3 - 2x)`.
            if (!TreeAnalyzer.TryGetPolynomial(d, x, out var dPoly) || !TreeAnalyzer.TryGetPolynomial(q, x, out var qPoly)
                || !TreeAnalyzer.TryGetPolynomial(p, x, out var pPoly) || !TreeAnalyzer.TryGetPolynomial(below, x, out var belowPoly)
                || !TreeAnalyzer.TryGetPolynomial(above, x, out var abovePoly))
                return null;
            Dictionary<EInteger, Entity>? squarefreePoly = null;
            if (squarefree is not null && !TreeAnalyzer.TryGetPolynomial(squarefree, x, out squarefreePoly))
                return null;
            var dPrimePoly = PolynomialDerivative(dPoly);
            var qSquared = PolynomialProduct(qPoly, qPoly);
            var dSquared = PolynomialProduct(dPoly, dPoly);
            var scale = squarefreePoly is null ? belowPoly : PolynomialProduct(squarefreePoly, belowPoly);
            var wronskian = h is null ? null : PolynomialDifference(PolynomialProduct(PolynomialDerivative(pPoly), qPoly), PolynomialProduct(pPoly, PolynomialDerivative(qPoly)));
            var columns = new List<Dictionary<EInteger, Entity>>();
            for (var k = 0; k <= degreeN; k++)
            {
                var xk = new Dictionary<EInteger, Entity> { [k] = Number.Integer.One };
                var xkPrime = k == 0 ? new Dictionary<EInteger, Entity>() : new Dictionary<EInteger, Entity> { [k - 1] = Number.Integer.Create(k) };
                var term = PolynomialProduct(PolynomialDifference(PolynomialProduct(xkPrime, dPoly), PolynomialProduct(xk, dPrimePoly)), qSquared);
                if (wronskian is not null)
                    term = PolynomialSum(term, PolynomialProduct(PolynomialProduct(wronskian, xk), dPoly));
                columns.Add(PolynomialProduct(term, scale));
            }
            var dSquaredBelow = PolynomialProduct(dSquared, belowPoly);
            for (var j = 0; j <= degreeM; j++)
                columns.Add(PolynomialProduct(new Dictionary<EInteger, Entity> { [j] = Number.Integer.One }, dSquaredBelow));
            var targetRead = squarefreePoly is null
                ? PolynomialProduct(PolynomialProduct(abovePoly, dSquared), qSquared)
                : PolynomialProduct(PolynomialProduct(abovePoly, dSquared), squarefreePoly);

            var powers = columns.SelectMany(c => c.Keys).Concat(targetRead.Keys).Distinct().ToList();
            var width = columns.Count;
            var matrix = new Entity[powers.Count][];
            var rhs = new Entity[powers.Count];
            for (var row = 0; row < powers.Count; row++)
            {
                matrix[row] = new Entity[width];
                for (var column = 0; column < width; column++)
                    matrix[row][column] = columns[column].TryGetValue(powers[row], out var entry) ? entry.InnerSimplified : Number.Integer.Zero;
                rhs[row] = targetRead.TryGetValue(powers[row], out var wanted) ? wanted.InnerSimplified : Number.Integer.Zero;
            }
            if (!Functions.PartialFractions.TrySolveLinear(matrix, rhs, out var values))
                return null;

            Entity solvedN = 0;
            for (var k = 0; k <= degreeN; k++)
                if (values[k] != Number.Integer.Zero)
                    solvedN += values[k] * (k == 0 ? Number.Integer.One : k == 1 ? x : MathS.Pow(x, k));
            Entity solvedM = 0;
            for (var j = 0; j <= degreeM; j++)
                if (values[degreeN + 1 + j] != Number.Integer.Zero)
                    solvedM += values[degreeN + 1 + j] * (j == 0 ? Number.Integer.One : j == 1 ? x : MathS.Pow(x, j));
            solvedN = Functions.PartialFractions.Bare(solvedN);
            solvedM = Functions.PartialFractions.Bare(solvedM);

            Entity exponential = h is null ? Number.Integer.One : MathS.Pow(MathS.e, h);
            var rationalPart = exponential * solvedN / d;
            var integrand = exponential * above / below;
            // Over the rationals the identity is checked coefficient by coefficient, exactly,
            // which is what the solve established; the sampled check is for symbols, and on a
            // rational part of degree fifty-eight it was four seconds of differentiating.
            var checkedExactly = IdentityHoldsExactly(columns, values, targetRead);
            if (squarefree is null)
            {
                if (!checkedExactly && !Functions.PartialFractions.HoldsAtSampledPoints(rationalPart.Differentiate(x), integrand, x))
                    return null;
                return rationalPart.InnerSimplified;
            }

            var logarithmicPart = solvedM / squarefree;
            if (!checkedExactly && !Functions.PartialFractions.HoldsAtSampledPoints(rationalPart.Differentiate(x) + logarithmicPart, integrand, x))
                return null;
            if (solvedM == Number.Integer.Zero || solvedM.Evaled is Number.Complex { IsZero: true })
                return rationalPart.InnerSimplified;
            // The logarithmic part is a proper rational function over a squarefree denominator,
            // and the splits are what answer that; handing it to the whole integrator instead
            // sent one of them through every substitution and by-parts attempt there is, thirty
            // seconds to decline what the splits decline in a few milliseconds.
            // The rest is checked as the ansatz was: a logarithmic part whose coefficients
            // came out of the elimination as quotients of forty-fifth-degree polynomials in
            // the symbols was integrated wrongly further down, and the sum was a wrong answer
            // that every part of the way had passed.
            // https://github.com/asc-community/AngouriMath/issues/1369
            if (SolveByPartialFractions(logarithmicPart.InnerSimplified, x, integrateByParts: false) is not { } rest
                || !Functions.PartialFractions.DerivativeHoldsAtSampledPoints(rest, logarithmicPart, x))
                return null;
            return (rationalPart + rest).InnerSimplified;
        }

        /// <summary>
        /// The largest degree, of the numerator, the denominator or the ansatz polynomial, that
        /// <see cref="SolveByExponentialAnsatz"/> and the Hermite reduction take on. Twelve
        /// while the system was eliminated as expressions; over the rationals it is lifted
        /// p-adically, and sixty-three unknowns -- Welz's <c>1/((3 - 2x)^(21/2) (1 + x + 2x^2)^10)</c>
        /// under <c>u = sqrt(3 - 2x)</c> -- are fifty milliseconds.
        /// </summary>
        private const int MaximumAnsatzDegree = 64;

        /// <summary>
        /// A power of x times a whole power of its logarithm, <c>x^p ln(x)^n</c>, by the
        /// closed reduction
        /// <c>x^(p + 1) sum_(k = 0..n) (-1)^k n!/(n - k)! ln(x)^(n - k)/(p + 1)^(k + 1)</c>, for
        /// any <c>p</c> but <c>-1</c>, where it is <c>ln(x)^(n + 1)/(n + 1)</c>. And the same
        /// for <c>F = A + B ln(c x^r)</c> in the logarithm's place, whose derivative is
        /// <c>s/x</c> for <c>s = B r</c>: each step of parts brings a factor <c>s</c>, so the
        /// sum is over <c>(-s)^k n!/(n - k)! F^(n - k)/(p + 1)^(k + 1)</c>, and at <c>p = -1</c>
        /// it is <c>F^(n + 1)/((n + 1) s)</c>. Rubi's <c>(e x)^q (a + b ln(c x^n))^3</c>, and
        /// <c>t^(-2 - m) (A + B ln(e t^n))^2</c> in the variable of the quotient substitution.
        /// </summary>
        /// <remarks>
        /// By parts <c>n</c> times, written out: each step takes one from the power of the
        /// logarithm and divides by <c>p + 1</c>. Timofeev's <c>x^m ln(x)^2</c> and
        /// <c>ln(x)^2/x^(5/2)</c> had no antiderivative -- the first step of by parts was
        /// taken and the second, on <c>x^m ln(x)</c> with the symbol still in the exponent,
        /// was not -- where <c>x^m ln(x)</c> and <c>x^2 ln(x)^2</c> were answered. Closed, exact,
        /// and volunteered at any depth.
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveAPowerTimesAPowerOfTheLogarithm(Entity expr, Entity.Variable x)
        {
            Entity? p = null;
            var n = 0;
            Entity constant = Number.Integer.One;
            var logarithm = MathS.Ln(x);
            Entity? affine = null;   // F, where the factor is a power of A + B ln(c x^r) and not of ln x
            foreach (var (factor, underneath) in FactorsOfTheIntegrand(expr))
            {
                if (!factor.ContainsNode(x))
                {
                    constant = underneath ? constant / factor : constant * factor;
                    continue;
                }
                if (factor == x && p is null)
                    p = underneath ? Number.Integer.MinusOne : Number.Integer.One;
                else if (factor is Powf(var @base, var exponent) && @base == x && !exponent.ContainsNode(x) && p is null)
                    p = underneath ? -exponent : exponent;
                else if (factor is Powf(Mulf(var scale, var scaled), var scaledExponent) && scaled == x && !scale.ContainsNode(x) && !scaledExponent.ContainsNode(x) && p is null)
                {
                    // `(d x)^m` as `d^m x^m`, the generic case, as the table's power rule takes it.
                    p = underneath ? -scaledExponent : scaledExponent;
                    var scalePower = MathS.Pow(scale, scaledExponent);
                    constant = underneath ? constant / scalePower : constant * scalePower;
                }
                else if (factor == logarithm && n == 0 && !underneath)
                    n = 1;
                else if (factor is Powf(var l, Number.Integer whole) && l == logarithm && whole.EInteger.Sign > 0 && whole.EInteger.CanFitInInt32() && n == 0 && !underneath)
                    n = whole.EInteger.ToInt32Unchecked();
                else if (n == 0 && !underneath && factor is not Powf && IsAffineInALogarithmOfAPowerOfX(factor, x))
                    (affine, n) = (factor, 1);
                else if (n == 0 && !underneath && factor is Powf(var f, Number.Integer wholeOfF) && wholeOfF.EInteger.Sign > 0 && wholeOfF.EInteger.CanFitInInt32() && IsAffineInALogarithmOfAPowerOfX(f, x))
                    (affine, n) = (f, wholeOfF.EInteger.ToInt32Unchecked());
                else
                    return null;
            }
            if (n == 0 || n > 12 || p is null && affine is null)
                return null;
            // A power of F alone is the case p = 0.
            p = (p ?? Number.Integer.Zero).InnerSimplified;
            if (p.Evaled is Number.Complex and not Number.Real)
                return null;
            // The factor F and s with F' = s/x: ln x itself, or A + B ln(c x^r) with s = B r.
            Entity slopeOfF = Number.Integer.One;
            if (affine is { })
            {
                logarithm = affine;
                slopeOfF = Functions.PartialFractions.Bare((affine.Differentiate(x) * x).Simplify());
                if (slopeOfF.ContainsNode(x) || slopeOfF.Evaled is Number.Complex { IsZero: true })
                    return null;
            }
            if ((p + 1).InnerSimplified.Evaled is Number.Complex { IsZero: true })
                return (constant * MathS.Pow(logarithm, n + 1) / ((n + 1) * slopeOfF)).InnerSimplified;
            // Written out term by term, each with its coefficient folded: the derivative of
            // `x^3 ln(x)/3 - x^3/9` cancels symbolically against `x^2 ln(x)`, where a nested
            // `x^3 (ln(x)/3 - 1/9)` left a residual the simplifier did not close.
            var pPlusOne = (p + 1).InnerSimplified;
            var powerOfX = MathS.Pow(x, pPlusOne);
            Entity sum = Number.Integer.Zero;
            var fallingFactorial = EInteger.One;   // n!/(n - k)!
            for (var k = 0; k <= n; k++)
            {
                if (k > 0)
                    fallingFactorial = fallingFactorial.Multiply(EInteger.FromInt32(n - k + 1));
                var sign = k % 2 == 0 ? Number.Integer.One : Number.Integer.MinusOne;
                var slopePower = k == 0 || slopeOfF == Number.Integer.One ? Number.Integer.One : MathS.Pow(slopeOfF, k);
                var coefficient = (constant * sign * Number.Integer.Create(fallingFactorial) * slopePower / MathS.Pow(pPlusOne, k + 1)).InnerSimplified;
                Entity term = n - k == 0 ? coefficient * powerOfX : coefficient * powerOfX * (n - k == 1 ? logarithm : MathS.Pow(logarithm, n - k));
                sum = sum == Number.Integer.Zero ? term : sum + term;
            }
            return sum.InnerSimplified;
        }

        /// <summary>
        /// Whether <paramref name="expr"/> is <c>A + B ln(c x^r)</c> with <c>A</c>, <c>B</c>,
        /// <c>c</c> and <c>r</c> free of <paramref name="x"/> and the logarithm present: one
        /// logarithm of <paramref name="x"/> in it, the expression linear in that logarithm,
        /// and the logarithm's argument a constant times a power of <paramref name="x"/>.
        /// </summary>
        private static bool IsAffineInALogarithmOfAPowerOfX(Entity expr, Entity.Variable x)
        {
            Entity? logarithm = null;
            foreach (var node in expr.Nodes)
                if (node is Logf(var @base, var argument) && @base == MathS.e && argument.ContainsNode(x))
                {
                    if (logarithm is not null && logarithm != node)
                        return false;
                    logarithm = node;
                }
            if (logarithm is not Logf(_, var antilogarithm))
                return false;
            // The argument c x^r: its logarithmic derivative is r/x.
            var rate = Functions.PartialFractions.Bare((antilogarithm.Differentiate(x) * x / antilogarithm).Simplify());
            if (rate.ContainsNode(x) || rate.Nodes.Any(node => node == MathS.NaN))
                return false;
            var placeholder = Variable.CreateUnique(expr, "u_log");
            var inThePlaceholder = expr.Replace(node => node == logarithm ? placeholder : node);
            if (inThePlaceholder.ContainsNode(x))
                return false;
            return TreeAnalyzer.TryGetPolyLinear(inThePlaceholder, placeholder, out var slope, out _) && slope is { } && !TreeAnalyzer.IsZero(slope);
        }

        /// <summary>
        /// The same reduction with a power of x below: <c>P Q^(m/2)/x^n</c> is
        /// <c>R Q^(k + 1/2)/x^(n - 1) + K_1/sqrt(Q) + K_2/(x sqrt(Q))</c>, the two remainders the
        /// table's and the linear-beside-the-root rule's.
        /// </summary>
        /// <remarks>
        /// With <c>s = k + 1/2</c> the derivative of <c>R Q^s/x^(n - 1)</c> is
        /// <c>Q^(s - 1) x^(-n) ((R' x - (n - 1) R) Q + s R Q' x)</c>, and the two remainders
        /// over the same <c>Q^(s - 1) x^(-n)</c> are <c>K_1 Q^(-k) x^n</c> and
        /// <c>K_2 Q^(-k) x^(n - 1)</c>, polynomials both; the identity is one equation per
        /// power of x, linear in the coefficients of <c>R</c> and in the two constants.
        /// Stewart's <c>sqrt(x^2 - a^2)/x^4</c> and <c>sqrt(a^2 - x^2)/x^2</c> had no
        /// antiderivative, with the symbol in the radicand; the second is
        /// <c>-sqrt(a^2 - x^2)/x - arcsin(x/a)</c>.
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveAPolynomialOverAPowerOfXTimesAnOddHalfPowerOfAQuadratic(Entity expr, Entity.Variable x)
        {
            Entity? radicand = null;
            var m = 0;
            var n = 0;
            Entity polynomial = Number.Integer.One;
            Entity constant = Number.Integer.One;
            foreach (var (factor, underneath) in FactorsOfTheIntegrand(expr))
            {
                if (!factor.ContainsNode(x))
                {
                    constant = underneath ? constant / factor : constant * factor;
                    continue;
                }
                if (factor is Powf(var @base, Number.Rational exponent) && exponent is not Number.Integer
                    && exponent.ERational.Denominator.Equals(EInteger.FromInt32(2)) && exponent.ERational.Numerator.CanFitInInt32()
                    && radicand is null)
                {
                    radicand = @base;
                    m = exponent.ERational.Numerator.ToInt32Unchecked() * (underneath ? -1 : 1);
                    continue;
                }
                if (underneath)
                {
                    if (factor == x && n == 0)
                        n = 1;
                    else if (factor is Powf(var xOf, Number.Integer whole) && xOf == x && whole.EInteger.Sign > 0 && whole.EInteger.CanFitInInt32() && n == 0)
                        n = whole.EInteger.ToInt32Unchecked();
                    else
                        return null;
                    continue;
                }
                polynomial = polynomial * factor;
            }
            if (n == 0 || n > 8 || radicand is null || !TreeAnalyzer.TryGetPolyQuadratic(radicand, x, out var a, out var b, out var c)
                || TreeAnalyzer.IsZero(a) || TreeAnalyzer.IsZero(c)
                || !TreeAnalyzer.TryGetPolynomial(polynomial.Expand(), x, out var pRead) || pRead.Keys.Any(k => k.Sign < 0 || !k.CanFitInInt32())
                || pRead.Values.Any(coefficient => coefficient.ContainsNode(x)))
                return null;
            var degreeOfP = pRead.Count == 0 ? 0 : pRead.Keys.Max()!.ToInt32Unchecked();
            if (degreeOfP >= n || degreeOfP > 8 || m > 9 || m < -9)
                return null;   // a proper quotient by the power; anything else is the rule before this one's after a division
            foreach (var coefficient in new[] { a, b, c, constant }.Concat(pRead.Values))
                if (coefficient.Evaled is Number.Complex and not Number.Real)
                    return null;

            var q = new Dictionary<EInteger, Entity> { [EInteger.Zero] = c, [EInteger.One] = b, [EInteger.FromInt32(2)] = a };
            var qPrime = new Dictionary<EInteger, Entity> { [EInteger.Zero] = b, [EInteger.One] = (2 * a).InnerSimplified };
            Dictionary<EInteger, Entity> PowerOfQ(int power)
            {
                var result = new Dictionary<EInteger, Entity> { [EInteger.Zero] = Number.Integer.One };
                for (var i = 0; i < power; i++)
                    result = PolynomialProduct(result, q);
                return result;
            }
            static Dictionary<EInteger, Entity> Monomial(int power) => new() { [EInteger.FromInt32(power)] = Number.Integer.One };
            static Dictionary<EInteger, Entity> Scaled(Dictionary<EInteger, Entity> poly, Entity by)
            {
                var scaled = new Dictionary<EInteger, Entity>();
                foreach (var pair in poly)
                    scaled[pair.Key] = (by * pair.Value).InnerSimplified;
                return scaled;
            }
            var positive = m >= -1;
            var jPrime = positive ? (m + 1) / 2 : 0;
            var k = positive ? 0 : (m + 1) / 2;   // s = k + 1/2, and m = 2k - 1 for a negative m
            var sValue = Number.Rational.Create(2 * k + 1, 2);
            var target = positive ? PolynomialProduct(pRead, PowerOfQ(jPrime)) : pRead;
            var degreeOfTarget = target.Count == 0 ? 0 : target.Keys.Max()!.ToInt32Unchecked();
            var degreeOfR = System.Math.Max(degreeOfTarget, 2 * (-k) + n) + 1;
            var columns = new List<Dictionary<EInteger, Entity>>();
            for (var i = 0; i <= degreeOfR; i++)
            {
                // ((i - n + 1) x^i) Q + s x^(i + 1) Q'
                var column = PolynomialSum(
                    Scaled(PolynomialProduct(Monomial(i), q), Number.Integer.Create(i - n + 1)),
                    Scaled(PolynomialProduct(Monomial(i + 1), qPrime), sValue));
                columns.Add(column);
            }
            var qToMinusK = PowerOfQ(-k);
            columns.Add(PolynomialProduct(qToMinusK, Monomial(n)));
            columns.Add(PolynomialProduct(qToMinusK, Monomial(n - 1)));
            var rows = columns.SelectMany(column => column.Keys).Concat(target.Keys).Max()!.ToInt32Unchecked() + 1;
            var matrix = new Entity[rows][];
            var rhs = new Entity[rows];
            for (var row = 0; row < rows; row++)
            {
                var power = EInteger.FromInt32(row);
                matrix[row] = new Entity[columns.Count];
                for (var column = 0; column < columns.Count; column++)
                    matrix[row][column] = columns[column].TryGetValue(power, out var entry) ? entry : Number.Integer.Zero;
                rhs[row] = target.TryGetValue(power, out var wanted) ? wanted : Number.Integer.Zero;
            }
            if (!Functions.PartialFractions.TrySolveLinear(matrix, rhs, out var values) || values is null)
                return null;
            Entity r = Number.Integer.Zero;
            for (var i = 0; i <= degreeOfR; i++)
            {
                var value = values[i].InnerSimplified;
                if (value.Evaled is Number.Complex { IsZero: true })
                    continue;
                r = r + value * (i == 0 ? Number.Integer.One : i == 1 ? x : MathS.Pow(x, i));
            }
            var k1 = values[degreeOfR + 1].InnerSimplified;
            var k2 = values[degreeOfR + 2].InnerSimplified;
            var root = MathS.Pow(radicand, Number.Rational.Create(1, 2));
            Entity answer = r == Number.Integer.Zero
                ? Number.Integer.Zero
                : r * MathS.Pow(radicand, sValue) / (n == 1 ? Number.Integer.One : n == 2 ? x : MathS.Pow(x, n - 1));
            if (k1.Evaled is not Number.Complex { IsZero: true })
            {
                if (IntegralPatterns.TryStandardIntegrals(1 / root, x) is not { } table || table is Piecewise && a.Evaled is not Number)
                    return null;
                answer = answer + k1 * table;
            }
            if (k2.Evaled is not Number.Complex { IsZero: true })
            {
                if (SolveALinearBesideTheRootOfAQuadratic(1 / (x * root), x) is not { } besideTheRoot || besideTheRoot is Piecewise && a.Evaled is not Number)
                    return null;
                answer = answer + k2 * besideTheRoot;
            }
            answer = (constant * answer).InnerSimplified;
            if (answer.Nodes.Any(node => node is Number.Complex { IsNaN: true })
                || !Functions.PartialFractions.HoldsAtSampledPoints(answer.Differentiate(x), expr, x))
                return null;
            return answer;
        }

        /// <summary>
        /// An exponential of a linear times a polynomial times an odd half power of a
        /// quadratic, <c>e^(a x + b) P Q^(m/2)</c>, closed by the ansatz <c>F = e^(a x + b) R Q^(k + 1/2)</c>
        /// with <c>m = 2k - 1</c>: <c>F' = e^(a x + b) Q^(k - 1/2) (a R Q + R' Q + (k + 1/2) R Q')</c>,
        /// and <c>a R Q + R' Q + (k + 1/2) R Q' = P</c> is a linear system in the coefficients
        /// of <c>R</c>, exact.
        /// </summary>
        /// <remarks>
        /// Timofeev's <c>e^x (1 - x - x^2)/sqrt(1 - x^2)</c> is <c>(e^x sqrt(1 - x^2))'</c> and
        /// had no antiderivative: by parts goes round in a circle and nothing substitutes.
        /// Liouville's theorem says the elementary antiderivative of <c>e^(a x)</c> times an
        /// algebraic function, where there is one, is <c>e^(a x)</c> times an algebraic function
        /// of the same field, which is what is looked for; there is no remainder term, unlike
        /// the reduction without the exponential, so an empty solution is a decline.
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveAnExponentialTimesAnOddHalfPowerOfAQuadratic(Entity expr, Entity.Variable x)
        {
            if (ReadOneExponentialTimesTheRest(expr, x) is not var (exponent, rest) || exponent is null
                || !TreeAnalyzer.TryGetPolyLinear(exponent, x, out var a, out _) || a.ContainsNode(x) || TreeAnalyzer.IsZero(a))
                return null;
            Entity? radicand = null;
            var m = 0;
            Entity polynomial = Number.Integer.One;
            Entity constant = Number.Integer.One;
            foreach (var (factor, underneath) in FactorsOfTheIntegrand(rest))
            {
                if (!factor.ContainsNode(x))
                {
                    constant = underneath ? constant / factor : constant * factor;
                    continue;
                }
                if (factor is Powf(var @base, Number.Rational power) && power is not Number.Integer
                    && power.ERational.Denominator.Equals(EInteger.FromInt32(2)) && power.ERational.Numerator.CanFitInInt32()
                    && radicand is null)
                {
                    radicand = @base;
                    m = power.ERational.Numerator.ToInt32Unchecked() * (underneath ? -1 : 1);
                    continue;
                }
                if (underneath)
                    return null;
                polynomial = polynomial * factor;
            }
            if (radicand is null || !TreeAnalyzer.TryGetPolyQuadratic(radicand, x, out var qa, out var qb, out var qc)
                || TreeAnalyzer.IsZero(qa)
                || !TreeAnalyzer.TryGetPolynomial(polynomial.Expand(), x, out var pRead) || pRead.Keys.Any(k => k.Sign < 0 || !k.CanFitInInt32())
                || pRead.Values.Any(coefficient => coefficient.ContainsNode(x)))
                return null;
            var degreeOfP = pRead.Count == 0 ? 0 : pRead.Keys.Max()!.ToInt32Unchecked();
            if (degreeOfP > 8 || m > 9 || m < -9)
                return null;
            var k = (m + 1) / 2;   // m = 2k - 1, exact for odd m of either sign
            var q = new Dictionary<EInteger, Entity> { [EInteger.Zero] = qc, [EInteger.One] = qb, [EInteger.FromInt32(2)] = qa };
            var qPrime = new Dictionary<EInteger, Entity> { [EInteger.Zero] = qb, [EInteger.One] = (2 * qa).InnerSimplified };
            var halfOdd = Number.Rational.Create(2 * k + 1, 2);
            var degreeOfR = degreeOfP + 1;
            var columns = new List<Dictionary<EInteger, Entity>>();
            for (var i = 0; i <= degreeOfR; i++)
            {
                var monomial = new Dictionary<EInteger, Entity> { [EInteger.FromInt32(i)] = Number.Integer.One };
                var column = PolynomialProduct(PolynomialDerivative(monomial), q);
                var alongQ = new Dictionary<EInteger, Entity>();
                foreach (var pair in PolynomialProduct(monomial, q))
                    alongQ[pair.Key] = (a * pair.Value).InnerSimplified;
                var alongQPrime = new Dictionary<EInteger, Entity>();
                foreach (var pair in PolynomialProduct(monomial, qPrime))
                    alongQPrime[pair.Key] = (halfOdd * pair.Value).InnerSimplified;
                columns.Add(PolynomialSum(PolynomialSum(column, alongQ), alongQPrime));
            }
            var rows = columns.SelectMany(column => column.Keys).Concat(pRead.Keys).Max()!.ToInt32Unchecked() + 1;
            var matrix = new Entity[rows][];
            var rhs = new Entity[rows];
            for (var row = 0; row < rows; row++)
            {
                var power = EInteger.FromInt32(row);
                matrix[row] = new Entity[columns.Count];
                for (var column = 0; column < columns.Count; column++)
                    matrix[row][column] = columns[column].TryGetValue(power, out var entry) ? entry : Number.Integer.Zero;
                rhs[row] = pRead.TryGetValue(power, out var wanted) ? wanted : Number.Integer.Zero;
            }
            if (!Functions.PartialFractions.TrySolveLinear(matrix, rhs, out var values) || values is null)
                return null;
            Entity r = Number.Integer.Zero;
            for (var i = 0; i <= degreeOfR; i++)
            {
                var value = values[i].InnerSimplified;
                if (value.Evaled is Number.Complex { IsZero: true })
                    continue;
                r = r + value * (i == 0 ? Number.Integer.One : i == 1 ? x : MathS.Pow(x, i));
            }
            if (r == Number.Integer.Zero)
                return null;
            var answer = (constant * MathS.Pow(MathS.e, exponent) * r * MathS.Pow(radicand, Number.Rational.Create(2 * k + 1, 2))).InnerSimplified;
            if (answer.Nodes.Any(node => node is Number.Complex { IsNaN: true })
                || !Functions.PartialFractions.HoldsAtSampledPoints(answer.Differentiate(x), expr, x))
                return null;
            return answer;
        }

        /// <summary>
        /// A polynomial times an odd half power of a quadratic, <c>P Q^(m/2)</c>, reduced to
        /// <c>R Q^(k + 1/2) + K/sqrt(Q)</c> by one linear solve, with <c>K/sqrt(Q)</c> the
        /// table's -- an arcsine or a logarithm by the sign of the leading coefficient, a
        /// piecewise where that sign is a symbol's.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For <c>m = 2j - 1</c> at least <c>-1</c> the ansatz is <c>F = R sqrt(Q)</c>, and
        /// <c>F' = P Q^j/sqrt(Q) - K/sqrt(Q)</c> is <c>R' Q + R Q'/2 + K = P Q^j</c>; for
        /// <c>m = -2j - 3</c> it is <c>F = R Q^(-j - 1/2)</c>, and the identity
        /// <c>R' Q - (j + 1/2) R Q' + K Q^(j + 1) = P</c>. Each is one equation per power of
        /// x, linear in the coefficients of <c>R</c> and in <c>K</c>, exact; a solution is an
        /// answer and its absence a decline.
        /// </para>
        /// <para>
        /// Hearn's <c>r/sqrt(-alpha^2 - 2k r + 2pe r^2)</c>, Stewart's
        /// <c>x^2/(a^2 - x^2)^(3/2)</c> and Apostol's <c>(a^2 - x^2)^(5/2)</c> had no
        /// antiderivative: with a symbol in the radicand the trigonometric substitution has
        /// no sign to go on and Euler's takes a root of it, while <c>1/sqrt(Q)</c> alone was
        /// answered, as a piecewise on the sign of the leading coefficient, with
        /// <c>arcsin(x/sqrt(a^2))</c> right for either sign of <c>a</c>. The reduction is what
        /// every table does before that line. Numeric coefficients reach this only from a
        /// sub-integral, since the substitutions in front answer them.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveAPolynomialTimesAnOddHalfPowerOfAQuadratic(Entity expr, Entity.Variable x)
        {
            Entity? radicand = null;
            var m = 0;
            Entity polynomial = Number.Integer.One;
            Entity constant = Number.Integer.One;
            foreach (var (factor, underneath) in FactorsOfTheIntegrand(expr))
            {
                if (!factor.ContainsNode(x))
                {
                    constant = underneath ? constant / factor : constant * factor;
                    continue;
                }
                if (factor is Powf(var @base, Number.Rational exponent) && exponent is not Number.Integer
                    && exponent.ERational.Denominator.Equals(EInteger.FromInt32(2)) && exponent.ERational.Numerator.CanFitInInt32()
                    && radicand is null)
                {
                    radicand = @base;
                    m = exponent.ERational.Numerator.ToInt32Unchecked() * (underneath ? -1 : 1);
                    continue;
                }
                if (underneath)
                    return null;
                polynomial = polynomial * factor;
            }
            if (radicand is null || !TreeAnalyzer.TryGetPolyQuadratic(radicand, x, out var a, out var b, out var c)
                || TreeAnalyzer.IsZero(a)
                || !TreeAnalyzer.TryGetPolynomial(polynomial.Expand(), x, out var pRead) || pRead.Keys.Any(k => k.Sign < 0 || !k.CanFitInInt32())
                || pRead.Values.Any(coefficient => coefficient.ContainsNode(x)))
                return null;
            var degreeOfP = pRead.Count == 0 ? 0 : pRead.Keys.Max()!.ToInt32Unchecked();
            if (degreeOfP > 8 || m > 9 || m < -9)
                return null;
            foreach (var coefficient in new[] { a, b, c, constant }.Concat(pRead.Values))
                if (coefficient.Evaled is Number.Complex and not Number.Real)
                    return null;

            var q = new Dictionary<EInteger, Entity> { [EInteger.Zero] = c, [EInteger.One] = b, [EInteger.FromInt32(2)] = a };
            var qPrime = new Dictionary<EInteger, Entity> { [EInteger.Zero] = b, [EInteger.One] = (2 * a).InnerSimplified };
            Dictionary<EInteger, Entity> PowerOfQ(int j)
            {
                var result = new Dictionary<EInteger, Entity> { [EInteger.Zero] = Number.Integer.One };
                for (var i = 0; i < j; i++)
                    result = PolynomialProduct(result, q);
                return result;
            }
            // The target and the coefficient of R's monomials and of K, by the sign of m.
            var positive = m >= -1;
            var j = positive ? (m + 1) / 2 : (-m - 3) / 2;
            var target = positive ? PolynomialProduct(pRead, PowerOfQ(j)) : pRead;
            var degreeOfTarget = target.Count == 0 ? 0 : target.Keys.Max()!.ToInt32Unchecked();
            var degreeOfR = System.Math.Max(degreeOfTarget, 2 * j + 2) + 1;
            var factorOnRQPrime = positive ? Number.Rational.Create(1, 2) : Number.Rational.Create(-(2 * j + 1), 2);
            var columns = new List<Dictionary<EInteger, Entity>>();
            for (var k = 0; k <= degreeOfR; k++)
            {
                var monomial = new Dictionary<EInteger, Entity> { [EInteger.FromInt32(k)] = Number.Integer.One };
                var scaled = new Dictionary<EInteger, Entity>();
                foreach (var pair in PolynomialProduct(monomial, qPrime))
                    scaled[pair.Key] = (factorOnRQPrime * pair.Value).InnerSimplified;
                columns.Add(PolynomialSum(PolynomialProduct(PolynomialDerivative(monomial), q), scaled));
            }
            columns.Add(positive ? new Dictionary<EInteger, Entity> { [EInteger.Zero] = Number.Integer.One } : PowerOfQ(j + 1));
            var rows = columns.SelectMany(column => column.Keys).Concat(target.Keys).Max()!.ToInt32Unchecked() + 1;
            var matrix = new Entity[rows][];
            var rhs = new Entity[rows];
            for (var row = 0; row < rows; row++)
            {
                var power = EInteger.FromInt32(row);
                matrix[row] = new Entity[columns.Count];
                for (var column = 0; column < columns.Count; column++)
                    matrix[row][column] = columns[column].TryGetValue(power, out var entry) ? entry : Number.Integer.Zero;
                rhs[row] = target.TryGetValue(power, out var wanted) ? wanted : Number.Integer.Zero;
            }
            if (!Functions.PartialFractions.TrySolveLinear(matrix, rhs, out var values) || values is null)
                return null;
            Entity r = Number.Integer.Zero;
            for (var k = 0; k <= degreeOfR; k++)
            {
                var value = values[k].InnerSimplified;
                if (value.Evaled is Number.Complex { IsZero: true })
                    continue;
                r = r + value * (k == 0 ? Number.Integer.One : k == 1 ? x : MathS.Pow(x, k));
            }
            var kValue = values[degreeOfR + 1].InnerSimplified;
            var root = MathS.Pow(radicand, Number.Rational.Create(1, 2));
            var powerOfQInFront = positive ? root : MathS.Pow(radicand, Number.Rational.Create(-(2 * j + 1), 2));
            Entity reduced = r == Number.Integer.Zero ? Number.Integer.Zero : r * powerOfQInFront;
            Entity answer;
            if (kValue.Evaled is Number.Complex { IsZero: true })
                answer = reduced;
            else if (IntegralPatterns.TryStandardIntegrals(1 / root, x) is not { } table)
                return null;
            else if (table is Piecewise piecewise)
            {
                // The table's arm for a vanishing leading coefficient is not this
                // reduction's, which divided by it: on that arm the integrand is a polynomial
                // times a power of a linear, and the chain answers it as that.
                var arms = new List<Providedf>();
                foreach (var arm in piecewise.Cases)
                {
                    if (arm.Predicate == a.EqualTo(0))
                    {
                        var overALinear = polynomial * MathS.Pow(b * x + c, Number.Rational.Create(m, 2));
                        if (Integration.ComputeIndefiniteIntegral(overALinear.InnerSimplified, x, integrateByParts: false) is not { } onTheArm)
                            return null;
                        arms.Add(new Providedf(onTheArm, arm.Predicate));
                    }
                    else
                        arms.Add(new Providedf(reduced + kValue * arm.Expression, arm.Predicate));
                }
                answer = MathS.Piecewise(arms);
            }
            else
                answer = reduced + kValue * table;
            answer = (constant * answer).InnerSimplified;
            if (answer.Nodes.Any(node => node is Number.Complex { IsNaN: true })
                || !Functions.PartialFractions.HoldsAtSampledPoints(answer.Differentiate(x), expr, x))
                return null;
            return answer;
        }

        /// <summary>
        /// A constant over a linear beside the square root of a quadratic,
        /// <c>K/((x - p) sqrt(Q))</c>, by the reciprocal of the linear: with <c>t = 1/(x - p)</c>
        /// the root becomes one of a quadratic in <c>t</c> alone, and the table answers that.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>Q(p + 1/t) = (Q(p) t^2 + Q'(p) t + a)/t^2</c>, so <c>sqrt(Q) = sqrt(R(t))/|t|</c>
        /// with <c>R(t) = Q(p) t^2 + Q'(p) t + a</c>, and <c>dx = -dt/t^2</c>: the integrand is
        /// <c>-K sgn(t) dt/sqrt(R(t))</c>, whose integral is the table's, an arcsine or a
        /// logarithm by the sign of <c>Q(p)</c> -- a piecewise where that sign is a symbol's.
        /// The sign of <c>t</c> is the sign of <c>x - p</c>, and the antiderivative is
        /// <c>-K sgn(x - p) G(1/(x - p))</c> on both sides of <c>p</c>, exactly.
        /// </para>
        /// <para>
        /// This is the shape the Euler substitution answers at length, as a partial-fraction
        /// decomposition in <c>t</c>, and declines for a leading coefficient that is a
        /// symbol of the wrong sign: Hearn's <c>1/(r sqrt(-alpha^2 - epsilon^2 + 2h r^2 - 2k r^4))</c>
        /// is <c>1/(2u sqrt(-alpha^2 - epsilon^2 + 2h u - 2k u^2))</c> under <c>u = r^2</c>, with
        /// <c>-2k</c> in front and <c>-alpha^2 - epsilon^2</c> behind, and neither Euler's first
        /// nor second substitution has a real radical to take. Here <c>Q(0)</c> is
        /// <c>-alpha^2 - epsilon^2</c>, the arcsine arm, and the answer is Rubi's. Closed, and
        /// volunteered at any depth for it: the reciprocal substitution and <c>u = x^2</c> both
        /// hand it what they make.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveALinearBesideTheRootOfAQuadratic(Entity expr, Entity.Variable x)
        {
            var (numerator, denominator) = Functions.SingleQuotient.Of(expr);
            if (numerator.ContainsNode(x))
                return null;
            Entity? radicand = null;
            Entity? linear = null;
            Entity constant = Number.Integer.One;
            foreach (var factor in Mulf.LinearChildren(denominator))
            {
                if (!factor.ContainsNode(x))
                {
                    constant = constant * factor;
                    continue;
                }
                if (factor is Powf(var @base, Number.Rational half) && half == Number.Rational.Create(1, 2) && radicand is null)
                    radicand = @base;
                else if (linear is null && TreeAnalyzer.TryGetPolyLinear(factor, x, out var slope, out _)
                    && slope.Evaled is not Number.Complex { IsZero: true })
                    linear = factor;
                else
                    return null;
            }
            if (radicand is null || linear is null
                || !TreeAnalyzer.TryGetPolyQuadratic(radicand, x, out var a, out var b, out var c)
                || a.Evaled is Number.Complex { IsZero: true }
                || !TreeAnalyzer.TryGetPolyLinear(linear, x, out var m, out var n))
                return null;
            // A coefficient that is a number is a real one, as in every rule about a real root.
            foreach (var coefficient in new[] { a, b, c, m, n, numerator, constant })
                if (coefficient.Evaled is Number.Complex and not Number.Real)
                    return null;

            // (x - p) with p = -n/m, and K = numerator/(m constant).
            var p = (-n / m).InnerSimplified;
            var k = (numerator / (m * constant)).InnerSimplified;
            var atP = (a * p * p + b * p + c).InnerSimplified;
            var slopeAtP = (2 * a * p + b).InnerSimplified;
            var t = Variable.CreateUnique(expr, "t_recip");
            var inT = 1 / MathS.Pow(atP * MathS.Sqr(t) + slopeAtP * t + a, Number.Rational.Create(1, 2));
            if (IntegralPatterns.TryStandardIntegrals(inT, t) is not { } g)
                return null;
            var answer = (-k * MathS.Signum(x - p) * g.Substitute(t, 1 / (x - p))).InnerSimplified;
            return answer.Nodes.Any(node => node is Number.Complex { IsNaN: true }) ? null : answer;
        }

        /// <summary>
        /// A polynomial over a product of distinct linears, beside the square root of a
        /// quadratic above or below the bar, taken apart as the rational function it is over
        /// that root: <c>N sqrt(Q)/D</c> is <c>N Q/(D sqrt(Q))</c>, and <c>P/D</c> is a polynomial
        /// plus a constant over each linear, so the integrand is a polynomial over the root
        /// plus one <c>K/((x - p) sqrt(Q))</c> per linear -- the rule before this one's shape,
        /// each.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Moses' <c>sqrt(A^2 + B^2 (1 - y^2))/(1 - y^2)</c>: the root is of <c>A^2 + B^2 - B^2 y^2</c>,
        /// so this is <c>(A^2 + B^2 - B^2 y^2)/((1 - y^2) sqrt(Q))</c>, whose rational part is
        /// <c>B^2 + (A^2)/(1 - y^2)</c>, and <c>1/(1 - y^2)</c> is a half over <c>1 - y</c> and a
        /// half over <c>1 + y</c>. Each piece is a line of the table, and the whole was declined:
        /// the Euler substitution takes the generic first substitution for a symbolic leading
        /// coefficient, with <c>sqrt(-B^2)</c> in it.
        /// </para>
        /// <para>
        /// Only where there is something to take apart -- at least two pieces, each strictly
        /// smaller: a polynomial over the root alone, or one linear with nothing divided out,
        /// is the rule before this one's or the chain's already, and asking again would be
        /// asking the same question. The polynomial part goes to the chain, for
        /// <c>x^n/sqrt(Q)</c> is the trigonometric substitution's or Euler's by its shape; the
        /// linears are closed.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveARationalFunctionBesideTheRootOfAQuadratic(Entity expr, Entity.Variable x)
        {
            var (numerator, denominator) = Functions.SingleQuotient.Of(expr);
            Entity? radicand = null;
            var rootBelow = false;
            var wholePower = 0;
            Entity above = Number.Integer.One;
            Entity below = Number.Integer.One;
            foreach (var (side, isBelow) in new[] { (numerator, false), (denominator, true) })
                foreach (var factor in Mulf.LinearChildren(side))
                {
                    if (factor is Powf(var @base, Number.Rational half) && half is not Number.Integer && @base.ContainsNode(x))
                    {
                        if (radicand is not null || half.ERational.Denominator.CompareTo(EInteger.FromInt32(2)) != 0
                            || !half.ERational.Numerator.CanFitInInt32())
                            return null;
                        radicand = @base;
                        var n = half.ERational.Numerator.ToInt32Unchecked();
                        rootBelow = isBelow != (n < 0);
                        // Q^(n/2) is Q^((|n| - 1)/2) sqrt(Q) on the side it is on.
                        wholePower = (System.Math.Abs(n) - 1) / 2;
                        if (wholePower > 0)
                        {
                            if (rootBelow) below = below * MathS.Pow(@base, wholePower);
                            else above = above * MathS.Pow(@base, wholePower);
                        }
                        continue;
                    }
                    if (factor.ContainsNode(x) && !TreeAnalyzer.TryGetPolynomial(factor, x, out _))
                        return null;
                    if (isBelow) below = below * factor;
                    else above = above * factor;
                }
            if (radicand is null || !TreeAnalyzer.TryGetPolyQuadratic(radicand, x, out var a, out _, out _)
                || a.Evaled is Number.Complex { IsZero: true })
                return null;
            // The quadratic standing whole beside its own root, up to a constant, is one power
            // of one base: `u^2 (1/2 - u^2/2)^(-1/2) / (1 - u^2)` is `sqrt(2) u^2 (1 - u^2)^(-3/2)`,
            // and is asked as that. Taken apart over the linears of `1 - u^2` instead, each
            // piece's answer carries a sign, `sgn(u - 1)`, of which the whole has no need --
            // and whose derivative is not read where the argument is not shown to be real, so
            // the answer to Timofeev's `arcsin(sqrt((x - a)/(x + a)))` could not be checked.
            if (WithTheQuadraticAsItsRoot(expr, radicand, x) is { } gathered)
                return Integration.ComputeIndefiniteIntegral(gathered, x, integrateByParts: false);
            // Written over the root: N sqrt(Q)/D is N Q/(D sqrt(Q)).
            if (!rootBelow)
                above = above * radicand;
            if (!below.ContainsNode(x))
                return null;

            // The denominator as distinct linears: factored over the integers where it is
            // numeric, and as written otherwise.
            var factors = new List<Entity>();
            Entity constant = Number.Integer.One;
            var written = Functions.PolynomialFactoring.TryFactor(below, x, out var factored) && factored is not null ? factored : below;
            foreach (var factor in Mulf.LinearChildren(written))
            {
                if (!factor.ContainsNode(x))
                {
                    constant = constant * factor;
                    continue;
                }
                if (!TreeAnalyzer.TryGetPolyLinear(factor, x, out var slope, out _) || slope.Evaled is Number.Complex { IsZero: true })
                    return null;
                factors.Add(factor);
            }
            // Distinct: two factors with the same root are one repeated, which is not this.
            for (var i = 0; i < factors.Count; i++)
                for (var j = i + 1; j < factors.Count; j++)
                    if (!TryReadAsQuotient(factors[i] / factors[j], out var top, out var bottom)
                        || Functions.PolynomialGcd.TryCancel(top, bottom, out _))
                        return null;
            var linears = factors.Aggregate(Number.Integer.One as Entity, (product, factor) => product * factor);

            // P/D as a polynomial plus a proper part, the proper part over each linear.
            Entity polynomialPart = Number.Integer.Zero;
            Entity properNumerator = above;
            if (TreeAnalyzer.PolynomialLongDivision(above, linears, genericCase: true, inTermsOf: x) is var (quotient, proper)
                && quotient.Evaled is not Number.Complex { IsZero: true })
            {
                polynomialPart = quotient;
                var (properTop, properBottom) = Functions.SingleQuotient.Of(proper);
                if (properBottom != linears && !TryReadAsQuotient(proper, out properTop, out properBottom))
                    return null;
                properNumerator = properTop;
                if (properNumerator.ContainsNode(x) && !TreeAnalyzer.TryGetPolynomial(properNumerator, x, out _))
                    return null;
            }
            var pieces = new List<Entity>();
            if (polynomialPart.Evaled is not Number.Complex { IsZero: true })
                pieces.Add(polynomialPart);
            if (properNumerator.Evaled is not Number.Complex { IsZero: true })
            {
                if (factors.Count == 1)
                    pieces.Add(properNumerator / factors[0]);
                else if (Functions.PartialFractions.TrySplitOverWrittenFactors(properNumerator, linears, x, out var decomposition) && decomposition is not null)
                {
                    // The split comes back as a sum over a constant; the constant goes to each term.
                    var (terms, over) = decomposition is Divf(var splitTop, var splitBottom) && !splitBottom.ContainsNode(x)
                        ? (splitTop, splitBottom) : (decomposition, Number.Integer.One as Entity);
                    foreach (var term in Sumf.LinearChildren(terms))
                        if (term.Evaled is not Number.Complex { IsZero: true })
                            pieces.Add(term / over);
                }
                else
                    return null;
            }
            if (pieces.Count < 2)
                return null;   // nothing was taken apart

            var root = MathS.Pow(radicand, Number.Rational.Create(1, 2));
            Entity answer = Number.Integer.Zero;
            foreach (var piece in pieces)
            {
                // Written as `coefficient / sqrt(Q)`, the shape the table reads for a constant.
                var overTheRoot = (piece / constant).InnerSimplified / root;
                var integrated = SolveALinearBesideTheRootOfAQuadratic(overTheRoot, x)
                    ?? (piece.ContainsNode(x) && !TreeAnalyzer.TryGetPolynomial(piece, x, out _) ? null : Integration.ComputeIndefiniteIntegral(overTheRoot, x, integrateByParts: false));
                if (integrated is null)
                    return null;
                answer = answer + integrated;
            }
            answer = answer.InnerSimplified;
            return answer.Nodes.Any(node => node is Number.Complex { IsNaN: true }) ? null : answer;
        }

        /// <summary>
        /// The product with a whole power of a constant multiple of <paramref name="radicand"/>
        /// written as that constant's power times the power of <paramref name="radicand"/>
        /// itself, so that the normalisation gathers it with the root; null where no factor is
        /// such a multiple, the factor that is <paramref name="radicand"/> as written included,
        /// since that one the normalisation has gathered already.
        /// </summary>
        private static Entity? WithTheQuadraticAsItsRoot(Entity expr, Entity radicand, Entity.Variable x)
        {
            if (!TreeAnalyzer.TryGetPolynomial(radicand, x, out var radicandMonomials))
                return null;
            var found = false;
            Entity? rebuilt = null;
            foreach (var factor in Mulf.LinearChildren(expr))
            {
                var (@base, power) = factor is Powf(var inner, Number.Integer whole) ? (inner, whole) : (factor, Number.Integer.One);
                Entity written = factor;
                if (!found && @base != radicand && @base.ContainsNode(x) && ConstantRatio(@base) is { } ratio)
                {
                    found = true;
                    written = MathS.Pow(ratio, power) * MathS.Pow(radicand, power);
                }
                rebuilt = rebuilt is null ? written : rebuilt * written;
            }
            return found ? rebuilt : null;

            Entity? ConstantRatio(Entity polynomial)
            {
                if (!TreeAnalyzer.TryGetPolynomial(polynomial, x, out var monomials) || monomials.Count != radicandMonomials.Count)
                    return null;
                Entity? ratio = null;
                foreach (var pair in monomials)
                {
                    if (!radicandMonomials.TryGetValue(pair.Key, out var reference))
                        return null;
                    var here = (pair.Value / reference).InnerSimplified;
                    if (here.ContainsNode(x) || (ratio is not null && here != ratio))
                        return null;
                    ratio ??= here;
                }
                return ratio;
            }
        }

        /// <summary>
        /// A rational function of <c>x</c> and one square root of a quadratic in <c>x</c>,
        /// rationalised by an Euler substitution and handed to the rational integrator.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The trigonometric substitution beside this answers <c>x^m sqrt(a + b x^2)^k</c> and
        /// nothing wider; <c>sqrt(2 - x - x^2)/x^2</c>, <c>1/((4 + x^2) sqrt(1 + 4x^2))</c> and
        /// <c>x sqrt(2 r x - x^2)</c> had no antiderivative, and neither did any of the
        /// integrands that other rules reduce to a rational function of <c>x</c> and one such
        /// root -- the nested radicals of Bondarenko's suite under <c>u = sqrt(1 + x)</c>, the
        /// by-parts remainders of Charlwood's inverse functions.
        /// </para>
        /// <para>
        /// <b>Euler's three substitutions</b>, for <c>Q = a x^2 + b x + c</c>:
        /// </para>
        /// <list type="bullet">
        /// <item><c>a &gt; 0</c>: <c>sqrt(Q) = t - sqrt(a) x</c>, so <c>x = (t^2 - c)/(2 sqrt(a) t + b)</c>.</item>
        /// <item><c>c &gt; 0</c>: <c>sqrt(Q) = x t + sqrt(c)</c>, so <c>x = (2 sqrt(c) t - b)/(a - t^2)</c>.</item>
        /// <item><c>c = 0</c>: <c>sqrt(Q) = x t</c>, so <c>x = b/(t^2 - a)</c> -- the third substitution at
        /// the root the quadratic has at zero.</item>
        /// </list>
        /// <para>
        /// Each makes <c>x</c>, <c>sqrt(Q)</c> and <c>dx/dt</c> rational in <c>t</c>, so the
        /// integrand becomes a rational function of <c>t</c>; whichever applies with its radical
        /// a rational number is taken first, and a symbol in <c>a</c> or <c>c</c> takes the first
        /// in the generic case. The result goes to the rational integrator <b>directly</b> --
        /// long division, the splits, the Hermite reduction, the binomial rule -- and not back
        /// into the chain: an earlier Euler rule rewrote and handed on, and its cost was in the
        /// open search below it and in a <c>Simplify</c> it ran before it could tell whether it
        /// applied at all (https://github.com/asc-community/AngouriMath/issues/1265). This one
        /// decides by reading the tree, in microseconds, and finishes in one closed step or not
        /// at all.
        /// </para>
        /// <para>
        /// Bounded in the degree of the rational function it produces, since a degree the
        /// splits cannot factor is declined by them at a cost that grows with it. Asked, not
        /// volunteered, like every rule that lands on a search of any size.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveByEulerSubstitution(Entity expr, Entity.Variable x)
        {

            // One square root of a quadratic in x, and otherwise a rational function of x --
            // or, besides those, powers of `x + sqrt(Q)` and `sqrt(Q) - x` to any exponent,
            // which the first substitution makes powers of `t`: see below.
            Entity? radicand = null;
            var powersOfTheSubstitution = new List<Powf>();
            foreach (var node in expr.Nodes)
            {
                if (!node.ContainsNode(x))
                    continue;
                switch (node)
                {
                    case Variable or Sumf or Minusf or Mulf or Divf:
                        break;
                    case Powf(_, Number.Integer):
                        break;
                    case Powf(var radicalBase, Number.Rational half) when half.ERational.Denominator.Equals(EInteger.FromInt32(2))
                            && radicalBase.Nodes.All(inner => inner is not Powf(_, Number.Rational r) || r is Number.Integer):
                        if (radicand is null)
                            radicand = radicalBase;
                        else if (radicand != radicalBase)
                            return null;
                        break;
                    case Powf(var @base, var exponent) when !exponent.ContainsNode(x) && @base.Nodes.Any(inner => inner is Powf(_, Number.Rational)):
                        powersOfTheSubstitution.Add((Powf)node);
                        break;
                    default:
                        return null;
                }
            }
            // The radical inside such a power is the radical: `(x + sqrt(1 + x^2))^b` holds it.
            if (radicand is null)
                foreach (var power in powersOfTheSubstitution)
                    foreach (var inner in power.Base.Nodes)
                        if (inner is Powf(var innerBase, Number.Rational innerHalf) && innerHalf.ERational.Denominator.Equals(EInteger.FromInt32(2)) && innerBase.ContainsNode(x))
                        {
                            if (radicand is null) radicand = innerBase;
                            else if (radicand != innerBase) return null;
                        }
            if (radicand is null
                || !TreeAnalyzer.TryGetPolyQuadratic(radicand, x, out var a, out var b, out var c)
                || a.Evaled is Number.Complex { IsZero: true })
                return null;
            // A missing linear term is the trigonometric substitution's and answered more shortly there.
            if (b.Evaled is Number.Complex { IsZero: true } && expr.Nodes.Count(n => n == radicand) == 1
                && TryReadAPowerTimesARadicalQuadratic(expr, x))
                return null;

            var t = Variable.CreateUnique(expr, "t_euler");
            Entity xInT, rootInT;
            Entity backSubstitution;
            // A coefficient that is a number has to be a real one: `sqrt(x^2 - i)` is not a
            // radical any of the three substitutions is about, and taking `-i` for a symbol
            // produced `NaN` for an answer.
            if (a.Evaled is Number.Complex and not Number.Real || b.Evaled is Number.Complex and not Number.Real
                || c.Evaled is Number.Complex and not Number.Real)
                return null;
            var aValue = a.Evaled as Number.Real;
            var cValue = c.Evaled as Number.Real;
            Entity sqrtA = MathS.Sqrt(a).InnerSimplified;
            Entity sqrtC = MathS.Sqrt(c).InnerSimplified;
            var aPositive = aValue is { IsPositive: true };
            var cPositive = cValue is { IsPositive: true };
            var aRational = aPositive && sqrtA.Evaled is Number.Rational;
            var cRational = cPositive && sqrtC.Evaled is Number.Rational;
            // Whichever applies with its radical a rational number first; then a real one; then
            // the generic case for a symbol.
            int which;
            if (cValue is { IsZero: true }) which = 3;
            else if (aRational) which = 1;
            else if (cRational) which = 2;
            else if (aPositive) which = 1;
            else if (cPositive) which = 2;
            else if (aValue is null) which = 1;
            else if (cValue is null) which = 2;
            else if (aValue.IsNegative && cValue.IsNegative && b.Evaled is Number.Real)
            {
                // a < 0 and c < 0: the radical is real between the roots where there are two,
                // and none of the three substitutions reads that. Shifted to the vertex,
                // x = y - b/(2a), the quadratic is a y^2 + c' with c' = c - b^2/(4a), positive
                // exactly when the roots are real, and that is the second substitution's:
                // `1/(x sqrt(-5 + 10x - 4x^2))`, Hearn's under u = r^2.
                var shift = (-b / (2 * a)).InnerSimplified;
                if ((c - b * b / (4 * a)).Evaled is not Number.Real { IsPositive: true })
                    return null;
                var y = Variable.CreateUnique(expr, "y_euler");
                if (SolveByEulerSubstitution(expr.Substitute(x, y + shift), y) is not { } inY)
                    return null;
                // The radicand comes back written as `Q(x - s + s)`; it is written as `Q(x)`.
                var writtenBack = radicand.Substitute(x, y + shift).Substitute(y, x - shift);
                return inY.Substitute(y, x - shift).Replace(node => node == writtenBack ? radicand : node).InnerSimplified;
            }
            else return null;   // a < 0 and c < 0 with no real root: the radical is nowhere real

            switch (which)
            {
                case 3:
                    // sqrt(Q) = x t: x = b/(t^2 - a)
                    xInT = b / (MathS.Sqr(t) - a);
                    rootInT = xInT * t;
                    backSubstitution = MathS.Pow(radicand, Number.Rational.Create(1, 2)) / x;
                    break;
                case 1:
                    // sqrt(Q) = t - sqrt(a) x: x = (t^2 - c)/(2 sqrt(a) t + b)
                    xInT = (MathS.Sqr(t) - c) / (2 * sqrtA * t + b);
                    rootInT = t - sqrtA * xInT;
                    backSubstitution = MathS.Pow(radicand, Number.Rational.Create(1, 2)) + sqrtA * x;
                    break;
                default:
                    // sqrt(Q) = x t + sqrt(c): x = (2 sqrt(c) t - b)/(a - t^2)
                    xInT = (2 * sqrtC * t - b) / (a - MathS.Sqr(t));
                    rootInT = xInT * t + sqrtC;
                    backSubstitution = (MathS.Pow(radicand, Number.Rational.Create(1, 2)) - sqrtC) / x;
                    break;
            }

            // **A power of the substitution itself.** With `t = sqrt(Q) + sqrt(a) x`, a factor
            // `(x + sqrt(Q))^b` is `t^b` for `a = 1`, exactly and for any `b`, and `sqrt(Q) - x`
            // is `c/t`, since `(sqrt(Q) + x)(sqrt(Q) - x) = Q - x^2 = b x + c` -- `c/t` when the
            // linear term is absent. Welz's `(x + sqrt(b + x^2))^a` and Bondarenko's
            // `1/(1 + sqrt(x + sqrt(1 + x^2)))` are both this: the first becomes Laurent
            // monomials in `t` to the power `a`, the second a rational function of `sqrt(t)`,
            // and each is handed to the chain in `t` rather than to the rational integrator,
            // since neither is a quotient of polynomials.
            var powered = false;
            if (powersOfTheSubstitution.Count > 0)
            {
                if (which != 1 || !(a.Evaled is Number.Integer { IsZero: false } aOne && aOne.EInteger.Equals(EInteger.One)))
                    return null;
                // `x - sqrt(Q)` is the same substitution with the root's sign flipped:
                // `t = x - sqrt(Q)` gives the same `x(t)`, `sqrt(Q) = x - t` in place of `t - x`,
                // and `t = x - sqrt(Q)` on the way back. Both signs in one integrand is neither.
                var root = MathS.Pow(radicand, Number.Rational.Create(1, 2));
                var flipped = (bool?)null;
                foreach (var power in powersOfTheSubstitution)
                {
                    var plus = (power.Base - (x + root)).Simplify().Evaled is Number.Complex { IsZero: true };
                    var minus = (power.Base - (x - root)).Simplify().Evaled is Number.Complex { IsZero: true };
                    if (!plus && !minus)
                        return null;
                    if (flipped is null)
                        flipped = minus;
                    else if (flipped != minus)
                        return null;
                    expr = expr.Substitute(power, MathS.Pow(t, power.Exponent));
                }
                if (flipped == true)
                {
                    rootInT = xInT - t;
                    backSubstitution = x - root;
                }
                powered = true;
            }

            var dxdt = xInT.Differentiate(t);
            var rewritten = expr.Replace(node =>
                node is Powf(var @base, Number.Rational half) && @base == radicand
                    ? MathS.Pow(rootInT, Number.Integer.Create(half.ERational.Numerator))
                    : node)
                .Substitute(x, xInT) * dxdt;
            if (powered)
            {
                if (rewritten.ContainsNode(x))
                    return null;
                // With the written factors cancelled first: `1/sqrt(Q)` brings `(t^2 + c)` below
                // the bar and `dx/dt` brings it above, and beside a symbolic power of `t`
                // nothing later cancels them.
                var (poweredAbove, poweredBelow) = Functions.SingleQuotient.Of(rewritten.InnerSimplified);
                // The powers of t with an exponent that is not a whole number are set aside,
                // and what is left -- two polynomials in t -- is cancelled by their gcd, since
                // the two spellings of `t^2 + c` the substitution produces are not equal as
                // written; then they are put back.
                Entity setAside = Number.Integer.One;
                Entity polynomialAbove = Number.Integer.One;
                foreach (var factor in Mulf.LinearChildren(poweredAbove))
                    if (factor is Powf(var pb, var pe) && pb == t && pe is not Number.Integer)
                        setAside = setAside * factor;
                    else
                        polynomialAbove = polynomialAbove * factor;
                Entity cancelledPowered = polynomialAbove / poweredBelow;
                if (Functions.PolynomialGcd.TryCancel(polynomialAbove.InnerSimplified, poweredBelow.InnerSimplified, out var poweredByGcd) && poweredByGcd is not null)
                    cancelledPowered = Functions.PartialFractions.Bare(poweredByGcd);
                var poweredInT = Functions.SingleQuotient.Combine(setAside * cancelledPowered).Simplify();
                if (poweredInT is Providedf(var bareInT, _))
                    poweredInT = bareInT;
                var inTByTheChain = Integration.ComputeIndefiniteIntegral(poweredInT, t, integrateByParts: true);
                if (inTByTheChain is null)
                    return null;
                var poweredAnswer = inTByTheChain.Substitute(t, backSubstitution).InnerSimplified;
                return poweredAnswer.Nodes.Any(n => n is Number.Complex { IsNaN: true }) ? null : poweredAnswer;
            }
            var (numerator, denominator) = Functions.SingleQuotient.Of(rewritten.InnerSimplified);
            var cancelled = CancelCommonFactors(numerator, denominator);
            (numerator, denominator) = Functions.SingleQuotient.Of(cancelled);
            // And by the polynomial gcd where the written factors do not match: the third
            // substitution leaves `(2t)^2 - 2(t^2 + 1)` above and `2t^2 - (t^2 + 1)` below for
            // `1/((1 + x^2)^2 sqrt(x^2 - 1))`, one factor twice the other, and the degree read
            // off the uncancelled quotient was ten where the bound is eight. Only where the
            // bound would otherwise refuse it, since the gcd hands back expanded sides and the
            // rational integrator does better with the written ones.
            // ...and only where the uncancelled degree is within twice the bound: the gcd of
            // two polynomials of degree thirty with the coefficients a power eleven over two
            // produces did not return in ninety seconds for `1/((3 - 2x + x^2)^(11/2) (1 + x + 2x^2)^5)`,
            // which the bound alone declined in a moment.
            if (TreeAnalyzer.TryGetPolynomial(numerator, t, out var beforeAbove) && TreeAnalyzer.TryGetPolynomial(denominator, t, out var beforeBelow)
                && System.Math.Max(beforeAbove.Count == 0 ? 0 : (int)beforeAbove.Keys.Max()!.ToInt32Checked(),
                                   beforeBelow.Count == 0 ? 0 : (int)beforeBelow.Keys.Max()!.ToInt32Checked()) is var uncancelled
                && uncancelled > MaximumEulerDegree && uncancelled <= 2 * MaximumEulerDegree
                && Functions.PolynomialGcd.TryCancel(numerator, denominator, out var byGcd) && byGcd is not null)
                (numerator, denominator) = Functions.SingleQuotient.Of(Functions.PartialFractions.Bare(byGcd));
            // Each side rebuilt as a polynomial from its coefficients, so that a factor which has
            // collapsed to a constant -- `-1 - t^2 + 2 t t - 1 - t^2` is `-2` -- is one before
            // the rational readers see it: one of them divided by that factor's zero leading
            // coefficient and read a NaN. Every coefficient must be finite for the same reason.
            if (!TreeAnalyzer.TryGetPolynomial(numerator, t, out var above) || !TreeAnalyzer.TryGetPolynomial(denominator, t, out var below))
                return null;
            var degree = System.Math.Max(above.Count == 0 ? 0 : (int)above.Keys.Max()!.ToInt32Checked(),
                                         below.Count == 0 ? 0 : (int)below.Keys.Max()!.ToInt32Checked());
            if (degree > MaximumEulerDegree)
                return null;
            Entity? Rebuilt(Dictionary<EInteger, Entity> read)
            {
                Entity? built = null;
                foreach (var pair in read.OrderBy(pair => pair.Key))
                {
                    var coefficient = pair.Value.InnerSimplified;
                    if (coefficient.Evaled is Number.Complex { IsFinite: false })
                        return null;
                    if (coefficient.Evaled is Number.Complex { IsZero: true })
                        continue;
                    var k = pair.Key.ToInt32Checked();
                    Entity term = k == 0 ? coefficient : coefficient * (k == 1 ? t : MathS.Pow(t, k));
                    built = built is null ? term : built + term;
                }
                return built ?? Number.Integer.Zero;
            }
            // The denominator keeps its written factors -- with `sqrt(c)` irrational only the
            // split over written factors reads it -- and each factor is rebuilt on its own.
            Entity? cleanDenominator = null;
            foreach (var factor in Mulf.LinearChildren(denominator))
            {
                var (@base, power) = factor is Powf(var b0, Number.Integer e0) ? (b0, e0) : (factor, Number.Integer.One);
                if (!TreeAnalyzer.TryGetPolynomial(@base, t, out var baseRead) || Rebuilt(baseRead) is not { } cleanBase)
                    return null;
                if (cleanBase == Number.Integer.Zero)
                    return null;
                Entity cleanFactor = power == Number.Integer.One ? cleanBase : MathS.Pow(cleanBase, power);
                cleanDenominator = cleanDenominator is null ? cleanFactor : cleanDenominator * cleanFactor;
            }
            if (Rebuilt(above) is not { } cleanNumerator || cleanDenominator is null)
                return null;
            var rational = cleanNumerator / cleanDenominator;

            // The Rothstein-Trager resultant behind the splits, for a denominator they cannot
            // take apart: `1/(1 + x sqrt(1 - x^2))` is one over a quartic in t irreducible over
            // the rationals, whose residues are in a quadratic field.
            // ...and a polynomial in t, which none of those reads: `1/(x sqrt(3x - x^2))` under
            // the third substitution is the constant -2/3.
            var inT = SolveByPartialFractions(rational, t, integrateByParts: false)
                   ?? IntegralPatterns.TryStandardIntegrals(rational, t)
                   ?? SolveByRothsteinTrager(rational, t)
                   ?? (cleanDenominator.Evaled is Number ? Integration.ComputeIndefiniteIntegral(rational, t, integrateByParts: false) : null);
            if (inT is null)
                return null;
            var answer = inT.Substitute(t, backSubstitution).InnerSimplified;
            // Not answering is legitimate; answering NaN is not.
            return answer.Nodes.Any(n => n is Number.Complex { IsNaN: true }) ? null : answer;
        }

        /// <summary>
        /// A rational function of <c>x</c> and one square root of a <b>palindromic quartic</b>
        /// <c>a x^4 + d x^3 + b x^2 + s d x + a</c>, integrated by <c>u = x - 1/x</c> or
        /// <c>u = x + 1/x</c> as <c>s</c> is <c>-1</c> or <c>+1</c>: the quartic is <c>x^2</c>
        /// times a quadratic in <c>u</c>, and the rest becomes a rational function of <c>u</c>
        /// where it is one.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Charlwood's <c>(1 + x^2)/((1 - x^2) sqrt(1 + x^4))</c> had no antiderivative. A root of
        /// a quartic is nothing Euler's substitutions or the trigonometric ones read, and it is
        /// not a binomial. But <c>1 + x^4 = x^2 (x^2 + 1/x^2) = x^2 ((x - 1/x)^2 + 2)</c>, and with
        /// <c>u = x - 1/x</c>, <c>du = (1 + 1/x^2) dx</c>, the integrand is <c>-du/(u sqrt(u^2 + 2))</c>
        /// -- a root of a quadratic over a rational function, which Euler answers. Its
        /// companion <c>(1 - x^2)/((1 + x^2) sqrt(1 + x^4))</c> wants <c>u = x + 1/x</c>, under
        /// which <c>x^2 + 1/x^2</c> is <c>u^2 - 2</c>. Both are tried.
        /// </para>
        /// <para>
        /// <b>The algebra.</b> With <c>u = x + s/x</c> for <c>s = -1</c> or <c>+1</c>:
        /// <c>x^2 + 1/x^2 = u^2 - 2s</c> and <c>x + s/x = u</c>, so
        /// <c>Q = a x^4 + d x^3 + b x^2 + s d x + a = x^2 (a u^2 + d u + b - 2 a s)</c>;
        /// and <c>dx = x^2 du/(x^2 - s)</c>. For <c>N/(D sqrt(Q))</c> the integrand is then
        /// <c>[N x/(D (x^2 - s))] du/sqrt(a u^2 + d u + b - 2as)</c>, and for <c>N sqrt(Q)/D</c> it
        /// is <c>[N x^3/(D (x^2 - s))] sqrt(a u^2 + d u + b - 2as) du</c>. Odd terms fix the
        /// sign: Timofeev's <c>(1 - x^2)/((1 + 2ax + x^2) sqrt(1 + 2ax + 2bx^2 + 2ax^3 + x^4))</c>
        /// is one of <c>u = x + 1/x</c> only, <c>-du/((u + 2a) sqrt(u^2 + 2au + 2b - 2))</c>, and
        /// its coefficients are symbols. The bracket is a rational
        /// function of <c>x</c>; the rule asks whether it is one of <c>u</c>, by undetermined
        /// coefficients on <c>P(u)/S(u)</c> and a check at sampled points, and declines where
        /// it is not -- which is the only way this can fail, and is exact.
        /// </para>
        /// <para>
        /// <b>The sign of <c>x</c>.</b> <c>sqrt(Q) = |x| sqrt(a u^2 + d u + b - 2as)</c>, and the
        /// rule takes <c>|x| = x</c>: what comes out is an antiderivative for <c>x &gt; 0</c>. It
        /// is made one everywhere by parity where the integrand has one, which is exact: an
        /// odd integrand has an even antiderivative, so <c>F(|x|)</c> serves on both sides, and
        /// an even one has an odd antiderivative, <c>sgn(x) F(|x|)</c>. An integrand of neither
        /// parity -- every one with odd terms under the root -- is answered by the sign of
        /// <c>x</c> instead: the integrand in <c>u</c> is <c>sgn(x) R(u) du/sqrt(q(u))</c> on both
        /// sides, so <c>sgn(x) G(x + s/x)</c> is an antiderivative on both, and is what is
        /// returned. A caller that knows its variable is positive, as the exponential
        /// substitution does of <c>u = e^x</c>, says so and gets <c>G(x + s/x)</c> bare.
        /// </para>
        /// <para>
        /// A half-odd power of the quartic, <c>Q^(3/2)</c>, is <c>Q sqrt(Q)</c>, a whole power
        /// beside the root, and is read as that: <c>sinh(x)^2 sinh(2x)/(1 - sinh(x)^2)^(3/2)</c>
        /// under <c>u = e^x</c> is a rational function over <c>(6u^2 - u^4 - 1)^(3/2)</c>.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveByReciprocalSubstitution(Entity expr, Entity.Variable x, bool variableIsPositive = false)
        {
            if (!Integration.AnsweringTheQuestionAsked && !variableIsPositive)
                return null;
            var (numerator, denominator) = Functions.SingleQuotient.Of(expr);

            // Exactly one radical, a half-odd power of a palindromic quartic, as a factor above
            // or below the bar; polynomials elsewhere.
            Entity? quartic = null;
            var rootBelow = false;
            Entity above = Number.Integer.One;
            Entity below = Number.Integer.One;
            foreach (var (side, isBelow) in new[] { (numerator, false), (denominator, true) })
                foreach (var factor in Mulf.LinearChildren(side))
                {
                    if (factor is Powf(var @base, Number.Rational half) && half is not Number.Integer && @base.ContainsNode(x))
                    {
                        if (quartic is not null || half.ERational.Denominator.CompareTo(EInteger.FromInt32(2)) != 0
                            || !half.ERational.Numerator.CanFitInInt32())
                            return null;
                        quartic = @base;
                        var n = half.ERational.Numerator.ToInt32Unchecked();
                        rootBelow = isBelow != (n < 0);
                        // Q^(n/2) = Q^((|n| - 1)/2) sqrt(Q), the whole power on the root's side.
                        var whole = (System.Math.Abs(n) - 1) / 2;
                        if (whole > 0)
                        {
                            if (rootBelow) below = below * MathS.Pow(@base, whole);
                            else above = above * MathS.Pow(@base, whole);
                        }
                        continue;
                    }
                    if (factor.ContainsNode(x) && !TreeAnalyzer.TryGetPolynomial(factor, x, out _))
                        return null;
                    if (isBelow) below = below * factor;
                    else above = above * factor;
                }
            if (quartic is null || !TreeAnalyzer.TryGetPolynomial(quartic, x, out var read))
                return null;
            Entity Coefficient(int degree) => read.TryGetValue(EInteger.FromInt32(degree), out var c) ? c : Number.Integer.Zero;
            if (read.Keys.Any(k => !k.CanFitInInt32() || k.ToInt32Unchecked() is not (0 or 1 or 2 or 3 or 4)))
                return null;
            var a = Coefficient(4);
            var b = Coefficient(2);
            var c = Coefficient(0);
            // The odd terms: `d x^3 + e x` is `x^2 (d x + e/x)`, which is `d x^2 u` under
            // `u = x + s/x` exactly when `e = s d`. Timofeev's
            // `sqrt(1 + 2ax + 2bx^2 + 2ax^3 + x^4)` is one, under `u = x + 1/x`, and its
            // coefficients are symbols: a coefficient is a real number or not a number at all.
            var d = Coefficient(3);
            var e = Coefficient(1);
            static bool IsRealOrSymbolic(Entity coefficient) => coefficient.Evaled is Number.Real || coefficient.Evaled is not Number;
            // `2a - 2a` is not collected by the inner simplification; the full one is asked
            // only where the difference is not a number already.
            static bool IsIdenticallyZero(Entity difference) => difference.Evaled is Number number ? number == 0 : TreeAnalyzer.IsZero(difference.Simplify());
            if (a.Evaled is not Number.Real { IsZero: false } || !IsIdenticallyZero(a - c)
                || !IsRealOrSymbolic(b) || !IsRealOrSymbolic(d) || !IsRealOrSymbolic(e))
                return null;

            foreach (var sign in new[] { -1, 1 })
            {
                if (!IsIdenticallyZero(e - sign * d))
                    continue;
                // R(x) = N x^k / (D (x^2 - s)), k = 1 with the root below, 3 above.
                var bracket = above * (rootBelow ? x : MathS.Pow(x, 3)) / (below * (MathS.Sqr(x) - sign));
                if (!TryWriteInTheReciprocalVariable(bracket, x, sign, out var u, out var inU))
                    continue;
                var quadratic = a * MathS.Sqr(u) + d * u + (b - 2 * sign * a);
                var integrand = (inU * MathS.Pow(quadratic, Number.Rational.Create(rootBelow ? -1 : 1, 2))).InnerSimplified;
                if (Integration.ComputeIndefiniteIntegral(integrand, u, integrateByParts: true) is not { } inTermsOfU)
                    continue;
                var forPositiveX = inTermsOfU.Substitute(u, x + Number.Integer.Create(sign) / x);
                if (forPositiveX.Nodes.Any(node => node == MathS.NaN))
                    continue;
                if (variableIsPositive)
                    return forPositiveX;

                // Extended to x < 0 by parity where the integrand has one, and by the sign
                // of x otherwise: sqrt(Q) is |x| sqrt(q(u)) on both sides, so the integrand
                // in u carries a factor sgn(x) there, and sgn(x) G(u(x)) is exact everywhere.
                return ExtendedByParity(expr, forPositiveX, x) ?? MathS.Signum(x) * forPositiveX;
            }
            return null;
        }

        /// <summary>
        /// <paramref name="forPositive"/>, an antiderivative of <paramref name="integrand"/> for
        /// <paramref name="x"/> positive, extended to the other side by parity, which is exact:
        /// an odd integrand has an even antiderivative, so <c>F(|x|)</c> serves on both sides,
        /// and an even one has an odd antiderivative, <c>sgn(x) F(|x|)</c>. An integrand of
        /// neither parity is declined rather than answered on half the line.
        /// </summary>
        private static Entity? ExtendedByParity(Entity integrand, Entity forPositive, Entity.Variable x)
        {
            // In the generic case, as every rule answers: the conditions a simplification
            // attached on the way -- each denominator nonzero -- are not part of the answer.
            forPositive = Functions.PartialFractions.Bare(forPositive);
            var reflected = integrand.Substitute(x, -x);
            if (Functions.PartialFractions.HoldsAtSampledPoints(reflected, -integrand, x))
                return forPositive.Substitute(x, MathS.Abs(x));
            if (Functions.PartialFractions.HoldsAtSampledPoints(reflected, integrand, x))
                return MathS.Signum(x) * forPositive.Substitute(x, MathS.Abs(x));
            return null;
        }

        /// <summary>
        /// The rational function <paramref name="bracket"/> of <paramref name="x"/> written as
        /// one of <c>u = x + sign/x</c>, by undetermined coefficients on <c>P(u)/S(u)</c> and a
        /// check at sampled points; <see langword="false"/> where it is not one.
        /// </summary>
        private static bool TryWriteInTheReciprocalVariable(Entity bracket, Entity.Variable x, int sign, out Entity.Variable u, out Entity inU)
        {
            u = Variable.CreateUnique(bracket, "u_recip");
            inU = Number.Integer.Zero;
            var simplified = bracket.InnerSimplified;
            if (simplified is Providedf(var bare, _))
                simplified = bare;
            var (top, bottom) = Functions.SingleQuotient.Of(simplified);
            if (!TreeAnalyzer.TryGetPolynomial(top, x, out var topRead) || !TreeAnalyzer.TryGetPolynomial(bottom, x, out var bottomRead))
                return false;
            var degree = System.Math.Max(
                topRead.Count == 0 ? 0 : topRead.Keys.Max()!.ToInt32Checked(),
                bottomRead.Count == 0 ? 0 : bottomRead.Keys.Max()!.ToInt32Checked());
            // Cancelled by the polynomial gcd where the written bracket runs past the bound
            // but not far past it: the exponential substitution hands the quartic to a whole
            // power beside its root, and `(u^2 - 1)^2 (u^4 - 1) u^3/(u^5 Q (u^2 + 1))` is
            // degree eleven as written and `-(u^2 - 1)^3/(u^3 (u^2 + 1/u^2 - 6))` in lowest terms.
            if (degree > MaximumReciprocalDegree && degree <= 2 * MaximumReciprocalDegree
                && Functions.PolynomialGcd.TryCancel(top, bottom, out var byGcd) && byGcd is not null)
            {
                (top, bottom) = Functions.SingleQuotient.Of(Functions.PartialFractions.Bare(byGcd));
                if (!TreeAnalyzer.TryGetPolynomial(top, x, out topRead) || !TreeAnalyzer.TryGetPolynomial(bottom, x, out bottomRead))
                    return false;
                degree = System.Math.Max(
                    topRead.Count == 0 ? 0 : topRead.Keys.Max()!.ToInt32Checked(),
                    bottomRead.Count == 0 ? 0 : bottomRead.Keys.Max()!.ToInt32Checked());
            }
            if (degree > MaximumReciprocalDegree)
                return false;

            // A(x) S(u) = B(x) P(u), multiplied through by x^m so that u^j is the polynomial
            // x^(m-j) (x^2 + s)^j: one linear identity in x per monomial, in the coefficients
            // of P and S. Homogeneous, so one coefficient of S is set to one -- each in turn,
            // since the right one is whichever S has.
            var m = degree;
            var powersOfU = new List<Entity>();
            for (var j = 0; j <= m; j++)
                powersOfU.Add(Functions.PartialFractions.Bare((MathS.Pow(x, m - j) * MathS.Pow(MathS.Sqr(x) + sign, j)).Expand()));
            var columns = new List<Dictionary<EInteger, Entity>>();
            // Expanding attaches the conditions it cleared denominators under; the columns are
            // polynomials and want none of them.
            for (var i = 0; i <= m; i++)
            {
                if (!TreeAnalyzer.TryGetPolynomial(Functions.PartialFractions.Bare((-bottom * powersOfU[i]).Expand()), x, out var column)) return false;
                columns.Add(column);
            }
            for (var j = 0; j <= m; j++)
            {
                if (!TreeAnalyzer.TryGetPolynomial(Functions.PartialFractions.Bare((top * powersOfU[j]).Expand()), x, out var column)) return false;
                columns.Add(column);
            }
            var monomials = columns.SelectMany(column => column.Keys).Distinct().OrderBy(k => k).ToList();
            for (var fixedS = 0; fixedS <= m; fixedS++)
            {
                var fixedColumn = m + 1 + fixedS;
                var unknowns = Enumerable.Range(0, 2 * (m + 1)).Where(k => k != fixedColumn).ToList();
                var matrix = new Entity[monomials.Count][];
                var rhs = new Entity[monomials.Count];
                for (var row = 0; row < monomials.Count; row++)
                {
                    matrix[row] = new Entity[unknowns.Count];
                    for (var k = 0; k < unknowns.Count; k++)
                        matrix[row][k] = columns[unknowns[k]].TryGetValue(monomials[row], out var entry) ? entry : Number.Integer.Zero;
                    rhs[row] = columns[fixedColumn].TryGetValue(monomials[row], out var fixedEntry) ? -fixedEntry : Number.Integer.Zero;
                }
                var solved = Functions.PartialFractions.TrySolveLinearWithSymbols(matrix, rhs, out var values);
                if (!solved || values is null)
                    continue;
                Entity p = Number.Integer.Zero;
                Entity q = Number.Integer.Zero;
                for (var k = 0; k < unknowns.Count; k++)
                {
                    var index = unknowns[k];
                    var value = values[k].InnerSimplified;
                    if (value.Evaled is Number.Complex { IsZero: true })
                        continue;
                    if (index <= m)
                        p += value * MathS.Pow(u, index);
                    else
                        q += value * MathS.Pow(u, index - m - 1);
                }
                q += MathS.Pow(u, fixedS);
                if (p.Evaled is Number.Complex { IsZero: true })
                    continue;
                var candidate = (p / q).InnerSimplified;
                if (candidate is Providedf(var inner, _))
                    candidate = inner;
                // Checked as a fact about the function, not the arithmetic.
                if (!Functions.PartialFractions.HoldsAtSampledPoints(bracket, candidate.Substitute(u, x + Number.Integer.Create(sign) / x), x))
                    continue;
                inU = candidate;
                return true;
            }
            return false;
        }

        /// <summary>The largest degree the reciprocal substitution rewrites; past it the system is not worth building.</summary>
        private const int MaximumReciprocalDegree = 6;

        /// <summary>
        /// The largest degree, in <c>t</c>, of the rational function
        /// <see cref="SolveByEulerSubstitution"/> hands to the rational integrator.
        /// </summary>
        private const int MaximumEulerDegree = 8;

        /// <summary>
        /// Whether <paramref name="expr"/> is <c>x^m sqrt(a + b x^2)^k</c> up to a constant --
        /// the shape the trigonometric substitution answers.
        /// </summary>
        private static bool TryReadAPowerTimesARadicalQuadratic(Entity expr, Entity.Variable x)
        {
            foreach (var (factor, _) in FactorsOfTheIntegrand(expr))
            {
                if (!factor.ContainsNode(x))
                    continue;
                if (factor == x || factor is Powf(var b1, Number.Integer) && b1 == x)
                    continue;
                if (factor is Powf(var b2, Number.Rational) && TreeAnalyzer.TryGetPolyQuadratic(b2, x, out _, out var linear, out _)
                    && linear.Evaled is Number.Complex { IsZero: true })
                    continue;
                return false;
            }
            return true;
        }

        /// <summary>
        /// A product of sines and cosines of <b>different</b> arguments, rewritten as a sum by the
        /// product-to-sum identities and then integrated term by term.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>sin(A)sin(B) = (cos(A - B) - cos(A + B))/2</c>,
        /// <c>cos(A)cos(B) = (cos(A - B) + cos(A + B))/2</c>,
        /// <c>sin(A)cos(B) = (sin(A + B) + sin(A - B))/2</c>. Each application turns two factors
        /// into a sum of two single ones, and a sine or cosine of something linear in the variable
        /// is a rule the integrator already has — so the whole family comes out at once.
        /// <c>sin(x)sin(2x)</c>, <c>cos(x)cos(2x)</c>, <c>cos(3x)sin(2x)</c> and
        /// <c>sin(x)sin(2x)sin(3x)</c> had no antiderivative between them.
        /// </para>
        /// <para>
        /// <b>Why this rather than a substitution.</b> These are the shapes every substitution in
        /// the chain declines, and rightly: there is no inner function to substitute for. A
        /// product of trigonometric functions of unequal arguments is not a function of any one of
        /// them, which is what <see cref="SolveByHalfAngleSubstitution"/> notices when it finds an
        /// <c>x</c> left over after rewriting — <c>sin(2x)</c> is not <c>sin(x)</c>. The identity
        /// is the tool, not a change of variable.
        /// </para>
        /// <para>
        /// <b>It terminates.</b> Each rewrite replaces two trigonometric factors with a sum whose
        /// terms hold one each, so the number of such factors in any one product strictly
        /// decreases, and the recursion is over strictly simpler products. Arguments equal to each
        /// other are left alone, since <c>sin(A)^2</c> is a power rather than a product of two
        /// arguments and wants a reduction formula instead.
        /// </para>
        /// <para>
        /// <b>Nothing is assumed and no condition is owed.</b> These are identities on the whole
        /// complex plane, not rewrites that hold on an interval: both sides are entire, so unlike
        /// every substitution here the answer is an antiderivative everywhere the integrand is,
        /// with no branch to pick and no interval to be inside.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveByProductToSum(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            if (RewriteOneProduct(expr, x) is not { } rewritten)
                return null;
            return Integration.ComputeIndefiniteIntegral(rewritten, x, integrateByParts);
        }

        /// <summary>
        /// The product with one pair of trigonometric factors replaced by the sum it equals, or
        /// <see langword="null"/> where there is no such pair.
        /// </summary>
        private static Entity? RewriteOneProduct(Entity expr, Entity.Variable x)
        {
            var factors = Mulf.LinearChildren(expr).ToList();
            if (factors.Count < 2)
                return null;

            // A whole power of a sine or a cosine is that many factors: Hearn's
            // `cos(x)^2 sin(2x + 3)` is `cos(x) (cos(x) sin(2x + 3))`, and one pair at a time
            // still leaves fewer trigonometric factors than it found.
            static (Entity Base, int Power) Unpowered(Entity factor)
                => factor is Powf(var @base, Number.Integer n) && @base is Sinf or Cosf && n.EInteger.Sign > 0 && n.EInteger.CanFitInInt32()
                    ? (@base, n.EInteger.ToInt32Checked())
                    : (factor, 1);
            for (var i = 0; i < factors.Count; i++)
                for (var j = i + 1; j < factors.Count; j++)
                {
                    var (first, firstPower) = Unpowered(factors[i]);
                    var (second, secondPower) = Unpowered(factors[j]);
                    // Both arguments have to mention the variable. A sine of a constant is a
                    // number as far as this integral is concerned, and pairing it with a real
                    // factor would turn one term into two for nothing.
                    var replacement = (first, second) switch
                    {
                        (Sinf(var a), Sinf(var b)) when a != b && a.ContainsNode(x) && b.ContainsNode(x)
                            => (MathS.Cos(a - b) - MathS.Cos(a + b)) / 2,
                        (Cosf(var a), Cosf(var b)) when a != b && a.ContainsNode(x) && b.ContainsNode(x)
                            => (MathS.Cos(a - b) + MathS.Cos(a + b)) / 2,
                        (Sinf(var a), Cosf(var b)) when a != b && a.ContainsNode(x) && b.ContainsNode(x)
                            => (MathS.Sin(a + b) + MathS.Sin(a - b)) / 2,
                        (Cosf(var a), Sinf(var b)) when a != b && a.ContainsNode(x) && b.ContainsNode(x)
                            => (MathS.Sin(b + a) + MathS.Sin(b - a)) / 2,
                        _ => (Entity?)null
                    };
                    if (replacement is null)
                        continue;

                    var product = replacement;
                    if (firstPower > 1)
                        product *= firstPower == 2 ? first : MathS.Pow(first, firstPower - 1);
                    if (secondPower > 1)
                        product *= secondPower == 2 ? second : MathS.Pow(second, secondPower - 1);
                    for (var k = 0; k < factors.Count; k++)
                        if (k != i && k != j)
                            product *= factors[k];
                    return product;
                }
            return null;
        }

        /// <summary>
        /// A rational function of <c>e^(k x)</c>, turned into a rational function of one variable
        /// by <c>u = e^(k x)</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// With <c>u = e^(k x)</c> we have <c>dx = du/(k u)</c>, and every exponential in the
        /// integrand becomes a whole power of <c>u</c>, so a quotient built from them is a
        /// quotient of polynomials — which the rational integrator answers.
        /// </para>
        /// <para>
        /// <b>What was missing.</b> <c>1/(1 + e^x)</c> had no antiderivative, and neither did
        /// <c>1/(e^x + e^(-x))</c>. <c>e^x/(1 + e^x)</c> did, which is the shape of the gap: the
        /// general substitution answers an exponential integrand only when the numerator happens
        /// to be the derivative of something in it, and declines the rest for want of anything to
        /// substitute for.
        /// </para>
        /// <para>
        /// <b>It is also how the hyperbolic functions get integrated</b>, since they are not nodes
        /// here — <c>tanh(x)</c> is built as <c>(e^(2x) - 1)/(e^(2x) + 1)</c> and <c>sech(x)</c>
        /// as <c>2/(e^x + e^(-x))</c>, so both are rational functions of an exponential, and
        /// <c>tanh</c>, <c>coth</c>, <c>sech</c> and <c>csch</c> had no antiderivative at all.
        /// </para>
        /// <para>
        /// The answers come out in the exponential rather than as <c>ln(cosh(x))</c> or
        /// <c>2 arctan(e^x)</c>, and unfolded — the rational integrator writes a logarithm as
        /// <c>ln((2ax + b - D)/(2ax + b + D))</c> and leaves the arithmetic in the coefficients
        /// standing. That is its shape on plain rational integrands too, not something this
        /// rewrite introduces; <c>1/(x(x+1))</c> comes out as
        /// <c>ln((2x + 1 - 1)/(2x + 1 + 1))</c> with nothing exponential in sight.
        /// </para>
        /// <para>
        /// <b>One <c>k</c> for the whole integrand.</b> The exponents are read as linear in the
        /// variable and their slopes taken together by greatest common divisor, so <c>e^x</c>
        /// beside <c>e^(2x)</c> gives <c>u</c> and <c>u^2</c> under one substitution. A slope that
        /// is not a whole number, or an exponent that is not linear — <c>e^(x^2)</c>, whose
        /// integral is not elementary at all — is declined.
        /// </para>
        /// <para>
        /// <b>No condition is owed.</b> <c>e^(k x)</c> is positive for every real <c>x</c> and
        /// never zero, so the substitution is invertible on the whole line and introduces no
        /// interval of its own; <c>u</c> is a genuine change of variable rather than a branch.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        /// <summary>
        /// A quotient by an exponential, handed on as the product with its reciprocal:
        /// <c>N/b^(f(x))</c> as <c>N * b^(-f(x))</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>sin(x) * e^(-x)</c> had an antiderivative and <c>sin(x)/e^x</c> did not. They are
        /// the same integrand, and the difference is only which node is on top: integration by
        /// parts matches a <c>Mulf</c> and nothing else, so a quotient never reached it. The
        /// simplifier does not turn one into the other — it keeps <c>sin(x)/e^x</c> as written —
        /// so nothing upstream closed the gap either.
        /// </para>
        /// <para>
        /// The family is wider than the trigonometric case that exposed it: <c>x/e^x</c>,
        /// <c>ln(x)/e^x</c> and <c>sin(x)/2^x</c> were all declined for the same reason, and each
        /// is answered once written as a product.
        /// </para>
        /// <para>
        /// <b>Only an exponential denominator</b> — a base free of the variable with the variable
        /// in the exponent. <c>x^n</c> in a denominator is a rational function and belongs to
        /// partial fractions, which reads it as written; turning that into a negative power would
        /// take it away from the rules that answer it. A numerator free of the variable is left
        /// alone too, since <see cref="SolveAsPolynomialTerm"/> already takes that one.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveByDividingByAnExponential(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            if (expr is not Divf(var numerator, var denominator))
                return null;
            if (!numerator.ContainsNode(x))
                return null;
            if (denominator is not Powf(var @base, var power))
                return null;
            if (@base.ContainsNode(x) || !power.ContainsNode(x))
                return null;

            return Integration.ComputeIndefiniteIntegral(
                numerator * MathS.Pow(@base, (-power).InnerSimplified), x, integrateByParts);
        }

        /// <summary>
        /// A polynomial in <c>x</c> times a rational function of exponentials of <c>x</c>,
        /// by parts against the whole rational function: <c>x tanh(x)^2</c> is
        /// <c>x ((e^(2x) - 1)/(e^(2x) + 1))^2</c>, whose antiderivative under <c>u = e^(2x)</c> is
        /// <c>x - tanh(x)</c>, a polynomial and a rational function of the exponential again,
        /// and what parts leaves is <c>x - tanh(x)</c> itself, a degree lower in <c>x</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The general parts rule splits the sum <c>(x + x e^(4x))/(e^(2x) + 1)^2</c> and takes
        /// each term on its own, and each term's antiderivative in <c>u</c> holds a logarithm
        /// of <c>e^(2x) + 1</c> -- the two cancel in the sum and neither on its own -- so each
        /// leaves <c>x ln(e^(2x) + 1)</c> behind, a dilogarithm, and twenty-five seconds of
        /// search that found nothing. Here the rational function goes to the exponential
        /// substitution whole, and where its antiderivative keeps a logarithm of an
        /// exponential's sum the integrand is declined at once: <c>x/(e^x + 1)</c> is not
        /// elementary, and this says so in a millisecond.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        /// <summary>
        /// A power of an exponential of <c>x</c> with a positive constant base, written as the
        /// exponential of the product: <c>(e^x)^(1/3)</c> is <c>e^(x/3)</c> and <c>(3^(3x))^(1/4)</c>
        /// is <c>3^(3x/4)</c>, exactly, since the base is positive; and it is only in that
        /// spelling that the exponential rules read them. Timofeev's
        /// <c>(cos(x/2) + sin(x/2))/(e^x)^(1/3)</c> and <c>cos(3x/2)/(3^(3x))^(1/4)</c>.
        /// </summary>
        internal static Entity? SolveByFlatteningAPowerOfAnExponential(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            var flattened = expr.Replace(node =>
                node is Powf(Powf(var @base, var inner), var outer)
                && !@base.ContainsNode(x) && inner.ContainsNode(x) && !outer.ContainsNode(x)
                && (@base == MathS.e || @base.Evaled is Number.Real { IsPositive: true })
                && outer.Evaled is Number.Real
                    ? MathS.Pow(@base, (outer * inner).InnerSimplified)
                    : node);
            // The same question in another spelling, asked as one of its own, so that the
            // closed rules that answer only at the top -- the exponential times a
            // trigonometric -- are consulted for it.
            return flattened == expr ? null : Integration.ComputeAsAQuestionOfItsOwn(flattened, x, integrateByParts);
        }

        internal static Entity? SolveAPolynomialTimesARationalFunctionOfAnExponential(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            // The polynomial factors above the bar, and the rest, which holds x in exponents only.
            var (above, below) = Functions.SingleQuotient.Of(expr);
            Entity polynomial = Number.Integer.One;
            Entity restAbove = Number.Integer.One;
            foreach (var factor in Mulf.LinearChildren(above))
            {
                if (factor.ContainsNode(x) && !factor.Nodes.Any(node => node is Powf(var b, var e) && b == MathS.e && e.ContainsNode(x))
                    && TreeAnalyzer.TryGetPolynomial(factor, x, out var read) && read.Keys.All(k => k.Sign >= 0) && read.Values.All(c => !c.ContainsNode(x)))
                    polynomial = polynomial * factor;
                else
                    restAbove = restAbove * factor;
            }
            if (!polynomial.ContainsNode(x))
                return null;
            var rest = (restAbove / below).InnerSimplified;
            // A rational function, with the exponential below the bar somewhere: a polynomial
            // times an exponential alone is parts', and keeps its form there.
            if (!rest.ContainsNode(x) || !rest.Nodes.Any(node => node is Divf(_, var d) && d.ContainsNode(x) || node is Powf(var b, Number.Integer { IsNegative: true }) && b.ContainsNode(x)))
                return null;
            var placeholder = Variable.CreateUnique(expr, "u_exp");
            var withoutExponentials = rest.Replace(node =>
                node is Powf(var b, var e) && b == MathS.e && e.ContainsNode(x) && TreeAnalyzer.TryGetPolyLinear(e, x, out _, out _) ? placeholder : node);
            if (withoutExponentials.ContainsNode(x) || withoutExponentials.Nodes.Any(node => node is Powf(_, Number.Rational r) && r is not Number.Integer && node.ContainsNode(placeholder)))
                return null;

            if (SolveByExponentialSubstitution(rest, x, integrateByParts: false) is not { } antiderivative)
                return null;
            // ln(e^(k x)) is k x; any other logarithm of the exponential's sums makes the next
            // step a dilogarithm.
            antiderivative = Functions.PartialFractions.Bare(antiderivative.Replace(node =>
                node is Logf(var @base, var argument) && @base == MathS.e && argument is Powf(var b, var e) && b == MathS.e ? e : node));
            if (antiderivative.Nodes.Any(node => node is Logf && node.ContainsNode(x)))
                return null;
            var derivative = polynomial.Differentiate(x).InnerSimplified;
            if (derivative.Evaled is Number.Complex { IsZero: true })
                return polynomial * antiderivative;
            var remainder = Functions.PartialFractions.Bare((derivative * antiderivative).InnerSimplified);
            if (Integration.ComputeIndefiniteIntegral(remainder, x, integrateByParts) is not { } integrated)
                return null;
            var answer = polynomial * antiderivative - integrated;
            return answer.Nodes.Any(node => node == MathS.NaN) ? null : answer;
        }

        internal static Entity? SolveByExponentialSubstitution(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            // Rational slopes, so that `e^(x/2)` beside `e^x` is read: the base is `e^(k x)` with
            // `k` the greatest common divisor of the slopes, and every exponential is a whole
            // power of it. `e^(x/2)/sqrt(e^x - 1)` is `2/sqrt(u^2 - 1)` that way, and was declined
            // for the half.
            // Any one base free of x, not only e: Timofeev's `1/sqrt(a^(2x) - 1)` is
            // `1/(u ln(a) sqrt(u^2 - 1))` under `u = a^x`, with `dx = du/(u ln a)`.
            // Numeric bases that are whole powers of one base are written in it first:
            // Rubi's `2^x/sqrt(a + b/4^x)` is `2^x/sqrt(a + b/2^(2x))`, and rational in `u = 2^x`.
            expr = WithNumericBasesUnified(expr, x);
            // And the slopes need only be rational multiples of one another: with a symbol
            // for the slope, `F^(c + d x)` beside `F^(2c + 2dx)`, the base is `F^(d x)` and
            // `dx = du/(d u ln F)`. Rubi's `F^(c + d x) x/(a + b F^(c + d x))^2`.
            var slopes = new List<ERational>();
            var offsets = new Dictionary<Entity, (ERational Slope, Entity Offset)>();
            var underARadical = new HashSet<Entity>();
            Entity? commonBase = null;
            Entity? slopeUnit = null;
            // The first exponential's slope and offset, and whether every other's offset is
            // in the same ratio to its slope: then every exponent is a multiple of one
            // `x + shift`, and u is a power of the base at that rather than at x. Rubi's
            // `csch(29/10 + 13/10 x)^3 (17/10 + 23/10 sech(29/10 + 13/10 x)^2)^3` is a
            // rational function of `u = e^(13/10 x + 29/10)` with numbers for its
            // coefficients, where `u = e^(13/10 x)` left `e^(29/10)` in every one of them,
            // a quadratic the rational rules do not factor over, and each term seconds.
            // Compared cross-multiplied, so that a symbolic slope is never divided by.
            (ERational Slope, Entity Offset)? first = null;
            var oneShift = true;
            foreach (var node in expr.Nodes)
            {
                if (node is Powf(_, Number.Rational fractional) && fractional is not Number.Integer)
                    foreach (var inside in node.Nodes)
                        underARadical.Add(inside);
                if (node is not Powf(var @base, var exponent) || @base.ContainsNode(x) || exponent is Number)
                    continue;
                if (!exponent.ContainsNode(x))
                    continue;
                if (commonBase is null)
                    commonBase = @base;
                else if (commonBase != @base)
                    return null;
                if (!TreeAnalyzer.TryGetPolyLinear(exponent, x, out var slope, out var offset))
                    return null;   // not linear in x, so not a power of one exponential
                ERational multiple;
                if (slope.Evaled is Number.Rational rational)
                {
                    if (rational.ERational.IsZero || slopeUnit is not null)
                        return null;
                    multiple = rational.ERational;
                }
                else
                {
                    if (slopes.Count > 0 && slopeUnit is null)
                        return null;
                    slopeUnit ??= slope;
                    if (Functions.PartialFractions.Bare((slope / slopeUnit).Simplify()).Evaled is not Number.Rational ratio || ratio.ERational.IsZero)
                        return null;
                    multiple = ratio.ERational;
                }
                slopes.Add(multiple);
                offsets[node] = (multiple, offset);
                if (first is null)
                    first = (multiple, offset);
                else if (oneShift && !IsTheZeroPolynomial(offset * Number.Rational.Create(first.Value.Slope) - first.Value.Offset * Number.Rational.Create(multiple)))
                    oneShift = false;
            }
            if (slopes.Count == 0 || commonBase is null)
                return null;
            if (commonBase != MathS.e && (commonBase.Evaled is Number.Complex and not Number.Real || commonBase.Evaled is Number.Real { IsNegative: true } || TreeAnalyzer.IsZero(commonBase)))
                return null;   // a base that is not a positive real is not an exponential of the kind substituted for
            var logarithmOfTheBase = commonBase == MathS.e ? Number.Integer.One : MathS.Ln(commonBase);
            if (slopeUnit is not null)
                logarithmOfTheBase = (logarithmOfTheBase * slopeUnit).InnerSimplified;

            var numerators = EInteger.Zero;
            var denominators = EInteger.One;
            foreach (var slope in slopes)
            {
                numerators = numerators.Gcd(slope.Numerator.Abs());
                denominators = denominators.Multiply(slope.Denominator).Divide(denominators.Gcd(slope.Denominator));
            }
            if (numerators.IsZero)
                return null;
            var k = ERational.Create(numerators, denominators);
            // The sign of the base is chosen for the radicals: with every exponential under a
            // root of negative slope, `u = e^(-x)` makes `sqrt(1 + e^(-x))` into `sqrt(1 + u)`,
            // where `u = e^x` would make it `sqrt(1 + 1/u)`, a root of a quotient that nothing
            // rationalises. `u` is positive either way, since it is an exponential.
            var slopesUnderRoots = offsets.Where(pair => underARadical.Contains(pair.Key)).Select(pair => pair.Value.Slope).ToList();
            if (slopesUnderRoots.Count > 0 && slopesUnderRoots.All(slope => slope.Sign < 0))
                k = k.Negate();

            var u = Variable.CreateUnique(expr, "u_exp");

            // e^(k_i x + m_i) is e^(m_i) times u^(k_i/k), and k_i/k is a whole number by
            // construction. Built directly rather than by substituting x and simplifying, for the
            // same reason as the radical substitutions: (u^(1/k))^(k_i) is not something the
            // simplifier will reduce, and is right not to.
            // With one shift, e^(k_i x + m_i) is u^(k_i/k) exactly for
            // u = e^(k x + k m_1/k_1), and no e^(m_i) is left in the coefficients.
            var shifted = oneShift && first is { } firstOne && !TreeAnalyzer.IsZero(firstOne.Offset) && !firstOne.Offset.ContainsNode(x);
            var shiftOfU = shifted ? (Number.Rational.Create(k.Divide(first!.Value.Slope)) * first.Value.Offset).InnerSimplified : Number.Integer.Zero;
            var rewritten = expr.Replace(node =>
                offsets.TryGetValue(node, out var found)
                    ? (shifted ? Number.Integer.One : MathS.Pow(commonBase, found.Offset))
                      * MathS.Pow(u, Number.Integer.Create(found.Slope.Divide(k).ToLowestTerms().Numerator))
                    : node);

            if (rewritten.ContainsNode(x))
                return null;

            // dx = du/(k u). Combined into one quotient and then simplified, in that order, and
            // the order decides four of these. Combine does not cancel, so 1/(e^x + e^(-x)) leaves
            // u/(u(u^2 + 1)) -- which the rational integrator declines although it answers
            // 1/(u^2 + 1) at once. Simplifying first instead leaves the nesting for Combine to
            // flatten and the common factor never meets a cancellation.
            // The logarithm of the base stays outside: simplified into the quotient,
            // `1/((a + b u)^2 ln F)` is a quadratic below the bar with `ln F` in every
            // coefficient, and the symbolic quadratic rule answers that as a piecewise on
            // whether `b^2 ln F` is zero, where `1/(a + b u)^2` is `-1/(b (a + b u))`.
            var integrand = Functions.SingleQuotient.Combine(
                rewritten / (Number.Rational.Create(k) * u)).Simplify();
            if (integrand is Providedf(var inner, _))
                integrand = inner;
            // A whole power of a product is written as the product of the powers: the
            // simplifier writes `4a^2 u^2` as `(a u)^2 4`, and no rational reader sees the `u^2`
            // inside -- Timofeev's `1/(a^2 + b^2 cosh(x)^2)` was declined in that spelling and
            // is answered in the other.
            for (var round = 0; round < 4; round++)
            {
                var distributed = integrand.Replace(node =>
                    node is Powf(Mulf(var l, var r), Number.Integer power) ? MathS.Pow(l, power) * MathS.Pow(r, power) : node);
                if (distributed == integrand)
                    break;
                integrand = distributed;
            }
            // u is an exponential, so it is positive, and a root holding a power of it gives
            // that power up: `sqrt(1 + tanh(4x))` is a root of `2u/(1 + u)` here and nothing
            // rationalises that; as `sqrt(2) sqrt(u)/sqrt(1 + u)` it is one substitution more.
            integrand = FactorANonnegativeVariableOutOfRadicals(integrand, u);

            // What that leaves can be two square roots of linears in u -- `sqrt(u)/(u sqrt(1 + u))`
            // for `sqrt(1 + tanh(4x))` -- which the linear-radical substitution answers only
            // when told that u is not negative, as an exponential is; asked directly, so that
            // it is told.
            if (SolveByLinearRadicalSubstitution(integrand, u, integrateByParts, variableIsNonnegative: true) is { } byARoot)
                return Finished(byARoot);
            // The reciprocal substitution, told the same: a hyperbolic function under a root
            // is a palindromic quartic under it here, and `u - 1/u` is `2 sinh(x)`. Under a
            // root only: a rational function of u is the rational rules' below, in one closed
            // step, and `cosh/sinh^4` -- `8u (u^2 + 1)/(u^2 - 1)^4` -- came back from here as
            // a reduction over `1/(u^2 - 1)^k` with a logarithm at the bottom, which is
            // `-1/(3 sinh^3)` after four seconds of simplifying the product it stands in.
            if (HasARadicalOf(integrand, u) && SolveByReciprocalSubstitution(integrand, u, variableIsPositive: true) is { } byTheReciprocal)
                return Finished(byTheReciprocal);
            // The same question in u, not a step in the search for it: asked at the top when
            // this was, so the rules that answer only there -- the symbolic quadratic
            // denominator, a piecewise on its discriminant -- are consulted. Timofeev's
            // `1/(a^2 + b^2 cosh(x)^2)` is `4u/(4a^2 u^2 + b^2 (u^2 + 1)^2)`, answered at the top
            // and declined one level down.
            if (Integration.ComputeAsAQuestionOfItsOwn(integrand, u, integrateByParts) is not { } result)
                return null;
            return Finished(result);

            Entity? Finished(Entity result)
            {
                var rate = slopeUnit is null ? Number.Rational.Create(k)
                    : Functions.PartialFractions.Bare((Number.Rational.Create(k) * slopeUnit).Simplify());
                var answer = result.Substitute(u, MathS.Pow(commonBase, (shifted ? rate * x + shiftOfU : rate * x).InnerSimplified));
                if (logarithmOfTheBase != Number.Integer.One)
                    answer /= logarithmOfTheBase;
                return answer.Nodes.Any(node => node == MathS.NaN) ? null : answer;
            }
        }

        /// <summary>
        /// A product with a whole negative power of a polynomial in <paramref name="x"/> of
        /// two or more terms among its factors, with every such power written below the bar
        /// and asked again as a question of its own: <c>u^2 (a u^2 + b)^(-3)</c> as
        /// <c>u^2/(a u^2 + b)^3</c>. The gathering of powers on the way into the chain writes
        /// <c>u^3/((a u^2 + b)^3 u)</c> -- the exponential substitution's quotient for Rubi's
        /// <c>1/(b/f^x + a f^x)^3</c> -- in the first spelling, a product with a power in it,
        /// which no rational rule reads, where the second is a quotient every one of them
        /// reads; the symbolic quadratic was declined for it. A power of the variable alone
        /// is left as it is, since that spelling is read, and <see cref="SolveAsPolynomialTerm"/>
        /// writes <c>1/x^n</c> back into it; and a lone power, <c>(a u^2 + b)^(-3)</c>, is the
        /// closed rules' as written. Where the quotient was the question one level up -- the
        /// constant-over-a-power branch of <see cref="SolveAsPolynomialTerm"/> asks the power
        /// -- the cycle guard declines it and the chain goes on. A rational function of
        /// <paramref name="x"/> only: the rewrite is another pass through the chain, and for
        /// anything else it was a search that found nothing the spelling had hidden.
        /// </summary>
        internal static Entity? SolveWithPolynomialPowersBelowTheBar(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            if (expr is not (Mulf or Divf) || !IsARationalFunction(expr, x))
                return null;
            var rewritten = WithPolynomialPowersBelowTheBar(expr, x);
            return rewritten == expr ? null : Integration.ComputeAsAQuestionOfItsOwn(rewritten, x, integrateByParts);
        }

        /// <summary>
        /// Powers of two linears beside a power of <c>A + B ln(K (L1/L2)^n)</c> in the same
        /// two linears, by the substitution <c>t = L1/L2</c>: with <c>L1 = a + b x</c> and
        /// <c>L2 = c + d x</c>, <c>L1 = D t/(b - d t)</c>, <c>L2 = D/(b - d t)</c> and
        /// <c>dx = D dt/(b - d t)^2</c> for <c>D = b c - a d</c>, so the integrand is a power
        /// of <c>t</c> times a power of <c>b - d t</c> times a power of <c>A + B ln(K t^n)</c>,
        /// which the rule above answers as a rational function beside a logarithm of
        /// <c>t</c>. Rubi's <c>(f + g x)^m (h + i x)^q (A + B ln(e ((a + b x)/(c + d x))^n))^p</c>
        /// with <c>f + g x</c> and <c>h + i x</c> proportional to the log's linears, which
        /// the rule above takes step by step and the substitution search spent its budget on:
        /// <c>(A + B ln(e (a + b x)/(c + d x)))/((a g + b g x)^2 (c j + d j x)^2)</c> is
        /// <c>(A + B ln(e t))(b - d t)^2/(g^2 j^2 D^3 t^2)</c>, which answers at once. The
        /// exponents may be symbols; <c>(g L1)^m</c> is taken as <c>g^m L1^m</c> and
        /// <c>K L1^n/L2^n</c> as <c>K t^n</c>, the generic case, and the answer is checked
        /// against the integrand at sampled points before it is given.
        /// </summary>
        internal static Entity? SolveByTheQuotientOfTheLogarithmsLinears(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            if (!Integration.AnsweringTheQuestionAskedOrOneBelow)
                return null;
            expr = WithNegativePowersOfProductsApart(expr);
            // The logarithm's argument: a product of constants and powers of two linears with
            // opposite exponents.
            Entity? logarithm = null;
            foreach (var node in expr.Nodes)
                if (node is Logf(var @base, var antilogarithm) && @base == MathS.e && antilogarithm.ContainsNode(x))
                {
                    if (logarithm is not null && logarithm != antilogarithm)
                        return null;
                    logarithm = antilogarithm;
                }
            if (logarithm is null)
                return null;
            var leaves = new List<(Entity Base, Entity Power)>();
            GatherPowers(logarithm, Number.Integer.One);
            Entity constant = Number.Integer.One;
            (Entity Slope, Entity Offset, Entity Linear)? first = null, second = null;
            Entity firstPower = Number.Integer.Zero, secondPower = Number.Integer.Zero;
            foreach (var (leaf, power) in leaves)
            {
                if (!leaf.ContainsNode(x))
                {
                    constant *= MathS.Pow(leaf, power);
                    continue;
                }
                if (!TryReadLinear(leaf, out var slope, out var offset))
                    return null;
                if (first is null)
                {
                    first = (slope, offset, leaf);
                    firstPower = power;
                }
                else if (ProportionalTo(slope, offset, first.Value) is { } ratio)
                {
                    constant *= MathS.Pow(ratio, power);
                    firstPower += power;
                }
                else if (second is null)
                {
                    second = (slope, offset, leaf);
                    secondPower = power;
                }
                else if (ProportionalTo(slope, offset, second.Value) is { } secondRatio)
                {
                    constant *= MathS.Pow(secondRatio, power);
                    secondPower += power;
                }
                else
                    return null;
            }
            if (first is not { } l1 || second is not { } l2)
                return null;
            var exponent = firstPower.InnerSimplified;
            if (exponent.Evaled is Number.Complex { IsZero: true } || !AreEqualAsPolynomials(exponent, (-secondPower).InnerSimplified))
                return null;
            var (a, b, c, d) = (l1.Offset, l1.Slope, l2.Offset, l2.Slope);
            var determinant = (b * c - a * d).InnerSimplified;
            if (determinant.Evaled is Number.Complex { IsZero: true } || AreProportionalAtSampledPoints(l1.Linear, l2.Linear, x))
                return null;

            // Every other factor holding x is a power of a linear proportional to one of the
            // two, or the logarithm's own factor. The powers of t and of b - d t are summed,
            // so that the integrand in t is one power of each over or under the bar: with
            // `(b - d t)^2` above and `(b - d t)^(-4)` below as they came, the rule for the
            // logarithm expanded its rational part to a page.
            var t = Variable.CreateUnique(expr, "u_quot");
            var denominatorInT = (b - d * t).InnerSimplified;
            Entity constantInFront = determinant;
            Entity powerOfT = Number.Integer.Zero;
            Entity powerOfDenominator = Number.Integer.Create(-2);   // dx = D dt/(b - d t)^2
            Entity? logarithmsFactor = null;
            // A polynomial beside the logarithm is by parts in x as it stands, one closed
            // step; in t it is a power of b - d t below the bar, which is the slower way.
            var aPolynomialBesideTheLogarithm = true;
            foreach (var (factor, underneath) in FactorsOfTheIntegrand(expr))
            {
                if (!factor.ContainsNode(x))
                {
                    constantInFront = underneath ? constantInFront / factor : constantInFront * factor;
                    continue;
                }
                var (@base, power) = factor is Powf(var pb, var pp) && !pp.ContainsNode(x) ? (pb, pp) : (factor, (Entity)Number.Integer.One);
                if (underneath)
                    power = (-power).InnerSimplified;
                if (!@base.ContainsNode(logarithm) && power.Evaled is not Number.Integer { EInteger.Sign: >= 0 })
                    aPolynomialBesideTheLogarithm = false;
                if (@base.ContainsNode(logarithm))
                {
                    if (logarithmsFactor is not null)
                        return null;
                    var inT = @base.Replace(node => node == logarithm ? constant * MathS.Pow(t, exponent) : node);
                    if (inT.ContainsNode(x))
                        return null;
                    logarithmsFactor = MathS.Pow(inT, power);
                    continue;
                }
                if (!TryReadLinear(@base, out var slope, out var offset))
                    return null;
                if (ProportionalTo(slope, offset, l1) is { } ratio1)
                {
                    constantInFront *= MathS.Pow(ratio1 * determinant, power);
                    powerOfT += power;
                    powerOfDenominator -= power;
                }
                else if (ProportionalTo(slope, offset, l2) is { } ratio2)
                {
                    constantInFront *= MathS.Pow(ratio2 * determinant, power);
                    powerOfDenominator -= power;
                }
                else
                    return null;
            }
            if (logarithmsFactor is null || aPolynomialBesideTheLogarithm)
                return null;
            Entity above = constantInFront * logarithmsFactor, below = Number.Integer.One;
            foreach (var (@base, power) in new[] { (t, Functions.PartialFractions.Bare(powerOfT.Simplify())), (denominatorInT, Functions.PartialFractions.Bare(powerOfDenominator.Simplify())) })
            {
                if (power.Evaled is Number.Complex { IsZero: true })
                    continue;
                if (power.Evaled is Number.Real { IsNegative: true })
                    below *= MathS.Pow(@base, (-power).InnerSimplified);
                else
                    above *= MathS.Pow(@base, power);
            }
            var rewritten = (below == Number.Integer.One ? above : above / below).InnerSimplified;
            if (rewritten.ContainsNode(x))
                return null;
            if (Integration.ComputeAsAQuestionOfItsOwn(rewritten, t, integrateByParts) is not { } integrated)
                return null;
            var answer = integrated.Substitute(t, l1.Linear / l2.Linear);
            if (answer.Nodes.Any(node => node == MathS.NaN) || !Functions.PartialFractions.HoldsAtSampledPoints(answer.Differentiate(x), expr, x))
                return null;
            return answer;

            void GatherPowers(Entity node, Entity power)
            {
                switch (node)
                {
                    case Mulf(var left, var right):
                        GatherPowers(left, power);
                        GatherPowers(right, power);
                        break;
                    case Divf(var numerator, var denominator):
                        GatherPowers(numerator, power);
                        GatherPowers(denominator, (-power).InnerSimplified);
                        break;
                    case Powf(var @base, var exponentOfPower) when !exponentOfPower.ContainsNode(x):
                        GatherPowers(@base, (power * exponentOfPower).InnerSimplified);
                        break;
                    default:
                        leaves.Add((node, power));
                        break;
                }
            }

            // Linear in x in whatever spelling the chain gave it: `a g^0 b^(-1) + x`, for
            // `(a g + b g x)` made monic, is not read as a polynomial, and is read by its
            // derivative.
            bool TryReadLinear(Entity linear, out Entity slope, out Entity offset)
            {
                if (TreeAnalyzer.TryGetPolyLinear(linear, x, out var readSlope, out var readOffset) && readSlope is { } && readOffset is { })
                {
                    (slope, offset) = (readSlope, readOffset);
                    return !TreeAnalyzer.IsZero(slope);
                }
                slope = Functions.PartialFractions.Bare(linear.Differentiate(x).Simplify());
                offset = Functions.PartialFractions.Bare((linear - slope * x).Simplify());
                return !slope.ContainsNode(x) && !offset.ContainsNode(x) && !TreeAnalyzer.IsZero(slope)
                    && slope.Evaled is not Number.Complex { IsZero: true };
            }

            // The constant `slope/other.Slope` where the linear is that multiple of the
            // other, else null: cross-multiplied, so that no symbol is divided by.
            Entity? ProportionalTo(Entity slope, Entity offset, (Entity Slope, Entity Offset, Entity Linear) other)
            {
                // Decided at sampled points, since the linear arrives in whatever spelling the
                // chain gave it -- `a g^0 b^(-1) + x` for `(a g + b g x)` made monic -- and
                // then the ratio read exactly.
                if (!AreEqualAsPolynomials((offset * other.Slope).InnerSimplified, (other.Offset * slope).InnerSimplified)
                    && !AreProportionalAtSampledPoints(slope * x + offset, other.Linear, x))
                    return null;
                var ratio = Functions.PartialFractions.Bare((slope / other.Slope).Simplify());
                return ratio.Nodes.Any(node => node == MathS.NaN) || ratio.ContainsNode(x) ? null : ratio;
            }

            // Equal as polynomials in their symbols, exactly.
            static bool AreEqualAsPolynomials(Entity left, Entity right)
            {
                if (left == right)
                    return true;
                var difference = Functions.PartialFractions.Bare((left - right).Simplify());
                if (difference.Evaled is Number.Complex { IsZero: true })
                    return true;
                var symbols = difference.Vars.ToList();
                if (symbols.Count == 0)
                    return false;
                var indices = new Dictionary<Variable, int>();
                foreach (var symbol in symbols)
                    indices[symbol] = indices.Count;
                return Functions.MultivariatePolynomial.TryParse(difference, indices) is { IsZero: true };
            }
        }

        /// <summary>
        /// A rational function of <paramref name="x"/> times a whole power of a factor whose
        /// derivative is rational -- <c>A + B ln(R)</c> or <c>A + B arctan(R)</c> with <c>R</c>
        /// rational in <paramref name="x"/> -- by parts with the power differentiated:
        /// <c>F^p Q = (F^p int Q)' - p F^(p-1) F' int Q</c>, and the remainder is a rational
        /// function times the next lower power, rational outright for the first. Rubi's
        /// <c>(f + g x)^m (A + B ln(e ((a + b x)/(c + d x))^n))^p</c> for whole <c>m</c> and
        /// <c>p</c>, of which <c>(f + g x)(A + B ln(e (a + b x)^2/(c + d x)^2))</c> and
        /// <c>(A + B ln(e (a + b x)/(c + d x)))/(f + g x)^5</c> were a substitution search past
        /// its budget on the symbols, simplifying the integrand over each candidate's
        /// derivative, before the same step of parts was reached below it.
        /// </summary>
        /// <remarks>
        /// The step is exact and closed: what is integrated is rational, what is left is
        /// asked as a question of its own with the power one lower, and the power is what
        /// the recursion decreases on. At any depth where the rational function holds
        /// <c>x</c>, since the step is closed and its own remainder is two levels down once
        /// a constant has been taken out in front; against a constant alone it is by parts
        /// against one, asked at the top or one below it like the rest of by parts.
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveARationalFunctionTimesAPowerOfALogarithm(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            // A whole negative power of a product is the product of the powers: the constant
            // over a product is handed on as `((a + b x)(c + d x)(A + B ln(...)))^(-1)`, one
            // factor with everything in it.
            expr = WithNegativePowersOfProductsApart(expr);
            Entity? differentiated = null;
            Entity writtenPower = Number.Integer.One;
            Entity rational = Number.Integer.One;
            foreach (var (factor, underneath) in FactorsOfTheIntegrand(expr))
            {
                if (!factor.ContainsNode(x) || IsARationalFunction(factor, x))
                {
                    rational = underneath ? rational / factor : rational * factor;
                    continue;
                }
                if (differentiated is not null)
                    return null;
                var (@base, exponent) = factor is Powf(var b, var p) && !p.ContainsNode(x) ? (b, p) : (factor, Number.Integer.One);
                if (!@base.Nodes.Any(node => node is Logf or Arctanf or Arccotanf && node.ContainsNode(x)))
                    return null;
                differentiated = @base;
                writtenPower = underneath ? (-exponent).InnerSimplified : exponent;
            }
            if (differentiated is null)
                return null;
            // Against a constant alone this is by parts against one, which is asked at the
            // top or one below and not volunteered: offered at every depth, `ln(x)^2` was
            // answered inside a by-parts search that its decline used to cut short, and
            // Bronstein's `(x^2 + 2x ln x + ln^2 x + (1 + x) sqrt(x + ln x))/(x (x + ln x)^2)`
            // went from a second to twelve.
            if (!rational.ContainsNode(x) && !Integration.AnsweringTheQuestionAskedOrOneBelow)
                return null;
            // Differentiated with each logarithm of a product written as the sum of the
            // logarithms of its factors, the constant dropped: the derivative of
            // `ln(e (a + b x)^2/(c + d x)^2)` is then `2b/(a + b x) - 2d/(c + d x)`, where the
            // quotient rule on the argument as written gives a quotient of quartics that the
            // partial fractions below spend their budget on.
            // And term by term, each term of the sum differentiated on its own: simplified
            // whole, `b/(a + b x) - d/(c + d x)` is brought over one bar, and the pieces below
            // then hold a quadratic with symbols in it.
            var derivativeTerms = new List<Entity>();
            foreach (var term in TermsDistributed(WithLogarithmsOfProductsApart(differentiated, x)))
            {
                var termDerivative = Functions.PartialFractions.Bare(term.Differentiate(x).InnerSimplified);
                if (termDerivative == Number.Integer.Zero || termDerivative.Evaled is Number.Complex { IsZero: true })
                    continue;
                if (!IsARationalFunction(termDerivative, x))
                    return null;
                derivativeTerms.AddRange(TermsDistributed(termDerivative));
            }
            if (derivativeTerms.Count == 0)
                return null;
            // The rational function a constant multiple of the derivative: `F^p F'` for any
            // power, `F^(p + 1)/(p + 1)`, or `ln F` for the reciprocal. Rubi's
            // `(A + B ln(e (a + b x)^n/(c + d x)^n))/((a + b x)(c + d x))` is
            // `(A + B ln(...))^2/(2 B n (b c - a d))`, which the substitution search spent its
            // budget simplifying towards. Decided at sampled points, then the constant read
            // exactly and the answer differentiated back.
            Entity derivative = Number.Integer.Zero;
            foreach (var term in derivativeTerms)
                derivative += term;
            if (AreProportionalAtSampledPoints(rational, derivative, x))
            {
                var (above, below) = Functions.SingleQuotient.Of(Functions.SingleQuotient.Combine(rational / derivative).InnerSimplified);
                if (TryReadTheRatioOfBrackets(above, below, x, out var ratio) && !ratio.ContainsNode(x))
                {
                    var nextPower = (writtenPower + 1).InnerSimplified;
                    var reciprocal = nextPower.Evaled is Number.Complex { IsZero: true };
                    // Checked with the plain logarithm, whose derivative evaluates at the
                    // points; given with the modulus where the codomain asks for it, as
                    // the table gives `ln|f|` for `f'/f`.
                    var primitive = reciprocal ? ratio * MathS.Ln(differentiated) : ratio * MathS.Pow(differentiated, nextPower) / nextPower;
                    if (Functions.PartialFractions.HoldsAtSampledPoints(primitive.Differentiate(x), expr, x))
                        return (reciprocal ? ratio * IntegralPatterns.AntiderivativeLog(differentiated) : primitive).InnerSimplified;
                }
            }
            if (writtenPower is not Number.Integer { EInteger.Sign: > 0 } wholePower || wholePower.EInteger.CompareTo(EInteger.FromInt32(8)) > 0)
                return null;
            var power = wholePower.EInteger.ToInt32Checked();
            if (Integration.ComputeIndefiniteIntegral(rational, x, integrateByParts: false) is not { } integralOfTheRational)
                return null;
            integralOfTheRational = Functions.PartialFractions.Bare(integralOfTheRational.InnerSimplified);
            if (!IsARationalFunction(integralOfTheRational, x))
                return null;   // a logarithm from the rational part: the remainder would hold two
            // The antiderivative of the rational function is fixed up to a constant, and the
            // constant is chosen so that it vanishes at a pole of the derivative, which the
            // remainder then does not have: for `(A + B ln((a + b x)/(c + d x)))^2/(c + d x)^2`
            // the antiderivative of `1/(c + d x)^2` is `(a + b x)/((b c - a d)(c + d x))` and
            // not `-1/(d (c + d x))`, and the remainder is `(A + B ln(...))/(c + d x)^2`, one
            // step more of the same, where with the other constant it held
            // `(A + B ln(...))/(a + b x)`, which is a dilogarithm on its own. Rubi's constant.
            integralOfTheRational = MatchedToAPoleOf(integralOfTheRational, derivativeTerms, x);
            // Term by term over the sum the logarithmic derivative is, each term a rational
            // function over one factor: brought over one bar, `x (b c - a d)/((a + b x)(c + d x))`
            // was a quadratic below the bar with symbols in it, answered as a piecewise on
            // its discriminant, where `b x/(a + b x)` and `d x/(c + d x)` are each a line.
            // For a higher power the derivative stays whole: the pole the antiderivative was
            // matched to cancels only against the derivative as one fraction,
            // `n (b c - a d)/((a + b x)(c + d x))` times `(a + b x)/((b c - a d)(c + d x))`, and
            // the pieces of the split each keep a logarithm of their own.
            // Each piece as its partial fractions over the written linear factors, whole part
            // apart: the pole the antiderivative was matched to has a zero residue and drops
            // out, and every piece is a constant over one power of one factor, which is a
            // line to integrate beside the power of the logarithm. Combined and cancelled
            // instead, the product kept the pole's factor in the numerator, `-d^2 (a + b x)`
            // over `(a + b x)(c + d x)^2`, and sent the piece down the substitution search.
            // For a higher power the fractions of all the terms are collected first, like
            // over like: the `1/(c + d x)` fractions of the two terms cancel between them,
            // and each on its own beside the power of the logarithm is a dilogarithm.
            var remainders = new List<Entity>();
            if (power == 1)
                foreach (var term in derivativeTerms)
                    remainders.Add(AsPartialFractions(term * integralOfTheRational, x));
            else
            {
                // Keyed by the part of the denominator holding x; the constants go with the
                // coefficient, which is then a quotient of polynomials in the symbols and is
                // put in lowest terms -- `d^10 (a d - b c)^3` over `d^8 (a d - b c)^2` is
                // `d^2 (a d - b c)` -- since the pieces come from the derivatives at the roots
                // with nothing cancelled.
                var collected = new List<(Entity Below, Entity Above)>();
                foreach (var term in derivativeTerms)
                    foreach (var fraction in TermsDistributed(AsPartialFractions(term * integralOfTheRational, x)))
                    {
                        var (fractionAbove, fractionBelow) = Functions.SingleQuotient.Of(fraction);
                        Entity holdingX = Number.Integer.One, constantBelow = Number.Integer.One;
                        foreach (var factor in Mulf.LinearChildren(fractionBelow))
                            if (factor.ContainsNode(x))
                                holdingX = holdingX == Number.Integer.One ? factor : holdingX * factor;
                            else
                                constantBelow = constantBelow == Number.Integer.One ? factor : constantBelow * factor;
                        var coefficientPart = constantBelow == Number.Integer.One ? fractionAbove : fractionAbove / constantBelow;
                        var at = collected.FindIndex(pair => pair.Below == holdingX);
                        if (at >= 0)
                            collected[at] = (holdingX, collected[at].Above + coefficientPart);
                        else
                            collected.Add((holdingX, coefficientPart));
                    }
                foreach (var (fractionBelow, fractionAbove) in collected)
                {
                    var coefficient = Functions.PartialFractions.InLowestTermsOverTheSymbols(fractionAbove);
                    if (coefficient == Number.Integer.Zero || coefficient.Evaled is Number.Complex { IsZero: true })
                        continue;
                    var piece = fractionBelow == Number.Integer.One ? coefficient : coefficient / fractionBelow;
                    remainders.Add(Number.Integer.Create(power) * MathS.Pow(differentiated, power - 1) * piece);
                }
            }
            Entity? integratedRemainder = null;
            foreach (var remainder in remainders)
            {
                if (Integration.ComputeAsAQuestionOfItsOwn(remainder, x, integrateByParts) is not { } integratedPiece)
                    return null;
                integratedRemainder = integratedRemainder is null ? integratedPiece : integratedRemainder + integratedPiece;
            }
            if (integratedRemainder is null)
                return null;
            var answer = MathS.Pow(differentiated, power) * integralOfTheRational - integratedRemainder;
            return answer.Nodes.Any(node => node == MathS.NaN) ? null : answer;
        }

        /// <summary>
        /// <paramref name="expr"/> with every whole negative power of a product among its
        /// factors written as the product of the powers: <c>(X Y)^(-2)</c> is
        /// <c>X^(-2) Y^(-2)</c>.
        /// </summary>
        private static Entity WithNegativePowersOfProductsApart(Entity expr)
        {
            static Entity Distributed(Entity inner, Number.Integer exponent)
            {
                var (above, below) = Functions.SingleQuotient.Of(inner);
                Entity result = Number.Integer.One;
                foreach (var factor in Mulf.LinearChildren(above))
                    result = result == Number.Integer.One ? MathS.Pow(factor, exponent) : result * MathS.Pow(factor, exponent);
                if (below != Number.Integer.One)
                    foreach (var factor in Mulf.LinearChildren(below))
                        result = result * MathS.Pow(factor, Number.Integer.Create(-exponent.EInteger));
                return result;
            }
            Entity Apart(Entity node) => node switch
            {
                Mulf(var left, var right) => Apart(left) * Apart(right),
                Divf(var above, var below) => Apart(above) / Apart(below),
                Powf(var inner, Number.Integer { EInteger.Sign: < 0 } exponent) when inner is Mulf or Divf => Distributed(inner, exponent),
                _ => node,
            };
            return Apart(expr);
        }

        /// <summary>
        /// <paramref name="product"/> brought over one bar and written as its whole part plus
        /// its partial fractions over the written linear factors of the denominator, the
        /// symbolic ones by the derivatives at the roots; <paramref name="product"/> over one
        /// bar as it came where the denominator is not such a product.
        /// </summary>
        private static Entity AsPartialFractions(Entity product, Entity.Variable x)
        {
            var combined = Functions.SingleQuotient.Combine(product).InnerSimplified;
            var (above, below) = Functions.SingleQuotient.Of(combined);
            if (below == Number.Integer.One || !below.ContainsNode(x))
                return combined;
            above = Functions.PartialFractions.Bare(above.Expand().InnerSimplified);
            if (!TreeAnalyzer.TryGetPolynomial(above, x, out var aboveRead) || !TreeAnalyzer.TryGetPolynomial(below, x, out var belowRead)
                || aboveRead.Count == 0 || belowRead.Count == 0)
                return combined;
            Entity whole = Number.Integer.Zero;
            var proper = above;
            if (aboveRead.Keys.Max()!.CompareTo(belowRead.Keys.Max()!) >= 0)
            {
                // The division hands back the remainder over the divisor already, as the
                // proper fraction it is.
                if (TreeAnalyzer.PolynomialLongDivision(above, below, genericCase: true, inTermsOf: x) is not var (divided, remainder))
                    return combined;
                whole = Functions.PartialFractions.Bare(divided.InnerSimplified);
                if (whole.Evaled is Number.Complex { IsZero: true })
                    whole = Number.Integer.Zero;
                (proper, below) = Functions.SingleQuotient.Of(Functions.PartialFractions.Bare(remainder.InnerSimplified));
            }
            if (proper == Number.Integer.Zero || proper.Evaled is Number.Complex { IsZero: true })
                return whole;
            Entity fractions;
            if (below == Number.Integer.One)
                fractions = proper;
            else if (Functions.PartialFractions.TrySplitOverWrittenFactors(proper, below, x, out var decomposition))
            {
                // The decomposition comes as its sum over the denominator's constant; each
                // fraction over the constant on its own, so that the terms are read apart.
                fractions = decomposition is Divf(var sum, var constant) && sum is Sumf or Minusf
                    ? Sumf.LinearChildren(sum).Select(fraction => fraction / constant).Aggregate((so, next) => so + next)
                    : decomposition;
            }
            else
                fractions = proper / below;
            return whole == Number.Integer.Zero ? fractions : whole + fractions;
        }

        /// <summary>
        /// <paramref name="antiderivative"/> plus the constant that makes it vanish at a root
        /// of a linear factor below the bar of one of <paramref name="derivativeTerms"/>, the
        /// first such root at which it is finite; <paramref name="antiderivative"/> itself
        /// where there is none.
        /// </summary>
        private static Entity MatchedToAPoleOf(Entity antiderivative, List<Entity> derivativeTerms, Entity.Variable x)
        {
            var (_, ownDenominator) = Functions.SingleQuotient.Of(antiderivative);
            foreach (var term in derivativeTerms)
            {
                var (_, below) = Functions.SingleQuotient.Of(term);
                foreach (var factor in Mulf.LinearChildren(below))
                {
                    var linear = factor is Powf(var repeatedBase, Number.Integer) ? repeatedBase : factor;
                    if (!linear.ContainsNode(x))
                        continue;
                    if (!TreeAnalyzer.TryGetPolyLinear(linear, x, out var slope, out var offset) || slope.Evaled is Number.Complex { IsZero: true })
                    {
                        // A factor of higher degree, for a polynomial antiderivative: the
                        // constant is what the division leaves, so that the factor divides
                        // the antiderivative. Against `ln(1 + x^2)` the antiderivative of `x`
                        // is `(1 + x^2)/2`, and the remainder is then a polynomial.
                        if (!ownDenominator.ContainsNode(x) && TreeAnalyzer.TryGetPolynomial(linear, x, out _)
                            && TreeAnalyzer.PolynomialLongDivision(antiderivative, linear, genericCase: true, inTermsOf: x) is var (_, left)
                            && Functions.SingleQuotient.Of(Functions.PartialFractions.Bare(left.InnerSimplified)) is var (leftAbove, _)
                            && !leftAbove.ContainsNode(x))
                        {
                            var constant = Functions.PartialFractions.Bare(leftAbove.InnerSimplified);
                            if (!constant.ContainsNode(x) && !constant.Nodes.Any(node => node == MathS.NaN)
                                && constant != Number.Integer.Zero && constant.Evaled is not Number.Complex { IsZero: true })
                                return antiderivative - constant;
                        }
                        continue;
                    }
                    var root = Functions.PartialFractions.Bare((-offset / slope).InnerSimplified);
                    // A pole the antiderivative shares is not one it can vanish at.
                    var ownValueBelow = Functions.PartialFractions.Bare(ownDenominator.Substitute(x, root).Simplify());
                    if (ownValueBelow.Evaled is Number.Complex { IsZero: true } || ownValueBelow == Number.Integer.Zero)
                        continue;
                    var value = Functions.PartialFractions.Bare(antiderivative.Substitute(x, root).Simplify());
                    if (value.Nodes.Any(node => node == MathS.NaN) || value.ContainsNode(x))
                        continue;
                    return value == Number.Integer.Zero || value.Evaled is Number.Complex { IsZero: true }
                        ? antiderivative
                        : antiderivative - value;
                }
            }
            return antiderivative;
        }

        /// <summary>
        /// The terms of <paramref name="expr"/> with every product distributed over the sums
        /// among its factors: <c>B (ln(a + b x) - ln(c + d x))</c> is two terms.
        /// </summary>
        private static List<Entity> TermsDistributed(Entity expr)
        {
            if (expr is Sumf or Minusf)
                return Sumf.LinearChildren(expr).SelectMany(TermsDistributed).ToList();
            if (expr is not Mulf)
                return new List<Entity> { expr };
            var terms = new List<Entity> { Number.Integer.One };
            foreach (var factor in Mulf.LinearChildren(expr))
            {
                var factorTerms = TermsDistributed(factor);
                var next = new List<Entity>();
                foreach (var so in terms)
                    foreach (var part in factorTerms)
                        next.Add(so == Number.Integer.One ? part : so * part);
                terms = next;
            }
            return terms;
        }

        /// <summary>
        /// <paramref name="expr"/> with every natural logarithm of a product of powers written
        /// as the sum of the logarithms of the factors holding <paramref name="x"/>, each
        /// times its power, and the constant factors dropped. Equal to the original up to a
        /// constant on each interval where both are defined, which is all a derivative sees.
        /// </summary>
        private static Entity WithLogarithmsOfProductsApart(Entity expr, Entity.Variable x)
            => expr.Replace(node =>
            {
                if (node is not Logf(var @base, var argument) || @base != MathS.e || !argument.ContainsNode(x) || argument is not (Mulf or Divf or Powf))
                    return node;
                Entity sum = Number.Integer.Zero;
                foreach (var (factor, underneath) in FactorsOfTheIntegrand(argument))
                {
                    if (!factor.ContainsNode(x))
                        continue;
                    var (inner, power) = factor is Powf(var b, var p) && !p.ContainsNode(x) ? (b, p) : (factor, (Entity)Number.Integer.One);
                    var term = power == Number.Integer.One ? MathS.Ln(inner) : power * MathS.Ln(inner);
                    sum = underneath ? sum - term : sum + term;
                }
                return sum;
            });

        /// <summary>
        /// A partial-fraction decomposition, a sum over a constant, integrated one term at a
        /// time as each is written, with the constant on the result.
        /// </summary>
        /// <remarks>
        /// Handed to the chain whole, the sum is split as written only while every term is
        /// small (<see cref="LargestTermTakenAsWritten"/>), and past that by the expanding
        /// gather, which combines a term's constant with its quotient: `D^(-1) (P + Q x)/(1 + x^2)`
        /// with `P`, `Q` polynomials in the symbols became `(P + Q x)/(D + D x^2)`, a quadratic
        /// with symbols in every coefficient, and was integrated as a piecewise on the sign of
        /// its discriminant. A decomposition's terms are already the shapes the rules read.
        /// </remarks>
        private static Entity? IntegratedTermByTerm(Entity decomposition, Entity.Variable x, bool integrateByParts)
        {
            var (terms, over) = decomposition is Divf(var sum, var constant) && !constant.ContainsNode(x)
                ? (sum, constant) : (decomposition, Number.Integer.One as Entity);
            Entity total = Number.Integer.Zero;
            foreach (var term in Sumf.LinearChildren(terms))
            {
                if (term.Evaled is Number.Complex { IsZero: true })
                    continue;
                if (Integration.ComputeIndefiniteIntegral(term, x, integrateByParts) is not { } integrated)
                    return null;
                total += integrated;
            }
            return over == Number.Integer.One ? total : total / over;
        }

        /// <summary>
        /// Whether <paramref name="denominator"/> is written as a product of two or more
        /// distinct factors in <paramref name="x"/>, at least one of them linear, each linear
        /// or quadratic and to a whole power, with a symbol in a coefficient somewhere.
        /// </summary>
        /// <remarks>
        /// A quadratic beside the linears is allowed since the decomposition takes the linear
        /// blocks by their Taylor coefficients and the quadratic's numerator in the ring
        /// modulo the quadratic; with the gate asking for linears only,
        /// <c>u^4 (A + B u)/((a + b u)^4 (1 + u^2))</c> -- every rational function of the
        /// tangent with a power of a linear in it -- went to the Hermite reduction below,
        /// which answered in <c>a^63 b^10</c>.
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        private static bool IsAProductOfSymbolicLinearFactors(Entity denominator, Entity.Variable x)
        {
            var linears = 0;
            var quadratics = 0;
            foreach (var factor in Mulf.LinearChildren(denominator))
            {
                if (!factor.ContainsNode(x))
                    continue;
                var @base = factor is Powf(var b, Number.Integer { EInteger.Sign: > 0 }) ? b : factor;
                if (!TreeAnalyzer.TryGetPolynomial(@base, x, out var read) || read.Count == 0
                    || read.Values.Any(coefficient => coefficient.ContainsNode(x)))
                    return false;
                if (read.Keys.Max()!.Equals(EInteger.One))
                    linears++;
                else if (read.Keys.Max()!.Equals(EInteger.FromInt32(2)))
                    quadratics++;
                else
                    return false;
            }
            return linears >= 1 && linears + quadratics >= 2 && denominator.Vars.Any(v => v != x);
        }

        /// <summary>
        /// Whether both read as polynomials in <paramref name="x"/> with the numerator's
        /// degree below the denominator's; false where either does not read.
        /// </summary>
        private static bool IsProperByDegree(Entity numerator, Entity denominator, Entity.Variable x)
            => TreeAnalyzer.TryGetPolynomial(numerator, x, out var above) && above.Count > 0
               && TreeAnalyzer.TryGetPolynomial(denominator, x, out var below) && below.Count > 0
               && above.Keys.Max()!.CompareTo(below.Keys.Max()!) < 0;

        /// <summary>Whether every node of <paramref name="expr"/> holding <paramref name="x"/> is a sum, a product, a quotient or a whole power.</summary>
        private static bool IsARationalFunction(Entity expr, Entity.Variable x)
            => expr.Nodes.All(node => !node.ContainsNode(x) || node is Variable or Sumf or Minusf or Mulf or Divf || node is Powf(_, Number.Integer));

        /// <summary>
        /// <paramref name="expr"/> with every whole negative power of a polynomial in
        /// <paramref name="x"/> of two or more terms among its factors written below the bar,
        /// or <paramref name="expr"/> itself where there is none.
        /// </summary>
        private static Entity WithPolynomialPowersBelowTheBar(Entity expr, Entity.Variable x)
        {
            var (above, below) = expr is Divf(var over, var under) ? (over, under) : (expr, (Entity)Number.Integer.One);
            Entity kept = Number.Integer.One;
            var moved = false;
            foreach (var factor in Mulf.LinearChildren(above))
            {
                if (factor is Powf(var @base, Number.Integer { EInteger.Sign: < 0 } power) && @base.ContainsNode(x)
                    && TreeAnalyzer.TryGetPolynomial(@base, x, out var read) && read.Count >= 2)
                {
                    below = below == Number.Integer.One ? MathS.Pow(@base, -power) : below * MathS.Pow(@base, -power);
                    moved = true;
                }
                else
                    kept = kept == Number.Integer.One ? factor : kept * factor;
            }
            return moved ? kept / below : expr;
        }

        /// <summary>
        /// <paramref name="expr"/> with every exponential of <paramref name="x"/> whose base is a
        /// positive rational written in one base, where the bases are whole powers of one:
        /// <c>4^x</c> beside <c>2^x</c> is <c>2^(2x)</c>, and <c>2^x</c> beside <c>sqrt(2)^x</c>
        /// is <c>sqrt(2)^(2x)</c>. The common base is the smallest of them above one or a whole
        /// root of it; where no such base exists, or the bases are not all rationals,
        /// <paramref name="expr"/> as it came.
        /// </summary>
        private static Entity WithNumericBasesUnified(Entity expr, Entity.Variable x)
        {
            var bases = new List<ERational>();
            foreach (var node in expr.Nodes)
                if (node is Powf(var @base, var exponent) && !@base.ContainsNode(x) && exponent.ContainsNode(x) && exponent is not Number)
                {
                    if (@base.Evaled is not Number.Rational rational || rational.ERational.Sign <= 0 || rational.ERational.Equals(ERational.One))
                        return expr;
                    var written = rational.ERational.ToLowestTerms();
                    if (!bases.Contains(written))
                        bases.Add(written);
                }
            if (bases.Count < 2)
                return expr;
            // Every base above one, so that the powers are whole and positive where they exist.
            var aboveOne = bases.Select(b => b.CompareTo(ERational.One) < 0 ? ERational.One.Divide(b) : b).ToList();
            var smallest = aboveOne.OrderBy(b => b).First();
            Entity? common = null;
            var powers = new Dictionary<ERational, int>();
            for (var root = 1; root <= 4 && common is null; root++)
            {
                var candidate = MathS.Pow(Number.Rational.Create(smallest), Number.Rational.Create(1, root)).Evaled;
                if (candidate is not Number.Rational candidateRational)
                    continue;
                powers.Clear();
                var all = true;
                foreach (var @base in bases)
                {
                    var power = 0;
                    Entity accumulated = Number.Integer.One;
                    while (power < 64 && ((Number.Rational)accumulated).ERational.CompareTo(@base.CompareTo(ERational.One) < 0 ? ERational.One.Divide(@base) : @base) < 0)
                    {
                        accumulated = (accumulated * candidateRational).Evaled;
                        power++;
                    }
                    var matches = ((Number.Rational)accumulated).ERational.Equals(@base.CompareTo(ERational.One) < 0 ? ERational.One.Divide(@base) : @base);
                    if (!matches)
                    {
                        all = false;
                        break;
                    }
                    powers[@base] = @base.CompareTo(ERational.One) < 0 ? -power : power;
                }
                if (all)
                    common = candidateRational;
            }
            if (common is null)
                return expr;
            return expr.Replace(node =>
                node is Powf(var @base, var exponent) && !@base.ContainsNode(x) && exponent.ContainsNode(x) && exponent is not Number
                && @base.Evaled is Number.Rational rational && powers.TryGetValue(rational.ERational.ToLowestTerms(), out var power)
                    ? MathS.Pow(common, power == 1 ? exponent : (Number.Integer.Create(power) * exponent).InnerSimplified)
                    : node);
        }

        /// <summary>
        /// Whether <paramref name="exponent"/> is written as an even whole number times
        /// something, a minus sign in front allowed: <c>2 (c + d x)</c>, <c>-(2 x)</c>.
        /// </summary>
        private static bool WrittenWithAnEvenFactor(Entity exponent)
        {
            while (exponent is Mulf(Number.Integer { EInteger.IsEven: false } sign, var rest) && sign.EInteger.Abs().Equals(EInteger.One))
                exponent = rest;
            return exponent is Mulf(Number.Integer { EInteger.IsEven: true } factor, _) && !factor.EInteger.IsZero;
        }

        /// <summary>
        /// Every <c>x</c> of <paramref name="expr"/> in an exponential <c>e^(k y)</c> with
        /// <c>k</c> a whole number and <c>y</c> one linear form <c>c + d x</c> shared by all of
        /// them -- <c>coth(c + d x)</c> is written with <c>e^(2c + 2d x)</c> -- and in nothing
        /// else: each exponential node with its <c>k</c>, the form <c>y</c> scaled so that the
        /// <c>k</c> are whole and coprime -- or, with <paramref name="unitX"/> and a whole
        /// number for the slope, <c>y</c> as <c>x</c> itself plus the offset over the slope
        /// and each <c>k</c> the slope as written -- and <c>dx/dy</c>; <see langword="false"/>
        /// where the exponentials are of two forms, or an exponent is not linear in <c>x</c>.
        /// </summary>
        private static bool TryGatherExponentialsOfOneLinearForm(Entity expr, Entity.Variable x, bool unitX,
            out Dictionary<Entity, EInteger> exponentials, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Entity? y, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Entity? dxOverDy)
        {
            exponentials = new Dictionary<Entity, EInteger>();
            y = null;
            dxOverDy = null;
            var multiples = new List<(Entity Node, Number.Rational Multiple)>();
            Entity? slopeOfY = null;
            Entity? offsetOfY = null;
            foreach (var node in expr.Nodes)
            {
                if (node is not Powf(var @base, var exponent) || !exponent.ContainsNode(x))
                    continue;
                if (@base != MathS.e || !TreeAnalyzer.TryGetPolyLinear(exponent, x, out var slope, out var offset) || TreeAnalyzer.IsZero(slope))
                    return false;
                if (slopeOfY is null)
                {
                    (slopeOfY, offsetOfY) = (slope, offset);
                    multiples.Add((node, Number.Integer.One));
                    continue;
                }
                if (multiples.Any(known => known.Node == node))
                    continue;
                // The same linear form, to a rational multiple: the slopes' ratio is a number
                // and the offsets stand in the same ratio.
                // Bare: `-d/d` simplifies to `-1 provided not d = 0`, and the generic case is
                // what every neighbouring rule answers.
                var ratio = (slope / slopeOfY).InnerSimplified;
                var multiple = ratio.Evaled as Number.Rational ?? Functions.PartialFractions.Bare(ratio.Simplify()).Evaled as Number.Rational;
                if (multiple is null || multiple.IsZero)
                    return false;
                var offsetDifference = (offset - multiple * offsetOfY!).InnerSimplified;
                if (!TreeAnalyzer.IsZero(offsetDifference) && !IsTheZeroPolynomial(offsetDifference))
                    return false;
                multiples.Add((node, multiple));
            }
            if (multiples.Count == 0 || slopeOfY is null || offsetOfY is null)
                return false;
            // y scaled so that every multiple is a whole number: the least common multiple of
            // the denominators over the greatest common divisor of the numerators.
            var denominators = EInteger.One;
            var numerators = EInteger.Zero;
            foreach (var (_, multiple) in multiples)
            {
                denominators = denominators.Lcm(multiple.ERational.Denominator);
                numerators = numerators.Gcd(multiple.ERational.Numerator);
            }
            var yScaleRational = ERational.Create(numerators, denominators);
            // With a whole number for the first slope, and asked to, y is x itself (plus the
            // offset over the slope) and each k the slope as written -- the substitution with
            // the root wants `coth(x)`, which is `e^(2x)`, as k = 2 and u = tanh(x), where the
            // scaling by the gcd would make y = 2x, k = 1 and u = tanh(2x), in which coth(x) is
            // a quotient with the root in it and not `1/u`; the rational route wants the
            // opposite, y as coarse as it comes, `coth(2x)` as tanh(2x) and not a quartic in
            // tanh(x). A symbolic slope has no unit to keep, and takes the gcd.
            if (unitX && slopeOfY.Evaled is Number.Integer wholeSlope && !wholeSlope.EInteger.IsZero
                && multiples.All(m => m.Multiple.ERational.Multiply(wholeSlope.EInteger).Denominator.Equals(EInteger.One)))
                yScaleRational = ERational.Create(EInteger.One, wholeSlope.EInteger);
            var yScale = Number.Rational.Create(yScaleRational);
            foreach (var (node, multiple) in multiples)
                exponentials[node] = multiple.ERational.Divide(yScaleRational).ToLowestTerms().Numerator;
            y = (yScale * (slopeOfY * x + offsetOfY)).InnerSimplified;
            dxOverDy = TreeAnalyzer.IsZero(offsetOfY) && slopeOfY == Number.Integer.One && yScale == Number.Integer.One
                ? Number.Integer.One
                : (1 / (yScale * slopeOfY)).InnerSimplified;
            return true;
        }

        // A polynomial with its like terms collected, as a polynomial over every symbol in
        // it: the synthetic division leaves each coefficient a sum of products that
        // nothing below reads as one number.
        private static Entity Collected(Entity polynomial)
        {
            var symbols = polynomial.Vars.ToList();
            if (symbols.Count == 0)
                return polynomial;
            var indices = new Dictionary<Variable, int>();
            foreach (var symbol in symbols)
                indices[symbol] = indices.Count;
            if (Functions.MultivariatePolynomial.TryParse(polynomial, indices) is not { } parsed || parsed.IsZero)
                return polynomial;
            // The numeric content in front of the primitive part -- `4a + 4b u^2` as
            // `4 (a + b u^2)` -- which the symbolic partial fractions want out of the way: a
            // symbolic quadratic to a power with a number in every coefficient made them a
            // page of `a^48 b^15`.
            var primitive = parsed.Normalized();
            var leading = parsed;
            var primitiveLeading = primitive;
            for (var variable = 0; variable < symbols.Count; variable++)
            {
                leading = leading.LeadingCoefficientIn(variable);
                primitiveLeading = primitiveLeading.LeadingCoefficientIn(variable);
            }
            if (leading.ToEntity(symbols).Evaled is not Number.Rational leadingValue || primitiveLeading.ToEntity(symbols).Evaled is not Number.Rational primitiveValue || primitiveValue.IsZero)
                return parsed.ToEntity(symbols);
            var content = leadingValue / primitiveValue;
            return content == Number.Integer.One ? primitive.ToEntity(symbols) : content * primitive.ToEntity(symbols);
        }
        // Zero as a polynomial in its symbols, exactly: a remainder with symbols in it is
        // a sum of products that the inner simplification does not collect.
        private static bool IsTheZeroPolynomial(Entity expr)
        {
            if (expr == Number.Integer.Zero || expr.Evaled is Number.Complex { IsZero: true })
                return true;
            var symbols = expr.Vars.ToList();
            if (symbols.Count == 0)
                return false;
            var indices = new Dictionary<Variable, int>();
            foreach (var symbol in symbols)
                indices[symbol] = indices.Count;
            return Functions.MultivariatePolynomial.TryParse(expr, indices) is { IsZero: true };
        }
        /// <summary>
        /// An integrand that is a rational function of <c>tanh(y)</c> -- of <c>e^(2y)</c>, that
        /// is, even as a function of <c>v = e^y</c> -- with a symbol among its coefficients,
        /// integrated by <c>u = tanh(y)</c>: <c>e^(2y)</c> is <c>(1 + u)/(1 - u)</c> exactly and
        /// <c>dx</c> is <c>du/((1 - u^2) d)</c>, so the integrand is a rational function of
        /// <c>u</c> with the written factors kept -- <c>a + b coth(y)^2</c> is
        /// <c>(b + a u^2)/u^2</c> -- and roots of such functions are admitted as they are.
        /// Failing evenness, the half of <c>y</c>: every <c>e^(k y)</c> is <c>v^(2k)</c> for
        /// <c>v = e^(y/2)</c>, and the substitution is <c>u = tanh(y/2)</c>, the hyperbolic
        /// half-angle.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Under <c>u = e^x</c>, Rubi's <c>1/(a + b coth(x)^2)^2</c> is a rational function of the
        /// palindromic quartic <c>(a + b) u^4 + 2(b - a) u^2 + (a + b)</c> squared, whose roots
        /// are what the budget went on -- fifty-seven of the four hundred and seventeen in the
        /// hyperbolic sample were this; here it is <c>u^4/((b + a u^2)^2 (1 - u^2))</c>, which
        /// the symbolic partial fractions answer. Rubi's route for the hyperbolic tangent,
        /// cotangent, secant and cosecant families. With rational coefficients the exponential
        /// substitution's quartics are factored over the rationals and answered, and are left
        /// to it. Before the substitution search, which with a symbolic slope spends the whole
        /// budget on <c>u = c + d x</c> and never returns here.
        /// </para>
        /// <para>
        /// <b>How the written factors survive.</b> Each maximal rational function of <c>v</c>
        /// among the factors, powers and radicands -- and not the whole quotient -- is read as
        /// <c>v^m R(s)</c> with <c>s = v^2</c> and <c>m</c> either 0 or 1, which every rational
        /// function even or odd in <c>v</c> is, and homogenised on its own: <c>P(s)/Q(s)</c> as
        /// the polynomials <c>sum p_i (1 + u)^i (1 - u)^(d - i)</c> over the same for <c>Q</c>,
        /// with the powers of <c>1 - u</c> and <c>1 + u</c> both share divided out; the
        /// conjugate that <see cref="SolveByHyperbolicTangentSubstitution"/> clears its root
        /// with squares every degree, and left this integrand a quotient of degree sixteen
        /// whose common factor only a gcd over the symbols would find. The stray <c>v</c>'s
        /// cancel in an even integrand -- <c>sech(x)^4</c> is <c>(2v/(s + 1))^4</c> -- and one
        /// left over at the top is an odd integrand, which the half of <c>y</c> takes.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveARationalFunctionOfTheHyperbolicTangent(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            // An integrand that is a function of e^(2x) -- even as a function of v = e^x, node
            // by node -- is written in u with no root of 1 - u^2 to clear and nothing squared
            // by a conjugate: e^(2x) is (1 + u)/(1 - u) exactly, and `dx` is `du/(1 - u^2)`.
            // Each maximal rational function of v among the factors, powers and radicands --
            // `a + b coth(x)^2`, and not the whole quotient -- is read as v^m R(s) with s = v^2
            // and m either 0 or 1, which every rational function even or odd in v is, and
            // homogenised on its own: P(s)/Q(s) as the polynomials sum p_i (1 + u)^i (1 - u)^(d - i)
            // over the same for Q, with the powers of 1 - u and 1 + u that both share divided
            // out, so that `coth(x)` comes out as `1/u` and `a + b coth(x)^2` as
            // `(b + a u^2)/u^2`, and the written powers and products stand as they were. The
            // stray v's cancel in an even integrand -- `sech(x)^4` is `(2v/(s + 1))^4` -- and
            // one left over at the top is an odd integrand, not this route's. The rational
            // integrator then sees `u^4/((b + a u^2)^2 (1 - u^2))` and not the quotient of
            // degree sixteen the conjugates made of it.
            if (!Integration.AnsweringTheQuestionAskedOrOneBelow || !expr.Vars.Any(symbol => symbol != x))
                return null;
            if (!TryGatherExponentialsOfOneLinearForm(expr, x, unitX: false, out var exponentials, out var y, out var dxOverDy))
                return null;
            {
                var t = Variable.CreateUnique(expr, "u_tanh");
                var vt = Variable.CreateUnique(expr, "v_tanh");
                var square = Variable.CreateUnique(expr, "s_tanh");
                // With v = e^y first, u = tanh(y), which asks the integrand to be even in v --
                // `coth(2y)` is `(v^2 + 1)/(v^2 - 1)`; failing that with v = e^(y/2) and
                // u = tanh(y/2), which asks nothing, since every e^(k y) is then v^(2k): the
                // half-angle, `sinh(y)` as `2u/(1 - u^2)`. The finer y where the coarser answers,
                // for the lower degrees and the answer in the argument as written. The half only
                // where every exponent is written with an even factor in front -- `coth(c + d x)`
                // is written with `e^(2 (c + d x))`, and `tanh(c + d x)` is its argument's
                // tangent, not the half-angle of `2(c + d x)` -- and not with a root in the
                // integrand, which the half turns into a root of a quotient: `sinh(x)` writes
                // `e^x`, and its `u = tanh(x/2)` answers were what `u = e^x` answered in less,
                // Timofeev's `e^x/sqrt(e^(2x) + a^2)` among them, a root of a polynomial there.
                var halfIsTheArgument = !HasARadicalOf(expr, x) && exponentials.Keys.All(node => node is Powf(_, var exponent) && WrittenWithAnEvenFactor(exponent));
                foreach (var halved in new[] { false, true })
                {
                    if (halved && !halfIsTheArgument)
                        break;
                    var inV = expr.Replace(node =>
                        exponentials.TryGetValue(node, out var k)
                            ? MathS.Pow(vt, Number.Integer.Create(halved ? k.Multiply(2) : k))
                            : node);
                    if (inV.ContainsNode(x))
                        return null;
                    if (InTanh(inV) is not var (inT, parity) || parity != 0)
                        continue;
                    var rationalInT = WithThePowersOfOnePlusMinusTCollected(inT / (1 - MathS.Sqr(t)));
                    if (rationalInT.ContainsNode(square) || rationalInT.ContainsNode(vt) || rationalInT.Nodes.Any(node => node == MathS.NaN)
                        || Integration.ComputeAsAQuestionOfItsOwn(rationalInT, t, integrateByParts) is not { } evenResult)
                        return null;
                    var evenAnswer = evenResult.Substitute(t, MathS.Hyperbolic.Tanh(halved ? (y / 2).InnerSimplified : y));
                    if (evenAnswer.Nodes.Any(node => node == MathS.NaN))
                        return null;
                    // dx is dy/d, and twice that in the half of y.
                    var scale = halved ? (2 * dxOverDy).InnerSimplified : dxOverDy;
                    var scaledAnswer = scale == Number.Integer.One ? evenAnswer : evenAnswer * scale;
                    // Checked, as every answer with symbols in it that is assembled from
                    // pieces is: a root of a rational function of u goes on to rules that
                    // have written one through the imaginary unit.
                    if (!Functions.PartialFractions.DerivativeHoldsAtSampledPoints(scaledAnswer, expr, x))
                        return null;
                    return scaledAnswer;
                }
                return null;

                // The quotient with every factor that is a multiple of 1 + t or 1 - t, on either
                // side of the bar and to any whole power, gathered into one power of each: the
                // powers of s and the homogenisations each bring their own, `sech(x)^4` arrives
                // as `(1 - t)^4 (1 + t)^2/(1 - t)^2`, and the rational integrator would
                // otherwise meet them as separate factors that only the gcd cancels.
                Entity WithThePowersOfOnePlusMinusTCollected(Entity quotient)
                {
                    // A whole power of a product as the product of the powers, and of a
                    // quotient as the quotient of them, so that the factors are read one by one.
                    for (var round = 0; round < 8; round++)
                    {
                        var distributed = quotient.Replace(node => node switch
                        {
                            Powf(Mulf(var l, var r), Number.Integer power) => MathS.Pow(l, power) * MathS.Pow(r, power),
                            Powf(Divf(var l, var r), Number.Integer power) => MathS.Pow(l, power) / MathS.Pow(r, power),
                            _ => node
                        });
                        if (distributed == quotient)
                            break;
                        quotient = distributed;
                    }
                    var (above, below) = Functions.SingleQuotient.Of(Functions.SingleQuotient.Combine(quotient));
                    var plus = 0;
                    var minus = 0;
                    Entity constant = Number.Integer.One;
                    Entity keptAbove = Number.Integer.One;
                    Entity keptBelow = Number.Integer.One;
                    foreach (var (side, isBelow) in new[] { (above, false), (below, true) })
                        foreach (var factor in Mulf.LinearChildren(side))
                        {
                            var (factorBase, power) = factor is Powf(var inner, Number.Integer whole) ? (inner, whole.EInteger.ToInt32Checked()) : (factor, 1);
                            if (AsAMultipleOfOnePlusMinusT(factorBase) is var (scale, ofPlus, ofMinus))
                            {
                                var signedPower = isBelow ? -power : power;
                                plus += ofPlus * signedPower;
                                minus += ofMinus * signedPower;
                                constant *= MathS.Pow(scale, Number.Integer.Create(signedPower));
                            }
                            else if (isBelow)
                                keptBelow *= factor;
                            else
                                keptAbove *= factor;
                        }
                    Entity result = keptAbove / keptBelow * constant;
                    // What the two share as a power of 1 - t^2, which the rational integrator
                    // reads as one factor; the rest as the linear ones.
                    var shared = System.Math.Sign(plus) == System.Math.Sign(minus) ? System.Math.Sign(plus) * System.Math.Min(System.Math.Abs(plus), System.Math.Abs(minus)) : 0;
                    if (shared != 0)
                        result *= MathS.Pow(1 - MathS.Sqr(t), Number.Integer.Create(shared));
                    if (plus - shared != 0)
                        result *= MathS.Pow(1 + t, Number.Integer.Create(plus - shared));
                    if (minus - shared != 0)
                        result *= MathS.Pow(1 - t, Number.Integer.Create(minus - shared));
                    return Functions.PartialFractions.Bare(result.InnerSimplified);
                }
                // c (1 + t) as (c, 1, 0), c (1 - t) as (c, 0, 1) and c (1 - t^2) as (c, 1, 1), for
                // a number c; null otherwise.
                (Entity, int, int)? AsAMultipleOfOnePlusMinusT(Entity factor)
                {
                    if (!factor.ContainsNode(t) || !TreeAnalyzer.TryGetPolynomial(factor, t, out var monomials) || monomials.Count != 2
                        || !monomials.TryGetValue(EInteger.Zero, out var c0) || c0.Evaled is not Number.Rational scale || scale.IsZero)
                        return null;
                    if (monomials.TryGetValue(EInteger.One, out var c1) && c1.Evaled is Number.Rational slope)
                        return slope == scale ? (scale, 1, 0) : slope == -scale ? (scale, 0, 1) : null;
                    if (monomials.TryGetValue(EInteger.FromInt32(2), out var c2) && c2.Evaled is Number.Rational curvature && curvature == -scale)
                        return (scale, 1, 1);
                    return null;
                }
                // The node as v^m R with R written in t, m being 0 or 1, with each maximal
                // rational function of v among its factors, powers and radicands read on its
                // own; null where something is not one, or a sum mixes the two parities.
                (Entity, int)? InTanh(Entity node)
                {
                    if (!node.ContainsNode(vt))
                        return (node, 0);
                    switch (node)
                    {
                        case Powf(var @base, var exponent) when !exponent.ContainsNode(vt) && exponent != Number.Integer.One:
                        {
                            if (InTanh(@base) is not var (baseInT, baseParity))
                                return null;
                            if (baseParity == 0)
                                return (MathS.Pow(baseInT, exponent), 0);
                            // (v R)^n is v^n R^n: for n whole, s^(n/2) R^n or v s^((n-1)/2) R^n.
                            if (exponent is not Number.Integer { EInteger: var n })
                                return null;
                            var half = n.IsEven ? n.Divide(2) : n.Subtract(1).Divide(2);
                            return (half.IsZero ? MathS.Pow(baseInT, exponent) : MathS.Pow(baseInT, exponent) * PowerOfS(half), n.IsEven ? 0 : 1);
                        }
                        case Mulf(var left, var right):
                            return InTanh(left) is var (l, lp) && InTanh(right) is var (r, rp) ? WithParity(l * r, lp + rp) : null;
                        case Divf(var above, var below):
                            return InTanh(above) is var (a, ap) && InTanh(below) is var (b, bp) ? WithParity(a / b, ap - bp) : null;
                    }
                    if (!IsARationalFunction(node, vt))
                        return null;
                    var (numerator, denominator) = Functions.SingleQuotient.Of(Functions.SingleQuotient.Combine(node));
                    if (AsPowerOfVTimesEvenPart(numerator) is not var (aboveInS, aboveParity) || AsPowerOfVTimesEvenPart(denominator) is not var (belowInS, belowParity))
                        return null;
                    // v^p P'/(v^q Q'): with p - q = -1, v P'/(s Q').
                    if (aboveParity - belowParity == -1)
                        belowInS *= square;
                    if (DegreeInS(aboveInS) is not { } aboveDegree || DegreeInS(belowInS) is not { } belowDegree)
                        return null;
                    var d = System.Math.Max(aboveDegree, belowDegree);
                    if (Homogenised(aboveInS, d) is not { } top || Homogenised(belowInS, d) is not { } bottom)
                        return null;
                    return (InLowestTerms(top, bottom), aboveParity == belowParity ? 0 : 1);
                }
                // v^(2k + m) as s^k v^m: a whole power of v folded into the even part.
                (Entity, int)? WithParity(Entity ofT, int power)
                {
                    var m = ((power % 2) + 2) % 2;
                    var k = (power - m) / 2;
                    if (k == 0)
                        return (ofT, m);
                    return (ofT * PowerOfS(EInteger.FromInt32(k)), m);
                }
                // A polynomial in v that is even or odd in v, as (P'(s), m) with P(v) = v^m P'(v^2).
                (Entity, int)? AsPowerOfVTimesEvenPart(Entity polynomial)
                {
                    if (!polynomial.ContainsNode(vt))
                        return (polynomial, 0);
                    if (!TreeAnalyzer.TryGetPolynomial(polynomial.Expand().InnerSimplified, vt, out var monomials))
                        return null;
                    var odd = monomials.Keys.Any(power => !power.IsEven);
                    if (odd && monomials.Keys.Any(power => power.IsEven))
                        return null;
                    Entity sum = Number.Integer.Zero;
                    foreach (var pair in monomials)
                    {
                        if (pair.Key.Sign < 0)
                            return null;
                        var half = (odd ? pair.Key.Subtract(1) : pair.Key).Divide(2);
                        sum += pair.Value * (half.IsZero ? Number.Integer.One : MathS.Pow(square, Number.Integer.Create(half)));
                    }
                    return (sum, odd ? 1 : 0);
                }
                // s^k in t: (1 + t)^k over (1 - t)^k, or the other way up for k negative.
                Entity PowerOfS(EInteger k)
                {
                    var whole = Number.Integer.Create(k.Abs());
                    return k.Sign < 0 ? MathS.Pow(1 - t, whole) / MathS.Pow(1 + t, whole) : MathS.Pow(1 + t, whole) / MathS.Pow(1 - t, whole);
                }
                int? DegreeInS(Entity polynomial)
                {
                    if (!polynomial.ContainsNode(square))
                        return 0;
                    if (!TreeAnalyzer.TryGetPolynomial(polynomial.Expand().InnerSimplified, square, out var monomials) || monomials.Keys.Max() is not { } degree
                        || !degree.CanFitInInt32() || monomials.Keys.Any(power => power.Sign < 0))
                        return null;
                    return degree.ToInt32Unchecked();
                }
                // P(s) with s = (1 + t)/(1 - t), times (1 - t)^d for the degree d of the
                // quotient it stands in: a polynomial in t.
                Entity? Homogenised(Entity polynomial, int d)
                {
                    if (!polynomial.ContainsNode(square))
                        return d == 0 ? polynomial : polynomial * MathS.Pow(1 - t, d);
                    if (!TreeAnalyzer.TryGetPolynomial(polynomial.Expand().InnerSimplified, square, out var monomials))
                        return null;
                    Entity sum = Number.Integer.Zero;
                    foreach (var pair in monomials)
                    {
                        if (pair.Key.Sign < 0)
                            return null;
                        var i = pair.Key.ToInt32Unchecked();
                        sum += pair.Value * MathS.Pow(1 + t, i) * MathS.Pow(1 - t, d - i);
                    }
                    return Functions.PartialFractions.Bare(sum.Expand().InnerSimplified);
                }
                // The quotient with the powers of 1 - t and 1 + t both sides share divided out,
                // synthetically -- a polynomial is divisible by t - r exactly when it vanishes at
                // r -- and the sides collected as polynomials over every symbol.
                Entity InLowestTerms(Entity above, Entity below)
                {
                    foreach (var root in new[] { Number.Integer.One, Number.Integer.MinusOne })
                        while (DividedByTMinus(above, root) is { } aboveQuotient && DividedByTMinus(below, root) is { } belowQuotient)
                            (above, below) = (aboveQuotient, belowQuotient);
                    var collectedAbove = Collected(above);
                    var collectedBelow = Collected(below);
                    return collectedBelow == Number.Integer.One ? collectedAbove : collectedAbove / collectedBelow;
                }
                Entity? DividedByTMinus(Entity polynomial, Number.Integer root)
                {
                    if (!TreeAnalyzer.TryGetPolynomial(polynomial, t, out var monomials) || monomials.Keys.Max() is not { } degree || degree.Sign <= 0)
                        return null;
                    var n = degree.ToInt32Checked();
                    var quotient = new Entity[n];
                    Entity carry = Number.Integer.Zero;
                    for (var k = n; k >= 1; k--)
                    {
                        var coefficient = monomials.TryGetValue(EInteger.FromInt32(k), out var c) ? c : Number.Integer.Zero;
                        carry = (coefficient + root * carry).InnerSimplified;
                        quotient[k - 1] = carry;
                    }
                    var constant = monomials.TryGetValue(EInteger.Zero, out var c0) ? c0 : Number.Integer.Zero;
                    if (!IsTheZeroPolynomial((constant + root * carry).InnerSimplified))
                        return null;
                    Entity sum = Number.Integer.Zero;
                    for (var k = 0; k < n; k++)
                        if (!TreeAnalyzer.IsZero(quotient[k]))
                            sum += quotient[k] * MathS.Pow(t, k);
                    return Functions.PartialFractions.Bare(sum.Expand().InnerSimplified);
                }
            }

        }

        /// <summary>
        /// An integrand that is a function of <c>e^x</c> with a root in it, integrated by the
        /// substitution <c>u = tanh(x)</c>, the hyperbolic counterpart of the tangent's.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The library writes the hyperbolic functions as exponentials, and a root of one of
        /// them -- Timofeev's <c>cosh(x)(tanh(x) - cosh(2x))/((sinh(x)^2 + sinh(2x)) sqrt(sinh(2x)))</c>
        /// -- is under <c>u = e^x</c> a root of <c>(u^4 - 1)/u^2</c>, a quartic that nothing
        /// rationalises, where its trigonometric twin is a rational function of the tangent
        /// with <c>sqrt(2u/(1 - u^2))</c> in it and is answered. Under <c>u = tanh(x)</c> the
        /// hyperbolic functions are the same shapes: <c>cosh(x)</c> is <c>(1 - u^2)^(-1/2)</c>
        /// and <c>sinh(x)</c> is <c>u (1 - u^2)^(-1/2)</c>, exactly and for every real <c>x</c>,
        /// since the hyperbolic cosine is positive -- no sign to carry, where the tangent
        /// substitution carries <c>sgn(cos(x))</c>. So <c>e^x</c> is <c>(1 + u)/w</c> and
        /// <c>e^-x</c> is <c>(1 - u)/w</c> for <c>w = sqrt(1 - u^2)</c>, the integrand is
        /// rational in <c>u</c> and <c>w</c> apart from its own roots, and <c>dx = du/(1 - u^2)</c>.
        /// Every quotient -- each radicand, and the whole -- is brought over one bar with
        /// <c>w^2 = 1 - u^2</c> reduced and the denominator cleared of <c>w</c> by its
        /// conjugate, and the polynomials cancelled by their greatest common divisor, so that
        /// <c>e^x + e^-x</c> comes out as <c>2/w</c> and <c>sinh(2x)</c> as <c>2u/(1 - u^2)</c>;
        /// a radicand over a power of <c>1 - u^2</c> gives that power up to <c>w</c>, where it
        /// meets the rest, and a radicand that is a power of <c>1 - u^2</c> and <c>w</c> alone
        /// is a root of <c>1 - u^2</c> of another index -- <c>sech(x)^(3/4)</c> is
        /// <c>(1 - u^2)^(3/8)</c>.
        /// </para>
        /// <para>
        /// With a root in the integrand whose radicand holds exponentials of both signs
        /// -- as a Laurent polynomial in <c>e^x</c>, a polynomial over a power with a power on
        /// either side of it, or a rational function over anything else -- which is where
        /// <c>u = e^x</c> leaves a root of a quartic or of a quotient; with one sign only,
        /// <c>sqrt(e^x - 1)</c> or <c>sqrt(1 + tanh(x))</c>, the exponential substitution answers
        /// with a root of a linear, and is left to. And, with a symbol among the coefficients,
        /// for an integrand that is a rational function of <c>tanh(x)</c> -- one in which
        /// <c>w</c> cancels out -- with its roots, if any, of rational functions of it too:
        /// <c>1/(a + b coth(x)^2)^2</c> is <c>u^4/((b + a u^2)^2 (1 - u^2))</c> here, where under
        /// <c>u = e^x</c> it is a rational function of a symbolic palindromic quartic
        /// <c>(a + b) u^4 + 2(b - a) u^2 + (a + b)</c> squared, whose roots are what the budget
        /// went on; Rubi's route for the hyperbolic tangent, cotangent, secant and cosecant
        /// families. With rational coefficients the exponential substitution's quartics are
        /// factored over the rationals and answered, and are left to it.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveByHyperbolicTangentSubstitution(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            // At the top or one below it -- the terms of a sum split as written are one below --
            // since it lands on the chain: asked at every level by every rule that hands a
            // hyperbolic integrand on, it was a search at every depth.
            if (!Integration.AnsweringTheQuestionAskedOrOneBelow || !HasARadicalOf(expr, x))
                return null;
            if (!TryGatherExponentialsOfOneLinearForm(expr, x, unitX: true, out var exponentials, out var y, out var dxOverDy))
                return null;
            // And a radicand that holds exponentials of both signs -- as a Laurent polynomial
            // in v = e^x, a polynomial over a power of v of lower degree -- which is where
            // `u = e^x` leaves a root of a quartic; with one sign only, `sqrt(e^x - 1)` or
            // `sqrt(1 + tanh(x))`, the exponential substitution after this rule answers with a
            // root of a linear.
            var v = Variable.CreateUnique(expr, "v_exp");
            var bothSigns = false;
            foreach (var node in expr.Nodes)
            {
                if (node is not Powf(var radicand, Number.Rational root) || root is Number.Integer || !radicand.ContainsNode(x))
                    continue;
                var inV = radicand.Replace(inner => exponentials.TryGetValue(inner, out var k) ? k.Equals(EInteger.One) ? v : MathS.Pow(v, Number.Integer.Create(k)) : inner);
                var (above, below) = Functions.SingleQuotient.Of(Functions.SingleQuotient.Combine(inV));
                if (!TreeAnalyzer.TryGetPolynomial(above.Expand().InnerSimplified, v, out var aboveMonomials) || aboveMonomials.Keys.Max() is not { } aboveDegree
                    || !TreeAnalyzer.TryGetPolynomial(below.Expand().InnerSimplified, v, out var belowMonomials) || belowMonomials.Keys.Max() is not { } belowDegree
                    || belowDegree.Sign == 0)
                    continue;
                // Over a power of v, a Laurent polynomial: both signs where the numerator has
                // a power on either side of that one. Over anything else -- `sech(x)` is
                // `2v/(v^2 + 1)` -- a root of a rational function of v that `u = e^x` leaves a
                // root of a quotient.
                if (belowMonomials.Count != 1 || aboveDegree.CompareTo(belowDegree) > 0 && aboveMonomials.Keys.Min() is { } lowest && lowest.CompareTo(belowDegree) < 0)
                    bothSigns = true;
            }
            if (!bothSigns)
                return null;
            var u = Variable.CreateUnique(expr, "u_tanh");
            var w = Variable.CreateUnique(expr, "w_tanh");
            var oneMinusUSquared = 1 - MathS.Sqr(u);
            // e^(kx) is (1 + u)^k/w^k for k positive and (1 - u)^(-k)/w^(-k) for k negative, with
            // w for sqrt(1 - u^2): the integrand is then rational in u and w, and w^2 is
            // 1 - u^2. Every quotient -- each radicand, and the whole -- is brought over one
            // bar with its numerator and denominator reduced to A + B w, and the denominator
            // cleared of w by its conjugate, so that what is left is E/G + F w/G with E, F
            // and G polynomials in u; w is the root again at the end.
            var rewritten = expr.Replace(node =>
                exponentials.TryGetValue(node, out var k)
                    ? MathS.Pow(k.Sign > 0 ? 1 + u : 1 - u, Number.Integer.Create(k.Abs())) * MathS.Pow(w, Number.Integer.Create(k.Abs().Negate()))
                    : node);
            if (rewritten.ContainsNode(x))
                return null;
            Entity? Rationalized(Entity e)
            {
                var (top, bottom) = Functions.SingleQuotient.Of(Functions.SingleQuotient.Combine(e));
                if (Reduced(top) is not var (a, b) || Reduced(bottom) is not var (c, d))
                    return null;
                if (TreeAnalyzer.IsZero(d))
                    return Over(a, b, c);
                // (A + Bw)(C - Dw) over C^2 - D^2 w^2.
                return Over(Functions.PartialFractions.Bare((a * c - b * d * oneMinusUSquared).Expand().InnerSimplified),
                            Functions.PartialFractions.Bare((b * c - a * d).Expand().InnerSimplified),
                            Functions.PartialFractions.Bare((c * c - d * d * oneMinusUSquared).Expand().InnerSimplified));
            }
            // E/G + F w/G with the greatest common divisor of the three divided out: the
            // conjugate squares every degree, and `sinh(2x)` arrives as
            // `(4u - 4u^3)/(2 - 4u^2 + 2u^4)`, which is `2u/(1 - u^2)`. Over the rationals in
            // u alone, by Euclid; with a symbol in a coefficient the quotients are left as
            // they are.
            Entity Over(Entity e, Entity f, Entity g)
            {
                if (AsRationalPolynomial(e) is { } eP && AsRationalPolynomial(f) is { } fP && AsRationalPolynomial(g) is { } gP && !gP.IsZero)
                {
                    var divisor = Gcd(Gcd(eP, fP), gP);
                    if (!divisor.IsConstant)
                    {
                        eP = Quotient(eP, divisor);
                        fP = Quotient(fP, divisor);
                        gP = Quotient(gP, divisor);
                    }
                    // Monic below the bar.
                    var leading = gP.Leading;
                    var gEntity = gP.ScaleBy(ERational.One.Divide(leading)).ToEntity(u);
                    var rationalPart = eP.IsZero ? Number.Integer.Zero : eP.ScaleBy(ERational.One.Divide(leading)).ToEntity(u) / gEntity;
                    return fP.IsZero ? rationalPart : rationalPart + fP.ScaleBy(ERational.One.Divide(leading)).ToEntity(u) / gEntity * w;
                }
                var rational = TreeAnalyzer.IsZero(e) ? Number.Integer.Zero : e / g;
                return TreeAnalyzer.IsZero(f) ? rational : rational + f / g * w;
            }
            RationalPolynomial? AsRationalPolynomial(Entity polynomial)
            {
                if (TreeAnalyzer.IsZero(polynomial))
                    return RationalPolynomial.Zero;
                if (!TreeAnalyzer.TryGetPolynomial(polynomial, u, out var monomials) || monomials.Keys.Max() is not { } top)
                    return null;
                var coefficients = new ERational[top.ToInt32Checked() + 1];
                for (var i = 0; i < coefficients.Length; i++)
                    coefficients[i] = ERational.Zero;
                foreach (var pair in monomials)
                {
                    if (pair.Value.Evaled is not Number.Rational coefficient)
                        return null;
                    coefficients[pair.Key.ToInt32Checked()] = coefficient.ERational;
                }
                return RationalPolynomial.Create(coefficients);
            }
            static RationalPolynomial Gcd(RationalPolynomial left, RationalPolynomial right)
            {
                while (!right.IsZero)
                {
                    left.TryDivide(right, out _, out var remainder);
                    (left, right) = (right, remainder);
                }
                return left;
            }
            static RationalPolynomial Quotient(RationalPolynomial dividend, RationalPolynomial divisor)
                => dividend.TryDivide(divisor, out var quotient, out _) ? quotient : dividend;
            // A polynomial in u and w as A + B w, by w^2 = 1 - u^2.
            (Entity, Entity)? Reduced(Entity polynomial)
            {
                // Bare: the simplification attaches `provided not 1 - u^2 = 0` where a power of
                // w cancelled, and a condition is nothing a polynomial reader reads.
                var expanded = Functions.PartialFractions.Bare(polynomial.Expand().InnerSimplified);
                if (!expanded.ContainsNode(w))
                    return (expanded, Number.Integer.Zero);
                if (!TreeAnalyzer.TryGetPolynomial(expanded, w, out var byPowerOfW))
                    return null;
                Entity even = Number.Integer.Zero;
                Entity odd = Number.Integer.Zero;
                foreach (var pair in byPowerOfW)
                {
                    var power = pair.Key.ToInt32Checked();
                    var lifted = pair.Value * MathS.Pow(oneMinusUSquared, power / 2);
                    if (power % 2 == 0) even += lifted;
                    else odd += lifted;
                }
                return (Functions.PartialFractions.Bare(even.Expand().InnerSimplified), Functions.PartialFractions.Bare(odd.Expand().InnerSimplified));
            }
            // A polynomial in u that is a constant times a power of `1 - u^2`, coefficient by
            // coefficient; the power is zero for a constant.
            bool IsAConstantTimesAPowerOfOneMinusUSquared(Entity polynomial, out Entity constant, out int power)
            {
                constant = polynomial;
                power = 0;
                if (!polynomial.ContainsNode(u))
                    return true;
                if (!TreeAnalyzer.TryGetPolynomial(polynomial, u, out var monomials) || monomials.Keys.Max() is not { } degree || !degree.IsEven
                    || !TreeAnalyzer.TryGetPolynomial(MathS.Pow(oneMinusUSquared, degree.ToInt32Checked() / 2).Expand(), u, out var reference)
                    || reference.Count != monomials.Count)
                    return false;
                Entity? ratio = null;
                foreach (var pair in monomials)
                {
                    if (!reference.TryGetValue(pair.Key, out var expected))
                        return false;
                    var here = (pair.Value / expected).InnerSimplified;
                    if (ratio is null)
                        ratio = here;
                    else if (ratio != here)
                        return false;
                }
                if (ratio is null || ratio.ContainsNode(u))
                    return false;
                constant = ratio;
                power = degree.ToInt32Checked() / 2;
                return true;
            }
            // Each root, its radicand rationalised, stands as an atom of its own through the
            // polynomial work -- the gcd cancellation read `sqrt(2u/(1 - u^2))` as a
            // polynomial in u and answered `0 provided ...` -- and is put back at the end.
            // A radicand over a power of `1 - u^2` gives that power up to w, where it meets the
            // rest: `sqrt(sinh(2x))` is `sqrt(2u/(1 - u^2))`, which is `sqrt(2u)/w`, and beside
            // the `1/w` of the cosh in front the roots of `1 - u^2` cancel and `sqrt(2u)` is
            // all that is left -- a root of a polynomial, one substitution. And a radicand
            // that is a constant times a power of `1 - u^2` and of w outright -- `sech(x)` is
            // `w`, `cosh(x)` is `w/(1 - u^2)` -- is a root of `1 - u^2` of another index, an
            // atom of its own.
            var atoms = new Dictionary<Entity, Entity>();
            var combined = rewritten.Replace(node =>
            {
                if (node is not Powf(var radicand, Number.Rational root) || root is Number.Integer || !radicand.ContainsNode(u) && !radicand.ContainsNode(w)
                    || Rationalized(radicand) is not { } rationalRadicand)
                    return node;
                var atom = Variable.CreateUnique(expr, $"r{atoms.Count}_tanh");
                // Constant times w^k times a power of 1 - u^2: the whole radicand as one power.
                Entity? constantFactor = Number.Integer.One;
                var ofOneMinusUSquared = ERational.Zero;
                foreach (var factor in Mulf.LinearChildren(Functions.PartialFractions.Bare(rationalRadicand.InnerSimplified)))
                {
                    var (factorBase, factorPower) = factor is Powf(var inner, Number.Integer whole) ? (inner, whole.EInteger) : (factor, EInteger.One);
                    if (factorBase == w)
                        ofOneMinusUSquared = ofOneMinusUSquared.Add(ERational.Create(factorPower, EInteger.FromInt32(2)));
                    else if (!factorBase.ContainsNode(w) && IsAConstantTimesAPowerOfOneMinusUSquared(factorBase, out var constant, out var power))
                    {
                        ofOneMinusUSquared = ofOneMinusUSquared.Add(ERational.FromEInteger(factorPower.Multiply(EInteger.FromInt32(power))));
                        constantFactor = constantFactor * MathS.Pow(constant, Number.Integer.Create(factorPower));
                    }
                    else
                    {
                        constantFactor = null;
                        break;
                    }
                }
                if (constantFactor is not null)
                {
                    var exponent = Number.Rational.Create(ofOneMinusUSquared.Multiply(root.ERational).ToLowestTerms());
                    atoms[atom] = MathS.Pow(constantFactor.InnerSimplified, root) * (exponent == Number.Integer.Zero ? Number.Integer.One : MathS.Pow(oneMinusUSquared, exponent));
                    return atom;
                }
                var (radicandTop, radicandBottom) = Functions.SingleQuotient.Of(rationalRadicand);
                Entity ofW = Number.Integer.One;
                if (!radicandTop.ContainsNode(w) && !radicandBottom.ContainsNode(w)
                    && IsAConstantTimesAPowerOfOneMinusUSquared(radicandBottom, out var bottomConstant, out var m) && m > 0)
                {
                    var wPower = root.ERational.Multiply(ERational.FromInt32(-2 * m)).ToLowestTerms();
                    if (wPower.Denominator.Equals(EInteger.One))
                    {
                        rationalRadicand = bottomConstant.Evaled is Number.Rational number
                            ? (radicandTop * Number.Rational.Create(ERational.One.Divide(number.ERational))).InnerSimplified
                            : (radicandTop / bottomConstant).InnerSimplified;
                        ofW = MathS.Pow(w, Number.Integer.Create(wPower.Numerator));
                    }
                }
                atoms[atom] = rationalRadicand.ContainsNode(w)
                    ? MathS.Pow(rationalRadicand.Substitute(w, MathS.Sqrt(oneMinusUSquared)), root)
                    : MathS.Pow(rationalRadicand, root);
                return ofW == Number.Integer.One ? atom : atom * ofW;
            });
            // The atoms as factors of the whole, and the rest rationalised apart from them: an
            // atom inside a sum is declined, and an atom among the factors would otherwise be
            // squared by the conjugate and never reduced.
            var (wholeTop, wholeBottom) = Functions.SingleQuotient.Of(Functions.SingleQuotient.Combine(combined / oneMinusUSquared));
            Entity atomFactors = Number.Integer.One;
            Entity? restTop = null;
            Entity? restBottom = null;
            foreach (var (side, below) in new[] { (wholeTop, false), (wholeBottom, true) })
                foreach (var factor in Mulf.LinearChildren(side))
                {
                    var factorBase = factor is Powf(var inner, Number.Integer) ? inner : factor;
                    if (atoms.ContainsKey(factorBase))
                        atomFactors = below ? atomFactors / factor : atomFactors * factor;
                    else if (atoms.Keys.Any(factor.ContainsNode))
                        return null;
                    else if (below)
                        restBottom = restBottom is null ? factor : restBottom * factor;
                    else
                        restTop = restTop is null ? factor : restTop * factor;
                }
            if (Rationalized((restTop ?? Number.Integer.One) / (restBottom ?? Number.Integer.One)) is not { } rationalised)
                return null;
            var integrand = (rationalised * atomFactors).Substitute(w, MathS.Sqrt(oneMinusUSquared));
            foreach (var pair in atoms)
                integrand = integrand.Substitute(pair.Key, pair.Value);
            integrand = integrand.InnerSimplified;
            if (integrand.ContainsNode(x) || integrand.ContainsNode(w) || integrand.Nodes.Any(node => node == MathS.NaN))
                return null;
            if (Integration.ComputeAsAQuestionOfItsOwn(integrand, u, integrateByParts) is not { } result)
                return null;
            var answer = result.Substitute(u, MathS.Hyperbolic.Tanh(y));
            if (answer.Nodes.Any(node => node == MathS.NaN))
                return null;
            var scaled = dxOverDy == Number.Integer.One ? answer : answer * dxOverDy;
            // Checked where a symbol is involved: Rubi's `sqrt(a + b sech(x)) tanh(x)^5` came
            // back with `sqrt(a + i b sqrt(u^2 - 1))` in it, the root of `1 - u^2` written
            // through the imaginary unit by a rule below, and wrong on the reals.
            // https://github.com/asc-community/AngouriMath/issues/1370
            if (expr.Vars.Any(symbol => symbol != x) && !Functions.PartialFractions.DerivativeHoldsAtSampledPoints(scaled, expr, x))
                return null;
            return scaled;
        }

        /// <summary>
        /// An integrand carrying one symbolic parameter, scaled by it — <c>x = c t</c> — so that
        /// what is left to integrate has the variable alone in it.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>1/(a^3 + x^3)</c> had no antiderivative, and neither did <c>1/(a^4 - x^4)</c>,
        /// <c>1/(a^5 + x^5)</c> or any of the family <c>1/(x^k (a^n ± x^n))</c> — while
        /// <c>1/(8 + x^3)</c> and <c>1/(16 - x^4)</c> are answered at once. The parameter is the
        /// whole difference: the rational rules read a denominator as a polynomial <b>over the
        /// rationals</b>, and <c>a^3</c> is not a rational coefficient, so the factoring that
        /// answers <c>x^3 + 8</c> has nothing to work with on <c>x^3 + a^3</c>.
        /// </para>
        /// <para>
        /// <b>Scaling puts the parameter where it does no harm.</b> With <c>x = c t</c> and
        /// <c>dx = c dt</c>, a homogeneous integrand becomes <c>c^k</c> times a function of
        /// <c>t</c> alone: <c>1/(a^3 + x^3)</c> becomes <c>a^(-2) / (1 + t^3)</c>, whose
        /// denominator has integer coefficients again. The parameter comes back out as a constant
        /// factor and the answer is read at <c>t = x/c</c>.
        /// </para>
        /// <para>
        /// <b>Homogeneity is checked rather than assumed</b>, and it is what makes this
        /// terminate. The scaled integrand has to be exactly <c>c^k h(t)</c> for a whole
        /// <c>k</c>, which is verified by dividing it out; only <c>h</c> is handed on, so the
        /// sub-problem has one variable and cannot be scaled again. Without that check the rule
        /// would hand on something still carrying <c>c</c> and scale it once more at every level.
        /// </para>
        /// <para>
        /// <b>What it is not defined at.</b> <c>c = 0</c> is not a scaling, and the answer this
        /// produces is undefined there rather than wrong — <c>a^(-2) G(x/a)</c> has no value at
        /// <c>a = 0</c>, which is the honest report for a substitution that does not exist. The
        /// integrand itself is a different function there (<c>1/(a^3 + x^3)</c> is <c>1/x^3</c>),
        /// and is answered on its own if asked that way.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveByScalingTheVariable(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            // One parameter, and it is the scale. Two would each need their own and neither
            // clears the other; none means there is nothing in the way to begin with.
            Entity.Variable? scale = null;
            foreach (var variable in expr.Vars)
            {
                if (variable == x)
                    continue;
                if (scale is not null)
                    return null;
                scale = variable;
            }
            if (scale is null)
                return null;

            // dx = c dt.
            var t = Variable.CreateUnique(expr, "u_scale");
            var scaled = (expr.Substitute(x, scale * t) * scale).Simplify();
            if (scaled.ContainsNode(x))
                return null;

            // c^k h(t), with h read off at c = 1 and k found by dividing it out. A scaled
            // integrand that is not of that shape is not homogeneous and is declined.
            // The integrand with the scale set to one is the candidate h(t), and what is left
            // over when the scaled integrand is divided by it is the candidate factor. Read off
            // rather than searched for: if the two do separate, the quotient *is* the factor.
            var withoutScale = scaled.Substitute(scale, Number.Integer.One).Simplify();
            if (withoutScale.ContainsNode(scale) || withoutScale == Number.Integer.Zero)
                return null;

            var factor = (scaled / withoutScale).Simplify();
            // Collapsing the quotient attaches the condition that the denominator it cleared is
            // non-zero. That denominator is the integrand's own, so the condition says where the
            // integrand is defined and nothing about this rewrite; it is dropped for the same
            // reason the other rewrites drop theirs.
            if (factor is Providedf(var withoutCondition, _))
                factor = withoutCondition;

            // The whole of the homogeneity check. If the scale did not separate, what is left
            // still mentions t, and handing that on would carry the parameter into the
            // sub-problem -- which could then be scaled again, at every level, without end.
            if (factor.ContainsNode(t) || factor.ContainsNode(x))
                return null;

            if (Integration.ComputeIndefiniteIntegral(withoutScale, t, integrateByParts) is not { } result)
                return null;

            var answer = (factor * result).Substitute(t, x / scale);
            return answer.Nodes.Any(node => node == MathS.NaN) ? null : answer;
        }

        /// <summary>
        /// A power whose base holds the variable, written out and integrated term by term.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>(2x + 3x^2)^3</c> had no antiderivative, and it is a polynomial. The rule for a
        /// power integrates <c>x^n</c> and asks that the base <em>be</em> the variable, so a base
        /// that merely contains it — any polynomial but a bare <c>x</c> — matched nothing, and
        /// nothing else in the chain writes a power out. <c>(1 + x)^3</c> was answered only
        /// because the linear substitution reads it.
        /// </para>
        /// <para>
        /// <b>It runs last</b>, after every rule that can answer a power in its own terms.
        /// Expanding throws away whatever structure the power had — <c>(1 + x^2)^2</c> is
        /// answered as a power, and writing it out first would replace that answer with a longer
        /// one saying the same thing.
        /// </para>
        /// <para>
        /// <b>Only a positive whole exponent</b>, where writing it out is a finite identity. A
        /// negative one is a quotient and belongs to partial fractions; a fractional one is a
        /// radical and does not expand at all.
        /// </para>
        /// <para>
        /// The expansion is bounded by <see cref="MathS.Settings.MaxExpansionTermCount"/> like
        /// every other, and the rule declines where the expansion is not a sum — nothing was
        /// written out, so handing it on would ask the same question again.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveByExpandingAPower(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            if (expr is not Powf(var @base, Number.Integer power))
                return null;
            if (power.EInteger.CompareTo(EInteger.One) <= 0)
                return null;
            if (!@base.ContainsNode(x))
                return null;

            var written = expr.Expand();
            if (written is not Sumf and not Minusf)
                return null;

            return Integration.ComputeIndefiniteIntegral(written, x, integrateByParts);
        }

        /// <summary>
        /// Several square roots of polynomials in the variable, written as one:
        /// <c>sqrt(1 + x^2) sqrt(1 - x^2)</c> is <c>sqrt(1 - x^4)</c>, and a root below the bar
        /// is a root above it over its own base, <c>1/sqrt(B) = sqrt(B)/B</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>x/(sqrt(1 + x^2) sqrt(1 - x^2))</c> had no antiderivative and <c>x/sqrt(1 - x^4)</c>
        /// is one substitution. Nothing reads two radicals: the substitution wants one subtree
        /// to replace, Euler's rule wants one root of one quadratic, and each rule looked at
        /// this integrand and saw two of what it takes one of. It is what by parts leaves from
        /// <c>arcsin(x)/(1 + x^2)^(3/2)</c> and from <c>ln(x + sqrt(1 + x^2))/(1 - x^2)^(3/2)</c>,
        /// and from every other pairing of an inverse function's radical with the one in
        /// the power it stands over.
        /// </para>
        /// <para>
        /// <b>When the identity holds.</b> On the principal branch, <c>sqrt(P) sqrt(Q) = sqrt(PQ)</c>
        /// whenever at most one of <c>P</c> and <c>Q</c> is negative: with both negative the
        /// left is <c>i sqrt|P| * i sqrt|Q| = -sqrt|PQ|</c> and the right is <c>+sqrt|PQ|</c>.
        /// For <c>k</c> roots the same count applies -- <c>i^k</c> against <c>i^(k mod 2)</c> --
        /// so the rule asks whether there is a real <c>x</c> at which two of the bases are
        /// negative, and only rewrites when there is not. The bases are polynomials with
        /// numeric coefficients; their real roots cut the line into intervals on each of which
        /// every base keeps its sign, so the count at one point of each interval is the count
        /// on the interval. Where the solver cannot give the roots, the rule declines.
        /// </para>
        /// <para>
        /// A rewriting rule, unscoped: what it hands on has strictly fewer radicals than what
        /// it was given, and it is asked one level down more often than at the top, since the
        /// two-radical shape is what a by-parts step produces.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        /// <summary>
        /// A square root of a polynomial with a repeated factor, the factor taken out of the
        /// root: <c>sqrt(9 + 3x - 5x^2 + x^3)</c> is <c>sqrt((x - 3)^2 (x + 1))</c>, which is
        /// <c>|x - 3| sqrt(x + 1)</c>, and the modulus is <c>sgn(x - 3) (x - 3)</c> -- a constant
        /// sign on each side of the root, carried through the integration as a symbol and
        /// written back as the sign. Timofeev's <c>1/sqrt(9 + 3x - 5x^2 + x^3)</c> is
        /// <c>sgn(x - 3) ln(...)</c> that way, where the radical of a cubic was elliptic to
        /// every rule that read it.
        /// </summary>
        /// <remarks>
        /// Square roots only: an odd root of a power is the power of the root on the reals
        /// with no sign to keep. The sign is a symbol to the integration, so a symbol's
        /// square is one and its odd powers the symbol, and the answer holds wherever the
        /// factor is not zero, where the integrand is singular anyway. At the top only: a
        /// substitution's own variable is one it knows the sign of.
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveByTakingASquareFactorOutOfARoot(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            // With a sign at the top only: a substitution's variable below is one the
            // substitution knows the sign of -- `u = (x + 1)^(1/6)` is not negative -- and a
            // sign written for it here is one nothing can differentiate. A factor positive for
            // every real x needs no sign and comes out at any depth: `sqrt((u^2 + 1)^2 u)` is
            // `(u^2 + 1) sqrt(u)`, what the tangent substitution makes of `sqrt(sin(x)/cos(x)^5)`.
            var atTheTop = Integration.AnsweringTheQuestionAsked;
            // The signs are constants, one on each side of a factor's root, so they come out
            // in front of the integral: the integrand is asked without them, and the answer
            // is the product of the signs to their powers, an even power being one.
            Entity signs = Number.Integer.One;
            var rewritten = expr.Replace(node =>
            {
                if (node is not Powf(var radicand, Number.Rational exponent) || exponent is Number.Integer
                    || !exponent.ERational.Denominator.Equals(EInteger.FromInt32(2)) || !radicand.ContainsNode(x))
                    return node;
                if (Functions.PolynomialFactorization.FactorComplete(radicand, x) is not { } factorization
                    || factorization.Parts.All(part => part.Multiplicity < 2))
                    return node;
                if (factorization.Parts.Count > 6)
                    return node;
                // Out of the root: each factor to half its multiplicity, rounded down, with a
                // sign where that half is odd; under it: the constant and what is left.
                Entity outside = Number.Integer.One;
                Entity inside = Number.Rational.Create(factorization.Constant);
                var numerator = exponent.ERational.Numerator;
                foreach (var part in factorization.Parts)
                {
                    var factor = part.Factor.ToEntity(x);
                    var half = part.Multiplicity / 2;
                    if (half > 0)
                    {
                        // |f|^(half n) is f^(half n) sgn(f)^(half n).
                        var withASign = !numerator.Multiply(EInteger.FromInt32(half)).IsEven && !IsPositiveForReal(factor, x);
                        if (withASign && !atTheTop)
                        {
                            inside = inside * MathS.Pow(factor, 2 * half);
                            if (part.Multiplicity % 2 == 1)
                                inside = inside * factor;
                            continue;
                        }
                        outside = outside * (half == 1 ? factor : MathS.Pow(factor, half));
                        if (withASign)
                            signs = signs * MathS.Signum(factor);
                    }
                    if (part.Multiplicity % 2 == 1)
                        inside = inside * factor;
                }
                if (!outside.ContainsNode(x))
                    return node;
                return MathS.Pow(outside, Number.Integer.Create(numerator)) * MathS.Pow(inside, exponent);
            });
            if (rewritten == expr)
                return null;
            if (Integration.ComputeAsAQuestionOfItsOwn(rewritten, x, integrateByParts) is not { } result)
                return null;
            var answer = signs == Number.Integer.One ? result : signs * result;
            return answer.Nodes.Any(node => node == MathS.NaN) ? null : answer;
        }

        /// <summary>
        /// A fractional power of a constant times an even power of a function of x, the
        /// function taken out of the power with its sign: <c>(a sin(x)^2)^(5/2)</c> is
        /// <c>a^(5/2) |sin(x)|^5</c>, which is <c>a^(5/2) sgn(sin(x)) sin(x)^5</c> -- the sign
        /// a constant between the sine's zeros, carried through the integration as a symbol
        /// and written back. Rubi's <c>x sqrt(sin(x)^2)</c> is <c>sgn(sin(x)) (sin(x) - x cos(x))</c>
        /// that way, and <c>1/(csc(x)^2)^(7/2)</c> is <c>sgn(csc(x))</c> times the seventh power
        /// of the sine, where the substitutions read the root of the square as a modulus and
        /// answered in powers of it.
        /// </summary>
        /// <remarks>
        /// An even power of a real function is not negative, so <c>(c q)^p = c^p q^p</c> holds
        /// for the principal powers whatever <c>c</c> is, and <c>(f^(2k))^p</c> is <c>|f|^(2kp)</c>.
        /// Whole products of the exponents only, so that what is handed on is a whole power of
        /// the function; a polynomial radicand is the rule above's, which comes first. At the
        /// top only, as there: a substitution's variable is one it knows the sign of, and a
        /// sign written for it is one nothing differentiates.
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveByTakingAFunctionOutOfAPowerOfItsEvenPower(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            if (!Integration.AnsweringTheQuestionAsked)
                return null;
            Entity signs = Number.Integer.One;
            var rewritten = expr.Replace(node =>
            {
                if (node is not Powf(var radicand, Number.Rational exponent) || exponent is Number.Integer || !radicand.ContainsNode(x))
                    return node;
                // The radicand read as a constant times even whole powers of functions of x,
                // each power whole once multiplied by the exponent; anything else is not this
                // rule's.
                Entity constant = Number.Integer.One;
                var functions = new List<(Entity function, EInteger power)>();
                var top = radicand;
                if (radicand is Divf(var above, var below) && !below.ContainsNode(x))
                {
                    constant = Number.Integer.One / below;
                    top = above;
                }
                foreach (var factor in Mulf.LinearChildren(top))
                {
                    if (!factor.ContainsNode(x))
                    {
                        constant = constant * factor;
                        continue;
                    }
                    // A real function: a fractional power of one is not, where its base is
                    // negative, and its even power is then not the square of a modulus.
                    if (factor is not Powf(var function, Number.Integer degree) || !degree.EInteger.IsEven || degree.EInteger.Sign <= 0
                        || TreeAnalyzer.TryGetPolynomial(function, x, out _) || function is Powf(_, not Number.Integer))
                        return node;
                    var product = exponent.ERational.Multiply(ERational.FromEInteger(degree.EInteger)).ToLowestTerms();
                    if (!product.Denominator.Equals(EInteger.One))
                        return node;
                    functions.Add((function, product.Numerator));
                }
                if (functions.Count == 0)
                    return node;
                Entity outside = constant == Number.Integer.One ? Number.Integer.One : MathS.Pow(constant, exponent);
                foreach (var (function, power) in functions)
                {
                    // |f|^n is f^n for an even n and sgn(f) f^n for an odd one.
                    if (!power.IsEven)
                        signs = signs * MathS.Signum(function);
                    outside = outside * MathS.Pow(function, Number.Integer.Create(power));
                }
                return outside;
            });
            if (rewritten == expr)
                return null;
            if (Integration.ComputeAsAQuestionOfItsOwn(rewritten, x, integrateByParts) is not { } result)
                return null;
            var answer = signs == Number.Integer.One ? result : signs * result;
            return answer.Nodes.Any(node => node == MathS.NaN) ? null : answer;
        }

        /// <summary>
        /// A denominator factor that is a sum or difference of two square roots of
        /// polynomials, multiplied above and below by its conjugate:
        /// <c>(1 + x)/(sqrt(x^2 + 2x + 4) - sqrt(x^2 + x + 1))</c> is
        /// <c>(1 + x)(sqrt(x^2 + 2x + 4) + sqrt(x^2 + x + 1))/(x + 3)</c>, the product of the
        /// pair being the difference of the radicands -- and each of the two terms is then a
        /// root of a quadratic beside a rational function, Euler's. Timofeev's.
        /// </summary>
        /// <remarks>
        /// Two roots only: a polynomial beside one root, <c>x + sqrt(x^2 + 1)</c>, is Euler's
        /// substitution itself, and answered more shortly as that. The conjugate is nonzero
        /// wherever the factor is, and <c>(a - b)(a + b) = a^2 - b^2</c> holds for the
        /// principal roots as for any values, so nothing is assumed about signs. Once: the
        /// respelling has no such factor left.
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveByRationalisingASumOfRoots(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            if (!TryReadAsQuotient(expr, out var numerator, out var denominator) || !denominator.ContainsNode(x))
                return null;
            Entity? conjugate = null;
            Entity? product = null;
            Entity rest = Number.Integer.One;
            foreach (var factor in Mulf.LinearChildren(denominator))
            {
                if (conjugate is null && factor is Sumf or Minusf && Sumf.LinearChildren(factor).ToList() is { Count: 2 } terms
                    && terms.All(term => IsARootOfAPolynomial(term)))
                {
                    // The conjugate flips the sign of the second term; the product is the
                    // difference of the squares, each square a polynomial.
                    var first = terms[0];
                    var second = terms[1];
                    conjugate = first - second;
                    product = (SquareOf(first) - SquareOf(second)).InnerSimplified;
                    // Not the same radicand twice: the difference is zero, the factor is zero
                    // or a multiple of one root, and neither is this rule's.
                    if (product.Evaled is Number.Complex { IsZero: true } || product.Simplify().Evaled is Number.Complex { IsZero: true })
                        return null;
                    continue;
                }
                rest = rest * factor;
            }
            if (conjugate is null || product is null)
                return null;
            var respelled = Functions.PartialFractions.Bare(numerator * conjugate / (product * rest));
            return Integration.ComputeAsAQuestionOfItsOwn(respelled, x, integrateByParts);

            // The radicand need not be a polynomial in x: Timofeev's
            // `cos(3x)/(sqrt(3 cos(x)^2 - sin(x)^2) - sqrt(8 cos(x)^2 - 1))` has the difference
            // of the radicands, `4 cos(x)^2` by Pythagoras, below once it is respelled, and
            // each root beside it is a root of a quadratic in the sine under `u = sin(x)`.
            bool IsARootOfAPolynomial(Entity term)
            {
                var (coefficient, root) = term is Mulf(var l, var r) && !l.ContainsNode(x) ? (l, r) : (Number.Integer.One as Entity, term);
                return root is Powf(var radicand, Number.Rational half) && half.ERational.Denominator.Equals(EInteger.FromInt32(2))
                    && half.ERational.Numerator.Equals(EInteger.One) && radicand.ContainsNode(x)
                    && !coefficient.ContainsNode(x);
            }

            Entity SquareOf(Entity term)
            {
                var (coefficient, root) = term is Mulf(var l, var r) && !l.ContainsNode(x) ? (l, r) : (Number.Integer.One as Entity, term);
                return root is Powf(var radicand, Number.Rational half) && half.ERational.Numerator.Equals(EInteger.One) && half.ERational.Denominator.Equals(EInteger.FromInt32(2))
                    ? MathS.Sqr(coefficient) * radicand
                    : MathS.Sqr(term);
            }
        }

        internal static Entity? SolveByCombiningRadicals(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            // A secant or cosecant under a root is the reciprocal of a cosine or sine there:
            // `sqrt(sec(x)^4 - 1)` is a root of `(1 - cos(x)^4)/cos(x)^4`, a quotient whose
            // denominator is an even power, and comes apart as `sqrt(1 - cos(x)^4)/cos(x)^2`;
            // as written it is a root of a sum of secants that no rule reads.
            if (expr.Nodes.Any(node => node is Powf(var radicalBase, Number.Rational r) && r is not Number.Integer
                    && radicalBase.Nodes.Any(inner => inner is Secantf or Cosecantf)))
                expr = expr.Replace(node => node switch
                {
                    Secantf(var argument) => 1 / MathS.Cos(argument),
                    Cosecantf(var argument) => 1 / MathS.Sin(argument),
                    _ => node,
                });
            // Two square roots holding the variable, or there is nothing to combine; counted
            // before any of the rewriting below is paid for, since this runs on every
            // sub-integrand of the chain.
            // A root of a quotient counts as the two roots it splits into -- a sum with a
            // quotient in it, `1/cos(x)^4 - 1`, being a quotient once combined.
            if (expr.Nodes.Sum(node => node is Powf(var @base, Number.Rational half)
                    && half is not Number.Integer && half.ERational.Denominator.Equals(EInteger.FromInt32(2))
                    && @base.ContainsNode(x)
                        ? (@base is Divf || Functions.SingleQuotient.Of(AsOneQuotient(@base)).Denominator != Number.Integer.One ? 2 : 1)
                        : 0) < 2)
                return null;
            var rewritten = CombineRadicalsIn(expr, x);
            // The rewriting is exact and the rewritten integrand is the question asked in
            // another spelling, so at the top it is asked as one: the rules scoped to the
            // question -- the parity extension under a substitution, for one, which
            // `sec(x)/sqrt(sec(x)^4 - 1)` needs after the secant is written as a cosine here --
            // answer it then and not one level down.
            if (rewritten != expr
                && (Integration.AnsweringTheQuestionAsked
                    ? Integration.ComputeAsAQuestionOfItsOwn(rewritten, x, integrateByParts)
                    : Integration.ComputeIndefiniteIntegral(rewritten, x, integrateByParts)) is { } answer)
                return answer;
            // Two roots that combine only up to a sign, `sqrt(x - 1) sqrt(x + 1)`: the sign is
            // a constant between the real roots of the bases and goes in front of the
            // integral, at the top, where the answer is the caller's. After the exact
            // rewriting has had its turn, since a quotient with those two roots below may be
            // answered by the cancellation alone -- Hearn's
            // `(5x^4 sqrt(1 + x^3) - 3x^2 sqrt(1 - 2x + x^5) - 2 sqrt(1 + x^3))/(2 sqrt(1 + x^3) sqrt(1 - 2x + x^5))`
            // is, and combined under one root with the sign in front it ran the budget out;
            // so only where the one root is the only one left.
            if (Integration.AnsweringTheQuestionAsked && expr is Mulf or Divf
                && CombineRadicalsInAQuotient(SplitRootsOfQuotientsIn(expr, x), x, out var sign, withASign: true) is { } signed
                && sign is not null
                && signed.Nodes.Where(node => node is Powf(var @base, Number.Rational r) && r is not Number.Integer && @base.ContainsNode(x))
                    .Select(node => ((Powf)node).Base).Distinct().Count() == 1
                && Integration.ComputeAsAQuestionOfItsOwn(signed, x, integrateByParts) is { } result
                && !result.Nodes.Any(node => node == MathS.NaN))
                return sign * result;
            return null;
        }

        /// <summary>
        /// Every product or quotient in <paramref name="expr"/> with its square roots of
        /// polynomials combined into one, innermost first; and a small power of a sum holding
        /// such a root written out first, since <c>(sqrt(1 - x) + sqrt(1 + x))^2</c> is
        /// <c>2 + 2 sqrt(1 - x) sqrt(1 + x)</c>, which combines, and is not a shape any rule
        /// reads as it stands.
        /// </summary>
        private static Entity CombineRadicalsIn(Entity expr, Entity.Variable x)
            => SplitRootsOfQuotientsIn(expr, x).Replace(node => node is Mulf or Divf ? CombineRadicalsInAQuotient(node, x, out _) ?? node : node);

        /// <summary>
        /// Every root of a quotient of polynomials in <paramref name="expr"/> written as a
        /// quotient of roots, where that is exact: <c>sqrt(P/Q) = sqrt(P)/sqrt(Q)</c> on the
        /// principal branch unless <c>Q &lt; 0 &lt; P</c>, where the left is <c>i sqrt(P/|Q|)</c>
        /// and the right <c>-i sqrt(P/|Q|)</c> -- with both negative the two <c>i</c>s cancel and
        /// with <c>P</c> negative alone they agree. So the rule asks whether there is a real
        /// <c>x</c> with <c>Q(x) &lt; 0 &lt; P(x)</c>, on one point of each interval between the
        /// real roots, and splits only where there is not.
        /// </summary>
        /// <remarks>
        /// Charlwood's <c>arcsin(x/sqrt(1 - x^2))</c> by parts against one leaves
        /// <c>x (1 - x^2)^(-3/2)/sqrt((1 - 2x^2)/(1 - x^2))</c>, a root of a quotient that no
        /// rule reads; split, it is <c>x/((1 - x^2) sqrt(1 - 2x^2))</c>, which Euler answers.
        /// <c>1 - x^2 &lt; 0</c> only past <c>1</c>, where <c>1 - 2x^2</c> is negative too.
        /// </remarks>
        private static Entity SplitRootsOfQuotientsIn(Entity expr, Entity.Variable x)
            => expr.Replace(node =>
            {
                if (node is not Powf(var @base, Number.Rational power) || power is Number.Integer
                    || !power.ERational.Denominator.Equals(EInteger.FromInt32(2)) || !@base.ContainsNode(x))
                    return node;
                var (above, below) = Functions.SingleQuotient.Of(AsOneQuotient(@base));
                if (below == Number.Integer.One || !below.ContainsNode(x))
                    return node;
                // A denominator that is an even power of anything real is not negative, so the
                // split is exact whatever the numerator, and the root of the denominator is a
                // whole power where the exponents allow: `sqrt(N/cos(x)^4)` is `sqrt(N)/cos(x)^2`.
                if (Mulf.LinearChildren(below).All(factor => !factor.ContainsNode(x)
                        || factor is Powf(_, Number.Integer even) && even.EInteger.IsEven && even.EInteger.Sign > 0)
                    && power.ERational.Numerator.CanFitInInt32())
                {
                    var p = power.ERational.Numerator.ToInt32Unchecked();
                    Entity rootOfBelow = Number.Integer.One;
                    var whole = true;
                    foreach (var factor in Mulf.LinearChildren(below))
                    {
                        if (!factor.ContainsNode(x))
                        {
                            rootOfBelow *= MathS.Pow(factor, power);
                            continue;
                        }
                        var (f, twoK) = ((Powf)factor).DirectChildren is var children ? (children[0], ((Number.Integer)children[1]).EInteger.ToInt32Unchecked()) : default;
                        // (f^(2k))^(p/2) = |f|^(k p), and f^(k p) only for an even k p.
                        if ((twoK / 2 * p) % 2 != 0)
                        {
                            whole = false;
                            break;
                        }
                        rootOfBelow *= MathS.Pow(f, twoK / 2 * p);
                    }
                    if (whole)
                        return MathS.Pow(above, power) / rootOfBelow;
                }
                if (!TreeAnalyzer.TryGetPolynomial(above, x, out var aboveRead) || !TreeAnalyzer.TryGetPolynomial(below, x, out var belowRead)
                    || above.Vars.Any(v => v != x) || below.Vars.Any(v => v != x))
                    return node;
                // Quadratics at most on either side: those are what the rules behind this read,
                // and a root of a sextic set free is a search that ends nowhere --
                // `sin(x)/sqrt(1 - sin(x)^6)` under the tangent is one, and went from a
                // twenty-millisecond decline to a twenty-second one.
                static int Degree(Dictionary<EInteger, Entity> read) => read.Count == 0 ? 0 : read.Keys.Max()!.ToInt32Checked();
                if (Degree(aboveRead) > 2 || Degree(belowRead) > 2)
                    return node;
                // Kept as written, so that the base of the root below matches the same base
                // written beside it as a whole power, for the gathering that follows.
                if (!OnEveryRealInterval(new List<Entity> { above, below }, x, signs => !(signs[1] < 0 && signs[0] > 0)))
                    return node;
                return MathS.Pow(above, power) / MathS.Pow(below, power);
            });

        /// <summary>
        /// <paramref name="expr"/> as one quotient where it is a sum with a quotient among its
        /// terms -- <c>1/cos(x)^4 - 1</c> is <c>(1 - cos(x)^4)/cos(x)^4</c> -- and as it is
        /// otherwise, so that the combining is paid for only where it changes anything.
        /// </summary>
        private static Entity AsOneQuotient(Entity expr)
        {
            if (expr is not (Sumf or Minusf) || !Sumf.LinearChildren(expr).Any(term => term is Divf || term is Powf(_, Number.Integer { IsNegative: true })))
                return expr;
            // The numerator the combining writes, `1 - (1 - u^2)` for `1/(1 - u^2) - 1`, as the
            // polynomial it is, `u^2`: the even power of u below is read as written, and was
            // not read there -- Timofeev's `(tan(x) tan(2x))^(3/2)` under the tangent.
            var (above, below) = Functions.SingleQuotient.Of(Functions.SingleQuotient.Combine(expr));
            return above.Expand().InnerSimplified / below;
        }

        /// <summary>
        /// A factor of a product that is a small power of a sum holding a square root of a
        /// polynomial, written out and combined; any other factor as it is. Only as a factor:
        /// the same power under a root, `sqrt(1 - (sqrt(1 + x) - sqrt(x))^2)`, written out is
        /// a root of a sum with a root in it, no better than before and a search of its own.
        /// </summary>
        private static Entity WriteOutAPowerOfASumOfRadicals(Entity factor, Entity.Variable x)
        {
            if (factor is not Powf(Sumf or Minusf, Number.Integer power) || !power.EInteger.CanFitInInt32())
                return factor;
            var n = power.EInteger.ToInt32Unchecked();
            if (n is < 2 or > 4)
                return factor;
            var sum = factor.DirectChildren.First();
            if (!sum.Nodes.Any(node => TryReadASquareRootOfAPolynomial(node, x, out _, out _)))
                return factor;
            return CombineRadicalsIn(MathS.Pow(sum, n).Expand().InnerSimplified, x);
        }

        /// <summary>
        /// The product or quotient <paramref name="expr"/> with its two or more square roots of
        /// polynomials written as one, or <see langword="null"/> where there are fewer than two
        /// or the identity does not hold for them.
        /// </summary>
        private static Entity? CombineRadicalsInAQuotient(Entity expr, Entity.Variable x, out Entity? signInFront, bool withASign = false)
        {
            signInFront = null;
            // As one quotient, with a negative half-power read as the root it is below the bar,
            // and a sum factor above the bar cancelled against the same sum below it. That is
            // the derivative of `ln(x + sqrt(x^2 - 1))` as differentiation writes it,
            // `(1 + x/sqrt(x^2 - 1))/(x + sqrt(x^2 - 1))`, brought to the `1/sqrt(x^2 - 1)` it
            // is; by parts left it in the first spelling and nothing read it.
            var asQuotient = expr.Replace(node =>
                node is Powf(var @base, Number.Rational power) && power is not Number.Integer && power.ERational.Sign < 0
                    ? Number.Integer.One / MathS.Pow(@base, Number.Rational.Create(power.ERational.Negate()))
                    : node);
            var (numerator, denominator) = Functions.SingleQuotient.Of(asQuotient);
            var writtenOut = false;
            Entity WrittenOut(Entity side)
            {
                Entity built = Number.Integer.One;
                foreach (var factor in Mulf.LinearChildren(side))
                {
                    var written = WriteOutAPowerOfASumOfRadicals(factor, x);
                    writtenOut |= written != factor;
                    built = built == Number.Integer.One ? written : built * written;
                }
                return built;
            }
            numerator = WrittenOut(numerator);
            denominator = WrittenOut(denominator);
            var cancelled = CancelEqualSumFactors(numerator, denominator, out numerator, out denominator) | writtenOut;

            // Every factor with an odd half-power of a polynomial base is a whole power of the
            // base times one square root of it; the square roots are what combine. A whole
            // power of the same base, written beside it, is gathered in first: `P * P^(-1/2)`
            // is `P^(1/2)` exactly, since a whole power combines with any principal power of
            // the same base -- `(1 - x^2) sqrt(1 - 2x^2)/sqrt(1 - x^2)` is
            // `sqrt(1 - x^2) sqrt(1 - 2x^2)`, and what a root of a quotient splits into is
            // often beside its own base that way.
            var bases = new List<Entity>();
            Entity above = Number.Integer.One;
            Entity below = Number.Integer.One;
            var halves = new Dictionary<Entity, int>();      // base -> exponent in halves
            var halfExponents = new Dictionary<Entity, int>();
            var others = new List<(Entity Factor, bool Below)>();
            var gathered = false;
            foreach (var (side, isBelow) in new[] { (numerator, false), (denominator, true) })
                foreach (var factor in Mulf.LinearChildren(side))
                {
                    var sign = isBelow ? -1 : 1;
                    if (TryReadASquareRootOfAPolynomial(factor, x, out var @base, out var wholePower))
                    {
                        gathered |= halves.ContainsKey(@base);
                        halves[@base] = halves.TryGetValue(@base, out var so) ? so + sign * (2 * wholePower + 1) : sign * (2 * wholePower + 1);
                    }
                    else if (factor is Powf(var b, Number.Integer k) && k.EInteger.CanFitInInt32() && halves.ContainsKey(b))
                    {
                        halves[b] += sign * 2 * k.EInteger.ToInt32Unchecked();
                        gathered = true;
                    }
                    else if (factor.ContainsNode(x) && halves.ContainsKey(factor))
                    {
                        halves[factor] += sign * 2;
                        gathered = true;
                    }
                    else
                        others.Add((factor, isBelow));
                }
            // A whole power read before its root came: gathered on a second pass.
            foreach (var (factor, isBelow) in others.ToList())
            {
                var sign = isBelow ? -1 : 1;
                if (factor is Powf(var b, Number.Integer k) && k.EInteger.CanFitInInt32() && halves.ContainsKey(b))
                {
                    halves[b] += sign * 2 * k.EInteger.ToInt32Unchecked();
                    others.Remove((factor, isBelow));
                    gathered = true;
                }
                else if (factor.ContainsNode(x) && halves.ContainsKey(factor))
                {
                    halves[factor] += sign * 2;
                    others.Remove((factor, isBelow));
                    gathered = true;
                }
            }
            // **Over the irreducible factors**, where that cancels something: `sqrt(1 - x^4)`
            // over `sqrt(1 - x^2)` -- what by parts leaves from `x^3 arcsin(x)/sqrt(1 - x^4)` --
            // is `sqrt(1 + x^2)`, and the two roots as written share nothing. Each base with
            // an odd exponent is factored over the rationals, with the constant folded into
            // the first factor so that no `sqrt(-1)` is set free, and the root split over the
            // factors where that is exact -- at most one of them negative anywhere on the
            // reals; the whole polynomial factors beside them are factored the same way. The
            // refinement is kept only where it leaves fewer roots than it found, since
            // otherwise it only respells the integrand.
            if (TryRefineOverIrreducibleFactors(halves, others, x) is var (refinedHalves, refinedOthers))
            {
                halves = refinedHalves;
                others = refinedOthers;
                gathered = true;
            }
            foreach (var (factor, isBelow) in others)
                if (isBelow) below = below * factor;
                else above = above * factor;
            foreach (var pair in halves)
            {
                var n = pair.Value;
                if (n % 2 == 0)
                {
                    // A whole power after all: no root left of this base.
                    var whole = n / 2;
                    if (whole > 0) above = above * MathS.Pow(pair.Key, whole);
                    else if (whole < 0) below = below * MathS.Pow(pair.Key, -whole);
                    continue;
                }
                bases.Add(pair.Key);
                halfExponents[pair.Key] = n;
            }
            // Roots above and below the bar that do not combine as a product may combine as
            // the quotient they are: `sqrt(x^4 - 1)/sqrt(x^2 - 1)` has two negative bases inside
            // the unit interval, where `sqrt(P) sqrt(Q)` is `-sqrt(PQ)`, and is `sqrt(x^2 + 1)`
            // there all the same, since the two `i`s cancel in a quotient. Taken only where the
            // quotient under the one root cancels to a polynomial, so that what is handed on
            // has strictly fewer roots than what came in.
            // The signs of the bases on the intervals between their real roots, found once
            // for both questions: the roots are a solve per base, and asking twice on every
            // sub-integrand of a search that declines was three seconds of one.
            var signTable = bases.Count >= 2 ? SignsOnEveryRealInterval(bases, x) : null;
            var combineAsAProduct = signTable is not null && signTable.All(signs => signs.Count(sign => sign < 0) <= 1);
            // Two roots whose bases are both negative somewhere: `sqrt(x - 1) sqrt(x + 1)` is
            // `sqrt(x^2 - 1)` past 1 and `-sqrt(x^2 - 1)` before -1, the two `i`s multiplying
            // to -1. So `sqrt(P) sqrt(Q)` is `s sqrt(P Q)` with `s = (1 + sgn P + sgn Q - sgn P sgn Q)/2`,
            // which is -1 where both are negative and 1 elsewhere -- exactly, on every real
            // x -- and the same `s` serves the reciprocals and the quotient, being its own
            // reciprocal. The sign is a constant between the real roots of the bases, so it
            // goes in front of the integral; at the top only, where the caller can put it
            // there.
            if (withASign && !combineAsAProduct && bases.Count == 2 && signTable is not null
                && signTable.Any(signs => signs.All(s => s < 0)))
            {
                var (first, second) = (MathS.Signum(bases[0]), MathS.Signum(bases[1]));
                signInFront = (1 + first + second - first * second) / 2;
                combineAsAProduct = true;
            }
            if (bases.Count >= 2 && signTable is not null && !combineAsAProduct
                && CombinedAsAQuotient(bases, halfExponents, signTable, x) is { } asOneRoot)
            {
                var (rootOfAPolynomial, rest) = asOneRoot;
                // One root of each base is in the quotient; n = 2q + 1 above and n = 2q - 1 below.
                foreach (var @base in bases)
                {
                    var n = halfExponents[@base];
                    var q = n > 0 ? (n - 1) / 2 : (n + 1) / 2;
                    if (q > 0) above = above * MathS.Pow(@base, q);
                    else if (q < 0) below = below * MathS.Pow(@base, -q);
                }
                return Functions.PartialFractions.Bare(Functions.PartialFractions.Bare(above * rootOfAPolynomial * rest) / Functions.PartialFractions.Bare(below));
            }
            if (bases.Count < 2 || !combineAsAProduct)
            {
                // Nothing to combine, or roots that must not be: what was cancelled or
                // gathered is still worth handing on, each root on its own -- as the one
                // power `P^(n/2)` it is, since `sqrt(P)/P` beside a logarithm of `x + sqrt(P)` is
                // not the `1/sqrt(P)` the substitution for that logarithm reads.
                if (!cancelled && !gathered)
                    return null;
                foreach (var @base in bases)
                {
                    var n = halfExponents[@base];
                    if (n > 0) above = above * MathS.Pow(@base, Number.Rational.Create(n, 2));
                    else below = below * MathS.Pow(@base, Number.Rational.Create(-n, 2));
                }
                bases.Clear();
            }
            else
                foreach (var @base in bases)
                {
                    // n = 2q + 1 for every odd n, negative ones included: -1 = 2(-1) + 1.
                    var q = (halfExponents[@base] - 1) / 2;
                    if (q > 0) above = above * MathS.Pow(@base, q);
                    else if (q < 0) below = below * MathS.Pow(@base, -q);
                }

            Entity radical = Number.Integer.One;
            if (bases.Count > 0)
            {
                Entity product = Number.Integer.One;
                foreach (var @base in bases)
                    product = product * @base;
                radical = MathS.Sqrt(product.Expand().InnerSimplified);
            }
            var rewritten = ((above * radical).InnerSimplified / below.InnerSimplified).InnerSimplified;
            return rewritten is Providedf(var inner, _) ? inner : rewritten;
        }

        /// <summary>
        /// The bases with an odd exponent in halves, and the whole polynomial factors beside
        /// them, taken into their irreducible factors over the rationals and gathered again
        /// by factor; <see langword="null"/> where that leaves no fewer roots than it found.
        /// </summary>
        private static (Dictionary<Entity, int> Halves, List<(Entity Factor, bool Below)> Others)? TryRefineOverIrreducibleFactors(
            Dictionary<Entity, int> halves, List<(Entity Factor, bool Below)> others, Entity.Variable x)
        {
            if (!halves.Values.Any(n => n % 2 != 0))
                return null;
            var refined = new Dictionary<Entity, int>();
            var keptOthers = new List<(Entity Factor, bool Below)>();
            void Add(Entity factor, int n) => refined[factor] = refined.TryGetValue(factor, out var so) ? so + n : n;
            foreach (var pair in halves)
            {
                // The constant goes into whichever factor lets the split be exact: `1 - x^4` is
                // `-(x + 1)(x - 1)(x^2 + 1)`, and with the sign on `x + 1` two factors are
                // negative inside the unit interval, where with it on `x - 1` -- `(1 - x)` --
                // at most one is anywhere.
                var placements = IrreducibleFactorsWithTheSignPlaced(pair.Key, x);
                List<Entity>? parts = null;
                if (placements is not null && placements.Count > 0 && placements[0].Count > 1)
                    parts = pair.Value % 2 == 0
                        ? placements[0]
                        : placements.FirstOrDefault(candidate => AtMostOneIsNegativeOnTheReals(candidate, x));
                if (parts is null)
                {
                    Add(pair.Key, pair.Value);
                    continue;
                }
                foreach (var part in parts)
                    Add(part, pair.Value);
            }
            foreach (var (factor, isBelow) in others)
            {
                var (@base, power) = factor is Powf(var b, Number.Integer e) && e.EInteger.CanFitInInt32() && e.EInteger.Sign > 0
                    ? (b, e.EInteger.ToInt32Unchecked()) : (factor, 1);
                // A whole factor splits freely; the sign goes where it meets a root already read.
                var placements = @base.ContainsNode(x) ? IrreducibleFactorsWithTheSignPlaced(@base, x) : null;
                var parts = placements?.FirstOrDefault(candidate => candidate.Count(refined.ContainsKey) == candidate.Count)
                            ?? placements?.FirstOrDefault(candidate => candidate.Any(refined.ContainsKey));
                if (parts is null)
                {
                    keptOthers.Add((factor, isBelow));
                    continue;
                }
                foreach (var part in parts)
                    Add(part, (isBelow ? -2 : 2) * power);
            }
            var rootsBefore = halves.Values.Count(n => n % 2 != 0);
            var rootsAfter = refined.Values.Count(n => n % 2 != 0);
            return rootsAfter < rootsBefore ? (refined, keptOthers) : null;
        }

        /// <summary>
        /// The irreducible factors over the rationals of the polynomial <paramref name="polynomial"/>
        /// in <paramref name="x"/>, each repeated by its multiplicity, with the constant of the
        /// factorization multiplied into one of them -- one list per choice of which, so that
        /// the product is the polynomial exactly and a negative constant is never a factor of
        /// its own. <see langword="null"/> where it is not such a polynomial.
        /// </summary>
        private static List<List<Entity>>? IrreducibleFactorsWithTheSignPlaced(Entity polynomial, Entity.Variable x)
        {
            if (polynomial.Vars.Any(v => v != x))
                return null;
            if (Functions.PolynomialFactorization.FactorComplete(polynomial, x) is not { } factorization || factorization.Parts.Count == 0)
                return null;
            var parts = new List<Entity>();
            foreach (var part in factorization.Parts)
                for (var i = 0; i < part.Multiplicity; i++)
                    parts.Add(part.Factor.ToEntity(x));
            if (factorization.Constant.CompareTo(ERational.One) == 0)
                return new List<List<Entity>> { parts };
            var constant = Number.Rational.Create(factorization.Constant);
            var placements = new List<List<Entity>>();
            for (var i = 0; i < parts.Count; i++)
            {
                var placed = new List<Entity>(parts);
                placed[i] = (constant * parts[i]).InnerSimplified;
                placements.Add(placed);
            }
            return placements;
        }

        /// <summary>
        /// A sum that is a factor of both sides, cancelled -- decided by the difference
        /// simplifying to zero, since the two spellings of one sum need not be equal as
        /// written: `x + sqrt(1 + x^2)` against `sqrt(x^2 + 1) + x` is not zero as written,
        /// and where the cheap normalisation leaves it, the full one is asked, a by-parts
        /// remainder that keeps such a pair uncancelled costing ten seconds of substitution
        /// candidates otherwise. <see langword="true"/> when anything was cancelled.
        /// </summary>
        private static bool CancelEqualSumFactors(Entity numerator, Entity denominator, out Entity above, out Entity below)
        {
            var aboveFactors = Mulf.LinearChildren(numerator).ToList();
            var belowFactors = Mulf.LinearChildren(denominator).ToList();
            var cancelled = false;
            for (var i = 0; i < aboveFactors.Count; i++)
            {
                if (aboveFactors[i] is not (Sumf or Minusf))
                    continue;
                for (var j = 0; j < belowFactors.Count; j++)
                {
                    if (belowFactors[j] is not (Sumf or Minusf))
                        continue;
                    if ((aboveFactors[i] - belowFactors[j]).InnerSimplified.Evaled is Number.Complex { IsZero: true }
                        || (aboveFactors[i] - belowFactors[j]).Simplify().Evaled is Number.Complex { IsZero: true })
                    {
                        aboveFactors.RemoveAt(i);
                        belowFactors.RemoveAt(j);
                        i--;
                        cancelled = true;
                        break;
                    }
                }
            }
            above = aboveFactors.Count == 0 ? Number.Integer.One : aboveFactors.Aggregate((l, r) => l * r);
            below = belowFactors.Count == 0 ? Number.Integer.One : belowFactors.Aggregate((l, r) => l * r);
            return cancelled;
        }

        /// <summary>
        /// <c>P^(n/2)</c> with <c>n</c> odd and <c>P</c> a polynomial in <paramref name="x"/>
        /// with numeric coefficients, as <c>P^wholePower * sqrt(P)</c>.
        /// </summary>
        private static bool TryReadASquareRootOfAPolynomial(Entity factor, Entity.Variable x, out Entity @base, out int wholePower)
        {
            @base = 0;
            wholePower = 0;
            if (!TryReadAHalfPower(factor, out var candidate, out var numerator) || numerator % 2 == 0)
                return false;
            if (!candidate.ContainsNode(x) || candidate.Vars.Any(v => v != x))
                return false;
            if (!TreeAnalyzer.TryGetPolynomial(candidate, x, out _))
                return false;
            @base = candidate;
            // n = 2p + 1 for every odd n, negative ones included: -1 = 2(-1) + 1.
            wholePower = (numerator - 1) / 2;
            return true;
        }

        /// <summary>
        /// Whether no real <paramref name="x"/> makes two of the <paramref name="bases"/>
        /// negative at once, decided on one point of each interval between their real roots.
        /// <see langword="false"/> where the roots cannot be had.
        /// </summary>
        private static bool AtMostOneIsNegativeOnTheReals(List<Entity> bases, Entity.Variable x)
            => OnEveryRealInterval(bases, x, signs => signs.Count(sign => sign < 0) <= 1);

        /// <summary>
        /// The square roots with the bases <paramref name="bases"/>, those with a positive
        /// exponent in <paramref name="halfExponents"/> above the bar and the others below,
        /// written as one square root of the quotient of their bases, where that is exact and
        /// the quotient cancels to a polynomial: the root of that polynomial, and the rational
        /// function the cancellation left beside it; <see langword="null"/> otherwise.
        /// </summary>
        /// <remarks>
        /// With <c>a</c> of the bases above negative and <c>b</c> of those below, the left side is
        /// <c>i^a (1/i)^b = i^(a - b)</c> times the root of the moduli and the right is
        /// <c>i^((a + b) mod 2)</c> times it, so the identity holds exactly where
        /// <c>a - b</c> and <c>(a + b) mod 2</c> agree modulo four -- one negative on each side,
        /// or none, or two above beside one below. Asked on every interval between the real
        /// roots, as the product rule asks its own question.
        /// </remarks>
        private static (Entity Root, Entity Beside)? CombinedAsAQuotient(List<Entity> bases, Dictionary<Entity, int> halfExponents, List<List<int>> signTable, Entity.Variable x)
        {
            var aboveBases = bases.Where(b => halfExponents[b] > 0).ToList();
            var belowBases = bases.Where(b => halfExponents[b] < 0).ToList();
            if (aboveBases.Count == 0 || belowBases.Count == 0)
                return null;
            foreach (var signs in signTable)
            {
                var a = 0;
                var b = 0;
                for (var i = 0; i < bases.Count; i++)
                    if (signs[i] < 0)
                    {
                        if (halfExponents[bases[i]] > 0) a++;
                        else b++;
                    }
                if (((a - b) % 4 + 4) % 4 != (a + b) % 2)
                    return null;
            }
            Entity top = Number.Integer.One;
            Entity bottom = Number.Integer.One;
            foreach (var @base in aboveBases) top = top * @base;
            foreach (var @base in belowBases) bottom = bottom * @base;
            if (!Functions.PolynomialGcd.TryCancel(top.Expand().InnerSimplified, bottom.Expand().InnerSimplified, out var cancelled) || cancelled is null)
                return null;
            var (polynomial, remaining) = Functions.SingleQuotient.Of(Functions.PartialFractions.Bare(cancelled));
            if (remaining.ContainsNode(x) || !TreeAnalyzer.TryGetPolynomial(polynomial, x, out _))
                return null;
            // sqrt(P/c) for a constant c is sqrt(P)/sqrt(c), exactly, for c > 0; a negative c
            // is left under the root with the polynomial.
            if (remaining.Evaled is Number.Real { IsPositive: true })
                return (MathS.Sqrt(polynomial.InnerSimplified), 1 / MathS.Sqrt(remaining));
            return (MathS.Sqrt((polynomial / remaining).InnerSimplified), Number.Integer.One);
        }

        /// <summary>
        /// Whether <paramref name="holds"/> is true of the signs of the polynomials
        /// <paramref name="bases"/> on every interval between their real roots -- one point of
        /// each interval decides it, since no base changes sign inside one. <see langword="false"/>
        /// where the roots cannot be had.
        /// </summary>
        private static bool OnEveryRealInterval(List<Entity> bases, Entity.Variable x, System.Func<List<int>, bool> holds)
            => SignsOnEveryRealInterval(bases, x) is { } table && table.All(holds);

        /// <summary>
        /// The signs of the polynomials <paramref name="bases"/>, one list per interval between
        /// their real roots, in the order of <paramref name="bases"/>; <see langword="null"/>
        /// where the roots cannot be had.
        /// </summary>
        private static List<List<int>>? SignsOnEveryRealInterval(List<Entity> bases, Entity.Variable x)
        {
            var roots = new List<double>();
            foreach (var @base in bases)
            {
                if (MathS.SolveEquation(@base, x) is not Set.FiniteSet solutions)
                    return null;
                foreach (var solution in solutions.Elements)
                {
                    if (solution.Evaled is not Number.Complex value || !value.IsFinite)
                        return null;
                    if (System.Math.Abs((double)value.ImaginaryPart) < 1e-9)
                        roots.Add((double)value.RealPart);
                }
            }
            roots.Sort();
            var samples = new List<double>();
            if (roots.Count == 0)
                samples.Add(0.37);
            else
            {
                samples.Add(roots[0] - 1);
                samples.Add(roots[roots.Count - 1] + 1);
                for (var i = 0; i + 1 < roots.Count; i++)
                    if (roots[i + 1] - roots[i] > 1e-9)
                        samples.Add((roots[i] + roots[i + 1]) / 2);
            }
            var table = new List<List<int>>();
            foreach (var at in samples)
            {
                var signs = new List<int>();
                foreach (var @base in bases)
                {
                    if (@base.Substitute(x, at).Evaled is not Number.Real value || !value.IsFinite)
                        return null;
                    signs.Add(value < 0 ? -1 : value > 0 ? 1 : 0);
                }
                table.Add(signs);
            }
            return table;
        }

        /// <summary>
        /// A fractional power of a quotient whose denominator is positive for every real
        /// <paramref name="x"/> is written as the power of the numerator over the power of the
        /// denominator, a power of an even power of <c>x</c> among them as a power of <c>x</c>
        /// for <c>x &gt; 0</c>, and the integrand so written is asked of the chain; where a
        /// power of <c>x</c> was taken so, the answer is extended to <c>x &lt; 0</c> by parity.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>(P/Q)^r = P^r/Q^r</c> whenever <c>Q</c> is a positive real, since then
        /// <c>arg(P/Q) = arg(P)</c>; a polynomial in <c>x^2</c> with positive coefficients is one
        /// at every real <c>x</c> but zero, where the quotient was undefined anyway. Timofeev's
        /// <c>((2 + x^2)/x^2)^(7/9)/(2 + x^2)^(3/2)</c> is nothing any rule reads as written, and
        /// <c>x^(-14/9) (2 + x^2)^(7/9 - 3/2)</c> once the quotient is written apart, a binomial
        /// differential under <c>u = x^(1/9)</c>. The simplifier is right not to write it so
        /// for an <c>x</c> it knows nothing about; the integrator knows its variable is real.
        /// </para>
        /// <para>
        /// <c>(x^2)^(-7/9)</c> is <c>|x|^(-14/9)</c>, and is written as <c>x^(-14/9)</c>: what comes
        /// out is an antiderivative for <c>x &gt; 0</c>, made one everywhere by parity, which is
        /// exact, or not at all. Asked at the top and one level below it, where the remainder
        /// by parts leaves is asked, and no deeper, since it lands on the open chain.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveByWritingAPowerOfAQuotientApart(Entity expr, Entity.Variable x)
        {
            // At the top and one below it, where the remainder by parts leaves is asked --
            // Charlwood's `x^3 arcsec(x)/sqrt(x^4 - 1)` leaves a root of `1 - 1/x^2` there --
            // and no deeper: unscoped, it fed the search on `arcsin(sqrt(1 + x) - sqrt(x))`,
            // thirty levels of the same root written apart and combined again, five seconds
            // where declining takes one.
            if (!Integration.AnsweringTheQuestionAskedOrOneBelow)
                return null;
            var tookAPowerOfX = false;
            var written = expr.Replace(node =>
            {
                if (node is not Powf(var @base, Number.Rational exponent) || exponent is Number.Integer || !@base.ContainsNode(x))
                    return node;
                // As one quotient where it is a sum with a quotient in it: `1 - 1/x^2` is
                // `(x^2 - 1)/x^2`, the derivative of the arcsecant as it is written.
                var (above, below) = Functions.SingleQuotient.Of(AsOneQuotient(@base));
                if (below != Number.Integer.One && below.ContainsNode(x) && IsPositiveForReal(below, x) && OfModestDegree(above))
                    return MathS.Pow(above, exponent) * PowerOfAPositive(below, -exponent);
                // A polynomial with rational roots is the product of its linear factors, and
                // the product is what the splitting below reads: `(1 - x^2)^(1/4)` is
                // `((1 - x)(1 + x))^(1/4)`, which comes apart on `(-1, 1)` where the root is
                // real, and beside `sqrt(1 - x)` and `sqrt(1 + x)` -- Timofeev's 314 -- the
                // two linear radicals are then one rule's. Only there: only where every
                // factor found already stands under a fractional power of its own elsewhere
                // in the integrand, so that the splitting joins what is written apart. For a
                // root of a quadratic on its own, `sqrt(1 - x^2)`, Euler's substitution and
                // the trigonometric one are the answer, and written apart it went to the
                // rule for two linear radicals and its rational function of a higher degree:
                // six more of the suite's problems timed out, Charlwood's 41, 92, 152 and 155
                // among them.
                if (above is not Mulf && TreeAnalyzer.TryGetPolynomial(above, x, out _)
                    && Functions.PolynomialFactoring.TryFactor(above, x, out var factoredAbove)
                    && Mulf.LinearChildren(factoredAbove).Where(factor => factor.ContainsNode(x)).ToList() is { Count: >= 2 } linearFactors
                    && linearFactors.All(factor => StandsUnderAFractionalPowerElsewhere(factor is Powf(var g, Number.Integer) ? g : factor, node)))
                    above = WithANegativeConstantInsideAFactor(factoredAbove);
                // A denominator with an odd power in it is the even part times what is left,
                // and the even part is not negative: `sqrt(sin(x)/cos(x)^5)` is
                // `sqrt(sin(x)/cos(x)) sqrt(1/cos(x)^4)`, which is `sqrt(tan(x))/cos(x)^2`
                // exactly -- Timofeev's, and the binomial rule's from there.
                // The degree bound is on what is left under the root, and where every
                // polynomial factor comes out nothing polynomial is left.
                var oddRoot = !exponent.ERational.Denominator.IsEven;
                var everyFactorComesOut = oddRoot || AnEvenRootOfAProductSplits(above, below, exponent.ERational.Denominator);
                if (everyFactorComesOut || OfModestDegreeOrNotAPolynomial(above) && OfModestDegreeOrNotAPolynomial(below))
                {
                    // On either side of the bar: `sqrt(sin(x)^5/cos(x))` is `sin(x)^2 sqrt(sin(x)/cos(x))`.
                    Entity taken = Number.Integer.One;
                    Entity oddAbove = Number.Integer.One;
                    Entity oddBelow = Number.Integer.One;
                    foreach (var (side, underneath) in new[] { (above, false), (below, true) })
                        foreach (var factor in Mulf.LinearChildren(side))
                        {
                            // Under an odd root every real factor comes out as it is: the real
                            // root of a product is the product of the real roots, and that is
                            // the root a negative real has here -- `(-8)^(1/3)` is `-2`. So
                            // `((x - 1)^4 (x + 1)^2)^(1/3)` is `(x - 1)^(4/3) (x + 1)^(2/3)`
                            // for every real x, Timofeev's 318 as the rule for two linear
                            // radicals reads it. A polynomial factor, which is real; a root
                            // among the factors is complex somewhere, and there the roots of
                            // a product and the product of the roots part.
                            // And under an even root the same where the split has been checked
                            // to hold on every real interval, see AnEvenRootOfAProductSplits.
                            if (everyFactorComesOut && factor.ContainsNode(x)
                                && (factor is Powf(var polynomial, Number.Integer wholePower) && wholePower.EInteger.Sign > 0 ? (polynomial, wholePower.EInteger) : (factor, EInteger.One)) is var (realFactor, times)
                                && TreeAnalyzer.TryGetPolynomial(realFactor, x, out _))
                            {
                                taken = taken * MathS.Pow(realFactor, (Number.Integer.Create(underneath ? times.Negate() : times) * exponent).InnerSimplified);
                                continue;
                            }
                            // g^(2m r) is |g|^(2 m r): g itself where that power is even or g is
                            // positive, and for x the answer for x > 0 extended by parity;
                            // otherwise the factor stays where it is.
                            if (factor is Powf(var g, Number.Integer n) && n.EInteger.CompareTo(EInteger.FromInt32(2)) >= 0 && g.ContainsNode(x)
                                && n.EInteger.Divide(EInteger.FromInt32(2)).Multiply(EInteger.FromInt32(2)) is var even
                                && (Number.Integer.Create(underneath ? even.Negate() : even) * exponent).InnerSimplified is var power
                                && (g == x || power is Number.Integer { EInteger.IsEven: true } || IsPositiveForReal(g, x)))
                            {
                                var evenPower = MathS.Pow(g, Number.Integer.Create(even));
                                taken = taken * (g == x ? PowerOfAPositive(evenPower, underneath ? -exponent : exponent)
                                    : power is Number.Integer ? MathS.Pow(g, power) : MathS.Pow(evenPower, underneath ? -exponent : exponent));
                                if (!n.EInteger.IsEven)
                                {
                                    if (underneath) oddBelow = oddBelow * g;
                                    else oddAbove = oddAbove * g;
                                }
                            }
                            else if (underneath)
                                oddBelow = oddBelow * factor;
                            else
                                oddAbove = oddAbove * factor;
                        }
                    if (taken.ContainsNode(x))
                        return MathS.Pow(oddBelow == Number.Integer.One ? oddAbove : oddAbove / oddBelow, exponent) * taken;
                }
                // And a polynomial every monomial of which an even power of x divides: the root
                // of `x^4 + x^2` is `|x| sqrt(x^2 + 1)`, exactly, since `x^2` is not negative --
                // what the roots of Charlwood's `x^3 arcsec(x)/sqrt(x^4 - 1)` by parts combine to.
                if (TreeAnalyzer.TryGetPolynomial(@base, x, out var monomials) && monomials.Count > 1 && OfModestDegree(@base)
                    && monomials.Keys.Min() is { } lowest && lowest.CanFitInInt32() && lowest.ToInt32Unchecked() / 2 is var k and > 0)
                {
                    Entity rest = Number.Integer.Zero;
                    foreach (var pair in monomials.OrderBy(pair => pair.Key))
                    {
                        var degree = pair.Key.ToInt32Unchecked() - 2 * k;
                        Entity term = degree == 0 ? pair.Value : degree == 1 ? pair.Value * x : pair.Value * MathS.Pow(x, degree);
                        rest = rest == Number.Integer.Zero ? term : rest + term;
                    }
                    return PowerOfAPositive(MathS.Pow(x, 2 * k), exponent) * MathS.Pow(rest.InnerSimplified, exponent);
                }
                return node;
            });
            if (written == expr)
                return null;
            // A quotient of a sine by the cosine of the same argument set free is the tangent,
            // which is what the tangent substitution reads.
            written = written.Replace(node => node is Divf(Sinf(var a), Cosf(var b)) && a == b ? MathS.Tan(a) : node);
            // Bare: the simplification attaches `provided not x = 0` where it cancels a power
            // of x, and a condition on the integrand is a shape no rule reads.
            var forPositive = Integration.ComputeIndefiniteIntegral(Functions.PartialFractions.Bare(written), x, integrateByParts: true);
            if (forPositive is null || forPositive.Nodes.Any(node => node == MathS.NaN))
                return null;
            return tookAPowerOfX ? ExtendedByParity(expr, forPositive, x) : forPositive;

            // What is set free under the root is a polynomial of degree four at most: the
            // shapes the rules behind this read stop there, and a root of a sextic or a
            // twelfth-degree polynomial set free is a search that ends nowhere -- the
            // half-angle form of `cos(x)^2/sqrt(1 + cos(x)^2 + cos(x)^4)`, a root of a sum of
            // quotients by `(u^2 + 1)^6`, was fourteen seconds of one.
            bool OfModestDegree(Entity polynomial)
                => !polynomial.ContainsNode(x)
                   || TreeAnalyzer.TryGetPolynomial(polynomial, x, out var read) && read.Count > 0
                      && read.Keys.Max()!.CompareTo(EInteger.FromInt32(4)) <= 0;

            // ...and what is not a polynomial in x at all -- a sine -- is not a degree to bound.
            bool OfModestDegreeOrNotAPolynomial(Entity numerator)
                => !TreeAnalyzer.TryGetPolynomial(numerator, x, out _) || OfModestDegree(numerator);

            // Whether `linear`, or its negative, is the base of a fractional power somewhere
            // in the integrand other than `root` itself.
            bool StandsUnderAFractionalPowerElsewhere(Entity linear, Entity root)
            {
                var negated = (-linear).Expand();
                return expr.Nodes.Any(other => other != root && other is Powf(var b, Number.Rational r) && r is not Number.Integer
                                               && (b == linear || b == negated || (b - linear).Expand().Evaled is Number.Complex { IsZero: true } || (b + linear).Expand().Evaled is Number.Complex { IsZero: true }));
            }

            // `-(x - 1) (x + 1)`, which is how the factoring writes `1 - x^2`, as
            // `(1 - x) (x + 1)`: a negative constant multiplied into the first factor that is
            // not a power, since the splitting below asks the constant to be positive.
            Entity WithANegativeConstantInsideAFactor(Entity product)
            {
                Entity constant = Number.Integer.One;
                var factors = new List<Entity>();
                foreach (var factor in Mulf.LinearChildren(product))
                    if (factor.ContainsNode(x))
                        factors.Add(factor);
                    else
                        constant = constant * factor;
                if (constant.Evaled is not Number.Real { IsNegative: true })
                    return product;
                var bare = factors.FindIndex(factor => factor is not Powf);
                if (bare < 0)
                    return product;
                factors[bare] = (constant * factors[bare]).Expand();
                Entity rebuilt = factors[0];
                for (var i = 1; i < factors.Count; i++)
                    rebuilt = rebuilt * factors[i];
                return rebuilt;
            }

            // Whether `(c g_1^(n_1) ... g_k^(n_k))^(p/q)` with q even is `c^(p/q) g_1^(n_1 p/q) ...`
            // wherever the root is real: the polynomial factors are real, and on each interval
            // between their roots the product's argument is pi times the sum of the n_i of the
            // negative ones. Where that sum is odd the product is negative and its even root is
            // not real, so the integrand is not asked about there; where it is even the root is
            // real and positive, and the product of the principal powers agrees with it exactly
            // when their phases, pi n_i / q each, add up to a whole turn: the sum a multiple of
            // 2q. Not of q: `sqrt(t^2)` is `|t|`, and `t^(2/2)` is `t`, half a turn out below
            // zero -- which Timofeev's `sqrt(tan(x) tan(2x))` found. The constant must be
            // positive, since a negative one would take the phase the other way.
            // `((x - 1)^3 (x + 2)^5)^(1/4)` splits: above 1 both are positive, below -2 both
            // negative with 3 + 5 a multiple of 8, and between the product is negative.
            bool AnEvenRootOfAProductSplits(Entity above, Entity below, EInteger q)
            {
                var factors = new List<(Entity Polynomial, EInteger Times)>();
                foreach (var side in new[] { above, below })
                    foreach (var factor in Mulf.LinearChildren(side))
                    {
                        if (!factor.ContainsNode(x))
                        {
                            if (factor.Evaled is not Number.Real { IsPositive: true })
                                return false;
                            continue;
                        }
                        var (polynomial, times) = factor is Powf(var g, Number.Integer n) && n.EInteger.Sign > 0 ? (g, n.EInteger) : (factor, EInteger.One);
                        if (!TreeAnalyzer.TryGetPolynomial(polynomial, x, out _))
                            return false;
                        factors.Add((polynomial, times));
                    }
                if (factors.Count < 2)
                    return false;
                return OnEveryRealInterval(factors.Select(f => f.Polynomial).ToList(), x, signs =>
                {
                    var negativeTimes = EInteger.Zero;
                    for (var i = 0; i < signs.Count; i++)
                        if (signs[i] < 0)
                            negativeTimes = negativeTimes.Add(factors[i].Times);
                    return !negativeTimes.IsEven || negativeTimes.Remainder(q.ShiftLeft(1)).IsZero;
                });
            }

            // Q^r for a Q positive at every real x: a monomial `c x^(2k)` is `c^r x^(2kr)` for x > 0.
            Entity PowerOfAPositive(Entity positive, Number.Rational exponent)
            {
                if (TreeAnalyzer.TryGetPolynomial(positive, x, out var monomials) && monomials.Count == 1)
                {
                    var (degree, coefficient) = (monomials.Keys.First(), monomials.Values.First());
                    if (degree.Sign > 0)
                    {
                        tookAPowerOfX = true;
                        var power = MathS.Pow(x, (Number.Integer.Create(degree) * exponent).InnerSimplified);
                        return coefficient.Evaled is Number.Integer { IsZero: false } one && one.EInteger.Equals(EInteger.One)
                            ? power
                            : MathS.Pow(coefficient, exponent) * power;
                    }
                }
                return MathS.Pow(positive, exponent);
            }
        }

        /// <summary>
        /// Whether the polynomial <paramref name="expr"/> in <paramref name="x"/> is positive at
        /// every real <paramref name="x"/> but possibly zero, for the plain reason that every
        /// monomial is an even power with a positive coefficient.
        /// </summary>
        private static bool IsPositiveForReal(Entity expr, Entity.Variable x)
        {
            if (!TreeAnalyzer.TryGetPolynomial(expr, x, out var monomials) || monomials.Count == 0)
                return false;
            return monomials.All(pair => pair.Key.IsEven && pair.Value.Evaled is Number.Real { IsPositive: true });
        }

        /// <summary>
        /// The radicals of <paramref name="expr"/> with every power of <paramref name="u"/>
        /// that a radical can hold taken out of it, for a <paramref name="u"/> that is not
        /// negative: <c>(u^2 (1 + u))^(1/2)</c> is <c>u sqrt(1 + u)</c>, and
        /// <c>(2u/(1 + u))^(1/2)</c> is <c>sqrt(2u)/sqrt(1 + u)</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For the substitutions whose variable is one by construction -- <c>u = e^(k x)</c>,
        /// <c>u = (a x + b)^(1/q)</c> with <c>q</c> even, <c>u = x^(1/q)</c> likewise -- and
        /// for no one else: the simplifier is right not to call <c>sqrt(u^2 (1 + u))</c>
        /// <c>u sqrt(1 + u)</c> for a <c>u</c> it knows nothing about, and the substitution
        /// knows exactly this about its own. Two identities, each exact for <c>u &gt;= 0</c>:
        /// <c>(a b)^r = a^r b^r</c> whenever <c>a</c> is a non-negative real, since then
        /// <c>arg(a b) = arg(b)</c>; and <c>(P/Q)^r = P^r/Q^r</c> whenever <c>Q</c> is a positive
        /// real, for the same reason -- which a polynomial in <c>u</c> with non-negative
        /// coefficients and a positive constant term is, at every <c>u &gt;= 0</c>.
        /// </para>
        /// <para>
        /// Without it <c>1/sqrt(t + t^(3/2))</c> under <c>u = sqrt(t)</c> is
        /// <c>2u/sqrt(u^2 + u^3)</c>, a root of a cubic, and with it <c>2/sqrt(1 + u)</c>;
        /// <c>sqrt(1 + tanh(4x))</c> under <c>u = e^(8x)</c> is a root of <c>2u/(1 + u)</c>,
        /// which nothing rationalises, and with it <c>sqrt(2) sqrt(u)/sqrt(1 + u)</c>.
        /// </para>
        /// </remarks>
        private static Entity FactorANonnegativeVariableOutOfRadicals(Entity expr, Entity.Variable u)
            => expr.Replace(node =>
            {
                if (node is not Powf(var @base, Number.Rational exponent) || exponent is Number.Integer
                    || !@base.ContainsNode(u)
                    || !exponent.ERational.Denominator.CanFitInInt32() || !exponent.ERational.Numerator.CanFitInInt32())
                    return node;
                var p = exponent.ERational.Numerator.ToInt32Unchecked();
                var q = exponent.ERational.Denominator.ToInt32Unchecked();

                // A quotient whose denominator is positive at every u >= 0 comes apart first --
                // written as one quotient, since a sum with a quotient in it,
                // `1 - (u - 1/u)^2/4`, is `(4u^2 - (u^2 - 1)^2)/(4u^2)` and reads as nothing
                // until it is: `sinh(x)^2 sinh(2x)/(1 - sinh(x)^2)^(3/2)` under `u = e^x` is that
                // to the three halves, and is a palindromic quartic to the three halves over
                // `8u^3` once it is written out, which the reciprocal substitution answers.
                var (above, below) = Functions.SingleQuotient.Of(Functions.SingleQuotient.Combine(@base));
                Entity result;
                if (below != Number.Integer.One && below.ContainsNode(u) && IsPositiveForNonnegative(below, u))
                    result = Factored(above, u, p, q) / Factored(below, u, p, q);
                else if (below == Number.Integer.One)
                    result = Factored(above, u, p, q);
                else
                    return node;
                return result;
            });

        /// <summary>
        /// <c>P^(p/q)</c> with the largest <c>u^(k q)</c> dividing the polynomial <c>P</c> taken
        /// out as <c>u^(k p)</c>; <c>P^(p/q)</c> as it is where <c>P</c> is not a polynomial in
        /// <paramref name="u"/> or holds no such power.
        /// </summary>
        private static Entity Factored(Entity polynomial, Entity.Variable u, int p, int q)
        {
            var exponent = Number.Rational.Create(p, q);
            if (!polynomial.ContainsNode(u) || !TreeAnalyzer.TryGetPolynomial(polynomial, u, out var monomials) || monomials.Count == 0)
                return MathS.Pow(polynomial, exponent);
            var lowest = monomials.Keys.Min()!;
            if (!lowest.CanFitInInt32())
                return MathS.Pow(polynomial, exponent);
            // Rebuilt from its monomials even where nothing comes out, so that a numerator a
            // quotient split left as `2(u + 1) - 2` is the `2u` the next rule reads.
            var k = System.Math.Max(0, lowest.ToInt32Unchecked() / q);
            Entity rest = Number.Integer.Zero;
            foreach (var pair in monomials.OrderBy(pair => pair.Key))
            {
                var degree = pair.Key.ToInt32Unchecked() - k * q;
                Entity term = degree == 0 ? pair.Value : degree == 1 ? pair.Value * u : pair.Value * MathS.Pow(u, degree);
                rest = rest == Number.Integer.Zero ? term : rest + term;
            }
            // `u` rather than `u^1`, which the substitution rule's power rewriting does not read
            // as a power of u: `1/(u^1 sqrt(3 - 3u^2 + u^4))` was refused the candidate `u^2`.
            Entity taken = k * p == 1 ? u : MathS.Pow(u, k * p);
            return k == 0 ? MathS.Pow(rest.InnerSimplified, exponent) : taken * MathS.Pow(rest.InnerSimplified, exponent);
        }

        /// <summary>
        /// Whether the polynomial <paramref name="expr"/> in <paramref name="u"/> is positive at
        /// every <c>u &gt; 0</c> for the plain reason that every coefficient is a positive
        /// number. Without a constant term it is zero at <c>u = 0</c>, where a quotient by it
        /// is undefined on both sides of the identity that asks this, so that costs nothing:
        /// <c>4u^2</c> is what the combined denominator of <c>1 - (u - 1/u)^2/4</c> is.
        /// </summary>
        private static bool IsPositiveForNonnegative(Entity expr, Entity.Variable u)
        {
            if (!TreeAnalyzer.TryGetPolynomial(expr, u, out var monomials) || monomials.Count == 0)
                return false;
            return monomials.Values.All(coefficient => coefficient.Evaled is Number.Real { IsPositive: true });
        }

        private static int Lcm(int a, int b)
        {
            var (x, y) = (a, b);
            while (y != 0) (x, y) = (y, x % y);
            return a / x * b;
        }


        /// <summary>
        /// Bioche's first two rules: a rational function of <c>sin(x)</c> and <c>cos(x)</c>
        /// that is odd in the sine is a rational function of <c>u = cos(x)</c> times
        /// <c>sin(x) dx = -du</c>, and one odd in the cosine the mirror of it; with radicals
        /// of polynomials in the two even in the function admitted as coefficients.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Bondarenko's <c>1/(cos(x) + cos(3x))^5</c> is <c>1/(4 cos(x)^3 - 2 cos(x))^5</c>, odd in
        /// the cosine, and under <c>u = sin(x)</c> it is <c>1/((1 - u^2)^3 (2 - 4u^2)^5)</c>, a
        /// rational function with a denominator of degree sixteen; the half-angle
        /// substitution, which answers any rational function of the two, makes one of
        /// degree thirty in the tangent of the half angle and did not return within the
        /// budget. The parity is read off the polynomials: with <c>N/D</c> the integrand over
        /// the two and <c>f</c> the function it is odd in, <c>N/(D f)</c> is even in <c>f</c>
        /// exactly when, after clearing <c>D</c> against <c>D(-f)</c> where <c>D</c> is neither
        /// even nor odd, every power of <c>f</c> above is odd and every one below is even --
        /// and then each <c>f^2</c> is <c>1 - u^2</c>. Exact, and the substitution is a
        /// bijection on each interval between the zeros of <c>f</c>, the standing caveat on
        /// every trigonometric substitution here.
        /// </para>
        /// <para>
        /// In front of the half-angle substitution, for the smaller rational function and
        /// the shorter answer -- <c>sin(x)/(1 + cos(x)^2)</c> is <c>-arctan(cos(x))</c> here and
        /// a page in the half-angle tangent -- and behind everything that answers a power
        /// product or a homogeneous quotient in its own terms.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveByBiochesOddSubstitution(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            var sine = MathS.Sin(x);
            var cosine = MathS.Cos(x);
            if (!expr.Nodes.Any(node => node is Sinf or Cosf or Tanf or Cotanf or Secantf or Cosecantf && node.ContainsNode(x)))
                return null;
            var overTheTwo = expr.Replace(node => node switch
            {
                Tanf(var arg) when arg == x => sine / cosine,
                Cotanf(var arg) when arg == x => cosine / sine,
                Secantf(var arg) when arg == x => 1 / cosine,
                Cosecantf(var arg) when arg == x => 1 / sine,
                _ => node
            });
            // A radical of a polynomial in the two stands as a symbol while the parity is read:
            // Moses's `sqrt(A^2 + B^2 sin(x)^2)/sin(x)` is odd in the sine with the root even in
            // it, and under `u = cos(x)` the root is `sqrt(A^2 + B^2 (1 - u^2))`. The symbol
            // is a coefficient to the polynomials below, and is written in u with them.
            var s = Variable.CreateUnique(expr, "s_bioche");
            var c = Variable.CreateUnique(expr, "c_bioche");
            var radicals = new List<(Variable Symbol, Entity Base, Number.Rational Exponent)>();
            overTheTwo = overTheTwo.Replace(node =>
            {
                if (node is not Powf(var radicand, Number.Rational exponent) || exponent is Number.Integer || !radicand.ContainsNode(x))
                    return node;
                var symbol = Variable.CreateUnique(expr, "r_bioche" + radicals.Count);
                radicals.Add((symbol, radicand.Substitute(sine, s).Substitute(cosine, c), exponent));
                return symbol;
            });
            if (radicals.Any(radical => radical.Base.ContainsNode(x)
                                        || !TreeAnalyzer.TryGetPolynomial(radical.Base, s, out _) || !TreeAnalyzer.TryGetPolynomial(radical.Base, c, out _)))
                return null;
            if (overTheTwo.Nodes.Any(node => node.ContainsNode(x) && node is not (Variable or Sumf or Minusf or Mulf or Divf or Sinf or Cosf)
                                              && !(node is Powf(_, Number.Integer))))
                return null;
            if (overTheTwo.Nodes.Any(node => node is Sinf(var arg) && arg != x || node is Cosf(var arg2) && arg2 != x))
                return null;
            var inTheTwo = Functions.SingleQuotient.Combine(overTheTwo.Substitute(sine, s).Substitute(cosine, c));
            if (inTheTwo.ContainsNode(x))
                return null;   // x bare beside the two is not this rule's
            var (above, below) = Functions.SingleQuotient.Of(inTheTwo);
            above = above.Expand();
            below = below.Expand();
            if (!TreeAnalyzer.TryGetPolynomial(above, s, out _) || !TreeAnalyzer.TryGetPolynomial(below, s, out _)
                || !TreeAnalyzer.TryGetPolynomial(above, c, out _) || !TreeAnalyzer.TryGetPolynomial(below, c, out _))
                return null;

            foreach (var (odd, even, back, sign) in new[] { (c, s, sine, 1), (s, c, cosine, -1) })
            {
                // Every radical even in f, or it is not a function of u.
                if (radicals.Any(radical => ParityIn(radical.Base, odd) != 1))
                    continue;
                // N/(D f) even in f: with D even, N is odd and one f comes out of it; with D
                // odd, N is even and D f is; with D neither, both are cleared against D(-f).
                var parityOfBelow = ParityIn(below, odd);
                Entity numerator;
                Entity denominator;
                bool oneFOutOfTheNumerator;
                if (parityOfBelow == 1)
                {
                    numerator = above;
                    denominator = below;
                    oneFOutOfTheNumerator = true;
                }
                else if (parityOfBelow == -1)
                {
                    numerator = above;
                    denominator = (below * odd).Expand();
                    oneFOutOfTheNumerator = false;
                }
                else
                {
                    var mirrored = below.Substitute(odd, -odd).Expand();
                    numerator = (above * mirrored).Expand();
                    denominator = (below * mirrored).Expand();
                    oneFOutOfTheNumerator = true;
                }
                if (ParityIn(numerator, odd) != (oneFOutOfTheNumerator ? -1 : 1) || ParityIn(denominator, odd) != 1)
                    continue;
                var u = Variable.CreateUnique(expr, "u_bioche");
                var oneMinusUSquared = 1 - MathS.Sqr(u);
                if (InU(numerator, odd, even, u, oneMinusUSquared, oneFOutOfTheNumerator) is not { } aboveInU
                    || InU(denominator, odd, even, u, oneMinusUSquared, dividedByOdd: false) is not { } belowInU)
                    continue;
                var radicalsInU = new List<(Variable Symbol, Entity InU)>();
                foreach (var (symbol, radicand, exponent) in radicals)
                {
                    if (InU(radicand, odd, even, u, oneMinusUSquared, dividedByOdd: false) is not { } radicandInU)
                        break;
                    radicalsInU.Add((symbol, MathS.Pow(radicandInU, exponent)));
                }
                if (radicalsInU.Count != radicals.Count)
                    continue;
                foreach (var (symbol, radical) in radicalsInU)
                {
                    aboveInU = aboveInU.Substitute(symbol, radical);
                    belowInU = belowInU.Substitute(symbol, radical);
                }
                var integrand = Functions.PartialFractions.Bare(aboveInU / belowInU);
                if (Integration.ComputeIndefiniteIntegral(integrand, u, integrateByParts) is not { } result
                    || result.Nodes.Any(node => node == MathS.NaN))
                    continue;
                var answer = result.Substitute(u, back);
                return sign == 1 ? answer : -answer;
            }
            return null;

            // 1 for even in f, -1 for odd, 0 for neither; a zero polynomial is both.
            static int ParityIn(Entity polynomial, Entity.Variable f)
            {
                if (!TreeAnalyzer.TryGetPolynomial(polynomial, f, out var read))
                    return 0;
                var evenPowers = read.Keys.Any(k => k.IsEven);
                var oddPowers = read.Keys.Any(k => !k.IsEven);
                return evenPowers && oddPowers ? 0 : oddPowers ? -1 : 1;
            }

            // The polynomial with every f^2 written as 1 - u^2 and the other function as u,
            // one f divided out first where asked.
            static Entity? InU(Entity polynomial, Entity.Variable f, Entity.Variable other, Entity.Variable u, Entity oneMinusUSquared, bool dividedByOdd)
            {
                if (!TreeAnalyzer.TryGetPolynomial(polynomial, f, out var read))
                    return null;
                Entity sum = Number.Integer.Zero;
                foreach (var pair in read)
                {
                    if (pair.Key.Sign < 0 || !pair.Key.CanFitInInt32())
                        return null;
                    var power = pair.Key.ToInt32Unchecked() - (dividedByOdd ? 1 : 0);
                    if (power < 0 || power % 2 != 0)
                        return null;
                    var half = power / 2;
                    var term = pair.Value.Substitute(other, u) * (half == 0 ? Number.Integer.One : half == 1 ? oneMinusUSquared : MathS.Pow(oneMinusUSquared, half));
                    sum = sum == Number.Integer.Zero ? term : sum + term;
                }
                return sum;
            }
        }

        /// <summary>
        /// A rational function of <c>sin(x)</c> and <c>cos(x)</c>, turned into a rational function
        /// of one variable by the half-angle substitution <c>t = tan(x/2)</c> and handed to the
        /// machinery for those.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The Weierstrass substitution. Under <c>t = tan(x/2)</c>,
        /// <c>sin(x) = 2t/(1 + t^2)</c>, <c>cos(x) = (1 - t^2)/(1 + t^2)</c> and
        /// <c>dx = 2/(1 + t^2) dt</c>, so anything built from sines and cosines by the field
        /// operations becomes a quotient of polynomials — which partial fractions already answers.
        /// </para>
        /// <para>
        /// <b>What it is for.</b> A family of first-year integrals had no antiderivative at all:
        /// <c>1/(1 + cos(x))</c>, <c>1/(1 - sin(x))</c>, <c>1/(1 + cos(x)/2)</c>,
        /// <c>1/(b cos(x) + a sin(x))</c>. Measured against Rubi's suite, the integrals it rates
        /// as a single table lookup were the *worst*-served difficulty band we had, and this
        /// family is a large part of why.
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </para>
        /// <para>
        /// <b>The test is the rewrite itself</b>, as for the tangent above: replace every
        /// <c>sin(x)</c> and <c>cos(x)</c> and see whether an <c>x</c> survives. That declines
        /// <c>sin(x) + x</c>, and it declines <c>sin(x) * sin(2x)</c> as well — <c>sin(2x)</c> is
        /// not <c>sin(x)</c>, so an <c>x</c> is left behind. The second is the right answer for
        /// the wrong-looking reason: a product of sines is a sum by the product-to-sum identity
        /// and wants that rather than a rational function in <c>t</c>.
        /// </para>
        /// <para>
        /// <b>It runs after the tangent substitution</b>, which is the more specific tool: an
        /// integrand that is rational in <c>tan(x)</c> comes out of that one in terms of the
        /// tangent, where this one would answer it in terms of the half-angle and be right but
        /// unrecognisable.
        /// </para>
        /// <para>
        /// <b>The condition the answer inherits.</b> <c>tan(x/2)</c> is undefined at odd
        /// multiples of pi, so the antiderivative this produces is an antiderivative on each
        /// interval between them rather than across one — the constant may differ from one
        /// interval to the next. That is the standing property of this substitution and is the
        /// same one <see cref="SolveByTangentSubstitution"/> already carries; it is not a claim
        /// this rule makes and does not make the value wrong where it is defined.
        /// </para>
        /// </remarks>
        internal static Entity? SolveByHalfAngleSubstitution(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            // One argument linear in x for every trigonometric function of x, `a x + b` as
            // well as `x`: with `t = tan((a x + b)/2)`, `dx` is `2 dt/(a (1 + t^2))`. Hearn's
            // `sin(a x)/(b + c sin(a x))^2` was declined for the `a`, with the scaling of the
            // variable, the rule for it, at the end of the chain and refusing three symbols.
            Entity? argument = null;
            foreach (var node in expr.Nodes)
            {
                if (TrigonometricArgument(node) is not { } thisArgument || !thisArgument.ContainsNode(x))
                    continue;
                if (argument is null)
                    argument = thisArgument;
                else if (argument != thisArgument)
                    return null;
            }
            if (argument is null || !TreeAnalyzer.TryGetPolyLinear(argument, x, out var rate, out _)
                || rate.ContainsNode(x) || TreeAnalyzer.IsZero(rate))
                return null;
            var sine = MathS.Sin(argument);
            var cosine = MathS.Cos(argument);
            // The other four functions of x as quotients of the two, wherever either is
            // defined: Timofeev's `1/(3 + 2 sec(x))` is `cos(x)/(3 cos(x) + 2)`, and with the
            // secant left standing it had no sine or cosine for the substitution to read.
            // Only where one of the four is a term of a sum and the whole is rational in the
            // two, which is the shape this rule is for; a product of their powers is the power
            // rules' after this one has declined it, and read here `sec(x)^3 tan(x)^2` was
            // answered in the half-angle tangent, a page long, where the reduction gives it
            // in three terms -- and `tan(x)/(a^3 + b^3 tan(x)^2)^(1/3)` spent three seconds
            // on a cube root of a rational function of t before declining.
            static bool IsOneOfTheFour(Entity node, Entity argument)
                => node is Secantf(var a) && a == argument || node is Cosecantf(var b) && b == argument
                   || node is Tanf(var c) && c == argument || node is Cotanf(var d) && d == argument;
            if (expr.Nodes.Any(node => node is Sumf or Minusf && node.DirectChildren.Any(term => term.Nodes.Any(inner => IsOneOfTheFour(inner, argument))))
                && expr.Nodes.All(node => !node.ContainsNode(x)
                    || node is Variable or Sumf or Minusf or Mulf or Divf or Sinf or Cosf or Secantf or Cosecantf or Tanf or Cotanf
                    || node is Powf(_, Number.Integer)))
                expr = expr.Replace(node => node switch
                {
                    Secantf(var a) when a == argument => 1 / cosine,
                    Cosecantf(var a) when a == argument => 1 / sine,
                    Tanf(var a) when a == argument => sine / cosine,
                    Cotanf(var a) when a == argument => cosine / sine,
                    _ => node,
                });
            if (!expr.ContainsNode(sine) && !expr.ContainsNode(cosine))
                return null;

            var t = Variable.CreateUnique(expr, "u_half");
            var tSquared = MathS.Sqr(t);
            var inT = expr
                .Substitute(sine, 2 * t / (1 + tSquared))
                .Substitute(cosine, (1 - tSquared) / (1 + tSquared));
            if (inT.ContainsNode(x))
                return null;

            // Simplify rather than InnerSimplified, and that is not a preference. The rewrite
            // puts a quotient inside a quotient -- 1/cos(x) becomes 1 / ((1 - t^2)/(1 + t^2))
            // times 2/(1 + t^2) -- and partial fractions wants a single Divf of two polynomials.
            // InnerSimplified leaves the nesting alone, so every one of these was handed on in a
            // shape nothing downstream could read and came back unevaluated.
            // Combined into one quotient first, because the rewrite puts a quotient inside a
            // quotient and everything downstream wants a single Divf of two polynomials. Simplify
            // alone does not do it — it never puts a sum over a common denominator, so
            // 1/(1 - sin(x)) rewrote to 2/((t^2 + 1)(1 + (-2)t/(t^2 + 1))) and stopped there, one
            // distribution short of 2/(t^2 - 2t + 1), which is integrated at once.
            // https://github.com/asc-community/AngouriMath/issues/1239
            var integrand = Functions.SingleQuotient.Combine(inT * 2 / (rate * (1 + tSquared))).Simplify();

            // Collapsing the nesting attaches a condition saying the denominator it cleared is
            // non-zero, and that denominator is 1 + t^2 -- so 1/(1 + cos(x)) comes out as
            // `1 provided not 1 + t^2 = 0`, which is the answer with a guard on it. The guard is
            // dropped, and it is worth being exact about why rather than calling it vacuous:
            // 1 + t^2 is at least 1 for every *real* t and vanishes at t = +-i, so the condition
            // is not vacuous over the complex plane, which is this library's default codomain.
            //
            // It is sound to drop here because of what t is. The substitution is t = tan(x/2),
            // which this rule only reaches by rewriting a real trigonometric integrand; the
            // excluded points are not reachable values of it, and they do not appear in the
            // answer, which is written back in terms of x. The same strip is made for the same
            // reason in SolveBySubstitution.
            //
            // What this does *not* do is assume a condition away in general: the answer is still
            // an antiderivative on each interval between the poles of tan(x/2), which is the
            // standing caveat on this substitution and is recorded in the summary above.
            if (integrand is Providedf(var inner, _))
                integrand = inner;

            // Every sine and cosine brought its own `1 + t^2` below the bar, and clearing them
            // puts the same power of it above and below -- where the simplifier does not see
            // it once the denominator is a sum, `(1 + t^2)(1 - t^2 + 2t) + sqrt(2)(1 + t^2)^2`
            // for `1/(cos(x) + sin(x) + sqrt(2))`. Divided out by long division, as many times
            // as both sides allow: what is left is `2/((sqrt(2) - 1)t^2 + 2t + 1 + sqrt(2))`, a
            // quadratic below the bar, which was a quartic nothing split.
            var (numerator, denominator) = Functions.SingleQuotient.Of(integrand);
            var divided = false;
            // The remainder comes back over the divisor, `0/(1 + t^2)` when there is none.
            static bool NoRemainder(Entity rest)
                => rest is Divf(var top, _) ? NoRemainder(top) : rest.Evaled is Number.Complex { IsZero: true };
            while (TreeAnalyzer.PolynomialLongDivision(numerator, 1 + tSquared, inTermsOf: t) is var (aboveQuotient, aboveRest)
                   && NoRemainder(aboveRest)
                   && TreeAnalyzer.PolynomialLongDivision(denominator, 1 + tSquared, inTermsOf: t) is var (belowQuotient, belowRest)
                   && NoRemainder(belowRest))
            {
                numerator = aboveQuotient.InnerSimplified;
                denominator = belowQuotient.InnerSimplified;
                divided = true;
            }
            if (divided)
                integrand = (numerator / denominator).InnerSimplified;

            return Integration.ComputeIndefiniteIntegral(integrand, t, integrateByParts) is { } result
                ? result.Substitute(t, MathS.Tan(argument / 2))
                : null;
        }

        /// <summary>
        /// Whether <paramref name="expr"/> is built from exponentials <c>b^(k x + c)</c>, with
        /// <c>b</c> free of x, by the field operations and whole powers, with at least one such
        /// exponential and nothing else of x.
        /// </summary>
        private static bool IsARationalFunctionOfExponentials(Entity expr, Entity.Variable x)
        {
            var anExponential = false;
            foreach (var node in expr.Nodes)
            {
                if (!node.ContainsNode(x))
                    continue;
                switch (node)
                {
                    case Variable or Sumf or Minusf or Mulf or Divf:
                    case Powf(_, Number.Integer):
                        continue;
                    // With a rational slope, as the exponential substitution requires: with a
                    // symbol for the slope that substitution declines, and this search is
                    // what answers `1/(a + b e^(p x))^2`.
                    case Powf(var @base, var exponent) when !@base.ContainsNode(x)
                        && TreeAnalyzer.TryGetPolyLinear(exponent, x, out var slope, out _) && slope.Evaled is Number.Rational:
                        anExponential = true;
                        continue;
                    default:
                        return false;
                }
            }
            // A bare x outside every exponential is a polynomial factor, which is by parts'
            // and not the exponential substitution's: `x e^x` keeps its search.
            if (!anExponential)
                return false;
            var stripped = expr.Replace(node => node is Powf(var @base, var exponent) && !@base.ContainsNode(x) && exponent.ContainsNode(x) ? Number.Integer.One : node);
            return !stripped.ContainsNode(x);
        }

        /// <summary>
        /// A quotient whose numerator is a constant multiple of its denominator's derivative,
        /// with a symbolic exponent about: <c>c ln(D)</c>, the constant read off the terms.
        /// </summary>
        /// <remarks>
        /// The general substitution answers <c>(x^2 - 1)/(x^3 - 3x)</c> under <c>u = x^3 - 3x</c>,
        /// where the one-level simplification of the quotient by <c>du/dx</c> divides the two
        /// polynomials; with a symbol in the exponent -- Timofeev's
        /// <c>(x^(n - 1) - 1)/(x^n - n x)</c> -- the derivative is written <c>x^n n / x - n</c>,
        /// and neither that quotient nor <c>(x^(n-1) - 1)/(n x^(n-1) - n)</c> is anything the
        /// simplifier reduces. So the numerator and the derivative are read term by term as
        /// a coefficient times a power of <c>x</c>, the powers of <c>x</c> in a term merged
        /// into one, the terms matched by exponent, and the coefficients' quotients asked to
        /// be one constant. https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveALogarithmicDerivativeWithASymbolicExponent(Entity expr, Entity.Variable x)
        {
            if (!TryReadAsQuotient(expr, out var numerator, out var denominator) || !denominator.ContainsNode(x))
                return null;
            if (!expr.Nodes.Any(node => node is Powf(var @base, var exponent) && @base.ContainsNode(x) && !exponent.ContainsNode(x) && exponent.Vars.Any()))
                return null;
            // Over a whole power of the denominator, `c D' / D^k`, the answer is a power too.
            var wholePower = 1;
            if (denominator is Powf(var powered, Number.Integer { EInteger.Sign: > 0 } k) && k.EInteger.CanFitInInt32() && k != Number.Integer.One)
            {
                denominator = powered;
                wholePower = k.EInteger.ToInt32Checked();
            }
            var above = TermsAsPowersOfX(numerator, x);
            var derivative = TermsAsPowersOfX(denominator.Differentiate(x), x);
            if (above is null || derivative is null || above.Count != derivative.Count)
                return null;
            Entity? constant = null;
            foreach (var (coefficient, exponent) in above)
            {
                var matching = derivative.Where(term => Bare((term.Exponent - exponent).Simplify()) == Number.Integer.Zero).ToList();
                if (matching.Count != 1)
                    return null;
                var ratio = Bare((coefficient / matching[0].Coefficient).Simplify());
                if (ratio.ContainsNode(x) || ratio.Nodes.Any(node => node == MathS.NaN))
                    return null;
                if (constant is null)
                    constant = ratio;
                else if (Bare((constant - ratio).Simplify()) != Number.Integer.Zero)
                    return null;
            }
            if (constant is null)
                return null;
            return wholePower == 1
                ? constant * MathS.Ln(denominator)
                : constant / (1 - wholePower) * MathS.Pow(denominator, 1 - wholePower);

            // A quotient of symbols simplifies under `provided not n = 0`, the integrand's
            // own condition; the value is what is compared.
            static Entity Bare(Entity simplified) => simplified is Providedf(var inner, _) ? inner : simplified;

            // Each term of a sum as a coefficient free of x times one power of x, the powers
            // of x in the term merged; null where a term holds x elsewhere.
            static List<(Entity Coefficient, Entity Exponent)>? TermsAsPowersOfX(Entity expr, Entity.Variable x)
            {
                // The derivative arrives under `provided not x = 0`, a condition of the
                // integrand's own domain, and the terms are read beneath it.
                if (expr is Providedf(var bare, _))
                    expr = bare;
                var terms = new List<(Entity, Entity)>();
                foreach (var term in Sumf.LinearChildren(expr))
                {
                    var (numerator, denominator) = Functions.SingleQuotient.Of(term);
                    Entity exponent = Number.Integer.Zero;
                    Entity coefficient = Number.Integer.One;
                    foreach (var (side, underneath) in new[] { (numerator, false), (denominator, true) })
                        foreach (var factor in Mulf.LinearChildren(side))
                        {
                            Entity? power = factor == x ? Number.Integer.One : factor is Powf(var @base, var e) && @base == x ? e : null;
                            if (power is null)
                            {
                                if (factor.ContainsNode(x))
                                    return null;
                                coefficient = underneath ? coefficient / factor : coefficient * factor;
                            }
                            else
                                exponent = underneath ? exponent - power : exponent + power;
                        }
                    terms.Add((coefficient, exponent.InnerSimplified));
                }
                return terms;
            }
        }

        /// <summary>
        /// Attempts to solve an integral using u-substitution.
        /// Looks for patterns where f(g(x)) * g'(x) can be integrated as F(g(x)).
        /// </summary>
        internal static Entity? SolveBySubstitution(Entity expr, Entity.Variable x, bool integrateByParts = true)
        {
            // A rational function over written linear factors with symbols in their
            // coefficients is the partial fractions', which read the coefficients off the
            // roots: `x^2/((a + b x)(c + d x)(f + g x)^2)` under `u = f/g + x` here was a
            // page of piecewise on the discriminant of the quadratic the other two make.
            if (IsARationalFunction(expr, x) && TryReadAsQuotient(expr, out _, out var writtenBelow) && IsAProductOfSymbolicLinearFactors(writtenBelow, x))
                return null;
            // A rational function of exponentials of linears in x with a whole power of a
            // sum of them in it is the exponential substitution's, exactly and at once, and
            // this search is not the tool for it: `tanh(x)^5/sech(x)^4` arrives as a fifth
            // power of a quotient of sums of `e^(2x)` over a fourth of one of `e^(-x)`, every sum a
            // candidate, and the search spent twenty-two seconds simplifying the quotient of
            // the integrand by each candidate's derivative to decline them all, where
            // `u = e^x` answers it in two. Without such a power -- `e^x/(1 + e^x)` -- the
            // search is quick and its answer, `ln(1 + e^x)`, the one to give.
            if (IsARationalFunctionOfExponentials(expr, x)
                && expr.Nodes.Any(node => node is Powf(var sum, Number.Integer power) && sum is Sumf or Minusf or Mulf or Divf && sum.ContainsNode(x)
                                          && power.EInteger.Abs().CompareTo(EInteger.FromInt32(2)) >= 0))
                return null;
            // And a function of exponentials with a root in it, past a modest size, the same:
            // Timofeev's `cosh(x)(tanh(x) - cosh(2x))/((sinh(x)^2 + sinh(2x)) sqrt(sinh(2x)))`
            // is a dozen sums of exponentials, each a candidate, and a minute of simplifying
            // to decline them all; `u = e^x` and the hyperbolic tangent are the substitutions
            // for it, and they follow this rule.
            if (expr.Complexity > LargestSymbolicIntegrandCollected && HasARadicalOf(expr, x)
                && !expr.Replace(node => node is Powf(var @base, var exponent) && !@base.ContainsNode(x) && exponent.ContainsNode(x) ? Number.Integer.One : node).ContainsNode(x))
                return null;
            // An exponential of a sum is the product of the exponentials, for this search
            // only: `e^(e^x) e^x` arrives as `e^(e^x + x)`, in which `e^x` is a candidate whose
            // derivative divides nothing, and written apart it is `e^u du`. Where two or more
            // terms of the exponent hold x; a constant term is left where it is.
            expr = expr.Replace(node =>
            {
                if (node is not Powf(var @base, var exponent) || exponent is not (Sumf or Minusf) || @base.ContainsNode(x)
                    || Sumf.LinearChildren(exponent).Count(term => term.ContainsNode(x)) < 2)
                    return node;
                Entity product = Number.Integer.One;
                foreach (var term in Sumf.LinearChildren(exponent))
                    product = product == Number.Integer.One ? MathS.Pow(@base, term) : product * MathS.Pow(@base, term);
                return product;
            });
            // Try to find a suitable substitution u = g(x)
            // We need to identify a composite function and check if du/dx appears in the integrand
            var candidates = FindSubstitutionCandidates(expr, x).ToList();
            // Every candidate as written first, and only then the sines and cosines again
            // with the even powers of their complement written in u: `cos(x)/sin(x)` under
            // `u = cos(x)` that way is `ln(1 - cos(x)^2)/2`, and under the sine it is written
            // with, `ln(sin(x))`, which is the answer to give.
            var firstPass = new Dictionary<Entity, Entity>();
            foreach (var complementEvenPowers in new[] { false, true })
            foreach (var u in candidates)
            {
                if (complementEvenPowers && !firstPass.ContainsKey(u))
                    continue;
                var duDx = u.Differentiate(x).InnerSimplified;

                // Under u = x^k the rewriting below takes x^m to u^(m/k) where that is whole
                // and leaves it where it is not, so a polynomial under a root with a power of
                // x that k does not divide keeps its x whatever else happens, and the
                // candidate is refused before its quotient is collected and simplified:
                // Hearn's `(2x^6 + ...)/((2x^2 - 1)^2 sqrt(x^4 + 4x^3 + 2x^2 + 1))` paid a
                // second each for x^3, x^4, x^5 and x^6 to be told so.
                if (u is Powf(var xOfPower, Number.Integer { EInteger.Sign: > 0 } k) && xOfPower == x && k != Number.Integer.One
                    && ARadicandHasAPowerNotDivisibleBy(expr, x, k.EInteger))
                    continue;
                // A sum that stands nowhere but inside a longer sum is a node of the tree
                // and not a subexpression anyone wrote: `1 + 2x^2` and `1 + 2x^2 + 4x^3` are
                // the left-nested partial sums of Hearn's radicand `1 + 2x^2 + 4x^3 + x^4`,
                // and each was half a second of rewriting and simplifying to be refused.
                if (u is Sumf && IsOnlyAPartialSum(expr, u))
                    continue;

                // A candidate that does not vary with x is no substitution at all, and its
                // derivative is zero: dividing the integrand by it gives NaN, which then
                // contains no x and so passes the test below and is returned as the answer.
                // `ln(e)`, picked up as a logarithm anywhere in the integrand, was enough --
                // it is how the antiderivative of x * ln(x) came back holding a NaN.
                if (!u.ContainsNode(x) || duDx.Evaled == 0)
                    continue;

                // Try to express expr as h(u) * du/dx
                // If successful, integral becomes ∫h(u)du
                var uSub = Variable.CreateUnique(expr, "u_sub");

                // Try to divide expr by duDx and check if result is independent of x
                // Replace all occurrences of u's expression with a temporary variable
                // For a power of x, the quotient is collected before the powers of x are
                // rewritten: the rewriting reads written powers, and `1/(x sqrt(P))` over `2x`
                // holds two bare x's that are `x^2` only once collected -- and a bare x is not
                // a power of x^2, so the candidate was refused.
                // Under u = cos(a), an even power of sin(a) is a power of 1 - u^2, and the odd
                // ones are a sine times one; the same the other way round. Written so after the
                // division by du/dx has taken one sine out of `sin(x)/sqrt(1 - sin(x)^6)`, since
                // that is where the sixth power becomes a polynomial in u.
                Entity integrandInU;
                if (complementEvenPowers)
                {
                    // From what the first pass computed, not computed again.
                    var complemented = WithTheComplementInEvenPowers(firstPass[u], u, uSub);
                    if (complemented == firstPass[u])
                        continue;   // nothing the first pass did not see
                    integrandInU = complemented.Simplify(1);
                    if (integrandInU is Providedf(var innerComplemented, _)) integrandInU = innerComplemented;
                }
                else
                {
                    // For a whole power of x of an integrand of modest size, from the quotient
                    // written as one with its powers of x collected. Modest only, and for one
                    // with symbols in it small: a symbolic partial fraction written as one
                    // quotient is a page, and its simplification took twelve minutes of one
                    // test, where Hearn's `1/(r sqrt(-alpha^2 - epsilon^2 + 2h r^2 - 2k r^4))`
                    // is a line and `u = r^2` is its substitution. Whole only: `x^(-1/2)`
                    // collected the same way admitted a substitution the by-parts remainder of
                    // `arcsin(sqrt(1 + x) - sqrt(x))` was refused before, and a minute of search
                    // below it.
                    // Under a sine or a cosine, the cotangent, tangent, secant and cosecant of
                    // the same argument are written in the two: `cot(x) sin(x)^9` under `u = sin(x)`
                    // is `cos(x) sin(x)^8`, and the cotangent as written is nothing the
                    // rewriting reads.
                    // Outside a root only: `sqrt(4 sec(x)^2 + 5 tan(x)^2)` written in the cosine
                    // is a root of a quotient by `cos(x)^2`, which the simplification below takes
                    // as a quotient by `cos(x)`, its value on half the line.
                    var trigonometricArgument = u is Sinf or Cosf ? u.DirectChildren.First() : null;
                    var underARoot = new HashSet<Entity>();
                    if (trigonometricArgument is not null)
                        foreach (var node in expr.Nodes)
                            if (node is Powf(_, Number.Rational r) && r is not Number.Integer)
                                foreach (var inside in node.Nodes)
                                    underARoot.Add(inside);
                    var source = trigonometricArgument is null ? expr : expr.Replace(node => underARoot.Contains(node) ? node : node switch
                    {
                        Cotanf(var a) when a == trigonometricArgument => MathS.Cos(a) / MathS.Sin(a),
                        Tanf(var a) when a == trigonometricArgument => MathS.Sin(a) / MathS.Cos(a),
                        Secantf(var a) when a == trigonometricArgument => 1 / MathS.Cos(a),
                        Cosecantf(var a) when a == trigonometricArgument => 1 / MathS.Sin(a),
                        _ => node,
                    });
                    var quotient = u is Powf(var powerOfX, Number.Integer { EInteger.Sign: > 0 } wholePower) && powerOfX == x && wholePower != Number.Integer.One
                        && expr.Complexity <= (expr.Vars.Any(v => v != x) ? LargestSymbolicIntegrandCollected : LargestIntegrandOfferedSums)
                        ? WithThePowersOfXCollected(Functions.SingleQuotient.Combine(expr / duDx), x)
                        : source / duDx;
                    integrandInU = InTermsOf(quotient, u, uSub, x).Simplify(1);
                    if (integrandInU is Providedf(var innerExpr, _)) integrandInU = innerExpr; // TODO: singularities ignored but not handled properly
                    // A factor written on both sides of the bar cancelled, where x survived:
                    // the one-level simplification leaves `u/((a w + b)^2 p u)` as it is, and
                    // the candidate was refused for the u it did not cancel.
                    if (integrandInU.ContainsNode(x))
                    {
                        var (top, bottom) = Functions.SingleQuotient.Of(Functions.SingleQuotient.Combine(integrandInU));
                        var cancelled = CancelCommonFactors(top, bottom);
                        if (cancelled != integrandInU && !cancelled.ContainsNode(x))
                        {
                            integrandInU = cancelled.Simplify(1);
                            if (integrandInU is Providedf(var innerCancelled, _)) integrandInU = innerCancelled;
                        }
                    }
                    // A polynomial in x left over under a candidate that is itself a polynomial
                    // is written in the candidate where it is one in it: `(1 - x)^2` is
                    // `1 - 2x + x^2`, and that is `u` for Apostol's `(1 - 2x + x^2)^(1/5)/(1 - x)`,
                    // whose quotient by du/dx is `-u^(1/5)/(2 (1 - x)^2)` and was refused for
                    // the square that is not spelled as the candidate.
                    // For a quotient of modest size: Welz's `1/((3 - 2x)^(41/2) (1 + x + 2x^2)^20)`
                    // offers its quadratic, and expanding the twentieth power to be told it
                    // is not a polynomial in the other was twenty seconds.
                    if (integrandInU.ContainsNode(x) && u is Sumf or Minusf && integrandInU.Complexity <= LargestSymbolicIntegrandCollected
                        && TreeAnalyzer.TryGetPolynomial(u, x, out var candidateMonomials) && candidateMonomials.Keys.Max() is { } candidateDegree
                        && candidateDegree.CompareTo(EInteger.FromInt32(2)) >= 0)
                    {
                        // Over one bar, so that `(1 - x)(2x - 2)` is one polynomial and not two
                        // linears on either side of a nested division.
                        var (aboveTheBar, belowTheBar) = Functions.SingleQuotient.Of(Functions.SingleQuotient.Combine(integrandInU));
                        var inTheCandidate = WithPolynomialsInTheCandidate(aboveTheBar, u, uSub, x) / WithPolynomialsInTheCandidate(belowTheBar, u, uSub, x);
                        if (inTheCandidate.ContainsNode(x) == false || inTheCandidate != aboveTheBar / belowTheBar)
                        {
                            integrandInU = inTheCandidate.Simplify(1);
                            if (integrandInU is Providedf(var innerInTheCandidate, _)) integrandInU = innerInTheCandidate;
                        }
                    }
                    if (u is Sinf or Cosf && integrandInU.ContainsNode(x))
                        firstPass[u] = integrandInU;
                }

                // If the result doesn't contain x anymore, we found a valid substitution
                // and we can integrate with respect to u (treating u as a variable)
                // Dividing by a derivative that is zero gives NaN, and NaN contains no x,
                // so it passes the test below and is handed back as the answer. The check
                // on duDx above catches the cases where that is visible without work;
                // this catches the rest, where the derivative is only zero after
                // simplification -- d/dx (sin(x)^2 + cos(x)^2) is written as
                // 2sin(x)cos(x) - 2cos(x)sin(x), which Evaled cannot reduce with x still
                // symbolic. That is how the integral of sin(x)^2 + cos(x)^2 came back as
                // NaN * (sin(x)^2 + cos(x)^2).
                if (integrandInU.Nodes.Any(node => node == MathS.NaN))
                    continue;

                if (integrandInU.ContainsNode(x))
                    continue;
                if (Integration.ComputeIndefiniteIntegral(integrandInU, uSub, integrateByParts) is { } resultInU)
                {
                    // An even root is not negative wherever it is real, so a sign or a modulus
                    // of it that a rule below put in -- the answer for u > 0 extended by parity
                    // -- is the root itself: `sqrt(x)/sqrt(x + x^2)` under `u = sqrt(x)` is
                    // `2u^2/sqrt(u^2 + u^4)`, answered as `2 sgn(u) sqrt(1 + |u|^2)`, and
                    // `sgn(sqrt(x))` is a sign nothing differentiates.
                    if (u is Powf(_, Number.Rational evenRoot) && evenRoot.ERational.Denominator.IsEven)
                        resultInU = resultInU.Replace(node => node switch
                        {
                            Signumf(var argument) when argument == uSub => Number.Integer.One,
                            Absf(var argument) when argument == uSub => uSub,
                            _ => node,
                        });
                    // Substitute back: replace u with g(x)
                    return resultInU.Substitute(uSub, u);
                }

                // A power of u under a root -- `1/sqrt(u^2 (3 - 3u^2 + u^4))`, which is
                // `sin(x)/sqrt(1 - sin(x)^6)` under `u = cos(x)` -- comes out of the root as
                // `|u|`, and the rule for that takes `|u| = u`: what it answers holds for
                // `u > 0`, and is extended to `u < 0` by parity where the integrand has one, as
                // the reciprocal substitution extends its own. Nothing is known of the sign
                // of a cosine, and this is what makes that not matter.
                // Asked, not volunteered: the factored form is a second search per candidate,
                // and one level down the by-parts remainder of `arcsin(sqrt(1 + x) - sqrt(x))`
                // spent a minute in them.
                if (!Integration.AnsweringTheQuestionAsked)
                    continue;
                var factored = FactorANonnegativeVariableOutOfRadicals(integrandInU, uSub);
                if (factored != integrandInU && !factored.ContainsNode(x)
                    && Integration.ComputeIndefiniteIntegral(factored, uSub, integrateByParts) is { } forPositiveU
                    && !forPositiveU.Nodes.Any(node => node == MathS.NaN))
                {
                    // An even root is not negative, and what holds for u > 0 is the answer as
                    // it stands; `sqrt(x^(1/3))` under `1/(sqrt(x) - x^(-1/3))` was extended by
                    // parity it did not need, to an answer with a sign function in it.
                    if (u is Powf(_, Number.Rational root) && root.ERational.Denominator.IsEven)
                        return Functions.PartialFractions.Bare(forPositiveU).Substitute(uSub, u);
                    if (ExtendedByParity(integrandInU, forPositiveU, uSub) is { } onBothSides)
                        return onBothSides.Substitute(uSub, u);
                }
            }

            return null;
        }

        /// <summary>
        /// Every product in <paramref name="expr"/> with its powers of <paramref name="x"/>
        /// gathered into one: <c>x sqrt(P) 2 x</c> is <c>2 x^2 sqrt(P)</c>. The power rewriting
        /// of a substitution <c>u = x^r</c> reads written powers, and the quotient of an
        /// integrand by <c>du/dx</c> writes the differential's power beside the integrand's,
        /// where neither alone is a power of <c>u</c>.
        /// </summary>
        private static Entity WithThePowersOfXCollected(Entity expr, Entity.Variable x)
            => expr.Replace(node =>
            {
                // Across the bar of a quotient too: `x^9 sqrt(1 + x^5 + x^10)/(5x^4)` is
                // `x^5 sqrt(...)/5`, and `u = x^5` reads the collected power where it did not
                // read the two.
                if (node is Divf(var top, var bottom))
                {
                    var (topPower, topRest, topPowers) = Split(top);
                    var (bottomPower, bottomRest, bottomPowers) = Split(bottom);
                    if (topPowers + bottomPowers < 2 || bottomPowers == 0)
                        return node;
                    var net = (topPower - bottomPower).InnerSimplified;
                    if (net.Evaled is Number.Real { IsNegative: true })
                        return topRest / (MathS.Pow(x, (-net).InnerSimplified) * bottomRest);
                    if (net.Evaled is Number.Complex { IsZero: true })
                        return topRest / bottomRest;
                    return (net == Number.Integer.One ? x : MathS.Pow(x, net)) * topRest / bottomRest;
                }
                if (node is not Mulf)
                    return node;
                var (total, product, powers) = Split(node);
                if (powers < 2)
                    return node;
                var collected = total.InnerSimplified;
                return (collected == Number.Integer.One ? x : MathS.Pow(x, collected)) * product;

                // The total power of x among the written factors, the product of the rest,
                // and how many factors were powers of x.
                (Entity Power, Entity Remaining, int Count) Split(Entity product)
                {
                    Entity total = Number.Integer.Zero;
                    Entity rest = Number.Integer.One;
                    var count = 0;
                    foreach (var factor in Mulf.LinearChildren(product))
                    {
                        if (factor == x)
                        {
                            total += 1;
                            count++;
                        }
                        else if (factor is Powf(var @base, Number.Rational exponent) && @base == x)
                        {
                            total += exponent;
                            count++;
                        }
                        else
                            rest = rest == Number.Integer.One ? factor : rest * factor;
                    }
                    return (total, rest, count);
                }
            });

        /// <summary>
        /// For <paramref name="u"/> a sine or a cosine, every even power of the other function
        /// of the same argument in <paramref name="expr"/> written as a power of
        /// <c>1 - uSub^2</c>, and every odd one as the function times such a power; the
        /// expression itself where there is nothing to write.
        /// </summary>
        private static Entity WithTheComplementInEvenPowers(Entity expr, Entity u, Entity.Variable uSub)
        {
            Entity argument;
            Entity complement;
            switch (u)
            {
                case Sinf(var a):
                    argument = a;
                    complement = MathS.Cos(a);
                    break;
                case Cosf(var a):
                    argument = a;
                    complement = MathS.Sin(a);
                    break;
                default:
                    return expr;
            }
            // The other four functions of the argument as quotients of the sine and cosine
            // first, and the substituted function as u again where that has written it anew:
            // `sin(x)/sqrt(sec(x) - 1)` under `u = cos(x)` is `-1/sqrt(1/u - 1)`, and with the
            // secant left standing it kept its x.
            var written = expr.Replace(node => node switch
            {
                Secantf(var a) when a == argument => 1 / MathS.Cos(a),
                Cosecantf(var a) when a == argument => 1 / MathS.Sin(a),
                Tanf(var a) when a == argument => MathS.Sin(a) / MathS.Cos(a),
                Cotanf(var a) when a == argument => MathS.Cos(a) / MathS.Sin(a),
                _ => node,
            }).Substitute(u, uSub);
            // Not where that has put the complement under a root: `sqrt(4/cos(x)^2 + ...)`
            // under `u = sin(x)` is a root of a quotient by `cos(x)^2`, and the simplification
            // the substitution runs takes that root as `.../cos(x)`, which is its value where
            // the cosine is positive and its negative elsewhere -- Timofeev's
            // `(sec(x)^2 - 3 sqrt(4 sec(x)^2 + 5 tan(x)^2) tan(x))/(sin(x)^2 (...)^(3/2))` was
            // answered wrongly on half the line that way.
            var oneMinusSquare = 1 - MathS.Sqr(uSub);
            var rewritten = written.Replace(node =>
            {
                if (node is not Powf(var @base, Number.Integer power) || @base != complement
                    || !power.EInteger.CanFitInInt32() || power.EInteger.ToInt32Unchecked() is var n && n < 2)
                    return node;
                var half = n / 2;
                Entity even = half == 1 ? oneMinusSquare : MathS.Pow(oneMinusSquare, half);
                return n % 2 == 0 ? even : complement * even;
            });
            // An even power of the complement under a root is gone by now; one still there is
            // odd, and is refused rather than left to the simplification.
            return rewritten.Nodes.Any(node => node is Powf(var radicalBase, Number.Rational r) && r is not Number.Integer && radicalBase.ContainsNode(complement))
                ? expr
                : rewritten;
        }

        /// <summary>
        /// The substitution <c>u = sin(a)</c> or <c>u = cos(a)</c> where an odd power of the
        /// complement is left standing after the division by <c>du/dx</c>: that power is the
        /// sign of the complement times a power of <c>sqrt(1 - u^2)</c>, and the sign is a
        /// constant on every interval between the zeros of the complement, so the integrand
        /// is that constant times an algebraic function of <c>u</c>, which is integrated, and
        /// the sign goes back in as <c>sgn(cos(a))</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Charlwood's <c>ln(sin(x)) sqrt(1 + sin(x))</c> by parts leaves
        /// <c>-2 cos(x)^2/(sin(x) sqrt(1 + sin(x)))</c>, which under the sine is
        /// <c>-2 cos(x)/(u sqrt(1 + u)) du</c>, a cosine over: it is <c>-2 sgn(cos(x)) sqrt(1 - u)/u</c>,
        /// whose integral is <c>-4 sqrt(1 - u) + 4 atanh(sqrt(1 - u))</c> times the sign. And
        /// Charlwood's <c>cos(x)^2/sqrt(1 + cos(x)^2 + cos(x)^4)</c> under the cosine is
        /// <c>-u^2/(sin(x) sqrt(1 + u^2 + u^4))</c>, which is <c>-sgn(sin(x)) u^2/sqrt(1 - u^6)</c>
        /// once the roots combine, and <c>-sgn(sin(x)) arcsin(u^3)/3</c>. Exact wherever the
        /// complement is not zero, the generic case.
        /// </para>
        /// <para>
        /// A rule of its own and late, after the half-angle substitution: inside the general
        /// substitution it answered <c>1/(1 + sin(x))</c> as a sign times a root where the
        /// half-angle substitution gives the tangent of the half angle, and the remainder by
        /// parts left beside that root was nine seconds of search for
        /// <c>ln(sin(x))/(1 + sin(x))</c>, which the tangent answers in a moment. Only where
        /// the complement is a factor of the product and what is left is algebraic in
        /// <c>u</c>: inside a sum, <c>1/(cos(x) + sin(x))</c>, or beside a logarithm of <c>u</c>,
        /// the search with the sign in it ended nowhere.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveByTheSignOfTheComplement(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            // For an integrand with a root in it: a rational function of the sine and cosine
            // is the half-angle substitution's, answered before this, and Jeffrey's
            // `(-1 + 4 cos(x) + 5 cos(x)^2)/(-1 - 4 cos(x) - 3 cos(x)^2 + 4 cos(x)^3)` paid half
            // a second here on the way to it.
            if (!HasARadicalOf(expr, x))
                return null;
            foreach (var u in expr.Nodes.Where(node => node is Sinf or Cosf && node.ContainsNode(x)).Distinct().ToList())
            {
                var argument = u is Sinf(var sa) ? sa : ((Cosf)u).Argument;
                if (!TreeAnalyzer.TryGetPolyLinear(argument, x, out _, out _))
                    continue;
                var complement = u is Sinf ? MathS.Cos(argument) : MathS.Sin(argument);
                var duDx = u.Differentiate(x).InnerSimplified;
                if (duDx.Evaled == 0)
                    continue;
                var uSub = Variable.CreateUnique(expr, "u_sgn");
                var inU = WithTheComplementInEvenPowers(InTermsOf(expr / duDx, u, uSub, x).Simplify(1), u, uSub);
                if (inU is Providedf(var bare, _)) inU = bare;
                if (!inU.ContainsNode(complement))
                    continue;
                // The complement as a factor of the product only, and what is left algebraic.
                var (top, bottom) = Functions.SingleQuotient.Of(inU);
                if (!Mulf.LinearChildren(top).Concat(Mulf.LinearChildren(bottom))
                        .All(factor => !factor.ContainsNode(complement) || factor == complement || factor is Powf(var oddBase, Number.Integer) && oddBase == complement))
                    continue;
                var sign = Variable.CreateUnique(inU, "sgn_c");
                var withTheSign = inU.Replace(node => node == complement ? sign * MathS.Sqrt(1 - MathS.Sqr(uSub)) : node);
                if (withTheSign.ContainsNode(x) || !IsAlgebraicIn(withTheSign, uSub))
                    continue;
                withTheSign = withTheSign.Simplify(1);
                if (withTheSign is Providedf(var innerSigned, _)) withTheSign = innerSigned;
                if (withTheSign.ContainsNode(x) || withTheSign.Nodes.Any(node => node == MathS.NaN))
                    continue;
                if (Integration.ComputeIndefiniteIntegral(withTheSign, uSub, integrateByParts) is { } signedInU
                    && !signedInU.Nodes.Any(node => node == MathS.NaN))
                    return signedInU.Substitute(sign, MathS.Signum(complement)).Substitute(uSub, u);
            }
            return null;
        }

        /// <summary>
        /// Whether every occurrence of the sum <paramref name="u"/> in <paramref name="expr"/>
        /// is as a summand of a longer sum.
        /// </summary>
        private static bool IsOnlyAPartialSum(Entity expr, Entity u)
        {
            if (expr == u)
                return false;
            foreach (var node in expr.Nodes)
                if (node is not Sumf && node.DirectChildren.Any(child => child == u))
                    return false;
            return true;
        }

        /// <summary>
        /// Whether a polynomial in <paramref name="x"/> of degree at least two under a
        /// fractional power has a power of <paramref name="x"/> that <paramref name="k"/>
        /// does not divide.
        /// </summary>
        private static bool ARadicandHasAPowerNotDivisibleBy(Entity expr, Entity.Variable x, PeterO.Numbers.EInteger k)
        {
            foreach (var node in expr.Nodes)
                if (node is Powf(var radicand, Number.Rational exponent) && exponent is not Number.Integer && radicand.ContainsNode(x)
                    && TreeAnalyzer.TryGetPolynomial(radicand, x, out var monomials) && monomials.Count >= 2)
                    foreach (var power in monomials.Keys)
                        if (!power.Remainder(k).IsZero)
                            return true;
            return false;
        }

        /// <summary>
        /// <paramref name="expr"/> written in terms of <paramref name="uSub"/>, where that stands
        /// for <paramref name="u"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Ordinarily that is a plain substitution: the candidate occurs in the integrand, and
        /// replacing it is all that is wanted.
        /// </para>
        /// <para>
        /// <b>A power of the variable is the exception, and it is the one that mattered.</b>
        /// Substituting <c>u = x^2</c> into <c>x / (x^4 + 1)</c> replaces nothing, because
        /// <c>x^4</c> is not written as <c>(x^2)^2</c> and a substitution matches what is
        /// written. So the integrand kept its <c>x</c>, the candidate was rejected, and
        /// <c>int x / (x^4 + 1)</c> came back unevaluated while <c>int x^3 / (x^4 + 1)</c> —
        /// whose substitution does occur — did not.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
        /// </para>
        /// <para>
        /// So a power substitution rewrites the other powers of the variable into powers of
        /// itself. For <c>u = x^r</c> the identity is <c>x^n = u^(n/r)</c>, and the rewrite is
        /// made wherever <c>n/r</c> is a whole number — which for a whole <c>r</c> means
        /// <c>r</c> divides <c>n</c>, and is the only case this covered at first.
        /// </para>
        /// <para>
        /// <b>A fractional <c>r</c> is the case that reaches the other way</b>, and it is what
        /// <c>int sqrt(x)/(1 + x^2)</c> needs. There <c>u = sqrt(x)</c>, so <c>r</c> is
        /// <c>1/2</c> and <c>n/r</c> is <c>2n</c> — a whole number for every <c>n</c>, including
        /// the bare <c>x</c> that a whole <c>r</c> can never rewrite. The integrand becomes
        /// <c>2u^2/(1 + u^4)</c>, which is answered.
        /// </para>
        /// <para>
        /// Nothing is assumed by it: the caller still checks that no <c>x</c> survives, so a
        /// rewrite that does not clear the variable leaves the candidate rejected as before.
        /// </para>
        /// </remarks>
        private static Entity InTermsOf(Entity expr, Entity u, Entity.Variable uSub, Entity.Variable x)
        {
            if (u is not Powf(var powerBase, Number.Rational exponent) || powerBase != x)
                return expr.Substitute(u, uSub);
            var r = exponent.ERational;
            if (r.IsZero)
                return expr.Substitute(u, uSub);
            // The written powers of x first, and only then a bare x that is left over. Both in
            // one pass does not work, because Replace rewrites from the leaves up: the x inside
            // sqrt(x) is reached before the sqrt(x) node is, so with u = sqrt(x) it becomes
            // sqrt(u^2) rather than u, and the integrand ends up free of x and no more
            // integrable than it started.
            //
            // The exponent is read as a rational rather than a whole number because the
            // candidate is itself one of these nodes: with u = sqrt(x) the integrand still holds
            // a sqrt(x), and leaving it alone leaves an x that rejects the candidate. Anything
            // else -- x inside a function, or a base that is not x -- is left for the caller's
            // check on whether x survives.
            var written = expr.Replace(node =>
                node is Powf(var otherBase, Number.Rational otherExponent)
                && otherBase == x
                && Rewritten(otherExponent.ERational) is { } power
                    ? power
                    : node);
            return written.Replace(node =>
                node == x && Rewritten(PeterO.Numbers.ERational.One) is { } bare ? bare : node);

            // u^(n/r), where that is a whole power, and null where it is not. Done on the four
            // integers rather than by dividing the rationals and asking whether the result is
            // whole: an ERational quotient is not reduced, so 4/2 answers that question with a
            // denominator of two and every whole r would be refused.
            //
            // The ceiling is against a tiny r turning a modest power of x into an enormous one,
            // rather than against anything mathematical.
            Entity? Rewritten(PeterO.Numbers.ERational n)
            {
                var scaled = n.Numerator.Multiply(r.Denominator);
                var by = n.Denominator.Multiply(r.Numerator);
                if (by.IsZero || !scaled.Remainder(by).IsZero)
                    return null;
                var whole = scaled.Divide(by);
                if (whole.IsZero || whole.Abs().CompareTo(PeterO.Numbers.EInteger.FromInt32(64)) > 0)
                    return null;
                return whole.Equals(PeterO.Numbers.EInteger.One)
                    ? uSub
                    : MathS.Pow(uSub, Number.Integer.Create(whole));
            }
        }

        /// <summary>
        /// Every polynomial in <paramref name="x"/> in <paramref name="expr"/> of degree at least
        /// that of the polynomial <paramref name="u"/> that is a polynomial in <paramref name="u"/>
        /// with constant coefficients, written as that polynomial in <paramref name="uSub"/>:
        /// <c>F = r_0 + r_1 u + r_2 u^2 + ...</c> by repeated division, taken where every
        /// remainder is a constant.
        /// </summary>
        private static Entity WithPolynomialsInTheCandidate(Entity expr, Entity u, Entity.Variable uSub, Entity.Variable x)
        {
            if (!TreeAnalyzer.TryGetPolynomial(u, x, out var candidateMonomials) || candidateMonomials.Keys.Max() is not { } candidateDegree)
                return expr;
            var degree = candidateDegree.ToInt32Checked();
            return expr.Replace(node =>
            {
                // Small powers only: a written power is expanded to be read, and a large one
                // is a large polynomial to divide for nothing.
                if (node == x || !node.ContainsNode(x)
                    || node.Nodes.Any(inner => inner is Powf(_, var exponent) && (exponent is not Number.Integer wholePower || wholePower.EInteger.Abs().CompareTo(EInteger.FromInt32(4)) > 0))
                    || !TreeAnalyzer.TryGetPolynomial(node, x, out var monomials)
                    || monomials.Keys.Max() is not { } top || top.ToInt32Checked() < degree || top.ToInt32Checked() > 4 * degree)
                    return node;
                Entity? written = null;
                var rest = node;
                for (var power = 0; ; power++)
                {
                    if (!rest.ContainsNode(x))
                    {
                        var last = rest.InnerSimplified;
                        if (!TreeAnalyzer.IsZero(last))
                            written = Term(written, last, power);
                        return written ?? Number.Integer.Zero;
                    }
                    if (TreeAnalyzer.PolynomialLongDivision(rest, u, genericCase: true, inTermsOf: x) is not var (quotient, _))
                        return node;
                    var divided = quotient.Expand().InnerSimplified;
                    var remainder = (rest - divided * u).Expand().InnerSimplified;
                    if (remainder.ContainsNode(x))
                        return node;
                    if (!TreeAnalyzer.IsZero(remainder))
                        written = Term(written, remainder, power);
                    rest = divided;
                }
            });

            Entity Term(Entity? sum, Entity coefficient, int power)
            {
                Entity term = power == 0 ? coefficient
                    : coefficient == Number.Integer.One ? MathS.Pow(uSub, power)
                    : coefficient * MathS.Pow(uSub, power);
                return sum is null ? term : sum + term;
            }
        }

        /// <summary>The largest integrand the substitution rule offers its sums as candidates for.</summary>
        private const int LargestIntegrandOfferedSums = 120;

        /// <summary>
        /// The largest integrand with symbols in it whose quotient by <c>du/dx</c> is written
        /// as one with its powers of <c>x</c> collected, for <c>u</c> a power of <c>x</c>.
        /// </summary>
        private const int LargestSymbolicIntegrandCollected = 40;

        /// <summary>The largest sum the substitution rule offers as a candidate.</summary>
        private const int LargestSumOffered = 30;

        /// <summary>
        /// Finds potential substitution candidates u = g(x) from the expression.
        /// For example, common patterns to try:
        /// 1. f(ax + b) * a  ->  u = ax + b
        /// 2. f(x^n) * x^(n-1)  ->  u = x^n
        /// 3. f(g(x)) * g'(x)  ->  u = g(x)
        /// </summary>
        private static IEnumerable<Entity> FindSubstitutionCandidates(Entity expr, Entity.Variable x)
        {
            var candidates = new List<Entity>();
            var complements = new List<Entity>();
            // A sum is no candidate for a rational function: whatever `u = g(x)` with `g` a
            // polynomial would find in one, partial fractions find without it, and each
            // candidate costs one simplification of the quotient -- which, with an irrational
            // constant in the coefficients, is where `(1 + t^2)/((sqrt(2) - 1) t^2 + 2t + 1 + sqrt(2))`
            // spent eight seconds being declined by this rule before the rational integrator
            // answered it in a few milliseconds. The base of a written power stays a candidate
            // for a *polynomial*, since `x (x^2 + 1)^3` is answered as a power that way and as
            // a degree-eight polynomial otherwise; for a quotient it goes the way of the sums --
            // `(t^2 + t + 1)^2` below the bar is one more simplification of a rational function
            // for nothing, and Timofeev's `(1 + x^4)/((1 + x + x^2) sqrt(2 + x + x^2))` spent
            // thirteen of its fourteen seconds on those, over the rational functions Euler's
            // substitution and by parts handed down.
            var rational = IsRationalIn(expr, x);
            var polynomial = rational && TreeAnalyzer.TryGetPolynomial(expr, x, out _);
            // Nor a sum past a certain size: each sum candidate is one simplification of the
            // quotient, and the sums in the hundred-node remainders a by-parts step hands
            // down -- `x^3 + 3x^2 sqrt(x^2 + 1) + 3x (x^2 + 1) + (x^2 + 1)^(3/2)` was one -- are
            // seconds apiece for nothing; one such remainder of
            // `x ln(1 + x^2) ln(x + sqrt(1 + x^2))/sqrt(1 + x^2)` spent twelve seconds here.
            var large = expr.Complexity > LargestIntegrandOfferedSums;
            // For a rational function `u = x^k` is exact only when every exponent below the bar
            // is a multiple of k and every one above is k - 1 more than one -- the x^(k-1) of
            // du -- so the candidate is read off the exponents before the quotient is
            // simplified for it. Each simplification it saves is one to two seconds on the
            // degree-eight rational functions Euler's substitution hands down.
            HashSet<int>? exponentsAbove = null, exponentsBelow = null;
            if (rational && TryReadAsQuotient(expr, out var aboveForPowers, out var belowForPowers)
                && TreeAnalyzer.TryGetPolynomial(aboveForPowers, x, out var aboveRead)
                && TreeAnalyzer.TryGetPolynomial(belowForPowers, x, out var belowRead))
            {
                exponentsAbove = new HashSet<int>(aboveRead.Keys.Where(k => k.CanFitInInt32()).Select(k => k.ToInt32Unchecked()));
                exponentsBelow = new HashSet<int>(belowRead.Keys.Where(k => k.CanFitInInt32()).Select(k => k.ToInt32Unchecked()));
            }
            bool APowerCanBeExact(int k)
                => exponentsAbove is null || exponentsBelow is null
                   || (exponentsBelow.All(e => e % k == 0) && exponentsAbove.All(e => e % k == k - 1));
            // The reciprocal, where it is under a root: `sqrt(1/x + sqrt(1 + 1/x))` is
            // `sqrt(u + sqrt(1 + u))` over `-u^2` under `u = 1/x`, a nested radical of something
            // linear, and `1/x` is written as a quotient that no power candidate reads. Under a
            // root only -- offered for every `/x`, it opened a search on `(x^2 - 10)^(5/2)/x` and
            // `x ln(x)/sqrt(1 + x^2)` that did not return, where each is a second's work
            // without it. And not beside a root of a polynomial in the variable: under the
            // reciprocal that root becomes a root of a reciprocal, which offers the reciprocal
            // back, and `x^(-1/2)` is a candidate of its own that leads the same way -- on the
            // by-parts remainder of `arcsin(sqrt(1 + x) - sqrt(x))` the two alternated to the
            // depth limit, forty seconds where declining takes three.
            var besideARootOfAPolynomial = expr.Nodes.Any(node => node is Powf(var polynomialBase, Number.Rational rootPower) && rootPower is not Number.Integer
                    && polynomialBase.ContainsNode(x) && TreeAnalyzer.TryGetPolynomial(polynomialBase, x, out var radicand)
                    && radicand.Keys.Any(degree => degree.Sign > 0));
            if (!rational && expr.Nodes.Any(node => node is Powf(var radicalBase, Number.Rational radicalPower) && radicalPower is not Number.Integer
                    && radicalBase.Nodes.Any(inner => inner is Divf(_, var divisor) && (divisor == x || divisor is Powf(var pb, Number.Integer) && pb == x)))
                && !besideARootOfAPolynomial)
                candidates.Add(MathS.Pow(x, -1));
            foreach (var node in expr.Nodes) // Look for composite functions (functions of functions)
                switch (node)
                {
                    case TrigonometricFunction:
                        candidates.Add(node); // Trigonometric function itself (for cases like sin(x)*cos(x))
                        if (node.DirectChildren[0] != x && node.DirectChildren[0].ContainsNode(x))
                            candidates.Add(node.DirectChildren[0]); // Trigonometric functions with non-trivial arguments
                        // And the other of the pair, which need not be written to be the
                        // substitution: `sin(x)/sqrt(1 - sin(x)^6)` wants `u = cos(x)`, under
                        // which the sine that is left is the differential and the even powers
                        // of it are `1 - u^2`. Only for a sine or cosine, whose complement is
                        // reached through even powers alone; and after everything written,
                        // since `cos(x)/sin(x)` under `u = cos(x)` is `ln(1 - cos(x)^2)/2`, and
                        // under the sine it is written with, `ln(sin(x))`.
                        if (node is Sinf(var sineArgument) && sineArgument.ContainsNode(x))
                            complements.Add(MathS.Cos(sineArgument));
                        else if (node is Cosf(var cosineArgument) && cosineArgument.ContainsNode(x))
                            complements.Add(MathS.Sin(cosineArgument));
                        break;
                    case Powf(var @base, var exp):
                        // A whole negative power beside a root of a polynomial in the variable
                        // is the reciprocal that the guard above declines, and for its reason:
                        // as a power candidate `x^(-1)` took `sqrt(u - 1)/(u sqrt(u^2 - u))` to
                        // `sqrt(1/u - 1)/(u sqrt(1/u^2 - 1/u))` and back, sixteen levels of
                        // `1/(1/(1/x))` in the answer to Timofeev's `arccsc(x)/(x^2 (x^2 - 1)^(5/2))`.
                        // Beside a root of anything else it stays: Bronstein's
                        // `(5x^2 + 3(e^x + x)^(1/3) + e^x (3x + 2x^2))/(x (e^x + x)^(1/3))` is answered
                        // through it.
                        if (@base == x && (exp is not Number.Integer whole || !whole.EInteger.CanFitInInt32()
                                           || (whole.EInteger.Sign > 0 || !besideARootOfAPolynomial) && APowerCanBeExact(whole.EInteger.ToInt32Unchecked())))
                            candidates.Add(node); // Power expressions x^n
                        // And the roots of that power, which need not occur anywhere to be the
                        // right substitution: `int x / (x^4 + 1)` wants u = x^2, and x^2 appears
                        // nowhere in it. A divisor of the exponent is the condition for the
                        // rewrite to be exact -- see InTermsOf -- so the candidates are exactly
                        // the divisors, and each is still tested by whether it clears the
                        // variable. https://github.com/asc-community/AngouriMath/issues/233
                        if (@base == x
                            && exp is Number.Integer power
                            && power.EInteger.CanFitInInt32()
                            && power.EInteger.ToInt32Checked() is var n
                            && n > 2)
                            for (var divisor = 2; divisor + divisor <= n; divisor++)
                                if (n % divisor == 0 && APowerCanBeExact(divisor))
                                    candidates.Add(MathS.Pow(x, divisor));
                        // Exponential with non-trivial argument
                        if (@base != x && @base.ContainsNode(x) && (!rational || polynomial)) candidates.Add(@base);
                        if (exp != x && exp.ContainsNode(x)) candidates.Add(exp);
                        break;
                    case Logf(_, var antilog):
                        candidates.Add(node); // Logarithm itself (for cases like 1/(x*ln(x)))
                        if (antilog != x && antilog.ContainsNode(x)) candidates.Add(antilog); // Also add the argument if it's not just x
                        break;
                    case Sumf(var aug, var add) when !rational && !large && node.Complexity <= LargestSumOffered:
                        if (aug.ContainsNode(x) || add.ContainsNode(x)) candidates.Add(node); // Linear expressions ax + b
                        break;
                }
            // Sort by complexity - try simpler substitutions first
            var ordered = candidates.OrderBy(c => c.Complexity).Distinct().ToList();
            foreach (var complement in complements)
                if (!ordered.Contains(complement))
                    ordered.Add(complement);
            return ordered;
        }
    }
}