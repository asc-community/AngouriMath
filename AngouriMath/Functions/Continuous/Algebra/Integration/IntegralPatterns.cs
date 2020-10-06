namespace AngouriMath.Functions.Algebra
{
    internal static class IntegralPatterns
    {
        internal static Entity? TryStandardIntegrals(Entity expr, Entity.Variable x) => expr switch
        {
            Entity.Sinf(var arg) =>
                TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _) ?
                    -MathS.Cos(arg) / a :
                    null,

            Entity.Cosf(var arg) =>
                TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _) ?
                    MathS.Sin(arg) / a :
                    null,

            Entity.Tanf(var arg) =>
                TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _) ?
                    -MathS.Ln(MathS.Cos(arg)) / a :
                    null,

            Entity.Cotanf(var arg) =>
               TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out _) ?
                    MathS.Ln(MathS.Sin(arg)) / a :
                    null,

            Entity.Logf(var @base, var arg) => 
                !@base.Contains(x) && TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out var b) ?
                    ((b / a + x) * MathS.Ln(arg) - x) / MathS.Ln(@base) :
                    null,

            Entity.Powf(var @base, var power) =>
                !@base.Contains(x) && TreeAnalyzer.TryGetPolyLinear(power, x, out var a, out _) ?
                    MathS.Pow(@base, power) / (a * MathS.Ln(@base)) :
                    null,

            _ => null
        };
    }
}
