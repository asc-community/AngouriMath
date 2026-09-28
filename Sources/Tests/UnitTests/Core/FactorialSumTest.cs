//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// A polynomial times the factorial of the index, summed where the sum telescopes:
    /// Sullivan and Mackey's Prob 2.7.17, <c>sum(k k!, k, 1, n) = (n + 1)! - 1</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1409">#1409</a>
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class FactorialSumTest
    {
        [Theory]
        [InlineData("sum(k * factorial(k), k, 1, n)", "factorial(n + 1) - 1")]
        [InlineData("sum(k * factorial(k), k, 0, n)", "factorial(n + 1) - 1")]
        [InlineData("sum((k^2 + k + 1) * factorial(k), k, 1, n)", "(n + 1) * factorial(n + 1) - 1")]
        [InlineData("sum(c * k * factorial(k), k, 2, n)", "c * (factorial(n + 1) - 2)")]
        public void ATelescopingSumIsClosed(string sum, string expected)
        {
            var closed = sum.ToEntity().Simplify();
            Assert.IsNotType<Entity.Summationf>(closed);
            foreach (var n in new[] { 0, 1, 2, 3, 6, 11 })
            {
                var want = sum.ToEntity().Substitute("n", n).Substitute("c", 3).Simplify().Evaled;
                Assert.Equal(want, closed.Substitute("n", n).Substitute("c", 3).Simplify().Evaled);
                if (n >= 1)
                    Assert.Equal(want, expected.ToEntity().Substitute("n", n).Substitute("c", 3).Simplify().Evaled);
            }
        }

        /// <summary>
        /// Where no polynomial certificate exists the sum is left as written: the left factorial
        /// <c>sum(k!, k, 0, n)</c> has no closed form in factorials, and neither has <c>sum(k^2 k!)</c>.
        /// </summary>
        [Theory]
        [InlineData("sum(factorial(k), k, 0, n)")]
        [InlineData("sum(k^2 * factorial(k), k, 1, n)")]
        public void ASumThatDoesNotTelescopeIsLeft(string sum)
            => Assert.IsType<Entity.Summationf>(sum.ToEntity().Simplify());
    }
}
