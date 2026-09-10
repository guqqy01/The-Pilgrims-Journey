using LocalMap;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;
using System;

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

        [SerializeField] private float hordeSpawnRate = 0.25f;
        private float nextHordeSpawnTime;

        [SerializeField]
        private MonsterSO deathMonster;

        public event Action<float> OnHordeStart;
        public event Action OnHordeFinished;

        protected override void CleanUp()
        {
            base.CleanUp();
            OnHordeFinished = null;
            OnHordeFinished = null;
        }

        private void ProcessHordeTimer(float now, float difficulty)
        {
            if (!hordeActive)
                return;

            if (now >= hordeEndTime)
            {
                if(deathMonster != null)
                    SpawnMonster(deathMonster, GetAllSideTiles(PathPicker.Instance.PickedDirection.Value.OppositeDirection()).GetRandom(), difficulty);

                hordeActive = false;
                OnHordeFinished?.Invoke();
                return;
            }

            if (now >= nextHordeSpawnTime)
            {
                SpawnHordeBatch(difficulty);
                nextHordeSpawnTime = now + hordeSpawnRate;
            }
        }

        public void StartHorde()
        {
            hordeActive = true;
            hordeEndTime = Time.time + hordeLength;
            OnHordeStart?.Invoke(hordeLength);
            LocalMapThemeManager.Instance.SetState(LocalMapMusicState.Escape);
        }

        private void SpawnHordeBatch(float difficulty)
        {
            Direction side = PathPicker.Instance.PickedDirection.Value.OppositeDirection();

            foreach (var (settings, blendDirection) in _allActiveSpawnSettings)
            {
                foreach (var spawn in settings.MonsterSpawns)
                {
                    MonsterSO monster = spawn.monster;

                    if (monster.movementType == MonsterMovementType.Stationary)
                        continue;

                    TrySpawnHordeMonster(monster, side, blendDirection, difficulty);
                }
            }
        }
        
        private List<Vector2Int> GetAllSideTiles(Direction side, int maxSearchDepth = -1)
        {
            int w = Map.MapSizeX;
            int h = Map.MapSizeY;
            LocalMapGenerator map = LocalMapGenerator.Instance;

            if (maxSearchDepth < 0)
                maxSearchDepth = side is Direction.North or Direction.South ? h : w;

            for (int depth = 0; depth < maxSearchDepth; depth++)
            {
                List<Vector2Int> result = new();

                switch (side)
                {
                    case Direction.North:
                        for (int x = 0; x < w; x++)
                            result.Add(new Vector2Int(x, h - 1 - depth));
                        break;
                    case Direction.South:
                        for (int x = 0; x < w; x++)
                            result.Add(new Vector2Int(x, depth));
                        break;
                    case Direction.East:
                        for (int y = 0; y < h; y++)
                            result.Add(new Vector2Int(w - 1 - depth, y));
                        break;
                    case Direction.West:
                        for (int y = 0; y < h; y++)
                            result.Add(new Vector2Int(depth, y));
                        break;
                }

                result.RemoveAll(p => !map.IsConnectedToPath(p));

                if (result.Count > 0)
                    return result;
            }

            return new List<Vector2Int>();
        }

        private void TrySpawnHordeMonster(MonsterSO monster, Direction side, Direction? biomeDirection, float difficulty)
        {
            List<Vector2Int> positions = GetAllSideTiles(side);

            // Filter to this monster's biome zone
            if (biomeDirection.HasValue)
                positions = positions.Where(p =>
                    LocalMapGenerator.Instance.GetDominantBlendDirection(p) == biomeDirection).ToList();
            else
                positions = positions.Where(p =>
                    LocalMapGenerator.Instance.GetDominantBlendDirection(p) == null).ToList();

            positions = positions.OrderBy(_ => seededRandom.Next()).ToList();

            foreach (var p in positions)
            {
                if (SpotOccupied(p, monster.size, out Vector2 pos))
                    continue;

                SpawnMonster(monster, pos, difficulty);
                return;
            }
        }
    }
}