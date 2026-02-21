#!/usr/bin/env bash
set -euo pipefail

repo_root="$(git rev-parse --show-toplevel)"
cd "$repo_root"

current="$(git config --local --get core.hooksPath || true)"
if [[ -n "$current" && "${1:-}" != "--force" ]]; then
  echo "core.hooksPath is already set to '$current'. Use --force to override."
  exit 0
fi

git config --local core.hooksPath ".githooks"
echo "Configured core.hooksPath=.githooks"
