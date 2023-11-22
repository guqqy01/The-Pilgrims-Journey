using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Map;

public class GameManager : MonoBehaviour
{
    LocalMapGenerator mapGenerator;

    public Movement peasantPrefab;
    public Movement tankPrefab;

    [SerializeField]
    private float peasantSpawnRate = 10f;
    private float nextPeasantSpawnTime = 0f;

    [SerializeField]
    private float tankSpawnRate = 10f;
    private float nextTankSpawnTime = 0f;

    void Start()
    {
        mapGenerator = LocalMapGenerator.Instance;
        mapGenerator.CreateMap();
        PathPicker.instance.Setup();
    }

    void Update()
    {
        

        
    }

}
