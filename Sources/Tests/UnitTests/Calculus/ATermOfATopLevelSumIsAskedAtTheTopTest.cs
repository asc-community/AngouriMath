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
    /// A term of a sum asked at the top is asked at the top: the rules scoped to the question
    /// asked answer it, where one level down they decline.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1265">#1265</a>
    /// </summary>
    /// <remarks>
    /// Linearity splits the integrand before anything else, and every term it produced sat one
    /// level down, where the five scoped rules decline. So <c>(x^3 - 1)/(2 + x^3)^(1/3)</c> was
    /// declined although each of its two terms is a binomial differential the rule answers when
    /// asked directly, and <c>sec(x)^3 + x</c> although the secant reduction answers the first
    /// term. A term of a top-level sum is not a continuation of any rule's search, which is what
    /// the scope was measured to stop.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class ATermOfATopLevelSumIsAskedAtTheTopTest
    {
        [Theory]
        [InlineData("(x^3 - 1)/(2 + x^3)^(1/3)")]
        [InlineData("sec(x)^3 + x")]
        [InlineData("x^5*sqrt(1 + x^3) + sec(x)^3")]
        public void TheTermsAreAnsweredByTheScopedRules(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
        }

        /// <summary>
        /// The terms as written first, and expanded only if one of those fails: expanding
        /// <c>(1 + 1/x)/(x + ln(x))^(3/2)</c> writes it as two terms, neither of which is the
        /// substitution <c>u = x + ln(x)</c> the unexpanded one is, and Bronstein's
        /// <c>1/x + (1 + 1/x)/(x + ln(x))^(3/2)</c> was declined for that while each of its two
        /// terms alone was answered.
        /// </summary>
        [Theory]
        [InlineData("1/x + (1 + 1/x)/(x + ln(x))^(3/2)")]
        [InlineData("(1 + 1/x)/(x + ln(x))^(3/2) - 1/x^2")]
        public void TheTermsAsWrittenBeforeExpanded(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            var derivative = integral.Substitute("C", 0).Differentiate("x");
            var original = integrand.ToEntity();
            foreach (var at in new[] { 0.7, 1.3, 2.4 })
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                var difference = System.Math.Abs((double)(got - want).RealPart);
                Assert.True(difference < 1e-8, $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }

        /// <summary>
        /// Each term alone is answered, which is what makes the sum's decline a defect of the
        /// split and not of the rules.
        /// </summary>
        [Theory]
        [InlineData("x^3/(2 + x^3)^(1/3)")]
        [InlineData("sec(x)^3")]
        public void EachTermAloneIsAnswered(string integrand)
            => Assert.DoesNotContain("integral(", integrand.ToEntity().Integrate("x").Stringize());
    }
}
