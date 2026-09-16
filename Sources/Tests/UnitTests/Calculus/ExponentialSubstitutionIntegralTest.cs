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
    /// Integrands that are rational in <c>e^(k x)</c>, which <c>u = e^(k x)</c> turns into
    /// rational functions of one variable.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// The hyperbolic functions are here because they are not nodes in this library —
    /// <c>tanh(x)</c> is built as a quotient of exponentials — so this is the rule that
    /// integrates them, and none of the four had an antiderivative before it.
    /// </para>
    /// <para>
    /// Checked by differentiating back and comparing at points, never against a printed form.
    /// These come out in the exponential rather than as <c>ln(cosh(x))</c> or
    /// <c>2 arctan(e^x)</c>, and comparing shapes would fail on answers that are correct.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class ExponentialSubstitutionIntegralTest
    {
        /// <summary>
        /// Real points either side of zero. Every integrand here is defined on the whole real
        /// line except the two with a zero at <c>x = 0</c>, which is not among these.
        /// </summary>
        private static readonly double[] Points = { -0.5, 0.23, 0.61, 1.05, 1.7 };

        private static void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());

            var derivative = integral.Substitute("C", 0).Differentiate("x");
            var original = integrand.ToEntity();
            var compared = 0;
            foreach (var at in Points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                if (got.IsNaN || want.IsNaN)
                    continue;
                compared++;
                var difference = Math.Abs((double)(got - want).RealPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
            Assert.True(compared >= 4,
                $"only {compared} of {Points.Length} points were comparable for {integrand}, "
                + "so this asserts almost nothing");
        }

        /// <summary>
        /// The four hyperbolic functions that had no antiderivative at all, and the two spellings
        /// of the secant, since one is a helper over the other and only one of them is what the
        /// parser produces.
        /// </summary>
        [Theory]
        [InlineData("tanh(x)")]
        [InlineData("coth(x)")]
        [InlineData("sech(x)")]
        [InlineData("csch(x)")]
        [InlineData("1/cosh(x)")]
        [InlineData("tanh(x)^2")]
        public void TheHyperbolicFunctions(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A root of hyperbolic functions of both signs of the exponent is under <c>u = e^x</c>
        /// a root of a quartic that nothing rationalises; under <c>u = tanh(x)</c> the
        /// hyperbolic functions are the shapes their trigonometric twins are under the
        /// tangent, <c>cosh(x)</c> being <c>(1 - u^2)^(-1/2)</c> and <c>sinh(x)</c> being
        /// <c>u (1 - u^2)^(-1/2)</c> for every real <c>x</c>. Timofeev's hyperbolic twin of his
        /// 560 was a minute of Euler's substitution on that quartic, and <c>sech^(3/4) tanh^5</c>
        /// and <c>cosh^(1/3) sinh^3</c> had no antiderivative at all. Checked on both signs of
        /// <c>x</c> where the root is real, and where the hyperbolic sine of <c>2x</c> is
        /// positive for the roots of it.
        /// </summary>
        [Theory]
        [InlineData("cosh(x)*(tanh(x) - cosh(2*x))/((sinh(x)^2 + sinh(2*x))*sqrt(sinh(2*x)))", new[] { 0.3, 0.7, 1.2, 1.9 })]
        [InlineData("sinh(x)^3/sqrt(sinh(2*x))", new[] { 0.3, 0.7, 1.2, 1.9 })]
        [InlineData("sech(x)^(3/4)*tanh(x)^5", new[] { -1.4, -0.5, 0.3, 0.7, 1.2 })]
        [InlineData("cosh(x)^(1/3)*sinh(x)^3", new[] { -1.4, -0.5, 0.3, 0.7, 1.2 })]
        [InlineData("sech(x)^(3/2)*tanh(x)", new[] { -1.4, -0.5, 0.3, 0.7, 1.2 })]
        public void ARootOfHyperbolicFunctionsUnderTheHyperbolicTangent(string integrand, double[] points)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("NaN", integral.Stringize());
            var derivative = integral.Substitute("C", 0).Differentiate("x");
            var original = integrand.ToEntity();
            foreach (var at in points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                var difference = System.Math.Abs((double)(got - want).RealPart) + System.Math.Abs((double)(got - want).ImaginaryPart);
                var scale = System.Math.Max(1.0, System.Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-8, $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }

        /// <summary>
        /// Timofeev's <c>arccot(cosh x) cosh x/sinh^4 x</c>, a step of parts against
        /// <c>cosh/sinh^4</c>. Three things stood in the way, each a search past the budget:
        /// the sum <c>e^x + e^-x</c> above the bar was expanded into two terms though it is the
        /// derivative of the factor below it; under <c>u = e^x</c> the rational function beside
        /// the arccotangent was expanded into terms before parts saw the whole, and each
        /// term's antiderivative keeps a logarithm the whole's does not; and the whole's,
        /// <c>8(u^4 + u^2)/(u^2 - 1)^4</c>, was written with that logarithm three times over
        /// with coefficients summing to zero. Checked where the hyperbolic sine is not zero.
        /// </summary>
        [Fact]
        public void AnInverseFunctionOfAHyperbolicOneTimesARationalFunctionOfThem()
        {
            var integrand = "acot(cosh(x))*cosh(x)/sinh(x)^4".ToEntity();
            var integral = integrand.Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            var derivative = integral.Substitute("C", 0).Differentiate("x");
            foreach (var at in new[] { -1.7, -0.6, 0.4, 1.1, 2.3 })
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = integrand.Substitute("x", at).EvalNumerical();
                var difference = System.Math.Abs((double)(got - want).RealPart) + System.Math.Abs((double)(got - want).ImaginaryPart);
                var scale = System.Math.Max(1.0, System.Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-8, $"d/dx of the antiderivative is {got} at x = {at}, where the integrand is {want}");
            }
            // And the antiderivative of `cosh/sinh^4` itself, `-1/(3 sinh^3)`, carries no
            // logarithm: `8(u^4 + u^2)/(u^2 - 1)^4` under `u = e^x`, whose reduction wrote one.
            Assert.DoesNotContain("ln(", "cosh(x)/sinh(x)^4".ToEntity().Integrate("x").Stringize());
        }

        /// <summary>
        /// Quotients written in the exponential directly. <c>e^x/(1 + e^x)</c> was already
        /// answered — its numerator is the derivative of its denominator, which is what the
        /// general substitution looks for — and is here so that the rule cannot break it.
        /// </summary>
        [Theory]
        [InlineData("1/(1 + e^x)")]
        [InlineData("1/(1 - e^x)")]
        [InlineData("1/(e^x - 1)")]
        [InlineData("1/(e^x + e^(-x))")]
        [InlineData("e^x/(1 + e^x)")]
        [InlineData("e^x/(1 + e^(2*x))")]
        public void AQuotientOfExponentials(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Slopes that differ, taken together by their greatest common divisor so that one
        /// substitution serves the whole integrand.
        /// </summary>
        [Theory]
        [InlineData("1/(1 + e^(2*x))")]
        [InlineData("e^(2*x)/(1 + e^x)")]
        [InlineData("e^(3*x)/(e^x + 1)")]
        [InlineData("1/(e^(2*x) - e^(4*x))")]
        public void SlopesTakenTogether(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A fractional slope: the base is <c>e^(k x)</c> with <c>k</c> the greatest common
        /// divisor of the slopes as rationals, so <c>e^(x/2)</c> beside <c>e^x</c> is read as
        /// <c>u</c> beside <c>u^2</c>. Timofeev's <c>e^(x/2)/sqrt(e^x - 1)</c> is
        /// <c>2/sqrt(u^2 - 1)</c> that way, and was declined for the half.
        /// </summary>
        [Theory]
        [InlineData("e^(x/2)/sqrt(e^x - 1)")]
        [InlineData("e^(x/3)/(1 + e^x)")]
        [InlineData("1/(e^(x/2) + e^(x/3))")]
        public void AFractionalSlope(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// The sign of the base is chosen for the radicals: with every exponential under a root
        /// of negative slope, <c>u = e^(-x)</c> makes <c>sqrt(1 + e^(-x))</c> a root of something
        /// linear in <c>u</c>, where <c>u = e^x</c> would make it a root of a quotient that
        /// nothing rationalises. Bondarenko's two.
        /// </summary>
        [Theory]
        [InlineData("sqrt(1 + e^(-x))/(e^x - e^(-x))")]
        [InlineData("sqrt(1 + e^(-x))/sinh(x)")]
        [InlineData("sqrt(1 + e^x)")]
        public void TheBaseTakesTheSignOfTheRadicand(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// <c>u = e^(k x)</c> is positive, and a root holding a power of it gives that power up:
        /// <c>sqrt(e^(2x) + e^(3x))</c> is <c>sqrt(u^2 (1 + u))</c> under <c>u = e^x</c>, and the
        /// simplifier is right not to call that <c>u sqrt(1 + u)</c> for a <c>u</c> it knows
        /// nothing about -- the substitution knows exactly this about its own. The identity
        /// <c>(a b)^r = a^r b^r</c> is exact whenever <c>a</c> is a non-negative real.
        /// </summary>
        [Theory]
        [InlineData("sqrt(e^(2*x) + e^(3*x))")]
        [InlineData("e^x/sqrt(e^(2*x) - e^x)")]
        [InlineData("e^(2*x)/sqrt(e^(4*x) + e^(2*x))")]
        [InlineData("sqrt(1 + tanh(4*x))")]
        public void APowerOfThePositiveBaseLeavesTheRoot(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A polynomial times a rational function of exponentials, by parts against the whole
        /// rational function: <c>x tanh(x)^2</c> is <c>x ((e^(2x) - 1)/(e^(2x) + 1))^2</c>, whose
        /// antiderivative under <c>u = e^(2x)</c> is <c>x - tanh(x)</c>, and what parts leaves is
        /// that, a degree lower. The general parts rule split the sum and each term's
        /// antiderivative kept a logarithm the sum's does not -- twenty-five seconds of
        /// dilogarithms. Where the antiderivative keeps a logarithm of an exponential's sum the
        /// integrand is declined at once: <c>x/(e^x + 1)</c> is not elementary.
        /// </summary>
        [Theory]
        [InlineData("x*tanh(x)^2")]
        [InlineData("x*coth(x)^2")]
        [InlineData("x*sech(x)^2")]
        [InlineData("x*e^(2*x)/(e^(2*x) + 1)^2")]
        public void APolynomialTimesARationalFunctionOfTheExponential(string integrand) => DifferentiatesBack(integrand);

        [Theory]
        [InlineData("x/(e^x + 1)")]
        [InlineData("x*e^(2*x)/(e^x + 1)")]
        [InlineData("x^2*e^x/(e^x + 1)^2")]
        [InlineData("x^2*sech(x)^2")]
        public void ADilogarithmIsDeclinedAtOnce(string integrand)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var integral = integrand.ToEntity().Integrate("x");
            Assert.Contains("integral(", integral.Stringize());
            Assert.True(watch.ElapsedMilliseconds < 20_000, $"{integrand} took {watch.ElapsedMilliseconds} ms to decline");
        }

        /// <summary>
        /// A power of an exponential with a positive base is the exponential of the product,
        /// exactly, and only that spelling is one the exponential rules read: Timofeev's
        /// <c>(cos(x/2) + sin(x/2))/(e^x)^(1/3)</c>, <c>cos(3x/2)/(3^(3x))^(1/4)</c> and
        /// <c>cos(x/3)^3/sqrt(e^x)</c>; and <c>cosh(x) + sinh(x)</c>, a sum of quotients of
        /// exponentials that is <c>e^x</c>, collapses to it with the exponentials as
        /// indeterminates, so that <c>e^(m x)/(cosh(x) + sinh(x))</c> is <c>e^((m - 1) x)/(m - 1)</c>.
        /// The symbolic <c>1/(a^2 + b^2 cosh(x)^2)</c> is a piecewise on the discriminant of
        /// the quadratic in <c>u^2</c>, asked as a question of its own in <c>u</c> and with the
        /// power of a product the simplifier writes distributed, <c>a^2 u^2</c> for <c>(a u)^2</c>.
        /// </summary>
        [Theory]
        [InlineData("(cos(x/2) + sin(x/2))/(e^x)^(1/3)")]
        [InlineData("cos(3*x/2)/(3^(3*x))^(1/4)")]
        [InlineData("cos(x/3)^3/sqrt(e^x)")]
        public void APowerOfAnExponentialIsFlattened(string integrand) => DifferentiatesBack(integrand);

        [Fact]
        public void TheHyperbolicSumThatIsAnExponential()
        {
            var integral = "e^(m*x)/(cosh(x) + sinh(x))".ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            var pinned = integral.Substitute("m", 3).Substitute("C", 0).Differentiate("x");
            var original = "e^(3*x)/(cosh(x) + sinh(x))".ToEntity();
            foreach (var at in Points)
                Assert.True(Math.Abs((double)(pinned.Substitute("x", at).EvalNumerical() - original.Substitute("x", at).EvalNumerical()).RealPart) < 1e-9, $"at {at}");
        }

        [Theory]
        [InlineData("1/(a^2 + b^2*cosh(x)^2)")]
        [InlineData("1/(a^2 - b^2*cosh(x)^2)")]
        public void ASymbolicQuadraticInTheHyperbolicCosine(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            var pinned = integral.Substitute("a", 3).Substitute("b", 2).Substitute("C", 0).Differentiate("x");
            var original = integrand.ToEntity().Substitute("a", 3).Substitute("b", 2);
            foreach (var at in Points)
            {
                var got = pinned.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                Assert.True(Math.Abs((double)(got - want).RealPart) < 1e-8 && Math.Abs((double)(got - want).ImaginaryPart) < 1e-8, $"{integrand} at {at}: {got} for {want}");
            }
        }

        /// <summary>
        /// Any one base free of x, not only e: Timofeev's <c>1/sqrt(a^(2x) - 1)</c> is
        /// <c>1/(u ln(a) sqrt(u^2 - 1))</c> under <c>u = a^x</c>, with <c>dx = du/(u ln a)</c>.
        /// And <c>e^(e^x) e^x</c>, which arrives as <c>e^(e^x + x)</c>: the general substitution
        /// writes an exponential of a sum as the product for its search, and <c>u = e^x</c>
        /// then leaves <c>e^u</c>.
        /// </summary>
        [Theory]
        [InlineData("1/(1 + 2^x)")]
        [InlineData("3^x*sqrt(1 + 3^x)")]
        [InlineData("e^(e^x + x)")]
        [InlineData("e^(e^x)*e^x")]
        public void AnyBaseAndAnExponentOfASum(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A whole power of a sum of exponentials is left to this substitution by the general
        /// one, whose search spent twenty-two seconds declining every sum in
        /// <c>tanh(x)^5/sech(x)^4</c>, a rational function of <c>e^x</c> answered here in a few.
        /// </summary>
        [Theory]
        [InlineData("tanh(x)^5/sech(x)^4")]
        [InlineData("tanh(x)^3*cosh(x)^2")]
        public void APowerOfASumOfExponentials(string integrand) => DifferentiatesBack(integrand);

        [Fact]
        public void ASymbolicBase()
        {
            var integrand = "1/sqrt(-1 + a^(2*x))".ToEntity();
            var integral = integrand.Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            var derivative = integral.Substitute("C", 0).Substitute("a", 3).Simplify().Differentiate("x");
            var original = integrand.Substitute("a", 3);
            foreach (var at in new[] { 0.3, 0.7, 1.2, 1.9 })
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                Assert.True(Math.Abs((double)(got - want).RealPart) < 1e-8 && Math.Abs((double)(got - want).ImaginaryPart) < 1e-8, $"{integrand} at {at}: {got} for {want}");
            }
        }

        /// <summary>
        /// What the rule must not disturb: exponentials the other rules already answer, and which
        /// are not rational in <c>e^(k x)</c> at all.
        /// </summary>
        [Theory]
        [InlineData("e^x")]
        [InlineData("x*e^x")]
        [InlineData("e^(2*x)")]
        public void WhatWasAlreadyAnswered(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Declined, so the boundary is recorded rather than assumed. <c>e^(x^2)</c> has no
        /// elementary antiderivative at all, and its exponent is not linear, which is the check
        /// that stops it — a rewrite would leave an <c>x</c> standing beside the new variable.
        /// Should it later be answered by something else, this moves rather than being deleted.
        /// </summary>
        [Theory]
        [InlineData("e^(x^2)")]
        [InlineData("e^(1/x)")]
        public void WhatIsNotRationalInOneExponentialIsDeclined(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("u_exp", integral.Stringize());
        }

        /// <summary>
        /// <see cref="DifferentiatesBack"/> with every symbol but <c>x</c> pinned to a distinct
        /// value off the integers, so that a symbolic base or slope is a number on both sides.
        /// </summary>
        private static void DifferentiatesBackWithParametersPinned(string integrand, double[] points)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("NaN", integral.Stringize());
            var derivative = integral.Substitute("C", 0).Differentiate("x");
            var original = integrand.ToEntity();
            var pinned = 0;
            foreach (var parameter in original.Vars)
            {
                if (parameter.Name == "x")
                    continue;
                var value = new[] { 1.3, 2.1, 0.7, 1.9, 3.1, 0.4 }[pinned++ % 6] + pinned / 6;
                derivative = derivative.Substitute(parameter, value);
                original = original.Substitute(parameter, value);
            }
            var compared = 0;
            foreach (var at in points)
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
            Assert.True(compared >= 3, $"only {compared} of {points.Length} points were comparable for {integrand}");
        }

        /// <summary>
        /// Numeric bases that are whole powers of one base are written in it: Rubi's
        /// <c>2^x/sqrt(a + b/4^x)</c> is <c>2^x/sqrt(a + b/2^(2x))</c>, rational in
        /// <c>u = 2^x</c> with a root of a linear, and was declined for its two bases.
        /// </summary>
        [Theory]
        [InlineData("2^x/sqrt(a + b/4^x)")]
        [InlineData("4^x/sqrt(a + b/2^x)")]
        [InlineData("8^x/(1 + 2^x)")]
        public void BasesThatArePowersOfOne(string integrand)
            => DifferentiatesBackWithParametersPinned(integrand, new[] { -0.5, 0.23, 0.61, 1.05, 1.7 });

        /// <summary>
        /// The slopes need only be rational multiples of one another: with a symbol for the
        /// slope, <c>F^(c + d x)</c> beside <c>F^(2 d x)</c>, the base is <c>F^(d x)</c> and
        /// <c>dx = du/(d u ln F)</c>. Rubi's <c>F^(c + d x) x/(a + b F^(c + d x))^2</c> is what
        /// by parts leaves of it. The logarithm of the base stays outside the quotient
        /// integrated: simplified into it, <c>1/((a + b u)^2 ln F)</c> was a quadratic with
        /// <c>ln F</c> in every coefficient, answered as a piecewise on whether <c>b^2 ln F</c>
        /// is zero, where <c>1/(a + b u)^2</c> is <c>-1/(b (a + b u))</c>.
        /// </summary>
        [Theory]
        [InlineData("F^(c + d*x)/(a + b*F^(c + d*x))^2")]
        [InlineData("F^(2*d*x)/(1 + F^(d*x))")]
        [InlineData("F^(c + d*x)*x/(a + b*F^(c + d*x))^2")]
        public void ASymbolicSlope(string integrand)
            => DifferentiatesBackWithParametersPinned(integrand, new[] { -0.5, 0.23, 0.61, 1.05, 1.7 });

        /// <summary>
        /// A quotient the gathering on the chain's entry writes as a product with a negative
        /// power -- <c>u^3/((a u^2 + b)^3 u)</c> as <c>u^2 (a u^2 + b)^(-3)</c> -- is written
        /// back below the bar where the rational rules read it: Rubi's <c>1/(b/f^x + a f^x)^3</c>.
        /// </summary>
        [Theory]
        [InlineData("1/(b/f^x + a*f^x)^3")]
        [InlineData("1/(2/3^x + 5*3^x)^3")]
        public void ANegativePowerOfTheQuadraticIsWrittenBelowTheBar(string integrand)
            => DifferentiatesBackWithParametersPinned(integrand, new[] { -0.5, 0.23, 0.61, 1.05, 1.7 });

        /// <summary>
        /// A rational function of the hyperbolic tangent with a symbol among its coefficients,
        /// by <c>u = tanh(y)</c> with the written factors kept: Rubi's
        /// <c>1/(a + b coth(c + d x)^2)^2</c> is <c>u^4/((b + a u^2)^2 (1 - u^2))</c> there,
        /// where under <c>u = e^x</c> it was a symbolic palindromic quartic squared and a
        /// timeout. The argument may be any linear form, and the secant's odd exponential
        /// cancels in an even integrand.
        /// </summary>
        [Theory]
        [InlineData("1/(a + b*coth(x)^2)^2")]
        [InlineData("1/(a + b*coth(2*x + 1)^2)^2")]
        [InlineData("1/(a + b*coth(d*x)^2)^2")]
        [InlineData("1/(a + b*coth(c + d*x)^2)^2")]
        [InlineData("sech(x)^4/(a + b*sech(x)^2)^2")]
        [InlineData("sech(c + d*x)^4/(a + b*sech(c + d*x)^2)^2")]
        public void ARationalFunctionOfTheHyperbolicTangent(string integrand)
            => DifferentiatesBackWithParametersPinned(integrand, new[] { -0.5, 0.23, 0.61, 1.05, 1.7 });

        /// <summary>
        /// A cubed symbolic quadratic beside a quadratic with real roots was integrated to a
        /// wrong answer -- the Hermite ansatz's logarithmic part, with coefficients that are
        /// quotients of forty-fifth-degree polynomials in the symbols, was integrated wrongly
        /// as a sum where each term is right alone -- and is declined or right now, since the
        /// answer is checked at sampled points where it was not.
        /// https://github.com/asc-community/AngouriMath/issues/1369
        /// </summary>
        [Theory]
        [InlineData("1/((a + b*x^2)^3*(x^2 - 1))")]
        [InlineData("1/((a + b*x^2)^3*(2 - x^2))")]
        public void ACubedSymbolicQuadraticBesideARealQuadraticIsNotAnsweredWrongly(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            if (integral.Stringize().Contains("integral("))
                return;
            DifferentiatesBackWithParametersPinned(integrand, new[] { 0.31, 0.57, 1.29, 1.77, 2.41 });
        }

        /// <summary>
        /// A root of <c>a + b sech(x)</c> beside an odd power of the tangent was integrated
        /// through <c>i</c>: the substitution took a root of a negative radicand on the way
        /// and the answer's derivative was off at every real point. Declined or right now,
        /// as the tangent substitution checks its answer at sampled points where a symbol is
        /// involved. https://github.com/asc-community/AngouriMath/issues/1370
        /// </summary>
        [Theory]
        [InlineData("sqrt(a + b*sech(x))*tanh(x)^5")]
        [InlineData("sqrt(a + b*sech(x))*tanh(x)^3")]
        public void ARootOfTheSecantBesideAnOddTangentIsNotAnsweredWrongly(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            if (integral.Stringize().Contains("integral("))
                return;
            DifferentiatesBackWithParametersPinned(integrand, new[] { -0.7, 0.23, 0.61, 1.05, 1.7 });
        }

        /// <summary>
        /// The power-of-a-quadratic reduction recursed on a remainder the division had not
        /// reduced, one level a call, until the stack ran out and the process with it -- a
        /// crash, which no caller can catch. The reduction declines a remainder that is not
        /// linear now, and the integral is answered or declined, either way in a process
        /// that is still there. https://github.com/asc-community/AngouriMath/issues/1373
        /// </summary>
        [Theory]
        [InlineData("csch(29/10 + 13/10*x)^3*(17/10 + 23/10*sech(29/10 + 13/10*x)^2)^3")]
        [InlineData("csch(x)^3*(a + b*sech(x)^2)^3")]
        public void ACubedSecantQuadraticBesideACubedCosecantDoesNotOverflowTheStack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            if (integral.Stringize().Contains("integral("))
                return;
            DifferentiatesBackWithParametersPinned(integrand, new[] { 0.31, 0.57, 1.29, 1.77 });
        }
    }
}
