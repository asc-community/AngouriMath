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
    /// A half-odd power of x below two blocks in <c>x^2</c>, one of them to a power, as Rubi's 1.1.2.4
    /// writes it. Under <c>t = sqrt(x)</c> it is a power of t beside two blocks in <c>t^4</c>, which the
    /// split beside a power of x answers where its pieces over the power are read, at the question
    /// asked or one below it. It was answered there, after a second pass through the same split a
    /// level further down, where those pieces are not read and each went round the whole search to be
    /// declined: seven seconds of <c>1/(x^(3/2) (a + b x^2) (c + d x^2)^2)</c> were six of that. With
    /// both blocks to the first power the deeper split is read and still taken.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APowerOfXBesideBlocksIsSplitWhereThePiecesAreReadIntegralTest
    {
        [Theory]
        [InlineData("1/(x^(3/2)*(a + b*x^2)*(c + d*x^2)^2)")]
        [InlineData("1/(x^(5/2)*(a + b*x^2)*(c + d*x^2)^2)")]
        [InlineData("1/(x^(7/2)*(a + b*x^2)*(c + d*x^2)^2)")]
        [InlineData("1/(x^(3/2)*(a + b*x^2)*(c + d*x^2)^3)")]
        [InlineData("1/(x^(5/2)*(a + b*x^2)*(c + d*x^2)^3)")]
        [InlineData("1/(x^(7/2)*(a + b*x^2)*(c + d*x^2)^3)")]
        [InlineData("1/(x^(3/2)*(a + b*x^2)^2*(c + d*x^2)^2)")]
        [InlineData("1/(x^(5/2)*(a + b*x^2)*(c + d*x^2))")]
        public void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4).Substitute("d", 0.3);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { 0.3, 0.6, 0.9, 1.3, 1.8, 2.4 })
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
    }
}
