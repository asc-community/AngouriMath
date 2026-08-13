//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;
using System.Linq;
using AngouriMath.Functions;
using PeterO.Numbers;
using Xunit;

namespace AngouriMath.Tests.Algebra.Polynomials
{
    /// <summary>
    /// Arithmetic over <c>F_p</c> and Berlekamp factorisation on it.
    /// </summary>
    /// <remarks>
    /// The test that carries the weight is
    /// <see cref="EveryMonicPolynomialOfSmallDegreeFactorsCorrectly"/>: every monic
    /// polynomial of degree 2 to 4 over <c>F_2</c>, <c>F_3</c> and <c>F_5</c>, checked
    /// against a brute-force trial division that shares no code with the algorithm under
    /// test. The hand-computed cases above it say what a given answer should be; only the
    /// exhaustive one says that nothing in between is wrong. Its oracle is itself checked,
    /// in <see cref="TrialDivisionAgreesWithGausssCount"/>, against Gauss's count of the
    /// monic irreducibles — otherwise an oracle that called everything irreducible would
    /// pass silently.
    /// </remarks>
    [Trait("Area", "Algebra")]
    public sealed class PrimeFieldFactorizationTest
    {
        private static PrimeFieldPolynomial Poly(long prime, params long[] coefficientsLowestFirst)
            => PrimeFieldPolynomial.Create(coefficientsLowestFirst, prime);

        private static void AssertSame(PrimeFieldPolynomial expected, PrimeFieldPolynomial actual)
            => Assert.True(expected.SameAs(actual), $"expected {expected}, got {actual}");

        /// <summary>The factorisation, having asserted that there is one.</summary>
        private static IReadOnlyList<PrimeFieldPolynomial> Factored(PrimeFieldPolynomial poly)
        {
            var factors = PrimeFieldFactorization.Factor(poly);
            Assert.True(factors is not null, $"declined to factor {poly}");
            return factors!;
        }

        /// <summary>Every monic polynomial of the given degree, in a fixed order.</summary>
        private static IEnumerable<PrimeFieldPolynomial> MonicPolynomials(long prime, int degree)
        {
            var count = 1L;
            for (var i = 0; i < degree; i++)
                count *= prime;
            for (var index = 0L; index < count; index++)
            {
                var coefficients = new long[degree + 1];
                coefficients[degree] = 1;
                var rest = index;
                for (var i = 0; i < degree; i++)
                {
                    coefficients[i] = rest % prime;
                    rest /= prime;
                }
                yield return PrimeFieldPolynomial.Create(coefficients, prime);
            }
        }

        /// <summary>
        /// Irreducibility decided by dividing by every monic polynomial that could be a
        /// proper factor. Deliberately shares nothing with Berlekamp beyond the division.
        /// </summary>
        private static bool IsIrreducibleByTrialDivision(PrimeFieldPolynomial poly)
        {
            if (poly.Degree < 1)
                return false;
            for (var degree = 1; 2 * degree <= poly.Degree; degree++)
                foreach (var candidate in MonicPolynomials(poly.Prime, degree))
                    if (poly.Remainder(candidate).IsZero)
                        return false;
            return true;
        }

        // --- construction and the shape of the representation ---

        [Fact]
        public void CreateReducesIntoTheFieldAndDropsTrailingZeros()
        {
            // -1 is 4 and -7 is 3 modulo 5; 10 and 15 are 0 and so are not stored at all.
            var poly = Poly(5, -1, -7, 10, 15);
            Assert.Equal(new long[] { 4, 3 }, poly.Coefficients);
            Assert.Equal(1, poly.Degree);
            Assert.False(poly.IsZero);
            Assert.False(poly.IsMonic);
        }

        [Fact]
        public void ZeroHasDegreeMinusOneAndIsNotMonic()
        {
            var zero = PrimeFieldPolynomial.Zero(7);
            Assert.True(zero.IsZero);
            Assert.Equal(-1, zero.Degree);
            Assert.False(zero.IsMonic);
            Assert.Empty(zero.Coefficients);
            AssertSame(zero, Poly(7, 0, 0, 0));
        }

        [Fact]
        public void OneAndMonomialAreWhatTheySay()
        {
            Assert.Equal(new long[] { 1 }, PrimeFieldPolynomial.One(7).Coefficients);
            Assert.True(PrimeFieldPolynomial.One(7).IsMonic);
            Assert.Equal(new long[] { 0, 0, 0, 1 }, PrimeFieldPolynomial.Monomial(3, 7).Coefficients);
            Assert.Equal(3, PrimeFieldPolynomial.Monomial(3, 7).Degree);
        }

