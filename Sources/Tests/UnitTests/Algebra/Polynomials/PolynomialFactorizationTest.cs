//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Collections.Generic;
using System.Linq;
using AngouriMath;
using AngouriMath.Extensions;
using AngouriMath.Functions;
using PeterO.Numbers;
using Xunit;
using static AngouriMath.Entity;

namespace AngouriMath.Tests.Algebra.Polynomials
{
    /// <summary>
    /// Univariate factorisation over the rationals — square-free decomposition, then
    /// Berlekamp modulo a prime, then a Hensel lift and recombination.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a> item 43.
    /// </summary>
    [Trait("Area", "Polynomials")]
    public sealed class PolynomialFactorizationTest
    {
        /// <summary>Coefficients lowest power first, the order the layer uses throughout.</summary>
        private static IntegerPolynomial P(params long[] coefficients)
            => IntegerPolynomial.Create(coefficients.Select(EInteger.FromInt64).ToArray());

        private static IntegerPolynomial Product(params IntegerPolynomial[] factors)
        {
            var product = IntegerPolynomial.One;
            foreach (var factor in factors)
            {
                Assert.NotNull(product.Multiply(factor));
                product = product.Multiply(factor)!;
            }
            return product;
        }

        #region the dense integer polynomial itself

        [Fact]
        public void TrailingZeroesAreNotStored()
        {
            Assert.Equal(-1, P().Degree);
            Assert.Equal(-1, P(0, 0, 0).Degree);
            Assert.True(P(0, 0).IsZero);
            Assert.Equal(1, P(1, 2, 0, 0).Degree);
        }

        [Fact]
        public void ArithmeticIsWhatItSays()
        {
            // (1 + x) + (1 - x) = 2
            Assert.True(P(1, 1).Add(P(1, -1)).SameAs(P(2)));
            // (1 + x) - (1 - x) = 2x
            Assert.True(P(1, 1).Subtract(P(1, -1)).SameAs(P(0, 2)));
            // (1 + x)(1 - x) = 1 - x^2
            Assert.True(P(1, 1).Multiply(P(1, -1))!.SameAs(P(1, 0, -1)));
            // (x - 1)(x^2 + x + 1) = x^3 - 1
            Assert.True(P(-1, 1).Multiply(P(1, 1, 1))!.SameAs(P(-1, 0, 0, 1)));
        }

        [Fact]
        public void ExactDivisionRefusesWhatItCannotDivide()
        {
            Assert.True(P(-1, 0, 0, 1).DivideExact(P(-1, 1))!.SameAs(P(1, 1, 1)));
            // x^2 + 1 has no linear factor over Z, and the division must say so rather
            // than round: this is the check every candidate factor is accepted on.
            Assert.Null(P(1, 0, 1).DivideExact(P(-1, 1)));
            // Exact over Q but not over Z, which is not good enough here.
            Assert.Null(P(1, 1).DivideExact(P(2)));
            Assert.True(P(2, 2).DivideExact(P(2))!.SameAs(P(1, 1)));
            Assert.Null(P(1, 1).DivideExact(IntegerPolynomial.Zero));
        }

        [Fact]
        public void DerivativeAndContent()
        {
            // (x^3 - 1)' = 3x^2
            Assert.True(P(-1, 0, 0, 1).Derivative().SameAs(P(0, 0, 3)));
            Assert.True(P(7).Derivative().IsZero);
            Assert.Equal(EInteger.FromInt32(6), P(6, 12, -18).Content());
            // The primitive part carries the sign, so that two greatest common divisors
            // computed by different routes are comparable rather than merely associate.
            Assert.True(P(-2, -4).PrimitivePart().SameAs(P(1, 2)));
            Assert.True(P(6, 12, -18).PrimitivePart().SameAs(P(-1, -2, 3)));
        }

        #endregion

        #region greatest common divisor over Z

