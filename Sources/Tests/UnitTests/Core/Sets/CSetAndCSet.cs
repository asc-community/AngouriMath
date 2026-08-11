//
// Copyright (c) 2019-2022 Angouri.
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
    [Trait("Area", "Core")]
    public sealed class CSetAndCSet
    {
        private readonly Set A = new ConditionalSet("x", "x > 0");
        private readonly Set A1 = new ConditionalSet("y", "y > 0");
        private readonly Set B = new ConditionalSet("x", "x xor true");
        private readonly Set C = new ConditionalSet("x", "x5 - x - 1 = 0");
        private readonly Set D = new ConditionalSet("x", "x < 0");

        private void Test(Set actual, ConditionalSet expected)
        {
            var csetAct = Assert.IsType<ConditionalSet>(actual.Simplify());
            Assert.Equal(expected, csetAct);
        }

        private void TestArb(Entity actual, Entity expected)
        {
            Assert.Equal(expected, actual);
        }

        [Fact] public void VarDoesntMatter1() => Test(A, new("y", "y > 0")); // { x | f(x) } == { y | f(y) }
        [Fact] public void VarDoesntMatter2() => Test(B, new("y", "not y"));

        [Fact] public void Union1() => Test(A.Unite(A1), new("x", "x > 0"));
        [Fact] public void Union2() => Test(A1.Unite(A), new("x", "x > 0"));
        [Fact] public void Union3() => Test(A.Unite(B), new("x", "x implies x > 0"));
        [Fact] public void Union4() => Test(B.Unite(A), new("x", "x implies x > 0"));

        [Fact] public void Intersection1() => Test(A1.Intersect(A), new("x", "x > 0"));
        [Fact] public void Intersection2() => Test(A.Intersect(A1), new("x", "x > 0"));
        [Fact] public void Intersection3() => TestArb(A.Intersect(D).Simplify(), Set.Empty);
        [Fact] public void Intersection4() => TestArb(D.Intersect(A).Simplify(), Set.Empty);

        // https://github.com/asc-community/AngouriMath/issues/878
        // The predicate was passed to the argument-expanding helper, which lifts a Providedf
        // out of an argument onto the whole expression. For a node that binds a variable that
        // puts the bound variable outside its own binder: `{ x : 1/x = 0 }` came back as
        // `{ } provided not x = 0`. Membership is the predicate holding, and a predicate that
        // is False where its condition holds and undefined where it does not admits nothing,
        // so the answer is the empty set with no condition to place anywhere.
        [Theory]
        [InlineData("{ x : 1/x = 0 }")]
        [InlineData("{ x : x > 0 and x < 0 }")]
        public void APredicateThatIsNeverTrueGivesTheEmptySetAndNoCondition(string input)
            => Assert.Equal(Set.Empty, input.ToEntity().Simplify());

        // Nothing may name the bound variable outside the set, whatever the answer turns out
        // to be, so this is asserted on the shape rather than on one expected result.
        [Theory]
        [InlineData("{ x : 1/x = 0 }")]
        [InlineData("{ x : x/x = 1 }")]
        [InlineData("{ x : x > 0 and x < 0 }")]
        [InlineData("{ x : x = a and x > a }")]
        public void SimplifyingASetBuilderLeavesNoConditionOutsideTheBinder(string input)
            => Assert.DoesNotContain(input.ToEntity().Simplify().DirectChildren, node => node is Providedf);

        // For a predicate that holds everywhere InnerSimplify returned the node's Codomain,
        // which is Domain.Any for a set-builder, and SpecialSet.Create has no case for Any --
        // so it threw AngouriBugException out of Simplify on valid input rather than answering.
        [Theory]
        [InlineData("{ x : 1 = 1 }")]
        [InlineData("{ x : x = x }")]
        [InlineData("{ x : x/x = 1 }")]
        [InlineData("{ x : x > 0 or x <= 0 }")]
        public void APredicateThatHoldsEverywhereDoesNotThrow(string input)
            => Assert.IsAssignableFrom<Set>(input.ToEntity().Simplify());
    }
}
