//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Core.Transformations;
using AngouriMath.Core.Transformations.Matching;
using AngouriMath.Functions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Exporters.Csv;

namespace DotnetBenchmark
{
    /// <summary>
    /// A rewrite rule as an arm of a <c>switch</c> against the same rule as data, on the same
    /// input, one node at a time.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This measures the thing that decides whether
    /// <a href="https://github.com/asc-community/AngouriMath/issues/248">#248</a> can actually
    /// happen. <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a> v2.0
    /// says a slower <c>Simplify</c> for the common case is unacceptable and that the fast path
    /// must survive as a path. A <c>switch</c> arm is a type test and a few field reads; a
    /// pattern is an object walk that allocates a <c>Bindings</c> per hole and enumerates with
    /// iterators. It would be surprising if they cost the same, and the question is whether the
    /// difference is a factor to engineer around or a factor that ends the design.
    /// </para>
    /// <para>
    /// <b>The miss is the case that matters.</b> A rewrite pass asks every rule about every node,
    /// so nearly every question asked of a rule set is answered "no". A form that is quick to
    /// accept and slow to decline would be slower in practice than these numbers suggest if only
    /// the hits were measured, which is why each set is measured on a node it rewrites and on one
    /// it does not.
    /// </para>
    /// <para>
    /// Read the allocation column first, as with <see cref="TransformationLayer"/>: it is
    /// deterministic where the timings on an ordinary machine move ten percent between runs.
    /// </para>
    /// </remarks>
    [ArtifactsPath(@"./benchmark_results.csv")]
    [CsvExporter(CsvSeparator.Semicolon)]
    [MemoryDiagnoser]
    public class MatchingEngine
    {
        // A node the DivisionPreparing set rewrites -- a * (1 / b) -- and one it does not.
        private static readonly Entity divisionHit = "a * (1 / b)";
        private static readonly Entity divisionMiss = "a + b";

        // The Pythagorean identity as the switch sees it: two terms of one sum.
        private static readonly Entity pythagorasAdjacent = "sin(x) ^ 2 + cos(x) ^ 2";

        // And buried, where the switch cannot see it at all and Gathered can. There is no
        // switch counterpart to compare against here -- that is the point of the rule -- so
        // this measures what the new capability costs, not a like-for-like difference.
        private static readonly Entity pythagorasBuried = "a + sin(x) ^ 2 + b + cos(x) ^ 2 + c";

        // A long sum with nothing to find: the shape that would make an n-ary matcher expensive
        // if it were going to be, since every operand is tried in every position.
        private static readonly Entity longMiss = "aa + ab + ac + ad + ae + af + ag + ah";

        [Benchmark(Baseline = true)]
        public Entity DivisionSwitchHit() => Patterns.DivisionPreparingRules(divisionHit);

        [Benchmark]
        public Entity DivisionDataHit() => MatchedRules.DivisionPreparing.ApplyHere(divisionHit);

        [Benchmark]
        public Entity DivisionSwitchMiss() => Patterns.DivisionPreparingRules(divisionMiss);

        [Benchmark]
        public Entity DivisionDataMiss() => MatchedRules.DivisionPreparing.ApplyHere(divisionMiss);

        [Benchmark]
        public Entity PythagorasSwitchAdjacent() => Patterns.TrigonometricRules(pythagorasAdjacent);

        [Benchmark]
        public Entity PythagorasDataAdjacent()
            => MatchedRules.PythagoreanIdentity.ApplyHere(pythagorasAdjacent);

        [Benchmark]
        public Entity PythagorasDataBuried()
            => MatchedRules.PythagoreanIdentity.ApplyHere(pythagorasBuried);

        [Benchmark]
        public Entity PythagorasDataLongMiss()
            => MatchedRules.PythagoreanIdentity.ApplyHere(longMiss);

        // ---- One whole pass over a tree, which is the unit the pipeline actually spends ----
        //
        // Everything above is one node. What decides whether the migration is affordable is a
        // pass: every node of a real expression asked, where most nodes are the wrong shape and
        // the answer is no. The two sets here are the same rules -- the switch is
        // Patterns.DivisionPreparingRules and the data set was proven to agree with it in #938 --
        // so this is a like-for-like swap of one rule set inside the machinery that uses it.

        private static readonly Entity passInput =
            "(x ^ 3 + 3 * x ^ 2 * y + 3 * x * y ^ 2 + y ^ 3) / (x + y) + a * (1 / b) + sin(x) / 2";

        private static readonly RewriteRuleSet divisionAsData = new(
            "DivisionPreparingAsData",
            "The same rules as RewriteRules.DivisionPreparing, expressed as data.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            MatchedRules.DivisionPreparing.ApplyHere);

        [Benchmark]
        public Entity PassSwitch() => RewriteRules.DivisionPreparing.ApplyOnce(passInput);

        [Benchmark]
        public Entity PassData() => divisionAsData.ApplyOnce(passInput);
    }
}
