using AngouriMath;
using AngouriMath.Extensions;
using Microsoft.VisualBasic.CompilerServices;
using PeterO.Numbers;
using System;
using System.Collections.Generic;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;
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

WriteLine("phi(2^x) / 2^x".Simplify());

//WriteLine("(log(e, x) * (log(e, x) + 1) * x ^ x + x ^ (x - 1)) * x ^ x ^ x".Latexise());

//WriteLine("((2x2 + 10x + 1) ^ (1/5) - (x2 + 10x + 1) ^ (1/7)) ^ 35".Expand().Simplify());
