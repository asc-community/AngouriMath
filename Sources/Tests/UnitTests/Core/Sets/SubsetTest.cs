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
using static AngouriMath.Entity.Set;

namespace AngouriMath.Tests.Core.Sets
{
    /// <summary>
    /// <c>A subset B</c>, set equality by double containment, and <c>powerset(A)</c>: the set
    /// statements and constructions of chapter 3 of Sullivan and Mackey's <i>An
    /// Introduction to Proofs</i> (§§3.3–3.5), with its examples and exercises as the rows.
    /// <see href="https://github.com/asc-community/AngouriMath/issues/1409"/>
    /// </summary>
    [Trait("Area", "Sets")]
    public sealed class SubsetTest
    {
        private static Entity Decided(string statement) => statement.ToEntity().Evaled;

        /// <summary>§3.4.1: listed sets against the number sets.</summary>
        [Theory]
        [InlineData("{142, 857} subset ZZ+", "True")]
        [InlineData("{sqrt(3), -pi, 8.2} subset RR", "True")]
        [InlineData("{142, -857} subset ZZ+", "False")]
        [InlineData("{sqrt(3), -pi, 8.2} subset QQ", "False")]
        [InlineData("{1, 2, 12} subset RR", "True")]
        [InlineData("{-5, 8, 12} subset ZZ+", "False")]
        [InlineData("{1, 2} subset {1, 2, 3}", "True")]
        [InlineData("{1, 2, 3} subset {1, 2}", "False")]
        [InlineData("{1, 2} ⊆ {1, 2, 3}", "True")]
        [InlineData("{1, 2, 3} superset {1, 2}", "True")]
        [InlineData("{1, 2, 3} ⊇ {1, 2, 4}", "False")]
        public void AListedSetIsCheckedMemberByMember(string statement, string expected)
            => Assert.Equal(expected.ToEntity(), Decided(statement));

        /// <summary>§3.4.1 again: a set builder against a number set, through the solver.</summary>
        [Theory]
        [InlineData("{ x in RR : x^2 = 1 } subset ZZ", "True")]
        [InlineData("{ x in RR : x^2 = 5 } subset ZZ", "False")]
        [InlineData("{ x in ZZ : x >= 1 } subset ZZ+", "True")]
        [InlineData("{ x in ZZ : x >= 0 } subset ZZ+", "False")]
        [InlineData("{ x in ZZ : x > 4 } subset [0; +oo)", "True")]
        [InlineData("{ x in RR : x > 4 } subset (0; 10)", "False")]
        public void ASetBuilderIsCheckedThroughTheSolver(string statement, string expected)
            => Assert.Equal(expected.ToEntity(), Decided(statement));

        /// <summary>The chain <c>ZZ+ ⊂ ZZ* ⊂ ZZ ⊂ QQ ⊂ RR ⊂ CC</c>, both ways, and the booleans beside it.</summary>
        [Theory]
        [InlineData("ZZ+ subset ZZ", "True")]
        [InlineData("ZZ subset QQ", "True")]
        [InlineData("QQ subset ZZ", "False")]
        [InlineData("RR subset CC", "True")]
        [InlineData("CC subset RR", "False")]
        [InlineData("ZZ subset ZZ", "True")]
        [InlineData("BB subset RR", "False")]
        public void TheNumberSetsNest(string statement, string expected)
            => Assert.Equal(expected.ToEntity(), Decided(statement));

        /// <summary>Intervals against intervals and against the number sets.</summary>
        [Theory]
        [InlineData("[0; 1] subset [0; 2]", "True")]
        [InlineData("[0; 2] subset [0; 1]", "False")]
        [InlineData("(0; 1) subset [0; 1]", "True")]
        [InlineData("[0; 1] subset (0; 1]", "False")]
        [InlineData("[0; 1] subset RR", "True")]
        [InlineData("[0; 1] subset QQ", "False")]
        [InlineData("[0; 1] subset CC", "True")]
        [InlineData("[3; 3] subset ZZ", "True")]
        [InlineData("RR subset (-oo; +oo)", "True")]
        [InlineData("RR subset [0; +oo)", "False")]
        [InlineData("[0; 1] subset {0, 1}", "False")]
        public void IntervalsAreComparedByTheirEnds(string statement, string expected)
            => Assert.Equal(expected.ToEntity(), Decided(statement));

