using System;
using AngouriMath;
using AngouriMath.Extensions;
using static AngouriMath.MathS;
using static AngouriMath.Entity;
using System.Numerics;


// Imagine this API

var H = MathS.Matrices.Matrix(new Entity[,] { { 1, 0 }, { 0, "x" } });
var q = MathS.Matrices.Vector(1, 3);
Console.WriteLine(H);
Console.WriteLine(H * q);
Console.WriteLine(q * q.T);
Console.WriteLine(H.Pow(31).Simplify());
