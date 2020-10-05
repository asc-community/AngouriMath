using AngouriMath.Functions.Algebra;
using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace AngouriMath.Functions.Algebra
{
    internal static class IntegralPatterns
    {
        internal static Entity? TryStandardIntegrals(Entity expr, Entity.Variable x) => expr switch
        {
            Entity.Sinf(var arg) =>
                arg == x ? 
                    -MathS.Cos(arg) :
                arg is Entity.Mulf(var a, var y) && !a.Contains(x) && y == x ?
                    -MathS.Cos(arg) / a :
                null,

            Entity.Cosf(var arg) =>
                arg == x ?
                    MathS.Sin(arg) :
                arg is Entity.Mulf(var a, var y) && !a.Contains(x) && y == x ?
                    MathS.Sin(arg) / a :
                null,

            Entity.Tanf(var arg) =>
                arg == x || arg == -x ?
                    -MathS.Ln(MathS.Cos(arg)) :
                    null,

            Entity.Cotanf(var arg) =>
                arg == x || arg == -x ?
                    MathS.Ln(MathS.Sin(arg)) :
                    null,

            Entity.Logf(var @base, var arg) => 
                !@base.Contains(x) && (arg == x || arg == -x) ?
                    (arg * MathS.Ln(arg) - arg) / MathS.Ln(@base) :
                    null,

            Entity.Powf(var @base, var power) =>
                !@base.Contains(x) ?
                    MathS.Pow(@base, power) / MathS.Ln(@base) :
                    null,

            _ => null
        };
    }
}
