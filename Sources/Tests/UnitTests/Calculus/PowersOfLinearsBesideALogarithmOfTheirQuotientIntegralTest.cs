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
    /// Powers of linears beside a power of <c>A + B ln(k ((a + b x)/(c + d x))^n)</c>, Rubi's 3.2.1,
    /// on the three readings that sent them past the corpus's budget: the reciprocal of a product
    /// handed on as its power -1, which written apart hid its linear under a power of a power; the
    /// logarithm of a power of the quotient, whose derivative was taken whole and brought every
    /// remainder over a cubic; and a remainder over a power of a third linear, whose zero term put
    /// a condition on the constants at the top of the question in that linear. Answered and
    /// differentiated back.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class PowersOfLinearsBesideALogarithmOfTheirQuotientIntegralTest
    {
        [Theory]
        [InlineData("1/((a*g + b*g*x)^3*(A + B*ln(k*(a + b*x)/(c + d*x))))")]
        [InlineData("1/((a*g + b*g*x)^3*(A + B*ln(k*((a + b*x)/(c + d*x))^n))^2)")]
        [InlineData("1/((c*g + d*g*x)^2*(A + B*ln(k*((a + b*x)/(c + d*x))^n))^2)")]
        [InlineData("1/((a*g + b*g*x)^2*(A + B*ln(k*(c + d*x)/(a + b*x)))^2)")]
        [InlineData("1/((a*g + b*g*x)^3*(A + B*ln(k*(c + d*x)^2/(a + b*x)^2))^2)")]
        public void TheReciprocalReadAsTheQuotient(string integrand) => AnsweredAndDifferentiatedBack(integrand);

        [Theory]
        [InlineData("(a*g + b*g*x)^4*(A + B*ln(k*((a + b*x)/(c + d*x))^n))")]
        [InlineData("(c*g + d*g*x)^4*(A + B*ln(k*((a + b*x)/(c + d*x))^n))")]
        [InlineData("(f + g*x)^3*(A + B*ln(k*((a + b*x)/(c + d*x))^n))")]
        public void TheLogarithmOfAPowerOfTheQuotientDifferentiatedApart(string integrand) => AnsweredAndDifferentiatedBack(integrand);

        [Theory]
        [InlineData("(A + B*ln(k*(a + b*x)/(c + d*x)))/(f + g*x)^5")]
        [InlineData("(A + B*ln(k*(a + b*x)^2/(c + d*x)^2))/(f + g*x)^4")]
        [InlineData("(a + b*ln(c*x^n))/(d + h*x)^4")]
        public void ARemainderOverAPowerOfALinearAskedWithoutTheConditionOfAZeroTerm(string integrand) => AnsweredAndDifferentiatedBack(integrand);

        private static void AnsweredAndDifferentiatedBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4).Substitute("d", 0.3)
                .Substitute("f", 0.9).Substitute("g", 1.1).Substitute("h", 0.5).Substitute("A", 0.6).Substitute("B", 0.8).Substitute("k", 1.7).Substitute("n", 1.4);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { 0.3, 0.6, 0.8, 1.3, 1.9, 2.9 })
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
