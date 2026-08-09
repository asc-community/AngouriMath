//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AngouriMath;
using Xunit;

namespace AngouriMath.Tests.Common
{
    /// <summary>
    /// The public surface, written down, so that changing it is a deliberate act.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Removing or renaming a public member breaks every consumer at compile time, and adding
    /// one is a promise that has to be kept for the rest of the major version. Neither shows
    /// up in a test run: the suite calls the API it knows about, so a member that vanishes
    /// takes its own tests with it and the summary line stays green.
    /// </para>
    /// <para>
    /// This is what the RS0016/RS0017 analyzer used to do before its settings were retired in
    /// <a href="https://github.com/asc-community/AngouriMath/pull/835">#835</a> — the settings
    /// were dead, but the guarantee was worth keeping, and this restores it without the
    /// analyzer package. The measured surface is compared against
    /// <c>PublicApi.txt</c> beside this file; a difference fails with both lists spelled out.
    /// </para>
    /// <para>
    /// To accept an intended change, run the suite once with <c>AM_UPDATE_PUBLIC_API=1</c> and
    /// commit the result. The diff is then part of review, which is the whole point — the file
    /// is not a chore to keep in sync, it is the record of what was promised and when.
    /// </para>
    /// <para>
    /// One assembly, one target framework. The surface genuinely differs per framework — the
    /// generic-math members exist on <c>net7.0</c> and later only — so this pins the framework
    /// the tests run on and says nothing about <c>netstandard2.0</c>.
    /// </para>
    /// </remarks>
    [Trait("Area", "Common")]
    public sealed class PublicApiSurfaceTest
    {
        // The build preserves the folder the Content item sits in, so it lands under Common/.
        private static readonly string BaselinePath =
            Path.Combine(AppContext.BaseDirectory, "Common", "PublicApi.txt");

        [Fact]
        public void ThePublicSurfaceIsTheOneOnRecord()
        {
            var measured = Surface();

            if (Environment.GetEnvironmentVariable("AM_UPDATE_PUBLIC_API") is "1")
            {
                // Written next to the sources rather than to the output directory, which is
                // where the copy the test reads lives and would be overwritten by the build.
                var source = Path.Combine(SourceDirectory(), "PublicApi.txt");
                File.WriteAllLines(source, measured);
                return;
            }

            Assert.True(File.Exists(BaselinePath),
                $"No public API baseline at {BaselinePath}. "
                + "Run the suite with AM_UPDATE_PUBLIC_API=1 to write one, then commit it.");

            var recorded = new SortedSet<string>(File.ReadAllLines(BaselinePath)
                                                     .Where(l => l.Length > 0), StringComparer.Ordinal);

            var removed = recorded.Except(measured, StringComparer.Ordinal).ToList();
            var added = measured.Except(recorded, StringComparer.Ordinal).ToList();
            if (removed.Count is 0 && added.Count is 0)
                return;

            var message = new StringBuilder();
            message.AppendLine("The public surface no longer matches PublicApi.txt.");
            message.AppendLine("If the change is intended, re-run with AM_UPDATE_PUBLIC_API=1 and commit the file.");
            Describe(message, "REMOVED — breaks every consumer at compile time", removed);
            Describe(message, "ADDED — a promise for the rest of the major version", added);
            Assert.True(false, message.ToString());
        }

        private static void Describe(StringBuilder into, string heading, List<string> members)
        {
            if (members.Count is 0) return;
            into.AppendLine().AppendLine($"{heading} ({members.Count}):");
            foreach (var member in members.Take(40))
                into.AppendLine("  " + member);
            if (members.Count > 40)
                into.AppendLine($"  ... and {members.Count - 40} more");
        }

        /// <summary>Walks up from the test binary to this file's directory.</summary>
        private static string SourceDirectory()
        {
            var path = AppContext.BaseDirectory;
            while (path is not null && Path.GetFileName(path) is not "UnitTests")
                path = Path.GetDirectoryName(path);
            if (path is null)
                throw new InvalidOperationException("Could not find the UnitTests directory from "
                                                    + AppContext.BaseDirectory);
            return Path.Combine(path, "Common");
        }

