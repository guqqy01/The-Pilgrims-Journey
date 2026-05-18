using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using System;
using System.Linq;
namespace Tiles
{
    public class TileUIPrefabManager : Singleton<TileUIPrefabManager>
    {
        [SerializeField]
        private SerializedDictionary<TypeReference, TileUIBase> tileTypeReferenceAndTileUIPrefab;

        private Dictionary<Type, TileUIBase> _tileTypeAndTileUIPrefab;

        public IReadOnlyDictionary<Type, TileUIBase> TileTypeAndUIPrefab => _tileTypeAndTileUIPrefab;

        private void Awake()
        {
            _tileTypeAndTileUIPrefab = tileTypeReferenceAndTileUIPrefab.ToDictionary(x => x.Key.Type, y => y.Value);
        }
        public bool TryGetTileUIBase(TileBase tileBase, out TileUIBase tileUIBase)
        {
            return TileTypeAndUIPrefab.TryGetValue(tileBase.GetType(), out tileUIBase);
        }
    }

}

