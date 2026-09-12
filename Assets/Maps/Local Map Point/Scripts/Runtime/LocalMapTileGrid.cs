using GameManagerCalls;
using System.Collections.Generic;
using UnityEngine;

namespace LocalMap
{
    internal class LocalMapTileGrid
    {
        private readonly LocalMapGenerator _owner;

        public LocalMapTileGrid(LocalMapGenerator owner)
        {
            _owner = owner;
        }

        public void PlaceTiles(
            LocalMapGenerationSettings generationSettings,
            MapGenerationPreviewSettings previewSettings,
            List<int> terrainNoiseIndecies)
        {
            DestroyTiles();

            int mapSizeX = _owner.MapSizeX;
            int mapSizeY = _owner.MapSizeY;

            _owner.Tiles = new MapTile[mapSizeX, mapSizeY];

            generationSettings.SetupSeeds();
            foreach (var (_, blendSettings) in _owner.BiomeBlender.ActiveBlends)
                blendSettings?.SetupSeeds();

            bool[,] pathGrid = _owner.Pathway != null ? _owner.PathGenerator.GetPathGrid(generationSettings) : null;
            _owner.PathGrid = pathGrid;

            for (int xPos = 0; xPos < mapSizeX; xPos++)
            {
                for (int yPos = 0; yPos < mapSizeY; yPos++)
                {
                    MapTile mapTile = Object.Instantiate(_owner.MapTilePrefab, new Vector3(xPos, yPos), Quaternion.identity,
                        _owner.MaptileParent != null ? _owner.MaptileParent : _owner.transform);

                    Vector2Int position = new Vector2Int(xPos, yPos);

                    int? level = GameManager.Instance != null ? GameManager.Instance.Level : (int?)null;

                    if (pathGrid != null && pathGrid[xPos, yPos] && previewSettings.HasFlag(MapGenerationPreviewSettings.BaseTiles))
                    {
                        // Path tiles use the dominant biome's path sprite at this position
                        LocalMapGenerationSettings dominantSettings = _owner.BiomeBlender.GetDominantBlend(position) ?? generationSettings;
                        mapTile.Init(dominantSettings.GetPathTileSprite(level), position, true);
                    }
                    else
                    {
                        TileTypeAndVisual tileTypeAndSprite = _owner.BiomeBlender.GetMapTileSprite(generationSettings, position, level, previewSettings, terrainNoiseIndecies);
                        mapTile.Init(tileTypeAndSprite, position, tileTypeAndSprite.TileType.IsWalkable());
                    }

                    _owner.Tiles[xPos, yPos] = mapTile;
                }
            }
        }

        public void DestroyTiles()
        {
            MapTile[,] tiles = _owner.Tiles;

            if (tiles != null)
            {
                for (int i = 0; i < tiles.GetLength(0); i++)
                {
                    for (int j = 0; j < tiles.GetLength(1); j++)
                    {
                        if (tiles[i, j] == null) continue;

                        if (Application.isPlaying) Object.Destroy(tiles[i, j].gameObject);
                        else Object.DestroyImmediate(tiles[i, j].gameObject);
                    }
                }
            }

            _owner.BorderTiles.DestroyBorderTiles();
        }
    }
}
