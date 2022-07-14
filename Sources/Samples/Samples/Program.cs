//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;

using System;
using static AngouriMath.MathS.Hyperbolic;

Console.WriteLine(Arsinh("x"));
Console.WriteLine(Arsinh(Sinh("x")).Substitute("x", 10).Evaled);
Console.WriteLine("----------------------");
Console.WriteLine(Arcosh("x"));
Console.WriteLine(Arcosh(Cosh("x")).Substitute("x", 10).Evaled);
Console.WriteLine("----------------------");
Console.WriteLine(Artanh("x"));
Console.WriteLine(Artanh(Tanh("x")).Substitute("x", 10).Evaled);
Console.WriteLine("----------------------");
Console.WriteLine(Arcotanh("x"));
Console.WriteLine(Arcotanh(Cotanh("x")).Substitute("x", 10).Evaled);
Console.WriteLine("----------------------");
Console.WriteLine(Arsech("x"));
Console.WriteLine(Arsech(Sech("x")).Substitute("x", 10).Evaled);
Console.WriteLine("----------------------");
Console.WriteLine(Arcosech("x"));
Console.WriteLine(Arcosech(Cosech("x")).Substitute("x", 10).Evaled);
Console.WriteLine("----------------------");
