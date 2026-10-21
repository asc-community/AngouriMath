//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PeterO.Numbers;
using Xunit;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// The library's greatest common divisor and reduction to lowest terms, which every
    /// <see cref="Rational"/> is made through: they must answer exactly what PeterO's
    /// <see cref="EInteger.Gcd(EInteger)"/> and <see cref="ERational.ToLowestTerms"/> answer, and
    /// answer quickly where PeterO's does not -- when the common factor is itself large.
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class GreatestCommonDivisorTest
    {
        private static EInteger RandomInteger(Random random, int bits)
        {
            var bytes = new byte[bits / 8 + 2];
            random.NextBytes(bytes);
            bytes[^1] = 0;
            return EInteger.FromBytes(bytes, littleEndian: true).ShiftRight(bytes.Length * 8 - 8 - bits);
        }

        private static EInteger Signed(Random random, EInteger value) => random.Next(2) == 0 ? value : value.Negate();

        /// <summary>
        /// Pairs PeterO answers in reasonable time, so it can be the reference: random ones from
        /// one bit to three thousand, the edges of the machine-word path, and common factors of
        /// up to three hundred bits, past which PeterO itself becomes slow.
        /// </summary>
        private static IEnumerable<(EInteger, EInteger)> Pairs()
        {
            var two = EInteger.FromInt32(2);
            var edges = new[]
            {
                EInteger.Zero, EInteger.One, EInteger.FromInt32(-1), EInteger.FromInt32(6), EInteger.FromInt32(-4),
                EInteger.FromInt64(long.MaxValue), EInteger.FromInt64(long.MinValue), EInteger.FromInt64(long.MinValue + 1),
                two.Pow(62), two.Pow(63), two.Pow(63).Negate(), two.Pow(63).Add(EInteger.One), two.Pow(64), two.Pow(64).Subtract(EInteger.One),
                two.Pow(64).Negate(), two.Pow(200), EInteger.FromInt32(3).Pow(40), EInteger.FromInt32(3).Pow(41).Negate(),
            };
            foreach (var a in edges)
                foreach (var b in edges)
                    yield return (a, b);
            var random = new Random(1338);
            foreach (var bits in new[] { 1, 7, 31, 32, 33, 62, 63, 64, 65, 100, 192, 193, 400, 1000, 3000 })
                for (var i = 0; i < 20; i++)
                    yield return (Signed(random, RandomInteger(random, bits)), Signed(random, RandomInteger(random, random.Next(1, bits + 1))));
            foreach (var commonBits in new[] { 1, 20, 63, 64, 65, 120, 300 })
                for (var i = 0; i < 6; i++)
                {
                    var common = RandomInteger(random, commonBits).Add(EInteger.One);
                    yield return (Signed(random, common.Multiply(RandomInteger(random, random.Next(1, 1600)))),
                                  Signed(random, common.Multiply(RandomInteger(random, random.Next(1, 1200)))));
                }
        }

        [Fact]
        public void TheDivisorIsPeterOs()
        {
            foreach (var (a, b) in Pairs())
                Assert.Equal(a.Gcd(b), a.GreatestCommonDivisor(b));
        }

        [Fact]
        public void TheReductionIsPeterOs()
        {
            foreach (var (a, b) in Pairs())
            {
                if (b.IsZero)
                    continue;
                var fraction = ERational.Create(a, b);
                var expected = fraction.ToLowestTerms();
                var actual = fraction.InLowestTerms();
                Assert.Equal(expected, actual);
                Assert.Equal(expected.IsNegative, actual.IsNegative);
                Assert.Equal(expected.Numerator, actual.Numerator);
                Assert.Equal(expected.Denominator, actual.Denominator);
                // A zero is downcast to the integer 0, whose sign is not the one under test.
                if (!a.IsZero)
                    Assert.Equal(expected, Rational.Create(a, b).ERational);
            }
        }

        /// <summary>A zero keeps its sign, as PeterO's does, and what is not finite is itself.</summary>
        [Fact]
        public void ZerosAndWhatIsNotFinite()
        {
            foreach (var value in new[] { ERational.Zero, ERational.NegativeZero, ERational.Create(EInteger.Zero, EInteger.FromInt32(7)),
                                          ERational.Zero.Multiply(ERational.Create(-1, 2)), ERational.PositiveInfinity, ERational.NegativeInfinity })
            {
                var expected = value.ToLowestTerms();
                var actual = value.InLowestTerms();
                Assert.Equal(expected, actual);
                Assert.Equal(expected.IsNegative, actual.IsNegative);
            }
            Assert.True(ERational.NaN.InLowestTerms().IsNaN());
        }

        /// <summary>
        /// Common factors of a thousand bits and more, where PeterO takes from a second to more
        /// than a minute a pair: checked without it, by the divisor dividing both and leaving
        /// cofactors that share nothing -- which PeterO decides quickly, a coprime pair being its
        /// fast case.
        /// </summary>
        [Fact]
        public void ALargeCommonFactorIsFound()
        {
            var random = new Random(565);
            foreach (var (commonBits, leftBits, rightBits) in new[] { (1000, 1600, 1200), (1000, 800, 600), (1117, 1541, 1132), (5000, 8000, 6000) })
            {
                var common = RandomInteger(random, commonBits).Add(EInteger.One);
                var a = common.Multiply(RandomInteger(random, leftBits)).Negate();
                var b = common.Multiply(RandomInteger(random, rightBits));
                var divisor = a.GreatestCommonDivisor(b);
                Assert.True(a.Remainder(divisor).IsZero && b.Remainder(divisor).IsZero);
                Assert.Equal(EInteger.One, a.Divide(divisor).Gcd(b.Divide(divisor)));
                Assert.True(divisor.Remainder(common).IsZero);

                var reduced = ERational.Create(a, b).InLowestTerms();
                Assert.Equal(ERational.Create(a.Divide(divisor), b.Divide(divisor)), reduced);
                Assert.Equal(EInteger.One, reduced.Numerator.Gcd(reduced.Denominator));
            }
        }

        /// <summary>
        /// The sum that made Rubi's 1.1.2.4:565 time out: two fractions of fifteen hundred bits
        /// whose sum shares a factor of 1117 bits between numerator and denominator, which PeterO
        /// took 1.5 s to find and the library takes microseconds for. A hundred of them, through
        /// <see cref="Rational"/> arithmetic, on their own thread so that a regression fails the
        /// test rather than hanging the run; the bound is a minute, against tens of milliseconds
        /// expected and two and a half minutes for PeterO.
        /// </summary>
        [Fact]
        public void ALargeCommonFactorIsQuick()
        {
            static ERational PowerOf(ERational value, int power)
            {
                var product = ERational.One;
                for (var i = 0; i < power; i++)
                    product = product.Multiply(value);
                return product;
            }
            var x = Rational.Create(PowerOf(ERational.Create(23, 10), 333) * ERational.Create(-41, 2) * PowerOf(ERational.Create(-17, 10), 7) * PowerOf(ERational.Create(4, 5), 4));
            var y = Rational.Create(PowerOf(ERational.Create(23, 10), 331) * ERational.Create(-55, 2) * PowerOf(ERational.Create(-17, 10), 5) * PowerOf(ERational.Create(4, 5), 6));
            var unreduced = x.ERational + y.ERational;
            Assert.True(unreduced.Numerator.Abs().GreatestCommonDivisor(unreduced.Denominator).GetUnsignedBitLengthAsInt64() > 1100);

            var sums = Task.Factory.StartNew(() =>
            {
                var sum = (Entity.Number)x;
                for (var i = 0; i < 100; i++)
                    sum = x + y;
                return sum;
            }, TaskCreationOptions.LongRunning);
            Assert.True(sums.Wait(TimeSpan.FromMinutes(1)));
            var result = Assert.IsAssignableFrom<Rational>(sums.Result).ERational;
            Assert.Equal(EInteger.One, result.Numerator.Gcd(result.Denominator));
            Assert.Equal(0, result.CompareTo(unreduced));
        }
    }
}
