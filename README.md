[![Codacy Badge](https://api.codacy.com/project/badge/Grade/1e172cdf699645b59567032dd1ae5cab)](https://www.codacy.com/manual/Angourisoft/MathS?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=Angourisoft/MathS&amp;utm_campaign=Badge_Grade)
![Nuget](https://img.shields.io/nuget/dt/AngouriMath?color=blue&label=NuGet%20installs&logoColor=blue)
![GitHub](https://img.shields.io/github/license/AngouriSoft/MathS?color=purple)

[Nuget](https://www.nuget.org/packages/AngouriMath "Link to .NET package repository")

## AngouriMath
AngouriMath is a cross-platform open-source library that enables to work with non-linear 
multi-variable expressions.

README navigation:
- [Installation](#inst)
- [Examples](#exam)
  - [Evaluation](#eval)
  - [Substitution](#subs)
  - [Derivation](#deri)
  - [Simplification](#simp)
  - [Numbers](#numb)
  - [Equations](#equa)
  - [Equation systems](#eqsys)
  - [Compilation](#comp)
  - [Sets](#sets)
  - [LaTeX](#late)
  - [Number system](#numsys)

If you are new to AM, we suggest you checking out some samples instead of reading boring 
documentation. If you prefer full manual to AM, see [Wiki](https://github.com/asc-community/AngouriMath/wiki).
If you want to contribute, we surely appreciate it, but so far do not have documentation for
you. It will appear soon!

### <a name="inst"></a>Installation

The easiest way to install AM is to install it from 
[Nuget](https://www.nuget.org/packages/AngouriMath "Link to .NET package repository").

Alternatively, if you use git entirely, that is how you clone the repo:
```
git clone --recurse-submodules https://github.com/asc-community/AngouriMath
```
Add this repo to your project's dependencies:
```
git submodule add https://github.com/asc-community/AngouriMath
```
After cloning, you do not need to set up it.

However, you should make sure that you have a Java runtime installed for ANTLR to run.

It is ready to use, just add the reference to
- the `Numbers` project at `PeterONumbers/Numbers/Numbers.csproj` and
- the `AngouriMath` project at `AngouriMath/AngouriMath.csproj`

from your solution.

### <a name="exam"></a>Examples

#### <a name="eval"></a>Use as a simple calculator
```cs
var inp = "1 + 2 * log(3, 9)";
var expr = MathS.FromString(inp);
Console.WriteLine(expr.Eval());
>>> 5
```

```cs
Console.WriteLine("2 / 3 + sqrt(-16)".Eval());
>>> 2 / 3 + 4i
```

```cs
Console.WriteLine("(-2) ^ 3".Eval());
>>> -8
```

#### <a name="subs"></a>Substitute variables
```cs
var x = MathS.Var("x");
Entity expr = x * 2 + MathS.Sin(x) / MathS.Sin(MathS.Pow(2, x));
var subs = expr.Substitute("x", 0.3);
Console.WriteLine(subs.Eval());
>>> 0,9134260185941638995386706112
```

#### <a name="deri"></a>Find derivatives
```cs
var x = MathS.Var("x");
var func = MathS.Sqr(x) + MathS.Ln(MathS.Cos(x) + 3) + 4 * x;
var derivative = func.Derive(x);
Console.WriteLine(derivative.Simplify());
>>> 4 + sin(x) / (log(e, cos(x) + 3) ^ 2 * (cos(x) + 3)) + 2 * x
```

#### <a name="simp"></a>Simplify
```cs
var x = MathS.Var("x");
var a = MathS.Var("a");
var b = MathS.Var("b");
var expr = MathS.Sqrt(x) / x + a * b + b * a + (b - x) * (x + b) + 
    MathS.Arcsin(x + a) + MathS.Arccos(a + x);
Console.WriteLine(expr.Simplify());
>>> 2 * a * b + b ^ 2 + 1 / 2 * pi + x ^ (-1 / 2) - x ^ 2
```

#### <a name="late"></a>Render latex
```cs
var x = MathS.Var("x");
var y = MathS.Var("y");
var expr = x.Pow(y) + MathS.Sqrt(x + y / 4) * (6 / x);
Console.WriteLine(expr.Latexise());
>>> {x}^{y}+\sqrt{x+\frac{y}{4}}\times \frac{6}{x}
```

#### <a name="equa"></a>Solve equations analytically
Under developing now and forever (always available)
```cs
Entity expr = "(sin(x)2 - sin(x) + a)(b - x)((-3) * x + 2 + 3 * x ^ 2 + (x + (-3)) * x ^ 3)";
foreach (var root in expr.SolveEquation("x"))
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

#### <a name="eqsys"></a>Solve systems of non-linear equations
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

#### <a name="comp"></a>Compile functions
Compiled functions work 15x+ faster
```cs
var x = MathS.Var("x");
var expr = MathS.Sin(x) + MathS.Sqrt(x) / (MathS.Sqrt(x) + MathS.Cos(x)) + MathS.Pow(x, 3);
var func = expr.Compile(x);
Console.WriteLine(func.Substitute(3));
```

#### <a name="sets"></a>Work with sets
```cs
var A = new Set(3, 4, (5, 6)); // {3, 4} | [5; 6]
var B = new Set((x, MathS.Sqrt(x)), 4);
var C = (A | B) & A;
```

#### <a name="numb"></a>Work with numbers
```cs
var rat1 = Number.CreateRational(3, 4);
var rat2 = Number.CreateRational(5, 6);
Console.WriteLine((rat1 + rat2).ToString());
>>> 19 / 12
```

#### <a name="numsys"></a>Translate number systems
```cs
string x = MathS.ToBaseN(-32.25, 4);
Console.WriteLine("-32.25(10) = " + x + "(4)");
double y = MathS.FromBaseN("AB.3", 16);
Console.WriteLine("AB.3(16) = " + y + "(1)");
>>> -32.25(10) = -200.1(4)
>>> AB.3(16) = 171,1875(1)
```
