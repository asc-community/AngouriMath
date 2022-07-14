//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using System;
using static AngouriMath.MathS;

var (x, y) = Var("x", "y");

Console.WriteLine(GreaterThan(x, y));
Console.WriteLine(GreaterThan(6, 5));
Console.WriteLine(GreaterThan(6, 5).EvalBoolean());
Console.WriteLine(GreaterThan(6, 6).EvalBoolean());
Console.WriteLine("----------------------------------");
Console.WriteLine(LessThan(x, y));
Console.WriteLine(LessThan(6, 5));
Console.WriteLine(LessThan(6, 5).EvalBoolean());
Console.WriteLine(LessThan(6, 6).EvalBoolean());
Console.WriteLine("----------------------------------");
Console.WriteLine(GreaterOrEqualThan(x, y));
Console.WriteLine(GreaterOrEqualThan(6, 5));
Console.WriteLine(GreaterOrEqualThan(6, 5).EvalBoolean());
Console.WriteLine(GreaterOrEqualThan(6, 6).EvalBoolean());
Console.WriteLine("----------------------------------");
Console.WriteLine(LessOrEqualThan(x, y));
Console.WriteLine(LessOrEqualThan(6, 5));
Console.WriteLine(LessOrEqualThan(6, 5).EvalBoolean());
Console.WriteLine(LessOrEqualThan(6, 6).EvalBoolean());
Console.WriteLine("----------------------------------");
var statement1 = GreaterThan(Sqr(x), 5);
Console.WriteLine(statement1);
Console.WriteLine(statement1.Solve("x"));
Console.WriteLine("----------------------------------");
var statement2 = GreaterThan(Sqr(x), 16) & LessThan(x, y);
Console.WriteLine(statement2);
Console.WriteLine(statement2.Solve("x"));
Console.WriteLine("----------------------------------");
var statement3 = LessThan(Sqr(x), 16) & GreaterThan(x, 2);
Console.WriteLine(statement3);
Console.WriteLine(statement3.Solve("x"));