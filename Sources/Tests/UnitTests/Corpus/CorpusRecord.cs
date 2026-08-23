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
using System.Text;

namespace AngouriMath.Tests.Corpus
{
    /// <summary>
    /// How the corpus result leaves the process: a <b>baseline</b> that is committed and carries
    /// the history, and a <b>report</b> that is written on every run and uploaded as a CI
    /// artefact.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Item 36 of <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a> asks
    /// for the corpus result "as a CI artefact with per-commit history". Those are two different
    /// files because they answer two different questions, and one file cannot be both.
    /// </para>
    /// <para>
    /// The <b>report</b> answers <i>what happened in this run</i>. It carries the measured
    /// verdict, the note explaining it and the answer the library gave, plus the machine it ran
    /// on and the commit. It is written to the test output directory — never into the source
    /// tree, which would make every local run dirty the working copy — and uploaded with
    /// <c>if: always()</c>, because a run that fell over part way still measured the cases that
    /// ran and those are the interesting ones when something has just broken.
    /// </para>
    /// <para>
    /// The <b>baseline</b> answers <i>what did the corpus look like at commit X</i>, for every X,
    /// forever. That is what "per-commit history" means, and an accumulated pile of artefacts
    /// cannot supply it: artefacts expire (90 days at most on this plan), they cannot be diffed,
    /// and they are not attributable to the commit that moved a number. A file in the repository
    /// is, through <c>git log -p</c> and <c>git blame</c>, and it does not expire. The two
    /// destination repositories <a href="https://github.com/asc-community/AngouriMath/issues/500">#500</a>
    /// names for the equivalent job on the benchmark side — <c>AngouriMathLab/performance-reports</c>
    /// and <c>-tools</c> — were checked and still do not exist, so pushing the history elsewhere
    /// was not an option to weigh.
    /// </para>
    /// <para>
    /// The baseline is <b>generated from <see cref="Corpus.All"/></b>, not from the run. That is
    /// deliberate: the per-problem expectation already lives in the corpus, as
    /// <see cref="Problem.Expect"/>, next to the problem it is about, and a second hand-maintained
    /// copy of it would be a second thing to edit and a second thing to get wrong. This file is a
    /// projection of the corpus, so keeping it current is mechanical rather than a judgement, and
    /// it states the two things <see cref="Problem.Expect"/> does not state anywhere: the totals,
    /// and the membership as a flat list, so that a corpus which quietly shrinks shows up in the
    /// diff. The gate checks the projection is current; a separate gate checks the corpus itself
    /// still measures what it claims. Neither can pass while the other is stale.
    /// </para>
    /// <para>
    /// The shape — a committed record, an <c>AM_UPDATE_*</c> environment variable to regenerate
    /// it, and a failure that spells out both directions — is the one
    /// <c>Common/PublicApiSurfaceTest</c> already uses for the public API surface. It is followed
    /// here rather than a second convention invented alongside it.
    /// </para>
    /// </remarks>
    public static class CorpusRecord
    {
        /// <summary>The environment variable that rewrites <see cref="BaselineSourcePath"/>.</summary>
        public const string UpdateVariable = "AM_UPDATE_CORPUS_BASELINE";

        /// <summary>The name of the committed record, in the source tree and in the output.</summary>
        public const string BaselineFileName = "corpus-baseline.tsv";

        /// <summary>The name of the per-run report, in the output directory only.</summary>
        public const string ReportFileName = "corpus-report.tsv";

        /// <summary>
        /// The copy the test reads. The build preserves the folder the <c>Content</c> item sits
        /// in, so it lands under <c>Corpus/</c>.
        /// </summary>
        public static string BaselinePath { get; } =
            Path.Combine(AppContext.BaseDirectory, "Corpus", BaselineFileName);

        /// <summary>The committed original, which is what an update has to rewrite.</summary>
        public static string BaselineSourcePath => Path.Combine(SourceDirectory(), BaselineFileName);

        /// <summary>
        /// Written next to the test binary, which is what the workflow uploads. Not written into
        /// the source tree: a test must not dirty the working copy it is run from.
        /// </summary>
        public static string ReportPath { get; } =
            Path.Combine(AppContext.BaseDirectory, ReportFileName);

        /// <summary>
        /// How bad a verdict is, worst last. Used only to say whether a change is an improvement
        /// or a regression, and it deliberately leaves a gap.
        /// </summary>
        /// <remarks>
        /// <see cref="Verdict.Solved"/> beats everything and <see cref="Verdict.Wrong"/> loses to
        /// everything — AGENTS.md is explicit that an answer which is not right is worse than no
        /// answer. <see cref="Verdict.Unsolved"/> beats <see cref="Verdict.Error"/> and
        /// <see cref="Verdict.Timeout"/> because declining, in bounded time, is a legitimate
        /// answer and the other two are defects. Between <i>threw</i> and <i>hung</i> there is no
        /// defensible ordering, so they are given the <b>same</b> rank and a move between them is
        /// reported as a change rather than as progress in either direction.
        /// </remarks>
        public static int Rank(Verdict verdict) => verdict switch
        {
            Verdict.Solved => 0,
            Verdict.Unsolved => 1,
            Verdict.Timeout => 2,
            Verdict.Error => 2,
            Verdict.Wrong => 3,
            _ => 3,
        };

