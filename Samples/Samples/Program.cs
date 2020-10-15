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

//WriteLine("((x + a) / (x - a))^x".Limit("x", "0"));
//WriteLine("(x - a * x) / (b * x - x)".Limit("x", "0"));
//WriteLine("sin(x2) / sin(x)".Limit("x", "0").Simplify());
WriteLine("b x".Limit("x", "+oo"));
//WriteLine("(a + -1) / (1 - b) + -(1 - a) / (b + -1)".Simplify());
//for (int i = 0; i < 1; i++)
//    WriteLine(MathS.Internal.AreEqualNumerically("(a - 1) / (1 - b) - (1 - a) / (b - 1)", "0"));