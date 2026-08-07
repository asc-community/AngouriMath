//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using PeterO.Numbers;
using Xunit;

namespace AngouriMath.Tests.Common
{
    /// <summary>
    /// Regression tests for parsing, numeric precision and packaging.
    /// Each test names the issue it locks down, so a future refactor that
    /// reintroduces the bug fails loudly.
    /// </summary>
    [Trait("Area", "Common")]
    public sealed class NumericsRegressionTest
    {
        // https://github.com/asc-community/AngouriMath/issues/625
        // `pow(a, b)` used to throw UnhandledParseException, because `pow` was lexed
        // as the implicit product p*o*w and then `(a, b)` was not a valid operand.
        [Theory]
        [InlineData("pow(2, 3)", "2 ^ 3")]
        [InlineData("pow(a, b)", "a ^ b")]
        [InlineData("pow(x + 1, 2)", "(x + 1) ^ 2")]
        [InlineData("pow(2, pow(3, 4))", "2 ^ 3 ^ 4")]
        public void Issue625_PowParses(string input, string expected) =>
            Assert.Equal(expected.ToEntity(), input.ToEntity());

        [Fact]
        public void Issue625_PowEvaluates() =>
            Assert.Equal(8, "pow(2, 3)".EvalNumerical());

        [Fact]
        public void Issue625_PowRequiresTwoArguments() =>
            Assert.Throws<AngouriMath.Core.Exceptions.FunctionArgumentCountException>(
                () => "pow(2)".ToEntity());

        // https://github.com/asc-community/AngouriMath/issues/584
        // 0.125 has no terminating expansion in base 5 (it is 0.0303... repeating),
        // so the digit loop used to spin forever. Terminating expansions must stay exact,
        // and non-terminating ones must stop at the configured decimal precision.
        [Theory]
        [InlineData(2, "1101.001")]
        [InlineData(8, "15.1")]
        [InlineData(10, "13.125")]
        [InlineData(16, "D.2")]
        public void Issue584_TerminatingExpansionsStayExact(int radix, string expected) =>
            Assert.Equal(expected, MathS.ToBaseN(13.125m, radix));

        [Fact]
        public void Issue584_NonTerminatingExpansionTerminatesQuickly()
        {
            var actual = MathS.ToBaseN(13.125m, 5);
            Assert.StartsWith("23.0303030303", actual);
            // 100 decimal digits of precision is ~144 base-5 digits; assert we are
            // bounded rather than pinning the exact constant.
            Assert.InRange(actual.Length, 100, 200);
        }

        [Fact]
        public void Issue584_NonTerminatingExpansionRoundTrips()
        {
            var text = MathS.ToBaseN(13.125m, 5);
            var back = MathS.FromBaseN(text, 5);
            Assert.True((back - 13.125m).Abs() < 0.0001m,
                $"round-trip of {text} gave {back}");
        }

        // Adjacent to https://github.com/asc-community/AngouriMath/issues/584:
        // a zero integer part used to render as the empty string,
        // so ToBaseN(0.5m, 2) produced ".1" and ToBaseN(0m, 2) produced "".
        [Theory]
        [InlineData(0, 2, "0")]
        [InlineData(0, 16, "0")]
        public void Issue584_ZeroIntegerPartIsRendered(int value, int radix, string expected) =>
            Assert.Equal(expected, MathS.ToBaseN(value, radix));

        [Fact]
        public void Issue584_LeadingZeroBeforePoint() =>
            Assert.Equal("0.1", MathS.ToBaseN(0.5m, 2));

        [Fact]
        public void Issue584_NegativeLeadingZeroBeforePoint() =>
            Assert.Equal("-0.1", MathS.ToBaseN(-0.5m, 2));

        [Fact]
        public void Issue584_HonoursPrecisionSetting()
        {
            using var _ = MathS.Settings.DecimalPrecisionContext.Set(
                new PeterO.Numbers.EContext(10, PeterO.Numbers.ERounding.HalfUp, -100, 1000, false));
            var actual = MathS.ToBaseN(13.125m, 5);
            Assert.StartsWith("23.0303", actual);
            Assert.InRange(actual.Length, 10, 30);
        }

        // https://github.com/asc-community/AngouriMath/issues/602
        // https://github.com/asc-community/AngouriMath/issues/210
        // Downcasting used to collapse anything below PrecisionErrorZeroRange (1e-16)
        // onto the integer 0, which destroyed every legitimate small number even
        // though the working precision is 100 digits.
        [Theory]
        [InlineData("e ^ (-5.4 - 6.3 * 6.56)", 5.0849589e-21)]
        [InlineData("e ^ (-40)", 4.2483543e-18)]
        [InlineData("e ^ (-50)", 1.9287498e-22)]
        [InlineData("2 ^ (-100)", 7.8886091e-31)]
        public void Issue602_SmallMagnitudesSurviveEvaluation(string input, double expected)
        {
            var actual = input.ToEntity().EvalNumerical().RealPart.EDecimal.ToDouble();
            Assert.NotEqual(0, actual);
            Assert.Equal(1, actual / expected, 6);
        }