        [Theory]
        // gcd(x^2 - 1, x^2 + 2x + 1) = x + 1
        [InlineData(new long[] { -1, 0, 1 }, new long[] { 1, 2, 1 }, new long[] { 1, 1 })]
        // gcd(x^3 - 1, x^2 - 1) = x - 1
        [InlineData(new long[] { -1, 0, 0, 1 }, new long[] { -1, 0, 1 }, new long[] { -1, 1 })]
        // Coprime.
        [InlineData(new long[] { 1, 0, 1 }, new long[] { 2, 0, 1 }, new long[] { 1 })]
        // The content is part of the divisor: gcd(2x^2 - 2, 4x + 4) = 2x + 2.
        [InlineData(new long[] { -2, 0, 2 }, new long[] { 4, 4 }, new long[] { 2, 2 })]
        // One divides the other.
        [InlineData(new long[] { -1, 0, 1 }, new long[] { -1, 1 }, new long[] { -1, 1 })]
        public void GcdIsWhatItShouldBe(long[] left, long[] right, long[] expected)
        {
            var gcd = IntegerPolynomial.Gcd(P(left), P(right));
            Assert.True(gcd.SameAs(P(expected)), $"expected {P(expected)}, got {gcd}");
            // Symmetric, and a divisor of both — checked by dividing, not by trusting.
            Assert.True(IntegerPolynomial.Gcd(P(right), P(left)).SameAs(P(expected)));
            Assert.NotNull(P(left).DivideExact(gcd));
            Assert.NotNull(P(right).DivideExact(gcd));
        }

        [Fact]
        public void GcdWithZeroIsTheOtherOne()
        {
            Assert.True(IntegerPolynomial.Gcd(P(-2, -2), IntegerPolynomial.Zero).SameAs(P(2, 2)));
            Assert.True(IntegerPolynomial.Gcd(IntegerPolynomial.Zero, IntegerPolynomial.Zero).IsZero);
        }

        /// <summary>
        /// The quotients by the greatest common divisor share nothing further, which is the
        /// half of the definition that a merely-common divisor would fail.
        /// </summary>
        [Fact]
        public void QuotientsByTheGcdAreCoprime()
        {
            var left = Product(P(1, 1), P(1, 1), P(-2, 1));
            var right = Product(P(1, 1), P(3, 1));
            var gcd = IntegerPolynomial.Gcd(left, right);
            Assert.True(gcd.SameAs(P(1, 1)));
            var quotientLeft = left.DivideExact(gcd);
            var quotientRight = right.DivideExact(gcd);
            Assert.NotNull(quotientLeft);
            Assert.NotNull(quotientRight);
            Assert.True(IntegerPolynomial.Gcd(quotientLeft!, quotientRight!).IsConstant);
        }

        #endregion

        #region square-free decomposition

        [Fact]
        public void YunSeparatesTheMultiplicities()
        {
            // (x + 1)^2 (x + 2)
            var poly = Product(P(1, 1), P(1, 1), P(2, 1));
            var parts = SquareFreeDecomposition.Decompose(poly);
            Assert.NotNull(parts);
            Assert.Equal(2, parts!.Count);
            Assert.True(parts.Single(p => p.Multiplicity == 1).Factor.SameAs(P(2, 1)));
            Assert.True(parts.Single(p => p.Multiplicity == 2).Factor.SameAs(P(1, 1)));
        }

        [Fact]
        public void ASquareFreeInputComesBackWhole()
        {
            var poly = P(-1, 0, 1);                       // x^2 - 1
            var parts = SquareFreeDecomposition.Decompose(poly);
            Assert.NotNull(parts);
            var part = Assert.Single(parts!);
            Assert.Equal(1, part.Multiplicity);
            Assert.True(part.Factor.SameAs(poly));
        }

        [Fact]
        public void APurePowerIsOnePartAtItsMultiplicity()
        {
            // (x^2 + 1)^3, whose repeated factor has no rational root at all — the case a
            // root-finding factoriser cannot see.
            var poly = Product(P(1, 0, 1), P(1, 0, 1), P(1, 0, 1));
            var parts = SquareFreeDecomposition.Decompose(poly);
            Assert.NotNull(parts);
            var part = Assert.Single(parts!);
            Assert.Equal(3, part.Multiplicity);
            Assert.True(part.Factor.SameAs(P(1, 0, 1)));
        }

