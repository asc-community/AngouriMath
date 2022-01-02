//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using HonkSharp.Fluency;
using HonkSharp.Functional;
using System;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;

namespace DotnetBenchmark
{
    [MemoryDiagnoser]
    public class RAMUsageTest
    {
        [Benchmark] public Integer AllocInteger() => 1242;
        [Benchmark] public Entity AllocMatrix1x1() => MathS.FromString("[1]", useCache: false);
        [Benchmark] public Entity AllocMatrix1x2() => MathS.FromString("[1, 2]", useCache: false);
        [Benchmark] public Entity AllocMatrix1x10() => MathS.FromString("[1, 2, 3, 4, 5, 6, 7, 8, 9, 10]", useCache: false);
        [Benchmark] public Entity AllocMatrix1x100() => MathS.FromString(LList.Of((1..100).AsRange()).ToString(), useCache: false);
        [Benchmark] public Integer AddTwoIntegers() => (Integer)1231 + 234982;
        [Benchmark] public Entity ParseAndSimplifySimple() => "x + x".ToEntity().Simplify();
        [Benchmark] public Entity ParseHuge() => MathS.FromString("(2*(-2*4*cos(x)sin(x)+2*(2*-2*cos(x)sin(x)+-1*4*cos(x)sin(x))+2*(2*-2*cos(x)sin(x)+-1*4*cos(x)sin(x)+2*(2*-2*cos(x)sin(x)+-1*4*cos(x)sin(x))+2*-2cos(x)*sin(x)+2*-2*cos(x)sin(x)+0+0)+0+4*-2*cos(x)sin(x)+0+cos(x)sin(x)*1*2^2*1*-2+2*2*-2*cos(x)sin(x)+-2*(2*2cos(x)*sin(x)+0+4cos(x)*sin(x)+0))+8*-2*cos(x)sin(x)+2*(2*-2*cos(x)sin(x)+-1*4*cos(x)sin(x)+2*(2*-2*cos(x)sin(x)+-1*4*cos(x)sin(x))+2*-2cos(x)*sin(x)+2*-2*cos(x)sin(x)+0+0)+-2*(2*(2*2cos(x)*sin(x)+0+4cos(x)*sin(x)+0)+0+2*2cos(x)*sin(x)+0+4cos(x)*sin(x)+sin(x)*2^2*cos(x)*1*1+4cos(x)*sin(x))+-4*(2*2cos(x)*sin(x)+0+4cos(x)*sin(x)+0)+4*(2*-2*cos(x)sin(x)+-1*4*cos(x)sin(x))+sin(x)*4cos(x)*2*-2+-4*4*cos(x)sin(x)+0+2*4*-2*cos(x)sin(x)+0)-(12*2*cos(x)sin(x)+4*2*cos(x)sin(x)+2*(2*(2*2cos(x)*sin(x)+0+4cos(x)*sin(x)+0)+0+2*2cos(x)*sin(x)+0+4cos(x)*sin(x)+sin(x)*2^2*cos(x)*1*1+4cos(x)*sin(x))+-2*(2*-2*cos(x)sin(x)+-1*4*cos(x)sin(x)+2*(2*-2*cos(x)sin(x)+-1*4*cos(x)sin(x))+2*-2cos(x)*sin(x)+2*-2*cos(x)sin(x)+0+0)+-4*(2*-2*cos(x)sin(x)+-1*4*cos(x)sin(x))+4*(2*2cos(x)*sin(x)+0+4cos(x)*sin(x)+0)+16cos(x)*sin(x)+sin(x)*4^2*cos(x)*1*1+2*(2*4*cos(x)sin(x)+2*(2*2cos(x)*sin(x)+0+4cos(x)*sin(x)+0)+2*(2*(2*2cos(x)*sin(x)+0+4cos(x)*sin(x)+0)+0+2*2cos(x)*sin(x)+0+4cos(x)*sin(x)+sin(x)*2^2*cos(x)*1*1+4cos(x)*sin(x))+0+4*2*cos(x)sin(x)+0+8cos(x)*sin(x)+2*sin(x)*2^2*cos(x)*1*1+-2*(2*-2*cos(x)sin(x)+-1*4*cos(x)sin(x))))", false);
        [Benchmark] public Entity ParseAndSimplifyHuge() => MathS.FromString("(2*(-2*4*cos(x)sin(x)+2*(2*-2*cos(x)sin(x)+-1*4*cos(x)sin(x))+2*(2*-2*cos(x)sin(x)+-1*4*cos(x)sin(x)+2*(2*-2*cos(x)sin(x)+-1*4*cos(x)sin(x))+2*-2cos(x)*sin(x)+2*-2*cos(x)sin(x)+0+0)+0+4*-2*cos(x)sin(x)+0+cos(x)sin(x)*1*2^2*1*-2+2*2*-2*cos(x)sin(x)+-2*(2*2cos(x)*sin(x)+0+4cos(x)*sin(x)+0))+8*-2*cos(x)sin(x)+2*(2*-2*cos(x)sin(x)+-1*4*cos(x)sin(x)+2*(2*-2*cos(x)sin(x)+-1*4*cos(x)sin(x))+2*-2cos(x)*sin(x)+2*-2*cos(x)sin(x)+0+0)+-2*(2*(2*2cos(x)*sin(x)+0+4cos(x)*sin(x)+0)+0+2*2cos(x)*sin(x)+0+4cos(x)*sin(x)+sin(x)*2^2*cos(x)*1*1+4cos(x)*sin(x))+-4*(2*2cos(x)*sin(x)+0+4cos(x)*sin(x)+0)+4*(2*-2*cos(x)sin(x)+-1*4*cos(x)sin(x))+sin(x)*4cos(x)*2*-2+-4*4*cos(x)sin(x)+0+2*4*-2*cos(x)sin(x)+0)-(12*2*cos(x)sin(x)+4*2*cos(x)sin(x)+2*(2*(2*2cos(x)*sin(x)+0+4cos(x)*sin(x)+0)+0+2*2cos(x)*sin(x)+0+4cos(x)*sin(x)+sin(x)*2^2*cos(x)*1*1+4cos(x)*sin(x))+-2*(2*-2*cos(x)sin(x)+-1*4*cos(x)sin(x)+2*(2*-2*cos(x)sin(x)+-1*4*cos(x)sin(x))+2*-2cos(x)*sin(x)+2*-2*cos(x)sin(x)+0+0)+-4*(2*-2*cos(x)sin(x)+-1*4*cos(x)sin(x))+4*(2*2cos(x)*sin(x)+0+4cos(x)*sin(x)+0)+16cos(x)*sin(x)+sin(x)*4^2*cos(x)*1*1+2*(2*4*cos(x)sin(x)+2*(2*2cos(x)*sin(x)+0+4cos(x)*sin(x)+0)+2*(2*(2*2cos(x)*sin(x)+0+4cos(x)*sin(x)+0)+0+2*2cos(x)*sin(x)+0+4cos(x)*sin(x)+sin(x)*2^2*cos(x)*1*1+4cos(x)*sin(x))+0+4*2*cos(x)sin(x)+0+8cos(x)*sin(x)+2*sin(x)*2^2*cos(x)*1*1+-2*(2*-2*cos(x)sin(x)+-1*4*cos(x)sin(x))))", false).Simplify();
    }
}
