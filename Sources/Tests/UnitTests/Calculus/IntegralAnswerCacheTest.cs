//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Diagnostics;
using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// The integrator keeps the answers it has already worked out, because it is asked the same
    /// question over and over — <c>Simplify</c> puts one integral through every rewritten
    /// candidate it generates, which measured at 562 calls for 3 distinct integrands.
    /// https://github.com/asc-community/AngouriMath/issues/1156
    /// </summary>
    /// <remarks>
    /// The tests that matter here are the ones below on the codomain. A cache like this does not
    /// fail by returning nonsense; it fails by returning a <i>correct</i> answer that was worked
    /// out under settings the caller is no longer standing in.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class IntegralAnswerCacheTest
    {
        /// <summary>
        /// <c>1/x</c> integrates to <c>ln(abs(x))</c> on the reals and <c>ln(x)</c> on the complex
        /// plane, so it separates the two codomains in one expression. Asked under each in turn,
        /// in one flow, which is where a cache that does not notice the setting would answer the
        /// second question with the first one's answer.
        /// </summary>
        [Fact]
        public void ANarrowedCodomainIsNotAnsweredFromTheWiderOne()
        {
            var complex = "1 / x".ToEntity().Integrate("x").Stringize();

            string real;
            using (MathS.Settings.Codomain.Set(Domain.Real))
                real = "1 / x".ToEntity().Integrate("x").Stringize();

            Assert.Contains("abs", real);
            Assert.DoesNotContain("abs", complex);
        }

        /// <summary>The same, asked in the other order — a cache is only sound in both.</summary>
        [Fact]
        public void AWidenedCodomainIsNotAnsweredFromTheNarrowerOne()
        {
            string real;
            using (MathS.Settings.Codomain.Set(Domain.Real))
                real = "1 / x".ToEntity().Integrate("x").Stringize();

            var complex = "1 / x".ToEntity().Integrate("x").Stringize();

            Assert.Contains("abs", real);
            Assert.DoesNotContain("abs", complex);
        }

        /// <summary>
        /// And with the scope entered while an answer for the same integrand is already held,
        /// which is the ordering that a cache keyed on anything coarser than the settings
        /// themselves gets wrong.
        /// </summary>
        [Fact]
        public void AnAnswerHeldFromBeforeAScopeIsNotUsedInsideIt()
        {
            var before = "1 / x".ToEntity().Integrate("x").Stringize();
            Assert.DoesNotContain("abs", before);

            using (MathS.Settings.Codomain.Set(Domain.Real))
                Assert.Contains("abs", "1 / x".ToEntity().Integrate("x").Stringize());

            Assert.DoesNotContain("abs", "1 / x".ToEntity().Integrate("x").Stringize());
        }

        /// <summary>
        /// Repeating a question gives the same answer, which is the property a cache is allowed
        /// to have and the one that would break first if a stale entry were served.
        /// </summary>
        [Theory]
        [InlineData("1 / (x ^ 2 + 1)")]
        [InlineData("1 / (x ^ 2 + 1) ^ 2")]
        [InlineData("x * ln(x)")]
        [InlineData("sqrt(tan(x))")]
        [InlineData("sin(x) / (x ^ 2 + 1) ^ 2")]
        public void AskingTwiceAnswersTheSame(string integrand)
        {
            var first = integrand.ToEntity().Integrate("x");
            var second = integrand.ToEntity().Integrate("x");
            Assert.Equal(first, second);
        }

        /// <summary>
        /// An integrand with no elementary antiderivative, which runs every solver to exhaustion
        /// and so pays the full cost of the search. It took <b>115 seconds</b> before the answers
        /// were kept, and about one after.
        /// </summary>
        /// <remarks>
        /// The bound is two orders of magnitude above what it now takes and a quarter of what it
        /// used to, so it is a guard against the regression rather than a measurement of the
        /// machine. A timing assertion tight enough to be a benchmark would only flake.
        /// </remarks>
        [Fact]
        public void AnIntegralWithNoAnswerStillFinishesQuickly()
        {
            var watch = Stopwatch.StartNew();
            var answer = "sin(x) / (x ^ 2 + 1) ^ 2".ToEntity().Integrate("x").Simplify();
            watch.Stop();

            Assert.Contains("integral(", answer.Stringize());
            Assert.True(watch.Elapsed.TotalSeconds < 30,
                $"took {watch.Elapsed.TotalSeconds:F1}s; it was 115s before the answers were kept");
        }
    }
}
