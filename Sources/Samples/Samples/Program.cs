using System;
using AngouriMath;
using AngouriMath.Extensions;
using static AngouriMath.MathS;
using static AngouriMath.Entity;
using System.Numerics;
using System.ComponentModel;
using System.Linq;
using static AngouriMath.Entity.Set;


// Console.WriteLine("[ [ 1, 2 ] ; [ 3, 4 ] ]".ToEntity());
// var Fn = "[ 1, 0 ]T * ([ [ 1, 1 ], [ 1, 0 ] ] ^ n * [ 1, 0 ])".ToEntity();
// Console.WriteLine(Fn.Substitute("n", 1_000_000).EvalNumerical().ToString().Length);
// Console.WriteLine(I_3.WithElement(0, 1, 1).WithElement(1, 2, 1));

// Console.WriteLine("(| [ a, b, c ] |)".Simplify());
// Console.WriteLine("(| [ 2, 3, 6 ] |)".EvalNumerical());


return;

Console.WriteLine(Fib(4));

return;

var matrix = MathS.Matrix(
    new Entity[,]
    {
        { 546, 43 },
        { 23, 9 },
        { 134, 67 }
    });
var indexer = Indexer(matrix, "x", "y");
Console.WriteLine(indexer.Latexise());
return;
Console.WriteLine(indexer.Substitute(("x", "y"), (2.3, 1)).InnerSimplified);


static Entity Indexer(Matrix m, Variable rowId, Variable colId)
{
    Entity before =
        Enumerable.Range(0, m.RowCount)
        .Select(c => Piecewise((1, rowId.Equalizes(c)), (0, true))).ToVector().T;
    Entity after =
        Enumerable.Range(0, m.ColumnCount)
        .Select(c => Piecewise((1, colId.Equalizes(c)), (0, true))).ToVector();
    return (before * m * after).Provided(rowId.In("ZZ") & colId.In("ZZ") & (rowId >= 0) & (rowId < m.RowCount) & (colId >= 0) & (colId < m.ColumnCount));
}

int Fib(int n) => n switch
    {
        0 => 0,
        1 => 1,
        _ => Fib(n - 1) + Fib(n - 2)
    };

namespace System.Runtime.CompilerServices
{
    // C# 9 requires this class to be defined to act as modreq in records and init-only members.
    // This is defined in .NET 5 but not lower-level targets like .NET Standard 2.0.
    /// <summary>
    /// Reserved to be used by the compiler for tracking metadata.
    /// This class should not be used by developers in source code.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class IsExternalInit { }
}

// var M = "[ 0, 1, 0 ]T *  [ [ 1, 15 ], [ 25, 34 ], [ 67, 89 ] ] * [ 1, 0 ]".ToEntity();
// Console.WriteLine(M.Substitute("n", 6).Evaled);
// // Console.WriteLine(Det(H - "x" * I_3).Simplify());
// 
// static Matrix GetElement(Matrix m, int i, int j)
// {
//     var prefixM = MathS.ZeroVector
// }

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