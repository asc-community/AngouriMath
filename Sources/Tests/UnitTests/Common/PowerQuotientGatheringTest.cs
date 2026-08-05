//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Threading.Tasks;
using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Common
{
    /// <summary>
    /// <c>a^p / b^p</c> is gathered into <c>(a/b)^p</c>, but stopped being gathered as soon
    /// as one of the bases was itself a power: <c>(a^2)^x / (b^2)^x</c> gathers and
    /// <c>(a^2)^x / b^x</c> did not. The cause is that <c>(b^c)^p</c> is rewritten to
    /// <c>b^(c*p)</c> on the child, on the way up, so by the time the pair is looked at it
    /// has already happened -- and where it applies to only one of the two, the exponents
    /// no longer match. https://github.com/asc-community/AngouriMath/issues/740
    /// </summary>
    public sealed class PowerQuotientGatheringTest
    {
        private static Entity Simplified(string expr) => expr.ToEntity().Simplify();

        /// <summary>
        /// The rewrite has to preserve the value, not just the shape. Checked at a point
        /// where every base is positive, which is where <c>a^p / b^(c*p) = (a/b^c)^p</c>
        /// holds without a branch argument.
        /// </summary>
        private static void AssertSameValueAt(string expr, params (string Variable, string Value)[] at)
        {
            var original = expr.ToEntity();
            var simplified = original.Simplify();
            Entity Substituted(Entity e)
            {
                foreach (var (variable, value) in at)
                    e = e.Substitute(variable, value.ToEntity());
                return e;
            }
            Assert.Equal(
                Substituted(original).EvalNumerical().RealPart.EDecimal.ToDouble(),
                Substituted(simplified).EvalNumerical().RealPart.EDecimal.ToDouble(),
                9);
        }

        // A single power is what the gathering is for, and what the limit machinery reads a
        // 1^oo off. Asserted as "one power" rather than as a string, since which of the
        // equivalent single powers the complexity metric picks is not the point.
        private static void AssertGathersIntoOnePower(string expr)
        {
            var simplified = Simplified(expr);
            Assert.True(simplified is Entity.Powf,
                $"{expr} came back as {simplified.Stringize()}, which is not a single power");
        }

        [Theory]
        [InlineData("(a ^ 2 + 1) ^ x / (a ^ 2) ^ x")]
        [InlineData("(x ^ 2 + 1) ^ x / (x ^ 2) ^ x")]
        [InlineData("(x ^ 3 + 1) ^ x / (x ^ 3) ^ x")]
        [InlineData("(a ^ 2) ^ x / b ^ x")]
        [InlineData("(x ^ 2) ^ x / (x ^ 2 + 1) ^ x")]
        [InlineData("x ^ (2 * a) / y ^ a")]
        public void AQuotientOfPowersGathersWhenOneBaseIsItselfAPower(string expr) =>
            AssertGathersIntoOnePower(expr);

        [Theory]
        [InlineData("(a ^ 2 + 1) ^ x / (a ^ 2) ^ x")]
        [InlineData("(a ^ 2) ^ x / b ^ x")]
        [InlineData("x ^ (2 * a) / y ^ a")]
        [InlineData("(x ^ 3 + 1) ^ x / (x ^ 3) ^ x")]
        public void GatheringDoesNotChangeTheValue(string expr) =>
            AssertSameValueAt(expr, ("a", "17/10"), ("b", "23/10"), ("x", "13/10"), ("y", "31/10"));

        /// <summary>
        /// Why it is worth gathering. The limit machinery reads a 1^oo off a single power
        /// and cannot see one in a quotient, so the same function was answered or not
        /// according only to how it had been written -- and spent five and a half seconds
        /// not answering.
        /// </summary>
        [Theory]
        [InlineData("(x ^ 2 + 1) ^ x / (x ^ 2) ^ x")]
        [InlineData("(x ^ 3 + 1) ^ x / (x ^ 3) ^ x")]
        [InlineData("((x ^ 2 + 1) / x ^ 2) ^ x")]
        public void TheQuotientFormIsAnsweredAsTheSinglePowerFormIs(string expr)
        {
            var limit = Task.Run(() => expr.ToEntity().Limit("x", "+oo").Simplify());
            Assert.True(limit.Wait(System.TimeSpan.FromSeconds(30)), $"{expr} did not terminate");
            Assert.Equal(Entity.Number.Integer.Create(1), limit.Result);
        }

        // What must not change. A numeric exponent is not written as a product, so nothing
        // here matches it, and x^4 / y^2 keeps the form it had.
        [Theory]
        [InlineData("x ^ 4 / y ^ 2")]
        [InlineData("x ^ 4 / y ^ 3")]
        [InlineData("a ^ 2 / b")]
        public void AQuotientWithNumericExponentsIsUnaffected(string expr) =>
            Assert.False(Simplified(expr) is Entity.Powf,
                $"{expr} was gathered into {Simplified(expr).Stringize()}");

        // What already worked, and still does.
        [Theory]
        [InlineData("(y + 1) ^ x / y ^ x")]
        [InlineData("a ^ x / b ^ x")]
        [InlineData("(a ^ 2) ^ x / (b ^ 2) ^ x")]
        [InlineData("(x + 1) ^ (2 * x) / x ^ (2 * x)")]
        public void TheQuotientsThatAlreadyGatheredStillDo(string expr) =>
            AssertGathersIntoOnePower(expr);

        /// <summary>
        /// The limits #739 fixed go through the same gathering, so they are pinned here as
        /// well: a change to which quotients gather is a change to which of these are
        /// answered.
        /// </summary>
        [Theory]
        [InlineData("(x - 5) ^ x / x ^ x", "+oo", "1 / e ^ 5")]
        [InlineData("(x + 1) ^ x / x ^ x", "+oo", "e")]
        [InlineData("(x - 5) ^ x / x ^ x", "-oo", "1 / e ^ 5")]
        public void TheSecondRemarkableLimitStillReadsWhatGatheringGivesIt(
            string expr, string approach, string expected)
        {
            var limit = expr.ToEntity().Limit("x", approach.ToEntity()).Simplify();
            Assert.Equal(Entity.Number.Integer.Create(0),
                (limit - expected.ToEntity()).Simplify());
        }

        /// <summary>
        /// What is deliberately left. A fractional c would have to divide the exponent
        /// rather than move into the base, so <c>(sqrt(x) + 1)^x / sqrt(x)^x</c> would
        /// become <c>((sqrt(x) + 1)^2 / x)^(x/2)</c> -- a squared numerator to buy a
        /// gathered form. That is a judgement about output rather than the gap this fixes,
        /// so it is pinned as it stands rather than forced.
        /// </summary>
        [Fact]
        public void AFractionalExponentRatioIsStillNotGathered() =>
            Assert.False(Simplified("(sqrt(x) + 1) ^ x / sqrt(x) ^ x") is Entity.Powf);
    }
}
