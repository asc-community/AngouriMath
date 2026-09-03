//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using PeterO.Numbers;

namespace AngouriMath.Functions.Algebra
{
    internal static class IntegralPatterns
    {
        /// <summary>
        /// The logarithm an antiderivative of <c>f'/f</c> is written with: <c>ln(abs(f))</c> when
        /// the codomain is the reals, <c>ln(f)</c> when it is the complex plane.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>ln(abs(f))</c> is an antiderivative of <c>f'/f</c> only on the real line. <c>abs</c>
        /// is not holomorphic, so off the real line the textbook form is not an antiderivative of
        /// anything -- differentiating it does not return the integrand. That is
        /// https://github.com/asc-community/AngouriMath/issues/946, and separating by codomain is
        /// the answer given there.
        /// </para>
        /// <para>
        /// Under <see cref="AngouriMath.Core.Domain.Real"/> this returns exactly what the table
        /// returned before, so a caller who has said they are working on the reals sees no change.
        /// The default codomain is the complex plane, so the default answer does change --
        /// recorded in <c>BREAKING-CHANGES.md</c>.
        /// </para>
        /// <para>
        /// This is for an <c>abs</c> the rule <i>introduces</i>. Where the integrand already
        /// carries one -- the rule for <c>ln(abs(ax + b))</c> below -- the <c>abs</c> in the
        /// result is the caller's own and is left alone.
        /// </para>
        /// </remarks>
        internal static Entity AntiderivativeLog(Entity arg)
            => MathS.Settings.Codomain.Value is AngouriMath.Core.Domain.Real
                ? MathS.Ln(MathS.Abs(arg))
                : MathS.Ln(arg);

        /// <summary>
        /// The argument through which each rule below reads its integrand's dependence on
        /// <paramref name="x"/>, or <see langword="null"/> for a shape none of them matches.
        /// </summary>
        /// <remarks>
        /// The conditions on the two two-argument nodes are the ones their own rules carry:
        /// a base that mentions <c>x</c> is a different integral, not this one.
        /// </remarks>
        private static Entity? RateBearingArgument(Entity expr, Entity.Variable x) => expr switch
        {
            Entity.Sinf(var arg) => arg,
            Entity.Cosf(var arg) => arg,
            Entity.Secantf(var arg) => arg,
            Entity.Cosecantf(var arg) => arg,
            Entity.Tanf(var arg) => arg,
            Entity.Cotanf(var arg) => arg,
            Entity.Absf(var arg) => arg,
            Entity.Signumf(var arg) => arg,
            Entity.Arcsinf(var arg) => arg,
            Entity.Arccosf(var arg) => arg,
            Entity.Arctanf(var arg) => arg,
            Entity.Arccotanf(var arg) => arg,
            Entity.Logf(var @base, var arg) when !@base.ContainsNode(x) => arg,
            Entity.Powf(var @base, var power) when !@base.ContainsNode(x) => power,
            _ => null
        };

        internal static Entity? TryStandardIntegrals(Entity expr, Entity.Variable x) => expr switch
        {
            // Every rule below divides by the linear rate of its integrand's argument, and
            // an argument that mentions x without depending on it has a rate of zero. That
            // put a literal division by zero into the answer, so `e ^ (x + -x)` integrated
            // to `e ^ (x + -x) / (0 * ln(e))`, which evaluates to NaN. Each of those
            // integrands depends on x only through that argument, so a zero rate means the
            // integrand is a constant and integrates to itself times x.
            //
            // The rate has to be decidably zero: a symbolic one, as in sin(a * x), is not,
            // and answering sin(a * x) * x there would be wrong for every non-zero a.
            //
            // Reachable from ordinary input once two powers of one base are gathered:
            // e^x * e^(-x) becomes e^(x + -x), whose rate is zero while `x + -x` is still
            // written out. https://github.com/asc-community/AngouriMath/issues/785
            _ when RateBearingArgument(expr, x) is { } constant
                   && TreeAnalyzer.TryGetPolyLinear(constant, x, out var rate, out _)
                   && rate.Evaled is Entity.Number.Complex { IsZero: true }
                => expr * x,

            Entity.Sinf(var arg) when
                TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _)  =>
                    -MathS.Cos(arg) / a,

            Entity.Cosf(var arg) when
                TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _) =>
                    MathS.Sin(arg) / a,

            // sin(u)^n * cos(u)^m for whole n and m, which covers sin(x)^2 and cos(x)^2
            // as much as sin(x)^3 or sin(x)^2 * cos(x)^2.
            _ when TryReadSineCosinePowers(expr, x, out var trigArg, out var sinePower, out var cosinePower)
                   && sinePower + cosinePower >= 2
                   && TreeAnalyzer.TryGetPolyLinear(trigArg, x, out var trigRate, out _) =>
                       IntegrateSineCosinePowers(trigArg, sinePower, cosinePower, trigRate, x),

