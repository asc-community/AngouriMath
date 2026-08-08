//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Core.Transformations;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Exporters.Csv;

namespace DotnetBenchmark
{
    /// <summary>
    /// The transformation layer, measured for time and allocation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Two claims are made about this layer and neither is safe without something that
    /// measures them: that routing the 1.x entry points through it costs nothing, and that
    /// recording rewrites costs nothing when nobody is recording. Both have already been
    /// broken once during development -- the second by a closure the compiler allocated at
    /// method entry regardless of the early return, which cost <c>Simplify</c> a fifth of
    /// its allocation with the feature switched off, and which no test would have noticed.
    /// </para>
    /// <para>
    /// Read the allocation column first. It is deterministic and reproduces to the tenth of
    /// a kilobyte, where the timings on an ordinary machine vary by ten percent run to run
    /// and hide exactly this kind of regression.
    /// </para>
    /// </remarks>
    [ArtifactsPath(@"./benchmark_results.csv")]
    [CsvExporter(CsvSeparator.Semicolon)]
    [MemoryDiagnoser]
    public class TransformationLayer
    {
        private static readonly Entity simplifyInput = "x + 3 / 3 + x ^ 0 - log(e, e2)";
        private static readonly Entity quotientInput = "(x ^ 3 + 3 * x ^ 2 * y + 3 * x * y ^ 2 + y ^ 3) / (x + y)";
        private static readonly Entity expandInput = "(x + y) ^ 6";
        private static readonly Entity factorizeInput = "x * y + y + 1 + x";
        private static readonly Entity surdInput = "1 / (sqrt(3) + 5)";
        private static readonly Entity unorderedInput = "z + y + x + sin(b) + a";
        private static readonly Entity derivativeInput = "x + 3 + arccos(x + 2) / sqrt(x2 + 1)";

        // The 1.x surface, which now reaches its algorithm through the layer. These are the
        // numbers to compare against a build from before the layer existed.
        [Benchmark] public void Simplify() => simplifyInput.Simplify();
        [Benchmark] public void SimplifyQuotient() => quotientInput.Simplify();
        [Benchmark] public void Expand() => expandInput.Expand();
        [Benchmark] public void Factorize() => factorizeInput.Factorize();
        [Benchmark] public void Differentiate() => derivativeInput.Differentiate("x");

        // The layer reached directly.
        [Benchmark] public void SimplificationTransformation() => Transformation.Simplification.Apply(simplifyInput);
        [Benchmark] public void Normalization() => Transformation.Normalization.Apply(unorderedInput);
        [Benchmark] public void Rationalisation() => Transformation.Rationalisation.Apply(surdInput);
        [Benchmark] public void Substitution() => Transformation.Substitution("x", 3).Apply(quotientInput);

        // A single rewrite pass, the unit everything above is built out of.
        [Benchmark] public void OneRewritePass() => RewriteRules.Common.ApplyOnce(quotientInput);
        [Benchmark] public void OneRewritePassThatMatchesNothing() => RewriteRules.PhiFunction.ApplyOnce(quotientInput);

        // The pair that matters. SimplifyWhileRecording is expected to cost more -- it
        // collects a step per rewrite. Simplify above is the one that must not move: it is
        // the same call with nobody listening, and if the two ever converge, the recording
        // machinery has leaked into the ordinary path.
        [Benchmark]
        public void SimplifyWhileRecording()
        {
            using var recording = RewriteRecording.Start();
            simplifyInput.Simplify();
        }
    }
}
