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

Entity expr = "cos(theta / 2) ^ 2 + (e^(-i phi) * e^(i phi)) sin(theta / 2)^2";
Console.WriteLine(expr.Latexise());
