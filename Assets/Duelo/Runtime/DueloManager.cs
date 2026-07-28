using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;

public class DueloManager : MonoBehaviour
{
    public static DueloManager Instance { get; private set; }

    [Header("Match Context")]
    public string matchId;
    public string p1Id;
    public string p2Id;
    public string p1Name;
    public string p2Name;
    public string p1Pic;
    public string p2Pic;
    public float turnTimer;
    public int gridSize;
    public string matchMode;

    [Header("Player Settings")]
    public string localPlayerId;
    public bool isP1;

    public event Action<JSONNode> OnStateReceived;
    public event Action<string, JSONNode> OnEventReceived;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ParseUrlParameters();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Report THIS GameObject's name so the DUELO host targets it with
        // SendMessage. The host falls back to "GameManager" for older builds,
        // so the bridge object can be named anything (e.g. "DueloManager").
        DueloBridge.NotifyReady(gameObject.name);
    }

    private void ParseUrlParameters()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string url = Application.absoluteURL;
        if (url.Contains("?"))
        {
            string queryString = url.Split('?')[1];
            string[] parameters = queryString.Split('&');
            foreach (string param in parameters)
            {
                string[] kvp = param.Split(new[] { '=' }, 2);
                if (kvp.Length == 2)
                {
                    string key = kvp[0];
                    string value = UnityWebRequest.UnEscapeURL(kvp[1]);
                    switch (key)
                    {
                        case "matchId": matchId = value; break;
                        case "p1Id": p1Id = value; break;
                        case "p2Id": p2Id = value; break;
                        case "p1Name": p1Name = value; break;
                        case "p2Name": p2Name = value; break;
                        case "p1Pic": p1Pic = value; break;
                        case "p2Pic": p2Pic = value; break;
                        case "localPlayerId": localPlayerId = value; break;
                        case "turnTimer": float.TryParse(value, out turnTimer); break;
                        case "gridSize": int.TryParse(value, out gridSize); break;
                        case "matchMode": matchMode = value; break;
                    }
                }
            }
        }
#else
        // Mock data for editor testing
        matchId = "mock-match-id";
        p1Id = "p1";
        p2Id = "p2";
        p1Name = "Player 1";
        p2Name = "Player 2";
        turnTimer = 30f;
        matchMode = "realtime";
        localPlayerId = "p1"; // Default to P1 in editor
#endif
        isP1 = localPlayerId == p1Id;
    }

    public void SetLocalPlayerId(string userId)
    {
        if (string.IsNullOrEmpty(userId)) return;

        localPlayerId = userId;
        isP1 = localPlayerId == p1Id;
    }

    public void ApplyStateEnvelope(JSONNode node)
    {
        if (node == null) return;

        string stateLocalPlayerId = node["localPlayerId"];
        SetLocalPlayerId(stateLocalPlayerId);
    }

    // Backward-compatible target. DUELO sends to GameManager.
    public void ReceiveState(string json)
    {
        Debug.Log($"[DueloManager] ReceiveState: {json}");
        JSONNode node = JSON.Parse(json);
        ApplyStateEnvelope(node);
        OnStateReceived?.Invoke(node);
    }

    // Backward-compatible target. DUELO sends to GameManager.
    public void ReceiveEvent(string json)
    {
        Debug.Log($"[DueloManager] ReceiveEvent: {json}");
        JSONNode node = JSON.Parse(json);
        string type = node["type"];
        JSONNode data = node["data"];
        OnEventReceived?.Invoke(type, data);
    }

    public void SubmitMove(JSONNode moveData)
    {
        DueloBridge.SubmitMove(moveData.ToString());
    }

    public void NotifyMatchEnd(string result, string winnerId, int durationSeconds)
    {
        JSONObject payload = new JSONObject();
        payload["result"] = result;
        payload["matchId"] = matchId;
        payload["winnerId"] = winnerId;
        payload["gameDurationSeconds"] = durationSeconds;
        DueloBridge.NotifyMatchEnd(payload.ToString());
    }
}