        [Theory]
        [InlineData(1L)]
        [InlineData(0L)]
        [InlineData(-5L)]
        [InlineData(PrimeFieldPolynomial.MaxPrime + 1)]
        public void AModulusOutOfRangeIsRefused(long prime)
            => Assert.Throws<ArgumentOutOfRangeException>(() => PrimeFieldPolynomial.Zero(prime));

        [Fact]
        public void ANegativeMonomialDegreeIsRefused()
            => Assert.Throws<ArgumentOutOfRangeException>(() => PrimeFieldPolynomial.Monomial(-1, 5));

        [Fact]
        public void MixingTwoFieldsIsRefused()
        {
            var left = Poly(5, 1, 1);
            var right = Poly(7, 1, 1);
            Assert.Throws<ArgumentException>(() => left.Add(right));
            Assert.Throws<ArgumentException>(() => left.Multiply(right));
            Assert.Throws<ArgumentException>(() => left.Remainder(right));
            Assert.False(left.SameAs(right));
        }

        // --- field arithmetic against hand-computed results ---

        [Fact]
        public void AdditionAndSubtractionReduceIntoTheField()
        {
            // (3 + 5x) + (6 + 4x) = 9 + 9x = 2 + 2x over F_7.
            AssertSame(Poly(7, 2, 2), Poly(7, 3, 5).Add(Poly(7, 6, 4)));
            // (3 + 5x) - (6 + 4x) = -3 + x = 4 + x.
            AssertSame(Poly(7, 4, 1), Poly(7, 3, 5).Subtract(Poly(7, 6, 4)));
            // A subtraction whose leading terms cancel loses the degree.
            AssertSame(Poly(7, 4), Poly(7, 3, 5).Subtract(Poly(7, 6, 5)));
            AssertSame(PrimeFieldPolynomial.Zero(7), Poly(7, 3, 5).Subtract(Poly(7, 3, 5)));
        }

        [Fact]
        public void MultiplicationIsConvolutionModuloThePrime()
        {
            // (1 + 2x)(3 + 4x) = 3 + 10x + 8x^2 = 3 + 3x^2 over F_5.
            AssertSame(Poly(5, 3, 0, 3), Poly(5, 1, 2).Multiply(Poly(5, 3, 4)));
            AssertSame(PrimeFieldPolynomial.Zero(5), Poly(5, 1, 2).Multiply(PrimeFieldPolynomial.Zero(5)));
            AssertSame(Poly(5, 1, 2), Poly(5, 1, 2).Multiply(PrimeFieldPolynomial.One(5)));
        }

        [Fact]
        public void DivisionMatchesTheHandComputation()
        {
            // (x^3 + 2x + 1) = (x + 3)(x^2 + 4x + 4) + 3 over F_7.
            var dividend = Poly(7, 1, 2, 0, 1);
            var divisor = Poly(7, 3, 1);
            AssertSame(Poly(7, 4, 4, 1), dividend.Quotient(divisor));
            AssertSame(Poly(7, 3), dividend.Remainder(divisor));
        }

        [Fact]
        public void DivisionByALowerDegreeOrNonMonicDivisorStillReconstructs()
        {
            // A divisor of higher degree leaves the dividend untouched as the remainder.
            var small = Poly(11, 1, 1);
            var big = Poly(11, 5, 0, 0, 2);
            Assert.True(small.Quotient(big).IsZero);
            AssertSame(small, small.Remainder(big));

            // Nothing above assumed a monic divisor; the leading coefficient is inverted.
            foreach (var dividend in MonicPolynomials(5, 3))
                foreach (var divisor in new[] { Poly(5, 1, 3), Poly(5, 4, 0, 2), Poly(5, 2) })
                {
                    var quotient = dividend.Quotient(divisor);
                    var remainder = dividend.Remainder(divisor);
                    Assert.True(remainder.Degree < divisor.Degree);
                    AssertSame(dividend, quotient.Multiply(divisor).Add(remainder));
                }
        }

        [Fact]
        public void DivisionByZeroIsRefused()
        {
            var zero = PrimeFieldPolynomial.Zero(5);
            Assert.Throws<DivideByZeroException>(() => Poly(5, 1, 1).Quotient(zero));
            Assert.Throws<DivideByZeroException>(() => Poly(5, 1, 1).Remainder(zero));
        }

