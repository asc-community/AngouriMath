using System;
using System.Numerics;
using System.Security.Cryptography;
using AngouriMath;
using AngouriMath.Core;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace DotnetBenchmark
{
    public class CompiledFunc
    {
        private readonly VariableEntity x;
        private readonly FastExpression sinFunc;
        private readonly FastExpression sqrtFunc;
        private readonly FastExpression multiFunc;

        public CompiledFunc()
        {
            x = MathS.Var("x");
            sinFunc = MathS.Sin(x).Compile(x);
            sqrtFunc = MathS.Sqrt(x).Compile(x);
            multiFunc = ((MathS.Log(x, 3) + MathS.Sqr(x)) * MathS.Sin(x + MathS.Cosec(x))).Compile(x);
        }

        [Benchmark]
        public Number SinFunc() => sinFunc.Call(3);
        [Benchmark]
        public Number SqrtFunc() => sqrtFunc.Call(3);
        [Benchmark]
        public Number MultiFunc() => multiFunc.Call(3);
        [Benchmark]
        public Complex ComplexSin() => Complex.Sin(3);
        [Benchmark]
        public Complex ComplexSqrt() => Complex.Sqrt(3);
        [Benchmark]
        public Complex ComplexMulti() => (Complex.Log(3, 3) + Complex.Pow(3, 2)) * Complex.Sin(3 + 1 / Complex.Sin(3));
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<CompiledFunc>();
        }
    }
}