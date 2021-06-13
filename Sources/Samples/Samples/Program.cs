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
Entity expr = "1 + 2 + sqrt(2) + x + y";
Func<double, double, double> someFunc = expr.Compile<double, double, double>("x", "y");
Console.WriteLine(someFunc(3, 5));

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