        [Fact]
        public void MakeMonicDividesByTheLeadingCoefficient()
        {
            // 3^-1 is 2 modulo 5, so 4 + 3x becomes 8 + 6x = 3 + x.
            AssertSame(Poly(5, 3, 1), Poly(5, 4, 3).MakeMonic());
            AssertSame(PrimeFieldPolynomial.Zero(5), PrimeFieldPolynomial.Zero(5).MakeMonic());
            AssertSame(PrimeFieldPolynomial.One(5), Poly(5, 4).MakeMonic());
        }

        // --- greatest common divisor ---

        [Fact]
        public void GcdOfKnownCases()
        {
            // x^2 - 1 = (x - 1)(x + 1) over F_7, so the gcd with x - 1 is x - 1 = x + 6.
            AssertSame(Poly(7, 6, 1), Poly(7, -1, 0, 1).Gcd(Poly(7, -1, 1)));
            // x^2 + 1 and x + 1 are coprime over F_7 (-1 is not a square there).
            AssertSame(PrimeFieldPolynomial.One(7), Poly(7, 1, 0, 1).Gcd(Poly(7, 1, 1)));
            // The result is monic even where neither argument is.
            AssertSame(Poly(5, 3, 1), Poly(5, 2, 4).Gcd(Poly(5, 4, 3)));
        }

        [Fact]
        public void GcdWithItselfAndWithZero()
        {
            foreach (var poly in MonicPolynomials(5, 3).Concat(new[] { Poly(5, 2, 4), Poly(5, 3) }))
            {
                AssertSame(poly.MakeMonic(), poly.Gcd(poly));
                AssertSame(poly.MakeMonic(), poly.Gcd(PrimeFieldPolynomial.Zero(5)));
                AssertSame(poly.MakeMonic(), PrimeFieldPolynomial.Zero(5).Gcd(poly));
            }
            Assert.True(PrimeFieldPolynomial.Zero(5).Gcd(PrimeFieldPolynomial.Zero(5)).IsZero);
        }

        [Fact]
        public void GcdDividesBothAndIsDivisibleByEveryCommonDivisor()
        {
            foreach (var left in MonicPolynomials(3, 3))
                foreach (var right in MonicPolynomials(3, 2))
                {
                    var gcd = left.Gcd(right);
                    Assert.True(gcd.IsMonic);
                    Assert.True(left.Remainder(gcd).IsZero, $"{gcd} does not divide {left}");
                    Assert.True(right.Remainder(gcd).IsZero, $"{gcd} does not divide {right}");
                    // Greatest, not merely common: anything dividing both divides it.
                    foreach (var common in MonicPolynomials(3, 1).Concat(MonicPolynomials(3, 2)))
                        if (left.Remainder(common).IsZero && right.Remainder(common).IsZero)
                            Assert.True(gcd.Remainder(common).IsZero, $"{common} divides both but not {gcd}");
                }
        }

        // --- powers ---

        [Fact]
        public void PowModAgreesWithRepeatedMultiplication()
        {
            var modulus = Poly(5, 1, 1, 0, 1);
            foreach (var basePoly in new[] { Poly(5, 3, 2), Poly(5, 0, 1), Poly(5, 4), PrimeFieldPolynomial.Zero(5) })
            {
                var expected = PrimeFieldPolynomial.One(5).Remainder(modulus);
                for (var exponent = 0; exponent <= 20; exponent++)
                {
                    AssertSame(expected, basePoly.PowMod(EInteger.FromInt32(exponent), modulus));
                    expected = expected.Multiply(basePoly).Remainder(modulus);
                }
            }
        }

        [Fact]
        public void PowModRefusesANegativeExponent()
            => Assert.Throws<ArgumentOutOfRangeException>(
                () => Poly(5, 0, 1).PowMod(EInteger.FromInt32(-1), Poly(5, 1, 0, 1)));

        [Fact]
        public void PowModHandlesAnExponentBeyondLong()
        {
            // Fermat in F_p[x]/(f) for an irreducible f of degree 3: the multiplicative
            // group has p^3 - 1 elements, so anything nonzero raised to it is one.
            var modulus = Poly(2, 1, 1, 0, 1);
            var order = EInteger.FromInt32(2).Pow(3).Subtract(EInteger.One);
            var big = order.Multiply(EInteger.FromString("100000000000000000000000"));
            AssertSame(PrimeFieldPolynomial.One(2), Poly(2, 0, 1).PowMod(big, modulus));
        }

