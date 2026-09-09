//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath.Extensions;
using AngouriMath.Functions;
using Xunit;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// Writing an expression as one quotient, which is what other systems call <c>together</c> or
    /// <c>ratsimp</c> and what <see cref="Entity.Simplify(int)"/> deliberately does not do.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1239">#1239</a>
    /// </summary>
    /// <remarks>
    /// Checked by value rather than by printed form. The point of this is the *shape* — a single
    /// division at the root — and the shape it produces is not canonical, so asserting a spelling
    /// would pin whatever the arithmetic happened to build rather than what was asked for. Each
    /// case therefore asserts two things: the result has no division below the root, and it agrees
    /// numerically with what it came from.
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class SingleQuotientTest
    {
        private static readonly double[] Points = { 0.37, 1.13, 2.71, -0.8 };

        /// <summary>
        /// True when a division appears anywhere below the root, which is exactly what this is
        /// supposed to remove.
        /// </summary>
        private static bool HasNestedDivision(Entity expr)
        {
            var root = expr is Entity.Divf(var numerator, var denominator) ? new[] { numerator, denominator } : new[] { expr };
            foreach (var part in root)
                foreach (var node in part.Nodes)
                    if (node is Entity.Divf)
                        return true;
            return false;
        }

        private static void CombinesTo(string source)
        {
            var original = source.ToEntity();
            var combined = SingleQuotient.Combine(original);

            Assert.False(HasNestedDivision(combined),
                $"{source} came back as {combined}, which still has a division inside it");

            var compared = 0;
            foreach (var at in Points)
            {
                var want = original.Substitute("t", at).EvalNumerical();
                var got = combined.Substitute("t", at).EvalNumerical();
                if (want.IsNaN || got.IsNaN)
                    continue;
                compared++;
                var difference = Math.Abs((double)(got - want).RealPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-12,
                    $"{source} is {want} at t = {at}, and {combined} is {got}");
            }
            Assert.True(compared >= 3,
                $"only {compared} of {Points.Length} points were comparable for {source}");
        }

        /// <summary>A sum with one fraction in it, which <c>Simplify</c> leaves split.</summary>
        [Theory]
        [InlineData("1 + 2/(1+t^2)")]
        [InlineData("3 + 10*t/(t^2+1)")]
        [InlineData("t - 1/t")]
        public void ASumWithAFraction(string source) => CombinesTo(source);

        /// <summary>A sum of two fractions, which needs both denominators.</summary>
        [Theory]
        [InlineData("1/(t^2+1) + 1/(t+1)")]
        [InlineData("1/t + 1/(t+1) + 1/(t+2)")]
        public void ASumOfFractions(string source) => CombinesTo(source);

        /// <summary>A fraction inside a fraction, which is the shape the half-angle rewrite makes.</summary>
        [Theory]
        [InlineData("1/(1 + 2/(1+t^2))")]
        [InlineData("2/((t^2+1) * (1 + (-2)*t/(t^2+1)))")]
        [InlineData("(1/t) / (1/(t+1))")]
        public void AFractionInsideAFraction(string source) => CombinesTo(source);

        /// <summary>A whole power of a quotient distributes; a negative one turns it over.</summary>
        [Theory]
        [InlineData("(1/t)^3")]
        [InlineData("(t/(t+1))^(-2)")]
        public void AWholePowerOfAQuotient(string source) => CombinesTo(source);

        /// <summary>
        /// Nothing to combine, so nothing is done — the denominator comes back as <c>1</c> and the
        /// expression is handed straight back rather than wrapped in a division by one.
        /// </summary>
        [Theory]
        [InlineData("t^2 + 2*t + 1")]
        [InlineData("sin(t) + 3")]
        public void NothingToCombine(string source)
        {
            var original = source.ToEntity();
            var (_, denominator) = SingleQuotient.Of(original);
            Assert.Equal(Entity.Number.Integer.One, denominator);
            Assert.Equal(original, SingleQuotient.Combine(original));
        }

        /// <summary>
        /// A fractional exponent is left whole, because <c>(a/b)^(1/2)</c> is not
        /// <c>sqrt(a)/sqrt(b)</c> across the branch cut, and this must not assert that it is.
        /// </summary>
        [Fact]
        public void AFractionalPowerIsLeftAlone()
        {
            var original = "(t/(t+1))^(1/2)".ToEntity();
            var (_, denominator) = SingleQuotient.Of(original);
            Assert.Equal(Entity.Number.Integer.One, denominator);
        }

        /// <summary>
        /// Nothing is cancelled, and that is deliberate: cancelling needs a gcd, which needs to
        /// know what the expression is a polynomial in, and this runs before anything has decided
        /// that. <c>t/t</c> comes back as a quotient rather than as <c>1</c>.
        /// </summary>
        [Fact]
        public void NothingIsCancelled()
        {
            var (numerator, denominator) = SingleQuotient.Of("(1/t) * t".ToEntity());
            Assert.Equal("t".ToEntity(), numerator);
            Assert.Equal("t".ToEntity(), denominator);
        }
    }
}
