using AYellowpaper.SerializedCollections;
using UnityEngine;
using System.Collections.Generic;
using GameManagerCalls;
namespace LevelingSystem
{
    public class ExperienceOrbManager : Singleton<ExperienceOrbManager>, IOnCompletedLevel
    {
        [SerializeField]
        private SerializedDictionary<int, ExperienceOrb> orbPrefabs;

        private readonly Dictionary<int, ExperienceOrbPool> pools = new();
        private List<int> sortedValues;

        List<ExperienceOrb> spawnedExperienceOrbs = new List<ExperienceOrb>();

        private void Awake()
        {
            sortedValues = new List<int>(orbPrefabs.Keys);
            sortedValues.Sort((a, b) => b.CompareTo(a));

            foreach (var kvp in orbPrefabs)
            {
                pools[kvp.Key] = new ExperienceOrbPool(
                    kvp.Value,
                    kvp.Key,
                    transform);
            }
        }

        public void SpawnOrbs(int totalExperience, Vector3 position)
        {
            foreach (int value in sortedValues)
            {
                int count = totalExperience / value;
                if (count <= 0)
                    continue;

                totalExperience -= count * value;

                ExperienceOrbPool pool = pools[value];

                for (int i = 0; i < count; i++)
                {
                    Vector3 offset = Random.insideUnitCircle * 0.2f;

                    ExperienceOrb orb = pool.Get(position + offset);
                    spawnedExperienceOrbs.Add(orb);
                }
            }
        }

        public void OnLevelCompleted()
        {
            foreach(ExperienceOrb orb in spawnedExperienceOrbs)
            {
                if(orb == null)
                    continue;

                orb.Release();
            }

            spawnedExperienceOrbs.Clear();
        }
    }

}