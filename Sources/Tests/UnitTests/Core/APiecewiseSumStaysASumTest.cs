//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Linq;
using AngouriMath.Extensions;
using Xunit;
using static AngouriMath.Entity;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// A sum of two piecewises whose cases do not line up stays a sum of the two: combined case
    /// by case it lists every case of one against every case of the other.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1874">#1874</a>
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class APiecewiseSumStaysASumTest
    {
        private static Entity ThreeSigns(string of, string positive, string zero, string negative)
            => $"piecewise({positive} provided {of} > 0, {zero} provided {of} = 0, {negative} provided {of} < 0)".ToEntity();

        [Fact]
        public void CasesOnDifferentQuantitiesStayASum()
        {
            var sum = (ThreeSigns("a", "1", "2", "3") + ThreeSigns("b", "x", "y", "z")).InnerSimplified;
            var asASum = Assert.IsType<Sumf>(sum);
            Assert.IsType<Piecewise>(asASum.Augend);
            Assert.IsType<Piecewise>(asASum.Addend);
        }

        [Fact]
        public void CasesOnOneQuantityStillCombine()
        {
            // The three that agree and the two opposite strict signs: five, not nine.
            var sum = (ThreeSigns("a", "1", "2", "3") + ThreeSigns("a", "x", "y", "z")).InnerSimplified;
            Assert.IsType<Piecewise>(sum);
        }

        [Fact]
        public void ASumHoldingAPiecewiseIsNotCopiedIntoTheCasesOfAnother()
        {
            var sum = (ThreeSigns("a", "1", "2", "3") + ThreeSigns("b", "x", "y", "z") + ThreeSigns("c", "u", "v", "w")).InnerSimplified;
            Assert.Equal(3, sum.Nodes.Count(node => node is Piecewise));
        }

        [Fact]
        public void TheIssuesIntegralIsTheSumOfItsTerms()
        {
            Entity one = "1/(b*x^2 + m)";
            Entity two = "1/(sqrt(a + b*x^2)*(b*x^2 + m))";
            var whole = (one - two).Integrate("x").Stringize().Length;
            var apart = one.Integrate("x").Stringize().Length + two.Integrate("x").Stringize().Length;
            Assert.True(whole <= apart + 16, $"{whole} characters for the difference, {apart} for its terms");
        }
    }
}
