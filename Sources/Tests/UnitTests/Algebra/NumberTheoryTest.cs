//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using System.Linq;
using Xunit;
using static AngouriMath.Entity.Number;
using AngouriMath.Extensions;

namespace AngouriMath.Tests.Algebra
{
    public sealed class NumberTheoryTest
    {
        [Theory]
        [InlineData(8633, 8051, 97, 14, -15)]
        [InlineData(8633, 8051, 194, 28, -30)]
        [InlineData(8633, 8051, -97, -14, 15)]
        [InlineData(8633, -8051, 97, 14, 15)]
        [InlineData(-8633, 8051, 97, -14, -15)]
        [InlineData(4, 3, 7, 1, 1)]
        [InlineData(3, 17, 37, 1, 2)]
        [InlineData(2, 5, 7, 1, 1)]
        [InlineData(5, 2, 7, 1, 1)]
        [InlineData(-2, 5, 7, -1, 1)]
        [InlineData(2, -5, 7, 1, -1)]
        [InlineData(-2, -5, 7, -1, -1)]
        [InlineData(2, 5, -7, -1, -1)]
        [InlineData(16, 7, 2, 1, -2)]
        [InlineData(7, 16, 2, -2, 1)]
        public void DiophantineEquationTest(int a, int b, int c, int x, int y)
        {
            Integer A = a; Integer B = b; Integer C = c;
            Integer X = x; Integer Y = y;

            var sol = MathS.ExperimentalFeatures.SolveDiophantineEquation(A, B, C);
            Assert.Equal((X, Y), sol.ShouldBeNotNull());
        }

        [Theory]
        [InlineData("1/3 + 1/2", 2)]
        [InlineData("1/7 + 1/2", 2)]
        [InlineData("1/3", 1)]
        [InlineData("1/9", 1)]
        [InlineData("1/17 + 1/3", 2)]
        [InlineData("2/17 + 1/3", 2)]
        [InlineData("5/17 + 1/3", 2)]
        [InlineData("1/(17^2) + 1/3", 2)]
        [InlineData("1/3 + 1/9 + 1/27 + 1/81", 1)]
        [InlineData("11/3 + 4/9 + 5/27 + 7/81", 1)]
        [InlineData("1/17 - 1/3", 2)]
        [InlineData("-2/17 + 1/3", 2)]
        [InlineData("-1/17 - 2/3", 2)]
        [InlineData("1", 1)]
        public void RationalDecompositionTest(string rationalRaw, int count)
        {
            var input = (Rational)rationalRaw.ToEntity().InnerSimplified;
            var decomposed = MathS.ExperimentalFeatures.DecomposeRational(input).ToArray();
            Assert.True(count == decomposed.Length, $"Expected: {count}\nActual: {decomposed.Length}\nElements: {string.Join(", ", decomposed)}");
            var sum = decomposed.Select(c => c.numerator / c.denPrime.Pow(c.denPower)).Aggregate((a, b) => a + b).InnerSimplified;
            Assert.Equal(input, sum);
        }
    }
}
