using System;
using AngouriMath;
using static System.Console;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;
using static AngouriMath.MathS;
using AngouriMath.Extensions;
using static AngouriMath.Entity.Number;

WriteLine(
    ("1x_1 + 4x_2 + 5x_3 + 2x_4 + 3x_5 - 1",
    "-1x_1 + 5x_2 + 1x_3 + 3x_4 + 2x_5 - 2",
    " 3x_1 + 2x_2 - 2x_3 + 1x_4 + 0x_5 + 7",
    "-1x_1 + 2x_2 + 4x_3 + 1x_4 + 2x_5 - 3"
    ).SolveSystem("x_1", "x_2", "x_3", "x_4")?.Stringize()
    );
