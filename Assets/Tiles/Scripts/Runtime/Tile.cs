using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tiles
{

    [Serializable]
    public class Tile<TItem>
    {
        public TileSO TileSO { get; }
        public TItem Item { get; protected set; }

        public bool locked;

        public Action<Tile<TItem>> ActionDestroyTile;
        public Action<float> OnRotated;

        public delegate void OnUpdateUI();
        public OnUpdateUI onUpdateUI;

        public List<Vector2Int> Shape => TileSO.Shape;
        public List<Vector2Int> RotatedShape => TileSO.Shape.Rotate(Rotation);

        private float rotation;

        public Vector2Int PlacedPosition;
        public TileGrid<TItem> TileGrid;

        public float Rotation
        {
            set
            {
                rotation = value.RoundTo(90f);
                OnRotated?.Invoke(rotation);
            }
            get
            {
                return rotation;
            }
        }

        public virtual void OnDestroy()
        {
            
            OnRotated = null;
            onUpdateUI = null;
            ActionDestroyTile = null;
        }

        public Tile(TileSO tileSO, TItem unitSO)
        {
            TileSO = tileSO;
            Item = unitSO;
        }

        protected void DestroyTile()
        {
            ActionDestroyTile?.Invoke(this);
        }

        
    }
 }

