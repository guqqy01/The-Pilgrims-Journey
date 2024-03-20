using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Map;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public LocalMapGenerationSettings LocalMapSettings { get; private set; }

    public int Level = 1;

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

    public void SetLocalMapObject(LocalMapGenerationSettings settings)
    {
        LocalMapSettings = settings;
    }

    public void SetupLocalMap()
    {
        LocalMapGenerator.Instance.generationSettings = LocalMapSettings;
        LocalMapGenerator.Instance.CreateMap();
    }


    void Update()
    {
        

        
    }

}
