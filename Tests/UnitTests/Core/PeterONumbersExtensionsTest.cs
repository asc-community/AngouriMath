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
        static readonly EDecimal epsilon = 3e-16m;
        static readonly EContext context = MathS.Settings.DecimalPrecisionContext;
        static readonly int testCount = 1000;
        static readonly Random Random = new Random();

        void Test(Func<double, double> expected, Func<EDecimal, EContext, EDecimal> actual)
        {
            for (int i = 0; i < testCount; i++)
            {
                var d = Random.NextDouble();
                var input = EDecimal.FromDouble(d);
                d = expected(d);
                var d1 = actual(input, context);
                Assert.IsTrue((EDecimal.FromDouble(d) - d1).Abs().LessThan(epsilon),
                    $"\nInput: {input}\nExpected: {d}\nActual: {d1}");
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
                var d = Math.Atan2(y, x);
                var z = dy.Atan2(dx, context);
                Assert.IsTrue((EDecimal.FromDouble(d) - z).Abs().LessThan(epsilon),
                    $"\nInput y: {dy}\nInput x: {dx}\nExpected: {d}\nActual: {z}");
            }
        }
        [TestMethod]
        public void DebugWithMe()
        {
            var input = 0.51036466588748841122225030630943365395069122314453125;
            var expected = Math.Acos(input);
            var actual = EDecimal.FromDouble(input).Acos(context);
            Assert.IsTrue((EDecimal.FromDouble(expected) - actual).Abs().LessThan(epsilon));
        }
    }
}