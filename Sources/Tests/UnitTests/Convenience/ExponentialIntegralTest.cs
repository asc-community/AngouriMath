//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using PeterO.Numbers;
using Xunit;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Tests.Convenience
{
    /// <summary>
    /// <c>Ei</c> and <c>li</c>: parsed, printed, differentiated, and evaluated to the working
    /// precision anywhere in the complex plane, real on the real line.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1501">#1501</a>
    /// </summary>
    [Trait("Area", "Convenience")]
    public sealed class ExponentialIntegralTest
    {
        /// <summary>
        /// Against mpmath at 200 digits, which agrees with itself at 400, to 40 significant digits
        /// of each part. The rows cross the line where the series gives way to the asymptotic
        /// series, <c>|z|</c> near 242 at the default hundred digits: on the positive axis, on the
        /// negative axis where the series cancels hardest, off both, and to either side of the cut.
        /// Evaluated with downcasting off, which would otherwise read a value near an integer as it.
        /// </summary>
        [Theory]
        [InlineData("Ei(1)", "1.89511781635593675546652093433163426901706058", "0")]
        [InlineData("Ei(-1)", "-0.219383934395520273677163775460121649031047293", "0")]
        [InlineData("Ei(1/2)", "0.454219904863173579920523812662802365281405554", "0")]
        [InlineData("Ei(10)", "2492.2289762418777591384401439985248489896471", "0")]
        [InlineData("Ei(-10)", "-0.00000415696892968532427740285981027818038434629008", "0")]
        [InlineData("Ei(-50)", "-3.78326402955045901869896785402128578030289319e-24", "0")]
        [InlineData("Ei(1 + i)", "1.76462598556385406842673816135123796600830441", "2.38776985151052241926279208910379606440733385")]
        [InlineData("Ei(-2 + 3 * i)", "0.0248262079441993629248773807229211028405695629", "3.1619093285008378611294800795824905828539385")]
        [InlineData("Ei(-2 - 3 * i)", "0.0248262079441993629248773807229211028405695629", "-3.1619093285008378611294800795824905828539385")]
        [InlineData("Ei(3 * i)", "0.119629786008000327626472281176677850546836525", "3.41944885479436487562905194275172468726309743")]
        [InlineData("Ei(250)", "1.50462471265474795301825494931648658537648215e+106", "0")]
        [InlineData("Ei(-250)", "-1.06343914395045543733541152348676920552148787e-111", "0")]
        [InlineData("Ei(200 + 100 * i)", "1.7622176381354418803681440397433568456677786e+84", "-2.72429575213741943899636543524741744743922712e+84")]
        [InlineData("Ei(300 * i)", "-0.00333219991859211177997045482583878506957971328", "3.14167741500864613848363394508061340847609756")]
        [InlineData("Ei(-1/1000)", "-6.33153936413614933200278637638633557540146053", "0")]
        [InlineData("Ei(1/100000)", "-10.9356998000436955039277854609505294865926943", "0")]
        [InlineData("li(2)", "1.04516378011749278484458888919461313652261558", "0")]
        [InlineData("li(1/2)", "-0.378671043061087976727207184636560980551234041", "0")]
        [InlineData("li(10)", "6.16559950478729793752298175266952274913060281", "0")]
        [InlineData("li(1000)", "177.609657990152226687640623948699317978557703", "0")]
        [InlineData("li(-1)", "0.0736679120464254859901009652301496718698774623", "3.42273337877736278959237506179774280544439443")]
        [InlineData("li(i)", "0.472000651439568650777606107614127836507330543", "2.94155849494938509930099998002132677208944604")]
        [InlineData("li(3 + 4 * i)", "3.134375550464577526492014568304543970026172", "2.67692478177787423923844623250717722975494117")]
        public void AgreesWithMpmath(string expression, string real, string imaginary)
        {
            using var _ = MathS.Settings.DowncastingEnabled.Set(false);
            var value = MathS.FromString(expression, useCache: false).EvalNumerical();
            Close(EDecimal.FromString(real), value.RealPart.EDecimal);
            Close(EDecimal.FromString(imaginary), value.ImaginaryPart.EDecimal);
        }

        private static void Close(EDecimal expected, EDecimal actual)
        {
            var context = EContext.ForPrecision(60);
            var scale = expected.Abs().CompareTo(EDecimal.Create(1, -300)) > 0 ? expected.Abs() : EDecimal.Create(1, -300);
            var error = actual.Subtract(expected, context).Abs().Divide(scale, context);
            Assert.True(error.CompareTo(EDecimal.Create(1, -40)) < 0, $"expected {expected}, got {actual}");
        }

        /// <summary>
        /// Real on the negative axis, and <c>i pi</c> more just above it: the cut is the negative
        /// real axis, as for <c>ln</c>, with the axis itself on neither side.
        /// </summary>
        [Fact]
        public void ACutAlongTheNegativeAxisThatIsRealOnIt()
        {
            using var _ = MathS.Settings.DowncastingEnabled.Set(false);
            var onIt = MathS.FromString("Ei(-2)", useCache: false).EvalNumerical();
            var above = MathS.FromString("Ei(-2 + i/10^40)", useCache: false).EvalNumerical();
            Assert.True(onIt.ImaginaryPart.EDecimal.IsZero);
            Close(onIt.RealPart.EDecimal, above.RealPart.EDecimal);
            Close(EDecimal.PI(EContext.ForPrecision(60)), above.ImaginaryPart.EDecimal);
        }

        /// <summary><c>li(z) = Ei(ln z)</c>, which is its definition here, off the real line as on it.</summary>
        [Theory]
        [InlineData("2")]
        [InlineData("1/3")]
        [InlineData("-5")]
        [InlineData("2 - 7 * i")]
        public void TheLogarithmicIntegralIsTheExponentialIntegralOfTheLogarithm(string z)
        {
            using var _ = MathS.Settings.DowncastingEnabled.Set(false);
            var li = MathS.FromString($"li({z})", useCache: false).EvalNumerical();
            var ei = MathS.FromString($"Ei(ln({z}))", useCache: false).EvalNumerical();
            Close(ei.RealPart.EDecimal, li.RealPart.EDecimal);
            Close(ei.ImaginaryPart.EDecimal, li.ImaginaryPart.EDecimal);
        }

        [Theory]
        [InlineData("Ei(+oo)", "+oo")]
        [InlineData("Ei(-oo)", "0")]
        [InlineData("li(0)", "0")]
        [InlineData("li(+oo)", "+oo")]
        public void ExactValues(string expression, string expected)
            => Assert.Equal(expected.ToEntity(), expression.ToEntity().InnerSimplified);

        [Theory]
        [InlineData("Ei(x)", "e ^ x / x")]
        [InlineData("Ei(2 x)", "e ^ (2 x) / x")]
        [InlineData("li(x)", "1 / ln(x)")]
        [InlineData("li(x ^ 2)", "2 x / ln(x ^ 2)")]
        public void TheDerivativeIsTheIntegrandOfTheDefinition(string expression, string derivative)
        {
            var difference = expression.ToEntity().Differentiate("x") - derivative.ToEntity();
            foreach (var point in new[] { "3/10", "-6/5", "2" })
                Assert.Equal(0, difference.Substitute("x", point.ToEntity()).EvalNumerical().Abs().EDecimal.ToDouble(), 12);
        }

        [Theory]
        [InlineData("Ei(x)", "Ei(x)", @"\operatorname{Ei}\left(x\right)", "sympy.Ei(x)")]
        [InlineData("li(x)", "li(x)", @"\operatorname{li}\left(x\right)", "sympy.li(x)")]
        public void PrintsAndReadsBack(string written, string printed, string latex, string sympy)
        {
            var entity = written.ToEntity();
            Assert.Equal(printed, entity.ToString());
            Assert.Equal(entity, printed.ToEntity());
            Assert.Equal(latex, entity.Latexize());
            Assert.Contains(sympy, MathS.ToSympyCode(entity));
        }

        [Fact]
        public void TheEntryPointsAgreeWithTheParser()
        {
            Assert.Equal("Ei(x)".ToEntity(), MathS.Ei("x"));
            Assert.Equal("li(x)".ToEntity(), MathS.Li("x"));
        }

        /// <summary>
        /// <c>Ei</c> is undefined only at 0, and <c>li</c> at 1; over the reals, <c>li</c> also asks
        /// for a non-negative argument, whose logarithm is real.
        /// </summary>
        [Theory]
        [InlineData("Ei(x)", "0", false)]
        [InlineData("Ei(x)", "-3", true)]
        [InlineData("li(x)", "1", false)]
        [InlineData("li(x)", "1/2", true)]
        public void TheDomainLeavesOutOnlyTheSingularity(string expression, string at, bool defined)
        {
            var condition = expression.ToEntity().DomainConditionIn(MathS.Settings.Codomain).Substitute("x", at.ToEntity()).Simplify();
            Assert.Equal(defined ? Boolean.True : Boolean.False, condition);
        }
    }
}
