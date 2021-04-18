using System;
using AngouriMath;
using AngouriMath.Extensions;
using static AngouriMath.MathS;
using static AngouriMath.Entity;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

var m2 = MathS.Matrix(new Entity[,]
    {
        {  1,  -1,  0, -2,  0 },
        { -3,  -6, -2,  1,  3 },
        {  0,  -7, -1, -5,  2 },
        {  3,   4,  1, -1, -2 },
        { -6, -19, -5, -3,  8 }
    }
);

Console.WriteLine(m2.RowEchelonForm);
