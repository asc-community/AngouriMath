//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath.Extensions;
using Xunit;
using static AngouriMath.Entity;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// The sign and the modulus of an expression with a logarithm in it are differentiated as on
    /// the real line where the logarithm's argument is positive, as they are where the bases of
    /// its roots are, and left unevaluated elsewhere. The antiderivative of Rubi's
    /// <c>sqrt(ln(a x^n)^2)/x</c> carries <c>sgn(ln(a x^n))</c>, and could not be checked by
    /// differentiating it back at any point.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1186">#1186</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ASignOfALogarithmIsFlatWhereTheLogarithmIsRealTest
    {
        [Theory]
        [InlineData("sgn(ln(x))", "3", "0")]
        [InlineData("sgn(ln(x))", "1/2", "0")]
        [InlineData("sgn(ln(2 * x^3))", "1/4", "0")]
        [InlineData("sgn(ln(x) - 1/2)", "5", "0")]
        [InlineData("abs(ln(x))", "3", "1/3")]
        [InlineData("abs(ln(x))", "1/2", "-2")]
        [InlineData("abs(ln(sqrt(x) + 1))", "4", "1/12")]
        public void WhereTheArgumentIsPositiveTheRealLineRuleHolds(string expression, string at, string derivative)
            => Assert.Equal(derivative.ToEntity().Evaled,
                expression.ToEntity().Differentiate("x").Substitute("x", at.ToEntity()).Evaled);

        // To another positive base as well: |log_2(x)|' is -1/(x ln 2) below 1.
        [Fact]
        public void ToAnotherPositiveBase()
        {
            var got = (double)((Number.Complex)"abs(log(2, x))".ToEntity().Differentiate("x").Substitute("x", "1/4".ToEntity()).EvalNumerical()).RealPart;
            Assert.Equal(-4 / Math.Log(2), got, 12);
        }

        // Where the argument is negative the logarithm is off the real line, and sgn(z) is
        // z / |z|, which turns with z: nothing is claimed there.
        [Theory]
        [InlineData("sgn(ln(x))", "-2")]
        [InlineData("abs(ln(x - 1))", "1/2")]
        [InlineData("sgn(ln(sqrt(x) - 1))", "1/4")]
        public void WhereTheArgumentIsNegativeNothingIsClaimed(string expression, string at)
        {
            var derivative = expression.ToEntity().Differentiate("x").Substitute("x", at.ToEntity()).Evaled;
            Assert.IsNotType<Number.Integer>(derivative);
            Assert.Contains("derivative(", derivative.Stringize());
        }

        // A symbol is complex, and so is a base off the real line or one in x: those are left
        // as they were.
        [Theory]
        [InlineData("sgn(ln(a * x))")]
        [InlineData("abs(ln(x) * a)")]
        [InlineData("sgn(log(i, x))")]
        [InlineData("sgn(log(x, 2))")]
        public void WhatIsNotRealOnAnIntervalIsLeftUnevaluated(string expression)
            => Assert.IsType<Derivativef>(expression.ToEntity().Differentiate("x"));

        [Theory(Timeout = 30000)]
        [InlineData("derivative(sgn(ln(x)), x)")]
        [InlineData("derivative(abs(ln(x)), x)")]
        [InlineData("derivative(sgn(ln(x)) * x, x, 2)")]
        public void TheDerivativeNodeOfSuchASignTerminates(string expression)
        {
            var entity = expression.ToEntity();
            Assert.NotNull(entity.Simplify());
            Assert.NotNull(entity.Evaled);
        }

        [Theory]
        [InlineData("sqrt(ln(a*x^n)^2)/x")]
        [InlineData("sqrt(ln(x)^2)/x")]
        [InlineData("abs(ln(a*x))/x")]
        public void AnAntiderivativeCarryingSuchASignDifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("n", 2.3);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { 0.11, 0.31, 0.57, 0.83, 1.29, 1.77, 2.41, 5.7 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                compared++;
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.Equal(8, compared);
        }
    }
}