        /// <summary>
        /// The three properties that define the decomposition, on a spread of inputs built
        /// so that the multiplicities are known in advance but the routine is not told them.
        /// </summary>
        [Theory]
        [InlineData(2, 1, 1)]
        [InlineData(1, 2, 1)]
        [InlineData(3, 1, 2)]
        [InlineData(1, 1, 4)]
        [InlineData(2, 3, 1)]
        public void TheDecompositionMultipliesBackAndIsSquareFree(int first, int second, int third)
        {
            var bases = new[] { P(-1, 1), P(1, 0, 1), P(2, 1) };
            var powers = new[] { first, second, third };
            var poly = IntegerPolynomial.One;
            for (var i = 0; i < bases.Length; i++)
                for (var j = 0; j < powers[i]; j++)
                    poly = poly.Multiply(bases[i])!;

            var parts = SquareFreeDecomposition.Decompose(poly);
            Assert.NotNull(parts);

            var rebuilt = IntegerPolynomial.One;
            foreach (var part in parts!)
            {
                // Square-free: sharing nothing with its own derivative.
                Assert.True(IntegerPolynomial.Gcd(part.Factor, part.Factor.Derivative()).IsConstant);
                for (var i = 0; i < part.Multiplicity; i++)
                    rebuilt = rebuilt.Multiply(part.Factor)!;
            }
            Assert.True(rebuilt.SameAs(poly.PrimitivePart()));

            // Pairwise coprime.
            for (var i = 0; i < parts.Count; i++)
                for (var j = i + 1; j < parts.Count; j++)
                    Assert.True(IntegerPolynomial.Gcd(parts[i].Factor, parts[j].Factor).IsConstant);
        }

        #endregion

        #region factorisation into irreducibles

        /// <summary>
        /// A deterministic table of polynomials irreducible over <c>Q</c>. Products of these
        /// are the inputs of the sweep below, so that the answer is known before the
        /// factoriser is asked.
        /// </summary>
        private static readonly IntegerPolynomial[] Irreducibles =
        {
            P(1, 1),            // x + 1
            P(-1, 1),           // x - 1
            P(2, 1),            // x + 2
            P(-3, 1),           // x - 3
            P(1, 2),            // 2x + 1
            P(1, 0, 1),         // x^2 + 1
            P(2, 0, 1),         // x^2 + 2
            P(-2, 0, 1),        // x^2 - 2
            P(1, 1, 1),         // x^2 + x + 1
            P(-2, 0, 0, 1),     // x^3 - 2, Eisenstein at 2
        };

        public static IEnumerable<object[]> IrreduciblePairs()
        {
            for (var i = 0; i < Irreducibles.Length; i++)
                for (var j = i; j < Irreducibles.Length; j++)
                    yield return new object[] { i, j };
        }

        /// <summary>
        /// Every product of two irreducibles from the table factors back into exactly those
        /// two. Squares are included, so this covers the repeated case as well.
        /// </summary>
        [Theory]
        [MemberData(nameof(IrreduciblePairs))]
        public void AProductOfTwoIrreduciblesFactorsBackIntoThem(int first, int second)
        {
            var expected = new[] { Irreducibles[first], Irreducibles[second] };
            var poly = Product(expected);

            var parts = PolynomialFactorization.FactorPrimitive(poly.PrimitivePart());
            Assert.NotNull(parts);

            var got = new List<string>();
            foreach (var part in parts!)
                for (var i = 0; i < part.Multiplicity; i++)
                    got.Add(part.Factor.ToString());
            Assert.Equal(
                expected.Select(f => f.ToString()).OrderBy(s => s, System.StringComparer.Ordinal).ToArray(),
                got.OrderBy(s => s, System.StringComparer.Ordinal).ToArray());
        }

        public static IEnumerable<object[]> IrreducibleTriples()
        {
            for (var i = 0; i < Irreducibles.Length; i += 2)
                for (var j = i; j < Irreducibles.Length; j += 3)
                    for (var k = j; k < Irreducibles.Length; k += 4)
                        yield return new object[] { i, j, k };
        }

        [Theory]
        [MemberData(nameof(IrreducibleTriples))]
        public void AProductOfThreeIrreduciblesFactorsBackIntoThem(int first, int second, int third)
        {
            var expected = new[] { Irreducibles[first], Irreducibles[second], Irreducibles[third] };
            var poly = Product(expected);
            if (poly.Degree > IntegerPolynomial.MaxDegree)
                return;

            var parts = PolynomialFactorization.FactorPrimitive(poly.PrimitivePart());
            Assert.NotNull(parts);

            var got = new List<string>();
            foreach (var part in parts!)
                for (var i = 0; i < part.Multiplicity; i++)
                    got.Add(part.Factor.ToString());
            Assert.Equal(
                expected.Select(f => f.ToString()).OrderBy(s => s, System.StringComparer.Ordinal).ToArray(),
                got.OrderBy(s => s, System.StringComparer.Ordinal).ToArray());
        }

