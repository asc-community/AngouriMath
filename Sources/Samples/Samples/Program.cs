//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using static AngouriMath.MathS;

var (x, y) = Var("x", "y");
var expr1 = x + y;
Console.WriteLine(expr1.IsConstant);
Console.WriteLine(expr1.Evaled.IsConstant);
Console.WriteLine("-----------------------------");
var expr2 = 5 + x * i;
Console.WriteLine(expr2.IsConstant);
Console.WriteLine(expr2.Substitute(x, 3).IsConstant);
Console.WriteLine("-----------------------------");
var expr3 = GreaterThan(5, 3);
Console.WriteLine(expr3.IsConstant);
Console.WriteLine("-----------------------------");
var expr4 = pi + 0 * e;
Console.WriteLine(expr4.IsConstant);