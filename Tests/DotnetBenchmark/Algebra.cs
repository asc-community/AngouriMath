using AngouriMath;
using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace DotnetBenchmark
{
    public class AlgebraTest
    {
        private readonly VariableEntity x = MathS.Var("x");
        private readonly Entity exprEasy;
        private readonly Entity exprMedium;
        private readonly Entity exprHard;
        public AlgebraTest()
        {
            exprEasy = x + MathS.Sqr(x) - 3;
            exprMedium = MathS.Sin(x + MathS.Cos(x)) + MathS.Sqrt(x + MathS.Sqr(x));
            exprHard = MathS.Sin(x + MathS.Arcsin(x)) / (MathS.Sqr(x) + MathS.Cos(x)) * MathS.Arccos(x / 1200 + 0.00032 / MathS.Cotan(x + 43));
        }
        [Benchmark]
        public void SolveEasy() => exprEasy.SolveNt(x);
        [Benchmark]
        public void SolveMedium() => exprMedium.SolveNt(x);
        [Benchmark]
        public void SolveHard() => exprHard.SolveNt(x);
    }
}
