using System;
using AngouriMath;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Boolean;
using static AngouriMath.MathS;
using AngouriMath.Extensions;
using static System.Console;
using static AngouriMath.Entity.Set;
using System.Diagnostics;
using System.Collections;
using System.Numerics;
using PeterO.Numbers;

WriteLine(@"(x15 - 3^15 - 15 3^14(x - 3)) / (x - 3)2".Limit("x", "3", AngouriMath.Core.ApproachFrom.Left));
Entity expr = "x + 2";

