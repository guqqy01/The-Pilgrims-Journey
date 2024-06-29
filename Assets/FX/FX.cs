using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FX : MonoBehaviour
{
    private Material fXMaterial;

    private void Awake()
    {
        fXMaterial = GetComponent<Renderer>().material;
    }

    public void Init(float greyScaleValue, Color color)
    {
        fXMaterial.SetFloat("_EffectAmount", greyScaleValue);
        fXMaterial.SetColor("_Tint", color);
    }

    public void DestoryFX()
    {
        Destroy(gameObject);
    }
}
