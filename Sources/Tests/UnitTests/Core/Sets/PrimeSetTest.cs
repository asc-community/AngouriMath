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

namespace AngouriMath.Tests.Core.Sets
{
    /// <summary>
    /// <c>PP</c> is the set of primes <c>{2, 3, 5, 7, ...}</c>: membership decided for a whole
    /// number by trial division and left open past the machine word, the set cut by an interval
    /// to its members, its least member and the next prime after a bound, and its place at the
    /// bottom of the chain <c>PP ⊂ ZZ+ ⊂ ZZ* ⊂ ZZ</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1450">#1450</a>
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class PrimeSetTest
    {
        [Theory]
        [InlineData("2 in PP", "True")]
        [InlineData("7 in PP", "True")]
        [InlineData("97 in PP", "True")]
        [InlineData("1 in PP", "False")]
        [InlineData("0 in PP", "False")]
        [InlineData("-3 in PP", "False")]
        [InlineData("8 in PP", "False")]
        [InlineData("91 in PP", "False")]
        [InlineData("1/2 in PP", "False")]
        [InlineData("2.5 in PP", "False")]
        [InlineData("i in PP", "False")]
        [InlineData("{1, 2} in PP", "False")]
        // Arithmetic is evaluated before membership is decided.
        [InlineData("2^5 - 1 in PP", "True")]
        [InlineData("2^11 - 1 in PP", "False")]
        [InlineData("10^18 + 9 in PP", "True")]
        // The chain.
        [InlineData("PP subset ZZ+", "True")]
        [InlineData("PP subset ZZ*", "True")]
        [InlineData("PP subset ZZ", "True")]
        [InlineData("ZZ+ subset PP", "False")]
        [InlineData("PP subset PP", "True")]
        [InlineData("PP intersect ZZ", "PP")]
        [InlineData("PP unite ZZ+", "ZZ+")]
        [InlineData("PP intersect BB", "{}")]
        [InlineData("PP \\ PP", "{}")]
        [InlineData("ZZ+ \\ PP", "{ x in ZZ+ : not x in PP }")]
        // Cut by an interval: the members listed, and the prime-counting function through them.
        [InlineData("PP intersect [1; 30]", "{ 2, 3, 5, 7, 11, 13, 17, 19, 23, 29 }")]
        [InlineData("PP intersect (2; 11)", "{ 3, 5, 7 }")]
        [InlineData("PP intersect [24; 28]", "{}")]
        [InlineData("card(PP intersect [1; 100])", "25")]
        [InlineData("card(PP intersect [1; 1000])", "168")]
        // The least member, and the next prime after a bound (Bertrand: there always is one).
        [InlineData("min(PP)", "2")]
        [InlineData("min(x, x in PP and x > 14)", "17")]
        [InlineData("min(x, x in PP and x >= 17)", "17")]
        [InlineData("min(PP intersect (100; +oo))", "101")]
        [InlineData("min(x, x in PP and x > 10^18)", "1000000000000000003")]
        [InlineData("min(ZZ+)", "1")]
        [InlineData("min(ZZ*)", "0")]
        [InlineData("min(x, x in ZZ+ and x >= 7/2)", "4")]
        [InlineData("min(x, x in PP and x > 14 and x < 20)", "17")]
        [InlineData("min(x, 3 < x and x in ZZ+ and x <= 5)", "4")]
        // The greatest member, and the previous prime before a bound.
        [InlineData("max(x, x in PP and x < 14)", "13")]
        [InlineData("max(x, x in PP and x <= 2)", "2")]
        [InlineData("max(PP intersect (-oo; 100])", "97")]
        [InlineData("max(PP intersect [1; 30])", "29")]
        [InlineData("max(x, x > 10^18 and x in PP and x < 10^18 + 100)", "1000000000000000079")]
        [InlineData("max(ZZ intersect (-oo; 5))", "4")]
        [InlineData("max(x, x in ZZ* and x < 1)", "0")]
        // Quantified over the primes: every one is at least 2 and not every one is even.
        [InlineData("forall p in PP : p >= 2", "True")]
        [InlineData("forall p in PP : p > 2", "False")]
        [InlineData("forall p in PP : 2 divides p", "False")]
        [InlineData("exists p in PP : p = 4", "False")]
        [InlineData("exists p in PP : p > 100", "True")]
        // The n-th prime, a function of its own: PP carries no order.
        [InlineData("prime(1)", "2")]
        [InlineData("prime(25)", "97")]
        [InlineData("prime(1000)", "7919")]
        [InlineData("prime(prime(2))", "5")]
        [InlineData("prime(2) + prime(3)", "8")]
        [InlineData("prime(0)", "NaN")]
        [InlineData("prime(-1)", "NaN")]
        [InlineData("prime(1/2)", "NaN")]
        // The p-adic valuation (Sullivan and Mackey's Ex 5.5.1: n = 2^m (2 l + 1), m its 2-adic one).
        [InlineData("valuation(12, 2)", "2")]
        [InlineData("valuation(12, 3)", "1")]
        [InlineData("valuation(12, 5)", "0")]
        [InlineData("valuation(-40, 2)", "3")]
        [InlineData("valuation(2^100 * 3, 2)", "100")]
        [InlineData("valuation(0, 3)", "+oo")]
        [InlineData("valuation(12, 4)", "NaN")]
        [InlineData("valuation(1/2, 2)", "NaN")]
        [InlineData("2^valuation(12, 2) * 3^valuation(12, 3)", "12")]
        public void Decided(string input, string expected)
            => Assert.Equal(expected.ToEntity(), input.ToEntity().Evaled);

        [Theory]
        // A symbol and a whole number past the machine word are left as a membership, a set
        // with no least member as an extremum, and a cut too wide to list as an intersection.
        [InlineData("x in PP", typeof(Set.Inf))]
        [InlineData("10^30 + 57 in PP", typeof(Set.Inf))]
        [InlineData("min(ZZ)", typeof(Minimumf))]
        [InlineData("max(PP)", typeof(Maximumf))]
        [InlineData("max(x, x in PP and x < 2)", typeof(Maximumf))]
        [InlineData("max(x, x in ZZ+ and x < 1)", typeof(Maximumf))]
        [InlineData("min(x, x in PP and x > 14 and x < 16)", typeof(Minimumf))]
        [InlineData("PP intersect [1; 10^7]", typeof(Set.Intersectionf))]
        [InlineData("prime(n)", typeof(Primef))]
        [InlineData("prime(10^9)", typeof(Primef))]
        [InlineData("valuation(n, 2)", typeof(Valuationf))]
        [InlineData("valuation(12, p)", typeof(Valuationf))]
        public void LeftAsWritten(string input, System.Type node)
            => Assert.IsType(node, input.ToEntity().Evaled);

        [Fact]
        public void PrintsAndReadsBack()
        {
            Assert.Equal("PP", MathS.Sets.Primes.ToString());
            Assert.Equal(@"\mathbb{P}", MathS.Sets.Primes.Latexize());
            Assert.Contains("sympy.S.Primes", MathS.ToSympyCode(MathS.Sets.Primes));
            Assert.Equal(MathS.Sets.Primes, "PP".ToEntity());
            Assert.Equal("min(PP)", "min(PP)".ToEntity().ToString());
            Assert.Equal(@"\min \mathbb{P}", "min(PP)".ToEntity().Latexize());
            Assert.Equal("min({ x in PP : x > 14 })", "min(x, x in PP and x > 14)".ToEntity().ToString());
            Assert.Equal(AngouriMath.Core.Domain.Prime, "domain(x, PP)".ToEntity().Codomain);
            Assert.Equal(AngouriMath.Core.Domain.Prime, "prime(n)".ToEntity().Codomain);
            Assert.Equal("prime(n)", MathS.NumberTheory.Prime("n").ToString());
            Assert.Equal(@"\operatorname{prime}\left(n\right)", "prime(n)".ToEntity().Latexize());
            Assert.Contains("sympy.prime(n)", MathS.ToSympyCode("prime(n)".ToEntity()));
            Assert.Equal("prime(n)".ToEntity(), "prime(n)".ToEntity().ToString().ToEntity());
            Assert.Equal("valuation(n, p)", MathS.NumberTheory.Valuation("n", "p").ToString());
            Assert.Equal(@"v_{p}\left(n\right)", "valuation(n, p)".ToEntity().Latexize());
            Assert.Contains("sympy.multiplicity(p, n)", MathS.ToSympyCode("valuation(n, p)".ToEntity()));
            Assert.Equal("valuation(n, p)".ToEntity(), "valuation(n, p)".ToEntity().ToString().ToEntity());
        }

        [Theory]
        [InlineData("domain(2 * x, PP)", "2", "NaN")]
        [InlineData("domain(2 * x, PP)", "1", "2")]
        [InlineData("domain(x + 1, PP)", "6", "7")]
        [InlineData("domain(x + 1, PP)", "7", "NaN")]
        public void ADomainOfPrimesIsNaNOffThem(string input, string at, string expected)
            => Assert.Equal(expected.ToEntity(), input.ToEntity().Substitute("x", at.ToEntity()).Evaled);
    }
}
