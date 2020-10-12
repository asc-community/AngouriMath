/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

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
                !@base.ContainsNode(x) && TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out var b) ?
                    ((b / a + x) * MathS.Ln(arg) - x) / MathS.Ln(@base) :
                    null,

            Entity.Powf(var @base, var power) =>
                !@base.ContainsNode(x) && TreeAnalyzer.TryGetPolyLinear(power, x, out var a, out _) ?
                    MathS.Pow(@base, power) / (a * MathS.Ln(@base)) :
                    null,

            _ => null
        };
    }
}
