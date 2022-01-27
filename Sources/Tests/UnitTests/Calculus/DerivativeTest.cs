//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using HonkSharp.Fluency;
using Xunit;
using static AngouriMath.Entity;

namespace AngouriMath.Tests.Calculus
{
    public sealed class DerivativeTest
    {
        private static readonly Variable x = "x";
        private static readonly Variable y = "y";
        private static readonly Variable f = "f";

        [Fact]
        public void Test1()
        {
            var func = MathS.Sqr(x) + 2 * x + 1;
            var derived = func.Differentiate(x);
            Assert.Equal(2 + 2 * x, derived.Simplify());
        }
        [Fact]
        public void TestSin()
        {
            var func = MathS.Sin(x);
            Assert.Equal(MathS.Cos(x), func.Differentiate(x).Simplify());
        }
        [Fact]
        public void TestCosCustom()
        {
            var func = MathS.Cos(MathS.Pow(x, 3));
            var expected = -3 * MathS.Sin(MathS.Pow(x, 3)) * MathS.Sqr(x);
            var actual = func.Differentiate(x).Simplify();
            Assert.Equal(expected, actual);
        }
        [Fact]
        public void TestPow()
        {
            var func = MathS.Pow(MathS.e, x);
            Assert.Equal(func, func.Differentiate(x).Simplify());
        }
        [Fact]
        public void TestPoly()
        {
            var func = MathS.Pow(x, 4);
            Assert.Equal(4 * MathS.Pow(x, 3), func.Differentiate(x).Simplify());
        }
        [Fact]
        public void TestCusfunc()
        {
            var func = MathS.Sin(x).Pow(2);
            Assert.Equal(MathS.Sin(2 * x), func.Differentiate(x).Simplify(3));
        }
        [Fact]
        public void TestTan()
        {
            var func = MathS.Tan(2 * x);
            Assert.Equal(2 / MathS.Pow(MathS.Cos(2 * x), 2), func.Differentiate(x).Simplify());
        }
        [Fact]
        public void TestCoTan()
        {
            var func = MathS.Cotan(2 * x);
            Assert.Equal(-2 / MathS.Pow(MathS.Sin(2 * x), 2), func.Differentiate(x).Simplify());
        }
        [Fact]
        public void TestArc1()
        {
            var func = MathS.Arcsin(x);
            Assert.Equal(1 / MathS.Sqrt(1 - MathS.Sqr(x)), func.Differentiate(x).Simplify());
        }
        [Fact]
        public void TestArc2()
        {
            var func = MathS.Arcsin(2 * x);
            Assert.Equal(1 / MathS.Sqrt(1 - MathS.Sqr(2 * x)) * 2, func.Differentiate(x).Simplify());
        }
        [Fact]
        public void TestArc3()
        {
            var func = MathS.Arccos(2 * x);
            Assert.Equal((-1) / MathS.Sqrt(1 - MathS.Sqr(2 * x)) * 2, func.Differentiate(x).Simplify());
        }
        [Fact]
        public void TestArc4()
        {
            var func = MathS.Arctan(2 * x);
            Assert.Equal(2 / (1 + 4 * MathS.Sqr(x)), func.Differentiate(x).Simplify());
        }
        [Fact]
        public void TestArc5()
        {
            var func = MathS.Arccotan(2 * x);
            Assert.Equal(-2 / (1 + 4 * MathS.Sqr(x)), func.Differentiate(x).Simplify());
        }
        [Fact]
        public void TestNaN()
        {
            var func = MathS.Numbers.Create(double.NaN);
            Assert.Equal(MathS.Numbers.Create(double.NaN), func.Differentiate(x).Simplify());
        }
        [Fact]
        public void TestNaN2()
        {
            var func = MathS.Pow(21, MathS.Numbers.Create(double.NaN));
            Assert.Equal(MathS.Numbers.Create(double.NaN), func.Differentiate(x).Simplify());
        }

        [Fact]
        public void TestDerOverDer2()
        {
            var func = MathS.Derivative("x + 2", "y");
            var derFunc = func.Differentiate(x);
            Assert.Equal(0, derFunc);
        }

        [Fact]
        public void TestSgnDer()
        {
            Entity func = "sgn(x + 2)";
            var derived = func.Differentiate("x");
            Assert.Equal(MathS.Derivative(func, x), derived);
        }

        [Fact]
        public void TestAbsDer()
        {
            Entity func = "abs(x + 2)";
            var derived = func.Differentiate("x");
            Assert.Equal(MathS.Signum("x + 2").Simplify(), derived.Simplify());
        }

        [Fact]
        public void TestSecant()
        {
            Entity func = "sec(2x)";
            var derived = func.Differentiate("x");
            Assert.Equal(2 * MathS.Sec("2x") * MathS.Tan("2x"), derived.Simplify());
        }

        [Fact]
        public void TestCosecant()
        {
            Entity func = "csc(2x)";
            var derived = func.Differentiate("x");
            Assert.Equal(-2 * MathS.Cosec("2x") * MathS.Cotan("2x"), derived.Simplify());
        }

        [Fact]
        public void TestArcsecant()
        {
            Entity func = "arcsec(2x)";
            var derived = func.Differentiate("x");
            Assert.Equal("(1/2) / (sqrt(1 + (-1/4) / x2)x2)".Simplify(), derived.Simplify());
        }

        [Fact]
        public void TestArccosecant()
        {
            Entity func = "arccosec(2x)";
            var derived = func.Differentiate("x");
            Assert.Equal("-1/2 / (sqrt(1 + (-1/4) / x2)x2)".Simplify(), derived.Simplify());
        }

        

        [Fact] public void TestNamedAppliedFunctions1()
            => f.Apply(x.Pow(2))  // f (x ^ 2)
                .Differentiate(x)
                .ShouldBe(
                    MathS.Derivative(f.Apply(x.Pow(2)), x.Pow(2)) * (2 * x) // derivative (f (x ^ 2), x ^ 2) * (2 * x)
                    );

        [Fact] public void TestNamedAppliedFunctions2()
            => f.Apply(x.Pow(2))
                .Differentiate(x)
                .LambdaOver(f)
                .Apply("sin")
                .InnerSimplified
                .ShouldBe(MathS.Cos(x.Pow(2)) * (2 * x));

        [Fact] public void TestNamedAppliedFunctions3()
            =>f.Apply(x.Pow(2), x.Pow(3))
                .Alias(out var ff)
                .Differentiate(x)
                .ShouldBe(
                    MathS.Derivative(ff, x.Pow(2)) * (2 * x)
                    + MathS.Derivative(ff, x.Pow(3)) * (3 * x.Pow(2))
                );
    }
}
