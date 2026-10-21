//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// A limit at -oo is read at +oo with -x written for x, and the logarithms a limit gathers are
    /// gathered on the signs their operands hold on the approach. Those signs have to be read on
    /// the approach the rewritten expression is on: read against -oo, -x and 1 - x are positive,
    /// <c>ln(-x) + ln(1 - x)</c> was gathered into one logarithm, and
    /// <c>lim x-&gt;-oo ln(x) + ln(x + 1) - ln(x^2 + x)</c> came back 0. Both operands are negative
    /// there, so each logarithm carries <c>i pi</c> and the limit is <c>2 pi i</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/2022">#2022</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class LogarithmsGatheredOnTheirOwnApproachTest
    {
        [Theory]
        [InlineData("ln(x) + ln(x + 1) - ln(x^2 + x)", "-oo", "2 * i * pi")]
        [InlineData("ln(x) + ln(x - 1) - ln(x^2 - x)", "-oo", "2 * i * pi")]
        [InlineData("(ln(x) + ln(x + 1) - ln(x^2 + x)) * x / (x + 1)", "-oo", "2 * i * pi")]
        [InlineData("(ln(x) + ln(x + 1) - ln(x^2 + x))^2", "-oo", "-4 * pi^2")]
        [InlineData("ln(-x) + ln(1 - x) - ln(x^2 - x)", "+oo", "2 * i * pi")]
        // Where the operands are positive the logarithms do gather, on either side.
        [InlineData("ln(-x) + ln(-x - 1) - ln(x^2 + x)", "-oo", "0")]
        [InlineData("ln(-x) + ln(1 - x) - ln(x^2 - x)", "-oo", "0")]
        [InlineData("ln(x) + ln(x + 1) - ln(x^2 + x)", "+oo", "0")]
        [InlineData("ln(x^2) - 2 * ln(x)", "-oo", "-2 * i * pi")]
        [InlineData("ln(x - 1) + ln(x - 2) - ln(x^2 - 3 * x + 2)", "0", "2 * i * pi")]
        public void IsTheLimitOfTheLogarithmsAsTheyAre(string expression, string dest, string limit)
        {
            var want = limit.ToEntity().EvalNumerical();
            AssertIs(want, expression.ToEntity().Limit("x", dest), expression, dest, "Limit");
            AssertIs(want, $"limit({expression}, x, {dest})".ToEntity().Simplify(), expression, dest, "Simplify");
        }

        private static void AssertIs(Entity.Number.Complex want, Entity limit, string expression, string dest, string how)
        {
            Assert.DoesNotContain("limit(", limit.Stringize());
            var got = limit.EvalNumerical();
            var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
            Assert.True(difference < 1e-9, $"{how}: lim x->{dest} {expression} is {limit} = {got}, where it is {want}");
        }
    }
}
