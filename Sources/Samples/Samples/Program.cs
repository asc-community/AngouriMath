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


// Console.WriteLine("[ [ 1, 2 ] ; [ 3, 4 ] ]".ToEntity());
var Fn = "[ 1, 0 ]T * ([ [ 1, 1 ], [ 1, 0 ] ] ^ n * [ 1, 0 ])".ToEntity();
Console.WriteLine(Fn.Substitute("n", 1_000_000).EvalNumerical().ToString().Length);
