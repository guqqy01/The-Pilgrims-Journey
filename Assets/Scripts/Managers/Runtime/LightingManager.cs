using GameManagerCalls;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static UnityEngine.ParticleSystem;
using UnityEngine.U2D;
public class LightingManager : MonoBehaviour, IOnAfterCreateLocalMap, IOnClear
{
    [SerializeField]
    private Light2D light2D;
    [SerializeField]
    private AnimationCurve curve;

    [SerializeField]
    private Firefly FireflyPrefab;

    List<Firefly> fireflys = new List<Firefly>();

    [SerializeField]
    private MinMaxCurve numberOfFireflys;

    public void Clear()
    {
        foreach(var firefly in fireflys)
        {
            Destroy(firefly.gameObject);
        }

        fireflys.Clear();
    }

    

    private void LightIntensity()
    {
        if(light2D != null)
            light2D.intensity = curve.Evaluate(GameManager.Instance.Level);
    }

    private void AddOrRemoveFireFlys()
    {
        int numberOfFlys = Mathf.RoundToInt(numberOfFireflys.Evaluate(GameManager.Instance.Level, Random.value));

        int difference = numberOfFlys - fireflys.Count;

        if(difference > 0)
        {
            for (int i = 0; i < difference; i++)
            {
                float rotation = Random.Range(0f, 2 * Mathf.PI);
                Vector2 viewportPosition = new Vector2(Mathf.Cos(rotation), Mathf.Sin(rotation)) * Random.Range(1f, 1.5f) + Vector2.one / 2f;
                Vector2 position = Camera.main.ViewportToWorldPoint(viewportPosition);


                Firefly fly = Instantiate(FireflyPrefab, position, Quaternion.identity);
                fireflys.Add(fly);
            }
        }
        else if (difference < 0)
        {
            int amountToRemove = -difference;

            for (int i = 0; i < amountToRemove; i++)
            {
                fireflys[i].LeaveMap();
            }

            fireflys.RemoveRange(0, amountToRemove);
        }
    }

    public void AfterCreateLocalMap()
    {
        LightIntensity();
        AddOrRemoveFireFlys();
    }
}
