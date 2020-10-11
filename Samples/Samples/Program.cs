using System;
using AngouriMath;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Boolean;
using static AngouriMath.MathS;
using AngouriMath.Extensions;
using static System.Console;

WriteLine("a = b = c = d = e".ToEntity());
Console.WriteLine("4^x + 2^x - (2^x)^3 - a".SolveEquation("x"));

