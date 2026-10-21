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
    /// A rational function of a hyperbolic sine or cosine over a symbolic quadratic in it, asked under
    /// Osborn's rule as one of a sine over a quadratic before the tangent of the half argument: under that
    /// tangent Rubi's 6.7.1 <c>sinh(x)^2/(a + b sinh(x) + c sinh(x)^2)</c> is a quartic with symbols in it,
    /// and its partial fractions ran past the budget.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AHyperbolicQuotientOverAQuadraticInItAsTheSinesIntegralTest
    {
        [Theory]
        [InlineData("sinh(x)^2/(a + b*sinh(x) + c*sinh(x)^2)")]
        [InlineData("sinh(x)^3/(a + b*sinh(x) + c*sinh(x)^2)")]
        [InlineData("cosh(x)^2/(a + b*cosh(x) + c*cosh(x)^2)")]
        public void IsAnswered(string integrand)
        {
            Entity? answer = null;
            // A thread of its own, so that the guard is not spent waiting behind other tests' work.
            var task = Task.Factory.StartNew(() => answer = integrand.ToEntity().Integrate("x"),
                CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            Assert.True(task.Wait(LimitTermination.Guard), $"{integrand} did not finish within {LimitTermination.Guard}");
            var integral = answer!;
            Assert.DoesNotContain("integral(", integral.Stringize());
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in new[] { -1.7, -0.6, 0.4, 1.3, 2.2 })
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
