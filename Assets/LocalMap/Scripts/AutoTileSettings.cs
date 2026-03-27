using UnityEngine;
using System.Collections.Generic;

namespace LocalMap.Editor
{
    [CreateAssetMenu(menuName = "Local Map/Auto Tile Settings")]
    public class AutoTileSettings : ScriptableObject
    {
        public List<AutoTile> autoTileSetups;
    }


}
