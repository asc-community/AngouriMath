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
        private readonly FastExpression func;

        public CompiledFunc()
        {
            x = MathS.Var("x");
            func = MathS.Sin(x).Compile(x);
        }

        [Benchmark]
        public Number Call3() => func.Call(3);
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<CompiledFunc>();
        }
    }
}