//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

// Shared by every harness that writes a report. Linked into each project with
// <Compile Include="../shared/MeasuredCommit.cs" />, so the four that had no commit line
// and the ones that did cannot drift apart.
//
// Why a report has to name its build: a harness report is committed to this workspace and
// read later as though it described the library. It describes a *build*. Without the commit,
// "byte-identical to the last run" and "the harness never wrote and you are reading a stale
// file" are the same diff, and both have happened here.

using System;
using System.IO;
using System.Linq;

namespace Harness
{
    static class Measured
    {
        /// <summary>
        /// The commit of the library this was measured against.
        /// </summary>
        /// <remarks>
        /// Read from the <c>ProjectReference</c> in this harness's own project file, not from the
        /// loaded assembly: the build system copies AngouriMath.dll into the harness's output
        /// directory, which is not in a repository, so the assembly's location says nothing about
        /// which checkout it came from. A report generated from a branch and read later as the
        /// library's behaviour has cost half a day here before, so this is derived rather than
        /// written down.
        /// </remarks>
        internal static string Commit()
        {
            try
            {
                // The harness's own project directory, resolved from the assembly rather than
                // from the working directory. Run from the workspace root -- which is how these
                // are invoked -- CurrentDirectory has no .csproj in it, so this silently reported
                // "unknown build": a report that cannot name what it measured, which is the exact
                // failure this method exists to prevent.
                var projectDirectory = Directory
                    .GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;
                var project = Directory.GetFiles(projectDirectory, "*.csproj").FirstOrDefault();
                if (project is null)
                    return "unknown build";
                var reference = File.ReadAllLines(project)
                    .Select(line => line.Trim())
                    .FirstOrDefault(line => line.StartsWith("<ProjectReference", StringComparison.Ordinal));
                if (reference is null)
                    return "unknown build";
                var quoted = reference.Split('"');
                if (quoted.Length < 2)
                    return "unknown build";
                // A ProjectReference is relative to the project file that declares it, never to
                // the working directory.
                return At(Path.GetFullPath(Path.Combine(
                    projectDirectory, quoted[1].Replace('\\', Path.DirectorySeparatorChar))));
            }
            catch { return "unknown build"; }
        }

        /// <summary>
        /// The commit of a library checkout named directly, for a harness with no
        /// <c>ProjectReference</c> of its own to read.
        /// </summary>
        /// <remarks>
        /// <c>docsamples</c> compiles the wiki's samples against a project it *generates*, so the
        /// reference it measures lives in that generated file rather than in its own. It knows the
        /// path already; without this overload its report is the only one that cannot say which
        /// build it compiled against.
        /// </remarks>
        internal static string Commit(string projectOrDirectory)
        {
            try { return At(projectOrDirectory); }
            catch { return "unknown build"; }
        }

        /// <summary>
        /// The commit of the repository that contains <paramref name="path"/>.
        /// </summary>
        /// <remarks>
        /// A worktree's <c>.git</c> is a *file* pointing at the real one, and every build measured
        /// here is in a worktree -- so the walk upwards has to accept either. The decoration names
        /// HEAD, remote-tracking branches and tags only: the worktrees share their local branches,
        /// and another session's scratch branch at the same commit named itself in a report.
        /// </remarks>
        private static string At(string path)
        {
            var at = Directory.Exists(path) ? path : Path.GetDirectoryName(Path.GetFullPath(path));
            while (at is not null && !Directory.Exists(Path.Combine(at, ".git"))
                                  && !File.Exists(Path.Combine(at, ".git")))
                at = Path.GetDirectoryName(at);
            if (at is null)
                return "unknown build";
            var git = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(
                "git", $"-C \"{at}\" log -1 --format=%h%d --decorate-refs=refs/remotes --decorate-refs=refs/tags --decorate-refs=HEAD") { RedirectStandardOutput = true });
            var line = git.StandardOutput.ReadToEnd().Trim();
            git.WaitForExit();
            return line.Length == 0 ? "unknown build" : line;
        }
    }
}
