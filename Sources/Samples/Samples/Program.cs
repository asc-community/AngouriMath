//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using HonkSharp.Fluency;

using System;
using AngouriMath.Core.NovaSyntax;
using static AngouriMath.Entity;
using AngouriMath.Extensions;
using static AngouriMath.Entity.Number;

using static AngouriMath.MathS;
using static System.Console;
using PeterO.Numbers;


// var system = Equations(
// "(x2 + y) / 2",
// "y - x - 3"
// );
// Console.WriteLine(system.Solve("x", "y"));

var expr = @"
x + b a c
";

var lexer = new NovaLexer(expr);
while (true)
{
    var token = lexer.Next();
    if (token.Kind == AngouriMathTokenType.End) break;
    Console.WriteLine(token);
}

using var _ = MathS.Diagnostic.OutputExplicit.Set(true);

WriteLine(((Sumf)expr).Addend.GetType());

// lexer.Next()
// var parser = new NovaParser(lexer);
// Console.WriteLine(parser.ParseExpression().Ok.Value);
/*

Console.WriteLine(@"
apply(
    lambda(x, y, apply(x, y)),
    y)
".Simplify());
*/