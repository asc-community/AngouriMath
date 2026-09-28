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
    /// A power of x times an exponential of a quadratic in a logarithm, <c>x^p G^(Q(ln(c x^r)))</c>,
    /// onto the Gaussian by <c>t = ln(c x^r)</c>: Rubi's 2.3,
    /// <c>F^(f (a + b ln(c (d + e x)^n))^2) (g + h x)^m</c>, every one of which was left unevaluated.
    /// https://github.com/asc-community/AngouriMath/issues/1501
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ExponentialOfAQuadraticInALogarithmTest
    {
        /// <summary>Past 1, where every logarithm here is real for the pins below.</summary>
        private static readonly double[] Points = { 1.3, 1.9, 2.6, 3.4 };
        private static readonly (string Name, string Value)[] Pins =
            { ("F", "2"), ("a", "1/2"), ("b", "2/3"), ("c", "5/4"), ("n", "3"), ("m", "1/2"), ("d", "1/2"), ("k", "2"), ("f", "3/4"), ("g", "2"), ("h", "1/3") };

        private static Entity DifferentiatesBack(string integrand)
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
            return integral;
        }

        [Theory]
        [InlineData("e^(ln(x)^2)")]
        [InlineData("x*e^(ln(x)^2)")]
        [InlineData("e^(-ln(x)^2)/x^2")]
        [InlineData("2^(ln(x)^2 + ln(x))")]
        [InlineData("x^m*e^(ln(x)^2)")]
        public void OfTheLogarithmItself(string integrand) => DifferentiatesBack(integrand);

        [Theory]
        [InlineData("F^(f*(a + b*ln(c*x^n))^2)")]
        [InlineData("x^2*F^(f*(a + b*ln(c*x^n))^2)")]
        [InlineData("F^(-f*(a + b*ln(c*x^n)^2))/x^3")]
        [InlineData("x^m*F^(f*(a + b*ln(c*x^n))^2)")]
        [InlineData("(1 + x)^2*F^(f*(a + b*ln(c*x^n))^2)")]
        public void OfALogarithmOfAPower(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// The logarithm of a power of a linear, under <c>u = d + e x</c>, with a constant multiple
        /// of the linear or a polynomial beside it, as Rubi's 2.3 writes them.
        /// </summary>
        [Theory]
        [InlineData("F^(f*(a + b*ln(c*(d + k*x)^n))^2)")]
        [InlineData("(d*g + k*g*x)^m*F^(f*(a + b*ln(c*(d + k*x)^n))^2)")]
        [InlineData("F^(f*(a + b*ln(c*(d + k*x)^n)^2))/(d*g + k*g*x)^2")]
        [InlineData("(g + h*x)^3*F^(f*(a + b*ln(c*(d + k*x)^n))^2)")]
        [InlineData("e^(ln((d + k*x)^n)^2)*(d + k*x)^m")]
        public void OfALogarithmOfAPowerOfALinear(string integrand) => DifferentiatesBack(integrand);

        /// <summary>Beside <c>1/x</c> it is the Gaussian in <c>ln(x)</c> alone.</summary>
        [Fact]
        public void BesideTheReciprocalItIsTheGaussianOfTheLogarithm()
            => Assert.Contains(DifferentiatesBack("e^(ln(x)^2)/x").Nodes, node => node is Entity.Erfif);
    }
}
