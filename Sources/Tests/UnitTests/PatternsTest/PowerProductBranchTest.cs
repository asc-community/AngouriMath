//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.PatternsTest
{
    /// <summary>
    /// <c>{1}^n * {2}^n = ({1} * {2})^n</c> was applied for every exponent, and it is false
    /// across the branch cuts: <c>sqrt(-1) * sqrt(-1)</c> is <c>i * i = -1</c> while
    /// <c>sqrt((-1)(-1))</c> is <c>sqrt(1) = 1</c>.
    /// https://github.com/asc-community/AngouriMath/issues/801
    /// </summary>
    /// <remarks>
    /// The same mistake as https://github.com/asc-community/AngouriMath/issues/752, in two
    /// rules that sit immediately below the one #752 fixed and were not looked at then.
    /// </remarks>
    [Trait("Area", "PatternsTest")]
    public sealed class PowerProductBranchTest
    {
        /// <summary>
        /// The property, checked where a branch error shows: both bases negative. Compared
        /// against the expression it came from rather than against an expected form, since
        /// what matters is that simplifying did not change the value.
        /// </summary>
        [Theory]
        [InlineData("x ^ (1/2) * y ^ (1/2)")]
        [InlineData("sqrt(x) * sqrt(y)")]
        [InlineData("sqrt(x) / sqrt(y)")]
        [InlineData("x ^ (1/3) * y ^ (1/3)")]
        [InlineData("x ^ (3/2) * y ^ (3/2)")]
        public void SimplifyingKeepsTheValueAtNegativeBases(string expr)
        {
            var simplified = expr.ToEntity().Simplify();
            foreach (var (x, y) in new[] { (-1.0, -1.0), (-2.0, -3.0), (-0.5, -4.0), (-1.5, 2.5) })
            {
                var before = expr.ToEntity().Substitute("x", x).Substitute("y", y).EvalNumerical();
                var after = simplified.Substitute("x", x).Substitute("y", y).EvalNumerical();
                var difference = Math.Abs(before.RealPart.EDecimal.ToDouble() - after.RealPart.EDecimal.ToDouble())
                               + Math.Abs(before.ImaginaryPart.EDecimal.ToDouble() - after.ImaginaryPart.EDecimal.ToDouble());
                Assert.True(difference < 1e-9,
                    $"{expr} simplified to {simplified.Stringize()}, which at x = {x}, y = {y} "
                    + $"is {after.Stringize()} rather than {before.Stringize()}");
            }
        }

        /// <summary>
        /// The quotient form had the same hole -- <c>sqrt(2) / sqrt(-3)</c> is
        /// <c>-0.8165i</c> where <c>(2 / -3)^(1/2)</c> is <c>+0.8165i</c> -- and is fixed
        /// too. It was left out of the first pass because guarding it cost *answers*: the
        /// gathering was what let the limit machinery read a <c>1^oo</c> out of a quotient.
        /// The limit reader now recognises the quotient itself, where the identity is
        /// checkable, so the guard costs nothing.
        /// https://github.com/asc-community/AngouriMath/issues/802
        /// </summary>
        [Fact]
        public void TheQuotientFormKeepsItsValueToo()
        {
            var simplified = "sqrt(x) / sqrt(y)".ToEntity().Simplify();
            var before = "sqrt(x) / sqrt(y)".ToEntity().Substitute("x", 2).Substitute("y", -3).EvalNumerical();
            var after = simplified.Substitute("x", 2).Substitute("y", -3).EvalNumerical();
            Assert.True(
                Math.Abs(before.ImaginaryPart.EDecimal.ToDouble() - after.ImaginaryPart.EDecimal.ToDouble()) < 1e-9,
                $"sqrt(x) / sqrt(y) simplified to {simplified.Stringize()}, which at x = 2, y = -3 "
                + $"is {after.Stringize()} rather than {before.Stringize()}");
        }

        /// <summary>
        /// The rule must keep firing where it is sound, or this would be a fix that removes
        /// an answer. A whole exponent is safe whatever the signs, and positive bases are
        /// safe whatever the exponent.
        /// </summary>
        [Theory]
        [InlineData("sqrt(2) * sqrt(3)", "sqrt(6)")]
        [InlineData("x ^ 2 * y ^ 2", "(x * y) ^ 2")]
        [InlineData("x ^ 3 * y ^ 3", "(x * y) ^ 3")]
        public void TheSoundCasesStillGather(string expr, string expected)
        {
            var simplified = expr.ToEntity().Simplify();
            Assert.Equal(expected.ToEntity().Simplify(), simplified);
        }

        /// <summary>
        /// A third leak of the same identity: the polynomial parser read <c>(u^2)^(3/2)</c> as
        /// <c>u^3</c>, and through the long-division rule <c>(u^2)^(3/2)/u</c> simplified to
        /// <c>u^2</c> and <c>sqrt(u^2)/(-u)</c> to <c>-1</c>, wrong for every negative <c>u</c>.
        /// An odd inner power is not read either: <c>(u^3)^(3/2)</c> is <c>i</c> at <c>u = -1</c>
        /// and <c>u^(9/2)</c> is <c>-i</c> there. Checked at negative and positive values
        /// against the expression it came from.
        /// https://github.com/asc-community/AngouriMath/issues/1387
        /// </summary>
        [Theory]
        [InlineData("(u^2)^(3/2)/u")]
        [InlineData("sqrt(u^2)/(-u)")]
        [InlineData("(u^2)^(5/2)/u^3")]
        [InlineData("(u^4)^(1/2)*u")]
        [InlineData("(u^3)^(3/2)/u")]
        [InlineData("(u^3)^(1/2)/u")]
        public void ARootOfAnEvenPowerKeepsItsValue(string expr)
        {
            var simplified = expr.ToEntity().Simplify();
            foreach (var u in new[] { -2.0, -0.5, 0.7, 3.0 })
            {
                var before = expr.ToEntity().Substitute("u", u).EvalNumerical();
                var after = simplified.Substitute("u", u).EvalNumerical();
                var difference = Math.Abs(before.RealPart.EDecimal.ToDouble() - after.RealPart.EDecimal.ToDouble())
                               + Math.Abs(before.ImaginaryPart.EDecimal.ToDouble() - after.ImaginaryPart.EDecimal.ToDouble());
                Assert.True(difference < 1e-9,
                    $"{expr} simplified to {simplified.Stringize()}, which at u = {u} is {after.Stringize()} rather than {before.Stringize()}");
            }
        }

        /// <summary>A whole outer power still multiplies, whatever the sign of the base.</summary>
        [Theory]
        [InlineData("(u^3)^2/u", "u^5")]
        [InlineData("(u^2)^3/u", "u^5")]
        public void AWholeOuterPowerStillMultiplies(string expr, string expected)
        {
            var simplified = expr.ToEntity().Simplify();
            var bare = simplified is Entity.Providedf(var inner, _) ? inner : simplified;
            Assert.Equal(expected.ToEntity().Simplify(), bare.Simplify());
        }
    }
}
