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

            Entity.Divf(var numerator, var denominator) when
                !numerator.ContainsNode(x) 
                && TreeAnalyzer.TryGetPolyQuadratic(denominator, x, out var a, out var b, out var c)
                && a is not Integer { IsZero: true } // Ensure it's actually quadratic (a != 0), not linear
                    => IntegrateRationalQuadratic(numerator, a, b, c, x),

            _ => null
        };

        private static Entity IntegrateRationalQuadratic(Entity numerator, Entity a, Entity b, Entity c, Entity.Variable x)
        {
            // ∫ k/(ax^2 + bx + c) dx
            // The formula depends on the discriminant: Δ = 4ac - b^2
            
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
            
            // Return as piecewise based on discriminant
            return MathS.Piecewise([
                new Entity.Providedf(arctanCase, discriminant > 0),
                new Entity.Providedf(perfectSquareCase, discriminant.Equalizes(0)),
                new Entity.Providedf(lnCase, discriminant < 0)
            ]).InnerSimplified;
        }
    }
}
