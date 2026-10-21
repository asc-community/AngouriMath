//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath.Extensions;
using AngouriMath.Functions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// The Hermite reductions of a power of <c>a + b cosh(x) + c sinh(x)</c> under <c>u = e^x</c>
    /// are systems in three symbols whose fraction-free elimination over <c>Q</c> declines: its
    /// image modulo a prime says so first, and the rule after the decline answers. Rubi's 6.7.1.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AnEliminationDeclinedModuloAPrimeIntegralTest
    {
        [Theory]
        [InlineData("1/(a*cosh(x) + b*sinh(x))^3")]
        [InlineData("1/(a*cosh(x) + b*sinh(x))^5")]
        [InlineData("(A + C*sinh(x))/(b*cosh(x) + c*sinh(x))^3")]
        [InlineData("1/(a + b*cosh(x) + c*sinh(x))^3")]
        [InlineData("1/(a + a*cosh(x) + c*sinh(x))^3")]
        [InlineData("(B*cosh(x) + C*sinh(x))/(a + b*cosh(x) + c*sinh(x))^2")]
        public void APowerOfALinearInTheHyperbolicCosineAndSine(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4)
                .Substitute("A", 0.9).Substitute("B", 1.7).Substitute("C", 0.6);
            // The constant of integration is C, or a fresh name beside the integrand's C; either
            // way it is constant in x.
            var derivative = Pinned(integral).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -1.2, -0.7, 0.3, 0.8, 1.3, 2.1 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                if (want.IsNaN)
                    continue;
                compared++;
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand}");
        }

        /// <summary>
        /// A system large enough to be eliminated modulo the prime first, and solvable: the
        /// image declines nothing, and the elimination over <c>Q</c> answers.
        /// </summary>
        [Fact]
        public void ASolvableSystemIsStillSolved()
        {
            // Seven unknowns, lower triangular with sums of symbols on the diagonal.
            var size = 7;
            var matrix = new Entity[size][];
            var rhs = new Entity[size];
            for (var row = 0; row < size; row++)
            {
                matrix[row] = new Entity[size];
                for (var column = 0; column < size; column++)
                    matrix[row][column] = column > row ? 0
                        : column == row ? $"a + {row + 1} b".ToEntity()
                        : $"c + {row - column}".ToEntity();
                rhs[row] = $"a b + {row}".ToEntity();
            }
            Assert.True(PartialFractions.TrySolveOverPolynomials(matrix, rhs, out var values));
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4);
            for (var row = 0; row < size; row++)
            {
                Entity sum = 0;
                for (var column = 0; column < size; column++)
                    sum += matrix[row][column] * values![column];
                var difference = Pinned(sum - rhs[row]).EvalNumerical();
                Assert.True(Math.Abs((double)difference.RealPart) < 1e-9, $"row {row} is off by {difference}");
            }
        }

        /// <summary>
        /// And one that is not: two rows ask one combination of the unknowns for two different
        /// values, which the image finds and the elimination over <c>Q</c> declines.
        /// </summary>
        [Fact]
        public void AnInconsistentSystemIsDeclined()
        {
            var size = 7;
            var matrix = new Entity[size][];
            var rhs = new Entity[size];
            for (var row = 0; row < size; row++)
            {
                matrix[row] = new Entity[size];
                for (var column = 0; column < size; column++)
                    matrix[row][column] = row >= size - 2 ? $"a + {column + 1} b".ToEntity()
                        : column == row ? $"a + {row + 1} b".ToEntity()
                        : 0;
                rhs[row] = $"c + {row}".ToEntity();
            }
            Assert.False(PartialFractions.TrySolveOverPolynomials(matrix, rhs, out _));
        }
    }
}
