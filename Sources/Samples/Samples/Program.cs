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
Matrix m = "[[3 - lambda, -1, 0, -2, 0], [-3, -4 - lambda, -2, 1, 3], [0, -7, 1 - lambda, -5, 2], [3, 4, 1, 1 - lambda, -2], [-6, -19, -5, -3, 10 - lambda]]";
Console.WriteLine(m.Determinant.Simplify());
// using var _ = MathS.Settings.MaxExpansionTermCount.Set(50);
// Console.WriteLine("(a + b)100".Expand());