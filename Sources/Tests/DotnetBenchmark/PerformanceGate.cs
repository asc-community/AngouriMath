//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BenchmarkDotNet.Reports;

namespace DotnetBenchmark
{
    /// <summary>
    /// Compares a <see cref="CommonFunctionsInterVersion"/> run against the committed
    /// <c>performance-baseline.json</c> and fails the build when what the library allocates on the
    /// popular use cases has moved -- in either direction.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Allocation is gated and time is not, because only one of the two is a property of the
    /// code.</b> Allocated bytes per operation is what the program asked the allocator for; run the
    /// same code on the same input and it comes back the same number. Wall-clock time on a
    /// GitHub-hosted runner is a property of whoever else is on that host, so a threshold tight
    /// enough to catch a real slowdown would be red for reasons that are not the change, and a gate
    /// people learn to ignore is worse than no gate at all.
    /// </para>
    /// <para>
    /// The thresholds come from measuring it. Three consecutive runs of one unchanged build, all
    /// eighteen benchmarks, on a machine deliberately left loaded (eight cores, load average ~16,
    /// which is closer to a shared runner than an idle desktop is):
    /// </para>
    /// <list type="bullet">
    /// <item><description>
    /// <b>allocation</b> spread at most 0.033% -- six bytes on eighteen kilobytes, for
    /// <c>ParseEasy</c> -- across the sixteen benchmarks that do not compile a delegate, and
    /// identical to the byte on seven of them;
    /// </description></item>
    /// <item><description>
    /// <b>time</b> spread up to 51.8%, and 8-10% on nine of the eighteen.
    /// </description></item>
    /// </list>
    /// <para>
    /// Three orders of magnitude between the two is the whole argument. <see cref="AllocationBand"/>
    /// is 3% -- a hundred times the observed spread -- rather than 0%, because the baseline is also
    /// compared across .NET versions and machines, where a BCL change can move an allocation a
    /// little with nothing in this repository having changed. <see cref="TimeCeilingFactor"/> is 3x
    /// because that is not a performance threshold but a catastrophe one: it catches an accidental
    /// blow-up and deliberately catches nothing smaller.
    /// </para>
    /// <para>
    /// <b>The two <c>Compile</c> benchmarks are the exception, and are excluded by name</b> in the
    /// baseline's <c>ungated</c> map. <c>CompileHard</c> moved 3.6% between those same three runs
    /// and <c>CompileEasy</c> 1.0%, because what they measure includes the runtime's own work
    /// building and JIT-compiling a delegate, which is not reproducible the way the library's
    /// allocation is. They are the reason the exclusion is a recorded decision with a reason rather
    /// than a benchmark quietly missing from the file.
    /// </para>
    /// <para>
    /// <b>It fails on an improvement too</b>, exactly as the corpus gate does, and for the same
    /// reason: a baseline that overstates what the library allocates silently permits giving the
    /// gain back. An improvement is reported as an improvement, with the file to copy.
    /// </para>
    /// </remarks>
    internal static class PerformanceGate
    {
        /// <summary>The argument that asks <c>Program</c> to compare rather than to measure.</summary>
        internal const string Command = "PerformanceGate";

        /// <summary>
        /// The benchmark class the baseline covers: the popular use cases named by
        /// https://github.com/asc-community/AngouriMath/issues/746.
        /// </summary>
        internal const string GatedBenchmark = nameof(CommonFunctionsInterVersion);

        /// <summary>How far allocation may move, in either direction, before the gate fails.</summary>
        private const double AllocationBand = 0.03;

        /// <summary>
        /// A move must also be this many bytes per operation to count, so that a benchmark
        /// allocating almost nothing does not fail on a percentage of almost nothing.
        /// </summary>
        private const long AllocationFloorBytes = 128;

        /// <summary>Time fails only above this factor. See the remarks: a catastrophe gate.</summary>
        private const double TimeCeilingFactor = 3.0;

        /// <summary>Time above this factor is called out for a human to look at, and does not fail.</summary>
        private const double TimeNoticeFactor = 1.25;

        private static readonly JsonSerializerOptions Json = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        /// <summary>One benchmark's two numbers.</summary>
        internal sealed record Measurement(long AllocatedBytes, double MeanNanoseconds);

