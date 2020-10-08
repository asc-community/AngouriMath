using System;
using AngouriMath;
using static System.Console;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;
using static AngouriMath.MathS;
using AngouriMath.Extensions;
using static AngouriMath.Entity.Number;

var a = new Interval(3, true, 5, true);
var b = new Interval(4, true, 6, false);
var un = a.Unite(b);
Console.WriteLine(un.InnerSimplify());
