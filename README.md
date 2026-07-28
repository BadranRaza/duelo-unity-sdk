# DUELO Unity SDK

Official Unity package for connecting a WebGL game to DUELO's authoritative
match host.

## Install

Unity Package Manager → **Add package from git URL**:

```text
https://github.com/BadranRaza/duelo-unity-sdk.git#v1.0.0
```

Pin a release tag. Do not consume `main` in production games.

Then:

1. Run **DUELO → Setup Project**.
2. Add one `DueloManager` component to the scene.
3. Keep the game adapter in the game repository.
4. Subscribe to `OnStateReceived` and `OnEventReceived`.
5. Submit move intentions with `SubmitMove`.
6. Call `DueloBridge.NotifyPlayable()` only after the first authoritative board
   state is visibly rendered.

The package preserves the global `DueloManager`, `DueloBridge`, and
`SimpleJSON` API used by existing games.

## Ownership boundary

Unity renders game state and sends move intentions. DUELO owns identity,
timers, reconnect/exit, results, stakes, balances, payouts, and settlement.
Unity must never receive or render financial data.

The WebGL template is installed into
`Assets/WebGLTemplates/Duelo/index.html`, because Unity only discovers custom
templates from that project-level special folder. The installer updates
DUELO-managed templates and refuses to silently overwrite unmanaged ones.

## Development

This repository is also a Unity 6000.4 developer project. Edit
`Assets/Duelo/`, then run:

```bash
./scripts/sync-sdk.sh
./scripts/test-package.sh
```

The sync copies the editable SDK, tests, sample, and every `.meta` file into
the root UPM payload. See [RELEASING.md](RELEASING.md).
