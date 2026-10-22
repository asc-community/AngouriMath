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
    /// An even rational function over a power of a biquadratic of three terms beside another factor
    /// in x^2, split in <c>w = x^2</c>: <c>1/((x^2 - 1)(c + d x^2 + f x^4)^2)</c> ran past the budget,
    /// the series at the roots of <c>x^2 - 1</c> coming to coefficients of a hundred thousand nodes,
    /// and so did Rubi's 6.1.7 <c>csch(u)/(a - b sinh(u)^4)^2</c>, which is it under <c>w = cosh(u)</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AnEvenQuotientOverAPowerOfABiquadraticIntegralTest
    {
        [Theory]
        [InlineData("1/((x^2 - 1)*(c + d*x^2 + f*x^4)^2)")]
        [InlineData("x^2/((x^2 + 1)*(c + d*x^2 + f*x^4)^2)")]
        // Rubi 6.1.7:285.
        [InlineData("csch(c + d*x)/(a - b*sinh(c + d*x)^4)^2")]
        public void IsAnsweredWithinTheGuard(string integrand)
        {
            Entity? integral = null;
            // A thread of its own, so that the guard is not spent waiting behind other tests' work.
            var task = Task.Factory.StartNew(() => integral = integrand.ToEntity().Integrate("x"),
                CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            Assert.True(task.Wait(LimitTermination.Guard), $"{integrand} did not finish within {LimitTermination.Guard}");
            var text = integral!.Stringize();
            Assert.DoesNotContain("integral(", text);
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4)
                .Substitute("d", 0.9).Substitute("f", 0.8);
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
