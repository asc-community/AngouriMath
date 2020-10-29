using System;
using AngouriMath;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Boolean;
using static AngouriMath.MathS;
using AngouriMath.Extensions;
using static System.Console;
using static AngouriMath.Entity.Set;
using System.Diagnostics;
using System.Collections;
using System.Numerics;
using PeterO.Numbers;

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
//WriteLine(ToSympyCode("3 ^ (2 * i * pi)".EvalNumerical()));
//ToSympyCode("3 + i".EvalNumerical());
//WriteLine("((x + 1) / (x + 2)) ^ (1 - x)".Limit("x", "+oo"));
WriteLine("(8 ^ x - 6 ^ x) / x".Limit("x", "0"));