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
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Utils
{
    /// <summary>
    /// How long CI takes, and whether that has changed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Run with <c>dotnet run CiTiming</c> from <c>Sources/Utils/Utils</c>. Requires the
    /// GitHub CLI (<c>gh</c>) to be installed and authenticated; it is the only thing here
    /// that knows how to talk to the API, and using it avoids a token in this repository.
    /// </para>
    /// <para>
    /// Two durations are reported per job and they answer different questions. <em>Exec</em>
    /// is from the moment a runner picks the job up to the moment it finishes, which is what
    /// changes when a test suite grows. <em>Queue</em> is from the run being created to a
    /// runner picking it up, which changes when several pull requests are in flight and says
    /// nothing about the code. Reporting only the sum of the two — which is what the web UI
    /// shows — makes a busy afternoon look like a performance regression.
    /// </para>
    /// <para>
    /// The baseline is committed rather than derived, because GitHub keeps run history for a
    /// bounded window and logs for ninety days. Without a file in the repository there is
    /// nothing left to compare against once that window passes, which is exactly the
    /// situation this was written in.
    /// </para>
    /// </remarks>
    public static class CiTiming
    {
        private const string Repo = "asc-community/AngouriMath";

        /// <summary>How many runs to ask for. The API caps a page at a thousand.</summary>
        private const int RunsToFetch = 400;

        /// <summary>Runs per workflow to open up for per-job timings. Each costs a request.</summary>
        private const int JobSample = 10;

        /// <summary>Reported as a regression above this. Runner-to-runner noise is a few percent.</summary>
        private const double RegressionThreshold = 0.15;

        private static void Log(string msg) => Console.WriteLine("CiTiming: " + msg);

        public static void Do()
        {
            var baselinePath = Path.Join(Program.GetPathIntoSources(), "Utils", "ci-baseline.json");
            var recording = Environment.GetEnvironmentVariable("CI_TIMING_RECORD") == "1";

            Log($"asking {Repo} for the last {RunsToFetch} runs");
            var runs = FetchRuns();
            if (runs.Count is 0)
            {
                Log("no successful runs came back — is gh installed and authenticated?");
                return;
            }
            Log($"{runs.Count} successful runs, "
                + $"{runs.Min(r => r.Created):yyyy-MM-dd} to {runs.Max(r => r.Created):yyyy-MM-dd}");

            var measured = Measure(runs);
            var baseline = recording ? null : ReadBaseline(baselinePath);
            Report(measured, baseline);

            if (recording)
            {
                WriteBaseline(baselinePath, measured, runs);
                Log($"baseline written to {baselinePath}");
            }
        }

        private readonly record struct Run(long Id, string Workflow, DateTimeOffset Created);

        private readonly record struct JobTiming(string Workflow, string Job, double Exec, double Queue);

        /// <summary>Median exec and queue seconds for one workflow job, over the sample.</summary>
        private sealed record Stat(string Workflow, string Job, double Exec, double Queue, int Samples);

        private static List<Run> FetchRuns()
        {
            var json = Gh($"run list --repo {Repo} --limit {RunsToFetch} "
                          + "--json name,conclusion,createdAt,databaseId");
            if (json is null) return new List<Run>();

            var runs = new List<Run>();
            foreach (var element in JsonDocument.Parse(json).RootElement.EnumerateArray())
            {
                // Only successful runs: a run that failed stopped early, so its duration is
                // the duration of getting to the failure and not of the work.
                if (element.GetProperty("conclusion").GetString() != "success") continue;
                runs.Add(new Run(
                    element.GetProperty("databaseId").GetInt64(),
                    element.GetProperty("name").GetString() ?? "?",
                    DateTimeOffset.Parse(element.GetProperty("createdAt").GetString()!,
                                         CultureInfo.InvariantCulture)));
            }
            return runs;
        }

        private static List<Stat> Measure(List<Run> runs)
        {
            var timings = new List<JobTiming>();
            foreach (var workflow in runs.Select(r => r.Workflow).Distinct().OrderBy(n => n, StringComparer.Ordinal))
            {
                // Newest first, so the answer is about the code as it stands.
                var sample = runs.Where(r => r.Workflow == workflow)
                                 .OrderByDescending(r => r.Created)
                                 .Take(JobSample)
                                 .ToList();
                Log($"  {workflow}: opening {sample.Count} runs");
                foreach (var run in sample)
                    timings.AddRange(JobsOf(run));
            }

            return timings
                .GroupBy(t => (t.Workflow, t.Job))
                .Select(g => new Stat(g.Key.Workflow, g.Key.Job,
                                      Median(g.Select(t => t.Exec)), Median(g.Select(t => t.Queue)), g.Count()))
                .OrderByDescending(s => s.Exec)
                .ToList();
        }

        private static IEnumerable<JobTiming> JobsOf(Run run)
        {
            var json = Gh($"api /repos/{Repo}/actions/runs/{run.Id}/jobs?per_page=100");
            if (json is null) yield break;

            JsonElement jobs;
            try
            {
                if (!JsonDocument.Parse(json).RootElement.TryGetProperty("jobs", out jobs)) yield break;
            }
            catch (JsonException) { yield break; }

            foreach (var job in jobs.EnumerateArray())
            {
                var startedRaw = job.GetProperty("started_at").GetString();
                var completedRaw = job.GetProperty("completed_at").GetString();
                if (startedRaw is null || completedRaw is null) continue;

                var started = DateTimeOffset.Parse(startedRaw, CultureInfo.InvariantCulture);
                var completed = DateTimeOffset.Parse(completedRaw, CultureInfo.InvariantCulture);
                if (completed <= started) continue;

                yield return new JobTiming(run.Workflow, job.GetProperty("name").GetString() ?? "?",
                                           (completed - started).TotalSeconds,
                                           Math.Max(0, (started - run.Created).TotalSeconds));
            }
        }

        private static void Report(List<Stat> measured, (Snapshot Latest, List<Snapshot> History)? baseline)
        {
            var latest = baseline?.Latest;
            // The oldest snapshot on file, so a slow drift over many small steps is visible.
            var oldest = baseline?.History.FirstOrDefault() ?? latest;

            Console.WriteLine();
            var sinceHeader = oldest is null || oldest == latest ? "" : $"since {oldest.Recorded}";
            Console.WriteLine($"{"workflow / job",-46}{"exec",8}{"queue",8}"
                              + $"{(latest is null ? "" : latest.Recorded),12}{"change",9}{sinceHeader,17}");
            Console.WriteLine(new string('-', 100));

            var regressions = new List<string>();
            foreach (var stat in measured)
            {
                var key = Key(stat);
                var line = $"{Truncate(key, 46),-46}{Minutes(stat.Exec),8}{Minutes(stat.Queue),8}";

                if (latest is not null && latest.Jobs.TryGetValue(key, out var was) && was > 0)
                {
                    var change = (stat.Exec - was) / was;
                    line += $"{Minutes(was),12}{change,9:+0.0%;-0.0%;0.0%}";
                    if (change > RegressionThreshold) regressions.Add($"{key} {change:+0%}");

                    if (oldest is not null && oldest != latest
                        && oldest.Jobs.TryGetValue(key, out var first) && first > 0)
                        line += $"{(stat.Exec - first) / first,17:+0.0%;-0.0%;0.0%}";
                }
                else if (latest is not null)
                    line += $"{"new",12}";

                Console.WriteLine(line);
            }

            Console.WriteLine();
            // The jobs run in parallel, so what a contributor waits for is the slowest one,
            // not the sum. The sum is what the project costs the runner pool.
            if (measured.Count > 0)
                Log($"slowest job {Minutes(measured[0].Exec)} ({measured[0].Job} in {measured[0].Workflow}); "
                    + $"{Minutes(measured.Sum(s => s.Exec))} of runner time per commit");

            if (regressions.Count > 0)
            {
                Log($"{regressions.Count} job(s) above the {RegressionThreshold:P0} threshold since "
                    + $"{latest!.Recorded}:");
                foreach (var r in regressions) Log("  " + r);
            }
            else if (latest is not null)
                Log("nothing above the threshold");
        }

        private static string Key(Stat s) => s.Workflow + " / " + s.Job;

        private static string Minutes(double seconds) => (seconds / 60).ToString("0.0", CultureInfo.InvariantCulture) + "m";

        private static string Truncate(string s, int n) => s.Length <= n ? s : s[..(n - 1)] + "…";

        private static double Median(IEnumerable<double> values)
        {
            var sorted = values.OrderBy(v => v).ToList();
            if (sorted.Count is 0) return 0;
            return sorted.Count % 2 is 1
                ? sorted[sorted.Count / 2]
                : (sorted[sorted.Count / 2 - 1] + sorted[sorted.Count / 2]) / 2;
        }

        /// <summary>One dated set of measurements: the current one, or an older one kept for history.</summary>
        private sealed record Snapshot(string Recorded, Dictionary<string, double> Jobs);

        private static (Snapshot Latest, List<Snapshot> History)? ReadBaseline(string path)
        {
            if (!File.Exists(path))
            {
                Log($"no baseline at {path} — run with CI_TIMING_RECORD=1 to write one");
                return null;
            }
            var root = JsonDocument.Parse(File.ReadAllText(path)).RootElement;
            var history = new List<Snapshot>();
            if (root.TryGetProperty("history", out var past))
                history.AddRange(past.EnumerateArray().Select(ReadSnapshot));
            return (ReadSnapshot(root), history);
        }

        private static Snapshot ReadSnapshot(JsonElement element)
            => new(element.GetProperty("recorded").GetString() ?? "?",
                   element.GetProperty("jobs").EnumerateObject()
                          .ToDictionary(p => p.Name, p => p.Value.GetDouble()));

        /// <summary>
        /// Keeps every earlier snapshot. GitHub drops run history after a bounded window and
        /// logs after ninety days, so a measurement that is not written down here cannot be
        /// recovered later — which is how the January 2026 numbers were nearly lost.
        /// </summary>
        private static void WriteBaseline(string path, List<Stat> measured, List<Run> runs)
        {
            var previous = ReadBaseline(path);

            using var stream = File.Create(path);
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
            {
                Indented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // so "C++ Test" reads as itself
            });
            writer.WriteStartObject();
            writer.WriteString("comment", "Median seconds a CI job spends executing, queue time excluded. "
                                        + "Written by Sources/Utils/Utils/CiTiming.cs; see its remarks.");
            writer.WriteString("recorded", runs.Max(r => r.Created).ToString("yyyy-MM-dd"));
            WriteJobs(writer, measured.ToDictionary(Key, s => s.Exec));

            writer.WriteStartArray("history");
            if (previous is { } p)
                foreach (var snapshot in p.History.Append(p.Latest))
                {
                    writer.WriteStartObject();
                    writer.WriteString("recorded", snapshot.Recorded);
                    WriteJobs(writer, snapshot.Jobs);
                    writer.WriteEndObject();
                }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        private static void WriteJobs(Utf8JsonWriter writer, Dictionary<string, double> jobs)
        {
            writer.WriteStartObject("jobs");
            foreach (var (key, seconds) in jobs.OrderBy(j => j.Key, StringComparer.Ordinal))
                writer.WriteNumber(key, Math.Round(seconds, 1));
            writer.WriteEndObject();
        }

        /// <summary>Runs <c>gh</c> and hands back stdout, or null with a reason logged.</summary>
        private static string? Gh(string arguments)
        {
            try
            {
                using var process = Process.Start(new ProcessStartInfo("gh", arguments)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                });
                if (process is null) { Log("could not start gh"); return null; }

                var stdout = process.StandardOutput.ReadToEnd();
                var stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();
                if (process.ExitCode is not 0)
                {
                    Log($"gh {arguments.Split(' ')[0]} exited {process.ExitCode}: {stderr.Trim()}");
                    return null;
                }
                return stdout;
            }
            catch (Exception e)
            {
                Log("gh could not be run: " + e.Message);
                return null;
            }
        }
    }
}
