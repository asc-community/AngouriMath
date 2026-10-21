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
    /// Rubi's rows that ran past the corpus's budget beneath the polynomial term, each of them in
    /// one simplification of a rational function with symbols in it: a substitution candidate's
    /// quotient that still varied with x, or a scaled integrand that was not homogeneous. Neither
    /// can lose its x by a rewriting that keeps the value, and the search now reads that off two
    /// points instead. <c>tan(c + d x)^(11/2)/(a + b tan(c + d x))^3</c> took thirty-five seconds
    /// and the other two did not return in two minutes.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ASubstitutionThatKeepsTheVariableIsNotSimplifiedIntegralTest
    {
        private static Entity IntegrateWithinTheGuard(string integrand)
        {
            Entity? answer = null;
            // A thread of its own, so that the guard is not spent waiting behind other tests' work,
            // and a wall clock that only a search which does not end can reach.
            var task = Task.Factory.StartNew(() => answer = integrand.ToEntity().Integrate("x"),
                CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            Assert.True(task.Wait(LimitTermination.Guard), $"{integrand} did not finish within {LimitTermination.Guard}");
            return answer!;
        }

        [Theory]
        // Rubi 4.3.2.1:690, under the tangent and then the root of it.
        [InlineData("tan(c + d*x)^(11/2)/(a + b*tan(c + d*x))^3", new[] { 0.1, 0.3, 0.5, 0.7, 0.9, 1.1 })]
        // Rubi 4.3.2.1:349, in the imaginary sum, which the scaling read twice before declining.
        [InlineData("tan(c + d*x)^4/(a + i*a*tan(c + d*x))^(4/3)", new[] { -0.9, -0.4, 0.2, 0.6, 1.1, 1.7 })]
        // Rubi 4.1.2.3:77, under the half angle, where the sine is positive.
        [InlineData("1/((c + d*sin(k + f*x))*sqrt(g*sin(k + f*x))*sqrt(a + a*sin(k + f*x)))", new[] { 0.1, 0.3, 0.5, 0.7, 0.9, 1.1 })]
        // Rubi 1.2.3.2:854, in the shifted linear and then its square: what is left there is the
        // square written with the shift in it, which is the candidate once its terms are collected.
        [InlineData("1/((d + g*x)*(a + b*(d + g*x)^2 + c*(d + g*x)^4)^3)", new[] { 0.1, 0.3, 0.5, 0.7, 0.9, 1.1 })]
        // Rubi 1.1.3.2:3483, in the shifted linear and then its cube: the shift's `d^0` is
        // `1 provided not d = 0` inner-simplified, and the condition comes off what is kept.
        [InlineData("1/((c + d*x)*(a + b*(c + d*x)^3))", new[] { 0.1, 0.3, 0.5, 0.7, 0.9, 1.1 })]
        // Rubi 2.3:805, under `u = x^x`, whose kept quotient holds an `x/x` the inner simplification
        // leaves: what is left is a power of the candidate, and so it is simplified after all.
        [InlineData("e^(x^x)*x^(2*x)*(1 + ln(x))", new[] { 0.1, 0.3, 0.5, 0.7, 0.9, 1.1 })]
        public void IsAnswered(string integrand, double[] points)
        {
            var integral = IntegrateWithinTheGuard(integrand);
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4)
                .Substitute("d", 0.9).Substitute("f", 0.8).Substitute("g", 1.7).Substitute("k", 0.2);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in points)
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                if (want.IsNaN || got.IsNaN)
                    continue;
                compared++;
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand}");
        }

        /// <summary>
        /// Rubi 2.3's <c>F^(k sqrt(1 - a x)/sqrt(1 + a x))/(1 - a^2 x^2)</c>, under the exponent: what
        /// it leaves in x is a power of the candidate, read off the simplified quotient, and so that
        /// one is still simplified. The answer, <c>-Ei(ln(F) u)/a</c>, has no value at <c>k = 0</c>,
        /// where the candidate is constant and the integrand is <c>1/(1 - a^2 x^2)</c>, and it says so.
        /// </summary>
        [Fact]
        public void KeepsTheConditionWhereTheCandidateVaries()
        {
            const string integrand = "F^(k*sqrt(1 - a*x)/sqrt(1 + a*x))/(1 - a^2*x^2)";
            var integral = IntegrateWithinTheGuard(integrand);
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.Contains("provided not k = 0", text);
            Entity Pinned(Entity e) => e.Substitute("F", 1.7).Substitute("k", 0.6).Substitute("a", 1.3);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -0.6, -0.4, -0.2, 0.1, 0.3, 0.5 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                if (want.IsNaN || got.IsNaN)
                    continue;
                compared++;
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand}");
        }
    }
}
