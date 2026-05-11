using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
namespace Tiles
{

    public class TileUIPrefabManager<TItem> : Singleton<TileUIPrefabManager<TItem>>
    {
        [SerializeField]
        private SerializedDictionary<TileSO, TileUI<TItem>> tileSOAndPrefab;

        public IReadOnlyDictionary<TileSO, TileUI<TItem>> TileSOAndPrefab => tileSOAndPrefab;
    }

}

