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
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Common
{
    /// <summary>
    /// Every node survives every pipeline: no crash, no hang, no exception the caller has no
    /// reason to expect.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The generalisation of <c>LimitTerminatesOnEveryNodeTest</c>. Limits were where the
    /// hole showed — seven node types killed the process by inheriting a default that
    /// returned the node instead of <see langword="null"/> — but nothing made limits special.
    /// A node reaches a dozen pipelines and <c>Docs/Contributing/AddingNode.cs</c> lists
    /// them; anything missed there is missed silently, because the node that breaks is
    /// exactly the one nobody wrote a test for.
    /// </para>
    /// <para>
    /// So the node types are enumerated rather than listed, and each is put through every
    /// pipeline. What is asserted is only what every pipeline owes a caller: return, in
    /// finite time, either an answer or an <see cref="AngouriMath.Core.Exceptions.AngouriMathBaseException"/>.
    /// Not that the answer is right — the per-node tests are for that — and not that there
    /// is one at all, since an unevaluated node is a legitimate answer here.
    /// </para>
    /// https://github.com/asc-community/AngouriMath/issues/829
    /// </remarks>
    [Trait("Area", "Common")]
    public sealed class EveryNodeSurvivesEveryPipelineTest
    {
        private static readonly Entity.Variable X = MathS.Var("x");

        public static IEnumerable<object[]> EveryNodeType()
        {
            var built = new List<Entity>();
            foreach (var type in typeof(Entity).Assembly.GetTypes()
                         .Where(t => t.IsSealed && !t.IsAbstract && typeof(Entity).IsAssignableFrom(t))
                         .OrderBy(t => t.FullName, StringComparer.Ordinal))
            {
                var constructor = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                    .Where(c => c.GetParameters().Length > 0
                                && c.GetParameters().All(p => p.ParameterType == typeof(Entity)))
                    .OrderBy(c => c.GetParameters().Length)
                    .FirstOrDefault();
                if (constructor is null)
                    continue;
                try
                {
                    built.Add((Entity)constructor.Invoke(
                        constructor.GetParameters().Select(_ => (object)(Entity)X).ToArray()));
                }
                catch (Exception) { /* a node that will not take a bare variable is not the subject */ }
            }
            Assert.NotEmpty(built);
            return built.Select(n => new object[] { n });
        }

        /// <summary>
        /// The pipelines a node reaches, named so a failure says which one broke.
        /// </summary>
        private static readonly (string Name, Action<Entity> Run)[] Pipelines =
        {
            ("Stringize",     e => e.Stringize()),
            ("Latexise",      e => e.Latexise()),
            ("ToString",      e => e.ToString()),
            ("InnerSimplified", e => { _ = e.InnerSimplified; }),
            ("Evaled",        e => { _ = e.Evaled; }),
            ("Simplify",      e => e.Simplify()),
            ("Expand",        e => e.Expand()),
            ("Factorize",     e => e.Factorize()),
            ("Differentiate", e => e.Differentiate(X)),
            ("Integrate",     e => e.Integrate(X)),
            ("Substitute",    e => e.Substitute(X, 2)),
            ("Vars",          e => { _ = e.Vars.Count; }),
            ("Nodes",         e => { _ = e.Nodes.Count(); }),
            ("Complexity",    e => { _ = e.Complexity; }),
            ("Solve",         e => e.SolveEquation(X)),
        };

        [Theory]
        [MemberData(nameof(EveryNodeType))]
        public void EveryPipelineReturns(Entity node)
        {
            var broken = new List<string>();
            foreach (var (name, run) in Pipelines)
            {
                // Each in a task with a timeout: a regression here is non-termination, and
                // the suite must fail rather than hang. A stack overflow cannot be caught at
                // all, so the signal for that one is the run aborting.
                string? failure = null;
                var finished = Task.Run(() =>
                {
                    try { run(node); }
                    catch (AngouriMath.Core.Exceptions.AngouriMathBaseException) { /* the library's own, and legitimate */ }
                    catch (NotSupportedException) { /* documented refusal, e.g. uncompilable shapes */ }
                    catch (Exception e) { failure = e.GetType().FullName + ": " + e.Message.Split('\n')[0]; }
                }).Wait(TimeSpan.FromSeconds(30));

                if (!finished)
                    broken.Add($"{name} did not terminate");
                else if (failure is { })
                    broken.Add($"{name} threw {failure}");
            }

            Assert.True(broken.Count is 0,
                $"{node.Stringize()} [{node.GetType().Name}]:\n  " + string.Join("\n  ", broken));
        }
    }
}
