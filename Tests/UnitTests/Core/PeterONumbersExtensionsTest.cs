// Based on https://github.com/raminrahimzada/CSharp-Helper-Classes/blob/55a721db249768976de588ac4475c33caf7a0954/Math/DecimalMath/DecimalMathUnitTests.cs

using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AngouriMath;
using PeterO.Numbers;

namespace UnitTests.Core
{
    [TestClass]
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
            Assert.IsTrue(
                double.IsNaN(e) && a.IsNaN()
                || double.IsPositiveInfinity(e) && (a.IsPositiveInfinity() || a.GreaterThan(EDecimal.FromDouble(double.MaxValue)))
                || double.IsNegativeInfinity(e) && (a.IsNegativeInfinity() || a.LessThan(EDecimal.FromDouble(double.MinValue)))
                || e == 0 && (a.IsZero || a.Abs().LessThan(EDecimal.FromDouble(double.Epsilon)))
                || relDiff.CompareToTotal(maxRelDiff) < 0,
                $"\nInput: {input}\nExpected: {e}\nActual: {a}\nRel Diff: {relDiff}\nMax Rel Diff: {maxRelDiff}");
        }

        void Test(Func<double, double> expected, Func<EDecimal, EContext, EDecimal> actual)
        {
            for (int i = 0; i < testCount; i++)
            {
                var e = Random.NextDouble() * Random.Next(-500, 501);
                var input = EDecimal.FromDouble(e);
                e = expected(e);
                var a = actual(input, context);
                AssertTest(input, e, a);
            }
        }

        [TestMethod] public void TestMethodAsin() => Test(Math.Asin, PeterONumbersExtensions.Asin);
        [TestMethod] public void TestMethodAcos() => Test(Math.Acos, PeterONumbersExtensions.Acos);
        [TestMethod] public void TestMethodAtan() => Test(Math.Atan, PeterONumbersExtensions.Atan);
        [TestMethod] public void TestMethodSin() => Test(Math.Sin, PeterONumbersExtensions.Sin);
        [TestMethod] public void TestMethodCos() => Test(Math.Cos, PeterONumbersExtensions.Cos);
        [TestMethod] public void TestMethodTan() => Test(Math.Tan, PeterONumbersExtensions.Tan);
        [TestMethod] public void TestMethodSinh() => Test(Math.Sinh, PeterONumbersExtensions.Sinh);
        [TestMethod] public void TestMethodCosh() => Test(Math.Cosh, PeterONumbersExtensions.Cosh);
        [TestMethod] public void TestMethodTanh() => Test(Math.Tanh, PeterONumbersExtensions.Tanh);
        [TestMethod]
        public void TestMethodAtan2()
        {
            for (int i = 0; i < testCount; i++)
            {
                double x = Random.NextDouble();
                double y = Random.NextDouble();
                EDecimal dx = EDecimal.FromDouble(x);
                EDecimal dy = EDecimal.FromDouble(y);
                var e = Math.Atan2(y, x);
                var a = dy.Atan2(dx, context);
                AssertTest((y, x), e, a);
            }
        }
        [TestMethod]
        public void ImpreciseSin() // This is why we need a large precision
        {
            var input = 248.1858140380055601781350560486316680908203125;
            var expected = Math.Sin(input);
            var @decimal = EDecimal.FromDouble(input);
            var actual = @decimal.Sin(context);
            AssertTest(EDecimal.FromDouble(input), expected, actual);
        }
        [TestMethod]
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

        [TestMethod]
        public void FactorialInt()
        {
            Assert.AreEqual(EInteger.FromInt32(1), PeterONumbersExtensions.Factorial(0));
            Assert.AreEqual(EInteger.FromInt32(1), PeterONumbersExtensions.Factorial(1));
            Assert.AreEqual(EInteger.FromInt32(2), PeterONumbersExtensions.Factorial(2));
            Assert.AreEqual(EInteger.FromInt32(6), PeterONumbersExtensions.Factorial(3));
            Assert.AreEqual(EInteger.FromInt32(24), PeterONumbersExtensions.Factorial(4));
            Assert.AreEqual(EInteger.FromInt32(120), PeterONumbersExtensions.Factorial(5));

            Assert.AreEqual(
                    EInteger.FromString("9425947759838359420851623124482936749562312794702543768327889353416977599316221476503087861591808346911623490003549599583369706302603264000000000000000000000000"),
                    PeterONumbersExtensions.Factorial(101));

            var expected = EInteger.One;
            for (int n = 1; n < 1000; n++)
            {
                expected *= n;
                Assert.AreEqual(expected, PeterONumbersExtensions.Factorial(n));
            }
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => PeterONumbersExtensions.Factorial(-1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => PeterONumbersExtensions.Factorial(-2));
        }

        // Results from wolframalpha.com:
        // Relative pos  v 3    v 10           v 25                     v 50    (Shift to right by 1 for 0.5!)            v 100
        // 1.5!    =  1.3293403881791370204736256125058588870981620920917903461603558423896834634432741360312129925539084990621701
        // 0.5!    =  0.886226925452758013649083741670572591398774728061193564106903894926455642295516090687475328369272332708113411812
        // (-0.5)! =  1.77245385090551602729816748334114518279754945612238712821380778985291128459103218137495065673854466541622682362428
        // (-1.5)! = -3.544907701811032054596334966682290365595098912244774256427615579705822569182064362749901313477089330832453647248
        [DataTestMethod]
        [DataRow("0", 100, "1")]
        [DataRow("1", 100, "1")]
        [DataRow("2", 100, "2")]
        [DataRow("3", 100, "6")]
        [DataRow("4", 100, "24")]
        [DataRow("5", 100, "120")]
        [DataRow("-1", 100, "NaN")]
        [DataRow("-2", 100, "NaN")]
        [DataRow("1.5", 3, "1.33")]
        [DataRow("1.5", 10, "1.329340388")]
        [DataRow("1.5", 25, "1.329340388179137020473626")]
        [DataRow("1.5", 50, "1.3293403881791370204736256125058588870981620920918")]
        [DataRow("1.5", 100, "1.329340388179137020473625612505858887098162092091790346160355842389683463443274136031212992553908499")]
        [DataRow("0.5", 3, "0.886")]
        [DataRow("0.5", 10, "0.8862269255")]
        [DataRow("0.5", 25, "0.8862269254527580136490837")]
        [DataRow("0.5", 50, "0.88622692545275801364908374167057259139877472806119")]
        [DataRow("0.5", 100, "0.8862269254527580136490837416705725913987747280611935641069038949264556422955160906874753283692723327")]
        [DataRow("-0.5", 3, "1.77")]
        [DataRow("-0.5", 10, "1.772453851")]
        [DataRow("-0.5", 25, "1.772453850905516027298167")]
        [DataRow("-0.5", 50, "1.7724538509055160272981674833411451827975494561224")]
        [DataRow("-0.5", 100, "1.772453850905516027298167483341145182797549456122387128213807789852911284591032181374950656738544665")]
        [DataRow("-1.5", 3, "-3.54")]
        [DataRow("-1.5", 10, "-3.544907702")]
        [DataRow("-1.5", 25, "-3.544907701811032054596335")]
        [DataRow("-1.5", 50, "-3.5449077018110320545963349666822903655950989122448")]
        [DataRow("-1.5", 100, "-3.544907701811032054596334966682290365595098912244774256427615579705822569182064362749901313477089331")]
        public void Factorial(string input, int precision, string output)
        {
            Assert.AreEqual(output, EDecimal.FromString(input).Factorial(EContext.ForPrecision(precision)).ToString());
            Assert.AreEqual(output, EDecimal.FromString(input).Increment().Gamma(EContext.ForPrecision(precision)).ToString());
        }
    }
}
