using System;
using System.Linq.Expressions;
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
        private readonly Func<Complex, Complex> linqSin;

        public CompiledFunc()
        {
            x = MathS.Var("x");
            sinFunc = MathS.Sin(x).Compile(x);
            sqrtFunc = MathS.Sqrt(x).Compile(x);
            multiFunc = ((MathS.Log(x, 3) + MathS.Sqr(x)) * MathS.Sin(x + MathS.Cosec(x))).Compile(x);
            Expression<Func<Complex, Complex>> expr = (x) => Complex.Sin(x);
            linqSin = expr.Compile();
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
        [Benchmark]
        public Complex LinqSin() => linqSin(3);
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<CompiledFunc>();
        }
    }
}