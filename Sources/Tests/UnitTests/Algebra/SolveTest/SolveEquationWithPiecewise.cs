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
    }
}
