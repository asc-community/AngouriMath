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
    /// A power of the cosine beside a rational function of the secant of the same argument, written
    /// in the cosine alone, where it is one quotient. Rubi's 4.5.4.2.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ACosineBesideASecantBinomialIntegralTest
    {
        [Theory]
        [InlineData("cos(c + d*x)^2*(A + K*sec(c + d*x)^2)/(a + b*sec(c + d*x))", 6000)]
        [InlineData("cos(c + d*x)^3*(B*sec(c + d*x) + K*sec(c + d*x)^2)/(a + b*sec(c + d*x))", 6000)]
        [InlineData("cos(c + d*x)^2*(A + B*sec(c + d*x) + K*sec(c + d*x)^2)/(a + b*sec(c + d*x))", 15000)]
        [InlineData("cos(x)^2/(a + b*sec(x))", 1500)]
        [InlineData("sin(x)^2/(a + b*csc(x))", 1500)]
        public void DifferentiatesBack(string integrand, int atMostCharacters)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.True(integral.Stringize().Length <= atMostCharacters, $"{integral.Stringize().Length} characters");
            Entity Pinned(Entity e) => e.Substitute("a", 2).Substitute("b", 3).Substitute("c", "1/4".ToEntity()).Substitute("d", "3/2".ToEntity())
                .Substitute("A", 5).Substitute("B", 7).Substitute("K", 11);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in new[] { "0.35", "0.6", "1.1", "1.4" })
            {
                var want = original.Substitute("x", at.ToEntity()).EvalNumerical();
                var got = derivative.Substitute("x", at.ToEntity()).EvalNumerical();
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }
    }
}
