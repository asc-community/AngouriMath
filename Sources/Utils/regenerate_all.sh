#!/usr/bin/env bash
#
# Regenerates every checked-in generated file that is not the ANTLR parser, in place.
#
# The parser has its own job, GrammarUpToDate.yml, because regenerating it needs a Java runtime
# and this needs a dotnet tool -- keeping them apart means neither is red for the other's reason.
#
# Run from anywhere; paths are resolved from this script's own location, not from the working
# directory, because a report that writes where you are not reading goes stale invisibly.
#
# https://github.com/asc-community/AngouriMath/issues/1034

set -euo pipefail

sources="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
kernel="$sources/AngouriMath"

# The three T4 templates, read out of the project file rather than listed here -- a list written
# twice is a list that drifts, and this one would drift silently in the direction of covering
# less. `None Update="a\b.tt"` is MSBuild's Windows separator, so it is translated on the way out.
templates=$(
    grep -o 'None Update="[^"]*\.tt"' "$kernel/AngouriMath.csproj" \
    | sed 's/None Update="//; s/"$//; s|\\|/|g'
)

if [ -z "$templates" ]; then
    echo "no .tt templates found in AngouriMath.csproj -- has the declaration shape changed?" >&2
    exit 1
fi

# `dotnet tool install -g dotnet-t4` installs a binary called `t4`, not `dotnet-t4`, so it is not
# reachable as `dotnet t4` and is only on PATH if the tools directory is. Looked up rather than
# assumed, because the failure otherwise reads as "you misspelled a built-in dotnet command".
t4=$(command -v t4 || true)
if [ -z "$t4" ] && [ -x "$HOME/.dotnet/tools/t4" ]; then
    t4="$HOME/.dotnet/tools/t4"
fi
if [ -z "$t4" ]; then
    echo "t4 not found. Install it with: dotnet tool install -g dotnet-t4" >&2
    exit 1
fi

for template in $templates; do
    echo "t4: $template"
    "$t4" "$kernel/$template" -o "$kernel/${template%.tt}.cs"
done

# And the two hand-written generators, which write their own destinations.
echo "utils: ExtensionGenerator"
(cd "$sources/Utils" && dotnet run --project Utils -c Release ExtensionGenerator)
echo "utils: AdditionalExtensionsTestGenerator"
(cd "$sources/Utils" && dotnet run --project Utils -c Release AdditionalExtensionsTestGenerator)
