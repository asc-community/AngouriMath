//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Linq;
using Xunit;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// What the kernel assembly references, held to a written list.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>The gate <c>Docs/Contributing/Packaging.md</c> asks for and did not have.</b> §7 proposed
    /// "a check that fails on the commit that adds a dependency rather than on the release that
    /// ships it", and §11 states the kernel's third-party set as a list of four whose growth is a
    /// packaging decision rather than an implementation detail. Neither was enforced by anything.
    /// </para>
    /// <para>
    /// <b>It had already drifted before this was written.</b> The document records 13 referenced
    /// assemblies; the count had become 14, because <c>System.Text.Json</c> arrived with
    /// <c>Core/Serialization</c>. Nothing reached a consumer's restore, since the NuGet groups did
    /// not change — but that is the difference between "no harm done" and "noticed", and only the
    /// second is a property.
    /// </para>
    /// <para>
    /// <b>What this can and cannot see.</b> <c>UnitTests</c> targets one framework, so this is the
    /// reference set of that leg. The <c>netstandard2.0</c> build carries <c>System.Memory</c> in
    /// addition and is checked by nothing here; that is a real gap and a separate piece of work,
    /// not something to paper over by loosening this. A reference is also not a package: the
    /// framework assemblies below are part of the shared framework and cost a consumer nothing,
    /// which is why the third-party ones are counted separately.
    /// </para>
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class KernelDependenciesTest
    {
        /// <summary>
        /// The four in <c>Packaging.md</c> §11. Adding to this list is a packaging decision:
        /// it changes what every consumer restores, so it belongs in a pull request that says so.
        /// </summary>
        /// <remarks>
        /// Assembly names, not package names, and one of them differs: the package
        /// <c>PeterO.Numbers</c> ships an assembly called <c>Numbers</c>. Worth knowing before
        /// grepping for the wrong string.
        /// </remarks>
        private static readonly string[] ThirdParty =
        {
            "GenericTensor",
            "HonkSharp",
            "Numbers",
            "Antlr4.Runtime.Standard",
        };

        private static string[] Referenced() =>
            typeof(MathS).Assembly
                .GetReferencedAssemblies()
                .Select(name => name.Name!)
                .OrderBy(name => name, System.StringComparer.Ordinal)
                .ToArray();

        [Fact]
        public void TheKernelReferencesNothingItIsNotRecordedAsReferencing()
        {
            var expected = new[]
            {
                "Antlr4.Runtime.Standard",
                "GenericTensor",
                "HonkSharp",
                "Numbers",
                "System.Collections",
                "System.Collections.Concurrent",
                "System.Console",
                "System.Linq",
                "System.Linq.Expressions",
                "System.Memory",
                "System.Runtime",
                "System.Runtime.Numerics",
                "System.Text.Json",
                "System.Threading",
            };

            var actual = Referenced();

            var added = actual.Except(expected).ToList();
            Assert.True(added.Count == 0,
                $"the kernel references {added.Count} assemblies this list does not record: "
                + string.Join(", ", added)
                + ". Adding one is a packaging decision — see Docs/Contributing/Packaging.md §11 — "
                + "so record it here in the same change that adds it.");

            // The other direction, so the list cannot outlive what it describes: a dependency that
            // goes away should be deleted here rather than left asserting nothing.
            var gone = expected.Except(actual).ToList();
            Assert.True(gone.Count == 0,
                $"{gone.Count} assemblies are recorded here and no longer referenced, and should be "
                + "deleted: " + string.Join(", ", gone));
        }

        /// <summary>
        /// The number that matters to a consumer, separately from the framework ones, because it
        /// is what a restore actually fetches.
        /// </summary>
        [Fact]
        public void TheThirdPartyDependenciesAreTheFourThatWereAgreed()
        {
            var actual = Referenced()
                .Where(name => !name.StartsWith("System.", System.StringComparison.Ordinal))
                .OrderBy(name => name, System.StringComparer.Ordinal)
                .ToArray();

            Assert.Equal(
                ThirdParty.OrderBy(name => name, System.StringComparer.Ordinal).ToArray(),
                actual);
        }
    }
}
