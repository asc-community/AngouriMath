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
using static AngouriMath.Entity.Number;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// The number theory of chapter 6 of Sullivan and Mackey's proofs book on top of the
    /// congruence node: a linear congruence solved to a residue class, two classes met by the
    /// Chinese remainder theorem (the non-coprime case included), a class cut down by an
    /// interval, the multiplicative inverse as a class representative, and the residue-based
    /// decisions of a quantified statement.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1409">#1409</a>
    /// </summary>
    [Trait("Area", "Discrete")]
    public sealed class ResidueClassTest
    {
        [Theory]
        // Example 6.5.26: 3 x = 11 (mod 7) through the inverse of 3, which is 5.
        [InlineData("3 x = 11 (mod 7)", "{ x in ZZ : x = 6 (mod 7) }")]
        [InlineData("x = 2 (mod 6)", "{ x in ZZ : x = 2 (mod 6) }")]
        [InlineData("x + 3 = 1 (mod 5)", "{ x in ZZ : x = 3 (mod 5) }")]
        // A common factor of a and n: the class is modulo n / gcd, or there is none.
        [InlineData("2 x = 4 (mod 6)", "{ x in ZZ : x = 2 (mod 3) }")]
        [InlineData("4 x = 8 (mod 12)", "{ x in ZZ : x = 2 (mod 3) }")]
        [InlineData("2 x = 3 (mod 6)", "{}")]
        // Not linear: the residues that satisfy it, or none (2 is not a square modulo 5).
        [InlineData("x^2 = 1 (mod 8)", "{ x in ZZ : x = 1 (mod 8) or x = 3 (mod 8) or x = 5 (mod 8) or x = 7 (mod 8) }")]
        [InlineData("x^2 = 2 (mod 5)", "{}")]
        // §6.5.4: the story of the coins, and Try 6.
        [InlineData("x = 1 (mod 2) and x = 1 (mod 3) and x = 2 (mod 5)", "{ x in ZZ : x = 7 (mod 30) }")]
        [InlineData("x = 1 (mod 2) and x = 1 (mod 3) and x = 2 (mod 5) and 250 <= x and x <= 300", "{ 277 }")]
        [InlineData("x = 3 (mod 5) and x = 4 (mod 7)", "{ x in ZZ : x = 18 (mod 35) }")]
        // pp. 451-452 and §6.5.5 Remind 7: moduli that share a factor, consistent or not.
        [InlineData("x = 3 (mod 4) and x = 2 (mod 6)", "{}")]
        [InlineData("x = 3 (mod 4) and x = 5 (mod 6)", "{ x in ZZ : x = 11 (mod 12) }")]
        [InlineData("x = 2 (mod 6) and x = 5 (mod 9)", "{ x in ZZ : x = 14 (mod 18) }")]
        [InlineData("x = 2 (mod 7) and 0 <= x and x < 30", "{ 2, 9, 16, 23 }")]
        public void ACongruenceIsSolvedToAResidueClass(string statement, string solutions)
            => Assert.Equal(solutions.ToEntity(), statement.ToEntity().Solve("x"));

        [Theory]
        [InlineData("{ x in ZZ : x = 3 (mod 7) } /\\ { x in ZZ : x = 4 (mod 5) }", "{ x in ZZ : x = 24 (mod 35) }")]
        [InlineData("{ x in ZZ : x = 3 (mod 7) } /\\ [0; 20]", "{ 3, 10, 17 }")]
        [InlineData("14 in { x in ZZ : x = 2 (mod 6) }", "True")]
        [InlineData("15 in { x in ZZ : x = 2 (mod 6) }", "False")]
        public void AResidueClassIsASet(string expression, string expected)
            => Assert.Equal(expected.ToEntity(), expression.ToEntity().Simplify());

        [Theory]
        // Example 6.5.23 and §6.5.5 Try 4: the representative in [1, n - 1], or none.
        [InlineData(2, 3, 2)]
        [InlineData(3, 4, 3)]
        [InlineData(2, 4, null)]
        [InlineData(3, 10, 7)]
        [InlineData(7, 15, 13)]
        [InlineData(6, 15, null)]
        [InlineData(5, 12, 5)]
        [InlineData(7, 11, 8)]
        [InlineData(6, 27, null)]
        [InlineData(11, 18, 5)]
        [InlineData(70, 84, null)]
        [InlineData(8, 17, 15)]
        public void TheModularInverseIsAClassRepresentative(int a, int n, int? inverse)
            => Assert.Equal(inverse is { } value ? Integer.Create(value) : null, MathS.NumberTheory.ModularInverse(a, n));

        [Fact]
        public void TheInversesModuloAPrimeAreAPermutation()
        {
            // p. 446: mod 7 the inverses of 1..6 are 1, 4, 5, 2, 3, 6, and only 1 and 6 are their own.
            var inverses = new[] { 1, 4, 5, 2, 3, 6 };
            for (var a = 1; a <= 6; a++)
                Assert.Equal(Integer.Create(inverses[a - 1]), MathS.NumberTheory.ModularInverse(a, 7));
        }

        [Theory]
        // Example 6.5.14 / Problem 5.7.15, by the six residues.
        [InlineData("forall n in ZZ : 6 divides n^3 + 5 n", "True")]
        [InlineData("forall n in ZZ+ : 6 divides n^3 + 5 n", "True")]
        [InlineData("forall n in ZZ : 5 divides n^3 + 5 n", "False")]
        // Problem 6.7.5 corrected: an integer prime to 6 has a square one more than a multiple of 24.
        [InlineData("forall p in ZZ : (not 2 divides p and not 3 divides p) implies p^2 = 1 (mod 24)", "True")]
        [InlineData("forall x in ZZ : x^2 = 0 (mod 4) or x^2 = 1 (mod 4)", "True")]
        [InlineData("exists x in ZZ : x^2 = 2 (mod 5)", "False")]
        [InlineData("exists x in ZZ : x^2 = 4 (mod 5)", "True")]
        // Lemma 6.5.10, the modular arithmetic lemma, at particular residues.
        [InlineData("forall a, b in ZZ : (a = 1 (mod 3) and b = 2 (mod 3)) implies a + b = 0 (mod 3)", "True")]
        [InlineData("forall n in ZZ : n^2 = n (mod 2)", "True")]
        [InlineData("forall x in ZZ : 4 divides x^2 implies 2 divides x", "True")]
        // Example 6.5.27: 3 x^2 - 5 y^2 = 1 has no solutions, since x^2 = 2 (mod 5) has none;
        // Problem 4.11.7: x^2 - y^2 = 14 has none, by parity.
        [InlineData("exists x, y in ZZ : 3 x^2 - 5 y^2 = 1", "False")]
        [InlineData("exists x, y in ZZ : x^2 - y^2 = 14", "False")]
        [InlineData("forall x, y in ZZ : not (x^2 - y^2 = 14)", "True")]
        [InlineData("exists x, y, z in ZZ : x^2 + y^2 + z^2 = 7", "False")]
        [InlineData("exists x, y in ZZ : x^2 + y^2 = 3", "False")]
        // And a witness where there is one.
        [InlineData("exists x, y in ZZ : x^2 - y^2 = 15", "True")]
        [InlineData("exists x, y in ZZ : x^2 + y^2 = 5", "True")]
        // Example 6.5.12: the order of 5 modulo 7 is 6.
        [InlineData("exists n in ZZ+ : 5^n = 1 (mod 7)", "True")]
        // A power of a base prime to the modulus repeats with the base's order, so the residues
        // of the exponent decide it: §5.2.4 Try 3, Example 5.3.7 (2^n + 1 modulo 7 is 2, 3 or 5
        // by n modulo 3), and the powers of 7 modulo 6 and 5.
        [InlineData("forall n in ZZ+ : 3 divides 7^n - 4^n", "True")]
        [InlineData("forall n in ZZ* : not (7 divides 2^n + 1)", "True")]
        [InlineData("forall n in ZZ* : 2^n + 1 = 3 (mod 7) or 2^n + 1 = 5 (mod 7) or 2^n + 1 = 2 (mod 7)", "True")]
        [InlineData("forall n in ZZ+ : 6 divides 7^n - 1", "True")]
        [InlineData("forall n in ZZ+ : 5 divides 7^n - 1", "False")]
        [InlineData("exists n in ZZ+ : 5 divides 7^n - 1", "True")]
        [InlineData("forall n in ZZ* : 8 divides 3^(2 n) - 1", "True")]
        [InlineData("forall n in ZZ+ : 9 divides 4^n + 15 n - 1", "True")]
        [InlineData("forall n in ZZ+ : 3 divides 2^n", "False")]
        public void AQuantifiedStatementIsDecidedByResidues(string statement, string expected)
            => Assert.Equal(expected.ToEntity(), statement.ToEntity().Simplify());

        [Theory]
        // Over ZZ a power has a fractional value at a negative exponent, and a base sharing a
        // factor with the modulus repeats only eventually: neither is decided by residues.
        [InlineData("forall n in ZZ : 3 divides 7^n - 4^n")]
        [InlineData("forall n in ZZ+ /\\ [2; +oo) : 4 divides 2^n")]
        public void APowerIsLeftAsWrittenWhereItsResiduesDoNotRepeat(string statement)
            => Assert.IsType<Forallf>(statement.ToEntity().Simplify());

        [Theory]
        // Examples 6.5.15-6.5.17: the quadratic residues modulo 3..8 and the cubic ones modulo 7, 9.
        [InlineData("({0, 1, 2})^2 mod 3", "{ 0, 1 }")]
        [InlineData("({0, 1, 2, 3})^2 mod 4", "{ 0, 1 }")]
        [InlineData("({0, 1, 2, 3, 4})^2 mod 5", "{ 0, 1, 4 }")]
        [InlineData("({0, 1, 2, 3, 4, 5})^2 mod 6", "{ 0, 1, 3, 4 }")]
        [InlineData("({0, 1, 2, 3, 4, 5, 6})^2 mod 7", "{ 0, 1, 2, 4 }")]
        [InlineData("({0, 1, 2, 3, 4, 5, 6, 7})^2 mod 8", "{ 0, 1, 4 }")]
        [InlineData("({0, 1, 2, 3, 4, 5, 6})^3 mod 7", "{ 0, 1, 6 }")]
        [InlineData("({0, 1, 2, 3, 4, 5, 6, 7, 8})^3 mod 9", "{ 0, 1, 8 }")]
        public void TheResidueTablesAreFiniteSetImages(string expression, string residues)
            => Assert.Equal(residues.ToEntity(), expression.ToEntity().Simplify());
    }
}
