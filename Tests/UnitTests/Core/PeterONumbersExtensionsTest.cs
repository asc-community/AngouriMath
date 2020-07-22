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
        static readonly EDecimal precision = 2e-16m;
        static readonly EContext context = MathS.Settings.DecimalPrecisionContext;
        static readonly int testCount = 1000;
        static readonly Random Random = new Random();

        void AssertTest(object input, double e, EDecimal a) =>
            Assert.IsTrue(
                double.IsNaN(e) && a.IsNaN()
                || double.IsPositiveInfinity(e) && a.IsPositiveInfinity()
                || double.IsNegativeInfinity(e) && a.IsNegativeInfinity()
                || e == 0 && a.IsZero
                || (EDecimal.FromDouble(e) - a).Abs().LessThan(EDecimal.FromDouble(e).Abs() * precision),
                $"\nInput: {input}\nExpected: {e}\nActual: {a}");

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
        public void DebugWithMe()
        {
            var input = 0.51036466588748841122225030630943365395069122314453125;
            var expected = Math.Acos(input);
            var @decimal = EDecimal.FromDouble(input);
            var actual = @decimal.Acos(context);
            AssertTest(EDecimal.FromDouble(input), expected, actual);
        }
    }
}