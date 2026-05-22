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
            IDirectMonsterSpawn spawn, 
            List<Vector2Int> points,
            Vector2Int heroPosOnGrid)
        {
            var list = new WeightedChanceList<Vector2>();

            foreach (var p in points)
            {
                if (!IsWithinSpawnDistance(spawn, p, heroPosOnGrid))
                    continue;

                if (SpotOccupied(p, spawn.MonsterSO.size, out Vector2 pos))
                    continue;

                list.Add(pos, 1);
            }
            

            if (list.weightedList.Count == 0)
                return false;

            SpawnMonster(spawn.MonsterSO, list.GetRandom());
            return true;
        }
    }

}