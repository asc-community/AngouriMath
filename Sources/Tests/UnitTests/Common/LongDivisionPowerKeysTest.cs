//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Linq;
using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Common
{
    /// <summary>
    /// Polynomial long division keeps its monomials in a dictionary keyed by the power, and
    /// the power is an <c>EDecimal</c>, which is equal only to an <c>EDecimal</c> of the same
    /// scale -- so <c>2</c> and <c>2.0</c> are the same number and different keys. Every power
    /// the algorithm computes carries the scale of that arithmetic, so dividing <c>a^2</c> by
    /// <c>a^(1/2)</c> looked for <c>1.5 + 0.5 = 2.0</c> and missed the <c>2</c> already there.
    /// The term it should have cancelled was added as a second entry instead, the loop ran a
    /// second time over it, and the quotient came back **negated**:
    /// <c>2 * a / sqrt(a)</c> simplified to <c>-2 * sqrt(a)</c>.
    /// https://github.com/asc-community/AngouriMath/issues/751
    /// </summary>
    [Trait("Area", "Common")]
    public sealed class LongDivisionPowerKeysTest
    {
        /// <summary>
        /// Checked by value and not by printed form: the magnitude was right all along and
        /// only the sign was wrong, so a test that compared shapes could have passed.
        /// </summary>
        private static void AssertSimplifiesToSameValueAt(string expr, string variable, string point)
        {
            var original = expr.ToEntity();
            var simplified = original.Simplify();
            Entity At(Entity e)
            {
                var substituted = e.Substitute(variable, point.ToEntity());
                while (substituted is Entity.Providedf(var inner, _)) substituted = inner;
                return substituted;
            }
            Assert.Equal(
                At(original).EvalNumerical().RealPart.EDecimal.ToDouble(),
                At(simplified).EvalNumerical().RealPart.EDecimal.ToDouble(),
                9);
        }

        [Theory]
        [InlineData("a ^ 2 / sqrt(a)")]
        [InlineData("2 * a / sqrt(a)")]
        [InlineData("a * 2 / a ^ (1/2)")]
        [InlineData("a * 3 / a ^ (1/2)")]
        [InlineData("a * (-2) / a ^ (1/2)")]
        [InlineData("a / sqrt(a)")]
        [InlineData("a ^ 3 * 2 / a ^ (1/2)")]
        [InlineData("a ^ 2 / a ^ (1/3)")]
        [InlineData("a * 2 / a ^ (1/4)")]
        [InlineData("a ^ (5/2) / a ^ (1/2)")]
        public void AQuotientLeavingAFractionalPowerKeepsItsValue(string expr) =>
            AssertSimplifiesToSameValueAt(expr, "a", "17/10");

        /// <summary>
        /// The answers, stated outright, since "same value" would also be satisfied by leaving
        /// the quotient alone -- and before this the fractional cases were not left alone, they
        /// were answered wrongly.
        /// </summary>
        [Theory]
        [InlineData("a ^ 2 / sqrt(a)", "a ^ (3/2)")]
        [InlineData("2 * a / sqrt(a)", "2 * sqrt(a)")]
        [InlineData("a / sqrt(a)", "sqrt(a)")]
        [InlineData("a ^ 2 / a ^ (1/3)", "a ^ (5/3)")]
        public void TheQuotientIsAnsweredAndNotMerelyLeftAlone(string expr, string expected)
        {
            var simplified = expr.ToEntity().Simplify();
            while (simplified is Entity.Providedf(var inner, _)) simplified = inner;
            Assert.Equal(Entity.Number.Integer.Create(0),
                (simplified - expected.ToEntity()).Simplify());
        }

        // What must not change: an integer remainder was always right, because 2 - 1 and 1
        // land on the same scale and so on the same key.
        [Theory]
        [InlineData("x ^ 2 / x", "x")]
        [InlineData("(x ^ 2 - 1) / (x - 1)", "x + 1")]
        [InlineData("(x ^ 3 - 1) / (x - 1)", "x ^ 2 + x + 1")]
        [InlineData("(x ^ 4 - 1) / (x ^ 2 - 1)", "x ^ 2 + 1")]
        [InlineData("(x ^ 3 - 6 * x ^ 2 + 11 * x - 6) / (x - 1)", "x ^ 2 - 5 * x + 6")]
        public void AnIntegerRemainderIsUnaffected(string expr, string expected)
        {
            var simplified = expr.ToEntity().Simplify();
            while (simplified is Entity.Providedf(var inner, _)) simplified = inner;
            Assert.Equal(Entity.Number.Integer.Create(0),
                (simplified - expected.ToEntity()).Simplify());
        }

        /// <summary>
        /// The property the whole class fails, and the one that found it: a simplification has
        /// the value of the expression it came from. Swept over a spread of quotients rather
        /// than the reported one, since the reported one was the only member of its family
        /// anybody had written down.
        /// </summary>
        [Fact]
        public void NoQuotientOfPowersChangesValueUnderSimplification()
        {
            string[] numerators = { "a", "a ^ 2", "a ^ 3", "2 * a", "3 * a ^ 2", "a * 2" };
            string[] denominators = { "sqrt(a)", "a ^ (1/2)", "a ^ (1/3)", "a ^ (2/3)", "a", "a ^ 2" };
            foreach (var numerator in numerators)
                foreach (var denominator in denominators)
                    AssertSimplifiesToSameValueAt($"({numerator}) / ({denominator})", "a", "17/10");
        }
    }
}
