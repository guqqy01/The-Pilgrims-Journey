using UnityEngine;
using System.Collections.Generic;

namespace LocalMap.Editor
{
    [CreateAssetMenu(menuName = "Local Map/Auto Tile Settings")]
    public class AutoTileSettings : ScriptableObject
    {
        public List<AutoTile> autoTileSetups;

        public List<TileType> drawOrderForType;

        public AutoTile GetAutoTile(TileType underType, TileType overType)
        {
            foreach(AutoTile autoTile in autoTileSetups)
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
