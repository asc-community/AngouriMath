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
        /// </remarks>
        internal static Entity? SolveByPartialFractions(Entity expr, Entity.Variable x, bool integrateByParts)
        {
            if (expr is not Entity.Divf(var numerator, var denominator))
                return null;

            if (Functions.PolynomialFactoring.TrySplitOffRationalRoot(
                    numerator, denominator, x, out var simple, out var restNumerator, out var restDenominator)
                && Integration.ComputeIndefiniteIntegral(simple, x, integrateByParts) is { } first
                && Integration.ComputeIndefiniteIntegral(restNumerator / restDenominator, x, integrateByParts) is { } rest)
                return first + rest;

            if (Functions.PartialFractions.TrySplitIntoCoprimeParts(
                    numerator, denominator, x, out var left, out var right)
                && Integration.ComputeIndefiniteIntegral(left, x, integrateByParts) is { } overOne
                && Integration.ComputeIndefiniteIntegral(right, x, integrateByParts) is { } overOther)
                return overOne + overOther;

            if (Functions.PartialFractions.TrySplitBiquadraticOverTheReals(
                    numerator, denominator, x, out var overOneReal, out var overOtherReal)
                && Integration.ComputeIndefiniteIntegral(overOneReal, x, integrateByParts) is { } realFirst
                && Integration.ComputeIndefiniteIntegral(overOtherReal, x, integrateByParts) is { } realRest)
                return realFirst + realRest;

            return null;
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