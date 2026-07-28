# Basic integration

Add one `DueloManager` and your game-owned adapter to the same scene. The
adapter subscribes to authoritative state/events, submits move intentions, and
calls `DueloBridge.NotifyPlayable()` only after the first board state is visible.

`BasicDueloAdapter` demonstrates the SDK calls. Replace its example move and
rendering logs with the game-specific adapter; do not put game rules in the SDK.