        // --- the derivative, including characteristic p ---

        [Fact]
        public void DerivativeOfTheUsualKind()
        {
            // d/dx (3x^2 + 4x + 1) = 6x + 4 over F_7.
            AssertSame(Poly(7, 4, 6), Poly(7, 1, 4, 3).Derivative());
            Assert.True(Poly(7, 5).Derivative().IsZero);
            Assert.True(PrimeFieldPolynomial.Zero(7).Derivative().IsZero);
        }

        [Theory]
        [InlineData(2L)]
        [InlineData(3L)]
        [InlineData(5L)]
        [InlineData(7L)]
        public void TheDerivativeOfXToThePVanishes(long prime)
        {
            Assert.True(PrimeFieldPolynomial.Monomial((int)prime, prime).Derivative().IsZero);
            // And so the whole derivative of any polynomial in x^p does.
            var poly = PrimeFieldPolynomial.Monomial(2 * (int)prime, prime)
                .Add(PrimeFieldPolynomial.Monomial((int)prime, prime))
                .Add(PrimeFieldPolynomial.One(prime));
            Assert.True(poly.Derivative().IsZero);
            // Which is exactly the p-th power it is: x^2p + x^p + 1 = (x^2 + x + 1)^p.
            var root = Poly(prime, 1, 1, 1);
            var power = PrimeFieldPolynomial.One(prime);
            for (var i = 0; i < prime; i++)
                power = power.Multiply(root);
            AssertSame(poly, power);
        }

        [Fact]
        public void SquareFreedomAgreesWithTheDefinition()
        {
            Assert.True(Poly(5, 1, 0, 1).IsSquareFree);
            // x^2 + 1 = (x + 1)^2 over F_2.
            Assert.False(Poly(2, 1, 0, 1).IsSquareFree);
            Assert.False(Poly(5, 0, 0, 1).IsSquareFree);
            // A p-th power has a vanishing derivative rather than a nontrivial gcd with it.
            Assert.False(PrimeFieldPolynomial.Monomial(5, 5).IsSquareFree);
            Assert.False(PrimeFieldPolynomial.Zero(5).IsSquareFree);
            Assert.True(PrimeFieldPolynomial.One(5).IsSquareFree);

            // Against the definition: no monic polynomial of degree >= 1 squares into it.
            foreach (var poly in MonicPolynomials(3, 4))
            {
                var hasSquare = MonicPolynomials(3, 1).Concat(MonicPolynomials(3, 2))
                    .Any(divisor => poly.Remainder(divisor.Multiply(divisor)).IsZero);
                Assert.Equal(!hasSquare, poly.IsSquareFree);
            }
        }

        // --- known factorisations ---

        [Fact]
        public void KnownFactorisations()
        {
            // x^2 + 1 = (x + 2)(x + 3) over F_5.
            var overFive = Factored(Poly(5, 1, 0, 1));
            Assert.Equal(2, overFive.Count);
            AssertSame(Poly(5, 2, 1), overFive[0]);
            AssertSame(Poly(5, 3, 1), overFive[1]);

            // x^3 + x + 1 has no root in F_2 and so, being a cubic, is irreducible.
            var cubic = Poly(2, 1, 1, 0, 1);
            var overTwo = Factored(cubic);
            Assert.Equal(1, overTwo.Count);
            AssertSame(cubic, overTwo[0]);

            // A linear polynomial is its own factorisation.
            var linear = Poly(5, 4, 1);
            var linearFactors = Factored(linear);
            Assert.Equal(1, linearFactors.Count);
            AssertSame(linear, linearFactors[0]);

            // The unit has no irreducible factors, which is an empty list and not a refusal.
            Assert.Empty(Factored(PrimeFieldPolynomial.One(5)));
        }

