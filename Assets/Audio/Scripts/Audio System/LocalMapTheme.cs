using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Local Map Theme")]
public class LocalMapTheme : ScriptableObject
{
    public Theme Relax;
    public AudioClipAndSettings RelaxToPlayImpact;
    public Theme Play;
    public AudioClipAndSettings PlayToEscapeImpact;
    public Theme Escape;
}