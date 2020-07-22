[![Codacy Badge](https://api.codacy.com/project/badge/Grade/1e172cdf699645b59567032dd1ae5cab)](https://www.codacy.com/manual/Angourisoft/MathS?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=Angourisoft/MathS&amp;utm_campaign=Badge_Grade)
![Test](https://github.com/asc-community/AngouriMath/workflows/Test/badge.svg)
![Nuget](https://img.shields.io/nuget/dt/AngouriMath?color=blue&label=NuGet%20installs&logoColor=blue)
![GitHub](https://img.shields.io/github/license/AngouriSoft/MathS?color=purple)


[NuGet](https://www.nuget.org/packages/AngouriMath "Link to .NET package repository")

## AngouriMath
AngouriMath is a cross-platform open-source library that enables to work with non-linear 
multi-variable expressions. Written in C#.

Reach the main contributor on discord: Oryp4ik#0120. 

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
Console.WriteLine(expr.Eval());
```
<img src="https://render.githubusercontent.com/render/math?math=5">

```cs
Console.WriteLine("2 / 3 + sqrt(-16)".Eval());
>>> 2 / 3 + 4i
```
<img src="https://render.githubusercontent.com/render/math?math=\frac{2}{3} %2B 4i">

```cs
Console.WriteLine("(-2) ^ 3".Eval());
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
var x = MathS.Var("x");
var expr = MathS.Sin(x) + MathS.Sqrt(x) / (MathS.Sqrt(x) + MathS.Cos(x)) + MathS.Pow(x, 3);
var func = expr.Compile(x);
Console.WriteLine(func.Substitute(3));
```

```cs
var expr = "sin(x) + sqrt(x) / (sqrt(x) + cos(x)) + x3";
var compiled = expr.Compile("x");
Console.WriteLine(compiled.Substitute(4));
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

Use pull requests to contribute to it. If you want to regenerate the parser, follow these steps:
1. Change files from ./AngouriMath/Core/FromString/Antlr/ with the extensions of ".interp", ".tokens", ".g"
2. Assure you have jre on your machine
3. Run `start ./AngouriMath/antlr_rerun.bat` or `./AngouriMath/antlr_rerun.bat` to regenerate the parser via ANTLR