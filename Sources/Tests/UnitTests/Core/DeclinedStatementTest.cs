//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Runtime.ExceptionServices;
using AngouriMath;
using AngouriMath.Core.Exceptions;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// The statement solver declines an inequality it cannot read by answering nothing, which the
    /// public <c>Solve</c> reports as unsupported and the library's own callers -- the quantifier
    /// decision, a hypothesis cut, a pre-image -- read as not solved, without an exception thrown
    /// and caught on the way. https://github.com/asc-community/AngouriMath/issues/1540
    /// </summary>
    [Trait("Area", "Algebra")]
    public sealed class DeclinedStatementTest
    {
        /// <summary>How many of the library's exceptions are thrown on this thread while <paramref name="action"/> runs, caught or not.</summary>
        private static int ThrownDuring(Action action)
        {
            var thread = Environment.CurrentManagedThreadId;
            var thrown = 0;
            void Count(object? sender, FirstChanceExceptionEventArgs e)
            {
                if (e.Exception is AngouriMathBaseException && Environment.CurrentManagedThreadId == thread)
                    thrown++;
            }
            AppDomain.CurrentDomain.FirstChanceException += Count;
            try
            {
                action();
            }
            finally
            {
                AppDomain.CurrentDomain.FirstChanceException -= Count;
            }
            return thrown;
        }

        [Theory]
        // The quantifier asks the solver where the body fails; a hypothesis on a whole number
        // is asked for the members it cuts to; a pre-image asks where the image is in the set.
        [InlineData("forall x in RR : sin(x) <= 1")]
        [InlineData("forall k in ZZ : sin(k) > 0 implies not k = 0")]
        [InlineData("{ x in RR : sin(x) in (0; 1) }")]
        public void ADeclineIsNotThrown(string statement)
        {
            var parsed = MathS.FromString(statement, useCache: false);
            Assert.Equal(0, ThrownDuring(() => parsed.Simplify()));
        }

        [Theory]
        [InlineData("sin(x) > 0")]
        [InlineData("sin(x) in (0; 1)")]
        [InlineData("x > 0 and sin(x) > 0")]
        [InlineData("not (x < 0 or sin(x) <= 0)")]
        public void ThePublicSolveStillSaysSo(string statement)
        {
            var thrown = Assert.Throws<NotSufficientlySupportedException>(() => MathS.FromString(statement, useCache: false).Solve("x"));
            Assert.Contains("degree", thrown.Message);
        }
    }
}
