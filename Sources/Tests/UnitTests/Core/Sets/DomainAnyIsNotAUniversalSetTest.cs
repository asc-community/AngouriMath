//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Core.Exceptions;
using AngouriMath.Extensions;
using Xunit;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;

namespace UnitTests.Core.Sets
{
    /// <summary>
    /// The boundary between a mathematical set, the ambient domain a question is asked in, and a
    /// node's codomain. <a href="https://github.com/asc-community/AngouriMath/issues/996">#996</a>
    /// asked whether there should be a universal set and whether <see cref="Domain.Any"/> is it;
    /// the answer is that <c>Any</c> is a codomain and not a set, and that a set constraining
    /// nothing is already written <c>{ x : True }</c>.
    /// </summary>
    /// <remarks>
    /// These pin the invariant rather than the implementation: each one would have to be argued
    /// against before a universal set could be added, which is the point of writing them down.
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class DomainAnyIsNotAUniversalSetTest
    {
        private static readonly ConditionalSet Unconstrained = new(MathS.Var("x"), Boolean.True);

        #region A set-builder with a true predicate is the unconstrained set

        /// <summary>
        /// A predicate that holds everywhere reduces to <c>{ x : True }</c> and not to a special
        /// set — there is none to reduce to. It used to throw
        /// <see cref="AngouriBugException"/> instead
        /// (<a href="https://github.com/asc-community/AngouriMath/issues/878">#878</a>).
        /// </summary>
        [Theory]
        [InlineData("{ x : 1 = 1 }")]
        [InlineData("{ x : x = x }")]
        [InlineData("{ x : true }")]
        public void APredicateThatHoldsEverywhereIsTheUnconstrainedSet(string source)
            => Assert.Equal(Unconstrained, source.ToEntity().Simplify());

        /// <summary>
        /// And it admits values of every sort, which is what a caller wanting a universal set
        /// wants of one: a number, a truth value and a matrix are each decided members.
        /// </summary>
        [Theory]
        [InlineData("3")]
        [InlineData("-1/2")]
        [InlineData("i")]
        [InlineData("true")]
        [InlineData("[1, 2]")]
        [InlineData("{ 1, 2 }")]
        public void TheUnconstrainedSetAdmitsValuesOfEverySort(string element)
        {
            Assert.True(Unconstrained.TryContains(element.ToEntity(), out var contains),
                "membership of { x : True } is decidable");
            Assert.True(contains);
        }

        /// <summary>It prints, reads back as itself, and simplifying it again changes nothing.</summary>
        [Fact]
        public void TheUnconstrainedSetRoundTripsAndIsStable()
        {
            var printed = Unconstrained.Stringize();
            Assert.Equal(Unconstrained, printed.ToEntity());
            Assert.Equal(Unconstrained, Unconstrained.Simplify());
            Assert.Equal(Unconstrained, Unconstrained.Simplify().Simplify());
        }

        /// <summary>
        /// The bound name is not part of it, so two spellings of the same set are one set. A
        /// <see cref="SpecialSet"/> would give that for free; a set-builder gives it too, which
        /// is one fewer reason to want one.
        /// </summary>
        [Fact]
        public void TheUnconstrainedSetDoesNotDependOnItsBoundName()
            => Assert.Equal(Unconstrained, "{ y : true }".ToEntity().Simplify());

        #endregion

        #region Domain.Any is a codomain, and there is no set of it

        /// <summary>
        /// The five domains that name a set convert to it; <see cref="Domain.Any"/> does not, and
        /// says so rather than inventing one. This is the load-bearing assertion of #996: the
        /// moment <c>Any</c> answers here it has become a universal set by accident.
        /// </summary>
        [Theory]
        [InlineData(Domain.Boolean)]
        [InlineData(Domain.Integer)]
        [InlineData(Domain.Rational)]
        [InlineData(Domain.Real)]
        [InlineData(Domain.Complex)]
        public void EveryDomainThatNamesASetConvertsToOne(Domain domain)
            => Assert.IsAssignableFrom<SpecialSet>((Entity)domain);

        /// <inheritdoc cref="EveryDomainThatNamesASetConvertsToOne"/>
        [Fact]
        public void TheUnconstrainedCodomainNamesNoSet()
        {
            Assert.Throws<NotSufficientlySupportedException>(() => SpecialSet.Create(Domain.Any));
            Assert.Throws<NotSufficientlySupportedException>(() => (Entity)Domain.Any);
        }

        /// <summary>
        /// <c>{ x : True }</c> carries <see cref="Domain.Any"/> as its codomain, and that is a
        /// statement about the node rather than about its members: the codomain of the node
        /// naming the set and the set itself are different things, and the first cannot be turned
        /// into the second.
        /// </summary>
        [Fact]
        public void TheUnconstrainedSetHasTheUnconstrainedCodomainAndIsStillNotIt()
        {
            Assert.Equal(Domain.Any, Unconstrained.Codomain);
            Assert.Throws<NotSufficientlySupportedException>(
                () => SpecialSet.Create(Unconstrained.Codomain));
        }

        /// <summary>
        /// There is no spelling for a universal set, and the two that have been suggested for one
        /// — <c>AA</c> and <c>UU</c> — are ordinary variables. Introducing either is a decision
        /// this issue declined to take, so it should fail here first.
        /// </summary>
        [Theory]
        [InlineData("AA")]
        [InlineData("UU")]
        public void ThereIsNoLiteralForAUniversalSet(string source)
            => Assert.IsType<Variable>(source.ToEntity());

        /// <summary>
        /// <c>Any</c> is read as the unconstrained codomain in the second argument of
        /// <c>domain(...)</c> and is an ordinary variable everywhere else — it is a keyword in
        /// that one position and never a set literal.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/1048">#1048</a>
        /// </summary>
        [Fact]
        public void AnyIsACodomainKeywordAndNotASetLiteral()
        {
            Assert.IsType<Variable>("Any".ToEntity());
            var widened = "abs(x)".ToEntity().WithCodomain(Domain.Any);
            Assert.Equal(Domain.Any, widened.Codomain);
            Assert.Equal(widened, widened.Stringize().ToEntity());
        }

        #endregion

        #region The ambient domain is not the solution set

        /// <summary>
        /// #996's own example. <c>x - x</c> is not defined for arbitrary values — <c>true - true</c>
        /// is <see cref="MathS.NaN"/> — so the solution set is the domain the question was asked
        /// in and not everything that could be written in place of <c>x</c>.
        /// </summary>
        [Fact]
        public void ATautologyAnswersTheDomainTheQuestionWasAskedInAndNotAUniversalSet()
        {
            var solutions = "x - x = 0".ToEntity().Solve("x");
            Assert.Equal(MathS.Sets.C, solutions);
            Assert.IsType<SpecialSet.Complexes>(solutions);
        }

        /// <summary>
        /// And that answer is a real restriction rather than a permissive one: <c>CC</c> decides
        /// against a truth value and against a matrix, which is what makes it different from a
        /// universal set. <a href="https://github.com/asc-community/AngouriMath/issues/995">#995</a>
        /// </summary>
        [Theory]
        [InlineData("true")]
        [InlineData("false")]
        [InlineData("[1, 2]")]
        public void TheDomainAnEquationIsSolvedInExcludesWhatItDoesNotContain(string element)
        {
            var solutions = "x - x = 0".ToEntity().Solve("x");
            Assert.True(solutions.TryContains(element.ToEntity(), out var contains));
            Assert.False(contains);
        }

        /// <summary>
        /// The ambient codomain is a setting on the question, so it is not something an
        /// expression carries: reading over the reals does not turn the solution set into a
        /// different kind of object, and nothing about it produces a universal set.
        /// </summary>
        [Fact]
        public void TheAmbientCodomainIsAPropertyOfTheQuestion()
        {
            Assert.Equal(Domain.Complex, MathS.Settings.Codomain.Value);
            using var _ = MathS.Settings.Codomain.Set(Domain.Real);
            var solutions = "x - x = 0".ToEntity().Solve("x");
            Assert.IsAssignableFrom<SpecialSet>(solutions);
            Assert.True(solutions.TryContains("true".ToEntity(), out var contains));
            Assert.False(contains);
        }

        #endregion

        #region A complement needs no universe

        /// <summary>
        /// <c>a implies b</c> is solved as <c>{ x : not a } \/ solve(b)</c>. The complement is a
        /// set-builder and names no universe, which is what the <c>TODO</c> asking for "a
        /// universal set to subtract from" turned out to need. Taking it in the statement node's
        /// codomain instead answered <c>{ 2 } \/ BB</c> — truth values in the solution set of a
        /// numeric question.
        /// </summary>
        [Fact]
        public void AnImplicationIsSolvedWithoutNamingAUniverse()
        {
            var solutions = "(x = 1) implies (x = 2)".ToEntity().Solve("x");

            Assert.True(solutions.TryContains(2, out var two));
            Assert.True(two, "2 satisfies the implication");
            Assert.True(solutions.TryContains(1, out var one));
            Assert.False(one, "1 makes the antecedent true and the consequent false");
            Assert.True(solutions.TryContains(3, out var three));
            Assert.True(three, "3 makes the antecedent false");
        }

        /// <summary>
        /// And the answer is a set-builder rather than a domain: nothing in it asserts what
        /// <c>x</c> ranges over, which the previous answer did by naming <c>BB</c>.
        /// </summary>
        [Fact]
        public void AnImplicationsAnswerAssertsNoDomain()
        {
            var solutions = "(x = 1) implies (x = 2)".ToEntity().Solve("x");
            Assert.False(solutions is SpecialSet, "the answer names no domain");
            Assert.DoesNotContain("BB", solutions.Stringize());
        }

        #endregion

        #region An unbounded interval over no constraint is not a domain

        /// <summary>
        /// <c>(-oo; +oo)</c> is the domain it is an interval of, where that domain names a set.
        /// Widened to <see cref="Domain.Any"/> it names none, so it is left as written — asking
        /// for the set of <c>Any</c> here threw <see cref="NotSufficientlySupportedException"/>
        /// out of <c>Solve</c> on input a caller could write.
        /// </summary>
        [Fact]
        public void AnUnboundedIntervalBecomesADomainOnlyWhereThereIsOne()
        {
            Assert.Equal(MathS.Sets.R, "(-oo; +oo)".ToEntity().Simplify());
            Assert.Equal(MathS.Sets.C, "domain((-oo; +oo), CC)".ToEntity().Simplify());

            var unconstrained = "domain((-oo; +oo), Any)".ToEntity();
            Assert.Equal(Domain.Any, unconstrained.Codomain);
            Assert.Equal(unconstrained, unconstrained.Simplify());
        }

        /// <inheritdoc cref="AnUnboundedIntervalBecomesADomainOnlyWhereThereIsOne"/>
        [Fact]
        public void SolvingOverAnUnboundedIntervalOverNoConstraintDoesNotThrow()
            => Assert.Null(Record.Exception(
                () => "domain((-oo; +oo), Any) = RR".ToEntity().Solve("x")));

        #endregion
    }
}
