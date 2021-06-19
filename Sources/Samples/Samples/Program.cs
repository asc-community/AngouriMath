using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using AngouriMath;
using AngouriMath.Extensions;
using AngouriMath.Functions;
using Antlr4.Runtime;
using static AngouriMath.MathS;

// Console.WriteLine("cbrt(7 + 21sqrt(-3)) + cbrt(7 - 21sqrt(-3))".ToEntity().Evaled);
// Console.WriteLine("cbrt(7) + cbrt(7)".ToEntity().Evaled);

var twoPiOver7 = MathS.Sqrt(1 - MathS.Pow("1/6" * (-1 + MathS.Cbrt((7 + 21 * Sqrt(-3)) / 2) + MathS.Cbrt((7 - 21 * Sqrt(-3)) / 2)), 2));

Console.WriteLine(twoPiOver7.Latexise());
Console.WriteLine(twoPiOver7.Evaled);
Console.WriteLine("2pi / 7".ToEntity().Sin().Evaled);

TrigonometricAngleExpansion.GetSineOfHalvedAngle("2/7", twoPiOver7);
// Console.WriteLine(TrigonometricAngleExpansion.GetSineOfHalvedAngle("2/7", twoPiOver7));

Console.ReadLine();
// Entity expr = "x + sin(2x) + 3";
// Func<double, double> f = expr.Compile<double, double>("x");
// Console.WriteLine(f(4));
/*
Console.Write(Unsafe.SizeOf<Entity.Number.Integer>());

static int PackedSizeOf(Type type)
{
    var res = 0;
    foreach (var field in type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
    {
        if (field.GetType().IsValueType)
            res += PackedSizeOf(field.GetType());
        else
            res += 8;
    }
    return type.BaseType is null ? res : res + PackedSizeOf(type.BaseType);
}
*/