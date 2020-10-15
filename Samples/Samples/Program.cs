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

//WriteLine("((x + a) / (x - a))^x".Limit("x", "0"));
//WriteLine("(x - a * x) / (b * x - x)".Limit("x", "0"));
//WriteLine("(a + -1) / (1 - b) + -(1 - a) / (b + -1)".Simplify());
WriteLine(Complex.Sin(new Complex(0, 30)));
WriteLine(Complex.Sin(new Complex(0, 30)) * Complex.Sin(new Complex(0, 30)));
WriteLine(Complex.Cos(new Complex(0, 30)));
WriteLine(Complex.Cos(new Complex(0, 30)) * Complex.Cos(new Complex(0, 30)));
WriteLine(
    Complex.Sin(new Complex(0, 30)) * Complex.Sin(new Complex(0, 30)) +
    Complex.Cos(new Complex(0, 30)) * Complex.Cos(new Complex(0, 30))
    );
WriteLine("sin(x)2 + cos(x)2".Substitute("x", "30i").EvalNumerical());
//Entity a = (0, 30);
//WriteLine(a);