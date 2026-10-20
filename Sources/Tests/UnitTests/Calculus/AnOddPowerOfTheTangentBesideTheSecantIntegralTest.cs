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
    /// An odd power of the tangent beside a function of the secant, in <c>u = sec(y)</c>, and the
    /// cotangent beside one of the cosecant. Rubi's 4.5.1.4.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AnOddPowerOfTheTangentBesideTheSecantIntegralTest
    {
        [Theory]
        [InlineData("cot(g + f*x)/(a + b*sec(g + f*x))^(3/2)")]
        [InlineData("cot(x)^3/(a + b*sec(x))^(3/2)")]
        [InlineData("tan(x)^3/(a + b*csc(x))")]
        public void InTheSecant(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 40000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.4).Substitute("g", 0.2).Substitute("f", 0.7);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -1.2, -0.7, 0.3, 0.8, 1.3, 2.9 })
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
        /// A symbolic power of <c>b sec(y)</c>, which is <c>(b u)^m</c> in <c>u</c>: Rubi's 4.3.0,
        /// declined only because <c>m</c> was not a number. Checked for a negative <c>b</c> as well,
        /// where the integrand is complex.
        /// </summary>
        [Theory]
        [InlineData("(b*sec(g + f*x))^m*tan(g + f*x)^3", 0.4)]
        [InlineData("(b*sec(g + f*x))^m*tan(g + f*x)^5", 0.4)]
        [InlineData("cot(g + f*x)^3*(b*csc(g + f*x))^m", 0.4)]
        [InlineData("cot(g + f*x)^5*(b*csc(g + f*x))^m", 0.4)]
        [InlineData("sec(x)^m*tan(x)^3", 0.4)]
        [InlineData("(b*sec(g + f*x))^m*tan(g + f*x)^3", -0.4)]
        [InlineData("cot(g + f*x)^5*(b*csc(g + f*x))^m", -0.4)]
        public void ASymbolicPowerOfTheSecant(string integrand, double b)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 2000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("b", b).Substitute("m", 0.6).Substitute("g", 0.2).Substitute("f", 0.7);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -1.2, -0.7, 0.3, 0.8, 1.3, 2.9 })
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
        /// A symbolic power of a sum in the secant leaves <c>(a + b u)^m/u</c>, which has no
        /// elementary antiderivative, and so does a negative power of the tangent beside
        /// <c>(b sec(y))^m</c>: declined.
        /// </summary>
        [Theory]
        [InlineData("(a + b*sec(x))^m*tan(x)^3")]
        [InlineData("(b*sec(x))^m*cot(x)")]
        public void ASymbolicPowerThatIsNotElementary(string integrand)
            => Assert.Contains("integral(", integrand.ToEntity().Integrate("x").Stringize());
    }
}
