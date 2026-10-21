//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Threading;
using System.Threading.Tasks;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// Integrands that were answered, but only after a rule ahead of the one that answers had
    /// spent twenty to thirty seconds declining: a sum split into elliptic terms before the pair
    /// rule was asked, the radical of an inverse hyperbolic function combined into a spelling
    /// nothing reads, a numerator of thousands of terms expanded by three readers of quadratics
    /// that wanted a power below the bar it did not have, and substitution candidates simplified
    /// over their derivatives where two points already showed that no function of them was the
    /// integrand. Rubi's 6.1.1, 6.6.1, 7.2.4, 7.6.2, 4.7.7 and 3.2.1, which the corpus counted
    /// as solved only because its worker let a case run past its budget, and which timed out under
    /// load. What is asserted is that each comes back, answered, with a wall far above the run.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class SlowDeclinesAheadOfTheAnsweringRuleIntegralTest
    {
        [Theory]
        [InlineData("x/csch(x)^(3/2) + 1/3*x*sqrt(csch(x))", 0.3)]
        [InlineData("x/csch(x)^(5/2) + 3/5*x/sqrt(csch(x))", 0.3)]
        [InlineData("x/csch(x)^(7/2) - 5/21*x*sqrt(csch(x))", 0.3)]
        [InlineData("x^3*acsch(sqrt(x))", 0.3)]
        [InlineData("x^4*(d + k*x^2)^3*(a + b*acosh(c*x))", 0.8)]
        [InlineData("sin(x)*tan(6*x)", 0.05)]
        [InlineData("1/((a*g + b*g*x)^2*(A + B*ln(k*(c + d*x)^2/(a + b*x)^2))^2)", 0.3)]
        // Polynomials of high degree, whose quotient over the derivative of `x^24` or `x^36` takes
        // values of two thousand digits at the two points the repeat test compares: Rubi's
        // 1.1.4.2, under a second each, which with that test widened did not return in minutes.
        [InlineData("x^24*(a*x + b*x^38)^12", 0.3)]
        [InlineData("x^36*(a + b*x^37)^12", 0.3)]
        public void IsAnswered(string integrand, double from)
        {
            var integral = Run(() => integrand.ToEntity().Integrate("x"));
            Assert.DoesNotContain("integral(", integral.Stringize());
            Entity Pinned(Entity e) => e
                .Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 1.4).Substitute("d", 0.6)
                .Substitute("g", 0.9).Substitute("k", 1.7).Substitute("A", 0.5).Substitute("B", 1.1);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var step in new[] { 0.0, 0.11, 0.23, 0.37, 0.52, 0.71, 0.93 })
            {
                var at = from + step;
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                if (want.IsNaN)
                    continue;
                compared++;
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-7 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand}");
        }

        /// <summary>
        /// On a thread of its own, so that a test host whose pool is busy cannot hold it back, and
        /// with a wall far above the run, so that the bound is reached only by a computation that
        /// does not finish.
        /// </summary>
        private static Entity Run(Func<Entity> work)
        {
            var task = Task.Factory.StartNew(work, CancellationToken.None,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);
            Assert.True(task.Wait(TimeSpan.FromMinutes(3)), "did not return in three minutes");
            return task.Result;
        }
    }
}
