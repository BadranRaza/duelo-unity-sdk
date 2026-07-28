#!/usr/bin/env bash
set -euo pipefail

cd "$(git rev-parse --show-toplevel)"

sync_tree() {
  local source=$1
  local target=$2
  rm -rf "$target"
  mkdir -p "$(dirname "$target")"
  cp -R "$source" "$target"
}

sync_tree Assets/Duelo/Runtime Runtime
cp Assets/Duelo/Runtime.meta Runtime.meta
sync_tree Assets/Duelo/Editor Editor
cp Assets/Duelo/Editor.meta Editor.meta
sync_tree Assets/Duelo/Tests Tests
cp Assets/Duelo/Tests.meta Tests.meta
sync_tree Assets/Duelo/Samples/BasicIntegration Samples~/BasicIntegration
cp Assets/Duelo/Samples/BasicIntegration.meta Samples~/BasicIntegration.meta

printf 'Synced editable SDK to root UPM payload.\n'
