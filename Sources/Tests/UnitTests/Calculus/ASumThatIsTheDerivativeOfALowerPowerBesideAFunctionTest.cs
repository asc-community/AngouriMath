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
    /// A sum whose terms are not elementary apart, integrated by parts the other way in a term that is a power of a
    /// linear, <c>L^(-k)</c> with <c>k &gt;= 2</c>, beside a function: the power integrated and the function differentiated,
    /// where what that leaves cancels against the other terms. <c>psi_1(a + b x)/x^2 - b psi_2(a + b x)/x</c> is
    /// <c>(-psi_1(a + b x)/x)'</c>. Rubi's 8.6.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/2075">#2075</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ASumThatIsTheDerivativeOfALowerPowerBesideAFunctionTest
    {
        [Theory]
        [InlineData("polygamma(1, a + b*x)/x^2 - b*polygamma(2, a + b*x)/x", "-polygamma(1, a + b*x)/x")]
        [InlineData("digamma(a + b*x)/x^2 - b*polygamma(1, a + b*x)/x", "-digamma(a + b*x)/x")]
        // The second term has a power of -2 as well, and is what the first leaves.
        [InlineData("polygamma(1, x)/(a + b*x)^3 - polygamma(2, x)/(2*b*(a + b*x)^2)", "-polygamma(1, x)/(2*b*(a + b*x)^2)")]
        // Two such derivatives at once.
        [InlineData("polygamma(1, x)/x^2 - polygamma(2, x)/x + sin(x)/x^3 - cos(x)/(2*x^2)", "-polygamma(1, x)/x - sin(x)/(2*x^2)")]
        public void IsTheDerivativeOfTheProduct(string integrand, string expected)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            var difference = integral.Substitute("C", 0) - expected.ToEntity();
            var derivative = integral.Substitute("C", 0).Differentiate("x");
            foreach (var (a, b, x) in new[] { (1.3, 0.7, 0.9), (2.1, 1.9, 1.6), (0.6, 2.4, 3.1) })
            {
                Entity At(Entity e) => e.Substitute("a", a).Substitute("b", b).Substitute("x", x);
                var off = At(difference).EvalNumerical();
                Assert.True(Math.Abs((double)off.RealPart) + Math.Abs((double)off.ImaginaryPart) < 1e-12,
                    $"the antiderivative of {integrand} is {integral}, not {expected}, at a = {a}, b = {b}, x = {x}");
                var want = At(integrand.ToEntity()).EvalNumerical();
                var got = At(derivative).EvalNumerical();
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {x}, where the integrand is {want}");
            }
        }

        /// <summary>
        /// A symbolic order, which no point evaluates: the product is read the same way, and its derivative is the
        /// integrand as written.
        /// </summary>
        [Fact]
        public void ASymbolicOrder()
        {
            var integral = "polygamma(n, a + b*x)/x^2 - b*polygamma(1 + n, a + b*x)/x".ToEntity().Integrate("x");
            Assert.Equal("-polygamma(n, a + b * x) / x + C", integral.Stringize());
        }

        /// <summary>
        /// <c>psi_1(x)/x^2 + psi_2(x)/x</c> is not such a derivative: what parts leaves is <c>2 psi_2(x)/x</c>, and that is not
        /// elementary. Declined, not answered.
        /// </summary>
        [Fact]
        public void ARemainderThatDoesNotCancelIsDeclined()
            => Assert.Contains("integral(", "polygamma(1, x)/x^2 + polygamma(2, x)/x".ToEntity().Integrate("x").Stringize());
    }
}
