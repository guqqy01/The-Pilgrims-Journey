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
    private Direction pathDirection;

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
        PathPicker.instance.SetPositions(mapGenerator.pathway.PathNodes[pathDirection]);
    }

    void Update()
    {
        if (Time.time >= nextPeasantSpawnTime)
        {
            Movement mov = Instantiate(peasantPrefab, (Vector2)mapGenerator.pathway.PathNodes[pathDirection][0], Quaternion.identity);
            mov.Init(mapGenerator.pathway.PathNodes[pathDirection]);
            nextPeasantSpawnTime = Time.time + peasantSpawnRate;
        }

        if (Time.time >= nextTankSpawnTime)
        {
            Movement mov = Instantiate(tankPrefab, (Vector2)mapGenerator.pathway.PathNodes[pathDirection][0], Quaternion.identity);
            mov.Init(mapGenerator.pathway.PathNodes[pathDirection]);
            nextTankSpawnTime = Time.time + tankSpawnRate;
        }
    }

}
