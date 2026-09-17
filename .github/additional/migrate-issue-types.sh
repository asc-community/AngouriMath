#!/bin/bash
# asc-community/AngouriMath#1383 -- the restructuring the maintainer authorised on 2026-09-16:
#
#   1. every issue gets its type from its labels:
#        Bug, Minor bug            -> Bug
#        Proposal, Design document -> Feature
#      and an issue with neither is a goal, which has no type: a goal is the meta-issue its
#      sub-issues hang off, and a blank issue is one until the triage says otherwise
#   2. the type labels `Bug`, `Minor bug`, `Proposal` come off (with --remove-labels)
#   3. questions (`Question`, or `Opinions wanted` alone) are redirected to Discussions with a
#      comment, and closed when already answered (with --close-questions; open ones without an
#      answer are listed, not closed)
#
# Dry by default: prints what it would do. --apply runs it. Needs `gh` with push on the repo.
set -euo pipefail
REPO=asc-community/AngouriMath
APPLY=0; REMOVE=0; CLOSEQ=0
for a in "$@"; do case $a in --apply) APPLY=1;; --remove-labels) REMOVE=1;; --close-questions) CLOSEQ=1;; esac; done

# The issues.
gh api --paginate "repos/$REPO/issues?state=all&per_page=100" \
  --jq '.[] | select(.pull_request == null) | [.number, .state, (.type.name // "-"), ([.labels[].name] | join("|"))] | @tsv' \
| sort -n | while IFS=$'\t' read -r number state type labels; do
  question=0
  case "|$labels|" in *"|Question|"*) question=1;; esac
  case "|$labels|" in *"|Opinions wanted|"*) case "|$labels|" in *"|Proposal|"*|*"|Bug|"*|*"|Minor bug|"*|*"|Accepted|"*|*"|Design document|"*|*"|Agentic goal|"*) ;; *) question=1;; esac;; esac
  want=""
  case "|$labels|" in
    *"|Bug|"*|*"|Minor bug|"*) want=Bug;;
    *"|Proposal|"*|*"|Design document|"*) want=Feature;;
  esac
  if [ -n "$want" ] && [ "$type" != "$want" ]; then
    echo "#$number ($state): $type -> $want  [$labels]"
    [ $APPLY = 1 ] && gh api -X PATCH "repos/$REPO/issues/$number" -f type="$want" --silent
  fi
  if [ $REMOVE = 1 ] && [ $APPLY = 1 ] && [ -n "$want" ]; then
    for l in Bug "Minor bug" Proposal; do
      case "|$labels|" in *"|$l|"*) gh issue edit "$number" --repo "$REPO" --remove-label "$l" >/dev/null && echo "  -label '$l'";; esac
    done
  fi
  if [ $question = 1 ] && [ "$state" = open ]; then
    echo "#$number: a question -> Discussions  [$labels]"
    if [ $CLOSEQ = 1 ] && [ $APPLY = 1 ]; then
      gh issue comment "$number" --repo "$REPO" --body "Questions and requests for opinions live in [Discussions](https://github.com/asc-community/AngouriMath/discussions) now (#1383), not in the issue tracker; this one is closed as answered there or here, and a work item that comes of it gets its own issue. Reopen if something concrete was left unanswered." >/dev/null
      gh issue close "$number" --repo "$REPO" >/dev/null && echo "  closed"
    fi
  fi
done
