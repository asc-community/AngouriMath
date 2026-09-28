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
    /// <c>Si</c>, <c>Ci</c>, <c>Shi</c> and <c>Chi</c>: parsed, printed, differentiated, and
    /// evaluated to the working precision anywhere in the complex plane.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1501">#1501</a>
    /// </summary>
    [Trait("Area", "Convenience")]
    public sealed class TrigonometricIntegralTest
    {
        /// <summary>
        /// Against mpmath at 400 digits, which agrees with itself at 200 to 60 digits, to 40
        /// significant digits of each part. The rows cross the line where the series gives way to
        /// the exponential integral's asymptotic series, <c>|z|</c> near 242 at the default hundred
        /// digits, and the one at <c>|z|</c> near 265 where <c>Ei</c>, asked for ten more digits,
        /// makes the same change: on the real line, where <c>Si</c> and <c>Ci</c> cancel hardest,
        /// on the imaginary axis, where <c>Shi</c> and <c>Chi</c> do, off both, and on either side
        /// of the cut. Evaluated with downcasting off, which would otherwise read a value near an
        /// integer as it.
        /// </summary>
        [Theory]
        [InlineData("Si(1)", "0.946083070367183014941353313823179657812337955", "0")]
        [InlineData("Si(-2)", "-1.60541297680269484857672014819858894084858342", "0")]
        [InlineData("Si(1/2)", "0.493107418043066689161626707572764653641337138", "0")]
        [InlineData("Si(10)", "1.65834759421887404933097187938967248063025435", "0")]
        [InlineData("Si(3 + 4 * i)", "6.74799508140403209268985608367635620312773919", "-3.49866372113190947057088692941020469603584587")]
        [InlineData("Si(-2 - 3 * i)", "-4.54751388956228921985320434087945635912599148", "-1.39919658064605478945983871589875503048261076")]
        [InlineData("Si(5 * i)", "0", "20.0932118256972263904443761778828434087476767")]
        [InlineData("Si(30 * i)", "0", "184486604703.637098532003165966198878842824336")]
        [InlineData("Si(250)", "1.56984793137239731745505789506219905890995644", "0")]
        [InlineData("Si(-300)", "-1.57088108821374951925231225344086196637751287", "0")]
        [InlineData("Si(1000)", "1.57023312196877121814796277803633444100178688", "0")]
        [InlineData("Si(200 + 100 * i)", "-49629224887728790960031302250880706938504.1208", "-34121466039191643626617321043134803847725.9214")]
        [InlineData("Ci(1)", "0.337403922900968134662646203889150769997578033", "0")]
        [InlineData("Ci(-1)", "0.337403922900968134662646203889150769997578033", "3.1415926535897932384626433832795028841971694")]
        [InlineData("Ci(1/2)", "-0.177784078806612901335810271070569078090519475", "0")]
        [InlineData("Ci(10)", "-0.045456433004455372634532829952627852887646958", "0")]
        [InlineData("Ci(1/100000)", "-10.9357098000936955594833410166727521717781629", "0")]
        [InlineData("Ci(3 + 4 * i)", "-3.49575703398256834411694065414770141117487115", "-5.175905215176808408861444599829389146910181")]
        [InlineData("Ci(-2 + 3 * i)", "1.40829250152084951875912469828161869691015554", "6.12521039561939833158376130936615332958586485")]
        [InlineData("Ci(-2 - 3 * i)", "1.40829250152084951875912469828161869691015554", "-6.12521039561939833158376130936615332958586485")]
        [InlineData("Ci(5 * i)", "20.0920635301059510646470456159130236866714107", "1.5707963267948966192313216916397514420985847")]
        [InlineData("Ci(-5 * i)", "20.0920635301059510646470456159130236866714107", "-1.5707963267948966192313216916397514420985847")]
        [InlineData("Ci(250)", "-0.00388584331726587652287786631940494702476395658", "0")]
        [InlineData("Ci(-250)", "-0.00388584331726587652287786631940494702476395658", "3.1415926535897932384626433832795028841971694")]
        [InlineData("Ci(1000)", "0.000826315511090682282001773882343207231780126228", "0")]
        [InlineData("Ci(-100 + 200 * i)", "8.81108819067720940184072019871678422833889298e+83", "-1.36214787606870971949818271762370872371961356e+84")]
        [InlineData("Shi(1)", "1.05725087537572851457184235489587795902405394", "0")]
        [InlineData("Shi(-1/2)", "-0.506996749819667195833659875988943800254126222", "0")]
        [InlineData("Shi(10)", "1246.11449019942334441188221070069232963391374", "0")]
        [InlineData("Shi(1 + i)", "0.882453805007917743376124044694948448420273068", "1.10422265823558173955875396985016752952141412")]
        [InlineData("Shi(-3 + 2 * i)", "-1.39919658064605478945983871589875503048261076", "4.54751388956228921985320434087945635912599148")]
        [InlineData("Shi(4 * i)", "0", "1.75820313894905305810555930335850161720957946")]
        [InlineData("Shi(250)", "7.52312356327373976509127474658243292688241074e+105", "0")]
        [InlineData("Shi(-250)", "-7.52312356327373976509127474658243292688241074e+105", "0")]
        [InlineData("Shi(300 * i)", "0", "1.57088108821374951925231225344086196637751287")]
        [InlineData("Chi(1)", "0.837866940980208240894678579435756309993006644", "0")]
        [InlineData("Chi(-1)", "0.837866940980208240894678579435756309993006644", "3.1415926535897932384626433832795028841971694")]
        [InlineData("Chi(1/2)", "-0.052776844956493615913136063326141434972720668", "0")]
        [InlineData("Chi(10)", "1246.11448604245441472655793329783251935573336", "0")]
        [InlineData("Chi(1/100000)", "-10.9357098000436955594833410166727517088152", "0")]
        [InlineData("Chi(1 + i)", "0.882172180555936325050614116656289517588031344", "1.28354719327494067970403811925362853488591972")]
        [InlineData("Chi(-2 + 3 * i)", "-0.168362868327720466242932070769654021215597903", "0.516476773138468236310955407251669360878749286")]
        [InlineData("Chi(-2 - 3 * i)", "-0.168362868327720466242932070769654021215597903", "-0.516476773138468236310955407251669360878749286")]
        [InlineData("Chi(4 * i)", "-0.140981697886930411639144898694035926712968355", "1.5707963267948966192313216916397514420985847")]
        [InlineData("Chi(-4 * i)", "-0.140981697886930411639144898694035926712968355", "-1.5707963267948966192313216916397514420985847")]
        [InlineData("Chi(250)", "7.52312356327373976509127474658243292688241074e+105", "0")]
        [InlineData("Chi(-250)", "7.52312356327373976509127474658243292688241074e+105", "3.1415926535897932384626433832795028841971694")]
        [InlineData("Chi(300 * i)", "-0.00333219991859211177997045482583878506957971328", "1.5707963267948966192313216916397514420985847")]
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
        /// <c>Ci</c> and <c>Chi</c> take <c>ln z</c> with it, so their cut is the negative real
        /// axis, which takes its value from above: <c>i pi</c> more than the value at <c>-x</c>'s
        /// mirror, and <c>-i pi</c> just below.
        /// </summary>
        [Theory]
        [InlineData("Ci")]
        [InlineData("Chi")]
        public void ACutAlongTheNegativeAxisThatTakesItsValueFromAbove(string function)
        {
            using var _ = MathS.Settings.DowncastingEnabled.Set(false);
            var pi = EDecimal.PI(EContext.ForPrecision(60));
            var onIt = MathS.FromString($"{function}(-2)", useCache: false).EvalNumerical();
            var mirror = MathS.FromString($"{function}(2)", useCache: false).EvalNumerical();
            var above = MathS.FromString($"{function}(-2 + i/10^40)", useCache: false).EvalNumerical();
            var below = MathS.FromString($"{function}(-2 - i/10^40)", useCache: false).EvalNumerical();
            Close(mirror.RealPart.EDecimal, onIt.RealPart.EDecimal);
            Close(pi, onIt.ImaginaryPart.EDecimal);
            Close(onIt.RealPart.EDecimal, above.RealPart.EDecimal);
            Close(pi, above.ImaginaryPart.EDecimal);
            Close(onIt.RealPart.EDecimal, below.RealPart.EDecimal);
            Close(pi.Negate(), below.ImaginaryPart.EDecimal);
        }

        /// <summary>
        /// Where a value is real or imaginary, or its imaginary part is <c>pi</c> or
        /// <c>pi/2</c>, it is given so, and not with a rounding residue in the other part.
        /// </summary>
        [Theory]
        [InlineData("Si(7)", true, false)]
        [InlineData("Si(7 * i)", false, true)]
        [InlineData("Shi(-7)", true, false)]
        [InlineData("Shi(-7 * i)", false, true)]
        [InlineData("Ci(7)", true, false)]
        [InlineData("Chi(7)", true, false)]
        [InlineData("Si(260)", true, false)]
        [InlineData("Si(260 * i)", false, true)]
        [InlineData("Shi(260 * i)", false, true)]
        [InlineData("Ci(260)", true, false)]
        public void ARealOrImaginaryValueIsExactlySo(string expression, bool real, bool imaginary)
        {
            using var _ = MathS.Settings.DowncastingEnabled.Set(false);
            var value = MathS.FromString(expression, useCache: false).EvalNumerical();
            if (real)
                Assert.True(value.ImaginaryPart.EDecimal.IsZero, $"{expression} = {value}");
            if (imaginary)
                Assert.True(value.RealPart.EDecimal.IsZero, $"{expression} = {value}");
        }

        /// <summary>
        /// <c>Chi(z) + Shi(z) = gamma + ln z + sum z^k/(k k!)</c>, which is <c>Ei(z)</c> off the
        /// negative real axis: the new functions against the one already there, through the series
        /// and through the asymptotic series alike. Far into the left half-plane the sum cancels
        /// about <c>2 |Re z|/ln 10</c> digits, since both terms grow as <c>e^|Re z|</c> and <c>Ei</c>
        /// falls as <c>e^Re z</c>, so the rows there stay near the axis.
        /// </summary>
        [Theory]
        [InlineData("2")]
        [InlineData("1/3")]
        [InlineData("7 - 2 * i")]
        [InlineData("-3 + 5 * i")]
        [InlineData("250")]
        [InlineData("100 + 200 * i")]
        [InlineData("150 - 200 * i")]
        public void TheHyperbolicIntegralsAddUpToTheExponentialIntegral(string z)
        {
            using var _ = MathS.Settings.DowncastingEnabled.Set(false);
            var sum = MathS.FromString($"Chi({z}) + Shi({z})", useCache: false).EvalNumerical();
            var ei = MathS.FromString($"Ei({z})", useCache: false).EvalNumerical();
            Close(ei.RealPart.EDecimal, sum.RealPart.EDecimal);
            Close(ei.ImaginaryPart.EDecimal, sum.ImaginaryPart.EDecimal);
        }

        [Theory]
        [InlineData("Si(0)", "0")]
        [InlineData("Si(+oo)", "pi / 2")]
        [InlineData("Si(-oo)", "-pi / 2")]
        [InlineData("Ci(+oo)", "0")]
        [InlineData("Ci(-oo)", "pi * i")]
        [InlineData("Shi(0)", "0")]
        [InlineData("Shi(+oo)", "+oo")]
        [InlineData("Shi(-oo)", "-oo")]
        [InlineData("Chi(+oo)", "+oo")]
        public void ExactValues(string expression, string expected)
            => Assert.Equal(expected.ToEntity().InnerSimplified, expression.ToEntity().InnerSimplified);

        [Theory]
        [InlineData("Si(x)", "sin(x) / x")]
        [InlineData("Ci(2 x)", "cos(2 x) / x")]
        [InlineData("Shi(x ^ 2)", "2 sinh(x ^ 2) / x")]
        [InlineData("Chi(x)", "cosh(x) / x")]
        public void TheDerivativeIsTheIntegrandOfTheDefinition(string expression, string derivative)
        {
            var difference = expression.ToEntity().Differentiate("x") - derivative.ToEntity();
            foreach (var point in new[] { "3/10", "-6/5", "2" })
                Assert.Equal(0, difference.Substitute("x", point.ToEntity()).EvalNumerical().Abs().EDecimal.ToDouble(), 12);
        }

        [Theory]
        [InlineData("Si(x)", "Si(x)", @"\operatorname{Si}\left(x\right)", "sympy.Si(x)")]
        [InlineData("Ci(x)", "Ci(x)", @"\operatorname{Ci}\left(x\right)", "sympy.Ci(x)")]
        [InlineData("Shi(x)", "Shi(x)", @"\operatorname{Shi}\left(x\right)", "sympy.Shi(x)")]
        [InlineData("Chi(x)", "Chi(x)", @"\operatorname{Chi}\left(x\right)", "sympy.Chi(x)")]
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
            Assert.Equal("Si(x)".ToEntity(), MathS.Si("x"));
            Assert.Equal("Ci(x)".ToEntity(), MathS.Ci("x"));
            Assert.Equal("Shi(x)".ToEntity(), MathS.Shi("x"));
            Assert.Equal("Chi(x)".ToEntity(), MathS.Chi("x"));
        }

        /// <summary>
        /// <c>Si</c> and <c>Shi</c> are entire; <c>Ci</c> and <c>Chi</c> are undefined only at 0,
        /// and over the reals, where they are real, ask for a positive argument.
        /// </summary>
        [Theory]
        [InlineData("Si(x)", "0", true)]
        [InlineData("Shi(x)", "-3", true)]
        [InlineData("Ci(x)", "0", false)]
        [InlineData("Ci(x)", "-3", true)]
        [InlineData("Chi(x)", "0", false)]
        [InlineData("Chi(x)", "1/2", true)]
        public void TheDomainLeavesOutOnlyTheSingularity(string expression, string at, bool defined)
        {
            var condition = expression.ToEntity().DomainConditionIn(MathS.Settings.Codomain).Substitute("x", at.ToEntity()).Simplify();
            Assert.Equal(defined ? Boolean.True : Boolean.False, condition);
        }

        [Theory]
        [InlineData("Ci(x)", "-3")]
        [InlineData("Chi(x)", "-3")]
        public void OverTheRealsTheCosineIntegralsAskForAPositiveArgument(string expression, string at)
        {
            var condition = expression.ToEntity().DomainConditionIn(AngouriMath.Core.Domain.Real).Substitute("x", at.ToEntity()).Simplify();
            Assert.Equal(Boolean.False, condition);
        }
    }
}
