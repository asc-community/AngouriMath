//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using static AngouriMath.Entity;

namespace AngouriMath.Functions
{
    internal static partial class Patterns
    {
        internal static Entity TrigonometricRules(Entity x) => x switch
        {
            // sin({}) * cos({}) = 1/2 * sin(2{})
            Mulf(Sinf(var any1), Cosf(var any1a)) when any1 == any1a => Rational.Create(1, 2) * new Sinf(2 * any1),
            Mulf(Cosf(var any1), Sinf(var any1a)) when any1 == any1a => Rational.Create(1, 2) * new Sinf(2 * any1),

            // arc1({}) + arc2({}) = pi/2
            Sumf(Arcsinf(var any1), Arccosf(var any1a)) when any1 == any1a => MathS.pi / 2,
            Sumf(Arccosf(var any1), Arcsinf(var any1a)) when any1 == any1a => MathS.pi / 2,
            Sumf(Arctanf(var any1), Arccotanf(var any1a)) when any1 == any1a => MathS.pi / 2,
            Sumf(Arccotanf(var any1), Arctanf(var any1a)) when any1 == any1a => MathS.pi / 2,

            // tan * cot = 1
            Mulf(Tanf(var any1), Cotanf(var any1a)) when any1 == any1a => 1,
            Mulf(Cotanf(var any1), Tanf(var any1a)) when any1 == any1a => 1,

            // arcfunc(func(x)) = x
            Arcsinf(Sinf(var any1)) => any1,
            Arccosf(Cosf(var any1)) => any1,
            Arctanf(Tanf(var any1)) => any1,
            Arccotanf(Cotanf(var any1)) => any1,

            // func(arcfunc(x)) = x
            Sinf(Arcsinf(var any1)) => any1,
            Cosf(Arccosf(var any1)) => any1,
            Tanf(Arctanf(var any1)) => any1,
            Cotanf(Arccotanf(var any1)) => any1,

            // sin(:)^2 + cos(:)^2 = 1
            Sumf(Powf(Sinf(var any1), Integer(2)),
                 Powf(Cosf(var any1a), Integer(2))) when any1 == any1a => 1,
            Sumf(Powf(Cosf(var any1), Integer(2)),
                 Powf(Sinf(var any1a), Integer(2))) when any1 == any1a => 1,

            Minusf(Powf(Sinf(var any1), Integer(2)), Powf(Cosf(var any1a), Integer(2))) when any1 == any1a =>
                -1 * (new Powf(new Cosf(any1), 2) - new Powf(new Sinf(any1), 2)),
            Minusf(Powf(Cosf(var any1), Integer(2)), Powf(Sinf(var any1a), Integer(2))) when any1 == any1a =>
                new Cosf(2 * any1),

            Divf(var any1, Secantf(var any2)) => any1 * any2.Cos(),
            Divf(var any1, Cosecantf(var any2)) => any1 * any2.Sin(),

            Mulf(Secantf(var any1), Cosf(var any1a)) when any1 == any1a => 1,
            Mulf(Cosf(var any1a), Secantf(var any1)) when any1 == any1a => 1,

            Mulf(Cosecantf(var any1), Sinf(var any1a)) when any1 == any1a => 1,
            Mulf(Sinf(var any1a), Cosecantf(var any1)) when any1 == any1a => 1,
            
            // TODO: add more secant/cosecant patterns

            Arcsinf(Divf(var number, var notNumber)) when number is Number && notNumber is not Number => new Arccosecantf(notNumber / number),
            Arccosf(Divf(var number, var notNumber)) when number is Number && notNumber is not Number => new Arcsecantf(notNumber / number),
            Arccosecantf(Divf(var number, var notNumber)) when number is Number && notNumber is not Number => new Arcsinf(notNumber / number),
            Arcsecantf(Divf(var number, var notNumber)) when number is Number && notNumber is not Number => new Arccosf(notNumber / number),

            _ => x
        };
        internal static Entity ExpandTrigonometricRules(Entity x) => x switch
        {
            Mulf(Rational(Integer(1), Integer(2)), Sinf(Mulf(Integer(2), var any1))) => new Sinf(any1) * new Cosf(any1),

            Cosf(Mulf(Integer(2), var any1)) =>
                new Powf(new Cosf(any1), Integer.Create(2)) - new Powf(new Sinf(any1), 2),

            _ => x
        };

        internal static Entity CollapseTrigonometricFunctions(Entity x) => x switch
        {
            // sin / cos = tan
            Divf(Sinf(var any1), Cosf(var any1a)) when any1 == any1a => any1.Tan(),

            // cos / sin = cotan
            Divf(Cosf(var any1), Sinf(var any1a)) when any1 == any1a => any1.Cotan(),

            Divf(var any1, Sinf(var any2)) => any1 * any2.Cosec(),
            Divf(var any1, Cosf(var any2)) => any1 * any2.Sec(),
            _ => x
        };

        /// <summary>
        /// For this it is true that any trigonometric function is either sin or cos
        /// </summary>
        internal static Entity NormalTrigonometricForm(Entity x) => x switch
        {
            Tanf(var any1) => any1.Sin() / any1.Cos(),
            Cotanf(var any1) => any1.Cos() / any1.Sin(),
            Secantf(var any1) => 1 / any1.Cos(),
            Cosecantf(var any1) => 1 / any1.Sin(),
            _ => x
        };

        /// <summary>
        /// Here, we replace x with t which represents e^(ix).
        /// <list type="table">
        /// <item>sin(ax + b) = (t^a * e^(i*b) - t^(-a) * e^(-i*b)) / (2i)</item>
        /// <item>cos(ax + b) = (t^a * e^(i*b) + t^(-a) * e^(-i*b)) / 2</item>
        /// </list>
        /// </summary>
        internal static Func<Entity, Entity> TrigonometricToExponentialRules(Variable from, Variable to) => tree =>
        {
            // sin(ax + b) = (t^a * e^(i*b) - t^(-a) * e^(-i*b)) / (2i)
            Entity SinResult(Variable x, Entity a, Entity b) =>
                x == from
                ? MathS.Pow(to, a) * (MathS.Pow(MathS.e, b * MathS.i) / (2 * MathS.i)) - MathS.Pow(to, -a) * MathS.Pow(MathS.e, -b * MathS.i) / (2 * MathS.i)
                : tree;
            // cos(ax + b) = (t^a * e^(i*b) + t^(-a) * e^(-i*b)) / 2
            Entity CosResult(Variable x, Entity a, Entity b) =>
                x == from
                ? MathS.Pow(to, a) * (MathS.Pow(MathS.e, b * MathS.i) / 2) + MathS.Pow(to, -a) * MathS.Pow(MathS.e, -b * MathS.i) / 2
                : tree;
            // SolveLinear should also solve tan and cotan equations, but currently Polynomial solver cannot handle big powers
            // uncomment lines above when it will be fixed (TODO)
            // e.g. tan(ax + b) = -i + (2i)/(1 + e^(2i*b) t^(2a))
            return tree switch
            {
                Sinf(var arg) when TreeAnalyzer.TryGetPolyLinear(arg, from, out var a, out var b) =>
                    SinResult(from, a.InnerSimplified, b.InnerSimplified),

                Cosf(var arg) when TreeAnalyzer.TryGetPolyLinear(arg, from, out var a, out var b) =>
                    CosResult(from, a.InnerSimplified, b.InnerSimplified),

                _ => tree
            };
        };
    }
}
