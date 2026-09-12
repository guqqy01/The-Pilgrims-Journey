using System.Collections.Generic;
using UnityEngine;

namespace LocalMap
{
    internal class LocalMapWalkability
    {
        private readonly LocalMapGenerator _owner;
        private bool[,] _connectedToPath;

        public LocalMapWalkability(LocalMapGenerator owner)
        {
            _owner = owner;
        }

        public void BuildConnectedRegion()
        {
            int mapSizeX = _owner.MapSizeX;
            int mapSizeY = _owner.MapSizeY;

            _connectedToPath = new bool[mapSizeX, mapSizeY];

            bool[,] pathGrid = _owner.PathGrid;
            if (pathGrid == null)
                return;

            Queue<Vector2Int> frontier = new Queue<Vector2Int>();

            for (int x = 0; x < mapSizeX; x++)
            {
                for (int y = 0; y < mapSizeY; y++)
                {
                    if (pathGrid[x, y])
                    {
                        _connectedToPath[x, y] = true;
                        frontier.Enqueue(new Vector2Int(x, y));
                    }
                }
            }

            while (frontier.Count > 0)
            {
                Vector2Int current = frontier.Dequeue();

                foreach (Vector2Int neighbour in GetAdjacentNeighbours(current))
                {
                    if (_connectedToPath[neighbour.x, neighbour.y])
                        continue;

                    MapTile neighbourTile = _owner.Tiles[neighbour.x, neighbour.y];

                    if (!neighbourTile.IsFullyWalkable)
                        continue;

                    _connectedToPath[neighbour.x, neighbour.y] = true;
                    frontier.Enqueue(neighbour);
                }
            }
        }

        public IEnumerable<Vector2Int> GetAdjacentNeighbours(Vector2Int pos)
        {
            int mapSizeX = _owner.MapSizeX;
            int mapSizeY = _owner.MapSizeY;

            if (pos.x > 0) yield return new Vector2Int(pos.x - 1, pos.y);
            if (pos.x < mapSizeX - 1) yield return new Vector2Int(pos.x + 1, pos.y);
            if (pos.y > 0) yield return new Vector2Int(pos.x, pos.y - 1);
            if (pos.y < mapSizeY - 1) yield return new Vector2Int(pos.x, pos.y + 1);

            if (pos.x > 0 && pos.y > 0) yield return new Vector2Int(pos.x - 1, pos.y - 1);
            if (pos.x < mapSizeX - 1 && pos.y > 0) yield return new Vector2Int(pos.x + 1, pos.y - 1);
            if (pos.x > 0 && pos.y < mapSizeY - 1) yield return new Vector2Int(pos.x - 1, pos.y + 1);
            if (pos.x < mapSizeX - 1 && pos.y < mapSizeY - 1) yield return new Vector2Int(pos.x + 1, pos.y + 1);
        }

        public bool IsPositionOutOfMap(Vector2 position)
        {
            Vector2Int v2IntPos = position.V2ToV2Int();
            return !_owner.Tiles.IsInBounds(v2IntPos.x, v2IntPos.y);
        }

        public bool IsConnectedToPath(Vector2Int tilePos)
        {
            if (_connectedToPath == null)
                return false;

            if (!_connectedToPath.IsInBounds(tilePos.x, tilePos.y))
                return false;

            return _connectedToPath[tilePos.x, tilePos.y];
        }

        public TileType? GetTileTypeAtPosition(Vector2 position)
        {
            Vector2Int v2IntPos = position.V2ToV2Int();
            if (!_owner.Tiles.IsInBounds(v2IntPos.x, v2IntPos.y))
                return null;

            return _owner.Tiles[v2IntPos.x, v2IntPos.y].GetTileTypeAtPosition(position);
        }

        public bool IsPositionOnFullyWalkableTile(Vector2 position)
        {
            Vector2Int v2IntPos = position.V2ToV2Int();

            if (!IsConnectedToPath(v2IntPos))
                return false;

            return _owner.Tiles[v2IntPos.x, v2IntPos.y].IsFullyWalkable;
        }

        public bool IsPositionWalkable(Vector2 position)
        {
            Vector2Int v2IntPos = position.V2ToV2Int();

            if (!IsConnectedToPath(v2IntPos))
                return false;

            MapTile tile = _owner.Tiles[v2IntPos.x, v2IntPos.y];
            if (tile == null)
                return false;

            return tile.IsPositionWalkable(position);
        }

