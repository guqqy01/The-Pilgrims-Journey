using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Monsters
{

    /// <summary>
    /// Stationary Spawns
    /// </summary>
    public partial class MonsterSpawnManager
    {
        private bool TrySpawnStationary(
            IStationaryMonsterSpawn spawn,
            List<KeyValuePair<int, List<Vector2Int>>> weightedPoints)
        {
            if (spawn.MonsterSO == null)
                return false;

            foreach (var (_, points) in weightedPoints)
            {
                var shuffled = points.OrderBy(_ => seededRandom.Next()).ToList();

                foreach (var p in shuffled)
                {
                    if (SpotOccupied(p, spawn.MonsterSO.size, out Vector2 pos))
                        continue;

                    SpawnMonster(spawn.MonsterSO, pos);
                    MarkOccupied(p, spawn.MonsterSO.size);
                    return true;
                }
            }

            return false;
        }
    }

}
