using System;
using AngouriMath;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Boolean;
using static AngouriMath.MathS;
using AngouriMath.Extensions;
using static System.Console;
using static AngouriMath.Entity.Set;

var expr = "1 + 2^x + 4^x + 8^x - c".SolveEquation("x");
foreach(var sol in (FiniteSet)expr)
{
    WriteLine(sol.Simplify());
}

