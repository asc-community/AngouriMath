using AngouriMath;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using AngouriMath.Core;
using System.Linq.Expressions;
using AngouriMath.Core.Numerix;

namespace DotnetBenchmark
{
    public class CacheCompiledFuncTest
    {
        private readonly FastExpression complexFunc;
        private readonly Func<Complex, Complex> linqComp;
        private readonly Entity notCompiled;
        private readonly VariableEntity x = MathS.Var("x");
        public CacheCompiledFuncTest()
        {

            notCompiled = MathS.Sin(MathS.Sqr(x)) + MathS.Cos(MathS.Sqr(x)) + MathS.Sqr(x) + MathS.Sin(MathS.Sqr(x));
            complexFunc = notCompiled.Compile(x);

            Expression<Func<Complex, Complex>> linqExpr = x => Complex.Sin(Complex.Pow(x, 2)) + Complex.Cos(Complex.Pow(x, 2)) + Complex.Pow(x, 2) + Complex.Sin(Complex.Pow(x, 2));
            linqComp = linqExpr.Compile();
        }
        [Benchmark]
        public ComplexNumber MyCompiled() => complexFunc.Call(3);
        [Benchmark]
        public Complex SysIncode() => Complex.Sin(Complex.Pow(3, 2)) + Complex.Cos(Complex.Pow(3, 2)) + Complex.Pow(3, 2) + Complex.Sin(Complex.Pow(3, 2));
        [Benchmark]
        public Complex LinqCompiled() => linqComp.Invoke(3);
        [Benchmark]
        public ComplexNumber NotCompiled() => notCompiled.Substitute(x, 3).Eval();
    }
}
