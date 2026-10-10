using UnityEngine;
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
        AudioListenerManager.WhenReady(manager => manager.RegisterListener(this));
    }

    private void OnDisable()
    {
        AudioListenerManager.IfReady(manager => manager.UnregisterListener(this));
    }

    public void ActivateListener() => audioListener.enabled = true;
    public void DeactivateListener() => audioListener.enabled = false;
}