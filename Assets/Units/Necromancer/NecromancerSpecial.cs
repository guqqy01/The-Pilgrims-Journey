using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Units
{
    public class NecromancerSpecial : MonoBehaviour
    {
        private Movement movement;

        [SerializeField]
        private List<UnitSO> unitSpawns;

        [SerializeField]
        private float spawnCooldown = 4f;

        private float TimeToNextSpawn;

        private void Awake()
        {
            movement = GetComponent<Movement>();
        }


        private void Update()
        {
            if (Time.time < TimeToNextSpawn)
                return;

            UnitSO randomUnit = unitSpawns.GetRandom();

            Unit unit = Instantiate(randomUnit.unitPrefab, transform.position, Quaternion.identity);
            unit.Init(randomUnit, movement.Path, movement.CurrentPathIndex);

            TimeToNextSpawn = Time.time + spawnCooldown;
        }
    }
}

