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
                arg is Entity.Mulf(var a1, var y1) && !a1.Contains(x) && y1 == x ?
                    -MathS.Cos(arg) / a1 :
                arg is Entity.Mulf(var y2, var a2) && !a2.Contains(x) && y2 == x ?
                    -MathS.Cos(arg) / a2 :
                arg is Entity.Divf(var y3, var a3) && !a3.Contains(x) && y3 == x ?
                    -MathS.Cos(arg) * a3 :
                null,

            Entity.Cosf(var arg) =>
                arg == x ?
                    MathS.Sin(arg) :
                arg is Entity.Mulf(var a1, var y1) && !a1.Contains(x) && y1 == x ?
                    MathS.Sin(arg) / a1 :
                arg is Entity.Mulf(var y2, var a2) && !a2.Contains(x) && y2 == x ?
                    MathS.Sin(arg) / a2 :
                arg is Entity.Divf(var y3, var a3) && !a3.Contains(x) && y3 == x ?
                    MathS.Sin(arg) * a3 :
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
