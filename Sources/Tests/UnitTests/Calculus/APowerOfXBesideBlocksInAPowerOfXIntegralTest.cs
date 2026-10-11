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
    /// A power of x beside two blocks in one power of x is split over the blocks in
    /// <c>u = x^n</c> with the power of x kept outside: <c>1/(x^7 (a + b x^3)(c + d x^3))</c> is a
    /// power of x over one block twice. Rubi's 1.1.3.4.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APowerOfXBesideBlocksInAPowerOfXIntegralTest
    {
        [Theory]
        [InlineData("1/(x*(a + b*x^3)*(c + d*x^3))")]
        [InlineData("1/(x^4*(a + b*x^3)*(c + d*x^3))")]
        [InlineData("1/(x^7*(a + b*x^3)*(c + d*x^3))")]
        [InlineData("1/(x^6*(a + b*x^4)*(c + d*x^4))")]
        public void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Entity Pinned(Entity e) => e.Substitute("a", 2).Substitute("b", 3).Substitute("c", 5).Substitute("d", 7);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in new[] { 0.35, 0.6, 0.85, 1.4 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }

        /// <summary>
        /// Split at the power of x first, the rest over both blocks had coefficients pages long,
        /// and the answer was 265,499 characters; over the blocks first it is 351.
        /// </summary>
        [Fact]
        public void TheAnswerIsTheSizeOfItsPieces()
            => Assert.True("1/(x^7*(a + b*x^3)*(c + d*x^3))".ToEntity().Integrate("x").Stringize().Length < 2000);
    }
}
