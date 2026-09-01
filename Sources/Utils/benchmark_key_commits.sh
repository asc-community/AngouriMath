#!/usr/bin/env bash
#
# Runs the inter-version CPU benchmark against every commit in key-commits.txt and writes one
# table of what it measured.
#
# https://github.com/asc-community/AngouriMath/issues/529
#
# The benchmark that runs is always the one at HEAD, overlaid onto each checked-out commit, so
# that what differs between rows is the kernel rather than the question. #529 calls this
# "replaces the Program.cs".
#
# Each commit is checked out *whole* rather than just Sources/AngouriMath, which was tried first
# and does not work: the kernel's rule sets are produced by a source generator that lives
# elsewhere in the tree, so a kernel on its own is missing `Patterns.SortRulesArms` and will not
# compile. Measured, not guessed.
#
# A commit that will not build is reported as such rather than skipped. The benchmark at HEAD
# calls the public API as it is now, so an old enough commit genuinely cannot be measured this
# way, and that is worth seeing rather than silently omitting.
#
# Run from anywhere.

set -uo pipefail

here="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repository="$(cd "$here/../.." && pwd)"
benchmark="Sources/Tests/DotnetBenchmark"
list="$repository/$benchmark/key-commits.txt"
work="${BENCHMARK_WORK_DIR:-$(mktemp -d)}"
out="${BENCHMARK_OUTPUT:-$repository/$benchmark/benchmark_results.csv/key-commits.md}"

if [ ! -f "$list" ]; then
    echo "no key-commits.txt at $list" >&2
    exit 1
fi

mkdir -p "$(dirname "$out")"
results="$work/results"
mkdir -p "$results"

commits=$(grep -vE '^\s*(#|$)' "$list")
echo "measuring:"; echo "$commits" | sed 's/^/  /'

for commit in $commits; do
    resolved=$(git -C "$repository" rev-parse --short "$commit" 2>/dev/null)
    if [ -z "$resolved" ]; then
        echo "=== $commit: not a commit in this repository"
        echo "unresolved" > "$results/$commit.status"
        continue
    fi

    echo "=== $commit ($resolved)"
    tree="$work/$commit"
    rm -rf "$tree"; mkdir -p "$tree"
    git -C "$repository" archive "$commit" | tar -x -C "$tree" || { echo "archive failed" > "$results/$commit.status"; continue; }

    # The benchmark from HEAD, so every row is asked the same question.
    rm -rf "$tree/$benchmark"
    mkdir -p "$tree/Sources/Tests"
    cp -r "$repository/$benchmark" "$tree/Sources/Tests/"
    rm -rf "$tree/$benchmark/bin" "$tree/$benchmark/obj" "$tree/$benchmark/benchmark_results.csv"

    if ! (cd "$tree/$benchmark" && dotnet build -c Release > "$results/$commit.build.log" 2>&1); then
        echo "  did not build — see $results/$commit.build.log"
        echo "did not build" > "$results/$commit.status"
        continue
    fi
    if ! (cd "$tree/$benchmark" && dotnet run -c Release --no-build CommonFunctionsInterVersion > "$results/$commit.run.log" 2>&1); then
        echo "  did not run — see $results/$commit.run.log"
        echo "did not run" > "$results/$commit.status"
        continue
    fi

    measured="$tree/$benchmark/benchmark_results.csv/measured-CommonFunctionsInterVersion.json"
    if [ -f "$measured" ]; then
        cp "$measured" "$results/$commit.json"
        echo "measured $resolved" > "$results/$commit.status"
        echo "  measured"
    else
        echo "ran but wrote nothing" > "$results/$commit.status"
        echo "  ran but wrote no results file"
    fi
done

python3 "$here/combine_key_commits.py" "$list" "$results" "$out" || exit 1
echo
echo "wrote $out"
