mergeInto(LibraryManager.library, {
  NotifyReady: function (objectNamePtr) {
    // Self-describing handshake: report the Unity GameObject that hosts the
    // DUELO bridge so the React host can SendMessage to it by name instead of
    // relying on a hardcoded magic name. Falls back to {} for older hosts.
    var bridgeObject = objectNamePtr ? UTF8ToString(objectNamePtr) : "";
    window.parent.postMessage(
      {
        type: "duelo-ready",
        payload: bridgeObject ? { bridgeObject: bridgeObject } : {},
      },
      window.location.origin,
    );
  },
  NotifyPlayable: function () {
    window.parent.postMessage(
      { type: "duelo-playable", payload: {} },
      window.location.origin,
    );
  },
  SubmitMove: function (moveJson) {
    var matchId = new URLSearchParams(window.location.search).get("matchId");
    window.parent.postMessage(
      {
        type: "duelo-move",
        payload: { matchId: matchId, move: JSON.parse(UTF8ToString(moveJson)) },
      },
      window.location.origin,
    );
  },
  NotifyMatchEnd: function (resultJson) {
    window.parent.postMessage(
      {
        type: "duelo-match-end",
        payload: JSON.parse(UTF8ToString(resultJson)),
      },
      window.location.origin,
    );
  },
});
