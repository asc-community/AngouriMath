//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Algebra
{
    /// <summary>
    /// A numeric root that lands near a simple ratio is rewritten as that ratio. These
    /// pin when that is allowed to happen, since a ratio is read as an exact answer.
    /// </summary>
    [Trait("Area", "Algebra")]
    public sealed class RootDowncastTest
    {
        private static Entity SingleRoot(string equation) =>
            Assert.Single((Entity.Set.FiniteSet)equation.ToEntity().Solve("x"));

        // https://github.com/asc-community/AngouriMath/issues/235
        // x^41 + 6x + 1 = 0 answered -1/6, which is not a root: it leaves
        // -1/80204967233062404407033075859456. The tolerance that guesses the ratio was
        // also deciding whether the guess was right, and at 1e-7 a residual of 1.25e-32
        // passes for zero.
        [Fact]
        public void Issue235_NearRationalRootIsNotPresentedAsExact()
        {
            var root = SingleRoot("x + x + x + x + x + x + x41 + 1 = 0");
            Assert.False(root is Entity.Number.Rational,
                $"root came back as the exact {root.Stringize()}, which is not a root");
            // It is still the right root, to the accuracy the numeric solver has.
            Assert.Equal(-1.0 / 6, root.EvalNumerical().RealPart.EDecimal.ToDouble(), 15);
        }

        [Fact]
        public void Issue235_TheRejectedRatioReallyIsNotARoot() =>
            Assert.NotEqual(Entity.Number.Integer.Create(0),
                "x + x + x + x + x + x + x41 + 1".ToEntity().Substitute("x", "-1/6".ToEntity()).Evaled);

        // Roots that genuinely are ratios must still come back as ratios, or the fix has
        // traded one wrong answer for uglier right ones.
        [Theory]
        [InlineData("2 * x - 1 = 0", "1/2")]
        [InlineData("x ^ 2 - 1/4 = 0", "1/2")]
        [InlineData("3 * x + 2 = 0", "-2/3")]
        [InlineData("x ^ 2 + 2 * x + 1 = 0", "-1")]
        public void GenuineRationalRootsStayExact(string equation, string expected)
        {
            var roots = (Entity.Set.FiniteSet)equation.ToEntity().Solve("x");
            Assert.Contains(expected.ToEntity().InnerSimplified, roots);
        }

        // And irrational ones must not be rounded into a ratio either.
        [Fact]
        public void IrrationalRootsStayIrrational()
        {
            var roots = (Entity.Set.FiniteSet)"x ^ 2 - 2 = 0".ToEntity().Solve("x");
            Assert.Contains("sqrt(2)".ToEntity().InnerSimplified, roots);
        }
    }
}
