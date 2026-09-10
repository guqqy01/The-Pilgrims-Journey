using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Theme")]
public class Theme : ScriptableObject
{
    public List<AudioClipAndSettings> Songs;
}