        [Theory]
        [InlineData(2L)]
        [InlineData(3L)]
        [InlineData(5L)]
        [InlineData(7L)]
        public void XToThePMinusXIsTheProductOfEveryLinearFactor(long prime)
        {
            var poly = PrimeFieldPolynomial.Monomial((int)prime, prime)
                .Subtract(PrimeFieldPolynomial.Monomial(1, prime));
            Assert.True(poly.IsMonic);
            Assert.True(poly.IsSquareFree);

            var factors = Factored(poly);
            Assert.Equal((int)prime, factors.Count);

            // Every element of the field is a root, each exactly once.
            var roots = new HashSet<long>();
            var product = PrimeFieldPolynomial.One(prime);
            foreach (var factor in factors)
            {
                Assert.Equal(1, factor.Degree);
                Assert.True(factor.IsMonic);
                roots.Add((prime - factor.Coefficients[0]) % prime);
                product = product.Multiply(factor);
            }
            Assert.Equal((int)prime, roots.Count);
            AssertSame(poly, product);
        }

        [Fact]
        public void FactorDeclinesWhatItIsNotContractedFor()
        {
            // Not square-free: x^2 + 1 = (x + 1)^2 over F_2.
            Assert.Null(PrimeFieldFactorization.Factor(Poly(2, 1, 0, 1)));
            // Not monic.
            Assert.Null(PrimeFieldFactorization.Factor(Poly(5, 1, 0, 2)));
            // The zero polynomial, which is not monic either.
            Assert.Null(PrimeFieldFactorization.Factor(PrimeFieldPolynomial.Zero(5)));
            // A composite modulus is not a field, and the arithmetic itself does not check.
            Assert.Null(PrimeFieldFactorization.Factor(Poly(4, 1, 0, 1)));
            // Past the guards rather than hanging on them.
            Assert.Null(PrimeFieldFactorization.Factor(Poly(1031, 1, 0, 1)));
            Assert.Null(PrimeFieldFactorization.Factor(
                PrimeFieldPolynomial.Monomial(PrimeFieldFactorization.MaxDegree + 1, 2)
                    .Add(PrimeFieldPolynomial.One(2))));
        }

        [Fact]
        public void FactoringTwiceGivesTheIdenticalSequence()
        {
            // x^6 - 1 splits into all six nonzero linear factors over F_7, so there is
            // plenty of room for two runs to disagree on an order.
            var poly = PrimeFieldPolynomial.Monomial(6, 7).Subtract(PrimeFieldPolynomial.One(7));
            var first = Factored(poly);
            var second = Factored(poly);
            Assert.Equal(6, first.Count);
            Assert.Equal(first.Count, second.Count);
            for (var i = 0; i < first.Count; i++)
                AssertSame(first[i], second[i]);
        }

        // --- the exhaustive cross-check, and the check on its oracle ---

        [Theory]
        [InlineData(2L)]
        [InlineData(3L)]
        [InlineData(5L)]
        public void TrialDivisionAgreesWithGausssCount(long prime)
        {
            // Gauss's count of the monic irreducibles of degree d over F_p is
            // (1/d) sum over e dividing d of mu(e) p^(d/e). If the oracle below were
            // wrong in either direction -- and one that answered "irreducible" always
            // would leave the exhaustive test toothless -- these would not match.
            var expected = new[]
            {
                prime,
                (prime * prime - prime) / 2,
                (prime * prime * prime - prime) / 3,
                (prime * prime * prime * prime - prime * prime) / 4,
            };
            for (var degree = 1; degree <= 4; degree++)
                Assert.Equal(
                    expected[degree - 1],
                    MonicPolynomials(prime, degree).Count(IsIrreducibleByTrialDivision));
        }

        [Theory]
        [InlineData(2L)]
        [InlineData(3L)]
        [InlineData(5L)]
        public void EveryMonicPolynomialOfSmallDegreeFactorsCorrectly(long prime)
        {
            for (var degree = 2; degree <= 4; degree++)
                foreach (var poly in MonicPolynomials(prime, degree))
                {
                    if (!poly.IsSquareFree)
                    {
                        Assert.Null(PrimeFieldFactorization.Factor(poly));
                        continue;
                    }
                    var factors = Factored(poly);

                    var product = PrimeFieldPolynomial.One(prime);
                    foreach (var factor in factors)
                    {
                        Assert.True(factor.IsMonic, $"{factor} is a factor of {poly} but is not monic");
                        Assert.True(factor.Degree >= 1, $"{poly} was given the unit {factor} as a factor");
                        Assert.True(
                            IsIrreducibleByTrialDivision(factor),
                            $"{factor} is a factor of {poly} but is not irreducible");
                        product = product.Multiply(factor);
                    }
                    Assert.True(
                        product.SameAs(poly),
                        $"{poly} is not the product of {string.Join(" * ", factors)}");
                }
        }
    }
}
