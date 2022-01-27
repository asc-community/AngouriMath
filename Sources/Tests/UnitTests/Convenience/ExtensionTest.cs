//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using Xunit;
using AngouriMath.Extensions;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Tests.Convenience
{
    public class ExtensionTest
    {
        [Fact]
        public void TestSystem2()
        {
            var res = ("x", "y").SolveSystem("x", "y");
            var exp = MathS.Matrix(new Entity[,] { { 0, 0 } });
            Assert.Equal(exp, res);
        }

        [Fact]
        public void TestSystem3()
        {
            var res = ("x", "y", "z").SolveSystem("x", "y", "z");
            var exp = MathS.Matrix(new Entity[,] { { 0, 0, 0 } });
            Assert.Equal(exp, res);
        }

        [Fact]
        public void TestSystem4()
        {
            var res = ("x", "y", "z", "t").SolveSystem("x", "y", "z", "t");
            var exp = MathS.Matrix(new Entity[,] { { 0, 0, 0, 0 } });
            Assert.Equal(exp, res);
        }

        [Fact]
        public void TestSystem5()
        {
            var res = ("x", "y", "z", "t", "k").SolveSystem("x", "y", "z", "t", "k");
            var exp = MathS.Matrix(new Entity[,] { { 0, 0, 0, 0, 0 } });
            Assert.Equal(exp, res);
        }

        [Fact]
        public void TestSystem6()
        {
            var res = ("x", "y", "z", "t", "k", "p").SolveSystem("x", "y", "z", "t", "k", "p");
            var exp = MathS.Matrix(new Entity[,] { { 0, 0, 0, 0, 0, 0 } });
            Assert.Equal(exp, res);
        }

        [Fact]
        public void TestSystem7()
        {
            var res = ("x", "y", "z", "t", "k", "p", "r").SolveSystem("x", "y", "z", "t", "k", "p", "r");
            var exp = MathS.Matrix(new Entity[,] { { 0, 0, 0, 0, 0, 0, 0 } });
            Assert.Equal(exp, res);
        }

        [Fact]
        public void TestSystem8()
        {
            var res = ("x", "y", "z", "t", "k", "p", "r", "l").SolveSystem("x", "y", "z", "t", "k", "p", "r", "l");
            var exp = MathS.Matrix(new Entity[,] { { 0, 0, 0, 0, 0, 0, 0, 0 } });
            Assert.Equal(exp, res);
        }

        [Theory]
        [InlineData(10, 4)]
        [InlineData(100, 40)]
        [InlineData(12345678910, 4938271560)]
        [InlineData(3284976598, 1639347264)]
        [InlineData(1, 1)]
        [InlineData(-10, 0)]
        [InlineData(-100, 0)]
        [InlineData(-12345678910, 0)]
        [InlineData(-3284976598, 0)]
        [InlineData(-1, 0)]
        [InlineData(0, 0)]
        public void TestComputePhi(long i, long expected)
        {
            Assert.Equal(expected, ((Integer) i).Phi());
            Assert.Equal(expected, MathS.NumberTheory.Phi((Integer) i).Evaled);
        }

        [Theory]
        [InlineData("phi(10)", 4)]
        [InlineData("phi(9 + 1)", 4)]
        [InlineData("phi(10 - 100000)", 0)]
        [InlineData("phi(11 * 13 * 17 * 19)", 10 * 12 * 16 * 18)]
        [InlineData("phi(11^5)", 146410)]
        [InlineData("phi(3 * 5 * 7 * 11 * 13 * 17)", 2 * 4 * 6 * 10 * 12 * 16)]
        [InlineData("phi(3 * 5 * 7 * 11 * 13 * 17) - 33", 2 * 4 * 6 * 10 * 12 * 16 - 33)]
        [InlineData("phi(10)^phi(10)", 4 * 4 * 4 * 4)]
        public void TestEvalPhi(string expression, double expected)
        {
            Assert.Equal(expected, expression.EvalNumerical());
        }

        [Theory]
        [InlineData(56456456, 64)]
        [InlineData(154564465, 16)]
        [InlineData(1, 1)]
        [InlineData(54654654, 8)]
        [InlineData(6, 4)]
        [InlineData(4294967296, 33)]
        [InlineData(11, 2)]
        [InlineData(2, 2)]
        [InlineData(5, 2)]
        public void TestCountDivisors(long i, long expected)
        {
            Assert.Equal(expected, ((Integer)i).CountDivisors());
            Assert.Equal(expected, MathS.NumberTheory.CountDivisors(i).Evaled);
        }
    }
}
