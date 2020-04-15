[![Codacy Badge](https://api.codacy.com/project/badge/Grade/1e172cdf699645b59567032dd1ae5cab)](https://www.codacy.com/manual/Angourisoft/MathS?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=Angourisoft/MathS&amp;utm_campaign=Badge_Grade)
![Nuget](https://img.shields.io/nuget/dt/AngouriMath?color=blue&label=NuGet%20installs&logoColor=blue)
![GitHub](https://img.shields.io/github/license/AngouriSoft/MathS?color=purple)


Nuget: https://www.nuget.org/packages/AngouriMath
## AngouriMath
AngouriMath is an open-source library that enables to work with non-linear 
multi-variable expressions. Its functionality includes derivation, 
variable substitution, equation solving, equation system solving, definite integration, 
formula-to-latex formatting, and some more.

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
var inp = "1 + 2 * log(9, 3)";
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
>>> 0,9134260185941638
```

#### Find derivatives
```cs
var x = MathS.Var("x");
var func = MathS.Sqr(x) + MathS.Ln(MathS.Cos(x) + 3) + 4 * x;
var derivative = func.Derive(x);
Console.WriteLine(derivative.Eval());
>>> 2 * x + -1 * sin(x) / (cos(x) + 3) + 4
```

#### Build expressions faster
```cs
Entity expr = "sqr(x + y)";
Console.WriteLine(expr.Expand().Simplify());
>>> 2 * x * y + x ^ 2 + y ^ 2
```

#### Simplify
```cs
var x = MathS.Var("x");
var a = MathS.Var("a");
var b = MathS.Var("b");
var expr = MathS.Sqrt(x) / x + a * b + b * a + (b - x) * (x + b) + 
    MathS.Arcsin(x + a) + MathS.Arccos(a + x);
Console.WriteLine(expr.Simplify());
>>> 1.5707963267948966 + 2 * a * b + b ^ 2 + x ^ (-0.5) - x ^ 2
```

#### Render latex
```cs
var x = MathS.Var("x");
var y = MathS.Var("y");
var expr = x.Pow(y) + MathS.Sqrt(x + y / 4) * (6 / x);
Console.WriteLine(expr.Latexise());
>>> {x}^{y}+\sqrt{x+\frac{y}{4}}*\frac{6}{x}
```

#### Play with complex numbers
```cs
var expr = MathS.Pow(MathS.e, MathS.pi * MathS.i);
Console.WriteLine(expr);
Console.WriteLine(expr.Eval());
>>> 2,718281828459045 ^ 3,141592653589793i
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
Console.WriteLine(expr2.DefiniteIntegral(x, 0, MathS.pi));
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
var expr = MathS.FromString("3x3 + 2 2 2 - x(3 0.5)");
Console.WriteLine(expr);
>>> 3 * x ^ 3 + 2 ^ 2 ^ 2 - x * sqrt(3)
```

#### Try SymPy syntax
```cs
var x = SySyn.Symbol("x");
var expr = SySyn.Exp(x) + x;
Console.WriteLine(SySyn.Diff(expr));
Console.WriteLine(SySyn.Diff(expr, x));
Console.WriteLine(SySyn.Diff(expr, x, x));
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

## Full documentation (not updated since January)

### Entity methods

#### Derivation
expr.Derive(x) - derivation for variable x.
```cs
var x = MathS.Var("x");
var expr = MathS.Sqr(x);
Console.WriteLine(expr.Derive(x)); // 2 * x
```
How it works? We have some rules for derivation which are applied to each node, 
for example (a + b)' = a' + b'. So, we replace each node according to the 
appropriate rule.

#### Evaluation & Simplification
```expr.Simplify(level)``` simplifies an expression. Level is number of 
iterations (relevant for long expressions).

```expr.Eval() = expr.Simplify(1)``` - recommended to use to evaluation 
substituted expression.

```expr.Simplify() = expr.Simplify(2)``` - use to simplify expressions, 
a * x + x = (a + 1) * x
```cs
var x = MathS.Var("x");
var expr = 3 * x + x;
Console.WriteLine(expr.Simplify()); // 4 * x
```
How it works? Thanks to the pattern system, now we are able to find subtrees that 
we know how to simplify. The full list of used patterns 
presents in file Patterns.cs.

#### Expansion & Collapse
```expr.Expand(level=2)``` - expands the expression trying to remove all the braces
(for example, a * (1 + x) = a * x + a * 1). level - number of iterations.

```expr.Collapse(level=2)``` - collapses the expression trying to remove all the
powers (for example, x^2 - y^2 = (x - y) * (x + y) ).

#### To string
```expr.ToString()``` - string presentation of an expression.

```expr.Latexise()``` - neat output in LaTeX format.

How it works? For each node we encounter, we use the appropriate latex syntax.

#### Solving equations
By this time, only Newton's method over one variable is available. However, 
we will soon release version with analytical solver

expr.SolveNt(from, to, stepCount, precision) - find roots assuming we are 
solving equation expr=0.

The algorithm iterates on [from.Re; to.Re] for real part and on [from.Im; to.Im] 
for imaginary part.

The higher stepCount is, the more roots the function can find

Precision - if you get similar roots that you think are equal, you can increase 
this argument.

You can also decrease MathS.EQUALITY_THRESHOLD which is responsible for comparing 
Numbers.

#### Integration
By this time, only definite integration over one variable is available.

```expr.DefiniteIntegral(x, from, to)``` - numerically counts integral from ```from``` to ```to```. Note that you can specify the two parameters in Complex numbers.

#### Compilation
```expr.Compile(a, b, c...)``` the arguments are arguments of the target 
function. You should list all the used variables in the order you will then call.

```fe.Call(a, b, c...)``` the arguments are Numbers in the order of variables. 
Returns Number.
```
var x = MathS.Var("x");
var expr = MathS.Sqr(x) + 2 * x;
var func = expr.Compile(x);
Console.WriteLine(func.Call(3));
```

#### Function list

MathS.
Log(num, base), Pow(base, power), Sqrt(x), Sqr(x), Sin(x), Cos(x), Tan(x), Cotan(x), 
Sec(x), Cosec(x), Arcsin(x), Arccos(x), Arctan(x), Arccotan(x), B(x), TB(x)

MathS.FromString(str) - returns Entity

MathS.FromLinq(expr) - returns Entity

#### SymPy syntax

SymPy is a very well-known library, so to make ours more convenient for people
experienced in SymPy, we made the same syntax. Start functions with SySyn:

```
SySyn.Symbol(string)
SySyn.Diff(Entity, x, x...)
SySyn.Simplify(Entity)
SySyn.Solve(Entity, VariableEntity)
SySyn.Expand(Entity)
SySyn.Evalf(Entity)
SySyn.Latex(Entity)
SySyn.Exp(Entity)
```

More are coming soon...

#### How does it work?

You can learn how some methods work here: https://habr.com/ru/post/486496/<br> 

Full description is not ready yet.

#### I know how to improve it

As the library is completely open-source and will be kept open-source, we welcome
contributors to the project. What you need to do is to clone the repo to your local
machine, change whatever you want, and send a pull request. If your changes are
relevant, we will merge it for sure. :)

