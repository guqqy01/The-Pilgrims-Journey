using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
namespace LocalMap
{
    [Serializable]
    public class TerrainNoise
    {
        public string name;

        public int priority;
        public Vector2 scale = 0.1f * Vector2.one;

        public List<TerrainNoiseItem> items;

        private Vector2 seed;

        public void SetupSeed()
        {
            seed = new Vector2(Random.Range(0, 100f), Random.Range(0, 100f));
        }

        public Sprite GetTile(Vector2Int position)
        {
            float x = (seed.x + (float)position.x) * scale.x;
            float y = (seed.y + (float)position.y) * scale.y;
            
            float noise = Mathf.PerlinNoise(x, y);

            List<TerrainNoiseItem> itemsInRange = new List<TerrainNoiseItem>();

            foreach(TerrainNoiseItem item in items)
            {
                if(item.perlinNoiseRange.InBetweenValues(noise))
                    itemsInRange.Add(item);
            }

            WeightedChanceList<Sprite> itemsInListWeightList = new WeightedChanceList<Sprite>();
            foreach (TerrainNoiseItem terrainNoiseListItem in itemsInRange)
            {
                itemsInListWeightList.weightedList.AddRange(terrainNoiseListItem.list.weightedList);
            }

            return itemsInListWeightList.GetRandom();
        }
    }

    [Serializable]
    public class TerrainNoiseItem
    {
        public Vector2 perlinNoiseRange;
        public WeightedChanceList<Sprite> list;
    }
}


