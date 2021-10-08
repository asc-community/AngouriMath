//
// Copyright (c) 2019-2021 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using HonkSharp.Fluency;

using System;
using static AngouriMath.Entity;
using AngouriMath.Extensions;
using static AngouriMath.Entity.Number;

using static AngouriMath.MathS;
using static System.Console;
using PeterO.Numbers;

System.Collections.Generic.IList<int> a = new int[3];

Entity.Matrix matrix = @"
[
    [1.000000000000000547458310833800074927650525249394612805610000, 0, 0, -9.55753574799863785565952768061466479249665075036612773690000E-15],
    [0, 1.000000000000000547458310833800074927650525249394612805610000, 0, 2.24075209004037969624007863746621061783995000E-15], 
    [0, 0, 1, 0],
    [0, 0, 0, 1]
]";
var identity = MathS.IdentityMatrix(4);
// using var _ = MathS.Settings.PrecisionErrorCommon.Set(0.1m);
using var _ = MathS.Diagnostic.OutputExplicit.Set(true);

WriteLine(matrix.ToString(true));
WriteLine(matrix == identity);
Entity a = "a + 0.001";
Entity b = "a + 0.002";
WriteLine(a == b);

Entity.Matrix a1 = @"[
    [1, 0, 0, 0],
    [0, 1, 0, 0],
    [0, 0, 1, 0],
    [0, 0, 0, 1]
]";
Entity b1 = @"[
    [1.000000000000000547458310833800074927650525249394612805610000, 0, 0, -9.55753574799863785565952768061466479249665075036612773690000E-15], 
    [0, 1.000000000000000547458310833800074927650525249394612805610000, 0, 2.24075209004037969624007863746621061783995000E-15],
    [0, 0, 1, 0],
    [0, 0, 0, 1]
]";
WriteLine(a1 == identity);
using var __ = MathS.Settings.PrecisionErrorZeroRange.Set(0.1m);
WriteLine(a1 == b1);
// WriteLine(b1 == identity);
// WriteLine(matrix == b1);