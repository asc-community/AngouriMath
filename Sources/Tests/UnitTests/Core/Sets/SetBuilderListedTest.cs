//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core.Sets
{
    /// <summary>
    /// A set builder over a set of numbers is listed, or written as intervals, where that takes no
    /// search: a listed set is filtered, and otherwise the predicate compares rational functions of
    /// low degree, or is an equation in moduli of linear functions, which the solvers answer
    /// exactly. Sullivan and Mackey's §3.3.3-4, §3.3.7 Q6, Prop 3.9.6 and Prob 3.11.4.
    /// https://github.com/asc-community/AngouriMath/issues/1409
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class SetBuilderListedTest
    {
        [Theory]
        [InlineData("{ x in RR : x^2 - 2 = 0 }", "{ -sqrt(2), sqrt(2) }")]
        [InlineData("{ x in ZZ+ : x^2 - 2 = 0 }", "{ }")]
        [InlineData("{ x in ZZ+ : abs(x) = 5 }", "{ 5 }")]
        [InlineData("{ x in RR : abs(x) = 5 }", "{ -5, 5 }")]
        [InlineData("{ a in ZZ+ : a < 0 }", "{ }")]
        [InlineData("{ r in RR : r^2 < 0 }", "{ }")]
        [InlineData("{ x in { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 } : x >= 7 }", "{ 7, 8, 9, 10 }")]
        [InlineData("{ x in ZZ+ : x + 8/x <= 6 }", "{ 2, 3, 4 }")]
        [InlineData("{ n in ZZ+ : n^2 < 39 }", "{ 1, 2, 3, 4, 5, 6 }")]
        [InlineData("{ x in ZZ : x^2 < 10 }", "{ -3, -2, -1, 0, 1, 2, 3 }")]
        [InlineData("{ n in PP : n < 20 }", "{ 2, 3, 5, 7, 11, 13, 17, 19 }")]
        [InlineData("{ x in [0; 10] : x^2 > 4 }", "(2; 10]")]
        [InlineData("{ x in RR : x^2 + 1 = 0 }", "{ }")]
        [InlineData("{ x in ZZ : x > 0 and x < 4 or x = 10 }", "{ 1, 2, 3, 10 }")]
        public void Listed(string written, string listed)
            => Assert.Equal(listed.ToEntity().InnerSimplified, written.ToEntity().Simplify());

        /// <summary>Where listing would take a search, or has no end, the set builder is left as written.</summary>
        [Theory]
        [InlineData("{ x in RR : sin(x) > 0 }")]
        [InlineData("{ x in ZZ : x > 5 }")]
        [InlineData("{ x in RR : x > a }")]
        [InlineData("{ x in QQ : not x in ZZ }")]
        public void LeftAsWritten(string written)
            => Assert.IsType<Entity.Set.ConditionalSet>(written.ToEntity().Simplify());
    }
}
