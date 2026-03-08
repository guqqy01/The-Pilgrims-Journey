using UnityEngine;
using System;
using System.Collections.Generic;

namespace LocalMap
{
    [CreateAssetMenu()]
    public class LocalMapGenerationSettings : ScriptableObject
    {
        public WeightedChanceList<Sprite> baseTiles;

        public AutoBaseTiles autoBaseTiles;

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

        public Sprite GetMapTileSprite(Vector2Int position)
        {
            terrainNoiseLists.Sort(new Comparison<TerrainNoise>((x, y) => x.priority.CompareTo(y.priority)));

            for (int i = 0; i < terrainNoiseLists.Count; i++)
            {
                Sprite sprite = terrainNoiseLists[i].GetTile(position);

                if(sprite != null)
                    return sprite;
            }

            return baseTiles.GetRandom(GameManager.Instance.Level);
        }

        public Sprite GetMapTileSpriteForPreview(Vector2Int position, MapGenerationPreviewSettings previewSettings, List<int> terrainNoiseIndecies)
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
                return baseTiles.GetRandom();

            return null;
        }

        public Sprite GetPathTileSprite()
        {
            return pathTiles.GetRandom(GameManager.Instance.Level);
        }

    }

    [Serializable]
    public class AutoBaseTiles
    {
        [Header("Normal")]
        public Sprite TopLeft;
        public Sprite TopCenter;
        public Sprite TopRight;

        public Sprite TopAndLeft;
        public Sprite TopAndRight;

        public Sprite CenterLeft;
        public Sprite CenterRight;

        public Sprite BottomAndLeft;
        public Sprite BottomAndRight;

        public Sprite BottomLeft;
        public Sprite BottomCenter;
        public Sprite BottomRight;

        [Header("Outline")]
        public Sprite TopLeftOutline;
        public Sprite TopCenterOutline;
        public Sprite TopRightOutline;

        public Sprite TopAndLeftOutline;
        public Sprite TopAndRightOutline;

        public Sprite CenterLeftOutline;
        public Sprite CenterRightOutline;

        public Sprite BottomAndLeftOutline;
        public Sprite BottomAndRightOutline;

        public Sprite BottomLeftOutline;
        public Sprite BottomCenterOutline;
        public Sprite BottomRightOutline;
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


