//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using Xunit;
using AngouriMath.Extensions;
using System.Globalization;

namespace AngouriMath.Tests.Algebra
{
    public sealed class FunctionTest
    {
        // Testing function GetAllRoots
        // TODO: make it via [Theory]
        [Fact]
        public void TestRoots0()
        {
            var num = 3;
            var pow = 3;
            foreach (var root in Entity.Number.GetAllRoots(num, pow))
                Assert.Equal(num, Entity.Number.Pow(root, pow));
        }
        [Fact]
        public void TestRoots1()
        {
            var num = 5 + MathS.i * 5;
            var pow = 4;
            foreach (var root in Entity.Number.GetAllRoots(num, pow))
                Assert.Equal(num, Entity.Number.Pow(root, pow));
        }
        [Fact]
        public void TestRoots2()
        {
            var num = -3 + MathS.i * 8;
            var pow = 5;
            foreach (var root in Entity.Number.GetAllRoots(num, pow))
                Assert.Equal(num, Entity.Number.Pow(root, pow));
        }
        [Fact]
        public void TestRoots3()
        {
            var num = -3 + MathS.i * 8;
            var pow = 8;
            foreach (var root in Entity.Number.GetAllRoots(num, pow))
                Assert.Equal(num, Entity.Number.Pow(root, pow));
        }

        // Testing functions of Base Convert
        [Fact]
        public void TestBaseConvertTo0()
        {
            Assert.Equal("101", MathS.ToBaseN(5, 2));
            Assert.Equal("F", MathS.ToBaseN(15, 16));
        }

        [Theory]
        [InlineData("-101",      -5, 2   )]
        [InlineData("-F",        -15, 16 )]
        [InlineData("-10001100", -140, 2 )]
        [InlineData("-12012",    -140, 3 )]
        [InlineData("-2030",     -140, 4 )]
        [InlineData("-1030",     -140, 5 )]
        [InlineData("-352",      -140, 6 )]
        [InlineData("-260",      -140, 7 )]
        [InlineData("-214",      -140, 8 )]
        [InlineData("-165",      -140, 9 )]
        [InlineData("-140",      -140, 10)]
        [InlineData("-118",      -140, 11)]
        [InlineData("-B8",       -140, 12)]
        [InlineData("-AA",       -140, 13)]
        [InlineData("-A0",       -140, 14)]
        [InlineData("-95",       -140, 15)]
        [InlineData("-8C",       -140, 16)]
        public void TestBaseConvertTo1(string expected, int number, int system)
        {
            Assert.Equal(expected, MathS.ToBaseN(number, system));
        }

        [Theory]
        [InlineData("1000.001", "8.125", 2 )]
        [InlineData("20.02",    "8.125", 4 )]
        [InlineData("12.043",   "8.125", 6 )]
        [InlineData("10.1",     "8.125", 8 )]
        [InlineData("8.125",    "8.125", 10)]
        [InlineData("8.16",     "8.125", 12)]
        [InlineData("8.1A7",    "8.125", 14)]
        [InlineData("8.2",      "8.125", 16)]
        [InlineData("1000.111", "8.875", 2 )]
        [InlineData("20.32",    "8.875", 4 )]
        [InlineData("12.513",   "8.875", 6 )]
        [InlineData("10.7",     "8.875", 8 )]
        [InlineData("8.875",    "8.875", 10)]
        [InlineData("8.A6",     "8.875", 12)]
        [InlineData("8.C37",    "8.875", 14)]
        [InlineData("8.E",      "8.875", 16)]
        public void TestBaseConvertTo2(string expected, string decimalInString, int system)
        {
            Assert.Equal(expected, MathS.ToBaseN(decimal.Parse(decimalInString, CultureInfo.InvariantCulture), system));
        }

