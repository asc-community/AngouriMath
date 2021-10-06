//
// Copyright (c) 2019-2021 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using BenchmarkDotNet.Attributes;
using HonkSharp.Fluency;
using HonkSharp.Functional;
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
    }
}
