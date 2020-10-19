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


WriteLine(@"x \/ { 1, 2 } = { 1, 2, 3 }".Solve("x"));
WriteLine(@"x \/ { 1, 2 } = { 1, 3 }".Solve("x"));
