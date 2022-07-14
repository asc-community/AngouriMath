//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;

using System;
using AngouriMath.Extensions;
using static AngouriMath.MathS;

Entity expr = "sin";
Console.WriteLine(expr);
var applied = expr.Apply(pi / 3);
Console.WriteLine(applied);
Console.WriteLine(applied.Simplify());
Console.WriteLine(applied.Evaled);
Console.WriteLine("------------------------------");
var lambda = Lambda("x", "x ^ 3 + x");
Console.WriteLine(lambda);
Console.WriteLine(lambda.Apply("3"));
Console.WriteLine(lambda.Apply("3").Evaled);
Console.WriteLine("------------------------------");
var lambda2 = Lambda("y", "y".ToEntity().Apply(pi / 3));
Console.WriteLine(lambda2);
Console.WriteLine(lambda2.Apply("sin").Simplify());
Console.WriteLine(lambda2.Apply("cos").Simplify());
Console.WriteLine(lambda2.Apply("tan").Simplify());
Console.WriteLine("------------------------------");
var lambda3 = Lambda("x", Lambda("y", Lambda("z", "x + y / z")));
Console.WriteLine(lambda3);
Console.WriteLine(lambda3.Apply(5));
Console.WriteLine(lambda3.Apply(5).Simplify());
Console.WriteLine(lambda3.Apply(5).Apply(10));
Console.WriteLine(lambda3.Apply(5).Apply(10).Simplify());
Console.WriteLine(lambda3.Apply(5, 10));
Console.WriteLine(lambda3.Apply(5, 10).Simplify());
Console.WriteLine(lambda3.Apply(5, 10, 7));
Console.WriteLine(lambda3.Apply(5, 10, 7).Simplify());