        /// <summary>
        /// Irreducible over <c>Q</c>, and each of these is a trap for a different reason.
        /// <c>x^4 + 1</c> and <c>x^4 - 10x^2 + 1</c> are the classical ones: both factor
        /// modulo <i>every</i> prime, so a factoriser that reads its answer off the modular
        /// factorisation instead of recombining and dividing will split them and be wrong.
        /// </summary>
        [Theory]
        [InlineData(new long[] { -2, 0, 1 })]              // x^2 - 2
        [InlineData(new long[] { 1, 1, 1 })]               // x^2 + x + 1
        [InlineData(new long[] { -2, 0, 0, 1 })]           // x^3 - 2
        [InlineData(new long[] { 1, 0, 0, 0, 1 })]         // x^4 + 1
        [InlineData(new long[] { 1, 0, -10, 0, 1 })]       // x^4 - 10x^2 + 1
        [InlineData(new long[] { -1, -1, 0, 0, 0, 1 })]    // x^5 - x - 1
        public void AnIrreduciblePolynomialIsReportedAsOneFactor(long[] coefficients)
        {
            var parts = PolynomialFactorization.FactorPrimitive(P(coefficients));
            Assert.NotNull(parts);
            var part = Assert.Single(parts!);
            Assert.Equal(1, part.Multiplicity);
            Assert.True(part.Factor.SameAs(P(coefficients)));
        }

        /// <summary>
        /// The Swinnerton-Dyer polynomial with roots <c>±√2 ±√3 ±√5</c>. It is irreducible
        /// over <c>Q</c> and yet has no irreducible factor of degree above two modulo
        /// <i>any</i> prime, so the recombination has to try every subset of the four modular
        /// quadratics, find that none of them divides, and conclude irreducibility from the
        /// search being exhausted. Nothing smaller exercises that path.
        /// </summary>
        [Fact]
        public void TheSwinnertonDyerOctichIsIrreducible()
        {
            var poly = P(576, 0, -960, 0, 352, 0, -40, 0, 1);
            var parts = PolynomialFactorization.FactorPrimitive(poly);
            Assert.NotNull(parts);
            var part = Assert.Single(parts!);
            Assert.Equal(1, part.Multiplicity);
            Assert.True(part.Factor.SameAs(poly));

            // The defining property, asserted rather than assumed — and with it, evidence
            // that the answer above came from an exhausted recombination and not from the
            // shortcut that returns early when the polynomial is irreducible modulo a prime.
            var usable = 0;
            foreach (var prime in new long[] { 7, 11, 13, 17, 19, 23 })
            {
                var reduced = poly.ToPrimeField(prime);
                if (reduced is null || !reduced.IsSquareFree)
                    continue;
                var modular = PrimeFieldFactorization.Factor(reduced.MakeMonic());
                Assert.NotNull(modular);
                Assert.True(modular!.Count >= 2, $"expected a split modulo {prime}, got {modular.Count}");
                usable++;
            }
            Assert.True(usable > 0, "no usable prime, so the property above went unchecked");
        }

        /// <summary>
        /// Three quadratics with no rational roots between them. Modulo a prime each may
        /// split into linear factors, so the recombination has to pair the right ones back
        /// together rather than returning six linear factors that do not exist over <c>Q</c>.
        /// </summary>
        [Fact]
        public void ThreeRootlessQuadraticsAreRecombinedCorrectly()
        {
            var poly = Product(P(1, 0, 1), P(2, 0, 1), P(3, 0, 1));
            var parts = PolynomialFactorization.FactorPrimitive(poly);
            Assert.NotNull(parts);
            Assert.Equal(3, parts!.Count);
            Assert.All(parts, p => Assert.Equal(2, p.Factor.Degree));
            Assert.True(Product(parts.Select(p => p.Factor).ToArray()).SameAs(poly));
        }

        [Fact]
        public void SixDistinctLinearFactorsAreAllFound()
        {
            var expected = new[] { P(-1, 1), P(-2, 1), P(-3, 1), P(-4, 1), P(-5, 1), P(-6, 1) };
            var parts = PolynomialFactorization.FactorPrimitive(Product(expected));
            Assert.NotNull(parts);
            Assert.Equal(6, parts!.Count);
            Assert.All(parts, p => Assert.Equal(1, p.Factor.Degree));
        }

