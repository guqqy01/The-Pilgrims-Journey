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
            MonsterSpawn spawn,
            List<KeyValuePair<int, List<Vector2Int>>> weightedPoints)
        {
            if (spawn.monster == null)
                return false;

            foreach (var (_, points) in weightedPoints)
            {
                var shuffled = points.OrderBy(_ => seededRandom.Next()).ToList();

                foreach (var p in shuffled)
                {
                    if (SpotOccupied(p, spawn.monster.size, out Vector2 pos))
                        continue;

                    SpawnMonster(spawn.monster, pos);
                    MarkOccupied(p, spawn.monster.size);
                    return true;
                }
            }

            return false;
        }

        private void PreSpawnStationaryMonsters()
        {
            foreach (var (settings, biomeDirection) in _allActiveSpawnSettings)
            {
                foreach (var spawn in settings.rangeAndMonster
                             .Where(s => s is StationaryMonsterSpawn)
                             .Select(x => x as StationaryMonsterSpawn))
                {
                    int amount = Mathf.FloorToInt(spawn.maxAmountVSLevel.Evaluate(GameManager.Instance.Level));

                    // Get weighted points and filter to this biome's zone
                    var points = weightMap.WeightAndMapPointsConnectedToPath(spawn.range);

                    if (biomeDirection.HasValue)
                        points = FilterPointsToBiomeZone(points, biomeDirection.Value);
                    else
                        points = FilterPointsToBaseDominantZone(points);

                    if (points == null || points.Count == 0)
                        continue;

                    // Flatten to a single ordered list for index-based access
                    List<Vector2Int> flatPoints = new List<Vector2Int>();
                    foreach (var (_, positions) in points)
                        flatPoints.AddRange(positions);

                    int placed = 0;
                    int flatIndex = 0;

                    while (placed < amount && flatIndex < flatPoints.Count)
                    {
                        Vector2Int p = flatPoints[flatIndex];
                        flatIndex++;

                        if (SpotOccupied(p, spawn.monster.size, out Vector2 _))
                            continue;

                        SpawnMonster(spawn.monster, p);
                        MarkOccupied(p, spawn.monster.size);
                        placed++;
                    }
                }
            }
        }
    }

}
