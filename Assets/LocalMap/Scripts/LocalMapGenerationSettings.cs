using UnityEngine;
using System;
using System.Collections.Generic;

namespace LocalMap
{
    [CreateAssetMenu()]
    public class LocalMapGenerationSettings : ScriptableObject
    {
        public WeightedChanceList<Sprite> baseTiles;

        public AutoTile autoBaseTiles;

        public WeightedChanceList<Sprite> pathTiles;

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

        public Sprite GetMapTileSpriteForPreview(Vector2Int position, float? level, MapGenerationPreviewSettings previewSettings, List<int> terrainNoiseIndecies)
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
                    Sprite sprite = includedNoise[i].GetTile(position);

                    if (sprite != null)
                        return sprite;
                }
            }

            if(previewSettings.HasFlag(MapGenerationPreviewSettings.BaseTiles))
                return level.HasValue ? baseTiles.GetRandom(level.Value): baseTiles.GetRandom();

            return null;
        }

        public Sprite GetPathTileSprite(float? level)
        {
            return level.HasValue ? pathTiles.GetRandom(level.Value) : pathTiles.GetRandom();
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


