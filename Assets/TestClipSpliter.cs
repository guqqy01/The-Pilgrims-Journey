using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestClipSpliter : MonoBehaviour
{
    [SerializeField]
    private AudioClip clip;
    [SerializeField]
    private List<float> times;

    [SerializeField]
    private RangeSlider slider;
}