        /// <summary>The committed record, rendered from the corpus.</summary>
        public static string RenderBaseline(IReadOnlyList<Problem> problems)
        {
            var sb = new StringBuilder();
            sb.Append("# AngouriMath corpus baseline. One line per problem, tab separated.\n");
            sb.Append("# Generated from Corpus.All -- not hand-edited. To update it, change Expect\n");
            sb.Append($"# in Corpus.cs, re-run the suite with {UpdateVariable}=1, and commit both.\n");
            sb.Append("# Its git history is the per-commit history of the corpus -- item 36 of\n");
            sb.Append("# https://github.com/asc-community/AngouriMath/issues/746.\n");
            sb.Append(Totals(problems.Select(p => p.Expect), problems.Count));
            sb.Append("name\top\tverdict\n");
            // Ordered by name rather than by declaration, so that inserting a problem moves one
            // line rather than every line after it.
            foreach (var problem in problems.OrderBy(p => p.Name, StringComparer.Ordinal))
                sb.Append($"{problem.Name}\t{problem.Op}\t{problem.Expect}\n");
            return sb.ToString();
        }

        /// <summary>One <c># totals</c> line, in the order the verdicts are declared in.</summary>
        private static string Totals(IEnumerable<Verdict> verdicts, int count)
        {
            var list = verdicts.ToList();
            var sb = new StringBuilder("# totals");
            foreach (var verdict in Enum.GetValues(typeof(Verdict)).Cast<Verdict>())
                sb.Append($"\t{verdict}={list.Count(v => v == verdict)}");
            return sb.Append($"\tTotal={count}\n").ToString();
        }

        /// <summary>
        /// The per-run report: everything the baseline holds, plus what was actually measured,
        /// why, and where.
        /// </summary>
        public static string RenderReport(IReadOnlyList<ReportRow> rows, long elapsedMilliseconds)
        {
            var sb = new StringBuilder();
            sb.Append("# AngouriMath corpus report. One line per problem, tab separated.\n");
            sb.Append("# Written by CorpusGateTest on every run; uploaded as a CI artefact.\n");
            sb.Append($"# generated\t{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}\n");
            sb.Append($"# commit\t{Environment.GetEnvironmentVariable("GITHUB_SHA") ?? "-"}\n");
            sb.Append($"# ref\t{Environment.GetEnvironmentVariable("GITHUB_REF") ?? "-"}\n");
            sb.Append($"# os\t{Sanitize(System.Runtime.InteropServices.RuntimeInformation.OSDescription)}\n");
            sb.Append($"# runtime\t{Sanitize(System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription)}\n");
            sb.Append($"# elapsed_ms\t{elapsedMilliseconds}\n");
            sb.Append(Totals(rows.Select(r => r.Verdict), rows.Count));
            sb.Append("name\top\texpect\tverdict\tstatus\tnote\tanswer\n");
            foreach (var row in rows.OrderBy(r => r.Name, StringComparer.Ordinal))
                sb.Append($"{row.Name}\t{row.Op}\t{row.Expect}\t{row.Verdict}\t{row.Status}"
                          + $"\t{Sanitize(row.Note)}\t{Sanitize(row.Answer)}\n");
            return sb.ToString();
        }

        /// <summary>One row of the report.</summary>
        public sealed record ReportRow(
            string Name, Op Op, Verdict Expect, Verdict Verdict, string Status, string Note, string Answer);

        /// <summary>
        /// A tab or a newline inside a field would make the row unreadable as TSV, and an answer
        /// is the library's own printed output, so neither can be ruled out.
        /// </summary>
        private static string Sanitize(string field)
        {
            if (string.IsNullOrEmpty(field)) return "-";
            var text = field.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ').Trim();
            return text.Length == 0 ? "-" : text.Length > 160 ? text[..157] + "..." : text;
        }

        /// <summary>
        /// Two texts compared as lines, so that the difference can be shown rather than asserted.
        /// Line endings are normalised: the file is committed once and read on three operating
        /// systems, and git may hand any of them a different ending for the same content.
        /// </summary>
        public static IReadOnlyList<string> Lines(string text) =>
            text.Replace("\r\n", "\n").Split('\n').Where(l => l.Length > 0).ToList();

        /// <summary>Walks up from the test binary to this file's directory in the source tree.</summary>
        private static string SourceDirectory()
        {
            var path = AppContext.BaseDirectory;
            while (path is not null && Path.GetFileName(path) is not "UnitTests")
                path = Path.GetDirectoryName(path);
            if (path is null)
                throw new InvalidOperationException("Could not find the UnitTests directory from "
                                                    + AppContext.BaseDirectory);
            return Path.Combine(path, "Corpus");
        }
    }
}
