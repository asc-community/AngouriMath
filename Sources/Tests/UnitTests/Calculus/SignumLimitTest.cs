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
    /// The limit of a signum. It had no reading of its own and handed back an unevaluated limit
    /// of the very expression it was asked about, which did not merely fail to answer: the
    /// two-sided path compares its two one-sided results by evaluating them, evaluating a limit
    /// computes it, and computing it arrived back at the same place. The recursion ended by
    /// overflowing the stack, which kills the process rather than raising anything a caller
    /// could catch -- https://github.com/asc-community/AngouriMath/issues/704.
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class SignumLimitTest
    {
        private static Entity Limit(string expression, string destination) =>
            expression.ToEntity().Limit("x", destination.ToEntity()).Simplify();

        /// <summary>
        /// Away from zero the sign is constant, so the limit is the sign of whatever the
        /// argument tends to -- including at the infinities, where it is 1 and -1.
        /// </summary>
        [Theory]
        [InlineData("signum(x)", "2", "1")]
        [InlineData("signum(x)", "-2", "-1")]
        [InlineData("signum(x - 5)", "2", "-1")]
        [InlineData("signum(x ^ 2 + 1)", "0", "1")]
        [InlineData("signum(x)", "+oo", "1")]
        [InlineData("signum(x)", "-oo", "-1")]
        public void TheSignOfWhereTheArgumentGoes(string expression, string destination, string expected) =>
            Assert.Equal(expected.ToEntity().Evaled, Limit(expression, destination).Evaled);

        /// <summary>
        /// Where the argument tends to zero there is nothing to say: the sign is 1 on one side
        /// and -1 on the other, and which one is taken depends on the direction the argument
        /// approaches from rather than only on what it tends to. Left unevaluated -- and, above
        /// all, terminating, which is what this test is really for.
        /// </summary>
        [Theory]
        [InlineData("signum(x)", "0")]
        [InlineData("signum(x ^ 3)", "0")]
        public void AtZeroItIsLeftUnevaluatedAndTerminates(string expression, string destination)
        {
            var task = Task.Run(() => Limit(expression, destination));
            Assert.True(task.Wait(TimeSpan.FromSeconds(30)), "the limit did not terminate");
            Assert.IsType<Entity.Limitf>(task.Result);
        }

        [Theory]
        [InlineData(ApproachFrom.Left)]
        [InlineData(ApproachFrom.Right)]
        public void OneSidedAtZeroTerminatesToo(ApproachFrom side)
        {
            var task = Task.Run(() => "signum(x)".ToEntity().Limit("x", 0, side).Simplify());
            Assert.True(task.Wait(TimeSpan.FromSeconds(30)), "the limit did not terminate");
            Assert.IsType<Entity.Limitf>(task.Result);
        }

        /// <summary>
        /// A signum inside a larger expression must not drag the whole limit down with it.
        /// </summary>
        [Theory]
        [InlineData("signum(x) * x", "2", "2")]
        [InlineData("signum(x) + 1", "3", "2")]
        [InlineData("abs(x)", "2", "2")]
        public void ItComposes(string expression, string destination, string expected) =>
            Assert.Equal(expected.ToEntity().Evaled, Limit(expression, destination).Evaled);
    }
}
