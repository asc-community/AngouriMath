using System;
using AngouriMath;
using AngouriMath.Extensions;
using static AngouriMath.MathS;
using static AngouriMath.Entity;
using System.Numerics;

var expr = (decimal)"2 / 3".EvalNumerical();
Console.WriteLine(expr);