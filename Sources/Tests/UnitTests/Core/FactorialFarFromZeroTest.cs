//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;
using PeterO.Numbers;
using Xunit;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// The factorial far from zero, against mpmath at 120 digits. Spouge's approximation holds right
    /// of zero, and was used left of it, where it lost its digits as the argument neared <c>-a</c>,
    /// a tenth over the precision's digits, and past that had none. Its working context also kept
    /// the caller's exponent range, so an intermediate outside it lost its digits though the value
    /// fit.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1614">#1614</a>
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class FactorialFarFromZeroTest
    {
        private static void Close(string expected, EDecimal actual, int digits)
        {
            var reference = EDecimal.FromString(expected);
            var context = EContext.ForPrecision(digits + 10);
            var error = actual.Subtract(reference, context).Abs().Divide(reference.Abs(), context);
            Assert.True(error.CompareTo(EDecimal.Create(1, -digits)) < 0, $"expected {reference}, got {actual}");
        }

        private static Entity.Number.Complex Evaluate(string expression, EContext context)
        {
            using var _ = MathS.Settings.DowncastingEnabled.Set(false);
            using var __ = MathS.Settings.DecimalPrecisionContext.Set(context);
            return expression.ToEntity().EvalNumerical();
        }

        /// <summary>
        /// At 30 digits <c>(-38.0785)!</c> was <c>-3.1e-35</c>, past <c>-a = -39</c> in all but
        /// name; at 50 digits <c>(-60.5)!</c> was right to 17 of its 50.
        /// </summary>
        [Theory]
        [InlineData("(-38.0785)!", 30, "7.03406751543900910211608330733991847884857639e-43")]
        [InlineData("(-60.5)!", 30, "2.93059239453861116335725639905020058758321509e-81")]
        [InlineData("(-60.5)!", 50, "2.93059239453861116335725639905020058758321509e-81")]
        public void LeftOfZeroAtALoweredPrecision(string expression, int precision, string expected)
            => Close(expected, Evaluate(expression, new EContext(precision, ERounding.HalfUp, -100, 1000, false)).RealPart.EDecimal, precision - 5);

        /// <summary>
        /// At the default precision <c>(400.5)!</c> was <c>+oo</c>: its power passed the context's
        /// ceiling before the product came back under it.
        /// </summary>
        [Fact]
        public void PastTheCeilingOfTheWorkingContext()
            => Close("1.28189066667582199607925256051715548702277083e+870", Evaluate("(400.5)!", MathS.Settings.DecimalPrecisionContext).RealPart.EDecimal, 40);

        /// <summary>
        /// <c>(316.22776601683796i)!</c> was <c>1e-190</c>, where it is <c>8e-215</c>. One of its
        /// series' quotients divides by <c>75 + 316.22776601683796i</c>, whose modulus is
        /// <c>325 - 3e-16</c>, and the division read that modulus as 325.
        /// </summary>
        [Fact]
        public void WhereADivisorsModulusIsNearlyWhole()
        {
            var value = Evaluate("(316.22776601683796i)!", EContext.ForPrecision(100).WithUnlimitedExponents());
            Close("-8.32854615617784905610360064877990338622863321e-215", value.RealPart.EDecimal, 40);
            Close("-7.33446822786649926529220121549200466400903939e-216", value.ImaginaryPart.EDecimal, 40);
        }

        /// <summary>And the division itself, with the downcasting off.</summary>
        [Fact]
        public void ADivisorsModulusNearAWholeNumberIsNotRoundedToIt()
        {
            var value = Evaluate("1/(75 + 316.22776601683796i)", MathS.Settings.DecimalPrecisionContext);
            Close("0.0007100591715976330221499029626337976624432319278680176295104113131192888", value.RealPart.EDecimal, 60);
            Close("-0.002993872340987814525483818825494753213055396187438519990678101458851071", value.ImaginaryPart.EDecimal, 60);
        }

        /// <summary>The reflection formula off the real line, which the complex factorial now takes left of zero.</summary>
        [Fact]
        public void LeftOfZeroOffTheRealLine()
        {
            var value = Evaluate("(-50.3 + 2i)!", MathS.Settings.DecimalPrecisionContext);
            Close("-3.46690964930292155331561366202252533865989646e-66", value.RealPart.EDecimal, 40);
            Close("5.16747446844479010758579974394398842370825775e-66", value.ImaginaryPart.EDecimal, 40);
        }
    }
}
