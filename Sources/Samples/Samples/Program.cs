using System;
using AngouriMath;
using AngouriMath.Extensions;
using static AngouriMath.MathS;
using static AngouriMath.Entity;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using static AngouriMath.Entity.Set;
using AngouriMath.Core;
using System.Linq;

var sol = SolveSystem(
    "6x + 2y2 + z = 0",
    "5x + y + 2z = 3",
    "4x + 3y + 11z2 = 6"
)?.Simplify();
if (sol is Matrix m)
    Console.WriteLine(m.ToString(multilineFormat: true));

static Matrix? SolveSystem(Entity eq1, Entity eq2, Entity eq3)
{
    var res = new MatrixBuilder(3);
    if (eq1.Solve("x") is not FiniteSet Fyz) return res.ToMatrix();
    if (eq2.Solve("y") is not FiniteSet Fxz) return res.ToMatrix();
    if (eq3.Solve("z") is not FiniteSet Fxy) return res.ToMatrix();

    foreach (var (f1yz, f2xz, f3xy) in (Fyz, Fxz, Fxy).Multiply().Enum("Outer. "))
    {
        var f1yf3xy = f1yz.Substitute("z", f3xy);
        var f2zf1yz = f2xz.Substitute("x", f1yz);
        var f3xf2zx = f3xy.Substitute("y", f2xz);
        
        if ((f1yf3xy - "x").SolveEquation("x") is not FiniteSet G1y) continue;
        if ((f2zf1yz - "y").SolveEquation("y") is not FiniteSet G2z) continue;
        if ((f3xf2zx - "z").SolveEquation("z") is not FiniteSet G3x) continue;

        foreach (var (g1y, g2z, g3x) in (G1y, G2z, G3x).Multiply().Enum("  Inner. "))
        {
            var k1x = g1y.Substitute("y", g2z).Substitute("z", g3x);
            var k2y = g2z.Substitute("z", g3x).Substitute("x", g1y);
            var k3z = g3x.Substitute("x", g1y).Substitute("y", g2z);

            if ((k1x - "x").SolveEquation("x") is not FiniteSet xSol) continue;
            if ((k2y - "y").SolveEquation("y") is not FiniteSet ySol) continue;
            if ((k3z - "z").SolveEquation("z") is not FiniteSet zSol) continue;

            foreach (var (x, y, z) in (xSol, ySol, zSol).Multiply())
                res.Add(new[] { x, y, z });
        }
    }

    return res.ToMatrix();
}

public static class Ext
{
    public static IEnumerable<(Entity, Entity, Entity)> Multiply(this (FiniteSet, FiniteSet, FiniteSet) sets)
    {
        foreach (var a in sets.Item1)
        foreach (var b in sets.Item2)
        foreach (var c in sets.Item3)
            yield return (a, b, c);
    }

    public static IEnumerable<T> Enum<T>(this IEnumerable<T> seq, string prefix)
    {
        var id = 0;
        foreach (var s in seq)
        {
            id++;
            Console.WriteLine($"{prefix} #{id} Processing...");
            yield return s;
            Console.WriteLine($"{prefix} #{id} OK.");
        }
    }
}