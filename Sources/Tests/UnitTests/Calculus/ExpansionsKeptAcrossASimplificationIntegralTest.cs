//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// The rational functions a substitution leaves are simplified, and a simplification's
    /// nested runs expanded, factored and simplified the same trees over and over: the tangent's
    /// integrand for Rubi's 4.1.7 <c>sin(c + d x)^4/(a + b sin(c + d x)^2)^2</c> cost 12 GB, 11 GB
    /// of it one product of 342 terms expanded 72 times. Each is now done once a simplification.
    /// The answers are the ones made afresh gave; the symbols are this test's own, so that no
    /// answer the integrator remembers from another test is what is measured.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ExpansionsKeptAcrossASimplificationIntegralTest
    {
        /// <summary>
        /// Made afresh, the four cost 12.1, 8.8, 7.8 and 21.6 GB; kept, 0.4, 0.5, 0.8 and 2.6. The
        /// last is mostly polynomials in <c>1 - u^2</c> factored past the degree the factoring
        /// takes, and the simplifications of the sums they came from.
        /// </summary>
        [Theory]
        [InlineData("sin(kc + kd*x)^4/(ka + kb*sin(kc + kd*x)^2)^2", 3_000_000_000L, -1.2, -0.7, 0.3, 0.8, 1.3, 2.1)]
        [InlineData("csc(kc + kd*x)^4/(ka + kb*sin(kc + kd*x)^2)^2", 3_000_000_000L, -1.2, -0.7, 0.3, 0.8, 1.3, 2.1)]
        [InlineData("e^acoth(ka*x)/(kc - kc/(ka*x))^4", 3_000_000_000L, 0.9, 1.3, 1.7, 2.2, 2.9, 3.6)]
        [InlineData("e^acoth(ka*x)*(kc - kc/(ka^2*x^2))^4", 8_000_000_000L, 0.9, 1.3, 1.7, 2.2, 2.9, 3.6)]
        public void AnsweredAsBeforeForAFractionOfTheAllocation(string integrand, long bytes, double p1, double p2, double p3, double p4, double p5, double p6)
        {
            var before = GC.GetAllocatedBytesForCurrentThread();
            var integral = integrand.ToEntity().Integrate("x");
            var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(allocated < bytes, $"{allocated / 1e9:F2} GB to integrate {integrand}, over the {bytes / 1e9:F0} GB bound");
            Entity Pinned(Entity e) => e.Substitute("ka", 1.3).Substitute("kb", 0.7).Substitute("kc", 0.4).Substitute("kd", 0.3);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { p1, p2, p3, p4, p5, p6 })
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
        /// The simplification itself, of that integrand as the substitution writes it: the form it
        /// always had, for a thirtieth of the 12 GB it cost.
        /// </summary>
        [Fact]
        public void TheTangentsIntegrandSimplifiesAsBefore()
        {
            var written = "(wt*1)^4/((wb*(wt*1)^2 + wa*1^2 + wa*(wt*1)^2)^2*(1 + wt^2))".ToEntity();
            var before = GC.GetAllocatedBytesForCurrentThread();
            var simplified = written.Simplify();
            var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.Equal("wt ^ 4 / ((wa * (1 + wt ^ 2) + wb * wt ^ 2) ^ 2 * (wt ^ 2 + 1))", simplified.Stringize());
            Assert.True(allocated < 3_000_000_000L, $"{allocated / 1e9:F2} GB to simplify {written}");
        }
    }
}