        [Theory]
        [InlineData("1e-20", 1e-20)]
        [InlineData("0.00000000000000000001", 1e-20)]
        [InlineData("1e-40", 1e-40)]
        public void Issue602_SmallLiteralsSurviveParsing(string input, double expected)
        {
            var actual = input.ToEntity().EvalNumerical().RealPart.EDecimal.ToDouble();
            Assert.NotEqual(0, actual);
            Assert.Equal(1, actual / expected, 6);
        }

        // A rational's decimal form is rounded into the precision context when it is built,
        // so 2^(-1000) flushes to zero and 2^10000 saturates to +oo. Taking the logarithm
        // off that decimal reported -oo / +oo for answers that are ordinary numbers.
        [Theory]
        [InlineData("ln(2 ^ 1000) / ln(2 ^ (-1000))")]
        [InlineData("ln(2 ^ 10000) / ln(2 ^ (-10000))")]
        public void Issue210_LogarithmOfExtremePowersIsNotInfinite(string input) =>
            Assert.Equal(-1, input.ToEntity().EvalNumerical().RealPart.EDecimal.ToDouble(), 10);

        [Theory]
        [InlineData("ln(2 ^ (-1000))", -693.14718055994531)]
        [InlineData("ln(2 ^ 10000)", 6931.4718055994531)]
        [InlineData("ln(2 ^ (-10000))", -6931.4718055994531)]
        public void Issue210_LogarithmOfExtremePowersIsAccurate(string input, double expected) =>
            Assert.Equal(1, input.ToEntity().EvalNumerical().RealPart.EDecimal.ToDouble() / expected, 10);

        // The ordinary logarithm paths must be untouched by the above.
        [Theory]
        [InlineData("log(2, 8)", 3)]
        [InlineData("log(10, 1000)", 3)]
        [InlineData("ln(e)", 1)]
        [InlineData("ln(1)", 0)]
        public void Issue210_OrdinaryLogarithmsAreUnaffected(string input, double expected) =>
            Assert.Equal(expected, input.ToEntity().EvalNumerical().RealPart.EDecimal.ToDouble(), 12);

        // The whole point of the downcasting tolerance is that residuals left by exact
        // cancellation still land on 0. Those sit around 1e-99, so tightening the
        // tolerance must not disturb them.
        [Theory]
        [InlineData("sin(pi)")]
        [InlineData("cos(pi / 2)")]
        [InlineData("sqrt(2) ^ 2 - 2")]
        [InlineData("e ^ ln(5) - 5")]
        public void Issue602_ExactCancellationStillReachesZero(string input) =>
            Assert.Equal(Entity.Number.Integer.Create(0), input.ToEntity().EvalNumerical());

        // An explicitly widened tolerance must still be honoured verbatim.
        [Fact]
        public void Issue602_ExplicitToleranceIsStillRespected()
        {
            using var _ = MathS.Settings.PrecisionErrorZeroRange.Set(EDecimal.Create(1, -10));
            Assert.Equal(Entity.Number.Integer.Create(0), "1e-20".ToEntity().EvalNumerical());
        }

        // https://github.com/asc-community/AngouriMath/issues/561
        // The reporter's 4x4 matrix spans 1e5 to 1e50, so entries of its inverse fall
        // below 1e-16 and downcasting rounded them to zero. Fixed by the same change as
        // https://github.com/asc-community/AngouriMath/issues/602;
        // pinned here because the matrix path reaches it by its own route.
        [Fact]
        public void Issue561_LargeValuedMatrixInvertsExactly()
        {
            var matrix = MathS.Matrix(new Entity[4, 4]
            {
                { 112085.589993654798036004649475216865539550781250, 637642819921.509799957275390625, 20692733745923116440.0, 985012471441596398530723840.0 },
                { 637642819921.509799957275390625, 20692733745923116440.0, 985012471441596398530723840.0, 53035823659846109665657533505732608.0 },
                { 20692733745923116440.0, 985012471441596398530723840.0, 53035823659846109665657533505732608.0, 3088144109088191970827418502918634458841088.0 },
                { 985012471441596398530723840.0, 53035823659846109665657533505732608.0, 3088144109088191970827418502918634458841088.0, 190454802288659943182846234171424053458511757049856.0 }
            });
            var product = (Entity.Matrix)(matrix * matrix.Inverse).InnerSimplified;
            Assert.Equal(MathS.IdentityMatrix(4), product);
        }

        // Tightening the tolerance above stops Newton's own noise from being rounded away
        // with it. The iteration works in double precision and starts from a grid, so the
        // same root reached from different starting points differs in the last digits;
        // left alone, the one real root of this quintic came back as four different
        // complex numbers with imaginary parts around 1e-19.
        [Fact]
        public void Issue602_NewtonRootsDoNotCarryIterationNoise()
        {
            var roots = "x ^ 5 + 3 * x + 1 = 0".ToEntity().Solve("x");
            var finite = Assert.IsType<Entity.Set.FiniteSet>(roots);
            Assert.Contains(finite, root => root is Entity.Number.Real);
            Assert.DoesNotContain(finite, root =>
                root is Entity.Number.Complex complex and not Entity.Number.Real
                && complex.ImaginaryPart.EDecimal.Abs().ToDouble() < 1e-15);
        }
    }
}
