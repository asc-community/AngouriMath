//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;
using static AngouriMath.Entity;

namespace AngouriMath.Tests.PatternsTest
{
    /// <summary>
    /// The parity identities: cos, sec and abs are even, and sin, tan, cotan, cosec and sgn
    /// are odd. Absent before, so an expression that cancels exactly did not — sin(-x) + sin(x)
    /// came back as itself. https://github.com/asc-community/AngouriMath/issues/929
    /// </summary>
    /// <remarks>
    /// What is asserted is the cancellation rather than the shape of a single function, and
    /// deliberately. sin(-x) and -sin(x) rate exactly equal under the complexity criteria — 14
    /// each — so which of them a lone sin(-x) simplifies to is a tie settled by generation
    /// order, and pinning it would pin the tie-break rather than the identity. The cancellation
    /// is what the identity is for and it does not depend on the tie.
    /// </remarks>
    [Trait("Area", "PatternsTest")]
    public sealed class ParityTest
    {
        private static void AssertSimplifiesToZero(string expression)
        {
            var simplified = expression.ToEntity().Simplify();
            // The odd reciprocal functions carry the condition they are defined under, which
            // is right: tan(-x) + tan(x) is undefined at the poles, not zero there.
            while (simplified is Providedf(var inner, _)) simplified = inner;
            Assert.Equal(Number.Integer.Create(0), simplified);
        }

        [Theory]
        [InlineData("cos(-x) - cos(x)")]
        [InlineData("sec(-x) - sec(x)")]
        [InlineData("abs(-x) - abs(x)")]
        [InlineData("cos(-2 * x) - cos(2 * x)")]
        [InlineData("abs(-2 * x) - abs(2 * x)")]
        public void AnEvenFunctionDropsTheNegation(string expression) =>
            AssertSimplifiesToZero(expression);

        [Theory]
        [InlineData("sin(-x) + sin(x)")]
        [InlineData("tan(-x) + tan(x)")]
        [InlineData("cotan(-x) + cotan(x)")]
        [InlineData("cosec(-x) + cosec(x)")]
        [InlineData("sgn(-x) + sgn(x)")]
        [InlineData("sin(-2 * x) + sin(2 * x)")]
        public void AnOddFunctionLiftsTheNegationOut(string expression) =>
            AssertSimplifiesToZero(expression);

        /// <summary>
        /// A bare negation and a negative numeric coefficient are the same shape —
        /// <c>Mulf</c> of a negative real — and both have to be reached. Before this, only a
        /// coefficient of magnitude two or more was, and only for cosine, because what
        /// happened to fold it was the multiple-angle expansion rather than parity.
        /// </summary>
        [Theory]
        [InlineData("cos(-x)", "cos(x)")]
        [InlineData("cos(-2 * x)", "cos(2 * x)")]
        [InlineData("abs(-x)", "abs(x)")]
        [InlineData("abs(-1/2 * x)", "abs(1/2 * x)")]
        public void AnEvenFunctionReachesTheSameFormEitherWay(string written, string expected) =>
            Assert.Equal(expected.ToEntity().Simplify(), written.ToEntity().Simplify());

        /// <summary>
        /// The identities are sound over the whole complex plane, so nothing here may acquire
        /// a condition that the expression it came from did not have. `abs` and the entire
        /// functions must come back unconditioned.
        /// </summary>
        [Theory]
        [InlineData("cos(-x)")]
        [InlineData("sin(-x)")]
        [InlineData("abs(-x)")]
        [InlineData("sgn(-x)")]
        public void NoConditionIsAcquired(string expression) =>
            Assert.False(expression.ToEntity().Simplify() is Providedf);

        /// <summary>
        /// A positive coefficient is left alone, so the rule cannot be cycling with whatever
        /// would put the sign back.
        /// </summary>
        [Theory]
        [InlineData("sin(2 * x)")]
        [InlineData("cos(2 * x)")]
        [InlineData("abs(2 * x)")]
        public void APositiveCoefficientIsUntouched(string expression) =>
            Assert.Equal(expression.ToEntity(), expression.ToEntity().InnerSimplified);
    }
}
