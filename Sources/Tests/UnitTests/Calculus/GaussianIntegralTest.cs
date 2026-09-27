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
    /// The integrals that answer in the error functions: the Gaussian <c>F^(a x^2 + b x + c)</c>
    /// by the square it completes, the Gaussian beside an even power of <c>x</c> by parts two
    /// powers at a time, and the error functions themselves by parts against 1.
    /// https://github.com/asc-community/AngouriMath/issues/1501
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class GaussianIntegralTest
    {
        /// <summary>Off 0, where the negative powers are undefined.</summary>
        private static readonly double[] Points = { -1.7, -0.6, 0.35, 0.9, 1.45 };

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

        [Theory]
        [InlineData("e^(-x^2)")]
        [InlineData("e^(-x^2 + 2*x + 1)")]
        [InlineData("e^(-3*x^2 + x)")]
        [InlineData("(1/2)^(x^2)")]
        public void TheGaussianIsAnErrorFunction(string integrand)
            => Assert.Contains(DifferentiatesBack(integrand).Nodes, node => node is Entity.Erff);

        /// <summary>
        /// Where the square's coefficient is decidably positive the answer is written with
        /// <c>erfi</c> and a real root, not as an error function of an imaginary argument.
        /// </summary>
        [Theory]
        [InlineData("e^(x^2)")]
        [InlineData("2^(x^2)")]
        [InlineData("e^(2*x^2 - x)")]
        public void AGrowingGaussianIsAnImaginaryErrorFunction(string integrand)
        {
            var integral = DifferentiatesBack(integrand);
            Assert.Contains(integral.Nodes, node => node is Entity.Erfif);
            Assert.DoesNotContain(integral.Nodes, node => node == MathS.i);
        }

        /// <summary>
        /// A symbolic exponent is answered for the generic case, as <c>F^(a x)/(a ln F)</c> is,
        /// and the answer differentiates back whichever sign the square's coefficient turns out
        /// to have: the root of <c>-b ln f</c> is imaginary for a positive <c>b ln f</c>, and the
        /// answer is still right.
        /// </summary>
        [Theory]
        [InlineData("f^(a + b*x^2)", "-3/2")]
        [InlineData("f^(a + b*x^2)", "3/2")]
        [InlineData("x^2*f^(a + b*x^2)", "-3/2")]
        [InlineData("x^8*f^(a + b*x^2)", "-3/2")]
        [InlineData("f^(a + b*x^2)/x^4", "3/2")]
        public void ASymbolicGaussian(string integrand, string b)
            => DifferentiatesBack(integrand, ("f", "2"), ("a", "1/3"), ("b", b));

        [Fact]
        public void ASymbolicGaussianWithALinearTerm()
            => DifferentiatesBack("f^(a + b*x + c*x^2)", ("f", "3"), ("a", "1/2"), ("b", "2/3"), ("c", "-5/4"));

        /// <summary>A symbolic exponent carries no condition that its linear part is zero.</summary>
        [Fact]
        public void ASymbolicGaussianIsUnconditional()
            => Assert.DoesNotContain(DifferentiatesBack("f^(a + b*x^2)", ("f", "2"), ("a", "1/3"), ("b", "-1")).Nodes,
                node => node is Entity.Providedf);

        [Theory]
        [InlineData("x^2*e^(-x^2)")]
        [InlineData("x^4*e^(x^2)")]
        [InlineData("x^8*e^(-x^2)")]
        [InlineData("e^(-x^2)/x^2")]
        [InlineData("e^(x^2)/x^4")]
        [InlineData("3*x^6*e^(-2*x^2)/5")]
        public void TheGaussianBesideAnEvenPower(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// An odd negative power ends at <c>int e^(A x^2)/x</c>, which is the exponential
        /// integral, so it is not taken.
        /// </summary>
        [Fact]
        public void TheGaussianOverAnOddPowerIsNotAnErrorFunction()
            => Assert.Contains("integral(", "e^(-x^2)/x".ToEntity().Integrate("x").Stringize());

        [Theory]
        [InlineData("erf(x)")]
        [InlineData("erf(2*x + 1)")]
        [InlineData("erfc(3*x)")]
        [InlineData("erfi(x/2)")]
        [InlineData("5*erfc(1 - x)")]
        public void TheErrorFunctionsByParts(string integrand) => DifferentiatesBack(integrand);

        /// <summary>Over the whole line, through the error functions' values at <c>+-oo</c>.</summary>
        [Theory]
        [InlineData("e^(-x^2)", "sqrt(pi)")]
        [InlineData("e^(-x^2/2)", "sqrt(2 * pi)")]
        [InlineData("e^(-(x - 1)^2)", "sqrt(pi)")]
        public void TheGaussianOverTheWholeLine(string integrand, string value)
            => Assert.Equal(value.ToEntity().Simplify(), MathS.Integral(integrand.ToEntity(), "x", "-oo".ToEntity(), "+oo".ToEntity()).Simplify());
    }
}
