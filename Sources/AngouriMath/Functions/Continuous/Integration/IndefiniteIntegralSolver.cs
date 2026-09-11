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
            static Entity? TryIntegrateByPartsOnce(Entity v, Entity u, Variable x, int wholeSize)
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
                var remainingIntegral = Integration.ComputeIndefiniteIntegral(
                    remaining, x, remaining.Nodes.Count() < wholeSize);
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
                    && TryIntegrateByPartsOnce(f, g, x, wholeSize) is { } logFirstF) return logFirstF;
                if (IsDifferentiatedBeforeAPolynomial(g) && MathS.TryPolynomial(f, x, out _)
                    && TryIntegrateByPartsOnce(g, f, x, wholeSize) is { } logFirstG) return logFirstG;

                // Case 1: One term is polynomial - use recursive polynomial integration by parts
                if (MathS.TryPolynomial(f, x, out var fPoly)) return IntegrateByPartsPolynomial(fPoly, g, x);
                if (MathS.TryPolynomial(g, x, out var gPoly)) return IntegrateByPartsPolynomial(gPoly, f, x);

                // Case 2: Neither is polynomial - try single-step integration by parts
                // This handles cases like ln(abs(x)) × ln(abs(x))
                // Try both orderings: f as v, g as u OR g as v, f as u
                if (TryIntegrateByPartsOnce(f, g, x, wholeSize) is { } result1) return result1;
                if (TryIntegrateByPartsOnce(g, f, x, wholeSize) is { } result2) return result2;
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

            // Special case for powers of integrable functions, try integration by parts on base × base
            // e.g., ln(abs(x))^2 = ln(abs(x)) × ln(abs(x))
            if (expr is Powf(var @base, Integer(2)) && TryIntegrateByPartsOnce(@base, @base, x, wholeSize) is { } result) return result;

            return null;
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
        internal static Entity? SolveByTrigonometricPowerSubstitution(Entity expr, Entity.Variable x)
        {
            if (!Integration.AnsweringTheQuestionAsked)
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
        /// A <b>binomial differential</b> <c>x^m (a + b x^n)^(p/q)</c>, in the case Chebyshev's
        /// criterion makes a polynomial: where <c>(m + 1)/n</c> is a whole number of at least one.
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
        /// which is a polynomial in <c>u</c> exactly when <c>s</c> is a whole number of at least
        /// one — the first of Chebyshev's three cases. Expanded by the binomial theorem and
        /// integrated term by term, so the rule is <b>closed</b> and asks the integrator nothing.
        /// </para>
        /// <para>
        /// <b>The other two cases are not here.</b> <c>p/q</c> whole is a whole power of a
        /// polynomial, which expanding already answers; and <c>s + p/q</c> whole wants
        /// <c>u = ((a + b x^n)/x^n)^(1/q)</c>, which leaves a polynomial over a power of
        /// <c>u^q - b</c> — a rational function rather than a polynomial, so a search rather than
        /// a closed answer. Chebyshev also proved there is no fourth case: outside those three the
        /// integrand has no elementary antiderivative at all, which is worth knowing before
        /// anyone goes looking.
        /// </para>
        /// <para>
        /// <b>Asked, not volunteered</b>:
        /// <a href="https://github.com/asc-community/AngouriMath/issues/1265">#1265</a>.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveABinomialDifferential(Entity expr, Entity.Variable x)
        {
            if (!Integration.AnsweringTheQuestionAsked)
                return null;
            if (!TryReadABinomialDifferential(expr, x, out var power, out var exponent,
                    out var inner, out var free, out var leading, out var factor))
                return null;

            // s = (m + 1)/n, whole and at least one, which is what makes the expansion finite.
            if ((power + 1) % inner != 0)
                return null;
            var s = (power + 1) / inner;
            if (s < 1)
                return null;

            var q = exponent.Denominator.ToInt32Checked();
            var p = exponent.Numerator.ToInt32Checked();
            var a = Number.Rational.Create(free);
            var b = Number.Rational.Create(leading);

            // (q/(n b^s)) * int (u^q - a)^(s-1) u^(p+q-1) du, the binomial expanded.
            Entity total = 0;
            var binomial = EInteger.One;
            var u = Variable.CreateUnique(expr, "u_binom");
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

            var outside = Number.Integer.Create(q)
                        / (Number.Integer.Create(inner) * MathS.Pow(b, Number.Integer.Create(s)));
            var bracket = (a + b * MathS.Pow(x, Number.Integer.Create(inner))).InnerSimplified;
            var back = MathS.Pow(bracket, Number.Rational.Create(EInteger.One, exponent.Denominator));
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
        /// <b>Two substitutions, chosen by the sign of the quadratic's leading coefficient.</b>
        /// With <c>r = sqrt(|a/b|)</c>:
        /// </para>
        /// <list type="bullet">
        /// <item><description>
        /// <c>b &gt; 0</c>: <c>x = r tan(t)</c>, under which <c>a + b x^2 = a sec(t)^2</c> and
        /// <c>dx = r sec(t)^2 dt</c>, leaving <c>sin(t)^m cos(t)^(-m-k-2)</c>.
        /// </description></item>
        /// <item><description>
        /// <c>b &lt; 0</c>: <c>x = r sin(t)</c>, under which <c>a + b x^2 = a cos(t)^2</c> and
        /// <c>dx = r cos(t) dt</c>, leaving <c>sin(t)^m cos(t)^(k+1)</c>.
        /// </description></item>
        /// </list>
        /// <para>
        /// Both need <c>a</c> and <c>b</c> of known sign with <c>a</c> positive, so that the
        /// radicand is the one the substitution assumes it is. <c>b x^2 - a</c> with both signs
        /// the other way is the secant substitution and a third branch; it is left out rather
        /// than guessed at, because its domain is two intervals rather than one and the answer
        /// owes a condition this does not yet write.
        /// </para>
        /// <para>
        /// <b>Coming back.</b> The answer arrives in <c>sin(t)</c>, <c>cos(t)</c> and
        /// <c>tan(t)</c>, each of which is algebraic in <c>x</c> under the substitution — no
        /// <c>arctan</c> appears, because the closed core never leaves a bare <c>t</c> behind.
        /// </para>
        /// <para>
        /// <b>Asked, not volunteered</b>, like the rule it hands to:
        /// <a href="https://github.com/asc-community/AngouriMath/issues/1265">#1265</a>.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static Entity? SolveARadicalOfAQuadraticAsTrigonometric(
            Entity expr, Entity.Variable x, bool aWholePowerCounts = false)
        {
            if (!Integration.AnsweringTheQuestionAsked)
                return null;
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
        /// <c>int y^m (A + c y^2)^(k/2) dy</c> for odd <c>k</c>, written back in terms of
        /// <paramref name="inTermsOf"/> -- which is the variable itself where the quadratic had no
        /// linear term, and the shifted variable where it did.
        /// </summary>
        private static Entity? IntegrateAPowerTimesARadicalQuadratic(
            Entity expr, Entity inTermsOf, int power, int half, ERational constant, ERational quadratic)
        {
            // Both coefficients decided, and the constant one positive: that is what makes the
            // radicand `A(1 + tan^2)` or `A(1 - sin^2)` rather than something whose sign the
            // substitution would have to guess at. `c y^2 - A`, the signs the other way round, is
            // the secant substitution -- a third branch whose domain is two intervals rather than
            // one, so the answer owes a condition this does not write, and it is declined.
            if (constant.Sign <= 0 || quadratic.IsZero)
                return null;

            var a = Number.Rational.Create(constant);
            var b = Number.Rational.Create(quadratic);
            var r = MathS.Sqrt(Number.Rational.Create(constant / quadratic.Abs()));

            var t = Variable.CreateUnique(expr, "t_trig");
            // b > 0: y = r tan(t) leaves sin^m cos^(-m-k-2); b < 0: y = r sin(t) leaves
            // sin^m cos^(k+1). The constant is r^(m+1) A^(k/2) either way.
            var sinePower = ERational.FromInt32(power);
            var cosinePower = ERational.FromInt32(
                quadratic.Sign > 0 ? -power - half - 2 : half + 1);
            var coefficient = MathS.Pow(r, power + 1) * MathS.Pow(a, Number.Rational.Create(half, 2));

            if (IntegrateAPowerOfSineTimesAPowerOfCosine(t, sinePower, cosinePower, 1, 1) is not { } inT)
                return null;

            // Back in terms of the variable. Each of the three is algebraic under the
            // substitution, so no inverse trigonometric function appears -- and the closed core
            // never leaves a bare `t` behind, which the check at the end confirms rather than
            // assumes.
            var radical = MathS.Sqrt((a + b * MathS.Sqr(inTermsOf)).InnerSimplified);
            var (sine, cosine, tangent) = quadratic.Sign > 0
                ? (inTermsOf * MathS.Sqrt(b) / radical,
                   MathS.Sqrt(a) / radical,
                   inTermsOf * MathS.Sqrt(b) / MathS.Sqrt(a))
                : (inTermsOf * MathS.Sqrt(-b) / MathS.Sqrt(a),
                   radical / MathS.Sqrt(a),
                   inTermsOf * MathS.Sqrt(-b) / radical);

            // `t` itself appears whenever the reduction bottoms out on `int 1 dt`, and that is
            // where an inverse trigonometric function enters an answer that is otherwise
            // algebraic -- `int sqrt(1 - x^2)/x^2 dx` holds an arcsine for exactly this reason.
            var angle = quadratic.Sign > 0
                ? MathS.Arctan(inTermsOf * MathS.Sqrt(b) / MathS.Sqrt(a))
                : MathS.Arcsin(inTermsOf * MathS.Sqrt(-b) / MathS.Sqrt(a));

            var answer = inT.Replace(node => node switch
            {
                Sinf(var inner) when inner == t => sine,
                Cosf(var inner) when inner == t => cosine,
                Tanf(var inner) when inner == t => tangent,
                Variable v when v == t => angle,
                _ => node
            });
            return answer.ContainsNode(t) ? null : (coefficient * answer).InnerSimplified;
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
                numerator * MathS.Pow(@base, -power), x, integrateByParts);
        }

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