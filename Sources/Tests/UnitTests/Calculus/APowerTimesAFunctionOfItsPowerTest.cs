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
    /// <c>x^(k n - 1) g(x^n)</c> with a symbolic <c>n</c> is <c>u^(k - 1) g(u)/n</c> under
    /// <c>u = x^n</c>; Rubi's 6.5.2 and 6.6.2 write the power in front as <c>(e x)^(n - 1)</c>,
    /// which is <c>e^(n - 1) x^(n - 1)</c> for a positive <c>e</c>, and the answer says so.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APowerTimesAFunctionOfItsPowerTest
    {
        private static readonly double[] Points = { 0.3, 0.7, 1.1, 1.9, 2.6 };

        /// <summary>Pinned with <c>n = 5/2</c>, <c>a = 1/3</c>, <c>b = 1/2</c>, <c>c = 3/2</c>, <c>d = 2/3</c>, <c>e = 5/4</c>.</summary>
        private static void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Entity Pinned(Entity e) => e.Substitute("C", 0).Substitute("n", "5/2".ToEntity()).Substitute("a", "1/3".ToEntity()).Substitute("b", "1/2".ToEntity())
                .Substitute("c", "3/2".ToEntity()).Substitute("d", "2/3".ToEntity()).Substitute("pe", "5/4".ToEntity());
            var derivative = Pinned(integral).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in Points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                if (got.IsNaN || want.IsNaN)
                    continue;
                compared++;
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 4, $"only {compared} of {Points.Length} points were comparable for {integrand}");
        }

        [Theory]
        [InlineData("x^(n-1)*e^(x^n)")]
        [InlineData("x^(2*n-1)*e^(x^n)")]
        [InlineData("(pe*x)^(-1+n)*(a+b*csch(c+d*x^n))")]
        [InlineData("(pe*x)^(-1+n)/(a+b*csch(c+d*x^n))")]
        [InlineData("(pe*x)^(-1+n)*sech(c+d*x^n)^2")]
        [InlineData("(pe*x)^(-1+n)/(a+b*sech(c+d*x^n))^2")]
        public void APowerTimesAFunctionOfItsPower(string integrand) => DifferentiatesBack(integrand);

        [Fact]
        public void TheScaleIsSaidToBePositive()
        {
            var integral = "(pe*x)^(-1+n)*e^(x^n)".ToEntity().Integrate("x");
            Assert.Contains(integral.Nodes, node => node is Entity.Providedf(_, var predicate) && predicate == "pe > 0".ToEntity());
            Assert.Equal("e^(x^n)/n".ToEntity().Simplify(), "x^(n-1)*e^(x^n)".ToEntity().Integrate("x").Substitute("C", 0).Simplify());
        }

        /// <summary>A power in front that is not one less than a multiple of the power inside is left alone.</summary>
        [Fact]
        public void APowerThatIsNotTheDerivativeIsLeftAsWritten()
            => Assert.Contains("integral(", "x^n*e^(x^n)".ToEntity().Integrate("x").Stringize());
    }
}