        public bool TryFindNearestFullyWalkable(Vector2 givenPos, out Vector2 walkablePosition, int maxSearchRadius = -1)
        {
            int mapSizeX = _owner.MapSizeX;
            int mapSizeY = _owner.MapSizeY;

            walkablePosition = Vector2.zero;

            if (IsPositionOnFullyWalkableTile(givenPos))
            {
                walkablePosition = givenPos;
                return true;
            }

            Vector2Int centerTile = new Vector2Int(
                Mathf.Clamp(Mathf.RoundToInt(givenPos.x), 0, mapSizeX - 1),
                Mathf.Clamp(Mathf.RoundToInt(givenPos.y), 0, mapSizeY - 1)
            );

            int maxRadius = maxSearchRadius >= 0 ? maxSearchRadius : Mathf.Max(mapSizeX, mapSizeY);

            for (int radius = 1; radius <= maxRadius; radius++)
            {
                Vector2Int bestTile = default;
                bool foundTile = false;
                float minDistance = float.MaxValue;

                void CheckTile(Vector2Int tilePos)
                {
                    if (tilePos.x < 0 || tilePos.x >= mapSizeX || tilePos.y < 0 || tilePos.y >= mapSizeY)
                        return;

                    if (!IsConnectedToPath(tilePos))
                        return;

                    if (!_owner.Tiles[tilePos.x, tilePos.y].IsFullyWalkable)
                        return;

                    float distance = Vector2.Distance(givenPos, (Vector2)tilePos);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        bestTile = tilePos;
                        foundTile = true;
                    }
                }

                if (radius == 1)
                    CheckTile(centerTile);

                for (int x = -radius; x <= radius; x++)
                {
                    CheckTile(centerTile + new Vector2Int(x, -radius));
                    CheckTile(centerTile + new Vector2Int(x, radius));
                }

                for (int y = -radius + 1; y <= radius - 1; y++)
                {
                    CheckTile(centerTile + new Vector2Int(-radius, y));
                    CheckTile(centerTile + new Vector2Int(radius, y));
                }

                if (foundTile)
                {
                    walkablePosition = bestTile;
                    return true;
                }
            }

            return false;
        }

        public bool TryFindNearestWalkable(Vector2 givenPos, out Vector2 walkablePosition, int maxSearchRadius = -1)
        {
            int mapSizeX = _owner.MapSizeX;
            int mapSizeY = _owner.MapSizeY;

            if (IsPositionWalkable(givenPos))
            {
                walkablePosition = givenPos;
                return true;
            }

            Vector2Int centerTile = new Vector2Int(
                Mathf.Clamp(Mathf.RoundToInt(givenPos.x), 0, mapSizeX - 1),
                Mathf.Clamp(Mathf.RoundToInt(givenPos.y), 0, mapSizeY - 1)
            );

            int maxRadius = maxSearchRadius >= 0 ? maxSearchRadius : Mathf.Max(mapSizeX, mapSizeY);

            for (int radius = 1; radius <= maxRadius; radius++)
            {
                Vector2 candidateWalkablePos;
                Vector2 closestWalkablePosition = default;
                bool foundWalkable = false;
                float minDistance = float.MaxValue;

                if (radius == 1)
                {
                    if (TryGetNearestWalkablePositionInTile(centerTile, givenPos, out candidateWalkablePos))
                    {
                        float distance = Vector2.Distance(candidateWalkablePos, givenPos);

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestWalkablePosition = candidateWalkablePos;
                        }
                        foundWalkable = true;
                    }
                }

                for (int x = -radius; x <= radius; x++)
                {
                    if (TryGetNearestWalkablePositionInTile(centerTile + new Vector2Int(x, -radius), givenPos, out candidateWalkablePos))
                    {
                        float distance = Vector2.Distance(candidateWalkablePos, givenPos);
                        if (distance < minDistance) { minDistance = distance; closestWalkablePosition = candidateWalkablePos; }
                        foundWalkable = true;
                    }
                    if (TryGetNearestWalkablePositionInTile(centerTile + new Vector2Int(x, radius), givenPos, out candidateWalkablePos))
                    {
                        float distance = Vector2.Distance(candidateWalkablePos, givenPos);
                        if (distance < minDistance) { minDistance = distance; closestWalkablePosition = candidateWalkablePos; }
                        foundWalkable = true;
                    }
                }

                for (int y = -radius + 1; y <= radius - 1; y++)
                {
                    if (TryGetNearestWalkablePositionInTile(centerTile + new Vector2Int(-radius, y), givenPos, out candidateWalkablePos))
                    {
                        float distance = Vector2.Distance(candidateWalkablePos, givenPos);
                        if (distance < minDistance) { minDistance = distance; closestWalkablePosition = candidateWalkablePos; }
                        foundWalkable = true;
                    }
                    if (TryGetNearestWalkablePositionInTile(centerTile + new Vector2Int(radius, y), givenPos, out candidateWalkablePos))
                    {
                        float distance = Vector2.Distance(candidateWalkablePos, givenPos);
                        if (distance < minDistance) { minDistance = distance; closestWalkablePosition = candidateWalkablePos; }
                        foundWalkable = true;
                    }
                }

                if (foundWalkable)
                {
                    walkablePosition = closestWalkablePosition;
                    return true;
                }
            }

            walkablePosition = default;
            return false;
        }

        private bool TryGetNearestWalkablePositionInTile(Vector2Int tilePos, Vector2 searchOrigin, out Vector2 walkablePosition)
        {
            walkablePosition = default;

            int mapSizeX = _owner.MapSizeX;
            int mapSizeY = _owner.MapSizeY;

            if (tilePos.x < 0 || tilePos.x >= mapSizeX || tilePos.y < 0 || tilePos.y >= mapSizeY)
                return false;

            if (!IsConnectedToPath(tilePos))
                return false;

            return _owner.Tiles[tilePos.x, tilePos.y].TryGetNearestWalkablePosition(searchOrigin, out walkablePosition);
        }
    }
}
