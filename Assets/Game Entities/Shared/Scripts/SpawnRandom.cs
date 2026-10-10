using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spawning
{
    public static class SpawnRandom
    {
        public static T PickWeighted<T>(IList<T> items, Func<T, float> weightOf, System.Random rng) where T : class
        {
            if (items == null || items.Count == 0)
                return null;

            float total = 0f;
            for (int i = 0; i < items.Count; i++)
                total += Mathf.Max(0f, weightOf(items[i]));

            if (total <= 0f)
                return items[rng.Next(items.Count)];

            float roll = (float)rng.NextDouble() * total;
            for (int i = 0; i < items.Count; i++)
            {
                roll -= Mathf.Max(0f, weightOf(items[i]));
                if (roll <= 0f)
                    return items[i];
            }

            return items[items.Count - 1];
        }

        public static void Shuffle<T>(IList<T> list, System.Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        /// <summary>Returns a multiplier around 1. amount 0 = always 1, amount 1 = 0.1 to 1.9.</summary>
        public static float Jitter(System.Random rng, float amount)
            => 1f + ((float)rng.NextDouble() * 2f - 1f) * Mathf.Clamp01(amount) * 0.9f;
    }
}