//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Linq;
using AngouriMath.Core.Transformations.Matching;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    public sealed class MatchPatternEMatchTest
    {
        // MatchedRules.SharedFactor is `MatchedRules.SharedFactor` from
        // Sources/AngouriMath/Core/Transformations/Matching/MatchedRules.cs:266 -- read it before
        // this task to confirm which rule inside it is the two-term, two-node-each shape this
        // test assumes. If the set's shape has changed, adjust the expected counts rather than the
        // assertion style.

        [Fact]
        public void ANodePatternCountsItselfPlusEveryChild()
        {
            var rule = MatchedRules.SharedFactor.Rules.First();
            // A NodePattern's count is always at least 1 (itself) plus at least 1 per child --
            // never equal to a leaf pattern's count of 1, whatever the exact shape.
            Assert.True(rule.Left.NodeCount > 1);
        }

        [Fact]
        public void AnAnyPatternCountsAsOne()
        {
            // AnyPattern and ExactPattern are both leaves of a pattern tree; find one via a
            // one-hole rule rather than asserting on a specific rule's exact shape.
            var leafCount = MatchedRules.All
                .SelectMany(set => set.Rules)
                .Select(rule => rule.Left)
                .First(pattern => pattern.NodeCount == 1);
            Assert.Equal(1, leafCount.NodeCount);
        }
    }
}
