using AngouriMath;
using HonkSharp.Fluency;

using System;
using static AngouriMath.Entity;
using AngouriMath.Extensions;

var (f, x, n) = (MathS.Var("f"), MathS.Var("x"), MathS.Var("n"));
var number3 = f.Apply(f.Apply(f.Apply(x))).LambdaOver(x).LambdaOver(f);
var succ = f.Apply(n.Apply(f).Apply(x)).LambdaOver(x).LambdaOver(f).LambdaOver(n);
Console.WriteLine(number3);
Console.WriteLine(succ);
Console.WriteLine(succ.Apply(number3));
Console.WriteLine(succ.Apply(number3).InnerSimplified);