            Entity.Secantf(var arg) when
                TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _) =>
                    MathS.Hyperbolic.Artanh(MathS.Sin(arg)) / a,

            Entity.Cosecantf(var arg) when
                TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _) =>
                    AntiderivativeLog(MathS.Tan(0.5 * arg)) / a,

            Entity.Tanf(var arg) when
                TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _) =>
                    -AntiderivativeLog(MathS.Cos(arg)) / a,

            Entity.Cotanf(var arg) when
               TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _) =>
                    AntiderivativeLog(MathS.Sin(arg)) / a,

            Entity.Logf(var @base, var arg) when
                !@base.ContainsNode(x) && TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out var b) =>
                    ((b / a + x) * MathS.Ln(arg) - x) / MathS.Ln(@base),

            Entity.Powf(var @base, var power) when
                !@base.ContainsNode(x) && TreeAnalyzer.TryGetPolyLinear(power, x, out var a, out _) =>
                    MathS.Pow(@base, power) / (a * MathS.Ln(@base)),

            Entity.Absf(var arg) when
                TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _) => // ∫ |ax + b| dx = sgn(ax + b) * (ax + b)^2 / (2a)
                    MathS.Signum(arg) * MathS.Pow(arg, 2) / (2 * a),

            Entity.Signumf(var arg) when
                TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _) => // ∫ sgn(ax + b) dx = |ax + b| / a
                    MathS.Abs(arg) / a,

            // ∫ ln|ax + b| dx = ((ax + b)/a) * (ln|ax + b| - 1)
            Entity.Logf(var @base, Entity.Absf(var arg)) when
                @base == MathS.e && TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _) =>
                    (arg / a) * (MathS.Ln(MathS.Abs(arg)) - 1),

            Entity.Divf(var numerator, var denominator) when
                !numerator.ContainsNode(x)
                && TreeAnalyzer.TryGetPolyQuadratic(denominator, x, out var a, out var b, out var c) // ∫ k/(ax^2 + bx + c) dx
                    => IntegrateRationalQuadratic(numerator, a, b, c, x),

            // The inverse trigonometric functions, each of which is integration by parts
            // against 1 -- a shape the by-parts solver does not look for, since there is no
            // product to split.
            Entity.Arcsinf(var arg) when
                TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _) =>
                    (arg * MathS.Arcsin(arg) + MathS.Sqrt(1 - arg * arg)) / a,

            Entity.Arccosf(var arg) when
                TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _) =>
                    (arg * MathS.Arccos(arg) - MathS.Sqrt(1 - arg * arg)) / a,

            Entity.Arctanf(var arg) when
                TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _) =>
                    (arg * MathS.Arctan(arg) - AntiderivativeLog(1 + arg * arg) / 2) / a,

            Entity.Arccotanf(var arg) when
                TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _) =>
                    (arg * MathS.Arccotan(arg) + AntiderivativeLog(1 + arg * arg) / 2) / a,

            // ∫ B^(px + q) * sin(mx + n) dx and its cosine twin. Integrating by parts
            // twice returns the integral it started from, so the usual machinery cycles
            // rather than terminating; solving that equation for the integral once gives
            // the closed form below, which is what goes in the table.
            Entity.Mulf(var exponential, Entity.Sinf(var wave)) when
                IsExponentialRate(exponential, x, out var rate)
                && TreeAnalyzer.TryGetPolyLinear(wave, x, out var frequency, out _) =>
                    exponential * (rate * MathS.Sin(wave) - frequency * MathS.Cos(wave))
                        / (rate * rate + frequency * frequency),

            Entity.Mulf(Entity.Sinf(var wave), var exponential) when
                IsExponentialRate(exponential, x, out var rate)
                && TreeAnalyzer.TryGetPolyLinear(wave, x, out var frequency, out _) =>
                    exponential * (rate * MathS.Sin(wave) - frequency * MathS.Cos(wave))
                        / (rate * rate + frequency * frequency),

            Entity.Mulf(var exponential, Entity.Cosf(var wave)) when
                IsExponentialRate(exponential, x, out var rate)
                && TreeAnalyzer.TryGetPolyLinear(wave, x, out var frequency, out _) =>
                    exponential * (rate * MathS.Cos(wave) + frequency * MathS.Sin(wave))
                        / (rate * rate + frequency * frequency),

            Entity.Mulf(Entity.Cosf(var wave), var exponential) when
                IsExponentialRate(exponential, x, out var rate)
                && TreeAnalyzer.TryGetPolyLinear(wave, x, out var frequency, out _) =>
                    exponential * (rate * MathS.Cos(wave) + frequency * MathS.Sin(wave))
                        / (rate * rate + frequency * frequency),

            // ∫ sqrt(ax^2 + bx + c) dx, which is one integration by parts away from the
            // reciprocal form below and is written in terms of it.
            Entity.Powf(var radicand, Entity.Number.Rational(Entity.Number.Integer(1), Entity.Number.Integer(2))) when
                TreeAnalyzer.TryGetPolyQuadratic(radicand, x, out var qa, out var qb, out var qc)
                && qa.Evaled is Entity.Number.Complex { IsZero: false }
                    => IntegrateRootOfQuadratic(qa, qb, qc, radicand, x),

            // ∫ k / (x^2 * sqrt(ax^2 + c)) dx, the shape a trigonometric substitution is
            // usually taught for. Differentiating sqrt(ax^2 + c)/x gives exactly
            // -c/(x^2 * sqrt(ax^2 + c)), so every sign of a and c is the one formula and
            // there is no case analysis to get wrong. Without it 1/(x^2 * sqrt(x^2 - 1))
            // had no antiderivative at all.
            _ when TryReadOverSquareTimesRoot(expr, x, out var overFactor, out var overRadicand, out var overConstant)
                => -overFactor * MathS.Sqrt(overRadicand) / (overConstant * x),

            // ∫ k / sqrt(ax^2 + bx + c) dx -- the arcsine and logarithm forms. Without
            // these, 1/sqrt(1 - x^2) had no antiderivative at all.
            Entity.Divf(var numerator,
                        Entity.Powf(var radicand, Entity.Number.Rational(Entity.Number.Integer(1), Entity.Number.Integer(2)))) when
                !numerator.ContainsNode(x)
                && TreeAnalyzer.TryGetPolyQuadratic(radicand, x, out var ra, out var rb, out var rc)
                    => IntegrateOverRootOfQuadratic(numerator, ra, rb, rc, radicand, x),

            // ∫ (px + q)/(ax^2 + bx + c) dx. Only the constant numerator was covered, so
            // x/(x^2 + 2x + 5) and (x + 3)/(x^2 + 3x + 2) had no antiderivative at all.
            Entity.Divf(var numerator, var denominator) when
                numerator.ContainsNode(x)
                && TreeAnalyzer.TryGetPolyLinear(numerator, x, out var p, out var q)
                && TreeAnalyzer.TryGetPolyQuadratic(denominator, x, out var a, out var b, out var c)
                && a.Evaled is Entity.Number.Complex { IsZero: false }
                    => IntegrateLinearOverQuadratic(p, q, a, b, c, denominator, x),

            // ∫ N(x)/(ax^2 + bx + c)^n dx for a whole n of two or more and a proper fraction.
            // After the single-power arms above, which already read a denominator like
            // (x + 1)^2 -- expanded, it is a quadratic, so the perfect-square arm there answers
            // it and nothing about that changes.
            Entity.Divf(var numerator, Entity.Powf(var repeated, Entity.Number.Integer repetitions)) when
                repetitions.EInteger.CanFitInInt32()
                && repetitions.EInteger.ToInt32Checked() is var n and >= 2
                && TreeAnalyzer.TryGetPolyQuadratic(repeated, x, out var qa, out var qb, out var qc)
                && (!numerator.ContainsNode(x) || qa.Evaled is Entity.Number.Complex { IsZero: false })
                && IntegrateRationalOverPowerOfQuadratic(numerator, qa, qb, qc, repeated, n, x) is { } repeatedAnswer
                    => repeatedAnswer,

            // ∫ (px + q)/(bx + c) dx, the same rewrite one degree down: the quotient is the
            // constant p/b plus a remainder over the divisor. x/(x + 1) had no antiderivative.
            Entity.Divf(var numerator, var denominator) when
                numerator.ContainsNode(x)
                && TreeAnalyzer.TryGetPolyLinear(numerator, x, out var p, out var q)
                && TreeAnalyzer.TryGetPolyLinear(denominator, x, out var b, out var c)
                && b.Evaled is Entity.Number.Complex { IsZero: false }
                    => p * x / b + (q - p * c / b) * AntiderivativeLog(denominator) / b,

            // 1/cos(u)^2 and 1/sin(u)^2, which are written that way at least as often as
            // sec(u)^2 and csc(u)^2 and were not recognised in that form.
            Entity.Divf(var numerator, Entity.Powf(Entity.Cosf(var arg), Entity.Number.Integer(2))) when
                !numerator.ContainsNode(x) && TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _) =>
                    numerator * MathS.Tan(arg) / a,

            Entity.Divf(var numerator, Entity.Powf(Entity.Sinf(var arg), Entity.Number.Integer(2))) when
                !numerator.ContainsNode(x) && TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _) =>
                    -numerator * MathS.Cotan(arg) / a,

            Entity.Powf(Entity.Secantf(var arg), Entity.Number.Integer(2)) when
                TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _) =>
                    MathS.Tan(arg) / a,

            Entity.Powf(Entity.Cosecantf(var arg), Entity.Number.Integer(2)) when
                TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _) =>
                    -MathS.Cotan(arg) / a,

            _ => null
        };


        /// <summary>
        /// Reads an expression as <c>sin(arg)^n * cos(arg)^m</c>, where either power may be
        /// zero, or reports that it is not of that shape. Both factors have to be functions
        /// of the same argument.
        /// </summary>
        private static bool TryReadSineCosinePowers(
            Entity expr, Entity.Variable x, out Entity arg, out int sinePower, out int cosinePower)
        {
            arg = 0;
            sinePower = cosinePower = 0;
            switch (expr)
            {
                case Entity.Sinf(var a):
                    arg = a; sinePower = 1; return a.ContainsNode(x);
                case Entity.Cosf(var a):
                    arg = a; cosinePower = 1; return a.ContainsNode(x);
                case Entity.Powf(Entity.Sinf(var a), Entity.Number.Integer power)
                    when power.EInteger.Sign > 0 && power.EInteger.CanFitInInt32():
                    arg = a; sinePower = power.EInteger.ToInt32Checked(); return a.ContainsNode(x);
                case Entity.Powf(Entity.Cosf(var a), Entity.Number.Integer power)
                    when power.EInteger.Sign > 0 && power.EInteger.CanFitInInt32():
                    arg = a; cosinePower = power.EInteger.ToInt32Checked(); return a.ContainsNode(x);
                case Entity.Mulf(var left, var right)
                    when TryReadSineCosinePowers(left, x, out var leftArg, out var leftSine, out var leftCosine)
                         && TryReadSineCosinePowers(right, x, out var rightArg, out var rightSine, out var rightCosine)
                         && leftArg == rightArg:
                    arg = leftArg;
                    sinePower = leftSine + rightSine;
                    cosinePower = leftCosine + rightCosine;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// The antiderivative of <c>sin(u)^n * cos(u)^m</c> where <c>u</c> is linear in x
        /// with slope <paramref name="rate"/>.
        /// </summary>
        /// <remarks>
        /// With an odd power there is a substitution: peel one factor off to be the
        /// differential and write what is left in the other function, which turns the
        /// integral into a polynomial. With both powers even there is no such factor to
        /// peel, so the halved-angle identities go in instead and the result is integrated
        /// again -- the total power halves each time, so this ends.
        /// </remarks>
        private static Entity IntegrateSineCosinePowers(
            Entity arg, int sinePower, int cosinePower, Entity rate, Entity.Variable x)
        {
            if (sinePower == 0 && cosinePower == 0)
                return x;

            // cos^(2k+1) * sin^n: let s = sin(u), ds = cos(u) du
            //   = (1/rate) * sum_j C(k, j) (-1)^j s^(n + 2j + 1) / (n + 2j + 1)
            //
            // Tried before the sine substitution, so that where both powers are odd the
            // answer comes back in sin rather than cos: sin(x)cos(x) integrates to
            // sin(x)^2/2, which is the form everyone writes, rather than the equally
            // correct -cos(x)^2/2 that differs from it by a constant.
            if (cosinePower % 2 == 1)
            {
                var k = (cosinePower - 1) / 2;
                Entity sum = 0;
                for (var j = 0; j <= k; j++)
                {
                    var power = sinePower + 2 * j + 1;
                    sum += Binomial(k, j) * (j % 2 == 0 ? 1 : -1) * MathS.Sin(arg).Pow(power) / power;
                }
                return sum / rate;
            }

            // sin^(2k+1) * cos^m: let c = cos(u), dc = -sin(u) du
            if (sinePower % 2 == 1)
            {
                var k = (sinePower - 1) / 2;
                Entity sum = 0;
                for (var j = 0; j <= k; j++)
                {
                    var power = cosinePower + 2 * j + 1;
                    sum += Binomial(k, j) * (j % 2 == 0 ? 1 : -1) * MathS.Cos(arg).Pow(power) / power;
                }
                return -sum / rate;
            }

            // Both even: sin^2 = (1 - cos(2u))/2 and cos^2 = (1 + cos(2u))/2, expanded into
            // powers of cos(2u), each of which is integrated by the same rules.
            var p = sinePower / 2;
            var q = cosinePower / 2;
            var doubled = 2 * arg;
            Entity result = 0;
            // (1 - t)^p (1 + t)^q with t = cos(2u), over 2^(p+q)
            for (var i = 0; i <= p; i++)
                for (var j = 0; j <= q; j++)
                {
                    var coefficient = Binomial(p, i) * Binomial(q, j) * (i % 2 == 0 ? 1 : -1);
                    var term = IntegrateSineCosinePowers(doubled, 0, i + j, 2 * rate, x);
                    result += coefficient * term;
                }
            return result / Entity.Number.Integer.Create(EInteger.One.ShiftLeft(p + q));
        }

        private static Entity.Number.Integer Binomial(int n, int k)
        {
            var result = EInteger.One;
            for (var i = 0; i < k; i++)
                result = result * EInteger.FromInt32(n - i) / EInteger.FromInt32(i + 1);
            return Entity.Number.Integer.Create(result);
        }

        /// <summary>
        /// Whether <paramref name="expr"/> is an exponential in <paramref name="x"/>, and
        /// at what rate: <c>B^(px + q)</c> grows as <c>e^(rate * x)</c> with
        /// <c>rate = p * ln(B)</c>. The constant factor <c>B^q</c> needs no separating out,
        /// because the antiderivative is written in terms of the original expression.
        /// </summary>
        private static bool IsExponentialRate(Entity expr, Entity.Variable x, out Entity rate)
        {
            rate = 0;
            if (expr is not Entity.Powf(var @base, var exponent)
                || @base.ContainsNode(x)
                || !TreeAnalyzer.TryGetPolyLinear(exponent, x, out var perX, out _))
                return false;
            rate = perX * MathS.Ln(@base);
            return true;
        }

        /// <summary>
        /// The antiderivative of <c>sqrt(a x^2 + b x + c)</c>:
        /// <c>(2ax + b) sqrt(Q) / (4a) + ((4ac - b^2) / (8a))</c> times the integral of
        /// <c>1/sqrt(Q)</c> -- integration by parts once, leaving the reciprocal form that
        /// <see cref="IntegrateOverRootOfQuadratic"/> already knows.
        /// </summary>
        /// <remarks>
        /// Only where the leading coefficient is a number other than zero. With a = 0 this
        /// is the square root of something linear, which the ordinary power rule already
        /// integrates, and dividing by a would not be allowed anyway.
        /// </remarks>
        /// <summary>
        /// Reads <c>k / (x^2 * sqrt(ax^2 + c))</c>, giving back k, the radicand and its
        /// constant term. The radicand has to be a quadratic in x with no linear term and
        /// with neither of its two coefficients zero: a zero constant makes the formula
        /// below divide by it, and with a zero a there is no root of x left to speak of.
        /// </summary>
        private static bool TryReadOverSquareTimesRoot(
            Entity expr, Entity.Variable x,
            out Entity factor, out Entity radicand, out Entity constantTerm)
        {
            factor = radicand = constantTerm = 0;
            if (expr is not Entity.Divf(var numerator, var denominator) || numerator.ContainsNode(x))
                return false;
            Entity coefficient = numerator;
            var squares = 0;
            Entity? root = null, constant = null;
            foreach (var part in Entity.Mulf.LinearChildren(denominator))
                switch (part)
                {
                    case Entity.Powf(var square, Entity.Number.Integer(2)) when square == x:
                        squares++;
                        break;
                    case Entity.Powf(var under, Entity.Number.Rational(Entity.Number.Integer(1), Entity.Number.Integer(2)))
                        when root is null
                            && TreeAnalyzer.TryGetPolyQuadratic(under, x, out var qa, out var qb, out var qc)
                            && qa.Evaled is Entity.Number.Complex { IsZero: false }
                            && qb.Evaled is Entity.Number.Complex { IsZero: true }
                            && qc.Evaled is Entity.Number.Complex { IsZero: false }:
                        (root, constant) = (under, qc);
                        break;
                    case var other when !other.ContainsNode(x):
                        coefficient /= other;
                        break;
                    default:
                        return false;
                }
            if (squares != 1 || root is null || constant is null)
                return false;
            (factor, radicand, constantTerm) = (coefficient, root, constant);
            return true;
        }

        private static Entity IntegrateRootOfQuadratic(
            Entity a, Entity b, Entity c, Entity radicand, Entity.Variable x)
            => (2 * a * x + b) * MathS.Sqrt(radicand) / (4 * a)
               + (4 * a * c - b * b) / (8 * a) * IntegrateOverRootOfQuadratic(1, a, b, c, radicand, x);

        /// <summary>
        /// The antiderivative of <c>k / sqrt(a x^2 + b x + c)</c>, which takes one of two
        /// forms depending on the sign of the leading coefficient:
        /// <list type="bullet">
        /// <item>a &lt; 0, an arc of a circle: <c>-k/sqrt(-a) * arcsin((2ax + b) / sqrt(b^2 - 4ac))</c></item>
        /// <item>a &gt; 0, a hyperbolic arc: <c>k/sqrt(a) * ln|2ax + b + 2 sqrt(a) sqrt(a x^2 + b x + c)|</c></item>
        /// </list>
        /// Returned as a piecewise on that sign, the way the rational quadratic below is,
        /// since which one applies is not known until a and the coefficients are.
        /// </summary>
        private static Entity IntegrateOverRootOfQuadratic(
            Entity numerator, Entity a, Entity b, Entity c, Entity radicand, Entity.Variable x)
        {
            var twoAxPlusB = 2 * a * x + b;

            // a < 0: the radicand is a downward parabola, positive between its roots
            var arcsinCase =
                -numerator * MathS.Arcsin(twoAxPlusB / MathS.Sqrt(b * b - 4 * a * c)) / MathS.Sqrt(-a);

            // a > 0
            var logarithmCase =
                numerator * AntiderivativeLog(twoAxPlusB + 2 * MathS.Sqrt(a) * MathS.Sqrt(radicand)) / MathS.Sqrt(a);

            // a = 0: sqrt(bx + c), which integrates as an ordinary power, and needs b ≠ 0 for
            // the same reason the rational case below does. Where the radicand has no x term
            // the integrand is the constant k/sqrt(c) and its antiderivative is kx/sqrt(c);
            // the division by zero otherwise left here is what made k/sqrt(a x^2 + c) answer
            // NaN for every symbolic a, since only a numeric a lets this arm be dropped.
            var linearCase = TreeAnalyzer.IsZero(b)
                ? numerator * x / MathS.Sqrt(c)
                : 2 * numerator * MathS.Sqrt(b * x + c) / b;

            return MathS.Piecewise([
                new Entity.Providedf(linearCase, a.EqualTo(0)),
                new Entity.Providedf(arcsinCase, a < 0),
                new Entity.Providedf(logarithmCase, a > 0)
            ]);
        }

        /// <summary>
        /// ∫ (px + q)/(ax^2 + bx + c) dx, by writing the numerator as a multiple of the
        /// denominator's derivative plus a constant:
        /// px + q = (p/2a)(2ax + b) + (q - pb/2a). The first part integrates to a logarithm
        /// and the second is the constant-numerator case above.
        /// </summary>
        private static Entity IntegrateLinearOverQuadratic(
            Entity p, Entity q, Entity a, Entity b, Entity c, Entity denominator, Entity.Variable x)
            => p / (2 * a) * AntiderivativeLog(denominator)
               + IntegrateRationalQuadratic(q - p * b / (2 * a), a, b, c, x);

        private static Entity IntegrateRationalQuadratic(Entity numerator, Entity a, Entity b, Entity c, Entity.Variable x)
        {
            // The formula depends on whether it's linear (a = 0) or quadratic (a ≠ 0)
            // Case 0: a = 0 (linear, not quadratic)
            // ∫ k/(bx + c) dx = (k/b) * ln|bx + c|, which needs b ≠ 0. Where the denominator
            // has no x term the integrand is the constant k/c and its antiderivative is kx/c;
            // writing the logarithm there divides by zero, and a Providedf carrying NaN takes
            // the whole piecewise with it. That is only ever reached when a is symbolic, since
            // a numeric a makes this arm decidably unreachable and it is dropped before the
            // NaN can propagate -- so k/(a x^2 + c) answered NaN while k/(2 x^2 + c) did not.
            var linearCase = TreeAnalyzer.IsZero(b)
                ? numerator * x / c
                : numerator * AntiderivativeLog(b * x + c) / b;
            
            // For true quadratics (a ≠ 0), discriminant Δ = 4ac - b^2 determines the form
            var discriminant = 4 * a * c - b * b;
            
            // Case 1: Δ > 0 (no real roots, use arctan)
            // Result: (2k/√Δ) * arctan((2ax + b)/√Δ)
            var sqrtDiscriminant = MathS.Sqrt(discriminant);
            var twoAxPlusB = 2 * a * x + b;
            var arctanCase = 2 * numerator * MathS.Arctan(twoAxPlusB / sqrtDiscriminant) / sqrtDiscriminant;
            
            // Case 2: Δ = 0 (perfect square, one repeated root)
            // ax^2 + bx + c = a(x + b/(2a))^2
            // Result: -2k/(2ax + b)
            var perfectSquareCase = -2 * numerator / twoAxPlusB;
            
            // Case 3: Δ < 0 (two distinct real roots, use logarithm)
            // Result: (k/√(-Δ)) * ln|(2ax + b - √(-Δ))/(2ax + b + √(-Δ))|
            var sqrtNegDiscriminant = MathS.Sqrt(-discriminant);
            var lnCase = numerator * AntiderivativeLog((twoAxPlusB - sqrtNegDiscriminant) / (twoAxPlusB + sqrtNegDiscriminant)) / sqrtNegDiscriminant;
            
            // Return as piecewise based on a and discriminant
            return MathS.Piecewise([
                new Entity.Providedf(linearCase, a.EqualTo(0)),
                new Entity.Providedf(arctanCase, discriminant > 0),
                new Entity.Providedf(perfectSquareCase, discriminant.EqualTo(0)),
                new Entity.Providedf(lnCase, discriminant < 0)
            ]).InnerSimplified;
        }

        /// <summary>
        /// ∫ k / (a x^2 + b x + c)^n dx for a whole n of two or more, by the reduction that
        /// takes one power off the denominator at a time.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Write <c>Q</c> for the quadratic, <c>u</c> for its derivative <c>2ax + b</c> and
        /// <c>D</c> for <c>4ac - b^2</c>. Then <c>u^2</c> is <c>4aQ - D</c>, so differentiating
        /// <c>u / Q^(m-1)</c> gives <c>2a(3 - 2m)/Q^(m-1) + (m-1)D/Q^m</c>, and reading that as
        /// an equation for the integral of <c>1/Q^m</c> leaves
        /// <code>
        /// J_m = (u / Q^(m-1) + 2a(2m - 3) J_(m-1)) / ((m - 1) D)
        /// </code>
        /// which is unrolled down to <c>J_1</c>, the case the table above already answers. It is
        /// an identity in a, b and c rather than a fact about signs, so one line serves a
        /// positive and a negative discriminant alike; <c>D = 0</c> is the only exclusion, and
        /// the division by <c>D</c> is why.
        /// </para>
        /// <para>
        /// The two shapes the reduction cannot speak for get their own arms, and each is
        /// elementary: with <c>a = 0</c> the denominator is a linear power, and with
        /// <c>D = 0</c> the quadratic is <c>u^2/(4a)</c> so the whole integrand is a power of
        /// <c>u</c>. Both are already answered for a numeric quadratic — by the power rule and
        /// by partial fractions — and the arms are here for a symbolic one, where the
        /// discriminant's sign is not known and dropping an arm is not available.
        /// </para>
        /// <para>
        /// What was missing was the irreducible case. <c>1/(x^2 - 1)^2</c> had an antiderivative
        /// and <c>1/(x^2 + 2x + 1)^2</c> had one, because a denominator with real roots comes
        /// apart into linear factors and never reaches here; <c>1/(x^2 + 1)^2</c> had none.
        /// https://github.com/asc-community/AngouriMath/issues/180
        /// </para>
        /// </remarks>
        private static Entity IntegrateOverPowerOfQuadratic(
            Entity numerator, Entity a, Entity b, Entity c, int power, Entity.Variable x)
        {
            var quadratic = a * x * x + b * x + c;
            var derivative = 2 * a * x + b;
            var discriminant = 4 * a * c - b * b;

            // a = 0: k/(bx + c)^n, an ordinary power -- and the constant k/c^n when b is zero
            // too, where writing the power would divide by it. Same reason as the rule above.
            var linearCase = TreeAnalyzer.IsZero(b)
                ? numerator * x / MathS.Pow(c, power)
                : numerator * MathS.Pow(b * x + c, 1 - power) / (b * (1 - power));

            // D = 0: the quadratic is u^2/(4a), so the integrand is k(4a)^n / u^(2n).
            var perfectSquareCase =
                numerator * MathS.Pow(4 * a, power) * MathS.Pow(derivative, 1 - 2 * power)
                    / (2 * a * (1 - 2 * power));

            var sqrtDiscriminant = MathS.Sqrt(discriminant);
            var sqrtNegDiscriminant = MathS.Sqrt(-discriminant);

            return MathS.Piecewise([
                new Entity.Providedf(linearCase, a.EqualTo(0)),
                new Entity.Providedf(perfectSquareCase, discriminant.EqualTo(0)),
                new Entity.Providedf(
                    Reduce(2 * MathS.Arctan(derivative / sqrtDiscriminant) / sqrtDiscriminant),
                    discriminant > 0),
                new Entity.Providedf(
                    Reduce(AntiderivativeLog((derivative - sqrtNegDiscriminant) / (derivative + sqrtNegDiscriminant))
                            / sqrtNegDiscriminant),
                    discriminant < 0)
            ]).InnerSimplified;

            Entity Reduce(Entity firstPower)
            {
                var accumulated = firstPower;
                for (var m = 2; m <= power; m++)
                    accumulated =
                        (derivative / MathS.Pow(quadratic, m - 1) + 2 * a * (2 * m - 3) * accumulated)
                            / ((m - 1) * discriminant);
                return numerator * accumulated;
            }
        }

        /// <summary>
        /// ∫ (px + q)/(a x^2 + b x + c)^n dx, by the same rewrite of the numerator the single
        /// power uses: <c>px + q = (p/2a)(2ax + b) + (q - pb/2a)</c>. The first part is the
        /// denominator's own derivative over a power of it, which integrates as a power; the
        /// second is the constant-numerator case above.
        /// </summary>
        /// <remarks>
        /// A numerator that is already a multiple of that derivative leaves nothing of the
        /// second part, and it is dropped rather than multiplied by zero. Multiplying is not
        /// harmless: the reduction is a quotient by the quadratic, so <c>0 * (2x/(x^2 + 1))</c>
        /// is <c>0 provided not 1 + x^2 = 0</c> and not <c>0</c> — correctly, since the factor
        /// has no value there. Carrying that condition would attach it to the answer for
        /// <c>x/(x^2 + 1)^2</c>, which had none before and is owed none.
        /// </remarks>
        private static Entity IntegrateLinearOverPowerOfQuadratic(
            Entity p, Entity q, Entity a, Entity b, Entity c, Entity quadratic, int power, Entity.Variable x)
        {
            var alongTheDerivative = p / (2 * a) * MathS.Pow(quadratic, 1 - power) / (1 - power);
            var constantPart = q - p * b / (2 * a);
            return TreeAnalyzer.IsZero(constantPart)
                ? alongTheDerivative
                : alongTheDerivative + IntegrateOverPowerOfQuadratic(constantPart, a, b, c, power, x);
        }

        /// <summary>
        /// ∫ N(x)/(a x^2 + b x + c)^n dx for a numerator of any degree, or <see langword="null"/>
        /// where the fraction is improper and this is not the rule for it.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Dividing the numerator by the quadratic writes <c>N = qQ + r</c> with <c>r</c> linear
        /// at worst, so <c>N/Q^n</c> is <c>q/Q^(n-1) + r/Q^n</c> — one power off the denominator
        /// and two degrees off the numerator at each step, which ends. The remainder is answered
        /// by the two helpers above and the quotient goes round again.
        /// </para>
        /// <para>
        /// <b>Improper fractions are declined rather than divided out here.</b> The recursion
        /// bottoms out at a single power, and a numerator still of degree two or more there is
        /// exactly the improper case, which
        /// <see cref="IndefiniteIntegralSolver.SolveByPartialFractions"/> already opens with
        /// <see cref="TreeAnalyzer.PolynomialLongDivision"/>. Declining hands it there instead of
        /// answering it twice, in two shapes, from two places.
        /// </para>
        /// <para>
        /// This is not only reach. <c>TryStandardIntegrals</c> runs before every search, so a
        /// shape answered here never reaches the candidate exploration that re-integrates
        /// rewritten forms — which is where this family's cost has always been. Both
        /// <c>x^2/(x^2 + 2)^2</c> and <c>(x^2 + 1)/(x^2 + 2)^2</c> were tens of seconds spent to
        /// return the integral unevaluated.
        /// </para>
        /// </remarks>
        private static Entity? IntegrateRationalOverPowerOfQuadratic(
            Entity numerator, Entity a, Entity b, Entity c, Entity quadratic, int power, Entity.Variable x)
        {
            if (!numerator.ContainsNode(x))
                return power == 1
                    ? IntegrateRationalQuadratic(numerator, a, b, c, x)
                    : IntegrateOverPowerOfQuadratic(numerator, a, b, c, power, x);

            if (TreeAnalyzer.TryGetPolyLinear(numerator, x, out var p, out var q))
                return power == 1
                    ? IntegrateLinearOverQuadratic(p, q, a, b, c, quadratic, x)
                    : IntegrateLinearOverPowerOfQuadratic(p, q, a, b, c, quadratic, power, x);

            if (power < 2)
                return null;

            var division = TreeAnalyzer.PolynomialLongDivision(numerator, quadratic);
            if (division is null)
                return null;

            // The quotient is what is wanted; the remainder that comes back with it is already
            // divided by the divisor -- x^2 over x^2 + 2 answers (1, -2/(x^2 + 2)) -- so taking
            // it as a polynomial and dividing it again is a numerator that is not one, and the
            // next division declines. Subtracting gives the polynomial remainder whatever shape
            // the division chose to report.
            // Reduced before either is used: the division reports its quotient built up term by
            // term, so x^2 over x^2 + 2 comes back as `0 + 1` rather than `1`, and the arms below
            // ask whether a numerator is constant.
            var quotient = division.Value.Divided.InnerSimplified;
            var remainder = (numerator - quotient * quadratic).InnerSimplified;

            return IntegrateRationalOverPowerOfQuadratic(remainder, a, b, c, quadratic, power, x) is { } head
                   && IntegrateRationalOverPowerOfQuadratic(quotient, a, b, c, quadratic, power - 1, x) is { } rest
                ? head + rest
                : null;
        }
    }
}
