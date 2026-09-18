//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Linq;
using AngouriMath;
using AngouriMath.Core.Exceptions;
using AngouriMath.Extensions;
using Xunit;
using static AngouriMath.Entity;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// <c>forall x in S : P</c>, <c>exists x in S : P</c> and <c>exists! x in S : P</c>: decided
    /// over a finite set by evaluation, over an infinite one by a counterexample or by the solver,
    /// and left as written otherwise. The rows are the worked examples and exercises of chapter 4
    /// of Sullivan and Mackey's proofs book, with the section each comes from.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1409">#1409</a>
    /// </summary>
    [Trait("Area", "Discrete")]
    public sealed class QuantifierTest
    {
        [Theory]
        // Problems 4.11.3-4: quantifier games over finite sets, decided by playing them.
        [InlineData("forall x in {1, 2, 3, 4} : exists y in {3, 4, 5, 6, 7, 8} : x + y = 7", "True")]
        [InlineData("forall x in {2, 3, 4, 5, 6} : exists y in {3, 4, 5, 6, 7, 8} : x + y = 7", "False")]
        [InlineData("exists s in {1, 2, 3} : forall t in {3, 4, 5} : exists v in {4, 5, 6, 7, 8} : s + t = v", "True")]
        // §4.7.3 Try 2: A = {1, 2, 3, 4}, B = {2, 3}, in both bracketings.
        [InlineData("forall x in {1, 2, 3, 4} : forall y in {2, 3} : x >= y implies x^2 >= 4", "True")]
        [InlineData("forall x in {1, 2, 3, 4} : (forall y in {2, 3} : x >= y) implies x^2 >= 4", "True")]
        // Example 7.2.13: x = x^3 on {-1, 0, 1}.
        [InlineData("forall x in {-1, 0, 1} : x = x^3", "True")]
        [InlineData("forall x in {1, 2, 3} : x > 1", "False")]
        [InlineData("exists! x in {1, 2, 3} : x > 2", "True")]
        [InlineData("exists! x in {1, 2, 3} : x > 1", "False")]
        // Over the empty set every member satisfies anything and none satisfies something.
        [InlineData("forall x in {} : x > 5", "True")]
        [InlineData("exists x in {} : x > 5", "False")]
        // The booleans are a finite set, so the laws of §4.6 are decided by their tables.
        [InlineData("forall p, q in BB : (p implies q) implies (not q implies not p)", "True")]
        [InlineData("forall p, q in BB : (p implies q) implies (q implies p)", "False")]
        [InlineData("forall p in BB : p or not p", "True")]
        // An index called i is the name, not the imaginary unit.
        [InlineData("forall i in {1, 2} : i > 0", "True")]
        public void DecidedOverAFiniteSet(string statement, string expected)
            => Assert.Equal(expected.ToEntity(), statement.ToEntity().Evaled);

        [Theory]
        // §4.3.4: the set matters. x^2 >= 0 over the reals and over the complex numbers.
        [InlineData("forall x in RR : x^2 >= 0", "True")]
        [InlineData("forall x in CC : x^2 >= 0", "False")]
        // §4.3.5 Try 4 (b): n = -1.
        [InlineData("forall n in ZZ : n^2 <= n^3", "False")]
        // Example 4.3.3.
        [InlineData("exists x in RR : x^2 - 4 x + 4 = 0", "True")]
        [InlineData("exists x in RR : x^2 + 1 = 0", "False")]
        [InlineData("exists x in CC : x^2 + 1 = 0", "True")]
        // §4.5.5 Try 1 (a), (c).
        [InlineData("forall x in ZZ : x > 0 or x < 0", "False")]
        [InlineData("forall x in RR : x < 0 implies x^3 < 0", "True")]
        // §4.5.5 Try 3: four false implications, each with a counterexample.
        [InlineData("forall t in RR : t^2 > 4 implies t > 2", "False")]
        [InlineData("forall x in RR : x > 3 implies x^2 < 9", "False")]
        [InlineData("forall x in RR : x < 3 implies x^2 < 9", "False")]
        [InlineData("forall t in RR : t^2 - 6 t + 9 >= 0 implies t >= 3", "False")]
        // §4.5.5 Try 4: P is 1/2 < x, Q is x < 3/2, R is x^2 = 4, S is x + 1 in ZZ+.
        [InlineData("forall x in ZZ+ : 1/2 < x", "True")]
        [InlineData("forall x in ZZ+ : x < 3/2 implies 1/2 < x", "True")]
        [InlineData("forall x in ZZ : x < 3/2 implies 1/2 < x", "False")]
        [InlineData("exists x in ZZ+ : not (x + 1 in ZZ+) or x^2 = 4", "True")]
        [InlineData("exists x in ZZ : x^2 = 4 and not (x + 1 in ZZ+)", "True")]
        [InlineData("forall x in RR : x^2 = 4 implies x + 1 in ZZ+", "False")]
        [InlineData("exists x in RR : 1/2 < x and x + 1 in ZZ+", "True")]
        // §4.2.1: the AGM inequality, and §4.9.5.
        [InlineData("forall x, y in RR : 2 x y <= x^2 + y^2", "True")]
        [InlineData("forall y in RR : y > 1 implies y^2 - 1 > 0", "True")]
        // Problem 4.11.1 over ZZ: P is 1 <= x <= 3, Q is 2 divides x, R is x^2 = 4.
        [InlineData("forall x in ZZ : (1 <= x and x <= 3) implies 2 divides x", "False")]
        [InlineData("exists x in ZZ : x^2 = 4 and 1 <= x and x <= 3", "True")]
        [InlineData("forall x in ZZ : x^2 = 4 implies 1 <= x and x <= 3", "False")]
        // The set may be an interval, a set builder, or a special set with no member of its own.
        [InlineData("forall x in (0; 1) : x^2 < 1", "True")]
        [InlineData("forall x in [0; 1] : x^2 < 1", "False")]
        [InlineData("exists x in {x in ZZ : x > 3} : x^2 = 16", "True")]
        [InlineData("forall x in {x in ZZ : x > 3} : x^2 > 9", "True")]
        [InlineData("forall x in {x in ZZ : x > 3} : x^2 > 16", "False")]
        [InlineData("exists x in ZZ : 2 x = 3", "False")]
        [InlineData("exists x in QQ : 2 x = 3", "True")]
        [InlineData("forall x in ZZ : 2 divides x", "False")]
        [InlineData("exists x in ZZ : 2 divides x", "True")]
        // Uniqueness: two real roots, one positive whole one.
        [InlineData("exists! x in RR : x^2 = 4", "False")]
        [InlineData("exists! x in ZZ+ : x^2 = 4", "True")]
        [InlineData("exists! x in RR : x + 1/x = 2", "True")]
        // §4.7: a negation passes through a quantifier by flipping it.
        [InlineData("not forall x in RR : x < 0 or x > 0", "True")]
        [InlineData("not exists x in RR : x^2 < 0", "True")]
        // A body without the name says the same of every member, and so does an identity.
        [InlineData("forall x in RR : 3 = 3", "True")]
        [InlineData("forall x in RR : x + y = y + x", "True")]
        [InlineData("forall x in RR : (x + 1)^2 = x^2 + 2 x + 1", "True")]
        [InlineData("forall x in CC : (x + y)^2 = x^2 + 2 x y + y^2", "True")]
        public void DecidedOverAnInfiniteSet(string statement, string expected)
            => Assert.Equal(expected.ToEntity(), statement.ToEntity().Simplify());

        [Theory]
        // A free variable, an unknown set, a body the solver does not read, nested quantifiers
        // over infinite sets: none is guessed.
        [InlineData("exists x in RR : x = y")]
        [InlineData("forall x in S : x > 0")]
        [InlineData("forall x in RR : sin(x) <= 1")]
        [InlineData("exists x in RR : sin(x) = 2")]
        [InlineData("forall x in RR : exists y in RR : y = x^3")]
        [InlineData("exists y in RR : forall x in RR : y = x^3")]
        public void LeftAsWrittenWhereNothingDecidesIt(string statement)
            => Assert.Equal(statement.ToEntity(), statement.ToEntity().Simplify());

        [Theory]
        [InlineData("forall x in RR : x^2 >= 0", "forall x in RR : x ^ 2 >= 0")]
        [InlineData("∀ x in RR : x^2 >= 0", "forall x in RR : x ^ 2 >= 0")]
        [InlineData("∃ x in RR : x^2 = 4", "exists x in RR : x ^ 2 = 4")]
        [InlineData("∃! x in RR : x^2 = 4", "exists! x in RR : x ^ 2 = 4")]
        [InlineData("forall x, y in RR : x + y = y + x", "forall x in RR : forall y in RR : x + y = y + x")]
        [InlineData("forall a in ZZ, b in ZZ+ : a < b", "forall a in ZZ : forall b in ZZ+ : a < b")]
        [InlineData("(forall x in RR : x^2 >= 0) and (exists x in RR : x < 0)", "(forall x in RR : x ^ 2 >= 0) and (exists x in RR : x < 0)")]
        [InlineData("not forall x in S : x > 0", "not (forall x in S : x > 0)")]
        [InlineData("forall x in S : x > 0 and x < 1", "forall x in S : x > 0 and x < 1")]
        public void PrintsAndReadsBack(string input, string printed)
        {
            var entity = input.ToEntity();
            Assert.Equal(printed, entity.ToString());
            Assert.Equal(entity, printed.ToEntity());
        }

        [Fact]
        public void Latex()
            => Assert.Equal(@"\forall x \in \mathbb{R} : {x}^{2} \geq 0", "forall x in RR : x^2 >= 0".ToEntity().Latexize());

        [Fact]
        public void TheNameIsBoundThroughoutAndTheRestIsFree()
        {
            var statement = "forall x in RR : x < y".ToEntity();
            Assert.Equal(new[] { MathS.Var("y") }, statement.FreeVariables);
            Assert.Equal(statement, statement.Substitute("x", 5));
            Assert.Equal("forall x in RR : x < 5".ToEntity(), statement.Substitute("y", 5));
            // A value that mentions the bound name is not captured by it.
            var renamed = statement.Substitute("y", "x");
            Assert.IsType<Forallf>(renamed);
            var bound = ((Forallf)renamed).Var;
            Assert.NotEqual((Entity)MathS.Var("x"), bound);
            Assert.Equal(bound < MathS.Var("x"), ((Forallf)renamed).Body);
        }

        [Fact]
        public void TheEntryPointsAgreeWithTheParser()
        {
            Assert.Equal("forall x in RR : x > 0".ToEntity(), MathS.ForAll("x", "RR", "x > 0"));
            Assert.Equal("exists x in RR : x > 0".ToEntity(), MathS.Exists("x", "RR", "x > 0"));
            Assert.Equal("exists! x in RR : x > 0".ToEntity(), MathS.ExistsUnique("x", "RR", "x > 0"));
        }

        [Fact]
        public void ANegationFlipsTheQuantifier()
        {
            Assert.IsType<Existsf>("not forall x in S : x > 0".ToEntity().Simplify());
            Assert.IsType<Forallf>("not exists x in S : x > 0".ToEntity().Simplify());
        }

        [Theory]
        // The set is mandatory, and a quantifier binds a name.
        [InlineData("forall x : x^2 >= 0")]
        [InlineData("exists x, y : x = y")]
        [InlineData("forall 3 in RR : 3 > 0")]
        [InlineData("forall x in RR")]
        public void ASetIsMandatoryAndTheNameIsAName(string input)
            => Assert.ThrowsAny<ParseException>(() => input.ToEntity());
    }
}
