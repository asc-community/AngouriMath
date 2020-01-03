[![Codacy Badge](https://api.codacy.com/project/badge/Grade/1e172cdf699645b59567032dd1ae5cab)](https://www.codacy.com/manual/Angourisoft/MathS?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=Angourisoft/MathS&amp;utm_campaign=Badge_Grade)
![Nuget](https://img.shields.io/nuget/dt/AngouriMath?color=blue&logo=NuGet)

Nuget: https://www.nuget.org/packages/AngouriMath

## AngouriMath
AngouriMath is an open-source library that enables to work with non-linear multi-variable expressions. Its functionality includes derivation, variable substitution, equation solving, definite integration, formula-to-latex formatting, and some more.

### Examples

#### Use as a simple calculator
```cs
var inp = "1 + 2 * log(9, 3)";
var expr = MathS.FromString(inp);
Console.WriteLine(expr.Eval());
>>> 5
```

#### Build an expression
```cs
var x = MathS.Var("x");
var y = MathS.Var("y");
var c = x * y + x / y;
Console.WriteLine(MathS.Sqr(c));
>>> (x * y + x / y) ^ 2
```

#### Substitute variables
```cs
var x = MathS.Var("x");
var expr = x * 2 + MathS.Sin(x) / MathS.Sin(MathS.Pow(2, x));
var subs = expr.Substitute(x, 0.3);
Console.WriteLine(subs.Simplify());
>>> 0,9134260185941638
```

#### Find derivatives
```cs
var x = MathS.Var("x");
var func = MathS.Sqr(x) + MathS.Ln(MathS.Cos(x) + 3) + 4 * x;
var derivative = func.Derive(x);
Console.WriteLine(derivative.Simplify());
>>> 2 * x + -1 * sin(x) / (cos(x) + 3) + 4
```

#### Build formulas
```cs
var x = MathS.Var("x");
var expr = (x + 3).Pow(x + 4);
Func<NumberEntity, Entity> wow = v => expr.Substitute(x, v).Simplify();
Console.WriteLine(wow(4));
Console.WriteLine(wow(5));
Console.WriteLine(wow(6));
>>> 5764801
>>> 134217728
>>> 3486784401
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

#### Solve equations
Only numerical solutions with Newton's method is supported yet :(
```cs
var x = MathS.Var("x");
var equation = (x - 1) * (x - 2) * (MathS.Sqr(x) + 1);
foreach (var re in equation.SolveNt(x))
    Console.Write(re.ToString() + "  ");
>>> 1  2  1i
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
Compiled functions work 15x faster
```cs
var x = MathS.Var("x");
var expr = MathS.Sin(x) + MathS.Sqrt(x) / (MathS.Sqrt(x) + MathS.Cos(x)) + MathS.Pow(x, 3);
var func = expr.Compile(x);
Console.WriteLine(func.Substitute(3));
```

#### Simplify
```cs
var x = MathS.Var("x");
var a = MathS.Var("a");
var b = MathS.Var("b");
var expr = MathS.Sqrt(x) / x + a * b + b * a + (b - x) * (x + b) + 
    MathS.Arcsin(x + a) + MathS.Arccos(a + x);
Console.WriteLine(expr.SimplifyIntelli(6));
>>> 1.5707963267948966 + 2 * a * b + b ^ 2 + x ^ (-0.5) - x ^ 2
```

#### Try new syntax
```cs
var expr = MathS.FromString("3x3 + 2 2 2 - x(3 0.5)");
Console.WriteLine(expr);
>>> 3 * x ^ 3 + 2 ^ 2 ^ 2 - x * sqrt(3)
```

## Full documentation

### Entity methods

#### Derivation
expr.Derive(x) - derivation for variable x.
```cs
var x = MathS.Var("x");
var expr = MathS.Sqr(x);
Console.WriteLine(expr.Derive(x)); // 2 * x
```
How it works? We have some rules for derivation which are applied to each node, for example (a + b)' = a' + b'. So, we replace each node according to the appropriate rule.

#### Evaluation & Simplification
```expr.Simplify(level)``` simplifies an expression. Level is number of iterations (relevant for long expressions).

```expr.Eval() = expr.Simplify(1)``` - recommended to use to evaluation substituted expression.

```expr.Simplify() = expr.Simplify(2)``` - use to simplify expressions, a * x + x = (a + 1) * x
```cs
var x = MathS.Var("x");
var expr = 3 * x + x;
Console.WriteLine(expr.Simplify()); // 4 * x
```
How it works? Thanks to the pattern system, now we are able to find subtrees that we know how to simplify. The full list of used patterns presents in file Patterns.cs.

#### Expansion & Collapse
```expr.Expand(level=2)``` - expands the expression trying to remove all the braces (for example, a * (1 + x) = a * x + a * 1). level - number of iterations.

```expr.Collapse(level=2)``` - collapses the expression trying to remove all the powers (for example, x^2 - y^2 = (x - y) * (x + y) ).

#### To string
```expr.ToString()``` - string presentation of an expression.

```expr.Latexise()``` - neat output in LaTeX format.

How it works? For each node we encounter, we use the appropriate latex syntax.

#### Solving equations
By this time, only Newton's method over one variable is available.

expr.SolveNt(from, to, stepCount, precision) - find roots assuming we are solving equation expr=0.

The algorithm iterates on [from.Re; to.Re] for real part and on [from.Im; to.Im] for imaginary part.

The higher stepCount is, the more roots the function can find

Precision - if you get similar roots that you think are equal, you can increase this argument.

You can also decrease MathS.EQUALITY_THRESHOLD which is responsible for comparing Numbers.

#### Integration
By this time, only definite integration over one variable is available.

```expr.DefiniteIntegral(x, from, to)``` - numerically counts integral from ```from``` to ```to```. Note that you can specify the two parameters in Complex numbers.

#### Compilation
```expr.Compile(a, b, c...)``` the arguments are arguments of the target function. You should list all the used variables in the order you will then call.

```fe.Call(a, b, c...)``` the arguments are Numbers in the order of variables. Retunrs Number.
```
var x = MathS.Var("x");
var expr = MathS.Sqr(x) + 2 * x;
var func = expr.Compile(x);
Console.WriteLine(func.Call(3));
```
Performane improved a lot. Testing on i7-7700HQ and expr=MathS.Sin(x) we get the following report:

| Function                             | Time per iteration              |
| ------------------------------------ | ------------------------------- |
| Substitute(x, 3).Eval() from 1.0.13  | 12000 ns                        |
| Substitute(x, 3).Eval() from 1.0.15  | 2500 ns                         |
| Call(3) from 1.0.15                  | 54 ns                           |
| Complex.Sin(3)                       | 27 ns                           |

If we take expr=(MathS.Log(x, 3) + MathS.Sqr(x)) * MathS.Sin(x + MathS.Cosec(x)), we get the following performance

| Method             | Time per iteration |
|--------------------|-------------------:|
| AM Compiled        | 350.767 ns         |
| In-code expression | 211.472 ns         |

So, for most cases using compilation will save you enough time even though Complex.Sin is still faster.

#### Function list

MathS.
Log(num, base), Pow(base, power), Sqrt(x), Sqr(x), Sin(x), Cos(x), Tan(x), Cotan(x), 
Sec(x), Cosec(x), Arcsin(x), Arccos(x), Arctan(x), Arccotan(x), B(x), TB(x)

MathS.FromString(str) - returns Entity

MathS.FromLinq(expr) - returns Entity
