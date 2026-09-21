//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;
using static AngouriMath.Entity;

namespace AngouriMath.Tests.Algebra
{
    /// <summary>
    /// A conjunction whose one side is a listed solution set and whose other is a condition:
    /// the members are kept where the condition is decided <c>True</c> at them, dropped where
    /// <c>False</c>, and the conjunction stays written the moment one is undecided -- which is
    /// #1036's case, where the condition was a search left open. <c>x^2 = 4 and not x = 2</c>
    /// is <c>{ -2 }</c>; and with it the injectivity of a linear map is decided, the
    /// reference's Def 7.4.6 (Sullivan and Mackey).
    /// <see href="https://github.com/asc-community/AngouriMath/issues/1409"/>,
    /// <see href="https://github.com/asc-community/AngouriMath/issues/1036"/>.
    /// </summary>
    [Trait("Area", "Algebra")]
    public sealed class ConjunctionFilterTest
    {
        [Theory]
        [InlineData("x^2 = 4 and not x = 2", "{-2}")]
        [InlineData("x^2 = 4 and not x = 3", "{-2, 2}")]
        [InlineData("x^2 = 4 and x > 0", "{2}")]
        [InlineData("(x - 1) (x - 2) = 0 and not x = 1 and not x = 2", "{}")]
        public void AListedSetIsFilteredByADecidedCondition(string statement, string expected)
        {
            var solved = statement.ToEntity().Solve("x");
            var wanted = (Set)expected.ToEntity().Evaled;
            Assert.Equal(Boolean.True, MathS.Sets.Subset(solved, wanted).Evaled);
            Assert.Equal(Boolean.True, MathS.Sets.Subset(wanted, solved).Evaled);
        }

        /// <summary>#1036: a member whose condition is not decided is not kept and not dropped -- the conjunction is left as written.</summary>
        [Fact]
        public void AnUndecidedConditionLeavesTheConjunctionWritten()
        {
            var solved = "x^6 + x*y + 1 = 0 and x - 1 = 0".ToEntity().Solve("x");
            Assert.IsType<Set.ConditionalSet>(solved);
        }

        /// <summary>The inner statement of injectivity, and injectivity itself, for a linear map; the quadratic is refuted by a witness.</summary>
        [Fact]
        public void InjectivityOfALinearMapIsDecided()
        {
            Assert.Equal(Boolean.True, "forall v in RR : 2 v + 1 = 2 u + 1 implies v = u".ToEntity().Evaled);
            Assert.Equal(Boolean.True, "forall u in RR : forall v in RR : 2 u + 1 = 2 v + 1 implies u = v".ToEntity().Evaled);
            Assert.Equal(Boolean.False, "forall u in RR : forall v in RR : u^2 = v^2 implies u = v".ToEntity().Evaled);
        }
    }
}
