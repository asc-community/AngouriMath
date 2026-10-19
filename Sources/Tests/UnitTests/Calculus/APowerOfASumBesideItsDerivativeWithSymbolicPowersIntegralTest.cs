//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// A power of a sum beside its derivative, where the terms hold a symbolic power of a
    /// logarithm: the derivative of <c>ln(c x^n)^q</c> is written <c>q ln(c x^n)^(q - 1) c n x^(n - 1)/(c x^n)</c>,
    /// which nothing brought down to the <c>n q ln(c x^n)^(q - 1)/x</c> the integrand writes. They were
    /// declined. Rubi's 3.5.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APowerOfASumBesideItsDerivativeWithSymbolicPowersIntegralTest
    {
        [Theory]
        [InlineData("(a*m*x^m + b*n*q*ln(c*x^n)^(q - 1))*(a*x^m + b*ln(c*x^n)^q)/x", "1/2*(a*x^m + b*ln(c*x^n)^q)^2")]
        [InlineData("(a*m*x^m + b*n*q*ln(c*x^n)^(q - 1))/(x*(a*x^m + b*ln(c*x^n)^q))", "ln(a*x^m + b*ln(c*x^n)^q)")]
        [InlineData("(a*(m - 1)*x^(m - 1) + b*n*q*ln(c*x^n)^(q - 1))/(a*x^m + b*x*ln(c*x^n)^q)", "ln(a*x^(m - 1) + b*ln(c*x^n)^q)")]
        public void TheSumToTheNextPower(string integrand, string expected)
            => Assert.Equal(expected.ToEntity().InnerSimplified, integrand.ToEntity().Integrate("x").Substitute("C", 0).InnerSimplified);
    }
}
