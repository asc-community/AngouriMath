<p align="center">
  <a href="https://github.com/asc-community/AngouriMath">
    <img src="./.github/additional/readme/icon_white.png" alt="AngouriMath logo" width="200" height="200">
  </a>
</p>

<h2 align="center">AngouriMath</h2>

<p align="center">
  <i>New open-source cross-platform symbolic algebra library for </i><b>C# · F# · Jupyter · C++ (WIP)</b>
  <br>
  <a href="https://am.angouri.org/quickstart/"><b>Get started</b></a>
  <b>·</b>
  <a href="#exam"><b>Examples</b></a>
  <b>·</b>
  <a href="#contrib"><b>Contributions</b></a>
  <b>·</b>
  <a href="https://am.angouri.org/whatsnew/"><b>What's new</b></a>
  <b>·</b>
  <a href="https://am.angouri.org/"><b>Website</b></a>
  <br>
  <br>
  <a href="https://dotnetfiddle.net/u901sI"><img src="https://img.shields.io/static/v1?label=Fiddle&message=Try%21&color=purple&style=flat&logo=.NET&labelColor=646"></a>
  <a href="https://mybinder.org/v2/gh/asc-community/AngouriMathLab/try?filepath=HelloBook.AngouriMath.Interactive.ipynb"><img src="https://img.shields.io/static/v1?label=Jupyter&message=Try%21&color=purple&style=flat&logo=Jupyter&labelColor=646"></a>
  <a href="https://discord.gg/YWJEX7a"><img alt="Discord" src="https://img.shields.io/discord/642350046213439489?color=darkgreen&label=Join+our+chat!&logo=discord&style=flat&labelColor=474&logoColor=white"></a>
</p>

<details><summary><strong>Status board</strong></summary>


