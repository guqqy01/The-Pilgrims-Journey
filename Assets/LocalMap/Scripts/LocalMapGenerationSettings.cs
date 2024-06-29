using UnityEngine;
using System;
using System.Collections.Generic;
using static UnityEngine.ParticleSystem;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.UIElements;
using System.Linq;
namespace Map
{
    [CreateAssetMenu()]
    public class LocalMapGenerationSettings : ScriptableObject
    {
        [SerializeField]
        public WeightedChanceList<Sprite> baseTiles;

        [SerializeField]
        public WeightedChanceList<Sprite> pathTiles;

        public List<OverlayMapObject> mapObjects;


        [SerializeField]
        public List<TerrainNoise> terrainNoiseLists;

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
    public class OverlayMapObject
    {
        public string name;

        public GameObject overlay;

        public Vector2Int size;

        public AnimationCurve amountToSpawnCurve;
    }

   
}


