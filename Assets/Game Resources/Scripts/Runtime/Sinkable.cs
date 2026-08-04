using System;
using UnityEditor.Build.Content;
using UnityEngine;

namespace LocalMap
{
    public class Sinkable : MonoBehaviour
    {
        [HideInInspector]
        public bool Grounded = false;

        private Vector2Int latestMapTilePos = new Vector2Int(-1, -1);

        public Action OnSunk;

        private void Update()
        {
            if (GetIfSunk(Grounded))
            {
                OnSunk?.Invoke();
            }
        }
        
        public void MapChanged()
        {
            latestMapTilePos = new Vector2Int(-1, -1);
        }

        private bool GetIfSunk(bool grounded)
        {
            if (!grounded)
                return false;

            Vector2 position = transform.position;
            Vector2Int tilePos = position.V2ToV2Int();

            MapTile[,] mapTiles = LocalMapGenerator.Instance.Tiles;

            if(!mapTiles.IsInBounds(tilePos.x, tilePos.y))
                return false;

            MapTile mapTile = mapTiles[tilePos.x, tilePos.y];
            TileType tileType = mapTile.TileType;

            if (latestMapTilePos == tilePos && tileType != TileType.Water)
                return false;
            
            latestMapTilePos = tilePos;

            if (tileType != TileType.Water)
                return false;

            if(mapTile.IsPositionWalkable(position))
                return false;

            return true;
        }

        public void InstantSinkCheck()
        {
            if (GetIfSunk(true))
            {
                OnSunk?.Invoke();
            }
        }
    }
}
