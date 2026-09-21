//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Linq;
using AngouriMath;
using AngouriMath.Extensions;
using Xunit;
using static AngouriMath.Entity;

namespace AngouriMath.Tests.Algebra
{
    /// <summary>
    /// <c>n in ZZ and 2^n > n^2</c> solved as a set: a window from the least member searched, the
    /// members listed, and the tails proved by the quantifier's induction -- so the answer is a
    /// finite set and rays of whole numbers, never a guess from the window. Sullivan and Mackey's
    /// Ex 5.3.2, §5.3.4 Try 4, Prob 5.7.2 and 5.7.8-9, with the thresholds the book derives.
    /// <see href="https://github.com/asc-community/AngouriMath/issues/1409"/>
    /// </summary>
    [Trait("Area", "Algebra")]
    public sealed class ThresholdSetTest
    {
        [Theory]
        // Ex 5.3.2: 2^n > n^2 exactly on {0, 1} and from 5.
        [InlineData("n in ZZ and 2^n > n^2", "{0, 1} \\/ (ZZ /\\ [5; +oo))")]
        [InlineData("n in ZZ+ and 2^n > n^2", "{1} \\/ (ZZ+ /\\ [5; +oo))")]
        // §5.3.4 Try 4: n^3 < 3^(n - 1) holds for n <= 0 and from 6.
        [InlineData("n in ZZ and n^3 < 3^(n - 1)", "(ZZ /\\ (-oo; 0]) \\/ (ZZ /\\ [6; +oo))")]
        // Prob 5.7.2: 3^n > n^4 at 1 and from 8; 4^n > n^4 at 1 and from 5.
        [InlineData("n in ZZ+ and 3^n > n^4", "{1} \\/ (ZZ+ /\\ [8; +oo))")]
        [InlineData("n in ZZ+ and 4^n > n^4", "{1} \\/ (ZZ+ /\\ [5; +oo))")]
        // Prob 5.7.8-9: n! > 2^n from 4, n! > 3^n from 7, n! > 5^n from 12.
        [InlineData("n in ZZ+ and n! > 2^n", "ZZ+ /\\ [4; +oo)")]
        [InlineData("n in ZZ+ and n! > 3^n", "ZZ+ /\\ [7; +oo)")]
        [InlineData("n in ZZ+ and n! > 5^n", "ZZ+ /\\ [12; +oo)")]
        // The inequality on either side of the conjunction, and the other comparisons.
        [InlineData("3^n < n! and n in ZZ+", "ZZ+ /\\ [7; +oo)")]
        [InlineData("n in ZZ+ and 2^n <= n^2", "{2, 3, 4}")]
        [InlineData("n in ZZ+ and 2^n >= n^2", "{1, 2} \\/ (ZZ+ /\\ [4; +oo))")]
        // One run over the whole window is the whole set, or none of it.
        [InlineData("n in ZZ* and 2^n >= n + 1", "ZZ*")]
        [InlineData("n in ZZ+ and 2^n < n", "{}")]
        [InlineData("n in ZZ and 2^n < 0", "{}")]
        public void TheMembersAreListedAndTheTailsProved(string statement, string expected)
        {
            var solved = statement.ToEntity().Solve("n");
            var wanted = expected.ToEntity().Evaled;
            // The same set two ways is the same set: compared by membership at the points a
            // threshold could be wrong at, rather than by the printed union.
            foreach (var at in Enumerable.Range(-20, 41))
                Assert.Equal(MathS.Sets.ElementInSet(at, (Set)wanted).Evaled, MathS.Sets.ElementInSet(at, solved).Evaled);
        }

        [Theory]
        // A tail the induction does not prove leaves the statement to the inequality solver,
        // which refuses an exponential: a window is evidence about the window only.
        [InlineData("n in ZZ and 2^n >= n^2 - 3 n + 3")]
        public void AnUnprovedTailIsNotGuessed(string statement)
            => Assert.ThrowsAny<AngouriMath.Core.Exceptions.NotSufficientlySupportedException>(() => statement.ToEntity().Solve("n"));
    }
}
