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
using Yoakke.Streams;
using Yoakke.SynKit.Lexer;

foreach (var c in "([A-Za-zΑ-ω])+(_([0-9A-Za-zΑ-ω])+)?")
    WriteLine($"{c}: {(int)c}");

var e = "x";

/*
var allTokens = new NovaLexer(e).LexAll();
var stream1 = new NovaLexer(e).ToStream().ToBuffered();
var stream2 = new EnumerableStream<IToken<AngouriMathTokenType>>(allTokens).ToBuffered();


WriteLine(string.Join(", ", allTokens));
WriteLine();
WriteLine(stream1.Peek());
WriteLine(stream2.Peek());

return;*/


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