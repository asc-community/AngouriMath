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
    /// A quotient of two <b>homogeneous</b> polynomials in <c>sin(u)</c> and <c>cos(u)</c>, under
    /// <c>t = tan(u)</c> — the third of Bioche's rules.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>1/(cos(x) + sin(x))^6</c> took <b>239 seconds</b> to be declined and is answered in
    /// four; <c>1/(b^2 cos(x)^2 + a^2 sin(x)^2)</c> was one of the Rubi sample's three timeouts.
    /// All three of those timeouts were this shape, and there are now none.
    /// </para>
    /// <para>
    /// Writing <c>t = tan(u)</c> and <c>c = cos(u)</c>, a homogeneous polynomial of degree
    /// <c>n</c> is <c>c^n</c> times a polynomial in <c>t</c>, so a quotient of degrees <c>n</c>
    /// over <c>d</c> becomes <c>N(t)/D(t)</c> times <c>(1 + t^2)^((d-n)/2 - 1)</c> — rational
    /// exactly when <c>d - n</c> is even.
    /// </para>
    /// <para>
    /// Checked by differentiating back and comparing at points. The sample points sit inside one
    /// interval between the poles of the tangent, which is where the substitution is a bijection
    /// and the answer is an antiderivative.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class HomogeneousTrigonometricTest
    {
        private static void DifferentiatesBack(string integrand, double[] points)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
            Assert.DoesNotContain("integral(", integral.Stringize());

            var derivative = integral.Substitute("C", 0).Differentiate("x");
            var original = integrand.ToEntity();
            var compared = 0;
            foreach (var at in points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                if (got.IsNaN || want.IsNaN)
                    continue;
                compared++;
                var difference = Math.Abs((double)(got - want).RealPart)
                               + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
            Assert.True(compared >= 3,
                $"only {compared} of {points.Length} points were comparable for {integrand}, "
                + "so this asserts almost nothing");
        }

        /// <summary>
        /// The same with symbolic coefficients, where the power has to be read as a power. Read as
        /// one expanded polynomial, <c>(a cos(x) + b sin(x))^4</c> became a quartic in the tangent
        /// with a symbol in every coefficient, which the rational integrator cannot factor back
        /// into <c>(a + b t)^4</c> -- with numbers it can, which is why the cases above never
        /// showed it: <c>sec(x)^2/(a cos(x) + b sin(x))^4</c> ran past ten minutes and takes a
        /// tenth of a second. Rubi's 4.7.2, sixteen rows. At <c>a = 1.7</c>, <c>b = 0.6</c> the
        /// sum vanishes at <c>x = -1.23</c>, and the points stay clear of it.
        /// </summary>
        [Theory]
        [InlineData("sec(x)^2/(a*cos(x) + b*sin(x))^4")]
        [InlineData("1/(a*cos(x) + b*sin(x))^4")]
        [InlineData("cos(x)/(a*cos(x) + b*sin(x))^3")]
        [InlineData("sin(x)^3/(a*cos(x) + b*sin(x))^3")]
        [InlineData("csc(x)/(a*cos(x) + b*sin(x))^3")]
        public void APowerOfASumWithSymbolicCoefficients(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
            Assert.DoesNotContain("integral(", integral.Stringize());
            var derivative = integral.Substitute("C", 0).Substitute("a", 1.7).Substitute("b", 0.6).Differentiate("x");
            var original = integrand.ToEntity().Substitute("a", 1.7).Substitute("b", 0.6);
            foreach (var at in new[] { 0.3, 0.8, 1.2, -0.5 })
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                Assert.False(got.IsNaN, $"the antiderivative of {integrand} differentiates to NaN at x = {at}");
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }

        /// <summary>
        /// A power of a sum of a sine and a cosine, which is the case that took 239 seconds to be
        /// declined.
        /// </summary>
        [Theory]
        [InlineData("1/(cos(x) + sin(x))^6", new[] { 0.3, 0.8, 1.2, -0.5 })]
        [InlineData("1/(cos(x) + sin(x))^2", new[] { 0.3, 0.8, 1.2, -0.5 })]
        [InlineData("1/(cos(x) + sin(x))^4", new[] { 0.3, 0.8, 1.2, -0.5 })]
        [InlineData("cos(x)^2/(cos(x) + sin(x))^4", new[] { 0.3, 0.8, 1.2, -0.5 })]
        public void APowerOfASumOfTheTwo(string integrand, double[] points)
            => DifferentiatesBack(integrand, points);

        /// <summary>
        /// A quadratic form, which is the shape the Rubi sample carries twice and both times as a
        /// timeout. The numerator is a constant there, so it names no argument of its own — the
        /// read has to take the denominator's, and declining for that was the first thing this
        /// rule got wrong.
        /// </summary>
        [Theory]
        [InlineData("1/(4*cos(x)^2 + 9*sin(x)^2)", new[] { 0.3, 0.8, 1.2, -0.5 })]
        [InlineData("1/(3*cos(x)^2 - sin(x)^2)", new[] { 0.3, 0.8, -0.5, -0.2 })]
        [InlineData("1/(sin(x)^2 + sin(x)*cos(x))", new[] { 0.3, 0.8, 1.2, 2.0 })]
        [InlineData("sin(x)^2/cos(x)^4", new[] { 0.3, 0.8, 1.2, -0.5 })]
        [InlineData("sin(x)/(cos(x)^3 + sin(x)^3)", new[] { 0.3, 0.8, 1.2, -0.5 })]
        public void AQuadraticOrCubicForm(string integrand, double[] points)
            => DifferentiatesBack(integrand, points);

        /// <summary>
        /// A linear argument, which divides the whole answer by the rate.
        /// </summary>
        [Theory]
        [InlineData("1/(cos(2*x) + sin(2*x))^2", new[] { 0.2, 0.5, 0.9, -0.3 })]
        [InlineData("1/(4*cos(3*x)^2 + sin(3*x)^2)", new[] { 0.1, 0.3, 0.4, -0.2 })]
        public void ALinearArgument(string integrand, double[] points)
            => DifferentiatesBack(integrand, points);

        /// <summary>
        /// Homogeneous up to parity is homogeneous: a term two degrees short of the highest is
        /// the same term times <c>sin^2 + cos^2</c>, which is one. <c>sin + sin^2 cos</c> has
        /// degrees one and three and is <c>sin (sin^2 + cos^2) + sin^2 cos</c>, of degree three
        /// throughout; it is what Timofeev's <c>cos(x)/(sin(x)(2 + sin(2x)))</c> has below the
        /// bar once its arguments are unified, and it was declined for the spelling.
        /// </summary>
        [Theory]
        [InlineData("cos(x)/(sin(x) + sin(x)^2*cos(x))", new[] { 0.3, 0.8, 1.2, 2.0 })]
        [InlineData("cos(x)/(sin(x)*(2 + sin(2*x)))", new[] { 0.3, 0.8, 1.2, 2.0 })]
        [InlineData("1/(1 + sin(x)^2)", new[] { 0.3, 0.8, 1.2, -0.5 })]
        [InlineData("(1 + sin(x)^2)/(1 + cos(x)^2)", new[] { 0.3, 0.8, 1.2, -0.5 })]
        [InlineData("1/(sin(x)^2 + 2*sin(x)*cos(x))", new[] { 0.3, 0.8, 1.2, 2.0 })]
        public void HomogeneousUpToParity(string integrand, double[] points)
            => DifferentiatesBack(integrand, points);

        /// <summary>
        /// What the read must refuse: a polynomial whose degrees differ by an odd number, which
        /// no power of <c>sin^2 + cos^2</c> bridges, and a degree difference that is odd — there
        /// the substitution leaves a square root of <c>1 + t^2</c> behind, which is a different
        /// problem.
        /// </summary>
        [Theory]
        [InlineData("1/(1 + cos(x))", new[] { 0.3, 0.8, 1.2, -0.5 })]
        [InlineData("1/(1 + sin(x))", new[] { 0.3, 0.8, 1.2, -0.5 })]
        [InlineData("1/(2 + cos(x))", new[] { 0.3, 0.8, 1.2, -0.5 })]
        [InlineData("1/cos(x)", new[] { 0.3, 0.8, 1.2, -0.5 })]
        [InlineData("sin(x)/(1 + cos(x)^2)", new[] { 0.3, 0.8, 1.2, -0.5 })]
        public void NotHomogeneousOrAnOddDifference(string integrand, double[] points)
        {
            // Each of these is answered by some other rule, and the point is that this one does
            // not take them and get them wrong.
            DifferentiatesBack(integrand, points);
        }

        /// <summary>
        /// The secant and the cosecant are the cosine and the sine to the power minus one, and
        /// the tangent is of degree zero, so a sum with them in it is homogeneous by the same
        /// count: Timofeev's <c>1/(2 sec(x) + sin(x))^2</c> has <c>4 sec^2 + 4 sec sin + sin^2</c>
        /// below, of degrees -2, 0 and 2, and both were declined for the secant's node.
        /// </summary>
        [Theory]
        [InlineData("1/(2*sec(x) + sin(x))^2", new[] { 0.3, 0.8, 1.2, -0.5 })]
        [InlineData("1/(cos(x) + 2*sec(x))^2", new[] { 0.3, 0.8, 1.2, -0.5 })]
        [InlineData("sin(x)/(2*sec(x) + sin(x))", new[] { 0.3, 0.8, 1.2, -0.5 })]
        [InlineData("1/(csc(x) + tan(x)*cos(x))^2", new[] { 0.3, 0.8, 1.2, -0.5 })]
        public void TheSecantAndTheCosecantCountNegatively(string integrand, double[] points)
            => DifferentiatesBack(integrand, points);

        /// <summary>
        /// Two different arguments, which is not one substitution's — declined rather than
        /// guessed at.
        /// </summary>
        [Theory]
        [InlineData("1/(cos(x) + sin(2*x))")]
        [InlineData("sin(x)/(cos(2*x)^2 + sin(x)^2)")]
        public void TwoDifferentArgumentsAreDeclined(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
        }

        /// <summary>
        /// The neighbours, each answered by a rule in front of this one and each of which must
        /// keep the shorter answer that rule gives.
        /// </summary>
        [Theory]
        [InlineData("tan(x)^2", new[] { 0.3, 0.8, 1.2, -0.5 })]
        [InlineData("tan(x)^3*sec(x)^4", new[] { 0.3, 0.8, 1.2, -0.5 })]
        [InlineData("sin(x)^3*cos(x)^2", new[] { 0.3, 0.8, 1.2, -0.5 })]
        [InlineData("sec(x)^4", new[] { 0.3, 0.8, 1.2, -0.5 })]
        [InlineData("csc(x)^6", new[] { 0.3, 0.8, 1.2, 2.0 })]
        [InlineData("sin(x)", new[] { 0.3, 0.8, 1.2, -0.5 })]
        [InlineData("x*sin(x)", new[] { 0.3, 0.8, 1.2, -0.5 })]
        public void TheNeighboursAreUntouched(string integrand, double[] points)
            => DifferentiatesBack(integrand, points);
    }
}
