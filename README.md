# Math#
Open-source Math engine for ASC

#### Build an expression
```cs
var x = MathS.Var("x");
var y = MathS.Var("y");
var c = x * y + x / y
Console.WriteLine(c ^ 2);
```

#### Substitute variables
```cs
var x = MathS.Var("x");
var expr = x * 2 + MathS.Sin(x) / MathS.Sin(2 ^ x);
var subs = expr.Substitute(x, 0.3);
Console.WriteLine(subs.Simplify());
```

#### Find derivatives
```cs
var x = MathS.Var("x");
var func = (x ^ 2) + MathS.Ln(Math.Cos(x) + 3) + 4 * x;
var derivative = func.Derive(x);
Console.WriteLine(derivative.Simplify());
```

#### Build formulas
```cs
var x = MathS.Var("x");
var expr = (x + 3) ^ (x + 4)l
Func<NumberEntity, Entity> wow = v => expr.Substitute(x, v).Simplify();
Console.WriteLine(wow(4));
Console.WriteLine(wow(5));
Console.WriteLine(wow(6));
```

Other features like plot are coming soon...
