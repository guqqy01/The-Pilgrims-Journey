using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
namespace Tiles
{

    public class TileUIPrefabManager : Singleton<TileUIPrefabManager>
    {
        [SerializeField]
        private SerializedDictionary<TileSO, TileUIBase> tileSOAndPrefab;

        public IReadOnlyDictionary<TileSO, TileUIBase> TileSOAndPrefab => tileSOAndPrefab;
    }

}

