using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using AngouriMath;
using Antlr4.Runtime;

// Console.Write(PackedSizeOf(typeof(Entity.Number.Integer)));
// int a = 3;
// ref int b = ref a;
// Console.Write(b.GetType().IsValueType);

//var a = Enumerable.Range(0, 1_000_000).Select(c => (Entity.Number.Integer) c).ToArray();

// Console.WriteLine(MathS.Compute.DefiniteIntegral("sin(x)", "x", (0, 0), MathS.DecimalConst.pi / 2));
Entity expr = "x + sin(2x) + 3";
Func<double, double> f = expr.Compile<double, double>("x");
Console.WriteLine(f(4));
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