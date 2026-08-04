using UnityEngine;
using Helper.Wait;
[RequireComponent(typeof(AudioListener))]
public class ManagedAudioListener : MonoBehaviour
{
    [SerializeField] private int priority;
    public int Priority => priority;

    private AudioListener audioListener;

    private void Awake()
    {
        audioListener = GetComponent<AudioListener>();
        audioListener.enabled = false; // manager decides who's active
    }

    private void OnEnable()
    {
        if (AudioListenerManager.HasInstance)
            AudioListenerManager.Instance.RegisterListener(this);
        else
            WaitForHelper.WaitForObjectThenPerformAction(
                () =>AudioListenerManager.Instance, 
                () => AudioListenerManager.Instance.RegisterListener(this));
    }

    private void OnDisable()
    {
        // Guard against recreating the singleton during teardown/quit.
        if (AudioListenerManager.HasInstance)
            AudioListenerManager.Instance.UnregisterListener(this);
    }

    public void ActivateListener() => audioListener.enabled = true;
    public void DeactivateListener() => audioListener.enabled = false;
}