        [Fact]
        public void CyclotomicSixSplitsIntoFour()
        {
            // x^6 - 1 = (x - 1)(x + 1)(x^2 + x + 1)(x^2 - x + 1)
            var parts = PolynomialFactorization.FactorPrimitive(P(-1, 0, 0, 0, 0, 0, 1));
            Assert.NotNull(parts);
            var got = parts!.Select(p => p.Factor.ToString())
                .OrderBy(s => s, System.StringComparer.Ordinal).ToArray();
            Assert.Equal(4, got.Length);
            Assert.All(parts, p => Assert.Equal(1, p.Multiplicity));
            var rebuilt = Product(parts!.Select(p => p.Factor).ToArray());
            Assert.True(rebuilt.SameAs(P(-1, 0, 0, 0, 0, 0, 1)));
        }

        /// <summary>
        /// Whatever comes out, multiplying it back has to give what went in. This is the
        /// property that separates an incomplete factorisation, which is a tolerable answer,
        /// from a wrong one, which is not.
        /// </summary>
        [Theory]
        [MemberData(nameof(IrreduciblePairs))]
        public void FactoringNeverChangesThePolynomial(int first, int second)
        {
            var poly = Product(Irreducibles[first], Irreducibles[second]).PrimitivePart();
            var parts = PolynomialFactorization.FactorPrimitive(poly);
            if (parts is null)
                return;                                   // a refusal is allowed; a wrong answer is not
            var rebuilt = IntegerPolynomial.One;
            foreach (var part in parts)
                for (var i = 0; i < part.Multiplicity; i++)
                    rebuilt = rebuilt.Multiply(part.Factor)!;
            Assert.True(rebuilt.SameAs(poly));
        }

        [Fact]
        public void FactoringIsDeterministic()
        {
            var poly = P(2, 0, 3, 0, 1);                  // (x^2 + 1)(x^2 + 2)
            var first = PolynomialFactorization.FactorPrimitive(poly);
            var second = PolynomialFactorization.FactorPrimitive(poly);
            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.Equal(
                first!.Select(p => p.Factor.ToString() + "^" + p.Multiplicity).ToArray(),
                second!.Select(p => p.Factor.ToString() + "^" + p.Multiplicity).ToArray());
        }

        #endregion

        #region the entity-level entry point

        [Theory]
        // The case the rational-root factoriser cannot reach: neither factor has a root.
        [InlineData("x ^ 4 + 3 * x ^ 2 + 2")]
        [InlineData("x ^ 4 - 5 * x ^ 2 + 4")]
        [InlineData("x ^ 6 - 1")]
        [InlineData("x ^ 4 - 1")]
        [InlineData("2 * x ^ 2 + 3 * x + 1")]
        [InlineData("6 * x ^ 2 + 18 * x + 12")]
        [InlineData("x ^ 2 / 2 + 3 * x / 2 + 1")]
        [InlineData("x ^ 5 + x ^ 4 + x ^ 3 + x ^ 2 + x + 1")]
        public void TheFactoredFormIsTheSamePolynomial(string input)
        {
            Assert.True(PolynomialFactorization.TryFactorIntoIrreducibles(
                input.ToEntity(), "x", out var factored));
            var difference = (input.ToEntity() - factored!).Expand().Simplify();
            while (difference is Providedf(var inner, _))
                difference = inner;
            Assert.Equal(Number.Integer.Create(0), difference);
        }

        [Fact]
        public void QuarticWithNoRootsStillFactors()
        {
            Assert.True(PolynomialFactorization.TryFactorIntoIrreducibles(
                "x ^ 4 + 3 * x ^ 2 + 2".ToEntity(), "x", out var factored));
            // Two quadratic factors, neither of which a search for rational roots could find.
            var product = Assert.IsType<Mulf>(factored);
            Assert.Equal(2, Mulf.LinearChildren(product).Count());
        }

        [Theory]
        [InlineData("x ^ 2 - 2")]
        [InlineData("x ^ 4 + 1")]
        [InlineData("x ^ 2 + x + 1")]
        [InlineData("sin(x) + x ^ 2")]
        [InlineData("x + 1")]
        public void NothingIsOfferedWhereThereIsNoFactorisation(string input)
            => Assert.False(PolynomialFactorization.TryFactorIntoIrreducibles(
                input.ToEntity(), "x", out _));

        #endregion
    }
}
