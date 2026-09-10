//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Diagnostics;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// A long sum is split before anything searches it, so integrating a polynomial costs about
    /// what integrating its terms costs.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Splitting a sum used to run fifth, behind the logarithm, two substitutions and partial
    /// fractions, each of which searched the <em>whole</em> sum first. The terms of
    /// <c>1 + 2x + ... + 25x^24</c> integrate in under a millisecond between them; the sum they
    /// add up to took twenty seconds, and every four more degrees roughly tripled it.
    /// </para>
    /// <para>
    /// <b>What this test binds is the shape of the cost, not a number of milliseconds.</b> The
    /// wall below is minutes where the work is a fraction of a second — it cannot fire on a slow
    /// runner, only on the growth coming back. A degree-40 polynomial on the old ordering
    /// extrapolates to something over twenty minutes.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class PolynomialSumIntegralTest
    {
        private static Entity Polynomial(int degree)
        {
            Entity built = 1;
            for (var power = 1; power <= degree; power++)
                built += (power + 1) * MathS.Pow(MathS.Var("x"), power);
            return built;
        }

        /// <summary>
        /// Degree 40, which is far past where the old ordering stopped returning in any useful
        /// time. The bound is deliberately enormous: this is a test about growth.
        /// </summary>
        [Fact]
        public void ALongPolynomialIsIntegratedWithoutSearchingIt()
        {
            var integrand = Polynomial(40);
            var watch = Stopwatch.StartNew();
            var integral = integrand.Integrate("x");
            watch.Stop();

            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.True(watch.Elapsed < TimeSpan.FromMinutes(2),
                $"a degree-40 polynomial took {watch.Elapsed.TotalSeconds:0.#} s to integrate, "
                + "which is the growth this test exists to catch rather than a slow machine");
        }

        /// <summary>
        /// And the answer is right, at several degrees, checked by differentiating back — a
        /// faster wrong answer would satisfy the test above on its own.
        /// </summary>
        [Theory]
        [InlineData(4)]
        [InlineData(12)]
        [InlineData(24)]
        [InlineData(40)]
        public void AndTheAnswerDifferentiatesBack(int degree)
        {
            var integrand = Polynomial(degree);
            var integral = integrand.Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());

            var derivative = integral.Substitute("C", 0).Differentiate("x");
            foreach (var at in new[] { 0.3, 0.7, 1.1 })
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = integrand.Substitute("x", at).EvalNumerical();
                var difference = Math.Abs((double)(got - want).RealPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of the degree-{degree} polynomial is {got} "
                    + $"at x = {at}, where the integrand is {want}");
            }
        }

        /// <summary>
        /// The other half of the ordering, and the reason the early split reads only a sum. A
        /// product with a sum in it is a rewrite away from being one, and expanding it that early
        /// takes the answer away from the substitution that gives the shorter one — these come
        /// out as a power, not as a written-out polynomial.
        /// </summary>
        [Theory]
        [InlineData("x * (x ^ 2 + 1) ^ 3", "C + (x ^ 2 + 1) ^ 4 / 8")]
        [InlineData("x * (x ^ 2 + 1) ^ 10", "C + (x ^ 2 + 1) ^ 11 / 22")]
        public void AProductWithASumInItKeepsItsCompactAnswer(string integrand, string expected)
            => Assert.Equal(expected.ToEntity().Simplify(),
                            integrand.ToEntity().Integrate("x").Simplify());
    }
}
