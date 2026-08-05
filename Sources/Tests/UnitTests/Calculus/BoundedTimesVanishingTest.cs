//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core;
using AngouriMath.Extensions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// The squeeze theorem in the one shape that does not need the bounded factor's own limit.
    /// Every rule above this one reads a limit as a value -- the descent substitutes each part's
    /// limit, l'Hopital's rule wants a determinate quotient, Gruntz compares rates of growth --
    /// so a factor with no limit at all, such as sin(x) at infinity, made each of them decline
    /// and left the product indeterminate in the shape (no limit) * 0.
    /// https://github.com/asc-community/AngouriMath/issues/723
    /// </summary>
    public sealed class BoundedTimesVanishingTest
    {
        private static void AssertLimit(string expression, string destination, string expected)
        {
            var task = Task.Run(() =>
                expression.ToEntity().Limit("x", destination.ToEntity()).Simplify());
            Assert.True(task.Wait(TimeSpan.FromSeconds(30)), "the limit did not terminate");
            Assert.Equal(expected.ToEntity().Evaled, task.Result.Evaled);
        }

        /// <summary>
        /// A bounded dividend over a diverging divisor. The standard example of the theorem is
        /// the first of these.
        /// </summary>
        [Theory]
        [InlineData("sin(x) / x", "+oo")]
        [InlineData("cos(x) / x", "+oo")]
        [InlineData("sin(x) / x ^ 2", "+oo")]
        [InlineData("sin(x) / x", "-oo")]
        [InlineData("sin(x ^ 2) / x", "+oo")]
        [InlineData("(2 + sin(x)) / x", "+oo")]
        [InlineData("(sin(x) + cos(x)) / x", "+oo")]
        [InlineData("sin(x) * cos(x) / x", "+oo")]
        public void BoundedOverDiverging(string expression, string destination) =>
            AssertLimit(expression, destination, "0");

        /// <summary>The same theorem written as a product rather than as a quotient.</summary>
        [Theory]
        [InlineData("sin(x) * e ^ (-x)", "+oo")]
        [InlineData("x * sin(1 / x)", "0")]
        [InlineData("x ^ 2 * sin(1 / x)", "0")]
        [InlineData("x * cos(1 / x)", "0")]
        [InlineData("signum(x) * x", "0")]
        public void BoundedTimesVanishing(string expression, string destination) =>
            AssertLimit(expression, destination, "0");

        /// <summary>
        /// The answers that were already given by putting the parts' own limits in place of the
        /// parts, which this must not disturb: an arctangent does have a limit at infinity, and
        /// the quotient is settled without any appeal to boundedness.
        /// </summary>
        [Theory]
        [InlineData("arctan(x) / x", "+oo")]
        [InlineData("arccotan(x) / x", "+oo")]
        [InlineData("signum(sin(x)) / x", "+oo")]
        public void TheLimitsThatWereAlreadyGiven(string expression, string destination) =>
            AssertLimit(expression, destination, "0");

        /// <summary>
        /// An oscillation whose argument grows without bound has no limit at all, which is a
        /// different claim from the one above and a stronger one than leaving the node
        /// unevaluated. It is asked after the squeeze theorem and not before it: the theorem is
        /// precisely the case where a factor with no limit of its own still leaves the product
        /// with one, and sin(x) / x would otherwise be settled as (no limit) / (+oo).
        /// </summary>
        [Theory]
        [InlineData("sin(x)", "+oo")]
        [InlineData("cos(x)", "+oo")]
        [InlineData("sin(x)", "-oo")]
        [InlineData("sin(x ^ 2)", "+oo")]
        [InlineData("sin(1 / x)", "0")]
        [InlineData("cos(1 / x)", "0")]
        public void AnOscillationHasNoLimit(string expression, string destination)
        {
            var task = Task.Run(() => expression.ToEntity()
                .Limit("x", destination.ToEntity(), ApproachFrom.BothSides));
            Assert.True(task.Wait(TimeSpan.FromSeconds(30)), "the limit did not terminate");
            Assert.Equal(MathS.NaN, task.Result.Evaled);
        }

        /// <summary>An argument that settles leaves the function settled with it.</summary>
        [Theory]
        [InlineData("cos(1 / x)", "+oo", "1")]
        [InlineData("sin(1 / x)", "+oo", "0")]
        [InlineData("sin(x) / x", "0", "1")]
        public void AnArgumentThatSettlesIsNotAnOscillation(string expression, string destination, string expected) =>
            AssertLimit(expression, destination, expected);

        /// <summary>
        /// Boundedness of a sine is a fact about a *real* argument. This library reads a free
        /// variable as complex, and sin(i * x) is sinh(x) up to a factor, which grows without
        /// bound -- so lim x-&gt;+oo sin(a * x) / x is not 0 for every a, and is left unanswered.
        /// It is the example in <see cref="Entity.Limit(Entity.Variable, Entity)"/>'s own
        /// documentation, which prints it unevaluated.
        /// </summary>
        [Theory]
        [InlineData("sin(x * a) / x", "+oo")]
        [InlineData("cos(a * x) / x", "+oo")]
        [InlineData("sin(x * a)", "+oo")]
        [InlineData("cos(a * x)", "+oo")]
        public void AComplexArgumentIsNotBounded(string expression, string destination)
        {
            var task = Task.Run(() => expression.ToEntity().Limit("x", destination.ToEntity()));
            Assert.True(task.Wait(TimeSpan.FromSeconds(30)), "the limit did not terminate");
            Assert.IsType<Entity.Limitf>(task.Result);
        }
    }
}
