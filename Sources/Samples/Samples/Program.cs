using System;
using AngouriMath;
using AngouriMath.Extensions;
using static AngouriMath.MathS;
using static AngouriMath.Entity;
using System.Numerics;


// Imagine this API

var H = Matrix(new Entity[,]
    {
        { "1", "2", "6" },
        { "3", "2", "9" },
        { "1", "1", "9" },
    });
// Console.WriteLine((H.Adjugate / H.Determinant).Evaled);
// Console.WriteLine(H.ComputeInverse());
Console.WriteLine(H.MainDiagonal);
// Console.WriteLine(Det(H - "x" * I_3).Simplify());
/*
Console.WriteLine(H);
Console.WriteLine(H * q);
Console.WriteLine(q.T * q);
Console.WriteLine(q * q.T);
Console.WriteLine(H.Pow(31).Simplify());

// Console.WriteLine(MathS.Series.TaylorExpansion("e^x", "x", "x", "0", 10));

var m1 = Matrices.Matrix(new Entity[,]
    {
        { 1, 3 },
        { 3, 4 }
    });

var m2 = Matrices.Matrix(new Entity[,]
    {
        { 1, 2 },
        { 3, "x ^ 3" }
    });
Console.WriteLine(m1 / m2);*/


// there exists x | f(x)
// <=>
// not ({ } = { x | f(x) })

// for all x | f(x)
// <=>
// { } = { x | not f(x) }

/*
 lim = L <=> for all eps (
                 eps > 0 implies 
                     there exists delta (
                        for all x (
                            |x - a| < delta
                                implies
                            |f(x) - L| < eps
                        )
                     )
*/

// for all eps > 0
// <=>
// { } = { y | not (y > 0) }