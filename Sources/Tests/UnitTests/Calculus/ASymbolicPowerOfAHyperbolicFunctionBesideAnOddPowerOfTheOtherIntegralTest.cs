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
    /// A symbolic power of <c>sinh(y)</c> beside an odd power of <c>cosh(y)</c>, under
    /// <c>u = sinh(y)</c>, and the other way about under <c>u = cosh(y)</c>. Rubi's 6.7.1.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    /// <remarks>Compared as complex numbers: a symbolic power of a negative sine is complex.</remarks>
    [Trait("Area", "Calculus")]
    public sealed class ASymbolicPowerOfAHyperbolicFunctionBesideAnOddPowerOfTheOtherIntegralTest
    {
        [Theory]
        [InlineData("cosh(a + b*x)^3*sinh(a + b*x)^n")]
        [InlineData("cosh(a + b*x)^5*sinh(a + b*x)^n")]
        [InlineData("cosh(a + b*x)^m*sinh(a + b*x)^3")]
        [InlineData("cosh(a + b*x)^m*sinh(a + b*x)^5")]
        [InlineData("2*cosh(x)^3/sinh(x)^n")]
        public void IsAPolynomialTimesAPowerInTheOther(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Entity Pinned(Entity e) => e.Substitute("a", 0.4).Substitute("b", 1.3).Substitute("n", 0.37).Substitute("m", -1.6);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in new[] { -1.9, -1.1, -0.7, 0.4, 1.1, 1.9 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }

        /// <summary>
        /// The reciprocals to the symbolic power and the tangent and cotangent beside them, read as
        /// powers of the two: <c>sech(y)^n tanh(y)^3</c> under <c>u = sech(y)</c>, and
        /// <c>coth(y)^3 csch(y)^n</c> under <c>u = csch(y)</c>, whose power is complex where the sine
        /// is negative and not that of <c>sinh(y)^(-n)</c>. Rubi's 6.7.1. Both signs of the
        /// argument at each slope.
        /// </summary>
        [Theory]
        [InlineData("sech(a + b*x)^n*tanh(a + b*x)^3", 0.4, 1.3)]
        [InlineData("coth(a + b*x)^3*csch(a + b*x)^n", 0.4, 1.3)]
        [InlineData("coth(a + b*x)^3*csch(a + b*x)^n", 0.9, -0.8)]
        [InlineData("sech(a + b*x)^m*tanh(a + b*x)^5", -0.4, 1.3)]
        [InlineData("coth(a + b*x)^5*csch(a + b*x)^m", 0.9, -0.8)]
        [InlineData("cosh(a + b*x)^n*tanh(a + b*x)^3", 0.4, 1.3)]
        [InlineData("sinh(a + b*x)^m*coth(a + b*x)^3", -0.4, 1.3)]
        [InlineData("csch(a + b*x)^n*cosh(a + b*x)^3", 0.4, 1.3)]
        [InlineData("2*sech(x)^n*sinh(x)^3", 0.0, 1.0)]
        public void ReadsTheReciprocalsAndTheQuotients(string integrand, double a, double b)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Entity Pinned(Entity e) => e.Substitute("a", a).Substitute("b", b).Substitute("n", 0.37).Substitute("m", -1.6);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in new[] { -1.9, -1.1, -0.7, 0.4, 1.1, 1.9 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }
    }
}
