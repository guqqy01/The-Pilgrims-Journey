using LocalMap;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;

namespace Monsters
{
    /// <summary>
    /// Hoard Management
    /// </summary>
    public partial class MonsterSpawnManager
    {
        private bool hordeActive;
        [Header("Horde")]
        [SerializeField]
        private float hordeLength = 30f;

        private float hordeEndTime;
        private int hordeMultiplier = 1;

        
        [SerializeField] private float hordeSpawnRate = 0.25f;
        private float nextHordeSpawnTime;

        [SerializeField]
        private MonsterSO deathMonster;

        private void ProcessHordeTimer(float now)
        {
            if (!hordeActive)
                return;

            if (now >= hordeEndTime)
            {
                SpawnMonster(deathMonster, GetAllSideTiles(PathPicker.Instance.PickedDirection.Value.OppositeDirection()).GetRandom());

                hordeActive = false;
                hordeMultiplier = 1;
                return;
            }

            if (now >= nextHordeSpawnTime)
            {
                SpawnHordeBatch();
                nextHordeSpawnTime = now + hordeSpawnRate;
            }
        }

        public void StartHorde(int multiplier)
        {
            hordeActive = true;
            hordeMultiplier = Mathf.Max(1, multiplier);
            hordeEndTime = Time.time + hordeLength;
        }

        private void SpawnHordeBatch()
        {
            // Use PathPicker as the authoritative spawn direction
            Direction side = PathPicker.Instance.PickedDirection.Value.OppositeDirection();

            foreach (var spawn in monsterSpawnSettings.rangeAndMonster)
            {
                MonsterSO monster = spawn.monster;

                if(monster.movementType == MonsterMovementType.Stationary)
                    continue;

                TrySpawnHordeMonster(monster, side);
            }
        }

        private List<Vector2Int> GetAllSideTiles(Direction side)
        {
            List<Vector2Int> result = new();

            int w = Map.MapSizeX;
            int h = Map.MapSizeY;

            switch (side)
            {
                case Direction.North:
                    for (int x = 0; x < w; x++)
                        result.Add(new Vector2Int(x, h - 1));
                    break;

                case Direction.South:
                    for (int x = 0; x < w; x++)
                        result.Add(new Vector2Int(x, 0));
                    break;

                case Direction.East:
                    for (int y = 0; y < h; y++)
                        result.Add(new Vector2Int(w - 1, y));
                    break;

                case Direction.West:
                    for (int y = 0; y < h; y++)
                        result.Add(new Vector2Int(0, y));
                    break;
            }

            result.RemoveAll(x => !LocalMapGenerator.Instance.IsConnectedToPath(x));

            return result;
        }

        private void TrySpawnHordeMonster(MonsterSO monster, Direction side)
        {
            List<Vector2Int> positions = GetAllSideTiles(side);

            // Shuffle for randomness
            positions = positions.OrderBy(_ => seededRandom.Next()).ToList();

            foreach (var p in positions)
            {
                if (SpotOccupied(p, monster.size, out Vector2 pos))
                    continue;

                if (!LocalMapGenerator.Instance.IsConnectedToPath(p))
                    continue;

                SpawnMonster(monster, pos);

                return;
            }
        }
    }
}