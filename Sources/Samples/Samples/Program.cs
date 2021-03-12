using AngouriMath;
using AngouriMath.Core.Multithreading;
using AngouriMath.Extensions;
using Microsoft.VisualBasic.CompilerServices;
using PeterO.Numbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;
using static AngouriMath.MathS;
using static AngouriMath.MathS.Hyperbolic;
using static System.Console;

//WriteLine(@"(x15 - 3^15 - 15 3^14(x - 3)) / (x - 3)2".Limit("x", "3", AngouriMath.Core.ApproachFrom.Left));

//WriteLine("log(e, ((-1 - sqrt(1 - 4 * -a)) / 2) ^ (1 / log(e, 2)))".Simplify());
//WriteLine("ln(((-1 - sqrt(1 - 4 * -3)) / 2) ^ (1 / ln(2)))".EvalNumerical());
//WriteLine("ln(-1/2 * (1 + sqrt(1 + 4 * 3))) / ln(2)".EvalNumerical());

//var n = "4 + 3i";
//var expr1 = "ln(a ^ b)";
//var expr2 = "b ln(a)";
//var ev1 = expr1.Substitute("a", n).Substitute("b", n).EvalNumerical();
//var ev2 = expr2.Substitute("a", n).Substitute("b", n).EvalNumerical();
//WriteLine(ev2 - ev1);
//WriteLine(MathS.pi.EvalNumerical() * 2);
//var expr = "4^x + 2^x - a";
////WriteLine(expr.Substitute("a", n).Substitute("x", ev1).EvalNumerical());
//WriteLine("(x - goose) * (x - momo) * (x - quack) * (x - momo * goose * quack)".Expand().SolveEquation("x"));
//WriteLine(Number.Arccosecant(Number.Cosecant(1.2)));

//var mat = MathS.Matrices.Matrix(new Entity[,]
//    {
//        { "(x + 0)2", "(x + 1)2", "(x + 2)2" },
//        { "(x + 1)2", "(x + 2)2", "(x + 3)2" },
//        { "(x + 2)2", "(x + 3)2", "(x + 4)2" }
//    }
//    );

//for (int i = -4; i <= 13; i++)
//    WriteLine(((Tensor)mat.Substitute("x", $"{i} / 3")).Determinant());
//MathS.Settings.MaxExpansionTermCount.Global(10000);

//WriteLine(MathS.TryPolynomial(mat.Det

//WriteLine("1 + 1 / x".Simplify());

//WriteLine("(log(e, x) * (log(e, x) + 1) * x ^ x + x ^ (x - 1)) * x ^ x ^ x".Latexise());

//WriteLine("((2x2 + 10x + 1) ^ (1/5) - (x2 + 10x + 1) ^ (1/7)) ^ 35".Expand().Simplify());

//Entity expr = "a e sin(x ^ 14 + 3)3 + a b c d sin(x ^ 14 + 2)4 - k d sin(x ^ 14 + 3)2 + sin(x ^ 14 + 3) + e = 0";
//
//var lastTask = MathS.Multithreading.RunAsync(() => {
//    WriteLine("Started computing...");
//    var res = expr.Solve("x");
//    WriteLine(res.Complexity);
//});
//
//var input = ReadLine() ?? "0";
//if (input == "stop")
//{
//    lastTask.Cancel();
//    WriteLine("Canceled...");
//}
//

//WriteLine("X + 2 / 3");
//Entity expr = "(((x provided a) + 1) * 2 provided b) + (3 provided c) / ((4 provided 5) provided d)";

// Entity expr = "2x2 - 4x - 8 = A(x3 + 4x) + B(x2 + 4) + C(x3 - x2) + D(x2 - x)";
// WriteLine(expr.Substitute("x", 2).Substitute("A", 0).Substitute("B", -2).Simplify().InnerSimplified);

// Entity eq = "piecewise(x2 provided b, (x + 1)2 provided d, (x + 2)2 provided g) = 4";
// var x = MathS.Var("x");
// WriteLine(eq.Solve(x));

// WriteLine("[{sqrt(3), sqrt(2)}; {sqrt(5), sqrt(10)}]".Simplify());
// var (x, y) = (MathS.Var("x"), MathS.Var("y"));
// var a = MathS.Piecewise(new[] { new Providedf(x, y), new Providedf(y, x), new Providedf(x + 333, y + 2) }, 3); 
// var b = MathS.FromString("piecewise(x provided y, y provided x, x + 2 provided y + 2, 3)");
// WriteLine(a == b);

// var func = "arccos";
// 
// Entity initial = @$"{func}(piecewise(a provided b, c provided d, e provided f))";
// Entity expected = @$"piecewise(({func}(a) provided b), ({func}(c) provided d), ({func}(e) provided f))";
// var actual = initial.InnerSimplified;

// WriteLine("piecewise(a provided b, c provided d, e)".Latexise());
// WriteLine(MathS.Piecewise(("a", "b"), ("c", "e"), ("g", true)).Latexise());
// WriteLine("(|x|) = a".Solve("x").InnerSimplified.Provided(MathS.Piecewise(("integral(x + 3, x)", "x > 0"), ("derivative(x + 3, x)", "x < 0"))).Latexise());
// Entity e = "(b implies a) and (b or c)"
//Entity abs = "piecewise(x provided x > 0, -x provided x <= 0)";
//WriteLine(abs.Substitute("x", 3).EvalNumerical());
//WriteLine(abs.Substitute("x", -3).EvalNumerical());

// WriteLine("x!!".ToEntity());

// WriteLine(Series.TaylorExpansion("a t3 + b t2 + c t + d", "t", "x", "0", 4));
// WriteLine("sqrt(x - 1) / e ^ (x - 1) + sin(x)".Differentiate("x").Differentiate("x").Differentiate("x").Differentiate("x").Differentiate("x"));

// (t - sin(t)) / (t3)

// WriteLine("(3t)^(2t2)".Limit("t", 0));
// WriteLine("arccotan(0)".EvalNumerical());

// var a = "c and (c or (a > 0)) and (c or (a < 0)) and (c or (a > 0) | (a < 0)) and (c or (a < x ^ 2*(1 - x))) and (c or (a < 0) or (a < x^2*(1 - x)))".Simplify();
// WriteLine(a);
//WriteLine("sin(t) / t".Limit("t", 0).Simplify());


// WriteLine("sin(a) * cos(b) * tan(c) / (tan(c)3 * sin(a)2 * cos(b)^(-2))".Simplify());

// Entity withPhi = "phi(5 ^ x) * 1 / 5 ^ x".ToEntity();
// Entity withoutPhi = "5 ^ (x - 1) * 4 * 1 / 5 ^ x".ToEntity();
// WriteLine(withPhi.SimplifiedRate);
// WriteLine(withoutPhi.SimplifiedRate);

// WriteLine("((a^x - 1) - (b^x - 1)) / x".Limit("x", 0));


// var func = "x > 3 and (a implies b)".Compile<int, bool, bool, bool>("x", "a", "b");
// Console.WriteLine(func(4, false, true));

var func = "x + sin(y) + 2ch(0)".Compile<System.Numerics.Complex, double, System.Numerics.Complex>("x", "y");
Console.WriteLine(func(new(3, 4), 1.2d));