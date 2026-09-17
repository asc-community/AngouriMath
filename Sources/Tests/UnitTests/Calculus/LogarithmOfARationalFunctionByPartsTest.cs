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
    /// A rational function times a whole power of <c>A + B ln(R)</c>, <c>R</c> rational, by
    /// one closed step of parts with the power differentiated; and the rational function a
    /// constant multiple of the derivative, as a power of the whole.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// Rubi's <c>(f + g x)^m (A + B ln(e ((a + b x)/(c + d x))^n))^p</c> family, sampled: the
    /// substitution search spent its budget simplifying the integrand over each candidate's
    /// derivative with symbols in every coefficient, before the same step of parts was
    /// reached below it. The derivative of the logarithm is taken factor by factor of its
    /// argument, and the remainder term by term, so that each piece is a rational function
    /// over one linear factor. Every answer is differentiated back with the symbols pinned.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class LogarithmOfARationalFunctionByPartsTest
    {
        private static readonly double[] Points = { 0.31, 0.77, 1.43, 2.19 };

        private static void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("NaN", integral.Stringize());
            var derivative = integral.Substitute("C", 0).Differentiate("x");
            Entity original = integrand.ToEntity();
            var pinned = 0;
            foreach (var parameter in original.Vars)
            {
                if (parameter.Name == "x")
                    continue;
                var value = new[] { 1.3, 2.1, 0.7, 1.9, 3.1, 0.4, 2.7, 1.1 }[pinned++ % 8] + pinned / 8;
                derivative = derivative.Substitute(parameter, value);
                original = original.Substitute(parameter, value);
            }
            var compared = 0;
            foreach (var at in Points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                if (got.IsNaN || want.IsNaN)
                    continue;
                compared++;
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 3, $"only {compared} of {Points.Length} points were comparable for {integrand}");
        }

        /// <summary>Rubi's, with the polynomial above or below the bar and the power of the logarithm one or two.</summary>
        [Theory]
        [InlineData("A + B*ln(k*(a + b*x)^n/(c + d*x)^n)")]
        [InlineData("(f + g*x)*(A + B*ln(k*(a + b*x)^2/(c + d*x)^2))")]
        [InlineData("(A + B*ln(k*(a + b*x)/(c + d*x)))/(f + g*x)^5")]
        [InlineData("(A + B*ln(k*((a + b*x)/(c + d*x))^n))/(a*g + b*g*x)^3")]
        [InlineData("(A + B*ln(k*((a + b*x)/(c + d*x))^n))^2/(c*g + d*g*x)^2")]
        public void ARationalFunctionTimesAPowerOfTheLogarithm(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// The rational function a constant multiple of the derivative:
        /// <c>(A + B ln(e (a + b x)^n/(c + d x)^n))/((a + b x)(c + d x))</c> is
        /// <c>(A + B ln(...))^2/(2 B n (b c - a d))</c>, and for the reciprocal a logarithm.
        /// </summary>
        [Theory]
        [InlineData("(A + B*ln(k*(a + b*x)^n/(c + d*x)^n))/((a + b*x)*(c + d*x))")]
        [InlineData("(A + B*ln(k*(a + b*x)/(c + d*x)))^3/((a + b*x)*(c + d*x))")]
        [InlineData("1/((a + b*x)*(c + d*x)*(A + B*ln(k*(a + b*x)/(c + d*x))))")]
        public void TheDerivativeDivides(string integrand) => DifferentiatesBack(integrand);

        /// <summary>The same step for an arctangent, whose derivative is rational too.</summary>
        [Theory]
        [InlineData("x*(A + B*arctan(c*x))")]
        [InlineData("(A + B*arctan(c*x))^2/(1 + c^2*x^2)")]
        public void AnArctangentTheSameWay(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Powers of the logarithm's own two linears beside a power of it, by
        /// <c>t = (a + b x)/(c + d x)</c>: a rational function of <c>t</c> beside a logarithm
        /// of <c>t</c>, one closed step, where the step of parts above went on with its
        /// coefficients growing to <c>b^205</c> and the substitution search went past its
        /// budget. The exponents may be symbols.
        /// </summary>
        [Theory]
        [InlineData("(A + B*ln(k*((a + b*x)/(c + d*x))^n))^2/(a*g + b*g*x)^4")]
        [InlineData("(A + B*ln(k*(a + b*x)^n/(c + d*x)^n))^3/(a + b*x)^3")]
        [InlineData("(A + B*ln(k*(c + d*x)^2/(a + b*x)^2))^2/(a*g + b*g*x)^4")]
        [InlineData("(A + B*ln(k*(a + b*x)/(c + d*x)))/((a*g + b*g*x)^2*(c*j + d*j*x)^2)")]
        [InlineData("(c*j + d*j*x)^3*(A + B*ln(k*(a + b*x)/(c + d*x)))^2/(a*g + b*g*x)^6")]
        [InlineData("(A + B*ln(k*((a + b*x)/(c + d*x))^n))^2/((a*g + b*g*x)^4*(c*j + d*j*x)^3)")]
        [InlineData("(a*g + b*g*x)^(-2 - m)*(c*j + d*j*x)^m*(A + B*ln(k*((a + b*x)/(c + d*x))^n))^2")]
        public void ByTheQuotientOfTheLogarithmsLinears(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A power of <c>x</c>, or of <c>d x</c>, times a whole power of <c>A + B ln(c x^n)</c>
        /// by the closed reduction of <c>x^p ln(x)^n</c>, each step of parts bringing the
        /// factor <c>B n</c>; a lone power of the affine logarithm is the case <c>p = 0</c>,
        /// and a linear argument is the same under its shift.
        /// </summary>
        [Theory]
        [InlineData("(d*x)^m*(a + b*ln(c*x^n))")]
        [InlineData("(f*x)^q*(a + b*ln(c*(d*x^m)^n))^3")]
        [InlineData("x^(-2 - m)*(A + B*ln(k*x^n))^2")]
        [InlineData("(a + b*ln(c*x^n))^2/x")]
        [InlineData("(a + b*ln(c*(d*x^m)^n))^4")]
        [InlineData("(a + b*ln(c*(d*(f + g*x)^m)^n))^4")]
        public void APowerTimesAPowerOfAnAffineLogarithm(string integrand) => DifferentiatesBack(integrand);
    }
}
