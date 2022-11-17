//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath.Core;
using static AngouriMath.MathS;
using AngouriMath;

var system = MathS.Equations(
            "2 * x_1 * (-66) - 6 * x_2 + 24 * x_3 - 12 * x_4 + 270",
            "-6 * x_1 - 2 * x_2 * 74 - 8 * x_3 + 4 * x_4 - 440",
            "24 * x_1 - 8 * x_2 - 2 * x_3 * 59 - 16 * x_4 - 190",
            "-12 * x_1 + 4 * x_2 - 16 * x_3 - 2 * x_4 * 71 + 20"
            );
Console.WriteLine(system.Solve("x_1", "x_2", "x_3", "x_4"));