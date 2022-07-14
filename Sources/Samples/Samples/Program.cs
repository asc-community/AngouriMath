//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;

using System;
using static AngouriMath.MathS;
var expr = Factorial(5);
Console.WriteLine(expr);
Console.WriteLine(expr.Evaled);
var expr2 = Gamma(4);
Console.WriteLine(expr2);
Console.WriteLine(expr2.Evaled);