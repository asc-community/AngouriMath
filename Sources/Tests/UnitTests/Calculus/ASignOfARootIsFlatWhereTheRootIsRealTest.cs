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
    /// The sign and the modulus of an expression real only where the bases of its roots are
    /// positive are differentiated as on the real line there, and left unevaluated elsewhere.
    /// The antiderivatives of Rubi's <c>sqrt(1 + x + sqrt(1 + x))</c>,
    /// <c>(x - 4)/((1 + x^(1/3)) sqrt(x))</c> and <c>x^(-1 - 3n/2) (b x^n)^(3/2)</c> carry
    /// <c>sgn(sqrt(1 + x) + 1/2)</c>, <c>sgn(x^(1/3))</c> and <c>sgn(x^(n/2))</c>, and could not be
    /// checked by differentiating them back at any point.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1186">#1186</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ASignOfARootIsFlatWhereTheRootIsRealTest
    {
        [Theory]
        [InlineData("sgn(sqrt(1 + x) + 1/2)", "0.31", "0")]
        [InlineData("sgn(x^(1/3))", "2", "0")]
        [InlineData("sgn(sqrt(x) - 1)", "3", "0")]
        [InlineData("sgn(sqrt(sqrt(x) - 1) - 2)", "9", "0")]
        [InlineData("abs(sqrt(x) - 1)", "4", "1/4")]
        [InlineData("abs(sqrt(x) - 1)", "1/4", "-1")]
        [InlineData("abs(sqrt(1 + x) + 1/2)", "3", "1/4")]
        public void WhereTheBasesArePositiveTheRealLineRuleHolds(string expression, string at, string derivative)
            => Assert.Equal(derivative.ToEntity().Evaled,
                expression.ToEntity().Differentiate("x").Substitute("x", at.ToEntity()).Evaled);

        // Where a base is negative the root is imaginary, and sgn(z) is z / |z|, which turns with
        // z: nothing is claimed there.
        [Theory]
        [InlineData("sgn(sqrt(1 + x) + 1/2)", "-2")]
        [InlineData("abs(sqrt(x) - 1)", "-3")]
        [InlineData("sgn(sqrt(sqrt(x) - 1) - 2)", "1/4")]
        public void WhereABaseIsNegativeNothingIsClaimed(string expression, string at)
        {
            var derivative = expression.ToEntity().Differentiate("x").Substitute("x", at.ToEntity()).Evaled;
            Assert.IsNotType<Number.Integer>(derivative);
            Assert.Contains("derivative(", derivative.Stringize());
        }

        // A symbol is complex, and so is a constant off the real line: those are left as they were.
        [Theory]
        [InlineData("sgn(sqrt(x) + i)")]
        [InlineData("sgn(sqrt(a * x))")]
        [InlineData("abs(x^(1/2) * a)")]
        [InlineData("sgn(x^x)")]
        public void WhatIsNotRealOnAnIntervalIsLeftUnevaluated(string expression)
            => Assert.IsType<Derivativef>(expression.ToEntity().Differentiate("x"));

        // The derivative of such a sign is a piecewise one case of which is the derivative node
        // itself, and the node's simplification asks for the derivative: it has to stop.
        [Theory(Timeout = 30000)]
        [InlineData("derivative(sgn(sqrt(x) - 1), x)")]
        [InlineData("derivative(abs(sqrt(x) - 1), x)")]
        [InlineData("derivative(sgn(sqrt(x) - 1), x, 2)")]
        [InlineData("derivative(sgn(sqrt(x) - 1) * x, x)")]
        public void TheDerivativeNodeOfSuchASignTerminates(string expression)
        {
            var entity = expression.ToEntity();
            Assert.NotNull(entity.Simplify());
            Assert.NotNull(entity.Evaled);
        }

        [Theory]
        [InlineData("x^(-1-3/2*n)*(b*x^n)^(3/2)", "0.31,0.57,0.83,1.29,1.77,2.41")]
        [InlineData("x^(-1-1/2*n)*(b*x^n)^(1/2)", "0.31,0.57,0.83,1.29,1.77,2.41")]
        [InlineData("x^(-1+1/2*n)/(b*x^n)^(1/2)", "0.31,0.57,0.83,1.29,1.77,2.41")]
        [InlineData("x^(-1+3/2*n)/(b*x^n)^(3/2)", "0.31,0.57,0.83,1.29,1.77,2.41")]
        [InlineData("x^(-1+1/2*q)/sqrt(b*x^n+c*x^(2*n-q)+a*x^q)", "0.31,0.57,0.83,1.29,1.77,2.41")]
        [InlineData("(-4+x)/((1+x^(1/3))*sqrt(x))", "0.31,0.57,0.83,1.29,1.77,2.41")]
        [InlineData("sqrt(1+x+sqrt(1+x))", "-0.83,-0.31,0.57,1.29,2.41,5.7")]
        [InlineData("1/sqrt(x+sqrt(1+x))", "-0.57,-0.31,0.57,1.29,2.41,5.7")]
        [InlineData("sqrt(-1+x-sqrt(x))/((-1+x)*sqrt(x))", "2.7,3.1,3.9,4.6,5.7,8.3")]
        [InlineData("(1+2*sqrt(1+x))/(x*sqrt(1+x)*sqrt(x+sqrt(1+x)))", "-0.57,-0.31,0.57,1.29,2.41,5.7")]
        public void AnAntiderivativeCarryingSuchASignDifferentiatesBack(string integrand, string points)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4)
                .Substitute("n", 2.3).Substitute("q", 1.1);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var point in points.Split(','))
            {
                var at = double.Parse(point, System.Globalization.CultureInfo.InvariantCulture);
                var want = original.Substitute("x", at).EvalNumerical();
                if (want.IsNaN || Math.Abs((double)want.ImaginaryPart) > 1e-12)
                    continue;
                var got = derivative.Substitute("x", at).EvalNumerical();
                compared++;
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand}");
        }
    }
}
