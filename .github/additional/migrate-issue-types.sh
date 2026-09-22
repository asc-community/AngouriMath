#!/usr/bin/env bash
# asc-community/AngouriMath#1383 -- migrate legacy issue labels to native issue types.
#
# Native taxonomy:
#   Goal        a triaged outcome or initiative; it may generate sub-issues over time
#   Bug         incorrect existing behaviour
#   Feature     new or intentionally changed user-facing behaviour
#   Maintenance internal upkeep without a primary user-facing behaviour change
#   no type     untriaged
#
# Legacy mappings:
#   Bug, Minor bug -> Bug
#   Agentic goal -> Goal
#   Proposal, Design document -> Feature
#
# When labels conflict, Bug wins first, then Agentic goal, then Feature.
#
# This script deliberately does not assign Maintenance to every unmapped issue. Unmapped issues
# remain untyped until triage. Organization type setup (including renaming Task to Maintenance and
# creating Goal) is separate and must be completed before applying this migration.
#
# Dry by default: prints what it would do. --apply runs it. Needs gh with repository write access.
set -euo pipefail

REPO=asc-community/AngouriMath
APPLY=0
REMOVE=0
CLOSEQ=0
LIST_UNTRIAGED=0

usage() {
  cat <<'EOF'
Usage: migrate-issue-types.sh [options]

Options:
  --apply             Perform type, label, comment, and close changes.
  --remove-labels     Remove legacy type labels after assigning native types.
  --close-questions   Comment on and close open question issues.
  --list-untriaged    List unmapped issues that remain without a native type.
  -h, --help          Show this help.

The default is a dry run.
EOF
}

for arg in "$@"; do
  case "$arg" in
    --apply) APPLY=1 ;;
    --remove-labels) REMOVE=1 ;;
    --close-questions) CLOSEQ=1 ;;
    --list-untriaged) LIST_UNTRIAGED=1 ;;
    -h|--help) usage; exit 0 ;;
    *) echo "error: unknown option: $arg" >&2; usage >&2; exit 2 ;;
  esac
done

has_label() {
  case "|$labels|" in
    *"|$1|"*) return 0 ;;
    *) return 1 ;;
  esac
}

for required_type in Goal Bug Feature Maintenance; do
  if ! gh api orgs/asc-community/issue-types --jq '.[].name' \
    | grep -Fqx -- "$required_type"; then
    echo "error: organization issue type '$required_type' is missing" >&2
    exit 1
  fi
done

gh api --paginate "repos/$REPO/issues?state=all&per_page=100" \
  --jq '.[] | select(.pull_request == null) | [.number, .state, (.type.name // "-"), ([.labels[].name] | join("|"))] | @tsv' \
| sort -n \
| while IFS=$'\t' read -r number state type labels; do
  question=0
  if has_label "Question"; then
    question=1
  elif has_label "Opinions wanted" \
    && ! has_label "Proposal" \
    && ! has_label "Bug" \
    && ! has_label "Minor bug" \
    && ! has_label "Accepted" \
    && ! has_label "Design document" \
    && ! has_label "Agentic goal"; then
    question=1
  fi

  want=""
  if has_label "Bug" || has_label "Minor bug"; then
    want=Bug
  elif has_label "Agentic goal"; then
    want=Goal
  elif has_label "Proposal" || has_label "Design document"; then
    want=Feature
  fi

  if [ -n "$want" ] && [ "$type" != "$want" ]; then
    echo "#$number ($state): $type -> $want  [$labels]"
    if [ "$APPLY" = 1 ]; then
      gh api -X PATCH "repos/$REPO/issues/$number" -f "type=$want" --silent
    fi
  fi

  if [ "$REMOVE" = 1 ] && [ -n "$want" ]; then
    for legacy_label in Bug "Minor bug" Proposal "Agentic goal"; do
      if has_label "$legacy_label"; then
        if [ "$APPLY" = 1 ]; then
          gh issue edit "$number" --repo "$REPO" --remove-label "$legacy_label" >/dev/null
          echo "  -label '$legacy_label'"
        else
          echo "  would remove label '$legacy_label'"
        fi
      fi
    done
  fi

  if [ "$question" = 1 ] && [ "$state" = open ]; then
    echo "#$number: a question -> Discussions  [$labels]"
    if [ "$CLOSEQ" = 1 ] && [ "$APPLY" = 1 ]; then
      gh issue comment "$number" --repo "$REPO" --body \
        "Questions and requests for opinions live in [Discussions](https://github.com/asc-community/AngouriMath/discussions) now (#1383), not in the issue tracker. If this becomes a concrete work item, open a new typed issue." \
        >/dev/null
      gh issue close "$number" --repo "$REPO" >/dev/null
      echo "  closed"
    fi
  elif [ "$LIST_UNTRIAGED" = 1 ] && [ -z "$want" ] && [ "$question" = 0 ]; then
    echo "#$number ($state): remains untyped/untriaged  [$labels]"
  fi
done
