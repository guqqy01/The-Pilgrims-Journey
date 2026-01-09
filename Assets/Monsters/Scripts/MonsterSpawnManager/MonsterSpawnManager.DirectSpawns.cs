using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace Monsters
{
    /// <summary>
    /// Direct Spawns
    /// </summary>
    public partial class MonsterSpawnManager
    {
        private bool TrySpawnDirect(
            MonsterSpawn spawn, 
            List<KeyValuePair<int, List<Vector2Int>>> weightedPoints,
            Vector2Int heroPosOnGrid)
        {
            var list = new WeightedChanceList<Vector2>();
            int maxWeight = weightedPoints.Max(w => w.Key);

            foreach (var (weight, points) in weightedPoints)
            {
                foreach (var p in points)
                {
                    if (!IsWithinSpawnDistance(spawn, p, heroPosOnGrid))
                        continue;

                    if (SpotOccupied(p, spawn.monster.size, out Vector2 pos))
                        continue;

                    int invertedWeight = Mathf.Max(maxWeight - weight, 0);
                    list.Add(pos, invertedWeight);
                }
            }

            if (list.weightedList.Count == 0)
                return false;

            SpawnMonster(spawn.monster, list.GetRandom());
            return true;
        }
    }

}