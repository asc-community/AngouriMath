<p align="center">
  <a href="https://github.com/asc-community/AngouriMath">
    <img src="./additional/readme/icon_white.png" alt="AngouriMath logo" width="200" height="200">
  </a>
</p>

<h3 align="center">AngouriMath</h3>

<p align="center">
  New, skyrocketing symbolic algebra library in .NET. Everything one would need.
  <br>
  <a href="#inst">Download</a>
  ·
  <a href="#exam">Examples</a>
  ·
  <a href="#contrib">Contributions</a>
  <br>
</p>

## What is it about?

AngouriMath is a cross-platform open-source library that enables to work with non-linear 
multi-variable expressions. Written in C#. Free and distributed under MIT license. Raised
by community, hence, any contribution is welcomed.

![Test](https://github.com/asc-community/AngouriMath/workflows/Test/badge.svg)
  [![NuGet](https://img.shields.io/nuget/vpre/AngouriMath?color=blue&label=NuGet)](https://www.nuget.org/packages/AngouriMath)
  [![GitHub](https://img.shields.io/github/license/AngouriSoft/MathS?color=purple)](https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md)
  [![Discord](https://img.shields.io/discord/642350046213439489?color=orange&label=Discord)](https://discord.gg/YWJEX7a)

### README navigation:
- [Installation](#inst)
- [Examples](#exam)
  - [Evaluation](#eval)
  - [Substitution](#subs)
  - [Derivation](#deri)
  - [Simplification](#simp)
  - [Boolean](#bool)
  - [Numbers](#numb)
  - [Equations](#equa)
  - [Equation systems](#eqsys)
  - [More complex equations](#stat)
  - [Compilation](#comp)
  - [Sets](#sets)
  - [LaTeX](#late)
  - [Number system](#numsys)
- [I want to contribute](#contrib)

If you are new to AM, we suggest you checking out some samples instead of reading boring 
documentation. If you prefer full manual to AM, see [Wiki](https://github.com/asc-community/AngouriMath/wiki).
If you want to contribute, we surely appreciate it, but so far do not have documentation for
you. It will appear soon!

### <a name="inst"></a>Installation

The easiest way to install AM is to install it from 
[Nuget](https://www.nuget.org/packages/AngouriMath "Link to .NET package repository").

If you need git commands, that is how you clone the repo
```
git clone --recurse-submodules https://github.com/asc-community/AngouriMath
```
Add this repo to your project's dependencies
```
git submodule add https://github.com/asc-community/AngouriMath
```
After cloning, you do not need to set up it. It is ready to use, just add the reference to the AngouriMath project from your solution.

### <a name="exam"></a>Examples

#### <a name="eval"></a>Use as a simple calculator
```cs
Entity expr = "1 + 2 * log(3, 9)";
Console.WriteLine(expr.EvalNumerical());
```
<img src="https://render.githubusercontent.com/render/math?math=5">

```cs
Console.WriteLine("2 / 3 + sqrt(-16)".EvalNumerical());
>>> 2 / 3 + 4i
```
<img src="https://render.githubusercontent.com/render/math?math=\frac{2}{3} %2B 4i">

```cs
Console.WriteLine("(-2) ^ 3".EvalNumerical());
```
<img src="https://render.githubusercontent.com/render/math?math=-8">

#### <a name="subs"></a>Substitute variables
```cs
Entity expr = "2x + sin(x) / sin(2 ^ x)";
var subs = expr.Substitute("x", 0.3m);
Console.WriteLine(subs);
```
<img src="https://render.githubusercontent.com/render/math?math=2\times \frac{3}{10}%2B\frac{\sin\left(\frac{3}{10}\right)}{\sin\left(\sqrt[10]{2}^{3}\right)}">

#### <a name="deri"></a>Find derivatives
```cs
var func = "x2 + ln(cos(x) + 3) + 4x";
var derivative = func.Derive("x");
Console.WriteLine(derivative.Simplify());
```
<img src="https://render.githubusercontent.com/render/math?math=4%2B\frac{\sin\left(x\right)}{{\ln\left(\cos\left(x\right)%2B3\right)}^{2}\times \left(\cos\left(x\right)%2B3\right)}%2B2\times x">

#### <a name="simp"></a>Simplify
```cs
Console.WriteLine("2x + x + 3 + (4 a * a^6) / a^3 / 5".Simplify());
```
<img src="https://render.githubusercontent.com/render/math?math=3%2B\frac{4}{5}\times {a}^{4}%2B3\times x">

```cs
var expr = "1/2 + sin(pi / 4) + (sin(3x)2 + cos(3x)2)";
Console.WriteLine(expr.Simplify());
```
<img src="https://render.githubusercontent.com/render/math?math=\frac{1}{2}\times \left(1%2B\sqrt{2}\right)%2B1">

#### <a name="bool"></a>Boolean algebra
```cs
// Those are equal
Entity expr1 = "a and b or c";
Entity expr2 = "a & b | c";

// as well as those
Entity expr3 = "a -> b";
Entity expr3 = "a implies b";
```

```cs
Entity expr = "a -> true";
Console.WriteLine(MathS.SolveBooleanTable(expr, "a"));
```

```
>>> Matrix[2 x 1]
>>> False
>>> True
```

#### <a name="late"></a>Build latex
```cs
var expr = "x ^ y + sqrt(x + y / 4)(6 / x)";
Console.WriteLine(expr.Latexise());
>>> {x}^{y}+\sqrt{x+\frac{y}{4}}\times \frac{6}{x}
```
<img src="https://render.githubusercontent.com/render/math?math={x}^{y}%2B\sqrt{x%2B\frac{y}{4}}\times \frac{6}{x}">

#### <a name="equa"></a>Solve equations analytically
```cs
Console.WriteLine("x2 + x + a".SolveEquation("x"));
```
<img src="https://render.githubusercontent.com/render/math?math=\left\{\frac{-1-\sqrt{1-4\times a}}{2},\frac{-1%2B\sqrt{1-4\times a}}{2}\right\}">

Under developing now and forever (always available)
```cs
Entity expr = "(sin(x)2 - sin(x) + a)(b - x)((-3) * x + 2 + 3 * x ^ 2 + (x + (-3)) * x ^ 3)";
Console.WriteLine(expr.SolveEquation("x").Latexise());
```
<img src="https://render.githubusercontent.com/render/math?math=\left\{-\left(-\arcsin\left(\frac{1-\sqrt{1-4\times a}}{2}\right)-2\times \pi\times n_{1}\right),-\left(-\pi--\arcsin\left(\frac{1-\sqrt{1-4\times a}}{2}\right)-2\times \pi\times n_{1}\right),-\left(-\arcsin\left(\frac{1%2B\sqrt{1-4\times a}}{2}\right)-2\times \pi\times n_{1}\right),-\left(-\pi--\arcsin\left(\frac{1%2B\sqrt{1-4\times a}}{2}\right)-2\times \pi\times n_{1}\right),\frac{-b}{-1},-i,i,1,2\right\}">

#### <a name="eqsys"></a>Solve systems of non-linear equations
Under developing now and forever (always available)

```cs
var system = MathS.Equations(
    "x2 + y + a",
    "y - 0.1x + b"
);
Console.WriteLine(system);
var solutions = system.Solve("x", "y");
Console.WriteLine(solutions);
```
System:

<img src="https://render.githubusercontent.com/render/math?math=\begin{cases}{x}^{2}%2By%2Ba = 0\\y-\frac{1}{10}\times x%2Bb = 0\\\end{cases}">

Result:

<img src="additional/readme/pic1.PNG">

```cs
var system = MathS.Equations(
    "cos(x2 + 1)^2 + 3y",
    "y * (-1) + 4cos(x2 + 1)"
);
Console.WriteLine(system.Latexise());
var solutions = system.Solve("x", "y");
Console.WriteLine(solutions);
```
<img src="https://render.githubusercontent.com/render/math?math=\begin{cases}{\cos\left({x}^{2}%2B1\right)}^{2}%2B3\times y = 0\\y\times -1%2B4\times \cos\left({x}^{2}%2B1\right) = 0\\\end{cases}">
(solution matrix is too complicated to show)

#### <a name="comp"></a>Compile functions
Compiled functions work 15x+ faster
```cs
var x = MathS.Variable("x");
var expr = MathS.Sin(x) + MathS.Sqrt(x) / (MathS.Sqrt(x) + MathS.Cos(x)) + MathS.Pow(x, 3);
var func = expr.Compile(x);
Console.WriteLine(func.Substitute(3));
```

```cs
var expr = "sin(x) + sqrt(x) / (sqrt(x) + cos(x)) + x3";
var compiled = expr.Compile("x");
Console.WriteLine(compiled.Substitute(4));
```

#### <a name="stat"></a>Solve statement
Equivalent to finding `x` such that those fit the constraints.
```cs
var set = "x2 = 16 and x > 0 or x = a".Solve("x");
Console.WriteLine(set);
>>> {4}|{a}
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

See more on [Wiki](https://github.com/asc-community/AngouriMath/wiki).


### <a name="contrib"></a>Contribution

We appreciate and welcome any contributors to AngouriMath.

Use pull requests to contribute to it. We also appreciate early pull requests so that we know what you are improving and
can help you with something.

Documentation for contributors and developers is <a href="./AngouriMath/Docs/Contributing/README.md">here</a>.
