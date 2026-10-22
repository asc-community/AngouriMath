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
    /// A logarithm of nested powers of one linear, <c>ln(c (d (e + f x)^p)^q)</c>, named as
    /// <c>p q ln(e + f x) + K</c>, K constant where it is continuous: Rubi's 3.3
    /// <c>(g + h x)^2 (a + b ln(c (d (e + f x)^p)^q))^3</c> was declined after eleven seconds and twelve
    /// gigabytes, where in the linear's logarithm it is a polynomial times a power of it.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ALogarithmOfNestedPowersOfALinearIntegralTest
    {
        [Theory]
        // Rubi 3.3:620, with k for Rubi's e.
        [InlineData("(g + h*x)^2*(a + b*ln(c*(d*(k + f*x)^p)^q))^3")]
        [InlineData("(g + h*x)*(a + b*ln(c*(d*(k + f*x)^p)^q))^3")]
        [InlineData("(g + h*x)^2*(a + b*ln(c*(d*(k + f*x)^p)^q))^2")]
        public void IsAnsweredWithTheLogarithmAsWritten(string integrand)
        {
            Entity? integral = null;
            // A thread of its own, so that the guard is not spent waiting behind other tests' work.
            var task = Task.Factory.StartNew(() => integral = integrand.ToEntity().Integrate("x"),
                CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            Assert.True(task.Wait(LimitTermination.Guard), $"{integrand} did not finish within {LimitTermination.Guard}");
            var text = integral!.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.Contains("ln(c * (d * (k + f * x) ^ p) ^ q)", text);
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4).Substitute("d", 0.9)
                .Substitute("f", 0.8).Substitute("g", 1.7).Substitute("h", 0.6).Substitute("k", 0.2).Substitute("p", 1.5).Substitute("q", 0.75);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in new[] { 0.3, 0.9, 1.4, 2.1, 3.3 })
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
