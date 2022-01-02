//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;
using BenchmarkDotNet.Attributes;
using System;
using System.Linq;
using System.Numerics;

namespace DotnetBenchmark
{
    public class BenchLinqCompilation
    {
        public readonly Func<Complex, Complex, Complex> NormalSimpleLambda =
            (a, b) => (Complex.Sin(a - b) + Complex.Cos(b - a / b) + Complex.Pow(a, b) / (a + b));
        public readonly Func<Complex, Complex, Complex> MySimpleLambda =
                      "sin(a - b) + cos(b - a / b) + a ^ b / (a + b)".Compile<Complex, Complex, Complex>("a", "b");

        public readonly Func<Complex, Complex, Complex, Complex> NormalComplicatedLambda =
            (a, b, c) => Complex.Sin(a + b + Complex.Cos(c)) * 6 + 
                         Complex.Pow(3, Complex.Sin(a + b + Complex.Cos(c))) + 
                         Complex.Tan(Complex.Sin(a + b + Complex.Cos(c))) - 
                         Complex.Log(Complex.Sin(a + b + Complex.Cos(c)), 3)
                         + Complex.Sin(a + b + Complex.Cos(c))
                            + Complex.Sin(a + b + Complex.Cos(c))
                                + Complex.Sin(a + b + Complex.Cos(c))
                                    + Complex.Sin(a + b + Complex.Cos(c))
                                        + Complex.Sin(a + b + Complex.Cos(c))
                                            + Complex.Sin(a + b + Complex.Cos(c))
                                                + Complex.Sin(a + b + Complex.Cos(c))
                                                    + Complex.Sin(a + b + Complex.Cos(c))
                                                        + Complex.Sin(a + b + Complex.Cos(c))
                                                            + Complex.Sin(a + b + Complex.Cos(c))
                                                                + Complex.Sin(a + b + Complex.Cos(c));
        public readonly Func<Complex, Complex, Complex, Complex> MyComplicatedLambda =
                         ("sin(a + b + cos(c)) * 6 +" +
                         "3 ^ sin(a + b + cos(c)) +" +
                         "tan(sin(a + b + cos(c)))-" +
                         "log(3, sin(a + b + cos(c))) +" +
                         string.Join("+", Enumerable.Range(0, 11).Select(_ => "sin(a + b + cos(c))"))
                        )
                         .Compile<Complex, Complex, Complex, Complex>("a", "b", "c");

        [Benchmark]
        public void BenchNormalSimple()
            => NormalSimpleLambda(6, 4);

        [Benchmark]
        public void BenchMySimple()
            => MySimpleLambda(6, 4);

        [Benchmark]
        public void BenchNormalComplicated()
            => NormalComplicatedLambda(1, 2, 3);

        [Benchmark]
        public void BenchMyComplicated()
            => MyComplicatedLambda(1, 2, 3);
    }
}
