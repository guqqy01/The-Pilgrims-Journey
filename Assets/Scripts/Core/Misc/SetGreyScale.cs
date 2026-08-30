
using UnityEngine;

public class SetGreyScale : MonoBehaviour
{
    [SerializeField]
    private float baseValue;
    [SerializeField]
    private Color baseColor;
    [SerializeField]
    private float flickerValue;
    [SerializeField]
    private Color flickerColor = Color.red;

    private Material material;

    public bool Flickering {  get; private set; }
    private const float percentageOfRemainingTimeToStartFlickering = 0.3f;
    private const float percentageOfRemainingTimeToChangeFlickerState = 0.2f;
    private float timeToStartFlickering;
    private float endFlickerTime;
    private float timeToChangeFlickerColor;
    private bool flickerOnOrOff;

    private void Flicker()
    {
        if (Time.time >= timeToChangeFlickerColor)
        {
            material.SetFloat("_EffectAmount", flickerOnOrOff ? flickerValue : baseValue);
            material.SetColor("_Tint", flickerOnOrOff ? flickerColor : baseColor);
            timeToChangeFlickerColor = Time.time + (endFlickerTime - Time.time) * percentageOfRemainingTimeToChangeFlickerState;
            flickerOnOrOff = !flickerOnOrOff;
        }
    }

    private void Update()
    {
        if (Time.time >= timeToStartFlickering && !Flickering)
            StartFlicker();

        if (!Flickering)
            return;

        if (Time.time >= endFlickerTime)
            EndFlicker();
        else
            Flicker();
    }

    public void SetEndFlickerTime(float endFlickerTime)
    {
        timeToStartFlickering = Time.time + (endFlickerTime - Time.time) * percentageOfRemainingTimeToStartFlickering;
        this.endFlickerTime = endFlickerTime;
    }

    private void StartFlicker()
    {
        Flickering = true;

        material.SetFloat("_EffectAmount", flickerValue);
        material.SetColor("_Tint", flickerColor);

        timeToChangeFlickerColor = Time.time + (endFlickerTime - Time.time) * percentageOfRemainingTimeToChangeFlickerState;

        flickerOnOrOff = true;
    }

    public void StartFlicker(float flickerTime)
    {
        Flickering = true;

        endFlickerTime = Time.time + flickerTime;

        material.SetFloat("_EffectAmount", flickerValue);
        material.SetColor("_Tint", flickerColor);

        timeToChangeFlickerColor = Time.time + (endFlickerTime - Time.time) * percentageOfRemainingTimeToChangeFlickerState;

        flickerOnOrOff = true;
    }

    public void EndFlicker()
    {
        Flickering = false;

        material.SetFloat("_EffectAmount", baseValue);
        material.SetColor("_Tint", baseColor);
    }

    private void Awake()
    {
        material = GetComponent<Renderer>().material;

        material.SetFloat("_EffectAmount", baseValue);
        material.SetColor("_Tint", baseColor);
    }

    public void Set(float value, Color color)
    {
        material.SetFloat("_EffectAmount", value);
        material.SetColor("_Tint", color);
    }
}
