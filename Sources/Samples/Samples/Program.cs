using System;
using AngouriMath;
using AngouriMath.Extensions;
using static AngouriMath.MathS;
using static AngouriMath.Entity;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;


Console.WriteLine("Stack ptr: " + LastStackPointer());
MakeLambda()(12);

static unsafe Action<int> MakeLambda()
{
    Console.WriteLine("Stack ptr: " + LastStackPointer());
    var localVar = 0;
    Action<int> res = a =>
    {
        localVar = a;
        Console.WriteLine("Ptr: " + (nint)Unsafe.AsPointer(ref localVar));
        Console.WriteLine("New value: " + localVar);
    };
    res(6);
    return res;
}

static unsafe nint LastStackPointer()
{
    byte a = 0;
    return (nint)Unsafe.AsPointer(ref a);
}
