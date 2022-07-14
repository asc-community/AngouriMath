//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath;
using static AngouriMath.MathS;

void Method()
{
    Console.WriteLine("---------------------");
    Entity a = 5.5m;
    Console.WriteLine(a);
    Entity b = 5.462m;
    Console.WriteLine(b);
    Entity c = 5.0m / 7.0m;
    Console.WriteLine(c);
}

Settings.FloatToRationalIterCount.As(1, Method);
Settings.FloatToRationalIterCount.As(3, Method);
Settings.FloatToRationalIterCount.As(5, Method);
Settings.FloatToRationalIterCount.As(10, Method);