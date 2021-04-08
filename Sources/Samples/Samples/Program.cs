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
var piecewise = (Piecewise)"piecewise(x provided a, y + x provided b, 3)";
var var = Var("d");
Console.WriteLine(piecewise.Integrate(var));
