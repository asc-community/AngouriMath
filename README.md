[![Codacy Badge](https://api.codacy.com/project/badge/Grade/1e172cdf699645b59567032dd1ae5cab)](https://www.codacy.com/manual/Angourisoft/MathS?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=Angourisoft/MathS&amp;utm_campaign=Badge_Grade)

Nuget: https://www.nuget.org/packages/MathSharp

### Math#
Open-source Math engine for ASC

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

#### Solve eqations
```cs
var x = MathS.Var("x");
var equation = (x - 1) * (x - 2) * (MathS.Sqr(x) + 1);
foreach (var re in equation.SolveNt(x))
    Console.Write(re.ToString() + "  ");
>>> 1  2  1i
```
