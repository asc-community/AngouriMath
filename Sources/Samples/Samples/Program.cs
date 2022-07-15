//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath;
using static AngouriMath.MathS.Utils;

Entity expr = "(x^2 + 2)(a + b + 2x) + x + sin(h)";
if (TryGetPolynomial(expr, "x", out var dict))
    foreach (var (pow, coef) in dict)
        Console.WriteLine($"Pow: {pow}. Coef: {coef}");
Console.WriteLine("------------------------");
Entity expr1 = "sin(x) + a";
if (TryGetPolynomial(expr1, "x", out var dict1))
    foreach (var (pow, coef) in dict1)
        Console.WriteLine($"Pow: {pow}. Coef: {coef}");
else
    Console.WriteLine("Failed to interpret as polynomial");
Console.WriteLine("------------------------");
Entity expr2 = "(x + a)(b + x) + a + 2 + x";
if (TryGetPolyQuadratic(expr2, "x", out var a, out var b, out var c))
    Console.WriteLine($"The expr is ({a}) * x^2 + ({b}) * x + ({c})");
Console.WriteLine("------------------------");
Entity expr3 = "(b + x) + a + 2 + x";
if (TryGetPolyLinear(expr3, "x", out var a1, out var b1))
    Console.WriteLine($"The expr is ({a1}) * x + ({b1})");