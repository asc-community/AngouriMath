using System;
using AngouriMath;
using static AngouriMath.Entity.Number;

using var _ = MathS.Settings.DecimalPrecisionContext.Set(new(20, PeterO.Numbers.ERounding.Ceiling, -10, 100, false));
using var __ = MathS.Settings.DowncastingEnabled.Set(false);
using var ___ = MathS.Settings.FloatToRationalIterCount.Set(0);

var a = new Asteroid(1000);
var b = new Asteroid(1000) { Position = 1000 };
Real time = 0;


while (a.Position < 900)
{
    var F = Funcs.ComputeGravity(a, b);
    a.ApplyForce(F, 1);
    a.Move(1);
    time += 1;
    Console.Write($"Time: {time}  Speed: {a.Speed}  Position: {a.Position}\r");
}

Console.WriteLine(a.Speed);
Console.WriteLine(time);
Console.WriteLine("Done");
Console.ReadLine();

public static class Funcs
{
    static readonly Real G = 3;
    public static Real ComputeGravity(Asteroid a, Asteroid b)
        => (Real)(G * a.Mass * b.Mass / (a.Position - b.Position).Pow(2)).EvalNumerical();
}

public sealed class Asteroid
{
    public Real Position { get; set; } = 0;
    public Real Speed { get; private set; } = 0;
    public Real Mass { get; }
    public Asteroid(Real mass) => Mass = mass;
    public void Move(Real time)
        => Position += Speed * time;
    public void ApplyForce(Real force, Real time)
        => Speed += force / Mass * time;
}