using UnityEngine;

namespace Monsters
{
    /// <summary>
    /// Monster Death
    /// </summary>
    public partial class MonsterSpawnManager
    {
        private void OnMonsterDie(Health health)
        {
            if (!health.TryGetComponent(out Monster monster))
                return;

            HandleStationaryDeath(monster);

            if (aliveCountByMonster.ContainsKey(monster.MonsterSO))
            {
                aliveCountByMonster[monster.MonsterSO]--;
            }
            else
            {
                Debug.LogWarning($"Monster: {monster.MonsterSO.name}, is not in the aliveCountByMonster Dictionary");
            }
            
            Monsters.Remove(monster);

            health.onDie -= OnMonsterDie;
            HandleRespawnDelay(monster.MonsterSO);
        }



        private void HandleStationaryDeath(Monster m)
        {
            if (!m.TryGetComponent(out MonsterMovement movement))
                return;

            Vector2Int pos = movement.StationaryPosition;
            if (pos == default)
                return;

            for (int x = 0; x < m.MonsterSO.size.x; x++)
            {
                for (int y = 0; y < m.MonsterSO.size.y; y++)
                    occupiedSpots.Remove(pos + new Vector2Int(x, y));
            }
        }

        private void HandleRespawnDelay(MonsterSO monster)
        {
            var spawn = monsterSpawnSettings.rangeAndMonster.Find(s => s.monster == monster);
            
            if(spawn.monster == null || spawn.maxAmountVSLevel == null)
            {
                return;
            }

            int previous = aliveCountByMonster[monster];
            int maxAllowed = Mathf.FloorToInt(spawn.maxAmountVSLevel.Evaluate(GameManager.Instance.Level));

            if (previous >= maxAllowed)
            {
                float now = Time.time;
                nextSpawnTimeByMonster[monster] = now + GetNextSpawnDelay(spawn);
            }
        }

    }
}
