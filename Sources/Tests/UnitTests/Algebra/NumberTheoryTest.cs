using AngouriMath;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static AngouriMath.Entity.Number;

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
    }
}
