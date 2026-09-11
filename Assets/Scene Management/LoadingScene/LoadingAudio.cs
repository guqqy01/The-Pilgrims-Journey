using UnityEngine;

public class LoadingAudio : MonoBehaviour
{
    [SerializeField]
    private Theme theme;
    [SerializeField]
    private float fadeOut = 0.2f;
    private void OnEnable()
    {
        ThemeManager.IfReady((manager) => manager.PlayTheme(theme));
    }

    private void OnDisable()
    {
        ThemeManager.IfReady(StopIfPlaying);
    }

    private void StopIfPlaying(ThemeManager themeManager)
    {
        if (themeManager.IsPlayingTheme(theme))
            themeManager.StopTheme(fadeOut);
    }
}
