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
    /// A condition with no x in it holds for every x or for none, so the integral of <c>f provided C</c>
    /// is the integral of f, provided C. <c>(x^2 sin(x)) provided not k = 0</c> was declined where
    /// <c>x^2 sin(x)</c> is answered, and inner simplification writes such a condition at the top of
    /// the questions rules hand on: <c>d^0</c> is <c>1 provided not d = 0</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AConditionFreeOfTheVariableIntegralTest
    {
        [Theory]
        [InlineData("(x^2*sin(x)) provided not k = 0", "not k = 0")]
        [InlineData("(1/((a + b*x)*3*x)) provided not d = 0", "not d = 0")]
        [InlineData("(1/x) provided a > 0", "a > 0")]
        public void IsAnsweredUnderTheCondition(string integrand, string condition)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.Contains($"provided {condition}", text);
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("d", 0.9).Substitute("k", 0.4);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in new[] { 0.3, 0.8, 1.4, 2.1 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                Assert.False(got.IsNaN, $"d/dx of the antiderivative of {integrand} has no value at x = {at}, where the integrand is {want}");
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }

        /// <summary>Where the condition fails the integrand has no value, and neither has the answer.</summary>
        [Fact]
        public void HasNoValueWhereTheConditionFails()
        {
            var integral = "(1/x) provided a > 0".ToEntity().Integrate("x").Substitute("C", 0);
            Assert.True(integral.Substitute("a", -1).Substitute("x", 2).EvalNumerical().IsNaN);
        }
    }
}
