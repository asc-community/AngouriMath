// Based on https://github.com/raminrahimzada/CSharp-Helper-Classes/blob/55a721db249768976de588ac4475c33caf7a0954/Math/DecimalMath/DecimalMathUnitTests.cs

using System;
using System.Diagnostics;
using Xunit;
using AngouriMath;
using PeterO.Numbers;

namespace UnitTests.Core
{
    public class PeterONumbersExtensionsTest
    {
        // The precision of System.Math on non-Windows systems is a bit off
        static readonly EDecimal precision = 3e-14m;
        static readonly EContext context = new EContext(25, ERounding.HalfUp, -324, 308, false);
        static readonly int testCount = 1000;
        static readonly Random Random = new Random();

        void AssertTest(object input, double e, EDecimal a)
        {
            var relDiff = (EDecimal.FromDouble(e) - a).Divide(EDecimal.FromDouble(e), context).Abs();
            var maxRelDiff = precision;
            Assert.True(
                double.IsNaN(e) && a.IsNaN()
                || double.IsPositiveInfinity(e) && (a.IsPositiveInfinity() || a.GreaterThan(EDecimal.FromDouble(double.MaxValue)))
                || double.IsNegativeInfinity(e) && (a.IsNegativeInfinity() || a.LessThan(EDecimal.FromDouble(double.MinValue)))
                || e == 0 && (a.IsZero || a.Abs().LessThan(EDecimal.FromDouble(double.Epsilon)))
                || relDiff.CompareToTotal(maxRelDiff) < 0,
                $"\nInput: {input}\nExpected: {e}\nActual: {a}\nRel Diff: {relDiff}\nMax Rel Diff: {maxRelDiff}");
        }

        static readonly double[] mustTest = { 1.0, 0.0, -0.0, -1.0, double.PositiveInfinity, double.NegativeInfinity, double.NaN };
        void Test(Func<double, double> expected, Func<EDecimal, EContext, EDecimal> actual)
        {
            void TestIteration(double e)
            {
                var input = EDecimal.FromDouble(e);
                e = expected(e);
                var a = actual(input, context);
                AssertTest(input, e, a);
            }
            foreach (var x in mustTest)
                TestIteration(x);
            for (int i = 0; i < testCount; i++)
                TestIteration(Random.NextDouble() * Random.Next(-500, 501));
        }

        [Fact] public void TestMethodAsin() => Test(Math.Asin, NumbersExtensions.Asin);
        [Fact] public void TestMethodAcos() => Test(Math.Acos, NumbersExtensions.Acos);
        [Fact] public void TestMethodAtan() => Test(Math.Atan, NumbersExtensions.Atan);
        [Fact] public void TestMethodSin() => Test(Math.Sin, NumbersExtensions.Sin);
        [Fact] public void TestMethodCos() => Test(Math.Cos, NumbersExtensions.Cos);
        [Fact] public void TestMethodTan() => Test(Math.Tan, NumbersExtensions.Tan);
        [Fact] public void TestMethodSinh() => Test(Math.Sinh, NumbersExtensions.Sinh);
        [Fact] public void TestMethodCosh() => Test(Math.Cosh, NumbersExtensions.Cosh);
        [Fact] public void TestMethodTanh() => Test(Math.Tanh, NumbersExtensions.Tanh);
        [Fact]
        public void TestMethodAtan2()
        {
            void TestIteration(double x, double y)
            {
                EDecimal dx = EDecimal.FromDouble(x);
                EDecimal dy = EDecimal.FromDouble(y);
                var e = Math.Atan2(y, x);
                var a = dy.Atan2(dx, context);
                AssertTest((y, x), e, a);
            }
            for (int i = 0; i < testCount; i++)
                TestIteration(Random.NextDouble() * Random.Next(-500, 501), Random.NextDouble() * Random.Next(-500, 501));
            foreach (var x in mustTest)
            foreach (var y in mustTest)
                    TestIteration(x, y);
        }
        [Fact]
        public void ImpreciseSin() // This is why we need a large precision
        {
            var input = 248.1858140380055601781350560486316680908203125;
            var expected = Math.Sin(input);
            var @decimal = EDecimal.FromDouble(input);
            var actual = @decimal.Sin(context);
            AssertTest(EDecimal.FromDouble(input), expected, actual);
        }
        [Fact]
        public void DebugWithMe()
        {
            var input = 248.1858140380055601781350560486316680908203125;
            var expected = Math.Sin(input);
            var @decimal = EDecimal.FromDouble(input);
            var actual = @decimal.Sin(context);
            AssertTest(EDecimal.FromDouble(input), expected, actual);
        }

        // End of https://github.com/raminrahimzada/CSharp-Helper-Classes/blob/55a721db249768976de588ac4475c33caf7a0954/Math/DecimalMath/DecimalMathUnitTests.cs