        /// <summary>
        /// The algebra: <c>S ⊆ S</c>, <c>∅ ⊆ S</c>, a union is inside a set when both halves are,
        /// a set is inside an intersection when it is inside both, and the difference and the
        /// intersection are inside what they were cut from -- for symbolic sets, where no member
        /// can be looked at (Lemma 3.9.2 and §3.9.5).
        /// </summary>
        [Theory]
        [InlineData("A subset A", "True")]
        [InlineData("{} subset A", "True")]
        [InlineData("A \\ B subset A", "True")]
        [InlineData("A /\\ B subset A", "True")]
        [InlineData("A /\\ B subset B", "True")]
        [InlineData("A subset A \\/ B", "True")]
        [InlineData("B subset A \\/ B", "True")]
        [InlineData("A \\/ B subset A", "A \\/ B subset A")]
        [InlineData("A subset B", "A subset B")]
        [InlineData("powerset(A /\\ B) subset powerset(A)", "True")]
        public void TheAlgebraDecidesSymbolicSets(string statement, string expected)
            => Assert.Equal(expected.ToEntity(), Decided(statement));

        /// <summary>A subset statement is about sets; a number or a truth value on a side is NaN.</summary>
        [Fact]
        public void ANumberIsNotASet()
        {
            Assert.True(Decided("1 subset {1}").IsNaN);
            Assert.True(Decided("{1} subset 1").IsNaN);
        }

        /// <summary>
        /// §3.3.3 and §3.3.7 Q9: equality of listed sets was structural already and is pinned;
        /// equality by double containment reaches a set builder against a number set, and two
        /// intervals.
        /// </summary>
        [Theory]
        [InlineData("{A, E, I, O, U} = {U, E, I, A, O}", "True")]
        [InlineData("{a, a, a} = {a}", "True")]
        [InlineData("{a, b, c} = {a, a, b, c, a, b}", "True")]
        [InlineData("{ x in ZZ : x >= 1 } = ZZ+", "True")]
        [InlineData("{ x in ZZ : x >= 0 } = ZZ+", "False")]
        [InlineData("[0; 1] = [0; 2]", "False")]
        [InlineData("(0; 1) = [0; 1]", "False")]
        [InlineData("{1, 2} = {1, 2, 3}", "False")]
        [InlineData("ZZ = QQ", "False")]
        [InlineData("{ x in RR : x^2 = 1 } = {-1, 1}", "True")]
        [InlineData("{1, x} = {1, 2}", "{1, x} = {1, 2}")]
        public void SetsAreEqualByDoubleContainment(string statement, string expected)
            => Assert.Equal(expected.ToEntity(), Decided(statement));

        /// <summary>§3.4.1, Ex 3.4.3–4, §3.4.5 Try 1–2: the power set listed, and its size.</summary>
        [Fact]
        public void ThePowerSetOfAListedSetIsListed()
        {
            Assert.Equal("{ {}, {1}, {2}, {3}, {1, 2}, {1, 3}, {2, 3}, {1, 2, 3} }".ToEntity(), Decided("powerset({1, 2, 3})"));
            Assert.Equal("{ {} }".ToEntity(), Decided("powerset({})"));
            Assert.Equal("{ {}, { {} } }".ToEntity(), Decided("powerset(powerset({}))"));
            Assert.Equal("{ {}, { {} }, { {1, {}} }, { {}, {1, {}} } }".ToEntity(), Decided("powerset({ {}, {1, {}} })"));
            Assert.Equal("8".ToEntity(), Decided("card(powerset({1, 2, 3}))"));
            Assert.Equal("4".ToEntity(), Decided("card(powerset({1, 2}))"));
            Assert.Equal("2".ToEntity(), Decided("card(powerset({1}))"));
            Assert.Equal("1".ToEntity(), Decided("card(powerset({}))"));
        }

        /// <summary>§3.3.7 Q7: listed sets count their members, sets among them.</summary>
        [Theory]
        [InlineData("card({})", "0")]
        [InlineData("card({1, 2, 10})", "3")]
        [InlineData("card({1, {}})", "2")]
        [InlineData("card({ {} })", "1")]
        public void AListedSetIsCounted(string statement, string expected)
            => Assert.Equal(expected.ToEntity(), Decided(statement));

