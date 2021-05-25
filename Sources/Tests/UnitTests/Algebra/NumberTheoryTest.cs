using AngouriMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using static AngouriMath.Entity.Number;
using AngouriMath.Extensions;

namespace UnitTests.Algebra
{
    public sealed class NumberTheoryTest
    {
        [Theory]
        [InlineData(8633, 8051, 97, 14, -15)]
        [InlineData(8633, 8051, 194, 28, -30)]
        [InlineData(8633, 8051, -97, -14, 15)]
        [InlineData(8633, -8051, 97, 14, 15)]
        [InlineData(-8633, 8051, 97, -14, -15)]
        public void DiophantineEquationTest(int a, int b, int c, int x, int y)
        {
            Integer A = a; Integer B = b; Integer C = c;
            Integer X = x; Integer Y = y;

            var sol = MathS.NumberTheory.SolveDiophantineEquation(A, B, C);
            Assert.Equal((X, Y), sol.ShouldBeNotNull());
        }

        [Theory]
        [InlineData("1/3 + 1/2", 2)]
        public void RationalDecompositionTest(string rationalRaw, int count)
        {
            var input = (Rational)rationalRaw.ToEntity().InnerSimplified;
            var decomposed = MathS.NumberTheory.DecomposeRational(input);
            Assert.Equal(count, decomposed.Count());
            var sum = decomposed.Select(c => c.numerator / c.denPrime.Pow(c.denPower)).Aggregate((a, b) => a + b).InnerSimplified;
            Assert.Equal(input, sum);
        }
    }
}
