//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Convenience
{
    /// <summary>
    /// log10 and log2 are standard in C, Python, numpy and MATLAB, and the grammar did not
    /// have them -- while having both functions already, as log(x) and log(2, x). So
    /// nothing was missing but the spelling, and what happened instead was worse to read
    /// than an ordinary implicit product: log10 lexes as the variable log followed by 10,
    /// and x2 means x^2 by design, so log10(100) came out as log^10 * 100.
    /// https://github.com/asc-community/AngouriMath/issues/733
    /// </summary>
    [Trait("Area", "Convenience")]
    public sealed class LogBaseNamesParsedTest
    {
        [Theory]
        [InlineData("log10(100)", 2)]
        [InlineData("log10(1000)", 3)]
        [InlineData("log2(8)", 3)]
        [InlineData("log2(1024)", 10)]
        public void ANamedBaseIsTheLogarithmToThatBase(string written, int expected) =>
            Assert.Equal(expected, written.ToEntity().EvalNumerical());

        // The same function under the spelling that already worked, so the two agree
        // rather than merely each being defined.
        [Theory]
        [InlineData("log10(x)", "log(x)")]
        [InlineData("log10(x + 1)", "log(10, x + 1)")]
        [InlineData("log2(x)", "log(2, x)")]
        public void ItIsTheSameFunctionAsTheTwoArgumentForm(string written, string same) =>
            Assert.Equal(same.ToEntity(), written.ToEntity());

        /// <summary>
        /// Only the exact name followed by a bracket is the function. Without the bracket
        /// these are still the implicit power that <c>x2</c> means, and a different name
        /// beginning the same way is still the implicit product that <c>a(b + c)</c> means.
        /// </summary>
        [Theory]
        [InlineData("log2x", "log ^ 2 * x")]
        [InlineData("log10", "log ^ 10")]
        [InlineData("logx(y)", "logx * y")]
        [InlineData("log3(x)", "log ^ 3 * x")]
        public void EverythingElseParsesAsItDid(string written, string expected) =>
            Assert.Equal(expected, written.ToEntity().Stringize());

        // The one-argument log is base 10 and the two-argument one names its base; neither
        // moves.
        [Theory]
        [InlineData("log(100)", 2)]
        [InlineData("log(2, 8)", 3)]
        [InlineData("log(3, 81)", 4)]
        public void TheExistingSpellingsAreUnaffected(string written, int expected) =>
            Assert.Equal(expected, written.ToEntity().EvalNumerical());
    }
}
