[![Codacy Badge](https://api.codacy.com/project/badge/Grade/1e172cdf699645b59567032dd1ae5cab)](https://www.codacy.com/manual/Angourisoft/MathS?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=Angourisoft/MathS&amp;utm_campaign=Badge_Grade)
![Nuget](https://img.shields.io/nuget/dt/AngouriMath?color=blue&label=NuGet%20installs&logoColor=blue)
![GitHub](https://img.shields.io/github/license/AngouriSoft/MathS?color=purple)

![alt text](./banner.png "Logo")

[Nuget](https://www.nuget.org/packages/AngouriMath "Link to .NET package repository")

## AngouriMath
AngouriMath is an open-source library that enables to work with non-linear 
multi-variable expressions. Its functionality includes derivation, 
variable substitution, equation solving, equation system solving, definite integration, 
formula-to-latex formatting, working with mathematical sets, and some more.

If you are new to AM, we suggest you checking out some samples instead of reading boring 
documentation. If you prefer full manual to AM, see [Wiki](https://github.com/asc-community/AngouriMath/wiki).
If you want to contribute, we surely appreciate it, but so far do not have documentation for
you. It will appear soon!

### Examples

#### Build an expression
```cs
var x = MathS.Var("x");
var y = MathS.Var("y");
var c = x * y + x / y;
Console.WriteLine(MathS.Sqr(c));
>>> (x * y + x / y) ^ 2
```

#### Use as a simple calculator
```cs
var inp = "1 + 2 * log(3, 9)";
var expr = MathS.FromString(inp);
Console.WriteLine(expr.Eval());
>>> 5
```

#### Substitute variables
```cs
var x = MathS.Var("x");
var expr = x * 2 + MathS.Sin(x) / MathS.Sin(MathS.Pow(2, x));
var subs = expr.Substitute(x, 0.3);
Console.WriteLine(subs.Eval());
>>> 0,9134260185941638995386706112
```

#### Find derivatives
```cs
var x = MathS.Var("x");
var func = MathS.Sqr(x) + MathS.Ln(MathS.Cos(x) + 3) + 4 * x;
var derivative = func.Derive(x);
Console.WriteLine(derivative.Simplify());
>>> 4 + (-1) * sin(x) / (cos(x) + 3) + 2 * x
```

#### Build expressions faster
```cs
Entity expr = "sqr(x + y)";
Console.WriteLine(expr.Expand().Simplify());
>>> x ^ 2 + 2 * x * y + y ^ 2
```

#### Simplify
```cs
var x = MathS.Var("x");
var a = MathS.Var("a");
var b = MathS.Var("b");
var expr = MathS.Sqrt(x) / x + a * b + b * a + (b - x) * (x + b) + 
    MathS.Arcsin(x + a) + MathS.Arccos(a + x);
Console.WriteLine(expr.Simplify());
>>> 2 * a * b + b ^ 2 + pi / 2 + x ^ (-1 / 2) - x ^ 2
```

#### Render latex
```cs
var x = MathS.Var("x");
var y = MathS.Var("y");
var expr = x.Pow(y) + MathS.Sqrt(x + y / 4) * (6 / x);
Console.WriteLine(expr.Latexise());
>>> {x}^{y}+\sqrt{x+\frac{y}{4}}\times \frac{6}{x}
```

#### Play with complex numbers
```cs
var expr = MathS.Pow(MathS.e, MathS.pi * MathS.i);
Console.WriteLine(expr);
Console.WriteLine(expr.Eval());
>>> e ^ (pi * i)
>>> -1
```

#### Solve equations analytically
Under developing now and forever (always available)
```cs
Entity expr = "(sin(x)2 - sin(x) + a)(b - x)((-3) * x + 2 + 3 * x ^ 2 + (x + (-3)) * x ^ 3)";
foreach (var root in expr.Solve("x"))
    Console.WriteLine(root);
>>> arcsin((1 - sqrt(1 + (-4) * a)) / 2) - (-2) * n * pi
>>> 2 * n * pi + pi - arcsin((1 - sqrt(1 + (-4) * a)) / 2)
>>> arcsin(0.5 * (1 + sqrt(1 + (-4) * a))) - (-2) * n * pi
>>> 2 * n * pi + pi - arcsin((1 + sqrt(1 + (-4) * a)) / 2)
>>> b
>>> -i
>>> i
>>> 1
>>> 2
```

#### Solve systems of non-linear equations
Under developing now and forever (always available)
```cs
var system = MathS.Equations(
    "cos(x2 + 1)^2 + 3y",
    "y * (-1) + 4cos(x2 + 1)"
);
Console.WriteLine(system.Latexise());
var solutions = system.Solve("x", "y");
Console.WriteLine(Solutions.PrintOut());
```

#### Integrate
Only definite integral over single variable is supported yet :(
```cs
var x = MathS.Var("x");
var expr = MathS.Sin(x) + MathS.Sqrt(x) / (MathS.Sqrt(x) + MathS.Cos(x)) + MathS.Pow(x, 3);
Console.WriteLine(expr.DefiniteIntegral(x, -3, 3));
var expr2 = MathS.Sin(x);
Console.WriteLine(expr2.DefiniteIntegral(x, 0, MathS.DecimalConst.pi));
>>> 5.56669223384056 + 0.0889406793629381i
>>> 1.98003515236381
```

#### Compile functions
Compiled functions work 15x+ faster
```cs
var x = MathS.Var("x");
var expr = MathS.Sin(x) + MathS.Sqrt(x) / (MathS.Sqrt(x) + MathS.Cos(x)) + MathS.Pow(x, 3);
var func = expr.Compile(x);
Console.WriteLine(func.Substitute(3));
```

#### Try new syntax
```cs
Entity expr = "3x3 + 2 2 2 - x(3 0.5)";
Console.WriteLine(expr);
>>> 3 * x ^ 3 + 2 ^ 2 ^ 2 - x * sqrt(3)
```

#### Work with sets
```cs
var A = new Set(3, 4, (5, 6)); // {3, 4} | [5; 6]
var B = new Set((x, MathS.Sqrt(x)), 4);
var C = (A | B) & A;
```

#### Try SymPy syntax
```cs
var x = SySyn.Symbol("x");
var expr = SySyn.Exp(x) + x;
Console.WriteLine(SySyn.Diff(expr));
Console.WriteLine(SySyn.Diff(expr, x));
Console.WriteLine(SySyn.Diff(expr, x, x));
```

#### Work with numbers
```cs
var rat1 = Number.CreateRational(3, 4);
var rat2 = Number.CreateRational(5, 6);
Console.WriteLine((rat1 + rat2).ToString());
>>> 19 / 12
```

#### Translate number systems
```cs
string x = MathS.ToBaseN(-32.25, 4);
Console.WriteLine("-32.25(10) = " + x + "(4)");
double y = MathS.FromBaseN("AB.3", 16);
Console.WriteLine("AB.3(16) = " + y + "(1)");
>>> -32.25(10) = -200.1(4)
>>> AB.3(16) = 171,1875(1)
```

### Performance
Performane improved a lot. Testing on i7-7700HQ and ```expr = MathS.Sin(x)``` we get the following report:

| Function                             | Time per iteration              |
| ------------------------------------ | ------------------------------- |
| Substitute(x, 3).Eval() from 1.0.13  | 12000 ns                        |
| Substitute(x, 3).Eval() from 1.0.15  | 2500 ns                         |
| Call(3) from 1.0.15                  | 54 ns                           |
| Complex.Sin(3)                       | 27 ns                           |

If we take ```expr = MathS.Sin(MathS.Sqr(x)) + MathS.Cos(MathS.Sqr(x)) + MathS.Sqr(x) + MathS.Sin(MathS.Sqr(x))```, AM Compiled
is faster than any other methods:

| Method                   | Time per iteration |
|------------------------- |-------------------:|
| AM Compiled              |  310.0 ns          |
| In-code expression       |  424.2 ns          |
| LinqCompiled             |  435.9 ns          |
| Substitute(x, 3).Eval()  | 6777.3 ns          |

It is true since release of 1.0.17.1 Beta, when cache instructions in compiled functions were added.

Finally, if we take ```expr = (MathS.Log(x, 3) + MathS.Sqr(x)) * MathS.Sin(x + MathS.Cosec(x))```, 
we get the following performance

| Method                   | Time per iteration |
|--------------------------|-------------------:|
| AM Compiled              |  380.8 ns          |
| In-code expression       |  211.5 ns          |
| Substitute(x, 3).Eval()  | 5656.3 ns          |

So, for most cases compilation will save you enough time even though built-in functions are 
still faster sometimes.

## More information

More info about methods see on [Wiki](https://github.com/asc-community/AngouriMath/wiki).
