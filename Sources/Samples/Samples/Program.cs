using System;
using AngouriMath;
using AngouriMath.Extensions;
using static AngouriMath.MathS;
using static AngouriMath.Entity;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

// Entity expr = "alpha_beta";
// Console.WriteLine(Unsafe.SizeOf<GCHandle>());

Entity expr = "a + b";
var handle = GCHandle.Alloc(expr);
Console.WriteLine(handle);
Console.WriteLine();
