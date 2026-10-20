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
    /// A product of powers of quotients of linears with a symbol for an exponent, in which one
    /// linear stands in two of them, written over each linear once with a constant <c>K</c> in
    /// front: <c>e^atanh(a x) x^m sqrt(1 - a^2 x^2)</c> is <c>K x^m (a x + 1)</c>, and
    /// <c>e^(2 p atanh(a x)) (c - a^2 c x^2)^p</c> is <c>K (a x + 1)^(2 p)</c>. Rubi's 7.3.6 and
    /// 7.4.2, which ran out of time or were left unevaluated.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class EachLinearWrittenOnceUnderASymbolicPowerIntegralTest
    {
        /// <summary>
        /// On both sides of every root, so that <c>K</c> is checked wherever it takes another
        /// value, with the exponents pinned off the integers: the integrand is complex to the left
        /// of 0 and beyond the roots, and compared there too.
        /// </summary>
        private static readonly double[] Points = { -2.9, -1.6, -0.95, -0.4, -0.15, 0.15, 0.4, 0.95, 1.6, 2.9 };

        [Theory]
        [InlineData("e^atanh(a*x)*x^m*sqrt(1-a^2*x^2)", "a=1.37,m=0.37")]
        [InlineData("e^atanh(a*x)*x^m*(1-a^2*x^2)^(3/2)", "a=1.37,m=0.37")]
        [InlineData("e^atanh(a*x)*x^m*(c-a^2*c*x^2)^(5/2)", "a=1.37,c=2.11,m=0.37")]
        [InlineData("e^atanh(a*x)*x^m*(c-a^2*c*x^2)^(5/2)", "a=-1.37,c=-2.11,m=-0.37")]
        [InlineData("e^atanh(a*x)*x^m*sqrt(c-c/(a^2*x^2))", "a=1.37,c=2.11,m=0.37")]
        [InlineData("e^acoth(a*x)*x^m*sqrt(c-a^2*c*x^2)", "a=1.37,c=2.11,m=0.37")]
        [InlineData("x^m*sqrt(c-a^2*c*x^2)/e^acoth(a*x)", "a=1.37,c=-2.11,m=0.37")]
        [InlineData("e^acoth(a*x)*x^m*sqrt(c-c/(a^2*x^2))", "a=1.37,c=2.11,m=0.37")]
        [InlineData("e^(2*(1+p)*atanh(a*x))/(1-a^2*x^2)^p", "a=1.37,p=0.61")]
        [InlineData("e^(2*(1+p)*atanh(a*x))/(c-a^2*c*x^2)^p", "a=1.37,c=2.11,p=0.61")]
        [InlineData("e^(2*p*atanh(a*x))*(c-a^2*c*x^2)^p", "a=1.37,c=2.11,p=0.61")]
        [InlineData("e^(2*p*atanh(a*x))*(c-a^2*c*x^2)^p", "a=1.37,c=-2.11,p=-0.61")]
        [InlineData("(c-a^2*c*x^2)^p/e^(2*p*atanh(a*x))", "a=1.37,c=2.11,p=0.61")]
        [InlineData("e^(2*p*acoth(a*x))*(c-a^2*c*x^2)^p", "a=1.37,c=2.11,p=0.61")]
        [InlineData("(c-a^2*c*x^2)^p/e^(2*p*acoth(a*x))", "a=1.37,c=2.11,p=0.61")]
        [InlineData("e^(4*acoth(a*x))*(c-a*c*x)^p", "a=1.37,c=2.11,p=0.61")]
        [InlineData("e^(n*acoth(a*x))*(c-a*c*x)^(2+1/2*n)", "a=1.37,c=2.11,n=1.3")]
        [InlineData("e^(n*acoth(a*x))*(c-a*c*x)^(1+1/2*n)", "a=1.37,c=-2.11,n=1.3")]
        public void IsAnsweredWithEachLinearOnce(string integrand, string pins)
            => DifferentiatesBack(integrand, pins);

        /// <summary>
        /// Two powers that are not whole once each linear is written once -- here
        /// <c>K (a x + 1)^(n/2 - 1) (a x - 1)^(-n/2 - 1)</c> -- and a power of a linear beside a
        /// polynomial that shares its root, are left to the rules behind, which answer them.
        /// </summary>
        [Theory]
        [InlineData("e^(n*atanh(a*x))/(c-a^2*c*x^2)", "a=1.37,c=2.11,n=1.3")]
        [InlineData("e^(n*acoth(a*x))/(c-a*c*x)^2", "a=1.37,c=2.11,n=1.3")]
        [InlineData("(a+b*x)^m*(a^2-b^2*x^2)^2", "a=1.37,b=0.83,m=0.37")]
        public void IsLeftToTheRulesBehind(string integrand, string pins)
            => DifferentiatesBack(integrand, pins);

        private static void DifferentiatesBack(string integrand, string pins)
        {
            var integral = integrand.ToEntity().Integrate("x").Substitute("C", 0);
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.DoesNotContain("NaN", text);
            Assert.True(text.Length < 5000, $"{text.Length} characters of answer for {integrand}");
            Entity Pin(Entity e)
            {
                foreach (var pin in pins.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    var parts = pin.Split('=');
                    e = e.Substitute(parts[0], double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture));
                }
                return e;
            }
            var derivative = Pin(integral).Differentiate("x");
            var original = Pin(integrand.ToEntity());
            var compared = 0;
            foreach (var at in Points)
            {
                var want = original.Substitute("x", at).EvalNumerical();
                if (want.IsNaN)
                    continue;
                var got = derivative.Substitute("x", at).EvalNumerical();
                compared++;
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                Assert.True(difference < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand}");
        }
    }
}
