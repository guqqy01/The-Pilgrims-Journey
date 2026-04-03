using UnityEngine;
using System;
using System.Collections.Generic;
using LocalMap.Editor;
using NUnit.Framework;

namespace LocalMap
{
    [CreateAssetMenu()]
    public class LocalMapGenerationSettings : ScriptableObject
    {
        public AutoTileSettings autoBaseTileSettings;

        public WeightedChanceList<TileTypeAndSprite> baseTilesAndType;
        public WeightedChanceList<TileTypeAndSprite> pathTilesAndType;

        public List<OverlayMapObject> mapObjects;

        public List<TerrainNoise> terrainNoiseLists;

        public Vector2 pathWidthRange = new Vector2(1f, 3f);

        public Vector2 pathNoiseScale = new Vector2(50f, 50f);

        public Vector2 pathNoiseTiling;

        public float cullingAmount = 0.2f;

        public void SetupSeeds()
        {
            foreach(TerrainNoise noise in terrainNoiseLists)
            {
                noise.SetupSeed();
            }
        }

        public TileTypeAndSprite GetMapTileSprite(Vector2Int position, float? level, MapGenerationPreviewSettings previewSettings, List<int> terrainNoiseIndecies)
        {
            if (previewSettings.HasFlag(MapGenerationPreviewSettings.TerrainNoise) && terrainNoiseIndecies != null && terrainNoiseIndecies.Count > 0)
            {
                List<TerrainNoise> includedNoise = new List<TerrainNoise>();

                foreach (int i in terrainNoiseIndecies)
                {
                    includedNoise.Add(terrainNoiseLists[i]);
                }

                includedNoise.Sort(new Comparison<TerrainNoise>((x, y) => x.priority.CompareTo(y.priority)));

                for (int i = 0; i < includedNoise.Count; i++)
                {
                    TileTypeAndSprite tileTypeAndSprite = includedNoise[i].GetTile(position);

                    if (tileTypeAndSprite != null)
                        return tileTypeAndSprite;
                }
            }

            if(previewSettings.HasFlag(MapGenerationPreviewSettings.BaseTiles))
                return level.HasValue ? baseTilesAndType.GetRandom(level.Value): baseTilesAndType.GetRandom();

            return null;
        }

        public TileTypeAndSprite GetPathTileSprite(float? level)
        {
            return level.HasValue ? pathTilesAndType.GetRandom(level.Value) : pathTilesAndType.GetRandom();
        }
    }

    [Serializable]
    public class OverlayMapObject
    {
        public string name;

        public GameObject overlay;

        public Vector2Int size;

        public AnimationCurve amountToSpawnCurve;
    }

   
}


