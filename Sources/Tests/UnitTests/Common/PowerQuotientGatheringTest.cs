//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Linq;
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
    /// <para/>
    /// <b>That gathering no longer happens in <c>Simplify</c>, and these tests moved with
    /// it.</b> <c>(a/b)^p</c> is not <c>a^p / b^p</c> across the branch cuts --
    /// <c>sqrt(2)/sqrt(-3)</c> is <c>-0.8165i</c> where <c>(2/-3)^(1/2)</c> is
    /// <c>+0.8165i</c> -- so the rule is now conditioned like the rest of its family
    /// (https://github.com/asc-community/AngouriMath/issues/802).
    /// <para/>
    /// Nothing is lost by that, because the gathering was a *means*: #740 wanted it so the
    /// limit machinery could read a <c>1^oo</c> out of a quotient. The limit reader now
    /// recognises the quotient itself, where the identity is checkable -- there is a
    /// destination to be near, and both bases can be required to stay positive on the way to
    /// it. So the assertions below moved from the mechanism to the outcome it existed for:
    /// they ask whether the limit is answered, not whether <c>Simplify</c> prints one power.
    /// </summary>
    [Trait("Area", "Common")]
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

        /// <summary>
        /// The quotients #740 was about, asked as the question it was really asking: does the
        /// limit come out? The shapes it used to assert -- a single power in Simplify's
        /// output -- are no longer produced, and should not be: see the note on the class.
        /// </summary>
        [Theory]
        [InlineData("(x ^ 2 + 1) ^ x / (x ^ 2) ^ x")]
        [InlineData("(x ^ 3 + 1) ^ x / (x ^ 3) ^ x")]
        [InlineData("(x ^ 2) ^ x / (x ^ 2 + 1) ^ x")]
        [InlineData("(sqrt(x) + 1) ^ x / sqrt(x) ^ x")]
        public void AQuotientOfPowersIsStillAnsweredAsALimit(string expr)
        {
            var limit = Task.Run(() => expr.ToEntity().Limit("x", "+oo").Simplify());
            Assert.True(limit.Wait(LimitTermination.Guard), $"{expr} did not terminate");
            Assert.False(limit.Result.Nodes.Any(node => node is Entity.Limitf),
                $"{expr} came back unevaluated: {limit.Result.Stringize()}");
        }

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
            Assert.True(limit.Wait(LimitTermination.Guard), $"{expr} did not terminate");
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

        /// <summary>
        /// These used to be asserted to gather into one power. They no longer do, because
        /// their bases are symbolic and nothing can say the quotient stays on the principal
        /// branch -- which is the whole of #802. What must still hold is that simplifying
        /// them does not change what they are, so that is what is asserted.
        /// </summary>
        [Theory]
        [InlineData("(y + 1) ^ x / y ^ x")]
        [InlineData("a ^ x / b ^ x")]
        [InlineData("(a ^ 2) ^ x / (b ^ 2) ^ x")]
        [InlineData("(x + 1) ^ (2 * x) / x ^ (2 * x)")]
        public void TheQuotientsThatNoLongerGatherKeepTheirValue(string expr) =>
            AssertSameValueAt(expr, ("a", "17/10"), ("b", "23/10"), ("x", "13/10"), ("y", "31/10"));

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
        /// This was the case deliberately left: a fractional c would have to divide the
        /// exponent rather than move into the base, so it would have become
        /// <c>((sqrt(x) + 1)^2 / x)^(x/2)</c> -- a squared numerator to buy a gathered form.
        /// <para/>
        /// It gathers now, and better, without that trade. Narrowing the
        /// <c>(a^b)^c = a^(b*c)</c> rule to where it is true
        /// (https://github.com/asc-community/AngouriMath/issues/752) means
        /// <c>sqrt(x)^x</c> no longer flattens to <c>x^(x/2)</c>, so both exponents stay
        /// <c>x</c> and the ordinary pairing rule reaches it. The gap this file is about was
        /// a rewrite running ahead of the pairing, and one less rewrite is one less way to
        /// run ahead of it.
        /// </summary>
        [Fact]
        public void AFractionalExponentRatioKeepsItsValue()
        {
            // The gathering this used to assert is gone with #802, and the limit it was
            // wanted for is covered by AQuotientOfPowersIsStillAnsweredAsALimit above. What
            // is checked here is that simplifying still does not change the value.
            AssertSameValueAt("(sqrt(x) + 1) ^ x / sqrt(x) ^ x", ("x", "13/10"));
        }
    }
}
