using System.Collections.Generic;
using UnityEngine;

namespace LocalMap
{
    internal class LocalMapObjectSpawner
    {
        private readonly LocalMapGenerator _owner;
        private List<Vector2Int> _occupiedSpots = new List<Vector2Int>();
        private readonly Dictionary<string, int> _mapObjects = new Dictionary<string, int>();

        public LocalMapObjectSpawner(LocalMapGenerator owner)
        {
            _owner = owner;
        }

        public void ClearOccupied() => _occupiedSpots.Clear();

        public void MarkOccupied(Vector2Int spot) => _occupiedSpots.Add(spot);

        public void DestroyMapObjects()
        {
            List<GameObject> mapObjectGOs = _owner.mapObjectGOs;

            for (int i = 0; i < mapObjectGOs.Count; i++)
            {
                if (Application.isPlaying)
                    Object.Destroy(mapObjectGOs[i]);
                else
                    Object.DestroyImmediate(mapObjectGOs[i]);
            }

            mapObjectGOs.Clear();
        }

        private bool AreMapTilesValid(Vector2Int point, Vector2Int size, TileType validTypes)
        {
            MapTile[,] tiles = _owner.Tiles;

            for (int xPos = point.x; xPos < point.x + size.x; xPos++)
            {
                for (int yPos = point.y; yPos < point.y + size.y; yPos++)
                {
                    if (!tiles.IsInBounds(xPos, yPos) || !validTypes.HasFlag(tiles[xPos, yPos].TileType))
                        return false;
                }
            }

            return true;
        }

        private bool CanPlaceObject(Vector2Int spawnPos, Vector2Int size, TileType validTileTypes)
        {
            if (SpotOccupied(spawnPos, size, true))
                return false;

            if (!_owner.BiomeBlender.IsBaseDominant(spawnPos))
                return false;

            return AreMapTilesValid(spawnPos, size, validTileTypes);
        }

        private bool CanPlaceMapObject(Vector2Int spawnPos, MapObject mapObject)
        {
            return CanPlaceObject(spawnPos, mapObject.size, mapObject.validPlacementTiles);
        }

        public void SpawnMapObjects(LocalMapGenerationSettings generationSettings)
        {
            DestroyMapObjects();

            int mapSizeX = _owner.MapSizeX;
            int mapSizeY = _owner.MapSizeY;

            if (_occupiedSpots == null)
                _occupiedSpots = new List<Vector2Int>();

            // Collect all unique map object lists needed across the map,
            // picking the dominant biome per spawn attempt based on position
            int breaker;

            // We need to spawn from both base settings and any blended biomes,
            // using position to determine which biome owns each spawn point
            List<MapObject> baseObjects = generationSettings.mapObjects;

            // Spawn base biome objects - they get the whole map but lose spots near blend walls
            foreach (MapObject mapObject in baseObjects)
            {
                int amountToPlace = Mathf.CeilToInt(mapObject.amountToSpawnCurve.Evaluate(UnityEngine.Random.value));
                breaker = mapSizeX * mapSizeY;

                while (amountToPlace > 0 && breaker > 0)
                {
                    int xPos = UnityEngine.Random.Range(0, mapSizeX);
                    int yPos = UnityEngine.Random.Range(0, mapSizeY);
                    Vector2Int spawnPos = new Vector2Int(xPos, yPos);

                    // Only place if this position is dominated by the base biome
                    if (CanPlaceMapObject(spawnPos, mapObject))
                    {
                        PlaceMapObject(mapObject, xPos, yPos);
                        breaker = mapSizeX * mapSizeY;
                        amountToPlace--;
                    }
                    else
                    {
                        breaker--;
                    }
                }
            }

            // Spawn each blended biome's objects in their dominant zones
            foreach (var (direction, blendSettings) in _owner.BiomeBlender.ActiveBlends)
            {
                if (blendSettings == null) continue;

                foreach (MapObject mapObject in blendSettings.mapObjects)
                {
                    int amountToPlace = Mathf.CeilToInt(mapObject.amountToSpawnCurve.Evaluate(UnityEngine.Random.value));
                    breaker = mapSizeX * mapSizeY;

                    while (amountToPlace > 0 && breaker > 0)
                    {
                        int xPos = UnityEngine.Random.Range(0, mapSizeX);
                        int yPos = UnityEngine.Random.Range(0, mapSizeY);
                        Vector2Int spawnPos = new Vector2Int(xPos, yPos);

                        if (!SpotOccupied(spawnPos, mapObject.size, true)
                            && _owner.BiomeBlender.GetDominantBlend(spawnPos) == blendSettings
                            && AreMapTilesValid(spawnPos, mapObject.size, mapObject.validPlacementTiles))
                        {
                            PlaceMapObject(mapObject, xPos, yPos);
                            breaker = mapSizeX * mapSizeY;
                            amountToPlace--;
                        }
                        else
                        {
                            breaker--;
                        }
                    }
                }
            }
        }

