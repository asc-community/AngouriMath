//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;
using AngouriMath.Numerics;
using PeterO.Numbers;
using Xunit;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// The interval evaluation holds the value it stands for -- the hundred-digit decimal
    /// evaluation's, inside its bounds -- and is narrow where nothing cancels, declines what it
    /// does not read, and compares in three outcomes.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1019">#1019</a>
    /// </summary>
    public sealed class IntervalEvaluationTest
    {
        [Theory]
        [InlineData("sqrt(2)")]
        [InlineData("pi")]
        [InlineData("e^2")]
        [InlineData("sin(1)")]
        [InlineData("cos(1/3)")]
        [InlineData("tan(3/10)")]
        [InlineData("sec(1) + csc(2) + cot(3)")]
        [InlineData("ln(7)")]
        [InlineData("log(2, 10)")]
        [InlineData("arctan(2)")]
        [InlineData("arcsin(1/3)")]
        [InlineData("arccos(-2/3)")]
        [InlineData("(2/3)^(1/3)")]
        [InlineData("(1 + 2i)^3")]
        [InlineData("1/(3 + 4i)")]
        [InlineData("e^(i * pi/4)")]
        [InlineData("sqrt(-3)")]
        [InlineData("sin(1 + i)")]
        [InlineData("cos(2 - 3i)")]
        [InlineData("ln(-2 + i)")]
        [InlineData("ln(-2)")]
        [InlineData("abs(-3 + 4i)")]
        [InlineData("(sin(1)^2 + cos(1)^2) * 7/3")]
        [InlineData("sqrt(2) provided 1 < 2")]
        [InlineData("piecewise(1 provided 2 < 1, sin(1) provided 3 > 2)")]
        public void TheIntervalHoldsTheValue(string expression)
        {
            var expr = expression.ToEntity();
            var interval = IntervalEvaluation.Of(expr);
            Assert.True(interval.HasValue, $"{expression} was not evaluated");
            var value = expr.EvalNumerical();
            Holds(interval!.Value.Re, value.RealPart.EDecimal, expression, "real");
            Holds(interval.Value.Im, value.ImaginaryPart.EDecimal, expression, "imaginary");
        }

        private static void Holds(Interval interval, EDecimal value, string expression, string part)
        {
            Assert.True(EDecimal.FromDouble(interval.Low).CompareTo(value) <= 0 && EDecimal.FromDouble(interval.High).CompareTo(value) >= 0,
                $"the {part} part of {expression}, {value}, is outside [{interval.Low:R}, {interval.High:R}]");
            var scale = System.Math.Max(1.0, interval.Magnitude);
            Assert.True(interval.High - interval.Low <= 1e-12 * scale,
                $"the {part} part of {expression} is [{interval.Low:R}, {interval.High:R}], wider than it has to be");
        }

        /// <summary>
        /// Declined: a variable, a pole, the logarithm of zero, a node the pilot does not read,
        /// and a value whose condition fails.
        /// </summary>
        [Theory]
        [InlineData("x + 1")]
        [InlineData("1/0")]
        [InlineData("ln(0)")]
        [InlineData("tan(pi/2)")]
        [InlineData("3!")]
        [InlineData("1 provided 3 < 2")]
        public void UndecidedWhereItShouldBe(string expression)
            => Assert.Null(IntervalEvaluation.Of(expression.ToEntity()));

        /// <summary>
        /// Where cancellation leaves double intervals too wide to tell, forty digits tell: the
        /// hundred million cancels and leaves the sine, which the doubles hold only to within a
        /// few hundred-millionths.
        /// </summary>
        [Fact]
        public void MoreDigitsSettleWhatDoublesCannot()
        {
            var cancelled = "(10^8 + sin(1)) - 10^8".ToEntity();
            var sine = "sin(1)".ToEntity();
            Assert.Null(IntervalEvaluation.Agree(IntervalEvaluation.Of(cancelled)!.Value, IntervalEvaluation.Of(sine)!.Value, 1e-9));
            Assert.True(PreciseEvaluation.Agree(PreciseEvaluation.Of(cancelled, 40)!.Value, PreciseEvaluation.Of(sine, 40)!.Value, 1e-9));
            var nearby = "sin(1) + 1/1000000".ToEntity();
            Assert.False(PreciseEvaluation.Agree(PreciseEvaluation.Of(nearby, 40)!.Value, PreciseEvaluation.Of(sine, 40)!.Value, 1e-9));
        }

        [Theory]
        [InlineData("sqrt(2)")]
        [InlineData("e^2 - pi")]
        [InlineData("sin(1 + i) * cos(2 - 3i)")]
        [InlineData("ln(-2 + i) + arctan(2) - arcsin(1/3)")]
        [InlineData("(2/3)^(1/3) + sec(1) + csc(2)")]
        [InlineData("sqrt(2) provided 1 < 2")]
        public void ThePreciseIntervalHoldsTheValue(string expression)
        {
            var expr = expression.ToEntity();
            var interval = PreciseEvaluation.Of(expr, 40);
            Assert.True(interval.HasValue, $"{expression} was not evaluated");
            var value = expr.EvalNumerical();
            Assert.True(interval!.Value.Re.Low.CompareTo(value.RealPart.EDecimal) <= 0 && interval.Value.Re.High.CompareTo(value.RealPart.EDecimal) >= 0,
                $"the real part of {expression}, {value.RealPart}, is outside [{interval.Value.Re.Low}, {interval.Value.Re.High}]");
            Assert.True(interval.Value.Im.Low.CompareTo(value.ImaginaryPart.EDecimal) <= 0 && interval.Value.Im.High.CompareTo(value.ImaginaryPart.EDecimal) >= 0,
                $"the imaginary part of {expression}, {value.ImaginaryPart}, is outside [{interval.Value.Im.Low}, {interval.Value.Im.High}]");
            Assert.True(interval.Value.Re.High.Subtract(interval.Value.Re.Low).Abs().CompareTo(EDecimal.FromString("1E-30")) <= 0,
                $"the real part of {expression} is wider than forty digits should leave it");
        }

        [Fact]
        public void AgreementHasThreeOutcomes()
        {
            var one = IntervalEvaluation.Of("1".ToEntity())!.Value;
            var identity = IntervalEvaluation.Of("sin(1)^2 + cos(1)^2".ToEntity())!.Value;
            var nearby = IntervalEvaluation.Of("1 + 1/1000000".ToEntity())!.Value;
            Assert.True(IntervalEvaluation.Agree(identity, one, 1e-9));
            Assert.False(IntervalEvaluation.Agree(nearby, one, 1e-9));
            var wide = ComplexInterval.Real(new Interval(0.9, 1.1));
            Assert.Null(IntervalEvaluation.Agree(wide, one, 1e-9));
        }
    }
}
