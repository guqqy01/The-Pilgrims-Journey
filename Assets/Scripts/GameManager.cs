using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Map;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public LocalMapGenerationSettings LocalMapSettings { get; private set; }

    public int Level = 1;

    private int unitsPassed = 0;

    [SerializeField]
    private AnimationCurve requiredUnits;

    [SerializeField]
    private Text levelText;
    [SerializeField]
    private Text requiredUnitsText;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        LocalMapGenerator.Instance.CreateMap();
        PrizeWindow.instance.gameObject.SetActive(true);
        PrizeWindow.instance.LoadPrize();

        levelText.text = "Level " + Level;
        requiredUnitsText.text = unitsPassed + "/" + Mathf.CeilToInt(requiredUnits.Evaluate(Level));
    }

    public void SetLocalMapObject(LocalMapGenerationSettings settings)
    {
        LocalMapSettings = settings;
    }

    public void SetupLocalMap()
    {
        LocalMapGenerator.Instance.generationSettings = LocalMapSettings;
        LocalMapGenerator.Instance.CreateMap();
    }

    public void UnitPassed()
    {
        unitsPassed++;
        if (unitsPassed >= requiredUnits.Evaluate(Level))
        {
            //CompletedLevel
            TilePlayer.Instance.StopAndReset();

            PrizeWindow.instance.gameObject.SetActive(true);
            PrizeWindow.instance.LoadPrize();

            Level++;

            unitsPassed = 0;

            LocalMapGenerator.Instance.CreateMap();

            levelText.text = "Level " + Level;
        }

        requiredUnitsText.text = unitsPassed + "/" + Mathf.CeilToInt(requiredUnits.Evaluate(Level));
    }

}
