using AngouriMath;
using BenchmarkDotNet.Attributes;
using PeterO.Numbers;

namespace DotnetBenchmark
{
    public class AlgebraTest
    {
        private readonly VariableEntity x = MathS.Var("x");
        private readonly Entity exprEasy;
        private readonly Entity exprMedium;
        private readonly Entity exprHard;
        private readonly Entity exprSolvable;

        // [GlobalSetup] can be replaced with constructor? https://benchmarkdotnet.org/articles/overview.html
        public AlgebraTest()
        {
            exprEasy = x + MathS.Sqr(x) - 3;
            exprMedium = MathS.Sin(x + MathS.Cos(x)) + MathS.Sqrt(x + MathS.Sqr(x));
            exprHard = MathS.Sin(x + MathS.Arcsin(x)) / (MathS.Sqr(x) + MathS.Cos(x)) * MathS.Arccos(x / 1200 + 0.00032 / MathS.Cotan(x + 43));
            exprSolvable = MathS.FromString("3arccos(2x + a)3 + 6arccos(2x + a)2 - a3 + 3");
        }

        
        [Benchmark]
        public void SolveEasy() => exprEasy.SolveNt(x);
        [Benchmark]
        public void SolveMedium() => exprMedium.SolveNt(x);
        [Benchmark]
        public void SolveHard() => exprHard.SolveNt(x);
        [Benchmark]
        public void SolveAnal() => exprSolvable.SolveEquation(x);
        

        private EDecimal dec = 3;
        private readonly EDecimal coef = EDecimal.FromDecimal(0.2m);

        [Benchmark]
        public void Cos()
        {
            dec = dec.Negate(MathS.Settings.DecimalPrecisionContext);
            dec = dec.Multiply(dec, MathS.Settings.DecimalPrecisionContext);
            dec = dec.Multiply(coef);
            dec = dec.Cos(MathS.Settings.DecimalPrecisionContext);
        }
    }
}
