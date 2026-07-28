#!/usr/bin/env bash
set -euo pipefail

cd "$(git rev-parse --show-toplevel)"

required=(
  package.json
  package.json.meta
  README.md.meta
  CHANGELOG.md.meta
  INSTALL.md.meta
  LICENSE.md.meta
  THIRD_PARTY_NOTICES.md.meta
  Runtime/Duelo.Runtime.asmdef
  Runtime.meta
  Runtime/DueloManager.cs
  Runtime/DueloManager.cs.meta
  Runtime/Plugins/WebGL/DueloBridge.jslib
  Editor/Duelo.Editor.asmdef
  Editor.meta
  Editor/DueloWebGLTemplateInstaller.cs
  Editor/Templates/WebGL/Duelo/index.html
  Tests/Editor/Duelo.EditorTests.asmdef
  Tests.meta
  Samples~/BasicIntegration/BasicDueloAdapter.cs
  Samples~/BasicIntegration.meta
  Assets/Duelo/Runtime/Duelo.Runtime.asmdef
  Assets/Duelo/Editor/Duelo.Editor.asmdef
  Assets/Duelo/Tests/Editor/Duelo.EditorTests.asmdef
  Assets/Duelo/Samples/BasicIntegration/BasicDueloAdapter.cs
  Packages/manifest.json
  ProjectSettings/ProjectVersion.txt
  THIRD_PARTY_NOTICES.md
)

for path in "${required[@]}"; do
  [[ -f "$path" ]] || {
    printf 'missing required package file: %s\n' "$path" >&2
    exit 1
  }
done

node <<'NODE'
const fs = require("fs");
const pkg = JSON.parse(fs.readFileSync("package.json", "utf8"));
if (pkg.name !== "com.duelo.unity-sdk") throw new Error("wrong package name");
if (pkg.version !== "1.0.0") throw new Error("wrong package version");
if (pkg.unity !== "6000.4") throw new Error("wrong minimum Unity version");
if (!pkg.samples?.some((sample) => sample.path === "Samples~/BasicIntegration")) {
  throw new Error("BasicIntegration sample is not declared");
}
NODE

grep -qx 'guid: 7c3737f8677977d4599f0b5b9cd66a49' Runtime/DueloManager.cs.meta
grep -q 'Copyright (c) 2012-2022 Markus Göbel' THIRD_PARTY_NOTICES.md
grep -q 'hasNotifiedPlayable' Samples~/BasicIntegration/BasicDueloAdapter.cs

diff -qr Assets/Duelo/Runtime Runtime
diff -qr Assets/Duelo/Editor Editor
diff -qr Assets/Duelo/Tests Tests
diff -qr Assets/Duelo/Samples/BasicIntegration Samples~/BasicIntegration
cmp Assets/Duelo/Runtime.meta Runtime.meta
cmp Assets/Duelo/Editor.meta Editor.meta
cmp Assets/Duelo/Tests.meta Tests.meta
cmp Assets/Duelo/Samples/BasicIntegration.meta Samples~/BasicIntegration.meta

if grep -R -nF 'Assets/Duelo/Runtime/Plugins/WebGL/DueloBridge.jslib' \
  Assets/Duelo/Editor Editor; then
  echo 'hardcoded editable-only bridge path found' >&2
  exit 1
fi

if grep -R -nF 'Assets/Duelo/Editor/Templates/WebGL/Duelo/index.html' \
  Assets/Duelo/Editor Editor; then
  echo 'hardcoded editable-only template path found' >&2
  exit 1
fi

printf 'DUELO Unity SDK package checks passed.\n'