        public T SpawnObject<T>(T spawnedObject, string name, Vector2Int size, TileType validTileTypes) where T : MonoBehaviour
        {
            int mapSizeX = _owner.MapSizeX;
            int mapSizeY = _owner.MapSizeY;
            int breaker = 2 * mapSizeX * mapSizeY;

            while (breaker > 0)
            {
                int xPos = UnityEngine.Random.Range(0, mapSizeX);
                int yPos = UnityEngine.Random.Range(0, mapSizeY);
                Vector2Int spawnPos = new Vector2Int(xPos, yPos);

                // Only place if this position is dominated by the base biome
                if (CanPlaceObject(spawnPos, size, validTileTypes))
                {
                    return PlaceObject(spawnedObject, name, spawnPos, size);
                }
                else
                {
                    breaker--;
                }
            }

            return null;
        }

        private T PlaceObject<T>(T prefab, string name, Vector2Int position, Vector2Int size) where T : MonoBehaviour
        {
            int xPos = position.x;
            int yPos = position.y;
            int xSize = size.x;
            int ySize = size.y;

            GameObject spawnedObject = Object.Instantiate(
                prefab.gameObject,
                new Vector3(xPos + (xSize - 1) / 2f, yPos + (ySize - 1) / 2f),
                Quaternion.identity,
                _owner.transform);

            _owner.mapObjectGOs.Add(spawnedObject);

            if (_mapObjects.ContainsKey(name))
                _mapObjects[name]++;
            else
                _mapObjects.Add(name, 1);

            for (int x = 0; x < xSize; x++)
                for (int y = 0; y < ySize; y++)
                    _occupiedSpots.Add(new Vector2Int(xPos + x, yPos + y));

            return spawnedObject.GetComponent<T>();
        }

        private void PlaceMapObject(MapObject mapObject, int xPos, int yPos)
        {
            _owner.mapObjectGOs.Add(Object.Instantiate(
                mapObject.overlay,
                new Vector3(xPos + (mapObject.size.x - 1) / 2f, yPos + (mapObject.size.y - 1) / 2f),
                Quaternion.identity,
                _owner.transform));

            if (_mapObjects.ContainsKey(mapObject.name))
                _mapObjects[mapObject.name]++;
            else
                _mapObjects.Add(mapObject.name, 1);

            for (int x = 0; x < mapObject.size.x; x++)
                for (int y = 0; y < mapObject.size.y; y++)
                    _occupiedSpots.Add(new Vector2Int(xPos + x, yPos + y));
        }

        private bool SpotOccupied(Vector2Int point, Vector2Int size, bool useVisualPath)
        {
            int mapSizeX = _owner.MapSizeX;
            int mapSizeY = _owner.MapSizeY;
            bool[,] pathGrid = _owner.PathGrid;
            Pathway pathway = _owner.Pathway;

            for (int xPos = point.x; xPos < point.x + size.x; xPos++)
            {
                for (int yPos = point.y; yPos < point.y + size.y; yPos++)
                {
                    if (
                            _occupiedSpots.Contains(new Vector2Int(xPos, yPos)) ||
                            xPos > mapSizeX ||
                            yPos > mapSizeY ||
                            (useVisualPath ?
                                pathGrid != null && xPos < pathGrid.GetLength(0) && yPos < pathGrid.GetLength(1) && pathGrid[xPos, yPos] :
                                pathway != null && pathway.ContainsStep(new Vector2Int(xPos, yPos)))
                        )
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
