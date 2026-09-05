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

        /// <summary>
        /// Whether a reference is part of the framework rather than something a consumer restores.
        /// </summary>
        /// <remarks>
        /// <c>netstandard</c> is on this list because it is the reference assembly the
        /// netstandard2.0 leg resolves against, and it appears or does not depending on which leg
        /// is loaded. The first version of this file asserted an exact set that had been measured
        /// on one leg, passed locally on <c>net10.0</c>, and failed the <c>C# Test</c> workflow on
        /// exactly that name. Which framework assemblies appear is a fact about the build leg;
        /// which third-party ones appear is the packaging decision, and only the second is
        /// asserted exactly.
        /// </remarks>
        private static bool IsFramework(string name) =>
            name.StartsWith("System.", System.StringComparison.Ordinal)
            || name is "System" or "netstandard" or "mscorlib";

        private static string[] Referenced() =>
            typeof(MathS).Assembly
                .GetReferencedAssemblies()
                .Select(name => name.Name!)
                .OrderBy(name => name, System.StringComparer.Ordinal)
                .ToArray();

        /// <summary>
        /// The framework assemblies seen so far, across the legs this has run on. A superset rather
        /// than an equality: a leg that resolves fewer of them is not a packaging event, while one
        /// that pulls in something new — <c>System.Text.Json</c> arriving with
        /// <c>Core/Serialization</c> is the case in point — is exactly what wants seeing.
        /// </summary>
        private static readonly string[] Framework =
        {
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
            "netstandard",
        };

        [Fact]
        public void TheKernelReferencesNothingItIsNotRecordedAsReferencing()
        {
            var recorded = Framework.Concat(ThirdParty).ToList();
            var added = Referenced().Except(recorded).ToList();

            Assert.True(added.Count == 0,
                $"the kernel references {added.Count} assemblies this list does not record: "
                + string.Join(", ", added)
                + ". Adding one is a packaging decision — see Docs/Contributing/Packaging.md §11 — "
                + "so record it here in the same change that adds it.");
        }

        /// <summary>
        /// The set that matters to a consumer, separately from the framework ones, because it is
        /// what a restore actually fetches. Asserted exactly, in both directions.
        /// </summary>
        [Fact]
        public void TheThirdPartyDependenciesAreTheFourThatWereAgreed()
        {
            var actual = Referenced()
                .Where(name => !IsFramework(name))
                .OrderBy(name => name, System.StringComparer.Ordinal)
                .ToArray();

            Assert.Equal(
                ThirdParty.OrderBy(name => name, System.StringComparer.Ordinal).ToArray(),
                actual);
        }
    }
}
