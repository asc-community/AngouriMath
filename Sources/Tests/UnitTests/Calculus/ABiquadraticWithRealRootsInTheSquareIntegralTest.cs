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
    /// Over a biquadratic whose discriminant is a positive number times even powers, both roots in
    /// <c>x^2</c> are real, and each term is written by the sign of its root: for a positive root
    /// <c>r</c> the arctangent <c>atan(x/sqrt(-r))</c> has an imaginary argument, on its cut wherever
    /// <c>x^2 > r</c>. Rubi's 7.4.2 <c>e^(3 acoth(a x)) sqrt(c - c/(a x))/x</c> comes to
    /// <c>u^4/((a - u^2)(2 a - u^2))</c> under <c>u = sqrt((a x + 1)/x)</c>, and its answer had no value
    /// between 0 and <c>1/a</c>, where the integrand is real.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ABiquadraticWithRealRootsInTheSquareIntegralTest
    {
        [Theory]
        // Rubi 7.4.2:592.
        [InlineData("e^(3*acoth(a*x))*sqrt(c-c/(a*x))/x", new[] { 0.4, 0.9, 1.6, 2.5, -1.8, -3.1 })]
        [InlineData("e^(3*acoth(a*x))*sqrt(1-1/(a*x))/x", new[] { 0.4, 0.9, 1.6, 2.5, -1.8, -3.1 })]
        // Between and beyond the roots of x^2 = a and x^2 = 2 a, 1.14 and 1.61 at a = 1.3.
        [InlineData("1/((a - x^2)*(2*a - x^2))", new[] { 0.4, 1.4, 2.5, -0.7, -2.2 })]
        [InlineData("x^4/((a - x^2)*(2*a - x^2))", new[] { 0.4, 1.4, 2.5, -0.7, -2.2 })]
        public void HasAValueWhereTheIntegrandIsReal(string integrand, double[] points)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Entity Pinned(Entity e) => e.Substitute("a", 0.7).Substitute("c", 1.07);
            Entity PinnedForTheQuartic(Entity e) => e.Substitute("a", 1.3);
            Func<Entity, Entity> pin = integrand.Contains("acoth") ? Pinned : PinnedForTheQuartic;
            var antiderivative = pin(integral.Substitute("C", 0));
            var derivative = antiderivative.Differentiate("x");
            var original = pin(integrand.ToEntity());
            foreach (var at in points)
            {
                var want = original.Substitute("x", at).EvalNumerical();
                Assert.True(Math.Abs((double)want.ImaginaryPart) < 1e-9, $"{integrand} is {want} at x = {at}, which is not a point where it is real");
                Assert.False(antiderivative.Substitute("x", at).EvalNumerical().IsNaN, $"the antiderivative of {integrand} has no value at x = {at}, where the integrand is {want}");
                var got = derivative.Substitute("x", at).EvalNumerical();
                Assert.False(got.IsNaN, $"d/dx of the antiderivative of {integrand} has no value at x = {at}, where the integrand is {want}");
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }

        [Theory]
        // Rubi 1.1.1.2:1343 and 1359: a biquadratic with the imaginary unit in it and a positive discriminant,
        // whose roots are not real, keeps its arctangents; split by their signs it had no arm that held.
        [InlineData("1/((a - i*a*x)^(3/4)*(a + i*a*x)^(1/4))")]
        [InlineData("(a - i*a*x)^(3/4)/(a + i*a*x)^(7/4)")]
        public void KeepsItsArctangentsOverComplexCoefficients(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            var derivative = integral.Substitute("C", 0).Substitute("a", 1.3).Differentiate("x");
            var original = integrand.ToEntity().Substitute("a", 1.3);
            foreach (var at in new[] { -2.2, -0.9, 0.4, 1.4 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                Assert.False(got.IsNaN, $"d/dx of the antiderivative of {integrand} has no value at x = {at}, where the integrand is {want}");
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }

        [Theory]
        [InlineData("1/((a - x^2)*(2*a - x^2))")]
        [InlineData("x^4/((a - x^2)*(2*a - x^2))")]
        public void IsRealPastBothRoots(string integrand)
        {
            // Past both roots each logarithm's argument is positive, and the antiderivative is real;
            // as arctangents of imaginary arguments it was on their cuts there.
            var antiderivative = integrand.ToEntity().Integrate("x").Substitute("C", 0).Substitute("a", 1.3);
            foreach (var at in new[] { 2.5, -2.2 })
            {
                var value = antiderivative.Substitute("x", at).EvalNumerical();
                Assert.False(value.IsNaN, $"the antiderivative of {integrand} has no value at x = {at}");
                Assert.True(Math.Abs((double)value.ImaginaryPart) < 1e-9, $"the antiderivative of {integrand} is {value} at x = {at}");
            }
        }
    }
}
