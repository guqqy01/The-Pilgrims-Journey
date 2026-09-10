using System.Collections.Generic;
using UnityEngine;

public class AudioListenerManager : MonoBehaviour
{
    private readonly List<ManagedAudioListener> listeners = new List<ManagedAudioListener>();

    public ManagedAudioListener ActiveListenerController { get; private set; }

    public static AudioListenerManager Instance { get; private set; }
    public static bool HasInstance { get { return Instance != null; } }
    private void Awake()
    {
        Instance = this;
    }

    public void RegisterListener(ManagedAudioListener controller)
    {
        if (controller == null || listeners.Contains(controller))
            return;

        listeners.Add(controller);
        UpdateActive();
    }

    public void UnregisterListener(ManagedAudioListener controller)
    {
        if (!listeners.Remove(controller))
            return;

        if (ActiveListenerController == controller)
            ActiveListenerController = null;

        UpdateActive();
    }

    private void UpdateActive()
    {
        ManagedAudioListener best = null;
        int bestPriority = int.MinValue;

        // Highest priority wins; first-registered wins ties.
        foreach (var listener in listeners)
        {
            if (listener.Priority > bestPriority)
            {
                bestPriority = listener.Priority;
                best = listener;
            }
        }

        if (best == ActiveListenerController)
            return;

        ActiveListenerController?.DeactivateListener();
        ActiveListenerController = best;
        ActiveListenerController?.ActivateListener();
    }
}