![Solution Build](https://github.com/asc-community/AngouriMath/actions/workflows/EverythingBuild.yml/badge.svg)

#### Builds and tests
|       | Kernel/C# | F# | Interactive | C++ |
|-------|-----------|----|-------------|-----|
| Build | ![C#/Kernel Build](https://github.com/asc-community/AngouriMath/actions/workflows/CSharpBuild.yml/badge.svg) | ![F# Build](https://github.com/asc-community/AngouriMath/actions/workflows/FSharpBuild.yml/badge.svg) | ![Interactive Build](https://github.com/asc-community/AngouriMath/actions/workflows/InteractiveBuild.yml/badge.svg) | ![C++ Build](https://github.com/asc-community/AngouriMath/actions/workflows/CPPBuild.yml/badge.svg) | 
| Test  | ![C# Test](https://github.com/asc-community/AngouriMath/actions/workflows/CSharpTest.yml/badge.svg) | ![F# Test](https://github.com/asc-community/AngouriMath/actions/workflows/FSharpTest.yml/badge.svg) | ![Interactive Test](https://github.com/asc-community/AngouriMath/actions/workflows/InteractiveTest.yml/badge.svg) | ![C++ Test](https://github.com/asc-community/AngouriMath/actions/workflows/CPPTest.yml/badge.svg) |

Note, that all tests and builds are tested for the following three operating systems: Windows, Ubuntu, Mac OS.

#### Coverage
| Kernel/C# | F# | C++ |
|-----------|----|-----|
| <a href="https://codecov.io/gh/asc-community/AngouriMath"><img src="https://codecov.io/gh/asc-community/AngouriMath/branch/master/graph/badge.svg?token=XaA0JGyNrS"/></a> | ??? | ??? |

#### Versions
|     | Kernel/C# | F# | Interactive | C++ |
|-----|-----------|----|-------------|-----|
| Prerelease | <a href="https://www.nuget.org/packages/AngouriMath"><img alt="Nuget (with prereleases)" src="https://img.shields.io/nuget/vpre/AngouriMath?color=blue&label=NuGet&logo=nuget&style=flat-square"></a> | <a href="https://www.nuget.org/packages/AngouriMath.FSharp"><img alt="Nuget (with prereleases)" src="https://img.shields.io/nuget/vpre/AngouriMath.FSharp?color=blue&label=NuGet&logo=nuget&style=flat-square"></a> | <a href="https://www.nuget.org/packages/AngouriMath.Interactive"><img alt="Nuget (with prereleases)" src="https://img.shields.io/nuget/vpre/AngouriMath.Interactive?color=blue&label=NuGet&logo=nuget&style=flat-square"></a> | <img alt="GitHub release (latest SemVer including pre-releases)" src="https://img.shields.io/github/v/release/asc-community/AngouriMathLab?include_prereleases&label=GH%20Releases"> |
| Stable | <a href="https://www.nuget.org/packages/AngouriMath"><img alt="Nuget" src="https://img.shields.io/nuget/v/AngouriMath?color=blue&label=NuGet&logo=nuget&style=flat-square"></a> | <a href="https://www.nuget.org/packages/AngouriMath.FSharp"><img alt="Nuget" src="https://img.shields.io/nuget/v/AngouriMath.FSharp?color=blue&label=NuGet&logo=nuget&style=flat-square"></a> | <a href="https://www.nuget.org/packages/AngouriMath.Interactive"><img alt="Nuget" src="https://img.shields.io/nuget/v/AngouriMath.Interactive?color=blue&label=NuGet&logo=nuget&style=flat-square"></a> | WIP |
| Downloads | <a href="https://www.nuget.org/packages/AngouriMath"><img alt="Nuget" src="https://img.shields.io/nuget/dt/AngouriMath?color=darkblue&label=Downloads&style=flat-square"></a> | <a href="https://www.nuget.org/packages/AngouriMath.FSharp"><img alt="Nuget" src="https://img.shields.io/nuget/dt/AngouriMath.FSharp?color=darkblue&label=Downloads&style=flat-square"></a> | <a href="https://www.nuget.org/packages/AngouriMath.Interactive"><img alt="Nuget" src="https://img.shields.io/nuget/dt/AngouriMath.Interactive?color=darkblue&label=Downloads&style=flat-square"></a> | WIP |

There are also latest-master versions (updated on every push to master) on [MyGet](https://www.myget.org/feed/Packages/angourimath):
| MyGet | Downloads |
|-------|-----------|
| [![MyGet (with prereleases)](https://img.shields.io/myget/angourimath/vpre/AngouriMath?label=AngouriMath)](https://www.myget.org/feed/angourimath/package/nuget/AngouriMath) | ![MyGet](https://img.shields.io/myget/angourimath/dt/AngouriMath?label=Downloads) |
| [![MyGet (with prereleases)](https://img.shields.io/myget/angourimath/vpre/AngouriMath.FSharp?label=AngouriMath.FSharp)](https://www.myget.org/feed/angourimath/package/nuget/AngouriMath.FSharp) | ![MyGet](https://img.shields.io/myget/angourimath/dt/AngouriMath.FSharp?label=Downloads) |
| [![MyGet (with prereleases)](https://img.shields.io/myget/angourimath/vpre/AngouriMath.Interactive?label=AngouriMath.Interactive)](https://www.myget.org/feed/angourimath/package/nuget/AngouriMath.Interactive) | ![MyGet](https://img.shields.io/myget/angourimath/dt/AngouriMath.Interactive?label=Downloads) |
  
Source to install from MyGet:
```
https://www.myget.org/F/angourimath/api/v3/index.json  
```
  
#### Other info
| Website | Stars | License |
|---------|-------|---------|
| <a href="https://am.angouri.org"><img alt="Website" src="https://img.shields.io/website?down_message=Down&label=Website&up_message=Up&url=https%3A%2F%2Fam.angouri.org&style=flat-square"></a> | <img alt="GitHub Repo stars" src="https://img.shields.io/github/stars/asc-community/AngouriMath?label=Stars&style=flat-square"> | <img alt="GitHub" src="https://img.shields.io/github/license/asc-community/AngouriMath?color=dark-green&label=License&style=flat-square"> |
</details>

## What is it about?

<a href="#jupyter"><img src="./.github/additional/readme/side.PNG" align="right" width="25%" alt="AngouriMath now supports Jupyter integration"/></a>

<a href="https://am.angouri.org">AngouriMath</a> is an open source symbolic algebra library.
That is, via AngouriMath, you can
automatically <a href="https://am.angouri.org/wiki/07.-Solvers.html">solve</a> 
equations, systems of equations, work with sets,
<a href="https://am.angouri.org/wiki/05.-Differentiation.html">differentiate</a>,
<a href="https://am.angouri.org/wiki/01.-Expressions.html">parse</a> from string,
<a href="https://am.angouri.org/wiki/09.-Compilation.html">compile expressions</a>, work
with <a href="https://am.angouri.org/wiki/10.-Matrices.html">matrices</a>, find limits,
convert an expression to LaTeX, and <a href="https://am.angouri.org/wiki/">many other things</a>.

It is not a CAS, so you can use it in any your project by installing it from 
<a href="https://www.nuget.org/packages/AngouriMath">NuGet</a>. AngouriMath
can be used in calculators, algebra systems, educational/quiz apps, graphics,
TeX rendering applications, etc.

It is free to use even in commercial projects. We work on it a lot, so your requests on 
<a href="https://github.com/asc-community/AngouriMath/issues">issues</a> are likely to
be considered within a few hours.

## Quick start
1. Install AngouriMath from [NuGet](https://www.nuget.org/packages/AngouriMath).
2. Write the following code:
```cs
using AngouriMath; using System;
Entity expr = "x + sin(y)";
Console.WriteLine(expr);
```
3. Run.

<a href="https://am.angouri.org/quickstart/"><strong>More detailed Quick Start</strong></a>.

If you are new to AM, we suggest you checking out some samples instead of reading boring 
documentation. If you want to contribute, we would be happy to welcome you in our
community.

For any questions, feel free to contact us via <a href="https://discord.gg/YWJEX7a">Discord</a>.

Official website: [am.angouri.org](https://am.angouri.org/).

<a id="exam"></a>

## Examples

Expand any section to see. Examples with live shell are on the [website](https://am.angouri.org/).

<details><summary><strong>Computations</strong></summary>

Use as a simple calculator:
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

Build expressions with variables and substitute them:
```cs
Entity expr = "2x + sin(x) / sin(2 ^ x)";
var subs = expr.Substitute("x", 0.3m);
Console.WriteLine(subs);
```
<img src="https://render.githubusercontent.com/render/math?math=2\times \frac{3}{10}%2B\frac{\sin\left(\frac{3}{10}\right)}{\sin\left(\sqrt[10]{2}^{3}\right)}">

Simplify complicated expressions:
```cs
Console.WriteLine("2x + x + 3 + (4 a * a^6) / a^3 / 5".Simplify());
```
<img src="https://render.githubusercontent.com/render/math?math=3%2B\frac{4}{5}\times {a}^{4}%2B3\times x">

```cs
var expr = "1/2 + sin(pi / 4) + (sin(3x)2 + cos(3x)2)";
Console.WriteLine(expr.Simplify());
```
<img src="https://render.githubusercontent.com/render/math?math=\frac{1}{2}\times \left(1%2B\sqrt{2}\right)%2B1">

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

</details>

<details><summary><strong>Algebra</strong></summary>

Start with boolean algebra:
```cs
Entity expr1 = "a and b or c";

// Those are the same
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

Next, solve some equations:
```cs
Console.WriteLine("x^2 + x + a".SolveEquation("x"));
```
<img src="https://render.githubusercontent.com/render/math?math=\left\{\frac{-1-\sqrt{1-4\times a}}{2},\frac{-1%2B\sqrt{1-4\times a}}{2}\right\}">

Under developing now and forever (always available)
```cs
Entity expr = "(sin(x)^2 - sin(x) + a)(b - x)((-3) * x + 2 + 3 * x ^ 2 + (x + (-3)) * x ^ 3)";
Console.WriteLine(expr.SolveEquation("x").Latexise());
```
<img src="https://render.githubusercontent.com/render/math?math=\left\{-\left(-\arcsin\left(\frac{1-\sqrt{1-4\times a}}{2}\right)-2\times \pi\times n_{1}\right),-\left(-\pi--\arcsin\left(\frac{1-\sqrt{1-4\times a}}{2}\right)-2\times \pi\times n_{1}\right),-\left(-\arcsin\left(\frac{1%2B\sqrt{1-4\times a}}{2}\right)-2\times \pi\times n_{1}\right),-\left(-\pi--\arcsin\left(\frac{1%2B\sqrt{1-4\times a}}{2}\right)-2\times \pi\times n_{1}\right),\frac{-b}{-1},-i,i,1,2\right\}">

Try some inequalities:
```cs
Console.WriteLine("(x - 6)(x + 9) >= 0".Solve("x"));
```
<img src="https://render.githubusercontent.com/render/math?math=\left\{-9,6\right\}\cup\left(-\infty%3B-9\right)\cup\left(6%3B\infty\right)">

Systems of equations:
```cs
var system = MathS.Equations(
    "x^2 + y + a",
    "y - 0.1x + b"
);
Console.WriteLine(system);
var solutions = system.Solve("x", "y");
Console.WriteLine(solutions);
```
System:

<img src="https://render.githubusercontent.com/render/math?math=\begin{cases}{x}^{2}%2By%2Ba = 0\\y-\frac{1}{10}\times x%2Bb = 0\\\end{cases}">

Result:

<img src="./.github/additional/readme/pic1.PNG">

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

</details>

<details><summary><strong>Calculus</strong></summary>

Find derivatives:
```cs
Entity func = "x^2 + ln(cos(x) + 3) + 4x";
Entity derivative = func.Differentiate("x");
Console.WriteLine(derivative.Simplify());
```
<img src="https://render.githubusercontent.com/render/math?math=4%2B\frac{\sin\left(x\right)}{{\ln\left(\cos\left(x\right)%2B3\right)}^{2}\times \left(\cos\left(x\right)%2B3\right)}%2B2\times x">

Find limits:
```cs
WriteLine("(a x^2 + b x) / (e x - h x^2 - 3)".Limit("x", "+oo").InnerSimplified);
```
<img src="https://render.githubusercontent.com/render/math?math=\frac{a}{-h}">

Find integrals:
```cs
WriteLine("x^2 + a x".Integrate("x").InnerSimplified);
```
<img src="https://render.githubusercontent.com/render/math?math=\frac{{x}^{3}}{3}%2Ba\times \frac{{x}^{2}}{2}">

</details>

<details><summary><strong>Sets</strong></summary>

There are four types of sets:
```cs
WriteLine("{ 1, 2 }".Latexise());
WriteLine("[3; +oo)".Latexise());
WriteLine("RR".Latexise());
WriteLine("{ x : x^8 + a x < 0 }".Latexise());
```

<img src="https://render.githubusercontent.com/render/math?math=\left\{ 1, 2 \right\}">
<img src="https://render.githubusercontent.com/render/math?math=\left[3%3B \infty \right)">
<img src="https://render.githubusercontent.com/render/math?math=\mathbb{R}">
<img src="https://render.githubusercontent.com/render/math?math=\left\{ x %3A {x}^{8}%2B a\times x < 0 \right\}">

And there operators:
```cs
WriteLine(@"A \/ B".Latexise());
WriteLine(@"A /\ B".Latexise());
WriteLine(@"A \ B".Latexise());
```

<img src="https://render.githubusercontent.com/render/math?math=A\cup B">
<img src="https://render.githubusercontent.com/render/math?math=A\cap B">
<img src="https://render.githubusercontent.com/render/math?math=A\setminus B">

</details>

<details><summary><strong>Syntax</strong></summary>

You can build LaTeX with AngouriMath:
```cs
var expr = "x ^ y + sqrt(x) + integral(sqrt(x) / a, x, 1) + derive(sqrt(x) / a, x, 1) + limit(sqrt(x) / a, x, +oo)";
Console.WriteLine(expr.Latexise());
>>> {x}^{y}+\sqrt{x}+\int \left[\frac{\sqrt{x}}{a}\right] dx+\frac{d\left[\frac{\sqrt{x}}{a}\right]}{dx}+\lim_{x\to \infty } \left[\frac{\sqrt{x}}{a}\right]
```
<img src="https://render.githubusercontent.com/render/math?math={x}^{y}%2B\sqrt{x}%2B\int\left[\frac{\sqrt{x}}{a}\right]dx%2B\frac{d\left[\frac{\sqrt{x}}{a}\right]}{dx}%2B\lim_{x\to\infty}\left[\frac{\sqrt{x}}{a}\right]">

You can parse `Entity` from string with
```cs
var expr = MathS.FromString("x + 2 + sqrt(x)");
Entity expr = "x + 2 + sqrt(x)";
```

A few convenient features: `x2` => `x^2`, `a x` => `a * x`, `(...)2` => `(...)^2`, `2(...)` => `2 * (...)`

</details>

<details><summary><strong>Compilation</strong></summary>

Now you can compile expressions with pritimives into native lambdas. They will be
at least as fast as if you wrote them in line in code, or faster if you have
same subexpressions in your expression.

```cs
Entity expr = "a and x > 3";
var func = expr.Compile<bool, double, bool>("a", "x");
WriteLine(func(true, 6));
WriteLine(func(false, 6));
WriteLine(func(true, 2));
WriteLine(func(false, 2));
```

Output:

```
True
False
False
False
```

</details>

<details><summary><strong>F#</strong></summary>

<a href="https://www.nuget.org/packages/AngouriMath.FSharp">Download</a>

Not everything is supported directly from F#, so if something missing, you will need
to call the necessary methods from AngouriMath.

```fs
open Functions
open Operators
open Shortcuts

printfn "%O" (solutions "x" "x + 2 = 0")

printfn "%O" (simplified (solutions "x" "x^2 + 2 a x + a^2 = 0"))

printfn "%O" (``dy/dx`` "x^2 + a x")

printfn "%O" (integral "x" "x2 + e")

printfn "%O" (``lim x->0`` "sin(a x) / x")

printfn "%O" (latex "x / e + alpha + sqrt(x) + integral(y + 3, y, 1)")

```

</details>

<details><summary><strong>C++ (Experimental)</strong></summary>

At the moment, AngouriMath.CPP is in the experimental phase. See <a href="https://am.angouri.org/quickstart/#cpp">how to get AngouriMath for C++</a>.
```cpp
#include <AngouriMath.h>

int main()
{
    AngouriMath::Entity expr = "x y + 2";
    std::cout << expr.Differentiate("x");
}
```

</details>

<details><summary><strong>Jupyter</strong></summary>

If you already installed <a href="https://jupyter.org/install">Jupyter</a> and 
<a href="https://github.com/dotnet/interactive">Interactive</a> for it, install
<a href="https://www.nuget.org/packages/AngouriMath.Interactive">package</a> by copying this to your first cell:

```cs
#r "nuget:AngouriMath.Interactive, *-*"
```

Now any `ILatexiseable` will be displayed as LaTeX. Try example
<a href="https://mybinder.org/v2/gh/asc-community/Try/main?filepath=HelloBook.AngouriMath.Interactive.ipynb">here</a>, or 
<a href="https://nbviewer.jupyter.org/github/asc-community/Try/blob/main/notebooks/HelloBook.AngouriMath.Interactive.ipynb">view</a> it

Check the <a href="Sources/Samples/Interactive.Sample.ipynb">F#</a> and 
<a href="Sources/Samples/CSharp.Interactive.Sample.ipynb">C#</a> samples.

</details>

<details><summary><strong>Multithreading</strong></summary>

You are guaranteed that all functions in AM run in one thread. It is also guaranteed that you can safely run multiple 
functions from AM in different threads, that is, all static variables and lazy properties are thread-safe.

There is also support of cancellation a task. However, to avoid injecting the cancellation token argument into all methods,
we use `AsyncLocal<T>` instead. That is why instead of passing your token to all methods what you need is to pass it once
to the `MathS.Multithreading.SetLocalCancellationToken(CancellationToken)` method.

There is a sample code demonstrating cancellation:

```cs
var cancellationTokenSource = new CancellationTokenSource();

// That goes instead of passing your token to methods
MathS.Multithreading.SetLocalCancellationToken(cancellationTokenSource.Token);

// Then you normally run your task
var currTask = Task.Run(() => InputText.Text.Solve("x"), cancellationTokenSource.Token);

try
{
    await currTask;
    LabelState.Text = currTask.Result.ToString();
}
catch (OperationCanceledException)
{
    LabelState.Text = "Operation canceled";
}
```

</details>

If you want, you can add a badge to your repo:
```
[![Powered by AngouriMath](https://img.shields.io/badge/Powered%20by-AngouriMath-purple?style=flat-square&labelColor=646)](https://am.angouri.org)
```
[![Powered by AngouriMath](https://img.shields.io/badge/Powered%20by-AngouriMath-purple?style=flat-square&labelColor=646)](https://am.angouri.org)

## <a name="contrib"></a>Contribution

We appreciate and welcome any contributors to AngouriMath. Current tasks can be tracked
on <a href="https://github.com/asc-community/AngouriMath/projects">this page</a>.

Use pull requests to contribute to it. We also appreciate early pull requests so that we know what you are improving and
can help you with something.

Documentation for contributors and developers is <a href="./Sources/AngouriMath/Docs/Contributing/README.md">here</a>.

## <a name="license"></a>License

<a href="./LICENSE.md"><img alt="GitHub" src="https://img.shields.io/github/license/asc-community/AngouriMath?color=purple&label=License&style=flat-square"></a>

The project is open source, but can be used in closed commercial projects. There is no restriction on it
with the only requirement to keep the MIT license with all distributives of AngouriMath.
