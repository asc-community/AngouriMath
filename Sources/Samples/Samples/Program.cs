using System;
using AngouriMath;
using AngouriMath.Extensions;
using static AngouriMath.MathS;
using static AngouriMath.Entity;
using System.Numerics;


// Imagine this API

var H = MathS.Matrix(new Entity[,] { { 1, 1 }, { 1, -1 } }) * "sqrt(2) / 2";
var q = MathS.Vector(1, 0);
var res = H * q;
Console.WriteLine();