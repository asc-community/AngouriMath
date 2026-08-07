//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Common
{
    /// <summary>
    /// What <see cref="Entity.Stringize()"/> is for: parsing what it prints gives back the
    /// expression it printed. Anything else makes the printed form a lie, and a silent one,
    /// since a wrong reading is still a valid expression.
    /// </summary>
    public sealed class StringizeRoundTripTest
    {
        private static void AssertRoundTrip(string source)
        {
            var original = source.ToEntity();
            var printed = original.Stringize();
            Assert.Equal(original, printed.ToEntity());
        }

        /// <summary>
        /// Powers group to the right, so it is the base that needs bracketing when it is a power
        /// of its own -- the mirror of what the left-associative operators need. Printing
        /// (2 ^ 3) ^ 2 as 2 ^ 3 ^ 2 did not merely look wrong: the first is 64 and the second is
        /// 512, so the printed form was a different expression.
        /// </summary>
        [Theory]
        [InlineData("(x ^ y) ^ z")]
        [InlineData("((x ^ y) ^ z) ^ w")]
        [InlineData("x ^ (y ^ z)")]
        [InlineData("x ^ y ^ z")]
        [InlineData("(2 ^ 2) ^ 3")]
        [InlineData("2 ^ 2 ^ 3")]
        [InlineData("(x + 1) ^ (y + 1)")]
        [InlineData("(-x) ^ 2")]
        [InlineData("2 ^ (-x)")]
        public void PowersKeepTheirGrouping(string source) => AssertRoundTrip(source);

        [Fact]
        public void ANestedPowerKeepsItsValue()
        {
            var original = "(2 ^ 3) ^ 2".ToEntity();
            Assert.Equal(original.Evaled, original.Stringize().ToEntity().Evaled);
        }

        /// <summary>
        /// These three used to be printed in spellings the parser does not have. A lambda came
        /// back as an implication, an application as a power, and a piecewise as a product with
        /// "if" read as an undeclared variable -- or, with more than one case, as a parse error.
        /// </summary>
        [Theory]
        [InlineData("lambda(x, x + 1)")]
        [InlineData("lambda(x, lambda(y, x + y))")]
        [InlineData("apply(lambda(x, x + 1), 2)")]
        [InlineData("apply(lambda(x, lambda(y, x + y)), 1, 2)")]
        [InlineData("piecewise(1 provided x > 0)")]
        [InlineData("piecewise(1 provided x > 0, 2 provided x < 0)")]
        [InlineData("piecewise(x + 1 provided x > 0, x - 1 provided x < 0)")]
        public void TheNodesWithoutAnOperatorSpellingPrintAsTheirFunction(string source) =>
            AssertRoundTrip(source);

        [Theory]
        [InlineData("x + y")]
        [InlineData("x - y")]
        [InlineData("x * y")]
        [InlineData("x / y")]
        [InlineData("-x")]
        [InlineData("x!")]
        [InlineData("(x + y) * z")]
        [InlineData("x + y * z")]
        [InlineData("(x - y) - z")]
        [InlineData("x - (y - z)")]
        [InlineData("x / y / z")]
        [InlineData("x / (y / z)")]
        [InlineData("-(x + y)")]
        public void ArithmeticKeepsItsGrouping(string source) => AssertRoundTrip(source);

        [Theory]
        [InlineData("sin(x)")]
        [InlineData("cos(x)")]
        [InlineData("tan(x)")]
        [InlineData("cotan(x)")]
        [InlineData("sec(x)")]
        [InlineData("cosec(x)")]
        [InlineData("arcsin(x)")]
        [InlineData("arccos(x)")]
        [InlineData("arctan(x)")]
        [InlineData("arccotan(x)")]
        [InlineData("arcsec(x)")]
        [InlineData("arccosec(x)")]
        [InlineData("sinh(x)")]
        [InlineData("cosh(x)")]
        [InlineData("tanh(x)")]
        [InlineData("cotanh(x)")]
        [InlineData("sech(x)")]
        [InlineData("cosech(x)")]
        [InlineData("ln(x)")]
        [InlineData("log(2, x)")]
        [InlineData("sqrt(x)")]
        [InlineData("abs(x)")]
        [InlineData("signum(x)")]
        [InlineData("phi(x)")]
        [InlineData("gamma(x)")]
        [InlineData("derivative(x ^ 2, x, 1)")]
        [InlineData("limit(x, x, 0)")]
        [InlineData("limitleft(x, x, 0)")]
        [InlineData("limitright(x, x, 0)")]
        public void FunctionsRoundTrip(string source) => AssertRoundTrip(source);

        [Theory]
        [InlineData("a and b")]
        [InlineData("a or b")]
        [InlineData("a xor b")]
        [InlineData("not a")]
        [InlineData("a implies b")]
        [InlineData("a and b or c")]
        [InlineData("a or b and c")]
        [InlineData("not (a and b)")]
        [InlineData("x > y")]
        [InlineData("x < y")]
        [InlineData("x >= y")]
        [InlineData("x <= y")]
        [InlineData("x = y")]
        [InlineData("x <> y")]
        [InlineData("x provided y")]
        public void BooleansRoundTrip(string source) => AssertRoundTrip(source);

        [Theory]
        [InlineData("{ 1, 2, 3 }")]
        [InlineData("[1; 2]")]
        [InlineData("(1; 2)")]
        [InlineData("[1; 2)")]
        [InlineData("A unite B")]
        [InlineData("A intersect B")]
        [InlineData("A setsubtract B")]
        [InlineData("x in A")]
        [InlineData("RR")]
        [InlineData("CC")]
        [InlineData("ZZ")]
        [InlineData("QQ")]
        [InlineData("BB")]
        public void SetsRoundTrip(string source) => AssertRoundTrip(source);

        [Theory]
        [InlineData("[[1, 2], [3, 4]]")]
        [InlineData("[1, 2, 3]")]
        [InlineData("1/2")]
        [InlineData("-1/2")]
        [InlineData("i")]
        [InlineData("-i")]
        [InlineData("2 + 3 * i")]
        [InlineData("+oo")]
        [InlineData("-oo")]
        [InlineData("e")]
        [InlineData("pi")]
        public void MatricesAndNumbersRoundTrip(string source) => AssertRoundTrip(source);

        /// <summary>
        /// The cases above round-trip the expression as written, so <c>i / 2</c> stays a
        /// division and never reaches the printer for a complex <em>number</em>. Evaluating
        /// first is what exercises that printer, and it printed a pure imaginary with a
        /// fractional part as <c>1/2i</c> -- which the parser reads as <c>1/(2i)</c>, the
        /// negation of what was printed. The mixed case was already right, bracketing it as
        /// <c>2 + (3/4)i</c>; the pure-imaginary arm returned before that bracketing was
        /// applied.
        ///
        /// Silent, and it misleads a reader as much as a parser: every root of a
        /// trigonometric equation is printed through this.
        /// </summary>
        [Theory]
        [InlineData("i / 2")]
        [InlineData("-i / 2")]
        [InlineData("3 * i / 4")]
        [InlineData("-5 * i / 3")]
        [InlineData("2 + 3 * i / 4")]
        [InlineData("2 - 3 * i / 4")]
        [InlineData("i")]
        [InlineData("-i")]
        [InlineData("2 * i")]
        [InlineData("-2 * i")]
        [InlineData("1/2")]
        [InlineData("2 + 3 * i")]
        public void AnEvaluatedComplexNumberRoundTrips(string source)
        {
            var value = source.ToEntity().Evaled;
            Assert.Equal(value, value.Stringize().ToEntity().Evaled);
        }
    }
}
