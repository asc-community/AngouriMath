using System;
using System.Linq.Expressions;
using System.Numerics;
using AngouriMath;
using AngouriMath.Core;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace DotnetBenchmark
{
    public class CompiledFuncTest
    {
        private readonly VariableEntity x;
        private readonly FastExpression multiFunc;
        private readonly Entity multiFuncNotCompiled;
        private readonly Func<Complex, Complex> linqFunc;
        
        public CompiledFuncTest()
        {
            x = MathS.Var("x");
            multiFuncNotCompiled = (MathS.Log(x, 3) + MathS.Sqr(x)) * MathS.Sin(x + MathS.Cosec(x));
            multiFunc = multiFuncNotCompiled.Compile(x);
            Expression<Func<Complex, Complex>> expr = x => (Complex.Log(x, 3) + Complex.Pow(x, 2)) * Complex.Sin(x + 1 / Complex.Sin(x));
            linqFunc = expr.Compile();
        }
        [Benchmark]
        public void MultiFunc() => multiFunc.Call(3);
        [Benchmark]
        public void LinqSin() => linqFunc(3);
        [Benchmark]
        public void NotCompiled() => multiFuncNotCompiled.Substitute(x, 3).Eval();
    }

    public class Program
    {
        public static void Main(string[] _)
        {
            //BenchmarkRunner.Run<CacheCompiledFuncTest>();
            BenchmarkRunner.Run<CompiledFuncTest>();
            //BenchmarkRunner.Run<AlgebraTest>();
        }
    }
}