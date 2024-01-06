//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using Xunit;
using Xunit.Abstractions;

namespace UnitTests.Algebra.SolveTest
{
    public sealed class SolvableButReturnsNull
    {
        private readonly ITestOutputHelper output;
        public SolvableButReturnsNull(ITestOutputHelper output)
        {
                this.output = output;
        }

        [Theory]
        [InlineData("x - y - 1000", "y")]
        [InlineData("x - y- 1000", "y - 0")]
        [InlineData("x - y- 1000", "y -1 + 1")]
        [InlineData("x - y- 1000", "y - 50")]
        public void ShouldResolveAll(string expr1, string expr2)
        {
            output.WriteLine(expr1);
            output.WriteLine(expr2);
            var system = MathS.Equations(expr1, expr2);
            var result = system.Solve("x","y");
            output.WriteLine(result.Stringize());
            Assert.NotNull(result);
        }
    }
}
