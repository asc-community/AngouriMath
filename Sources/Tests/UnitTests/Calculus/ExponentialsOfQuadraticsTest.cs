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
    /// A product of exponentials of quadratics, with sines, cosines and hyperbolic functions of
    /// quadratics beside them, multiplied out into exponentials: each term is the Gaussian's.
    /// https://github.com/asc-community/AngouriMath/issues/1501
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ExponentialsOfQuadraticsTest
    {
        private static readonly double[] Points = { -1.3, -0.6, 0.35, 0.9, 1.45 };

        /// <summary>
        /// Integrates, pins the parameters, and compares the derivative of the answer with the
        /// integrand at <see cref="Points"/>. The parameters are pinned after integrating, so the
        /// rule is asked the symbolic question.
        /// </summary>
        private static Entity DifferentiatesBack(string integrand, params (string Name, string Value)[] pins)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Entity Pinned(Entity e) => pins.Aggregate(e, (current, pin) => current.Substitute(pin.Name, pin.Value.ToEntity()));
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in Points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            return integral;
        }

        private static readonly (string, string)[] Pins =
            { ("a", "1/3"), ("b", "1/2"), ("c", "-1"), ("d", "2/3"), ("f", "2"), ("g", "3"), ("p", "3/4"), ("q", "1/5") };

        /// <summary>
        /// Rubi's 4.7.6: a sine or cosine written as exponentials of the imaginary unit times its
        /// argument, so the answer holds an error function of a complex argument.
        /// </summary>
        [Theory]
        [InlineData("e^(x^2)*sin(x)")]
        [InlineData("e^(x^2)*cos(2*x + 1)")]
        [InlineData("e^x*sin(x^2)")]
        [InlineData("e^x*cos(1 + x + x^2)")]
        [InlineData("2^(x^2 + x)*cos(x^2)")]
        [InlineData("e^(-x^2)*sin(x)^2")]
        [InlineData("e^x*cos(x^2 + x)^3")]
        [InlineData("x*e^(x^2)*sin(x)")]
        [InlineData("f^(a + b*x + c*x^2)*sin(d + p*x + q*x^2)")]
        [InlineData("f^(a + b*x)*cos(d + q*x^2)^2")]
        [InlineData("f^(a + c*x^2)*sin(d + p*x)^3")]
        public void ASineOrCosineBesideAnExponential(string integrand)
            => DifferentiatesBack(integrand, Pins);

        /// <summary>Rubi's 6.1.5 and 6.2.5, where every exponent is real.</summary>
        [Theory]
        [InlineData("e^x*sinh(x^2)")]
        [InlineData("2^(x^2)*cosh(x + 1)")]
        [InlineData("e^(x^2)*sinh(x)^2")]
        [InlineData("f^(a + b*x + c*x^2)*sinh(d + p*x + q*x^2)")]
        [InlineData("f^(a + c*x^2)*cosh(d + p*x)^2")]
        [InlineData("f^(a + b*x)*sinh(d + q*x^2)^3")]
        public void AHyperbolicBesideAnExponential(string integrand)
            => DifferentiatesBack(integrand, Pins);

        /// <summary>
        /// Rubi's 6.1.4 and 6.2.4: a power of a hyperbolic function of a quadratic beside a
        /// polynomial, which was taken by parts past the budget.
        /// </summary>
        [Theory]
        [InlineData("x^2*sinh(1 + x + x^2)^2")]
        [InlineData("(1 + 2*x)^2*cosh(x - x^2)^2")]
        [InlineData("x^2*cosh(a + b*x + c*x^2)^2")]
        public void APowerOfAHyperbolicBesideAPolynomial(string integrand)
            => DifferentiatesBack(integrand, Pins);

        /// <summary>
        /// A polynomial, or a power of <c>x</c> below the bar, beside terms whose exponents have no
        /// linear part: the moments are taken about 0, and a negative even power is one too.
        /// Rubi's 6.1.3, <c>x^2 sinh(a + b x^2)^3</c> and <c>sinh(a + b x^2)^3/x^2</c>.
        /// </summary>
        [Theory]
        [InlineData("x^2*sinh(a + b*x^2)^3")]
        [InlineData("sinh(a + b*x^2)^3/x^2")]
        [InlineData("cosh(a + b*x^2)^3/x^4")]
        [InlineData("(1 + x^2)*e^(c*x^2)*sin(q*x^2)")]
        [InlineData("x^3*f^(a + c*x^2)*cos(d + q*x^2)")]
        public void APowerBesideTermsWithoutALinearPart(string integrand)
            => DifferentiatesBack(integrand, Pins);

        /// <summary>Rubi's 2.3: two exponentials of quadratics, of different bases.</summary>
        [Theory]
        [InlineData("2^(x^2)*3^(x + 1)")]
        [InlineData("f^(a + b*x + c*x^2)*g^(d + p*x + q*x^2)")]
        public void TwoExponentials(string integrand)
            => DifferentiatesBack(integrand, Pins);

        /// <summary>
        /// A sine or cosine beside a polynomial only, or of a linear argument beside an
        /// exponential of one, is left to the rules that answer it without an imaginary unit.
        /// </summary>
        [Theory]
        [InlineData("x*sin(x^2)")]
        [InlineData("e^x*sin(2*x)")]
        [InlineData("e^(2*x)*cos(x)^2")]
        public void ARealAnswerStaysReal(string integrand)
            => Assert.DoesNotContain(DifferentiatesBack(integrand).Nodes, node => node == MathS.i);
    }
}
