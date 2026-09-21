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

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// A <see cref="ProofRecording"/> collects the steps by which a quantified statement was
    /// decided: each names the rule, as Sullivan and Mackey name it and as a Lean 4 tactic
    /// would, and what was tried and abandoned is not among them. Item 8 of the
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1409">#1409</a> docket: a
    /// proof is the trace of the decisions, recorded rather than discarded.
    /// </summary>
    [Trait("Area", "Discrete")]
    public sealed class ProofRecordingTest
    {
        // Parsed without the cache: a statement evaluated once is evaluated, and the steps of a
        // decision already made are not made again -- a recording sees fresh decisions only.
        private static Entity Fresh(string statement) => MathS.FromString(statement, useCache: false);

        private static (Entity verdict, ProofStep[] steps) Prove(string statement)
        {
            using var proof = ProofRecording.Start();
            var verdict = Fresh(statement).Evaled;
            return (verdict, proof.Steps.ToArray());
        }

        [Fact]
        public void AnInductionIsABaseCaseAndAStep()
        {
            var (verdict, steps) = Prove("forall n in ZZ+ : sum(1/(k (k + 1)), k, 1, n) = n/(n + 1)");
            Assert.Equal(Entity.Boolean.True, verdict);
            var last = steps[^1];
            Assert.Equal(0, last.Depth);
            Assert.Contains("induction", last.Rule);
            Assert.Equal("Nat.le_induction", last.Lemma);
            // The base case, evaluated, and the step, an identity once the denominators are
            // seen to be non-zero on the set -- each one level below the induction.
            Assert.Contains(steps, step => step.Depth == 1 && step.Rule.Contains("base case") && step.Verdict == Entity.Boolean.True);
            Assert.Contains(steps, step => step.Depth == 1 && step.Lemma == "ring" && step.Verdict == Entity.Boolean.True);
            Assert.Contains(steps, step => step.Depth == 2 && step.Statement is Existsf && step.Verdict == Entity.Boolean.False);
        }

        [Fact]
        public void AClosedFormIsReadThroughItsCase()
        {
            var (_, steps) = Prove("forall n in ZZ+ : sum(k, k, 1, n) = n (n + 1) / 2");
            Assert.Contains(steps, step => step.Depth == 0 && step.Rule.Contains("piecewise") && step.Lemma == "if_pos");
            Assert.Contains(steps, step => step.Depth == 1 && step.Lemma == "ring");
        }

        [Theory]
        [InlineData("forall n in ZZ : 6 divides n^3 + 5 n", "residues", "decide")]
        [InlineData("forall x in RR : x^2 >= 0", "no solution", "nlinarith")]
        [InlineData("exists x in ZZ : x^2 = 4", "a member", "exact ⟨_, by decide⟩")]
        [InlineData("forall b in RR : exists a in RR : a^3 = b", "image", "Set.range_subset_iff")]
        [InlineData("forall n in ZZ+ : 3^n >= 2^(n + 1)", "least member", "decide")]
        public void EachRouteNamesItsRuleAndItsLemma(string statement, string rule, string lemma)
        {
            var (_, steps) = Prove(statement);
            var last = steps[^1];
            Assert.Equal(0, last.Depth);
            Assert.Contains(rule, last.Rule);
            Assert.Equal(lemma, last.Lemma);
        }

        [Fact]
        public void ACutSetIsShiftedAndTheInductionBelowIt()
        {
            var (verdict, steps) = Prove("forall n in ZZ+ /\\ [5; +oo) : 2^n > n^2");
            Assert.Equal(Entity.Boolean.True, verdict);
            Assert.Contains(steps, step => step.Depth == 0 && step.Rule.Contains("shifted by 5"));
            Assert.Contains(steps, step => step.Depth == 1 && step.Rule.Contains("induction from 0"));
            Assert.Contains(steps, step => step.Depth == 2 && step.Rule.Contains("base case"));
        }

        [Fact]
        public void WhatWasTriedAndAbandonedIsNotAStep()
        {
            // The identity route is tried on the sum before the induction and declines; the
            // sign calculus tries a multiplier that fails before the one that works. Neither
            // attempt's sub-decisions survive.
            var (_, steps) = Prove("forall n in ZZ+ /\\ [5; +oo) : 2^n > n^2");
            Assert.DoesNotContain(steps, step => step.Verdict == Entity.Boolean.False);
            var (_, induction) = Prove("forall n in ZZ+ : sum(1/(k (k + 1)), k, 1, n) = n/(n + 1)");
            Assert.All(induction.Where(step => step.Verdict == Entity.Boolean.False), step => Assert.IsType<Existsf>(step.Statement));
        }

        [Fact]
        public void NothingIsRecordedOutsideAScope()
        {
            ProofStep[] inside;
            using (var proof = ProofRecording.Start())
            {
                _ = Fresh("forall n in ZZ : 6 divides n^3 + 5 n").Evaled;
                inside = proof.Steps.ToArray();
                proof.Dispose();
                _ = Fresh("forall n in ZZ : 2 divides n^2 + n").Evaled;
                Assert.Equal(inside.Length, proof.Steps.Count);
            }
            Assert.NotEmpty(inside);
            // And a fresh statement outside any scope evaluates with no scope to record into.
            Assert.Equal(Entity.Boolean.True, Fresh("forall n in ZZ : 2 divides n^2 + n").Evaled);
        }

        [Fact]
        public void TheWrittenFormIndentsSubStepsUnderTheirStatement()
        {
            using var proof = ProofRecording.Start();
            _ = Fresh("forall n in ZZ+ /\\ [5; +oo) : 2^n > n^2").Evaled;
            var written = proof.Written();
            Assert.StartsWith("    2 ^ (5 + 0) > (5 + 0) ^ 2 is True: the base case", written);
            Assert.EndsWith("[Nat.le_induction]\n", written.Replace("\r\n", "\n"));
        }
    }
}
