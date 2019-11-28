# Math#
Open-source Math engine for ASC

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
<a href="https://www.codecogs.com/eqnedit.php?latex={x}^{y}&plus;\sqrt{x&plus;\frac{y}{4}}*\frac{6}{x}" target="_blank"><img src="https://latex.codecogs.com/gif.latex?{x}^{y}&plus;\sqrt{x&plus;\frac{y}{4}}*\frac{6}{x}" title="{x}^{y}+\sqrt{x+\frac{y}{4}}*\frac{6}{x}" /></a>

Other features like plot are coming soon...
