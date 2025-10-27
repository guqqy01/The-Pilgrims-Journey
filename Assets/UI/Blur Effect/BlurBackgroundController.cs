using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.Collections;
public class BlurBackgroundController : MonoBehaviour, ISingletonToBeInitiatedFromRoot
{
    public static BlurBackgroundController Instance { get; private set; }

    [SerializeField]
    private Volume blurVolume;

    [SerializeField]
    private float timeToBlur = 0.4f;
    private float BlurSpeed => 1f / timeToBlur;

    public void InitiateSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void EnableBlur()
    {
        StartCoroutine(Blur(true));
    }

    public void DisableBlur()
    {
        StartCoroutine(Blur(false));
    }

    public IEnumerator Blur(bool on)
    {
        float desiredWeight = on ? 1.0f : 0.0f;
        float sign = on ? 1f : -1f;

        while(blurVolume.weight != desiredWeight)
        {
            blurVolume.weight = Mathf.Clamp01(blurVolume.weight + sign * BlurSpeed * Time.unscaledDeltaTime);
            yield return null;
        }
    }

    
}
