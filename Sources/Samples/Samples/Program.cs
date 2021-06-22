using System;
using System.Collections.Generic;
using AngouriMath;
using static AngouriMath.Entity.Number;

Entity e = "x^2 + 3 = 0";

var solutions = e.Solve("x");
foreach(var solution in solutions.DirectChildren)
{
    Console.WriteLine(solution);
}    