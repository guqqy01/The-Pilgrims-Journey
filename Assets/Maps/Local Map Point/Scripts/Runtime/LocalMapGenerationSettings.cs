using UnityEngine;
using System;
using System.Collections.Generic;
using LocalMap.Editor;
using Monsters;
namespace LocalMap
{
    [CreateAssetMenu()]
    public class LocalMapGenerationSettings : ScriptableObject
    {
        public WeightedChanceList<TileTypeAndVisual> baseTilesAndType;
        public WeightedChanceList<TileTypeAndVisual> pathTilesAndType;

        public List<MapObject> mapObjects;

        public List<DynamicMapObject> dynamicMapObjects;

        public List<TerrainNoise> terrainNoiseLists;

        public Vector2 pathWidthRange = new Vector2(1f, 3f);

        public Vector2 pathNoiseScale = new Vector2(50f, 50f);

        public Vector2 pathNoiseTiling;

        public float cullingAmount = 0.2f;

        public MonsterSpawnSettings MonsterSpawnSettings;

        public void SetupSeeds()
        {
            foreach(TerrainNoise noise in terrainNoiseLists)
            {
                noise.SetupSeed();
            }
        }

        public TileTypeAndVisual GetMapTileSprite(Vector2Int position, float? level, MapGenerationPreviewSettings previewSettings, List<int> terrainNoiseIndecies)
        {
            if (previewSettings.HasFlag(MapGenerationPreviewSettings.TerrainNoise) && terrainNoiseIndecies != null && terrainNoiseIndecies.Count > 0)
            {
                List<TerrainNoise> includedNoise = new List<TerrainNoise>();

                foreach (int i in terrainNoiseIndecies)
                {
                    if(terrainNoiseLists.IsInBounds(i))
                        includedNoise.Add(terrainNoiseLists[i]);
                }

                includedNoise.Sort(new Comparison<TerrainNoise>((x, y) => x.priority.CompareTo(y.priority)));

                for (int i = 0; i < includedNoise.Count; i++)
                {
                    TileTypeAndVisual tileTypeAndSprite = includedNoise[i].GetTile(position);

                    if (tileTypeAndSprite != null)
                        return tileTypeAndSprite;
                }
            }

            if(previewSettings.HasFlag(MapGenerationPreviewSettings.BaseTiles))
                return level.HasValue ? baseTilesAndType.GetRandom(level.Value): baseTilesAndType.GetRandom();

            return null;
        }

        public TileTypeAndVisual GetPathTileSprite(float? level)
        {
            return level.HasValue ? pathTilesAndType.GetRandom(level.Value) : pathTilesAndType.GetRandom();
        }
    }

    [Serializable]
    public class MapObject
    {
        public string name;

        public GameObject overlay;

        public Vector2Int size;

        public AnimationCurve amountToSpawnCurve;

        public TileType validPlacementTiles;
    }

    public enum DynamicMapObjectSpawnMethod
    {
        RandomInMap,
        OutsideOfMap,
        OutsideOfMapHorizontal,
        OutsideOfMapVertical,
        SpecificPosition,
    }
}


