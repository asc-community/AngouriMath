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
    /// Rubi's 6.1.7 <c>csch(c + d x)^m (a + b sinh(c + d x)^3)^p</c>. Under <c>u = e^(c + d x)</c> it is a
    /// quotient with a power of a symbolic polynomial of the sixth degree in it, which is simplified
    /// without the expanded and factored forms the simplifier offers after its search; and the split
    /// of the expanded integrand into terms, which declines, stops at the first term declined. Each
    /// row cost 5.2 to 10.5 GB, and the answers are the ones they were.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    /// <remarks>
    /// Each row has symbols of its own, so that no answer the thread remembers from another test
    /// makes its bound hold without the work it measures. The bounds fail with either change
    /// undone: with the quotient simplified in full every row, at 4.9 to 8.8 GB, and with every
    /// term of the split asked the second and the fourth, at 5.2 and 2.8 GB.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class AHyperbolicCosecantBesideAPowerOfABinomialInTheCubedSineIntegralTest
    {
        [Theory]
        [InlineData("csch(v + w*x)^5*(p + q*sinh(v + w*x)^3)^2", "v,w,p,q", 2_000_000_000L)]
        [InlineData("csch(r + s*x)^6*(k + m*sinh(r + s*x)^3)^3", "r,s,k,m", 3_000_000_000L)]
        [InlineData("csch(g + h*x)*(n + t*sinh(g + h*x)^3)^3", "g,h,n,t", 2_000_000_000L)]
        [InlineData("csch(y + z*x)^7*(f + u*sinh(y + z*x)^3)^3", "y,z,f,u", 2_000_000_000L)]
        public void AnsweredWithinItsAllocation(string integrand, string symbols, long mostBytes)
        {
            var before = GC.GetAllocatedBytesForCurrentThread();
            var integral = integrand.ToEntity().Integrate("x");
            var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.True(allocated < mostBytes, $"{integrand} allocated {allocated / 1_000_000} MB, past {mostBytes / 1_000_000} MB");
            DifferentiatesBack(integrand, integral, symbols);
        }

        /// <summary>
        /// Where the powers of <c>u</c> above the bar and below it are of one degree, seven here, the
        /// factored form the simplifier offers after its search wins and is kept: from the search's
        /// form alone the antiderivative was 1,938 characters where it is 571.
        /// </summary>
        [Fact]
        public void WherePowersOfTheExponentialCancelTheFactoredFormIsKept()
        {
            const string integrand = "csch(c + d*x)^7*(a + b*sinh(c + d*x)^3)^2";
            var integral = integrand.ToEntity().Integrate("x");
            Assert.True(integral.ToString().Length < 1000, $"the antiderivative of {integrand} is {integral.ToString().Length} characters");
            DifferentiatesBack(integrand, integral, "c,d,a,b");
        }

        /// <summary>
        /// And where the sum to a power has no constant term in <c>u</c>, so that <c>u</c> divides it:
        /// under <c>u = e^x</c> Rubi's 6.7.1 <c>(A + B cosh(x) + C sinh(x))/(b cosh(x) + c sinh(x))^2</c>
        /// is a quotient with such a sum squared, the factored form wins there, and from the search's
        /// form alone the antiderivative was 178,682 characters where it is 3,485.
        /// </summary>
        [Fact]
        public void WhereTheSumHasNoConstantTermTheFactoredFormIsKept()
        {
            const string integrand = "(A + B*cosh(x) + C*sinh(x))/(b*cosh(x) + c*sinh(x))^2";
            var integral = integrand.ToEntity().Integrate("x");
            Assert.True(integral.ToString().Length < 10000, $"the antiderivative of {integrand} is {integral.ToString().Length} characters");
            DifferentiatesBack(integrand, integral, "A,B,C,b,c");
        }

        private static void DifferentiatesBack(string integrand, Entity integral, string symbols)
        {
            Assert.DoesNotContain("integral(", integral.Stringize());
            var names = symbols.Split(',');
            var values = new[] { 0.3, 1.1, 1.3, 0.7, 0.4 };
            Entity Pinned(Entity e)
            {
                for (var k = 0; k < names.Length; k++)
                    e = e.Substitute(names[k], values[k]);
                return e;
            }
            // A constant of integration differentiates away; C is a coefficient of one integrand here.
            var derivative = Pinned(integral).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { 0.2, 0.45, 0.7, 0.95, 1.2, 1.5 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                if (want.IsNaN)
                    continue;
                compared++;
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-8 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand}");
        }
    }
}
