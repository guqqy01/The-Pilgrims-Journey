using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using System;
using System.Linq;
namespace Tiles.UI
{
    public class TileUIPrefabManager : Singleton<TileUIPrefabManager>
    {
        [SerializeField]
        private SerializedDictionary<TypeReference<TileBase>, TileUIBase> tileTypeReferenceAndTileUIPrefab;
        private Dictionary<Type, TileUIBase> _tileTypeAndTileUIPrefab;
        public IReadOnlyDictionary<Type, TileUIBase> TileTypeAndUIPrefab => _tileTypeAndTileUIPrefab;

        public override void InitiateFromRootAtAwake()
        {
            base.InitiateFromRootAtAwake(); // sets Instance

            if (Instance != this)
                return; // duplicate singleton was destroyed by base call, don't build

            _tileTypeAndTileUIPrefab = new Dictionary<Type, TileUIBase>();

            if (tileTypeReferenceAndTileUIPrefab == null)
                return;

            foreach (var kvp in tileTypeReferenceAndTileUIPrefab)
            {
                Type type = kvp.Key.Type;

                if (type == null)
                {
                    Debug.LogError($"[TileUIPrefabManager] Could not resolve Type for entry mapped to prefab '{kvp.Value?.name}'.");
                    continue;
                }

                _tileTypeAndTileUIPrefab[type] = kvp.Value;
            }
        }

        public bool TryGetTileUIBase(TileBase tileBase, out TileUIBase tileUIBase)
        {
            return TileTypeAndUIPrefab.TryGetValue(tileBase.GetType(), out tileUIBase);
        }
    }

}

