using GameManagerCalls;
using System.Collections.Generic;
using UnityEngine;

namespace LocalMap
{
    // Indexed [x + borderSize, y + borderSize]. Only the ring outside the real
    // map is populated - the interior overlaps with the main tile grid and is left null.
    internal class LocalMapBorderTiles
    {
        private readonly LocalMapGenerator _owner;
        private MapTile[,] _borderTiles;

        public LocalMapBorderTiles(LocalMapGenerator owner)
        {
            _owner = owner;
        }

        public MapTile GetBorderTile(int x, int y)
        {
            if (_borderTiles == null)
                return null;

            int borderSize = _owner.BorderSize;
            int bx = x + borderSize;
            int by = y + borderSize;

            if (bx < 0 || bx >= _borderTiles.GetLength(0) || by < 0 || by >= _borderTiles.GetLength(1))
                return null;

            return _borderTiles[bx, by];
        }

        public void PlaceBorderTiles(
            LocalMapGenerationSettings generationSettings,
            MapGenerationPreviewSettings previewSettings,
            List<int> terrainNoiseIndecies)
        {
            DestroyBorderTiles();

            int borderSize = _owner.BorderSize;
            if (borderSize <= 0)
                return;

            int mapSizeX = _owner.MapSizeX;
            int mapSizeY = _owner.MapSizeY;

            _borderTiles = new MapTile[mapSizeX + borderSize * 2, mapSizeY + borderSize * 2];

            int? level = GameManager.Instance != null ? GameManager.Instance.Level : (int?)null;

            HashSet<Vector2Int> extendedPathPositions =
                previewSettings.HasFlag(MapGenerationPreviewSettings.BaseTiles)
                    ? GetExtendedPathPositions(generationSettings)
                    : new HashSet<Vector2Int>();

            for (int xPos = -borderSize; xPos < mapSizeX + borderSize; xPos++)
            {
                for (int yPos = -borderSize; yPos < mapSizeY + borderSize; yPos++)
                {
                    if (xPos >= 0 && xPos < mapSizeX && yPos >= 0 && yPos < mapSizeY)
                        continue;

                    Vector2Int position = new Vector2Int(xPos, yPos);

                    MapTile mapTile = Object.Instantiate(_owner.MapTilePrefab, new Vector3(xPos, yPos), Quaternion.identity,
                        _owner.MaptileParent != null ? _owner.MaptileParent : _owner.transform);

                    TileTypeAndVisual tileTypeAndSprite = extendedPathPositions.Contains(position)
                        ? generationSettings.GetPathTileSprite(level)
                        : _owner.BiomeBlender.GetMapTileSprite(generationSettings, position, level, previewSettings, terrainNoiseIndecies);

                    mapTile.Init(tileTypeAndSprite, position, false);

                    _borderTiles[xPos + borderSize, yPos + borderSize] = mapTile;
                }
            }
        }

        public void RunAutoTilePass()
        {
            int borderSize = _owner.BorderSize;
            int mapSizeX = _owner.MapSizeX;
            int mapSizeY = _owner.MapSizeY;

            for (int xPos = -borderSize; xPos < mapSizeX + borderSize; xPos++)
            {
                for (int yPos = -borderSize; yPos < mapSizeY + borderSize; yPos++)
                {
                    MapTile tile = _owner.GetTileAt(xPos, yPos);
                    if (tile == null)
                        continue;

                    Grid3x3<TileType> tileTypeGrid = new Grid3x3<TileType>();
                    FillExtendedTileTypeGrid(tileTypeGrid, xPos, yPos);
                    tile.SetupAutoTiles(_owner.autoBaseTileSettings, tileTypeGrid);
                }
            }
        }

        private TileType GetExtendedTileType(int x, int y)
        {
            MapTile tile = _owner.GetTileAt(x, y);
            return tile != null ? tile.TileType : TileType.Nothing;
        }

        private void FillExtendedTileTypeGrid(Grid3x3<TileType> grid, int xPos, int yPos)
        {
            grid.TopLeft = GetExtendedTileType(xPos - 1, yPos + 1);
            grid.TopCenter = GetExtendedTileType(xPos, yPos + 1);
            grid.TopRight = GetExtendedTileType(xPos + 1, yPos + 1);
            grid.CenterLeft = GetExtendedTileType(xPos - 1, yPos);
            grid.CenterRight = GetExtendedTileType(xPos + 1, yPos);
            grid.BottomLeft = GetExtendedTileType(xPos - 1, yPos - 1);
            grid.BottomCenter = GetExtendedTileType(xPos, yPos - 1);
            grid.BottomRight = GetExtendedTileType(xPos + 1, yPos - 1);
        }

        private HashSet<Vector2Int> GetExtendedPathPositions(LocalMapGenerationSettings generationSettings)
        {
            HashSet<Vector2Int> positions = new HashSet<Vector2Int>();

            Pathway pathway = _owner.Pathway;
            if (pathway?.AllDirections == null)
                return positions;

            int pathHalfWidth = Mathf.Max(1, Mathf.RoundToInt(generationSettings.pathWidthRange.x));

            // Extend every path's exit stub (the wall each direction heads out towards)
            foreach (Direction direction in pathway.AllDirections)
            {
                if (!pathway.SectionNodesPaths.TryGetValue(direction, out List<Vector2Int> nodes) || nodes.Count == 0)
                    continue;

                Vector2Int exitPoint = nodes[nodes.Count - 1];
                AddExtensionStrip(positions, exitPoint, DirectionToStep(direction), pathHalfWidth);
            }

            // Extend the entrance stub too - only ToDirections[0] actually starts
            // at pathStart (placed at StartWall, one tile outside the map); every
            // other direction's path starts at an internal node instead.
            if (pathway.ToDirections.Count > 0 &&
                pathway.SectionNodesPaths.TryGetValue(pathway.ToDirections[0], out List<Vector2Int> primaryNodes) &&
                primaryNodes.Count > 0)
            {
                Vector2Int entrancePoint = primaryNodes[0];
                AddExtensionStrip(positions, entrancePoint, DirectionToStep(_owner.StartWall), pathHalfWidth);
            }

            return positions;
        }

        private void AddExtensionStrip(HashSet<Vector2Int> positions, Vector2Int origin, Vector2Int step, int pathHalfWidth)
        {
            if (step == Vector2Int.zero)
                return;

            Vector2Int perpendicular = new Vector2Int(-step.y, step.x);
            int borderSize = _owner.BorderSize;

            for (int i = 0; i <= borderSize; i++)
            {
                Vector2Int center = origin + step * i;

                for (int w = -pathHalfWidth; w <= pathHalfWidth; w++)
                {
                    positions.Add(center + perpendicular * w);
                }
            }
        }

        private static Vector2Int DirectionToStep(Direction direction)
        {
            return direction switch
            {
                Direction.North => Vector2Int.up,
                Direction.South => Vector2Int.down,
                Direction.East => Vector2Int.right,
                Direction.West => Vector2Int.left,
                _ => Vector2Int.zero
            };
        }

        public void DestroyBorderTiles()
        {
            if (_borderTiles == null)
                return;

            for (int i = 0; i < _borderTiles.GetLength(0); i++)
            {
                for (int j = 0; j < _borderTiles.GetLength(1); j++)
                {
                    if (_borderTiles[i, j] == null) continue;

                    if (Application.isPlaying) Object.Destroy(_borderTiles[i, j].gameObject);
                    else Object.DestroyImmediate(_borderTiles[i, j].gameObject);
                }
            }

            _borderTiles = null;
        }
    }
}
