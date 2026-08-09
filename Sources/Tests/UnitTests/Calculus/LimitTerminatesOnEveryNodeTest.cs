//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// A limit over any node terminates.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="Entity.ComputeLimitDivideEtImpera"/> used to default to
    /// <c>new Limitf(this, ...)</c>, so a node that did not override it crashed the process:
    /// the caller evaluates the returned node to compare it, evaluating a <c>Limitf</c>
    /// computes the limit, and computing arrives back at the default. Seven node types
    /// inherited it and every one of them overflowed the stack —
    /// <c>floor</c>, <c>ceil</c>, <c>round</c>, <c>min</c>, <c>max</c>, <c>gcd</c> and
    /// <c>phi</c>.
    /// </para>
    /// <para>
    /// The per-node regression tests could not have caught it, because the defect is in what
    /// a node inherits by <i>not</i> being mentioned. So this enumerates the node types
    /// instead of listing them: a node added tomorrow is covered on the day it is added,
    /// which is the only shape of test that fixes this class of bug rather than its
    /// instances.
    /// </para>
    /// <para>
    /// Termination is the assertion, not the answer. Most of these have no limit this
    /// library can compute and come back unevaluated, which is the honest result; what is
    /// forbidden is not returning at all.
    /// </para>
    /// https://github.com/asc-community/AngouriMath/issues/829
    /// https://github.com/asc-community/AngouriMath/issues/833
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class LimitTerminatesOnEveryNodeTest
    {
        /// <summary>
        /// One instance of every concrete node type under <see cref="Entity"/> that can be
        /// built from a variable, so that the limit has something to descend into.
        /// </summary>
        public static IEnumerable<object[]> EveryNodeType()
        {
            var x = MathS.Var("x");
            var built = new List<Entity>();

            foreach (var type in typeof(Entity).Assembly
                         .GetTypes()
                         .Where(t => t.IsSealed && !t.IsAbstract && typeof(Entity).IsAssignableFrom(t))
                         .OrderBy(t => t.FullName, StringComparer.Ordinal))
            {
                // The one-argument constructor over Entity is the common shape; where a node
                // takes more, fill every parameter it will accept with the variable.
                var constructor = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                    .Where(c => c.GetParameters().Length > 0
                                && c.GetParameters().All(p => p.ParameterType == typeof(Entity)))
                    .OrderBy(c => c.GetParameters().Length)
                    .FirstOrDefault();
                if (constructor is null)
                    continue;

                Entity node;
                try
                {
                    node = (Entity)constructor.Invoke(
                        constructor.GetParameters().Select(_ => (object)x).ToArray());
                }
                catch (Exception)
                {
                    // A node whose constructor rejects a plain variable is not the subject
                    // here; the point is the ones that accept one and then recurse.
                    continue;
                }
                built.Add(node);
            }

            Assert.NotEmpty(built);
            return built.Select(n => new object[] { n });
        }

        [Theory]
        [MemberData(nameof(EveryNodeType))]
        public void ALimitOverAnyNodeTerminates(Entity node)
        {
            // In a task with a timeout rather than measured for speed: a regression here is
            // non-termination, and the run must fail rather than hang the suite. A stack
            // overflow cannot be caught at all, so the guard is that the assembly gets to
            // run its remaining tests.
            var finished = Task.Run(() =>
            {
                foreach (var destination in new Entity[] { 0, "+oo".ToEntity(), "-oo".ToEntity() })
                    foreach (var side in new[] { ApproachFrom.BothSides, ApproachFrom.Left, ApproachFrom.Right })
                        node.Limit(MathS.Var("x"), destination, side);
            }).Wait(TimeSpan.FromSeconds(30));

            Assert.True(finished, $"a limit over {node.Stringize()} did not terminate");
        }
    }
}
