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
    /// Integrands that are the derivative of a product of their powers with a function from
    /// inside their bracket in it besides -- <c>F^(c (a + b x)) (f x)^m sin(d + e x)</c> -- or that
    /// and a constant times a function of the bracket, with a power whose base and exponent both
    /// hold x kept as an exponential is, and three rewritings: a Pythagorean pair in a sum written
    /// as one, a negative exponential below the bar multiplied out, and a polynomial factor that is
    /// a constant multiple of a substitution's derivative divided out exactly. Rubi's 4.7.6, 4.7.7,
    /// 6.1.4, 6.2.4 and 6.7.1; each was declined, or ran out of time splitting the sum into terms
    /// that are not elementary alone.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AnExactDerivativeWithAFunctionRaisedFromNothingIntegralTest
    {
        [Theory]
        // A sine raised from nothing beside a symbolic power and an exponential of a constant base.
        [InlineData("f*g^(c*(a+b*x))*(f*x)^(-2+m)*(k*x*cos(d+k*x)+(-1+m+b*c*x*ln(g))*sin(d+k*x))")]
        [InlineData("f*g^(c*(a+b*x))*(f*x)^m*(k*x*cos(d+k*x)+(1+m+b*c*x*ln(g))*sin(d+k*x))")]
        [InlineData("g^(c*(a+b*x))*(f*x)^m*(k*x*cos(d+k*x)+(m+b*c*x*ln(g))*sin(d+k*x))/x")]
        // A sum whose cosine is in one term only, with x/sqrt(R) taken out of all three.
        [InlineData("x^4/(b*sqrt(x^3+3*sin(a+b*x)))+x^2*cos(a+b*x)/sqrt(x^3+3*sin(a+b*x))+4/3*x*sqrt(x^3+3*sin(a+b*x))/b")]
        // The derivative of -sinh(Q)/x beside 2 c cosh(Q), whose integral is the error functions'.
        [InlineData("-b*cosh(a+b*x+c*x^2)/x+sinh(a+b*x+c*x^2)/x^2")]
        [InlineData("-b*cosh(a+b*x-c*x^2)/x+sinh(a+b*x-c*x^2)/x^2")]
        [InlineData("cosh(a+b*x+c*x^2)/x^2-b*sinh(a+b*x+c*x^2)/x")]
        [InlineData("cosh(a+b*x-c*x^2)/x^2-b*sinh(a+b*x-c*x^2)/x")]
        // A power of a variable base to a variable exponent, kept.
        [InlineData("cosh(x)^x*(ln(cosh(x))+x*tanh(x))")]
        // The two rewritings.
        [InlineData("e^(cos(x)^2+sin(x)^2)")]
        [InlineData("(cos(x)+sin(x))/(1/e^x+sin(x))")]
        // A polynomial factor, or the polynomial parts of a sum, a constant multiple of the
        // derivative of the substitution's quadratic.
        [InlineData("1/2*b*tan(a+b*x+c*x^2)/c+x*tan(a+b*x+c*x^2)")]
        [InlineData("(b/(2*c)+x)*sin(a+b*x+c*x^2)^2")]
        [InlineData("(b/(2*c)+x)*sec(a+b*x+c*x^2)^2")]
        public void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.DoesNotContain("NaN", text);
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4).Substitute("d", 0.3)
                .Substitute("f", 1.1).Substitute("g", 2.3).Substitute("k", 0.9).Substitute("m", 1.7);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { 0.3, 0.8, 1.3, 1.9, 2.9, 3.4 })
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
        /// The candidate itself, closed: a sine raised beside the powers gives the one product, with
        /// no integral asked of anything.
        /// </summary>
        [Fact]
        public void TheSineIsRaisedIntoOneProduct()
        {
            var text = "g^(c*(a+b*x))*(f*x)^m*(k*x*cos(d+k*x)+(m+b*c*x*ln(g))*sin(d+k*x))/x".ToEntity().Integrate("x").Stringize();
            Assert.Contains("sin(d + k * x)", text);
            Assert.DoesNotContain("cos(", text);
        }
    }
}
