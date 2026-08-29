//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;
using System.Linq;
using AngouriMath;
using AngouriMath.Core.Transformations.Matching;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    /// <summary>
    /// <see cref="MatchPattern.BuildableNodeTypes"/> names the node types
    /// <c>MatchPattern.Construct</c> builds, and <see cref="EGraph"/> resolves an e-node's
    /// operator through it. Nothing in the language keeps a list and the <see langword="switch"/>
    /// beside it in step, so these two tests do: whichever one is edited alone, one of them fails.
    /// </summary>
    /// <remarks>
    /// The e-graph used to carry its own copy of the same fourteen types. A type added to
    /// <c>Construct</c> and not to that copy was not a compiler error and not a test failure --
    /// it silently stopped being reachable from the e-graph, which is exactly the kind of gap a
    /// reflective enumeration closes and a hand-written list cannot.
    /// </remarks>
    [Trait("Area", "Transformations")]
    public sealed class BuildableNodeTypesTest
    {
        /// <summary>
        /// Every concrete node type there is. Abstract is the only exclusion, for the reason
        /// <c>EveryNodeSurvivesEveryPipelineTest</c> gives: several concrete number types are
        /// not sealed, and filtering on sealed drops them.
        /// </summary>
        private static IEnumerable<Type> ConcreteNodeTypes =>
            typeof(Entity).Assembly.GetTypes()
                .Where(type => !type.IsAbstract && typeof(Entity).IsAssignableFrom(type))
                .OrderBy(type => type.FullName, StringComparer.Ordinal);

        [Fact]
        public void EveryNodeTypeConstructBuildsIsDeclaredBuildable()
        {
            var actuallyBuildable = ConcreteNodeTypes
                .Where(type => MatchPattern.CanConstruct(type, 1) || MatchPattern.CanConstruct(type, 2))
                .OrderBy(type => type.Name, StringComparer.Ordinal)
                .ToList();
            var declared = MatchPattern.BuildableNodeTypes
                .OrderBy(type => type.Name, StringComparer.Ordinal)
                .ToList();

            Assert.Equal(declared, actuallyBuildable);
        }

        /// <summary>
        /// <see cref="MatchPattern.CanConstruct"/> is a lookup in the same table that does the
        /// building, so it says yes without proving a constructor runs. This builds every declared
        /// type for real: a constructor that threw, or that returned something of another type,
        /// would otherwise be a claim nothing checked.
        /// </summary>
        [Fact]
        public void EveryDeclaredTypeReallyBuildsAtItsOwnArity()
        {
            foreach (var type in MatchPattern.BuildableNodeTypes)
            {
                var arity = MatchPattern.CanConstruct(type, 1) ? 1 : 2;
                var children = Enumerable.Repeat((Entity)Entity.Number.Integer.Zero, arity).ToArray();

                var built = MatchPattern.ConstructNode(type, children);

                Assert.NotNull(built);
                Assert.IsType(type, built);
                // And it declines the arity it does not take, rather than reading past its children.
                Assert.Null(MatchPattern.ConstructNode(
                    type, Enumerable.Repeat((Entity)Entity.Number.Integer.Zero, 3 - arity).ToArray()));
            }
        }

        /// <summary>
        /// The lookup the e-graph goes through answers for every declared type and for nothing
        /// else -- so an operator name that reaches it either resolves to a type <c>Construct</c>
        /// really builds, or resolves to nothing at all.
        /// </summary>
        [Fact]
        public void TheNameLookupAnswersForExactlyTheDeclaredTypes()
        {
            foreach (var type in MatchPattern.BuildableNodeTypes)
                Assert.Same(type, MatchPattern.NodeTypeNamed(type.Name));

            foreach (var type in ConcreteNodeTypes.Except(MatchPattern.BuildableNodeTypes))
                Assert.Null(MatchPattern.NodeTypeNamed(type.Name));
        }

        /// <summary>
        /// Two node types printing the same <c>Name</c> would make the lookup ambiguous, and the
        /// e-graph keys e-nodes on that name -- so the two would share one e-class.
        /// </summary>
        [Fact]
        public void NoTwoBuildableNodeTypesShareAName()
            => Assert.Equal(
                MatchPattern.BuildableNodeTypes.Count,
                MatchPattern.BuildableNodeTypes.Select(type => type.Name).Distinct().Count());
    }
}
