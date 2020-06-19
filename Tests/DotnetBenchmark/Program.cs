using System;
using System.Linq.Expressions;
using System.Numerics;
using AngouriMath;
using AngouriMath.Core.Numerix;
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
        private readonly Complex CToSub = 3;
        private readonly ComplexNumber CNumToSub = 3;
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
            MathS.Settings.DowncastingEnabled.Set(true);
        }

        ~NumbersBenchmark()
        {
            MathS.Settings.DowncastingEnabled.Unset();
        }
        private readonly RealNumber a = 3.4m;
        private readonly RealNumber b = 5.4m;
            /*
        [Benchmark]
        public void InitComplexImpl() => Number.Create(3.4m, 56m);
        [Benchmark]
        public void InitComplexImpl2() => Number.Create(3.4, 56);
        [Benchmark]
        public void InitComplexExpl() => Number.Create(a, b);
        [Benchmark]
        public void InitReal() => Number.Create(3.4m);
        [Benchmark]
        public void InitRational() => Number.CreateRational(6, 7);
        [Benchmark]
        public void InitInteger() => Number.Create(68);
        */

        private readonly RealNumber e = new RealNumber(3.4m);
        private readonly RealNumber f = new RealNumber(3.42748273484m);

        private readonly ComplexNumber c = new ComplexNumber(
                new RealNumber(3.4m),
                new RealNumber(6.4m)
            );

        private readonly ComplexNumber d = new ComplexNumber(
            new RealNumber(3.487449272953435m),
            new RealNumber(6.401380141304m)
        );
        [Benchmark]
        public void DowncastComplexSuccessfully()
            => Number.Functional.Downcast(c);
        [Benchmark]
        public void DowncastComplexNotSucc()
            => Number.Functional.Downcast(d);

        [Benchmark] 
        public void DowncastRealSuccessfully()
            => Number.Functional.Downcast(e);
        [Benchmark]
        public void DowncastRealNotSucc()
            => Number.Functional.Downcast(f);

        [Benchmark]
        public void FindRationalSuccess()
            => Number.Functional.FindRational(3.4m);
        [Benchmark]
        public void FindRationalNotSuccess()
            => Number.Functional.FindRational(3.48426482675284m);
    }

    public class Program
    {
        public static void Main(string[] _)
        {
            //BenchmarkRunner.Run<CacheCompiledFuncTest>();
            //BenchmarkRunner.Run<CommonFunctionsInterVersionTest>();
            Entity a = "(sin(x) + sqr(sin(x)) + a)4 + (sin(x) + sqr(sin(x)) + a) + b";
            Console.WriteLine(a.SolveEquation("x"));
        }
    }
}