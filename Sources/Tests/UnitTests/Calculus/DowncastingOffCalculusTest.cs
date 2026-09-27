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
    /// Integration and limits compute as with the downcasting on, whatever the caller's setting,
    /// on the input's decimals read as the exact rationals they are. With it off, a whole number
    /// parsed as a decimal and no literal zero read as zero, so an integral came back as
    /// <c>NaN</c> or unevaluated, and so did a limit, where the default setting answers.
    /// https://github.com/asc-community/AngouriMath/issues/1490
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class DowncastingOffCalculusTest
    {
        private static readonly (string Name, string Value)[] Pins = { ("a", "1/3"), ("b", "2"), ("c", "1/2"), ("d", "3") };
        private static readonly double[] Points = { -0.7, -0.2, 0.3, 0.8 };

        /// <summary>Parsed with the downcasting off, bypassing the parse cache, which the setting does not key.</summary>
        private static Entity ParsedOff(string text)
        {
            using var _ = MathS.Settings.DowncastingEnabled.Set(false);
            return MathS.FromString(text, useCache: false);
        }

        private static Entity IntegratedOff(Entity integrand)
        {
            using var _ = MathS.Settings.DowncastingEnabled.Set(false);
            return integrand.Integrate("x");
        }

        /// <summary>Checked with the default setting, with the parameters pinned.</summary>
        private static void DifferentiatesBack(string integrand, Entity answer)
        {
            Assert.DoesNotContain("integral(", answer.Stringize());
            Assert.DoesNotContain("NaN", answer.Stringize());
            Entity Pinned(Entity e) => Pins.Aggregate(e, (current, pin) => current.Substitute(pin.Name, pin.Value.ToEntity()));
            var derivative = Pinned(answer.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in Points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                Assert.True(difference < 1e-9, $"d/dx of {answer} is {got} at x = {at}, where {integrand} is {want}");
            }
        }

        /// <summary>Parsed with the default setting and integrated with the downcasting off.</summary>
        [Theory]
        [InlineData("(a + b*arcsin(c*x))/sqrt(d - c^2*d*x^2)")]
        [InlineData("1/sqrt(2 - 2*x^2)")]
        [InlineData("1/(1 + c^2*x^2)")]
        public void IntegratedWithTheDowncastingOff(string integrand)
            => DifferentiatesBack(integrand, IntegratedOff(MathS.FromString(integrand, useCache: false)));

        /// <summary>Parsed and integrated with the downcasting off, where every number is a decimal.</summary>
        [Theory]
        [InlineData("1/sqrt(2 - 2*x^2)")]
        [InlineData("x^2*sin(x)")]
        [InlineData("(a + b*arcsin(c*x))/sqrt(d - c^2*d*x^2)")]
        public void ParsedAndIntegratedWithTheDowncastingOff(string integrand)
            => DifferentiatesBack(integrand, IntegratedOff(ParsedOff(integrand)));

        [Theory]
        [InlineData("sin(c*x)/x", "c")]
        [InlineData("(1 - cos(c*x))/x^2", "c^2/2")]
        public void ALimitWithTheDowncastingOff(string expression, string value)
        {
            var limit = LimitOff(ParsedOff(expression));
            Assert.Equal(value.ToEntity().Simplify(), limit.Simplify());
        }

        private static Entity LimitOff(Entity expression)
        {
            using var _ = MathS.Settings.DowncastingEnabled.Set(false);
            return expression.Limit("x", 0);
        }

        /// <summary>
        /// The cost: a decimal comes back as the rational it holds, <c>0.1 x</c> integrating to
        /// <c>x^2/20</c> rather than <c>0.05 x^2</c>. Read before any simplification, which with
        /// the default setting would make the two equal.
        /// </summary>
        [Fact]
        public void ADecimalComesBackAsTheRationalItIs()
        {
            var answer = IntegratedOff(ParsedOff("0.1 * x"));
            Assert.All(answer.Nodes.OfType<Entity.Number.Real>(), number => Assert.IsAssignableFrom<Entity.Number.Rational>(number));
            Assert.Equal("x ^ 2 / 20".ToEntity().Simplify(), answer.Substitute("C", 0).Simplify());
        }
    }
}
