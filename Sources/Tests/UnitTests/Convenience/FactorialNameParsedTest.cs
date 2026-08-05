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
    /// factorial is the name the function has in sympy, MATLAB, Mathematica and Python's
    /// math module, and the grammar did not have it -- while having the function already,
    /// as the postfix x! and as MathS.Factorial. So nothing was missing but the spelling,
    /// and a one-argument call under a name the grammar does not know never errors: it
    /// falls through to the implicit multiplication that lets a(b + c) mean a * (b + c),
    /// so factorial(5) came out as the product factorial * 5, silently, where it is 120.
    /// https://github.com/asc-community/AngouriMath/issues/733
    /// </summary>
    public sealed class FactorialNameParsedTest
    {
        [Theory]
        [InlineData("factorial(0)", 1)]
        [InlineData("factorial(1)", 1)]
        [InlineData("factorial(5)", 120)]
        [InlineData("factorial(6)", 720)]
        public void TheNameIsTheFactorial(string written, int expected) =>
            Assert.Equal(expected, written.ToEntity().EvalNumerical());

        // The same function as the postfix spelling that already worked, so the two agree
        // rather than merely each being defined.
        [Theory]
        [InlineData("factorial(x)", "x!")]
        [InlineData("factorial(x + 1)", "(x + 1)!")]
        [InlineData("factorial(2 * x)", "(2 * x)!")]
        public void ItIsTheSameFunctionAsThePostfix(string written, string same) =>
            Assert.Equal(same.ToEntity(), written.ToEntity());

        // gamma is the same fact one shift along, and the grammar already had it.
        [Theory]
        [InlineData("factorial(5)", "gamma(6)")]
        [InlineData("factorial(0)", "gamma(1)")]
        public void ItAgreesWithGammaOneShiftAlong(string written, string shifted) =>
            Assert.Equal(shifted.ToEntity().EvalNumerical(), written.ToEntity().EvalNumerical());

        /// <summary>
        /// Only the exact name followed by a bracket is the function. On its own the name
        /// is still an ordinary variable, followed by a digit it is still the implicit
        /// power that <c>x2</c> means, and a different name beginning the same way is
        /// still the implicit product that <c>a(b + c)</c> means.
        /// </summary>
        [Theory]
        [InlineData("factorial", "factorial")]
        [InlineData("factorial2", "factorial ^ 2")]
        [InlineData("factoriall(x)", "factoriall * x")]
        [InlineData("factorial3(x)", "factorial ^ 3 * x")]
        public void EverythingElseParsesAsItDid(string written, string expected) =>
            Assert.Equal(expected, written.ToEntity().Stringize());

        // The postfix spelling does not move.
        [Theory]
        [InlineData("5!", 120)]
        [InlineData("0!", 1)]
        [InlineData("3!", 6)]
        public void TheExistingSpellingIsUnaffected(string written, int expected) =>
            Assert.Equal(expected, written.ToEntity().EvalNumerical());
    }
}
