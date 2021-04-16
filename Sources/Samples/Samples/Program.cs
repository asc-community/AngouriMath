using System;
using AngouriMath;
using AngouriMath.Extensions;
using static AngouriMath.MathS;
using static AngouriMath.Entity;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

Entity expr = "1 / a * -1 * a";
Console.WriteLine(expr);
Console.WriteLine(expr.Latexise());