        /// <summary>
        /// Every publicly reachable member, one per line. Protected members count: a consumer
        /// can derive, so they are as much a promise as a public one.
        /// </summary>
        private static SortedSet<string> Surface()
        {
            var lines = new SortedSet<string>(StringComparer.Ordinal);
            foreach (var type in typeof(Entity).Assembly.GetTypes()
                         .Where(Visible)
                         .OrderBy(t => t.FullName, StringComparer.Ordinal))
            {
                var kind = type.IsEnum ? "enum" : type.IsInterface ? "interface"
                         : type.IsValueType ? "struct" : "class";
                lines.Add($"{kind} {Nice(type)}");

                const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic
                                         | BindingFlags.Instance | BindingFlags.Static
                                         | BindingFlags.DeclaredOnly;
                foreach (var member in type.GetMembers(Flags))
                    if (Signature(type, member) is { } signature)
                        lines.Add(signature);
            }
            return lines;
        }

        private static string? Signature(Type type, MemberInfo member)
        {
            switch (member)
            {
                case MethodInfo m when Reachable(m):
                    // Accessors are reported through the property or event that owns them.
                    if (m.IsSpecialName && (m.Name.StartsWith("get_", StringComparison.Ordinal)
                                         || m.Name.StartsWith("set_", StringComparison.Ordinal)
                                         || m.Name.StartsWith("add_", StringComparison.Ordinal)
                                         || m.Name.StartsWith("remove_", StringComparison.Ordinal)))
                        return null;
                    return $"{Nice(type)}.{m.Name}({Parameters(m.GetParameters())}) : {Nice(m.ReturnType)}";
                case ConstructorInfo c when Reachable(c):
                    return $"{Nice(type)}.ctor({Parameters(c.GetParameters())})";
                case PropertyInfo p when (p.GetMethod ?? p.SetMethod) is { } a && Reachable(a):
                    return $"{Nice(type)}.{p.Name} {{ }} : {Nice(p.PropertyType)}";
                case FieldInfo f when f.IsPublic || f.IsFamily || f.IsFamilyOrAssembly:
                    return $"{Nice(type)}.{f.Name} : {Nice(f.FieldType)}";
                case EventInfo e:
                    return $"{Nice(type)}.{e.Name} (event)";
                default:
                    return null;
            }
        }

        private static bool Reachable(MethodBase m) => m.IsPublic || m.IsFamily || m.IsFamilyOrAssembly;

        private static string Parameters(ParameterInfo[] parameters)
            => string.Join(", ", parameters.Select(p => Nice(p.ParameterType)));

        private static bool Visible(Type type)
        {
            while (type.IsNested)
            {
                if (!type.IsNestedPublic && !type.IsNestedFamily && !type.IsNestedFamORAssem)
                    return false;
                type = type.DeclaringType!;
            }
            return type.IsPublic;
        }

        /// <summary>
        /// A stable name. By-ref, array and pointer types carry an assembly-qualified
        /// <see cref="Type.FullName"/> that embeds the assembly version, so comparing those raw
        /// would report every such signature as changed on each version bump.
        /// </summary>
        private static string Nice(Type type)
        {
            if (type.IsGenericParameter) return type.Name;
            if (type.IsByRef) return Nice(type.GetElementType()!) + "&";
            if (type.IsPointer) return Nice(type.GetElementType()!) + "*";
            if (type.IsArray)
                return Nice(type.GetElementType()!) + "[" + new string(',', type.GetArrayRank() - 1) + "]";
            if (type.IsGenericType)
                return (type.FullName ?? type.Name).Split('`')[0]
                     + "<" + string.Join(",", type.GetGenericArguments().Select(Nice)) + ">";
            return type.FullName ?? type.Name;
        }
    }
}
