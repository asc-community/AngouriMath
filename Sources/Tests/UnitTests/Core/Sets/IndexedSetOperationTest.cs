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

namespace AngouriMath.Tests.Core.Sets
{
    /// <summary>
    /// Indexed unions and intersections, the complement relative to a universe, and the
    /// integer range <c>[n] = ZZ+ /\ [1; n]</c> the reference writes them over (Sullivan and
    /// Mackey, §§3.5–3.6 and Problem 3.11.21), with its examples as the rows. An index name is
    /// <c>k</c> or <c>n</c> below where the book writes <c>i</c>, which is the imaginary unit here.
    /// <see href="https://github.com/asc-community/AngouriMath/issues/1409"/>
    /// </summary>
    [Trait("Area", "Sets")]
    public sealed class IndexedSetOperationTest
    {
        private static Entity Evaluated(string expression) => expression.ToEntity().Evaled;

        /// <summary>An integer set cut by a numeric interval lists its members: the book's <c>[n]</c>.</summary>
        [Theory]
        [InlineData("ZZ /\\ [1; 10]", "{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}")]
        [InlineData("ZZ+ /\\ [-2; 3]", "{1, 2, 3}")]
        [InlineData("[0; 2.5] /\\ ZZ*", "{0, 1, 2}")]
        [InlineData("ZZ /\\ (0; 3)", "{1, 2}")]
        [InlineData("ZZ /\\ (1; 2)", "{}")]
        [InlineData("RR /\\ [0; 1]", "[0; 1]")]
        [InlineData("QQ /\\ [0; 1]", "QQ /\\ [0; 1]")]
        [InlineData("ZZ /\\ [1; n]", "ZZ /\\ [1; n]")]
        [InlineData("card(ZZ+ /\\ [1; 12])", "12")]
        public void AnIntegerRangeIsListed(string expression, string expected)
            => Assert.Equal(expected.ToEntity().Evaled, Evaluated(expression));

        /// <summary>Ex 3.6.1: <c>⋃_{i=1}^{10} {i, 2i}</c>; Ex 3.6.6: <c>A_i = {i-2, …, i+2}</c> over <c>{1,2,3}</c> and over <c>{-1,0,1}</c>.</summary>
        [Theory]
        [InlineData("union({k, 2 k}, k in ZZ+ /\\ [1; 10])", "{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 14, 16, 18, 20}")]
        [InlineData("union({k-2, k-1, k, k+1, k+2}, k in {1, 2, 3})", "{-1, 0, 1, 2, 3, 4, 5}")]
        [InlineData("intersection({k-2, k-1, k, k+1, k+2}, k in {1, 2, 3})", "{1, 2, 3}")]
        [InlineData("union({k-2, k-1, k, k+1, k+2}, k in {-1, 0, 1})", "{-3, -2, -1, 0, 1, 2, 3}")]
        [InlineData("intersection({k-2, k-1, k, k+1, k+2}, k in {-1, 0, 1})", "{-1, 0, 1}")]
        [InlineData("union([k; k + 1], k in {0, 1, 2})", "[0; 3]")]
        [InlineData("union({k}, k in {})", "{}")]
        public void AFamilyOverAListedIndexSetIsFolded(string expression, string expected)
            => Assert.Equal(expected.ToEntity().Evaled, Evaluated(expression));

        /// <summary>
        /// Over an infinite index set the family is an object, and membership goes through the
        /// quantifiers: <c>x in union(A_n, n in I)</c> is <c>exists n in I : x in A_n</c>. Problem
        /// 3.11.20's <c>⋂ [n] = {1}</c> and <c>⋃ [n] = ZZ+</c>, and §3.9.5 Try 8's
        /// <c>⋂ [0, 1/n) = {0}</c>, read at their members.
        /// </summary>
        [Theory]
        [InlineData("1 in intersection(ZZ+ /\\ [1; n], n in ZZ+)", "True")]
        [InlineData("2 in intersection(ZZ+ /\\ [1; n], n in ZZ+)", "False")]
        [InlineData("7 in union(ZZ+ /\\ [1; n], n in ZZ+)", "True")]
        [InlineData("0 in intersection([0; 1/n), n in ZZ+)", "True")]
        [InlineData("0.1 in intersection([0; 1/n), n in ZZ+)", "False")]
        [InlineData("1.5 in union([1; (k+1)/k], k in ZZ+)", "True")]
        public void MembershipOfAnInfiniteFamilyIsDecidedByTheQuantifiers(string expression, string expected)
            => Assert.Equal(expected.ToEntity(), Evaluated(expression));

        /// <summary>Ex 3.5.11: the complement of a set in a universe is the universe less the set.</summary>
        [Theory]
        [InlineData("complement({1, 2}, ZZ+ /\\ [1; 5])", "{3, 4, 5}")]
        [InlineData("complement({1, 2, 3, 4, 5}, ZZ+ /\\ [1; 7])", "{6, 7}")]
        public void AComplementIsRelativeToAUniverse(string expression, string expected)
            => Assert.Equal(expected.ToEntity().Evaled, Evaluated(expression));

        /// <summary>The index is bound: substituting for it changes nothing, and a value that mentions it is kept out of its reach.</summary>
        [Fact]
        public void TheIndexIsBound()
        {
            var family = "union({k, m}, k in {1, 2})".ToEntity();
            Assert.Equal(family, family.Substitute("k", 5));
            Assert.Equal("union({k, 3}, k in {1, 2})".ToEntity(), family.Substitute("m", 3));
            var renamed = family.Substitute("m", "k + 1");
            Assert.Equal("{1, 2, k + 1}".ToEntity().Evaled, renamed.Evaled);
            Assert.Equal("union(A, k in I)", "union(A, k in I)".ToEntity().ToString());
            Assert.Equal("intersection(A, k in I)", MathS.Sets.IndexedIntersection("A", "k", "I").ToString());
            Assert.Equal(@"\bigcup_{k \in I} A", "union(A, k in I)".ToEntity().Latexize());
        }
    }
}