        [Theory]
        [InlineData("-1000.001", "-8.125", 2 )]
        [InlineData("-20.02",    "-8.125", 4 )]
        [InlineData("-12.043",   "-8.125", 6 )]
        [InlineData("-10.1",     "-8.125", 8 )]
        [InlineData("-8.125",    "-8.125", 10)]
        [InlineData("-8.16",     "-8.125", 12)]
        [InlineData("-8.1A7",    "-8.125", 14)]
        [InlineData("-8.2",      "-8.125", 16)]
        [InlineData("-1000.111", "-8.875", 2 )]
        [InlineData("-20.32",    "-8.875", 4 )]
        [InlineData("-12.513",   "-8.875", 6 )]
        [InlineData("-10.7",     "-8.875", 8 )]
        [InlineData("-8.875",    "-8.875", 10)]
        [InlineData("-8.A6",     "-8.875", 12)]
        [InlineData("-8.C37",    "-8.875", 14)]
        [InlineData("-8.E",      "-8.875", 16)]
        public void TestConvertTo3(string expected, string decimalInString, int system)
        {
            Assert.Equal(expected, MathS.ToBaseN(decimal.Parse(decimalInString, CultureInfo.InvariantCulture), system));
        }

        [Fact]
        public void TestBaseConvertFrom0()
        {
            Assert.Equal(10, MathS.FromBaseN("A", 16));
            Assert.Equal(10, MathS.FromBaseN("1010", 2));
        }

        [Fact]
        public void TestBaseConvertFrom1()
        {
            Assert.Equal(-10.25m, MathS.FromBaseN("-A.4", 16));
            Assert.Equal(-140, MathS.FromBaseN("-A0", 14));
        }

        [Fact]
        public void TestBaseConvertFrom2()
        {
            Assert.Equal(-0.125m, MathS.FromBaseN("-0.125", 10));
            Assert.Equal(0.25m, MathS.FromBaseN("0.3", 12));
        }

        [Theory]
        [InlineData("sgn(4)", "1")]
        [InlineData("sgn(-4)", "-1")]
        [InlineData("sgn(45.363)", "1")]
        [InlineData("sgn(-45.363)", "-1")]
        [InlineData("sgn(+1 + i)", "+sqrt(2) / 2 + sqrt(2) / 2 * i")]
        [InlineData("sgn(+1 - i)", "+sqrt(2) / 2 - sqrt(2) / 2 * i")]
        [InlineData("sgn(-1 + i)", "-sqrt(2) / 2 + sqrt(2) / 2 * i")]
        [InlineData("sgn(-1 - i)", "-sqrt(2) / 2 - sqrt(2) / 2 * i")]
        [InlineData("sgn(sgn(4))", "1")]
        [InlineData("sgn(sgn(-4))", "-1")]
        [InlineData("sgn(sgn(45.363))", "1")]
        [InlineData("sgn(sgn(-45.363))", "-1")]
        [InlineData("sgn(sgn(+1 + i))", "+sqrt(2) / 2 + sqrt(2) / 2 * i")]
        [InlineData("sgn(sgn(+1 - i))", "+sqrt(2) / 2 - sqrt(2) / 2 * i")]
        [InlineData("sgn(sgn(-1 + i))", "-sqrt(2) / 2 + sqrt(2) / 2 * i")]
        [InlineData("sgn(sgn(-1 - i))", "-sqrt(2) / 2 - sqrt(2) / 2 * i")]
        [InlineData("sgn(0)", "0")]
        public void TestSignum(string expr, string answer)
        {
            var actual = expr.ToEntity().EvalNumerical();
            var expected = answer.ToEntity().EvalNumerical();
            var error = (actual - expected).Abs();
            Assert.True(error < MathS.Numbers.Create(1e-8m),
                $"\nError: {error.Stringize()}" + 
                $"\nActual: {actual.Stringize()}" +
                $"\nExpected: {expected.Stringize()}");
        }

        [Theory]
        [InlineData("abs(5)", "5")]
        [InlineData("abs(-5)", "5")]
        [InlineData("abs(3 + 4i)", "5")]
        [InlineData("abs(3 - 4i)", "5")]
        [InlineData("abs(abs(3 - 4i))", "5")]
        [InlineData("abs(- 3 + 4i)", "5")]
        [InlineData("abs(-sqrt(1) / 2 + sqrt(3) / 2 * i)", "1")]
        [InlineData("abs(0)", "0")]
        [InlineData("abs(-1) + 1", "2")]
        public void TestAbs(string expr, string answer)
        {
            var actual = expr.ToEntity().EvalNumerical();
            var expected = answer.ToEntity().EvalNumerical();
            var error = (actual - expected).Abs();
            Assert.True(error < MathS.Numbers.Create(1e-8m),
                $"\nError: {error.Stringize()}" +
                $"\nActual: {actual.Stringize()}" +
                $"\nExpected: {expected.Stringize()}");
        }
    }
}