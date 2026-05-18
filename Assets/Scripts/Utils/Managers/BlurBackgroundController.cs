using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.Collections;
using GameManagerCalls;
using System.Collections.Generic;

public class BlurBackgroundController : Singleton< BlurBackgroundController>, IOnSetupNewGame
{
    private readonly HashSet<string> blurRequests = new HashSet<string>();

    [SerializeField]
    private Volume blurVolume;

    [SerializeField]
    private float timeToBlur = 0.4f;
    private float BlurSpeed => 1f / timeToBlur;

    private Coroutine currentCoroutine;

    public void EnableBlur(string reference)
    {
        blurRequests.Add(reference);

        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(Blur(true));
    }

    public void RemoveBlurRequest(string reference)
    {
        blurRequests.Remove(reference);

        if (blurRequests.Count > 0)
            return;

        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(Blur(false));
    }

    private IEnumerator Blur(bool on)
    {
        float desiredWeight = on ? 1.0f : 0.0f;
        float sign = on ? 1f : -1f;

        while(blurVolume.weight != desiredWeight)
        {
            blurVolume.weight = Mathf.Clamp01(blurVolume.weight + sign * BlurSpeed * Time.unscaledDeltaTime);
            yield return null;
        }
    }

    public void OnSetupNewGame()
    {
        blurRequests.Clear();
        blurVolume.weight = 0;
    }
}
