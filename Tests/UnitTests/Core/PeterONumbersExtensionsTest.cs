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
    }
}
