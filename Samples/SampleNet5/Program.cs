using static System.Console;
using AngouriMath.Extensions;
using static AngouriMath.MathS;
using AngouriMath;
using static AngouriMath.Entity.Set;
using static AngouriMath.Entity.Number;


// Hello world in AM
WriteLine("Alright, let's start from a hello world");
WriteLine("2 + 3 is " + "2 + 3".EvalNumerical().Stringize());
WriteLine();

// Simplify
WriteLine("x + 3 + 4 + x + 2x + 23 + a".Simplify().Stringize());
WriteLine();

// Build expressions
var x = Var("x");
var expr = Sin(x) + Sqrt(x) + Integral(Sin(Sin(x)), x);
WriteLine(expr.Stringize());
WriteLine();

// Derive
WriteLine(expr.Derive(x).Simplify().Stringize());
WriteLine();

// Solve a simple equation
WriteLine("x2 = 3".Solve("x").Stringize());
WriteLine();

// Solve a complicated statement
WriteLine("(x - 2)(x - 3) = 0 and (x - 1)(x - 2) = 0".Solve("x").InnerSimplified.Stringize());
WriteLine("sin(a x)2 + c sin(2 a x) = c".Solve("x"));
WriteLine();

// Work with sets
WriteLine(@"{ 1, 2, 3 } \/ { 3, 5 }".Simplify());
WriteLine(@"[a; b) \/ { b }".Simplify());
WriteLine();

// Differentiate, integrate, find limits
WriteLine("x2 + a x".Derive("x").InnerSimplified);
WriteLine("x2 + a x".Integrate("x").InnerSimplified);
WriteLine("(a x2 + b x) / (e x - h x2 - 3)".Limit("x", "+oo").InnerSimplified);
WriteLine();

// Boolean
WriteLine("true and false implies true".Simplify());
WriteLine(string.Join("       ", new[] { "a", "b", "c", "F" }));
WriteLine(MathS.Boolean.BuildTruthTable("a and b implies c", "a", "b", "c"));
WriteLine();

// LaTeX
WriteLine("sqrt(x) + x / a + integral(sin(x), x, 1)".Latexise());