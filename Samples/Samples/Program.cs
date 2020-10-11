using System;
using AngouriMath;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Boolean;
using static AngouriMath.MathS;
using AngouriMath.Extensions;
using static System.Console;
using static AngouriMath.Entity.Set;

var expr = "x^ln(7) + 3".SolveEquation("x");
foreach(var sol in (FiniteSet)expr)
{
    WriteLine(sol.Simplify());
}

