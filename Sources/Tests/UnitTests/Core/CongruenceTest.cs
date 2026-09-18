//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Core.Exceptions;
using AngouriMath.Extensions;
using Xunit;
using static AngouriMath.Entity;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// <c>a = b (mod n)</c> is the statement that <c>n</c> divides <c>a - b</c>: a relation, with
    /// the modulus written once at the end of the line, and not the remainder operator. The rows
    /// are the worked examples and exercises of chapter 6 of Sullivan and Mackey's proofs book.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1409">#1409</a>
    /// </summary>
    [Trait("Area", "Discrete")]
    public sealed class CongruenceTest
    {
        [Theory]
        // Example 6.5.12: 5^6 is one more than a multiple of 7, and no smaller power is.
        [InlineData("5^6 = 1 (mod 7)", "True")]
        [InlineData("5^3 = 1 (mod 7)", "False")]
        // Remainders are floored: -1 = (-1)·10 + 9.
        [InlineData("-1 = 9 (mod 10)", "True")]
        [InlineData("-3 = 17 (mod 10)", "True")]
        [InlineData("-33 = 107 (mod 10)", "True")]
        [InlineData("12 = 448237402 (mod 10)", "True")]
        [InlineData("37457 = 38201 (mod 10)", "False")]
        [InlineData("0 = 9 (mod 3)", "True")]
        [InlineData("4 = 28 (mod 3)", "True")]
        [InlineData("474 = 0 (mod 3)", "True")]
        [InlineData("122 = 2 (mod 3)", "True")]
        // Problem 6.7.11's spoof: an exponent is not reduced modulo n.
        [InlineData("2^1 = 2^4 (mod 3)", "False")]
        // Modulo 0 is equality, and the sign of the modulus is immaterial.
        [InlineData("3 = 3 (mod 0)", "True")]
        [InlineData("3 = 4 (mod 0)", "False")]
        [InlineData("7 = 1 (mod -3)", "True")]
        // The unicode spelling reads the same.
        [InlineData("5^6 ≡ 1 (mod 7)", "True")]
        // A chain is the conjunction of its links.
        [InlineData("-100 = -1 = 8 = 311 = -289 = 41 (mod 3)", "True")]
        [InlineData("1 = 4 = 6 (mod 3)", "False")]
        public void DecidedForNumbers(string statement, string expected)
            => Assert.Equal(expected.ToEntity(), statement.ToEntity().Evaled);

        [Theory]
        [InlineData("1/2 = 1 (mod 3)")]
        [InlineData("2 = 1 (mod 3/2)")]
        [InlineData("i = 1 (mod 3)")]
        public void NaNOffTheIntegers(string statement)
            => Assert.Equal(MathS.NaN, statement.ToEntity().Evaled);

        [Theory]
        // §6.5.5 Try 2: (n - a)^2 - a^2 = n^2 - 2 a n, every term a multiple of n.
        [InlineData("(n - a)^2 = a^2 (mod n)")]
        // Problem 6.7.22, and the freshman's dream for a prime exponent.
        [InlineData("(x + y)^2 = x^2 + y^2 (mod 2)")]
        [InlineData("(x + y)^3 = x^3 + y^3 (mod 3)")]
        [InlineData("(x + y)^5 = x^5 + y^5 (mod 5)")]
        [InlineData("(x + y)^7 = x^7 + y^7 (mod 7)")]
        [InlineData("x = x (mod n)")]
        [InlineData("a + n = a (mod n)")]
        [InlineData("a * n = 0 (mod n)")]
        [InlineData("3 = 3 (mod n)")]
        public void DecidedForSymbolsWhereTheDifferenceIsAMultipleOfTheModulus(string statement)
            => Assert.Equal(Boolean.True, statement.ToEntity().Simplify());

        [Theory]
        // 6 x^2 y^2 is not a multiple of 4 for every x, y (x = y = 1: 16 against 2).
        [InlineData("(x + y)^4 = x^4 + y^4 (mod 4)")]
        [InlineData("(x + y)^6 = x^6 + y^6 (mod 6)")]
        [InlineData("5^k = 1 (mod 7)")]
        [InlineData("x = 1 (mod 3)")]
        [InlineData("3 = 1 (mod n)")]
        [InlineData("a = b (mod n)")]
        public void LeftAsWrittenWhereItDependsOnTheValues(string statement)
            => Assert.Equal(statement.ToEntity(), statement.ToEntity().Simplify());

        [Fact]
        public void TheFreshmansDreamHoldsAtAPrimeAndFailsAtFour()
        {
            var atFour = "(x + y)^4 = x^4 + y^4 (mod 4)".ToEntity();
            Assert.Equal(Boolean.False, atFour.Substitute("x", 1).Substitute("y", 1).Evaled);
            Assert.Equal(Boolean.True, atFour.Substitute("x", 2).Substitute("y", 2).Evaled);
        }

        [Theory]
        [InlineData("a = b (mod n)", "a = b (mod n)")]
        [InlineData("a ≡ b (mod n)", "a = b (mod n)")]
        [InlineData("x + 1 = 2 * y (mod 7)", "x + 1 = 2 * y (mod 7)")]
        [InlineData("a = b = c (mod n)", "a = b (mod n) and b = c (mod n)")]
        [InlineData("not a = b (mod n)", "not a = b (mod n)")]
        public void PrintsInTheAsciiSpellingAndReadsBack(string input, string printed)
        {
            var entity = input.ToEntity();
            Assert.Equal(printed, entity.ToString());
            Assert.Equal(entity, printed.ToEntity());
        }

        [Fact]
        public void LatexAndSymPy()
        {
            var congruence = "a = b (mod n)".ToEntity();
            Assert.Equal(@"a \equiv b \pmod{n}", congruence.Latexize());
            Assert.Contains("sympy.Eq(sympy.Mod(a - b, n), 0)", MathS.ToSympyCode(congruence));
        }

        [Fact]
        public void TheEntryPointsAgreeWithTheParser()
        {
            Assert.Equal("a = b (mod n)".ToEntity(), MathS.NumberTheory.Congruent("a", "b", "n"));
            Assert.Equal("a = b (mod n)".ToEntity(), MathS.Var("a").CongruentTo("b", "n"));
            Assert.IsType<Congruentf>("a = b (mod n)".ToEntity());
        }

        /// <summary>
        /// The relation and the operator are different things: `5 mod 3` is the number 2, and
        /// `(mod n)` belongs to a congruence only.
        /// </summary>
        [Fact]
        public void TheOperatorIsUntouched()
        {
            Assert.Equal(Number.Integer.Create(2), "5 mod 3".ToEntity().Evaled);
            Assert.Equal(Number.Integer.Create(2), "(5 mod 3)".ToEntity().Evaled);
            Assert.Equal(Boolean.True, "5 mod 3 = 2".ToEntity().Evaled);
        }

        [Theory]
        [InlineData("x < y (mod 3)")]
        [InlineData("x <= y (mod 3)")]
        [InlineData("x (mod 3)")]
        [InlineData("x ≡ y")]
        public void AModulusNeedsACongruenceAndACongruenceNeedsAModulus(string input)
            => Assert.ThrowsAny<ParseException>(() => input.ToEntity());
    }
}
