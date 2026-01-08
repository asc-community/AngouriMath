//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;
using Xunit;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Tests.Algebra
{
    public sealed class SolveEquationWithPiecewise
    {
        [Theory]
        [InlineData("piecewise((x + 2)2 provided y > 0, (x + 1)2 provided y = 0, x2 provided y < 0) = 9",
            "1 provided y > 0", "-5 provided y > 0", 
            "2 provided y = 0 and not y > 0", "-4 provided y = 0 and not y > 0",
            "3 provided y < 0 and (not y > 0 and not y = 0)", "-3 provided y < 0 and (not y > 0 and not y = 0)")]
        public void CheckIfRootWasObtained(string eq,
            string? r1 = null, string? r2 = null,
            string? r3 = null, string? r4 = null,
            string? r5 = null, string? r6 = null)
        {
            var sols = eq.Solve("x");
            foreach (var expectedRoot in new[] { r1, r2, r3, r4, r5, r6 })
                if (expectedRoot is not null)
                    Assert.True(((FiniteSet)sols).Contains(expectedRoot), $"Root {expectedRoot} expected to be in {sols}");
        }
        [Theory]
        [InlineData("((x + a) provided a > 0) = 5", "{ -(a + -5) provided a > 0 }")] // Linear with provided, should preserve condition
        [InlineData("1 / x = 2", "{ 1/2 }")] // Division with implicit condition x ≠ 0
        //[InlineData("a / x = 2", "{ a / 2 provided not a = 0 }", Skip = "TODO")] // Division with implicit condition x ≠ 0
        //[InlineData("x + x / x = y", "{ -(1 + -y) provided not -(1 + -y) = 0}", Skip = "TODO")] // Partial function
        [InlineData("x ^ (a provided x > 0) = 8", "{ 8 ^ (1 / a) provided 8 ^ (1 / a) > 0 }")] // Power with possible domain restrictions
        [InlineData("(x ^ 2 provided x > 0) = 4", "{ 2 }")] // Quadratic with provided condition
        [InlineData("log(b, x) = 2", "{ b ^ 2 }")] // TODO: Logarithm should preserve domain conditions
        [InlineData("(x! provided x > 7) = 6", "{  }")] // Factorial with domain restrictions
        public void SolveWithProvidedPreservesConditions(string equation, string result)
        {
            var solutions = equation.Solve("x");
            Assert.IsType<FiniteSet>(solutions);
            Assert.Equal(result, solutions.ToString());
        }
        [Theory]
        [InlineData("((x + a) provided a > 0) + ((x + b) provided b < 0) = 10", "{ -(a + b + -10) / 2 provided a > 0 and b < 0 }")]
        [InlineData("((x + a) provided a > 0) + (x + b) = (10 provided b < 0)", "{ -(a + b + -10) / 2 provided a > 0 and b < 0 }")]
        [InlineData("((x + 1) provided x > 5) + ((x + 2) provided x < 0) = 10", "{  }")] // Conflicting conditions
        [InlineData("((x + a) provided a > 0) + ((x + b) provided a > 0) = 10", "{ -(a + b + -10) / 2 provided a > 0 }")] // Duplicate conditions
        public void SolveWithMultipleProvidedConditions(string equation, string result)
        {
            var solutions = equation.Solve("x");
            Assert.IsType<FiniteSet>(solutions);
            Assert.Equal(result, solutions.ToString());
        }

        [Theory]
        [InlineData("(x + 1) = 5 provided x >= 5", "{  }")]
        [InlineData("(x + 1) = 5 provided x >= 4", "{ 4 }")]
        [InlineData("(x + 1) = 5 provided x > 0", "{ 4 }")]
        [InlineData("x ^ 2 = 4 provided x > 0", "{ 2 }")]
        [InlineData("x ^ 2 = 4 provided x < 0", "{ -2 }")]
        [InlineData("x provided x", "{ True }")]
        //[InlineData("not x provided not x", "{ False }", Skip = "TODO")]
        public void SolveWithProvidedInEquation(string equation, string result)
        {
            var solutions = equation.Solve("x");
            Assert.IsType<FiniteSet>(solutions);
            Assert.Equal(result, solutions.ToString());
        }
    }
}
