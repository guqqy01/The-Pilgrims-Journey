using UnityEngine;
using System.Collections.Generic;

namespace LocalMap.Editor
{
    [CreateAssetMenu(menuName = "Local Map/Auto Tile Settings")]
    public class AutoTileSettings : ScriptableObject
    {
        public List<AutoTiles> autoTileSetups;

        public List<TileType> drawOrderForType;

        public AutoTiles GetAutoTile(TileType underType, TileType overType)
        {
            foreach(AutoTiles autoTile in autoTileSetups)
            {
                if(autoTile.OverTileType == overType && autoTile.UnderTileType.HasFlag(underType))
                {
                    return autoTile;
                }
            }

            return null;
        }
    }


}
