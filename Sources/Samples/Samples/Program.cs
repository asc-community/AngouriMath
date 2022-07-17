//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath;
using static AngouriMath.MathS;

Entity expr1 = 5;
Console.WriteLine($"{expr1}, IsConstantLeaf: {expr1.IsConstantLeaf}");
Entity expr2 = Sin(5);
Console.WriteLine($"{expr2}, IsConstantLeaf: {expr2.IsConstantLeaf}");
Entity expr3 = pi;
Console.WriteLine($"{expr3}, IsConstantLeaf: {expr3.IsConstantLeaf}");
Entity expr4 = 3 + 4 * i;
Console.WriteLine($"{expr4}, IsConstantLeaf: {expr4.IsConstantLeaf}");
Entity expr5 = (Entity)3 + 4 * i;
Console.WriteLine($"{expr5}, IsConstantLeaf: {expr5.IsConstantLeaf}");
var expr6 = expr5.InnerSimplified;
Console.WriteLine($"{expr6}, IsConstantLeaf: {expr6.IsConstantLeaf}");
var expr7 = GreaterThan(pi, e);
Console.WriteLine($"{expr7}, IsConstantLeaf: {expr7.IsConstantLeaf}");
var expr8 = expr7.Evaled;
Console.WriteLine($"{expr8}, IsConstantLeaf: {expr8.IsConstantLeaf}");