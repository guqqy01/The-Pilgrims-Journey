using Quests;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spawning
{
    /// <summary>
    /// All the knobs for how a level's spawning is generated. Assign one per biome
    /// (LocalMapGenerationSettings.SpawnDirectorSettings). Every ScalingValue is scaled by
    /// DifficultyLevelManager.DifficultyLevel when the level plan is built.
    /// </summary>
    [CreateAssetMenu(menuName = "Spawning/Spawn Director Settings")]
    public class SpawnDirectorSettings : ScriptableObject
    {
        [Header("Level")]
        [Tooltip("Seconds the spawn phase lasts. When it ends spawning stops, remaining units resolve, then the horde starts.")]
        public ScalingValue levelDuration = new ScalingValue(120f, 5f);

        [Tooltip("Spawn intensity over the level (x = 0..1 of level time). Multiplies unit and monster spawn rates.")]
        public AnimationCurve pacing = AnimationCurve.Linear(0f, 0.7f, 1f, 1.3f);

        [Range(0f, 1f)]
        [Tooltip("0 = evenly spaced, predictable spawns. 1 = very irregular timing and wave placement.")]
        public float chaos = 0.35f;

        [Header("Allied Units")]
        public List<UnitSpawnEntry> unitPool = new List<UnitSpawnEntry>();

        [Tooltip("How many allied units spawn over the level.")]
        public ScalingValue totalUnits = new ScalingValue(40f, 2f);

        [Tooltip("Fraction of totalUnits that must reach the end of the path.")]
        public ScalingValue requiredFraction = new ScalingValue(0.5f, 0.01f) { clamp = true, min = 0.2f, max = 0.85f };

        public ScalingValue groupSizeMin = new ScalingValue(2f);
        public ScalingValue groupSizeMax = new ScalingValue(5f, 0.25f);

        [Tooltip("Seconds between units inside one group.")]
        public float groupStagger = 0.4f;

        [Range(0.3f, 1f)]
        [Tooltip("Units stop spawning at this fraction of the level, leaving time for them to walk the path.")]
        public float unitSpawnWindow = 0.85f;

        [Header("Monsters")]
        [Tooltip("Multiplier on the spawn rate of normal monsters.")]
        public ScalingValue monsterDensity = new ScalingValue(1f, 0.1f);

        public ScalingValue maxMonstersAlive = new ScalingValue(60f, 5f);

        [Tooltip("How many of the biome's normal monster types are active this level (random subset). 0 = all.")]
        public ScalingValue activeNormalTypes = new ScalingValue(0f);

        [Header("Waves (picked from the biome's MonsterSpawnSettings wave list)")]
        public ScalingValue waveCount = new ScalingValue(1f, 0.3f);

        [Tooltip("Waves are placed between these fractions of the level.")]
        public Vector2 waveWindow = new Vector2(0.2f, 0.85f);

        public ScalingValue waveSizeMultiplier = new ScalingValue(1f, 0.15f);

        [Header("Quests")]
        public QuestSettings questSettings;
    }
}