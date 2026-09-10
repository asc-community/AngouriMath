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
            var splitted = TreeAnalyzer.GatherLinearChildrenOverSumAndExpand(expr, e => e.ContainsNode(x));
            if (splitted is null || splitted.Count < 2) return null; // nothing to do, let other solvers do the work
            return splitted.Select(e => Integration.ComputeIndefiniteIntegral(e, x, integrateByParts)).Aggregate((e1, e2) => (e1, e2) switch {
                (null, _) or (_, null) => null,
                (var int1, var int2) => int1 + int2
            });
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
            if (TreeAnalyzer.PolynomialLongDivision(numerator, denominator) is var (quotient, properPart)
                && quotient.Evaled != Entity.Number.Integer.Create(0)
                && Integration.ComputeIndefiniteIntegral(quotient, x, integrateByParts) is { } wholePart
                && Integration.ComputeIndefiniteIntegral(properPart, x, integrateByParts) is { } fractionPart)
                return wholePart + fractionPart;

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

            return null;
        }

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
        /// <b>Only a negative whole power.</b> A positive one is not a quotient; a fractional or
        /// symbolic exponent is not one either, and <c>(a/b)^(1/2)</c> is not <c>sqrt(a)/sqrt(b)</c>
        /// on the branch cut. Nothing here rewrites a quotient back into a power, so this cannot
        /// re-enter the mutual recursion <see cref="Integration.Normalized"/> records.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
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
                    over is Entity.Powf(var @base, var power) ?
                        Integration.ComputeIndefiniteIntegral(MathS.Pow(@base, -power), x, integrateByParts)?.Pipe(i => div * i) :
                        Integration.ComputeIndefiniteIntegral(MathS.Pow(over, -1), x, integrateByParts)?.Pipe(i => div * i) :
                !over.ContainsNode(x) ?
                    Integration.ComputeIndefiniteIntegral(div, x, integrateByParts)?.Pipe(i => i / over) :
                null,

            Entity.Powf(var @base, var power) =>
                !power.ContainsNode(x) && @base == x ?
                    power == -1 ?
                        IntegralPatterns.AntiderivativeLog(@base) :
                        MathS.Pow(x, power + 1) / (power + 1) :
                    null,

            Entity.Variable v =>
                v == x ? MathS.Pow(x, 2) / 2 : v * x,

            _ => null
        };

        internal static Entity? SolveIntegratingByParts(Entity expr, Entity.Variable x)
        {
            // Standard integration by parts for polynomial × function
            static Entity? IntegrateByPartsPolynomial(Entity polynomialToDifferentiate, Entity toIntegrate, Variable x, int currentRecursion = 0)
            {
                if (polynomialToDifferentiate == 0) return 0;
                if (currentRecursion == MathS.Settings.MaxExpansionTermCount) return null;

                var integral = Integration.ComputeIndefiniteIntegral(toIntegrate, x, false);
                if (integral is null) return null;
                var differential = polynomialToDifferentiate.Differentiate(x);
                var result = IntegrateByPartsPolynomial(differential, integral, x, currentRecursion + 1);
                return (result is null) ? null : polynomialToDifferentiate * integral - result;
            }

            // Generalized integration by parts: tries once with v and u both being integrable
            // ∫ v·u dx = v·∫u dx - ∫(v'·∫u dx) dx
            // Only attempts if both v and u can be integrated
            static Entity? TryIntegrateByPartsOnce(Entity v, Entity u, Variable x)
            {
                // Try to integrate u
                var integralOfU = Integration.ComputeIndefiniteIntegral(u, x, false);
                if (integralOfU is null) return null;

                // Differentiate v
                var derivativeOfV = v.Differentiate(x).InnerSimplified;
                if (derivativeOfV is Providedf(var inner, _)) derivativeOfV = inner; // TODO: signularities ignored but not handled properly
                if (derivativeOfV == Integer.Zero)
                    return v * integralOfU; // If v is constant, we're done

                // Try to integrate the remaining term: v' · ∫u dx
                var remaining = (derivativeOfV * integralOfU).Simplify(1);
                if (remaining is Providedf(var inner_, _)) remaining = inner_; // TODO: signularities ignored but not handled properly
                var remainingIntegral = Integration.ComputeIndefiniteIntegral(remaining, x, false);
                if (remainingIntegral is null) return null;

                return v * integralOfU - remainingIntegral;
            }

            if (expr is Entity.Mulf(var f, var g))
            {
                // Case 0: a logarithm times a polynomial. Only one of the two orders
                // terminates. Differentiating the polynomial, which is what Case 1 does,
                // leaves the logarithm to be integrated, and the antiderivative of ln(x)
                // holds an x*ln(x) that puts the original integral back in front of us --
                // that is how integral(x * ln(x), x) recursed until the stack ran out.
                // Differentiating the logarithm instead turns it into 1/x, which cancels
                // against the integrated polynomial and ends. (The L-before-A of LIATE.)
                if (f is Logf && MathS.TryPolynomial(g, x, out _)
                    && TryIntegrateByPartsOnce(f, g, x) is { } logFirstF) return logFirstF;
                if (g is Logf && MathS.TryPolynomial(f, x, out _)
                    && TryIntegrateByPartsOnce(g, f, x) is { } logFirstG) return logFirstG;

                // Case 1: One term is polynomial - use recursive polynomial integration by parts
                if (MathS.TryPolynomial(f, x, out var fPoly)) return IntegrateByPartsPolynomial(fPoly, g, x);
                if (MathS.TryPolynomial(g, x, out var gPoly)) return IntegrateByPartsPolynomial(gPoly, f, x);

                // Case 2: Neither is polynomial - try single-step integration by parts
                // This handles cases like ln(abs(x)) × ln(abs(x))
                // Try both orderings: f as v, g as u OR g as v, f as u
                if (TryIntegrateByPartsOnce(f, g, x) is { } result1) return result1;
                if (TryIntegrateByPartsOnce(g, f, x) is { } result2) return result2;
            }

            // Special case for powers of integrable functions, try integration by parts on base × base
            // e.g., ln(abs(x))^2 = ln(abs(x)) × ln(abs(x))
            if (expr is Powf(var @base, Integer(2)) && TryIntegrateByPartsOnce(@base, @base, x) is { } result) return result;

            return null;
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
        /// not a function of the tangent alone. <c>cotan</c> is <b>not</b> covered, since it is
        /// its own node rather than a reciprocal of this one, so <c>sqrt(cotan(x))</c> is still
        /// declined.
        /// </para>
        /// <para>
        /// <b>No condition is owed by the substitution</b>, but the answer inherits the
        /// tangent's: <c>u = tan(x)</c> is undefined exactly where the integrand is, since the
        /// integrand is a function of it, and <c>1 + u^2</c> is never zero for real <c>u</c>.
        /// </para>
        /// </remarks>
        internal static Entity? SolveByTangentSubstitution(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            var tangent = MathS.Tan(x);
            if (!expr.ContainsNode(tangent))
                return null;

            var uSub = Variable.CreateUnique(expr, "u_tan");
            var inU = expr.Substitute(tangent, uSub);
            if (inU.ContainsNode(x))
                return null;

            var integrand = (inU / (1 + MathS.Sqr(uSub))).InnerSimplified;
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
        internal static Entity? SolveByLinearRadicalSubstitution(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            // Every fractional power in the tree whose base is linear in x, collected with the
            // base it is over so that radicals over different bases are told apart.
            Entity? radicalBase = null;
            var denominators = new List<int>();
            foreach (var node in expr.Nodes)
            {
                if (node is not Powf(var @base, Number.Rational exponent) || exponent is Number.Integer)
                    continue;
                if (!@base.ContainsNode(x))
                    continue;
                if (!TreeAnalyzer.TryGetPolyLinear(@base, x, out var slope, out _) || slope.Evaled == 0)
                    return null;   // a radical over something that is not linear: not this rule's
                if (radicalBase is null)
                    radicalBase = @base;
                else if (radicalBase != @base)
                    return null;   // two different bases at once
                if (!exponent.ERational.Denominator.CanFitInInt32())
                    return null;
                denominators.Add(exponent.ERational.Denominator.ToInt32Checked());
            }
            if (radicalBase is null || denominators.Count == 0)
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

            return Integration.ComputeIndefiniteIntegral(integrand, u, integrateByParts) is { } result
                ? result.Substitute(u, MathS.Pow(radicalBase, Number.Rational.Create(1, q)))
                : null;
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
        internal static Entity? SolveByExponentialSubstitution(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            var slopes = new List<EInteger>();
            var offsets = new Dictionary<Entity, (EInteger Slope, Entity Offset)>();
            foreach (var node in expr.Nodes)
            {
                if (node is not Powf(var @base, var exponent) || @base != MathS.e)
                    continue;
                if (!exponent.ContainsNode(x))
                    continue;
                if (!TreeAnalyzer.TryGetPolyLinear(exponent, x, out var slope, out var offset))
                    return null;   // not linear in x, so not a power of one exponential
                if (slope.Evaled is not Number.Integer whole || whole.EInteger.IsZero)
                    return null;
                slopes.Add(whole.EInteger);
                offsets[node] = (whole.EInteger, offset);
            }
            if (slopes.Count == 0)
                return null;

            var k = slopes[0].Abs();
            foreach (var slope in slopes)
                k = k.Gcd(slope.Abs());
            if (k.IsZero)
                return null;

            var u = Variable.CreateUnique(expr, "u_exp");

            // e^(k_i x + m_i) is e^(m_i) times u^(k_i/k), and k_i/k is a whole number by
            // construction. Built directly rather than by substituting x and simplifying, for the
            // same reason as the radical substitutions: (u^(1/k))^(k_i) is not something the
            // simplifier will reduce, and is right not to.
            var rewritten = expr.Replace(node =>
                offsets.TryGetValue(node, out var found)
                    ? MathS.Pow(MathS.e, found.Offset)
                      * MathS.Pow(u, Number.Integer.Create(found.Slope / k))
                    : node);

            if (rewritten.ContainsNode(x))
                return null;

            // dx = du/(k u). Combined into one quotient and then simplified, in that order, and
            // the order decides four of these. Combine does not cancel, so 1/(e^x + e^(-x)) leaves
            // u/(u(u^2 + 1)) -- which the rational integrator declines although it answers
            // 1/(u^2 + 1) at once. Simplifying first instead leaves the nesting for Combine to
            // flatten and the common factor never meets a cancellation.
            var integrand = Functions.SingleQuotient.Combine(
                rewritten / (Number.Integer.Create(k) * u)).Simplify();
            if (integrand is Providedf(var inner, _))
                integrand = inner;

            if (Integration.ComputeIndefiniteIntegral(integrand, u, integrateByParts) is not { } result)
                return null;

            var answer = result.Substitute(u, MathS.Pow(MathS.e, Number.Integer.Create(k) * x));
            return answer.Nodes.Any(node => node == MathS.NaN) ? null : answer;
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
            foreach (var node in expr.Nodes) // Look for composite functions (functions of functions)
                switch (node)
                {
                    case TrigonometricFunction:
                        candidates.Add(node); // Trigonometric function itself (for cases like sin(x)*cos(x))
                        if (node.DirectChildren[0] != x && node.DirectChildren[0].ContainsNode(x))
                            candidates.Add(node.DirectChildren[0]); // Trigonometric functions with non-trivial arguments
                        break;
                    case Powf(var @base, var exp):
                        if (@base == x) candidates.Add(node); // Power expressions x^n
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
                                if (n % divisor == 0)
                                    candidates.Add(MathS.Pow(x, divisor));
                        // Exponential with non-trivial argument
                        if (@base != x && @base.ContainsNode(x)) candidates.Add(@base);
                        if (exp != x && exp.ContainsNode(x)) candidates.Add(exp);
                        break;
                    case Logf(_, var antilog):
                        candidates.Add(node); // Logarithm itself (for cases like 1/(x*ln(x)))
                        if (antilog != x && antilog.ContainsNode(x)) candidates.Add(antilog); // Also add the argument if it's not just x
                        break;
                    case Sumf(var aug, var add):
                        if (aug.ContainsNode(x) || add.ContainsNode(x)) candidates.Add(node); // Linear expressions ax + b
                        break;
                }
            // Sort by complexity - try simpler substitutions first
            return candidates.OrderBy(c => c.Complexity).Distinct();
        }
    }
}