using AYellowpaper.SerializedCollections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using GameManagerCalls;
using System;
using Random = UnityEngine.Random;
namespace LevelingSystem
{
    public class ExperienceOrbManager : Singleton<ExperienceOrbManager>, IOnCompletedLevel, ISaveLoad, IOnSave, IOnLoad, IOnClear
    {
        #region Inspector Fields

        [Header("Orb Prefabs (Value -> Prefab)")]
        [SerializeField] private SerializedDictionary<int, ExperienceOrb> orbPrefabs;

        [Header("Merge Logic")]
        [SerializeField] private float mergeRadius = 0.6f;
        [SerializeField] private float mergeCheckInterval = 0.25f;
        [SerializeField] private int maxMergesPerTick = 2;

        #endregion

        #region Private Fields

        private readonly Dictionary<int, ExperienceOrbPool> pools = new();
        private readonly HashSet<ExperienceOrb> spawnedOrbs = new();

        // Tier info
        private List<int> sortedValues;
        private Dictionary<int, int> nextTierValue;
        private Dictionary<int, int> mergeCounts;

        // Spatial hashing
        private readonly Dictionary<Vector2Int, List<ExperienceOrb>> spatialGrid = new();
        private static readonly Vector2Int[] NeighborOffsets =
        {
            new(0, 0),
            new(1, 0), new(-1, 0),
            new(0, 1), new(0, -1),
            new(1, 1), new(1, -1),
            new(-1, 1), new(-1, -1)
        };

        // Reusable buffers
        private readonly Dictionary<int, List<ExperienceOrb>> mergeCandidates = new();
        private readonly List<ExperienceOrb> clusterBuffer = new();

        private float nextMergeCheck;

        public string SaveFileName => "ExperienceOrbManager";

        #endregion

        #region Unity Methods

        private void Awake()
        {
            BuildPools();
            BuildTierData();
        }

        private void Update()
        {
            if (Time.time < nextMergeCheck) return;

            nextMergeCheck = Time.time + mergeCheckInterval;
            TryMergeOrbs();
        }

        #endregion

        #region Initialization

        private void BuildPools()
        {
            foreach (var kvp in orbPrefabs)
                pools[kvp.Key] = new ExperienceOrbPool(kvp.Value, kvp.Key, transform);
        }

        private void BuildTierData()
        {
            sortedValues = orbPrefabs.Keys.ToList();
            sortedValues.Sort();

            nextTierValue = new Dictionary<int, int>();
            mergeCounts = new Dictionary<int, int>();

            for (int i = 0; i < sortedValues.Count - 1; i++)
            {
                int current = sortedValues[i];
                int next = sortedValues[i + 1];

                nextTierValue[current] = next;
                mergeCounts[current] = next / current;
            }
        }

        #endregion

        #region Spawning

        public void SpawnOrbs(int totalXP, Vector3 position, bool doBounce = true)
        {
            for (int i = sortedValues.Count - 1; i >= 0; i--)
            {
                int value = sortedValues[i];
                int count = totalXP / value;
                if (count <= 0) continue;

                totalXP -= count * value;
                var pool = pools[value];

                for (int j = 0; j < count; j++)
                {
                    Vector3 offset = Random.insideUnitCircle * 0.2f;
                    var orb = pool.Get(position + offset);
                    orb.Initialize(value, pool, doBounce);
                    orb.OnReleased += HandleOrbReleased;
                    spawnedOrbs.Add(orb);
                }
            }
        }

        private void HandleOrbReleased(ExperienceOrb orb)
        {
            spawnedOrbs.Remove(orb);
        }

        #endregion

        #region Merging

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsMergeable(ExperienceOrb orb) => orb != null && orb.CanMerge;

        private Vector2Int GetCell(Vector3 pos) =>
            new(Mathf.FloorToInt(pos.x / mergeRadius), Mathf.FloorToInt(pos.y / mergeRadius));

        private void RebuildSpatialGrid()
        {
            spatialGrid.Clear();

            foreach (var orb in spawnedOrbs)
            {
                if (!IsMergeable(orb)) continue;

                Vector2Int cell = GetCell(orb.transform.position);
                if (!spatialGrid.TryGetValue(cell, out var list))
                    spatialGrid[cell] = list = new List<ExperienceOrb>();

                list.Add(orb);
            }
        }

