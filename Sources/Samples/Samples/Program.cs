using AngouriMath;
using HonkSharp.Fluency;

using System;
using static AngouriMath.Entity;
using AngouriMath.Extensions;
using static AngouriMath.Entity.Number;

/*
var (f, x, n) = (MathS.Var("f"), MathS.Var("x"), MathS.Var("n"));

var applied = f.Apply(x.Pow(2));
Console.WriteLine(applied);
Console.WriteLine(applied.Differentiate(x));
Console.WriteLine(applied.Differentiate(x).LambdaOver(f).Apply("sin").InnerSimplified);


return;
*/
/*
Entity expr = "5 xor (1 - 1)";
Console.WriteLine(expr.Simplify());
Console.WriteLine(Postprocess(expr.Simplify()));

Entity Postprocess(Entity entity)
    => entity.Replace(entity => entity switch
    {
        Xorf(Integer(0), var any1) => any1,
        Xorf(var any1, Integer(0)) => any1,
        var other => other
    });
    */
// Console.WriteLine(ReplaceIntsWithBooleans("5 xor 0").Evaled);
// 
// Entity ReplaceIntsWithBooleans(Entity expr) => expr.Substitute(0, false).Substitute(1, true);
// 
// 
// Entity ReplaceBooleansWithInts(Entity expr) => expr.Substitute(false, 0).Substitute(true, 1);

/*
var (f, x, n) = (MathS.Var("f"), MathS.Var("x"), MathS.Var("n"));
var number3 = f.Apply(f.Apply(f.Apply(x))).LambdaOver(x).LambdaOver(f);
var succ = f.Apply(n.Apply(f).Apply(x)).LambdaOver(x).LambdaOver(f).LambdaOver(n);
Console.WriteLine(number3);
Console.WriteLine(succ);
Console.WriteLine(succ.Apply(number3));
using var _ = MathS.Diagnostic.OutputExplicit.Set(true);
Console.WriteLine(succ.Apply(number3).InnerSimplified);*/

Entity expr =
@"
apply(
    lambda(let, 
        apply(let, 3.14, lambda(myPi,
        apply(let, 56, lambda(volume,
        apply(let, 30, lambda(length,
            volume / length - myPi
        ))))))
    ),
    lambda(f, x, apply(x, f))
)
";
Console.WriteLine(expr.InnerSimplified);