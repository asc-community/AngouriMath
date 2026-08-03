//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

namespace AngouriMath.Functions.Algebra
{
    internal static class IntegralPatterns
    {
        internal static Entity? TryStandardIntegrals(Entity expr, Entity.Variable x) => expr switch
        {
            Entity.Sinf(var arg) when
                TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _)  =>
                    -MathS.Cos(arg) / a,

            Entity.Cosf(var arg) when
                TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _) =>
                    MathS.Sin(arg) / a,

            // By power reduction: sin(u)^2 = (1 - cos(2u)) / 2, so the integral is
            // x/2 - sin(2u)/(4a). Without this, integrating sin(x)^2 fell through to
            // integration by parts and cycled there.
            Entity.Powf(Entity.Sinf(var arg), Entity.Number.Integer(2)) when
                TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _) =>
                    x / 2 - MathS.Sin(2 * arg) / (4 * a),

            // cos(u)^2 = (1 + cos(2u)) / 2
            Entity.Powf(Entity.Cosf(var arg), Entity.Number.Integer(2)) when
                TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _) =>
                    x / 2 + MathS.Sin(2 * arg) / (4 * a),

            Entity.Secantf(var arg) when
                TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _) =>
                    MathS.Hyperbolic.Artanh(MathS.Sin(arg)) / a,

            Entity.Cosecantf(var arg) when
                TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _) =>
                    MathS.Ln(MathS.Abs(MathS.Tan(0.5 * arg))) / a,

            Entity.Tanf(var arg) when
                TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _) =>
                    -MathS.Ln(MathS.Abs(MathS.Cos(arg))) / a,

            Entity.Cotanf(var arg) when
               TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _) =>
                    MathS.Ln(MathS.Abs(MathS.Sin(arg))) / a,

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
                    (arg * MathS.Arctan(arg) - MathS.Ln(MathS.Abs(1 + arg * arg)) / 2) / a,

            Entity.Arccotanf(var arg) when
                TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _) =>
                    (arg * MathS.Arccotan(arg) + MathS.Ln(MathS.Abs(1 + arg * arg)) / 2) / a,

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

            // ∫ k / sqrt(ax^2 + bx + c) dx -- the arcsine and logarithm forms. Without
            // these, 1/sqrt(1 - x^2) had no antiderivative at all.
            Entity.Divf(var numerator,
                        Entity.Powf(var radicand, Entity.Number.Rational(Entity.Number.Integer(1), Entity.Number.Integer(2)))) when
                !numerator.ContainsNode(x)
                && TreeAnalyzer.TryGetPolyQuadratic(radicand, x, out var ra, out var rb, out var rc)
                    => IntegrateOverRootOfQuadratic(numerator, ra, rb, rc, radicand, x),

            _ => null
        };

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
                numerator * MathS.Ln(MathS.Abs(twoAxPlusB + 2 * MathS.Sqrt(a) * MathS.Sqrt(radicand))) / MathS.Sqrt(a);

            // a = 0: sqrt(bx + c), which integrates as an ordinary power
            var linearCase = 2 * numerator * MathS.Sqrt(b * x + c) / b;

            return MathS.Piecewise([
                new Entity.Providedf(linearCase, a.EqualTo(0)),
                new Entity.Providedf(arcsinCase, a < 0),
                new Entity.Providedf(logarithmCase, a > 0)
            ]);
        }

        private static Entity IntegrateRationalQuadratic(Entity numerator, Entity a, Entity b, Entity c, Entity.Variable x)
        {
            // The formula depends on whether it's linear (a = 0) or quadratic (a ≠ 0)
            // Case 0: a = 0 (linear, not quadratic)
            // ∫ k/(bx + c) dx = (k/b) * ln|bx + c|
            var linearCase = numerator * MathS.Ln(MathS.Abs(b * x + c)) / b;
            
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
            var lnCase = numerator * MathS.Ln(MathS.Abs((twoAxPlusB - sqrtNegDiscriminant) / (twoAxPlusB + sqrtNegDiscriminant))) / sqrtNegDiscriminant;
            
            // Return as piecewise based on a and discriminant
            return MathS.Piecewise([
                new Entity.Providedf(linearCase, a.EqualTo(0)),
                new Entity.Providedf(arctanCase, discriminant > 0),
                new Entity.Providedf(perfectSquareCase, discriminant.EqualTo(0)),
                new Entity.Providedf(lnCase, discriminant < 0)
            ]).InnerSimplified;
        }
    }
}