        /// <summary>
        /// The shape of both <c>performance-baseline.json</c> and the file a run writes, on
        /// purpose: updating the baseline is then copying one file over the other.
        /// </summary>
        internal sealed record Record(
            string Comment,
            string Benchmark,
            string Commit,
            string MeasuredOn,
            string Runtime,
            string Machine,
            Dictionary<string, Measurement> Cases,
            Dictionary<string, string>? Ungated = null);

        private const string RecordComment =
            "Allocated bytes and mean nanoseconds per operation for the popular use cases of "
            + "https://github.com/asc-community/AngouriMath/issues/746. Checked by PerformanceGate.cs; "
            + "see Sources/AngouriMath/Docs/WhatsNew/version_performance_control.md for when updating it is legitimate.";

        /// <summary>
        /// The project directory, found from the binary rather than from the working directory, so
        /// that the gate answers the same whichever directory it is invoked from.
        /// </summary>
        private static string ProjectDirectory { get; } = FindProjectDirectory();

        private static string BaselinePath => Path.Combine(ProjectDirectory, "performance-baseline.json");

        private static string MeasurementPath(string benchmark)
            => Path.Combine(ProjectDirectory, "benchmark_results.csv", $"measured-{benchmark}.json");

        private static string FindProjectDirectory()
        {
            for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
                if (File.Exists(Path.Combine(dir.FullName, "DotnetBenchmark.csproj")))
                    return dir.FullName;
            return Environment.CurrentDirectory;
        }

        /// <summary>
        /// Writes what a run measured, in the baseline's own format. Called for every benchmark
        /// class, not only the gated one: the file is what a contributor copies over the baseline,
        /// and what the CI job uploads so that the baseline can be refreshed from a run on the
        /// hardware the gate will actually be checked on.
        /// </summary>
        internal static void WriteMeasurements(Summary summary)
        {
            var name = summary.BenchmarksCases.FirstOrDefault()?.Descriptor.Type.Name;
            if (name is null) return;

            var cases = new Dictionary<string, Measurement>(StringComparer.Ordinal);
            foreach (var report in summary.Reports)
            {
                var method = report.BenchmarkCase.Descriptor.WorkloadMethod.Name;
                var mean = report.ResultStatistics?.Mean;
                if (mean is null) continue;
                cases[method] = new Measurement(
                    report.GcStats.BytesAllocatedPerOperation,
                    mean.Value);
            }
            if (cases.Count == 0) return;

            var record = new Record(
                RecordComment,
                name,
                DescribeCommit(),
                DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                RuntimeInformation.FrameworkDescription,
                $"{RuntimeInformation.OSDescription.Split('\n')[0].Trim()}, "
                    + $"{RuntimeInformation.ProcessArchitecture}, {Environment.ProcessorCount} logical cores",
                cases.OrderBy(pair => pair.Key, StringComparer.Ordinal)
                     .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal),
                // Carried over from the baseline rather than recomputed, so that copying this file
                // over the baseline keeps the exclusions and the reasons for them.
                CarryOverUngated(name));

