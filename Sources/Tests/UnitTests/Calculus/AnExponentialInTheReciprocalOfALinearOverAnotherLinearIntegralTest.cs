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
    /// An exponential of a constant plus a multiple of <c>1/L</c>, <c>L = c + d x</c>, over a whole
    /// power of another linear <c>M = g + h x</c>, and of a power of <c>1/L</c> over a power of
    /// <c>L</c> where that is elementary. Under <c>u = 1/L</c> the first is an exponential of a
    /// linear over a power of a linear in <c>u</c>, the exponential integral of a shifted argument,
    /// and the second <c>u^(n - 2) F^(a + b u^k)</c> with <c>n - 1</c> a multiple of <c>k</c>. The
    /// substitution was reached only after every other rule had declined:
    /// <c>F^(e + f (a + b x)/(c + d x))/(g + h x)^n</c> took 4 to 12 GB for <c>n = 2</c> to 4, and
    /// <c>F^(a + b/(c + d x)^3)/(c + d x)^13</c> ran past the budget. Rubi's 2.3.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AnExponentialInTheReciprocalOfALinearOverAnotherLinearIntegralTest
    {
        [Theory]
        [InlineData("F^(m + f*(a + b*x)/(c + d*x))/(g + h*x)", 1.1)]
        [InlineData("F^(m + f*(a + b*x)/(c + d*x))/(g + h*x)^2", 1.1)]
        [InlineData("F^(m + f*(a + b*x)/(c + d*x))/(g + h*x)^3", 1.1)]
        [InlineData("F^(m + f*(a + b*x)/(c + d*x))/(g + h*x)^4", 1.1)]
        [InlineData("F^(m + f*(a + b*x)/(c + d*x))/(g + h*x)^3", -0.9)]
        [InlineData("F^(a + b/(c + d*x))/(k + f*x)^3", 1.1)]
        [InlineData("e^(k/(c + d*x))/(a + b*x)^3", -0.9)]
        [InlineData("f^(c/(a + b*x))/x^3", 1.1)]
        [InlineData("F^(a + b/(c + d*x))*(p + q*x)/(g + h*x)^2", 1.1)]
        [InlineData("F^(a + b/(c + d*x))/((g + h*x)*(p + q*x))", 1.1)]
        [InlineData("e^(1/(1 + x))/(2 + x)^2", 1.1)]
        [InlineData("F^(a + b/(c + d*x)^3)/(c + d*x)^13", 1.1)]
        [InlineData("F^(a + b/(c + d*x)^3)/(c + d*x)^4", 1.1)]
        [InlineData("F^(a + b/(c + d*x)^2)/(c + d*x)^7", 1.1)]
        [InlineData("F^(a + b/(c + d*x)^2)/(c + d*x)^3", 1.1)]
        [InlineData("e^(k/(1 + 2*x)^2)/(1 + 2*x)^5", -0.9)]
        public void DifferentiatesBack(string integrand, double k)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 5000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4).Substitute("d", 0.3)
                .Substitute("f", 2.3).Substitute("g", 0.6).Substitute("h", 1.9).Substitute("m", 0.5).Substitute("F", 1.7)
                .Substitute("p", 0.9).Substitute("q", 0.45).Substitute("k", k);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -0.9, -0.4, 0.3, 0.8, 1.3, 2.9 })
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
        /// Answered by the substitution before the rules that decline it: in its old place the
        /// declines cost 3.9, 7.9 and 11.7 GB for the second, third and fourth power, and 6.2 and
        /// 5.6 GB for the moments. Spelled in
        /// symbols no other test here uses, since an integral already answered on the thread is
        /// read from the cache and allocates nothing.
        /// </summary>
        [Theory]
        [InlineData("G^(n + s*(r + t*x)/(u + v*x))/(w + y*x)^2")]
        [InlineData("G^(n + s*(r + t*x)/(u + v*x))/(w + y*x)^3")]
        [InlineData("G^(n + s*(r + t*x)/(u + v*x))/(w + y*x)^4")]
        [InlineData("G^(n + s/(u + v*x)^2)/(u + v*x)^13")]
        [InlineData("G^(n + s/(u + v*x)^3)/(u + v*x)^10")]
        public void IsAnsweredBeforeTheSearch(string integrand)
        {
            var expression = integrand.ToEntity();
            var before = GC.GetAllocatedBytesForCurrentThread();
            var text = expression.Integrate("x").Stringize();
            var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.DoesNotContain("integral(", text);
            Assert.True(allocated < 1_000_000_000L, $"integrating {integrand} allocated {allocated} bytes");
        }
    }
}
