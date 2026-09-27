//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Linq;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// A power of x times a half-odd power of a logarithm, <c>x^p (A + B ln(c x^r))^n</c>, onto the
    /// Gaussian's moments by <c>t = sqrt(F)</c>: Rubi's 3.1.2, <c>(d x)^m (a + b ln(c x^n))^p</c> with
    /// <c>p</c> half an odd number, every one of which was left unevaluated.
    /// https://github.com/asc-community/AngouriMath/issues/1501
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class HalfOddPowerOfALogarithmTest
    {
        /// <summary>Past 1, where every logarithm here is real and positive for the pins below.</summary>
        private static readonly double[] Points = { 1.3, 1.9, 2.6, 3.4 };
        private static readonly (string Name, string Value)[] Pins =
            { ("a", "3/2"), ("b", "2/3"), ("c", "5/4"), ("n", "3"), ("m", "1/2"), ("d", "1/2"), ("k", "2"), ("f", "3/4"), ("g", "2"), ("h", "1/3"), ("p", "2"), ("q", "3/2") };

        private static void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Entity Pinned(Entity e) => Pins.Aggregate(e, (current, pin) => current.Substitute(pin.Name, pin.Value.ToEntity()));
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in Points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-9, $"d/dx of {integral} is {got} at x = {at}, where {integrand} is {want}");
            }
        }

        [Theory]
        [InlineData("sqrt(ln(x))")]
        [InlineData("x*sqrt(ln(x))")]
        [InlineData("x^2/sqrt(ln(x))")]
        [InlineData("sqrt(ln(x))/x^2")]
        [InlineData("x^3*ln(x)^(3/2)")]
        [InlineData("1/(x^3*ln(x)^(3/2))")]
        [InlineData("x/ln(x)^(5/2)")]
        public void OfTheLogarithmItself(string integrand) => DifferentiatesBack(integrand);

        [Theory]
        [InlineData("sqrt(ln(x^2))")]
        [InlineData("sqrt(ln(a*x^n))")]
        [InlineData("x^3*sqrt(ln(a*x^n))")]
        [InlineData("x^m/sqrt(ln(a*x^n))")]
        [InlineData("(a + b*ln(c*x^n))^(1/2)")]
        [InlineData("x^3*ln(a*x^n)^(3/2)")]
        [InlineData("x^2/ln(a*x^n)^(3/2)")]
        [InlineData("x^m/ln(a*x^n)^(5/2)")]
        public void OfALogarithmOfAPower(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// The logarithm of a power of a linear, with a polynomial beside it, under
        /// <c>u = d + e x</c>: Rubi's 3.3. Declined, these went by parts against the half-odd
        /// power's own antiderivative and took up to 55 s to give up.
        /// </summary>
        [Theory]
        [InlineData("sqrt(ln(2 + 3*x))")]
        [InlineData("(f + g*x)*(a + b*ln(c*(d + k*x)^n))^(3/2)")]
        [InlineData("(f + g*x)^2*sqrt(a + b*ln(c*(d + k*x)^n))")]
        [InlineData("(f + g*x)^3/sqrt(a + b*ln(c*(d + k*x)^n))")]
        [InlineData("(f + g*x)/(a + b*ln(c*(d + k*x)^n))^(5/2)")]
        [InlineData("(g + h*x)^2/sqrt(a + b*ln(c*(d*(k + f*x)^p)^q))")]
        public void OfALogarithmOfAPowerOfALinear(string integrand) => DifferentiatesBack(integrand);

        /// <summary>Beside <c>1/x</c> it is elementary, <c>F^(n + 1)/(s (n + 1))</c>.</summary>
        [Fact]
        public void BesideTheReciprocalItIsAPower()
            => Assert.Equal("2 * sqrt(ln(x)) + C".ToEntity(), "1/(x*sqrt(ln(x)))".ToEntity().Integrate("x"));
    }
}