        private void TryMergeOrbs()
        {
            RebuildSpatialGrid();
            BuildMergeCandidates();

            int mergesThisTick = 0;

            foreach (var kvp in mergeCandidates)
            {
                if (mergesThisTick >= maxMergesPerTick) break;

                int value = kvp.Key;
                if (!mergeCounts.TryGetValue(value, out int required)) continue;
                if (kvp.Value.Count < required) continue;

                if (TryMergeCluster(value, required))
                    mergesThisTick++;
            }
        }

        private void BuildMergeCandidates()
        {
            mergeCandidates.Clear();

            foreach (var orb in spawnedOrbs)
            {
                if (!IsMergeable(orb)) continue;

                int value = orb.ExperienceAmount;
                if (!mergeCandidates.TryGetValue(value, out var list))
                    mergeCandidates[value] = list = new List<ExperienceOrb>();

                list.Add(orb);
            }
        }

        private bool TryMergeCluster(int value, int requiredCount)
        {
            float radiusSqr = mergeRadius * mergeRadius;

            foreach (var cellEntry in spatialGrid)
            {
                var cell = cellEntry.Key;
                var cellOrbs = cellEntry.Value;

                foreach (var center in cellOrbs)
                {
                    if (!IsMergeable(center) || center.ExperienceAmount != value) continue;

                    clusterBuffer.Clear();
                    clusterBuffer.Add(center);
                    Vector3 centerPos = center.transform.position;

                    foreach (var offset in NeighborOffsets)
                    {
                        if (!spatialGrid.TryGetValue(cell + offset, out var neighbors)) continue;

                        foreach (var other in neighbors)
                        {
                            if (other == center || !IsMergeable(other) || other.ExperienceAmount != value) continue;

                            if ((centerPos - other.transform.position).sqrMagnitude <= radiusSqr)
                                clusterBuffer.Add(other);

                            if (clusterBuffer.Count >= requiredCount)
                            {
                                MergeOrbs(clusterBuffer, value);
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private void MergeOrbs(List<ExperienceOrb> orbs, int value)
        {
            int newValue = nextTierValue[value];
            Vector3 pos = Vector3.zero;

            foreach (var orb in orbs)
            {
                pos += orb.transform.position;
                orb.Release();
            }

            pos /= orbs.Count;

            var newOrb = pools[newValue].Get(pos);
            spawnedOrbs.Add(newOrb);
        }

        #endregion

        #region Cleanup

        public void OnLevelCompleted()
        {
            foreach (var orb in spawnedOrbs.ToArray())
                orb?.Release();

            spawnedOrbs.Clear();
        }

        public void Save()
        {
            SaveManager.IfFileExistsDelete(SaveFileName);
            SaveData saveData = new SaveData(spawnedOrbs.Select(orb => new OrbSaveInfo(orb.ExperienceAmount, orb.transform.position)).ToList());
            SaveProfile<SaveData> saveProfile = new SaveProfile<SaveData>(SaveFileName, saveData);
            SaveManager.SaveOver(saveProfile);
        }

        public void Load()
        {
            if (SaveManager.TryLoad(SaveFileName, out SaveProfile<SaveData> outSaveProfile))
            {
                SaveData saveData = outSaveProfile.saveData;

                List<OrbSaveInfo> orbSaveInfo = saveData.orbSaveInfos;

                foreach (var info in orbSaveInfo)
                    SpawnOrbs(info.ExperienceAmount, info.Position, false);
            }
            else
            {
                Debug.LogWarning($"No save data found for {SaveFileName}");
                return;
            }
        }

        public void Clear()
        {
            foreach (var orb in spawnedOrbs.ToArray())
                orb.Release();
        }

        [Serializable]
        public class SaveData : SaveProfileData
        {
            public List<OrbSaveInfo> orbSaveInfos;

            public SaveData(List<OrbSaveInfo> orbSaveInfos)
            {
                this.orbSaveInfos = orbSaveInfos;
            }
        }

        #endregion
    }

    [Serializable]
    public class OrbSaveInfo
    {
        public int ExperienceAmount;
        public Vector3 Position;

        public OrbSaveInfo(int experienceAmount, Vector3 position)
        {
            ExperienceAmount = experienceAmount;
            Position = position;
        }
    }
}
