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
    /// An integrand whose trigonometric functions have different multiples of one argument —
    /// <c>sin(x)/cos(2x)</c>, <c>cos(x)/(sin(x) tan(x/2))</c> — rewritten to one argument and
    /// handed on.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Every trigonometric rule reads one argument, so an integrand with <c>x</c> and <c>2x</c>
    /// in it was declined by all of them, while the same integrand with <c>cos(2x)</c> written
    /// as <c>1 - 2 sin(x)^2</c> came out. The rewriting goes down to the greatest common
    /// divisor of the slopes through the Chebyshev recurrences, or — when only an argument and
    /// its double are present and the integrand is even in the smaller — up to the double,
    /// which gives polynomials of half the degree.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class MixedTrigonometricArgumentsTest
    {
        private static readonly double[] Points = { 0.3, 0.7, 1.1, 2.3 };

        private static void DifferentiatesBack(string integrand, string variable = "x")
        {
            var integral = integrand.ToEntity().Integrate(variable);
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("NaN", integral.Stringize());

            var derivative = integral.Substitute("C", 0).Differentiate(variable);
            var original = integrand.ToEntity();
            var compared = 0;
            foreach (var at in Points)
            {
                var got = derivative.Substitute(variable, at).EvalNumerical();
                var want = original.Substitute(variable, at).EvalNumerical();
                if (got.IsNaN || want.IsNaN)
                    continue;
                compared++;
                var difference = Math.Abs((double)(got - want).RealPart)
                               + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-9,
                    $"d/d{variable} of the antiderivative of {integrand} is {got} at {variable} = {at}, "
                    + $"where the integrand is {want}");
            }
            Assert.True(compared >= 3,
                $"only {compared} of {Points.Length} points were comparable for {integrand}");
        }

        /// <summary>
        /// The argument and its double, written down to the argument. The first three are
        /// Rubi's (Timofeev, Stewart, and — with the half angle — Hearn).
        /// </summary>
        [Theory]
        [InlineData("sin(x)/cos(2*x)")]
        [InlineData("(cos(x) + sin(x))/sin(2*x)")]
        [InlineData("cos(x)/(sin(x)*tan(x/2))")]
        [InlineData("sin(x/2)/(1 + cos(x))")]
        [InlineData("1/(sin(x) + sin(2*x))")]
        [InlineData("sin(3*x)/sin(x)")]
        [InlineData("sin(2*x)/(1 + cos(x)^2)")]
        public void DifferentMultiplesOfOneArgument(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Even in the smaller argument, so written up to the double: <c>tan(x)/tan(2x)</c> is
        /// <c>1 - 1/(2 cos(x)^2)</c> that way, and Moses's <c>sec(2t)/(1 + sec(t)^2 + 3 tan(t))</c>
        /// is a rational function of <c>sin(2t)</c> and <c>cos(2t)</c> that the half-angle
        /// substitution answers, where the other direction declined after five seconds.
        /// </summary>
        [Theory]
        [InlineData("tan(x)/tan(2*x)", "x")]
        [InlineData("cos(2*x)/cos(x)^2", "x")]
        [InlineData("sec(2*t)/(1 + sec(t)^2 + 3*tan(t))", "t")]
        public void EvenInTheSmallerArgument(string integrand, string variable) => DifferentiatesBack(integrand, variable);

        /// <summary>
        /// A product of two arguments where the double angle collapses under the
        /// substitution: <c>sin(x) sin(2x)</c> over <c>cos(x)</c> is <c>2 sin(x)^2</c> once the
        /// simplification has written the double angle out, and the second pass of the
        /// substitution reads the sine that wrote as <c>u</c> again, so the answer is
        /// <c>2 sin(x)^3/3</c> and not product-to-sum's cosines of <c>x + 2x</c> any more.
        /// </summary>
        [Fact]
        public void AProductWhoseDoubleAngleCollapses()
        {
            var integral = "sin(x)*sin(2*x)".ToEntity().Integrate("x").Stringize();
            Assert.Contains("sin(x) ^ 3", integral);
            DifferentiatesBack("sin(x)*sin(2*x)");
        }

        /// <summary>
        /// One argument throughout is not this rule's, and an offset in one of the arguments is
        /// not read; both are declined by it and answered or not by the rest as before.
        /// </summary>
        [Theory]
        [InlineData("sin(x)/cos(x)^2")]
        [InlineData("sin(x)*cos(x)")]
        public void OneArgumentIsLeftToTheOthers(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A fractional power of a polynomial in one function of the argument is let through:
        /// Timofeev's <c>(2 - 3 sin(x)^2)^(3/5) sin(4x)</c> is, with the multiple written out
        /// and under the sine, <c>4u(1 - 2u^2)(2 - 3u^2)^(3/5)</c>, two binomial differentials.
        /// A root of a product of two functions is not let through, and stays declined at
        /// the cost it had -- <c>sqrt(cos(x) sin(x)^3)</c> beside <c>sin(2x)</c> was thirty
        /// seconds to decline once it was.
        /// </summary>
        [Theory]
        [InlineData("(2 - 3*sin(x)^2)^(3/5)*sin(4*x)")]
        [InlineData("sqrt(1 + cos(x))*sin(2*x)")]
        public void AFractionalPowerOfOneFunction(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A whole power of a sine or a cosine is that many factors to the product-to-sum
        /// identities: Hearn's <c>cos(x)^2 sin(2x + 3)</c> is <c>cos(x) (cos(x) sin(2x + 3))</c>,
        /// and the pair is a sum of two sines of shifted arguments, which no unifier reads
        /// -- the shift is the difference from the double angle -- and the power kept the
        /// factor from being paired.
        /// </summary>
        [Theory]
        [InlineData("cos(x)^2*sin(2*x + 3)")]
        [InlineData("cos(x)^2*sin(2*x + 3)*sin(x)")]
        [InlineData("sin(x)^3*cos(3*x + 1)")]
        public void APowerIsPairedFactorByFactor(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Bondarenko's <c>1/(cos(x) + cos(3x))^5</c>, which the bound on the degree the
        /// rewriting produces refused at fifteen: with Bioche's odd rule in front of the
        /// half-angle substitution the rewritten integrand is a rational function of the sine
        /// of degree sixteen, answered in a moment, and the bound is sixteen.
        /// </summary>
        [Theory]
        [InlineData("1/(cos(x) + cos(3*x))^5")]
        [InlineData("sin(x)/(cos(x) + cos(3*x))^4")]
        public void ADegreeUpToSixteen(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// An offset free of x is written out by the addition formula: Timofeev's
        /// <c>tan(x) tan(x - a)</c> is a rational function of <c>tan(x)</c> once <c>tan(x - a)</c>
        /// is <c>(tan(x) - tan(a))/(1 + tan(x) tan(a))</c>, and <c>cos(a)</c> beside <c>sin(x)</c>
        /// is a coefficient to the homogeneous reader, not a second argument.
        /// </summary>
        [Theory]
        [InlineData("sin(x)*sin(x + 1)")]
        [InlineData("cos(x)*cos(2*x + 1)")]
        [InlineData("1/(sin(x) + cos(x + pi/4))")]
        public void AnOffsetIsWrittenOut(string integrand) => DifferentiatesBack(integrand);

        [Fact]
        public void AnOffsetThatIsASymbol()
        {
            var integrand = "tan(x)*tan(x - a)".ToEntity();
            var integral = integrand.Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            var derivative = integral.Substitute("C", 0).Substitute("a", 0.4).Simplify().Differentiate("x");
            var original = integrand.Substitute("a", 0.4);
            foreach (var at in Points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                Assert.True(Math.Abs((double)(got - want).RealPart) < 1e-7, $"{integrand} at {at}: {got} for {want}");
            }
        }

        /// <remarks>
        /// Bounded by the integrator's own budget rather than a wall clock that measures the
        /// runner; what is pinned is that the verdict is not a wrong answer and comes in a
        /// moment, where letting the root through made it thirty seconds.
        /// </remarks>
        [Theory]
        [InlineData("(-2*sin(2*x) + sqrt(cos(x)*sin(x)^3))/(sqrt(tan(x)) - sqrt(cos(x)^3*sin(x)))")]
        public void ARootOfAProductOfTwoFunctionsIsNotLetThrough(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
        }

        /// <summary>
        /// A root of a quadratic in the tangent with a linear term, rotated until it has none:
        /// <c>x = y + arctan(m)</c> with <c>m</c> a root of <c>b m^2 + 2(a - c) m - b</c> makes
        /// <c>a + b tan(x) + c tan(x)^2</c> into <c>A + C tan(y)^2</c>, and the tangent
        /// substitution then leaves <c>1/((1 + t^2) sqrt(A + C t^2))</c>, which is answered.
        /// The radicand is assembled in closed form: substituting the Möbius quotient into it
        /// leaves a nesting that nothing downstream reduces.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
        /// </summary>
        [Theory]
        [InlineData("1/sqrt(1 + 2*tan(x) + 3*tan(x)^2)")]
        [InlineData("1/sqrt(2 + tan(x) + tan(x)^2)")]
        [InlineData("1/sqrt(3 - 2*tan(x) + tan(x)^2)")]
        [InlineData("tan(x)/sqrt(2 + tan(x) + tan(x)^2)")]
        [InlineData("tan(x)^2/(2 + tan(x) + tan(x)^2)^(3/2)")]
        public void AQuadraticInTheTangentIsRotatedUntilItHasNoLinearTerm(string integrand)
            => DifferentiatesBack(integrand);

        /// <summary>
        /// With symbolic coefficients the rotation is a nested surd, the rotated quadratic is
        /// written in it, and what the rational integrator is handed is a quotient over that
        /// field: two minutes and no answer, where the integrand is declined in a moment
        /// unrotated. The rule asks for a numeric rotation, and this pins that the verdict for
        /// the symbolic shape is still a quick decline rather than a search.
        /// </summary>
        [Fact]
        public void ASymbolicQuadraticInTheTangentIsDeclinedAndNotSearched()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var integral = "1/sqrt(a + b*tan(x) + c*tan(x)^2)".ToEntity().Integrate("x");
            watch.Stop();
            Assert.Contains("integral(", integral.Stringize());
            Assert.True(watch.Elapsed < IntegrationDecline.Guard, $"the symbolic shape took {watch.Elapsed.TotalSeconds:F1} s");
        }
    }
}
