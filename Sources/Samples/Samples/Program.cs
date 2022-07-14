//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;

using System;
using static AngouriMath.MathS.Hyperbolic;

Console.WriteLine(Tanh("x"));
Console.WriteLine(Tanh("x").Substitute("x", 1.5).Evaled);
Console.WriteLine(Tanh("x").Substitute("x", 0.01).Evaled);

Console.WriteLine(Cotanh("x"));
Console.WriteLine(Cotanh("x").Substitute("x", 1.5).Evaled);
Console.WriteLine(Cotanh("x").Substitute("x", 0.01).Evaled);