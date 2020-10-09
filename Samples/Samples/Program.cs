using System;
using AngouriMath;
using static System.Console;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;
using static AngouriMath.MathS;
using AngouriMath.Extensions;
using static AngouriMath.Entity.Number;

var a = new ConditionalSet("x", "x > 0 or x > 0");
WriteLine(a.Simplify());
