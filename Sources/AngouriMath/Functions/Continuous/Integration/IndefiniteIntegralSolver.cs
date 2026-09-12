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
            if (expr is Sumf or Minusf)
            {
                // In the order the expanding gather uses -- the terms in the variable, then
                // what is free of it as one term -- so that the answer reads the same either way.
                // And only for terms of modest size: a partial-fraction term whose coefficient
                // is a page of unsimplified symbols took sixty seconds of every rule
                // simplifying the page for itself, where the expanding gather below tidies
                // its terms as it goes and takes half a second on the same sum.
                var inTheVariable = Sumf.LinearChildren(expr).Where(term => term.ContainsNode(x)).ToList();
                var free = Sumf.LinearChildren(expr).Where(term => !term.ContainsNode(x)).ToList();
                var asWritten = new List<Entity>(inTheVariable);
                if (free.Count > 0)
                    asWritten.Add(free.Aggregate((l, r) => l + r));
                if (asWritten.Count >= 2 && asWritten.All(term => term.Complexity <= LargestTermTakenAsWritten)
                    && Integrated(asWritten) is { } termByTerm)
                    return termByTerm;
            }
            var splitted = TreeAnalyzer.GatherLinearChildrenOverSumAndExpand(expr, e => e.ContainsNode(x));
            if (splitted is null || splitted.Count < 2) return null; // nothing to do, let other solvers do the work
            return Integrated(splitted);

            Entity? Integrated(List<Entity> terms)
                => terms.Select(e => Integration.ComputeAsAQuestionOfItsOwn(e, x, integrateByParts)).Aggregate((e1, e2) => (e1, e2) switch {
                    (null, _) or (_, null) => null,
                    (var int1, var int2) => int1 + int2
                });
        }

        /// <summary>The largest term a sum is split over as written, before the expanding gather.</summary>
        private const int LargestTermTakenAsWritten = 60;

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
            if (TreeAnalyzer.PolynomialLongDivision(numerator, denominator, genericCase: true, inTermsOf: x) is var (quotient, properPart)
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
            if (TryWriteInIrreducibleFactors(denominator, x) is { } refactored
                && (SolveByPartialFractions(numerator / refactored, x, integrateByParts)
                    ?? Integration.ComputeIndefiniteIntegral(numerator / refactored, x, integrateByParts)) is { } overIrreducibles)
                return overIrreducibles;

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
                && Integration.ComputeIndefiniteIntegral(overWrittenFactors, x, integrateByParts) is { } termByTerm)
                return termByTerm;

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
            Entity constantPart = Number.Integer.One;
            Entity variablePart = Number.Integer.One;
            foreach (var factor in Entity.Mulf.LinearChildren(denominator))
                if (factor.ContainsNode(x))
                    variablePart *= factor;
                else
                    constantPart *= factor;

            // Only where the denominator genuinely mixes the two. A denominator that is entirely
            // constant, or entirely in the variable, is one of the branches below's to answer,
            // and taking this one would hand on `N/1` and go round again.
            if (constantPart == Number.Integer.One || variablePart == Number.Integer.One)
                return null;

            return Integration.ComputeIndefiniteIntegral(numerator / variablePart, x, integrateByParts)
                ?.Pipe(i => i / constantPart);
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
            switch (expr)
            {
                case Entity.Divf(var dividend, var divisor):
                    (numerator, denominator) = (dividend, divisor);
                    return true;
                case Entity.Powf(var @base, Number.Integer power) when power.EInteger.Sign < 0:
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
                            case Entity.Powf(var @base, Number.Integer power) when power.EInteger.Sign < 0:
                                var positive = -power;
                                below *= positive == Number.Integer.One ? @base : MathS.Pow(@base, positive);
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
                var remaining = (bothOfThem.Differentiate(x) * integralOfTheRest).Simplify(1);
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
        /// The highest power of a factor <see cref="IsDifferentiatedBeforeAPolynomial"/>
        /// recognises among the factors of <paramref name="expr"/>, or zero where there is none:
        /// the first component of the measure integration by parts descends on.
        /// </summary>
        private static int HighestDifferentiatedPower(Entity expr)
        {
            var highest = 0;
            foreach (var (factor, underneath) in FactorsOfTheIntegrand(expr))
            {
                if (underneath || !IsDifferentiatedBeforeAPolynomial(factor))
                    continue;
                var power = factor is Powf(_, Number.Integer exponent) && exponent.EInteger.CanFitInInt32()
                    ? exponent.EInteger.ToInt32Unchecked()
                    : 1;
                if (power > highest)
                    highest = power;
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
        /// integrand as one quotient; <c>(null, null)</c> where there are not exactly two above
        /// the bar or nothing else beside them.
        /// </summary>
        private static (Entity? Both, Entity? Others) TryRegroupAroundBothDifferentiatedFactors(Entity expr)
        {
            var factors = FactorsOfTheIntegrand(expr);
            if (factors.Count < 3)
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
            if (both is null || (above is null && below is null))
                return (null, null);
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
                    null,

            _ => null
        };

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
            // a^2 + b^2, which is zero only when both are, and then the integrand is a polynomial.
            var scale = (MathS.Sqr(rate) + MathS.Sqr(frequency)).InnerSimplified;
            if (scale == 0)
                return null;

            var exponential = MathS.Pow(MathS.e, rate * x);
            var cosine = MathS.Cos(frequency * x);
            var sine = MathS.Sin(frequency * x);

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
        /// Exactly one inverse function, of the bare variable, and nothing left in <c>x</c>
        /// afterwards; otherwise declined. Handed to the integrator in <c>u</c>, where the
        /// trigonometric rules are.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveByInverseTrigonometricSubstitution(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            // The one inverse function of the bare variable.
            Entity? inverse = null;
            foreach (var node in expr.Nodes)
            {
                if (node is not (Arcsinf or Arccosf or Arctanf or Arcsecantf))
                    continue;
                if (node.DirectChildren.First() != x)
                    return null;
                if (inverse is not null && inverse != node)
                    return null;
                inverse = node;
            }
            if (inverse is null)
                return null;

            var u = Variable.CreateUnique(expr, "u_inv");
            // x in terms of u, dx/du, the quadratic whose root goes by construction and what it
            // becomes, and the way back.
            Entity xInU, dxdu, radicandBase, root;
            switch (inverse)
            {
                case Arcsinf:
                    (xInU, dxdu, radicandBase, root) = (MathS.Sin(u), MathS.Cos(u), 1 - MathS.Sqr(x), MathS.Cos(u));
                    break;
                case Arccosf:
                    (xInU, dxdu, radicandBase, root) = (MathS.Cos(u), -MathS.Sin(u), 1 - MathS.Sqr(x), MathS.Sin(u));
                    break;
                case Arctanf:
                    (xInU, dxdu, radicandBase, root) = (MathS.Tan(u), MathS.Sqr(MathS.Sec(u)), 1 + MathS.Sqr(x), MathS.Sec(u));
                    break;
                default:
                    (xInU, dxdu, radicandBase, root) = (MathS.Sec(u), MathS.Sec(u) * MathS.Tan(u), MathS.Sqr(x) - 1, MathS.Tan(u));
                    break;
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
                    if (!TryReadAHalfPower(node, out var @base, out var numerator)
                        || !IsTheSameQuadratic(@base, radicandBase, x))
                        return node;
                    radicalsRemoved++;
                    return MathS.Pow(root, numerator);
                });
            var exponentialOfTheInverse = expr.Nodes.Any(node =>
                node is Powf(var @base, var power) && !@base.ContainsNode(x) && power.ContainsNode(inverse));
            if (radicalsRemoved == 0 && !exponentialOfTheInverse)
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

            // The same question in another variable, not a step in the search for it: asked at
            // the top when this was, so the closed rules that answer only at the top --
            // `e^u sin(u)^3` is the exponential-times-trigonometric rule's -- are consulted.
            return Integration.ComputeAsAQuestionOfItsOwn(integrand, u, integrateByParts) is { } result
                ? result.Substitute(u, inverse)
                : null;
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
                    switch (piece)
                    {
                        case Sinf(var a):
                            thisDegree += power;
                            if (found is not null && found != a) return false;
                            found = a;
                            break;
                        case Cosf(var a):
                            thisDegree += power;
                            if (found is not null && found != a) return false;
                            found = a;
                            break;
                        default:
                            if (piece.ContainsNode(x))
                                return false;   // anything else in the term is not this shape
                            break;
                    }
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

            var tangent = MathS.Tan(x);
            if (!expr.ContainsNode(tangent))
                return null;

            var uSub = Variable.CreateUnique(expr, "u_tan");
            var inU = expr.Substitute(tangent, uSub);
            if (inU.ContainsNode(x))
                return null;

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
                if (!TreeAnalyzer.TryGetPolyLinear(@base, x, out var slope, out _) || slope.Evaled == 0)
                    continue;
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
            if (!TreeAnalyzer.TryGetPolyLinear(radicalBase, x, out var a, out var b))
                return null;

            var u = Variable.CreateUnique(expr, "u_rad");
            // x = (u^q - b) / a, and dx = (q/a) u^(q-1) du.
            var xInU = (MathS.Pow(u, q) - b) / a;
            var dx = Number.Integer.Create(q) / a * MathS.Pow(u, q - 1);

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
                    default:
                        return null;
                }
                if (TrigonometricArgument(node) is not { } argument || !argument.ContainsNode(x))
                    continue;
                if (!TreeAnalyzer.TryGetPolyLinear(argument, x, out var slope, out var offset)
                    || offset.Evaled is not Number.Integer { IsZero: true }
                    || slope.Evaled is not Number.Rational rational || rational.IsZero)
                    return null;
                slopes.Add(rational);
            }
            if (slopes.Count < 2)
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
            if (multiples.Count < 2)
                return null;
            if (ExpandedDegree(expr, x, common) > MaximumUnifiedDegree)
                return null;

            var theta = (Number.Rational.Create(common) * x).InnerSimplified;

            // Up before down. With only the argument and its double present, an integrand that
            // is even in the smaller one is a function of the doubled one alone, at half the
            // degree the other direction gives; that is tried first and the rest falls through.
            if (multiples.Count == 2 && multiples.Contains(1) && multiples.Contains(2)
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
                    || !TreeAnalyzer.TryGetPolyLinear(argument, x, out var slope, out _)
                    || slope.Evaled is not Number.Rational rational)
                    return node;
                var multiple = rational.ERational.Divide(common).ToLowestTerms();
                var n = multiple.Numerator.Abs().ToInt32Unchecked();
                var negative = multiple.Numerator.Sign < 0;
                var (sin, cos) = SineAndCosineOfAMultiple(n, sine, cosine);
                if (negative)
                    sin = -sin;
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
            return Integration.ComputeIndefiniteIntegral(cancelled, x, integrateByParts: false);
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
        private const int MaximumUnifiedDegree = 8;

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
            return exponent is null ? null : IntegrateByAnsatz(exponent, rest, x);
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
            var dPrime = d.Differentiate(x);
            var scale = squarefree is null ? below : squarefree * below;
            var columns = new List<Dictionary<EInteger, Entity>>();
            for (var k = 0; k <= degreeN; k++)
            {
                Entity xk = k == 0 ? Number.Integer.One : k == 1 ? x : MathS.Pow(x, k);
                Entity xkPrime = k == 0 ? Number.Integer.Zero : k == 1 ? Number.Integer.One : k * MathS.Pow(x, k - 1);
                Entity term = (xkPrime * d - xk * dPrime) * MathS.Sqr(q);
                if (h is not null)
                    term += (p.Differentiate(x) * q - p * q.Differentiate(x)) * xk * d;
                if (!TreeAnalyzer.TryGetPolynomial(term * scale, x, out var column))
                    return null;
                columns.Add(column);
            }
            for (var j = 0; j <= degreeM; j++)
            {
                Entity xj = j == 0 ? Number.Integer.One : j == 1 ? x : MathS.Pow(x, j);
                if (!TreeAnalyzer.TryGetPolynomial(xj * MathS.Sqr(d) * below, x, out var column))
                    return null;
                columns.Add(column);
            }
            var target = squarefree is null ? above * MathS.Sqr(d) * MathS.Sqr(q) : above * MathS.Sqr(d) * squarefree;
            if (!TreeAnalyzer.TryGetPolynomial(target, x, out var targetRead))
                return null;

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
            if (squarefree is null)
            {
                if (!Functions.PartialFractions.HoldsAtSampledPoints(rationalPart.Differentiate(x), integrand, x))
                    return null;
                return rationalPart.InnerSimplified;
            }

            var logarithmicPart = solvedM / squarefree;
            if (!Functions.PartialFractions.HoldsAtSampledPoints(rationalPart.Differentiate(x) + logarithmicPart, integrand, x))
                return null;
            if (solvedM == Number.Integer.Zero || solvedM.Evaled is Number.Complex { IsZero: true })
                return rationalPart.InnerSimplified;
            // The logarithmic part is a proper rational function over a squarefree denominator,
            // and the splits are what answer that; handing it to the whole integrator instead
            // sent one of them through every substitution and by-parts attempt there is, thirty
            // seconds to decline what the splits decline in a few milliseconds.
            return SolveByPartialFractions(logarithmicPart.InnerSimplified, x, integrateByParts: false) is { } rest
                ? (rationalPart + rest).InnerSimplified
                : null;
        }

        /// <summary>
        /// The largest degree, of the numerator, the denominator or the ansatz polynomial, that
        /// <see cref="SolveByExponentialAnsatz"/> takes on. The system is square in the degree,
        /// and past this the elimination is longer than any answer.
        /// </summary>
        private const int MaximumAnsatzDegree = 12;

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
            else return null;   // a < 0 and c < 0 with no root at zero: the radical is nowhere real

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

            var inT = SolveByPartialFractions(rational, t, integrateByParts: false)
                   ?? IntegralPatterns.TryStandardIntegrals(rational, t);
            if (inT is null)
                return null;
            var answer = inT.Substitute(t, backSubstitution).InnerSimplified;
            // Not answering is legitimate; answering NaN is not.
            return answer.Nodes.Any(n => n is Number.Complex { IsNaN: true }) ? null : answer;
        }

        /// <summary>
        /// A rational function of <c>x</c> and one square root of a <b>palindromic quartic</b>
        /// <c>a x^4 + b x^2 + a</c>, integrated by <c>u = x - 1/x</c> or <c>u = x + 1/x</c>: the
        /// quartic is <c>x^2</c> times a quadratic in <c>u</c>, and the rest becomes a rational
        /// function of <c>u</c> where it is one.
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
        /// <c>x^2 + 1/x^2 = u^2 - 2s</c>, so <c>Q = a x^4 + b x^2 + a = x^2 (a u^2 + b - 2 a s)</c>;
        /// and <c>dx = x^2 du/(x^2 - s)</c>. For <c>N/(D sqrt(Q))</c> the integrand is then
        /// <c>[N x/(D (x^2 - s))] du/sqrt(a u^2 + b - 2as)</c>, and for <c>N sqrt(Q)/D</c> it is
        /// <c>[N x^3/(D (x^2 - s))] sqrt(a u^2 + b - 2as) du</c>. The bracket is a rational
        /// function of <c>x</c>; the rule asks whether it is one of <c>u</c>, by undetermined
        /// coefficients on <c>P(u)/S(u)</c> and a check at sampled points, and declines where
        /// it is not -- which is the only way this can fail, and is exact.
        /// </para>
        /// <para>
        /// <b>The sign of <c>x</c>.</b> <c>sqrt(Q) = |x| sqrt(a u^2 + b - 2as)</c>, and the rule
        /// takes <c>|x| = x</c>: what comes out is an antiderivative for <c>x &gt; 0</c>. It is
        /// made one everywhere by parity, which is exact: an odd integrand has an even
        /// antiderivative, so <c>F(|x|)</c> serves on both sides, and an even one has an odd
        /// antiderivative, <c>sgn(x) F(|x|)</c>. An integrand of neither parity is declined
        /// rather than answered on half the line -- unless the caller knows its variable is
        /// positive, as the exponential substitution does of <c>u = e^x</c>, and says so.
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
            if (read.Keys.Any(k => !k.CanFitInInt32() || k.ToInt32Unchecked() is not (0 or 2 or 4)))
                return null;
            var a = Coefficient(4);
            var b = Coefficient(2);
            var c = Coefficient(0);
            if (a.Evaled is not Number.Real { IsZero: false } || (a - c).Evaled is not Number.Complex { IsZero: true }
                || b.Evaled is not Number.Real)
                return null;

            foreach (var sign in new[] { -1, 1 })
            {
                // R(x) = N x^k / (D (x^2 - s)), k = 1 with the root below, 3 above.
                var bracket = above * (rootBelow ? x : MathS.Pow(x, 3)) / (below * (MathS.Sqr(x) - sign));
                if (!TryWriteInTheReciprocalVariable(bracket, x, sign, out var u, out var inU))
                    continue;
                var quadratic = a * MathS.Sqr(u) + (b - 2 * sign * a);
                var integrand = (inU * MathS.Pow(quadratic, Number.Rational.Create(rootBelow ? -1 : 1, 2))).InnerSimplified;
                if (Integration.ComputeIndefiniteIntegral(integrand, u, integrateByParts: true) is not { } inTermsOfU)
                    continue;
                var forPositiveX = inTermsOfU.Substitute(u, x + Number.Integer.Create(sign) / x);
                if (forPositiveX.Nodes.Any(node => node == MathS.NaN))
                    continue;
                if (variableIsPositive)
                    return forPositiveX;

                // Extended to x < 0 by parity, or not at all.
                var reflected = expr.Substitute(x, -x);
                if (Functions.PartialFractions.HoldsAtSampledPoints(reflected, -expr, x))
                    return forPositiveX.Substitute(x, MathS.Abs(x));
                if (Functions.PartialFractions.HoldsAtSampledPoints(reflected, expr, x))
                    return MathS.Signum(x) * forPositiveX.Substitute(x, MathS.Abs(x));
                return null;
            }
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
                var solved = Functions.PartialFractions.TrySolveLinear(matrix, rhs, out var values);
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

            for (var i = 0; i < factors.Count; i++)
                for (var j = i + 1; j < factors.Count; j++)
                {
                    // Both arguments have to mention the variable. A sine of a constant is a
                    // number as far as this integral is concerned, and pairing it with a real
                    // factor would turn one term into two for nothing.
                    var replacement = (factors[i], factors[j]) switch
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

        internal static Entity? SolveByExponentialSubstitution(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            // Rational slopes, so that `e^(x/2)` beside `e^x` is read: the base is `e^(k x)` with
            // `k` the greatest common divisor of the slopes, and every exponential is a whole
            // power of it. `e^(x/2)/sqrt(e^x - 1)` is `2/sqrt(u^2 - 1)` that way, and was declined
            // for the half.
            var slopes = new List<ERational>();
            var offsets = new Dictionary<Entity, (ERational Slope, Entity Offset)>();
            var underARadical = new HashSet<Entity>();
            foreach (var node in expr.Nodes)
            {
                if (node is Powf(_, Number.Rational fractional) && fractional is not Number.Integer)
                    foreach (var inside in node.Nodes)
                        underARadical.Add(inside);
                if (node is not Powf(var @base, var exponent) || @base != MathS.e)
                    continue;
                if (!exponent.ContainsNode(x))
                    continue;
                if (!TreeAnalyzer.TryGetPolyLinear(exponent, x, out var slope, out var offset))
                    return null;   // not linear in x, so not a power of one exponential
                if (slope.Evaled is not Number.Rational rational || rational.ERational.IsZero)
                    return null;
                slopes.Add(rational.ERational);
                offsets[node] = (rational.ERational, offset);
            }
            if (slopes.Count == 0)
                return null;

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
            var rewritten = expr.Replace(node =>
                offsets.TryGetValue(node, out var found)
                    ? MathS.Pow(MathS.e, found.Offset)
                      * MathS.Pow(u, Number.Integer.Create(found.Slope.Divide(k).ToLowestTerms().Numerator))
                    : node);

            if (rewritten.ContainsNode(x))
                return null;

            // dx = du/(k u). Combined into one quotient and then simplified, in that order, and
            // the order decides four of these. Combine does not cancel, so 1/(e^x + e^(-x)) leaves
            // u/(u(u^2 + 1)) -- which the rational integrator declines although it answers
            // 1/(u^2 + 1) at once. Simplifying first instead leaves the nesting for Combine to
            // flatten and the common factor never meets a cancellation.
            var integrand = Functions.SingleQuotient.Combine(
                rewritten / (Number.Rational.Create(k) * u)).Simplify();
            if (integrand is Providedf(var inner, _))
                integrand = inner;
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
            // is a palindromic quartic under it here, and `u - 1/u` is `2 sinh(x)`.
            if (SolveByReciprocalSubstitution(integrand, u, variableIsPositive: true) is { } byTheReciprocal)
                return Finished(byTheReciprocal);
            if (Integration.ComputeIndefiniteIntegral(integrand, u, integrateByParts) is not { } result)
                return null;
            return Finished(result);

            Entity? Finished(Entity result)
            {
                var answer = result.Substitute(u, MathS.Pow(MathS.e, (Number.Rational.Create(k) * x).InnerSimplified));
                return answer.Nodes.Any(node => node == MathS.NaN) ? null : answer;
            }
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
        internal static Entity? SolveByCombiningRadicals(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            // Two square roots holding the variable, or there is nothing to combine; counted
            // before any of the rewriting below is paid for, since this runs on every
            // sub-integrand of the chain.
            // A root of a quotient counts as the two roots it splits into.
            if (expr.Nodes.Sum(node => node is Powf(var @base, Number.Rational half)
                    && half is not Number.Integer && half.ERational.Denominator.Equals(EInteger.FromInt32(2))
                    && @base.ContainsNode(x)
                        ? (@base is Divf || Functions.SingleQuotient.Of(@base).Denominator != Number.Integer.One ? 2 : 1)
                        : 0) < 2)
                return null;
            var rewritten = CombineRadicalsIn(expr, x);
            if (rewritten == expr)
                return null;
            return Integration.ComputeIndefiniteIntegral(rewritten, x, integrateByParts);
        }

        /// <summary>
        /// Every product or quotient in <paramref name="expr"/> with its square roots of
        /// polynomials combined into one, innermost first; and a small power of a sum holding
        /// such a root written out first, since <c>(sqrt(1 - x) + sqrt(1 + x))^2</c> is
        /// <c>2 + 2 sqrt(1 - x) sqrt(1 + x)</c>, which combines, and is not a shape any rule
        /// reads as it stands.
        /// </summary>
        private static Entity CombineRadicalsIn(Entity expr, Entity.Variable x)
            => SplitRootsOfQuotientsIn(expr, x).Replace(node => node is Mulf or Divf ? CombineRadicalsInAQuotient(node, x) ?? node : node);

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
                var (above, below) = Functions.SingleQuotient.Of(@base);
                if (below == Number.Integer.One || !below.ContainsNode(x))
                    return node;
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
        private static Entity? CombineRadicalsInAQuotient(Entity expr, Entity.Variable x)
        {
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
            if (bases.Count < 2 || !AtMostOneIsNegativeOnTheReals(bases, x))
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
        /// Whether <paramref name="holds"/> is true of the signs of the polynomials
        /// <paramref name="bases"/> on every interval between their real roots -- one point of
        /// each interval decides it, since no base changes sign inside one. <see langword="false"/>
        /// where the roots cannot be had.
        /// </summary>
        private static bool OnEveryRealInterval(List<Entity> bases, Entity.Variable x, System.Func<List<int>, bool> holds)
        {
            var roots = new List<double>();
            foreach (var @base in bases)
            {
                if (MathS.SolveEquation(@base, x) is not Set.FiniteSet solutions)
                    return false;
                foreach (var solution in solutions.Elements)
                {
                    if (solution.Evaled is not Number.Complex value || !value.IsFinite)
                        return false;
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
            foreach (var at in samples)
            {
                var signs = new List<int>();
                foreach (var @base in bases)
                {
                    if (@base.Substitute(x, at).Evaled is not Number.Real value || !value.IsFinite)
                        return false;
                    signs.Add(value < 0 ? -1 : value > 0 ? 1 : 0);
                }
                if (!holds(signs))
                    return false;
            }
            return true;
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
            return k == 0 ? MathS.Pow(rest.InnerSimplified, exponent) : MathS.Pow(u, k * p) * MathS.Pow(rest.InnerSimplified, exponent);
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
            var sine = MathS.Sin(x);
            var cosine = MathS.Cos(x);
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
            var integrand = Functions.SingleQuotient.Combine(inT * 2 / (1 + tSquared)).Simplify();

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
                ? result.Substitute(t, MathS.Tan(x / 2))
                : null;
        }

        /// <summary>
        /// Attempts to solve an integral using u-substitution.
        /// Looks for patterns where f(g(x)) * g'(x) can be integrated as F(g(x)).
        /// </summary>
        internal static Entity? SolveBySubstitution(Entity expr, Entity.Variable x, bool integrateByParts = true)
        {
            // Try to find a suitable substitution u = g(x)
            // We need to identify a composite function and check if du/dx appears in the integrand
            var candidates = FindSubstitutionCandidates(expr, x);
            foreach (var u in candidates)
            {
                var duDx = u.Differentiate(x).InnerSimplified;

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
                var integrandInU = InTermsOf(expr / duDx, u, uSub, x).Simplify(1);
                if (integrandInU is Providedf(var innerExpr, _)) integrandInU = innerExpr; // TODO: singularities ignored but not handled properly

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

                if (!integrandInU.ContainsNode(x) && Integration.ComputeIndefiniteIntegral(integrandInU, uSub, integrateByParts) is { } resultInU)
                    // Substitute back: replace u with g(x)
                    return resultInU.Substitute(uSub, u);
            }

            return null;
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

        /// <summary>The largest integrand the substitution rule offers its sums as candidates for.</summary>
        private const int LargestIntegrandOfferedSums = 120;

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
            if (!rational && expr.Nodes.Any(node => node is Powf(var radicalBase, Number.Rational radicalPower) && radicalPower is not Number.Integer
                    && radicalBase.Nodes.Any(inner => inner is Divf(_, var divisor) && (divisor == x || divisor is Powf(var pb, Number.Integer) && pb == x)))
                && !expr.Nodes.Any(node => node is Powf(var polynomialBase, Number.Rational rootPower) && rootPower is not Number.Integer
                    && polynomialBase.ContainsNode(x) && TreeAnalyzer.TryGetPolynomial(polynomialBase, x, out var radicand)
                    && radicand.Keys.Any(degree => degree.Sign > 0)))
                candidates.Add(MathS.Pow(x, -1));
            foreach (var node in expr.Nodes) // Look for composite functions (functions of functions)
                switch (node)
                {
                    case TrigonometricFunction:
                        candidates.Add(node); // Trigonometric function itself (for cases like sin(x)*cos(x))
                        if (node.DirectChildren[0] != x && node.DirectChildren[0].ContainsNode(x))
                            candidates.Add(node.DirectChildren[0]); // Trigonometric functions with non-trivial arguments
                        break;
                    case Powf(var @base, var exp):
                        if (@base == x && (exp is not Number.Integer whole || !whole.EInteger.CanFitInInt32() || APowerCanBeExact(whole.EInteger.ToInt32Unchecked())))
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
            return candidates.OrderBy(c => c.Complexity).Distinct();
        }
    }
}