using SimpleJSON;
using UnityEngine;

public sealed class BasicDueloAdapter : MonoBehaviour
{
    private DueloManager manager;
    private bool hasNotifiedPlayable;

    private void Start()
    {
        manager = DueloManager.Instance;
        if (manager == null)
        {
            Debug.LogError("[BasicDueloAdapter] Scene needs one DueloManager.");
            enabled = false;
            return;
        }

        manager.OnStateReceived += HandleState;
        manager.OnEventReceived += HandleEvent;
    }

    private void OnDestroy()
    {
        if (manager == null)
        {
            return;
        }

        manager.OnStateReceived -= HandleState;
        manager.OnEventReceived -= HandleEvent;
    }

    public void SubmitExampleMove()
    {
        var move = new JSONObject();
        move["position"] = 0;
        manager?.SubmitMove(move);
    }

    private void HandleState(JSONNode state)
    {
        RenderAuthoritativeState(state);
        if (!hasNotifiedPlayable)
        {
            hasNotifiedPlayable = true;
            DueloBridge.NotifyPlayable();
        }
    }

    private static void HandleEvent(string type, JSONNode data)
    {
        Debug.Log($"[BasicDueloAdapter] Event {type}: {data}");
    }

    private static void RenderAuthoritativeState(JSONNode state)
    {
        Debug.Log($"[BasicDueloAdapter] Render state: {state}");
    }
}
