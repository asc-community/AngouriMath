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
    /// A sine or cosine of a linear over x times a power of a binomial, with the power lowered by
    /// <c>1/(x Q^k) = 1/(a x Q^(k - 1)) - (b/a) x^(n - 1)/Q^k</c> for <c>Q = a + b x^n</c>: Rubi's
    /// 4.1.11 <c>sin(c + d x)/(x (a + b x^3)^3)</c> was declined, the split over x and the cube's three
    /// roots, each to the third power, past the size it hands on, where each of the two is answered.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1501">#1501</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APowerOfABinomialBesideXBelowASineIntegralTest
    {
        [Theory]
        // Rubi 4.1.11:136.
        [InlineData("sin(c + d*x)/(x*(a + b*x^3)^3)")]
        [InlineData("cos(c + d*x)/(x*(a + b*x^3)^3)")]
        public void IsAnsweredWithinTheGuard(string integrand)
        {
            Entity? integral = null;
            // A thread of its own, so that the guard is not spent waiting behind other tests' work.
            var task = Task.Factory.StartNew(() => integral = integrand.ToEntity().Integrate("x"),
                CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            Assert.True(task.Wait(LimitTermination.Guard), $"{integrand} did not finish within {LimitTermination.Guard}");
            var text = integral!.Stringize();
            Assert.DoesNotContain("integral(", text);
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4).Substitute("d", 0.9);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in new[] { -2.2, -0.9, 0.3, 1.4, 2.1 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                Assert.False(got.IsNaN, $"d/dx of the antiderivative of {integrand} has no value at x = {at}, where the integrand is {want}");
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }
    }
}
