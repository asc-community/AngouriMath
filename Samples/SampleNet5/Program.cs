using static System.Console;
using AngouriMath.Extensions;
using static AngouriMath.MathS;


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