        // Based on https://github.com/eobermuhlner/big-math/blob/ba75e9a80f040224cfeef3c2ac06390179712443/ch.obermuhlner.math.big/src/test/java/ch/obermuhlner/math/big/BigDecimalMathTest.java

        [Fact]
        public void FactorialInt()
        {
            Assert.Equal(EInteger.FromInt32(1), NumbersExtensions.Factorial(0));
            Assert.Equal(EInteger.FromInt32(1), NumbersExtensions.Factorial(1));
            Assert.Equal(EInteger.FromInt32(2), NumbersExtensions.Factorial(2));
            Assert.Equal(EInteger.FromInt32(6), NumbersExtensions.Factorial(3));
            Assert.Equal(EInteger.FromInt32(24), NumbersExtensions.Factorial(4));
            Assert.Equal(EInteger.FromInt32(120), NumbersExtensions.Factorial(5));

            Assert.Equal(
                    EInteger.FromString("9425947759838359420851623124482936749562312794702543768327889353416977599316221476503087861591808346911623490003549599583369706302603264000000000000000000000000"),
                    NumbersExtensions.Factorial(101));

            var expected = EInteger.One;
            for (int n = 1; n < 1000; n++)
            {
                expected *= n;
                Assert.Equal(expected, NumbersExtensions.Factorial(n));
            }
            Assert.Throws<ArgumentOutOfRangeException>(() => NumbersExtensions.Factorial(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => NumbersExtensions.Factorial(-2));
        }

        // Results from wolframalpha.com:
        // Relative pos  v 3    v 10           v 25                     v 50    (Shift to right by 1 for 0.5!)            v 100
        // 1.5!    =  1.3293403881791370204736256125058588870981620920917903461603558423896834634432741360312129925539084990621701
        // 0.5!    =  0.886226925452758013649083741670572591398774728061193564106903894926455642295516090687475328369272332708113411812
        // (-0.5)! =  1.77245385090551602729816748334114518279754945612238712821380778985291128459103218137495065673854466541622682362428
        // (-1.5)! = -3.544907701811032054596334966682290365595098912244774256427615579705822569182064362749901313477089330832453647248
        [Theory]
        [InlineData("0", 100, "1")]
        [InlineData("1", 100, "1")]
        [InlineData("2", 100, "2")]
        [InlineData("3", 100, "6")]
        [InlineData("4", 100, "24")]
        [InlineData("5", 100, "120")]
        [InlineData("-1", 100, "NaN")]
        [InlineData("-2", 100, "NaN")]
        [InlineData("1.5", 3, "1.33")]
        [InlineData("1.5", 10, "1.329340388")]
        [InlineData("1.5", 25, "1.329340388179137020473626")]
        [InlineData("1.5", 50, "1.3293403881791370204736256125058588870981620920918")]
        [InlineData("1.5", 100, "1.329340388179137020473625612505858887098162092091790346160355842389683463443274136031212992553908499")]
        [InlineData("0.5", 3, "0.886")]
        [InlineData("0.5", 10, "0.8862269255")]
        [InlineData("0.5", 25, "0.8862269254527580136490837")]
        [InlineData("0.5", 50, "0.88622692545275801364908374167057259139877472806119")]
        [InlineData("0.5", 100, "0.8862269254527580136490837416705725913987747280611935641069038949264556422955160906874753283692723327")]
        [InlineData("-0.5", 3, "1.77")]
        [InlineData("-0.5", 10, "1.772453851")]
        [InlineData("-0.5", 25, "1.772453850905516027298167")]
        [InlineData("-0.5", 50, "1.7724538509055160272981674833411451827975494561224")]
        [InlineData("-0.5", 100, "1.772453850905516027298167483341145182797549456122387128213807789852911284591032181374950656738544665")]
        [InlineData("-1.5", 3, "-3.54")]
        [InlineData("-1.5", 10, "-3.544907702")]
        [InlineData("-1.5", 25, "-3.544907701811032054596335")]
        [InlineData("-1.5", 50, "-3.5449077018110320545963349666822903655950989122448")]
        [InlineData("-1.5", 100, "-3.544907701811032054596334966682290365595098912244774256427615579705822569182064362749901313477089331")]
        [InlineData("Infinity", 100, "Infinity")]
        [InlineData("-Infinity", 100, "NaN")]
        [InlineData("NaN", 100, "NaN")]
        public void Factorial(string input, int precision, string output)
        {
            Assert.Equal(output, EDecimal.FromString(input).Factorial(EContext.ForPrecision(precision)).ToString());
            Assert.Equal(output, EDecimal.FromString(input).Increment().Gamma(EContext.ForPrecision(precision)).ToString());
        }
    }
}
