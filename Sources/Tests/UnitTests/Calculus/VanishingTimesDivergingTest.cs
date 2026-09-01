//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Extensions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// A product of something vanishing with something diverging, where the product also carries
    /// a factor that is neither. The children of a product arrive flattened through any division
    /// in it, so those leftovers can include a reciprocal, and multiplying one of those back on
    /// top rebuilt the very product the rewrite exists to take apart -- the quotient simplified
    /// straight back and the rule was handed its own input. Split into the denominator instead,
    /// x * ln(x) / cos(x) is answered rather than run at until it times out.
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class VanishingTimesDivergingTest
    {
        private static void AssertLimit(string expression, string expected)
        {
            var task = Task.Run(() =>
                expression.ToEntity().Limit("x", 0, ApproachFrom.Right).Simplify());
            Assert.True(task.Wait(LimitTermination.Guard), "the limit did not terminate");
            Assert.Equal(expected.ToEntity().Evaled, task.Result.Evaled);
        }

        [Theory]
        [InlineData("x * ln(x) / cos(x)", "0")]
        [InlineData("x * ln(x) / (1 + x)", "0")]
        [InlineData("sqrt(x) * ln(x) / (2 + x)", "0")]
        public void ALeftoverReciprocalGoesUnderTheLine(string expression, string expected) =>
            AssertLimit(expression, expected);

        /// <summary>The plain forms, which have no leftover at all, are unaffected.</summary>
        [Theory]
        [InlineData("x * ln(x)", "0")]
        [InlineData("sin(x) * ln(x)", "0")]
        [InlineData("sqrt(x) * ln(x)", "0")]
        [InlineData("x * cotan(x)", "1")]
        [InlineData("(1 - cos(x)) * cotan(x)", "0")]
        public void ThePlainFormsAreUnaffected(string expression, string expected) =>
            AssertLimit(expression, expected);
    }
}