            var path = MeasurementPath(name);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(record, Json) + Environment.NewLine);
            Console.WriteLine($"Measurements written to {path}");
        }

        /// <summary>
        /// The exclusions the baseline already records, if any. A benchmark is left out of the gate
        /// deliberately and with a reason written down, never by being absent.
        /// </summary>
        private static Dictionary<string, string>? CarryOverUngated(string benchmark)
        {
            if (benchmark != GatedBenchmark || !File.Exists(BaselinePath)) return null;
            try
            {
                return Read(BaselinePath).Ungated;
            }
            catch (Exception e) when (e is JsonException or IOException)
            {
                return null;
            }
        }

        /// <summary>The commit the run was taken at, for the record. Never fails the run.</summary>
        private static string DescribeCommit()
        {
            if (Environment.GetEnvironmentVariable("GITHUB_SHA") is { Length: > 0 } sha) return sha;
            try
            {
                using var git = Process.Start(new ProcessStartInfo("git", "rev-parse HEAD")
                {
                    WorkingDirectory = ProjectDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                });
                if (git is null) return "unknown";
                var output = git.StandardOutput.ReadToEnd().Trim();
                git.WaitForExit(5000);
                return git.ExitCode == 0 && output.Length > 0 ? output : "unknown";
            }
            catch (Exception e) when (e is System.ComponentModel.Win32Exception or InvalidOperationException)
            {
                return "unknown";
            }
        }

        private enum Verdict { Ok, Slow, Ungated, Regressed, Improved, Slower, Missing, New }

        private sealed record Row(
            string Method, Verdict Verdict,
            Measurement? Baseline, Measurement? Measured,
            double AllocationDelta, double TimeFactor);

        /// <summary>
        /// Reads the baseline and the last run of <see cref="GatedBenchmark"/> and reports on the
        /// difference. Returns a process exit code: 0 when nothing moved, 1 otherwise.
        /// </summary>
        internal static int Compare(TextWriter to)
        {
            if (!File.Exists(BaselinePath))
                return Fail(to, $"No baseline at {BaselinePath}.");

            var measurementPath = MeasurementPath(GatedBenchmark);
            if (!File.Exists(measurementPath))
                return Fail(to,
                    $"No measurements at {measurementPath}.\n"
                    + $"The gate compares a run against the baseline; it does not run the benchmark itself.\n"
                    + $"Run    dotnet run -c Release {GatedBenchmark}    first, from Sources/Tests/DotnetBenchmark.");

            Record baseline, measured;
            try
            {
                baseline = Read(BaselinePath);
                measured = Read(measurementPath);
            }
            catch (JsonException e)
            {
                return Fail(to, $"Could not read the baseline or the measurements: {e.Message}");
            }

            var rows = Rows(baseline, measured).ToList();
            to.WriteLine(Report(baseline, measured, rows));

            var moved = rows.Where(r => r.Verdict is not (Verdict.Ok or Verdict.Slow or Verdict.Ungated)).ToList();
            if (moved.Count == 0)
            {
                var gated = rows.Count(r => r.Verdict is not Verdict.Ungated);
                to.WriteLine($"PASSED. Allocation is what the baseline says it is on all {gated} gated "
                    + $"benchmarks; {rows.Count - gated} are not gated, for the reasons the baseline gives.");
                var slow = rows.Where(r => r.Verdict is Verdict.Slow).ToList();
                if (slow.Count > 0)
                    to.WriteLine($"        {slow.Count} benchmark(s) are more than "
                        + $"{TimeNoticeFactor:0.00}x the baseline time. That is reported, not gated -- "
                        + "see the remarks in PerformanceGate.cs -- but it is worth a look if the "
                        + "same rows are slow on a second run.");
                return 0;
            }

            to.WriteLine(Explain(baseline, moved));
            to.WriteLine();
            to.WriteLine("To accept these numbers as the new baseline, copy the file this run wrote:");
            to.WriteLine();
            to.WriteLine($"    cp {Relative(measurementPath)} \\");
            to.WriteLine($"       {Relative(BaselinePath)}");
            to.WriteLine();
            to.WriteLine("On a CI run it is in the benchmark-results artifact. Its content is:");
            to.WriteLine();
            to.WriteLine(File.ReadAllText(measurementPath).TrimEnd());
            to.WriteLine();
            return 1;
        }

        private static Record Read(string path)
            => JsonSerializer.Deserialize<Record>(File.ReadAllText(path), Json)
               ?? throw new JsonException($"{path} is empty");

        private static int Fail(TextWriter to, string message)
        {
            to.WriteLine();
            to.WriteLine("FAILED: " + message);
            to.WriteLine();
            return 1;
        }

        private static IEnumerable<Row> Rows(Record baseline, Record measured)
        {
            var ungated = baseline.Ungated ?? new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var method in baseline.Cases.Keys.Concat(measured.Cases.Keys)
                                                      .Distinct(StringComparer.Ordinal)
                                                      .OrderBy(m => m, StringComparer.Ordinal))
            {
                var was = baseline.Cases.TryGetValue(method, out var b) ? b : null;
                var now = measured.Cases.TryGetValue(method, out var m) ? m : null;
                if (ungated.ContainsKey(method))
                {
                    var delta = was is null || now is null || was.AllocatedBytes == 0
                        ? 0
                        : (now.AllocatedBytes - (double)was.AllocatedBytes) / was.AllocatedBytes;
                    var ratio = was is null || now is null || was.MeanNanoseconds <= 0
                        ? 1
                        : now.MeanNanoseconds / was.MeanNanoseconds;
                    yield return new Row(method, Verdict.Ungated, was, now, delta, ratio);
                    continue;
                }
                if (was is null) { yield return new Row(method, Verdict.New, null, now, 0, 0); continue; }
                if (now is null) { yield return new Row(method, Verdict.Missing, was, null, 0, 0); continue; }

                var allocationDelta = was.AllocatedBytes == 0
                    ? (now.AllocatedBytes == 0 ? 0 : double.PositiveInfinity)
                    : (now.AllocatedBytes - (double)was.AllocatedBytes) / was.AllocatedBytes;
                var timeFactor = was.MeanNanoseconds > 0 ? now.MeanNanoseconds / was.MeanNanoseconds : 1;
                var byBytes = Math.Abs(now.AllocatedBytes - was.AllocatedBytes) >= AllocationFloorBytes;

                var verdict =
                    byBytes && allocationDelta > AllocationBand ? Verdict.Regressed
                    : byBytes && allocationDelta < -AllocationBand ? Verdict.Improved
                    : timeFactor > TimeCeilingFactor ? Verdict.Slower
                    : timeFactor > TimeNoticeFactor ? Verdict.Slow
                    : Verdict.Ok;
                yield return new Row(method, verdict, was, now, allocationDelta, timeFactor);
            }
        }

        private static string Report(Record baseline, Record measured, IReadOnlyList<Row> rows)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine(new string('=', 100));
            sb.AppendLine($" Kernel performance gate -- {baseline.Benchmark}");
            sb.AppendLine(new string('=', 100));
            sb.AppendLine($" baseline  {Short(baseline.Commit)}, measured {baseline.MeasuredOn} on {baseline.Runtime}");
            sb.AppendLine($"           {baseline.Machine}");
            sb.AppendLine($" this run  {Short(measured.Commit)}, measured {measured.MeasuredOn} on {measured.Runtime}");
            sb.AppendLine($"           {measured.Machine}");
            sb.AppendLine();
            sb.AppendLine($" Allocation must stay within {AllocationBand * 100:0}% of the baseline in either direction;"
                + $" a move of under {AllocationFloorBytes} B/op");
            sb.AppendLine($" does not count. Time is reported and fails only above {TimeCeilingFactor:0.00}x, because a"
                + " shared runner's wall clock");
            sb.AppendLine(" is not a property of the code. Both thresholds are argued for in PerformanceGate.cs.");
            sb.AppendLine();
            sb.AppendLine("                        allocated B/op                        mean ns/op");
            sb.AppendLine(" method             baseline      measured  delta      baseline      measured  factor  verdict");
            sb.AppendLine(" " + new string('-', 98));
            foreach (var r in rows)
                sb.AppendLine(
                    $" {r.Method,-18}{Bytes(r.Baseline),13}{Bytes(r.Measured),14}{Delta(r),7}"
                    + $"{Nanoseconds(r.Baseline),14}{Nanoseconds(r.Measured),14}{Factor(r),8}  {Word(r.Verdict)}");
            sb.AppendLine(" " + new string('-', 98));
            if (baseline.Ungated is { Count: > 0 } ungated)
            {
                sb.AppendLine();
                sb.AppendLine(" Not gated, and why:");
                foreach (var (method, reason) in ungated.OrderBy(pair => pair.Key, StringComparer.Ordinal))
                    sb.AppendLine($"   {method,-18}{reason}");
            }
            return sb.ToString();
        }

        private static string Explain(Record baseline, IReadOnlyList<Row> moved)
        {
            var sb = new StringBuilder();
            var counts = moved.GroupBy(r => r.Verdict)
                              .OrderBy(g => g.Key)
                              .Select(g => $"{g.Count()} {Word(g.Key).ToLowerInvariant()}");
            sb.AppendLine($"FAILED: {string.Join(", ", counts)}.");
            sb.AppendLine();
            foreach (var r in moved)
            {
                switch (r.Verdict)
                {
                    case Verdict.Regressed:
                        sb.AppendLine($"  REGRESSED  {r.Method} allocates {r.Measured!.AllocatedBytes:N0} B/op, "
                            + $"{r.AllocationDelta * 100:0.0}% above the {r.Baseline!.AllocatedBytes:N0} B/op recorded at "
                            + $"{Short(baseline.Commit)}.");
                        sb.AppendLine("             Find what the change allocates that it did not before. If the "
                            + "extra allocation buys");
                        sb.AppendLine("             something the library now does and is worth it, say so in the "
                            + "pull request and update");
                        sb.AppendLine("             the baseline in the same change.");
                        break;
                    case Verdict.Improved:
                        sb.AppendLine($"  IMPROVED   {r.Method} allocates {r.Measured!.AllocatedBytes:N0} B/op, "
                            + $"{-r.AllocationDelta * 100:0.0}% BELOW the {r.Baseline!.AllocatedBytes:N0} B/op recorded at "
                            + $"{Short(baseline.Commit)}.");
                        sb.AppendLine("             Nothing is wrong. The gate fails so the gain gets recorded "
                            + "rather than absorbed: a");
                        sb.AppendLine("             baseline that overstates what the library allocates would let a "
                            + "later change give it");
                        sb.AppendLine("             back unnoticed. Update the baseline in this change, and say so "
                            + "in the pull request.");
                        break;
                    case Verdict.Slower:
                        sb.AppendLine($"  SLOWER     {r.Method} takes {r.TimeFactor:0.00}x the baseline time "
                            + $"({r.Measured!.MeanNanoseconds:N0} ns/op against {r.Baseline!.MeanNanoseconds:N0}).");
                        sb.AppendLine($"             The time gate is deliberately loose at {TimeCeilingFactor:0.00}x "
                            + "and a shared runner does not");
                        sb.AppendLine("             explain a factor this size, so treat it as real and re-run to "
                            + "confirm before");
                        sb.AppendLine("             updating anything.");
                        break;
                    case Verdict.Missing:
                        sb.AppendLine($"  MISSING    {r.Method} is in the baseline and was not measured.");
                        sb.AppendLine("             Either the benchmark was removed -- then remove it from the "
                            + "baseline in the same");
                        sb.AppendLine("             change -- or the run did not finish, in which case the gate has "
                            + "nothing to say and");
                        sb.AppendLine("             the benchmark step is what to look at.");
                        break;
                    case Verdict.New:
                        sb.AppendLine($"  NEW        {r.Method} was measured and is not in the baseline "
                            + $"({r.Measured!.AllocatedBytes:N0} B/op, {r.Measured.MeanNanoseconds:N0} ns/op).");
                        sb.AppendLine("             A benchmark nobody has recorded a number for is not gated. "
                            + "Add it to the baseline.");
                        break;
                }
                sb.AppendLine();
            }
            return sb.ToString().TrimEnd();
        }

        private static string Word(Verdict verdict) => verdict switch
        {
            Verdict.Ok => "ok",
            Verdict.Slow => "ok (slow)",
            Verdict.Ungated => "not gated",
            Verdict.Regressed => "REGRESSED",
            Verdict.Improved => "IMPROVED",
            Verdict.Slower => "SLOWER",
            Verdict.Missing => "MISSING",
            _ => "NEW",
        };

        private static string Short(string commit)
            => commit.Length >= 8 ? "commit " + commit[..8] : "commit " + commit;

        private static string Relative(string path)
        {
            var index = path.Replace('\\', '/').IndexOf("Sources/", StringComparison.Ordinal);
            return index < 0 ? path : path.Replace('\\', '/')[index..];
        }

        private static string Bytes(Measurement? m) => m is null ? "-" : m.AllocatedBytes.ToString("N0");

        private static string Nanoseconds(Measurement? m) => m is null
            ? "-"
            : m.MeanNanoseconds >= 1e7 ? m.MeanNanoseconds.ToString("0.00e+0")
            : m.MeanNanoseconds < 10 ? m.MeanNanoseconds.ToString("N2") : m.MeanNanoseconds.ToString("N0");

        private static string Delta(Row r) => r.Baseline is null || r.Measured is null
            ? "-"
            : double.IsInfinity(r.AllocationDelta) ? "+inf" : $"{r.AllocationDelta:+0.0%;-0.0%;0.0%}";

        private static string Factor(Row r) => r.Baseline is null || r.Measured is null
            ? "-"
            : $"{r.TimeFactor:0.00}x";
    }
}
