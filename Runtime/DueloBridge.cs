using System.Runtime.InteropServices;
using UnityEngine;

public static class DueloBridge
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    public static extern void NotifyReady(string objectName);

    [DllImport("__Internal")]
    public static extern void NotifyPlayable();

    [DllImport("__Internal")]
    public static extern void SubmitMove(string moveJson);

    [DllImport("__Internal")]
    public static extern void NotifyMatchEnd(string resultJson);
#else
    public static void NotifyReady(string objectName)
    {
        Debug.Log($"[DueloBridge] NotifyReady called: {objectName}");
    }

    public static void NotifyPlayable()
    {
        Debug.Log("[DueloBridge] NotifyPlayable called");
    }

    public static void SubmitMove(string moveJson)
    {
        Debug.Log($"[DueloBridge] SubmitMove called: {moveJson}");
    }

    public static void NotifyMatchEnd(string resultJson)
    {
        Debug.Log($"[DueloBridge] NotifyMatchEnd called: {resultJson}");
    }
#endif
}
