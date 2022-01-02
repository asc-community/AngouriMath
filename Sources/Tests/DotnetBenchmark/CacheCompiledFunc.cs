//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Core;
using System;
using System.Numerics;
using BenchmarkDotNet.Attributes;
using System.Linq.Expressions;

namespace DotnetBenchmark
{
    public class CacheCompiledFunc
    {
        private readonly FastExpression complexFunc;
        private readonly Func<Complex, Complex> myCompiledIntoLinq;
        private readonly Func<Complex, Complex> linqComp;
        private readonly Entity notCompiled;
        private readonly Entity.Variable x = MathS.Var("x");
        private readonly Entity.Number.Complex ComNumToSub = 3;
        private readonly Complex ComToSub = 3;
        public CacheCompiledFunc()
        {

            notCompiled = MathS.Sin(MathS.Sqr(x)) + MathS.Cos(MathS.Sqr(x)) + MathS.Sqr(x) + MathS.Sin(MathS.Sqr(x));
            complexFunc = notCompiled.Compile(x);

            Expression<Func<Complex, Complex>> linqExpr = x => Complex.Sin(Complex.Pow(x, 2)) + Complex.Cos(Complex.Pow(x, 2)) + Complex.Pow(x, 2) + Complex.Sin(Complex.Pow(x, 2));
            linqComp = linqExpr.Compile();

            myCompiledIntoLinq = notCompiled.Compile<Complex, Complex>(x);
        }
        [Benchmark] public Complex MyCompiled() => complexFunc.Call(ComToSub);
        [Benchmark] public Complex MyLinqCompiled() => myCompiledIntoLinq(ComToSub);
        [Benchmark] public Complex SysIncode() => Complex.Sin(Complex.Pow(3, 2)) + Complex.Cos(Complex.Pow(3, 2)) + Complex.Pow(3, 2) + Complex.Sin(Complex.Pow(3, 2));
        [Benchmark] public Complex LinqCompiled() => linqComp.Invoke(3);
        [Benchmark] public Entity.Number.Complex NotCompiled() => notCompiled.Substitute(x, 3).EvalNumerical();
    }
}
