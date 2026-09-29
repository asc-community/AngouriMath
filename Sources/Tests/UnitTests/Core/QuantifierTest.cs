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
        // The members that satisfy the conditions on the name the solver reads, listed or from a
        // least one, with the other conditions kept: Prob 8.9.24 at p = 7, where every C(7, k) is
        // a multiple of 7, and at 6, where C(6, 2) = 15 is not; Ex 5.3.2 from its threshold.
        [InlineData("forall k in ZZ : (0 < k and k < 7) implies 7 divides binomial(7, k)", "True")]
        [InlineData("forall k in ZZ : (0 < k and k < 6) implies 6 divides binomial(6, k)", "False")]
        [InlineData("exists k in ZZ : (0 < k and k < 6) and not 6 divides binomial(6, k)", "True")]
        [InlineData("exists! k in ZZ : (0 < k and k < 7) and 3 divides k and 2 divides k", "True")]
        [InlineData("forall k in ZZ : (0 < k and k < 7 and 2 divides k) implies 3 divides binomial(7, k) - 1", "False")]
        [InlineData("forall n in ZZ : n >= 5 implies 2^n > n^2", "True")]
        // Sullivan and Mackey's coins and special lands (Probs 2.7.8 and 5.7.4, §5.5.3 Try 3).
        // What n = p a + q b leaves of b is a divisibility and a sign, the least a is below a
        // period, and past a threshold the rest repeats: 13 is the largest sum 3- and 8-coins
        // do not make, 23 the largest of 4 and 9.
        [InlineData("exists a in ZZ* : exists b in ZZ* : 13 = 3 a + 8 b", "False")]
        [InlineData("exists a in ZZ* : exists b in ZZ* : 12 = 3 a + 8 b", "True")]
        [InlineData("exists a in ZZ* : exists b in ZZ* : 29 = 7 a + 6 b", "False")]
        [InlineData("exists a in ZZ+ : exists b in ZZ+ : 6 = 2 a + 3 b", "False")]
        [InlineData("exists a in ZZ+ : exists b in ZZ+ : 7 = 2 a + 3 b", "True")]
        [InlineData("forall n in ZZ : n >= 2 implies (exists a in ZZ* : exists b in ZZ* : n = 2 a + 3 b)", "True")]
        [InlineData("forall n in ZZ : n >= 14 implies (exists a in ZZ* : exists b in ZZ* : n = 3 a + 8 b)", "True")]
        [InlineData("forall n in ZZ : n >= 24 implies (exists a in ZZ* : exists b in ZZ* : n = 4 a + 9 b)", "True")]
        [InlineData("forall n in ZZ : n >= 23 implies (exists a in ZZ* : exists b in ZZ* : n = 4 a + 9 b)", "False")]
        // Floors and ceilings of n over a whole number beside n itself repeat where the drifts
        // cancel, and the residues decide them: floor(n/2) + ceil(n/2) = n, and Hermite's
        // identity for thirds.
        [InlineData("forall n in ZZ : floor(n/2) + ceil(n/2) = n", "True")]
        [InlineData("forall n in ZZ : floor(n/3) + floor((n + 1)/3) + floor((n + 2)/3) = n", "True")]
        [InlineData("forall n in ZZ : floor(n/2) = n/2", "False")]
        [InlineData("forall n in ZZ+ : ceil(n/2) - floor(n/2) <= 1", "True")]
        [InlineData("exists n in ZZ : floor(n/2) + ceil(n/2) = n + 1", "False")]
        // Prob 8.9.24 for every prime: that p is prime and 0 < k < p is established by the
        // quantifiers around the claim, in either order and with the hypothesis curried, and the
        // rule for p divides binomial(p, k) reads it. Without the primality, or without either
        // bound, it fails. An inner quantifier that binds p again hides what the outer one
        // established about it.
        [InlineData("forall p in PP : forall k in ZZ : (0 < k and k < p) implies p divides binomial(p, k)", "True")]
        [InlineData("forall k in ZZ : forall p in PP : (0 < k and k < p) implies p divides binomial(p, k)", "True")]
        [InlineData("forall p in PP : forall k in ZZ : 0 < k implies (k < p implies p divides binomial(p, k))", "True")]
        [InlineData("forall p in PP : forall k in ZZ+ : k < p implies p divides binomial(p, k)", "True")]
        [InlineData("forall p in PP : forall k in ZZ : (1 <= k and k <= p - 1) implies p divides binomial(p, k)", "True")]
        [InlineData("forall p in PP : p divides binomial(p, 1)", "True")]
        // Not for k = p: binomial(2, 2) = 1, which 2 does not divide.
        [InlineData("forall p in PP : p divides binomial(p, 2)", "False")]
        [InlineData("forall p in ZZ+ : forall k in ZZ : (0 < k and k < p) implies p divides binomial(p, k)", "False")]
        [InlineData("forall p in PP : forall k in ZZ : k < p implies p divides binomial(p, k)", "False")]
        [InlineData("forall p in PP : forall k in ZZ : (0 < k and k <= p) implies p divides binomial(p, k)", "False")]
        [InlineData("forall p in PP : forall p in ZZ+ : forall k in ZZ : (0 < k and k < p) implies p divides binomial(p, k)", "False")]
        // Over the primes, a statement repeating modulo M is its prime factors and its units: past
        // 3 every prime is a unit modulo 24 and p^2 - 1 a multiple of it, while 3 itself is not,
        // Sullivan and Mackey's Prob 6.7.5; and each unit is the residue of infinitely many primes.
        [InlineData("forall p in PP : p > 3 implies 24 divides p^2 - 1", "True")]
        [InlineData("forall p in PP : p > 2 implies 24 divides p^2 - 1", "False")]
        [InlineData("forall p in PP : p > 5 implies 240 divides p^4 - 1", "True")]
        [InlineData("forall p in PP : p > 2 implies 2 divides p + 1", "True")]
        [InlineData("exists p in PP : p > 3 and 3 divides p", "False")]
        [InlineData("exists p in PP : p > 10 and p = 3 (mod 4)", "True")]
        // Sullivan and Mackey's Prob 4.11.6: for whole x and y the witnesses are x - y and y - x,
        // and one of them is not negative. Not so in ZZ+, where x = y leaves 0, nor for reals,
        // where 1/2 and 0 leave ±1/2.
        [InlineData("forall x in ZZ : forall y in ZZ : exists z in ZZ* : x - y = z or y - x = z", "True")]
        [InlineData("forall x in ZZ : forall y in ZZ : exists z in ZZ+ : x - y = z or y - x = z", "False")]
        [InlineData("forall x in RR : forall y in RR : exists z in ZZ* : x - y = z or y - x = z", "False")]
        // Ex 7.2.6: f(z) = |2 z + 1| is a function from ZZ to Sullivan and Mackey's N, which is
        // ZZ+, since the modulus of a whole number is one, and 2 z + 1 = 0 has no whole root.
        // |z| is not, at 0, nor |z^2 - 4|, at 2.
        [InlineData("forall z in ZZ : abs(2 z + 1) in ZZ+", "True")]
        [InlineData("forall z in ZZ : abs(z) in ZZ+", "False")]
        [InlineData("forall z in ZZ : abs(z^2 - 4) in ZZ+", "False")]
        [InlineData("forall z in ZZ : abs(z - 3) in ZZ*", "True")]
        [InlineData("forall n in ZZ+ : abs(n) in ZZ+", "True")]
        // Prob 4.11.5: a square is not the greatest square, for y = x^2 + 1 is past every x, and
        // a term in x is a witness for every x at once. Where none of the terms tried works, a
        // single member may still decide it: x = 0 is below every square.
        [InlineData("exists x in RR : forall y in RR : x^2 - y^2 >= 0", "False")]
        [InlineData("forall x in RR : exists y in RR : y > x^2", "True")]
        [InlineData("exists x in ZZ : forall y in ZZ : x <= y", "False")]
        [InlineData("forall x in ZZ+ : exists y in ZZ+ : y > x", "True")]
        [InlineData("exists x in RR : forall y in RR : x <= y^2", "True")]
        // No number is every cube: whatever y is, the cube of x = y^2 + 1 is not y.
        [InlineData("exists y in RR : forall x in RR : y = x^3", "False")]
        [InlineData("forall x in RR : x^2 + 1 > x^2", "True")]
        [InlineData("forall x in RR : (x + 1)^2 < x^2 + 2 x", "False")]
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
        // A condition the hypothesis states is no condition on the claim.
        [InlineData("forall x in RR : not x = 0 implies x / x = 1", "True")]
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
        // A body that is True holds at every member and one that is False at none, whatever the
        // members are, and whether or not there are any.
        [InlineData("forall x in { x in RR : sin(x) > 0 } : True", "True")]
        [InlineData("exists x in A \\/ B : False", "False")]
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
        public void LeftAsWrittenWhereNothingDecidesIt(string statement)
            => Assert.Equal(statement.ToEntity(), statement.ToEntity().Simplify());

        [Fact]
        public void AWholeNumberIsNotAssumedOfAReal()
            // binomial(p, k) of a real k is not a whole number, so the rule for divisibility has
            // nothing to read, and the statement is not decided.
            => Assert.IsType<Forallf>("forall p in PP : forall k in RR : (0 < k and k < p) implies p divides binomial(p, k)".ToEntity().Simplify());

        [Fact]
        public void WhatTheQuantifiersEstablishDoesNotLeaveThem()
        {
            // One node, shared with the caller, that is True inside the statement only because the
            // quantifiers around it establish that p is prime and 0 < k < p. Outside them it is
            // not decided, after the statement has been decided as before.
            var claim = MathS.FromString("p divides binomial(p, k)", useCache: false);
            var statement = MathS.ForAll("p", "PP", MathS.ForAll("k", "ZZ", MathS.FromString("0 < k and k < p", useCache: false).Implies(claim)));
            Assert.Equal(Entity.Boolean.True, statement.Evaled);
            Assert.Equal(Entity.Boolean.True, statement.Simplify());
            Assert.IsType<Dividesf>(claim.Evaled);
            Assert.IsType<Dividesf>(claim.InnerSimplified);
            Assert.IsType<Dividesf>(claim.Simplify());
        }

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

        /// <summary>
        /// A quantifier after a connective takes the rest of the line, as it does at the start of
        /// one, so the brackets it needed there are optional: Sullivan and Mackey's §5.5.3 Try 3 as
        /// they write it. https://github.com/asc-community/AngouriMath/issues/1409
        /// </summary>
        [Theory]
        [InlineData("p implies forall x in S : x > 0", "p implies (forall x in S : x > 0)")]
        [InlineData("p -> exists x in S : x > 0 and x < 1", "p -> (exists x in S : x > 0 and x < 1)")]
        [InlineData("p and exists x in S : x > 0 or x < 1", "p and (exists x in S : x > 0 or x < 1)")]
        [InlineData("p & forall x in S : x > 0", "p & (forall x in S : x > 0)")]
        [InlineData("p or exists! x in S : x = 0", "p or (exists! x in S : x = 0)")]
        [InlineData("p xor forall x in S : x > 0", "p xor (forall x in S : x > 0)")]
        [InlineData("forall n in ZZ : n >= 2 implies exists a in ZZ* : exists b in ZZ* : n = 2 a + 3 b",
            "forall n in ZZ : n >= 2 implies (exists a in ZZ* : exists b in ZZ* : n = 2 a + 3 b)")]
        public void AQuantifierAfterAConnectiveTakesTheRestOfTheLine(string written, string bracketed)
            => Assert.Equal(bracketed.ToEntity(), written.ToEntity());

        [Fact]
        public void TheCoinsReadAsTheBookWritesThem()
            => Assert.Equal(Entity.Boolean.True,
                "forall n in ZZ : n >= 2 implies exists a in ZZ* : exists b in ZZ* : n = 2 a + 3 b".ToEntity().Simplify());

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
