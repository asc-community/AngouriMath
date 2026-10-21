//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Common
{
    /// <summary>
    /// <c>Simplify</c> returns on quotients over a symbolic denominator. Bringing two of their
    /// fractions over a common denominator re-simplifies the cross products, and reading those as
    /// polynomials expands squares of sums of thirty terms and more, whose exponent splits were
    /// enumerated in time exponential in the number of terms. The integrator met them after a
    /// substitution. <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Common")]
    public sealed class SimplifyOverASymbolicCommonDenominatorTest
    {
        [Fact]
        public void TheSquareOfALongSumExpands()
        {
            var names = Enumerable.Range(0, 30).Select(i => "s" + (char)('a' + i / 26) + (char)('a' + i % 26)).ToArray();
            var sum = string.Join(" + ", names);
            var expanded = Run(() => $"({sum})^2".ToEntity().Expand());
            // Thirty squares and 435 products of two, none of which combine.
            Assert.Equal(465, Entity.Sumf.LinearChildren(expanded).Count);
            var at = expanded;
            for (var i = 0; i < names.Length; i++)
                at = at.Substitute(names[i], i + 1);
            Assert.Equal((Entity)(465 * 465), at.Evaled);
        }

        // Each returns as it was written: the search finds nothing shorter. Under a second or
        // three each on a desktop, where the first did not return in ten minutes and the
        // third took forty-seven seconds.
        [Theory]
        [InlineData("u^4/((c*u + d*(2 - u))^2*(2 - u))")]
        [InlineData("u^3/((c*u + d)^2*(2 - u))")]
        [InlineData("((A + B*(1 - 2*c/d))*sin(h)*(1 - u^2) + B*(-1/2 + c/d - c^2/(2*d^2))*sin(h))/(c + d*(2*(1 - u^2) - 1))^2/(-sin(h))")]
        public void ReturnsWithTheValueItWasGiven(string written)
        {
            var expression = written.ToEntity();
            var simplified = Run(() => expression.Simplify(1));
            var compared = 0;
            foreach (var shift in new[] { 0.0, 0.37, 1.13 })
            {
                Entity Pinned(Entity e) => e
                    .Substitute("A", 1.3 + shift).Substitute("B", -0.7 + shift).Substitute("c", 0.4 + shift)
                    .Substitute("d", 1.9 - shift).Substitute("h", 0.6 + shift).Substitute("u", 0.3 + shift);
                var want = Pinned(expression).EvalNumerical();
                var got = Pinned(simplified).EvalNumerical();
                if (want.IsNaN)
                    continue;
                compared++;
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"{written} simplified to {simplified}, which is {got} where the expression is {want}");
            }
            Assert.Equal(3, compared);
        }

        /// <summary>
        /// On a thread of its own, so that a test host whose pool is busy cannot hold it back,
        /// and with a wall far above the run, so that the bound is reached only by a computation
        /// that does not finish.
        /// </summary>
        private static Entity Run(Func<Entity> work)
        {
            var task = Task.Factory.StartNew(work, CancellationToken.None,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);
            Assert.True(task.Wait(TimeSpan.FromMinutes(2)), "did not return in two minutes");
            return task.Result;
        }
    }
}
