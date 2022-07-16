//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath;
using static AngouriMath.MathS;

var (x, y, z) = Var("x", "y", "z");
Entity expr = Sin(x);
var substituted = expr.Substitute(x, pi / 3);

Console.WriteLine(expr);
Console.WriteLine(substituted);
Console.WriteLine(substituted.Simplify());

Console.WriteLine("-------------------------------");

var expr2 = Sin(x) + Cos(y + x) + Factorial(z);

var substituted2 =
    expr2
        .Substitute(x, 1)
        .Substitute(y, 2)
        .Substitute(z, 3);

Console.WriteLine(expr2);
Console.WriteLine(substituted2);

Console.WriteLine("-------------------------------");

var expr3 = Sin(x + y) + 1 / Sin(x + y);
var substituted3 = expr3.Substitute(Sin(x + y), Cos(x + y));
Console.WriteLine(expr3);
Console.WriteLine(substituted3);