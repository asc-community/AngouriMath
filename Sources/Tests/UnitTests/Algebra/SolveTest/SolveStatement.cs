//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using Xunit;

namespace AngouriMath.Tests.Algebra.SolveTest
{
    [Trait("Area", "Algebra")]
    public sealed class SolveStatement
    {
        [Theory]
        [InlineData("x2 = 3 and x > 0", "{ sqrt(3) }")]
        [InlineData("x4 = 3 and x in RR", "{ -3^(1/4), 3^(1/4) }")]
        public void TestStatementSolver(string statement, string expectedRaw)
        {
            var expr = MathS.FromString(statement);
            var expected = MathS.FromString(expectedRaw);
            var actual = expr.Solve("x");
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// A statement equal to a truth value is solved as the statement or its negation, and two
        /// statements equal as their equivalence, compared at points. Each was solved to the
        /// empty set. https://github.com/asc-community/AngouriMath/issues/1549
        /// </summary>
        [Theory]
        [InlineData("(x > 2) = (3 > 1)", "3 5/2", "2 0")]
        [InlineData("(x = 1) = true", "1", "2 0")]
        [InlineData("true = (x in {1, 2})", "1 2", "3")]
        [InlineData("(x > 2) = false", "2 0", "3")]
        [InlineData("(2 divides x) = true", "4 -2", "3")]
        [InlineData("(x = 1 (mod 3)) = true", "1 4 -2", "2 3")]
        [InlineData("(x > 0) = (x > 1)", "2 -1 0", "1/2 1")]
        public void AStatementEqualToATruthValueIsSolved(string statement, string inside, string outside)
        {
            var solved = MathS.FromString(statement, useCache: false).Solve("x");
            foreach (var (points, expected) in new[] { (inside, true), (outside, false) })
                foreach (var point in points.Split(' '))
                {
                    Assert.True(solved.TryContains(MathS.FromString(point), out var contains), $"{solved} does not decide {point}");
                    Assert.Equal(expected, contains);
                }
        }
    }
}
