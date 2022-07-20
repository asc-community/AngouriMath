//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath.Core;
using static AngouriMath.MathS;

var expr = Sqrt(-1);
Console.WriteLine(expr);
Console.WriteLine(expr.Codomain);
Console.WriteLine(expr.Evaled);
Console.WriteLine("------------------------------------");
var newExpr = expr.WithCodomain(Domain.Real);
Console.WriteLine(newExpr);
Console.WriteLine(newExpr.Codomain);
Console.WriteLine(newExpr.Evaled);