        /// <summary>
        /// §3.4.5 Try 5–9 and Try 3: a set is a member of a power set exactly when it is a
        /// subset of the argument -- for a power set of an infinite set too, which is an object
        /// rather than a list.
        /// </summary>
        [Theory]
        [InlineData("{1, 3, 7} in powerset(ZZ+)", "True")]
        [InlineData("ZZ+ in powerset(ZZ)", "True")]
        [InlineData("{-1} in powerset(ZZ+)", "False")]
        [InlineData("1 in powerset(ZZ)", "False")]
        [InlineData("{} in powerset({1})", "True")]
        [InlineData("{1} in powerset({1})", "True")]
        [InlineData("{2} in powerset({1})", "False")]
        [InlineData("powerset(ZZ+) subset powerset(ZZ)", "True")]
        [InlineData("powerset(ZZ) subset powerset(ZZ+)", "False")]
        public void MembershipOfAPowerSetIsSubsetOfTheArgument(string statement, string expected)
            => Assert.Equal(expected.ToEntity(), Decided(statement));

        /// <summary>
        /// §3.4.5 Try 3: <c>A = {x, ♥, {4}, ∅}</c>, the eleven statements (a)–(k). The book's
        /// <c>x</c> and <c>♥</c> are two distinct objects, so they are two distinct numbers here:
        /// with a symbol <c>x</c> the answer to <c>{x} in A</c> would rightly be open, since
        /// <c>x</c> might be <c>4</c>.
        /// </summary>
        [Theory]
        [InlineData("7 in A", "True")]
        [InlineData("{7} in A", "False")]
        [InlineData("{7} subset A", "True")]
        [InlineData("{4} in A", "True")]
        [InlineData("4 in A", "False")]
        [InlineData("{ {4} } subset A", "True")]
        [InlineData("{4} subset A", "False")]
        [InlineData("{} in A", "True")]
        [InlineData("{} subset A", "True")]
        [InlineData("{ {} } subset A", "True")]
        [InlineData("{7, 9} subset A", "True")]
        public void TheElevenStatementsAboutOneSet(string statement, string expected)
        {
            var a = "{7, 9, {4}, {}}".ToEntity();
            Assert.Equal(expected.ToEntity(), statement.ToEntity().Substitute("A", a).Evaled);
        }

        /// <summary>The nodes print as they parse, and the API builds them.</summary>
        /// <summary>
        /// <c>x subset 2</c> is not a statement about any <c>x</c>, since <c>2</c> is not a set: the
        /// predicate is NaN and holds nowhere, so the set it describes is empty -- and
        /// <c>Solve</c> answers the empty set rather than throwing, which the crash harness
        /// found it doing when the NaN predicate made the whole set-builder NaN.
        /// </summary>
        [Theory]
        [InlineData("x subset 2")]
        [InlineData("1/0 = x")]
        public void APredicateThatIsNaNHoldsNowhere(string statement)
        {
            Assert.Equal(Empty, statement.ToEntity().Solve("x"));
            Assert.Equal(Empty, $"{{ x : {statement} }}".ToEntity().Evaled);
        }

        [Fact]
        public void PrintingAndTheApi()
        {
            Assert.Equal("A subset B", "A ⊆ B".ToEntity().ToString());
            Assert.Equal("A subset B", "B ⊇ A".ToEntity().ToString());
            Assert.Equal("powerset(A)", "powerset(A)".ToEntity().ToString());
            Assert.Equal(@"A \subseteq B", "A subset B".ToEntity().Latexize());
            Assert.Equal("A subset B".ToEntity(), MathS.Sets.Subset("A", "B"));
            Assert.Equal("A subset B".ToEntity(), ((Entity)"A").SubsetOf("B"));
            Assert.Equal("powerset(A)".ToEntity(), MathS.Sets.PowerSet("A"));
            Assert.Equal("powerset(A)".ToEntity(), ((Entity)"A").PowerSet());
            Assert.Equal("A subset B".ToEntity(), "A subset B".ToEntity().ToString().ToEntity());
        }
    }
}
