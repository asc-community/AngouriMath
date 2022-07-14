//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using HonkSharp.Fluency;
using System;
using static AngouriMath.Entity;
using AngouriMath.Extensions;
using GenericTensor.Core;
using static AngouriMath.Entity.Number;
using static AngouriMath.MathS;
using static System.Console;
using PeterO.Numbers;


// var system = Equations(
// "(x2 + y) / 2",
// "y - x - 3"
// );
// Console.WriteLine(system.Solve("x", "y"));

var a = Var("A");
var b = Var("B");
var c = Var("C");
var d = Var("D");
var f = (!a & b) | (!b & a);

WriteLine(f.Simplify());

Console.WriteLine(@"
apply(
    lambda(x, y, apply(x, y)),
    y)
".Simplify());