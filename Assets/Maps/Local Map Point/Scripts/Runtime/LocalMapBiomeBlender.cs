using System.Collections.Generic;
using UnityEngine;

namespace LocalMap
{
    internal class LocalMapBiomeBlender
    {
        private readonly LocalMapGenerator _owner;
        private List<(Direction direction, LocalMapGenerationSettings settings)> _activeBlends
            = new List<(Direction, LocalMapGenerationSettings)>();

        public IReadOnlyList<(Direction direction, LocalMapGenerationSettings settings)> ActiveBlends => _activeBlends;

        public LocalMapBiomeBlender(LocalMapGenerator owner)
        {
            _owner = owner;
        }

        public void SetActiveBlends(List<(Direction direction, LocalMapGenerationSettings settings)> blends)
        {
            _activeBlends = blends ?? new List<(Direction, LocalMapGenerationSettings)>();
        }

        public Direction? GetDominantBlendDirection(Vector2Int position)
        {
            Direction? dominant = null;
            float bestWeight = 0.5f;

            foreach (var (direction, settings) in _activeBlends)
            {
                float w = GetBlendWeight(direction, position);
                if (w > bestWeight)
                {
                    bestWeight = w;
                    dominant = direction;
                }
            }

            return dominant; // null means base is dominant
        }

        public LocalMapGenerationSettings GetDominantBlend(Vector2Int position)
        {
            LocalMapGenerationSettings dominant = null;
            float bestWeight = 0.5f;

            foreach (var (direction, settings) in _activeBlends)
            {
                float w = GetBlendWeight(direction, position);
                if (w > bestWeight)
                {
                    bestWeight = w;
                    dominant = settings;
                }
            }

            return dominant; // null means base is dominant
        }

        public bool IsBaseDominant(Vector2Int position) => GetDominantBlend(position) == null;

        private float GetBlendWeight(Direction direction, Vector2Int position)
        {
            int mapSizeX = _owner.MapSizeX;
            int mapSizeY = _owner.MapSizeY;

            float baseWeight = direction switch
            {
                Direction.West => 1f - (float)position.x / (mapSizeX - 1),
                Direction.East => (float)position.x / (mapSizeX - 1),
                Direction.South => 1f - (float)position.y / (mapSizeY - 1),
                Direction.North => (float)position.y / (mapSizeY - 1),
                _ => 0f
            };

            // Sample Perlin noise offset per direction so each boundary has independent variation
            float noiseOffset = direction switch
            {
                Direction.North => 0f,
                Direction.South => 100f,
                Direction.East => 200f,
                Direction.West => 300f,
                _ => 0f
            };

            float noise = Mathf.PerlinNoise(
                position.x * _owner.BiomeBlendNoiseScale + noiseOffset,
                position.y * _owner.BiomeBlendNoiseScale + noiseOffset);

            // Remap noise from 0-1 to -1..1 and apply strength
            float noiseShift = (noise - 0.5f) * 2f * _owner.BiomeBlendNoiseStrength;

            return Mathf.Clamp01(baseWeight + noiseShift);
        }

        public TileTypeAndVisual GetMapTileSprite(
            LocalMapGenerationSettings generationSettings,
            Vector2Int position,
            float? level,
            MapGenerationPreviewSettings previewSettings,
            List<int> terrainNoiseIndecies)
        {
            if (_activeBlends.Count == 0)
                return generationSettings.GetMapTileSprite(position, level, previewSettings, terrainNoiseIndecies);

            LocalMapGenerationSettings dominant = generationSettings;
            float bestWeight = 0.5f;

            foreach (var (direction, settings) in _activeBlends)
            {
                float w = GetBlendWeight(direction, position);
                if (w > bestWeight)
                {
                    bestWeight = w;
                    dominant = settings;
                }
            }

            return dominant.GetMapTileSprite(position, level, previewSettings, terrainNoiseIndecies);
        }
    }
}
