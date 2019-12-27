using System;
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
        private readonly FastExpression mulFunc;

        public CompiledFunc()
        {
            x = MathS.Var("x");
            sinFunc = MathS.Sin(x).Compile(x);
            sqrtFunc = MathS.Sqrt(x).Compile(x);
            mulFunc = ((MathS.Log(x, 3) + MathS.Sqr(x)) * MathS.Sin(x + MathS.Cosec(x))).Compile(x);
        }

        [Benchmark]
        public Number SinFunc() => sinFunc.Call(3);
        [Benchmark]
        public Number SqrtFunc() => sqrtFunc.Call(3);
        [Benchmark]
        public Number MulFunc() => mulFunc.Call(3);
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<CompiledFunc>();
        }
    }
}