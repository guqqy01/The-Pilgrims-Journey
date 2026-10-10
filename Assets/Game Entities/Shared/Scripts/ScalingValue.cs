using System;
using UnityEngine;

namespace Spawning
{
    /// <summary>
    /// A designer-friendly value that scales with level difficulty:
    /// value = baseValue + perDifficulty * (difficulty - 1), optionally clamped.
    /// </summary>
    [Serializable]
    public struct ScalingValue
    {
        public float baseValue;

        [Tooltip("Added for every +1 of difficulty above 1.")]
        public float perDifficulty;

        public bool clamp;
        public float min;
        public float max;

        public ScalingValue(float baseValue, float perDifficulty = 0f)
        {
            this.baseValue = baseValue;
            this.perDifficulty = perDifficulty;
            clamp = false;
            min = 0f;
            max = 0f;
        }

        public float Evaluate(float difficulty)
        {
            float v = baseValue + perDifficulty * (difficulty - 1f);
            return clamp ? Mathf.Clamp(v, min, max) : v;
        }

        public int EvaluateInt(float difficulty) => Mathf.RoundToInt(Evaluate(difficulty));
    }
}