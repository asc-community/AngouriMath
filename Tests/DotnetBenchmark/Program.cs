using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Numerics;
using AngouriMath;
using AngouriMath.Extensions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace DotnetBenchmark
{
    public class CompiledFuncTest
    {
        private readonly Entity.Variable x;
        private readonly FastExpression multiFunc;
        private readonly Entity multiFuncNotCompiled;
        private readonly Func<Complex, Complex> linqFunc;
        private readonly Complex CToSub = 3;
        private readonly Entity.Number.Complex CNumToSub = 3;
        public CompiledFuncTest()
        {
            x = MathS.Var("x");
            multiFuncNotCompiled = (MathS.Log(3, x) + MathS.Sqr(x)) * MathS.Sin(x + MathS.Cosec(x));
            multiFunc = multiFuncNotCompiled.Compile(x);
            Expression<Func<Complex, Complex>> expr = x => (Complex.Log(x, 3) + Complex.Pow(x, 2)) * Complex.Sin(x + 1 / Complex.Sin(x));
            linqFunc = expr.Compile();
        }
        [Benchmark]
        public void MultiFunc() => multiFunc.Call(CToSub);
        [Benchmark]
        public void LinqSin() => linqFunc(3);
        [Benchmark]
        public void NotCompiled() => multiFuncNotCompiled.Substitute(x, 3).Eval();
    }

    public class NumbersBenchmark
    {
        public NumbersBenchmark()
        {
            
        }

        ~NumbersBenchmark()
        {
            
        }
        private readonly Entity.Number.Real a = 3.4m;
        private readonly Entity.Number.Real b = 5.4m;
            /*
        [Benchmark]
        public void InitComplexImpl() => Entity.Number.Complex.Create(3.4m, 56m);
        [Benchmark]
        public void InitComplexImpl2() => Entity.Number.Complex.Create(3.4, 56);
        [Benchmark]
        public void InitComplexExpl() => Entity.Number.Complex.Create(a, b);
        [Benchmark]
        public void InitReal() => Entity.Number.Real.Create(3.4m);
        [Benchmark]
        public void InitRational() => RationalNumber.Create(6, 7);
        [Benchmark]
        public void InitInteger() => IntegerNumber.Create(68);
        */

        [Benchmark]
        public void DowncastComplexSuccessfully() => Entity.Number.Complex.Create(3.4m, 6.4m);
        [Benchmark]
        public void DowncastComplexNotSucc() => Entity.Number.Complex.Create(3.487449272953435m, 6.401380141304m);

        [Benchmark] 
        public void DowncastRealSuccessfully() => Entity.Number.Real.Create(3.4m);
        [Benchmark]
        public void DowncastRealNotSucc() => Entity.Number.Real.Create(3.42748273484m);

        [Benchmark]
        public void FindRationalSuccess() => Entity.Number.Rational.FindRational(3.4m);
        [Benchmark]
        public void FindRationalNotSuccess() => Entity.Number.Rational.FindRational(3.48426482675284m);
    }

    public class Program
    {
        public static void Main(string[] _)
        {
            BenchmarkRunner.Run<AlgebraTest>();
        }
    }
}