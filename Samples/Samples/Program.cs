using System;
using AngouriMath;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Boolean;
using static AngouriMath.MathS;
using AngouriMath.Extensions;
using static System.Console;
using static AngouriMath.Entity.Set;

//WriteLine("a^x + b^x = c^x".Solve("x"));
//WriteLine("x e ^ x = a".Solve("x"));
//WriteLine("e ^ x + e ^ (2x) + c = d".Solve("x"));
//WriteLine("x ^ x + x ^ (2x) = c".Solve("x"));

//var sols = (FiniteSet)"4 ^ (x3 + x) + 2 ^ (x3 + x) = c".Solve("x");
//
////foreach (var sol in sols)
////    WriteLine(sol.Substitute("c", 1).Substitute("n_1", 1).Evaled);
//
//Entity sol = "4 ^ (x3 + x) + 2 ^ (x3 + x) = c";
//
//Console.WriteLine(MathS.ToSympyCode(sol));

WriteLine("x > a or x ^ 2 = a^3".Solve("x"));