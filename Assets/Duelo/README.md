# DUELO Unity SDK

This is the editable source of the `com.duelo.unity-sdk` package.

## Distribution status

Run `scripts/sync-sdk.sh` from the repository root after editing this folder.
The release gate compares this source, including `.meta` files, with the UPM
payload at the repository root.

## Runtime files

- `Runtime/DueloManager.cs` handles URL bootstrap data, match state envelopes, local-player assignment, and outbound move intents.
- `Runtime/DueloBridge.cs` exposes the WebGL JavaScript bridge to C#.
- `Runtime/Plugins/WebGL/DueloBridge.jslib` is the WebGL browser bridge plugin used by the C# bridge.
- `Editor/DueloWebGLSettings.cs` applies and verifies DUELO-compatible WebGL build settings.
- `Editor/DueloWebGLTemplateInstaller.cs` installs the Unity-required WebGL template from the SDK copy.
- `Editor/Templates/WebGL/Duelo/index.html` is the canonical DUELO WebGL template shipped with the SDK and emits `duelo-ready` after the Unity runtime is created. Game code must emit `duelo-playable` only after it has applied the first server state and the board is visible.

## Bridge handshake — self-describing target (do not hardcode names)

The DUELO host reaches Unity via `unityInstance.SendMessage(<object>, 'ReceiveState'|'ReceiveEvent', json)`, which targets a GameObject **by name**. The SDK is self-describing so the bridge GameObject can be named anything:

- `DueloManager.Start()` calls `DueloBridge.NotifyReady(gameObject.name)`, which sends `duelo-ready` with `payload.bridgeObject = "<that object's name>"`.
- The host stores `bridgeObject` and uses it for every `SendMessage`, falling back to `'GameManager'` only for older builds that send no name.

**Requirement:** the GameObject holding the component that receives `ReceiveState`/`ReceiveEvent` must be the same GameObject whose name is reported. Never assume it is named `GameManager`.

If a match hangs at "loading 95%" and then voids, open the iframe console: `SendMessage: object <name> not found!` means the targeted bridge object does not exist — verify `DueloManager.NotifyReady(gameObject.name)` runs and that the receiver lives on that object.

## DUELO mode owns result UI — the game must not show its own end screen

In DUELO mode the React host owns the post-match result (win/loss/draw, payout, rematch, void/refund), the turn timer, turn/status, player identity, and reconnect messaging. The Unity build must render **only the board and the non-financial win line** — no win/draw/game-over banner, timer, or player strip of its own. Gate every local end-screen / HUD call behind "not DUELO mode" so local (non-DUELO) play is unaffected.

The full, repo-agnostic list of what React owns vs what the Unity build may
render lives in **`HaseebDev/duelo` →
`memory-bank/docs/unity-integration.md` → "DUELO game integration
checklist"**. Run it before shipping or rebuilding this game.

## Build settings

Use **DUELO > Setup Project** after importing the SDK. Setup creates or repairs `Assets/WebGLTemplates/Duelo/index.html` from the SDK copy, then applies the required WebGL settings. It will not silently overwrite an existing non-DUELO template; if one exists, it asks before backing it up and replacing it.

Use **DUELO > Apply Compatible WebGL Settings** before a manual build if you only need to reapply settings. The package also runs a pre-build check for every WebGL build and forces the required settings:

- WebGL template: `PROJECT:Duelo`
- Compression format: Brotli
- Decompression fallback: disabled
- Data caching: enabled
- Debug symbols and diagnostics: disabled

Use **DUELO > Validate Compatible WebGL Settings** to inspect the current project state without changing anything.

## Unity special folders

One generated DUELO integration file must stay outside this folder because Unity requires a special asset location:

- `Assets/WebGLTemplates/Duelo/index.html` is the WebGL template selected in Project Settings.

The SDK auto-creates that file from `Assets/Duelo/Editor/Templates/WebGL/Duelo/index.html` when it is missing. Keep the `DUELO_TEMPLATE_VERSION` marker in managed templates so future SDK updates can safely refresh the generated copy.

Keep financial data out of Unity. DUELO sends gameplay identity and match bootstrap data only; balances, stakes, escrow, fees, payouts, match status, and visible turn timers stay in the React/API layer. Unity renders the game board and sends move intentions after DUELO starts the authoritative turn clock.
