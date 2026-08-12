//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// Gruntz's algorithm puts the subexpressions of the fastest comparability class in a set
    /// and then rewrites the expression by substituting each of them by name, so a member of
    /// that set has to occur in the expression as it stands. A power whose exponent moves was
    /// read as exp(p * ln(b)) when the set was built but left as written in the expression, so
    /// the set came back holding one exponential twice -- once as the constructed
    /// e^(x * ln(x)) and once as the denominator's own e^(ln(x) * x), the same product with
    /// its factors the other way round -- and the substitution found only the second of them.
    /// The numerator then went into the series as x^x, whose leading exponent reads as +1,
    /// which the algorithm concludes from as "tends to zero".
    ///
    /// No issue exists for this; it was found while measuring what a factorial's Stirling
    /// expansion would need, where x^x is the term the factorial is compared against.
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class GruntzMovingExponentTest
    {
        private static void AssertLimit(string expression, string expected) =>
            Assert.Equal(
                expected.ToEntity().Evaled,
                expression.ToEntity().Limit("x", "+oo".ToEntity()).Evaled);

        /// <summary>
        /// x^x and e^(x * ln(x)) are one function, so each of these is an expression whose
        /// value is known exactly rather than only asymptotically -- which is what makes the
        /// expected answers here beyond argument. Every one of them answered 0 before.
        /// </summary>
        [Theory]
        [InlineData("x ^ x / e ^ (x * ln(x))", "1")]
        [InlineData("e ^ (x * ln(x)) / x ^ x", "1")]
        [InlineData("x ^ (2 * x) / e ^ (2 * x * ln(x))", "1")]
        public void APowerAndItsExponentialAreOneFunction(string expression, string expected) =>
            AssertLimit(expression, expected);

        /// <summary>
        /// The same cancellation with something left over, so that the answer is not 1 and a
        /// rule which merely stopped saying 0 would not pass. The quotients are e^x, x and
        /// e^(-x) exactly.
        /// </summary>
        [Theory]
        [InlineData("x ^ x / e ^ (x * ln(x) - x)", "+oo")]
        [InlineData("x ^ x / e ^ (x * ln(x) + x)", "0")]
        public void WhatIsLeftOverDecidesIt(string expression, string expected) =>
            AssertLimit(expression, expected);

        /// <summary>
        /// Two of the cases above are no longer answered, and they are lost honestly: each comes
        /// back as an unevaluated <c>limit</c> node rather than as a value, so the caller is told
        /// that nothing was settled instead of being told something false.
        /// </summary>
        /// <remarks>
        /// Both need <c>ln(a^c) = c * ln(a)</c> — <c>d/dx (x^2)^x</c> carries <c>ln(x^2)</c>, and
        /// l'Hopital's rule reached it through <c>Simplify</c>. That identity is false off
        /// <c>ln</c>'s principal strip, so the simplifier no longer applies it
        /// (https://github.com/asc-community/AngouriMath/issues/902), and as x -> +oo the base
        /// really is positive, so what is missing here is a way to say so.
        /// <para/>
        /// Supplying it from the limit side does not reach: rewriting the expression before
        /// <c>Simplify</c> is called does pull the exponent out, and <c>Simplify</c>'s own
        /// candidate search then writes <c>(x^2)^x</c> back into a logarithm and needs the
        /// identity again. It is load-bearing *inside* the search, so restoring these two wants
        /// an assumption travelling with the expression rather than another pre-pass.
        /// <para/>
        /// An unevaluated node is asserted rather than <c>NaN</c> deliberately: <c>NaN</c> would
        /// claim the limit does not exist, and it does. If a value comes back here, the
        /// assumption mechanism has arrived and these two rows belong back in the theories above.
        /// </remarks>
        [Theory]
        [InlineData("(x ^ 2) ^ x / e ^ (2 * x * ln(x))")]
        [InlineData("x ^ x / e ^ (x * ln(x) - ln(x))")]
        public void AnExponentUnderALogarithmIsNotReadForNow(string expression)
        {
            var limit = expression.ToEntity().Limit("x", "+oo".ToEntity());
            Assert.True(limit is Entity.Limitf,
                $"{expression} came back as {limit.Stringize()}, which is a value rather than an "
                + "unevaluated limit -- see this test's remarks before changing it");
        }

        /// <summary>
        /// The claim the expected values above rest on, checked at a point rather than argued:
        /// the ratio is not merely close to 1, it is 1.
        /// </summary>
        [Theory]
        [InlineData("(50 ^ 50) / e ^ (50 * ln(50))")]
        [InlineData("(20 ^ 20) / e ^ (20 * ln(20))")]
        public void ThePowerAndTheExponentialAgreeAtAPoint(string written) =>
            Assert.Equal(Entity.Number.Integer.Create(1), written.ToEntity().EvalNumerical());

        // Growths that were already right and have to stay so, including the ones where a
        // moving exponent competes with a fixed one.
        [Theory]
        [InlineData("x ^ x / e ^ x", "+oo")]
        [InlineData("x ^ x / 2 ^ x", "+oo")]
        [InlineData("2 ^ x / x ^ 2", "+oo")]
        [InlineData("x ^ 2 / e ^ x", "0")]
        [InlineData("x ^ x / x ^ (2 * x)", "0")]
        [InlineData("x ^ (2 * x) / x ^ x", "+oo")]
        public void TheOrdinaryOnesAreUnchanged(string expression, string expected) =>
            AssertLimit(expression, expected);
    }
}
